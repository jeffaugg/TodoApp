using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApp.Application.DTOs;
using TodoApp.Domain;

namespace TodoApp.Application.Services.Interfaces
{
    public interface IUserService
    {
        Task <Guid> AddAsync(CreateUserDto user);
        Task<User> LoginAsync(string email, string password);
    }
}