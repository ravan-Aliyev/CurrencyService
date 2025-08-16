using System;
using CurrencyService.Application.Features.ConvertCurrency.Command.ConvertCurrencyCommand;
using CurrencyService.Application.Features.ConvertCurrency.DTOs;
using CurrencyService.Application.Features.ConvertCurrency.Query.GetHistoricalCurrencyQuery;
using CurrencyService.Domain.Models;
using Mapster;

namespace CurrencyService.Application.Mapping;

public class MappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ExchangeRate, ConvertCurrencyResponse>()
            .Map(dest => dest.FromCurrency, src => src.Base)
            .Map(dest => dest.ToCurrency, src => src.Rates.Keys.FirstOrDefault())
            .Map(dest => dest.OriginalAmount, src => src.Amount)
            .Map(dest => dest.ConvertedAmount, src => src.Rates.Values.FirstOrDefault())
            .Map(dest => dest.ConversionRate, src => src.Rates.Values.FirstOrDefault() / src.Amount);

        config.NewConfig<HistoricalRate, GetHistoricalCurrencyQueryResponse>()
            .Map(dest => dest.BaseCurrency, src => src.Base)
            .Map(dest => dest.Items,
                src => src.Rates.Select(kvp => new HistoryItems
                {
                    Date = DateTime.Parse(kvp.Key),
                    Rates = kvp.Value
                }).ToList());
    }
}
