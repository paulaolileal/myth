# Myth.Morph

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Morph?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Morph/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Morph?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Morph/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

Uma biblioteca .NET leve para transformação de objetos projetada para arquitetura limpa e Domain-Driven Design. Myth.Morph fornece uma abordagem declarativa baseada em esquemas para mapeamento de objetos com zero overhead de reflexão durante a transformação e integração completa com injeção de dependência.

## Por que Myth.Morph?

Diferente de bibliotecas de mapeamento pesadas que dependem de reflexão em tempo de execução e convenções, Myth.Morph oferece controle explícito sobre transformações mantendo seu código limpo e sustentável:

- **Mapeamentos autodocumentados**: Transformações são definidas onde pertencem - no tipo de origem
- **Type-safe**: Verificação em tempo de compilação para bindings de propriedades
- **Integrado com DI**: Acesso ao service provider para transformações complexas e operações assíncronas
- **Foco em performance**: Compilação de esquema na inicialização, zero reflexão durante mapeamento
- **Separação limpa**: Mantenha DTOs, entidades e view models claramente separados com regras de transformação explícitas

Perfeito para aplicações CQRS, Arquitetura Limpa e DDD onde transformações explícitas importam.

## Funcionalidades

- **Configuração de Esquema Declarativa**: Defina transformações usando API fluente com segurança em tempo de compilação
- **Mapeamento Automático de Propriedades**: Mapeamento baseado em convenção para nomes de propriedades correspondentes
- **Binding Manual**: Quatro estratégias de binding para máxima flexibilidade
- **Suporte Assíncrono**: Suporte de primeira classe para async/await em transformações I/O-bound
- **Injeção de Dependência**: Acesso completo ao service provider na lógica de transformação
- **Coleções Genéricas**: Mapeamento automático de coleções com transformação de elementos
- **Objetos Aninhados**: Suporte para transformação recursiva de grafos de objetos complexos
- **Ignorar Propriedades**: Exclusão explícita de propriedades do mapeamento
- **Logging Abrangente**: Logging de trace detalhado para depuração de transformações

## Instalação

```bash
dotnet add package Myth.Morph
```

## Início Rápido

### 1. Registrar Serviços

```csharp
// Em Program.cs ou Startup.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMorph();

var app = builder.BuildApp(); // Use BuildApp() ao invés de Build()
```

Para aplicações console:

```csharp
var services = new ServiceCollection();
services.AddMorph();

var provider = services.BuildWithGlobalProvider();
```

### 2. Definir Transformações

Implemente `IMorphable<TDestination>` no seu tipo de origem:

```csharp
public class CreateUserDto : IMorphable<User> {
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime BirthDate { get; set; }

    public void MorphTo( Schema<User> schema ) {
        schema
            .Bind(u => u.FullName, () => Name)
            .Bind(u => u.EmailAddress, () => Email)
            .Bind(u => u.Age, () => DateTime.Today.Year - BirthDate.Year);
    }
}
```

### 3. Transformar Objetos

```csharp
var dto = new CreateUserDto {
    Name = "João Silva",
    Email = "joao@exemplo.com",
    BirthDate = new DateTime(1990, 1, 1)
};

var user = dto.To<User>();
```

## Estratégias de Binding

Myth.Morph fornece quatro estratégias de binding para lidar com diferentes cenários de transformação.

### 1. Binding Direto de Valor

Mapear uma propriedade para um valor computado:

```csharp
public void MorphTo( Schema<User> schema ) {
    schema.Bind(u => u.FullName, () => $"{FirstName} {LastName}");
}
```

### 2. Binding com Service Provider

Acessar serviços do container DI para transformações complexas:

```csharp
public void MorphTo( Schema<Order> schema ) {
    schema.Bind(o => o.Customer, sp => {
        var customerService = sp.GetRequiredService<ICustomerService>();
        return customerService.GetCustomerById(CustomerId);
    });
}
```

### 3. Binding Assíncrono Direto

Para operações assíncronas sem service provider:

```csharp
public void MorphTo( Schema<User> schema ) {
    schema.BindAsync(u => u.Avatar, async () => {
        await Task.Delay(100); // Simular trabalho assíncrono
        return "default-avatar.png";
    });
}
```

### 4. Binding Assíncrono com Service Provider

Combinar operações assíncronas com DI:

