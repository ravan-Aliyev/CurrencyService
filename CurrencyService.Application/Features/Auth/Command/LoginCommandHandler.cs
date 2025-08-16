using System;
using CurrencyService.Application.Abstractions;
using CurrencyService.Domain.Exceptions;
using CurrencyService.Domain.Models;
using MediatR;
using Microsoft.Extensions.Options;

namespace CurrencyService.Application.Features.Auth.Command;

public class LoginCommandHandler : IRequestHandler<LoginCommandRequest, LoginCommandResponse>
{
    private readonly ITokenService _tokenService;
    private readonly UsersData _usersData;

    public LoginCommandHandler(ITokenService tokenService, IOptions<UsersData> usersDataOptions)
    {
        _usersData = usersDataOptions.Value;
        _tokenService = tokenService;
    }

    public async Task<LoginCommandResponse> Handle(LoginCommandRequest request, CancellationToken cancellationToken)
    {
        var result = await new LoginCommandValidator().ValidateAsync(request, cancellationToken);
        if (!result.IsValid)
            throw new CustomValidationException(result.Errors);

        var user = _usersData.Users.FirstOrDefault(u => u.Username == request.Username && u.Password == request.Password) ?? throw new UnauthorizedAccessException("Invalid username or password.");

        var userId = Guid.NewGuid().ToString();

        var token = _tokenService.GenerateToken(userId, request.Username, user.Roles);

        return new LoginCommandResponse
        {
            Token = token,
            Roles = user.Roles
        };
    }
}
