using System;
using FluentValidation;

namespace CurrencyService.Application.Features.ConvertCurrency.Query.GetLatestCurrencyQuery;

public class GetLatestCurrencyRequestValidator : AbstractValidator<GetLatestCurrencyQueryRequest>
{
    public GetLatestCurrencyRequestValidator()
    {
        RuleFor(x => x.BaseCurrency)
            .NotEmpty().WithMessage("Base currency cannot be empty.")
            .Length(3).WithMessage("Base currency must be exactly 3 characters long.");
    }
}
