# Myth.Flow.Actions

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Flow.Actions?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Flow.Actions/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Flow.Actions?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Flow.Actions/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

Uma poderosa biblioteca .NET que implementa os padrões CQRS e Arquitetura Orientada a Eventos com integração perfeita aos pipelines Myth.Flow. Construída para escalabilidade com suporte a múltiplos message brokers, estratégias de cache e recursos de resiliência de nível empresarial.

## Funcionalidades

- **Padrão CQRS**: Separação clara de Commands, Queries e Events com dispatcher centralizado
- **Arquitetura Orientada a Eventos**: Publish/subscribe com suporte a múltiplos handlers e message brokers
- **Integração com Pipeline**: Integração fluente com Myth.Flow para workflows compostos
- **Múltiplos Message Brokers**: Suporte a InMemory (dev/test), Kafka e RabbitMQ
- **Cache de Queries**: Cache integrado com provedores Memory e Redis
- **Padrões de Resiliência**: Políticas de retry com backoff exponencial, circuit breakers e dead letter queues
- **Auto-descoberta**: Registro automático de handlers via scan de assemblies
- **Integração OpenTelemetry**: Rastreamento distribuído e observabilidade integrados
- **Segurança de Tipo**: APIs totalmente tipadas com segurança em tempo de compilação

## Instalação

```bash
dotnet add package Myth.Flow.Actions
```

### Dependências Opcionais

```bash
# Para suporte a Kafka
dotnet add package Confluent.Kafka

# Para suporte a RabbitMQ
dotnet add package RabbitMQ.Client

# Para cache distribuído Redis
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

## Início Rápido

### 1. Configurar Serviços

```csharp
using Myth.Flow.Actions.Extensions;

var builder = WebApplication.CreateBuilder( args );

builder.Services.AddFlow( config => config
    .UseTelemetry( )
    .UseLogging( )
    .UseRetry( attempts: 3, backoffMs: 100 )
    .UseActions( actions => actions
        .UseInMemory( )
        .UseCaching( )
        .ScanAssemblies( typeof( Program ).Assembly )));

var app = builder.BuildApp( );
app.Run( );
```

### 2. Definir Commands, Queries e Events

#### Command

```csharp
using Myth.Interfaces;
using Myth.Models;

public record CreateUserCommand : ICommand<Guid> {
    public required string Email { get; init; }
    public required string Name { get; init; }
}

public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid> {
    private readonly IUserRepository _repository;

    public CreateUserCommandHandler( IUserRepository repository ) {
        _repository = repository;
    }

    public async Task<CommandResult<Guid>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default ) {
        var user = new User {
            Id = Guid.NewGuid( ),
            Email = command.Email,
            Name = command.Name
        };

        await _repository.AddAsync( user, cancellationToken );

        return CommandResult<Guid>.Success( user.Id );
    }
}
```

#### Query

```csharp
public record GetUserQuery : IQuery<UserDto> {
    public required Guid UserId { get; init; }
}

public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto> {
    private readonly IUserRepository _repository;

    public GetUserQueryHandler( IUserRepository repository ) {
        _repository = repository;
    }

    public async Task<QueryResult<UserDto>> HandleAsync(
        GetUserQuery query,
        CancellationToken cancellationToken = default ) {
        var user = await _repository.GetByIdAsync( query.UserId, cancellationToken );

        if ( user == null )
            return QueryResult<UserDto>.Failure( "Usuário não encontrado" );

        var dto = new UserDto {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name
        };

        return QueryResult<UserDto>.Success( dto );
    }
}
```

#### Event

```csharp
public record UserCreatedEvent : DomainEvent {
    public required Guid UserId { get; init; }
    public required string Email { get; init; }
}

public class UserCreatedEventHandler : IEventHandler<UserCreatedEvent> {
    private readonly IEmailService _emailService;

    public UserCreatedEventHandler( IEmailService emailService ) {
        _emailService = emailService;
    }

    public async Task HandleAsync(
        UserCreatedEvent @event,
        CancellationToken cancellationToken = default ) {
        await _emailService.SendWelcomeEmailAsync( @event.Email, cancellationToken );
    }
}
```

### 3. Usar o Dispatcher

#### Execução Simples de Command

```csharp
public class UserService {
    private readonly IDispatcher _dispatcher;

    public UserService( IDispatcher dispatcher ) {
        _dispatcher = dispatcher;
    }

