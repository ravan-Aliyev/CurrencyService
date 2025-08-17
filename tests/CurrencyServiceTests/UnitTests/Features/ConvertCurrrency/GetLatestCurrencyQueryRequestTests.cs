using CurrencyService.Application.Features.ConvertCurrency.Query.GetLatestCurrencyQuery;
using CurrencyService.Domain.Interfaces;
using CurrencyService.Domain.Models;
using MediatR;

namespace CurrencyService.Tests.UnitTests.Features.ConvertCurrrency;

public class GetLatestCurrencyQueryRequestTests
{
    [Fact]
    public void DefaultValues_ShouldBeExpected()
    {
        var req = new GetLatestCurrencyQueryRequest();

        Assert.Equal(string.Empty, req.BaseCurrency);
        Assert.Null(req.From);
        Assert.Null(req.To);
    }

    [Fact]
    public void Init_BaseCurrency_ShouldSetValue()
    {
        var req = new GetLatestCurrencyQueryRequest { BaseCurrency = "USD" };

        Assert.Equal("USD", req.BaseCurrency);
    }

    [Fact]
    public void InterfaceCast_ShouldExposeSameValues()
    {
        var req = new GetLatestCurrencyQueryRequest { BaseCurrency = "EUR" };
        ICurrencyRequest ireq = req;

        Assert.Equal("EUR", ireq.BaseCurrency);
        Assert.Null(ireq.From);
        Assert.Null(ireq.To);
    }

    [Fact]
    public void Implements_IRequest_Of_ExchangeRate()
    {
        var req = new GetLatestCurrencyQueryRequest();
        Assert.IsAssignableFrom<IRequest<ExchangeRate>>(req);
    }

    [Fact]
    public void NullAssignment_To_BaseCurrency_IsObservable()
    {
        var req = new GetLatestCurrencyQueryRequest { BaseCurrency = null! };
        Assert.Null(req.BaseCurrency);
    }

    [Fact]
    public void MultipleInstances_ShouldBeIndependent()
    {
        var req1 = new GetLatestCurrencyQueryRequest { BaseCurrency = "GBP" };
        var req2 = new GetLatestCurrencyQueryRequest { BaseCurrency = "JPY" };

        Assert.Equal("GBP", req1.BaseCurrency);
        Assert.Equal("JPY", req2.BaseCurrency);
        Assert.Null(req1.From);
        Assert.Null(req1.To);
        Assert.Null(req2.From);
        Assert.Null(req2.To);
    }
}