```csharp
public void MorphTo( Schema<Product> schema ) {
    schema.BindAsync(p => p.Reviews, async sp => {
        var reviewService = sp.GetRequiredService<IReviewService>();
        return await reviewService.GetReviewsAsync(ProductId);
    });
}
```

## Mapeamento Automático de Propriedades

Propriedades com nomes correspondentes e tipos compatíveis são mapeadas automaticamente:

```csharp
public class UserDto : IMorphable<User> {
    public string Name { get; set; }      // Auto-mapeia para User.Name
    public string Email { get; set; }     // Auto-mapeia para User.Email
    public int Age { get; set; }          // Auto-mapeia para User.Age

    public void MorphTo( Schema<User> schema ) {
        // Apenas defina mapeamentos customizados - mapeamento automático cuida do resto
        schema.Ignore(u => u.InternalId);
    }
}
```

### Funcionalidades do Mapeamento Automático

- **Correspondência de nomes**: Propriedades com nomes idênticos são mapeadas automaticamente
- **Conversão de tipos**: Lida com conversões de tipos primitivos (int para long, etc.)
- **Objetos aninhados**: Transforma recursivamente objetos aninhados usando mapeamentos registrados
- **Tratamento de null**: Lida com segurança valores null e tipos nullable
- **Mapeamento de coleções**: Mapeia automaticamente tipos de coleção compatíveis

## Ignorando Propriedades

Excluir propriedades de mapeamento manual e automático:

```csharp
public void MorphTo( Schema<User> schema ) {
    schema
        .Ignore(u => u.InternalId)
        .Ignore(u => u.CreatedBy)
        .Ignore(u => u.ModifiedBy);
}
```

## Transformações de Coleções

Transformar coleções com métodos de extensão type-safe:

```csharp
// Transformar enumerable
IEnumerable<UserDto> dtos = GetUserDtos();
IEnumerable<User> users = dtos.To<User>();

// Transformar com service provider
IEnumerable<User> users = dtos.To<User>(serviceProvider);

// Transformação assíncrona de coleção
IEnumerable<User> users = await dtos.ToAsync<User>();

// Transformação type-safe de coleção
List<UserDto> dtoList = GetUserDtos();
IEnumerable<User> users = dtoList.To<UserDto, User>();
```

## Mapeamento de Objetos Aninhados

Myth.Morph lida automaticamente com transformações aninhadas:

```csharp
public class OrderDto : IMorphable<Order> {
    public int OrderId { get; set; }
    public List<OrderItemDto> Items { get; set; }

    public void MorphTo( Schema<Order> schema ) {
        schema
            .Bind(o => o.Id, () => OrderId)
            .BindAsync(o => o.Items, async sp =>
                // Transformação de coleção aninhada
                await Items.ToAsync<OrderItem>(sp)
            );
    }
}

// OrderItemDto também implementa IMorphable<OrderItem>
public class OrderItemDto : IMorphable<OrderItem> {
    public string ProductName { get; set; }
    public decimal Price { get; set; }

    public void MorphTo( Schema<OrderItem> schema ) {
        // Mapeamento automático cuida das propriedades
    }
}
```

## Configuração Avançada

### Escaneamento de Assembly Customizado

Limitar assemblies escaneados para implementações IMorphable:

```csharp
services.AddMorph(settings => {
    settings.AddAssembly(Assembly.GetExecutingAssembly());
    settings.AddAssemblies(typeof(UserDto).Assembly, typeof(OrderDto).Assembly);
});
```

### Mapeamentos de Tipos Genéricos

Registrar mapeamentos entre interfaces genéricas e tipos concretos:

```csharp
services.AddMorph(settings => {
    settings.AddGenericMorph(typeof(IList<>), typeof(List<>));
    settings.AddGenericMorph(typeof(ICustomCollection<>), typeof(CustomCollection<>));

    // Mapeamento genérico type-safe
    settings.AddGenericMapping<IMyInterface<>, MyImplementation<>>();
});
```

### Mapeamentos Genéricos Padrão

Myth.Morph inclui estes mapeamentos padrão:

- `IList<>` → `List<>`
- `ICollection<>` → `List<>`
- `IDictionary<,>` → `Dictionary<,>`
- `ISet<>` → `HashSet<>`
- `IReadOnlyCollection<>` → `ReadOnlyCollection<>`
- `IReadOnlyList<>` → `List<>`
- `IReadOnlySet<>` → `HashSet<>`

### Limpar Mapeamentos Padrão

```csharp
services.AddMorph(settings => {
    settings.ClearGenericMappings()
            .AddGenericMapping<IList<>, ArrayList>(); // Usar mapeamento customizado
});
```

