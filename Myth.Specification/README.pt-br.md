<img  style="float: right;" src="myth-specification-logo.png" alt="drawing" width="250"/>

# Myth.Specification

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Specification?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Specification/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Specification?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Specification/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

Uma biblioteca .NET fluente e type-safe que implementa o **Padrão Specification** para construir consultas complexas, combináveis e testáveis. Mantenha sua lógica de negócio legível, manutenível e firmemente enraizada em conceitos de domínio.

## Por que usar Myth.Specification?

A construção tradicional de consultas geralmente leva a:
- Regras de negócio espalhadas por serviços e repositórios
- Lógica de consulta difícil de testar
- Lógica de filtragem duplicada em vários lugares
- Má legibilidade e manutenibilidade

Myth.Specification resolve esses problemas ao:
- Encapsular lógica de consulta em especificações reutilizáveis e combináveis
- Tornar as regras de negócio explícitas e autodocumentadas
- Fornecer uma API fluente e encadeável para construir consultas complexas
- Suportar filtragem, ordenação, paginação e validação em uma interface coesa

## Instalação

```bash
dotnet add package Myth.Specification
```

## Conceitos Principais

A biblioteca é construída em torno da interface `ISpec<T>` e da classe abstrata `SpecBuilder<T>`, fornecendo:

- **Filtragem**: Combine predicados com `And`, `Or`, `Not`, `AndIf`, `OrIf`
- **Ordenação**: Encadeie múltiplos critérios de ordenação com `Order` e `OrderDescending`
- **Paginação**: Aplique lógica de skip/take com `Skip`, `Take` ou `WithPagination`
- **Pós-processamento**: Aplique operações distintas com `DistinctBy`
- **Validação**: Verifique se entidades satisfazem especificações com `IsSatisfiedBy`
- **Execução**: Aplique especificações a queryables e enumere resultados

## Início Rápido

### Construção Básica de Especificações

```csharp
using Myth.Interfaces;
using Myth.Specifications;
using Myth.Extensions;

// Construir uma especificação
var spec = SpecBuilder<Person>
    .Create()
    .And( x => x.IsActive )
    .And( x => x.Age >= 18 )
    .Order( x => x.Name )
    .Skip( 10 )
    .Take( 20 );

// Aplicar a um queryable
var results = dbContext.People
    .Specify( spec )
    .ToList();
```

### Encapsulando Regras de Negócio

O verdadeiro poder vem de criar extensões de especificação reutilizáveis que representam conceitos de negócio:

```csharp
public static class PersonSpecifications {
    public static ISpec<Person> IsAdult( this ISpec<Person> spec ) {
        return spec.And( person => person.Age >= 18 );
    }

    public static ISpec<Person> IsActive( this ISpec<Person> spec ) {
        return spec.And( person => person.Status == PersonStatus.Active );
    }

    public static ISpec<Person> HasEmail( this ISpec<Person> spec ) {
        return spec.And( person => !string.IsNullOrEmpty( person.Email ) );
    }

    public static ISpec<Person> InCity( this ISpec<Person> spec, string city ) {
        return spec.And( person => person.Address.City == city );
    }

    public static ISpec<Person> RegisteredAfter( this ISpec<Person> spec, DateTime date ) {
        return spec.And( person => person.RegistrationDate >= date );
    }
}
```

Agora suas consultas leem como requisitos de negócio:

```csharp
var spec = SpecBuilder<Person>
    .Create()
    .IsActive()
    .IsAdult()
    .HasEmail()
    .InCity( "São Paulo" )
    .RegisteredAfter( DateTime.UtcNow.AddYears( -1 ) )
    .Order( x => x.Name )
    .WithPagination( pagination );

var activeAdultSubscribers = await repository.GetAsync( spec, cancellationToken );
```

## Referência da API

### Construindo Especificações

#### Criando Especificações

```csharp
// Iniciar com especificação vazia
var spec = SpecBuilder<Person>.Create();
```

#### Operadores Lógicos

```csharp
// And - adiciona uma condição de filtro
spec.And( x => x.Age >= 18 );
spec.And( otherSpec );

// AndIf - adiciona condicionalmente um filtro
spec.AndIf( includeInactive, x => x.IsActive == false );

// Or - adiciona uma condição de filtro alternativa
spec.Or( x => x.IsVip );
spec.Or( vipSpec );

// OrIf - adiciona condicionalmente um filtro alternativo
spec.OrIf( allowGuests, x => x.Role == "Guest" );

// Not - nega a especificação atual
spec.And( x => x.Status == "Banned" ).Not();
```

