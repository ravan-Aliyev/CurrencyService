using System;
using CurrencyService.Domain.Models;

namespace CurrencyService.Domain.Interfaces;

public interface ICurrencyService
{
    Task<ExchangeRate> GetLatestRatesAsync(string baseCurrency);
    Task<ExchangeRate> ConvertAsync(string from, string to, decimal amount);
    Task<HistoricalRate> GetHistoricalRatesAsync(string baseCurrency, DateTime start, DateTime end);
}
