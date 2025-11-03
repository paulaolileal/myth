<img  style="float: right;" src="Logo.jpg" alt="drawing" width="250"/>

# Myth Ecosystem

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

A comprehensive collection of production-ready .NET libraries designed for building robust, scalable enterprise applications. The Myth ecosystem promotes clean architecture, Domain-Driven Design (DDD), and modern software engineering practices with minimal boilerplate code.

## 🚀 Why Choose Myth?

- **🏗️ Clean Architecture**: Built with DDD principles and clean architecture patterns
- **⚡ Developer Experience**: Fluent APIs, minimal configuration, and extensive documentation
- **🔄 Seamless Integration**: Libraries work together through a global service provider
- **📦 Production Ready**: Battle-tested patterns with comprehensive error handling
- **🎯 Type Safety**: Compile-time safety with modern C# features
- **🧪 Testable**: Designed for easy unit testing and mocking

## 📚 Quick Start

### ASP.NET Core Application

```csharp
using Myth.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add comprehensive validation
builder.Services.AddGuard();

// Add pipeline orchestration with CQRS
builder.Services.AddFlow(config => config
    .UseTelemetry()
    .UseRetry(maxAttempts: 3, backoffMs: 1000)
    .UseActions(actions => actions
        .UseInMemory()        // or .UseKafka() / .UseRabbitMQ()
        .UseCaching()
        .ScanAssemblies(typeof(Program).Assembly)));

// Add object transformation
builder.Services.AddMorph();

// Add API versioning and Swagger
builder.Services.AddVersioning(1.0);
builder.Services.AddSwaggerVersioned(settings => {
    settings.Title = "My Enterprise API";
    settings.Description = "Production-ready API with comprehensive features";
});

// Auto-register repositories and services
builder.Services.AddServiceFromType<IRepository>();
builder.Services.AddServiceFromType<IDomainService>();

// Build with global service provider (enables cross-library integration)
var app = builder.BuildApp();

// Add validation middleware
app.UseGuard();
app.UseSwaggerVersioned();
app.MapControllers();

app.Run();
```

### Complete CQRS Example

```csharp
// Domain Entity with Validation
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
                .WithMessage("Email already exists"));
        });
    }
}

// Command Handler with Pipeline
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

            // Validate command
            .StepResultAsync(cmd => _validator.ValidateAndReturnAsync(cmd, ValidationContextKey.Create))

            // Transform to entity
            .Transform<UserEntity>(cmd => cmd.To<UserEntity>())

            // Save to repository
            .StepAsync(entity => _repository.AddAsync(entity))

            // Publish domain event
            .TapAsync(entity => _dispatcher.PublishEventAsync(new UserCreatedEvent { UserId = entity.Id }))

            // Transform to DTO
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
    /// Creates a new user
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

## 🏛️ Architecture Patterns

The Myth ecosystem enables several enterprise architecture patterns:

### Clean Architecture & DDD
```csharp
// Domain Layer
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

// Application Layer
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

### Event-Driven Architecture
```csharp
// Event Handlers for Cross-Cutting Concerns
public class OrderCreatedEventHandler : IEventHandler<OrderCreatedEvent> {
    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken) {
        // Send email notification
        // Update analytics
        // Trigger inventory check
    }
}

// Event Bus Configuration
builder.Services.AddFlow(config => config.UseActions(actions => actions
    .UseKafka(kafka => kafka
        .WithBootstrapServers("localhost:9092")
        .WithGroupId("order-processing"))
    .UseDeadLetterQueue()
    .UseCircuitBreaker()));
```

