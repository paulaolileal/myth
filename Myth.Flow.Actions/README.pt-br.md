# Myth.Flow.Actions

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Flow.Actions?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Flow.Actions/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Flow.Actions?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Flow.Actions/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

Uma poderosa biblioteca .NET que implementa os padrões CQRS e Arquitetura Orientada a Eventos com integração perfeita aos pipelines Myth.Flow. Construída para escalabilidade com suporte a múltiplos message brokers, estratégias de cache e recursos de resiliência de nível empresarial.

# ⭐ Funcionalidades

- **Padrão CQRS**: Separação clara de Commands, Queries e Events
- **Integração com Pipeline**: Integração fluente com Myth.Flow para workflows compostos
- **Múltiplos Message Brokers**: Suporte a InMemory (dev/test), Kafka e RabbitMQ
- **Cache de Queries**: Cache integrado com provedores Memory e Redis
- **Arquitetura Orientada a Eventos**: Publish/subscribe com suporte a múltiplos handlers
- **Padrões de Resiliência**: Políticas de retry com backoff exponencial, circuit breakers e dead letter queues
- **Auto-descoberta**: Registro automático de handlers via scan de assemblies
- **Integração OpenTelemetry**: Rastreamento distribuído e observabilidade integrados
- **Segurança de Tipo**: APIs totalmente tipadas com segurança em tempo de compilação

# 📦 Instalação

```bash
dotnet add package Myth.Flow.Actions
```

## Dependências Opcionais

```bash
# Para suporte a Kafka
dotnet add package Confluent.Kafka

# Para suporte a RabbitMQ
dotnet add package RabbitMQ.Client

# Para cache distribuído Redis
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

# 🚀 Início Rápido

## 1. Configurar Serviços

```csharp
using Myth.Flow.Actions.Extensions;
using Myth.Flow.Actions.Settings;

builder.Services.AddFlowActions(config =>
{
    config.BrokerType = MessageBrokerType.InMemory;
    config.TelemetryEnabled = true;
    config.CachingEnabled = true;
    config.AssembliesToScan.Add(typeof(Program).Assembly);
});
```

## 2. Definir Commands, Queries e Events

### Command

```csharp
using Myth.Interfaces;
using Myth.Models;

public record CreateUserCommand : ICommand<Guid>
{
    public required string Email { get; init; }
    public required string Name { get; init; }
}

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _repository;

    public CreateUserCommandHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<CommandResult<Guid>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = command.Email,
            Name = command.Name
        };

        await _repository.AddAsync(user, cancellationToken);

        return CommandResult<Guid>.Success(user.Id);
    }
}
```

### Query

```csharp
public record GetUserQuery : IQuery<UserDto>
{
    public required Guid UserId { get; init; }
}

public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto>
{
    private readonly IUserRepository _repository;

    public GetUserQueryHandler(IUserRepository repository)
    {
        _repository = repository;
    }

    public async Task<QueryResult<UserDto>> HandleAsync(
        GetUserQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await _repository.GetByIdAsync(query.UserId, cancellationToken);

        if (user == null)
            return QueryResult<UserDto>.Failure("Usuário não encontrado");

        var dto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name
        };

        return QueryResult<UserDto>.Success(dto);
    }
}
```

### Event

```csharp
public record UserCreatedEvent : DomainEvent
{
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
}

public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent>
{
    private readonly IEmailService _emailService;

    public UserCreatedEventHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task HandleAsync(
        UserCreatedEvent @event,
        CancellationToken cancellationToken = default)
    {
        await _emailService.SendWelcomeEmailAsync(@event.Email, cancellationToken);
    }
}
```

## 3. Usar no Pipeline

```csharp
using Myth.Flow;
using Myth.Flow.Actions.Extensions;

public class CreateUserContext
{
    public required string Email { get; init; }
    public required string Name { get; init; }
    public Guid? UserId { get; set; }
    public UserDto? User { get; set; }
}

