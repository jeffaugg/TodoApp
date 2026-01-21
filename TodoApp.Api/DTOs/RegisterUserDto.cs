using System;

namespace TodoApp.Api.DTOs;

public record RegisterUserDto(string Name, string Email, string Password);