using CurrencyService.Application;
using CurrencyService.Application.Behaviours;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CurrencyService.Tests.UnitTests.Registers;

public class ApplicationRegisterTests
{
    private readonly IServiceCollection _services;

    public ApplicationRegisterTests()
    {
        _services = new ServiceCollection();
    }

    [Fact]
    public void AddApplicationServices_RegistersMediatRWithCorrectAssembly()
    {
        // Act
        _services.AddApplicationServices();

        // Assert
        var mediatRDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IMediator));
        Assert.NotNull(mediatRDescriptor);
        Assert.Equal(ServiceLifetime.Transient, mediatRDescriptor.Lifetime);
    }

    [Fact]
    public void AddApplicationServices_RegistersCurrencyValidationBehaviourAsPipelineBehavior()
    {
        // Act
        _services.AddApplicationServices();

        // Assert
        var behaviorDescriptor = _services.FirstOrDefault(d => 
            d.ServiceType == typeof(IPipelineBehavior<,>) && 
            d.ImplementationType == typeof(CurrencyValidationBehaviour<,>));
        
        Assert.NotNull(behaviorDescriptor);
        Assert.Equal(ServiceLifetime.Transient, behaviorDescriptor.Lifetime);
    }

    [Fact]
    public void AddApplicationServices_RegistersTypeAdapterConfigAsSingleton()
    {
        // Act
        _services.AddApplicationServices();

        // Assert
        var configDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(TypeAdapterConfig));
        Assert.NotNull(configDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, configDescriptor.Lifetime);
    }

    [Fact]
    public void AddApplicationServices_RegistersIMapperAsScoped()
    {
        // Act
        _services.AddApplicationServices();

        // Assert
        var mapperDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IMapper));
        Assert.NotNull(mapperDescriptor);
        Assert.Equal(ServiceLifetime.Scoped, mapperDescriptor.Lifetime);
    }

    [Fact]
    public void AddApplicationServices_RegistersServiceMapperImplementation()
    {
        // Act
        _services.AddApplicationServices();

        // Assert
        var mapperDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IMapper));
        Assert.NotNull(mapperDescriptor);
    }

    [Fact]
    public void AddApplicationServices_ConfiguresMapsterWithGlobalSettings()
    {
        // Act
        _services.AddApplicationServices();

        // Assert
        var configDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(TypeAdapterConfig));
        Assert.NotNull(configDescriptor);
        
        var serviceProvider = _services.BuildServiceProvider();
        var config = serviceProvider.GetService<TypeAdapterConfig>();
        Assert.NotNull(config);
        Assert.Same(TypeAdapterConfig.GlobalSettings, config);
    }

    [Fact]
    public void AddApplicationServices_RegistersAllRequiredServices()
    {
        // Act
        _services.AddApplicationServices();

        // Assert
        var serviceTypes = _services.Select(s => s.ServiceType).ToList();
        
        Assert.Contains(typeof(IMediator), serviceTypes);
        Assert.Contains(typeof(IPipelineBehavior<,>), serviceTypes);
        Assert.Contains(typeof(TypeAdapterConfig), serviceTypes);
        Assert.Contains(typeof(IMapper), serviceTypes);
    }

    [Fact]
    public void AddApplicationServices_CanResolveAllServices()
    {
        // Arrange
        _services.AddLogging();
        _services.AddApplicationServices();
        var serviceProvider = _services.BuildServiceProvider();

        // Act & Assert
        Assert.NotNull(serviceProvider.GetService<IMediator>());
        Assert.NotNull(serviceProvider.GetService<TypeAdapterConfig>());
        Assert.NotNull(serviceProvider.GetService<IMapper>());
    }


    [Fact]
    public void AddApplicationServices_RegistersServicesWithCorrectLifetimes()
    {
        // Act
        _services.AddApplicationServices();

        // Assert
        var mediatRDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IMediator));
        var behaviorDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IPipelineBehavior<,>));
        var configDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(TypeAdapterConfig));
        var mapperDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IMapper));

        Assert.Equal(ServiceLifetime.Transient, mediatRDescriptor?.Lifetime);
        Assert.Equal(ServiceLifetime.Transient, behaviorDescriptor?.Lifetime);
        Assert.Equal(ServiceLifetime.Singleton, configDescriptor?.Lifetime);
        Assert.Equal(ServiceLifetime.Scoped, mapperDescriptor?.Lifetime);
    }

    [Fact]
    public void AddApplicationServices_CanResolveMediatRWithHandlers()
    {
        // Arrange
        _services.AddLogging();
        _services.AddApplicationServices();
        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var mediator = serviceProvider.GetService<IMediator>();

        // Assert
        Assert.NotNull(mediator);
        
        var handlerTypes = typeof(ApplicationRegister).Assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && 
                i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            .ToList();
        
        Assert.NotEmpty(handlerTypes);
    }

    [Fact]
    public void AddApplicationServices_RegistersTypeAdapterConfigBeforeMapper()
    {
        // Act
        _services.AddApplicationServices();

        // Assert
        var configIndex = _services.ToList().FindIndex(d => d.ServiceType == typeof(TypeAdapterConfig));
        var mapperIndex = _services.ToList().FindIndex(d => d.ServiceType == typeof(IMapper));
        
        Assert.True(configIndex < mapperIndex, "TypeAdapterConfig should be registered before IMapper");
    }

    [Fact]
    public void AddApplicationServices_RegistersServicesInCorrectOrder()
    {
        // Act
        _services.AddApplicationServices();

        // Assert
        var serviceList = _services.ToList();
        
        var mediatRIndex = serviceList.FindIndex(d => d.ServiceType == typeof(IMediator));
        var behaviorIndex = serviceList.FindIndex(d => d.ServiceType == typeof(IPipelineBehavior<,>));
        var configIndex = serviceList.FindIndex(d => d.ServiceType == typeof(TypeAdapterConfig));
        var mapperIndex = serviceList.FindIndex(d => d.ServiceType == typeof(IMapper));
        
        Assert.True(mediatRIndex >= 0, "MediatR should be registered");
        Assert.True(behaviorIndex >= 0, "Pipeline behavior should be registered");
        Assert.True(configIndex >= 0, "TypeAdapterConfig should be registered");
        Assert.True(mapperIndex >= 0, "IMapper should be registered");
    }
}
