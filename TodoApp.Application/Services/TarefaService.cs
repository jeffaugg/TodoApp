using System;
using TodoApp.Application.DTOs;
using TodoApp.Application.Services.Interfaces;
using TodoApp.Domain;
using TodoApp.Domain.Repositories.Interfaces;

namespace TodoApp.Application.Services;

public class TarefaService(ITarefaRepository tarefaRepository) : ITarefaService
{
    public async Task<Tarefa> AdicionarAsync(Guid userId, CreateTarefaDto dto)
    {
        var novaTarefa = new Tarefa(dto.Titulo, dto.Descricao, userId);
        return await tarefaRepository.AddAsync(novaTarefa);
    }

    public async Task<IEnumerable<Tarefa>> ListarDoUsuarioAsync(Guid userId)
    {
        return await tarefaRepository.GetAllByUserIdAsync(userId);
    }

    public async Task<Tarefa?> AtualizarAsync(Guid userId, Guid id, string novoTitulo, bool concluida)
    {
        var tarefa = await tarefaRepository.GetByIdAsync(id);

        if (tarefa == null || tarefa.UserId != userId) 
            throw new Exception("Tarefa não encontrada");

        tarefa.Update(novoTitulo, tarefa.Descricao);
        
        if (concluida) tarefa.Concluir();

        await tarefaRepository.UpdateAsync(tarefa);
        return tarefa;
    }

    public async Task<bool> DeletarAsync(Guid userId, Guid id)
    {
        var tarefa = await tarefaRepository.GetByIdAsync(id);

        if (tarefa == null || tarefa.UserId != userId) 
            return false;

        await tarefaRepository.DeleteAsync(tarefa);
        return true;
    }
}
