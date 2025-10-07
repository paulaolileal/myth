# Myth.Flow

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Flow?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Flow/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Flow?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Flow/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

Uma poderosa biblioteca .NET para construir pipelines de processamento de dados manuteníveis e testáveis com uma interface fluente e encadeável. Construída com recursos de nível empresarial incluindo políticas de retry automáticas, integração com OpenTelemetry, injeção de dependência e tratamento abrangente de erros.

# ⭐ Recursos

- **Interface Fluente**: Design de API simples e encadeável para código legível
- **Segurança de Tipos**: Tipagem forte com suporte a transformação de contexto
- **Retry Automático**: Políticas de retry configuráveis com backoff exponencial
- **Integração OpenTelemetry**: Rastreamento e observabilidade integrados
- **Injeção de Dependência**: Integração completa com DI do ASP.NET Core
- **Padrão Result**: Programação orientada a railway com `Result<T>`
- **Tratamento de Erros**: Tratamento abrangente de erros com exceções customizadas
- **Execução Condicional**: Execute etapas baseadas em predicados do contexto
- **Efeitos Colaterais**: Intercepte o pipeline para logging, métricas e eventos
- **Async/Await**: Suporte async de primeira classe em toda biblioteca

# 📦 Instalação

```bash
dotnet add package Myth.Flow
```

# 🚀 Início Rápido

## Uso Básico

```csharp
var input = new OrderContext { OrderId = 123 };

var result = await Pipeline.Start(input)
    .StepAsync<ValidationService>((svc, ctx) => svc.ValidateOrderAsync(ctx))
    .StepAsync<PaymentService>((svc, ctx) => svc.ProcessPaymentAsync(ctx))
    .StepAsync<InventoryService>((svc, ctx) => svc.ReserveItemsAsync(ctx))
    .Tap(ctx => Console.WriteLine($"Pedido {ctx.OrderId} concluído"))
    .ExecuteAsync();

if (result.IsSuccess)
{
    Console.WriteLine("Pedido processado com sucesso!");
}
```

## Configuração com Injeção de Dependência

### Program.cs (Minimal API)

```csharp
builder.Services.AddFlow(config =>
{
    config.EnableTelemetry = true;
    config.EnableLogging = true;
    config.DefaultRetryAttempts = 3;
    config.DefaultBackoffMs = 100;
});

// Registre seus serviços
builder.Services.AddScoped<ValidationService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<InventoryService>();
```

### Usando em Controllers/Services

```csharp
public class OrderController : ControllerBase
{
    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var context = new OrderContext { Request = request };

        var result = await Pipeline.Start(context)
            .WithTelemetry("CreateOrder")
            .WithRetry(maxAttempts: 3, backoffMs: 100)
            .StepResultAsync<OrderValidationService>(
                (svc, ctx) => svc.ValidateAsync(ctx))
            .StepResultAsync<OrderCreationService>(
                (svc, ctx) => svc.CreateAsync(ctx))
            .TapAsync<OrderEventService>(
                (svc, ctx) => svc.PublishOrderCreatedAsync(ctx))
            .Transform<OrderResponse>(ctx => new OrderResponse
            {
                OrderId = ctx.CreatedOrder!.Id,
                Status = ctx.CreatedOrder.Status
            })
            .ExecuteAsync();

        if (result.IsFailure)
            return BadRequest(new { error = result.ErrorMessage });

        return Ok(result.Value);
    }
}
```

# 🔧 Configuração

## Configuração Básica

```csharp
builder.Services.AddFlow(config =>
{
    config.EnableTelemetry = true;           // Habilita rastreamento OpenTelemetry
    config.EnableLogging = true;             // Habilita logging
    config.DefaultRetryAttempts = 3;         // Tentativas de retry padrão
    config.DefaultBackoffMs = 100;           // Backoff padrão em milissegundos
    config.ActivitySource = activitySource;  // ActivitySource customizado (opcional)
});
```

## Configuração do Pipeline

