using System;
using CurrencyService.Domain.Constants;
using CurrencyService.Domain.Exceptions;
using CurrencyService.Domain.Interfaces;
using CurrencyService.Domain.Models;
using MediatR;

namespace CurrencyService.Application.Features.ConvertCurrency.Query.GetLatestCurrencyQuery;

public class GetLatestCurrencyQueryHandler : IRequestHandler<GetLatestCurrencyQueryRequest, ExchangeRate>
{
    private readonly ICurrencyService _currencyService;

    public GetLatestCurrencyQueryHandler(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    public async Task<ExchangeRate> Handle(GetLatestCurrencyQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await new GetLatestCurrencyRequestValidator().ValidateAsync(request);

        if (!result.IsValid)
            throw new CustomValidationException(result.Errors);

        var exchangeRate = await _currencyService.GetLatestRatesAsync(request.BaseCurrency);

        if (exchangeRate == null)
            throw new InvalidOperationException("Failed to retrieve latest rates.");

        return exchangeRate;
    }
}
