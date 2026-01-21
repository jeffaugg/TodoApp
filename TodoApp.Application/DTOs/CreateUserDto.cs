using System.ComponentModel.DataAnnotations;

namespace TodoApp.Application.DTOs;

public record CreateUserDto(string Name, string Email, string Password);