<img  style="float: right;" src="myth-morph-logo.png" alt="drawing" width="250"/>

# Myth.Morph

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Morph?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Morph/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Morph?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Morph/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

Uma biblioteca .NET leve para transformação de objetos projetada para arquitetura limpa e Domain-Driven Design. Myth.Morph fornece uma abordagem declarativa baseada em esquemas para mapeamento de objetos com zero overhead de reflexão durante a transformação e integração completa com injeção de dependência.

## 🎯 Por que Myth.Morph?

**Mapeamento de objetos é um pesadelo oculto de performance e manutenção.** Bibliotecas estilo AutoMapper usam reflexão em runtime que mata performance e obscurece lógica de transformação. Mapeamento manual é verboso e propenso a erros—esquecer uma propriedade e dados são perdidos. DTOs poluem modelos de domínio, ou pior, entidades de domínio são expostas diretamente a APIs quebrando encapsulamento. **Myth.Morph resolve isso com transformações explícitas e compile-time safe** que são rápidas (schema compilado no startup, zero reflexão durante mapping), claras (mappings vivem com types) e DI-aware (transformações assíncronas com acesso a serviços).

### O Problema: Reflexão em Runtime = Performance Hell & Mágica

**AutoMapper**: Convenções baseadas em mágica. Reflexão em runtime em cada chamada de mapeamento—lento. Caixa preta—o que acontece? Sem ideia até runtime.

**Mapeamento Manual**: Verboso e duplicado em 10 lugares. Esqueceu de mapear `Address`? Shipping falha em produção. Atualizar modelo `User`? Boa sorte encontrando todos os mapeamentos.

**Problemas**: Performance (reflexão), manutenibilidade (mágica baseada em convenções), propenso a erros (propriedades esquecidas), sem acesso DI, não async.

### A Solução: Transformações Explícitas, Compile-Time Safe, DI-Aware

Mapeamento definido onde pertence—com o type. **Bindings explícitos**: compile-time safe via lambdas. **DI-aware**: acesso completo a service provider. **Async-ready**: await database/API calls naturalmente. **Rápido**: schema compilado uma vez, zero reflexão durante mapping.

### Por Que Escolher Myth.Morph?

**Performance**: Rápido (pre-compiled schema) vs lento (AutoMapper runtime reflection). **Explicitness**: Bindings explícitos vs mágica de convenções. **Location**: Com type (DDD) vs perfil separado. **Type Safety**: Compile-time (lambdas) vs runtime. **DI Access**: Nativo em transformações vs limitado. **Async Support**: First-class `.BindAsync()` vs limitado.

### Aplicações no Mundo Real

**APIs CQRS**: Map `CreateUserCommand` → `User` entity → `UserDto` response. Transformações diferentes por operação. Async validation/enrichment durante mapping.

**Clean Architecture**: Entities de domínio ficam puros. ViewModels sabem como serem criados de entities. Separação clara entre camadas.

**Microservices**: Transformar respostas de APIs third-party em modelos de domínio com async enrichment (chamar internal APIs, cache, DB).

**Event-Driven**: Transform domain entities em integration events. Async loading de dados relacionados antes de publicar.

### Fundamentos Conceituais

**Schema-Based Mapping**: Defina transformation schema uma vez, aplique muitas vezes. Compile schema para performance.

**Explicit over Implicit**: LINQ philosophy—bindings explícitos com compile-time safety beats mágica baseada em convenções.

**Single Responsibility (DDD)**: Mappings são responsabilidade do type que precisa deles. DTOs sabem como serem criados de entities.

**Fluent Interface**: Method chaining para configuração legível.

### Valor de Negócio

**Desenvolvedores**: 50% menos código de mapping vs manual. 10x mais rápido vs AutoMapper (reflexão). Debug claro (bindings explícitos, sem mágica). Transformações async.

**Arquitetos**: DDD-aligned (mappings pertencem a types). Clean separation (entities, DTOs, ViewModels). Performance (pre-compiled schemas). Escalável (async transformations).

**DevOps/SRE**: Performance previsível (sem reflexão). Fácil monitoramento (call stacks claros). Debuggable (código explícito).

**Times de Produto**: Desenvolvimento mais rápido (menos boilerplate). Menos bugs (bindings type-safe). Melhor API design (DTOs limpos, entities puros). Refactoring mais fácil.

## Funcionalidades

- **Padrões de Mapeamento Bidirecionais**: Suporte para padrões `IMorphableTo<T>` e `IMorphableFrom<T>`
- **Suporte a Proxies do Entity Framework**: Detecção e tratamento automático de proxies lazy-loading do EF Core
- **Resolução de Hierarquia de Herança**: Travessia configurável de profundidade para hierarquias de tipos complexas
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

## Suporte a Proxies do Entity Framework