    public async Task<Guid> CreateUserAsync( string email, string name ) {
        var command = new CreateUserCommand { Email = email, Name = name };
        var result = await _dispatcher.DispatchCommandAsync<CreateUserCommand, Guid>( command );

        if ( result.IsFailure )
            throw new InvalidOperationException( result.ErrorMessage );

        return result.Data;
    }
}
```

#### Query com Cache

```csharp
public async Task<UserDto?> GetUserAsync( Guid userId ) {
    var query = new GetUserQuery { UserId = userId };
    var cacheOptions = new CacheOptions {
        Enabled = true,
        CacheKey = $"user:{userId}",
        Ttl = TimeSpan.FromMinutes( 10 )
    };

    var result = await _dispatcher.DispatchQueryAsync<GetUserQuery, UserDto>(
        query,
        cacheOptions );

    return result.IsSuccess ? result.Data : null;
}
```

#### Publicação de Evento

```csharp
public async Task PublishUserCreatedAsync( Guid userId, string email ) {
    await _dispatcher.PublishEventAsync( new UserCreatedEvent {
        UserId = userId,
        Email = email
    });
}
```

### 4. Integração com Pipeline

```csharp
public async Task<Result<UserDto>> CreateAndRetrieveUserAsync( string email, string name ) {
    var command = new CreateUserCommand { Email = email, Name = name };

    var result = await Pipeline
        .Start( command )
        .Process<CreateUserCommand, Guid>( )
        .Transform( userId => new GetUserQuery { UserId = userId })
        .Query<GetUserQuery, UserDto>( ( query, cache ) => cache.UseCache(
            $"user:{query.UserId}",
            TimeSpan.FromMinutes( 10 )))
        .Transform( user => new UserCreatedEvent { UserId = user.Id, Email = user.Email })
        .Publish<UserCreatedEvent>( )
        .ExecuteAsync( );

    return result;
}
```

## Configuração

### Message Brokers

#### InMemory Broker

```csharp
services.AddFlow( config => config
    .UseActions( actions => actions
        .UseInMemory( options => {
            options.UseDeadLetterQueue = true;
            options.MaxRetries = 3;
        })
        .ScanAssemblies( typeof( Program ).Assembly )));
```

#### Kafka

```csharp
services.AddFlow( config => config
    .UseTelemetry( )
    .UseActions( actions => actions
        .UseKafka( options => {
            options.BootstrapServers = "localhost:9092";
            options.GroupId = "meu-servico";
            options.ClientId = "meu-servico-instancia-1";
            options.EnableAutoCommit = false;
            options.SessionTimeoutMs = 30000;
            options.AutoOffsetReset = "earliest";
        })
        .ScanAssemblies( typeof( Program ).Assembly )));
```

#### RabbitMQ

```csharp
services.AddFlow( config => config
    .UseTelemetry( )
    .UseActions( actions => actions
        .UseRabbitMQ( options => {
            options.HostName = "localhost";
            options.Port = 5672;
            options.UserName = "guest";
            options.Password = "guest";
            options.VirtualHost = "/";
            options.ExchangeName = "eventos-meu-servico";
            options.ExchangeType = "topic";
        })
        .ScanAssemblies( typeof( Program ).Assembly )));
```

### Cache

#### Memory Cache

```csharp
services.AddFlow( config => config
    .UseActions( actions => actions
        .UseInMemory( )
        .UseCaching( cache => {
            cache.ProviderType = CacheProviderType.Memory;
            cache.DefaultTtl = TimeSpan.FromMinutes( 5 );
        })
        .ScanAssemblies( typeof( Program ).Assembly )));
```

#### Redis Cache

```csharp
services.AddFlow( config => config
    .UseActions( actions => actions
        .UseInMemory( )
        .UseCaching( cache => {
            cache.ProviderType = CacheProviderType.Distributed;
            cache.ConnectionString = "localhost:6379";
            cache.DefaultTtl = TimeSpan.FromMinutes( 10 );
        })
        .ScanAssemblies( typeof( Program ).Assembly )));
```

## Interfaces Principais

### IDispatcher

Dispatcher central para todas as operações CQRS:

```csharp
public interface IDispatcher {
    Task<CommandResult> DispatchCommandAsync<TCommand>(
        TCommand command,
        CancellationToken cancellationToken = default )
        where TCommand : ICommand;

    Task<CommandResult<TResponse>> DispatchCommandAsync<TCommand, TResponse>(
        TCommand command,
        CancellationToken cancellationToken = default )
        where TCommand : ICommand<TResponse>;

    Task<QueryResult<TResponse>> DispatchQueryAsync<TQuery, TResponse>(
        TQuery query,
        CacheOptions? cacheOptions = null,
        CancellationToken cancellationToken = default )
        where TQuery : IQuery<TResponse>;

