# Myth.DependencyInjection.Providers

[![NuGet Version](https://img.shields.io/nuget/v/Myth.DependencyInjection.Providers?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.DependencyInjection.Providers/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.DependencyInjection.Providers?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.DependencyInjection.Providers/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

Uma biblioteca .NET que fornece configuração pré-pronta de injeção de dependências para bibliotecas terceiras comumente utilizadas em aplicações ASP.NET Core. Simplifica a integração de versionamento de API, documentação Swagger/OpenAPI avançada com interface moderna, e AutoMapper com padrões prontos para produção.

## Por que Myth.DependencyInjection.Providers?

Aplicações modernas ASP.NET Core requerem configuração consistente em múltiplos projetos para versionamento, documentação de API e mapeamento de objetos. Esta biblioteca elimina código de configuração repetitivo fornecendo:

- Versionamento de API pronto para produção com suporte a múltiplos leitores (URL, header, media type)
- **Documentação Swagger/OpenAPI avançada** com navegação hierárquica, busca em tempo real e interface moderna
- Documentação versionada com geração automática de endpoints e experiência de desenvolvedor aprimorada
- Integração com AutoMapper incluindo suporte a paginação
- Métodos de extensão para mapeamento simplificado de objetos em toda sua aplicação
- Configuração mínima com padrões sensatos e opções de personalização poderosas

## Funcionalidades

- **Versionamento de API**: Versionamento completo com suporte a URL segment, header e media type
- **Swagger/OpenAPI Avançado**: Interface de documentação moderna com experiência de desenvolvedor aprimorada
  - 🌲 **TreeView Hierárquica** - Endpoints organizados por tags com suporte a múltiplos níveis
  - 🔍 **Busca em Tempo Real** - Filtros dinâmicos por nome, método, descrição e caminho
  - 🎨 **Tema Claro/Escuro** - Detecção automática com alternância manual e persistência de preferência
  - ⚡ **Execução Direta** - Teste de API com um clique sem botão "Try it out"
  - 💾 **Cache Persistente** - Salva parâmetros e corpos de requisição entre sessões do navegador
  - 🔐 **Autenticação Avançada** - Suporte a Bearer, Basic e API Key com seleção via dropdown
  - ⌨️ **Atalhos de Teclado** - Recursos para usuários avançados (Ctrl+Enter, Ctrl+F, etc.)
  - 📊 **Monitoramento de Performance** - Timing de requisições, códigos de status coloridos e feedback visual
  - ✨ **UX Aprimorada** - JSON beautify, colapso de modelos, validação e histórico de requisições
- **Integração AutoMapper**: Configuração simplificada com mapeamentos de tipos de paginação e acesso global
- **Extensões de Mapeamento de Tipos**: Métodos de extensão estáticos para transformações de objetos convenientes
- **Integração de Autenticação**: Validação de autenticação ASP.NET Core e armazenamento seguro de credenciais
- **Experiência do Desenvolvedor**: APIs fluentes, código boilerplate mínimo e 100% de compatibilidade retroativa

## Instalação

```bash
dotnet add package Myth.DependencyInjection.Providers
```

## Início Rápido

### Exemplo de Configuração Completa

```csharp
using Myth.Extensions;

var builder = WebApplication.CreateBuilder( args );

builder.Services.AddControllers( );

builder.Services.AddVersioning( 1.0 );

// Configuração básica (100% compatível com versões anteriores)
builder.Services.AddDocs( settings => {
    settings.UseTitle( "Minha API" )
           .UseDescription( "Uma API abrangente para gerenciamento de recursos" )
           .UseContact( "Equipe de API", "api@minhaempresa.com", "https://minhaempresa.com/api" )
           .UseBearerAuthorization( );
} );

// OU com funcionalidades avançadas habilitadas
builder.Services.AddDocs( settings => {
    settings.UseTitle( "Documentação Avançada da API" )
           .UseDescription( "API com interface moderna e experiência de desenvolvedor aprimorada" )
           .UseContact( "Equipe de API", "api@minhaempresa.com", "https://minhaempresa.com/api" )

           // Habilitar todas as funcionalidades avançadas com padrões sensatos
           .UseAdvancedFeatures( )

           // Ou configurar individualmente
           .UseTreeView( enableHierarchy: true, tagSeparator: "/" )
           .UseSearch( enableRealTime: true )
           .UseTheme( SwaggerTheme.Auto, allowUserToggle: true )
           .UseCache( enablePersistence: true, expirationMinutes: 60 )
           .UseAuthentication( enableDropdown: true, validateTokens: false )
           .UseUI( enableDirectExecution: true, enableKeyboardShortcuts: true )
           .UsePerformance( enableTiming: true, enableStatusColors: true );
} );

builder.Services.AddTypeMapping( conf => {
    conf.CreateMap<UserEntity, UserDto>( );
    conf.CreateMap<OrderEntity, OrderDto>( );
} );

var app = builder.Build( );

// Opcional: Proteger Swagger com autenticação em produção
if ( app.Environment.IsProduction( ) ) {
    app.UseSwaggerAuthentication( );
}

app.UseDocs( );
app.UseAuthorization( );
app.MapControllers( );

app.Run( );
```

## Versionamento de API

### Configuração

A extensão `AddVersioning` configura o versionamento de API do ASP.NET Core com múltiplos leitores de versão:

```csharp
services.AddVersioning( 1.0 );
```

**Recursos:**
- Versionamento por segmento de URL: `/api/v1/users`
- Versionamento por header: `X-API-Version: 1.0`
- Versionamento por media type: `Accept: application/json;v=1.0`
- Relatório automático de versão nos headers de resposta
- Assumir versão padrão para requisições não especificadas

### Configuração do Controller

Os controllers devem ser decorados com atributos de versão:

```csharp
[ApiController]
[ApiVersion( "1.0" )]
[Route( "api/v{version:apiVersion}/[controller]" )]
public class UsersController : ControllerBase {

    [HttpGet]
    public IActionResult GetUsers( ) {
        return Ok( new[] { "User1", "User2" } );
    }
}
```

### Múltiplas Versões

```csharp
[ApiController]
[ApiVersion( "1.0" )]
[ApiVersion( "2.0" )]
[Route( "api/v{version:apiVersion}/[controller]" )]
public class ProductsController : ControllerBase {

    [HttpGet]
    [MapToApiVersion( "1.0" )]
    public IActionResult GetProductsV1( ) {
        return Ok( "Resposta da versão 1" );
    }

    [HttpGet]
    [MapToApiVersion( "2.0" )]
    public IActionResult GetProductsV2( ) {
        return Ok( "Resposta da versão 2" );
    }
}
```

### Descontinuando Versões

```csharp
[ApiController]
[ApiVersion( "1.0", Deprecated = true )]
[ApiVersion( "2.0" )]
[Route( "api/v{version:apiVersion}/[controller]" )]
public class LegacyController : ControllerBase {
}
```

## Documentação Swagger/OpenAPI Avançada

### Interface Moderna com Funcionalidades Aprimoradas

A biblioteca fornece uma interface Swagger UI completamente redesenhada com funcionalidades modernas que melhoram significativamente a experiência do desenvolvedor, mantendo 100% de compatibilidade retroativa.

#### Configuração Básica (Compatível com Versões Anteriores)

```csharp
// Configuração tradicional ainda funciona exatamente igual
services.AddDocs( settings => {
    settings.UseTitle( "API E-Commerce" )
           .UseDescription( "API RESTful para operações de e-commerce" )
           .UseContact( "Equipe de Desenvolvimento", "dev@ecommerce.com", "https://ecommerce.com/docs" )
           .UseBearerAuthorization( );
} );

app.UseDocs( );
```

#### Configuração Avançada com Funcionalidades Modernas

```csharp
services.AddDocs( settings => {
    settings.UseTitle( "API E-Commerce Moderna" )
           .UseDescription( "API com funcionalidades avançadas de documentação" )
           .UseContact( "Equipe de API", "api@ecommerce.com", "https://ecommerce.com/docs" )

           // Autenticação com suporte a dropdown
           .UseAuthentication(
               enableDropdown: true,        // Mostrar seletor de método de auth
               validateTokens: true,        // Validar contra ASP.NET Core auth
               requireAuth: false           // Exigir auth para acessar Swagger
           )
           .UseBearerAuthorization( )       // Método principal de auth

           // Navegação hierárquica
           .UseTreeView(
               enableHierarchy: true,       // Agrupar endpoints por tags
               tagSeparator: "/"            // Suporte a categorias aninhadas
           )

           // Busca em tempo real
           .UseSearch(
               enableRealTime: true,        // Buscar conforme digita
               searchFields: SearchFields.Name | SearchFields.Description | SearchFields.Path
           )

           // Suporte a temas
           .UseTheme(
               defaultTheme: SwaggerTheme.Auto,  // Respeitar preferência do sistema
               allowUserToggle: true             // Mostrar botão de alternância de tema
           )

           // Cache persistente
           .UseCache(
               enablePersistence: true,     // Salvar dados entre sessões
               expirationMinutes: 120,      // Expiração do cache
               enableHistory: true          // Manter histórico de requisições
           )

           // UX aprimorada
           .UseUI(
               enableKeyboardShortcuts: true,   // Ctrl+Enter, Ctrl+F, etc.
               enableDirectExecution: true,     // Sem botão "Try it out"
               enableJsonBeautify: true,        // Auto-formatar JSON
               enableModelCollapse: true        // Seções de modelo colapsáveis
           )

           // Monitoramento de performance
           .UsePerformance(
               enableTiming: true,          // Mostrar timing de requisições
               enableStatusColors: true,    // Colorir códigos de status HTTP
               enableProgressIndicators: true
           );
} );

// Opcional: Proteger Swagger em produção
if ( app.Environment.IsProduction( ) ) {
    app.UseSwaggerAuthentication( );  // Exigir auth para acessar Swagger
}

app.UseDocs( );
```

#### Configuração Rápida com Todas as Funcionalidades

```csharp
services.AddDocs( settings => {
    settings.UseTitle( "Minha API Avançada" )
           .UseDescription( "API com todas as funcionalidades modernas habilitadas" )
           .UseContact( "Equipe Dev", "dev@empresa.com", "https://empresa.com" )
           .UseBearerAuthorization( )
           .UseAdvancedFeatures( );  // Habilita tudo com padrões sensatos
} );
```

### Organização Hierárquica com Tags

Para aproveitar a funcionalidade TreeView, organize seus endpoints usando tags hierárquicas:

```csharp
[ApiController]
[Route( "api/[controller]" )]
public class UsersController : ControllerBase {

    [HttpGet]
    [Tags( "Usuários/Gerenciamento" )]        // Cria: Usuários → Gerenciamento
    public IActionResult GetUsers( ) { }

    [HttpPost]
    [Tags( "Usuários/Gerenciamento/Criar" )] // Cria: Usuários → Gerenciamento → Criar
    public IActionResult CreateUser( ) { }

    [HttpGet( "profile" )]
    [Tags( "Usuários/Perfil" )]               // Cria: Usuários → Perfil
    public IActionResult GetProfile( ) { }

    [HttpPut( "profile/avatar" )]
    [Tags( "Usuários/Perfil/Avatar" )]        // Cria: Usuários → Perfil → Avatar
    public IActionResult UpdateAvatar( ) { }
}
```

Isso cria uma estrutura hierárquica na interface do Swagger:
```
📋 Endpoints da API
└── 🔹 Usuários (4)
    ├── 📁 Gerenciamento (2)
    │   └── 📁 Criar (1)
    └── 📁 Perfil (2)
        └── 📁 Avatar (1)
```

### Métodos de Autenticação

#### Autenticação por API Key

```csharp
services.AddDocs( settings => {
    settings.UseApiKeyAuthorization( )
           .UseAuthentication( enableDropdown: true );
} );
```

#### Múltiplos Métodos de Autenticação

```csharp
services.AddDocs( settings => {
    settings.UseTitle( "API Segura" )
           .UseAuthentication(
               enableDropdown: true,        // Mostrar dropdown para alternar métodos
               validateTokens: true,        // Validar tokens no servidor
               requireAuth: app.Environment.IsProduction()  // Exigir auth apenas em prod
           )
           .UseBearerAuthorization( );     // Método padrão
} );
```

### Visão Geral das Funcionalidades Principais

#### 🔍 **Busca em Tempo Real**
- Buscar endpoints por nome, método HTTP, descrição ou caminho
- Resultados instantâneos com destaque
- Navegar diretamente para endpoints correspondentes
- Campos de busca e debouncing configuráveis

#### 🌲 **TreeView Hierárquica**
- Organizar endpoints por tags com níveis de aninhamento ilimitados
- Expandir/colapsar seções individualmente
- Mostrar contagem de endpoints por categoria
- Estrutura de navegação limpa e intuitiva

#### ⚡ **Execução Direta**
- Sem botão "Try it out" - executar requisições diretamente
- Botões específicos por método (🔍 Buscar, 📤 Criar, 🗑️ Excluir)
- Validação de campos obrigatórios antes da execução
- Indicadores visuais de carregamento e feedback de progresso

#### 💾 **Cache Persistente**
- Salvar parâmetros e corpos de requisição entre sessões do navegador
- Controles de cache individuais por endpoint (Carregar/Salvar/Limpar)
- Histórico de requisições com retenção configurável
- Armazenamento seguro e isolado por domínio

#### ⌨️ **Atalhos de Teclado**
- `Ctrl+Enter`: Executar requisição atual
- `Ctrl+F`: Focar na caixa de busca
- `Ctrl+Shift+T`: Alternar tema
- `Ctrl+Shift+F`: Beautificar JSON
- `Ctrl+Delete`: Limpar formulário atual

#### 📊 **Monitoramento de Performance**
- Exibição de timing de requisições em tempo real
- Códigos de status HTTP coloridos
- Informações de tamanho de resposta e cabeçalhos
- Histórico de requisições com métricas de performance

#### 🎨 **Temas Modernos**
- Modo escuro/claro automático baseado na preferência do sistema
- Alternância manual de tema com persistência
- Transições suaves e esquemas de cores modernos
- Suporte a alto contraste e acessibilidade

### Compatibilidade com API Legada

As novas funcionalidades avançadas são totalmente compatíveis com versões anteriores. O código existente continua funcionando sem alterações:

```csharp
// Isso ainda funciona exatamente igual
services.AddSwaggerVersioned( settings => {
    settings.Title = "API Legada";
    settings.Options.UseBasicAuthorization( );
} );

app.UseSwaggerVersioned( );
```

### Documentação XML

O Swagger inclui automaticamente comentários XML do seu assembly. Habilite a documentação XML no arquivo do projeto:

```xml
<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

Em seguida, documente seus endpoints:

```csharp
/// <summary>
/// Recupera todos os usuários ativos do sistema
/// </summary>
/// <returns>Uma lista de objetos de usuário</returns>
/// <response code="200">Retorna a lista de usuários</response>
/// <response code="401">Se o usuário não está autenticado</response>
[HttpGet]
[ProducesResponseType( StatusCodes.Status200OK )]
[ProducesResponseType( StatusCodes.Status401Unauthorized )]
public IActionResult GetUsers( ) {
    return Ok( users );
}
```

### Acessando a UI do Swagger

Após a configuração, a UI do Swagger está disponível em:
- `https://localhost:5001/swagger`

Cada versão da API obtém seu próprio endpoint:
- `https://localhost:5001/swagger/v1/swagger.json`
- `https://localhost:5001/swagger/v2/swagger.json`

## Integração com AutoMapper

### Configuração

```csharp
services.AddTypeMapping( conf => {
    conf.CreateMap<UserEntity, UserDto>( );
    conf.CreateMap<CreateUserRequest, UserEntity>( );
    conf.CreateMap<UpdateUserRequest, UserEntity>( );
} );
```

### Mapeamento de Paginação Integrado

A biblioteca configura automaticamente o mapeamento para tipos de paginação:

```csharp
IPaginated<UserEntity> paginatedEntities = repository.GetPaginated( );
IPaginated<UserDto> paginatedDtos = paginatedEntities.MapTo<IPaginated<UserDto>>( );
```

### Usando Perfis do AutoMapper

```csharp
public class UserMappingProfile : Profile {

    public UserMappingProfile( ) {
        CreateMap<UserEntity, UserDto>( )
            .ForMember( dest => dest.FullName, opt => opt.MapFrom( src => $"{src.FirstName} {src.LastName}" ) )
            .ForMember( dest => dest.IsActive, opt => opt.MapFrom( src => src.Status == UserStatus.Active ) );

        CreateMap<CreateUserRequest, UserEntity>( )
            .ForMember( dest => dest.Id, opt => opt.Ignore( ) )
            .ForMember( dest => dest.CreatedAt, opt => opt.MapFrom( _ => DateTime.UtcNow ) );
    }
}

services.AddTypeMapping( );
```

Os perfis são automaticamente descobertos e registrados de todos os assemblies da aplicação.

## Extensões de Mapeamento de Tipos

A biblioteca fornece métodos de extensão convenientes para mapeamento de objetos disponíveis em toda sua aplicação:

### Mapeamento Síncrono

```csharp
var user = userEntity.MapTo<UserDto>( );

var users = userEntities.Select( e => e.MapTo<UserDto>( ) ).ToList( );
```

### Mapeamento Assíncrono

Para operações assíncronas que retornam objetos mapeados:

```csharp
public async Task<UserDto> GetUserAsync( int id ) {
    return await repository.GetByIdAsync( id ).MapToAsync<UserEntity, UserDto>( );
}

public async ValueTask<OrderDto> GetOrderAsync( int id ) {
    return await repository.GetOrderAsync( id ).MapToAsync<OrderEntity, OrderDto>( );
}
```

### Tratamento de Exceções

Se `AddTypeMapping` não foi chamado, os métodos de mapeamento lançam `TypeMappingNotConfiguredException`:

```csharp
try {
    var dto = entity.MapTo<UserDto>( );
}
catch ( TypeMappingNotConfiguredException ex ) {
    logger.LogError( ex, "AutoMapper não configurado" );
}
```

## Exemplos do Mundo Real

### Integração com Domain-Driven Design

```csharp
public class OrdersController : ControllerBase {
    private readonly IOrderRepository _repository;

    public OrdersController( IOrderRepository repository ) {
        _repository = repository;
    }

    /// <summary>
    /// Cria um novo pedido
    /// </summary>
    [HttpPost]
    [ApiVersion( "1.0" )]
    public async Task<IActionResult> CreateOrder( CreateOrderRequest request ) {
        var orderEntity = request.MapTo<OrderEntity>( );

        await _repository.AddAsync( orderEntity );

        return CreatedAtAction(
            nameof( GetOrder ),
            new { id = orderEntity.Id },
            orderEntity.MapTo<OrderDto>( )
        );
    }

    /// <summary>
    /// Recupera pedidos paginados
    /// </summary>
    [HttpGet]
    [ApiVersion( "1.0" )]
    public async Task<IActionResult> GetOrders( [FromQuery] int page = 1, [FromQuery] int pageSize = 20 ) {
        var paginatedOrders = await _repository.GetPaginatedAsync( page, pageSize );

        var result = paginatedOrders.MapTo<IPaginated<OrderDto>>( );

        return Ok( result );
    }
}
```

### Integração com Camada de Serviço

```csharp
public class UserService : IUserService {
    private readonly IUserRepository _repository;

    public UserService( IUserRepository repository ) {
        _repository = repository;
    }

    public async Task<UserDto> GetUserByEmailAsync( string email ) {
        var user = await _repository.GetByEmailAsync( email );

        return user.MapTo<UserDto>( );
    }

    public async Task<IPaginated<UserDto>> SearchUsersAsync( string searchTerm, int page, int pageSize ) {
        var users = await _repository.SearchPaginatedAsync( searchTerm, page, pageSize );

        return users.MapTo<IPaginated<UserDto>>( );
    }
}
```

### Padrão CQRS

```csharp
public class GetUserQueryHandler : IQueryHandler<GetUserQuery, UserDto> {
    private readonly IUserRepository _repository;

    public GetUserQueryHandler( IUserRepository repository ) {
        _repository = repository;
    }

    public async Task<QueryResult<UserDto>> HandleAsync( GetUserQuery query, CancellationToken cancellationToken ) {
        var user = await _repository.GetByIdAsync( query.UserId, cancellationToken );

        if ( user == null )
            return QueryResult<UserDto>.NotFound( );

        return QueryResult<UserDto>.Success( user.MapTo<UserDto>( ) );
    }
}
```

## Referência de Configuração

### Métodos de Configuração Avançada do Swagger

| Método | Parâmetros | Descrição |
|--------|------------|-----------|
| `UseTitle(string)` | title | Título da API exibido na UI do Swagger |
| `UseDescription(string)` | description | Descrição e propósito da API |
| `UseContact(string, string, string)` | name, email, url | Informações de contato |
| `UseBearerAuthorization()` | - | Habilitar autenticação JWT Bearer |
| `UseBasicAuthorization()` | - | Habilitar autenticação Basic |
| `UseApiKeyAuthorization()` | - | Habilitar autenticação por API Key |
| `UseAdvancedFeatures()` | - | Habilitar todas as funcionalidades avançadas com padrões |

#### Configuração de Funcionalidades Avançadas

| Método | Parâmetros | Descrição |
|--------|------------|-----------|
| `UseTreeView(bool, string)` | enableHierarchy, tagSeparator | Organização hierárquica de endpoints |
| `UseSearch(bool, SearchFields)` | enableRealTime, searchFields | Configuração de busca em tempo real |
| `UseTheme(SwaggerTheme, bool)` | defaultTheme, allowUserToggle | Configurações de tema e aparência |
| `UseCache(bool, int, bool)` | enablePersistence, expirationMinutes, enableHistory | Configuração de cache persistente |
| `UseAuthentication(bool, bool, bool)` | enableDropdown, validateTokens, requireAuth | Configurações avançadas de autenticação |
| `UseUI(bool, bool, bool, bool)` | enableKeyboardShortcuts, enableDirectExecution, enableJsonBeautify, enableModelCollapse | Melhorias de UI/UX |
| `UsePerformance(bool, bool, bool)` | enableTiming, enableStatusColors, enableProgressIndicators | Monitoramento de performance |

#### Exemplos de Configuração

**Configuração Mínima:**
```csharp
settings.UseTitle("Minha API").UseBearerAuthorization();
```

**Configuração para Produção:**
```csharp
settings.UseTitle("API de Produção")
       .UseDescription("API segura com funcionalidades avançadas")
       .UseContact("Equipe de API", "api@empresa.com", "https://docs.empresa.com")
       .UseAuthentication(enableDropdown: true, validateTokens: true, requireAuth: true)
       .UseAdvancedFeatures();
```

**Seleção de Funcionalidades Personalizada:**
```csharp
settings.UseTitle("API Personalizada")
       .UseTreeView(enableHierarchy: true, tagSeparator: "::")
       .UseSearch(enableRealTime: false)  // Desabilitar busca em tempo real
       .UseTheme(SwaggerTheme.Dark, allowUserToggle: false)  // Forçar tema escuro
       .UseCache(enablePersistence: false)  // Desabilitar cache
       .UseUI(enableDirectExecution: false);  // Manter botão "Try it out"
```

### Propriedades do SwaggerSettings Legado (Ainda Suportadas)

| Propriedade | Tipo | Descrição | Obrigatório |
|-------------|------|-----------|-------------|
| `Title` | string | Título da API exibido na UI do Swagger | Sim |
| `Description` | string | Descrição e propósito da API | Sim |
| `DeprecatedDescription` | string | Mensagem exibida para versões descontinuadas | Não (padrão: "This version of API is deprecated!") |
| `ContactName` | string | Nome da pessoa ou equipe de contato | Não |
| `ContactEmail` | string | Endereço de e-mail de contato | Não |
| `ContactUrl` | string | URL de documentação ou suporte | Não |
| `Options` | SwaggerGenOptions | Acesso à configuração subjacente do Swagger | Não |

### Configuração de Versionamento

O método `AddVersioning` configura:
- Versão padrão da API (especificada por parâmetro)
- Leitor de segmento de URL: `/api/v1/...`
- Leitor de header: `X-API-Version: 1.0`
- Leitor de media type: `application/json;v=1.0`
- API explorer com substituição de versão
- Relatório de versão nos headers de resposta

## Dependências

- **Asp.Versioning.Mvc** (8.1.0): Framework de versionamento de API
- **Asp.Versioning.Mvc.ApiExplorer** (8.1.0): API explorer para endpoints versionados
- **AutoMapper** (13.0.1): Mapeamento objeto-para-objeto
- **Swashbuckle.AspNetCore** (6.6.2): Implementação Swagger/OpenAPI
- **Swashbuckle.AspNetCore.Annotations** (6.6.2): Suporte a anotações do Swagger
- **Myth.DependencyInjection**: Descoberta de tipos e varredura de assemblies
- **Myth.Repository**: Interfaces de paginação

## Benefícios Arquiteturais

Esta biblioteca promove arquitetura limpa ao:

1. **Separação de Responsabilidades**: DTOs para contratos de API, entidades para lógica de domínio
2. **Estratégia de Versionamento**: Evolução graciosa da API sem quebrar clientes
3. **Documentação**: Documentação automática de API sincronizada com código
4. **Segurança de Tipos**: Mapeamentos verificados em tempo de compilação entre camadas
5. **Consistência**: Configuração padronizada entre microserviços
6. **Testabilidade**: Fácil de mockar e testar com injeção de dependências

## Melhores Práticas

1. **Sempre versione suas APIs** desde o início, mesmo tendo apenas v1
2. **Use comentários XML** extensivamente para documentação Swagger abrangente
3. **Crie DTOs dedicados** para cada versão da API para manter compatibilidade retroativa
4. **Organize perfis do AutoMapper** por agregado de domínio ou contexto delimitado
5. **Configure mapeamentos de paginação** para todos os endpoints de coleção
6. **Use versionamento semântico** (1.0, 1.1, 2.0) para versões de API
7. **Documente mudanças quebradas** nas descrições de versões descontinuadas
8. **Teste mapeamentos** com testes unitários para capturar erros de configuração cedo

## Licença

Licenciado sob a Licença Apache, Versão 2.0. Veja o arquivo LICENSE para detalhes.