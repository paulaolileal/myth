# Myth.Repository.EntityFramework

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Repository.EntityFramework?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Repository.EntityFramework/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Repository.EntityFramework?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Repository.EntityFramework/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

Uma biblioteca .NET abrangente que fornece implementações robustas e ricas em recursos de repositórios para Entity Framework Core com padrões Domain-Driven Design (DDD), suporte ao padrão Specification e injeção de dependência automática.

## Índice
- [Funcionalidades](#-funcionalidades)
- [Instalação](#-instalação)
- [Início Rápido](#-início-rápido)
- [Conceitos Centrais](#-conceitos-centrais)
- [Tipos de Repositório](#-tipos-de-repositório)
- [Uso Avançado](#-uso-avançado)
- [Padrão Unit of Work](#-padrão-unit-of-work)
- [Injeção de Dependência](#-injeção-de-dependência)

## ⭐ Funcionalidades

- **BaseContext**: DbContext abstrato com descoberta automática de configuração de entidades
- **Implementações de Repositório**:
  - `ReadRepositoryAsync<TEntity>`: Operações somente leitura
  - `WriteRepositoryAsync<TEntity>`: Operações de escrita (Add, Update, Remove)
  - `ReadWriteRepositoryAsync<TEntity>`: Operações combinadas de leitura/escrita
- **Suporte ao Padrão Specification**: Integração perfeita com Myth.Specification
- **Consultas Baseadas em Expressões**: Suporte direto a expressões LINQ
- **Padrão Unit of Work**: Gerenciamento de transações com savepoints
- **Injeção de Dependência Automática**: Registro de repositórios sem configuração
- **Suporte à Paginação**: Resultados de consulta paginados integrados
- **Async/Await**: API totalmente assíncrona
- **Attach/Detach de Entidades**: Gerenciamento refinado do estado de entidades

## 📦 Instalação

Instale via NuGet Package Manager:

```bash
dotnet add package Myth.Repository.EntityFramework
```

Ou via Package Manager Console:

```powershell
Install-Package Myth.Repository.EntityFramework
```

## 🚀 Início Rápido

### 1. Crie Seu DbContext

```csharp
using Myth.Contexts;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : BaseContext {
    public ApplicationDbContext( DbContextOptions<ApplicationDbContext> options ) : base( options ) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Product> Products { get; set; }
}
```

O `BaseContext` descobre e aplica automaticamente todas as implementações de `IEntityTypeConfiguration<T>` no seu assembly.

### 2. Crie Interface do Repositório

```csharp
using Myth.Interfaces.Repositories.EntityFramework;

public interface IUserRepository : IReadWriteRepositoryAsync<User> {
    // Adicione métodos customizados se necessário
    Task<User?> GetByEmailAsync( string email, CancellationToken cancellationToken = default );
}
```

### 3. Implemente Repositório

```csharp
using Myth.Repositories.EntityFramework;

public class UserRepository : ReadWriteRepositoryAsync<User>, IUserRepository {

    public UserRepository( ApplicationDbContext context ) : base( context ) { }

    public async Task<User?> GetByEmailAsync( string email, CancellationToken cancellationToken = default ) {
        return await FirstOrDefaultAsync( u => u.Email == email, cancellationToken );
    }
}
```

### 4. Registre no Startup

```csharp
var builder = WebApplication.CreateBuilder( args );

// Registrar DbContext
builder.Services.AddDbContext<ApplicationDbContext>( options =>
    options.UseSqlServer( builder.Configuration.GetConnectionString( "DefaultConnection" ) ) );

// Registrar automaticamente TODOS os repositórios
builder.Services.AddRepositories( );

var app = builder.BuildApp( ); // Use BuildApp() para resolução de dependência entre bibliotecas
app.Run( );
```

### 5. Use nos Seus Serviços

```csharp
public class UserService {
    private readonly IUserRepository _userRepository;

    public UserService( IUserRepository userRepository ) {
        _userRepository = userRepository;
    }

    public async Task<User?> GetUserByEmailAsync( string email ) {
        return await _userRepository.GetByEmailAsync( email );
    }

    public async Task CreateUserAsync( User user ) {
        await _userRepository.AddAsync( user );
        await _userRepository.SaveChangesAsync( );
    }
}
```

## 🎯 Conceitos Centrais

### BaseContext

O `BaseContext` é uma classe base abstrata que estende `DbContext` e fornece configuração automática de entidades:

```csharp
public abstract class BaseContext : DbContext {
    protected override void OnModelCreating( ModelBuilder modelBuilder ) {
        // Aplica automaticamente todas as IEntityTypeConfiguration<T> do assembly
        modelBuilder.ApplyConfigurationsFromAssembly( GetType( ).Assembly );
    }
}
```

**Benefícios:**
- Sem registro manual de configuração
- Descoberta de mapeamento de entidades baseada em convenção
- Código mais limpo e manutenível

## 📂 Tipos de Repositório

### IReadRepositoryAsync<TEntity>

Repositório somente leitura para operações de consulta.

**Métodos Principais:**

```csharp
// Consultas básicas
Task<IEnumerable<TEntity>> ToListAsync( CancellationToken cancellationToken = default );
IQueryable<TEntity> AsQueryable( );
IEnumerable<TEntity> AsEnumerable( );

// Consultas filtradas
IQueryable<TEntity> Where( Expression<Func<TEntity, bool>> predicate );
Task<IEnumerable<TEntity>> SearchAsync( Expression<Func<TEntity, bool>> filter, Expression<Func<TEntity, bool>>? orderBy = null, CancellationToken cancellationToken = default );

// Paginação
Task<IPaginated<TEntity>> SearchPaginatedAsync( Expression<Func<TEntity, bool>> predicate, int take = 0, int skip = 0, Expression<Func<TEntity, bool>>? orderBy = null, CancellationToken cancellationToken = default );
Task<IPaginated<TEntity>> SearchPaginatedAsync( Expression<Func<TEntity, bool>> predicate, Pagination pagination, Expression<Func<TEntity, bool>>? orderBy = null, CancellationToken cancellationToken = default );

// Recuperação de item único
Task<TEntity?> FirstOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );
Task<TEntity?> LastOrDefaultAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );
Task<TEntity> FirstAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );
Task<TEntity> LastAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

// Operações de agregação
Task<int> CountAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );
Task<bool> AnyAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );
Task<bool> AllAsync( Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default );

// Metadados
string? GetProviderName( );
```

**Suporte ao Padrão Specification:**

Todos os métodos têm sobrecargas baseadas em specifications:

```csharp
Task<IEnumerable<TEntity>> SearchAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
Task<IPaginated<TEntity>> SearchPaginatedAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
IQueryable<TEntity> Where( ISpec<TEntity> specification );
Task<int> CountAsync( ISpec<TEntity> specification, CancellationToken cancellationToken = default );
// ... e mais
```

### IWriteRepositoryAsync<TEntity>

Operações de escrita para criar, atualizar e excluir entidades.

**Métodos:**

```csharp
// Criar
Task AddAsync( TEntity entity, CancellationToken cancellationToken = default );
Task AddRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );

// Atualizar
Task UpdateAsync( TEntity entity, CancellationToken cancellationToken = default );
Task UpdateRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );

// Excluir
Task RemoveAsync( TEntity entity, CancellationToken cancellationToken = default );
Task RemoveRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );

// Gerenciamento de estado de entidade
Task AttachAsync( TEntity entity, CancellationToken cancellationToken = default );
Task AttachRangeAsync( IEnumerable<TEntity> entities, CancellationToken cancellationToken = default );
```

### IReadWriteRepositoryAsync<TEntity>

Combina operações de leitura e escrita. Este é o tipo de repositório mais comumente usado.

**Uso:**

```csharp
public class ProductRepository : ReadWriteRepositoryAsync<Product>, IProductRepository {
    public ProductRepository( ApplicationDbContext context ) : base( context ) { }
}
```

## 🔧 Uso Avançado

### Trabalhando com Specifications

A biblioteca integra perfeitamente com `Myth.Specification`:

```csharp
using Myth.Specification;

// Definir specifications
public static class UserSpecifications {
    public static ISpec<User> IsActive( this ISpec<User> spec ) {
        return spec.And( u => u.IsActive );
    }

    public static ISpec<User> HasRole( this ISpec<User> spec, string role ) {
        return spec.And( u => u.Role == role );
    }
}

// Usar no repositório
public class UserRepository : ReadWriteRepositoryAsync<User>, IUserRepository {
    public UserRepository( ApplicationDbContext context ) : base( context ) { }

    public async Task<IEnumerable<User>> GetActiveAdminsAsync( ) {
        var spec = SpecBuilder<User>.Create( )
            .IsActive( )
            .HasRole( "Admin" )
            .Order( u => u.Name );

        return await SearchAsync( spec );
    }

    public async Task<IPaginated<User>> GetActiveUsersPaginatedAsync( int page, int pageSize ) {
        var spec = SpecBuilder<User>.Create( )
            .IsActive( )
            .Order( u => u.CreatedAt )
            .Skip( ( page - 1 ) * pageSize )
            .Take( pageSize );

        return await SearchPaginatedAsync( spec );
    }
}
```

### Exemplos de Paginação

**Usando Paginação Baseada em Expressões:**

```csharp
public async Task<IPaginated<Product>> GetProductsAsync( int page, int pageSize ) {
    var skip = ( page - 1 ) * pageSize;
    return await _productRepository.SearchPaginatedAsync(
        predicate: p => p.IsAvailable,
        take: pageSize,
        skip: skip,
        orderPredicate: p => p.Price
    );
}
```

**Usando Objeto Pagination:**

```csharp
public async Task<IPaginated<Product>> GetProductsAsync( Pagination pagination ) {
    return await _productRepository.SearchPaginatedAsync(
        predicate: p => p.IsAvailable,
        pagination: pagination,
        orderPredicate: p => p.Name
    );
}
```

**Estrutura do Resultado:**

```csharp
public interface IPaginated<T> {
    IEnumerable<T> Items { get; }
    int TotalItems { get; }
    int PageSize { get; }
    int CurrentPage { get; }
    int TotalPages { get; }
    bool HasPrevious { get; }
    bool HasNext { get; }
}
```

### Métodos de Repositório Customizados

Estenda repositórios base com métodos específicos do domínio:

```csharp
public interface IOrderRepository : IReadWriteRepositoryAsync<Order> {
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync( Guid customerId, CancellationToken cancellationToken = default );
    Task<Order?> GetOrderWithItemsAsync( Guid orderId, CancellationToken cancellationToken = default );
    Task<decimal> GetTotalRevenueAsync( DateTime from, DateTime to, CancellationToken cancellationToken = default );
}

public class OrderRepository : ReadWriteRepositoryAsync<Order>, IOrderRepository {

    public OrderRepository( ApplicationDbContext context ) : base( context ) { }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync( Guid customerId, CancellationToken cancellationToken = default ) {
        return await SearchAsync( o => o.CustomerId == customerId, o => o.OrderDate, cancellationToken );
    }

    public async Task<Order?> GetOrderWithItemsAsync( Guid orderId, CancellationToken cancellationToken = default ) {
        return await AsQueryable( )
            .Include( o => o.OrderItems )
            .ThenInclude( oi => oi.Product )
            .FirstOrDefaultAsync( o => o.Id == orderId, cancellationToken );
    }

    public async Task<decimal> GetTotalRevenueAsync( DateTime from, DateTime to, CancellationToken cancellationToken = default ) {
        var orders = await SearchAsync( o => o.OrderDate >= from && o.OrderDate <= to, null, cancellationToken );
        return orders.Sum( o => o.TotalAmount );
    }
}
```

## 🪄 Padrão Unit of Work

O `IUnitOfWorkRepository` fornece capacidades de gerenciamento de transações.

### Interface

```csharp
public interface IUnitOfWorkRepository : IAsyncDisposable {
    Task BeginTransactionAsync( CancellationToken cancellationToken = default );
    Task CommitAsync( CancellationToken cancellationToken = default );
    Task RollbackAsync( CancellationToken cancellationToken = default );
    Task CreateSavepointAsync( string savepointName, CancellationToken cancellationToken = default );
    Task RollbackToSavepointAsync( string savepointName, CancellationToken cancellationToken = default );
    Task<int> SaveChangesAsync( CancellationToken cancellationToken = default );
    Task<int> ExecuteSqlAsync( string query, IEnumerable<object>? parameters = null, CancellationToken cancellationToken = default );
}
```

### Implementação

```csharp
// Criar Unit of Work customizado
public class ApplicationUnitOfWork : BaseUnitOfWorkRepository {
    public ApplicationUnitOfWork( ApplicationDbContext context ) : base( context ) { }
}

// Ou usar implementação genérica
public class GenericUnitOfWork : UnitOfWorkRepository<ApplicationDbContext> {
    public GenericUnitOfWork( ApplicationDbContext context ) : base( context ) { }
}
```

### Registro

```csharp
// Opção 1: Implementação customizada
builder.Services.AddUnitOfWork<ApplicationUnitOfWork>( );

// Opção 2: Implementação genérica
builder.Services.AddUnitOfWorkForContext<ApplicationDbContext>( );
```

### Exemplo de Uso

**Transação Básica:**

```csharp
public class OrderService {
    private readonly IOrderRepository _orderRepository;
    private readonly IInventoryRepository _inventoryRepository;
    private readonly IUnitOfWorkRepository _unitOfWork;

    public OrderService( IOrderRepository orderRepository, IInventoryRepository inventoryRepository, IUnitOfWorkRepository unitOfWork ) {
        _orderRepository = orderRepository;
        _inventoryRepository = inventoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> CreateOrderAsync( CreateOrderRequest request ) {
        await _unitOfWork.BeginTransactionAsync( );

        try {
            // Criar pedido
            var order = new Order { CustomerId = request.CustomerId, OrderDate = DateTime.UtcNow };
            await _orderRepository.AddAsync( order );
            await _unitOfWork.SaveChangesAsync( );

            // Atualizar estoque
            foreach ( var item in request.Items ) {
                var inventory = await _inventoryRepository.FirstOrDefaultAsync( i => i.ProductId == item.ProductId );
                if ( inventory == null || inventory.Quantity < item.Quantity )
                    throw new InvalidOperationException( "Estoque insuficiente" );

                inventory.Quantity -= item.Quantity;
                await _inventoryRepository.UpdateAsync( inventory );
            }
            await _unitOfWork.SaveChangesAsync( );

            // Confirmar transação
            await _unitOfWork.CommitAsync( );
            return Result.Success( );
        }
        catch ( Exception ex ) {
            await _unitOfWork.RollbackAsync( );
            return Result.Failure( ex.Message );
        }
    }
}
```

**Transação com Savepoints:**

```csharp
public async Task<Result> ComplexOperationAsync( ) {
    await _unitOfWork.BeginTransactionAsync( );

    try {
        // Etapa 1: Criar usuário
        await CreateUserAsync( );
        await _unitOfWork.SaveChangesAsync( );
        await _unitOfWork.CreateSavepointAsync( "AfterUserCreation" );

        // Etapa 2: Criar perfil (pode falhar)
        try {
            await CreateUserProfileAsync( );
            await _unitOfWork.SaveChangesAsync( );
        }
        catch {
            // Retornar ao savepoint, mantendo criação do usuário
            await _unitOfWork.RollbackToSavepointAsync( "AfterUserCreation" );
            await CreateDefaultProfileAsync( );
            await _unitOfWork.SaveChangesAsync( );
        }

        await _unitOfWork.CommitAsync( );
        return Result.Success( );
    }
    catch ( Exception ex ) {
        await _unitOfWork.RollbackAsync( );
        return Result.Failure( ex.Message );
    }
}
```

## 🚀 Injeção de Dependência

### Registro Automático

A biblioteca fornece registro automático inteligente:

```csharp
// Registra TODAS as implementações de repositório dos assemblies da aplicação
builder.Services.AddRepositories( );
```

**Regras de Registro:**
- Tempo de vida Scoped por padrão (recomendado para repositórios)
- Exclui automaticamente tipos de teste (Test, Mock, Fake, Stub)
- Exclui automaticamente definições de tipos genéricos
- Combina repositórios com interfaces por convenção de nomenclatura

### Registro de Assembly Específico

```csharp
// Registrar repositórios de um assembly específico
builder.Services.AddRepositoriesFromAssembly( typeof( UserRepository ).Assembly );

// Com tempo de vida customizado
builder.Services.AddRepositoriesFromAssembly( typeof( UserRepository ).Assembly, ServiceLifetime.Transient );
```

### Registro Manual

```csharp
// Registrar repositório específico manualmente
builder.Services.AddRepository<IUserRepository, UserRepository>( );

// Com tempo de vida customizado
builder.Services.AddRepository<IUserRepository, UserRepository>( ServiceLifetime.Transient );
```

### Registro Unit of Work

```csharp
// Registrar implementação customizada de Unit of Work
builder.Services.AddUnitOfWork<ApplicationUnitOfWork>( );

// Ou registrar Unit of Work genérico para contexto específico
builder.Services.AddUnitOfWorkForContext<ApplicationDbContext>( );
```

### Exemplo de Configuração Completa

```csharp
var builder = WebApplication.CreateBuilder( args );

// Configuração do banco de dados
builder.Services.AddDbContext<ApplicationDbContext>( options => {
    options.UseSqlServer( builder.Configuration.GetConnectionString( "DefaultConnection" ) );
    options.EnableSensitiveDataLogging( builder.Environment.IsDevelopment( ) );
    options.EnableDetailedErrors( builder.Environment.IsDevelopment( ) );
} );

// Registro de repositórios
builder.Services.AddRepositories( ); // Auto-registrar todos os repositórios

// Registro Unit of Work
builder.Services.AddUnitOfWorkForContext<ApplicationDbContext>( );

// Construir app com resolução de dependência entre bibliotecas
var app = builder.BuildApp( );

app.Run( );
```

## 📝 Melhores Práticas

1. **Use Interfaces de Repositório**: Sempre dependa de interfaces, não implementações concretas
2. **Mantenha Repositórios Enxutos**: Lógica de negócio complexa pertence aos serviços, não repositórios
3. **Aproveite Specifications**: Use Myth.Specification para lógica de consulta reutilizável
4. **Use Unit of Work para Transações**: Quando operações abrangem múltiplos repositórios
5. **Async em Tudo**: Sempre use async/await para operações de banco de dados
6. **Descarte Adequadamente**: Repositórios implementam `IAsyncDisposable` quando necessário
7. **Use BuildApp()**: Substitua `builder.Build()` por `builder.BuildApp()` para integração entre bibliotecas

## 🔗 Bibliotecas Relacionadas

- **Myth.Repository**: Interfaces base de repositório e abstrações
- **Myth.Specification**: Implementação do padrão specification para construção de consultas
- **Myth.Commons**: Utilitários e extensões comuns

## 📄 Licença

Licenciado sob a Licença Apache 2.0. Veja [LICENSE](https://opensource.org/licenses/Apache-2.0) para detalhes.