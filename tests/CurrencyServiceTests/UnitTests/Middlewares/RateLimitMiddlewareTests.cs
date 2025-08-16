using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text;
using CurrencyService.Api.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CurrencyService.Tests.UnitTests.Middlewares;

public class RateLimitMiddlewareTests
{
    private readonly IMemoryCache _memoryCache;
    private readonly Mock<ILogger<RateLimitMiddleware>> _loggerMock;
    private readonly RequestDelegate _next;

    public RateLimitMiddlewareTests()
    {
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<RateLimitMiddleware>>();
        _next = (HttpContext context) => Task.CompletedTask;
    }

    private RateLimitMiddleware CreateMiddleware(int maxRequests = 3, TimeSpan? window = null)
    {
        return new RateLimitMiddleware(_next, _loggerMock.Object, _memoryCache, window ?? TimeSpan.FromSeconds(60));
    }

    private static DefaultHttpContext CreateContext(string ip)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        context.Response.Body = new MemoryStream();
        context.Request.Method = "GET";
        context.Request.Path = "/test";
        return context;
    }

    [Fact]
    public async Task InvokeAsync_FirstRequest_AllowsRequest()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateContext("127.0.0.1");
        var nextCalled = false;
        RequestDelegate next = (ctx) =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        middleware = new RateLimitMiddleware(next, _loggerMock.Object, _memoryCache);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.NotEqual(StatusCodes.Status429TooManyRequests, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_ExceedsLimit_ReturnsTooManyRequests()
    {
        // Arrange
        var middleware = CreateMiddleware(maxRequests: 3);
        var ip = "192.168.1.100";

        // Make 3 requests (limit)
        for (int i = 0; i < 3; i++)
        {
            var context = CreateContext(ip);
            await middleware.InvokeAsync(context);
        }

        // 4. request should fail
        var exceededContext = CreateContext(ip);

        // Act
        await middleware.InvokeAsync(exceededContext);

        // Assert
        Assert.Equal(StatusCodes.Status429TooManyRequests, exceededContext.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_DifferentIps_TrackedSeparately()
    {
        // Arrange
        var middleware = CreateMiddleware(maxRequests: 3);
        var ip1 = "127.0.0.1";
        var ip2 = "127.0.0.2";

        for (int i = 0; i < 3; i++)
        {
            await middleware.InvokeAsync(CreateContext(ip1));
            await middleware.InvokeAsync(CreateContext(ip2));
        }

        // Act - Next requests for both IPs should be blocked
        var blocked1 = CreateContext(ip1);
        var blocked2 = CreateContext(ip2);

        await middleware.InvokeAsync(blocked1);
        await middleware.InvokeAsync(blocked2);

        // Assert
        Assert.Equal(StatusCodes.Status429TooManyRequests, blocked1.Response.StatusCode);
        Assert.Equal(StatusCodes.Status429TooManyRequests, blocked2.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_RequestUnderLimit_AllowsMultipleRequests()
    {
        // Arrange
        var middleware = CreateMiddleware(maxRequests: 5);
        var ip = "10.0.0.1";

        int nextCalledCount = 0;
        RequestDelegate next = (ctx) =>
        {
            nextCalledCount++;
            return Task.CompletedTask;
        };

        middleware = new RateLimitMiddleware(next, _loggerMock.Object, _memoryCache);

        // Act - make 3 requests (under limit)
        for (int i = 0; i < 3; i++)
        {
            await middleware.InvokeAsync(CreateContext(ip));
        }

        // Assert
        Assert.Equal(3, nextCalledCount);
    }
}