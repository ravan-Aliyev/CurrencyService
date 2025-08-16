using System;
using System.Net;
using CurrencyService.Domain.Exceptions;
using CurrencyService.Domain.Wrappers;

namespace CurrencyService.Api.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            ErrorResponse errorResponse;

            switch (ex)
            {
                case CustomValidationException e:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    errorResponse = ErrorResponse.FromErrors(e.Errors, HttpStatusCode.BadRequest);
                    break;
                case UnsupportedCurrencyException e:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    errorResponse = ErrorResponse.FromMessage(e.Message, HttpStatusCode.BadRequest);
                    break;
                case UnauthorizedAccessException e:
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    errorResponse = ErrorResponse.FromMessage(e.Message, HttpStatusCode.Unauthorized);
                    break;
                default:
                    _logger.LogError($"Message: {ex.Message} StackTrace: {ex.StackTrace} InnerException: {ex.InnerException}");
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    errorResponse = ErrorResponse.FromMessage("An unexcepted error occured");
                    break;
            }

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }
}
