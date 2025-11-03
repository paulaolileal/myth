# Myth.Commons

[![NuGet Version](https://img.shields.io/nuget/v/Myth.commons?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Commons/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.commons?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Commons/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

**Myth.Commons** é uma biblioteca .NET fundamental que fornece utilitários essenciais e padrões para construir aplicações corporativas robustas e sustentáveis. Oferece serialização JSON, manipulação de strings, blocos de construção DDD e gerenciamento centralizado de service provider para resolução de dependências entre bibliotecas.

## Funcionalidades

- **Extensões JSON**: Serialização/desserialização flexível com configurações personalizáveis
- **Utilitários de String**: Conjunto rico de métodos de manipulação de strings
- **Extensões de URL**: Auxiliares para codificação de URL
- **Value Objects**: Classe base DDD para implementar value objects com igualdade estrutural
- **Constantes**: Constantes type-safe usando o padrão SmartEnum
- **Gerenciamento de Service Provider**: Service provider global para resolução de dependências entre bibliotecas
- **Serviços com Escopo**: Padrão para executar operações dentro de escopos de serviço automáticos
- **Suporte a Paginação**: Value objects e interfaces para resultados paginados
- **Extensões de Coleção**: Métodos auxiliares para trabalhar com enumeráveis

## Instalação

```bash
dotnet add package Myth.Commons
```

## Índice

- [Extensões JSON](#extensões-json)
- [Extensões de String](#extensões-de-string)
- [Extensões de URL](#extensões-de-url)
- [Value Objects](#value-objects)
- [Constantes](#constantes)
- [Gerenciamento de Service Provider](#gerenciamento-de-service-provider)
- [Serviços com Escopo](#serviços-com-escopo)
- [Paginação](#paginação)
- [Extensões de Coleção](#extensões-de-coleção)

## Extensões JSON

Serialização e desserialização JSON poderosa com System.Text.Json, oferecendo configuração global, conversores personalizados e estratégias flexíveis de nomenclatura.

### Uso Básico

```csharp
using Myth.Extensions;

// Serializar para JSON
var user = new User { Id = 1, Name = "John Doe", Email = "john@example.com" };
var json = user.ToJson();
// {"id":1,"name":"John Doe","email":"john@example.com"}

// Desserializar de JSON
var userObj = json.FromJson<User>();
```

### Configuração Global

Configure as configurações JSON globalmente para toda sua aplicação:

```csharp
JsonExtensions.Configure( settings => settings
    .UseCaseStrategy( CaseStrategy.SnakeCase )
    .IgnoreNull()
    .Minify()
);

var json = user.ToJson();
// {"id":1,"name":"John Doe","email":"john@example.com"}
```

### Configuração por Operação

Sobrescreva configurações globais para operações específicas:

```csharp
// Usar snake_case apenas para esta operação
var json = user.ToJson( settings => settings
    .UseCaseStrategy( CaseStrategy.SnakeCase )
);
// {"id":1,"name":"john doe","email":"john@example.com"}

// Minificar saída JSON
var compactJson = user.ToJson( settings => settings.Minify() );

// Ignorar valores nulos
var jsonWithoutNulls = user.ToJson( settings => settings.IgnoreNull() );
```

### Conversores de Interface para Tipo Concreto

Manipule interfaces e tipos abstratos durante serialização/desserialização:

```csharp
// Usando conversor genérico
var json = user.ToJson( settings => settings
    .UseInterfaceConverter<IAddress, Address>()
);

// Usando conversor não-genérico
var json = user.ToJson( settings => settings
    .UseInterfaceConverter( typeof( IAddress ), typeof( Address ) )
);
```

### Conversores JSON Personalizados

Adicione conversores System.Text.Json personalizados:

```csharp
var json = user.ToJson( settings => settings
    .UseCustomConverter( new CustomDateTimeConverter() )
);
```

### Configurações JSON Avançadas

Acesse o JsonSerializerOptions subjacente para controle refinado:

```csharp
var json = user.ToJson( settings => {
    settings.IgnoreNull().Minify();
    settings.OtherSettings = options => {
        options.MaxDepth = 64;
        options.NumberHandling = JsonNumberHandling.AllowReadingFromString;
    };
} );
```

### Estratégias de Case

Duas convenções de nomenclatura são suportadas:

```csharp
public enum CaseStrategy {
    CamelCase,  // myAwesomeProperty
    SnakeCase   // my_awesome_property
}
```

### Suporte a Objetos Dinâmicos

Desserialize para objetos dinâmicos:

```csharp
var json = "{\"name\":\"John\",\"age\":30}";
dynamic obj = json.FromJson<object>();
Console.WriteLine( obj.name ); // John
```

### Tratamento de Exceções

Todas as operações JSON lançam `JsonParsingException` em caso de falha:

```csharp
try {
    var obj = invalidJson.FromJson<User>();
} catch ( JsonParsingException ex ) {
    Console.WriteLine( $"Falha ao processar JSON: {ex.Message}" );
    Console.WriteLine( $"Exceção interna: {ex.InnerException?.Message}" );
}
```

## Extensões de String

Conjunto rico de utilitários para manipulação e análise de strings.

```csharp
using Myth.Extensions;

// Remover texto
var result = "Hello World".Remove( "World" ); // "Hello "

// Minificar (remover todos os espaços em branco)
var minified = "Hello   World\n\t".Minify(); // "HelloWorld"

// Alterar case da primeira letra
var lower = "Hello".ToFirstLower(); // "hello"
var upper = "hello".ToFirstUpper(); // "Hello"

// Extrair texto entre caracteres
var text = "The 'quick' brown fox";
var extracted = text.GetStringBetween( '\'' ); // "quick"

// Encontrar palavras
var sentence = "The quick brown fox";
var word = sentence.GetWordThatContains( "qui" ); // "quick"
var before = sentence.GetWordBefore( "brown" ); // "quick"
var after = sentence.GetWordAfter( "quick" ); // "brown"

// Operações de busca
var hasAny = "Hello World".ContainsAnyOf( "Hi", "Hello", "Hey" ); // true
var startsWithAny = "Hello World".StartsWithAnyOf( "Hi", "Hello" ); // true
```

## Extensões de URL

Codifique objetos para uso em URL:

```csharp
using Myth.Extensions;

var text = "Hello World";
var encoded = text.EncodeAsUrl(); // "Hello+World"

var flag = true;
var encodedFlag = flag.EncodeAsUrl(); // true (como boolean)
```

## Value Objects

Classe base para implementar value objects de Domain-Driven Design com igualdade estrutural.

```csharp
using Myth.ValueObjects;

public class Address : ValueObject {
    public string Street { get; }
    public string City { get; }
    public string ZipCode { get; }

    public Address( string street, string city, string zipCode ) {
        Street = street;
        City = city;
        ZipCode = zipCode;
    }

    protected override IEnumerable<object> GetAtomicValues() {
        yield return Street;
        yield return City;
        yield return ZipCode;
    }
}

// Value objects são comparados por seus valores, não por referência
var address1 = new Address( "123 Main St", "Springfield", "12345" );
var address2 = new Address( "123 Main St", "Springfield", "12345" );
var address3 = new Address( "456 Oak Ave", "Springfield", "12345" );

Console.WriteLine( address1 == address2 ); // true (mesmos valores)
Console.WriteLine( address1 == address3 ); // false (valores diferentes)

// Clonar value objects
var clone = address1.Clone();
```

### Benefícios de Value Objects

- **Imutabilidade**: Encoraja padrões de design imutáveis
- **Igualdade por Valor**: Trata automaticamente comparação de igualdade baseada em propriedades
- **Alinhamento com DDD**: Perfeito para modelagem de domínio e padrões táticos DDD
- **Type Safety**: Previne obsessão por primitivos

## Constantes

Constantes type-safe usando o padrão Ardalis.SmartEnum.

```csharp
using Myth.ValueObjects;

public class OrderStatus : Constant<OrderStatus, string> {
    public static readonly OrderStatus Pending = new( nameof( Pending ), "PENDING" );
    public static readonly OrderStatus Processing = new( nameof( Processing ), "PROCESSING" );
    public static readonly OrderStatus Completed = new( nameof( Completed ), "COMPLETED" );
    public static readonly OrderStatus Cancelled = new( nameof( Cancelled ), "CANCELLED" );

    private OrderStatus( string name, string value ) : base( name, value ) { }
}

// Uso
var status = OrderStatus.Pending;
string statusValue = status; // Conversão implícita para "PENDING"

// Obter a partir do valor
var status2 = OrderStatus.FromValue( "PROCESSING" ); // OrderStatus.Processing

// Obter a partir do nome
var status3 = OrderStatus.FromName( "Completed" ); // OrderStatus.Completed

// Listar todas as opções
var options = OrderStatus.GetOptions();
// "(Pending): PENDING | (Processing): PROCESSING | (Completed): COMPLETED | (Cancelled): CANCELLED"

// Switch com constantes (exaustivo)
var message = status switch {
    var s when s == OrderStatus.Pending => "Pedido pendente",
    var s when s == OrderStatus.Processing => "Pedido sendo processado",
    var s when s == OrderStatus.Completed => "Pedido concluído",
    var s when s == OrderStatus.Cancelled => "Pedido cancelado",
    _ => throw new InvalidOperationException()
};
```

### Benefícios de Constantes

- **Type Safety**: Segurança em tempo de compilação ao invés de strings/números mágicos
- **Suporte IntelliSense**: Autocomplete da IDE para todos os valores
- **Pattern Matching**: Funciona perfeitamente com expressões switch do C#
- **Listagem**: Fácil enumeração de todos os valores
- **Extensibilidade**: Adicione métodos e propriedades às constantes

## Gerenciamento de Service Provider

Gerenciamento de service provider global permite resolução de dependências entre bibliotecas sem acoplamento.

### Aplicações ASP.NET Core

Use `BuildApp()` ao invés de `Build()` para inicializar automaticamente o service provider global:

```csharp
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddFlow();
builder.Services.AddGuard();
builder.Services.AddFlowActions( config => { ... } );

var app = builder.BuildApp(); // Ao invés de builder.Build()

app.UseGuard();
app.Run();
```

### Aplicações Console / Background Services

Use o service provider global para aplicações não-web:

```csharp
var services = new ServiceCollection();
services.AddFlow();
services.AddGuard();
services.AddMyServices();

var serviceProvider = services.BuildServiceProvider();
MythServiceProvider.Initialize( serviceProvider );

// Agora todas as bibliotecas podem resolver dependências
var pipeline = Pipeline.Start( context );
```

### Acessando o Service Provider Global

```csharp
using Myth.ServiceProvider;

// Verificar se foi inicializado
if ( MythServiceProvider.IsInitialized ) {
    var provider = MythServiceProvider.Current;
}

// Obter ou lançar exceção se não inicializado
var requiredProvider = MythServiceProvider.GetRequired();

// Obter com fallback
var provider = MythServiceProvider.GetOrFallback( localServiceProvider );

// Tentar inicializar (padrão first-wins)
var initialized = MythServiceProvider.TryInitialize( serviceProvider );

// Forçar inicialização (sobrescreve existente)
MythServiceProvider.Initialize( serviceProvider );
```

### Integração com Bibliotecas Externas

Bibliotecas externas podem acessar serviços registrados:

```csharp
public class ThirdPartyLibrary {
    public void DoSomething() {
        var provider = ServiceCollectionExtensions.GetGlobalProvider();
        var validator = provider?.GetService<IValidator>();
        if ( validator != null ) {
            // Usar bibliotecas Myth sem acoplamento direto
        }
    }
}
```

### Suporte para Testes

Resete o provider global para testes unitários isolados:

```csharp
[Fact]
public void TestWithCleanProvider() {
    MythServiceProvider.Reset();

    var services = new ServiceCollection();
    // ... configurar serviços de teste
    var provider = services.BuildServiceProvider();
    MythServiceProvider.Initialize( provider );

    // Executar teste
}
```

## Serviços com Escopo

Padrão para executar operações dentro de escopos de serviço automáticos, perfeito para handlers transientes acessando dependências com escopo como repositórios com DbContext.

### Configuração

Registre o padrão de service provider com escopo uma vez em sua aplicação:

```csharp
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddScopedServiceProvider();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddDbContext<AppDbContext>();

var app = builder.BuildApp();
```

### Uso em Handlers

```csharp
public class CreateOrderHandler : ICommandHandler<CreateOrderCommand> {
    private readonly IScopedService<IOrderRepository> _repository;
    private readonly IScopedService<IEmailService> _emailService;

    public CreateOrderHandler(
        IScopedService<IOrderRepository> repository,
        IScopedService<IEmailService> emailService ) {
        _repository = repository;
        _emailService = emailService;
    }

    public async Task<CommandResult> HandleAsync(
        CreateOrderCommand command,
        CancellationToken ct ) {

        // Executar com gerenciamento automático de escopo
        var order = await _repository.ExecuteAsync( repo =>
            repo.CreateAsync( command.OrderData, ct )
        );

        // Executar operações void
        await _emailService.ExecuteAsync( email =>
            email.SendOrderConfirmationAsync( order.Id, ct )
        );

        return CommandResult.Success();
    }
}
```

### Operações Síncronas

```csharp
// Com valor de retorno
var result = _scopedService.Execute( service =>
    service.GetData()
);

// Operação void
_scopedService.Execute( service =>
    service.ProcessData()
);
```

### Operações Assíncronas

```csharp
// Com valor de retorno
var result = await _scopedService.ExecuteAsync( service =>
    service.GetDataAsync()
);

// Operação void
await _scopedService.ExecuteAsync( service =>
    service.ProcessDataAsync()
);
```

### Benefícios

- **Gerenciamento Automático de Escopo**: Sem criação ou disposição manual de escopo
- **Segurança de Tempo de Vida**: Acesse serviços com escopo de contextos transientes com segurança
- **API Limpa**: Interface fortemente tipada e fluente
- **Disposição Adequada**: Trata disposição síncrona e assíncrona corretamente
- **Alinhamento com DDD**: Perfeito para handlers CQRS acessando repositórios

## Paginação

Value objects e interfaces para implementar resultados paginados.

### Value Object de Paginação

```csharp
using Myth.ValueObjects;

// Paginação padrão (página 1, tamanho 10)
var pagination = Pagination.Default;

// Paginação personalizada
var customPagination = new Pagination( pageNumber: 2, pageSize: 20 );

// Obter todos os itens (página única)
var allItems = Pagination.All;

// Binding automático no ASP.NET Core
[HttpGet]
public IActionResult GetOrders( [FromQuery] Pagination pagination ) {
    // Automaticamente vincula da query string: ?$pagenumber=2&$pagesize=20
}
```

### Resultados Paginados

```csharp
using Myth.Interfaces.Results;
using Myth.Models.Results;

// Criar resultado paginado
var items = await repository.GetOrdersAsync( pagination );
var total = await repository.GetTotalCountAsync();
var totalPages = ( int )Math.Ceiling( ( double )total / pagination.PageSize );

var result = new Paginated<Order>(
    pageNumber: pagination.PageNumber,
    pageSize: pagination.PageSize,
    totalItems: total,
    totalPages: totalPages,
    items: items
);

// Acessar propriedades
Console.WriteLine( $"Página {result.PageNumber} de {result.TotalPages}" );
Console.WriteLine( $"Mostrando {result.Items.Count()} de {result.TotalItems} itens" );

// Retornar na API
return Ok( result );
```

### Interface IPaginated

Implemente tipos paginados personalizados:

```csharp
public class CustomPaginatedResult<T> : IPaginated<T> {
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
    public IEnumerable<T> Items { get; set; }

    // Adicionar propriedades personalizadas
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}
```

## Extensões de Coleção

Métodos auxiliares para trabalhar com coleções.

```csharp
using Myth.Extensions;

var items = new[] { "apple", "banana", "cherry" };

// Unir com separador
var result = items.ToStringWithSeparator( ", " );
// "apple, banana, cherry"

// Separador personalizado
var result2 = items.ToStringWithSeparator( " | " );
// "apple | banana | cherry"

// Separador padrão (", ")
var result3 = items.ToStringWithSeparator();
// "apple, banana, cherry"
```

## Padrões de Arquitetura

### Domain-Driven Design

Myth.Commons fornece blocos de construção essenciais de DDD:

- **Value Objects**: Implemente value objects de domínio com igualdade estrutural
- **Constantes**: Constantes e enumerações de domínio type-safe
- **Paginação**: Modelo de domínio para consultas paginadas

```csharp
// Value Object para Money
public class Money : ValueObject {
    public decimal Amount { get; }
    public string Currency { get; }

    public Money( decimal amount, string currency ) {
        Amount = amount;
        Currency = currency;
    }

    protected override IEnumerable<object> GetAtomicValues() {
        yield return Amount;
        yield return Currency;
    }

    public Money Add( Money other ) {
        if ( Currency != other.Currency )
            throw new InvalidOperationException( "Não é possível adicionar moedas com diferentes moedas" );

        return new Money( Amount + other.Amount, Currency );
    }
}

// Constantes type-safe
public class Currency : Constant<Currency, string> {
    public static readonly Currency USD = new( nameof( USD ), "USD" );
    public static readonly Currency EUR = new( nameof( EUR ), "EUR" );
    public static readonly Currency BRL = new( nameof( BRL ), "BRL" );

    private Currency( string name, string value ) : base( name, value ) { }
}
```

### Integração CQRS

Perfeito para padrões CQRS com gerenciamento de serviço com escopo:

```csharp
public class GetOrdersQueryHandler : IQueryHandler<GetOrdersQuery, IPaginated<Order>> {
    private readonly IScopedService<IOrderRepository> _repository;

    public GetOrdersQueryHandler( IScopedService<IOrderRepository> repository ) {
        _repository = repository;
    }

    public async Task<QueryResult<IPaginated<Order>>> HandleAsync(
        GetOrdersQuery query,
        CancellationToken ct ) {

        var result = await _repository.ExecuteAsync( async repo => {
            var items = await repo.GetOrdersAsync( query.Pagination, ct );
            var total = await repo.GetTotalCountAsync( ct );

            return new Paginated<Order>(
                query.Pagination.PageNumber,
                query.Pagination.PageSize,
                total,
                ( int )Math.Ceiling( ( double )total / query.Pagination.PageSize ),
                items
            );
        } );

        return QueryResult<IPaginated<Order>>.Success( result );
    }
}
```

### Clean Architecture

Suporta princípios de Clean Architecture:

- **Independência de Infraestrutura**: Serialização JSON sem dependências externas
- **Independência de Framework**: Funciona com qualquer tipo de aplicação .NET
- **Testabilidade**: Fácil de mockar e testar com interfaces claras
- **Separação de Responsabilidades**: Cada utilitário focado em responsabilidade única

## Melhores Práticas

### Serialização JSON

1. Configure configurações JSON globais uma vez na inicialização da aplicação
2. Use configurações por operação apenas quando necessário
3. Trate `JsonParsingException` para tratamento robusto de erros
4. Use conversores de interface para tipos polimórficos

### Value Objects

1. Torne value objects imutáveis
2. Sobrescreva `GetAtomicValues()` para incluir todas as propriedades que definem igualdade
3. Considere validação no construtor
4. Use para conceitos de domínio, não apenas transferência de dados

### Constantes

1. Use para enumerações específicas do domínio
2. Prefira constantes ao invés de strings/números mágicos
3. Adicione métodos de domínio às classes de constantes
4. Use com pattern matching para verificações exaustivas

### Service Provider

1. Inicialize o provider global uma vez na inicialização da aplicação
2. Use `BuildApp()` para aplicações ASP.NET Core
3. Use `TryInitialize()` para código de biblioteca (padrão first-wins)
4. Resete o provider em testes para isolamento

### Serviços com Escopo

1. Registre `AddScopedServiceProvider()` uma vez por aplicação
2. Use para acessar dependências com escopo de contextos transientes
3. Perfeito para handlers CQRS e background services
4. Disposição automática - sem gerenciamento manual de escopo necessário

## Dependências

- **Ardalis.SmartEnum** 8.0.0 - Para constantes type-safe
- **Microsoft.Extensions.DependencyInjection** 8.0.0 - Para suporte a DI
- **System.Text.Json** 8.0.5 - Para operações JSON

## Requisitos

- .NET 8.0 ou superior

## Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para enviar um Pull Request.

## Licença

Este projeto está licenciado sob a Licença Apache 2.0 - veja a [LICENSE](https://www.apache.org/licenses/LICENSE-2.0) para detalhes.

## Suporte

Para problemas, questões ou contribuições, visite o [repositório GitLab](https://gitlab.com/dotnet-myth/myth/-/tree/main/Myth.Commons).
