<img  style="float: right;" src="myth-repository-logo.png" alt="drawing" width="250"/>

# Myth.Repository

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Repository?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Repository/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Repository?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Repository/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

**Myth.Repository** é uma biblioteca .NET que fornece um conjunto limpo e padronizado de interfaces de repositório para acesso a dados. Promove separação de responsabilidades através da segregação leitura/escrita, integra perfeitamente com o padrão Specification, e suporta paginação nativa. Perfeita para construir aplicações maintíveis, testáveis e orientadas a domínio.

## Funcionalidades

- **Interfaces de Repositório**: Contratos padronizados para operações de acesso a dados
- **Segregação Leitura/Escrita**: Interfaces separadas para operações de leitura e escrita (compatível com CQRS)
- **Integração com Padrão Specification**: Suporte de primeira classe para `ISpec<T>` do Myth.Specification
- **Suporte a Expressões**: Funciona com specifications e expressões LINQ
- **Paginação**: Suporte nativo a paginação com resultados `IPaginated<T>`
- **Async-First**: Todas as operações são assíncronas com suporte a cancellation token
- **Operações de Consulta**: Capacidades ricas de consulta (Where, First, Last, Any, All, Count)
- **Operações em Lote**: Suporte para inserções, atualizações e exclusões em massa
- **Suporte IQueryable**: Acesso ao queryable subjacente para cenários avançados
- **IAsyncDisposable**: Limpeza adequada de recursos com disposal assíncrono

## Instalação

```bash
dotnet add package Myth.Repository
```

Para implementação Entity Framework Core:

```bash
dotnet add package Myth.Repository.EntityFramework
```

## Início Rápido

### Defina Sua Entidade

```csharp
public class Product {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public string Category { get; set; }
}
```

### Crie Interface do Repositório

```csharp
using Myth.Interfaces.Repositories.Base;

public interface IProductRepository : IReadWriteRepositoryAsync<Product> {
    // Adicione métodos customizados se necessário
}
```

### Implemente Repositório (Exemplo Entity Framework)

```csharp
using Myth.Repository.EntityFramework.Repositories;

public class ProductRepository : ReadWriteRepositoryAsync<Product>, IProductRepository {
    public ProductRepository( DbContext context ) : base( context ) {
    }
}
```

### Use na Sua Aplicação

```csharp
public class ProductService {
    private readonly IProductRepository _repository;

    public ProductService( IProductRepository repository ) {
        _repository = repository;
    }

    public async Task<Product?> GetProductByIdAsync( Guid id, CancellationToken cancellationToken ) {
        return await _repository.FirstOrDefaultAsync( p => p.Id == id, cancellationToken );
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync( CancellationToken cancellationToken ) {
        return await _repository.SearchAsync( p => p.IsActive, cancellationToken );
    }

    public async Task CreateProductAsync( Product product, CancellationToken cancellationToken ) {
        await _repository.AddAsync( product, cancellationToken );
    }
}
```

## Interfaces Principais

### IRepository

Interface marcadora base para todos os tipos de repositório.

```csharp
public interface IRepository { }
```

### IReadRepositoryAsync<TEntity>

Fornece operações somente leitura para consulta de dados.

```csharp
public interface IReadRepositoryAsync<TEntity> : IRepository, IAsyncDisposable {
    // Acesso queryable
    IQueryable<TEntity> Where( ISpec<TEntity> specification );
    IQueryable<TEntity> Where( Expression<Func<TEntity, bool>> predicate );
    IQueryable<TEntity> AsQueryable( );
    IEnumerable<TEntity> AsEnumerable( );

    // Operações de pesquisa
    Task<IEnumerable<TEntity>> SearchAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<IEnumerable<TEntity>> SearchAsync( Expression<Func<TEntity, bool>> filterPredicate, Expression<Func<TEntity, bool>>? orderPredicate = null, CancellationToken cancellationToken = default );

    // Pesquisa paginada
    Task<IPaginated<TEntity>> SearchPaginatedAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<IPaginated<TEntity>> SearchPaginatedAsync( Expression<Func<TEntity, bool>> filterPredicate, Pagination pagination, Expression<Func<TEntity, bool>>? orderPredicate = null, CancellationToken cancellationToken = default );

    // Recuperação de item único
    Task<TEntity?> FirstOrDefaultAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<TEntity?> FirstOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );
    Task<TEntity> FirstAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<TEntity> FirstAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

    Task<TEntity?> LastOrDefaultAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<TEntity?> LastOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );
    Task<TEntity> LastAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<TEntity> LastAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

    // Operações de agregação
    Task<int> CountAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<int> CountAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

    Task<bool> AnyAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<bool> AnyAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

    Task<bool> AllAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
    Task<bool> AllAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

    // Obter todos
    Task<IEnumerable<TEntity>> ToListAsync( CancellationToken cancellationToken = default );
}
```