### Repository Pattern with Specifications
```csharp
// Business Rules as Specifications
public static class OrderSpecifications {
    public static ISpec<Order> ForCustomer(this ISpec<Order> spec, CustomerId customerId) =>
        spec.And(o => o.CustomerId == customerId);

    public static ISpec<Order> WithStatus(this ISpec<Order> spec, OrderStatus status) =>
        spec.And(o => o.Status == status);

    public static ISpec<Order> Recent(this ISpec<Order> spec, TimeSpan timespan) =>
        spec.And(o => o.CreatedAt >= DateTime.UtcNow.Subtract(timespan));
}

// Usage in Repository
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

# 📦 Libraries

## 🔮 Core Foundation
- **[Myth.Commons](Myth.Commons/README.md)** - Essential utilities, value objects, and global service provider management
- **[Myth.DependencyInjection](Myth.DependencyInjection/README.md)** - Auto-discovery and convention-based service registration
- **[Myth.DependencyInjection.Providers](Myth.DependencyInjection.Providers/README.md)** - Pre-configured integrations for Swagger, AutoMapper, and API versioning

## 🔄 Data Flow & Orchestration
- **[Myth.Flow](Myth.Flow/README.md)** - Pipeline orchestration with Result pattern and OpenTelemetry integration
- **[Myth.Flow.Actions](Myth.Flow.Actions/README.md)** - CQRS, event-driven architecture with message brokers (Kafka, RabbitMQ)

## 🛡️ Validation & Safety
- **[Myth.Guard](Myth.Guard/README.md)** - Fluent validation with 100+ rules, context-aware validation, and ASP.NET Core middleware

## 🔄 Object Transformation
- **[Myth.Morph](Myth.Morph/README.md)** - Schema-based object transformation with self-defining mappable types

## 🗄️ Data Access
- **[Myth.Specification](Myth.Specification/README.md)** - Query specification pattern for encapsulating business rules
- **[Myth.Repository](Myth.Repository/README.md)** - Generic repository pattern interfaces with async support
- **[Myth.Repository.EntityFramework](Myth.Repository.EntityFramework/README.md)** - Entity Framework Core implementations with Unit of Work pattern

## 🌐 HTTP & APIs
- **[Myth.Rest](Myth.Rest/README.md)** - Fluent REST client with circuit breaker, retry policies, and certificate support

## 🧪 Testing
- **[Myth.Testing](Myth.Testing/README.md)** - Testing utilities, mocks, and base test classes for comprehensive testing

## 🏗️ Integration Examples

### E-Commerce Microservice
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
            // Validate command with business rules
            .StepResultAsync(cmd => _validator.ValidateAndReturnAsync(cmd, ValidationContextKey.Update))

            // Load existing product using specification
            .StepAsync(cmd => _productRepository.FirstOrDefaultAsync(ProductSpecifications.ById(cmd.ProductId)))

            // Apply business logic
            .Step((product, cmd) => {
                product.UpdateDetails(cmd.Name, cmd.Description, cmd.Price);
                return product;
            })

            // Save changes
            .StepAsync(product => _productRepository.UpdateAsync(product))

            // Publish integration event
            .TapAsync(product => _dispatcher.PublishEventAsync(new ProductUpdatedEvent {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price
            }))

            // Transform to DTO
            .Transform<ProductDto>(product => product.To<ProductDto>())

            .ExecuteAsync();
    }
}
```

### Event-Driven Order Processing
```csharp
// Order Event Handlers
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
            // Reserve inventory
            .TapAsync(evt => _inventoryService.ReserveItemsAsync(evt.OrderId, evt.Items))

            // Process payment
            .TapAsync(evt => _paymentService.ProcessPaymentAsync(evt.OrderId, evt.TotalAmount))

            // Send confirmation email
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
        // Update order status, trigger shipping workflow, etc.
    }
}
```

## 🎯 Key Benefits

### For Developers
- **Rapid Development**: Pre-built patterns reduce development time by 60-80%
- **Type Safety**: Compile-time checking prevents runtime errors
- **Testability**: Built-in support for unit testing and mocking
- **Documentation**: Comprehensive guides and examples for every scenario

### For Architects
- **Clean Architecture**: Promotes separation of concerns and maintainable code
- **Scalability**: Event-driven patterns support microservices and distributed systems
- **Observability**: Built-in OpenTelemetry and logging integration
- **Resilience**: Circuit breakers, retry policies, and error handling

### For DevOps
- **Production Ready**: Battle-tested patterns with comprehensive error handling
- **Monitoring**: OpenTelemetry integration for distributed tracing
- **Configuration**: Environment-based configuration with sensible defaults
- **Container Friendly**: Optimized for Docker and Kubernetes deployments

## 📖 Getting Started

1. **Choose Your Architecture**: Start with [Myth.Flow](Myth.Flow/README.md) for pipeline orchestration
2. **Add Validation**: Integrate [Myth.Guard](Myth.Guard/README.md) for comprehensive validation
3. **Enable CQRS**: Use [Myth.Flow.Actions](Myth.Flow.Actions/README.md) for command/query separation
4. **Add Data Access**: Implement [Myth.Repository](Myth.Repository/README.md) with [Myth.Specification](Myth.Specification/README.md)
5. **Transform Objects**: Use [Myth.Morph](Myth.Morph/README.md) for clean layer separation
6. **Build APIs**: Add [Myth.DependencyInjection.Providers](Myth.DependencyInjection.Providers/README.md) for API versioning and documentation

## 🤝 Contributing

We welcome contributions! Please read our contribution guidelines and submit pull requests for any improvements.

## 📄 License

Licensed under the Apache License, Version 2.0. See [LICENSE](https://opensource.org/licenses/Apache-2.0) for details.

---

**Build better software faster with the Myth ecosystem.** 🚀