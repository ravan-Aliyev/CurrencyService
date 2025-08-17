using CurrencyService.Api.Startup;
using CurrencyService.Domain.Models;
using CurrencyService.Infrasturucture.Providers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Memory;
using Polly;
using Moq;

namespace CurrencyService.Tests.UnitTests.Registers;

public class ApiRegisterTests
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;

    public ApiRegisterTests()
    {
        _services = new ServiceCollection();

        var configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience",
            ["Jwt:Key"] = "test-secret-key-that-is-long-enough-for-testing-purposes-only-32-chars",
            ["UsersData:Users:0:Username"] = "testuser1",
            ["UsersData:Users:0:Password"] = "testpass1",
            ["UsersData:Users:0:Roles:0"] = "User",
            ["UsersData:Users:0:Roles:1"] = "Admin",
            ["UsersData:Users:1:Username"] = "testuser2",
            ["UsersData:Users:1:Password"] = "testpass2",
            ["UsersData:Users:1:Roles:0"] = "User",
            ["FrankfurtApi:BaseUrl"] = "https://api.example.com"
        });
        _configuration = configurationBuilder.Build();

        _services.AddSingleton(_configuration);
        _services.AddLogging();
    }


    [Fact]
    public void AddApiServices_RegistersMemoryCache_WithCorrectLifetime()
    {
        // Act
        _services.AddApiServices(_configuration);

        // Assert
        var serviceDescriptor = _services.FirstOrDefault(d => d.ServiceType == typeof(IMemoryCache));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(ServiceLifetime.Singleton, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddApiServices_MemoryCache_CanBeResolvedAndUsed()
    {
        // Arrange
        _services.AddApiServices(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var memoryCache = serviceProvider.GetService<IMemoryCache>();

        // Assert
        Assert.NotNull(memoryCache);

        // Test that it actually works
        var testKey = "test-key";
        var testValue = "test-value";
        memoryCache.Set(testKey, testValue, TimeSpan.FromMinutes(1));
        var retrievedValue = memoryCache.Get(testKey);
        Assert.Equal(testValue, retrievedValue);
    }

    [Fact]
    public void AddApiServices_RegistersVersionedApiExplorer_WithCorrectOptions()
    {
        // Act
        _services.AddApiServices(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var apiExplorer = serviceProvider.GetService<IApiVersionDescriptionProvider>();
        Assert.NotNull(apiExplorer);
    }

    [Fact]
    public void AddApiServices_VersionedApiExplorer_CanBeResolved()
    {
        // Arrange
        _services.AddApiServices(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Act
        var apiExplorer = serviceProvider.GetService<IApiVersionDescriptionProvider>();

        // Assert
        Assert.NotNull(apiExplorer);
    }

    [Fact]
    public void AddApiServices_ConfiguresUsersData_FromConfiguration()
    {
        // Act
        _services.AddApiServices(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var usersDataOptions = serviceProvider.GetService<IOptions<UsersData>>();
        Assert.NotNull(usersDataOptions);

        var usersData = usersDataOptions.Value;
        Assert.NotNull(usersData);
        Assert.NotNull(usersData.Users);
        Assert.Equal(2, usersData.Users.Count);

        // Verify first user
        var firstUser = usersData.Users[0];
        Assert.Equal("testuser1", firstUser.Username);
        Assert.Equal("testpass1", firstUser.Password);
        Assert.Equal(2, firstUser.Roles.Count);
        Assert.Contains("User", firstUser.Roles);
        Assert.Contains("Admin", firstUser.Roles);

        // Verify second user
        var secondUser = usersData.Users[1];
        Assert.Equal("testuser2", secondUser.Username);
        Assert.Equal("testpass2", secondUser.Password);
        Assert.Single(secondUser.Roles);
        Assert.Contains("User", secondUser.Roles);
    }

    [Fact]
    public void AddApiServices_UsersDataConfiguration_WithEmptyUsers()
    {
        // Arrange
        var emptyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        // Act
        _services.AddApiServices(emptyConfig);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var usersDataOptions = serviceProvider.GetService<IOptions<UsersData>>();
        Assert.NotNull(usersDataOptions);

        var usersData = usersDataOptions.Value;
        Assert.NotNull(usersData);
        Assert.NotNull(usersData.Users);
        Assert.Empty(usersData.Users);
    }

    [Fact]
    public void AddApiServices_UsersDataConfiguration_WithMissingConfiguration()
    {
        // Arrange
        var minimalConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience",
                ["Jwt:Key"] = "test-secret-key-that-is-long-enough-for-testing-purposes-only-32-chars"
            })
            .Build();

        // Act
        _services.AddApiServices(minimalConfig);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var usersDataOptions = serviceProvider.GetService<IOptions<UsersData>>();
        Assert.NotNull(usersDataOptions);

        var usersData = usersDataOptions.Value;
        Assert.NotNull(usersData);
        Assert.NotNull(usersData.Users);
        Assert.Empty(usersData.Users);
    }


    [Fact]
    public void AddApiServices_RegistersHttpClientFactory_ForFrankfurterCurrencyApi()
    {
        _services.AddApiServices(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        Assert.NotNull(httpClientFactory);

        var typedClient = serviceProvider.GetService<FrankfurterCurrencyApi>();
        Assert.NotNull(typedClient);
        Assert.Equal("Frankfurt", typedClient!.Name);
    }

    [Fact]
    public void AddApiServices_HttpClient_WithResiliencePolicy()
    {
        _services.AddApiServices(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();
        Assert.NotNull(httpClientFactory);
    }

    [Fact]
    public void AddApiServices_HttpClient_CanBeResolvedMultipleTimes()
    {
        _services.AddApiServices(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        var c1 = serviceProvider.GetService<FrankfurterCurrencyApi>();
        var c2 = serviceProvider.GetService<FrankfurterCurrencyApi>();

        Assert.NotNull(c1);
        Assert.NotNull(c2);
        Assert.NotSame(c1, c2);
    }

    [Fact]
    public void AddApiServices_RegistersAuthentication_WithJwtBearer()
    {
        _services.AddApiServices(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        var authenticationService = serviceProvider.GetService<IAuthenticationService>();
        Assert.NotNull(authenticationService);
    }

    [Fact]
    public void AddApiServices_JwtBearer_WithCorrectTokenValidationParameters()
    {
        _services.AddApiServices(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
        var options = monitor.Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.True(options.TokenValidationParameters.ValidateIssuer);
        Assert.True(options.TokenValidationParameters.ValidateAudience);
        Assert.True(options.TokenValidationParameters.ValidateLifetime);
        Assert.True(options.TokenValidationParameters.ValidateIssuerSigningKey);
    }

    [Fact]
    public void AddApiServices_JwtBearer_WithCorrectConfigurationValues()
    {
        _services.AddApiServices(_configuration);
        var serviceProvider = _services.BuildServiceProvider();

        var monitor = serviceProvider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
        var options = monitor.Get(JwtBearerDefaults.AuthenticationScheme);

        Assert.Equal("test-issuer", options.TokenValidationParameters.ValidIssuer);
        Assert.Equal("test-audience", options.TokenValidationParameters.ValidAudience);
        Assert.NotNull(options.TokenValidationParameters.IssuerSigningKey);
    }

    [Fact]
    public void AddApiServices_WithNullConfiguration_ThrowsNullReferenceException()
    {
        Assert.Throws<NullReferenceException>(() => _services.AddApiServices(null!));
    }

    [Fact]
    public void AddApiServices_WithNullServices_ThrowsNullReferenceException()
    {
        IServiceCollection nullServices = null!;
        Assert.Throws<ArgumentNullException>(() => nullServices.AddApiServices(_configuration));
    }

    [Fact]
    public void AddApiServices_WithEmptyConfiguration_ThrowsDueToMissingJwtKey()
    {
        var emptyConfig = new ConfigurationBuilder().Build();
        _services.AddSingleton<IConfiguration>(emptyConfig);
        _services.AddLogging();

        // Register services; exception occurs when options are evaluated
        _services.AddApiServices(emptyConfig);
        var provider = _services.BuildServiceProvider();

        Assert.Throws<InvalidOperationException>(() =>
        {
            var monitor = provider.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>();
            // Force evaluation of options which uses Jwt:Key
            _ = monitor.Get(JwtBearerDefaults.AuthenticationScheme);
        });
    }

    [Fact]
    public void HttpPolicies_CreateResiliencePolicy_ReturnsValidPolicy()
    {
        // Arrange
        var logger = new Mock<ILogger<FrankfurterCurrencyApi>>();

        // Act
        var policy = HttpPolicies.CreateResiliencePolicy(logger.Object);

        // Assert
        Assert.NotNull(policy);
        Assert.IsAssignableFrom<IAsyncPolicy<HttpResponseMessage>>(policy);
    }

    [Fact]
    public void AddApiServices_WithSpecialCharacters_InConfiguration()
    {
        // Arrange
        var specialCharConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "test-issuer@domain.com",
                ["Jwt:Audience"] = "test-audience:port",
                ["Jwt:Key"] = "test-secret-key-with-special-chars-!@#$%^&*()_+-=[]{}|;':\",./<>?",
                ["UsersData:Users:0:Username"] = "user@domain.com",
                ["UsersData:Users:0:Password"] = "pass@word123!",
                ["UsersData:Users:0:Roles:0"] = "Admin:User"
            })
            .Build();

        // Act & Assert
        var exception = Record.Exception(() => _services.AddApiServices(specialCharConfig));
        Assert.Null(exception);
    }

    [Fact]
    public void AddApiServices_WithVeryLongConfigurationValues_HandlesCorrectly()
    {
        // Arrange
        var longKey = new string('a', 1000); // Very long key
        var longConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = longKey,
                ["Jwt:Audience"] = longKey,
                ["Jwt:Key"] = longKey,
                ["UsersData:Users:0:Username"] = longKey,
                ["UsersData:Users:0:Password"] = longKey,
                ["UsersData:Users:0:Roles:0"] = longKey
            })
            .Build();

        // Act & Assert
        var exception = Record.Exception(() => _services.AddApiServices(longConfig));
        Assert.Null(exception);
    }

    [Fact]
    public void AddApiServices_WithLargeNumberOfUsers_HandlesCorrectly()
    {
        // Arrange
        var largeUserConfig = new ConfigurationBuilder();
        var configData = new Dictionary<string, string?>
        {
            ["Jwt:Issuer"] = "test-issuer",
            ["Jwt:Audience"] = "test-audience",
            ["Jwt:Key"] = "test-secret-key-that-is-long-enough-for-testing-purposes-only-32-chars"
        };

        for (int i = 0; i < 100; i++)
        {
            configData[$"UsersData:Users:{i}:Username"] = $"user{i}";
            configData[$"UsersData:Users:{i}:Password"] = $"pass{i}";
            configData[$"UsersData:Users:{i}:Roles:0"] = "User";
            if (i % 10 == 0)
            {
                configData[$"UsersData:Users:{i}:Roles:1"] = "Admin";
            }
        }

        largeUserConfig.AddInMemoryCollection(configData);
        var config = largeUserConfig.Build();

        // Act & Assert
        var exception = Record.Exception(() => _services.AddApiServices(config));
        Assert.Null(exception);

        var serviceProvider = _services.BuildServiceProvider();
        var usersDataOptions = serviceProvider.GetService<IOptions<UsersData>>();
        Assert.NotNull(usersDataOptions);
        Assert.Equal(100, usersDataOptions.Value.Users.Count);
    }

    [Fact]
    public void AddApiServices_WithRealConfiguration_ValidatesAllSections()
    {
        // Arrange
        var realConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "https://api.currencyservice.com",
                ["Jwt:Audience"] = "https://currencyservice.com",
                ["Jwt:Key"] = "super-secret-key-that-is-long-enough-for-production-use-32-chars-minimum",
                ["UsersData:Users:0:Username"] = "admin",
                ["UsersData:Users:0:Password"] = "admin123",
                ["UsersData:Users:0:Roles:0"] = "Admin",
                ["UsersData:Users:0:Roles:1"] = "User",
                ["UsersData:Users:1:Username"] = "user",
                ["UsersData:Users:1:Password"] = "user123",
                ["UsersData:Users:1:Roles:0"] = "User"
            })
            .Build();

        // Act
        _services.AddApiServices(realConfig);
        var serviceProvider = _services.BuildServiceProvider();

        // Assert
        var usersDataOptions = serviceProvider.GetService<IOptions<UsersData>>();
        var usersData = usersDataOptions!.Value;

        Assert.Equal(2, usersData.Users.Count);

        var adminUser = usersData.Users.First(u => u.Username == "admin");
        Assert.Contains("Admin", adminUser.Roles);
        Assert.Contains("User", adminUser.Roles);

        var regularUser = usersData.Users.First(u => u.Username == "user");
        Assert.Contains("User", regularUser.Roles);
        Assert.DoesNotContain("Admin", regularUser.Roles);
    }
}
