using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CurrencyService.Domain.Models;
using CurrencyService.Infrasturucture.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace CurrencyService.Tests.UnitTests.Infrastructure;

public class FrankfurterCurrencyApiTests
{
    private readonly Mock<ILogger<FrankfurterCurrencyApi>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly FrankfurterCurrencyApi _api;

    public FrankfurterCurrencyApiTests()
    {
        _loggerMock = new Mock<ILogger<FrankfurterCurrencyApi>>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["FrankfurtApi:BaseUrl"]).Returns("https://api.frankfurter.app");

        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.frankfurter.app")
        };

        _api = new FrankfurterCurrencyApi(_httpClient, _loggerMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task GetLatestRatesAsync_WithValidBaseCurrency_ReturnsExchangeRate()
    {
        // Arrange
        var baseCurrency = "USD";
        var expectedResponse = new ExchangeRate
        {
            Amount = 1,
            Base = baseCurrency,
            Date = DateTime.UtcNow,
            Rates = new Dictionary<string, decimal> { { "EUR", 0.85m }, { "GBP", 0.73m } }
        };
        var jsonContent = JsonSerializer.Serialize(expectedResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains($"/latest?base={baseCurrency}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _api.GetLatestRatesAsync(baseCurrency);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Base, result.Base);
        Assert.Equal(expectedResponse.Rates.Count, result.Rates.Count);
        Assert.Equal(expectedResponse.Rates["EUR"], result.Rates["EUR"]);
        Assert.Equal(expectedResponse.Rates["GBP"], result.Rates["GBP"]);

        // Verify logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Sending request to /latest?base={baseCurrency}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ConvertAsync_WithValidParameters_ReturnsConvertedAmount()
    {
        // Arrange
        var fromCurrency = "USD";
        var toCurrency = "EUR";
        var amount = 100m;
        var expectedResponse = new ExchangeRate
        {
            Amount = amount,
            Base = fromCurrency,
            Date = DateTime.UtcNow,
            Rates = new Dictionary<string, decimal> { { toCurrency, 85m } }
        };
        var jsonContent = JsonSerializer.Serialize(expectedResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString().Contains($"/latest?from={fromCurrency}&to={toCurrency}&amount={amount}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _api.ConvertAsync(fromCurrency, toCurrency, amount);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Base, result.Base);
        Assert.Equal(expectedResponse.Amount, result.Amount);
        Assert.Equal(expectedResponse.Rates[toCurrency], result.Rates[toCurrency]);
    }

    [Fact]
    public async Task GetHistoricalRatesAsync_WithValidDateRange_ReturnsHistoricalRates()
    {
        // Arrange
        var baseCurrency = "USD";
        var startDate = new DateTime(2023, 8, 1);
        var endDate = new DateTime(2023, 8, 7);
        var expectedResponse = new HistoricalRate
        {
            Amount = 1,
            Base = baseCurrency,
            Start_Date = startDate.ToString("yyyy-MM-dd"),
            End_Date = endDate.ToString("yyyy-MM-dd"),
            Rates = new Dictionary<string, Dictionary<string, decimal>>
            {
                { "2023-08-01", new Dictionary<string, decimal> { { "EUR", 0.85m } } },
                { "2023-08-02", new Dictionary<string, decimal> { { "EUR", 0.86m } } }
            }
        };
        var jsonContent = JsonSerializer.Serialize(expectedResponse);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri.ToString().Contains($"/{startDate:yyyy-MM-dd}..{endDate:yyyy-MM-dd}?base={baseCurrency}")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            });

        // Act
        var result = await _api.GetHistoricalRatesAsync(baseCurrency, startDate, endDate);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Base, result.Base);
        Assert.Equal(expectedResponse.Rates.Count, result.Rates.Count);
        Assert.Contains("2023-08-01", result.Rates.Keys);
        Assert.Contains("2023-08-02", result.Rates.Keys);
    }

    [Fact]
    public async Task GetLatestRatesAsync_WhenApiReturnsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var baseCurrency = "USD";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("null", Encoding.UTF8, "application/json")
            });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _api.GetLatestRatesAsync(baseCurrency));

        Assert.Equal("Failed to retrieve latest rates.", exception.Message);

        // Verify error logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to retrieve latest rates")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task GetLatestRatesAsync_WhenHttpRequestFails_ThrowsAndLogsException()
    {
        // Arrange
        var baseCurrency = "USD";
        var expectedException = new HttpRequestException("Network error");

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => _api.GetLatestRatesAsync(baseCurrency));

        Assert.Equal("Network error", exception.Message);

        // Verify error logging
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("An error occurred while retrieving latest rates")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