var result = await Pipeline
    .Start(new CreateUserContext
    {
        Email = "usuario@exemplo.com",
        Name = "João Silva"
    })
    .WithTelemetry("CreateUserFlow")
    // Processar command
    .Process<CreateUserContext, CreateUserCommand, Guid>(
        ctx => new CreateUserCommand
        {
            Email = ctx.Email,
            Name = ctx.Name
        },
        (ctx, userId) => ctx.UserId = userId)
    // Query com cache
    .Query<CreateUserContext, GetUserQuery, UserDto>(
        ctx => new GetUserQuery { UserId = ctx.UserId!.Value },
        (ctx, user) => ctx.User = user,
        cacheKey: $"user:{ctx.UserId}",
        ttl: TimeSpan.FromMinutes(10))
    // Publicar evento
    .Publish<CreateUserContext, UserCreatedEvent>(ctx => new UserCreatedEvent
    {
        UserId = ctx.UserId!.Value,
        Email = ctx.Email
    })
    .ExecuteAsync();

if (result.IsSuccess)
{
    Console.WriteLine($"Usuário criado: {result.Value.User?.Name}");
}
```

# 🔧 Configuração

## InMemory Broker

```csharp
services.AddFlowActions(config =>
{
    config.BrokerType = MessageBrokerType.InMemory;
    config.AssembliesToScan.Add(typeof(Program).Assembly);
});
```

## Kafka 

```csharp
services.AddFlowActions(config =>
{
    config.BrokerType = MessageBrokerType.Kafka;
    config.BrokerConfigurationFactory = () => new KafkaOptions
    {
        BootstrapServers = "localhost:9092",
        GroupId = "meu-servico",
        ClientId = "meu-servico-instancia-1",
        EnableAutoCommit = false,
        SessionTimeoutMs = 30000,
        AutoOffsetReset = "earliest"
    };
    config.TelemetryEnabled = true;
    config.AssembliesToScan.Add(typeof(Program).Assembly);
});
```

## RabbitMQ

```csharp
services.AddFlowActions(config =>
{
    config.BrokerType = MessageBrokerType.RabbitMQ;
    config.BrokerConfigurationFactory = () => new RabbitMQOptions
    {
        HostName = "localhost",
        Port = 5672,
        UserName = "guest",
        Password = "guest",
        VirtualHost = "/",
        ExchangeName = "eventos-meu-servico",
        ExchangeType = "topic"
    };
    config.CachingEnabled = true;
    config.CacheConfiguration = cache =>
    {
        cache.ProviderType = CacheProviderType.Distributed;
        cache.ConnectionString = "localhost:6379";
        cache.DefaultTtl = TimeSpan.FromMinutes(5);
    };
    config.AssembliesToScan.AddRange(AppDomain.CurrentDomain.GetAssemblies());
});
```

# 📚 Extensões de Pipeline

## Process (Commands)

```csharp
// Command sem resposta
.Process<TContext, TCommand>(
    ctx => new TCommand { /* ... */ })

// Command com resposta
.Process<TContext, TCommand, TResponse>(
    ctx => new TCommand { /* ... */ },
    (ctx, response) => ctx.Result = response)
```

## Query (Operações de Leitura)

```csharp
// Query com configuração de cache opcional
.Query<TContext, TQuery, TResponse>(
    ctx => new TQuery { /* ... */ },
    (ctx, result) => ctx.Data = result,
    options =>
    {
        options.Enabled = true;
        options.CacheKey = $"chave:{ctx.Id}";
        options.Ttl = TimeSpan.FromMinutes(10);
        options.SlidingExpiration = true;
    })

// Query com chave de cache simples
.Query<TContext, TQuery, TResponse>(
    ctx => new TQuery { /* ... */ },
    (ctx, result) => ctx.Data = result,
    cacheKey: "minha-chave-cache",
    ttl: TimeSpan.FromMinutes(10),
    slidingExpiration: false)
```

## Publish (Eventos)

```csharp
// Publicar evento a partir de factory
.Publish<TContext, TEvent>(
    ctx => new TEvent { /* ... */ })