### IWriteRepositoryAsync<TEntity>

Fornece operações de escrita para modificação de dados.

```csharp
public interface IWriteRepositoryAsync<TEntity> : IRepository, IAsyncDisposable {
    // Operações únicas
    Task AddAsync( TEntity entity, CancellationToken cancellationToken = default );
    Task UpdateAsync( TEntity entity, CancellationToken cancellationToken = default );
    Task RemoveAsync( TEntity entity, CancellationToken cancellationToken = default );

    // Operações em lote
    Task AddRangeAsync( IEnumerable<TEntity> entity, CancellationToken cancellationToken = default );
    Task UpdateRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );
    Task RemoveRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );
}
```

### IReadWriteRepositoryAsync<TEntity>

Combina operações de leitura e escrita em uma única interface.

```csharp
public interface IReadWriteRepositoryAsync<TEntity> : IReadRepositoryAsync<TEntity>, IWriteRepositoryAsync<TEntity>, IAsyncDisposable { }
```

## Suporte à Paginação

### IPaginated<T>

Interface representando resultados paginados:

```csharp
public interface IPaginated<T> {
    int PageNumber { get; }       // Número da página atual (baseado em 1)
    int PageSize { get; }          // Número de itens por página
    int TotalPages { get; }        // Número total de páginas
    int TotalItems { get; }        // Número total de itens em todas as páginas
    IEnumerable<T> Items { get; }  // Itens na página atual
}
```

### Objeto de Valor Pagination

```csharp
public class Pagination {
    public int PageNumber { get; set; }  // Padrão: 1
    public int PageSize { get; set; }     // Padrão: 10

    // Helpers estáticos
    public static readonly Pagination Default = new( 1, 10 );
    public static readonly Pagination All = new( -1, -1 );
}
```

### Método de Extensão

```csharp
using Myth.Extensions;

// Converter qualquer IEnumerable para resultado paginado
var items = new List<Product> { /* ... */ };
var paginated = items.AsPaginated( pageSize: 20, skip: 0 );

// Ou com objeto Pagination
var pagination = new Pagination( pageNumber: 2, pageSize: 20 );
var paginated = items.AsPaginated( pagination );
```

## Exemplos de Uso

### Operações CRUD Básicas

```csharp
public class ProductService {
    private readonly IProductRepository _repository;

    public ProductService( IProductRepository repository ) {
        _repository = repository;
    }

    public async Task CreateAsync( Product product, CancellationToken ct ) {
        await _repository.AddAsync( product, ct );
    }

    public async Task CreateManyAsync( IEnumerable<Product> products, CancellationToken ct ) {
        await _repository.AddRangeAsync( products, ct );
    }

    public async Task UpdateAsync( Product product, CancellationToken ct ) {
        await _repository.UpdateAsync( product, ct );
    }

    public async Task DeleteAsync( Product product, CancellationToken ct ) {
        await _repository.RemoveAsync( product, ct );
    }
}
```

### Consultando com Expressões

```csharp
public class ProductService {
    private readonly IProductRepository _repository;

    public async Task<Product?> GetByIdAsync( Guid id, CancellationToken ct ) {
        return await _repository.FirstOrDefaultAsync( p => p.Id == id, ct );
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync( string category, CancellationToken ct ) {
        return await _repository.SearchAsync( p => p.Category == category, ct );
    }

    public async Task<int> CountActiveProductsAsync( CancellationToken ct ) {
        return await _repository.CountAsync( p => p.IsActive, ct );
    }

    public async Task<bool> HasExpensiveProductsAsync( CancellationToken ct ) {
        return await _repository.AnyAsync( p => p.Price > 1000, ct );
    }

    public async Task<bool> AreAllActiveAsync( CancellationToken ct ) {
        return await _repository.AllAsync( p => p.IsActive, ct );
    }
}
```

### Consultando com Specifications

