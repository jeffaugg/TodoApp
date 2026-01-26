# Passo a Passo para a Instalação do OpenTelemetry na Aplicação C#

Este guia detalha como configurar o OpenTelemetry em uma aplicação ASP.NET Core para coletar métricas.

## 1. Criar um Novo Pacote

Execute o comando para criar um novo pacote que será utilizado para centralizar todas as configurações de telemetria:

```bash
dotnet new classlib -n TodoApp.Telemetry
```

## 2. Instalação dos Pacotes NuGet

Navegue para o diretório do pacote criado:

```bash
cd TodoApp.Telemetry
```

E instale os pacotes abaixo:

### Extensões para Hosting
```bash
dotnet add package OpenTelemetry.Extensions.Hosting
```

### Pacote Principal de Exportação OTLP
```bash
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
```

### Instrumentação para ASP.NET Core
```bash
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
```

### Instrumentação para HTTP Client
```bash
dotnet add package OpenTelemetry.Instrumentation.Http
```

### Instrumentação para Entity Framework Core
```bash
dotnet add package OpenTelemetry.Instrumentation.EntityFrameworkCore --version 1.14.0-beta.2
```

### Instrumentação para Runtime (métricas de runtime)
```bash
dotnet add package OpenTelemetry.Instrumentation.Runtime
```

## 3. Instalação do Aspire no Docker Compose

Na pasta raiz do projeto, edite o arquivo `docker-compose.yml` e adicione o serviço do Aspire Dashboard que será utilizado para receber e visualizar as informações de telemetria da aplicação:

```yaml
  aspire:
    image: mcr.microsoft.com/dotnet/nightly/aspire-dashboard:latest
    ports:
      - 18888:18888 # Dashboard web interface
      - 18889:18889 # OTLP/gRPC
    environment:
      - DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true
```

**Nota:** Certifique-se de adicionar o serviço `aspire` na mesma rede do seu projeto, se necessário.

## 4. Configuração da Aplicação

### 4.1. Configurar o Endpoint OTLP no appsettings.json

No pacote `TodoApp.API`, edite o arquivo `appsettings.json` e adicione a URL do servidor Aspire na seção `ConnectionStrings`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "...",
    "OpenTelemetry": {
      "OtplEndpoint": "http://localhost:18889"
    }
  }
}
```

### 4.2. Criar a Classe de Configuração Base (TelemetrySetup)

No pacote **TodoApp.Telemetry**, crie uma pasta chamada **Config** e dentro dela crie uma classe chamada `TelemetrySetup.cs` com o seguinte código:

```csharp
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace TodoApp.Api.Telemetry
{
  public class TelemetrySetup
  {
    public const string ServiceName = "TodoApp";
    public const string ServiceVersion = "1.0.0";
  }
}
```

**Explicação:**
- **ServiceName** e **ServiceVersion**: Identificam o serviço nas ferramentas de observabilidade

### 4.3. Criar a Classe de Extensão (TelemetryConfig)

Agora crie uma pasta chamada **Extension** e dentro dela crie uma classe `TelemetryConfig.cs` com o seguinte código:

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TodoApp.Api.Telemetry;

namespace TodoApp.Telemetry.Extensions;

public static class TelemetryConfig
{
  public static void AddTelemetry(
    this IServiceCollection services, 
    IConfiguration configuration, 
    ILoggingBuilder logging)
  {
    // Obtém o endpoint OTLP do arquivo de configuração
    string otlpEndpoint = configuration.GetConnectionString("OpenTelemetry:OtplEndpoint")!;

    // Configura o OpenTelemetry
    services.AddOpenTelemetry()
      // 1. Configura o Resource
      .ConfigureResource(resource =>
        resource.AddService(
          serviceName: TelemetrySetup.ServiceName,
          serviceVersion: TelemetrySetup.ServiceVersion
        )
      )
      // 2. Configura Metrics
      .WithMetrics(metrics =>
      {
        metrics
          // Pega as métricas automaticamente de requisições
          .AddAspNetCoreInstrumentation()
          // Pega as métricas automatica de chamadas HTTP
          .AddHttpClientInstrumentation()
          // Pega as métricas automatica de uso de CPU, memória e etc.
          .AddRuntimeInstrumentation()
          // Exporta as métricas para o endpoint OTLP
          .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
      });
  }
}
```

## Explicação Detalhada da Classe TelemetryConfig