// Publicar contexto como evento (quando TContext implementa IEvent)
.Publish<TEvent>()
```

# 🔍 Uso Direto do Dispatcher

Para cenários onde você não precisa do pipeline completo:

```csharp
public class UserService
{
    private readonly IDispatcher _dispatcher;

    public UserService(IDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task<Guid> CreateUserAsync(string email, string name)
    {
        // Executar command
        var command = new CreateUserCommand { Email = email, Name = name };
        var result = await _dispatcher.DispatchCommandAsync<CreateUserCommand, Guid>(command);

        if (result.IsFailure)
            throw new InvalidOperationException(result.ErrorMessage);

        // Publicar evento
        await _dispatcher.PublishEventAsync(new UserCreatedEvent
        {
            UserId = result.Data,
            Email = email
        });

        return result.Data;
    }

    public async Task<UserDto?> GetUserAsync(Guid userId)
    {
        // Executar query com cache
        var query = new GetUserQuery { UserId = userId };
        var cacheOptions = new CacheOptions
        {
            Enabled = true,
            CacheKey = $"user:{userId}",
            Ttl = TimeSpan.FromMinutes(10)
        };

        var result = await _dispatcher.DispatchQueryAsync<GetUserQuery, UserDto>(
            query,
            cacheOptions);

        return result.IsSuccess ? result.Data : null;
    }
}
```

# 🛡️ Recursos de Resiliência

## Política de Retry

```csharp
using Myth.Flow.Resilience;

var retryPolicy = new RetryPolicy(
    maxAttempts: 3,
    baseBackoffMs: 1000,
    exponentialBackoff: true,
    logger: logger);

var result = await retryPolicy.ExecuteAsync(async () =>
{
    return await servicoExterno.CallAsync();
});
```

## Circuit Breaker

```csharp
var circuitBreaker = new CircuitBreakerPolicy(
    failureThreshold: 5,
    openDuration: TimeSpan.FromSeconds(30),
    logger: logger);

var result = await circuitBreaker.ExecuteAsync(async () =>
{
    return await servicoNaoConfiavel.CallAsync();
});

// Verificar estado do circuit
if (circuitBreaker.State == CircuitState.Open)
{
    // Circuit está aberto, chamadas ao serviço estão bloqueadas
}
```

## Dead Letter Queue

```csharp
services.AddFlowActions(config =>
{
    config.BrokerType = MessageBrokerType.InMemory;
    config.BrokerConfigurationFactory = () => new InMemoryBrokerOptions
    {
        EnableDeadLetterQueue = true,
        MaxRetries = 3
    };
});

// Acessar dead letter queue
public class MonitoringService
{
    private readonly DeadLetterQueue _dlq;

    public MonitoringService(DeadLetterQueue dlq)
    {
        _dlq = dlq;
    }

    public IEnumerable<DeadLetterMessage> GetFailedMessages()
    {
        return _dlq.GetAll();
    }

    public void RetryFailedMessage()
    {
        if (_dlq.TryDequeue(out var message))
        {
            // Tentar processar novamente a mensagem que falhou
        }
    }
}
```

# 📊 Telemetria & Observabilidade

## Integração OpenTelemetry

```csharp
services.AddFlowActions(config =>
{
    config.TelemetryEnabled = true;
    config.BrokerType = MessageBrokerType.InMemory;
    config.AssembliesToScan.Add(typeof(Program).Assembly);
});

// Activities são criadas automaticamente com os seguintes nomes:
// - Command.{NomeDoCommand}
// - Query.{NomeDaQuery}
// - Event.{NomeDoEvento}
// - EventBus.Publish.{NomeDoEvento}
// - EventHandler.{NomeDoHandler}
```

## Tags de Activity

Cada activity inclui tags relevantes:
- `pipeline.input.type`: Nome do tipo de contexto
- `cache.hit`: Se o resultado da query foi servido do cache
- Tags customizadas adicionais dos metadados

# 🎯 Padrões Avançados

## Múltiplos Event Handlers

Todos os handlers de um evento executam em paralelo:

```csharp
public class UserCreatedEmailHandler : IEventHandler<UserCreatedEvent>
{
    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken ct)
    {
        // Enviar email de boas-vindas
    }
}

