using System;
using MediatR;

namespace CurrencyService.Application.Features.Auth.Command;

public class LoginCommandRequest : IRequest<LoginCommandResponse>
{
    public string Username { get; set; }
    public string Password { get; set; }
}