#### Ordenação

```csharp
// Ordenar ascendente
spec.Order( x => x.LastName )
    .Order( x => x.FirstName );

// Ordenar descendente
spec.OrderDescending( x => x.RegistrationDate )
    .OrderDescending( x => x.Score );

// Combinar ascendente e descendente
spec.Order( x => x.Category )
    .OrderDescending( x => x.Priority );
```

#### Paginação

```csharp
// Paginação manual
spec.Skip( 20 ).Take( 10 );

// Usando value object Pagination
using Myth.ValueObjects;

var pagination = new Pagination( pageNumber: 3, pageSize: 20 );
spec.WithPagination( pagination );

// Casos especiais
Pagination.Default;  // Página 1, 10 itens
Pagination.All;      // Sem paginação (retorna todos)

// Combinando métodos de paginação
spec.WithPagination( pagination )
    .Skip( 5 )  // Skip adicional
    .Take( 3 ); // Limite adicional
```

#### Pós-Processamento

```csharp
// Remover duplicados baseado em propriedade
spec.DistinctBy( x => x.Email );
```

### Aplicando Especificações

#### Métodos de Extensão

```csharp
using Myth.Extensions;

IQueryable<Person> people = dbContext.People;

// Aplicar todas as transformações (filtro + ordenação + paginação)
var results = people.Specify( spec );

// Aplicar apenas filtragem
var filtered = people.Filter( spec );

// Aplicar apenas ordenação
var sorted = people.Sort( spec );

// Aplicar apenas paginação/pós-processamento
var paginated = people.Paginate( spec );
```

#### Métodos Diretos

```csharp
// Aplicar todas as transformações
IQueryable<Person> result = spec.Prepare( queryable );

// Aplicar transformações individuais
IQueryable<Person> filtered = spec.Filtered( queryable );
IQueryable<Person> sorted = spec.Sorted( queryable );
IQueryable<Person> processed = spec.Processed( queryable );

// Obter resultados diretamente
IQueryable<Person> items = spec.SatisfyingItemsFrom( queryable );
Person? singleItem = spec.SatisfyingItemFrom( queryable );
```

#### Validação

```csharp
// Verificar se uma entidade satisfaz a especificação
Person person = GetPerson();
bool isValid = spec.IsSatisfiedBy( person );
```

### Propriedades de Especificação

```csharp
// Acessar a expressão subjacente
Expression<Func<T, bool>> predicate = spec.Predicate;
Func<T, bool> query = spec.Query;

// Acessar funções de transformação
Func<IQueryable<T>, IOrderedQueryable<T>> sortFunc = spec.Sort;
Func<IQueryable<T>, IQueryable<T>> postProcessFunc = spec.PostProcess;

// Acessar rastreamento de paginação
int itemsSkipped = spec.ItemsSkiped;
int itemsTaken = spec.ItemsTaked;
```

## Integração com Padrão Repository

Myth.Specification funciona perfeitamente com padrões de repositório:

```csharp
public interface IPersonRepository {
    Task<IEnumerable<Person>> GetAsync( ISpec<Person> specification, CancellationToken cancellationToken = default );
    Task<Person?> GetSingleAsync( ISpec<Person> specification, CancellationToken cancellationToken = default );
    Task<int> CountAsync( ISpec<Person> specification, CancellationToken cancellationToken = default );
}

public class PersonRepository : IPersonRepository {
    private readonly DbContext _context;

    public async Task<IEnumerable<Person>> GetAsync( ISpec<Person> specification, CancellationToken cancellationToken = default ) {
        return await _context.People
            .Specify( specification )
            .ToListAsync( cancellationToken );
    }

    public async Task<Person?> GetSingleAsync( ISpec<Person> specification, CancellationToken cancellationToken = default ) {
        return specification.SatisfyingItemFrom( _context.People.AsQueryable() );
    }

    public async Task<int> CountAsync( ISpec<Person> specification, CancellationToken cancellationToken = default ) {
        return await _context.People
            .Filter( specification )
            .CountAsync( cancellationToken );
    }
}
```

## Exemplos do Mundo Real

### Busca de Produtos E-commerce

