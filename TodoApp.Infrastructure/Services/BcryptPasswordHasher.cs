using System;
using TodoApp.Application.Services.Interfaces;

namespace TodoApp.Infrastructure.Services;

public class BcryptPasswordHasher : IPasswordHasher
{
    public Task<string> HashPasswordAsync(string password)
    {
        return Task.FromResult(BCrypt.Net.BCrypt.HashPassword(password));
    }

    public Task<bool> VerifyPasswordAsync(string password, string passwordHash)
    {
        return Task.FromResult(BCrypt.Net.BCrypt.Verify(password, passwordHash));
    }
}