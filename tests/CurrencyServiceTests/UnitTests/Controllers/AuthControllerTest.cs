using System;
using System.Threading;
using System.Threading.Tasks;
using CurrencyService.Api.Controllers;
using CurrencyService.Application.Features.Auth.Command;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace CurrencyService.Tests.UnitTests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AuthController(_mediatorMock.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkResult()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = "testuser",
            Password = "testpassword"
        };

        var expectedResponse = new LoginCommandResponse
        {
            Token = "jwt-token-here",
            Roles = new[] { "User" }
        };

        _mediatorMock.Setup(m => m.Send(It.Is<LoginCommandRequest>(x =>
            x.Username == request.Username &&
            x.Password == request.Password), default))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var actualResponse = Assert.IsType<LoginCommandResponse>(okResult.Value);

        Assert.Equal(expectedResponse.Roles.Count(), actualResponse.Roles.Count());
        Assert.Equal(expectedResponse.Token, actualResponse.Token);

        _mediatorMock.Verify(m => m.Send(It.IsAny<LoginCommandRequest>(), default), Times.Once);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = "invaliduser",
            Password = "invalidpassword"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommandRequest>(), default))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid username or password"));

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _controller.Login(request));

        Assert.Equal("Invalid username or password", exception.Message);
    }


    [Fact]
    public async Task Login_WithEmptyUsername_ThrowsArgumentException()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = "",
            Password = "password"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommandRequest>(), default))
            .ThrowsAsync(new ArgumentException("Username cannot be empty"));

        // Act
       var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _controller.Login(request));

        Assert.Equal("Username cannot be empty", exception.Message);
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ThrowsArgumentException()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = "username",
            Password = ""
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommandRequest>(), default))
            .ThrowsAsync(new ArgumentException("Password cannot be empty"));

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _controller.Login(request));

        Assert.Equal("Password cannot be empty", exception.Message);
    }

    [Fact]
    public async Task AuthController_WithGenericException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = "testuser",
            Password = "testpassword"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommandRequest>(), default))
            .ThrowsAsync(new Exception("Unexpected error occurred"));

        // Act
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _controller.Login(request));

        Assert.Equal("Unexpected error occurred", exception.Message);
    }

    [Fact]
    public async Task AuthController_WithValidationException_ThrowsException()
    {
        // Arrange
        var request = new LoginCommandRequest
        {
            Username = "testuser",
            Password = "testpassword"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommandRequest>(), default))
            .ThrowsAsync(new FluentValidation.ValidationException("Validation failed"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _controller.Login(request));

        Assert.Equal("Validation failed", exception.Message);
    }
}
