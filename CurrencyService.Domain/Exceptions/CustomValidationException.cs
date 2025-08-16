using System;
using FluentValidation.Results;

namespace CurrencyService.Domain.Exceptions;

public class CustomValidationException : Exception
{
    public List<string> Errors { get; set; } = new List<string>();

    public CustomValidationException(string errorMessages)
    {
        Errors.Add(errorMessages);
    }

    public CustomValidationException(IEnumerable<ValidationFailure> failures)
    {
        foreach (var failure in failures)
        {
            Errors.Add(failure.ErrorMessage);
        }
    }

    public CustomValidationException(List<string> errors)
    {
        Errors = errors;
    }
}
