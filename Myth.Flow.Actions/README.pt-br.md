# Myth.Flow.Actions

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Flow.Actions?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Flow.Actions/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Flow.Actions?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Flow.Actions/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

Uma poderosa biblioteca .NET que implementa os padrões CQRS e Arquitetura Orientada a Eventos com integração perfeita aos pipelines Myth.Flow. Construída para escalabilidade com suporte a múltiplos message brokers, estratégias de cache e recursos de resiliência de nível empresarial.

## 🚀 API Pipeline Action-First

Esta biblioteca apresenta uma abordagem revolucionária **Action-First** que elimina o boilerplate de contexto e simplifica drasticamente o desenvolvimento de pipelines:

```csharp
// ❌ ANTIGO: Baseado em contexto (muito boilerplate)
Pipeline.Start(context)
    .Process<Context, Command>(ctx => new Command { ... }, (ctx, result) => ctx.Result = result)

// ✅ NOVO: Action-First (limpo e direto)
Pipeline.Start(new Command { ... }, serviceProvider)
    .Process<Command, Result>()
```

**Benefícios:**
- **70% menos código boilerplate** - Nenhuma classe de contexto necessária
- **Transformações type-safe** - Fluxo direto objeto-para-objeto no pipeline
- **Experiência de desenvolvedor intuitiva** - Actions prontas para execução
- **Configuração fluente de cache** - `x => x.UseCache("key", TimeSpan.FromMinutes(5))`
- **Pipelines utilitários** - Iniciar sem parâmetros para cenários funcionais

# ⭐ Funcionalidades

- **API Action-First**: Abordagem revolucionária de pipeline com zero boilerplate de contexto
- **Padrão CQRS**: Separação clara de Commands, Queries e Events
- **Integração com Pipeline**: Integração fluente com Myth.Flow para workflows compostos
- **Múltiplos Message Brokers**: Suporte a InMemory (dev/test), Kafka e RabbitMQ
- **Cache de Queries**: Cache integrado com provedores Memory e Redis com configuração fluente
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

# 🏗️ Arquitetura de Configuração

**Flow.Actions segue uma abordagem unificada de configuração onde aspectos transversais são controlados pela configuração base do Flow:**

## Responsabilidade de Configuração

| **Configuração do Flow** | **Configuração do Actions** |
|---------------------------|------------------------------|
| ✅ Telemetria (UseTelemetry/DisableTelemetry) | ✅ Message Brokers (UseInMemory/UseKafka/UseRabbitMQ) |
| ✅ Políticas de Retry (UseRetry/DisableRetry) | ✅ Cache de Queries (UseCaching) |
| ✅ Logging (UseLogging/DisableLogging) | ✅ Dead Letter Queue (UseDeadLetterQueue) |
| ✅ Filtros de Exceção (UseExceptionFilter) | ✅ Descoberta de Handlers (ScanAssemblies) |
| ✅ Activity Sources (UseActivitySource) | ✅ Inscrição de Eventos (AutoSubscribeEventHandlers) |

Este design garante **consistência** em toda a aplicação e **previne conflitos de configuração** entre operações de pipeline e CQRS.

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

## 3. Usar Pipeline Action-First

### Exemplo de Pipeline Simples

```csharp
using Myth.Flow.Actions;

// Execução direta de action - sem contexto necessário!
var result = await Pipeline
    .Start(new CreateUserCommand { Email = "usuario@exemplo.com", Name = "João Silva" }, serviceProvider)
    .Process<CreateUserCommand, Guid>()
    .ExecuteAsync();

if (result.IsSuccess)
{
    Console.WriteLine($"Usuário criado com ID: {result.Value}");
}
```

### Exemplo de Workflow Complexo

```csharp
// Encadear operações com transformações
var result = await Pipeline
    .Start(new CreateUserCommand { Email = "usuario@exemplo.com", Name = "João Silva" }, serviceProvider)
    .Process<CreateUserCommand, Guid>()                                        // Command → Guid
    .Transform(userId => new GetUserQuery { UserId = userId })                 // Guid → Query
    .Query<GetUserQuery, UserDto>(x => x.UseCache($"user:{userId}", TimeSpan.FromMinutes(10))) // Query com cache
    .Transform(user => new UserCreatedEvent { UserId = user.Id, Email = user.Email })          // User → Event
    .Publish<UserCreatedEvent>()                                               // Publicar evento
    .ExecuteAsync();

if (result.IsSuccess)
{
    Console.WriteLine("Workflow de criação de usuário concluído com sucesso!");
}
```

### Pipeline Utilitário (Início Vazio)

