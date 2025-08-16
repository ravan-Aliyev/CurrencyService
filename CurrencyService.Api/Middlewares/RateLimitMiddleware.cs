using System;
using System.Collections.Concurrent;
using System.Net;
using CurrencyService.Domain.Wrappers;
using Microsoft.Extensions.Caching.Memory;

namespace CurrencyService.Api.Middlewares;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitMiddleware> _logger;
    private readonly IMemoryCache _cache;
    private readonly int _maxRequests = 3;
    private readonly TimeSpan _timeWindow;

    public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger, IMemoryCache cache, TimeSpan? timeWindow = null)
    {
        _next = next;
        _logger = logger;
        _cache = cache;
        _timeWindow = timeWindow ?? TimeSpan.FromSeconds(60);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var key = $"RateLimit_{ipAddress}";
        var entry = _cache.GetOrCreate(key, e =>
        {
            e.AbsoluteExpirationRelativeToNow = _timeWindow;
            return new RateLimitEntry
            {
                Count = 0,
                ExpiresAt = DateTime.UtcNow.Add(_timeWindow)
            };
        });

        if (entry.Count >= _maxRequests)
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            var response = ErrorResponse.FromMessage("Too many requests. Please try again later.", HttpStatusCode.TooManyRequests);
            await context.Response.WriteAsJsonAsync(response);
            _logger.LogWarning("IP {Ip} has exceeded the request limit", ipAddress);
            return;
        }

        entry.Count++;
        _cache.Set(key, entry, entry.ExpiresAt - DateTime.UtcNow);

        await _next(context);
    }

    private class RateLimitEntry
    {
        public int Count { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
