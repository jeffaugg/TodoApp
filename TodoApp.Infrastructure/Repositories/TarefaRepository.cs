using System;
using Microsoft.EntityFrameworkCore;
using TodoApp.Domain;
using TodoApp.Domain.Repositories.Interfaces;
using TodoApp.Infrastructure.Contexts;

namespace TodoApp.Infrastructure.Repositories;

public class TarefaRepository(AppDbContext context) : ITarefaRepository
{
    public async Task<Tarefa> AddAsync(Tarefa tarefa)
    {
        await context.Tarefas.AddAsync(tarefa);
        await context.SaveChangesAsync();
        return tarefa;
    }

    public async Task<IEnumerable<Tarefa>> GetAllByUserIdAsync(Guid userId)
    {
        return await context.Tarefas
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.DataCriacao)
            .ToListAsync();
    }

    public async Task<Tarefa?> GetByIdAsync(Guid id)
    {
        return await context.Tarefas.FindAsync(id);
    }

    public async Task UpdateAsync(Tarefa tarefa)
    {
        context.Tarefas.Update(tarefa);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Tarefa tarefa)
    {
        context.Tarefas.Remove(tarefa);
        await context.SaveChangesAsync();
    }
}