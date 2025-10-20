# Myth.Guard

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Guard?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Guard/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Guard?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Guard/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

Uma poderosa biblioteca .NET para construir sistemas de validação sustentáveis e type-safe com uma API fluente e declarativa. Construída com recursos enterprise-grade incluindo validação consciente de contexto, integração assíncrona com service provider, middleware automático para ASP.NET Core e tratamento abrangente de erros.

# ⭐ Recursos

- **Interface Fluente**: API declarativa e encadeável para código de validação legível
- **Type Safety**: Tipagem forte com cobertura abrangente de regras para todos os tipos .NET
- **Validação Consciente de Contexto**: Diferentes regras de validação por operação (Create, Update, Delete, etc.)
- **Integração Assíncrona com Serviços**: Acesso ao container de injeção de dependência nas regras de validação
- **Middleware ASP.NET Core**: Tratamento automático de exceções de validação com respostas estruturadas
- **400+ Regras de Validação**: Extensas regras integradas para strings, números, coleções, datas e mais
- **Regras Customizadas**: Extensibilidade fácil com métodos `Respect()` e `RespectAsync()`
- **Validação Condicional**: Execute regras baseadas em condições da entidade ou campo
- **Agregação de Erros**: Colete todos os erros de validação antes de falhar
- **Customização de Status HTTP**: Configure códigos de status HTTP por regra de validação

# 📦 Instalação

```bash
dotnet add package Myth.Guard
```

# 🚀 Início Rápido

## Uso Básico

```csharp
public class CreateUserDto : IValidatable<CreateUserDto>
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public List<string> Tags { get; set; }

    public void Validate(ValidationBuilder<CreateUserDto> builder, ValidationContextKey? context = null)
    {
        builder.For(Name, x => x.NotEmpty().MinimumLength(2).MaximumLength(100));
        builder.For(Email, x => x.NotEmpty().Email());
        builder.For(Age, x => x.GreaterThan(0).LessThan(150));
        builder.For(Tags, x => x.NotEmpty().CountBetween(1, 10));
    }
}

// Validar e lançar exceção em caso de falha
await validator.ValidateAsync(dto);

// Ou validar e retornar resultado
var result = await validator.ValidateAndReturnAsync(dto);
if (!result.IsValid)
{
    // Tratar erros de validação
}
```

## Configuração de Injeção de Dependência

### Program.cs (Minimal API)

```csharp
builder.Services.AddGuard();

// Registre seus serviços para validação assíncrona
builder.Services.AddScoped<IUserService, UserService>();
```

### Adicionar Middleware

```csharp
var app = builder.Build();

app.UseGuard(); // Adiciona tratamento automático de exceções de validação

app.MapControllers();
```

### Uso em Controllers/Serviços

```csharp
public class UserController : ControllerBase
{
    private readonly IValidator _validator;

    public UserController(IValidator validator)
    {
        _validator = validator;
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
    {
        // Validar e lançar ValidationException em caso de falha
        await _validator.ValidateAsync(request, ValidationContextKey.Create);

        // Ou validar e verificar resultado sem lançar exceção
        var result = await _validator.ValidateAndReturnAsync(request, ValidationContextKey.Create);
        if (!result.IsValid)
        {
            return BadRequest(new { errors = result.Errors });
        }

        // Processar criação do usuário...
        return Ok(new { message = "Usuário criado com sucesso" });
    }
}
```

# 🔧 Configuração

## Configuração Básica

```csharp
// Configuração simples - nenhuma configuração adicional necessária
builder.Services.AddGuard();
app.UseGuard();
```

## Formato de Resposta de Erro

O middleware formata automaticamente erros de validação em respostas JSON estruturadas:

```json
{
    "code": "MULTIPLE_ERRORS",
    "errors": [
        {
            "field": "email",
            "message": "Email é obrigatório",
            "code": "VIOLATION"
        },
        {
            "field": "age",
            "message": "Valor deve ser maior que 18",
            "code": "VIOLATION"
        }
    ]
}
```

# 📋 Regras de Validação

## Regras de String

```csharp
builder.For(Email, x => x
    .NotEmpty()
    .Email()
    .MaximumLength(254));

builder.For(Name, x => x
    .NotEmpty()
    .MinimumLength(2)
    .MaximumLength(100)
    .OnlyLetters());

builder.For(Password, x => x
    .NotEmpty()
    .MinimumLength(8)
    .Matches(new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$"))
    .WithMessage("Senha deve conter maiúscula, minúscula e dígito"));

builder.For(PhoneNumber, x => x
    .NotEmpty()
    .Matches(new Regex(@"^\+\d{1,3}\d{10,14}$"))
    .WithMessage("Formato de telefone inválido"));
```

