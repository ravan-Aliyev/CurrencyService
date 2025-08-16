using System;
using System.Net.Http.Json;
using CurrencyService.Domain.Interfaces;
using CurrencyService.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CurrencyService.Infrasturucture.Providers;

public class FrankfurterCurrencyApi : ICurrencyApi
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FrankfurterCurrencyApi> _logger;

    public string Name => "Frankfurt";

    public FrankfurterCurrencyApi(HttpClient httpClient, ILogger<FrankfurterCurrencyApi> logger, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.BaseAddress = new Uri(configuration["FrankfurtApi:BaseUrl"] ?? throw new InvalidOperationException("Frankfurt API BaseUrl is not configured."));
    }

    public async Task<ExchangeRate> GetLatestRatesAsync(string baseCurrency)
    {
        var url = $"/latest?base={baseCurrency}";
        try
        {
            _logger.LogInformation("Sending request to {Url} to get latest rates for base currency: {BaseCurrency}", url, baseCurrency);
            var response = await _httpClient.GetFromJsonAsync<ExchangeRate>(url);

            if (response == null)
            {
                _logger.LogError("Failed to retrieve latest rates. Response was null for URL: {Url}", url);
                throw new InvalidOperationException("Failed to retrieve latest rates.");
            }

            _logger.LogInformation("Successfully retrieved latest rates for base currency: {BaseCurrency}", baseCurrency);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving latest rates for base currency: {BaseCurrency}", baseCurrency);
            throw;
        }
    }

    public async Task<ExchangeRate> ConvertAsync(string fromCurrency, string toCurrency, decimal amount)
    {
        var url = $"/latest?from={fromCurrency}&to={toCurrency}&amount={amount}";
        try
        {
            _logger.LogInformation("Sending request to {Url} to convert {Amount} from {FromCurrency} to {ToCurrency}", url, amount, fromCurrency, toCurrency);
            var response = await _httpClient.GetFromJsonAsync<ExchangeRate>(url);

            if (response == null)
            {
                _logger.LogError("Failed to convert currency. Response was null for URL: {Url}", url);
                throw new InvalidOperationException("Failed to convert currency.");
            }

            _logger.LogInformation("Successfully converted {Amount} from {FromCurrency} to {ToCurrency}", amount, fromCurrency, toCurrency);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while converting {Amount} from {FromCurrency} to {ToCurrency}", amount, fromCurrency, toCurrency);
            throw;
        }
    }

    public async Task<HistoricalRate> GetHistoricalRatesAsync(string baseCurrency, DateTime startDate, DateTime endDate)
    {
        var url = $"/{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}?base={baseCurrency}";
        try
        {
            _logger.LogInformation("Sending request to {Url} to get historical rates for base currency: {BaseCurrency} between {StartDate} and {EndDate}", url, baseCurrency, startDate, endDate);
            var data = await _httpClient.GetFromJsonAsync<HistoricalRate>(url);

            if (data == null)
            {
                _logger.LogError("Failed to retrieve historical rates. Response was null for URL: {Url}", url);
                throw new InvalidOperationException("Failed to retrieve historical rates.");
            }

            _logger.LogInformation("Successfully retrieved historical rates for base currency: {BaseCurrency} between {StartDate} and {EndDate}", baseCurrency, startDate, endDate);
            return data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving historical rates for base currency: {BaseCurrency} between {StartDate} and {EndDate}", baseCurrency, startDate, endDate);
            throw;
        }
    }
}