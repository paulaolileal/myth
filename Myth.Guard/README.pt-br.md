# Myth.Guard

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Guard?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Guard/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Guard?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Guard/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](README.md)

Uma poderosa biblioteca de validação fluente .NET projetada para aplicações enterprise. Construída com princípios de arquitetura limpa, Myth.Guard fornece validação declarativa com consciência de contexto, integração assíncrona com serviços e middleware automático para ASP.NET Core.

## Por que Myth.Guard?

A maioria das bibliotecas de validação forçam você a escolher entre validação baseada em atributos (inflexível) ou código de validação imperativo (verboso e disperso). Myth.Guard oferece uma terceira opção: **validação declarativa e fluente que vive com suas entidades**, promovendo Domain-Driven Design mantendo a lógica de validação sustentável e testável.

## Características Principais

- **API Fluente Declarativa**: Escreva regras de validação legíveis com métodos encadeáveis
- **Validação Consciente de Contexto**: Regras diferentes para operações Create, Update, Delete na mesma entidade
- **Integração Assíncrona com Serviços**: Acesse injeção de dependência para validação de banco de dados ou API
- **Tratador Global de Exceções**: Configure mapeamentos customizados de exceções com códigos de status e formatos de resposta
- **Tratamento Automático de Erros**: Middleware ASP.NET Core com respostas JSON estruturadas
- **100+ Regras Integradas**: Validação abrangente para strings, números, coleções, datas, booleanos, enums
- **Suporte a Tipos Nullable**: Suporte completo para tipos nullable com regras dedicadas
- **Regras Customizadas**: Fácil extensibilidade com métodos `Respect()` e `RespectAsync()`
- **Validação Condicional**: Regras condicionais no nível de campo e entidade
- **Parar em Falha**: Otimize a validação parando após falhas críticas
- **Customização de Status HTTP**: Configure códigos de status apropriados por erro de validação

## Instalação

```bash
dotnet add package Myth.Guard
```

## Início Rápido

### 1. Defina Validação na Sua Entidade

```csharp
public class CreateUserDto : IValidatable<CreateUserDto>
{
    public string Name { get; set; }
    public string Email { get; set; }
    public int Age { get; set; }
    public List<string> Tags { get; set; }

    public void Validate( ValidationBuilder<CreateUserDto> builder, ValidationContextKey? context = null )
    {
        builder.For( Name, x => x.NotEmpty().MinimumLength( 2 ).MaximumLength( 100 ) );
        builder.For( Email, x => x.NotEmpty().Email() );
        builder.For( Age, x => x.GreaterThan( 0 ).LessThan( 150 ) );
        builder.For( Tags, x => x.NotEmpty().CountBetween( 1, 10 ) );
    }
}
```

### 2. Configure Serviços e Middleware

```csharp
var builder = WebApplication.CreateBuilder( args );

builder.Services.AddGuard();

var app = builder.Build();

app.UseGuard(); // Adiciona tratamento automático de exceções de validação

app.MapControllers();
app.Run();
```

### 3. Use em Controllers

```csharp
public class UserController : ControllerBase
{
    private readonly IValidator _validator;

    public UserController( IValidator validator )
    {
        _validator = validator;
    }

    [HttpPost( "users" )]
    public async Task<IActionResult> CreateUser( [FromBody] CreateUserDto request )
    {
        // Valida e lança ValidationException em caso de falha
        await _validator.ValidateAsync( request, ValidationContextKey.Create );

        // Ou valida e verifica o resultado sem lançar exceção
        var result = await _validator.ValidateAndReturnAsync( request, ValidationContextKey.Create );

        if ( !result.IsValid )
            return BadRequest( new { errors = result.Errors } );

        // Processa criação do usuário...
        return Ok( new { message = "Usuário criado com sucesso" } );
    }
}
```

### Resposta Automática de Erro

Com o middleware `app.UseGuard()`, exceções de validação são automaticamente formatadas:

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
            "message": "Valor deve ser maior que 0",
            "code": "VIOLATION"
        }
    ]
}
```

## Referência de Regras de Validação

### Regras de String

```csharp
builder.For( Email, x => x
    .NotEmpty()
    .Email()
    .MaximumLength( 254 ) );