**Regras de String Disponíveis:**
- `NotEmpty()`, `MinimumLength(int)`, `MaximumLength(int)`, `LengthBetween(int, int)`
- `Email()`, `Url()` - Validação de formato
- `OnlyLetters()`, `OnlyNumbers()`, `Alphanumeric()` - Validação de tipo de caractere
- `StartsWith(string)`, `EndsWith(string)`, `Contains(string)` - Verificações de substring
- `Matches(Regex)` - Correspondência de padrão regex
- `EqualsTo(string)`, `BeOneOf(params string[])` - Verificações de enumeração

## Regras Numéricas

```csharp
builder.For(Age, x => x
    .GreaterThan(0)
    .LessThan(150));

builder.For(Salary, x => x
    .GreaterOrEquals(0)
    .LessThan(1000000m));

builder.For(Score, x => x
    .Between(0, 100)
    .When(score => score.HasValue));

builder.For(Quantity, x => x
    .Positive()
    .NotZero());
```

**Regras Numéricas Disponíveis (int, long, decimal, double, etc.):**
- `GreaterThan(T)`, `GreaterOrEquals(T)`, `LessThan(T)`, `LessOrEquals(T)`
- `Between(T min, T max)` - Validação de intervalo
- `Positive()`, `Negative()`, `Zero()`, `NotZero()`

## Regras de Coleção

```csharp
builder.For(Tags, x => x
    .NotEmpty()
    .CountBetween(1, 10)
    .All(tag => !string.IsNullOrWhiteSpace(tag))
    .Distinct());

builder.For(UserRoles, x => x
    .NotEmpty()
    .Any(role => role == "Admin" || role == "User")
    .None(role => role == "Banned"));
```

**Regras de Coleção Disponíveis:**
- `NotEmpty()` - Coleção não vazia
- `CountBetween(int min, int max)`, `CountGreaterThan(int)`, `CountLessThan(int)`
- `All<T>(Func<T, bool> predicate)` - Todos os elementos correspondem à condição
- `Any<T>(Func<T, bool> predicate)` - Pelo menos um corresponde
- `None<T>(Func<T, bool> predicate)` - Nenhum elemento corresponde
- `Distinct<T>()`, `DistinctBy<T, TKey>(Func<T, TKey> keySelector)` - Detecção de duplicatas

## Regras de DateTime

```csharp
builder.For(BirthDate, x => x
    .Past()
    .After(new DateTime(1900, 1, 1)));

builder.For(ScheduledDate, x => x
    .Future()
    .Before(DateTime.Now.AddYears(1)));

builder.For(AppointmentDate, x => x
    .Between(DateTime.Today, DateTime.Today.AddDays(30)));
```

**Regras de DateTime Disponíveis:**
- `Past()`, `Future()`, `Today()`
- `After(DateTime)`, `Before(DateTime)`, `Between(DateTime, DateTime)`
- `AfterOrEquals(DateTime)`, `BeforeOrEquals(DateTime)`

## Regras de Boolean e Enum

```csharp
builder.For(IsActive, x => x.IsTrue());
builder.For(IsDeleted, x => x.IsFalse());

builder.For(Role, x => x.BeInEnum<UserRole>());
builder.For(Status, x => x.BeOneOf(Status.Active, Status.Pending));
```

## Regras Genéricas (Todos os Tipos)

```csharp
builder.For(UserId, x => x
    .NotNull()
    .NotDefault());

builder.For(Email, x => x
    .NotNull()
    .NotEqualsTo("admin@example.com"));
```

**Regras Genéricas Disponíveis:**
- `NotNull()`, `BeNull()`
- `EqualsTo(T)`, `NotEqualsTo(T)`
- `BeDefault()`, `NotDefault()`
- `Respect(Func<T, bool> predicate)` - Validação customizada síncrona
- `RespectAsync(Func<T, CancellationToken, IServiceProvider, Task<bool>> predicate)` - Validação customizada assíncrona

# 🎯 Validação Consciente de Contexto

Um dos recursos mais poderosos são as regras de validação específicas por contexto:

