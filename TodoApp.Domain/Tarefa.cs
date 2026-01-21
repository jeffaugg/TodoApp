using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoApp.Domain
{
    public class Tarefa(string titulo, string descricao, Guid userId)
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public string Titulo { get; private set; } = titulo;
        public string Descricao { get; private set; } = descricao;
        public DateTime DataCriacao { get; private set; } = DateTime.UtcNow;
        public StatusTarefa Status { get; private set; } = StatusTarefa.Pendente;
        public Guid UserId { get; private set; } = userId;
    }
}