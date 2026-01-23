using System;
using FluentValidation;
using TodoApp.Application.DTOs;


namespace TodoApp.Api.Validators;

public class CreateTarefaValidator : AbstractValidator<CreateTarefaDto>
{
    public CreateTarefaValidator()
    {
        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("O título da tarefa é obrigatório.")
            .MinimumLength(3).WithMessage("O título deve ter pelo menos 3 caracteres.");
    }
}