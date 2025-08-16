using System;
using CurrencyService.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyService.Infrasturucture.Providers;

public class CurrencyApiFactory : ICurrencyApiFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _sources;

    public CurrencyApiFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _sources = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "Frankfurt", typeof(FrankfurterCurrencyApi) }
            };
    }

    public ICurrencyApi GetSource(string? sourceName = null)
    {
        sourceName = string.IsNullOrWhiteSpace(sourceName) ? "Frankfurt" : sourceName.Trim();

        if (!_sources.TryGetValue(sourceName, out var type))
            throw new ArgumentException($"Source '{sourceName}' not supported.");

        return (ICurrencyApi)_serviceProvider.GetRequiredService(type);
    }

    public IEnumerable<string> GetAvailableSources() => _sources.Keys;
}