Myth.Morph fornece detecção e tratamento automático de proxies lazy-loading do Entity Framework Core, garantindo transformações confiáveis mesmo ao trabalhar com entidades proxied.

### Cenário do Problema

Ao usar Entity Framework Core com lazy loading, o EF cria proxies dinâmicos das suas classes de entidade (usando Castle.Proxies). Essas classes proxy herdam de suas entidades mas têm types diferentes em tempo de execução, o que pode quebrar mapeamentos baseados em tipos.

```csharp
// Você define isso
public class User {
    public string Name { get; set; }
    public virtual Profile Profile { get; set; } // Lazy loading
}

// EF retorna isso em runtime
// Castle.Proxies.UserProxy : User
```

### Como Myth.Morph Resolve

O Morph detecta automaticamente proxies do EF e resolve para o tipo base da entidade:

```csharp
public class UserDto : IMorphableFrom<User> {
    public string Name { get; set; }
    public string ProfileName { get; set; }

    public void MorphFrom( Schema<User> schema ) {
        schema
            .Bind(() => Name, u => u.Name)
            .Bind(() => ProfileName, u => u.Profile.Name);
    }
}

// Funciona perfeitamente com entities proxied
var user = await dbContext.Users.Include(u => u.Profile).FirstAsync();
var dto = user.To<UserDto>(); // Morph detecta proxy e resolve corretamente
```

### Configuração de Travessia de Hierarquia

Para hierarquias de tipos complexas, você pode configurar o comportamento de fallback de herança:

```csharp
services.AddMorph(settings => {
    settings.ConfigureTypeResolver(options => {
        options.InheritanceDepth = 5;              // Máximo 5 níveis na cadeia de herança
        options.IncludeInterfaces = true;          // Incluir interfaces na resolução de hierarquia
        options.EnableInheritanceFallback = true;  // Tentar tipos pai se exato não encontrado
    });
});
```

### Otimização de Performance

O Morph usa otimização de caminho rápido para entidades não-proxy e cai para tratamento baseado em reflexão apenas quando proxies são detectados:

- **Caminho Rápido**: Conversão de tipo direta para entidades regulares (quase zero overhead)
- **Caminho de Proxy**: Resolução de tipo baseada em reflexão para entidades proxied (mínimo overhead)

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

O Myth.Morph suporta dois padrões de transformação:

**Padrão IMorphableTo** - A origem define como transformar para destino (comando → entidade):

```csharp
public class CreateUserDto : IMorphableTo<User> {
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

**Padrão IMorphableFrom** - O destino define como criar a partir da origem (entidade → DTO):

```csharp
public class UserDto : IMorphableFrom<User> {
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }

    public void MorphFrom( Schema<User> schema ) {
        schema
            .Bind(() => Name, u => u.FullName)
            .Bind(() => Email, u => u.EmailAddress)
            .Bind(() => Age, u => DateTime.Today.Year - u.BirthDate.Year);
    }
}
```

> **Dica**: Use `IMorphableTo` para comandos e inputs (criando entidades), e `IMorphableFrom` para DTOs e respostas (projetando a partir de entidades).

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

Myth.Morph fornece quatro estratégias de binding para lidar com diferentes cenários de transformação. Os exemplos abaixo mostram ambos os padrões IMorphableTo e IMorphableFrom.

### 1. Binding Direto de Valor

**IMorphableTo** - Mapear propriedade de destino para valor computado:

```csharp
public class CreateUserDto : IMorphableTo<User> {
    public string FirstName { get; set; }
    public string LastName { get; set; }

    public void MorphTo( Schema<User> schema ) {
        schema.Bind(u => u.FullName, () => $"{FirstName} {LastName}");
    }
}
```

**IMorphableFrom** - Mapear propriedade do DTO a partir de valor computado:

```csharp
public class UserDto : IMorphableFrom<User> {
    public string DisplayName { get; set; }

    public void MorphFrom( Schema<User> schema ) {
        schema.Bind(() => DisplayName, u => $"{u.FirstName} {u.LastName}");
    }
}
```

### 2. Binding com Service Provider

**IMorphableTo** - Acessar serviços para criar entidades:

```csharp
public class CreateOrderDto : IMorphableTo<Order> {
    public string CustomerId { get; set; }

    public void MorphTo( Schema<Order> schema ) {
        schema.Bind(o => o.Customer, sp => {
            var customerService = sp.GetRequiredService<ICustomerService>();
            return customerService.GetCustomerById(CustomerId);
        });
    }
}
```

**IMorphableFrom** - Acessar serviços para projetar DTOs:

```csharp
public class OrderDto : IMorphableFrom<Order> {
    public string CustomerName { get; set; }

