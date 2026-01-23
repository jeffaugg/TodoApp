namespace TodoApp.Api.DTOs;

public record UserResponseDto(
    Guid Id,
    string Name,
    string Email
);