public class UserCreatedAnalyticsHandler : IEventHandler<UserCreatedEvent>
{
    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken ct)
    {
        // Rastrear analytics
    }
}

public class UserCreatedNotificationHandler : IEventHandler<UserCreatedEvent>
{
    public async Task HandleAsync(UserCreatedEvent @event, CancellationToken ct)
    {
        // Enviar notificação push
    }
}

// Todos os três handlers executam concorrentemente quando o evento é publicado
```

## Steps Condicionais no Pipeline

```csharp
var result = await Pipeline.Start(context)
    .Process<Context, ValidateOrderCommand>(ctx => new ValidateOrderCommand { OrderId = ctx.OrderId })
    .When(
        ctx => ctx.Order?.TotalAmount > 1000,
        pipeline => pipeline
            .Process<Context, FraudCheckCommand>(ctx => new FraudCheckCommand { OrderId = ctx.OrderId })
            .Process<Context, ManagerApprovalCommand>(ctx => new ManagerApprovalCommand { OrderId = ctx.OrderId }))
    .Process<Context, ProcessPaymentCommand>(ctx => new ProcessPaymentCommand { OrderId = ctx.OrderId })
    .Publish<Context, OrderCompletedEvent>(ctx => new OrderCompletedEvent { OrderId = ctx.OrderId })
    .ExecuteAsync();
```

## Transformação de Contexto

```csharp
var result = await Pipeline.Start(orderContext)
    .Process<OrderContext, CreateOrderCommand, Guid>(
        ctx => new CreateOrderCommand { /* ... */ },
        (ctx, orderId) => ctx.OrderId = orderId)
    .Transform(ctx => new ShipmentContext
    {
        OrderId = ctx.OrderId,
        ShipmentId = Guid.NewGuid(),
        Address = ctx.ShippingAddress
    })
    .Process<ShipmentContext, CreateShipmentCommand>(
        ctx => new CreateShipmentCommand { /* ... */ })
    .ExecuteAsync();
```

# 🧪 Testes

```csharp
using Xunit;
using Microsoft.Extensions.DependencyInjection;

public class UserCommandHandlerTests
{
    [Fact]
    public async Task CreateUser_ComDadosValidos_DeveSerBemSucedido()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlowActions(config =>
        {
            config.BrokerType = MessageBrokerType.InMemory;
            config.AssembliesToScan.Add(typeof(CreateUserCommand).Assembly);
        });

        services.AddScoped<IUserRepository, InMemoryUserRepository>();
        services.AddScoped<IEmailService, FakeEmailService>();

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDispatcher>();

        // Act
        var command = new CreateUserCommand
        {
            Email = "teste@exemplo.com",
            Name = "Usuário Teste"
        };

        var result = await dispatcher.DispatchCommandAsync<CreateUserCommand, Guid>(command);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotEqual(Guid.Empty, result.Data);
    }

    [Fact]
    public async Task GetUser_ComCache_DeveRetornarDoCache()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMemoryCache();
        services.AddFlowActions(config =>
        {
            config.BrokerType = MessageBrokerType.InMemory;
            config.CachingEnabled = true;
            config.AssembliesToScan.Add(typeof(GetUserQuery).Assembly);
        });

        services.AddScoped<IUserRepository, InMemoryUserRepository>();

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IDispatcher>();

        var userId = Guid.NewGuid();
        var query = new GetUserQuery { UserId = userId };
        var cacheOptions = new CacheOptions
        {
            Enabled = true,
            CacheKey = $"user:{userId}",
            Ttl = TimeSpan.FromMinutes(10)
        };

        // Act
        var result1 = await dispatcher.DispatchQueryAsync<GetUserQuery, UserDto>(query, cacheOptions);
        var result2 = await dispatcher.DispatchQueryAsync<GetUserQuery, UserDto>(query, cacheOptions);

        // Assert
        Assert.True(result1.IsSuccess);
        Assert.False(result1.FromCache);
        Assert.True(result2.IsSuccess);
        Assert.True(result2.FromCache); // Segunda chamada deve vir do cache
    }
}
```

# 🏗️ Arquitetura

```
┌──────────────────────────────────────────────────────────────┐
│                    Myth.Flow Pipeline                         │
├──────────────────────────────────────────────────────────────┤
│  .Process()  │  .Query()  │  .Publish()  │  .When()  │ .Tap()│
└──────────────┴────────────┴──────────────┴───────────┴───────┘
                            ▼
