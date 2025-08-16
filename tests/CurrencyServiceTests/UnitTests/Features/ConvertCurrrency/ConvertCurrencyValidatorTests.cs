using System;
using System.Linq;
using CurrencyService.Application.Features.ConvertCurrency.Command.ConvertCurrencyCommand;
using FluentValidation.TestHelper;
using Xunit;

namespace CurrencyService.Tests.UnitTests.Features.ConvertCurrrency;

public class ConvertCurrencyValidatorTests
{
    private readonly ConvertCurrencyValidator _validator;

    public ConvertCurrencyValidatorTests()
    {
        _validator = new ConvertCurrencyValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = "USD",
            To = "EUR",
            Amount = 100
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidFromCurrency_ShouldHaveValidationError(string fromCurrency)
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = fromCurrency,
            To = "EUR",
            Amount = 100
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.From);
        result.ShouldHaveValidationErrorFor(x => x.From)
            .WithErrorMessage("From currency cannot be empty.");
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("A")]
    [InlineData("ABCD")]
    public void Validate_WithInvalidFromCurrencyLength_ShouldHaveValidationError(string fromCurrency)
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = fromCurrency,
            To = "EUR",
            Amount = 100
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.From);
        result.ShouldHaveValidationErrorFor(x => x.From)
            .WithErrorMessage("Base currency must be exactly 3 characters long.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithInvalidToCurrency_ShouldHaveValidationError(string toCurrency)
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = "USD",
            To = toCurrency,
            Amount = 100
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.To);
        result.ShouldHaveValidationErrorFor(x => x.To)
            .WithErrorMessage("To currency cannot be empty.");
    }

    [Theory]
    [InlineData("EU")]
    [InlineData("EURO")]
    [InlineData("A")]
    [InlineData("ABCD")]
    public void Validate_WithInvalidToCurrencyLength_ShouldHaveValidationError(string toCurrency)
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = "USD",
            To = toCurrency,
            Amount = 100
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.To);
        result.ShouldHaveValidationErrorFor(x => x.To)
            .WithErrorMessage("Base currency must be exactly 3 characters long.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Validate_WithInvalidAmount_ShouldHaveValidationError(decimal amount)
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = "USD",
            To = "EUR",
            Amount = amount
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Amount);
        result.ShouldHaveValidationErrorFor(x => x.Amount)
            .WithErrorMessage("Amount must be greater than zero.");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(999999.99)]
    public void Validate_WithValidAmount_ShouldNotHaveValidationError(decimal amount)
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = "USD",
            To = "EUR",
            Amount = amount
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Theory]
    [InlineData("USD", "EUR", 100)]
    [InlineData("EUR", "GBP", 50.75)]
    [InlineData("GBP", "JPY", 200.99)]
    [InlineData("JPY", "CAD", 1000)]
    public void Validate_WithValidCurrencyCodes_ShouldNotHaveValidationErrors(string from, string to, decimal amount)
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = from,
            To = to,
            Amount = amount
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithMultipleValidationErrors_ShouldHaveAllErrors()
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = "", // Invalid
            To = "EU", // Invalid length
            Amount = -50 // Invalid
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.From);
        result.ShouldHaveValidationErrorFor(x => x.To);
        result.ShouldHaveValidationErrorFor(x => x.Amount);

        var errors = result.Errors.ToList();
        Assert.Equal(6, errors.Count);
    }

    [Fact]
    public void Validate_WithEdgeCaseAmount_ShouldNotHaveValidationError()
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = "USD",
            To = "EUR",
            Amount = 0.0000000000000000000000000001m // Very small positive amount
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void Validate_WithSpecialCharactersInCurrency_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ConvertCurrencyRequest
        {
            From = "US$",
            To = "EUR",
            Amount = 100
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.From);
        result.ShouldHaveValidationErrorFor(x => x.From)
            .WithErrorMessage("From currency must consist of 3 uppercase letters.");
    }
}
