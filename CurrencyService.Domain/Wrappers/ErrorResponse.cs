using System;
using System.Net;
using System.Text.Json.Serialization;

namespace CurrencyService.Domain.Wrappers;

public class ErrorResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("statusCode")]
    public int StatusCode { get; set; }

    [JsonPropertyName("errors")]
    public IEnumerable<string> Errors { get; set; }

    public static ErrorResponse FromMessage(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
    {
        return new ErrorResponse
        {
            Message = message,
            StatusCode = (int)statusCode
        };
    }

    public static ErrorResponse FromErrors(IEnumerable<string> errors, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    {
        if (errors == null || !errors.Any())
        {
            return new ErrorResponse
            {
                Message = "No errors provided.",
                StatusCode = (int)statusCode,
                Errors = new List<string>()
            };
        }

        return new ErrorResponse
        {
            Message = "Multiple errors occurred.",
            StatusCode = (int)statusCode,
            Errors = errors
        };
    }
}