```csharp
var result = await Pipeline.Start(context)
    .WithTelemetry("OperationName")          // Define nome da operação para rastreamento
    .WithRetry(maxAttempts: 5, backoffMs: 200) // Configura retry para etapas subsequentes
    .StepAsync<MyService>((svc, ctx) => svc.ProcessAsync(ctx))
    .ExecuteAsync();
```

# 🔄 Etapas do Pipeline

## Etapas Síncronas

```csharp
.Step<MyService>((svc, ctx) => 
{
    // Processamento síncrono
    ctx.Data = svc.Transform(ctx.Data);
    return ctx;
})
```

## Etapas Assíncronas

```csharp
.StepAsync<MyService>((svc, ctx) => 
    svc.ProcessAsync(ctx))
```

## Etapas com Padrão Result

```csharp
.StepResultAsync<ValidationService>((svc, ctx) => 
    svc.ValidateAsync(ctx))
```

O padrão `Result<T>` permite que etapas retornem sucesso ou falha:

```csharp
public async Task<Result<OrderContext>> ValidateAsync(OrderContext context)
{
    if (string.IsNullOrEmpty(context.Request.Email))
        return Result<OrderContext>.Failure("Email é obrigatório");

    if (context.Request.Amount <= 0)
        return Result<OrderContext>.Failure("Valor deve ser positivo");

    return Result<OrderContext>.Success(context);
}
```

## Transformação de Contexto

Transforme o contexto do pipeline para um tipo diferente:

```csharp
.Transform<OutputContext>(ctx => new OutputContext
{
    Id = ctx.Entity.Id,
    Name = ctx.Entity.Name
})

.TransformAsync<OutputContext>(async ctx =>
{
    var data = await _service.GetDataAsync(ctx.Id);
    return new OutputContext { Data = data };
})
```

## Efeitos Colaterais (Tap)

Execute ações sem modificar o contexto:

```csharp
// Tap simples
.Tap(ctx => Console.WriteLine($"Processando: {ctx.Id}"))

// Tap assíncrono
.TapAsync(async ctx => 
    await _logger.LogAsync($"Etapa concluída: {ctx.Id}"))

// Tap com injeção de serviço
.TapAsync<EventPublisher>((svc, ctx) => 
    svc.PublishAsync(new OrderCreated(ctx.OrderId)))

.Tap<MetricsService>((svc, ctx) => 
    svc.IncrementCounter("orders_created"))
```

## Execução Condicional

```csharp
.When(
    ctx => ctx.Amount > 1000,
    pipeline => pipeline
        .StepAsync<FraudDetectionService>((svc, ctx) => 
            svc.CheckAsync(ctx))
        .StepAsync<ApprovalService>((svc, ctx) => 
            svc.RequestApprovalAsync(ctx)))
```

# 🔁 Políticas de Retry

Configure comportamento de retry para pipelines resilientes:

## Configuração Global de Retry

```csharp
builder.Services.AddFlow(config =>
{
    config.DefaultRetryAttempts = 3;
    config.DefaultBackoffMs = 100;
});
```

## Retry por Pipeline

```csharp
.WithRetry(maxAttempts: 5, backoffMs: 200)
```

Comportamento do retry:
- Backoff exponencial: delay = backoffMs × númeroDaTentativa
- Retry apenas em exceções (não em `Result.Failure`)
- `OperationCanceledException` nunca sofre retry
- Etapas individuais herdam a configuração de retry

## Exemplo de Retry

```csharp
var result = await Pipeline.Start(context)
    .WithRetry(maxAttempts: 3, backoffMs: 100)
    .StepAsync<ExternalApiService>((svc, ctx) => 
        svc.CallUnreliableApiAsync(ctx)) // Fará retry em exceções
    .ExecuteAsync();
```

# 📊 Observabilidade & Telemetria

## Integração OpenTelemetry

A biblioteca cria automaticamente activities para rastreamento distribuído:

```csharp
var result = await Pipeline.Start(context)
    .WithTelemetry("CreateUser")
    .StepResultAsync<ValidationService>((svc, ctx) => svc.ValidateAsync(ctx))
    .StepResultAsync<UserCreationService>((svc, ctx) => svc.CreateAsync(ctx))
    .ExecuteAsync();
```

