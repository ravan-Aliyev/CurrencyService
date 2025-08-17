using CurrencyService.Domain.Interfaces;
using CurrencyService.Infrasturucture;
using CurrencyService.Infrasturucture.Providers;
using S = CurrencyService.Infrasturucture.Services;
using CurrencyService.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;


namespace CurrencyService.Tests.UnitTests.Registers;

public class InfrastructureRegisterTests
{
    private readonly IServiceCollection _services;

    public InfrastructureRegisterTests()
    {
        _services = new ServiceCollection();
    }

    [Fact]
    public void AddInfrastructureServices_RegistersICurrencyServiceAsScoped()
    {
        // Act
        _services.AddInfrastructureServices();

        // Assert
        var serviceDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ICurrencyService));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
        Assert.Equal(typeof(S.CurrencyService), serviceDescriptor.ImplementationType);
    }

    [Fact]
    public void AddInfrastructureServices_RegistersICurrencyApiFactoryAsScoped()
    {
        // Act
        _services.AddInfrastructureServices();

        // Assert
        var serviceDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ICurrencyApiFactory));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
        Assert.Equal(typeof(CurrencyApiFactory), serviceDescriptor.ImplementationType);
    }

    [Fact]
    public void AddInfrastructureServices_RegistersITokenServiceAsScoped()
    {
        // Act
        _services.AddInfrastructureServices();

        // Assert
        var serviceDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ITokenService));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
        Assert.Equal(typeof(S.JwtTokenService), serviceDescriptor.ImplementationType);
    }

    [Fact]
    public void AddInfrastructureServices_RegistersICurrencyApiAsTransient()
    {
        // Act
        _services.AddInfrastructureServices();

        // Assert
        var serviceDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ICurrencyApi));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Transient, serviceDescriptor.Lifetime);
        Assert.Equal(typeof(FrankfurterCurrencyApi), serviceDescriptor.ImplementationType);
    }

    [Fact]
    public void AddInfrastructureServices_RegistersMemoryCache()
    {
        // Act
        _services.AddInfrastructureServices();

        // Assert
        var serviceDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IMemoryCache));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddInfrastructureServices_RegistersLogging()
    {
        // Act
        _services.AddInfrastructureServices();

        // Assert
        var serviceDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ILogger<>));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddInfrastructureServices_RegistersAllRequiredServices()
    {
        // Act
        _services.AddInfrastructureServices();

        // Assert
        var serviceTypes = _services.Select(s => s.ServiceType).ToList();

        Assert.Contains(typeof(ICurrencyService), serviceTypes);
        Assert.Contains(typeof(ICurrencyApiFactory), serviceTypes);
        Assert.Contains(typeof(ITokenService), serviceTypes);
        Assert.Contains(typeof(ICurrencyApi), serviceTypes);
        Assert.Contains(typeof(IMemoryCache), serviceTypes);
        Assert.Contains(typeof(ILogger<>), serviceTypes);
    }

    [Fact]
    public void AddInfrastructureServices_CanResolveAllServices()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
               .AddInMemoryCollection(new Dictionary<string, string?>
               {
                {"FrankfurtApi:BaseUrl", "https://api.example.app"},
                { "Jwt:Key", "test_key"},
                {"Jwt:Issuer", "test_issuer"},
                {"Jwt:Audience", "test_audience"}
               })
               .Build();

        _services.AddSingleton<IConfiguration>(configuration);
        _services.AddHttpClient();
        _services.AddInfrastructureServices();
        var serviceProvider = _services.BuildServiceProvider();

        // Act & Assert
        Assert.NotNull(serviceProvider.GetService<ICurrencyService>());
        Assert.NotNull(serviceProvider.GetService<ICurrencyApiFactory>());
        Assert.NotNull(serviceProvider.GetService<ITokenService>());
        Assert.NotNull(serviceProvider.GetService<ICurrencyApi>());
        Assert.NotNull(serviceProvider.GetService<IMemoryCache>());
        Assert.NotNull(serviceProvider.GetService<ILogger<object>>());
    }

    [Fact]
    public void AddInfrastructureServices_RegistersServicesWithCorrectLifetimes()
    {
        // Act
        _services.AddInfrastructureServices();

        // Assert
        var currencyServiceDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ICurrencyService));
        var factoryDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ICurrencyApiFactory));
        var tokenServiceDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ITokenService));
        var apiDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ICurrencyApi));
        var cacheDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IMemoryCache));
        var loggerDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ILogger<>));

        Assert.Equal(ServiceLifetime.Scoped, currencyServiceDescriptor?.Lifetime);
        Assert.Equal(ServiceLifetime.Scoped, factoryDescriptor?.Lifetime);
        Assert.Equal(ServiceLifetime.Scoped, tokenServiceDescriptor?.Lifetime);
        Assert.Equal(ServiceLifetime.Transient, apiDescriptor?.Lifetime);
        Assert.Equal(ServiceLifetime.Singleton, cacheDescriptor?.Lifetime);
        Assert.Equal(ServiceLifetime.Singleton, loggerDescriptor?.Lifetime);
    }

    [Fact]
    public void AddInfrastructureServices_RegistersCorrectImplementationTypes()
    {
        // Act
        _services.AddInfrastructureServices();

        // Assert
        var currencyServiceDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ICurrencyService));
        var factoryDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ICurrencyApiFactory));
        var tokenServiceDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ITokenService));
        var apiDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(ICurrencyApi));

        Assert.Equal(typeof(S.CurrencyService), currencyServiceDescriptor?.ImplementationType);
        Assert.Equal(typeof(CurrencyApiFactory), factoryDescriptor?.ImplementationType);
        Assert.Equal(typeof(S.JwtTokenService), tokenServiceDescriptor?.ImplementationType);
        Assert.Equal(typeof(FrankfurterCurrencyApi), apiDescriptor?.ImplementationType);
    }

    [Fact]
    public void AddInfrastructureServices_CanResolveCurrencyServiceWithDependencies()
    {
        // Arrange
        _services.AddInfrastructureServices();
        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var currencyService = serviceProvider.GetService<ICurrencyService>();

        // Assert
        Assert.NotNull(currencyService);
        Assert.IsType<S.CurrencyService>(currencyService);
    }

    [Fact]
    public void AddInfrastructureServices_CanResolveCurrencyApiFactoryWithDependencies()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
       .AddInMemoryCollection(new Dictionary<string, string?>
       {
                {"FrankfurtApi:BaseUrl", "https://api.example.app"},
       })
       .Build();

        _services.AddSingleton<IConfiguration>(configuration);
        _services.AddInfrastructureServices();
        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var factory = serviceProvider.GetService<ICurrencyApiFactory>();

        // Assert
        Assert.NotNull(factory);
        Assert.IsType<CurrencyApiFactory>(factory);
    }

    [Fact]
    public void AddInfrastructureServices_CanResolveTokenServiceWithDependencies()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
       .AddInMemoryCollection(new Dictionary<string, string?>
       {
                { "Jwt:Key", "test_key"},
                {"Jwt:Issuer", "test_issuer"},
                {"Jwt:Audience", "test_audience"}
       })
       .Build();

        _services.AddSingleton<IConfiguration>(configuration);
        _services.AddInfrastructureServices();
        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var tokenService = serviceProvider.GetService<ITokenService>();

        // Assert
        Assert.NotNull(tokenService);
        Assert.IsType<S.JwtTokenService>(tokenService);
    }

    [Fact]
    public void AddInfrastructureServices_CanResolveCurrencyApiWithDependencies()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
       .AddInMemoryCollection(new Dictionary<string, string?>
       {
            {"FrankfurtApi:BaseUrl", "https://api.example.app"},
       })
       .Build();

        _services.AddSingleton<IConfiguration>(configuration);
        _services.AddInfrastructureServices();
        _services.AddHttpClient();
        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var api = serviceProvider.GetService<ICurrencyApi>();

        // Assert
        Assert.NotNull(api);
        Assert.IsType<FrankfurterCurrencyApi>(api);
    }

    [Fact]
    public void AddInfrastructureServices_RegistersMultipleInstancesOfSameService()
    {
        // Act
        _services.AddInfrastructureServices();

        // Assert
        var currencyApiDescriptors = _services.Where(d => d.ServiceType == typeof(ICurrencyApi)).ToList();
        Assert.Single(currencyApiDescriptors);

        var loggerDescriptors = _services.Where(d => d.ServiceType == typeof(ILogger<>)).ToList();
        Assert.Single(loggerDescriptors);
    }
}