```csharp
using Myth.Specification;

// Definir specifications
public static class ProductSpecifications {
    public static ISpec<Product> IsActive( this ISpec<Product> spec ) {
        return spec.And( p => p.IsActive );
    }

    public static ISpec<Product> InCategory( this ISpec<Product> spec, string category ) {
        return spec.And( p => p.Category == category );
    }

    public static ISpec<Product> PriceRange( this ISpec<Product> spec, decimal min, decimal max ) {
        return spec.And( p => p.Price >= min && p.Price <= max );
    }

    public static ISpec<Product> OrderByName( this ISpec<Product> spec ) {
        return spec.Order( p => p.Name );
    }
}

// Usar no repositório
public class ProductService {
    private readonly IProductRepository _repository;

    public async Task<IEnumerable<Product>> GetActiveProductsInCategoryAsync(
        string category,
        CancellationToken ct ) {

        var spec = SpecBuilder<Product>.Create( )
            .IsActive( )
            .InCategory( category )
            .OrderByName( );

        return await _repository.SearchAsync( spec, ct );
    }

    public async Task<IEnumerable<Product>> GetAffordableProductsAsync(
        decimal maxPrice,
        CancellationToken ct ) {

        var spec = SpecBuilder<Product>.Create( )
            .IsActive( )
            .PriceRange( 0, maxPrice )
            .OrderByName( );

        return await _repository.SearchAsync( spec, ct );
    }
}
```

### Paginação

```csharp
public class ProductService {
    private readonly IProductRepository _repository;

    // Paginar com specification
    public async Task<IPaginated<Product>> GetProductsPageAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct ) {

        var pagination = new Pagination( pageNumber, pageSize );

        var spec = SpecBuilder<Product>.Create( )
            .IsActive( )
            .OrderByName( )
            .Skip( (pageNumber - 1) * pageSize )
            .Take( pageSize );

        return await _repository.SearchPaginatedAsync( spec, ct );
    }

    // Paginar com expressão
    public async Task<IPaginated<Product>> GetProductsByCategoryPageAsync(
        string category,
        Pagination pagination,
        CancellationToken ct ) {

        return await _repository.SearchPaginatedAsync(
            filterPredicate: p => p.Category == category && p.IsActive,
            pagination: pagination,
            orderPredicate: p => p.Name,
            cancellationToken: ct );
    }

    // Usar resultados paginados
    public async Task DisplayProductsAsync( CancellationToken ct ) {
        var result = await GetProductsPageAsync( pageNumber: 1, pageSize: 20, ct );

        Console.WriteLine( $"Página {result.PageNumber} de {result.TotalPages}" );
        Console.WriteLine( $"Total de itens: {result.TotalItems}" );

        foreach ( var product in result.Items ) {
            Console.WriteLine( $"- {product.Name}: R${product.Price}" );
        }
    }
}
```

### Consulta Avançada com IQueryable

```csharp
public class ProductService {
    private readonly IProductRepository _repository;

    public async Task<IEnumerable<ProductSummary>> GetProductSummariesAsync( CancellationToken ct ) {
        var queryable = _repository
            .AsQueryable( )
            .Where( p => p.IsActive )
            .GroupBy( p => p.Category )
            .Select( g => new ProductSummary {
                Category = g.Key,
                Count = g.Count( ),
                AveragePrice = g.Average( p => p.Price )
            } );

        return await queryable.ToListAsync( ct );
    }
}
```

### Segregação Leitura/Escrita (CQRS)

```csharp
// Serviço somente leitura
public class ProductQueryService {
    private readonly IReadRepositoryAsync<Product> _repository;

    public ProductQueryService( IReadRepositoryAsync<Product> repository ) {
        _repository = repository;
    }

    public async Task<IEnumerable<Product>> GetAllAsync( CancellationToken ct ) {
        return await _repository.ToListAsync( ct );
    }
}

// Serviço somente escrita
public class ProductCommandService {
    private readonly IWriteRepositoryAsync<Product> _repository;

    public ProductCommandService( IWriteRepositoryAsync<Product> repository ) {
        _repository = repository;
    }

    public async Task CreateAsync( Product product, CancellationToken ct ) {
        await _repository.AddAsync( product, ct );
    }
}
```

## Integração com Outras Bibliotecas Myth

### Com Myth.Specification

```csharp
var spec = SpecBuilder<Product>.Create( )
    .IsActive( )
    .InCategory( "Eletrônicos" )
    .PriceRange( 100, 500 )
    .OrderByName( )
    .Take( 50 );

var products = await _repository.SearchAsync( spec, cancellationToken );
```

### Com Myth.Flow.Actions (CQRS)