A classe `TelemetryConfig` é uma **classe de extensão estática** que centraliza toda a configuração do OpenTelemetry. Vamos entender cada parte:

### Método de Extensão
```csharp
public static void AddTelemetry(this IServiceCollection services, ...)
```
- É um **método de extensão** que permite chamar `services.AddTelemetry(...)` no `Program.cs`
- Recebe três parâmetros:
  - `IServiceCollection`: Para registrar serviços no container de DI
  - `IConfiguration`: Para ler configurações do `appsettings.json`
  - `ILoggingBuilder`: Para configurar o sistema de logging

#### 1. Configuração do Resource
```csharp
.ConfigureResource(resource => resource.AddService(...))
```
- Define **metadados** sobre o serviço que serão enviados com todos os dados de telemetria
- Permite identificar qual serviço gerou os dados nas ferramentas de observabilidade
- Inclui nome e versão do serviço

#### 2. Configuração de Metrics (Métricas)
```csharp
.WithMetrics(metrics => { ... })
```
- **Metrics** são valores numéricos que medem o desempenho do sistema
- `AddAspNetCoreInstrumentation()`: Coleta métricas automáticas do ASP.NET Core (requisições/segundo, latência, etc.)
- `AddHttpClientInstrumentation()`: Coleta métricas de chamadas HTTP
- `AddRuntimeInstrumentation()`: Coleta métricas do runtime .NET (uso de memória, GC, threads)
- `AddOtlpExporter()`: Envia as métricas para o Aspire Dashboard

**O que você verá no dashboard:**
- Taxa de requisições por segundo
- Latência média/p95/p99
- Uso de CPU e memória

## 5. Configurar e Testar Métricas (Default)

Neste passo, vamos configurar as métricas padrão do OpenTelemetry que coletam automaticamente informações sobre requisições HTTP, chamadas HTTP client e métricas do runtime .NET.

### 5.1. Adicionar Referência do TodoApp.Telemetry na API

Para conseguir usar a configuração de telemetria no projeto **TodoApp.API**, é necessário referenciar o projeto `TodoApp.Telemetry`:

```bash
dotnet add TodoApp.Api/TodoApp.Api.csproj reference TodoApp.Telemetry/TodoApp.Telemetry.csproj
```

### 5.2. Configurar a Telemetria no Program.cs

No arquivo `Program.cs` do projeto **TodoApp.API**, adicione a chamada para o método de extensão:

```csharp
using TodoApp.Telemetry.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ... outras configurações ...

// Adiciona a configuração de telemetria
builder.Services.AddTelemetry(builder.Configuration, builder.Logging);

// ... resto do código ...
```

### 5.3. Executar o Aspire Dashboard e Testar as Métricas Default

Para visualizar os dados de telemetria, inicie o Aspire Dashboard usando Docker Compose:

```bash
docker-compose up
```

Acesse o dashboard em: **http://localhost:18888**

Com o dashboard rodando:

1. Inicie a aplicação API
2. Faça algumas requisições à API (pode usar o arquivo `requests.http`)
3. Acesse o Aspire Dashboard em `http://localhost:18888`
4. Na aba de **Metrics**, você verá:
   - **Métricas de requisições HTTP**: Taxa de requisições por segundo, latência, status codes
   - **Métricas do runtime .NET**: Uso de CPU, memória, GC (Garbage Collector), threads

Essas são as métricas **padrão** que o OpenTelemetry coleta automaticamente sem precisar de código adicional.

### 5.4. Adicionar Métricas Customizadas (Counter)

Agora que você já testou as métricas padrão, vamos adicionar uma **métrica customizada** para contar quantos usuários foram criados. Isso permite rastrear eventos de negócio específicos da sua aplicação.

#### 5.4.1. Adicionar Meter e Counter no TelemetrySetup

No arquivo `TelemetrySetup.cs`, adicione o `Meter` e o `Counter`:

```csharp
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace TodoApp.Api.Telemetry
{
  public class TelemetrySetup
  {
    public const string ServiceName = "TodoApp";
    public const string ServiceVersion = "1.0.0";
    
    // Meter para criar métricas customizadas
    public static Meter Meter = new(ServiceName);
    
    // Counter para contar usuários criados
    public static Counter<int> UsersCreatedCounter =
        Meter.CreateCounter<int>("users_created", description: "Counts the number of users created");
  }
}
```

