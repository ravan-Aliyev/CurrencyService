using CurrencyService.Application.Behaviours;
using CurrencyService.Domain.Constants;
using CurrencyService.Domain.Exceptions;
using CurrencyService.Domain.Interfaces;
using MediatR;
using Moq;

namespace CurrencyService.Tests.UnitTests.Behaviours;

public class CurrencyValidationBehaviourTests
{
    private readonly CurrencyValidationBehaviour<TestCurrencyRequest, TestResponse> _behaviour;
    private readonly Mock<RequestHandlerDelegate<TestResponse>> _mockNext;
    private readonly CancellationToken _cancellationToken;

    public CurrencyValidationBehaviourTests()
    {
        _behaviour = new CurrencyValidationBehaviour<TestCurrencyRequest, TestResponse>();
        _mockNext = new Mock<RequestHandlerDelegate<TestResponse>>();
        _cancellationToken = CancellationToken.None;
    }

    private static CurrencyValidationBehaviour<TRequest, TestResponse> CreateBehaviour<TRequest>()
        where TRequest : IRequest<TestResponse>
        => new CurrencyValidationBehaviour<TRequest, TestResponse>();

    public class TestCurrencyRequest : IRequest<TestResponse>, ICurrencyRequest
    {
        public string? BaseCurrency { get; init; }
        public string? From { get; init; }
        public string? To { get; init; }
        public string? OtherProperty { get; init; }
    }

    public class TestNonCurrencyRequest : IRequest<TestResponse>
    {
        public string? SomeProperty { get; init; }
    }

    public class TestNullCurrencyRequest : IRequest<TestResponse>, ICurrencyRequest
    {
        public string? BaseCurrency { get; init; } = null;
        public string? From { get; init; } = null;
        public string? To { get; init; } = null;
    }

    public class TestResponse
    {
        public string Message { get; init; } = "Success";
    }