## Exemplos do Mundo Real

### Comando CQRS para Entidade

```csharp
public class CreateProductCommand : IMorphable<Product> {
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string CategoryId { get; set; }

    public void MorphTo( Schema<Product> schema ) {
        schema
            .Bind(p => p.Name, () => Name)
            .Bind(p => p.Price, () => Price)
            .Bind(p => p.Category, sp => {
                var categoryRepo = sp.GetRequiredService<ICategoryRepository>();
                return categoryRepo.GetById(CategoryId);
            })
            .Bind(p => p.CreatedAt, () => DateTime.UtcNow)
            .Bind(p => p.IsActive, () => true);
    }
}
```

### Resposta de API para Modelo de Domínio

```csharp
public class UserApiResponse : IMorphable<User> {
    public string Id { get; set; }
    public string FullName { get; set; }
    public string EmailAddress { get; set; }

    public void MorphTo( Schema<User> schema ) {
        schema
            .Bind(u => u.UserId, () => Guid.Parse(Id))
            .Bind(u => u.Name, () => FullName)
            .Bind(u => u.Email, () => EmailAddress)
            .BindAsync(u => u.Preferences, async sp => {
                var prefService = sp.GetRequiredService<IPreferenceService>();
                return await prefService.GetUserPreferencesAsync(Id);
            });
    }
}
```

### Entidade para DTO com Propriedades Computadas

```csharp
public class Product {
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductDto : IMorphable<Product> {
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string DisplayName { get; set; }

    public void MorphTo( Schema<Product> schema ) {
        // Id, Name, Price auto-mapeados
        schema.Ignore(p => p.CreatedAt);
    }
}

// Transformação reversa
var product = new Product { Id = 1, Name = "Widget", Price = 99.99m };
var dto = product.To<ProductDto>();
```

### Integração com Event Sourcing

```csharp
public class UserRegisteredEvent : IMorphable<User> {
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public string Name { get; set; }
    public DateTime RegisteredAt { get; set; }

    public void MorphTo( Schema<User> schema ) {
        schema
            .Bind(u => u.Id, () => UserId)
            .Bind(u => u.Email, () => Email)
            .Bind(u => u.FullName, () => Name)
            .Bind(u => u.CreatedDate, () => RegisteredAt)
            .Bind(u => u.IsActive, () => true)
            .Bind(u => u.EmailVerified, () => false);
    }
}
```

### Integração com Padrão Repository

```csharp
public class ProductService {
    private readonly IProductRepository _repository;
    private readonly IServiceProvider _serviceProvider;

    public ProductService( IProductRepository repository, IServiceProvider serviceProvider ) {
        _repository = repository;
        _serviceProvider = serviceProvider;
    }

    public async Task<ProductDto> GetProductAsync( int productId ) {
        var product = await _repository.GetByIdAsync(productId);
        return product.To<ProductDto>(_serviceProvider);
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync() {
        var products = await _repository.GetAllAsync();
        return await products.ToAsync<ProductDto>(_serviceProvider);
    }

    public async Task<Product> CreateProductAsync( CreateProductDto dto ) {
        var product = dto.To<Product>(_serviceProvider);
        return await _repository.AddAsync(product);
    }
}
```

## Verificando Disponibilidade de Mapeamento

Verificar se um mapeamento existe antes de tentar transformação:

```csharp
var dto = new UserDto();

// Verificar se mapeamento existe
if (dto.CanBindTo<User>()) {
    var user = dto.To<User>();
}

// Verificação type-safe
if (dto.CanBindTo<UserDto, User>()) {
    var user = dto.To<User>();
}
```

## Tratamento de Exceções

Myth.Morph fornece exceções específicas para diferentes cenários de erro:

### Tipos de Exceção

- **`BinderNotFoundException`**: Nenhum mapeamento registrado entre tipos de origem e destino
- **`BindException`**: Operação de binding de propriedade ou campo falhou
- **`InvalidMorphConfigurationException`**: SchemaRegistry não configurado adequadamente no DI

### Exemplo

```csharp
try {
    var user = dto.To<User>();
} catch ( BinderNotFoundException ex ) {
    logger.LogError("Nenhum mapeamento encontrado de {Source} para {Dest}", ex.SourceType, ex.DestType);
} catch ( BindException ex ) {
    logger.LogError("Falha no binding de propriedade: {Message}", ex.Message);
} catch ( InvalidMorphConfigurationException ex ) {
    logger.LogError("Morph não configurado: {Message}", ex.Message);
}
```

