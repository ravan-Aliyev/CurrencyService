using System;
using FluentValidation;

namespace CurrencyService.Application.Features.ConvertCurrency.Command.ConvertCurrencyCommand;

public class ConvertCurrencyValidator : AbstractValidator<ConvertCurrencyRequest>
{
    public ConvertCurrencyValidator()
    {
        RuleFor(x => x.From)
            .NotNull()
            .WithMessage("From currency cannot be null.")
            .NotEmpty().WithMessage("From currency cannot be empty.")
            .Length(3).WithMessage("Base currency must be exactly 3 characters long.")
            .Matches("^[A-Z]{3}$").WithMessage("From currency must consist of 3 uppercase letters.");

        RuleFor(x => x.To)
            .NotNull()
            .WithMessage("To currency cannot be null.")
            .NotEmpty().WithMessage("To currency cannot be empty.")
            .Length(3).WithMessage("Base currency must be exactly 3 characters long.")
            .Matches("^[A-Z]{3}$").WithMessage("To currency must consist of 3 uppercase letters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.");
    }
}