Cada etapa cria uma activity filha com tags:
- `pipeline.input.type`: Nome do tipo do contexto
- Tags específicas da etapa e informações de timing

## Integração de Logging

```csharp
public class UserCreationService
{
    private readonly ILogger<UserCreationService> _logger;

    public async Task<Result<UserContext>> CreateAsync(UserContext context)
    {
        _logger.LogInformation("Criando usuário: {Email}", context.Request.Email);

        try
        {
            // Lógica de criação do usuário
            _logger.LogInformation(
                "Usuário {Email} criado com sucesso com ID: {UserId}",
                context.CreatedUser.Email,
                context.CreatedUser.Id);

            return Result<UserContext>.Success(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao criar usuário: {Email}", context.Request.Email);
            return Result<UserContext>.Failure("Falha ao criar usuário", ex);
        }
    }
}
```

## Exemplo de Métricas

```csharp
public interface IUserMetrics
{
    void IncrementUserCreated();
    void IncrementUserFailed();
}

public class UserMetricsService : IUserMetrics
{
    private int _usersCreated;
    private int _usersFailed;

    public void IncrementUserCreated() => Interlocked.Increment(ref _usersCreated);
    public void IncrementUserFailed() => Interlocked.Increment(ref _usersFailed);
}

// Usar no pipeline
.TapAsync<UserObservabilityService>((svc, ctx) => 
    svc.RecordMetricsAsync(ctx))
```

# 🏗️ Padrões Avançados

## Padrão Repository com Transações

```csharp
public class UserCreationService
{
    private readonly IUserRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserCreationService> _logger;

    public async Task<Result<UserContext>> CreateUserAsync(UserContext context)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var user = new User
            {
                Email = context.Request.Email,
                PasswordHash = context.PasswordHash,
                Role = context.Request.Role
            };

            context.CreatedUser = await _repository.CreateAsync(user);
            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Usuário {Email} criado", user.Email);
            return Result<UserContext>.Success(context);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync();
            _logger.LogError(ex, "Falha ao criar usuário");
            return Result<UserContext>.Failure("Falha ao criar usuário", ex);
        }
    }
}
```

## Arquitetura Orientada a Eventos

```csharp
var result = await Pipeline.Start(context)
    .WithTelemetry("ProcessOrder")
    .StepResultAsync<OrderValidationService>(
        (svc, ctx) => svc.ValidateAsync(ctx))
    .StepResultAsync<OrderCreationService>(
        (svc, ctx) => svc.CreateAsync(ctx))
    .TapAsync<OrderEventService>(
        (svc, ctx) => svc.PublishOrderCreatedAsync(ctx))
    .TapAsync<NotificationService>(
        (svc, ctx) => svc.SendConfirmationEmailAsync(ctx))
    .TapAsync<MetricsService>(
        (svc, ctx) => svc.RecordOrderCreatedAsync(ctx))
    .Transform<OrderResponse>(ctx => new OrderResponse
    {
        OrderId = ctx.Order.Id,
        Status = ctx.Order.Status,
        CreatedAt = ctx.Order.CreatedAt
    })
    .ExecuteAsync();
```

## Pipeline de Validação Multi-Etapas

```csharp
public class UserRegistrationPipeline
{
    public async Task<Result<UserResponse>> RegisterAsync(RegisterUserRequest request)
    {
        var context = new UserContext { Request = request };

        var result = await Pipeline.Start(context)
            .WithTelemetry("RegisterUser")
            .WithRetry(maxAttempts: 2)
            
            // Etapas de validação
            .StepResultAsync<EmailValidationService>(
                (svc, ctx) => svc.ValidateEmailAsync(ctx))
            .StepResultAsync<PasswordValidationService>(
                (svc, ctx) => svc.ValidatePasswordAsync(ctx))
            .StepResultAsync<RoleValidationService>(
                (svc, ctx) => svc.ValidateRoleAsync(ctx))
            
            // Etapa de criação
            .StepResultAsync<UserCreationService>(
                (svc, ctx) => svc.CreateUserAsync(ctx))
            
            // Efeitos colaterais
            .TapAsync<EventPublisher>(
                (svc, ctx) => svc.PublishAsync(new UserRegistered(ctx.CreatedUser.Id)))
            .TapAsync<EmailService>(
                (svc, ctx) => svc.SendWelcomeEmailAsync(ctx.CreatedUser.Email))
            .TapAsync<MetricsService>(
                (svc, ctx) => svc.IncrementUserRegistrations())
            
            // Transformar para resposta
            .Transform<UserResponse>(ctx => new UserResponse
            {
                Id = ctx.CreatedUser.Id,
                Email = ctx.CreatedUser.Email,
                Role = ctx.CreatedUser.Role,
                CreatedAt = ctx.CreatedUser.CreatedAt
            })
            .ExecuteAsync();

        return result;
    }
}
```