## Considerações de Performance

### Melhores Práticas

1. **Reutilizar Service Provider**: Passar a mesma instância do service provider ao transformar múltiplos objetos

```csharp
var users = new List<User>();
foreach (var dto in dtos) {
    users.Add(dto.To<User>(serviceProvider)); // Reutilizar provider
}
```

2. **Usar Async para I/O**: Sempre usar bindings assíncronos para chamadas de banco de dados ou API

```csharp
schema.BindAsync(u => u.Profile, async sp => {
    var service = sp.GetRequiredService<IProfileService>();
    return await service.GetProfileAsync(UserId); // I/O Assíncrono
});
```

3. **Limitar Escaneamento de Assembly**: Reduzir tempo de inicialização especificando assemblies

```csharp
services.AddMorph(settings => {
    settings.ClearAssemblies()
            .AddAssembly(typeof(MyDto).Assembly);
});
```

4. **Processar Coleções em Lote**: Transformar coleções em bulk ao invés de individualmente

```csharp
// Bom
var users = dtos.To<User>();

// Evitar
var users = dtos.Select(d => d.To<User>()).ToList();
```

## Logging

Myth.Morph fornece logging abrangente em diferentes níveis:

- **Information**: Inicialização do registry, escaneamento de assemblies
- **Debug**: Execuções de mapeamento, resolução de tipos
- **Trace**: Bindings de propriedades, mapeamentos automáticos
- **Warning**: Carregamento parcial de tipos, falhas de mapeamento
- **Error**: Problemas de configuração, exceções de binding

Habilitar logging na sua aplicação:

```csharp
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

## Solução de Problemas

### Erro "ServiceProvider not configured"

```csharp
// Garantir que AddMorph() é chamado e BuildApp() é usado
services.AddMorph();
var app = builder.BuildApp(); // Não builder.Build()
```

### Erro "No mapping found"

```csharp
// Garantir que a origem implementa IMorphable<TDestination>
public class MyDto : IMorphable<MyEntity> {
    public void MorphTo( Schema<MyEntity> schema ) { }
}
```

### Erro "SchemaRegistry not found in DI"

```csharp
// Faltando registro AddMorph()
services.AddMorph(); // Adicionar isso
```

### Mapeamento Não Funcionando para Coleções Genéricas

```csharp
// Registrar mapeamento genérico se necessário
services.AddMorph(settings => {
    settings.AddGenericMapping<IMyCollection<>, MyCollection<>>();
});
```

### Propriedade Não Sendo Mapeada

Verificar estes problemas comuns:

1. Nome de propriedade não corresponde (sensível a maiúsculas)
2. Propriedade não gravável (sem setter)
3. Propriedade explicitamente ignorada
4. Incompatibilidade de tipo

## Integração com Outras Bibliotecas Myth

### Com Myth.Flow

```csharp
var result = await Pipeline.Start(createUserDto)
    .Step((dto, sp) => dto.To<User>(sp))
    .StepAsync<IUserRepository>((repo, user) => repo.AddAsync(user))
    .ExecuteAsync();
```

### Com Myth.Guard

```csharp
public class CreateUserDto : IMorphable<User>, IValidatable<CreateUserDto> {
    public string Name { get; set; }
    public string Email { get; set; }

    public void Validate( ValidationBuilder<CreateUserDto> builder, ValidationContextKey? context ) {
        builder.For(Name, x => x.NotEmpty().MinimumLength(3));
        builder.For(Email, x => x.Email());
    }

    public void MorphTo( Schema<User> schema ) {
        schema.Bind(u => u.FullName, () => Name);
        schema.Bind(u => u.EmailAddress, () => Email);
    }
}

// Usar no controller
await _validator.ValidateAsync(dto);
var user = dto.To<User>();
```

### Com Myth.Repository

```csharp
public class UserRepository : IUserRepository {
    public async Task<UserDto> GetUserDtoAsync( int userId ) {
        var user = await _dbContext.Users.FindAsync(userId);
        return user.To<UserDto>();
    }
}
```

## Contribuindo

Contribuições são bem-vindas! Por favor, leia nossas diretrizes de contribuição e sinta-se à vontade para enviar pull requests.

## Licença

Este projeto é licenciado sob a Licença Apache 2.0 - veja o arquivo LICENSE para detalhes.
