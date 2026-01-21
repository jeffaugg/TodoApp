using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TodoApp.Domain;
using TodoApp.Domain.Repositories.Interfaces;
using TodoApp.Infrastructure.Contexts;

namespace TodoApp.Infrastructure.Repositories
{
    public class UserRepository(AppDbContext context): IUserRepository
    {
        public async Task<User?> GetByEmailAsync(string email)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddAsync(User user){
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();
        }
    }
}