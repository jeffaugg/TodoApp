using TodoApp.Domain;

namespace TodoApp.Api.DTOs;

public record TarefaResponseDto(
    Guid Id,
    string Titulo,
    string Descricao,
    DateTime DataCriacao,
    StatusTarefa Status,
    Guid UserId
);
