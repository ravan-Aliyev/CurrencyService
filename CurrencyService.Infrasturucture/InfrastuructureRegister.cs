using System;
using CurrencyService.Domain.Interfaces;
using S = CurrencyService.Infrasturucture.Services;
using Microsoft.Extensions.DependencyInjection;
using CurrencyService.Infrasturucture.Providers;
using CurrencyService.Application.Abstractions;

namespace CurrencyService.Infrasturucture;

public static class InfrastuructureRegister
{
    public static void AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<ICurrencyService, S.CurrencyService>();
        services.AddScoped<ICurrencyApiFactory, CurrencyApiFactory>();
        services.AddScoped<ITokenService, S.JwtTokenService>();
        services.AddTransient<ICurrencyApi, FrankfurterCurrencyApi>();
        services.AddMemoryCache();
        services.AddLogging();
    }
}