    Task PublishEventAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default )
        where TEvent : IEvent;
}
```

### IEventBus

Publicação e assinatura de eventos:

```csharp
public interface IEventBus {
    Task PublishAsync<TEvent>(
        TEvent @event,
        CancellationToken cancellationToken = default )
        where TEvent : IEvent;

    void Subscribe<TEvent, THandler>( )
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>;

    void Unsubscribe<TEvent, THandler>( )
        where TEvent : IEvent
        where THandler : IEventHandler<TEvent>;
}
```

### Interfaces de Command, Query e Event

```csharp
public interface ICommand : IRequest<CommandResult> { }

public interface ICommand<TResponse> : IRequest<CommandResult<TResponse>> { }

public interface IQuery<TResponse> : IRequest<QueryResult<TResponse>> { }

public interface IEvent {
    string EventId { get; }
    DateTimeOffset OccurredAt { get; }
}
```

### Interfaces de Handler

```csharp
public interface ICommandHandler<TCommand>
    where TCommand : ICommand {
    Task<CommandResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default );
}

public interface ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse> {
    Task<CommandResult<TResponse>> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default );
}

public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse> {
    Task<QueryResult<TResponse>> HandleAsync(
        TQuery query,
        CancellationToken cancellationToken = default );
}

public interface IEventHandler<TEvent>
    where TEvent : IEvent {
    Task HandleAsync(
        TEvent @event,
        CancellationToken cancellationToken = default );
}
```

## Tipos de Result

### CommandResult

```csharp
public readonly struct CommandResult {
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
    public Dictionary<string, object>? Metadata { get; }

    public static CommandResult Success( Dictionary<string, object>? metadata = null );
    public static CommandResult Failure( string errorMessage, Exception? exception = null, Dictionary<string, object>? metadata = null );
}

