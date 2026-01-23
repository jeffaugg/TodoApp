using System;
using TodoApp.Api.Telemetry;
using TodoApp.Application.Services.Interfaces;
using TodoApp.Telemetry.Attributes;

namespace TodoApp.Infrastructure.Services;


[Telemetry]
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