```csharp
// Iniciar sem dados iniciais para funções utilitárias
var result = await Pipeline
    .Start(serviceProvider)
    .Transform(() => new GetActiveUsersQuery())
    .Query<GetActiveUsersQuery, List<UserDto>>()
    .Transform(users => new GenerateReportCommand { Users = users })
    .Process<GenerateReportCommand, ReportDto>()
    .ExecuteAsync();
```

## 🛠️ Steps Intermediários de Pipeline

A API Action-First agora suporta todos os métodos de pipeline do Myth.Flow para adicionar lógica personalizada, validação, telemetria e padrões de resiliência **entre** operações de action:

### Validação e Steps Personalizados

```csharp
public class UserCommandPipeline
{
    private readonly IValidationService _validationService;
    private readonly IUserService _userService;

    public UserCommandPipeline(
        IValidationService validationService,
        IUserService userService)
    {
        _validationService = validationService;
        _userService = userService;
    }

    public async Task<Result<Guid>> CreateUserAsync(CreateUserCommand command)
    {
        var result = await Pipeline
            .Start(command, serviceProvider)
            // Validar entrada antes do processamento
            .Step(state => {
                _validationService.ValidateEmail(state.CurrentRequest!.Email);
                return state;
            })
            // Adicionar lógica de negócio personalizada
            .StepAsync(async state => {
                await _userService.CheckUserLimitsAsync(state.CurrentRequest!.Email);
                return state;
            })
            .Process<CreateUserCommand, Guid>()
            .ExecuteAsync();

        return result;
    }
}
```

### Efeitos Colaterais e Logging

```csharp
public class UserQueryPipeline
{
    private readonly ILogger<UserQueryPipeline> _logger;
    private readonly IMetricsService _metricsService;

    public UserQueryPipeline(
        ILogger<UserQueryPipeline> logger,
        IMetricsService metricsService)
    {
        _logger = logger;
        _metricsService = metricsService;
    }

    public async Task<Result<UserDto>> GetUserAsync(Guid userId)
    {
        var result = await Pipeline
            .Start(new GetUserQuery { UserId = userId }, serviceProvider)
            // Registrar o início da operação
            .Tap(state =>
                _logger.LogInformation("Consultando usuário {UserId}", state.CurrentRequest!.UserId))
            .Query<GetUserQuery, UserDto>()
            // Registrar após consulta bem-sucedida
            .TapAsync(async state =>
                await _metricsService.RecordQueryExecutionAsync("GetUser"))
            .ExecuteAsync();

        return result;
    }
}
```

### Execução Condicional

```csharp
public class OrderCommandPipeline
{
    private readonly IPaymentService _paymentService;

    public OrderCommandPipeline(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<Result<OrderResult>> ProcessOrderAsync(ProcessOrderCommand command)
    {
        var result = await Pipeline
            .Start(command, serviceProvider)
            // Validar pagamento apenas se o pedido exigir
            .When(state => state.CurrentRequest!.RequiresPayment, builder =>
                builder.StepAsync(state =>
                    _paymentService.ValidatePaymentMethodAsync(state.CurrentRequest!.PaymentInfo)))
            .Process<ProcessOrderCommand, OrderResult>()
            .ExecuteAsync();

        return result;
    }
}
```

### Resiliência e Telemetria

```csharp
var result = await Pipeline
    .Start(new CallExternalApiQuery { Endpoint = "users" }, serviceProvider)
    // Adicionar rastreamento de telemetria
    .WithTelemetry("ExternalApiCall")
    // Configurar política de retry para chamadas externas
    .WithRetry(maxAttempts: 3, backoffMs: 1000)
    .Query<CallExternalApiQuery, ApiResponse>()
    .ExecuteAsync();
```

### Workflows Complexos com Múltiplos Steps

```csharp
public class OrderWorkflowPipeline
{
    private readonly ICustomerService _customerService;
    private readonly IInventoryService _inventoryService;
    private readonly ILogger<OrderWorkflowPipeline> _logger;
    private readonly INotificationService _notificationService;

    public OrderWorkflowPipeline(
        ICustomerService customerService,
        IInventoryService inventoryService,
        ILogger<OrderWorkflowPipeline> logger,
        INotificationService notificationService)
    {
        _customerService = customerService;
        _inventoryService = inventoryService;
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task<Result<OrderResult>> CreateOrderAsync(
        Guid customerId,
        List<OrderItem> items)
    {
        var result = await Pipeline
            .Start(new CreateOrderCommand { CustomerId = customerId, Items = items }, serviceProvider)
            // Validar cliente
            .StepAsync(async state => {
                await _customerService.ValidateCustomerAsync(state.CurrentRequest!.CustomerId);
                return state;
            })
            // Verificar inventário
            .StepAsync(async state => {
                await _inventoryService.ReserveItemsAsync(state.CurrentRequest!.Items);
                return state;
            })
            // Registrar antes do processamento
            .Tap(state =>
                _logger.LogInformation("Processando pedido para cliente {CustomerId}",
                    state.CurrentRequest!.CustomerId))
            // Processar o pedido
            .Process<CreateOrderCommand, OrderResult>()
            // Enviar notificação após sucesso
            .TapAsync(async state =>
                await _notificationService.SendOrderConfirmationAsync(state.CurrentRequest!))
            .ExecuteAsync();

        return result;
    }
}
```

