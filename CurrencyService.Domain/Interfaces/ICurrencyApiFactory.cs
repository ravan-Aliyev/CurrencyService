using System;

namespace CurrencyService.Domain.Interfaces;

public interface ICurrencyApiFactory
{
    ICurrencyApi GetSource(string? sourceName = null);
    IEnumerable<string> GetAvailableSources();
}
