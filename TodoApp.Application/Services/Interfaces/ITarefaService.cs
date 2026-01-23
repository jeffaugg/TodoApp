using System;
using TodoApp.Application.DTOs;
using TodoApp.Domain;

namespace TodoApp.Application.Services.Interfaces;

public interface ITarefaService
{
    Task<Tarefa> AdicionarAsync(Guid userId, CreateTarefaDto dto);
    
    Task<IEnumerable<Tarefa>> ListarDoUsuarioAsync(Guid userId);
    Task<Tarefa?> AtualizarAsync(Guid userId, Guid id, string novoTitulo, bool concluida);
    Task<bool> DeletarAsync(Guid userId, Guid id);
}