**Explicação:**
- **Meter**: É usado para criar métricas customizadas (contadores, medidores, histogramas)
- **Counter**: É um tipo de métrica que só aumenta (incrementa). Perfeito para contar eventos como "usuários criados"

#### 5.4.2. Registrar o Meter no TelemetryConfig

No arquivo `TelemetryConfig.cs`, adicione o `AddMeter` na configuração de métricas:

```csharp
      .WithMetrics(metrics =>
      {
        metrics
          // Registra o Meter customizado para capturar métricas manuais
          .AddMeter(TelemetrySetup.Meter.Name)
          // Pega as métricas automaticamente de requisições
          .AddAspNetCoreInstrumentation()
          // Pega as métricas automatica de chamadas HTTP
          .AddHttpClientInstrumentation()
          // Pega as métricas automatica de uso de CPU, memória e etc.
          .AddRuntimeInstrumentation()
          // Exporta as métricas para o endpoint OTLP
          .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
      });
```

**Explicação:**
- `AddMeter()`: Registra o `Meter` customizado para que as métricas criadas manualmente sejam capturadas e enviadas ao dashboard

#### 5.4.3. Adicionar Referência do TodoApp.Telemetry no Infrastructure

Para poder usar o `TelemetrySetup` no `UserRepository`, precisamos adicionar a referência do projeto `TodoApp.Telemetry` no projeto `TodoApp.Infrastructure`:

```bash
dotnet add TodoApp.Infrastructure/TodoApp.Infrastructure.csproj reference TodoApp.Telemetry/TodoApp.Telemetry.csproj
```

#### 5.4.4. Incrementar o Counter no UserRepository

No arquivo `UserRepository.cs`, no método `AddAsync`, adicione o incremento do contador após salvar o usuário:

```csharp
public async Task AddAsync(User user)
{
    await context.Users.AddAsync(user);
    await context.SaveChangesAsync();
    
    // Incrementa a métrica customizada de usuários criados
    TelemetrySetup.UsersCreatedCounter.Add(
        1,
        new KeyValuePair<string, object?>("user_name", user.Name)
    );
}
```

**Explicação:**
- `Add(1, ...)`: Incrementa o contador em 1
- O segundo parâmetro adiciona uma **tag** (`user_name`) que permite filtrar/agrupar as métricas no dashboard

### 5.5. Testar a Métrica Customizada

1. Reinicie a aplicação API para carregar as novas configurações

2. Faça uma requisição para cadastrar um novo usuário usando o arquivo `requests.http`:
   ```http
   POST http://localhost:5000/api/user/register
   Content-Type: application/json
   
   {
     "name": "João Silva",
     "email": "joao@example.com",
     "password": "senha123"
   }
   ```

3. Acesse o Aspire Dashboard em **http://localhost:18888**

4. Vá até a aba de **Metrics**

5. Procure pela métrica `users_created`

6. Você verá:
   - O contador aumentando conforme novos usuários são cadastrados
   - A possibilidade de filtrar/visualizar pela tag `user_name`
   - A métrica customizada aparecendo junto com as métricas padrão

Com isso, você agora tem tanto métricas automáticas quanto métricas customizadas de negócio funcionando!  

## 6. Configurar os Traces

No arquivo `TelemetryConfig.cs`, vamos adicionar agora as configurações de tracing. Atualize o método `AddTelemetry` adicionando a configuração de traces logo após a configuração de métricas:

```csharp
      .WithMetrics(metrics =>
      {
        metrics
          // Pega as métricas automaticamente de requisições
          .AddAspNetCoreInstrumentation()
          // Pega as métricas automatica de chamadas HTTP
          .AddHttpClientInstrumentation()
          // Pega as métricas automatica de uso de CPU, memória e etc.
          .AddRuntimeInstrumentation()
          // Exporta as métricas para o endpoint OTLP
          .AddOtlpExporter(options => options.Endpoint = new Uri(otlpEndpoint));
      })
      .WithTracing(tracing =>
      {
        tracing
          .SetErrorStatusOnException()
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddEntityFrameworkCoreInstrumentation()
          .AddOtlpExporter(option => option.Endpoint = new Uri(otlpEndpoint));
      });
```

### Explicação da Configuração de Traces

A configuração de **Traces** (rastreamento) permite acompanhar o fluxo completo de uma requisição através do sistema. Vamos entender cada parte:

