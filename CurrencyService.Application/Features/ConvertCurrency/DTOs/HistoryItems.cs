using System;

namespace CurrencyService.Application.Features.ConvertCurrency.DTOs;

public class HistoryItems
{
    public DateTime Date { get; set; }
    public Dictionary<string, decimal> Rates { get; set; } = new Dictionary<string, decimal>();
}