```csharp
public class UserDto : IValidatable<UserDto>
{
    public string Email { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public string Password { get; set; }

    public void Validate(ValidationBuilder<UserDto> builder, ValidationContextKey? context = null)
    {
        // Regras globais (aplicam a todos os contextos)
        builder.For(Email, x => x.NotEmpty().Email());
        builder.For(Age, x => x.GreaterThan(0).LessThan(150));

        // Regras específicas para criação
        builder.InContext(ValidationContextKey.Create, b =>
        {
            b.For(Email, x => x
                .RespectAsync(async (email, ct, sp) =>
                {
                    var userService = sp.GetRequiredService<IUserService>();
                    return await userService.IsEmailAvailableAsync(email, ct);
                })
                .WithMessage("Email já existe")
                .WithCode("EMAIL_EXISTS")
                .WithStatusCode(HttpStatusCode.Conflict));

            b.For(Password, x => x
                .NotEmpty()
                .MinimumLength(8)
                .Matches(new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$")));

            b.For(IsActive, x => x.IsTrue());
        });

        // Regras específicas para atualização
        builder.InContext(ValidationContextKey.Update, b =>
        {
            b.For(Age, x => x.GreaterOrEquals(18));
            // Senha é opcional na atualização
        });

        // Regras específicas para exclusão
        builder.InContext(ValidationContextKey.Delete, b =>
        {
            b.For(IsActive, x => x.IsFalse()
                .WithMessage("Não é possível excluir usuário ativo"));
        });
    }
}

// Uso com diferentes contextos
await validator.ValidateAsync(user, ValidationContextKey.Create);
await validator.ValidateAsync(user, ValidationContextKey.Update);
await validator.ValidateAsync(user, ValidationContextKey.Delete);
```

## Contextos Pré-definidos

```csharp
ValidationContextKey.Default     // Contexto padrão
ValidationContextKey.Create      // Para operações de criação
ValidationContextKey.Update      // Para operações de atualização
ValidationContextKey.Delete      // Para operações de exclusão
ValidationContextKey.GetByField  // Para consultas baseadas em campo
ValidationContextKey.GetAll      // Para operações de listagem
ValidationContextKey.Search      // Para operações de busca
ValidationContextKey.Activate    // Para operações de ativação
ValidationContextKey.Deactivate  // Para operações de desativação

// Contextos customizados
ValidationContextKey.Custom("MeuContextoCustomizado")
```

# 🔄 Validação Condicional

Execute regras de validação baseadas em condições:

## Condições no Nível do Campo

```csharp
builder.For(PhoneNumber, x => x
    .NotEmpty()
    .When(phone => !string.IsNullOrEmpty(phone)) // Apenas validar se não estiver vazio
    .Matches(new Regex(@"^\+\d{1,3}\d{10,14}$")));

builder.For(Password, x => x
    .NotEmpty()
    .Unless(pwd => IsExternalUser) // Pular para usuários externos
    .MinimumLength(8));
```

## Condições no Nível da Entidade

```csharp
builder.For(PhoneNumber, x => x
    .NotEmpty()
    .When<string, UserDto>(user => user.PhoneType == PhoneType.Required)
    .Unless<string, UserDto>(user => user.IsVerified));

builder.For(Salary, x => x
    .GreaterThan(0)
    .When<decimal, EmployeeDto>(emp => emp.EmploymentType == EmploymentType.FullTime));
```

# 🔁 Validação Assíncrona com Service Provider

Acesse o container de injeção de dependência para validação de banco de dados ou API:

```csharp
public class CreateOrderDto : IValidatable<CreateOrderDto>
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string CustomerEmail { get; set; }

    public void Validate(ValidationBuilder<CreateOrderDto> builder, ValidationContextKey? context = null)
    {
        builder.For(ProductId, x => x
            .GreaterThan(0)
            .RespectAsync(async (productId, ct, sp) =>
            {
                var productService = sp.GetRequiredService<IProductService>();
                return await productService.ExistsAsync(productId, ct);
            })
            .WithMessage("Produto não existe")
            .WithCode("PRODUCT_NOT_FOUND"));

        builder.For(Quantity, x => x
            .GreaterThan(0)
            .RespectAsync(async (quantity, ct, sp) =>
            {
                var productService = sp.GetRequiredService<IProductService>();
                var stock = await productService.GetStockAsync(ProductId, ct);
                return quantity <= stock;
            })
            .WithMessage("Estoque insuficiente")
            .WithCode("INSUFFICIENT_STOCK"));

        builder.For(CustomerEmail, x => x
            .NotEmpty()
            .Email()
            .RespectAsync(async (email, ct, sp) =>
            {
                var customerService = sp.GetRequiredService<ICustomerService>();
                return await customerService.IsActiveCustomerAsync(email, ct);
            })
            .WithMessage("Cliente não encontrado ou inativo")
            .WithCode("CUSTOMER_INACTIVE")
            .WithStatusCode(HttpStatusCode.NotFound));
    }
}
```

