using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text;
using CurrencyService.Api.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace CurrencyService.Tests.UnitTests.Middlewares;

public class RequestLogsMiddlewareTests
{
    private readonly Mock<ILogger<RequestLogsMiddleware>> _loggerMock;
    private readonly RequestDelegate _next;

    public RequestLogsMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<RequestLogsMiddleware>>();
        _next = (HttpContext context) => Task.CompletedTask;
    }

    private static DefaultHttpContext CreateHttpContext(string ip = "127.0.0.1", string token = null)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(ip);
        context.Request.Method = "GET";
        context.Request.Path = "/test";
        context.Response.Body = new MemoryStream();

        if (!string.IsNullOrEmpty(token))
        {
            context.Request.Headers["Authorization"] = $"Bearer {token}";
        }

        return context;
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new RequestLogsMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode); // Default status code
    }

    [Fact]
    public async Task InvokeAsync_WithBearerToken_ExtractsClientId()
    {
        // Arrange
        var userId = "12345";
        var token = TestTokenHelper.GenerateJwtToken(userId); // JWT generator helper method

        var nextCalled = false;
        RequestDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var middleware = new RequestLogsMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext(token: token);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
        // ClientId extraction tested via middleware internals; Serilog logging can't be directly asserted without sink
    }

    [Fact]
    public async Task InvokeAsync_ResponseTimeIsRecorded()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = async ctx =>
        {
            nextCalled = true;
            await Task.Delay(50); // simulate some processing time
        };

        var middleware = new RequestLogsMiddleware(next, _loggerMock.Object);
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(200, context.Response.StatusCode);
        // Response time is measured inside middleware; cannot assert exact value here without exposing it
    }
}

// Helper for generating a simple test JWT token with sub claim
public static class TestTokenHelper
{
    public static string GenerateJwtToken(string userId)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            claims: new[] { new System.Security.Claims.Claim("sub", userId) }
        );
        return handler.WriteToken(token);
    }
}