public readonly struct CommandResult<TResponse> {
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public TResponse? Data { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
    public Dictionary<string, object>? Metadata { get; }

    public static CommandResult<TResponse> Success( TResponse data, Dictionary<string, object>? metadata = null );
    public static CommandResult<TResponse> Failure( string errorMessage, Exception? exception = null, Dictionary<string, object>? metadata = null );
}
```

### QueryResult

```csharp
public readonly struct QueryResult<TData> {
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public TData? Data { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
    public bool FromCache { get; }
    public Dictionary<string, object>? Metadata { get; }

    public static QueryResult<TData> Success( TData data, bool fromCache = false, Dictionary<string, object>? metadata = null );
    public static QueryResult<TData> Failure( string errorMessage, Exception? exception = null, Dictionary<string, object>? metadata = null );
}
```

## Extensões de Pipeline

### Iniciando Pipelines

```csharp
Pipeline.Start<TRequest>( TRequest request )

Pipeline.Start( )
```

### Processando Commands

```csharp
.Process<TCommand>( )

.Process<TCommand, TResponse>( )
```

### Executando Queries

```csharp
.Query<TQuery, TResponse>( )

.Query<TQuery, TResponse>( ( query, cache ) => cache.UseCache( "chave", TimeSpan.FromMinutes( 5 )))
```

### Publicando Events

```csharp
.Publish<TEvent>( )
```

### Transformações

```csharp
.Transform<TNext>( current => new TNext { ... })

.TransformAsync<TNext>( async current => await CreateNextAsync( current ))

.TransformIf<TNext>(
    condition: current => current.IsValid,
    transform: current => new TNext { ... })

.TransformIf<TNext>(
    condition: current => current.Type == "Premium",
    transformTrue: current => new PremiumAction { ... },
    transformFalse: current => new StandardAction { ... })
```

## Recursos de Resiliência

### Política de Retry

```csharp
using Myth.Flow.Resilience;

var retryPolicy = new RetryPolicy(
    maxAttempts: 3,
    baseBackoffMs: 1000,
    exponentialBackoff: true,
    logger: logger );

var result = await retryPolicy.ExecuteAsync( async ( ) => {
    return await servicoExterno.CallAsync( );
});
```

### Circuit Breaker

```csharp
var circuitBreaker = new CircuitBreakerPolicy(
    failureThreshold: 5,
    openDuration: TimeSpan.FromSeconds( 30 ),
    logger: logger );

var result = await circuitBreaker.ExecuteAsync( async ( ) => {
    return await servicoNaoConfiavel.CallAsync( );
});

if ( circuitBreaker.State == CircuitState.Open ) {
    // Circuit está aberto, chamadas ao serviço estão bloqueadas
}
```

### Dead Letter Queue

```csharp
services.AddFlow( config => config
    .UseActions( actions => actions
        .UseInMemory( options => {
            options.UseDeadLetterQueue = true;
            options.MaxRetries = 3;
        })
        .ScanAssemblies( typeof( Program ).Assembly )));

public class MonitoringService {
    private readonly DeadLetterQueue _dlq;

    public MonitoringService( DeadLetterQueue dlq ) {
        _dlq = dlq;
    }

    public IEnumerable<DeadLetterMessage> GetFailedMessages( ) {
        return _dlq.GetAll( );
    }

    public void RetryFailedMessage( ) {
        if ( _dlq.TryDequeue( out var message )) {
            // Tentar processar novamente a mensagem que falhou
        }
    }
}
```

## Telemetria e Observabilidade

### Integração OpenTelemetry

```csharp
services.AddFlow( config => config
    .UseTelemetry( )
    .UseActions( actions => actions
        .UseInMemory( )
        .ScanAssemblies( typeof( Program ).Assembly )));

// Activities são criadas automaticamente com os seguintes nomes:
// - Command.{NomeDoCommand}
// - Query.{NomeDaQuery}
// - Event.{NomeDoEvento}
// - EventBus.Publish.{NomeDoEvento}
// - EventHandler.{NomeDoHandler}
```

### Tags de Activity

Cada activity inclui tags relevantes:
- `pipeline.input.type`: Nome do tipo de contexto
- `cache.hit`: Se o resultado da query foi servido do cache
- Tags customizadas adicionais dos metadados

## Padrões Avançados

### Múltiplos Event Handlers

Todos os handlers de um evento executam em paralelo:

```csharp
public class UserCreatedEmailHandler : IEventHandler<UserCreatedEvent> {
    public async Task HandleAsync( UserCreatedEvent @event, CancellationToken ct ) {
        // Enviar email de boas-vindas
    }
}

public class UserCreatedAnalyticsHandler : IEventHandler<UserCreatedEvent> {
    public async Task HandleAsync( UserCreatedEvent @event, CancellationToken ct ) {
        // Rastrear analytics
    }
}

public class UserCreatedNotificationHandler : IEventHandler<UserCreatedEvent> {
    public async Task HandleAsync( UserCreatedEvent @event, CancellationToken ct ) {
        // Enviar notificação push
    }
}

// Todos os três handlers executam concorrentemente quando o evento é publicado
```

### Workflows Complexos

```csharp
public async Task<Result<ShipmentDto>> ProcessOrderWorkflowAsync(
    Guid customerId,
    List<OrderItem> items,
    Address address ) {
    var command = new CreateOrderCommand {
        CustomerId = customerId,
        Items = items,
        ShippingAddress = address
    };

    var result = await Pipeline
        .Start( command )
        .Process<CreateOrderCommand, Guid>( )
        .Transform( orderId => new GetOrderQuery { OrderId = orderId })
        .Query<GetOrderQuery, OrderDto>( )
        .Transform( order => new CreateShipmentCommand {
            OrderId = order.Id,
            ShipmentId = Guid.NewGuid( ),
            Address = order.ShippingAddress,
            Items = order.Items
        })
        .Process<CreateShipmentCommand, ShipmentDto>( )
        .Transform( shipment => new ShipmentCreatedEvent {
            OrderId = shipment.OrderId,
            ShipmentId = shipment.Id,
            TrackingNumber = shipment.TrackingNumber
        })
        .Publish<ShipmentCreatedEvent>( )
        .ExecuteAsync( );

    return result;
}
```

### Workflows Condicionais

```csharp
public async Task<Result<OrderDto>> ValidateHighValueOrderAsync( Guid orderId ) {
    var command = new ValidateOrderCommand { OrderId = orderId };

    var result = await Pipeline
        .Start( command )
        .Process<ValidateOrderCommand, OrderDto>( )
        .TransformIf<FraudCheckCommand>(
            order => order.TotalAmount > 1000,
            order => new FraudCheckCommand { OrderId = order.Id })
        .Process<FraudCheckCommand>( )
        .Transform( fraudResult => new ProcessPaymentCommand { OrderId = orderId })
        .Process<ProcessPaymentCommand>( )
        .Transform( paymentResult => new OrderCompletedEvent { OrderId = orderId })
        .Publish<OrderCompletedEvent>( )
        .ExecuteAsync( );

    return result;
}
```

## Testes

### Testando Handlers

```csharp
using Xunit;
using FluentAssertions;

public class CreateUserCommandHandlerTests {
    [Fact]
    public async Task Handle_ComCommandValido_DeveRetornarSucesso( ) {
        // Arrange
        var repository = new InMemoryUserRepository( );
        var handler = new CreateUserCommandHandler( repository );
        var command = new CreateUserCommand {
            Email = "teste@exemplo.com",
            Name = "Usuário Teste"
        };

        // Act
        var result = await handler.HandleAsync( command );

        // Assert
        result.IsSuccess.Should( ).BeTrue( );
        result.Data.Should( ).NotBe( Guid.Empty );
    }
}
```

### Testando Pipelines

```csharp
using Microsoft.Extensions.DependencyInjection;

public class UserPipelineTests {
    private readonly IServiceProvider _serviceProvider;

    public UserPipelineTests( ) {
        var services = new ServiceCollection( );
        services.AddLogging( );
        services.AddFlow( config => config
            .UseActions( actions => actions
                .UseInMemory( )
                .UseCaching( )
                .ScanAssemblies( typeof( CreateUserCommand ).Assembly )));

        services.AddScoped<IUserRepository, InMemoryUserRepository>( );
        services.AddScoped<IEmailService, FakeEmailService>( );

        _serviceProvider = services.BuildWithGlobalProvider( );
    }

    [Fact]
    public async Task CreateAndRetrieveUser_DeveEncadearOperacoes( ) {
        // Arrange
        var command = new CreateUserCommand {
            Email = "teste@exemplo.com",
            Name = "Usuário Teste"
        };

        // Act
        var result = await Pipeline
            .Start( command )
            .Process<CreateUserCommand, Guid>( )
            .Transform( userId => new GetUserQuery { UserId = userId })
            .Query<GetUserQuery, UserDto>( ( query, cache ) => cache.UseCache(
                $"user:{query.UserId}",
                TimeSpan.FromMinutes( 5 )))
            .Transform( user => new UserCreatedEvent { UserId = user.Id, Email = user.Email })
            .Publish<UserCreatedEvent>( )
            .ExecuteAsync( );

        // Assert
        result.IsSuccess.Should( ).BeTrue( );
        result.Value.Should( ).NotBeNull( );
    }
}
```

## Arquitetura

```
┌──────────────────────────────────────────────────────────────┐
│                    Myth.Flow Pipeline                         │
├──────────────────────────────────────────────────────────────┤
│  .Process()  │  .Query()  │  .Publish()  │  .Transform()     │
└──────────────┴────────────┴──────────────┴───────────────────┘
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

## Boas Práticas

1. **Commands**: Use para operações que alteram estado, nomenclatura imperativa (CreateUser, UpdateOrder)
2. **Queries**: Use para operações de leitura, aproveite o cache, prefixe com Get/Find
3. **Events**: Use para comunicação desacoplada, nomenclatura no passado (UserCreated, OrderProcessed)
4. **Handlers**: Mantenha focados e testáveis, princípio de responsabilidade única
5. **Pipeline**: Encadeie operações logicamente, use fluxos condicionais quando necessário
6. **Testes**: Use broker InMemory para testes unitários rápidos e isolados
7. **Produção**: Use Kafka/RabbitMQ com políticas de retry e dead letter queues
8. **Cache**: Cache queries caras, use valores de TTL apropriados
9. **Telemetria**: Habilite para produção para rastrear fluxos de command/query/event
10. **Padrão Result**: Sempre verifique IsSuccess antes de acessar Data

## Convenções de Nomenclatura

- **Commands**: `{Verbo}{Substantivo}Command` (CreateUserCommand, UpdateOrderCommand)
- **Queries**: `{Get|Find}{Substantivo}Query` (GetUserQuery, FindOrdersQuery)
- **Events**: `{Substantivo}{VerboPassado}Event` (UserCreatedEvent, OrderProcessedEvent)
- **Handlers**: `{Request}Handler` (CreateUserCommandHandler, UserCreatedEventHandler)
- **Results**: Use CommandResult, QueryResult com tratamento adequado de sucesso/falha

## Contribuindo

Contribuições são bem-vindas! Por favor, siga o estilo de código existente e adicione testes para novas funcionalidades.

## Licença

Este projeto está licenciado sob a Licença Apache 2.0 - veja o arquivo LICENSE para detalhes.

## Projetos Relacionados

- [Myth.Flow](../Myth.Flow/README.md) - Framework principal de orquestração de pipelines
- [Myth.Commons](../Myth.Commons/README.md) - Utilitários e extensões comuns
- [Myth.Repository](../Myth.Repository/README.md) - Implementação do padrão Repository
