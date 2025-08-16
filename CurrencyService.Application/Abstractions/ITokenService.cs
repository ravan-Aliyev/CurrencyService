using System;

namespace CurrencyService.Application.Abstractions;

public interface ITokenService
{
    string GenerateToken(string userId, string username, IEnumerable<string> roles);
}
