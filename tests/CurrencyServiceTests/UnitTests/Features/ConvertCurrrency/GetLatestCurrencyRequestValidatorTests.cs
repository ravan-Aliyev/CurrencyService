using CurrencyService.Application.Features.ConvertCurrency.Query.GetLatestCurrencyQuery;

namespace CurrencyService.Tests.UnitTests.Features.ConvertCurrrency;

public class GetLatestCurrencyRequestValidatorTests
{
    private readonly GetLatestCurrencyRequestValidator _validator = new();

    [Theory]
    [InlineData("USD")]
    [InlineData("eur")]
    [InlineData("A1$")]
    public async Task Validate_ExactThreeCharacters_IsValid(string baseCurrency)
    {
        var request = new GetLatestCurrencyQueryRequest { BaseCurrency = baseCurrency };

        var result = await _validator.ValidateAsync(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_WhitespaceOnly_IsInvalid_WithNotEmptyMessage()
    {
        var request = new GetLatestCurrencyQueryRequest { BaseCurrency = "   " };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.BaseCurrency) && e.ErrorMessage == "Base currency cannot be empty.");
    }

    [Fact]
    public async Task Validate_Empty_IsInvalid_WithNotEmptyMessage()
    {
        var request = new GetLatestCurrencyQueryRequest { BaseCurrency = string.Empty };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.BaseCurrency) && e.ErrorMessage == "Base currency cannot be empty.");
    }

    [Fact]
    public async Task Validate_Null_IsInvalid_WithNotEmptyMessage()
    {
        var request = new GetLatestCurrencyQueryRequest { BaseCurrency = null! };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.BaseCurrency) && e.ErrorMessage == "Base currency cannot be empty.");
    }

    [Theory]
    [InlineData("US")]   // 2 chars
    [InlineData("U")]    // 1 char
    public async Task Validate_TooShort_IsInvalid_WithLengthMessage(string baseCurrency)
    {
        var request = new GetLatestCurrencyQueryRequest { BaseCurrency = baseCurrency };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.BaseCurrency) && e.ErrorMessage == "Base currency must be exactly 3 characters long.");
    }

    [Theory]
    [InlineData("USDX")] // 4 chars
    [InlineData("ABCDE")] // 5 chars
    public async Task Validate_TooLong_IsInvalid_WithLengthMessage(string baseCurrency)
    {
        var request = new GetLatestCurrencyQueryRequest { BaseCurrency = baseCurrency };

        var result = await _validator.ValidateAsync(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(request.BaseCurrency) && e.ErrorMessage == "Base currency must be exactly 3 characters long.");
    }
}