### Métodos Intermediários Disponíveis

- **`Step()`** - Operações síncronas (use dependências injetadas via construtor)
- **`StepAsync()`** - Operações assíncronas (use dependências injetadas via construtor)
- **`StepResult()`** - Operações retornando `Result<T>` para tratamento de erro
- **`StepResultAsync()`** - Operações assíncronas retornando `Result<T>`
- **`Tap()`** - Efeitos colaterais (logging, métricas, eventos) usando serviços injetados
- **`TapAsync()`** - Efeitos colaterais assíncronos usando serviços injetados
- **`When(predicate, configure)`** - Execução condicional de pipeline
- **`WithRetry(maxAttempts, backoffMs)`** - Políticas de retry com backoff exponencial
- **`WithTelemetry(operationName)`** - Rastreamento distribuído OpenTelemetry

Todos os métodos mantêm o design de API fluente e podem ser encadeados para workflows complexos preservando a segurança de tipos e a abordagem Action-First. Use injeção via construtor para fornecer dependências à sua classe de pipeline, então referencie essas dependências nas etapas do pipeline.

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

# 📚 API Pipeline Action-First

## Iniciar Pipeline

```csharp
// Iniciar com um objeto de request
Pipeline.Start(command, serviceProvider)
Pipeline.Start(query, serviceProvider)
Pipeline.Start(event, serviceProvider)

// Iniciar sem dados iniciais (para funções utilitárias)
Pipeline.Start(serviceProvider)
```

## Process (Commands)

```csharp
// Command sem resposta (quando TCommand : ICommand)
.Process<TCommand>()

// Command com resposta tipada (quando TCommand : ICommand<TResponse>)
.Process<TCommand, TResponse>()
```

## Query (Operações de Leitura)

```csharp
// Query sem cache
.Query<TQuery, TResponse>()

// Query com configuração de cache usando API fluente
.Query<TQuery, TResponse>(x => x
    .UseCache("chave-cache", TimeSpan.FromMinutes(10))
    .WithSlidingExpiration())

// Query com configuração simples de cache
.Query<TQuery, TResponse>(x => x.UseCache($"chave:{algumaId}", TimeSpan.FromMinutes(5)))
```

## Publish (Eventos)

```csharp
// Publicar evento (quando TEvent : IEvent)
.Publish<TEvent>()
```

## Transform

```csharp
// Transformar request atual para novo tipo
.Transform<TNext>(current => new TNext { /* ... */ })

// Transformação assíncrona
.TransformAsync<TNext>(async current => await CreateNextAsync(current))

// Transformação condicional
.TransformIf<TNext>(
    condition: current => current.IsValid,
    transform: current => new TNext { /* ... */ })

// Condicional com branches verdadeiro/falso
.TransformIf<TNext>(
    condition: current => current.Type == "Premium",
    transformTrue: current => new PremiumAction { /* ... */ },
    transformFalse: current => new StandardAction { /* ... */ })

// Transformação de pipeline vazio (quando iniciando sem dados)
.Transform<TRequest>(() => new TRequest { /* ... */ })
.TransformAsync<TRequest>(async () => await CreateRequestAsync())
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
        UseDeadLetterQueue = true,
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

## Processamento de Pedido de Alto Valor

```csharp
// Pipeline action-first com lógica condicional usando TransformIf
var result = await Pipeline
    .Start(new ValidateOrderCommand { OrderId = orderId }, serviceProvider)
    .Process<ValidateOrderCommand, OrderDto>()
    .TransformIf<FraudCheckCommand>(
        order => order.TotalAmount > 1000,
        order => new FraudCheckCommand { OrderId = order.Id })
    .Process<FraudCheckCommand>()
    .Transform(fraudResult => new ProcessPaymentCommand { OrderId = orderId })
    .Process<ProcessPaymentCommand>()
    .Transform(paymentResult => new OrderCompletedEvent { OrderId = orderId })
    .Publish<OrderCompletedEvent>()
    .ExecuteAsync();