    public void MorphFrom( Schema<Order> schema ) {
        schema.Bind(() => CustomerName, (o, sp) => {
            var customerService = sp.GetRequiredService<ICustomerService>();
            return customerService.GetCustomerName(o.CustomerId);
        });
    }
}
```

### 3. Binding Assíncrono Direto

**IMorphableTo** - Operações assíncronas para criar entidades:

```csharp
public class CreateUserDto : IMorphableTo<User> {
    public void MorphTo( Schema<User> schema ) {
        schema.BindAsync(u => u.Avatar, async () => {
            await Task.Delay(100); // Simular trabalho assíncrono
            return "default-avatar.png";
        });
    }
}
```

**IMorphableFrom** - Operações assíncronas para projetar DTOs:

```csharp
public class UserDto : IMorphableFrom<User> {
    public string AvatarUrl { get; set; }

    public void MorphFrom( Schema<User> schema ) {
        schema.BindAsync(() => AvatarUrl, async u => {
            await Task.Delay(100); // Simular carregamento de imagem
            return $"/avatars/{u.AvatarId}.png";
        });
    }
}
```

### 4. Binding Assíncrono com Service Provider

**IMorphableTo** - Combinar operações assíncronas com DI para criar entidades:

```csharp
public class CreateProductDto : IMorphableTo<Product> {
    public int ProductId { get; set; }

    public void MorphTo( Schema<Product> schema ) {
        schema.BindAsync(p => p.Reviews, async sp => {
            var reviewService = sp.GetRequiredService<IReviewService>();
            return await reviewService.GetReviewsAsync(ProductId);
        });
    }
}
```

**IMorphableFrom** - Combinar operações assíncronas com DI para projetar DTOs:

```csharp
public class ProductDto : IMorphableFrom<Product> {
    public List<ReviewDto> Reviews { get; set; }

    public void MorphFrom( Schema<Product> schema ) {
        schema.BindAsync(() => Reviews, async (p, sp) => {
            var reviewService = sp.GetRequiredService<IReviewService>();
            var reviews = await reviewService.GetReviewsAsync(p.Id);
            return reviews.Select(r => new ReviewDto { Text = r.Text }).ToList();
        });
    }
}
```

## Mapeamento Automático de Propriedades

Propriedades com nomes correspondentes e tipos compatíveis são mapeadas automaticamente em ambos os padrões:

**IMorphableTo** - Criar entidades a partir de DTOs:

```csharp
public class CreateUserDto : IMorphableTo<User> {
    public string Name { get; set; }      // Auto-mapeia para User.Name
    public string Email { get; set; }     // Auto-mapeia para User.Email
    public int Age { get; set; }          // Auto-mapeia para User.Age

    public void MorphTo( Schema<User> schema ) {
        // Apenas defina mapeamentos customizados - mapeamento automático cuida do resto
        schema.Ignore(u => u.InternalId);
    }
}
```

**IMorphableFrom** - Projetar DTOs a partir de entidades:

```csharp
public class UserDto : IMorphableFrom<User> {
    public string Name { get; set; }      // Auto-mapeia de User.Name
    public string Email { get; set; }     // Auto-mapeia de User.Email
    public int Age { get; set; }          // Auto-mapeia de User.Age

