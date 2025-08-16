using System;
using System.Net.Http;
using CurrencyService.Domain.Interfaces;
using CurrencyService.Infrasturucture.Providers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CurrencyService.Tests.UnitTests.Infrastructure;

public class CurrencyApiFactoryTests
{
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ICurrencyApi> _currencyApiMock;
    private readonly CurrencyApiFactory _currencyApiFactory;

    public CurrencyApiFactoryTests()
    {
        _currencyApiMock = new Mock<ICurrencyApi>();
        _serviceProviderMock = new Mock<IServiceProvider>();

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(FrankfurterCurrencyApi)))
            .Returns(_currencyApiMock.Object);

        _currencyApiFactory = new CurrencyApiFactory(_serviceProviderMock.Object);
    }

    [Fact]
    public void CreateCurrencyApi_WithValidConfiguration_ReturnsFrankfurterCurrencyApi()
    {
        // Act
        var result = _currencyApiFactory.GetSource();

        // Assert
        Assert.Same(_currencyApiMock.Object, result);
        _serviceProviderMock.Verify(x => x.GetService(typeof(FrankfurterCurrencyApi)), Times.Once);
    }

    [Fact]
    public void CreateCurrencyApi_WithUnsupportedProvider_ThrowsNotSupportedException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _currencyApiFactory.GetSource("UnsupportedProvider"));
        Assert.Contains("Source 'UnsupportedProvider' not supported.", exception.Message);
    }

    [Fact]
    public void CreateCurrencyApi_WithNullProvider_ThrowsArgumentException()
    {
        // Act
        var result = _currencyApiFactory.GetSource();

        // Assert
        Assert.Same(_currencyApiMock.Object, result);
        _serviceProviderMock.Verify(x => x.GetService(typeof(FrankfurterCurrencyApi)), Times.Once);
    }

    [Fact]
    public void CreateCurrencyApi_WithEmptyProvider_ThrowsArgumentException()
    {
        // Act
        var result = _currencyApiFactory.GetSource("");

        // Assert
        Assert.Same(_currencyApiMock.Object, result);
        _serviceProviderMock.Verify(x => x.GetService(typeof(FrankfurterCurrencyApi)), Times.Once);
    }

    [Fact]
    public void CreateCurrencyApi_WithWhitespaceProvider_ThrowsArgumentException()
    {
        var result = _currencyApiFactory.GetSource("   ");

        // Assert
        Assert.Same(_currencyApiMock.Object, result);
        _serviceProviderMock.Verify(x => x.GetService(typeof(FrankfurterCurrencyApi)), Times.Once);
    }

    [Theory]
    [InlineData("frankfurt")]
    [InlineData("FRANKFURT")]
    [InlineData("Frankfurt")]
    [InlineData(" frankfurt ")]
    public void CreateCurrencyApi_WithCaseInsensitiveProvider_ReturnsFrankfurterCurrencyApi(string provider)
    {
        // Act
        var result = _currencyApiFactory.GetSource(provider);

        // Assert
        Assert.Same(_currencyApiMock.Object, result);
        _serviceProviderMock.Verify(x => x.GetService(typeof(FrankfurterCurrencyApi)), Times.Once);
    }
}
