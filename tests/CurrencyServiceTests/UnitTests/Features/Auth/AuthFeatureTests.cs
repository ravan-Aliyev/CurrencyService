using System;
using System.Threading;
using System.Threading.Tasks;
using CurrencyService.Application.Features.Auth.Command;
using CurrencyService.Application.Abstractions;
using CurrencyService.Domain.Exceptions;
using CurrencyService.Domain.Models;
using MediatR;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using System.Collections.Generic;
using System.Linq;

namespace CurrencyService.Tests.UnitTests.Features.Auth;

public class AuthFeatureTests
{
    private readonly Mock<ITokenService> _tokenServiceMock;
    private readonly Mock<IOptions<UsersData>> _usersDataOptionsMock;
    private readonly UsersData _usersData;

    public AuthFeatureTests()
    {
        _tokenServiceMock = new Mock<ITokenService>();
        _usersDataOptionsMock = new Mock<IOptions<UsersData>>();
        
        // Setup default users data
        _usersData = new UsersData
        {
            Users = new List<User>
            {
                new User
                {
                    Username = "testuser",
                    Password = "testpassword",
                    Roles = new List<string> { "User" }
                },
                new User
                {
                    Username = "adminuser",
                    Password = "adminpassword",
                    Roles = new List<string> { "Admin", "User" }
                }
            }
        };
        
        _usersDataOptionsMock.Setup(x => x.Value).Returns(_usersData);
    }

