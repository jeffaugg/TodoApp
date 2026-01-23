using TodoApp.Application.DTOs;
using TodoApp.Application.Services.Interfaces;
using TodoApp.Domain;
using TodoApp.Domain.Repositories.Interfaces;
using TodoApp.Telemetry.Attributes;

namespace TodoApp.Application.Services;

[Telemetry]
public class UserService(IUserRepository userRepository, IPasswordHasher passwordHasher) : IUserService
{
    public async Task<Guid> AddAsync(CreateUserDto user)
    {
        var hasUserEmail = await userRepository.GetByEmailAsync(user.Email);
        if (hasUserEmail != null) throw new Exception("Email already in use");

        var hashedPassword = await passwordHasher.HashPasswordAsync(user.Password);
        var newUser = new User(user.Name, user.Email, hashedPassword);

        await userRepository.AddAsync(newUser);
        return newUser.Id;
    }

    public async Task<User> LoginAsync(string email, string password)
    {
        var user = await userRepository.GetByEmailAsync(email);

        if (user == null)
        {
            throw new Exception("Usuário ou senha inválidos");
        }

        var passwordMatches = await passwordHasher.VerifyPasswordAsync(password, user.PasswordHash);

        if (!passwordMatches)
        {
            throw new Exception("Usuário ou senha inválidos");
        }

        return user;
    }
}