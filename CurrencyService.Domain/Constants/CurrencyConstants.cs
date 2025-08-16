using System;

namespace CurrencyService.Domain.Constants;

public static class CurrencyConstants
{
    public static readonly HashSet<string> BlockedCurrencies = new()
        {
            "TRY", "PLN", "THB", "MXN"
        };
}