## Processamento Condicional

```csharp
var result = await Pipeline.Start(context)
    .StepResultAsync<OrderValidationService>(
        (svc, ctx) => svc.ValidateAsync(ctx))
    
    .When(
        ctx => ctx.Order.Amount > 1000,
        pipeline => pipeline
            .StepAsync<FraudCheckService>(
                (svc, ctx) => svc.CheckAsync(ctx))
            .StepAsync<ManagerApprovalService>(
                (svc, ctx) => svc.RequestApprovalAsync(ctx)))
    
    .When(
        ctx => ctx.Order.IsInternational,
        pipeline => pipeline
            .StepAsync<ComplianceService>(
                (svc, ctx) => svc.CheckComplianceAsync(ctx))
            .StepAsync<CurrencyConversionService>(
                (svc, ctx) => svc.ConvertCurrencyAsync(ctx)))
    
    .StepResultAsync<PaymentService>(
        (svc, ctx) => svc.ProcessPaymentAsync(ctx))
    .ExecuteAsync();
```

# ❌ Tratamento de Erros

## Padrão Result

A biblioteca usa o padrão Result para tratamento explícito de erros:

```csharp
public readonly struct Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public Exception? Exception { get; }
}
```

## Manipulando Resultados do Pipeline

```csharp
var result = await Pipeline.Start(context)
    .StepResultAsync<ValidationService>((svc, ctx) => svc.ValidateAsync(ctx))
    .StepResultAsync<ProcessingService>((svc, ctx) => svc.ProcessAsync(ctx))
    .ExecuteAsync();

if (result.IsSuccess)
{
    var data = result.Value;
    // Tratar sucesso
}
else
{
    var errorMessage = result.ErrorMessage;
    var exception = result.Exception;
    // Tratar falha
}
```

## Tipos de Exceção

- `PipelineException`: Erros gerais de execução do pipeline
- `PipelineConfigurationException`: Erros de configuração (serviços faltando, configuração inválida)

Exceções de configuração são fail-fast e sempre são relançadas para prevenir falhas silenciosas.

## Callbacks de Sucesso e Erro

```csharp
.StepAsync<MyService>(
    (svc, ctx) => svc.ProcessAsync(ctx),
    onSuccess: ctx => _logger.LogInformation("Etapa bem-sucedida"),
    onError: ex => _logger.LogError(ex, "Etapa falhou"))
```

# 🧪 Testes

O design do pipeline torna os testes simples:

```csharp
[Fact]
public async Task CreateUser_WithValidData_ShouldSucceed()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddSingleton<IUserRepository>(mockRepository);
    services.AddSingleton<IPasswordValidator>(mockPasswordValidator);
    services.AddSingleton<UserValidationService>();
    services.AddSingleton<UserCreationService>();
    services.AddLogging();
    services.AddFlow();

    var serviceProvider = services.BuildServiceProvider();
    var context = new UserContext { Request = validRequest };

    // Act
    var result = await Pipeline.Start(context, serviceProvider)
        .StepResultAsync<UserValidationService>(
            (svc, ctx) => svc.ValidateAsync(ctx))
        .StepResultAsync<UserCreationService>(
            (svc, ctx) => svc.CreateAsync(ctx))
        .ExecuteAsync();

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Value?.CreatedUser);
    Assert.Equal(validRequest.Email, result.Value.CreatedUser.Email);
}
```

