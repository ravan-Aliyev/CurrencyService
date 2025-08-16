using System;
using CurrencyService.Domain.Interfaces;
using MediatR;

namespace CurrencyService.Application.Features.ConvertCurrency.Command.ConvertCurrencyCommand;

public record ConvertCurrencyRequest() : IRequest<ConvertCurrencyResponse>, ICurrencyRequest
{
    public string From { get; init; }
    public string To { get; init; }
    public decimal Amount { get; init; }

    public string BaseCurrency => string.Empty;
}