builder.For( Name, x => x
    .NotEmpty()
    .MinimumLength( 2 )
    .MaximumLength( 100 )
    .OnlyLetters() );

builder.For( Password, x => x
    .NotEmpty()
    .MinimumLength( 8 )
    .Matches( new Regex( @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$" ) )
    .WithMessage( "Senha deve conter maiúscula, minúscula e dígito" ) );

builder.For( PhoneNumber, x => x
    .NotEmpty()
    .Matches( new Regex( @"^\+\d{1,3}\d{10,14}$" ) ) );
```

**Regras de String Disponíveis:**
- `NotEmpty()` - Não nulo, vazio ou apenas espaços em branco
- `MinimumLength(int)`, `MaximumLength(int)`, `LengthBetween(int, int)` - Validação de comprimento
- `Email()`, `Url()` - Validação de formato
- `OnlyLetters()`, `OnlyNumbers()`, `Alphanumeric()` - Validação de tipo de caractere
- `StartsWith(string)`, `EndsWith(string)`, `Contains(string)` - Verificações de substring
- `Matches(Regex)` - Correspondência de padrão regex
- `EqualsTo(string)`, `BeOneOf(params string[])` - Verificações de enumeração
- `AvailableCharacters(params char[])`, `ForbiddenCharacters(params char[])` - Lista de permitidos/proibidos
- `NoSymbols(char[]?)` - Validação de símbolos

### Regras Numéricas

```csharp
builder.For( Age, x => x
    .GreaterThan( 0 )
    .LessThan( 150 ) );

builder.For( Salary, x => x
    .GreaterOrEquals( 0 )
    .LessThan( 1000000m ) );

builder.For( Score, x => x
    .Between( 0, 100 )
    .When( score => score.HasValue ) );

builder.For( Quantity, x => x
    .Positive()
    .NotZero() );
```

**Regras Numéricas Disponíveis** (int, long, decimal, double, float, etc.):
- `GreaterThan(T)`, `GreaterOrEquals(T)` - Validação de valor mínimo
- `LessThan(T)`, `LessOrEquals(T)` - Validação de valor máximo
- `Between(T min, T max)` - Validação de intervalo (inclusivo)
- `Positive()`, `Negative()` - Validação de sinal
- `Zero()`, `NotZero()` - Verificações de valor zero

### Regras de Coleção

```csharp
builder.For( Tags, x => x
    .NotEmpty()
    .CountBetween( 1, 10 )
    .All( tag => !string.IsNullOrWhiteSpace( tag ) )
    .Distinct() );

builder.For( UserRoles, x => x
    .NotEmpty()
    .Any( role => role == "Admin" || role == "User" )
    .None( role => role == "Banned" ) );

builder.For( Products, x => x
    .DistinctBy( p => p.Sku ) );
```

**Regras de Coleção Disponíveis:**
- `NotEmpty()` - Coleção não nula e com elementos
- `CountBetween(int, int)`, `CountGreaterThan(int)`, `CountLessThan(int)` - Validação de tamanho
- `All<T>(Func<T, bool>)` - Todos os elementos correspondem à condição
- `Any<T>(Func<T, bool>)` - Pelo menos um corresponde
- `None<T>(Func<T, bool>)` - Nenhum elemento corresponde
- `Distinct<T>()` - Sem duplicatas (usando igualdade padrão)
- `DistinctBy<T, TKey>(Func<T, TKey>)` - Sem duplicatas por chave

### Regras DateTime e DateOnly

```csharp
builder.For( BirthDate, x => x
    .Past()
    .After( new DateTime( 1900, 1, 1 ) ) );

builder.For( ScheduledDate, x => x
    .Future()
    .Before( DateTime.Now.AddYears( 1 ) ) );

builder.For( AppointmentDate, x => x
    .Between( DateTime.Today, DateTime.Today.AddDays( 30 ) ) );

builder.For( EventDate, x => x.Today() );
```

**Regras DateTime/DateOnly Disponíveis:**
- `Past()`, `Future()`, `Today()` - Validação temporal
- `After(DateTime)`, `Before(DateTime)` - Comparação (exclusiva)
- `AfterOrEquals(DateTime)`, `BeforeOrEquals(DateTime)` - Comparação (inclusiva)
- `Between(DateTime, DateTime)` - Intervalo de datas (inclusivo)

### Regras Boolean e Enum

```csharp
builder.For( IsActive, x => x.IsTrue() );
builder.For( IsDeleted, x => x.IsFalse() );

builder.For( Role, x => x.BeInEnum<UserRole>() );
builder.For( Status, x => x.BeOneOf( Status.Active, Status.Pending ) );
```

### Regras de Constant

Valide valores e nomes contra tipos `Myth.Commons.ValueObjects.Constant<TConstant, TValue>`:

```csharp
// Defina suas constantes
public class Status : Constant<Status, string> {
    public static readonly Status Active = new( "Active", "A" );
    public static readonly Status Inactive = new( "Inactive", "I" );
    public static readonly Status Pending = new( "Pending", "P" );

    public Status( string name, string value ) : base( name, value ) { }
}

public class Priority : Constant<Priority, int> {
    public static readonly Priority Low = new( "Low", 1 );
    public static readonly Priority Medium = new( "Medium", 5 );
    public static readonly Priority High = new( "High", 10 );

    public Priority( string name, int value ) : base( name, value ) { }
}

// Valide valores e nomes de constantes
builder.For( StatusCode, x => x
    .NotEmpty()
    .ExistsInConstant<Status, string>() );

builder.For( StatusName, x => x
    .NotEmpty()
    .NameExistsInConstant<Status, string>() );

builder.For( PriorityLevel, x => x
    .ExistsInConstant<Priority, int>() );

builder.For( PriorityName, x => x
    .NotEmpty()
    .NameExistsInConstant<Priority, int>() );
```

**Regras de Constant Disponíveis:**
- `ExistsInConstant<TConstant, TValue>()` - Valida que um valor existe na definição da constante
- `NameExistsInConstant<TConstant, TValue>()` - Valida que um nome existe na definição da constante

**Mensagens de Erro:**
- Erro de valor: `"Value 'X' is not valid. Valid options are: A: Active | I: Inactive | P: Pending"`
- Erro de nome: `"Name 'Unknown' is not valid. Valid options are: 1: Low | 5: Medium | 10: High"`

### Regras Genéricas (Todos os Tipos)

```csharp
builder.For( UserId, x => x
    .NotNull()
    .NotDefault() );

builder.For( Email, x => x
    .NotNull()
    .NotEqualsTo( "admin@example.com" ) );
```

**Regras Genéricas Disponíveis:**
- `NotNull()`, `BeNull()` - Verificações de nulo
- `EqualsTo(T)`, `NotEqualsTo(T)` - Comparação de valor
- `BeDefault()`, `NotDefault()` - Verificações de valor padrão
- `Respect(Func<T, bool>)` - Validação customizada síncrona
- `RespectAsync(Func<T, CancellationToken, IServiceProvider, Task<bool>>)` - Validação customizada assíncrona

### Suporte a Tipos Nullable

Todas as regras numéricas, DateTime e booleanas têm versões nullable:

```csharp
builder.For( OptionalAge, x => x
    .GreaterThan( 18 )
    .When( age => age.HasValue ) );

builder.For( OptionalDate, x => x
    .Future()
    .When( date => date.HasValue ) );

builder.For( OptionalFlag, x => x.IsTrue() );
```

## Validação Consciente de Contexto

Defina diferentes regras de validação para diferentes operações:

```csharp
public class UserDto : IValidatable<UserDto>
{
    public string Email { get; set; }
    public int Age { get; set; }
    public bool IsActive { get; set; }
    public string Password { get; set; }

    public void Validate( ValidationBuilder<UserDto> builder, ValidationContextKey? context = null )
    {
        // Regras globais (aplicam a todos os contextos)
        builder.For( Email, x => x.NotEmpty().Email() );
        builder.For( Age, x => x.GreaterThan( 0 ).LessThan( 150 ) );

        // Regras específicas para criação
        builder.InContext( ValidationContextKey.Create, b =>
        {
            b.For( Email, x => x
                .RespectAsync( async ( email, ct, sp ) =>
                {
                    var userService = sp.GetRequiredService<IUserService>();
                    return await userService.IsEmailAvailableAsync( email, ct );
                } )
                .WithMessage( "Email já existe" )
                .WithCode( "EMAIL_EXISTS" )
                .WithStatusCode( HttpStatusCode.Conflict ) );

            b.For( Password, x => x
                .NotEmpty()
                .MinimumLength( 8 )
                .Matches( new Regex( @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).*$" ) ) );

            b.For( IsActive, x => x.IsTrue() );
        } );

        // Regras específicas para atualização
        builder.InContext( ValidationContextKey.Update, b =>
        {
            b.For( Age, x => x.GreaterOrEquals( 18 ) );
            // Senha é opcional na atualização
        } );

        // Regras específicas para exclusão
        builder.InContext( ValidationContextKey.Delete, b =>
        {
            b.For( IsActive, x => x.IsFalse()
                .WithMessage( "Não é possível excluir usuário ativo" ) );
        } );
    }
}

// Uso com diferentes contextos
await validator.ValidateAsync( user, ValidationContextKey.Create );
await validator.ValidateAsync( user, ValidationContextKey.Update );
await validator.ValidateAsync( user, ValidationContextKey.Delete );
```

### Contextos Pré-definidos

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
ValidationContextKey.Custom( "ImportacaoEmLote" )
```

## Validação Condicional

Execute regras baseadas em condições:

### Condições no Nível de Campo

```csharp
builder.For( PhoneNumber, x => x
    .NotEmpty()
    .When( phone => !string.IsNullOrEmpty( phone ) ) // Validar apenas se não estiver vazio
    .Matches( new Regex( @"^\+\d{1,3}\d{10,14}$" ) ) );

builder.For( Password, x => x
    .NotEmpty()
    .Unless( pwd => IsExternalUser ) // Pular para usuários externos
    .MinimumLength( 8 ) );
```

### Condições no Nível de Entidade

```csharp
builder.For( PhoneNumber, x => x
    .NotEmpty()
    .When<string, UserDto>( user => user.PhoneType == PhoneType.Required )
    .Unless<string, UserDto>( user => user.IsVerified ) );

builder.For( Salary, x => x
    .GreaterThan( 0 )
    .When<decimal, EmployeeDto>( emp => emp.EmploymentType == EmploymentType.FullTime ) );
```

## Validação Assíncrona com Service Provider

Acesse injeção de dependência para validação de banco de dados ou API:

```csharp
public class CreateOrderDto : IValidatable<CreateOrderDto>
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public string CustomerEmail { get; set; }

    public void Validate( ValidationBuilder<CreateOrderDto> builder, ValidationContextKey? context = null )
    {
        builder.For( ProductId, x => x
            .GreaterThan( 0 )
            .RespectAsync( async ( productId, ct, sp ) =>
            {
                var productService = sp.GetRequiredService<IProductService>();
                return await productService.ExistsAsync( productId, ct );
            } )
            .WithMessage( "Produto não existe" )
            .WithCode( "PRODUCT_NOT_FOUND" ) );

        builder.For( Quantity, x => x
            .GreaterThan( 0 )
            .RespectAsync( async ( quantity, ct, sp ) =>
            {
                var productService = sp.GetRequiredService<IProductService>();
                var stock = await productService.GetStockAsync( ProductId, ct );
                return quantity <= stock;
            } )
            .WithMessage( "Estoque insuficiente" )
            .WithCode( "INSUFFICIENT_STOCK" ) );

        builder.For( CustomerEmail, x => x
            .NotEmpty()
            .Email()
            .RespectAsync( async ( email, ct, sp ) =>
            {
                var customerService = sp.GetRequiredService<ICustomerService>();
                return await customerService.IsActiveCustomerAsync( email, ct );
            } )
            .WithMessage( "Cliente não encontrado ou inativo" )
            .WithCode( "CUSTOMER_INACTIVE" )
            .WithStatusCode( HttpStatusCode.NotFound ) );
    }
}
```

## Recursos Avançados

### Mensagens de Erro Customizadas

```csharp
// Mensagem estática
builder.For( Age, x => x
    .GreaterThan( 18 )
    .WithMessage( "Usuário deve ter pelo menos 18 anos" ) );

// Mensagem dinâmica usando valor do campo
builder.For( Age, x => x
    .GreaterThan( 18 )
    .WithMessage( age => $"Usuário deve ter pelo menos 18 anos, mas tem {age}" ) );

// Código de erro customizado
builder.For( Email, x => x
    .Email()
    .WithCode( "INVALID_EMAIL_FORMAT" ) );

// Código de status HTTP customizado
builder.For( UserId, x => x
    .RespectAsync( async ( id, ct, sp ) =>
    {
        var userService = sp.GetRequiredService<IUserService>();
        return await userService.ExistsAsync( id, ct );
    } )
    .WithMessage( "Usuário não encontrado" )
    .WithCode( "USER_NOT_FOUND" )
    .WithStatusCode( HttpStatusCode.NotFound ) ); // Retorna 404 em vez de 400
```

### Parar em Falha

Pare de validar um campo após a primeira falha:

```csharp
builder.For( Password, x => x
    .NotEmpty()
    .SetStopOnFailure() // Não verificar outras regras se estiver vazio
    .MinimumLength( 8 )
    .Matches( new Regex( @"[A-Z]" ) )
    .Matches( new Regex( @"[a-z]" ) )
    .Matches( new Regex( @"\d" ) ) );
```

### Regras de Negócio Complexas

```csharp
public class OrderDto : IValidatable<OrderDto>
{
    public decimal Amount { get; set; }
    public string CustomerType { get; set; }
    public List<OrderItem> Items { get; set; }
    public string CouponCode { get; set; }

    public void Validate( ValidationBuilder<OrderDto> builder, ValidationContextKey? context = null )
    {
        builder.For( Amount, x => x
            .GreaterThan( 0 )
            .When<decimal, OrderDto>( order => order.Items?.Any() == true ) );

        // Clientes premium podem ter valores de pedido mais altos
        builder.For( Amount, x => x
            .LessThan( 10000 )
            .Unless<decimal, OrderDto>( order => order.CustomerType == "Premium" ) );

        builder.For( Items, x => x
            .NotEmpty()
            .CountBetween( 1, 50 )
            .All( item => item.Quantity > 0 )
            .All( item => item.Price > 0 ) );

        // Validação de cupom
        builder.For( CouponCode, x => x
            .RespectAsync( async ( coupon, ct, sp ) =>
            {
                if ( string.IsNullOrEmpty( coupon ) ) return true; // Opcional

                var couponService = sp.GetRequiredService<ICouponService>();
                var isValid = await couponService.IsValidAsync( coupon, ct );
                var isApplicable = await couponService.IsApplicableToOrderAsync( coupon, Amount, ct );

                return isValid && isApplicable;
            } )
            .WithMessage( "Código de cupom inválido ou inaplicável" )
            .WithCode( "INVALID_COUPON" ) );
    }
}
```

## Tratamento de Erros

### Resultado de Validação

```csharp
var result = await validator.ValidateAndReturnAsync( dto );

Console.WriteLine( $"É Válido: {result.IsValid}" );
Console.WriteLine( $"Código de Status: {result.StatusCode}" );

if ( !result.IsValid )
{
    foreach ( var error in result.Errors )
    {
        Console.WriteLine( $"Campo: {error.Field}" );
        Console.WriteLine( $"Mensagem: {error.Message}" );
        Console.WriteLine( $"Código: {error.Code}" );
        Console.WriteLine( $"Status: {error.StatusCode}" );
    }
}
```

### Tratamento de Exceções

```csharp
try
{
    await validator.ValidateAsync( dto, ValidationContextKey.Create );
}
catch ( ValidationException ex )
{
    var errors = ex.ValidationResult.Errors;
    var statusCode = ex.ValidationResult.StatusCode;

    return BadRequest( new
    {
        message = "Validação falhou",
        errors = errors.Select( e => new
        {
            field = e.Field,
            message = e.Message,
            code = e.Code
        } )
    } );
}
```

### Resposta de Erro do Middleware

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

**Código de Status HTTP**: O código de status mais alto de todos os erros de validação (ex: se um erro tem `409 Conflict`, a resposta será `409`).

## Tratamento Global de Exceções

O Myth.Guard agora inclui um poderoso **Tratador Global de Exceções** que permite mapear qualquer tipo de exceção para respostas HTTP customizadas com códigos de status e formatos de erro apropriados.

### Comportamento Opt-In

Por padrão, `UseGuard()` **apenas trata `ValidationException`** automaticamente. Outras exceções **não são interceptadas** a menos que você configure handlers explicitamente para elas. Isso garante compatibilidade retroativa e lhe dá controle total.

### Configuração Rápida

Configure mapeamentos de exceções ao adicionar os serviços do Guard:

```csharp
builder.Services.AddGuard( options => {
    options.AutoMapCommonExceptions( );
} );
```

**Importante:** Sem chamar `AutoMapCommonExceptions()` ou configurar handlers customizados, apenas `ValidationException` será tratada pelo middleware.

O método `AutoMapCommonExceptions()` configura automaticamente padrões sensatos para exceções comuns do .NET:

- `ArgumentNullException` → 400 Bad Request
- `ArgumentException` → 400 Bad Request
- `InvalidOperationException` → 409 Conflict
- `UnauthorizedAccessException` → 403 Forbidden
- `NotImplementedException` → 501 Not Implemented
- `TimeoutException` → 408 Request Timeout
- Handler padrão → 500 Internal Server Error (com stack trace formatado em desenvolvimento)

### Mapeamentos Customizados de Exceções

Mapeie seus próprios tipos de exceção com configuração fluente:

```csharp
builder.Services.AddGuard( options => {
    // Mapear tipos específicos de exceção
    options
        .MapException<NotFoundException>( )
        .WithStatusCode( 404 )
        .WithErrorCode( "NOT_FOUND" )
        .WithResponse( ex => new {
            error = ex.Message,
            resourceType = ex.ResourceType
        } );

    options
        .MapException<BusinessRuleException>( )
        .WithStatusCode( 422 )
        .WithErrorCode( "BUSINESS_RULE_VIOLATION" )
        .WithResponse( ex => new {
            error = ex.Message,
            rule = ex.RuleName,
            details = ex.Details
        } )
        .OnBeforeResponse( ( ex, ctx ) => {
            _logger.LogWarning( ex, "Violação de regra de negócio: {Rule}", ex.RuleName );
        } );

    // Configurar handler padrão para exceções não mapeadas
    options
        .MapDefaultException( )
        .WithStatusCode( 500 )
        .WithErrorCode( "INTERNAL_ERROR" )
        .WithResponse( ex => new {
            error = _env.IsDevelopment( ) ? ex.Message : "Ocorreu um erro interno",
            trace = _env.IsDevelopment( ) ? ex.StackTrace : null
        } )
        .OnBeforeResponse( ( ex, ctx ) => {
            _logger.LogError( ex, "Exceção não tratada" );
        } );
} );
```

### Referência da API

#### `MapException<TException>()`

Cria um mapeamento para um tipo específico de exceção.

**Métodos Encadeáveis:**

- `.WithStatusCode( int statusCode )` - Define o código de status HTTP (ex: 404, 500)
- `.WithStatusCode( HttpStatusCode statusCode )` - Define o código de status HTTP usando enum (ex: HttpStatusCode.NotFound)
- `.WithStatusCode( Func<TException, int> resolver )` - Resolvedor de código de status dinâmico
- `.WithStatusCode( Func<TException, HttpStatusCode> resolver )` - Resolvedor de código de status dinâmico com enum
- `.WithErrorCode( string code )` - Define a string do código de erro
- `.WithErrorCode( Func<TException, string> resolver )` - Código de erro dinâmico
- `.WithResponse( Func<TException, object> builder )` - Constrói o objeto de resposta
- `.OnBeforeResponse( Action<TException, HttpContext> callback )` - Executa antes de escrever a resposta (para logging, telemetria, etc.)

#### `MapDefaultException()`

Configura o handler de fallback para exceções não mapeadas. Usa a mesma API fluente que `MapException<TException>()`.

#### `AutoMapCommonExceptions( bool includeStackTrace = true )`

Configura automaticamente handlers para exceções comuns do .NET com padrões sensatos. Em modo de desenvolvimento, inclui stack traces formatados para o handler padrão.

### Resolução de Exceções

O middleware usa **resolução consciente de herança** para encontrar o melhor handler correspondente:

1. **Correspondência exata**: Procura por handler registrado para o tipo exato da exceção
2. **Correspondência por herança**: Busca handlers de tipos base, priorizando a correspondência mais específica
3. **Handler padrão**: Retorna ao handler padrão se nenhuma correspondência for encontrada
4. **Fallback integrado**: Retorna erro genérico 500 se nenhum handler estiver configurado

### Formatação de Stack Trace

Quando `AutoMapCommonExceptions()` é usado com stack traces habilitados (padrão em desenvolvimento), os stack traces são automaticamente formatados para legibilidade:

**Antes:**
```
at MyApp.Services.UserService.GetUser(Int32 id) in C:\Projects\MyApp\Services\UserService.cs:line 42
at MyApp.Controllers.UserController.Get(Int32 id) in C:\Projects\MyApp\Controllers\UserController.cs:line 28
```

**Depois:**
```
  at MyApp.Services.UserService.GetUser(Int32 id) in C:\Projects\MyApp\Services\UserService.cs:line 42
  at MyApp.Controllers.UserController.Get(Int32 id) in C:\Projects\MyApp\Controllers\UserController.cs:line 28
```

### Exemplo Completo

```csharp
// Program.cs
using System.Net;

var builder = WebApplication.CreateBuilder( args );

builder.Services.AddGuard( options => {
    // Auto-mapear exceções comuns
    options.AutoMapCommonExceptions( );

    // Exceções de domínio customizadas usando enum
    options
        .MapException<EntityNotFoundException>( )
        .WithStatusCode( HttpStatusCode.NotFound )
        .WithErrorCode( "ENTITY_NOT_FOUND" )
        .WithResponse( ex => new {
            error = $"{ex.EntityType} com ID {ex.EntityId} não encontrado"
        } );

    // Ou usando código de status int
    options
        .MapException<DuplicateEntityException>( )
        .WithStatusCode( 409 )
        .WithErrorCode( "DUPLICATE_ENTITY" )
        .WithResponse( ex => new {
            error = ex.Message,
            conflictingField = ex.FieldName,
            existingId = ex.ExistingEntityId
        } );

    // Código de status dinâmico usando enum
    options
        .MapException<BusinessRuleException>( )
        .WithStatusCode( ex => ex.IsCritical ? HttpStatusCode.Forbidden : HttpStatusCode.UnprocessableEntity )
        .WithErrorCode( "BUSINESS_RULE_VIOLATION" )
        .WithResponse( ex => new {
            error = ex.Message,
            rule = ex.RuleName
        } );
} );

var app = builder.Build( );

// Habilitar tratamento global de exceções
app.UseGuard( );

app.MapControllers( );
app.Run( );
```

```csharp
// Uso no controller - sem necessidade de try/catch!
[ApiController]
[Route( "api/[controller]" )]
public class UsersController : ControllerBase {

    [HttpGet( "{id}" )]
    public async Task<UserDto> GetUser( int id ) {
        // Lança EntityNotFoundException se não encontrado
        // Automaticamente tratado pelo middleware do Guard
        return await _userService.GetByIdAsync( id );
    }
}
```

### Compatibilidade Retroativa

**ValidationException** continua funcionando exatamente como antes. O middleware detecta e trata automaticamente com o formato de erro estruturado existente:

```json
{
    "code": "MULTIPLE_ERRORS",
    "errors": [
        {
            "field": "email",
            "message": "Email é obrigatório",
            "code": "VIOLATION"
        }
    ]
}
```

Nenhuma alteração necessária no código de validação existente!

## Testes

O design de validação torna os testes diretos:

```csharp
[Fact]
public async Task CreateUser_WithInvalidEmail_ShouldFail()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddScoped<IUserService>( sp => mockUserService.Object );
    services.AddGuard();

    var serviceProvider = services.BuildServiceProvider();
    var validator = serviceProvider.GetRequiredService<IValidator>();

    var dto = new CreateUserDto
    {
        Name = "João Silva",
        Email = "email-invalido",
        Age = 25
    };

    // Act & Assert
    var exception = await Assert.ThrowsAsync<ValidationException>(
        () => validator.ValidateAsync( dto, ValidationContextKey.Create ) );

    exception.ValidationResult.Errors.Should().HaveCount( 1 );
    exception.ValidationResult.Errors.First().Field.Should().Be( "Email" );
    exception.ValidationResult.Errors.First().Code.Should().Be( "VIOLATION" );
}

[Fact]
public async Task CreateUser_WithExistingEmail_ShouldReturnConflict()
{
    // Arrange
    mockUserService.Setup( x => x.IsEmailAvailableAsync( "existente@teste.com", It.IsAny<CancellationToken>() ) )
               .ReturnsAsync( false );

    var dto = new CreateUserDto
    {
        Name = "João Silva",
        Email = "existente@teste.com",
        Age = 25
    };

    // Act
    var result = await validator.ValidateAndReturnAsync( dto, ValidationContextKey.Create );

    // Assert
    result.IsValid.Should().BeFalse();
    result.StatusCode.Should().Be( HttpStatusCode.Conflict );
    result.Errors.Should().ContainSingle( e => e.Code == "EMAIL_EXISTS" );
}
```

## Melhores Práticas

1. **Use Validação Consciente de Contexto**: Aproveite `ValidationContextKey` para regras específicas de operação
2. **Mantenha Validação Próxima às Entidades**: Implemente `IValidatable<T>` em DTOs para melhor manutenibilidade
3. **Adicione Middleware**: Use `app.UseGuard()` para tratamento automático de exceções
4. **Regras Assíncronas com Moderação**: Apenas para verificações de banco de dados/API que requerem serviços externos
5. **Mensagens de Erro Significativas**: Forneça mensagens claras e acionáveis para usuários
6. **Use Códigos de Status Customizados**: Defina códigos HTTP apropriados para diferentes falhas de validação
7. **Pare em Falhas Críticas**: Use `SetStopOnFailure()` para regras que impedem validação adicional
8. **Teste a Lógica de Validação**: Teste cenários positivos e negativos
9. **Separe Responsabilidades**: Mantenha validação focada, evite lógica de negócio em validadores
10. **Integração DDD**: Use validação como parte das invariantes do seu modelo de domínio

## Padrões de Arquitetura

### Integração com Padrão Repository

```csharp
public class UserRepository
{
    private readonly IValidator _validator;
    private readonly IDbContext _context;

    public UserRepository( IValidator validator, IDbContext context )
    {
        _validator = validator;
        _context = context;
    }

    public async Task<User> CreateAsync( CreateUserDto dto )
    {
        await _validator.ValidateAsync( dto, ValidationContextKey.Create );

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            Age = dto.Age
        };

        _context.Users.Add( user );
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User> UpdateAsync( int id, UpdateUserDto dto )
    {
        var user = await _context.Users.FindAsync( id );

        if ( user == null )
            throw new NotFoundException( "Usuário não encontrado" );

        await _validator.ValidateAsync( dto, ValidationContextKey.Update );

        user.Name = dto.Name;
        user.Age = dto.Age;

        await _context.SaveChangesAsync();

        return user;
    }
}
```

### Validação de Comando CQRS

```csharp
public class CreateOrderCommandHandler : ICommandHandler<CreateOrderCommand>
{
    private readonly IValidator _validator;
    private readonly IOrderRepository _repository;

    public CreateOrderCommandHandler( IValidator validator, IOrderRepository repository )
    {
        _validator = validator;
        _repository = repository;
    }

    public async Task<CommandResult> HandleAsync( CreateOrderCommand command )
    {
        try
        {
            await _validator.ValidateAsync( command.OrderData, ValidationContextKey.Create );

            var order = await _repository.CreateAsync( command.OrderData );

            return CommandResult.Success();
        }
        catch ( ValidationException ex )
        {
            return CommandResult.Failure( ex.ValidationResult.Errors );
        }
    }
}
```

## Considerações de Performance

1. **Regras Assíncronas**: Use `RespectAsync()` apenas quando necessário (chamadas de banco de dados/API)
2. **Parar em Falha**: Use `SetStopOnFailure()` para regras de validação caras
3. **Filtragem de Contexto**: Use contextos específicos para evitar execução desnecessária de regras
4. **Cache de Serviços**: Faça cache de chamadas de serviço caras em regras de validação assíncronas
5. **Overhead de Reflection**: Impacto mínimo de performance para casos de uso típicos

## Licença

Este projeto é licenciado sob a Licença Apache 2.0 - veja o arquivo LICENSE para detalhes.

## Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para enviar um Pull Request.

## Suporte

Para problemas, questões ou contribuições, visite o [repositório GitLab](https://gitlab.com/dotnet-myth/myth).
