using System;
using CurrencyService.Domain.Models;
using Xunit;

namespace CurrencyService.Tests.UnitTests.Domain;

public class CurrencyServiceDomainTests
{
    [Fact]
    public void ExchangeRate_WithValidData_InitializesCorrectly()
    {
        // Arrange
        var amount = 100m;
        var baseCurrency = "USD";
        var date = DateTime.UtcNow;
        var rates = new Dictionary<string, decimal>
        {
            { "EUR", 0.85m },
            { "GBP", 0.73m },
            { "JPY", 110.50m }
        };

        // Act
        var exchangeRate = new ExchangeRate
        {
            Amount = amount,
            Base = baseCurrency,
            Date = date,
            Rates = rates
        };

        // Assert
        Assert.Equal(amount, exchangeRate.Amount);
        Assert.Equal(baseCurrency, exchangeRate.Base);
        Assert.Equal(date, exchangeRate.Date);
        Assert.Equal(rates.Count, exchangeRate.Rates.Count);
        Assert.Equal(rates["EUR"], exchangeRate.Rates["EUR"]);
        Assert.Equal(rates["GBP"], exchangeRate.Rates["GBP"]);
        Assert.Equal(rates["JPY"], exchangeRate.Rates["JPY"]);
    }

    [Fact]
    public void ExchangeRate_WithEmptyRates_InitializesCorrectly()
    {
        // Arrange & Act
        var exchangeRate = new ExchangeRate
        {
            Amount = 1,
            Base = "USD",
            Date = DateTime.UtcNow,
            Rates = new Dictionary<string, decimal>()
        };

        // Assert
        Assert.Empty(exchangeRate.Rates);
    }

    [Fact]
    public void ExchangeRate_WithLargeAmount_HandlesCorrectly()
    {
        // Arrange & Act
        var exchangeRate = new ExchangeRate
        {
            Amount = decimal.MaxValue,
            Base = "USD",
            Date = DateTime.UtcNow,
            Rates = new Dictionary<string, decimal> { { "EUR", 0.85m } }
        };

        // Assert
        Assert.Equal(decimal.MaxValue, exchangeRate.Amount);
        Assert.Equal(0.85m, exchangeRate.Rates["EUR"]);
    }

    [Fact]
    public void ExchangeRate_WithSmallAmount_HandlesCorrectly()
    {
        // Arrange & Act
        var exchangeRate = new ExchangeRate
        {
            Amount = decimal.MinValue,
            Base = "USD",
            Date = DateTime.UtcNow,
            Rates = new Dictionary<string, decimal> { { "EUR", 0.85m } }
        };

        // Assert
        Assert.Equal(decimal.MinValue, exchangeRate.Amount);
        Assert.Equal(0.85m, exchangeRate.Rates["EUR"]);
    }

    [Fact]
    public void ExchangeRate_WithFutureDate_HandlesCorrectly()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddYears(1);

        // Act
        var exchangeRate = new ExchangeRate
        {
            Amount = 1,
            Base = "USD",
            Date = futureDate,
            Rates = new Dictionary<string, decimal> { { "EUR", 0.85m } }
        };

