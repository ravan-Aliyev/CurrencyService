using System;
using FluentValidation;

namespace CurrencyService.Application.Features.ConvertCurrency.Query.GetHistoricalCurrencyQuery;

public class GetHistoricalCurrencyValidation : AbstractValidator<GetHistoricalCurrencyQueryRequest>
{
    public GetHistoricalCurrencyValidation()
    {
        RuleFor(query => query.BaseCurrency)
            .NotEmpty().WithMessage("Base currency must not be empty.")
            .Length(3).WithMessage("Base currency must be exactly 3 characters long.");

        RuleFor(query => query.StartDate)
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Start date cannot be in the future.");

        RuleFor(query => query.EndDate)
            .GreaterThanOrEqualTo(query => query.StartDate).WithMessage("End date must be greater than or equal to start date.");
    }
}