┌──────────────────────────────────────────────────────────────┐
│                       IDispatcher                             │
├──────────────────────────────────────────────────────────────┤
│  DispatchCommandAsync  │  DispatchQueryAsync  │  PublishEvent│
└────────────────────────┴──────────────────────┴──────────────┘
                            ▼
┌─────────────────────┬────────────────────┬───────────────────┐
│  Command Handlers   │  Query Handlers    │    IEventBus      │
│  (Operações Write)  │  (Read + Cache)    │  (Pub/Sub)        │
└─────────────────────┴────────────────────┴───────────────────┘
                                               ▼
                            ┌──────────────────────────────────┐
                            │      IMessageBroker              │
                            ├──────────────────────────────────┤
                            │ InMemory │ Kafka │ RabbitMQ      │
                            └──────────────────────────────────┘
                                               ▼
                            ┌──────────────────────────────────┐
                            │      Event Handlers              │
                            ├──────────────────────────────────┤
                            │ Execução paralela por evento     │
                            └──────────────────────────────────┘
```

# 🎯 Boas Práticas

1. **Commands**: Use para operações que alteram estado, nomenclatura imperativa (CreateUser, UpdateOrder)
2. **Queries**: Use para operações de leitura, aproveite o cache, prefixe com Get/Find
3. **Events**: Use para comunicação desacoplada, nomenclatura no passado (UserCreated, OrderProcessed)
4. **Handlers**: Mantenha focados e testáveis, princípio de responsabilidade única
5. **Pipeline**: Encadeie operações logicamente, use .When() para fluxos condicionais
6. **Testes**: Use broker InMemory para testes unitários rápidos e isolados
7. **Produção**: Use Kafka/RabbitMQ com políticas de retry e dead letter queues
8. **Cache**: Cache queries caras, use valores de TTL apropriados
9. **Telemetria**: Habilite para produção para rastrear fluxos de command/query/event
10. **Padrão Result**: Sempre verifique IsSuccess antes de acessar Data

# 📝 Convenções de Nomenclatura

- **Commands**: `{Verbo}{Substantivo}Command` (CreateUserCommand, UpdateOrderCommand)
- **Queries**: `{Get|Find}{Substantivo}Query` (GetUserQuery, FindOrdersQuery)
- **Events**: `{Substantivo}{VerboPassado}Event` (UserCreatedEvent, OrderProcessedEvent)
- **Handlers**: `{Request}Handler` (CreateUserCommandHandler, UserCreatedEventHandler)
- **Results**: Use CommandResult, QueryResult com tratamento adequado de sucesso/falha

# 🤝 Contribuindo

Contribuições são bem-vindas! Por favor, siga o estilo de código existente e adicione testes para novas funcionalidades.

# 📄 Licença

Este projeto está licenciado sob a Licença Apache 2.0 - veja o arquivo LICENSE para detalhes.

# 🔗 Projetos Relacionados

- [Myth.Flow](../Myth.Flow/README.md) - Framework principal de orquestração de pipelines
- [Myth.Commons](../Myth.Commons/README.md) - Utilitários e extensões comuns
- [Myth.Repository](../Myth.Repository/README.md) - Implementação do padrão Repository