#### `SetErrorStatusOnException()`
- Marca automaticamente os traces como **erro** quando uma exceção é lançada durante o processamento
- Facilita a identificação de problemas no dashboard, destacando requisições que falharam

#### `AddAspNetCoreInstrumentation()`
- Captura automaticamente todas as requisições HTTP recebidas pelo ASP.NET Core
- Cria um trace para cada requisição HTTP, incluindo:
  - Método HTTP (GET, POST, PUT, DELETE, etc.)
  - Rota acessada
  - Status code da resposta
  - Tempo de processamento

#### `AddHttpClientInstrumentation()`
- Captura automaticamente todas as chamadas HTTP feitas pela aplicação usando `HttpClient`
- Permite rastrear chamadas para APIs externas, serviços downstream, etc.
- Mostra a cadeia completa de dependências da sua aplicação

#### `AddEntityFrameworkCoreInstrumentation()`
- Captura automaticamente todas as operações de banco de dados executadas pelo Entity Framework Core
- Rastreia queries SQL, tempo de execução e conexões com o banco
- Ajuda a identificar queries lentas ou problemas de performance no acesso aos dados

#### `AddOtlpExporter()`
- Exporta todos os traces coletados para o Aspire Dashboard através do protocolo OTLP
- Os traces serão visualizados no dashboard em tempo real

**O que você verá no dashboard:**
- **Fluxo completo de requisições**: Veja cada etapa do processamento de uma requisição
- **Tempo de resposta detalhado**: Quanto tempo cada operação levou (controller, banco de dados, chamadas HTTP)
- **Dependências**: Visualize quais serviços e bancos de dados sua aplicação está chamando
- **Erros**: Traces marcados como erro quando exceções ocorrem
- **Spans**: Cada operação individual (requisição HTTP, query SQL, chamada externa) aparece como um "span" no trace

### Como Testar os Traces

1. Reinicie a aplicação API

2. Realize alguma requisição do arquivo `requests.http`

3. No dashboard, navegue até a seção de Traces para ver o rastreamento completo das requisições

## 7. Configurar Traces Customizados com ActivitySource

Agora vamos adicionar traces customizados para rastrear operações específicas do nosso código. Isso nos permitirá ver o fluxo completo de uma operação, como a criação de um usuário, passo a passo.

### 7.1. Adicionar ActivitySource no TelemetrySetup

No arquivo `TelemetrySetup.cs`, adicione o `ActivitySource`:

```csharp
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace TodoApp.Api.Telemetry
{
  public class TelemetrySetup
  {
    public const string ServiceName = "TodoApp";
    public const string ServiceVersion = "1.0.0";
    
    public static readonly ActivitySource activitySource = new(ServiceName);
  }
}
```

**Explicação:**
- **ActivitySource**: É usado para criar traces customizados manualmente no código
- Permite rastrear operações específicas que você definir

### 7.2. Registrar o ActivitySource no TelemetryConfig

No arquivo `TelemetryConfig.cs`, adicione o `AddSource` na configuração de tracing:

```csharp
      .WithTracing(tracing =>
      {
        tracing
          .AddSource(TelemetrySetup.activitySource.Name)
          .SetErrorStatusOnException()
          .AddAspNetCoreInstrumentation()
          .AddHttpClientInstrumentation()
          .AddEntityFrameworkCoreInstrumentation()
          .AddOtlpExporter(option => option.Endpoint = new Uri(otlpEndpoint));
      });
```

**Explicação:**
- `AddSource()`: Registra o `ActivitySource` customizado para que os traces criados manualmente sejam capturados e enviados ao dashboard

### 7.3. Adicionar Referência do Pacote Telemetry no Projeto Application

Antes de adicionar os traces customizados, precisamos adicionar referência ao pacote `TodoApp.Telemetry` no projeto `TodoApp.Application`:

```bash
dotnet add TodoApp.Application/TodoApp.Application.csproj reference TodoApp.Telemetry/TodoApp.Telemetry.csproj
```

**Nota:** A referência do `TodoApp.Infrastructure` já foi adicionada na seção 5.4.3 quando configuramos as métricas customizadas.

### 7.4. Adicionar Traces Customizados nos Métodos

Agora vamos adicionar traces customizados para rastrear o fluxo completo da criação de um usuário.

#### No UserService.AddAsync

No arquivo `UserService.cs`, adicione o trace no início do método `AddAsync`:

