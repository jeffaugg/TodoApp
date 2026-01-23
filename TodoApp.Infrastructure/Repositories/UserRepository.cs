using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoApp.Api.Telemetry;
using TodoApp.Domain;
using TodoApp.Domain.Repositories.Interfaces;
using TodoApp.Infrastructure.Contexts;
using TodoApp.Telemetry.Attributes;

namespace TodoApp.Infrastructure.Repositories
{
    [Telemetry]
    public class UserRepository(AppDbContext context) : IUserRepository
    {
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddAsync(User user)
        {
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
            TelemetrySetup.UsersCreatedCounter.Add(1, new KeyValuePair<string, object?>("user_name", user.Name));
        }
    }
}