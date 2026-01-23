using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Api.DTOs;
using TodoApp.Api.Extensions;
using TodoApp.Application.DTOs;
using TodoApp.Application.Services.Interfaces;

namespace TodoApp.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TarefasController : ControllerBase
{
    private readonly ITarefaService _tarefaService;

    public TarefasController(ITarefaService tarefaService)
    {
        _tarefaService = tarefaService;
    }

    [HttpPost]
    public async Task<IActionResult> Criar(CreateTarefaDto dto)
    {
        var userId = User.GetId();

        var tarefaCriada = await _tarefaService.AdicionarAsync(userId, dto);

        return CreatedAtAction(nameof(Listar), null, tarefaCriada);
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var userId = User.GetId();

        var tarefas = await _tarefaService.ListarDoUsuarioAsync(userId);

        return Ok(tarefas);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] CreateTarefaDto dto)
    {
        var userId = User.GetId();

        var tarefaAtualizada = await _tarefaService.AtualizarAsync(userId, id, dto.Titulo, false);

        if (tarefaAtualizada == null)
            return NotFound(new ErrorResponseDto("Tarefa não encontrada ou não pertence a você."));

        return Ok(tarefaAtualizada);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Deletar(Guid id)
    {
        var userId = User.GetId();
        var sucesso = await _tarefaService.DeletarAsync(userId, id);

        if (!sucesso)
            return NotFound(new ErrorResponseDto("Tarefa não encontrada ou não pertence a você."));

        return NoContent();
    }
}