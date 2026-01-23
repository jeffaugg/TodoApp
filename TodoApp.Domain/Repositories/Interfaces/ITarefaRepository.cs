using System;

namespace TodoApp.Domain.Repositories.Interfaces;

public interface ITarefaRepository
{
    Task<Tarefa> AddAsync (Tarefa tarefa);
    Task<IEnumerable<Tarefa>> GetAllByUserIdAsync (Guid userId);
    Task<Tarefa?> GetByIdAsync (Guid id); 
    Task UpdateAsync (Tarefa tarefa);
    Task DeleteAsync(Tarefa tarefa);
}