# 📋 Melhores Práticas

1. **Use o Padrão Result**: Retorne `Result<T>` dos serviços para tratamento explícito de erros
2. **Configure Injeção de Dependência**: Sempre use DI para melhor testabilidade e manutenibilidade
3. **Habilite Telemetria**: Use `WithTelemetry()` para observabilidade em produção
4. **Configure Políticas de Retry**: Defina políticas de retry apropriadas para operações não confiáveis
5. **Separe Responsabilidades**: Mantenha as etapas focadas em responsabilidades únicas
6. **Use Tap para Efeitos Colaterais**: Mantenha efeitos colaterais (logging, métricas, eventos) separados do fluxo principal
7. **Trate Erros com Graça**: Sempre verifique `IsSuccess` antes de acessar `Value`
8. **Adicione Observabilidade**: Integre logging e métricas para monitoramento em produção
9. **Teste Etapas do Pipeline**: Teste serviços individuais e pipelines completos separadamente
10. **Use Execução Condicional**: Mantenha lógica condicional legível com `When()`

# 📊 Informações da Resposta

Toda execução do pipeline retorna informações abrangentes:

```csharp
var result = await Pipeline.Start(context)...ExecuteAsync();

Console.WriteLine($"É Sucesso: {result.IsSuccess}");
Console.WriteLine($"É Falha: {result.IsFailure}");
Console.WriteLine($"Mensagem de Erro: {result.ErrorMessage}");

if (result.IsSuccess)
{
    var value = result.Value;
    // Processar resultado bem-sucedido
}
```

# 🎯 Cenários Empresariais

## E-commerce com Diferentes Estratégias de Retry

```csharp
// Operações críticas - Retry conservador
builder.Services.AddFlow(config =>
{
    config.DefaultRetryAttempts = 2;
    config.DefaultBackoffMs = 200;
});

// Para APIs externas não confiáveis
var result = await Pipeline.Start(context)
    .WithRetry(maxAttempts: 5, backoffMs: 100)
    .StepAsync<ExternalPaymentService>((svc, ctx) => 
        svc.ProcessPaymentAsync(ctx))
    .ExecuteAsync();
```

## Comunicação entre Microserviços

```csharp
public class OrderOrchestrationService
{
    public async Task<Result<OrderResponse>> ProcessOrderAsync(CreateOrderRequest request)
    {
        var context = new OrderContext { Request = request };

        return await Pipeline.Start(context)
            .WithTelemetry("ProcessOrder")
            .WithRetry(maxAttempts: 3, backoffMs: 100)
            
            // Validação local
            .StepResultAsync<OrderValidationService>(
                (svc, ctx) => svc.ValidateAsync(ctx))
            
            // Chamada para serviço de usuário
            .StepAsync<UserServiceClient>(
                (svc, ctx) => svc.ValidateUserAsync(ctx))
            
            // Chamada para serviço de inventário
            .StepAsync<InventoryServiceClient>(
                (svc, ctx) => svc.ReserveItemsAsync(ctx))
            
            // Chamada para serviço de pagamento
            .StepAsync<PaymentServiceClient>(
                (svc, ctx) => svc.ProcessPaymentAsync(ctx))
            
            // Criar pedido localmente
            .StepResultAsync<OrderCreationService>(
                (svc, ctx) => svc.CreateOrderAsync(ctx))
            
            // Publicar evento
            .TapAsync<EventPublisher>(
                (svc, ctx) => svc.PublishOrderCreatedAsync(ctx))
            
            // Transformar resposta
            .Transform<OrderResponse>(ctx => new OrderResponse
            {
                OrderId = ctx.Order.Id,
                Status = ctx.Order.Status,
                TotalAmount = ctx.Order.TotalAmount
            })
            .ExecuteAsync();
    }
}
```

## Pipeline de Processamento de Dados