```csharp
public async Task<Guid> AddAsync(CreateUserDto user)
{
    using var activitySource = TelemetrySetup.activitySource.StartActivity("UserService.AddAsync");
    
    activitySource?.SetTag("step", "add_async_service");
    
    var hasUserEmail = await userRepository.GetByEmailAsync(user.Email);
    // ... resto do código
}
```

**Explicação:**
- `StartActivity()`: Cria um novo trace (activity) com o nome especificado
- `SetTag()`: Adiciona uma tag (metadado) ao trace para facilitar a identificação
- O `using` garante que o trace seja finalizado automaticamente ao sair do método

#### No UserRepository.GetByEmailAsync

No arquivo `UserRepository.cs`, adicione o trace no método `GetByEmailAsync`:

```csharp
public async Task<User?> GetByEmailAsync(string email)
{
    using var activitySource = TelemetrySetup.activitySource.StartActivity("UserRepository.GetByEmailAsync");
    
    activitySource?.SetTag("step", "get_by_email_async_repository");
    
    return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
}
```

#### No BcryptPasswordHasher.HashPasswordAsync

No arquivo `BcryptPasswordHasher.cs`, adicione o trace no método `HashPasswordAsync`:

```csharp
public Task<string> HashPasswordAsync(string password)
{
    using var activitySource = TelemetrySetup.activitySource.StartActivity("BcryptPasswordHasher.HashPasswordAsync");
    
    activitySource?.SetTag("step", "hash_password_service");
    
    return Task.FromResult(BCrypt.Net.BCrypt.HashPassword(password));
}
```

#### No UserRepository.AddAsync

No arquivo `UserRepository.cs`, adicione o trace no método `AddAsync`:

```csharp
public async Task AddAsync(User user)
{
    using var activitySource = TelemetrySetup.activitySource.StartActivity("UserRepository.AddAsync");
    
    activitySource?.SetTag("step", "add_async_repository");
    
    await context.Users.AddAsync(user);
    await context.SaveChangesAsync();
}
```

### 7.5. Como Testar os Traces Customizados

1. Certifique-se de que o Aspire Dashboard está rodando e a aplicação API está iniciada

2. Faça uma requisição para cadastrar um novo usuário usando o arquivo `requests.http`:
   ```http
   POST http://localhost:5000/api/user/register
   Content-Type: application/json
   
   {
     "name": "João Silva",
     "email": "joao@example.com",
     "password": "senha123"
   }
   ```

3. Acesse o Aspire Dashboard em **http://localhost:18888**

4. Navegue até a seção de **Traces**

5. Você verá o trace completo da requisição, mostrando todos os passos:
   - **UserService.AddAsync**: Início do processamento no serviço
   - **UserRepository.GetByEmailAsync**: Verificação se o email já existe
   - **BcryptPasswordHasher.HashPasswordAsync**: Hash da senha
   - **UserRepository.AddAsync**: Persistência do usuário no banco
   - **Entity Framework Core**: Query SQL executada

6. Clique em um trace específico para ver:
   - A hierarquia completa de spans (operações)
   - O tempo de cada operação individual
   - As tags adicionadas em cada etapa
   - O fluxo completo desde a requisição HTTP até a persistência no banco

Com os traces customizados configurados, você agora tem visibilidade detalhada de cada etapa do processamento de uma operação, permitindo identificar exatamente onde está ocorrendo lentidão ou problemas no fluxo da aplicação!

## 8. Configurar Logging no OpenTelemetry

O OpenTelemetry também pode capturar e exportar logs da sua aplicação. Alguns logs já são exportados automaticamente pelo ASP.NET Core, mas você também pode criar logs customizados para eventos específicos do seu negócio.

### 8.1. Configurar Logging no TelemetryConfig

No arquivo `TelemetryConfig.cs`, adicione a configuração de logging. O método `AddTelemetry` já recebe o parâmetro `ILoggingBuilder`, então vamos utilizá-lo:

```csharp
public static void AddTelemetry(
  this IServiceCollection services, 
  IConfiguration configuration, 
  ILoggingBuilder logging)
{
  // Obtém o endpoint OTLP do arquivo de configuração
  string otlpEndpoint = configuration.GetConnectionString("OpenTelemetry:OtplEndpoint")!;

  // Configura o OpenTelemetry
  services.AddOpenTelemetry()
    // ... configurações de Resource, Metrics e Tracing ...
    );

  // Configura Logging
  logging.AddOpenTelemetry(options =>
  {
    options
      .SetResourceBuilder(ResourceBuilder.CreateDefault()
        .AddService(TelemetrySetup.ServiceName, serviceVersion: TelemetrySetup.ServiceVersion))
      .AddOtlpExporter(otlpOptions => otlpOptions.Endpoint = new Uri(otlpEndpoint));
  });
}
```