    [Fact]
    public async Task LoginCommand_WithValidCredentials_ReturnsLoginResponse()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = "testuser",
            Password = "testpassword"
        };

        var expectedToken = "jwt-token-here";
        var expectedRoles = new List<string> { "User" };

        _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<string>(), request.Username, expectedRoles))
            .Returns(expectedToken);

        var handler = new LoginCommandHandler(_tokenServiceMock.Object, _usersDataOptionsMock.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedToken, result.Token);
        Assert.Equal(expectedRoles, result.Roles);

        _tokenServiceMock.Verify(x => x.GenerateToken(It.IsAny<string>(), request.Username, expectedRoles), Times.Once);
    }

    [Fact]
    public async Task LoginCommand_WithAdminCredentials_ReturnsAdminRoles()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = "adminuser",
            Password = "adminpassword"
        };

        var expectedToken = "admin-jwt-token";
        var expectedRoles = new List<string> { "Admin", "User" };

        _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<string>(), request.Username, expectedRoles))
            .Returns(expectedToken);

        var handler = new LoginCommandHandler(_tokenServiceMock.Object, _usersDataOptionsMock.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedToken, result.Token);
        Assert.Equal(expectedRoles, result.Roles);
        Assert.Equal(2, result.Roles.Count());
        Assert.Contains("Admin", result.Roles);
        Assert.Contains("User", result.Roles);
    }

    [Fact]
    public async Task LoginCommand_WithInvalidCredentials_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = "invaliduser",
            Password = "invalidpassword"
        };

        var handler = new LoginCommandHandler(_tokenServiceMock.Object, _usersDataOptionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(request, CancellationToken.None));

        Assert.Equal("Invalid username or password.", exception.Message);
    }

    [Fact]
    public async Task LoginCommand_WithWrongPassword_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        var handler = new LoginCommandHandler(_tokenServiceMock.Object, _usersDataOptionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(request, CancellationToken.None));

        Assert.Equal("Invalid username or password.", exception.Message);
    }

    [Fact]
    public async Task LoginCommand_WithNonExistentUsername_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = "nonexistentuser",
            Password = "anypassword"
        };

        var handler = new LoginCommandHandler(_tokenServiceMock.Object, _usersDataOptionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(request, CancellationToken.None));

        Assert.Equal("Invalid username or password.", exception.Message);
    }

    [Fact]
    public async Task LoginCommand_WithEmptyUsername_ThrowsCustomValidationException()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = "",
            Password = "testpassword"
        };

        var handler = new LoginCommandHandler(_tokenServiceMock.Object, _usersDataOptionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomValidationException>(
            () => handler.Handle(request, CancellationToken.None));

        Assert.NotNull(exception);
        Assert.NotNull(exception.Errors);
    }

    [Fact]
    public async Task LoginCommand_WithEmptyPassword_ThrowsCustomValidationException()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = "testuser",
            Password = ""
        };

        var handler = new LoginCommandHandler(_tokenServiceMock.Object, _usersDataOptionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomValidationException>(
            () => handler.Handle(request, CancellationToken.None));

        Assert.NotNull(exception);
        Assert.NotNull(exception.Errors);
    }

    [Fact]
    public async Task LoginCommand_WithNullUsername_ThrowsCustomValidationException()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = null!,
            Password = "testpassword"
        };

        var handler = new LoginCommandHandler(_tokenServiceMock.Object, _usersDataOptionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomValidationException>(
            () => handler.Handle(request, CancellationToken.None));

        Assert.NotNull(exception);
        Assert.NotNull(exception.Errors);
    }

    [Fact]
    public async Task LoginCommand_WithNullPassword_ThrowsCustomValidationException()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = "testuser",
            Password = null!
        };

        var handler = new LoginCommandHandler(_tokenServiceMock.Object, _usersDataOptionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomValidationException>(
            () => handler.Handle(request, CancellationToken.None));

        Assert.NotNull(exception);
        Assert.NotNull(exception.Errors);
    }

    [Fact]
    public async Task LoginCommand_WithWhitespaceUsername_ThrowsCustomValidationException()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = "   ",
            Password = "testpassword"
        };

        var handler = new LoginCommandHandler(_tokenServiceMock.Object, _usersDataOptionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomValidationException>(
            () => handler.Handle(request, CancellationToken.None));

        Assert.NotNull(exception);
        Assert.NotNull(exception.Errors);
    }

    [Fact]
    public async Task LoginCommand_WithWhitespacePassword_ThrowsCustomValidationException()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = "testuser",
            Password = "   "
        };

        var handler = new LoginCommandHandler(_tokenServiceMock.Object, _usersDataOptionsMock.Object);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomValidationException>(
            () => handler.Handle(request, CancellationToken.None));

        Assert.NotNull(exception);
        Assert.NotNull(exception.Errors);
    }

    [Fact]
    public async Task LoginCommand_WithUserHavingNoRoles_ReturnsEmptyRoles()
    {
        // Arrange
        var userWithNoRoles = new User
        {
            Username = "norolesuser",
            Password = "password",
            Roles = new List<string>()
        };

        var usersDataWithNoRoles = new UsersData
        {
            Users = new List<User> { userWithNoRoles }
        };

        var usersDataOptionsMock = new Mock<IOptions<UsersData>>();
        usersDataOptionsMock.Setup(x => x.Value).Returns(usersDataWithNoRoles);

        var request = new LoginCommandRequest
        {
            Username = "norolesuser",
            Password = "password"
        };

        var expectedToken = "token-for-no-roles-user";

        _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<string>(), request.Username, new List<string>()))
            .Returns(expectedToken);

        var handler = new LoginCommandHandler(_tokenServiceMock.Object, usersDataOptionsMock.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedToken, result.Token);
        Assert.Empty(result.Roles);
    }

    [Fact]
    public async Task LoginCommand_WithUserHavingMultipleRoles_ReturnsAllRoles()
    {
        // Arrange
        var userWithMultipleRoles = new User
        {
            Username = "multiroleuser",
            Password = "password",
            Roles = new List<string> { "User", "Moderator", "Editor" }
        };

        var usersDataWithMultipleRoles = new UsersData
        {
            Users = new List<User> { userWithMultipleRoles }
        };

        var usersDataOptionsMock = new Mock<IOptions<UsersData>>();
        usersDataOptionsMock.Setup(x => x.Value).Returns(usersDataWithMultipleRoles);

        var request = new LoginCommandRequest
        {
            Username = "multiroleuser",
            Password = "password"
        };

        var expectedToken = "token-for-multi-role-user";
        var expectedRoles = new List<string> { "User", "Moderator", "Editor" };

        _tokenServiceMock.Setup(x => x.GenerateToken(It.IsAny<string>(), request.Username, expectedRoles))
            .Returns(expectedToken);

        var handler = new LoginCommandHandler(_tokenServiceMock.Object, usersDataOptionsMock.Object);

        // Act
        var result = await handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedToken, result.Token);
        Assert.Equal(3, result.Roles.Count());
        Assert.Contains("User", result.Roles);
        Assert.Contains("Moderator", result.Roles);
        Assert.Contains("Editor", result.Roles);
    }
}
