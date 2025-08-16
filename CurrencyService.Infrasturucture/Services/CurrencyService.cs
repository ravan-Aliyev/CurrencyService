using System;
using CurrencyService.Domain.Constants;
using CurrencyService.Domain.Interfaces;
using CurrencyService.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CurrencyService.Infrasturucture.Services;

public class CurrencyService : ICurrencyService
{
    private readonly ICurrencyApiFactory _factory;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CurrencyService> _logger;
    private readonly HashSet<string> _restrictedCurrencies;

    public CurrencyService(ICurrencyApiFactory factory, IMemoryCache cache, ILogger<CurrencyService> logger)
    {
        _factory = factory;
        _cache = cache;
        _logger = logger;
        _restrictedCurrencies = CurrencyConstants.BlockedCurrencies;
    }

    public async Task<ExchangeRate> GetLatestRatesAsync(string baseCurrency)
    {
        if (_restrictedCurrencies.Contains(baseCurrency))
            throw new InvalidOperationException($"Currency {baseCurrency} is restricted.");

        var cacheKey = $"LatestRates_{baseCurrency}";
        if (_cache.TryGetValue(cacheKey, out ExchangeRate? cached) && cached != null) return cached;

        var provider = _factory.GetSource();
        var rates = await provider.GetLatestRatesAsync(baseCurrency);

        foreach (var r in _restrictedCurrencies)
            rates.Rates.Remove(r);

        _cache.Set(cacheKey, rates, TimeSpan.FromMinutes(5));
        return rates;
    }

    public async Task<ExchangeRate> ConvertAsync(string from, string to, decimal amount)
    {
        var cacheKey = $"Convert_{from}_{to}_{amount}";
        if (_cache.TryGetValue(cacheKey, out ExchangeRate? cached) && cached != null) return cached;

        var provider = _factory.GetSource();
        var result = await provider.ConvertAsync(from, to, amount);
        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(1));

        return result;
    }

    public async Task<HistoricalRate> GetHistoricalRatesAsync(string baseCurrency, DateTime start, DateTime end)
    {
        if (_restrictedCurrencies.Contains(baseCurrency))
            throw new InvalidOperationException($"Currency {baseCurrency} is restricted.");

        var cacheKey = $"Historical_{baseCurrency}_{start:yyyyMMdd}_{end:yyyyMMdd}";

        if (_cache.TryGetValue(cacheKey, out HistoricalRate? cached) && cached != null)
            return cached;

        var provider = _factory.GetSource();
        var result = await provider.GetHistoricalRatesAsync(baseCurrency, start, end);

        foreach (var dateRates in result.Rates.Values)
        {
            foreach (var restricted in _restrictedCurrencies)
            {
                dateRates.Remove(restricted);
            }
        }

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));

        return result;
    }
}