    public void MorphFrom( Schema<User> schema ) {
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

Myth.Morph lida automaticamente com transformações aninhadas em ambos os padrões:

**IMorphableTo** - Criar entidades com objetos aninhados:

```csharp
public class CreateOrderDto : IMorphableTo<Order> {
    public int OrderId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; }

    public void MorphTo( Schema<Order> schema ) {
        schema
            .Bind(o => o.Id, () => OrderId)
            .BindAsync(o => o.Items, async sp =>
                // Transformação de coleção aninhada
                await Items.ToAsync<OrderItem>(sp)
            );
    }
}

// CreateOrderItemDto implementa IMorphableTo<OrderItem>
public class CreateOrderItemDto : IMorphableTo<OrderItem> {
    public string ProductName { get; set; }
    public decimal Price { get; set; }

    public void MorphTo( Schema<OrderItem> schema ) {
        // Mapeamento automático cuida das propriedades
    }
}
```

**IMorphableFrom** - Projetar DTOs com objetos aninhados:

```csharp
public class OrderDto : IMorphableFrom<Order> {
    public int Id { get; set; }
    public List<OrderItemDto> Items { get; set; }

    public void MorphFrom( Schema<Order> schema ) {
        schema
            .Bind(() => Id, o => o.Id)
            .BindAsync(() => Items, async (o, sp) =>
                // Transformação de coleção aninhada
                await o.Items.ToAsync<OrderItemDto>(sp)
            );
    }
}

// OrderItemDto implementa IMorphableFrom<OrderItem>
public class OrderItemDto : IMorphableFrom<OrderItem> {
    public string ProductName { get; set; }
    public decimal Price { get; set; }

    public void MorphFrom( Schema<OrderItem> schema ) {
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

### Comando CQRS para Entidade (IMorphableTo)

Use `IMorphableTo` quando o tipo de comando/input define como criar a entidade:

```csharp
public class CreateProductCommand : IMorphableTo<Product> {
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

// Uso
var command = new CreateProductCommand { Name = "Widget", Price = 99.99m, CategoryId = "CAT-1" };
var product = command.To<Product>(serviceProvider);
```

### Entidade para DTO (IMorphableFrom)

Use `IMorphableFrom` quando o DTO define como projetar a partir da entidade:

```csharp
public class Product {
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
    public Category Category { get; set; }
}

public class ProductDto : IMorphableFrom<Product> {
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string DisplayName { get; set; }
    public string CategoryName { get; set; }

    public void MorphFrom( Schema<Product> schema ) {
        // Id, Name, Price auto-mapeados
        schema
            .Bind(() => DisplayName, p => $"{p.Name} - ${p.Price}")
            .Bind(() => CategoryName, p => p.Category.Name)
            .Ignore(p => p.CreatedAt);
    }
}

// Transformação de entidade para DTO
var product = await dbContext.Products.Include(p => p.Category).FirstAsync();
var dto = product.To<ProductDto>();
```

### Resposta de API para Modelo de Domínio (IMorphableTo)

```csharp
public class UserApiResponse : IMorphableTo<User> {
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

### Integração com Event Sourcing (IMorphableTo)

```csharp
public class UserRegisteredEvent : IMorphableTo<User> {
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

    // Usar IMorphableFrom para projetar entidades em DTOs
    public async Task<ProductDto> GetProductAsync( int productId ) {
        var product = await _repository.GetByIdAsync(productId);
        return product.To<ProductDto>(_serviceProvider);
    }

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync() {
        var products = await _repository.GetAllAsync();
        return await products.ToAsync<ProductDto>(_serviceProvider);
    }

    // Usar IMorphableTo para criar entidades a partir de comandos
    public async Task<Product> CreateProductAsync( CreateProductCommand command ) {
        var product = command.To<Product>(_serviceProvider);
        return await _repository.AddAsync(product);
    }

    // Usar IMorphableTo para atualizar entidades a partir de comandos
    public async Task<Product> UpdateProductAsync( int id, UpdateProductCommand command ) {
        var product = await _repository.GetByIdAsync(id);

        // Atualizar usando mapeamento manual ou criar novo método de extensão
        product.Name = command.Name;
        product.Price = command.Price;

        return await _repository.UpdateAsync(product);
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
// Garantir que o tipo de origem implementa IMorphableTo<TDestination>
public class CreateMyDto : IMorphableTo<MyEntity> {
    public void MorphTo( Schema<MyEntity> schema ) { }
}

// Ou que o tipo de destino implementa IMorphableFrom<TSource>
public class MyDto : IMorphableFrom<MyEntity> {
    public void MorphFrom( Schema<MyEntity> schema ) { }
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
public class CreateUserDto : IMorphableTo<User>, IValidatable<CreateUserDto> {
    public string Name { get; set; }
    public string Email { get; set; }

    public void Validate( ValidationBuilder<CreateUserDto> builder, ValidationContextKey? context ) {
        builder.For(Name, x => x.NotEmpty().MinimumLength(3));
        builder.For(Email, x => x.Email());
    }

    public void MorphTo( Schema<User> schema ) {
        schema
            .Bind(u => u.FullName, () => Name)
            .Bind(u => u.EmailAddress, () => Email);
    }
}

// Usar no controller
await _validator.ValidateAsync(dto);
var user = dto.To<User>();
```

### Com Myth.Repository

```csharp
public class UserRepository : IUserRepository {
    private readonly DbContext _dbContext;

    public UserRepository( DbContext dbContext ) {
        _dbContext = dbContext;
    }

    // Usar IMorphableFrom para projetar entidades em DTOs
    public async Task<UserDto> GetUserDtoAsync( int userId ) {
        var user = await _dbContext.Users.FindAsync(userId);
        return user.To<UserDto>(); // UserDto implementa IMorphableFrom<User>
    }

    // Funciona perfeitamente com proxies do Entity Framework
    public async Task<UserDto> GetUserWithProfileAsync( int userId ) {
        var user = await _dbContext.Users
            .Include(u => u.Profile) // Lazy loading cria proxy
            .FirstAsync(u => u.Id == userId);

        return user.To<UserDto>(); // Morph detecta e trata proxy automaticamente
    }
}
```

## Contribuindo

Contribuições são bem-vindas! Por favor, leia nossas diretrizes de contribuição e sinta-se à vontade para enviar pull requests.

## Licença

Este projeto é licenciado sob a Licença Apache 2.0 - veja o arquivo LICENSE para detalhes.
