# Myth.Testing

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Testing?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Testing/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Testing?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Testing/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](README.md)

Uma biblioteca abrangente de testes construída sobre xUnit que fornece classes base, utilitários e padrões para simplificar e aprimorar sua experiência de teste em aplicações .NET.

## Índice

- [Funcionalidades](#funcionalidades)
- [Instalação](#instalação)
- [Início Rápido](#início-rápido)
- [Componentes Principais](#componentes-principais)
- [Gerenciamento de Serviços](#gerenciamento-de-serviços)
- [Gerenciamento de Configuração](#gerenciamento-de-configuração)
- [Mock de Cliente HTTP](#mock-de-cliente-http)
- [Extensões FluentAssertions](#extensões-fluentassertions)
- [Melhores Práticas](#melhores-práticas)
- [Migração do NUnit](#migração-do-nunit)
- [Exemplos](#exemplos)

## Funcionalidades

- 🧪 **Classes Base de Teste**: Classes base pré-configuradas para testes unitários e de banco de dados
- 🔄 **Padrões Assíncronos**: Suporte integrado para testes assíncronos com gerenciamento de timeout
- 🗄️ **Testes de Banco de Dados**: Integração Entity Framework com bancos de dados em memória
- 🏗️ **Geração de Dados de Teste**: Bogus (Faker) integrado para criação de dados de teste realistas
- 🔧 **Container de Serviços**: Suporte à injeção de dependência para testes
- 📊 **FluentAssertions**: Extensões de asserção aprimoradas para melhor legibilidade dos testes
- 🎯 **Integração xUnit**: Framework de teste moderno com suporte a fixtures

## Instalação

```bash
dotnet add package Myth.Testing
```

## Início Rápido

### Testes Unitários Básicos

```csharp
public class UserServiceTests : BaseTests
{
    private readonly UserService _userService;

    public UserServiceTests()
    {
        // Registrar serviços
        AddService<UserService>(new UserService());
        _userService = GetRequiredService<UserService>();
    }

    [Fact]
    public async Task CreateUser_WithValidData_ShouldSucceed()
    {
        // Arrange - Criar dados de teste usando Bogus faker
        var user = new User
        {
            Id = _faker.Random.Guid(),
            Name = "João Silva",
            Email = "joao@exemplo.com",
            Age = _faker.Random.Int(18, 65),
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        // Act
        var result = await _userService.CreateUserAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
    }
}
```

### Testes de Banco de Dados

```csharp
public class UserRepositoryTests : BaseDatabaseTests<UserDbContext>
{
    private readonly UserRepository _repository;

    public UserRepositoryTests()
    {
        AddService<UserRepository, UserRepository>();
        _repository = GetRequiredService<UserRepository>();
    }

    [Fact]
    public async Task CreateUser_ShouldPersistToDatabase()
    {
        // Arrange
        await InitializeDatabaseAsync();

        var user = new UserEntity
        {
            Name = _faker.Name.FullName(),
            Email = _faker.Internet.Email(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(user);

        // Assert
        result.Should().NotBeNull();

        var dbUser = await GetContext().Users.FindAsync(result.Id);
        dbUser.Should().NotBeNull();

        await CleanupDatabaseAsync();
    }
}
```

## Componentes Principais

### BaseTests

A classe fundamental para testes unitários fornecendo:

- **Container de Serviços**: Configuração de injeção de dependência
- **Configuração**: Gerenciamento de configuração em memória
- **Integração Faker**: Faker Bogus pré-configurado para dados de teste
- **Gerenciamento de Serviços**: Registro e recuperação fácil de serviços

```csharp
public class MyTests : BaseTests
{
    public MyTests()
    {
        // Serviços são configurados automaticamente
        // Faker está disponível como _faker
        // Configuração é definida com valores de teste
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddTransient<IMyService, MyService>();
    }
}
```

### BaseDatabaseTests<TContext>

Estende BaseTests com suporte Entity Framework:

- **Banco de Dados em Memória**: Banco de dados isolado por classe de teste
- **Métodos Assíncronos**: Inicialização e limpeza do banco de dados
- **Acesso ao Contexto**: Acesso direto ao DbContext
- **Suporte a Transações**: Gerenciamento automático de transações

```csharp
public class DatabaseTests : BaseDatabaseTests<MyDbContext>
{
    [Fact]
    public async Task Test_WithDatabase()
    {
        await InitializeDatabaseAsync();

        // Sua lógica de teste aqui
        var context = GetContext();

        await CleanupDatabaseAsync();
    }
}
```

### Geração de Dados de Teste

O Myth.Testing inclui uma instância **Bogus (Faker)** pré-configurada disponível através do campo `_faker` em todas as classes de teste. Use-a diretamente para gerar dados de teste realistas:

**Categorias do Faker Disponíveis:**
- `_faker.Name` - Nomes e informações pessoais
- `_faker.Internet` - Endereços de email, URLs, nomes de usuário
- `_faker.Address` - Endereços, cidades, códigos postais
- `_faker.Phone` - Números de telefone
- `_faker.Commerce` - Nomes de produtos, preços
- `_faker.Date` - Datas e horários
- `_faker.Lorem` - Texto Lorem ipsum
- `_faker.Random` - Valores aleatórios (números, booleanos, enums)

**Exemplos de Uso:**
```csharp
public class UserServiceTests : BaseTests
{
    [Fact]
    public void CreateTestData_Examples()
    {
        // Criação simples de objeto
        var user = new User
        {
            Id = _faker.Random.Guid(),
            Name = _faker.Name.FullName(),
            Email = _faker.Internet.Email(),
            Age = _faker.Random.Int(18, 65),
            IsActive = _faker.Random.Bool(),
            CreatedDate = _faker.Date.Recent()
        };

        // Coleções
        var users = new List<User>();
        for (int i = 0; i < 10; i++)
        {
            users.Add(new User
            {
                Id = _faker.Random.Guid(),
                Name = _faker.Name.FullName(),
                Email = _faker.Internet.Email(),
                Age = _faker.Random.Int(18, 65),
                IsActive = i % 2 == 0, // Mistura de ativo/inativo
                CreatedDate = _faker.Date.Past()
            });
        }
    }
}

// Padrão de Métodos Helper
public static class TestDataHelper
{
    public static User CreateValidUser(Faker faker) => new()
    {
        Id = faker.Random.Guid(),
        Name = faker.Name.FullName(),
        Email = faker.Internet.Email(),
        Age = faker.Random.Int(18, 65),
        IsActive = true,
        CreatedDate = DateTime.UtcNow
    };

    public static List<User> CreateUserList(Faker faker, int count)
    {
        var users = new List<User>();
        for (int i = 0; i < count; i++)
        {
            users.Add(CreateValidUser(faker));
        }
        return users;
    }
}
```

### TestFixture

Fixtures compartilhados para recursos caros:

```csharp
public class DatabaseFixture : TestFixture
{
    protected override void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<MyDbContext>(options =>
            options.UseInMemoryDatabase("SharedTestDb"));
    }
}

[Collection("Database Collection")]
public class MyTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public MyTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }
}
```

### Extensões de Teste Assíncrono

Padrões aprimorados de teste assíncrono:

```csharp
// Teste de timeout
await TestExtensions.WithTimeoutAsync(
    () => service.LongRunningOperationAsync(),
    TimeSpan.FromSeconds(5)
);

// Teste de exceção
await TestExtensions.AssertThrowsAsync<InvalidOperationException>(
    () => service.InvalidOperationAsync()
);

// Teste de ausência de exceção
await TestExtensions.AssertDoesNotThrowAsync(
    () => service.ValidOperationAsync()
);
```

## Gerenciamento de Serviços

### Registro Básico de Serviços

```csharp
// Registrar instância
AddService<IUserService>(new UserService());

// Registrar tipo com tempo de vida
AddService<IUserService, UserService>(ServiceLifetime.Scoped);

// Configurar múltiplos serviços
ConfigureServices(services =>
{
    services.AddTransient<IService1, Service1>();
    services.AddSingleton<IService2, Service2>();
});
```

### Operações Avançadas de Serviços

```csharp
// Substituir serviço existente
ReplaceService<IUserService>(new MockUserService());

// Verificar se serviço está registrado
if (IsServiceRegistered<IUserService>())
{
    // Serviço existe
}

// Criar provedor com escopo
using var scope = CreateScope();
var scopedService = scope.ServiceProvider.GetRequiredService<IScopedService>();
```

## Gerenciamento de Configuração

```csharp
// Adicionar valores de configuração
AddConfigurationItem("Database:ConnectionString", "test-connection");
AddConfigurationItem("Api:BaseUrl", "https://test-api.com");

// Configuração fica automaticamente disponível via DI
var config = GetRequiredService<IConfiguration>();
var connectionString = config["Database:ConnectionString"];
```

## Mock de Cliente HTTP

Simule dependências HTTP externas para testes:

```csharp
// Mock de endpoint único
var httpClient = HttpClientMock.CreateClient(config => config
    .ForRoute("/api/usuarios/{id}")
    .UsingGet()
    .RespondWithSuccess()
    .WithJsonResponse(new { Id = 1, Name = "João Silva" }));

// Mock de múltiplos endpoints
var httpClient = HttpClientMock.CreateClientWithEndpoints(
    config => config.ForRoute("/api/usuarios").UsingGet().RespondWithSuccess(),
    config => config.ForRoute("/api/usuarios").UsingPost().RespondWith(HttpStatusCode.Created)
);

// Usar em testes de serviços
public class ApiServiceTests : BaseTests
{
    [Fact]
    public async Task GetUser_ShouldReturnUser()
    {
        var httpClient = HttpClientMock.CreateClient(config => config
            .ForRoute("/api/usuarios/1")
            .UsingGet()
            .RespondWithSuccess()
            .WithJsonResponse(new User { Id = 1, Name = "João" }));

        var service = new ApiService(httpClient);
        var result = await service.GetUserAsync(1);

        result.Should().NotBeNull();
        result.Name.Should().Be("João");
    }
}
```

## Extensões FluentAssertions

Asserções aprimoradas para testes MVC/API:

```csharp
// Asserções de código de status
result.Should().BeStatusCodeOk();
result.Should().BeStatusCodeCreated();
result.Should().BeStatusCodeNoContent();

// Extrair dados de resposta
var user = result.GetAs<User>();
user.Should().NotBeNull();
```

## Melhores Práticas

### 1. Organização de Testes

```csharp
public class UserServiceTests : BaseTests
{
    private readonly UserService _service;

    public UserServiceTests()
    {
        // Configuração no construtor
        ConfigureServices(services =>
        {
            services.AddTransient<IUserRepository, MockUserRepository>();
        });

        _service = GetRequiredService<UserService>();
    }

    [Fact]
    public async Task Method_Scenario_ExpectedBehavior()
    {
        // Arrange
        var input = new User
        {
            Id = _faker.Random.Guid(),
            Name = _faker.Name.FullName(),
            Email = _faker.Internet.Email(),
            Age = _faker.Random.Int(18, 65)
        };

        // Act
        var result = await _service.CreateUserAsync(input);

        // Assert
        result.Should().NotBeNull();
    }
}
```

### 2. Padrões de Teste de Banco de Dados

```csharp
public class UserRepositoryTests : BaseDatabaseTests<UserDbContext>
{
    [Fact]
    public async Task CreateUser_ShouldPersist()
    {
        // Sempre inicializar no início
        await InitializeDatabaseAsync();

        try
        {
            // Lógica do teste aqui
            var user = await _repository.CreateAsync(testUser);

            // Verificar persistência
            var saved = await GetContext().Users.FindAsync(user.Id);
            saved.Should().NotBeNull();
        }
        finally
        {
            // Sempre fazer limpeza
            await CleanupDatabaseAsync();
        }
    }
}
```

### 3. Padrões Assíncronos

```csharp
[Fact]
public async Task AsyncOperation_ShouldCompleteQuickly()
{
    await TestExtensions.WithTimeoutAsync(
        async () =>
        {
            var result = await _service.ProcessAsync();
            result.Should().NotBeNull();
        },
        TimeSpan.FromSeconds(2)
    );
}
```

### 4. Recursos Compartilhados

```csharp
// Use IClassFixture para compartilhar dentro de uma classe de teste
public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }
}

// Use Collection para compartilhar entre múltiplas classes de teste
[Collection("Integration Tests")]
public class ApiTests
{
    // Testes aqui compartilham a mesma instância de fixture
}
```

## Casos de Uso Avançados

### Teste de Integração com API

```csharp
public class UserApiTests : BaseTests
{
    private readonly HttpClient _httpClient;
    private readonly UserController _controller;

    public UserApiTests()
    {
        // Configurar mock de serviços
        var mockService = new Mock<IUserService>();
        mockService.Setup(x => x.GetUserAsync(It.IsAny<int>()))
                   .ReturnsAsync(new User { Id = 1, Name = "Teste" });

        AddService<IUserService>(mockService.Object);

        // Configurar controller
        _controller = GetRequiredService<UserController>();
    }

    [Fact]
    public async Task GetUser_WithValidId_ReturnsUser()
    {
        // Act
        var result = await _controller.GetUser(1);

        // Assert
        result.Should().BeStatusCodeOk();
        var user = result.GetAs<User>();
        user.Name.Should().Be("Teste");
    }
}
```

### Teste com Múltiplos Contextos de Banco

```csharp
public class MultiContextTests : BaseDatabaseTests<UserDbContext>
{
    private readonly OrderDbContext _orderContext;

    public MultiContextTests()
    {
        // Configurar segundo contexto
        var orderOptions = new DbContextOptionsBuilder<OrderDbContext>()
            .UseInMemoryDatabase("OrderTestDb")
            .Options;

        _orderContext = new OrderDbContext(orderOptions);
        AddService<OrderDbContext>(_orderContext);
    }

    [Fact]
    public async Task CreateUserAndOrder_ShouldWorkTogether()
    {
        await InitializeDatabaseAsync();

        // Criar usuário no primeiro contexto
        var user = new User { Name = "João" };
        GetContext().Users.Add(user);
        await GetContext().SaveChangesAsync();

        // Criar pedido no segundo contexto
        var order = new Order { UserId = user.Id, Total = 100.00m };
        _orderContext.Orders.Add(order);
        await _orderContext.SaveChangesAsync();

        // Verificar ambos
        var savedUser = await GetContext().Users.FindAsync(user.Id);
        var savedOrder = await _orderContext.Orders.FindAsync(order.Id);

        savedUser.Should().NotBeNull();
        savedOrder.Should().NotBeNull();
        savedOrder.UserId.Should().Be(user.Id);

        await CleanupDatabaseAsync();
    }
}
```

### Teste de Performance

```csharp
public class PerformanceTests : BaseTests
{
    [Fact]
    public async Task BulkOperation_ShouldCompleteWithinTimeLimit()
    {
        var service = GetRequiredService<BulkDataService>();
        var data = new UserBuilder(_faker).BuildList(1000);

        var stopwatch = Stopwatch.StartNew();

        await service.ProcessBulkDataAsync(data);

        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Menos de 5 segundos
    }

    [Fact]
    public async Task Service_ShouldHandleConcurrentRequests()
    {
        var service = GetRequiredService<ConcurrentService>();
        var tasks = new List<Task>();

        // Criar 50 tarefas concorrentes
        for (int i = 0; i < 50; i++)
        {
            var taskId = i;
            tasks.Add(Task.Run(async () =>
            {
                var result = await service.ProcessAsync(taskId);
                result.Should().NotBeNull();
            }));
        }

        // Aguardar todas completarem
        await Task.WhenAll(tasks);
    }
}
```

### Teste de Validação com Myth.Guard

```csharp
public class ValidationTests : BaseTests
{
    private readonly IValidator _validator;

    public ValidationTests()
    {
        AddService<IValidator, Validator>();
        _validator = GetRequiredService<IValidator>();
    }

    [Fact]
    public async Task ValidateUser_WithInvalidData_ShouldThrowValidationException()
    {
        // Arrange
        var invalidUser = new UserBuilder(_faker)
            .WithEmail("email-inválido") // Email inválido
            .Build();

        // Act & Assert
        await _validator.Invoking(v => v.ValidateAsync(invalidUser, ValidationContextKey.Create))
                       .Should().ThrowAsync<ValidationException>()
                       .Where(ex => ex.Message.Contains("Email"));
    }

    [Fact]
    public async Task ValidateUser_WithValidData_ShouldNotThrow()
    {
        // Arrange
        var validUser = new UserBuilder(_faker)
            .WithEmail("joao@exemplo.com")
            .WithName("João Silva")
            .Build();

        // Act & Assert
        await _validator.Invoking(v => v.ValidateAsync(validUser, ValidationContextKey.Create))
                       .Should().NotThrowAsync();
    }
}
```

### Teste de Pipeline com Myth.Flow

```csharp
public class PipelineTests : BaseTests
{
    [Fact]
    public async Task UserCreationPipeline_ShouldProcessSuccessfully()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockValidator = new Mock<IValidator>();

        AddService<IUserRepository>(mockRepository.Object);
        AddService<IValidator>(mockValidator.Object);

        var user = new UserBuilder(_faker).Build();

        // Act
        var result = await Pipeline.Start(user)
            .StepResultAsync<IValidator>((validator, u) => validator.ValidateAndReturnAsync(u))
            .StepAsync<IUserRepository>((repo, u) => repo.AddAsync(u))
            .Transform<UserDto>(u => u.To<UserDto>())
            .ExecuteAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        mockValidator.Verify(v => v.ValidateAndReturnAsync(user, null, default), Times.Once);
        mockRepository.Verify(r => r.AddAsync(user, default), Times.Once);
    }
}
```

## Migração do NUnit

Principais diferenças ao migrar do NUnit:

| NUnit | xUnit | Myth.Testing |
|-------|-------|--------------|
| `[OneTimeSetUp]` | Constructor | Constructor com Setup() |
| `[SetUp]` | Constructor | Manual `InitializeDatabaseAsync()` |
| `[TearDown]` | `IDisposable.Dispose` | Manual `CleanupDatabaseAsync()` |
| `[Test]` | `[Fact]` | `[Fact]` |
| `[TestCase]` | `[Theory]` | `[Theory]` |

### Exemplo de Migração

**Antes (NUnit):**
```csharp
[TestFixture]
public class UserServiceTests
{
    private UserService _service;

    [OneTimeSetUp]
    public void Setup()
    {
        _service = new UserService();
    }

    [Test]
    public void CreateUser_ShouldReturnUser()
    {
        var result = _service.CreateUser("João");
        Assert.IsNotNull(result);
    }
}
```

**Depois (Myth.Testing):**
```csharp
public class UserServiceTests : BaseTests
{
    private readonly UserService _service;

    public UserServiceTests()
    {
        AddService<UserService>(new UserService());
        _service = GetRequiredService<UserService>();
    }

    [Fact]
    public void CreateUser_ShouldReturnUser()
    {
        var result = _service.CreateUser("João");
        result.Should().NotBeNull();
    }
}
```

## Exemplos

Veja a pasta `Examples` para exemplos abrangentes de uso:

- `UserServiceTests.cs` - Padrões básicos de teste unitário
- `UserRepositoryTests.cs` - Testes de banco de dados com Entity Framework
- `SharedFixtureTests.cs` - Fixtures compartilhados e padrões de coleção
- `IntegrationTests.cs` - Testes de integração completos
- `PerformanceTests.cs` - Testes de performance e concorrência

## Integração com Outras Bibliotecas Myth

### Com Myth.Flow

```csharp
public class FlowTests : BaseTests
{
    [Fact]
    public async Task Pipeline_ShouldExecuteSteps()
    {
        var result = await Pipeline.Start("teste")
            .Step(x => x.ToUpper())
            .Step(x => $"Resultado: {x}")
            .ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Resultado: TESTE");
    }
}
```

### Com Myth.Repository

```csharp
public class RepositoryTests : BaseDatabaseTests<AppDbContext>
{
    private readonly IUserRepository _repository;

    public RepositoryTests()
    {
        AddService<IUserRepository, UserRepository>();
        _repository = GetRequiredService<IUserRepository>();
    }

    [Fact]
    public async Task Repository_ShouldSaveAndRetrieve()
    {
        await InitializeDatabaseAsync();

        var user = new UserBuilder(_faker).Build();
        await _repository.AddAsync(user);

        var saved = await _repository.FirstOrDefaultAsync(u => u.Id == user.Id);
        saved.Should().NotBeNull();

        await CleanupDatabaseAsync();
    }
}
```

### Com Myth.Guard

```csharp
public class GuardTests : BaseTests
{
    [Fact]
    public async Task Validation_ShouldWork()
    {
        var validator = GetRequiredService<IValidator>();
        var user = new UserBuilder(_faker).WithEmail("email-inválido").Build();

        await validator.Invoking(v => v.ValidateAsync(user))
                      .Should().ThrowAsync<ValidationException>();
    }
}
```

## Contribuindo

Ao estender Myth.Testing:

1. Siga os padrões estabelecidos para classes base
2. Garanta suporte async/await em toda a biblioteca
3. Forneça documentação XML abrangente
4. Inclua exemplos de uso
5. Mantenha compatibilidade com versões anteriores

## Licença

Este projeto está licenciado sob a Licença Apache 2.0 - veja [LICENSE](https://opensource.org/licenses/Apache-2.0) para detalhes.