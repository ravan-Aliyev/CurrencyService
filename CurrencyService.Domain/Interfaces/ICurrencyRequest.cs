using System;

namespace CurrencyService.Domain.Interfaces;

public interface ICurrencyRequest
{
    string? BaseCurrency { get; }
    string? From { get; }
    string? To { get; }
}
