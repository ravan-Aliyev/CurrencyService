using System;

namespace CurrencyService.Domain.Models;

public class ExchangeRate
{
    public decimal Amount { get; init; }
    public string Base { get; init; } = string.Empty;
    public DateTime Date { get; init; }
    public Dictionary<string, decimal> Rates { get; init; } = new();
}
