using System;
using CurrencyService.Domain.Interfaces;
using MediatR;
using System.Text.Json.Serialization;

namespace CurrencyService.Application.Features.ConvertCurrency.Query.GetHistoricalCurrencyQuery;

public class GetHistoricalCurrencyQueryRequest : IRequest<GetHistoricalCurrencyQueryResponse>, ICurrencyRequest
{
    public string BaseCurrency { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // ICurrencyRequest interface properties
    [JsonIgnore]
    public string? From { get; set; } = null;

    [JsonIgnore]
    public string? To { get; set; } = null;
}
