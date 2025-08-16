using System;

namespace CurrencyService.Domain.Models;

public class UsersData
{
    public List<User> Users { get; set; } = new List<User>();
}

public class User
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new List<string>();
}
