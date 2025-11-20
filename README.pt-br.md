<img  style="float: right;" src="myth-ecossystem-logo.png" alt="drawing" width="250"/>

# Ecossistema Myth

![Gitlab Pipeline Status](https://img.shields.io/gitlab/pipeline-status/dotnet-myth%2Fmyth?style=for-the-badge) ![Gitlab Code Coverage](https://img.shields.io/gitlab/pipeline-coverage/dotnet-myth%2Fmyth?job_name=test_job&branch=main&style=for-the-badge) ![GitLab Tag](https://img.shields.io/gitlab/v/tag/dotnet-myth%2Fmyth?style=for-the-badge) ![GitLab Last Commit](https://img.shields.io/gitlab/last-commit/dotnet-myth%2Fmyth?ref=develop&style=for-the-badge)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

Uma coleção abrangente de bibliotecas .NET prontas para produção, projetadas para construir aplicações empresariais robustas e escaláveis. O ecossistema Myth promove arquitetura limpa, Domain-Driven Design (DDD) e práticas modernas de engenharia de software com código boilerplate mínimo.

## 🚀 Por que Escolher Myth?

- **🏗️ Arquitetura Limpa**: Construído com princípios DDD e padrões de arquitetura limpa
- **⚡ Experiência do Desenvolvedor**: APIs fluentes, configuração mínima e documentação extensa
- **🔄 Integração Perfeita**: Bibliotecas funcionam juntas através de um provedor de serviços global
- **📦 Pronto para Produção**: Padrões testados em batalha com tratamento abrangente de erros
- **🎯 Segurança de Tipos**: Segurança em tempo de compilação com recursos modernos do C#
- **🧪 Testável**: Projetado para testes unitários e mocking fáceis

## 📚 Início Rápido

### Aplicação ASP.NET Core

```csharp
using Myth.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Adicionar validação abrangente
builder.Services.AddGuard();

// Adicionar orquestração de pipeline com CQRS
builder.Services.AddFlow(config => config
    .UseTelemetry()
    .UseRetry(maxAttempts: 3, backoffMs: 1000)
    .UseActions(actions => actions
        .UseInMemory()        // ou .UseKafka() / .UseRabbitMQ()
        .UseCaching()
        .ScanAssemblies(typeof(Program).Assembly)));

// Adicionar transformação de objetos
builder.Services.AddMorph();

// Adicionar versionamento de API e Swagger
builder.Services.AddVersioning(1.0);
builder.Services.AddSwaggerVersioned(settings => {
    settings.Title = "Minha API Empresarial";
    settings.Description = "API pronta para produção com recursos abrangentes";
});

// Auto-registrar repositórios e serviços
builder.Services.AddServiceFromType<IRepository>();
builder.Services.AddServiceFromType<IDomainService>();

// Construir com provedor de serviços global (habilita integração entre bibliotecas)
var app = builder.BuildApp();

// Adicionar middleware de validação
app.UseGuard();
app.UseSwaggerVersioned();
app.MapControllers();

app.Run();
```

### Exemplo Completo de CQRS

```csharp
// Entidade de Domínio com Validação
public class CreateUserCommand : IValidatable<CreateUserCommand>, ICommand<UserDto> {
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }

    public void Validate(ValidationBuilder<CreateUserCommand> builder, ValidationContextKey? context = null) {
        builder.For(Name, x => x.NotEmpty().MinimumLength(2).MaximumLength(100));
        builder.For(Email, x => x.NotEmpty().Email());
        builder.For(Age, x => x.GreaterThan(0).LessThan(150));

        builder.InContext(ValidationContextKey.Create, b => {
            b.For(Email, x => x
                .RespectAsync(async (email, ct, sp) => {
                    var userRepo = sp.GetRequiredService<IUserRepository>();
                    return !await userRepo.ExistsByEmailAsync(email, ct);
                })
                .WithMessage("Email já existe"));
        });
    }
}

// Command Handler com Pipeline
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, UserDto> {
    private readonly IUserRepository _repository;
    private readonly IValidator _validator;
    private readonly IDispatcher _dispatcher;

    public CreateUserCommandHandler(IUserRepository repository, IValidator validator, IDispatcher dispatcher) {
        _repository = repository;
        _validator = validator;
        _dispatcher = dispatcher;
    }

    public async Task<CommandResult<UserDto>> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken) {
        return await Pipeline.Start(command)
            .WithTelemetry("CreateUser")
            .WithRetry(maxAttempts: 3)

            // Validar comando
            .StepResultAsync(cmd => _validator.ValidateAndReturnAsync(cmd, ValidationContextKey.Create))

            // Transformar para entidade
            .Transform<UserEntity>(cmd => cmd.To<UserEntity>())

            // Salvar no repositório
            .StepAsync(entity => _repository.AddAsync(entity))

            // Publicar evento de domínio
            .TapAsync(entity => _dispatcher.PublishEventAsync(new UserCreatedEvent { UserId = entity.Id }))

            // Transformar para DTO
            .Transform<UserDto>(entity => entity.To<UserDto>())

            .ExecuteAsync(cancellationToken);
    }
}

// Controller
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class UsersController : ControllerBase {
    private readonly IDispatcher _dispatcher;

    public UsersController(IDispatcher dispatcher) {
        _dispatcher = dispatcher;
    }

    /// <summary>
    /// Cria um novo usuário
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserCommand command) {
        var result = await _dispatcher.DispatchCommandAsync(command);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(GetUser), new { id = result.Data.Id }, result.Data);

        return BadRequest(result.ErrorMessage);
    }
}
```

## 🏛️ Padrões Arquiteturais

O ecossistema Myth habilita vários padrões de arquitetura empresarial:

### Arquitetura Limpa & DDD
```csharp
// Camada de Domínio
public class Order : IValidatable<Order> {
    public OrderId Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    public Money TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }

    public void Validate(ValidationBuilder<Order> builder, ValidationContextKey? context = null) {
        builder.For(TotalAmount, x => x.GreaterThan(Money.Zero));
        builder.For(Status, x => x.BeInEnum());
    }
}

// Camada de Aplicação
public class OrderService : IOrderService {
    private readonly IValidator _validator;
    private readonly IOrderRepository _orderRepository;
    private readonly IDispatcher _dispatcher;

    public OrderService(IValidator validator, IOrderRepository orderRepository, IDispatcher dispatcher) {
        _validator = validator;
        _orderRepository = orderRepository;
        _dispatcher = dispatcher;
    }

    public async Task<OrderDto> ProcessOrderAsync(CreateOrderCommand command) {
        return await Pipeline.Start(command)
            .StepResultAsync(cmd => _validator.ValidateAndReturnAsync(cmd))
            .Transform<Order>(cmd => new Order(cmd.CustomerId, cmd.Items))
            .StepAsync(order => _orderRepository.AddAsync(order))
            .TapAsync(order => _dispatcher.PublishEventAsync(new OrderCreatedEvent(order.Id)))
            .Transform<OrderDto>(order => order.To<OrderDto>())
            .ExecuteAsync();
    }
}
```

### Arquitetura Orientada a Eventos
```csharp
// Event Handlers para Preocupações Transversais
public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent> {
    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken) {
        // Enviar notificação por email
        // Atualizar analytics
        // Disparar verificação de estoque
    }
}

// Configuração do Event Bus
builder.Services.AddFlow(config => config.UseActions(actions => actions
    .UseKafka(kafka => kafka
        .WithBootstrapServers("localhost:9092")
        .WithGroupId("order-processing"))
    .UseDeadLetterQueue()
    .UseCircuitBreaker()));
```

### Padrão Repository com Specifications
```csharp
// Regras de Negócio como Specifications
public static class OrderSpecifications {
    public static ISpec<Order> ForCustomer(this ISpec<Order> spec, CustomerId customerId) =>
        spec.And(o => o.CustomerId == customerId);

    public static ISpec<Order> WithStatus(this ISpec<Order> spec, OrderStatus status) =>
        spec.And(o => o.Status == status);

    public static ISpec<Order> Recent(this ISpec<Order> spec, TimeSpan timespan) =>
        spec.And(o => o.CreatedAt >= DateTime.UtcNow.Subtract(timespan));
}

// Uso no Repository
public async Task<IPaginated<OrderDto>> GetCustomerOrdersAsync(CustomerId customerId, int page = 1) {
    var spec = SpecBuilder<Order>.Create()
        .ForCustomer(customerId)
        .WithStatus(OrderStatus.Completed)
        .Recent(TimeSpan.FromDays(30))
        .Order(o => o.CreatedAt)
        .Skip((page - 1) * 20)
        .Take(20);

    var orders = await _repository.FindAsync(spec);
    return orders.To<IPaginated<OrderDto>>();
}
```

# 📦 Bibliotecas

## 🔮 Fundação Central
- **[Myth.Commons](Myth.Commons/README.pt-br.md)** - Utilitários essenciais, objetos de valor e gerenciamento global de provedor de serviços
- **[Myth.DependencyInjection](Myth.DependencyInjection/README.pt-br.md)** - Auto-descoberta e registro de serviços baseado em convenções
- **[Myth.DependencyInjection.Providers](Myth.DependencyInjection.Providers/README.pt-br.md)** - Integrações pré-configuradas para Swagger, AutoMapper e versionamento de API

## 🔄 Fluxo de Dados & Orquestração
- **[Myth.Flow](Myth.Flow/README.pt-br.md)** - Orquestração de pipeline com padrão Result e integração OpenTelemetry
- **[Myth.Flow.Actions](Myth.Flow.Actions/README.pt-br.md)** - CQRS, arquitetura orientada a eventos com message brokers (Kafka, RabbitMQ)

## 🛡️ Validação & Segurança
- **[Myth.Guard](Myth.Guard/README.pt-br.md)** - Validação fluente com 100+ regras, validação sensível ao contexto e middleware ASP.NET Core

## 🔄 Transformação de Objetos
- **[Myth.Morph](Myth.Morph/README.pt-br.md)** - Transformação de objetos baseada em schema com tipos auto-mapeáveis

## 🗄️ Acesso a Dados
- **[Myth.Specification](Myth.Specification/README.pt-br.md)** - Padrão de especificação de consulta para encapsular regras de negócio
- **[Myth.Repository](Myth.Repository/README.pt-br.md)** - Interfaces do padrão repositório genérico com suporte assíncrono
- **[Myth.Repository.EntityFramework](Myth.Repository.EntityFramework/README.pt-br.md)** - Implementações Entity Framework Core com padrão Unit of Work

## 🌐 HTTP & APIs
- **[Myth.Rest](Myth.Rest/README.pt-br.md)** - Cliente REST fluente com circuit breaker, políticas de retry e suporte a certificados

## 🧪 Testes
- **[Myth.Testing](Myth.Testing/README.pt-br.md)** - Utilitários de teste, mocks e classes base para testes abrangentes

## 🏗️ Exemplos de Integração

### Microserviço de E-Commerce
```csharp
public class ProductCatalogService {
    private readonly IValidator _validator;
    private readonly IProductRepository _productRepository;
    private readonly IDispatcher _dispatcher;

    public ProductCatalogService(IValidator validator, IProductRepository productRepository, IDispatcher dispatcher) {
        _validator = validator;
        _productRepository = productRepository;
        _dispatcher = dispatcher;
    }

    public async Task<ProductDto> UpdateProductAsync(UpdateProductCommand command) {
        return await Pipeline.Start(command)
            // Validar comando com regras de negócio
            .StepResultAsync(cmd => _validator.ValidateAndReturnAsync(cmd, ValidationContextKey.Update))

            // Carregar produto existente usando specification
            .StepAsync(cmd => _productRepository.FirstOrDefaultAsync(ProductSpecifications.ById(cmd.ProductId)))

            // Aplicar lógica de negócio
            .Step((product, cmd) => {
                product.UpdateDetails(cmd.Name, cmd.Description, cmd.Price);
                return product;
            })

            // Salvar alterações
            .StepAsync(product => _productRepository.UpdateAsync(product))

            // Publicar evento de integração
            .TapAsync(product => _dispatcher.PublishEventAsync(new ProductUpdatedEvent {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price
            }))

            // Transformar para DTO
            .Transform<ProductDto>(product => product.To<ProductDto>())

            .ExecuteAsync();
    }
}
```

### Processamento de Pedidos Orientado a Eventos
```csharp
// Event Handlers de Pedidos
public class OrderEventHandlers :
    IEventHandler<OrderCreatedEvent>,
    IEventHandler<PaymentProcessedEvent>,
    IEventHandler<InventoryReservedEvent> {

    private readonly IInventoryService _inventoryService;
    private readonly IPaymentService _paymentService;
    private readonly IEmailService _emailService;
    private readonly IDispatcher _dispatcher;

    public OrderEventHandlers(IInventoryService inventoryService, IPaymentService paymentService, IEmailService emailService, IDispatcher dispatcher) {
        _inventoryService = inventoryService;
        _paymentService = paymentService;
        _emailService = emailService;
        _dispatcher = dispatcher;
    }

    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken) {
        await Pipeline.Start(@event)
            // Reservar estoque
            .TapAsync(evt => _inventoryService.ReserveItemsAsync(evt.OrderId, evt.Items))

            // Processar pagamento
            .TapAsync(evt => _paymentService.ProcessPaymentAsync(evt.OrderId, evt.TotalAmount))

            // Enviar email de confirmação
            .TapAsync(evt => _emailService.SendOrderConfirmationAsync(evt.CustomerId, evt.OrderId))

            .ExecuteAsync(cancellationToken);
    }

    public async Task HandleAsync(PaymentProcessedEvent @event, CancellationToken cancellationToken) {
        if (@event.IsSuccessful) {
            await _dispatcher.PublishEventAsync(new OrderConfirmedEvent(@event.OrderId));
        } else {
            await _dispatcher.PublishEventAsync(new OrderCancelledEvent(@event.OrderId, @event.Reason));
        }
    }

    public async Task HandleAsync(InventoryReservedEvent @event, CancellationToken cancellationToken) {
        // Atualizar status do pedido, disparar workflow de envio, etc.
    }
}
```

## 🎯 Benefícios Principais

### Para Desenvolvedores
- **Desenvolvimento Rápido**: Padrões pré-construídos reduzem tempo de desenvolvimento em 60-80%
- **Segurança de Tipos**: Verificação em tempo de compilação previne erros de runtime
- **Testabilidade**: Suporte integrado para testes unitários e mocking
- **Documentação**: Guias abrangentes e exemplos para todos os cenários

### Para Arquitetos
- **Arquitetura Limpa**: Promove separação de responsabilidades e código manutenível
- **Escalabilidade**: Padrões orientados a eventos suportam microserviços e sistemas distribuídos
- **Observabilidade**: Integração integrada com OpenTelemetry e logging
- **Resiliência**: Circuit breakers, políticas de retry e tratamento de erros

### Para DevOps
- **Pronto para Produção**: Padrões testados em batalha com tratamento abrangente de erros
- **Monitoramento**: Integração OpenTelemetry para rastreamento distribuído
- **Configuração**: Configuração baseada em ambiente com padrões sensatos
- **Amigável a Containers**: Otimizado para implantações Docker e Kubernetes

## 📖 Primeiros Passos

1. **Escolha Sua Arquitetura**: Comece com [Myth.Flow](Myth.Flow/README.pt-br.md) para orquestração de pipeline
2. **Adicione Validação**: Integre [Myth.Guard](Myth.Guard/README.pt-br.md) para validação abrangente
3. **Habilite CQRS**: Use [Myth.Flow.Actions](Myth.Flow.Actions/README.pt-br.md) para separação comando/consulta
4. **Adicione Acesso a Dados**: Implemente [Myth.Repository](Myth.Repository/README.pt-br.md) com [Myth.Specification](Myth.Specification/README.pt-br.md)
5. **Transforme Objetos**: Use [Myth.Morph](Myth.Morph/README.pt-br.md) para separação limpa de camadas
6. **Construa APIs**: Adicione [Myth.DependencyInjection.Providers](Myth.DependencyInjection.Providers/README.pt-br.md) para versionamento de API e documentação

## 🤝 Contribuindo

Contribuições são bem-vindas! Por favor, leia nossas diretrizes de contribuição e envie pull requests para quaisquer melhorias.

## 📄 Licença

Licenciado sob a Licença Apache, Versão 2.0. Veja [LICENSE](https://opensource.org/licenses/Apache-2.0) para detalhes.

---

**Construa software melhor mais rapidamente com o ecossistema Myth.** 🚀
