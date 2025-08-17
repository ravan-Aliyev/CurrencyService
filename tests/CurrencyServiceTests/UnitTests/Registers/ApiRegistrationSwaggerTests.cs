using System;
using System.Collections.Generic;
using CurrencyService.Api.Startup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Swagger;
using Xunit;
using Moq;
using Microsoft.Extensions.Hosting;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace CurrencyService.Tests.UnitTests.Registers;

public class ApiRegistrationSwaggerTests
{
    private static ServiceProvider BuildProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddControllers();
        services.AddEndpointsApiExplorer();

        var envMock = new Mock<IHostEnvironment>();
        envMock.SetupGet(e => e.EnvironmentName).Returns(Environments.Development);
        envMock.SetupGet(e => e.ContentRootPath).Returns(Directory.GetCurrentDirectory());
        envMock.SetupGet(e => e.ApplicationName).Returns("TestApp");
        services.AddSingleton<IHostEnvironment>(envMock.Object);

        var webEnvMock = new Mock<IWebHostEnvironment>();
        webEnvMock.SetupGet(e => e.EnvironmentName).Returns(Environments.Development);
        webEnvMock.SetupGet(e => e.ContentRootPath).Returns(Directory.GetCurrentDirectory());
        webEnvMock.SetupGet(e => e.ApplicationName).Returns("TestApp");
        services.AddSingleton<IWebHostEnvironment>(webEnvMock.Object);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "issuer",
                ["Jwt:Audience"] = "aud",
                ["Jwt:Key"] = new string('k', 64),
                ["FrankfurtApi:BaseUrl"] = "https://api.example.com"
            })
            .Build();
        services.AddSingleton<IConfiguration>(config);
        services.AddApiServices(config);
        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddApiServices_RegistersSwaggerServices()
    {
        using var provider = BuildProvider();
        var swaggerProvider = provider.GetService<ISwaggerProvider>();
        Assert.NotNull(swaggerProvider);
    }

    [Fact]
    public void SwaggerProvider_GeneratesV1_WithBearerSecurityScheme()
    {
        using var provider = BuildProvider();
        var swaggerProvider = provider.GetRequiredService<ISwaggerProvider>();

        var doc = swaggerProvider.GetSwagger("v1");

        Assert.NotNull(doc);
        Assert.Equal("v1", doc.Info.Version);

        Assert.NotNull(doc.Components);
        Assert.True(doc.Components.SecuritySchemes.ContainsKey("Bearer"));
        var scheme = doc.Components.SecuritySchemes["Bearer"];
        Assert.Equal(SecuritySchemeType.Http, scheme.Type);
        Assert.Equal("Bearer", scheme.Scheme);
        Assert.Equal("JWT", scheme.BearerFormat);
    }
}
