using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using CurrencyService.Api.Middlewares;
using CurrencyService.Domain.Exceptions;
using CurrencyService.Domain.Wrappers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CurrencyService.Tests.UnitTests.Middlewares;

public class ExceptionHandlerMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<ExceptionHandlerMiddleware>> _loggerMock;
    private readonly ExceptionHandlerMiddleware _middleware;

    public ExceptionHandlerMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<ExceptionHandlerMiddleware>>();
        _middleware = new ExceptionHandlerMiddleware(_nextMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Invoke_WithCustomValidationException_ReturnsBadRequestWithErrors()
    {
        // Arrange
        var context = CreateHttpContext();
        var errors = new List<string> { "Invalid currency code", "Amount must be positive" };
        var exception = new CustomValidationException(errors);
        _nextMock.Setup(next => next(context)).ThrowsAsync(exception);

        // Act
        await _middleware.Invoke(context);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        var responseBody = GetResponseBody(context);
        var errorResponse = ParseErrorResponse(responseBody);

        Assert.Equal("Multiple errors occurred.", errorResponse.Message);
        Assert.Equal(400, errorResponse.StatusCode);
        Assert.Contains("Invalid currency code", errorResponse.Errors);
        Assert.Contains("Amount must be positive", errorResponse.Errors);
    }

    [Fact]
    public async Task Invoke_WithUnsupportedCurrencyException_ReturnsBadRequestWithMessage()
    {
        // Arrange
        var context = CreateHttpContext();
        var errorMessage = "TRY";
        var exception = new UnsupportedCurrencyException(errorMessage);
        _nextMock.Setup(next => next(context)).ThrowsAsync(exception);

        // Act
        await _middleware.Invoke(context);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);

        var responseBody = GetResponseBody(context);
        var errorResponse = ParseErrorResponse(responseBody);

        Assert.Equal("The currency code 'TRY' is not supported.", errorResponse.Message);
        Assert.Equal(400, errorResponse.StatusCode);
    }

    [Fact]
    public async Task Invoke_WithUnauthorizedAccessException_ReturnsUnauthorized()
    {
        // Arrange
        var context = CreateHttpContext();
        var errorMessage = "Access denied";
        var exception = new UnauthorizedAccessException(errorMessage);
        _nextMock.Setup(next => next(context)).ThrowsAsync(exception);

        // Act
        await _middleware.Invoke(context);

        // Assert
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);

        var responseBody = GetResponseBody(context);
        var errorResponse = ParseErrorResponse(responseBody);

        Assert.Equal(errorMessage, errorResponse.Message);
        Assert.Equal(401, errorResponse.StatusCode);
    }

    [Fact]
    public async Task Invoke_WithGenericException_ReturnsInternalServerError()
    {
        // Arrange
        var context = CreateHttpContext();
        var exception = new Exception("Unexpected error occurred");
        _nextMock.Setup(next => next(context)).ThrowsAsync(exception);

        // Act
        await _middleware.Invoke(context);

        // Assert
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);

        var responseBody = GetResponseBody(context);
        var errorResponse = ParseErrorResponse(responseBody);

        Assert.Equal("An unexcepted error occured", errorResponse.Message);
        Assert.Equal(500, errorResponse.StatusCode);

        // Verify that the error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Message: Unexpected error occurred")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Invoke_WithoutException_CallsNextMiddleware()
    {
        // Arrange
        var context = CreateHttpContext();
        _nextMock.Setup(next => next(context)).Returns(Task.CompletedTask);

        // Act
        await _middleware.Invoke(context);

        // Assert
        _nextMock.Verify(next => next(context), Times.Once);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static string GetResponseBody(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private static ErrorResponse ParseErrorResponse(string responseBody)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        return JsonSerializer.Deserialize<ErrorResponse>(responseBody, options);
    }
}
