# TodoApp

API RESTful para gerenciamento de tarefas com autenticação JWT, desenvolvida com .NET 10 e Clean Architecture.

**Stack:** .NET 10 | PostgreSQL | Entity Framework Core | JWT | FluentValidation | Docker

## Arquitetura

```
TodoApp/
├── TodoApp.Domain/          # Entidades e interfaces de repositórios
├── TodoApp.Application/     # Serviços e DTOs de aplicação
├── TodoApp.Infrastructure/  # Repositórios, DbContext e migrations
└── TodoApp.Api/            # Controllers, validadores e configurações
```

**Camadas:**
- **Domain:** `User`, `Tarefa`, `StatusTarefa` e contratos
- **Application:** Lógica de negócio e serviços
- **Infrastructure:** Persistência e serviços de infraestrutura (BCrypt, JWT)
- **API:** Endpoints, validação e DTOs

## Configuração

### 1. Clonar e iniciar PostgreSQL

```bash
git clone https://github.com/jeffaugg/TodoApp.git
cd TodoApp
docker-compose up -d
```

### 2. Restaurar dependências

```bash
dotnet restore
```

### 3. Aplicar migrations

```bash
dotnet ef database update --project TodoApp.Infrastructure --startup-project TodoApp.Api
```

### 4. Executar

```bash
dotnet run --project TodoApp.Api
```

**URLs:**
- API: `http://localhost:5206`
- Docs: `http://localhost:5206/docs`

### Variáveis de Ambiente (Opcional)

```bash
export ConnectionStrings__DefaultConnection="Server=localhost;Port=5433;Database=firstapidb;User Id=admin;Password=admin;"
export Jwt__Key="sua-chave-secreta"
export Jwt__Issuer="TodoApp"
export Jwt__Audience="TodoAppUsers"
```

## Comandos Úteis

### Migrations

```bash
# Criar migration
dotnet ef migrations add NomeDaMigration --project TodoApp.Infrastructure --startup-project TodoApp.Api

# Aplicar migrations
dotnet ef database update --project TodoApp.Infrastructure --startup-project TodoApp.Api

# Listar migrations
dotnet ef migrations list --project TodoApp.Infrastructure --startup-project TodoApp.Api
```

### Build

```bash
# Compilar
dotnet build

# Executar
dotnet run --project TodoApp.Api

# Restaurar dependências
dotnet restore
```



