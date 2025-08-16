using System;

namespace CurrencyService.Domain.Exceptions;

public class UnsupportedCurrencyException : Exception
{
    public UnsupportedCurrencyException(string currencyCode)
        : base($"The currency code '{currencyCode}' is not supported.")
    {
    }
}