```csharp
public class DataProcessingPipeline
{
    public async Task<Result<ProcessedData>> ProcessAsync(RawData data)
    {
        var context = new DataContext { RawData = data };

        return await Pipeline.Start(context)
            .WithTelemetry("ProcessData")
            .WithRetry(maxAttempts: 3, backoffMs: 200)
            
            // Validação
            .StepResultAsync<DataValidationService>(
                (svc, ctx) => svc.ValidateAsync(ctx))
            
            // Limpeza
            .StepAsync<DataCleaningService>(
                (svc, ctx) => svc.CleanAsync(ctx))
            
            // Enriquecimento
            .StepAsync<DataEnrichmentService>(
                (svc, ctx) => svc.EnrichAsync(ctx))
            
            // Transformação
            .StepAsync<DataTransformationService>(
                (svc, ctx) => svc.TransformAsync(ctx))
            
            // Persistência
            .StepResultAsync<DataPersistenceService>(
                (svc, ctx) => svc.SaveAsync(ctx))
            
            // Notificação
            .TapAsync<NotificationService>(
                (svc, ctx) => svc.NotifyCompletionAsync(ctx))
            
            // Métricas
            .Tap<MetricsService>((svc, ctx) => 
                svc.RecordProcessingTime(ctx.StartTime, DateTime.UtcNow))
            
            // Resultado final
            .Transform<ProcessedData>(ctx => ctx.ProcessedData!)
            .ExecuteAsync();
    }
}
```

# 🔍 Diagnóstico & Monitoramento

A biblioteca inclui diagnósticos integrados usando a API Activity do .NET:

```csharp
// Activities são automaticamente criadas com tags:
// - http.url
// - http.method
// - Informações de timing da operação
```

A integração com OpenTelemetry e outras ferramentas de observabilidade é perfeita.

# 💡 Exemplos de Uso Real

## Sistema de Registro de Usuário

```csharp
public async Task<Result<UserResponse>> RegisterUserAsync(RegisterUserRequest request)
{
    var context = new UserContext { Request = request };

    return await Pipeline.Start(context)
        .WithTelemetry("RegisterUser")
        .WithRetry(maxAttempts: 2, backoffMs: 100)
        
        // Validações
        .StepResultAsync<EmailValidationService>(
            (svc, ctx) => svc.ValidateEmailUniqueAsync(ctx))
        .StepResultAsync<PasswordValidationService>(
            (svc, ctx) => svc.ValidatePasswordStrengthAsync(ctx))
        .StepResultAsync<RoleValidationService>(
            (svc, ctx) => svc.ValidateRoleAsync(ctx))
        
        // Criação com transação
        .StepResultAsync<UserCreationService>(
            (svc, ctx) => svc.CreateUserWithTransactionAsync(ctx))
        
        // Eventos e notificações
        .TapAsync<EventPublisher>(
            (svc, ctx) => svc.PublishAsync(new UserRegistered(ctx.CreatedUser!.Id)))
        .TapAsync<EmailService>(
            (svc, ctx) => svc.SendWelcomeEmailAsync(ctx.CreatedUser!.Email))
        
        // Observabilidade
        .TapAsync<ObservabilityService>((svc, ctx) =>
        {
            svc.Logger.LogInformation(
                "Usuário {Email} criado com sucesso",
                ctx.CreatedUser!.Email);
            svc.Metrics.IncrementUserCreated();
            return Task.CompletedTask;
        })
        
        // Resposta
        .Transform<UserResponse>(ctx => new UserResponse
        {
            Id = ctx.CreatedUser!.Id,
            Email = ctx.CreatedUser.Email,
            Role = ctx.CreatedUser.Role,
            CreatedAt = ctx.CreatedUser.CreatedAt
        })
        .ExecuteAsync();
}
```

# 📖 Documentação Adicional

Para mais informações sobre configuração avançada, padrões de uso e exemplos, consulte a [documentação completa](https://github.com/yourusername/Myth.Flow/wiki).

# 📄 Licença

Este projeto está licenciado sob a Licença Apache 2.0 - veja o arquivo LICENSE para detalhes.

# 🤝 Contribuindo

Contribuições são bem-vindas! Por favor, sinta-se à vontade para enviar um Pull Request.

# 📧 Suporte

Para problemas, perguntas ou contribuições, visite o [repositório GitHub](https://github.com/paulaolileal/myth/).