# 🎨 Recursos Avançados

## Mensagens de Erro Customizadas

```csharp
// Mensagem estática
builder.For(Age, x => x
    .GreaterThan(18)
    .WithMessage("Usuário deve ter pelo menos 18 anos"));

// Mensagem dinâmica usando valor do campo
builder.For(Age, x => x
    .GreaterThan(18)
    .WithMessage(age => $"Usuário deve ter pelo menos 18 anos, mas tem {age}"));

// Código de erro customizado
builder.For(Email, x => x
    .Email()
    .WithCode("INVALID_EMAIL_FORMAT"));

// Código de status HTTP customizado
builder.For(UserId, x => x
    .RespectAsync(async (id, ct, sp) =>
    {
        var userService = sp.GetRequiredService<IUserService>();
        return await userService.ExistsAsync(id, ct);
    })
    .WithMessage("Usuário não encontrado")
    .WithCode("USER_NOT_FOUND")
    .WithStatusCode(HttpStatusCode.NotFound)); // Retorna 404 em vez de 400
```

## Parar em Falha

Parar de validar um campo após a primeira falha:

```csharp
builder.For(Password, x => x
    .NotEmpty()
    .SetStopOnFailure() // Não verificar outras regras se estiver vazio
    .MinimumLength(8)
    .Matches(new Regex(@"[A-Z]"))
    .Matches(new Regex(@"[a-z]"))
    .Matches(new Regex(@"\d")));
```

## Regras de Negócio Complexas

```csharp
public class OrderDto : IValidatable<OrderDto>
{
    public decimal Amount { get; set; }
    public string CustomerType { get; set; }
    public List<OrderItem> Items { get; set; }
    public string CouponCode { get; set; }

    public void Validate(ValidationBuilder<OrderDto> builder, ValidationContextKey? context = null)
    {
        builder.For(Amount, x => x
            .GreaterThan(0)
            .When<decimal, OrderDto>(order => order.Items?.Any() == true));

        // Clientes premium podem ter valores de pedido mais altos
        builder.For(Amount, x => x
            .LessThan(10000)
            .Unless<decimal, OrderDto>(order => order.CustomerType == "Premium"));

        builder.For(Items, x => x
            .NotEmpty()
            .CountBetween(1, 50)
            .All(item => item.Quantity > 0)
            .All(item => item.Price > 0));

        // Validação de cupom
        builder.For(CouponCode, x => x
            .RespectAsync(async (coupon, ct, sp) =>
            {
                if (string.IsNullOrEmpty(coupon)) return true; // Opcional

                var couponService = sp.GetRequiredService<ICouponService>();
                var isValid = await couponService.IsValidAsync(coupon, ct);
                var isApplicable = await couponService.IsApplicableToOrderAsync(coupon, Amount, ct);
                return isValid && isApplicable;
            })
            .WithMessage("Código de cupom inválido ou inaplicável")
            .WithCode("INVALID_COUPON"));
    }
}
```

# ❌ Tratamento de Erros

## Resultado de Validação

```csharp
var result = await validator.ValidateAndReturnAsync(dto);

Console.WriteLine($"É Válido: {result.IsValid}");
Console.WriteLine($"Código de Status: {result.StatusCode}");

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Campo: {error.Field}");
        Console.WriteLine($"Mensagem: {error.Message}");
        Console.WriteLine($"Código: {error.Code}");
        Console.WriteLine($"Status: {error.StatusCode}");
    }
}
```

## Tratamento de Exceções

```csharp
try
{
    await validator.ValidateAsync(dto, ValidationContextKey.Create);
}
catch (ValidationException ex)
{
    var errors = ex.ValidationResult.Errors;
    var statusCode = ex.ValidationResult.StatusCode;

    // Registrar erros ou transformar em resposta customizada
    return BadRequest(new
    {
        message = "Validação falhou",
        errors = errors.Select(e => new
        {
            field = e.Field,
            message = e.Message,
            code = e.Code
        })
    });
}
```