**Explicação:**
- `AddOpenTelemetry()`: Integra o sistema de logging do .NET com o OpenTelemetry
- `SetResourceBuilder()`: Define os metadados do serviço para os logs (mesmo nome e versão usados nas métricas e traces)
- `AddOtlpExporter()`: Exporta os logs para o Aspire Dashboard através do protocolo OTLP

**Logs que já são exportados automaticamente:**
- Logs do ASP.NET Core (requisições HTTP, erros, etc.)
- Logs de exceções não tratadas
- Logs do Entity Framework Core (queries, erros de conexão, etc.)

### 8.2. Adicionar Log Customizado no UserRepository

Vamos adicionar um log customizado quando um usuário for criado. No arquivo `UserRepository.cs`, você precisará injetar o `ILogger`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TodoApp.Api.Telemetry;
using TodoApp.Domain;
using TodoApp.Domain.Repositories.Interfaces;
using TodoApp.Infrastructure.Contexts;

namespace TodoApp.Infrastructure.Repositories
{
  public class UserRepository : IUserRepository
  {
    private readonly AppDbContext _context;
    private readonly ILogger<UserRepository> _logger;

    public UserRepository(AppDbContext context, ILogger<UserRepository> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task AddAsync(User user)
    {
      await _context.Users.AddAsync(user);
      await _context.SaveChangesAsync();
      
      // Incrementa a métrica customizada
      TelemetrySetup.UsersCreatedCounter.Add(
        1,
        new KeyValuePair<string, object?>("user_name", user.Name)
      );
      
      // Log customizado quando um usuário é criado
      _logger.LogInformation(
        "Novo usuário criado: {UserName} (ID: {UserId}, Email: {UserEmail})",
        user.Name,
        user.Id,
        user.Email
      );
    }

    // ... outros métodos ...
  }
}
```

**Explicação:**
- `ILogger<UserRepository>`: Logger tipado que identifica a origem do log
- `LogInformation()`: Cria um log com nível Information
- Os parâmetros `{UserName}`, `{UserId}`, `{UserEmail}` são **structured logging** - permitem filtrar e buscar logs por esses campos no dashboard

**Níveis de log disponíveis:**
- `LogTrace`: Informações muito detalhadas (debug)
- `LogDebug`: Informações de debug
- `LogInformation`: Informações gerais (como o exemplo acima)
- `LogWarning`: Avisos
- `LogError`: Erros
- `LogCritical`: Erros críticos

### 8.3. Testar os Logs

1. Certifique-se de que o Aspire Dashboard está rodando e a aplicação API está iniciada

2. Faça uma requisição para cadastrar um novo usuário usando o arquivo `requests.http`:
   ```http
   POST http://localhost:5000/api/user/register
   Content-Type: application/json
   
   {
     "name": "Maria Santos",
     "email": "maria@example.com",
     "password": "senha123"
   }
   ```

3. Acesse o Aspire Dashboard em **http://localhost:18888**

4. Navegue até a seção de **Logs**

5. Você verá:
   - **Logs automáticos**: Logs do ASP.NET Core sobre a requisição HTTP
   - **Log customizado**: O log que criamos no `UserRepository` com as informações do usuário criado
   - **Filtros**: Pode filtrar por nível (Information, Warning, Error), por texto, ou pelos campos estruturados (`UserName`, `UserId`, `UserEmail`)

6. Clique em um log específico para ver:
   - O nível do log
   - A mensagem completa
   - Os campos estruturados (parâmetros)
   - O timestamp
   - A origem do log (qual classe/método gerou)

**Benefícios dos logs estruturados:**
- **Correlação**: Os logs são automaticamente correlacionados com traces e métricas
- **Busca avançada**: Filtre por qualquer campo estruturado
- **Análise**: Identifique padrões e problemas rapidamente
- **Contexto completo**: Veja o que aconteceu antes e depois de um erro

Com o logging configurado, você agora tem observabilidade completa: **Métricas**, **Traces** e **Logs** todos integrados e visíveis no Aspire Dashboard!