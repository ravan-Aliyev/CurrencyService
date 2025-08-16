using System;
using CurrencyService.Domain.Constants;
using CurrencyService.Domain.Exceptions;
using CurrencyService.Domain.Interfaces;
using MediatR;

namespace CurrencyService.Application.Behaviours;

public class CurrencyValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is ICurrencyRequest currencyRequest)
        {
            var blocked = CurrencyConstants.BlockedCurrencies;

            if (blocked.Contains(currencyRequest.BaseCurrency))
                throw new UnsupportedCurrencyException(currencyRequest.BaseCurrency);

            if (blocked.Contains(currencyRequest.From))
                throw new UnsupportedCurrencyException(currencyRequest.From);

            if (blocked.Contains(currencyRequest.To))
                throw new UnsupportedCurrencyException(currencyRequest.To);
        }

        return await next();
    }
}
