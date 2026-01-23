using System;
using TodoApp.Api.Telemetry;
using TodoApp.Application.Services.Interfaces;

namespace TodoApp.Infrastructure.Services;

public class BcryptPasswordHasher : IPasswordHasher
{
    public Task<string> HashPasswordAsync(string password)
    {
        using var activitySource = TelemetrySetup.activitySource.StartActivity("BcryptPasswordHasher.HashPasswordAsync");

        activitySource?.SetTag("step", "hash_password_service");


        return Task.FromResult(BCrypt.Net.BCrypt.HashPassword(password));
    }

    public Task<bool> VerifyPasswordAsync(string password, string passwordHash)
    {
        using var activitySource = TelemetrySetup.activitySource.StartActivity("BcryptPasswordHasher.HashPasswordAsync");

        activitySource?.SetTag("step", "verify_password_async_service");

        return Task.FromResult(BCrypt.Net.BCrypt.Verify(password, passwordHash));
    }
}