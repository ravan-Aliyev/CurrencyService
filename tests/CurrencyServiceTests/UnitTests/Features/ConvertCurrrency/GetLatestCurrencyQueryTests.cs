using System;
using System.Threading;
using System.Threading.Tasks;
using CurrencyService.Application.Features.ConvertCurrency.Query.GetLatestCurrencyQuery;
using CurrencyService.Domain.Exceptions;
using CurrencyService.Domain.Interfaces;
using CurrencyService.Domain.Models;
using MediatR;
using Moq;
using Xunit;
using System.Collections.Generic;

namespace CurrencyService.Tests.UnitTests.Features.ConvertCurrrency;

public class GetLatestCurrencyQueryTests
{
    private readonly Mock<ICurrencyService> _currencyServiceMock;
    private readonly GetLatestCurrencyQueryHandler _handler;

    public GetLatestCurrencyQueryTests()
    {
        _currencyServiceMock = new Mock<ICurrencyService>();
        _handler = new GetLatestCurrencyQueryHandler(_currencyServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ReturnsExchangeRate()
    {
        // Arrange
        var request = new GetLatestCurrencyQueryRequest
        {
            BaseCurrency = "USD"
        };

        var expectedResponse = new ExchangeRate
        {
            Base = "USD",
            Amount = 1,
            Date = DateTime.UtcNow,
            Rates = new Dictionary<string, decimal> 
            { 
                { "EUR", 0.85m }, 
                { "GBP", 0.73m },
                { "JPY", 110.50m }
            }
        };

        _currencyServiceMock.Setup(x => x.GetLatestRatesAsync(request.BaseCurrency))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Base, result.Base);
        Assert.Equal(expectedResponse.Amount, result.Amount);
        Assert.Equal(expectedResponse.Date, result.Date);
        Assert.Equal(expectedResponse.Rates.Count, result.Rates.Count);
        Assert.Equal(expectedResponse.Rates["EUR"], result.Rates["EUR"]);
        Assert.Equal(expectedResponse.Rates["GBP"], result.Rates["GBP"]);
        Assert.Equal(expectedResponse.Rates["JPY"], result.Rates["JPY"]);

        _currencyServiceMock.Verify(x => x.GetLatestRatesAsync(request.BaseCurrency), Times.Once);
    }


    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    [InlineData("CAD")]
    public async Task Handle_WithDifferentBaseCurrencies_ReturnsCorrectExchangeRate(string baseCurrency)
    {
        // Arrange
        var request = new GetLatestCurrencyQueryRequest
        {
            BaseCurrency = baseCurrency
        };

        var expectedResponse = new ExchangeRate
        {
            Base = baseCurrency,
            Amount = 1,
            Date = DateTime.UtcNow,
            Rates = new Dictionary<string, decimal> { { "USD", 1.18m } }
        };

        _currencyServiceMock.Setup(x => x.GetLatestRatesAsync(baseCurrency))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(baseCurrency, result.Base);
        Assert.Equal(expectedResponse.Rates["USD"], result.Rates["USD"]);
    }

    [Fact]
    public async Task Handle_WithEmptyRates_ReturnsEmptyRatesDictionary()
    {
        // Arrange
        var request = new GetLatestCurrencyQueryRequest
        {
            BaseCurrency = "USD"
        };

        var expectedResponse = new ExchangeRate
        {
            Base = "USD",
            Amount = 1,
            Date = DateTime.UtcNow,
            Rates = new Dictionary<string, decimal>()
        };

        _currencyServiceMock.Setup(x => x.GetLatestRatesAsync(request.BaseCurrency))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Rates);
    }

    [Fact]
    public async Task Handle_WithLargeNumberOfRates_ReturnsAllRates()
    {
        // Arrange
        var request = new GetLatestCurrencyQueryRequest
        {
            BaseCurrency = "USD"
        };

        var rates = new Dictionary<string, decimal>();
        for (int i = 0; i < 100; i++)
        {
            rates.Add($"CUR{i:D3}", (decimal)(i + 1) / 100);
        }

        var expectedResponse = new ExchangeRate
        {
            Base = "USD",
            Amount = 1,
            Date = DateTime.UtcNow,
            Rates = rates
        };

        _currencyServiceMock.Setup(x => x.GetLatestRatesAsync(request.BaseCurrency))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(100, result.Rates.Count);
        Assert.Equal(0.01m, result.Rates["CUR000"]);
        Assert.Equal(1.00m, result.Rates["CUR099"]);
    }

    [Fact]
    public async Task Handle_WithFutureDate_ReturnsCorrectDate()
    {
        // Arrange
        var request = new GetLatestCurrencyQueryRequest
        {
            BaseCurrency = "USD"
        };

        var futureDate = DateTime.UtcNow.AddDays(1);
        var expectedResponse = new ExchangeRate
        {
            Base = "USD",
            Amount = 1,
            Date = futureDate,
            Rates = new Dictionary<string, decimal> { { "EUR", 0.85m } }
        };

        _currencyServiceMock.Setup(x => x.GetLatestRatesAsync(request.BaseCurrency))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(futureDate, result.Date);
    }

    [Fact]
    public async Task Handle_WithPastDate_ReturnsCorrectDate()
    {
        // Arrange
        var request = new GetLatestCurrencyQueryRequest
        {
            BaseCurrency = "USD"
        };

        var pastDate = DateTime.UtcNow.AddDays(-1);
        var expectedResponse = new ExchangeRate
        {
            Base = "USD",
            Amount = 1,
            Date = pastDate,
            Rates = new Dictionary<string, decimal> { { "EUR", 0.85m } }
        };

        _currencyServiceMock.Setup(x => x.GetLatestRatesAsync(request.BaseCurrency))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pastDate, result.Date);
    }


    [Fact]
    public async Task Handle_WithServiceThrowingException_PropagatesException()
    {
        // Arrange
        var request = new GetLatestCurrencyQueryRequest
        {
            BaseCurrency = "USD"
        };

        var expectedException = new InvalidOperationException("Service unavailable");
        _currencyServiceMock.Setup(x => x.GetLatestRatesAsync(request.BaseCurrency))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(request, CancellationToken.None));

        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task Handle_WithNullResponseFromService_ThrowsInvalidOperationException()
    {
        // Arrange
        var request = new GetLatestCurrencyQueryRequest
        {
            BaseCurrency = "USD"
        };

        _currencyServiceMock.Setup(x => x.GetLatestRatesAsync(request.BaseCurrency))
            .ReturnsAsync((ExchangeRate)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(request, CancellationToken.None));

        Assert.Equal("Failed to retrieve latest rates.", exception.Message);
    }

    [Fact]
    public async Task Handle_WithDifferentAmount_ReturnsCorrectAmount()
    {
        // Arrange
        var request = new GetLatestCurrencyQueryRequest
        {
            BaseCurrency = "USD"
        };

        var expectedResponse = new ExchangeRate
        {
            Base = "USD",
            Amount = 1000,
            Date = DateTime.UtcNow,
            Rates = new Dictionary<string, decimal> { { "EUR", 850m } }
        };

        _currencyServiceMock.Setup(x => x.GetLatestRatesAsync(request.BaseCurrency))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1000, result.Amount);
        Assert.Equal(850m, result.Rates["EUR"]);
    }
}