```

## Workflow de Pedido para Envio

```csharp
// Transformações diretas entre diferentes tipos de action
var result = await Pipeline
    .Start(new CreateOrderCommand
    {
        Items = items,
        CustomerId = customerId,
        ShippingAddress = address
    }, serviceProvider)
    .Process<CreateOrderCommand, Guid>()                           // Criar pedido → OrderId
    .Transform(orderId => new GetOrderQuery { OrderId = orderId }) // OrderId → Query
    .Query<GetOrderQuery, OrderDto>()                              // Obter detalhes completos do pedido
    .Transform(order => new CreateShipmentCommand                  // Order → Command de envio
    {
        OrderId = order.Id,
        ShipmentId = Guid.NewGuid(),
        Address = order.ShippingAddress,
        Items = order.Items
    })
    .Process<CreateShipmentCommand, ShipmentDto>()                 // Processar envio
    .Transform(shipment => new ShipmentCreatedEvent               // Shipment → Event
    {
        OrderId = shipment.OrderId,
        ShipmentId = shipment.Id,
        TrackingNumber = shipment.TrackingNumber
    })
    .Publish<ShipmentCreatedEvent>()                              // Notificar sobre envio
    .ExecuteAsync();
```

## Pipeline de Geração de Relatórios

```csharp
// Pipeline utilitário iniciando sem dados iniciais
var result = await Pipeline
    .Start(serviceProvider)
    .Transform(() => new GetMonthlyOrdersQuery { Month = DateTime.Now.Month })
    .Query<GetMonthlyOrdersQuery, List<OrderDto>>(x => x.UseCache("pedidos-mensais", TimeSpan.FromHours(1)))
    .Transform(orders => new GenerateReportCommand
    {
        Orders = orders,
        ReportType = ReportType.Monthly,
        GeneratedBy = currentUserId
    })
    .Process<GenerateReportCommand, ReportDto>()
    .Transform(report => new ReportGeneratedEvent
    {
        ReportId = report.Id,
        GeneratedBy = currentUserId
    })
    .Publish<ReportGeneratedEvent>()
    .ExecuteAsync();
```

# 🧪 Testes

## Testando Pipelines Action-First

```csharp
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Flow.Actions;

public class UserPipelineTests
{
    private readonly IServiceProvider _serviceProvider;

    public UserPipelineTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlow();
        services.AddFlowActions(config =>
        {
            config.UseInMemory()
                   .UseCaching(cache => cache.ProviderType = CacheProviderType.Memory)
                   .ScanAssemblies(typeof(CreateUserCommand).Assembly);
        });

        services.AddScoped<IUserRepository, InMemoryUserRepository>();
        services.AddScoped<IEmailService, FakeEmailService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task CreateUser_ComAPIActionFirst_DeveSerBemSucedido()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "teste@exemplo.com",
            Name = "Usuário Teste"
        };

        // Act - Usando API action-first
        var result = await Pipeline
            .Start(command, _serviceProvider)
            .Process<CreateUserCommand, Guid>()
            .ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task WorkflowCompletoUsuario_DeveEncadearOperacoes()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "workflow@exemplo.com",
            Name = "Usuário Workflow"
        };

        // Act - Encadear múltiplas operações
        var result = await Pipeline
            .Start(command, _serviceProvider)
            .Process<CreateUserCommand, Guid>()                                        // Criar usuário
            .Transform(userId => new GetUserQuery { UserId = userId })                 // Transformar para query
            .Query<GetUserQuery, UserDto>(x => x.UseCache($"user:{userId}", TimeSpan.FromMinutes(5))) // Obter usuário com cache
            .Transform(user => new UserCreatedEvent { UserId = user.Id, Email = user.Email })          // Transformar para evento
            .Publish<UserCreatedEvent>()                                               // Publicar evento
            .ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task PipelineVazio_ComTransforms_DeveFuncionar()
    {
        // Act - Iniciar sem dados iniciais
        var result = await Pipeline
            .Start(_serviceProvider)
            .Transform(() => new GetActiveUsersQuery())
            .Query<GetActiveUsersQuery, List<UserDto>>()
            .ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task WorkflowCondicional_DeveExecutarBaseadoNaCondicao()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "premium@exemplo.com",
            Name = "Usuário Premium"
        };

        // Act - Transformação condicional
        var result = await Pipeline
            .Start(command, _serviceProvider)
            .Process<CreateUserCommand, Guid>()
            .Transform(userId => new GetUserQuery { UserId = userId })
            .Query<GetUserQuery, UserDto>()
            .TransformIf<SendWelcomeEmailCommand>(
                user => user.Email.Contains("premium"),
                user => new SendWelcomeEmailCommand { Email = user.Email, IsPremium = true })
            .Process<SendWelcomeEmailCommand>()
            .ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}

## Testando Handlers Individuais

```csharp
public class CreateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_ComCommandValido_DeveRetornarSucesso()
    {
        // Arrange
        var repository = new InMemoryUserRepository();
        var handler = new CreateUserCommandHandler(repository);
        var command = new CreateUserCommand
        {
            Email = "teste@exemplo.com",
            Name = "Usuário Teste"
        };

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBe(Guid.Empty);
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
