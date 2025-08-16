using System;
using System.Threading;
using System.Threading.Tasks;
using CurrencyService.Application.Features.ConvertCurrency.Command.ConvertCurrencyCommand;
using CurrencyService.Domain.Exceptions;
using CurrencyService.Domain.Interfaces;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Moq;
using Xunit;
using System.Collections.Generic;
using CurrencyService.Domain.Models;

namespace CurrencyService.Tests.UnitTests.Features.ConvertCurrrency;

public class ConvertCurrencyCommandTests
{
    private readonly Mock<ICurrencyService> _currencyServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly ConvertCurrencyHandler _handler;

    public ConvertCurrencyCommandTests()
    {
        _currencyServiceMock = new Mock<ICurrencyService>();
        _mapperMock = new Mock<IMapper>();
        _handler = new ConvertCurrencyHandler(_currencyServiceMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ReturnsConvertedAmount()
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = "USD",
            To = "EUR",
            Amount = 100
        };

        var exchangeRate = new ExchangeRate
        {
            Base = "USD",
            Amount = 100,
            Date = DateTime.UtcNow,
            Rates = new Dictionary<string, decimal> { { "EUR", 0.85m } }
        };

        var expectedResponse = new ConvertCurrencyResponse
        {
            FromCurrency = "USD",
            ToCurrency = "EUR",
            OriginalAmount = 100,
            ConvertedAmount = 85,
            ConversionRate = 0.85m
        };

        _currencyServiceMock.Setup(x => x.ConvertAsync(request.From, request.To, request.Amount))
            .ReturnsAsync(exchangeRate);

        _mapperMock.Setup(x => x.Map<ConvertCurrencyResponse>(exchangeRate))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.FromCurrency, result.FromCurrency);
        Assert.Equal(expectedResponse.ToCurrency, result.ToCurrency);
        Assert.Equal(expectedResponse.OriginalAmount, result.OriginalAmount);
        Assert.Equal(expectedResponse.ConvertedAmount, result.ConvertedAmount);
        Assert.Equal(expectedResponse.ConversionRate, result.ConversionRate);

        _currencyServiceMock.Verify(x => x.ConvertAsync(request.From, request.To, request.Amount), Times.Once);
        _mapperMock.Verify(x => x.Map<ConvertCurrencyResponse>(exchangeRate), Times.Once);
    }


    [Fact]
    public async Task Handle_WithInvalidRequest_ThrowsCustomValidationException()
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = "", // Invalid empty currency
            To = "EUR",
            Amount = -100 // Invalid negative amount
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomValidationException>(
            () => _handler.Handle(request, CancellationToken.None));

        Assert.NotNull(exception);
        Assert.Contains(exception.Errors, e => e.Contains("From currency cannot be empty."));
        Assert.Contains(exception.Errors, e => e.Contains("Amount must be greater than zero"));
    }

    [Fact]
    public async Task Handle_WithNullFromCurrency_ThrowsCustomValidationException()
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = null!,
            To = "EUR",
            Amount = 100
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomValidationException>(
            () => _handler.Handle(request, CancellationToken.None));

        Assert.NotNull(exception);
        Assert.Contains(exception.Errors, e => e.Contains("From currency cannot be null"));
    }

    [Fact]
    public async Task Handle_WithInvalidCurrencyLength_ThrowsCustomValidationException()
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = "US", // Invalid 2-character currency
            To = "EUR",
            Amount = 100
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomValidationException>(
            () => _handler.Handle(request, CancellationToken.None));

        Assert.NotNull(exception);
        Assert.Contains(exception.Errors, e => e.Contains("Base currency must be exactly 3 characters long"));
    }

    [Fact]
    public async Task Handle_WithZeroAmount_ThrowsCustomValidationException()
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = "USD",
            To = "EUR",
            Amount = 0 // Invalid zero amount
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomValidationException>(
            () => _handler.Handle(request, CancellationToken.None));

        Assert.NotNull(exception);
        Assert.Contains(exception.Errors, e => e.Contains("Amount must be greater than zero"));
    }

    [Fact]
    public async Task Handle_WithLargeAmount_ProcessesSuccessfully()
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = "USD",
            To = "EUR",
            Amount = 999999999.99m // Large amount
        };

        var exchangeRate = new ExchangeRate
        {
            Base = "USD",
            Amount = 999999999.99m,
            Date = DateTime.UtcNow,
            Rates = new Dictionary<string, decimal> { { "EUR", 0.85m } }
        };

        var expectedResponse = new ConvertCurrencyResponse
        {
            FromCurrency = "USD",
            ToCurrency = "EUR",
            OriginalAmount = 999999999.99m,
            ConvertedAmount = 849999999.99m,
            ConversionRate = 0.85m
        };

        _currencyServiceMock.Setup(x => x.ConvertAsync(request.From, request.To, request.Amount))
            .ReturnsAsync(exchangeRate);

        _mapperMock.Setup(x => x.Map<ConvertCurrencyResponse>(exchangeRate))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.OriginalAmount, result.OriginalAmount);
        Assert.Equal(expectedResponse.ConvertedAmount, result.ConvertedAmount);
    }
}
