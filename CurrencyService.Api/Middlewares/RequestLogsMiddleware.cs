using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CurrencyService.Api.Middlewares;

public class RequestLogsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLogsMiddleware> _logger;

    public RequestLogsMiddleware(RequestDelegate next, ILogger<RequestLogsMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        var clientId = "Unknown";
        var authorizationHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
        {
            var token = authorizationHeader.Substring("Bearer ".Length);
            var jwtHandler = new JwtSecurityTokenHandler();

            if (jwtHandler.CanReadToken(token))
            {
                var jwtToken = jwtHandler.ReadJwtToken(token);
                clientId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            }
        }

        Log.ForContext("SourceContext", "RequestLogs")
            .Information("Incoming Request: Client IP: {ClientIp}, Client ID: {ClientId}, HTTP Method: {HttpMethod}, Endpoint: {Endpoint}",
            clientIp, clientId, context.Request.Method, context.Request.Path);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            Log.ForContext("SourceContext", "RequestLogs")
                .Information("Outgoing Response: Client IP: {ClientIp}, Client ID: {ClientId}, HTTP Method: {HttpMethod}, Endpoint: {Endpoint}, Response Code: {StatusCode}, Response Time: {ResponseTime}ms",
                clientIp, clientId, context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
        }
    }
}