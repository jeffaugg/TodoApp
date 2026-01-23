namespace TodoApp.Api.DTOs;

public record LoginResponseDto(
    string Message,
    string Token,
    Guid UserId,
    string Name
);
