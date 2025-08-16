using System;
using CurrencyService.Domain.Models;

namespace CurrencyService.Domain.Interfaces;

public interface ICurrencyApi
{
    string Name { get; }
    Task<ExchangeRate> GetLatestRatesAsync(string baseCurrency);
    Task<ExchangeRate> ConvertAsync(string fromCurrency, string toCurrency, decimal amount);
    Task<HistoricalRate> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate);
}
