using System;
using CurrencyService.Domain.Constants;
using CurrencyService.Domain.Interfaces;
using S = CurrencyService.Infrasturucture.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using CurrencyService.Domain.Models;

namespace CurrencyService.Tests.IntegrationTests;

public class CurrencyServiceIntegrationTest
{
    private readonly Mock<ICurrencyApiFactory> _factoryMock;
    private readonly Mock<ICurrencyApi> _apiMock;
    private readonly IMemoryCache _cache;
    private readonly S.CurrencyService _service;

    public CurrencyServiceIntegrationTest()
    {
        _factoryMock = new Mock<ICurrencyApiFactory>();
        _apiMock = new Mock<ICurrencyApi>();
        _cache = new MemoryCache(new MemoryCacheOptions());

        // Factory returns the mock API
        _factoryMock.Setup(f => f.GetSource(It.IsAny<string>())).Returns(_apiMock.Object);

        _service = new S.CurrencyService(_factoryMock.Object, _cache, new LoggerFactory().CreateLogger<S.CurrencyService>());
    }

    [Fact]
    public async Task GetLatestRatesAsync_ReturnsRates_AndCachesResult()
    {
        // Arrange
        var baseCurrency = "USD";
        var expectedRates = new ExchangeRate
        {
            Base = baseCurrency,
            Amount = 1,
            Date = DateTime.UtcNow,
            Rates = new Dictionary<string, decimal> { { "EUR", 0.85m }, { "GBP", 0.73m } }
        };

        _apiMock.Setup(a => a.GetLatestRatesAsync(baseCurrency))
                .ReturnsAsync(expectedRates);

        // Act
        var result1 = await _service.GetLatestRatesAsync(baseCurrency);
        var result2 = await _service.GetLatestRatesAsync(baseCurrency); // should come from cache

        // Assert
        Assert.NotNull(result1);
        Assert.Equal(baseCurrency, result1.Base);
        Assert.Contains("EUR", result1.Rates.Keys);

        // Cache test: API should only be called once
        _apiMock.Verify(a => a.GetLatestRatesAsync(baseCurrency), Times.Once);
        Assert.Equal(result1, result2); // same reference from cache
    }

    [Fact]
    public async Task GetLatestRatesAsync_Throws_ForRestrictedCurrency()
    {
        foreach (var restricted in CurrencyConstants.BlockedCurrencies)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetLatestRatesAsync(restricted));
        }
    }

    [Fact]
    public async Task ConvertAsync_ReturnsConvertedRate_AndCachesResult()
    {
        // Arrange
        var from = "USD";
        var to = "EUR";
        decimal amount = 100m;
        var converted = new ExchangeRate
        {
            Base = from,
            Amount = amount,
            Rates = new Dictionary<string, decimal> { { to, 85m } }
        };

        _apiMock.Setup(a => a.ConvertAsync(from, to, amount)).ReturnsAsync(converted);

        // Act
        var result1 = await _service.ConvertAsync(from, to, amount);
        var result2 = await _service.ConvertAsync(from, to, amount); // from cache

        // Assert
        Assert.Equal(85m, result1.Rates[to]);
        _apiMock.Verify(a => a.ConvertAsync(from, to, amount), Times.Once);
        Assert.Equal(result1, result2);
    }

    [Fact]
    public async Task GetHistoricalRatesAsync_RemovesRestrictedCurrencies_AndCaches()
    {
        // Arrange
        var baseCurrency = "USD";
        var start = DateTime.UtcNow.AddDays(-2);
        var end = DateTime.UtcNow;

        var historical = new HistoricalRate
        {
            Base = baseCurrency,
            Rates = new Dictionary<string, Dictionary<string, decimal>>
                {
                    { start.ToString("o"), new Dictionary<string, decimal> { { "EUR", 0.85m }, { "BTC", 0.00005m } } },
                    { end.ToString("o"), new Dictionary<string, decimal> { { "EUR", 0.86m }, { "BTC", 0.00006m } } }
                }
        };

        _apiMock.Setup(a => a.GetHistoricalRatesAsync(baseCurrency, start, end))
                .ReturnsAsync(historical);

        // Act
        var result = await _service.GetHistoricalRatesAsync(baseCurrency, start, end);

        // Assert restricted currencies removed
        foreach (var kvp in result.Rates)
        {
            foreach (var restricted in CurrencyConstants.BlockedCurrencies)
            {
                Assert.DoesNotContain(restricted, kvp.Value.Keys);
            }
        }

        // Cache test
        var cached = await _service.GetHistoricalRatesAsync(baseCurrency, start, end);
        _apiMock.Verify(a => a.GetHistoricalRatesAsync(baseCurrency, start, end), Times.Once);
        Assert.Equal(result, cached);
    }
}