```csharp
public class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, IEnumerable<Product>> {
    private readonly IReadRepositoryAsync<Product> _repository;

    public async Task<QueryResult<IEnumerable<Product>>> HandleAsync(
        GetProductsQuery query,
        CancellationToken cancellationToken ) {

        var products = await _repository.SearchAsync(
            p => p.Category == query.Category,
            cancellationToken );

        return QueryResult<IEnumerable<Product>>.Success( products );
    }
}
```

### Com Myth.Guard (Validação)

```csharp
public class ProductCommandService {
    private readonly IWriteRepositoryAsync<Product> _repository;
    private readonly IValidator _validator;

    public async Task CreateAsync( Product product, CancellationToken ct ) {
        await _validator.ValidateAsync( product, ValidationContextKey.Create, ct );
        await _repository.AddAsync( product, ct );
    }
}
```

## Padrões Arquiteturais

### Domain-Driven Design (DDD)

```csharp
// Raiz de Agregado
public class Order : IAggregateRoot {
    public Guid Id { get; private set; }
    private List<OrderItem> _items = new( );
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly( );

    public void AddItem( Product product, int quantity ) {
        _items.Add( new OrderItem( product, quantity ) );
    }
}

// Repositório na camada de domínio (interface)
public interface IOrderRepository : IReadWriteRepositoryAsync<Order> {
    Task<Order?> GetByIdWithItemsAsync( Guid id, CancellationToken ct );
}

// Implementação na camada de infraestrutura
public class OrderRepository : ReadWriteRepositoryAsync<Order>, IOrderRepository {
    public OrderRepository( DbContext context ) : base( context ) { }

    public async Task<Order?> GetByIdWithItemsAsync( Guid id, CancellationToken ct ) {
        return await AsQueryable( )
            .Include( o => o.Items )
            .FirstOrDefaultAsync( o => o.Id == id, ct );
    }
}
```

### Padrão Unit of Work

Veja `Myth.Repository.EntityFramework` para implementação `IUnitOfWorkRepository`.

```csharp
public class OrderService {
    private readonly IUnitOfWorkRepository _unitOfWork;

    public async Task ProcessOrderAsync( Order order, CancellationToken ct ) {
        await _unitOfWork.BeginTransactionAsync( ct );

        try {
            await _unitOfWork.AddAsync( order, ct );
            await _unitOfWork.SaveChangesAsync( ct );
            await _unitOfWork.CommitTransactionAsync( ct );
        }
        catch {
            await _unitOfWork.RollbackTransactionAsync( ct );
            throw;
        }
    }
}
```

## Melhores Práticas

1. **Use Specifications para Consultas Complexas**: Encapsule regras de negócio em extensões de specification
2. **Aproveite a Segregação Leitura/Escrita**: Use interfaces separadas quando seguir padrões CQRS
3. **Sempre Use Cancellation Tokens**: Suporte cancelamento gracioso em operações de longa duração
4. **Descarte Repositórios Adequadamente**: Use `await using` ou injeção de dependência para disposal automático
5. **Pagine Conjuntos de Resultados Grandes**: Use `SearchPaginatedAsync` para evitar carregar muitos dados
6. **Mantenha Repositórios Enxutos**: Mova lógica de negócio para serviços ou entidades de domínio
7. **Use IQueryable com Moderação**: Prefira specifications e expressões para melhor testabilidade
8. **Implemente Métodos Customizados**: Adicione métodos específicos do domínio às interfaces de repositório quando necessário

## Testes

```csharp
public class ProductServiceTests {
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly ProductService _service;

    public ProductServiceTests( ) {
        _mockRepository = new Mock<IProductRepository>( );
        _service = new ProductService( _mockRepository.Object );
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenExists( ) {
        var productId = Guid.NewGuid( );
        var product = new Product { Id = productId, Name = "Teste" };

        _mockRepository
            .Setup( r => r.FirstOrDefaultAsync( It.IsAny<Expression<Func<Product, bool>>>( ), default ) )
            .ReturnsAsync( product );

        var result = await _service.GetByIdAsync( productId, default );

        result.Should( ).NotBeNull( );
        result.Id.Should( ).Be( productId );
    }
}
```

## Pacotes Relacionados

- **Myth.Repository.EntityFramework**: Implementação Entity Framework Core com suporte Unit of Work
- **Myth.Specification**: Construa specifications de consulta complexas e reutilizáveis
- **Myth.Commons**: Objetos de valor e suporte à paginação (IPaginated<T>)

## Licença

Este projeto está licenciado sob a Licença Apache 2.0 - veja a [LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para detalhes.