        // Assert
        Assert.Equal(futureDate, exchangeRate.Date);
    }

    [Fact]
    public void ExchangeRate_WithPastDate_HandlesCorrectly()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddYears(-1);

        // Act
        var exchangeRate = new ExchangeRate
        {
            Amount = 1,
            Base = "USD",
            Date = pastDate,
            Rates = new Dictionary<string, decimal> { { "EUR", 0.85m } }
        };

        // Assert
        Assert.Equal(pastDate, exchangeRate.Date);
    }

    [Fact]
    public void ExchangeRate_WithManyRates_HandlesCorrectly()
    {
        // Arrange
        var rates = new Dictionary<string, decimal>();
        for (int i = 0; i < 1000; i++)
        {
            rates.Add($"CUR{i:D3}", (decimal)(i + 1) / 100);
        }

        // Act
        var exchangeRate = new ExchangeRate
        {
            Amount = 1,
            Base = "USD",
            Date = DateTime.UtcNow,
            Rates = rates
        };

        // Assert
        Assert.Equal(1000, exchangeRate.Rates.Count);
        Assert.Equal(0.01m, exchangeRate.Rates["CUR000"]);
        Assert.Equal(10.00m, exchangeRate.Rates["CUR999"]);
    }

    [Fact]
    public void HistoricalRate_WithValidData_InitializesCorrectly()
    {
        // Arrange
        var amount = 1m;
        var baseCurrency = "USD";
        var startDate = "2023-01-01";
        var endDate = "2023-01-07";
        var rates = new Dictionary<string, Dictionary<string, decimal>>
        {
            { "2023-01-01", new Dictionary<string, decimal> { { "EUR", 0.85m } } },
            { "2023-01-02", new Dictionary<string, decimal> { { "EUR", 0.86m } } }
        };

        // Act
        var historicalRate = new HistoricalRate
        {
            Amount = amount,
            Base = baseCurrency,
            Start_Date = startDate,
            End_Date = endDate,
            Rates = rates
        };

        // Assert
        Assert.Equal(amount, historicalRate.Amount);
        Assert.Equal(baseCurrency, historicalRate.Base);
        Assert.Equal(startDate, historicalRate.Start_Date);
        Assert.Equal(endDate, historicalRate.End_Date);
        Assert.Equal(rates.Count, historicalRate.Rates.Count);
        Assert.Equal(0.85m, historicalRate.Rates["2023-01-01"]["EUR"]);
        Assert.Equal(0.86m, historicalRate.Rates["2023-01-02"]["EUR"]);
    }

    [Fact]
    public void HistoricalRate_WithEmptyRates_InitializesCorrectly()
    {
        // Arrange & Act
        var historicalRate = new HistoricalRate
        {
            Amount = 1,
            Base = "USD",
            Start_Date = "2023-01-01",
            End_Date = "2023-01-07",
            Rates = new Dictionary<string, Dictionary<string, decimal>>()
        };

        // Assert
        Assert.Empty(historicalRate.Rates);
    }

    [Fact]
    public void HistoricalRate_WithSingleDayRange_HandlesCorrectly()
    {
        // Arrange & Act
        var historicalRate = new HistoricalRate
        {
            Amount = 1,
            Base = "USD",
            Start_Date = "2023-01-01",
            End_Date = "2023-01-01",
            Rates = new Dictionary<string, Dictionary<string, decimal>>
            {
                { "2023-01-01", new Dictionary<string, decimal> { { "EUR", 0.85m } } }
            }
        };

        // Assert
        Assert.Equal("2023-01-01", historicalRate.Start_Date);
        Assert.Equal("2023-01-01", historicalRate.End_Date);
        Assert.Single(historicalRate.Rates);
    }

    [Fact]
    public void HistoricalRate_WithLongDateRange_HandlesCorrectly()
    {
        // Arrange
        var rates = new Dictionary<string, Dictionary<string, decimal>>();
        for (int i = 0; i < 365; i++)
        {
            var date = DateTime.Parse("2023-01-01").AddDays(i).ToString("yyyy-MM-dd");
            rates.Add(date, new Dictionary<string, decimal> { { "EUR", 0.85m + (i * 0.001m) } });
        }

        // Act
        var historicalRate = new HistoricalRate
        {
            Amount = 1,
            Base = "USD",
            Start_Date = "2023-01-01",
            End_Date = "2023-12-31",
            Rates = rates
        };

        // Assert
        Assert.Equal(365, historicalRate.Rates.Count);
        Assert.Equal("2023-01-01", historicalRate.Start_Date);
        Assert.Equal("2023-12-31", historicalRate.End_Date);
    }

    [Fact]
    public void UsersData_WithValidData_InitializesCorrectly()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        var roles = new List<string> { "User" };

        var user = new User
        {
            Username = username,
            Password = password,
            Roles = roles
        };


        // Act
        var userData = new UsersData
        {
            Users = new List<User>
            {
                new User
                {
                    Username = username,
                    Password = password,
                    Roles = roles
                }
            }
        };

        // Assert
        Assert.Equal(username, user.Username);
        Assert.Equal(roles.Count, user.Roles.Count);
        Assert.Single(userData.Users);
    }

    [Fact]
    public void UsersData_WithDifferentRoles_HandlesCorrectly()
    {
        // Arrange & Act
        var adminUser = new User
        {
            Username = "admin",
            Password = "adminpassword",
            Roles = new List<string> { "Admin" }
        };

        var regularUser = new User
        {
            Username = "user",
            Password = "userpassword",
            Roles = new List<string> { "User" }
        };

        // Assert
        Assert.Equal("Admin", adminUser.Roles[0]);
        Assert.Equal("User", regularUser.Roles[0]);
    }

    [Fact]
    public void DomainModels_WithNullValues_HandleCorrectly()
    {
        // Arrange & Act
        var exchangeRate = new ExchangeRate
        {
            Amount = 1,
            Base = null!,
            Date = DateTime.UtcNow,
            Rates = null!
        };

        var historicalRate = new HistoricalRate
        {
            Amount = 1,
            Base = null!,
            Start_Date = null!,
            End_Date = null!,
            Rates = null!
        };

        var userData = new UsersData
        {
            Users = new List<User> { }
        };

        // Assert
        Assert.Null(exchangeRate.Base);
        Assert.Null(exchangeRate.Rates);
        Assert.Null(historicalRate.Base);
        Assert.Null(historicalRate.Start_Date);
        Assert.Null(historicalRate.End_Date);
        Assert.Null(historicalRate.Rates);
        Assert.Empty(userData.Users);
    }

    [Fact]
    public void DomainModels_WithSpecialCharacters_HandleCorrectly()
    {
        // Arrange & Act
        var exchangeRate = new ExchangeRate
        {
            Amount = 1,
            Base = "US$",
            Date = DateTime.UtcNow,
            Rates = new Dictionary<string, decimal> { { "EUR€", 0.85m } }
        };

        // Assert
        Assert.Equal("US$", exchangeRate.Base);
        Assert.Equal(0.85m, exchangeRate.Rates["EUR€"]);
    }

    [Fact]
    public void DomainModels_WithUnicodeCharacters_HandleCorrectly()
    {
        // Arrange & Act
        var exchangeRate = new ExchangeRate
        {
            Amount = 1,
            Base = "USD",
            Date = DateTime.UtcNow,
            Rates = new Dictionary<string, decimal> { { "€UR", 0.85m }, { "¥EN", 110.50m } }
        };

        // Assert
        Assert.Equal(0.85m, exchangeRate.Rates["€UR"]);
        Assert.Equal(110.50m, exchangeRate.Rates["¥EN"]);
    }
}
