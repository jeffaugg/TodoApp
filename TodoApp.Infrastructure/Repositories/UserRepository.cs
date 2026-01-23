using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Telemetry;
using TodoApp.Domain;
using TodoApp.Domain.Repositories.Interfaces;
using TodoApp.Infrastructure.Contexts;

namespace TodoApp.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        public async Task<User?> GetByEmailAsync(string email)
        {
            using var activitySource = TelemetrySetup.activitySource.StartActivity("UserRepository.GetByEmailAsync");

            activitySource?.SetTag("step", "get_by_email_async_repository");

            return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddAsync(User user)
        {
            using var activitySource = TelemetrySetup.activitySource.StartActivity("UserRepository.AddAsync");

            activitySource?.SetTag("step", "add_async_repository");

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            TelemetrySetup.UsersCreatedCounter.Add(1, new KeyValuePair<string, object?>("user_name", user.Name));
        }
    }
}