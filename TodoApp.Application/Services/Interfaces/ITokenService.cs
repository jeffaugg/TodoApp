using System;
using TodoApp.Domain;

namespace TodoApp.Application.Services.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}   