```csharp
public static class ProductSpecifications {
    public static ISpec<Product> InStock( this ISpec<Product> spec ) {
        return spec.And( p => p.StockQuantity > 0 );
    }

    public static ISpec<Product> InCategory( this ISpec<Product> spec, string category ) {
        return spec.And( p => p.Category == category );
    }

    public static ISpec<Product> InPriceRange( this ISpec<Product> spec, decimal min, decimal max ) {
        return spec.And( p => p.Price >= min && p.Price <= max );
    }

    public static ISpec<Product> WithRatingAbove( this ISpec<Product> spec, double rating ) {
        return spec.And( p => p.AverageRating >= rating );
    }

    public static ISpec<Product> OnSale( this ISpec<Product> spec ) {
        return spec.And( p => p.DiscountPercentage > 0 );
    }
}

// Em um serviço
public async Task<ProductSearchResult> SearchProductsAsync( ProductSearchRequest request ) {
    var spec = SpecBuilder<Product>
        .Create()
        .InStock()
        .AndIf( !string.IsNullOrEmpty( request.Category ), s => s.InCategory( request.Category ) )
        .AndIf( request.MinPrice.HasValue && request.MaxPrice.HasValue,
                s => s.InPriceRange( request.MinPrice.Value, request.MaxPrice.Value ) )
        .AndIf( request.MinRating.HasValue, s => s.WithRatingAbove( request.MinRating.Value ) )
        .AndIf( request.OnSaleOnly, s => s.OnSale() )
        .OrderDescending( p => p.AverageRating )
        .WithPagination( request.Pagination );

    var products = await _productRepository.GetAsync( spec );

    return new ProductSearchResult { Products = products };
}
```

### Gerenciamento de Usuários

```csharp
public static class UserSpecifications {
    public static ISpec<User> IsVerified( this ISpec<User> spec ) {
        return spec.And( u => u.EmailVerified );
    }

    public static ISpec<User> HasRole( this ISpec<User> spec, string role ) {
        return spec.And( u => u.Roles.Contains( role ) );
    }

    public static ISpec<User> LastLoginAfter( this ISpec<User> spec, DateTime date ) {
        return spec.And( u => u.LastLoginDate >= date );
    }

    public static ISpec<User> IsActive( this ISpec<User> spec ) {
        return spec.And( u => !u.IsDeleted && !u.IsSuspended );
    }
}

// Encontrar usuários inativos para limpeza
var inactiveUsers = SpecBuilder<User>
    .Create()
    .IsActive()
    .LastLoginAfter( DateTime.UtcNow.AddMonths( -6 ) )
    .Not();  // Negar para encontrar usuários inativos
```

### Filtragem de Log de Auditoria

```csharp
public static class AuditLogSpecifications {
    public static ISpec<AuditLog> ByUser( this ISpec<AuditLog> spec, Guid userId ) {
        return spec.And( log => log.UserId == userId );
    }

    public static ISpec<AuditLog> ByAction( this ISpec<AuditLog> spec, string action ) {
        return spec.And( log => log.Action == action );
    }

    public static ISpec<AuditLog> InDateRange( this ISpec<AuditLog> spec, DateTime start, DateTime end ) {
        return spec.And( log => log.Timestamp >= start && log.Timestamp <= end );
    }

    public static ISpec<AuditLog> WithErrors( this ISpec<AuditLog> spec ) {
        return spec.And( log => !log.Success );
    }
}

// Consultar logs de auditoria
var spec = SpecBuilder<AuditLog>
    .Create()
    .ByUser( userId )
    .InDateRange( startDate, endDate )
    .OrIf( includeErrors, s => s.WithErrors() )
    .OrderDescending( log => log.Timestamp )
    .WithPagination( pagination );
```

## Uso Avançado

### Composição de Especificações

```csharp
// Criar especificações base
var activeUsersSpec = SpecBuilder<User>
    .Create()
    .IsActive()
    .IsVerified();

var adminSpec = SpecBuilder<User>
    .Create()
    .HasRole( "Admin" );

// Compor especificações
var activeAdmins = activeUsersSpec.And( adminSpec );

// Ou combinar com filtros adicionais
var recentActiveAdmins = activeAdmins
    .LastLoginAfter( DateTime.UtcNow.AddDays( -7 ) )
    .Order( u => u.LastLoginDate );
```

### Construção Dinâmica de Especificações

