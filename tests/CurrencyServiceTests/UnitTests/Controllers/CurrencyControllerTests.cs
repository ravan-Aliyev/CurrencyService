using CurrencyService.Api.Controllers;
using CurrencyService.Application.Features.ConvertCurrency.Command.ConvertCurrencyCommand;
using CurrencyService.Application.Features.ConvertCurrency.DTOs;
using CurrencyService.Application.Features.ConvertCurrency.Query.GetHistoricalCurrencyQuery;
using CurrencyService.Application.Features.ConvertCurrency.Query.GetLatestCurrencyQuery;
using CurrencyService.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CurrencyService.Tests.UnitTests.Controllers;

public class CurrencyControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CurrencyController _controller;

    public CurrencyControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new CurrencyController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetLatestRates_WithValidBaseCurrency_ReturnsOkResult()
    {
        // Arrange
        var baseCurrency = "USD";
        var expectedResponse = new ExchangeRate
        {
            Base = baseCurrency,
            Amount = 1,
            Date = DateTime.UtcNow,
            Rates = new Dictionary<string, decimal> { { "EUR", 0.85m } }
        };

        _mediatorMock.Setup(m => m.Send(It.Is<GetLatestCurrencyQueryRequest>(x => x.BaseCurrency == baseCurrency), default))
                    .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetLatestRates(baseCurrency);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualResponse = Assert.IsType<ExchangeRate>(okResult.Value);
        Assert.Equal(expectedResponse.Base, actualResponse.Base);
        Assert.Equal(expectedResponse.Rates.Count, actualResponse.Rates.Count);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetLatestCurrencyQueryRequest>(), default), Times.Once);
    }

    [Fact]
    public async Task ConvertCurrency_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = "USD",
            To = "EUR",
            Amount = 100
        };
        var expectedResponse = new ConvertCurrencyResponse
        {
            FromCurrency = "USD",
            ToCurrency = "EUR",
            OriginalAmount = 100,
            ConvertedAmount = 85,
            ConversionRate = 0.85m
        };

        _mediatorMock.Setup(m => m.Send(It.Is<ConvertCurrencyRequest>(x =>
            x.From == request.From &&
            x.To == request.To &&
            x.Amount == request.Amount), default))
                    .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ConvertCurrency(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualResponse = Assert.IsType<ConvertCurrencyResponse>(okResult.Value);
        Assert.Equal(expectedResponse.FromCurrency, actualResponse.FromCurrency);
        Assert.Equal(expectedResponse.ToCurrency, actualResponse.ToCurrency);
        Assert.Equal(expectedResponse.ConvertedAmount, actualResponse.ConvertedAmount);
        _mediatorMock.Verify(m => m.Send(It.IsAny<ConvertCurrencyRequest>(), default), Times.Once);
    }

    [Fact]
    public async Task GetHistoricalRates_WithValidRequest_ReturnsOkResult()
    {
        // Arrange
        var request = new GetHistoricalCurrencyQueryRequest
        {
            BaseCurrency = "USD",
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };
        var expectedResponse = new GetHistoricalCurrencyQueryResponse
        {
            BaseCurrency = "USD",
            Items = new List<HistoryItems>
            {
                new HistoryItems
                {
                    Date = DateTime.UtcNow.AddDays(-7),
                    Rates = new Dictionary<string, decimal> { { "EUR", 0.85m } }
                }
            }
        };

        _mediatorMock.Setup(m => m.Send(It.Is<GetHistoricalCurrencyQueryRequest>(x =>
            x.BaseCurrency == request.BaseCurrency), default))
                    .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.GetHistoricalRates(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualResponse = Assert.IsType<GetHistoricalCurrencyQueryResponse>(okResult.Value);
        Assert.Equal(expectedResponse.BaseCurrency, actualResponse.BaseCurrency);
        Assert.Equal(expectedResponse.Items.Count(), actualResponse.Items.Count());
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetHistoricalCurrencyQueryRequest>(), default), Times.Once);
    }
}