## Resposta de Erro do Middleware

Ao usar `app.UseGuard()`, exceções de validação são automaticamente capturadas e formatadas:

```json
{
    "code": "MULTIPLE_ERRORS",
    "errors": [
        {
            "field": "email",
            "message": "Email já existe",
            "code": "EMAIL_EXISTS"
        },
        {
            "field": "password",
            "message": "Senha deve conter pelo menos 8 caracteres",
            "code": "VIOLATION"
        }
    ]
}
```

Código de Status HTTP: O código de status mais alto de todos os erros de validação (ex: se um erro tem `409 Conflict`, a resposta será `409`).

# 🧪 Testes

O design de validação torna os testes diretos:

```csharp
[Fact]
public async Task CreateUser_WithInvalidEmail_ShouldFail()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddScoped<IUserService>(sp => mockUserService.Object);
    services.AddGuard();

    var serviceProvider = services.BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator>();

    var dto = new CreateUserDto
    {
        Name = "João Silva",
        Email = "email-inválido",
        Age = 25
    };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ValidationException>(
        () => validator.ValidateAsync(dto, ValidationContextKey.Create));

    exception.ValidationResult.Errors.Should().HaveCount(1);
    exception.ValidationResult.Errors.First().Field.Should().Be("Email");
    exception.ValidationResult.Errors.First().Code.Should().Be("VIOLATION");
}

[Fact]
public async Task CreateUser_WithExistingEmail_ShouldReturnConflict()
{
    // Arrange
    mockUserService.Setup(x => x.IsEmailAvailableAsync("existente@teste.com", It.IsAny<CancellationToken>()))
               .ReturnsAsync(false);

    var dto = new CreateUserDto
    {
        Name = "João Silva",
        Email = "existente@teste.com",
        Age = 25
    };

    // Act
    var result = await validator.ValidateAndReturnAsync(dto, ValidationContextKey.Create);

    // Assert
    result.IsValid.Should().BeFalse();
    result.StatusCode.Should().Be(HttpStatusCode.Conflict);
    result.Errors.Should().ContainSingle(e => e.Code == "EMAIL_EXISTS");
}
```

## Testando Regras Customizadas

```csharp
[Fact]
public async Task ValidateOrderAmount_WithPremiumCustomer_ShouldAllowHigherAmount()
{
    // Arrange
    var order = new OrderDto
    {
        Amount = 15000, // Acima do limite normal
        CustomerType = "Premium",
        Items = new List<OrderItem> { new() { Quantity = 1, Price = 15000 } }
    };

    // Act
    var result = await validator.ValidateAndReturnAsync(order);

    // Assert
    result.IsValid.Should().BeTrue();
}
```

# 📋 Melhores Práticas

1. **Use Validação Consciente de Contexto**: Aproveite `ValidationContextKey` para regras específicas de operação
2. **Configure Injeção de Dependência**: Sempre use DI para acesso a serviços em validação assíncrona
3. **Adicione Middleware**: Use `app.UseGuard()` para tratamento automático de exceções
4. **Separe Responsabilidades**: Mantenha regras de validação focadas e livres de lógica de negócio
5. **Use Regras Assíncronas com Parcimônia**: Apenas para verificações de banco de dados/API que requerem serviços externos
6. **Trate Erros Graciosamente**: Sempre verifique `IsValid` antes de acessar dados validados
7. **Use Códigos de Status Customizados**: Defina códigos de status HTTP apropriados para diferentes falhas de validação
8. **Teste Lógica de Validação**: Teste cenários de validação positivos e negativos
9. **Pare em Falhas Críticas**: Use `SetStopOnFailure()` para regras que impedem validação adicional
10. **Use Mensagens de Erro Significativas**: Forneça mensagens de erro claras e acionáveis para usuários

# 🏗️ Padrões Avançados

## Pipeline de Validação Multi-Etapas