```csharp
public ISpec<Product> BuildProductSpec( ProductFilter filter ) {
    var spec = SpecBuilder<Product>.Create();

    if ( !string.IsNullOrEmpty( filter.SearchTerm ) ) {
        spec = spec.And( p => p.Name.Contains( filter.SearchTerm ) ||
                              p.Description.Contains( filter.SearchTerm ) );
    }

    if ( filter.Categories?.Any() == true ) {
        spec = spec.And( p => filter.Categories.Contains( p.Category ) );
    }

    if ( filter.MinPrice.HasValue ) {
        spec = spec.And( p => p.Price >= filter.MinPrice.Value );
    }

    if ( filter.MaxPrice.HasValue ) {
        spec = spec.And( p => p.Price <= filter.MaxPrice.Value );
    }

    return spec
        .Order( p => p.Name )
        .WithPagination( filter.Pagination );
}
```

### Testando Especificações

```csharp
[Fact]
public void ActiveAdultSpec_Should_Filter_Correctly() {
    // Arrange
    var person1 = new Person { Age = 25, IsActive = true };
    var person2 = new Person { Age = 17, IsActive = true };
    var person3 = new Person { Age = 30, IsActive = false };

    var spec = SpecBuilder<Person>
        .Create()
        .IsActive()
        .IsAdult();

    // Assert
    spec.IsSatisfiedBy( person1 ).Should().BeTrue();
    spec.IsSatisfiedBy( person2 ).Should().BeFalse();  // Não é adulto
    spec.IsSatisfiedBy( person3 ).Should().BeFalse();  // Não está ativo
}
```

## Melhores Práticas

### 1. Criar Métodos de Extensão de Especificação

Sempre encapsule lógica de negócio em métodos de extensão:

```csharp
// Bom
public static ISpec<Order> IsPending( this ISpec<Order> spec ) {
    return spec.And( o => o.Status == OrderStatus.Pending );
}

// Evitar
var spec = SpecBuilder<Order>.Create().And( o => o.Status == OrderStatus.Pending );
```

### 2. Usar Nomes Descritivos

Nomeie especificações de acordo com conceitos de negócio, não operações técnicas:

```csharp
// Bom
.IsEligibleForDiscount()
.RequiresApproval()
.HasExpired()

// Evitar
.CheckStatus()
.FilterByDate()
.ValidateField()
```

### 3. Manter Especificações Focadas

Cada método de especificação deve representar uma regra de negócio:

```csharp
// Bom
.IsActive()
.IsVerified()
.HasCompletedProfile()

// Evitar
.IsActiveAndVerifiedWithProfile()
```

### 4. Usar Especificações Condicionais

Aproveite `AndIf` e `OrIf` para filtros opcionais:

```csharp
var spec = SpecBuilder<Product>
    .Create()
    .InStock()
    .AndIf( !string.IsNullOrEmpty( category ), s => s.InCategory( category ) )
    .AndIf( minPrice.HasValue, s => s.MinimumPrice( minPrice.Value ) );
```

### 5. Compor Especificações para Reutilização

Construa especificações complexas a partir de simples:

```csharp
public static ISpec<User> IsActiveSubscriber( this ISpec<User> spec ) {
    return spec
        .IsActive()
        .IsVerified()
        .HasActiveSubscription();
}
```

## Considerações de Performance

- **Especificações geram Expression Trees**: Todas as operações de filtro compilam para SQL quando usadas com Entity Framework
- **Avaliação Lazy**: Especificações não são executadas até serem enumeradas
- **Ordenação Eficiente**: Múltiplas operações de ordenação são encadeadas eficientemente
- **Suporte a Paginação**: Operações Skip/Take traduzem diretamente para SQL OFFSET/FETCH

## Integração com Outras Bibliotecas Myth

### Myth.Repository

```csharp
public interface IRepository<T> {
    Task<IEnumerable<T>> FindAsync( ISpec<T> specification, CancellationToken cancellationToken = default );
}
```

### Myth.Flow

```csharp
var result = await Pipeline.Start( searchRequest )
    .Step( ctx => BuildSpecification( ctx ) )
    .StepAsync( async ( ctx, spec ) => await repository.GetAsync( spec ) )
    .ExecuteAsync();
```

### Myth.Guard

```csharp
builder.For( request.Pagination, x => x
    .Respect( p => p.PageNumber > 0 )
    .WithMessage( "Número da página deve ser positivo" ) );
```

## Contribuindo

Contribuições são bem-vindas! Por favor, leia as diretrizes de contribuição antes de enviar pull requests.

## Licença

Este projeto está licenciado sob a Apache License 2.0 - veja o arquivo LICENSE para detalhes.
