using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CurrencyService.Domain.Models;
using CurrencyService.Infrasturucture.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;
using System.Collections.Generic;

namespace CurrencyService.Tests.UnitTests.Infrastructure;

public class JwtTokenServiceTests
{
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly JwtTokenService _jwtTokenService;
    private const string TestSecretKey = "test-secret-key-that-is-long-enough-for-testing-purposes-only";
    private const string TestIssuer = "test-issuer";
    private const string TestAudience = "test-audience";

    public JwtTokenServiceTests()
    {
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(x => x["Jwt:Key"]).Returns(TestSecretKey);
        _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns(TestIssuer);
        _configurationMock.Setup(x => x["Jwt:Audience"]).Returns(TestAudience);

        _jwtTokenService = new JwtTokenService(_configurationMock.Object);
    }

    [Fact]
    public void GenerateToken_WithValidUser_ReturnsValidJwtToken()
    {
        // Arrange
        var user = new User
        {
            Username = "testuser",
            Password = "password123",
            Roles = new List<string>() { "User" }
        };

        var id = Guid.NewGuid().ToString();

        // Act
        var token = _jwtTokenService.GenerateToken(id, user.Username, user.Roles);

        // Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);
    }
}