```csharp
public class ComplexOrderDto : IValidatable<ComplexOrderDto>
{
    public CustomerInfo Customer { get; set; }
    public List<OrderItem> Items { get; set; }
    public PaymentInfo Payment { get; set; }
    public ShippingInfo Shipping { get; set; }

    public void Validate(ValidationBuilder<ComplexOrderDto> builder, ValidationContextKey? context = null)
    {
        // Validação do cliente
        builder.For(Customer, x => x.NotNull());
        builder.For(Customer?.Email, x => x
            .NotEmpty()
            .Email()
            .RespectAsync(async (email, ct, sp) =>
            {
                var customerService = sp.GetRequiredService<ICustomerService>();
                return await customerService.IsActiveAsync(email, ct);
            })
            .WithMessage("Cliente não encontrado ou inativo"));

        // Validação de itens
        builder.For(Items, x => x
            .NotEmpty()
            .CountBetween(1, 100)
            .All(item => item.Quantity > 0)
            .All(item => item.Price > 0));

        // Validação de pagamento
        builder.For(Payment?.CardNumber, x => x
            .NotEmpty()
            .Matches(new Regex(@"^\d{13,19}$"))
            .RespectAsync(async (cardNumber, ct, sp) =>
            {
                var paymentService = sp.GetRequiredService<IPaymentService>();
                return await paymentService.ValidateCardAsync(cardNumber, ct);
            })
            .WithMessage("Cartão de pagamento inválido"));

        // Validação de entrega
        builder.For(Shipping?.Address, x => x
            .NotEmpty()
            .MinimumLength(10)
            .RespectAsync(async (address, ct, sp) =>
            {
                var shippingService = sp.GetRequiredService<IShippingService>();
                return await shippingService.CanDeliverToAsync(address, ct);
            })
            .WithMessage("Entrega não disponível para este endereço"));

        // Validação cruzada de campos
        builder.InContext(ValidationContextKey.Create, b =>
        {
            // Valor total deve corresponder aos itens
            b.For(Payment?.Amount, x => x
                .EqualsTo(Items?.Sum(i => i.Price * i.Quantity))
                .WithMessage("Valor do pagamento não corresponde ao total do pedido"));

            // Entrega expressa requer cliente premium
            b.For(Shipping?.Type, x => x
                .Respect(type => type != "Express" || Customer?.Type == "Premium")
                .WithMessage("Entrega expressa disponível apenas para clientes premium"));
        });
    }
}
```

## Integração com Padrão Repository

```csharp
public class UserRepository
{
    private readonly IValidator _validator;
    private readonly IDbContext _context;

    public UserRepository(IValidator validator, IDbContext context)
    {
        _validator = validator;
        _context = context;
    }

    public async Task<User> CreateAsync(CreateUserDto dto)
    {
        // Validar antes da criação
        await _validator.ValidateAsync(dto, ValidationContextKey.Create);

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Age = dto.Age
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User> UpdateAsync(int id, UpdateUserDto dto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            throw new NotFoundException("Usuário não encontrado");

        // Validar antes da atualização
        await _validator.ValidateAsync(dto, ValidationContextKey.Update);

        user.Name = dto.Name;
        user.Age = dto.Age;
        // Email normalmente não é atualizado

        await _context.SaveChangesAsync();
        return user;
    }
}
```

## Validação de Comando CQRS

```csharp
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
{
    private readonly IValidator _validator;
    private readonly IOrderRepository _repository;

    public CreateOrderCommandHandler(IValidator validator, IOrderRepository repository)
    {
        _validator = validator;
        _repository = repository;
    }

    public async Task<CommandResult> HandleAsync(CreateOrderCommand command)
    {
        try
        {
            // Validar comando
            await _validator.ValidateAsync(command.OrderData, ValidationContextKey.Create);

            // Processar pedido
            var order = await _repository.CreateAsync(command.OrderData);

            return CommandResult.Success();
        }
        catch (ValidationException ex)
        {
            return CommandResult.Failure(ex.ValidationResult.Errors);
        }
    }
}
```

# 📊 Considerações de Performance

1. **Regras Assíncronas**: Use `RespectAsync()` apenas quando necessário (chamadas de banco de dados/API)
2. **Parar em Falha**: Use `SetStopOnFailure()` para regras de validação caras
3. **Filtragem de Contexto**: Use contextos específicos para evitar execução desnecessária de regras
4. **Cache de Serviços**: Faça cache de chamadas de serviço caras em regras de validação assíncronas
5. **Overhead de Reflection**: A biblioteca usa reflection para extrair valores de campo - impacto mínimo de performance para casos de uso típicos

# 📄 Licença

Este projeto é licenciado sob a Licença Apache 2.0 - veja o arquivo LICENSE para detalhes.

# 🤝 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para enviar um Pull Request.

# 📧 Suporte

Para problemas, questões ou contribuições, visite o [repositório GitHub](https://github.com/paulaolileal/myth).