using System;
using CurrencyService.Domain.Interfaces;
using CurrencyService.Domain.Models;
using MediatR;

namespace CurrencyService.Application.Features.ConvertCurrency.Query.GetLatestCurrencyQuery;

public class GetLatestCurrencyQueryRequest : IRequest<ExchangeRate>, ICurrencyRequest
{
    public string BaseCurrency { get; init; } = string.Empty;

    public string From => null!;

    public string To => null!;
}