    [Fact]
    public async Task Handle_WithValidCurrencies_ShouldCallNextAndReturnResponse()
    {
        var request = new TestCurrencyRequest { BaseCurrency = "USD", From = "EUR", To = "GBP" };
        var expectedResponse = new TestResponse { Message = "Test Response" };
        _mockNext.Setup(x => x(It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

        var result = await _behaviour.Handle(request, _mockNext.Object, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Message, result.Message);
        _mockNext.Verify(x => x(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMixedCaseCurrencies_ShouldPassValidation()
    {
        var request = new TestCurrencyRequest { BaseCurrency = "usd", From = "Eur", To = "gBp" };
        var expectedResponse = new TestResponse { Message = "Success" };
        _mockNext.Setup(x => x(It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

        var result = await _behaviour.Handle(request, _mockNext.Object, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Message, result.Message);
        _mockNext.Verify(x => x(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithBlockedBaseCurrency_ShouldThrowUnsupportedCurrencyException()
    {
        var request = new TestCurrencyRequest { BaseCurrency = "TRY", From = "USD", To = "EUR" };
        _mockNext.Setup(x => x(It.IsAny<CancellationToken>())).ReturnsAsync(new TestResponse());

        var exception = await Assert.ThrowsAsync<UnsupportedCurrencyException>(
            () => _behaviour.Handle(request, _mockNext.Object, _cancellationToken));

        Assert.Equal("The currency code 'TRY' is not supported.", exception.Message);
        _mockNext.Verify(x => x(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithBlockedFromCurrency_ShouldThrowUnsupportedCurrencyException()
    {
        var request = new TestCurrencyRequest { BaseCurrency = "USD", From = "PLN", To = "EUR" };
        _mockNext.Setup(x => x(It.IsAny<CancellationToken>())).ReturnsAsync(new TestResponse());

        var exception = await Assert.ThrowsAsync<UnsupportedCurrencyException>(
            () => _behaviour.Handle(request, _mockNext.Object, _cancellationToken));

        Assert.Equal("The currency code 'PLN' is not supported.", exception.Message);
        _mockNext.Verify(x => x(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithBlockedToCurrency_ShouldThrowUnsupportedCurrencyException()
    {
        var request = new TestCurrencyRequest { BaseCurrency = "USD", From = "EUR", To = "THB" };
        _mockNext.Setup(x => x(It.IsAny<CancellationToken>())).ReturnsAsync(new TestResponse());

        var exception = await Assert.ThrowsAsync<UnsupportedCurrencyException>(
            () => _behaviour.Handle(request, _mockNext.Object, _cancellationToken));

        Assert.Equal("The currency code 'THB' is not supported.", exception.Message);
        _mockNext.Verify(x => x(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNullCurrencyProperties_ShouldPassValidation()
    {
        var request = new TestNullCurrencyRequest();
        var behaviour = CreateBehaviour<TestNullCurrencyRequest>();
        var expectedResponse = new TestResponse { Message = "Success" };
        _mockNext.Setup(x => x(It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

        var result = await behaviour.Handle(request, _mockNext.Object, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Message, result.Message);
        _mockNext.Verify(x => x(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNonCurrencyRequest_ShouldCallNextAndReturnResponse()
    {
        var request = new TestNonCurrencyRequest { SomeProperty = "Test Value" };
        var behaviour = CreateBehaviour<TestNonCurrencyRequest>();
        var expectedResponse = new TestResponse { Message = "Success" };
        _mockNext.Setup(x => x(It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

        var result = await behaviour.Handle(request, _mockNext.Object, _cancellationToken);

        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Message, result.Message);
        _mockNext.Verify(x => x(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithCancelledToken_ShouldPassCancellationToNext()
    {
        var request = new TestCurrencyRequest { BaseCurrency = "USD", From = "EUR", To = "GBP" };
        var cts = new CancellationTokenSource();
        cts.Cancel();
        var expectedResponse = new TestResponse { Message = "Success" };
        _mockNext.Setup(x => x(It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

        var result = await _behaviour.Handle(request, _mockNext.Object, cts.Token);

        Assert.NotNull(result);
        Assert.Equal(expectedResponse.Message, result.Message);
        _mockNext.Verify(x => x(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenNextThrowsException_ShouldPropagateException()
    {
        var request = new TestCurrencyRequest { BaseCurrency = "USD", From = "EUR", To = "GBP" };
        var expectedException = new InvalidOperationException("Test exception");
        _mockNext.Setup(x => x(It.IsAny<CancellationToken>())).ThrowsAsync(expectedException);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _behaviour.Handle(request, _mockNext.Object, _cancellationToken));

        Assert.Same(expectedException, exception);
        Assert.Equal("Test exception", exception.Message);
    }

    [Fact]
    public void BlockedCurrencies_ShouldContainExpectedCurrencies()
    {
        Assert.Contains("TRY", CurrencyConstants.BlockedCurrencies);
        Assert.Contains("PLN", CurrencyConstants.BlockedCurrencies);
        Assert.Contains("THB", CurrencyConstants.BlockedCurrencies);
        Assert.Contains("MXN", CurrencyConstants.BlockedCurrencies);
        Assert.Equal(4, CurrencyConstants.BlockedCurrencies.Count);
    }

    [Fact]
    public void BlockedCurrencies_ShouldNotContainCommonCurrencies()
    {
        Assert.DoesNotContain("USD", CurrencyConstants.BlockedCurrencies);
        Assert.DoesNotContain("EUR", CurrencyConstants.BlockedCurrencies);
        Assert.DoesNotContain("GBP", CurrencyConstants.BlockedCurrencies);
        Assert.DoesNotContain("JPY", CurrencyConstants.BlockedCurrencies);
    }

    [Fact]
    public void UnsupportedCurrencyException_ShouldHaveCorrectMessage()
    {
        var currencyCode = "TEST";
        var exception = new UnsupportedCurrencyException(currencyCode);
        Assert.Equal($"The currency code '{currencyCode}' is not supported.", exception.Message);
    }
}
