using System;
using System.Threading;
using System.Threading.Tasks;
using CurrencyService.Application.Features.ConvertCurrency.Query.GetHistoricalCurrencyQuery;
using CurrencyService.Domain.Exceptions;
using CurrencyService.Domain.Interfaces;
using CurrencyService.Domain.Models;
using MediatR;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using MapsterMapper;
using CurrencyService.Application.Features.ConvertCurrency.DTOs;

namespace CurrencyService.Tests.UnitTests.Features.ConvertCurrrency;

public class GetHistoricalCurrencyQueryTests
{
    private readonly Mock<ICurrencyService> _currencyServiceMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly GetHistoricalCurrencyQueryHandler _handler;

    public GetHistoricalCurrencyQueryTests()
    {
        _currencyServiceMock = new Mock<ICurrencyService>();
        _mapperMock = new Mock<IMapper>();
        _handler = new GetHistoricalCurrencyQueryHandler(_currencyServiceMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ReturnsHistoricalRates()
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
                    Rates = new Dictionary<string, decimal> { { "EUR", 0.85m }, { "GBP", 0.73m } }
                },
                new HistoryItems
                {
                    Date = DateTime.UtcNow.AddDays(-6),
                    Rates = new Dictionary<string, decimal> { { "EUR", 0.86m }, { "GBP", 0.74m } }
                }
            }
        };

        _currencyServiceMock.Setup(x => x.GetHistoricalRatesAsync(request.BaseCurrency, request.StartDate, request.EndDate))
            .ReturnsAsync(new HistoricalRate
            {
                Base = "USD",
                Amount = 1,
                Start_Date = request.StartDate.ToString("yyyy-MM-dd"),
                End_Date = request.EndDate.ToString("yyyy-MM-dd"),
                Rates = new Dictionary<string, Dictionary<string, decimal>>
                {
                    { request.StartDate.ToString("yyyy-MM-dd"), new Dictionary<string, decimal> { { "EUR", 0.85m }, { "GBP", 0.73m } } },
                    { request.StartDate.AddDays(1).ToString("yyyy-MM-dd"), new Dictionary<string, decimal> { { "EUR", 0.86m }, { "GBP", 0.74m } } }
                }
            });

        _mapperMock.Setup(m => m.Map<GetHistoricalCurrencyQueryResponse>(It.IsAny<HistoricalRate>()))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedResponse.BaseCurrency, result.BaseCurrency);
        Assert.Equal(expectedResponse.Items.Count(), result.Items.Count());

        var firstItem = result.Items.First();
        Assert.Equal(DateTime.UtcNow.AddDays(-7).Date, firstItem.Date.Date);
        Assert.Equal(0.85m, firstItem.Rates["EUR"]);
        Assert.Equal(0.73m, firstItem.Rates["GBP"]);

        _currencyServiceMock.Verify(x => x.GetHistoricalRatesAsync(request.BaseCurrency, request.StartDate, request.EndDate), Times.Once);
    }


    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("GBP")]
    [InlineData("JPY")]
    public async Task Handle_WithDifferentBaseCurrencies_ReturnsCorrectHistoricalRates(string baseCurrency)
    {
        // Arrange
        var request = new GetHistoricalCurrencyQueryRequest
        {
            BaseCurrency = baseCurrency,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow
        };

        var expectedResponse = new GetHistoricalCurrencyQueryResponse
        {
            BaseCurrency = baseCurrency,
            Items = new List<HistoryItems>
            {
                new HistoryItems
                {
                    Date = DateTime.UtcNow.AddDays(-1),
                    Rates = new Dictionary<string, decimal> { { "USD", 1.18m } }
                }
            }
        };

        _currencyServiceMock.Setup(x => x.GetHistoricalRatesAsync(baseCurrency, request.StartDate, request.EndDate))
            .ReturnsAsync(new HistoricalRate
            {
                Base = baseCurrency,
                Amount = 1,
                Start_Date = request.StartDate.ToString("yyyy-MM-dd"),
                End_Date = request.EndDate.ToString("yyyy-MM-dd"),
                Rates = new Dictionary<string, Dictionary<string, decimal>>
                {
                    { request.StartDate.ToString("yyyy-MM-dd"), new Dictionary<string, decimal> { { "USD", 1.18m } } }
                }
            });

        _mapperMock.Setup(m => m.Map<GetHistoricalCurrencyQueryResponse>(It.IsAny<HistoricalRate>()))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(baseCurrency, result.BaseCurrency);
        Assert.Equal(1.18m, result.Items.First().Rates["USD"]);
    }

    [Fact]
    public async Task Handle_WithSingleDayRange_ReturnsSingleHistoricalRate()
    {
        // Arrange
        var singleDate = DateTime.UtcNow.AddDays(-1);
        var request = new GetHistoricalCurrencyQueryRequest
        {
            BaseCurrency = "USD",
            StartDate = singleDate,
            EndDate = singleDate
        };

        var expectedResponse = new GetHistoricalCurrencyQueryResponse
        {
            BaseCurrency = "USD",
            Items = new List<HistoryItems>
            {
                new HistoryItems
                {
                    Date = singleDate,
                    Rates = new Dictionary<string, decimal> { { "EUR", 0.85m } }
                }
            }
        };

        _currencyServiceMock.Setup(x => x.GetHistoricalRatesAsync(request.BaseCurrency, request.StartDate, request.EndDate))
            .ReturnsAsync(new HistoricalRate
            {
                Base = "USD",
                Amount = 1,
                Start_Date = singleDate.ToString("yyyy-MM-dd"),
                End_Date = singleDate.ToString("yyyy-MM-dd"),
                Rates = new Dictionary<string, Dictionary<string, decimal>>
                {
                    { singleDate.ToString("yyyy-MM-dd"), new Dictionary<string, decimal> { { "EUR", 0.85m } } }
                }
            });

        _mapperMock.Setup(m => m.Map<GetHistoricalCurrencyQueryResponse>(It.IsAny<HistoricalRate>()))
            .Returns(expectedResponse);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);


        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(singleDate.Date, result.Items.First().Date.Date);
    }

    [Fact]
    public async Task Handle_WithLongDateRange_ReturnsMultipleHistoricalRates()
    {
        // Arrange
        var request = new GetHistoricalCurrencyQueryRequest
        {
            BaseCurrency = "USD",
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow
        };

        var rates = new Dictionary<string, Dictionary<string, decimal>>();
        for (int i = 0; i < 31; i++)
        {
            var date = DateTime.UtcNow.AddDays(-30 + i);
            rates.Add(date.ToString("yyyy-MM-dd"), new Dictionary<string, decimal> { { "EUR", 0.85m + (i * 0.001m) } });
        }

        _currencyServiceMock.Setup(x => x.GetHistoricalRatesAsync(request.BaseCurrency, request.StartDate, request.EndDate))
            .ReturnsAsync(new HistoricalRate
            {
                Base = "USD",
                Amount = 1,
                Start_Date = request.StartDate.ToString("yyyy-MM-dd"),
                End_Date = request.EndDate.ToString("yyyy-MM-dd"),
                Rates = rates
            });

        _mapperMock.Setup(m => m.Map<GetHistoricalCurrencyQueryResponse>(It.IsAny<HistoricalRate>()))
            .Returns(new GetHistoricalCurrencyQueryResponse
            {
                BaseCurrency = "USD",
                Items = rates.Select(r => new HistoryItems
                {
                    Date = DateTime.Parse(r.Key),
                    Rates = r.Value
                }).ToList()
            });

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(31, result.TotalCount);

        var firstItem = result.Items.First();
        Assert.Equal(0.85m, firstItem.Rates["EUR"]);
    }

    [Fact]
    public async Task Handle_WithEmptyRates_ReturnsEmptyHistoricalRates()
    {
        // Arrange
        var request = new GetHistoricalCurrencyQueryRequest
        {
            BaseCurrency = "USD",
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };

        _currencyServiceMock.Setup(x => x.GetHistoricalRatesAsync(request.BaseCurrency, request.StartDate, request.EndDate))
            .ReturnsAsync(new HistoricalRate
            {
                Base = "USD",
                Amount = 1,
                Start_Date = request.StartDate.ToString("yyyy-MM-dd"),
                End_Date = request.EndDate.ToString("yyyy-MM-dd"),
                Rates = new Dictionary<string, Dictionary<string, decimal>>()
            });

        var expectedException = new UnsupportedCurrencyException(request.BaseCurrency);

        var exception = await Assert.ThrowsAsync<UnsupportedCurrencyException>(
       () => _handler.Handle(request, CancellationToken.None));

        Assert.Equal(expectedException.Message, exception.Message);
    }

    [Fact]
    public async Task Handle_WithFutureDates_ReturnsCorrectHistoricalRates()
    {
        // Arrange
        var request = new GetHistoricalCurrencyQueryRequest
        {
            BaseCurrency = "USD",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(7)
        };

        var expectedResponse = new GetHistoricalCurrencyQueryResponse
        {
            BaseCurrency = "USD",
            Items = new List<HistoryItems>
            {
                new HistoryItems
                {
                    Date = DateTime.UtcNow.AddDays(1),
                    Rates = new Dictionary<string, decimal> { { "EUR", 0.85m } }
                }
            }
        };

        _currencyServiceMock.Setup(x => x.GetHistoricalRatesAsync(request.BaseCurrency, request.StartDate, request.EndDate))
            .ReturnsAsync(new HistoricalRate
            {
                Base = "USD",
                Amount = 1,
                Start_Date = request.StartDate.ToString("yyyy-MM-dd"),
                End_Date = request.EndDate.ToString("yyyy-MM-dd"),
                Rates = new Dictionary<string, Dictionary<string, decimal>>
                {
                    { request.StartDate.ToString("yyyy-MM-dd"), new Dictionary<string, decimal> { { "EUR", 0.85m } } }
                }
            });

        // Act
        var exception = await Assert.ThrowsAsync<CustomValidationException>(
       () => _handler.Handle(request, CancellationToken.None));

        Assert.Contains("Start date cannot be in the future.", exception.Errors);
    }

    [Fact]
    public async Task Handle_WithCancellationToken_RespectsCancellation()
    {
        // Arrange
        var request = new GetHistoricalCurrencyQueryRequest
        {
            BaseCurrency = "USD",
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };

        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => _handler.Handle(request, cancellationTokenSource.Token));
    }

    [Fact]
    public async Task Handle_WithServiceThrowingException_PropagatesException()
    {
        // Arrange
        var request = new GetHistoricalCurrencyQueryRequest
        {
            BaseCurrency = "USD",
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };

        var expectedException = new InvalidOperationException("Service unavailable");
        _currencyServiceMock.Setup(x => x.GetHistoricalRatesAsync(request.BaseCurrency, request.StartDate, request.EndDate))
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
        var request = new GetHistoricalCurrencyQueryRequest
        {
            BaseCurrency = "USD",
            StartDate = DateTime.UtcNow.AddDays(-7),
            EndDate = DateTime.UtcNow
        };

        _currencyServiceMock.Setup(x => x.GetHistoricalRatesAsync(request.BaseCurrency, request.StartDate, request.EndDate))
            .ReturnsAsync((HistoricalRate)null!);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnsupportedCurrencyException>(
            () => _handler.Handle(request, CancellationToken.None));

        Assert.Equal($"The currency code '{request.BaseCurrency}' is not supported.", exception.Message);
    }

    [Fact]
    public async Task Handle_WithDifferentAmount_ReturnsCorrectAmount()
    {
        // Arrange
        var request = new GetHistoricalCurrencyQueryRequest
        {
            BaseCurrency = "USD",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow
        };

        var expectedResponse = new GetHistoricalCurrencyQueryResponse
        {
            BaseCurrency = "USD",
            Items = new List<HistoryItems>
            {
                new HistoryItems
                {
                    Date = DateTime.UtcNow.AddDays(-1),
                    Rates = new Dictionary<string, decimal> { { "EUR", 850m } }
                }
            }
        };

        _currencyServiceMock.Setup(x => x.GetHistoricalRatesAsync(request.BaseCurrency, request.StartDate, request.EndDate))
            .ReturnsAsync(new HistoricalRate
            {
                Base = "USD",
                Amount = 1000,
                Start_Date = request.StartDate.ToString("yyyy-MM-dd"),
                End_Date = request.EndDate.ToString("yyyy-MM-dd"),
                Rates = new Dictionary<string, Dictionary<string, decimal>>
                {
                    { request.StartDate.ToString("yyyy-MM-dd"), new Dictionary<string, decimal> { { "EUR", 850m } } }
                }
            });

        _mapperMock.Setup(m => m.Map<GetHistoricalCurrencyQueryResponse>(It.IsAny<HistoricalRate>()))
            .Returns(new GetHistoricalCurrencyQueryResponse
            {
                BaseCurrency = "USD",
                Items = new List<HistoryItems>
                {
                    new HistoryItems
                    {
                        Date = DateTime.UtcNow.AddDays(-1),
                        Rates = new Dictionary<string, decimal> { { "EUR", 850m } }
                    }
                },
                TotalCount = 1,
                Page = request.Page,
                PageSize = request.PageSize
            });

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(850m, result.Items.First().Rates["EUR"]);
    }
}
