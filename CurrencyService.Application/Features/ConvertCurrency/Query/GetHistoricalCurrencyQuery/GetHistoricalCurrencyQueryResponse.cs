using System;
using CurrencyService.Application.Features.ConvertCurrency.DTOs;

namespace CurrencyService.Application.Features.ConvertCurrency.Query.GetHistoricalCurrencyQuery;

public class GetHistoricalCurrencyQueryResponse
{
    public string BaseCurrency { get; set; } = string.Empty;
    public IEnumerable<HistoryItems> Items { get; set; } = Enumerable.Empty<HistoryItems>();

    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
