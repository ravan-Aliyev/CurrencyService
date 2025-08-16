using System;

namespace CurrencyService.Application.Features.Auth.Command;

public class LoginCommandResponse
{
    public string Token { get; set; }
    public IEnumerable<string> Roles { get; set; }
}
