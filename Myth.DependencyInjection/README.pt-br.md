# Myth.DependencyInjection

[![NuGet Version](https://img.shields.io/nuget/v/Myth.DependencyInjection?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.DependencyInjection/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.DependencyInjection?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.DependencyInjection/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](README.md)

Uma biblioteca .NET que simplifica a injeção de dependências com descoberta automática de tipos e registro de serviços baseado em convenções. Elimine código repetitivo e habilite arquiteturas de plugins com capacidades poderosas de escaneamento de assemblies e resolução de tipos.

## Funcionalidades

- **Descoberta de Tipos**: Descubra e escaneie automaticamente assemblies e tipos da aplicação
- **Registro Baseado em Convenções**: Registre automaticamente serviços baseado em convenções de nomenclatura de interfaces
- **Escaneamento de Assemblies**: Carregue e analise todos os assemblies do domínio da aplicação
- **Filtragem de Tipos**: Encontre tipos implementando interfaces específicas ou classes base
- **Detecção de Namespace**: Detecte automaticamente o namespace base da sua aplicação
- **Configuração Mínima**: Reduza código repetitivo de registro de DI
- **Suporte a Plugins**: Habilite arquiteturas de plugins com carregamento dinâmico de tipos

## Instalação

```bash
dotnet add package Myth.DependencyInjection
```

## Início Rápido

### Registro Automático de Serviços

Registre automaticamente todas as implementações de uma interface baseado em convenções de nomenclatura:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;

var services = new ServiceCollection();

// Encontra e registra automaticamente todas as implementações de repositório
services.AddServiceFromType<IRepository>();

// Resultado: IPersonRepository -> PersonRepository (Scoped)
//           IOrderRepository -> OrderRepository (Scoped)
//           IProductRepository -> ProductRepository (Scoped)
```

### Descoberta de Tipos

Descubra e analise tipos na sua aplicação:

```csharp
using Myth.ValueProviders;

// Obter namespace base
var baseNamespace = TypeProvider.BaseApplicationNamespace;
// Retorna: "MyApp" (de MyApp.Domain, MyApp.Services, etc.)

// Obter todos os assemblies da aplicação
var assemblies = TypeProvider.ApplicationAssemblies;
// Retorna: Todos os assemblies carregados da sua aplicação

// Obter todos os tipos concretos
var types = TypeProvider.ApplicationTypes;
// Retorna: Todos os tipos não-abstratos e não-interfaces

// Encontrar tipos implementando interface específica
var handlers = TypeProvider.GetTypesAssignableFrom<ICommandHandler>();
// Retorna: Todos os tipos implementando ICommandHandler
```

## Cenários de Uso

### 1. Auto-Registro do Padrão Repository

Elimine o registro manual de repositórios:

```csharp
// Antes: Registro manual para cada repositório
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IOrderRepository, OrderRepository>();
services.AddScoped<IProductRepository, ProductRepository>();
// ... dezenas mais

// Depois: Registro em uma linha
services.AddServiceFromType<IRepository>();
```

Convenção: O nome da classe de implementação deve conter o nome da interface.
- `IUserRepository` -> `UserRepository` (válido)
- `IOrderRepository` -> `OrderRepository` (válido)
- `IProductRepository` -> `ProductRepositoryImpl` (válido)

### 2. Registro de Handlers CQRS

Registre automaticamente handlers de comando e consulta:

```csharp
// Registrar todos os command handlers
services.AddServiceFromType<ICommandHandler>(ServiceLifetime.Transient);

// Registrar todos os query handlers
services.AddServiceFromType<IQueryHandler>(ServiceLifetime.Transient);

// Registrar todos os event handlers
services.AddServiceFromType<IEventHandler>(ServiceLifetime.Scoped);
```

### 3. Arquitetura de Plugins

Descubra e carregue plugins dinamicamente:

```csharp
// Encontrar todos os tipos de plugin
var pluginTypes = TypeProvider.GetTypesAssignableFrom<IPlugin>();

foreach (var pluginType in pluginTypes) {
    var plugin = (IPlugin)Activator.CreateInstance(pluginType);
    plugin.Initialize();

    Console.WriteLine($"Plugin carregado: {plugin.Name}");
}
```

### 4. Tempo de Vida de Serviço Personalizado

Especifique o tempo de vida do serviço ao registrar:

```csharp
// Registrar como Singleton
services.AddServiceFromType<ICache>(ServiceLifetime.Singleton);

// Registrar como Transient
services.AddServiceFromType<IValidator>(ServiceLifetime.Transient);

// Registrar como Scoped (padrão)
services.AddServiceFromType<IUnitOfWork>(ServiceLifetime.Scoped);
```

### 5. Domain-Driven Design (DDD)

Auto-registre serviços de domínio e repositórios:

```csharp
public class Startup {
    public void ConfigureServices(IServiceCollection services) {
        // Camada de infraestrutura
        services.AddServiceFromType<IRepository>(ServiceLifetime.Scoped);

        // Serviços de domínio
        services.AddServiceFromType<IDomainService>(ServiceLifetime.Scoped);

        // Serviços de aplicação
        services.AddServiceFromType<IApplicationService>(ServiceLifetime.Scoped);
    }
}
```

### 6. Análise e Documentação de Assemblies

Gere documentação sobre a estrutura da sua aplicação:

```csharp
using Myth.ValueProviders;

public class AssemblyAnalyzer {
    public void PrintApplicationStructure() {
        Console.WriteLine($"Namespace Base: {TypeProvider.BaseApplicationNamespace}");

        Console.WriteLine("\nAssemblies:");
        foreach (var assembly in TypeProvider.ApplicationAssemblies) {
            Console.WriteLine($"  - {assembly.GetName().Name} v{assembly.GetName().Version}");
        }

        Console.WriteLine("\nImplementações de Serviços:");
        var services = TypeProvider.GetTypesAssignableFrom<IService>();
        foreach (var service in services) {
            Console.WriteLine($"  - {service.FullName}");
        }
    }
}
```

## Referência da API

### TypeProvider

Classe estática para descoberta de tipos e assemblies.

#### Propriedades

```csharp
// Obtém a primeira parte do namespace da sua aplicação
public static string? BaseApplicationNamespace { get; }

// Obtém todos os assemblies da sua aplicação
public static IEnumerable<Assembly> ApplicationAssemblies { get; }

// Obtém todos os tipos concretos exportados pela sua aplicação
public static IEnumerable<Type> ApplicationTypes { get; }
```

#### Métodos

```csharp
// Obtém tipos derivados ou implementando o tipo especificado
public static IEnumerable<Type> GetTypesAssignableFrom<TType>()
```

**Exemplos:**

```csharp
// Obter namespace base
var ns = TypeProvider.BaseApplicationNamespace;
// "MyCompany" de "MyCompany.ECommerce.Domain"

// Obter todos os assemblies
var assemblies = TypeProvider.ApplicationAssemblies;

// Obter todos os tipos
var allTypes = TypeProvider.ApplicationTypes;

// Obter tipos implementando interface
var repositories = TypeProvider.GetTypesAssignableFrom<IRepository>();
var handlers = TypeProvider.GetTypesAssignableFrom<IHandler>();
var validators = TypeProvider.GetTypesAssignableFrom<IValidator>();
```

### ServiceCollectionExtensions

Métodos de extensão para `IServiceCollection`.

#### Métodos

```csharp
// Adiciona todas as implementações do tipo especificado à coleção de serviços
public static IServiceCollection AddServiceFromType<TType>(
    this IServiceCollection services,
    ServiceLifetime serviceLifetime = ServiceLifetime.Scoped
)
```

**Parâmetros:**
- `TType`: A interface base ou tipo para buscar implementações
- `serviceLifetime`: O tempo de vida do serviço (Scoped, Transient, ou Singleton). Padrão: Scoped

**Retorna:** A coleção de serviços para encadeamento

**Lança:**
- `InterfaceNotFoundException`: Quando uma implementação não tem interface correspondente

**Convenção de Nomenclatura:**
O método encontra implementações combinando o nome da interface dentro do nome da implementação.
- Interface: `IPersonRepository` -> Implementação: `PersonRepository` (combina: "PersonRepository")
- Interface: `IOrderService` -> Implementação: `OrderService` (combina: "OrderService")

**Exemplos:**

```csharp
// Scoped (padrão)
services.AddServiceFromType<IRepository>();

// Transient
services.AddServiceFromType<IValidator>(ServiceLifetime.Transient);

// Singleton
services.AddServiceFromType<ICache>(ServiceLifetime.Singleton);
```

## Melhores Práticas

### 1. Organize por Camadas

Estruture seu registro de DI por camada arquitetural:

```csharp
public void ConfigureServices(IServiceCollection services) {
    // Camada de Persistência
    services.AddServiceFromType<IRepository>(ServiceLifetime.Scoped);

    // Camada de Domínio
    services.AddServiceFromType<IDomainService>(ServiceLifetime.Scoped);

    // Camada de Aplicação
    services.AddServiceFromType<IApplicationService>(ServiceLifetime.Scoped);

    // Camada de Infraestrutura
    services.AddServiceFromType<IExternalService>(ServiceLifetime.Transient);
}
```

### 2. Use Interfaces Marcadoras

Crie interfaces marcadoras para auto-registro:

```csharp
// Interface marcadora para serviços de domínio
public interface IDomainService { }

public interface IOrderService : IDomainService {
    Task<Order> CreateOrderAsync(CreateOrderCommand command);
}

public class OrderService : IOrderService {
    public async Task<Order> CreateOrderAsync(CreateOrderCommand command) {
        // Implementação
    }
}

// Auto-registrar todos os serviços de domínio
services.AddServiceFromType<IDomainService>();
```

### 3. Siga Convenções de Nomenclatura

Garanta nomenclatura consistente para descoberta automática:

```csharp
// Bom: Nome combina com interface
public interface IUserRepository { }
public class UserRepository : IUserRepository { }

// Bom: Nome da implementação contém nome da interface
public interface IProductRepository { }
public class ProductRepositoryImpl : IProductRepository { }

// Ruim: Sem correlação de nomenclatura
public interface IOrderRepository { }
public class OrderDataAccess : IOrderRepository { }  // Não será descoberto automaticamente
```

### 4. Combine com Registro Manual

Use auto-registro para convenções, manual para exceções:

```csharp
// Auto-registrar a maioria dos serviços
services.AddServiceFromType<IRepository>();
services.AddServiceFromType<IService>();

// Registrar manualmente casos especiais
services.AddSingleton<IConfiguration>(configuration);
services.AddScoped<ICurrentUser, CurrentUserAccessor>();
services.AddTransient<IEmailSender, SendGridEmailSender>();
```

### 5. Valide Registros

Verifique que os tipos foram registrados corretamente:

```csharp
var serviceProvider = services.BuildServiceProvider();

// Verificar se serviços críticos estão registrados
var userRepo = serviceProvider.GetService<IUserRepository>();
if (userRepo == null) {
    throw new InvalidOperationException("IUserRepository não registrado");
}

// Ou use GetRequiredService para lançar exceção se não encontrado
var orderService = serviceProvider.GetRequiredService<IOrderService>();
```

## Como Funciona

### Processo de Descoberta de Tipos

1. **Carregamento de Assembly**: Escaneia `AppDomain.CurrentDomain.BaseDirectory` para todos os arquivos `.dll`
2. **Carregamento Dinâmico**: Carrega assemblies ainda não carregados no AppDomain atual
3. **Filtragem**: Exclui assemblies dinâmicos e mantém apenas tipos concretos
4. **Cache**: Resultados são computados uma vez por acesso de propriedade para performance

### Processo de Auto-Registro

1. **Escaneamento de Tipo**: Encontra todos os tipos implementando o tipo/interface base especificado
2. **Combinação de Interface**: Para cada implementação, encontra a interface correspondente por nome
3. **Registro**: Cria um `ServiceDescriptor` e adiciona à coleção de serviços
4. **Validação**: Lança `InterfaceNotFoundException` se nenhuma interface correspondente for encontrada

## Considerações de Performance

- **Escaneamento de Assembly**: Realizado uma vez por acesso a `ApplicationAssemblies`; resultados não são cacheados entre acessos
- **Descoberta de Tipo**: Filtros são aplicados eficientemente usando LINQ
- **Tempo de Registro**: Auto-registro acontece na inicialização da aplicação, não em runtime
- **Bases de Código Grandes**: Para aplicações com centenas de assemblies, considere registro manual para caminhos críticos

## Integração com Outras Bibliotecas Myth

Myth.DependencyInjection funciona perfeitamente com outras bibliotecas Myth:

```csharp
using Myth.Extensions;
using Myth.ValueProviders;

var builder = WebApplication.CreateBuilder(args);

// Auto-registrar repositórios (Myth.Repository)
builder.Services.AddServiceFromType<IRepository>();

// Adicionar suporte a pipeline Flow (Myth.Flow)
builder.Services.AddFlow();

// Adicionar validação Guard (Myth.Guard)
builder.Services.AddGuard();

// Construir com suporte a provedor global
var app = builder.BuildApp();

app.Run();
```

## Solução de Problemas

### InterfaceNotFoundException

**Problema:** `InterfaceNotFoundException: Not found a interface that corresponds to type`

**Solução:** Garanta que o nome da classe de implementação contenha o nome da interface:

```csharp
// Problemático
public interface IUserRepository { }
public class UserDataAccess : IUserRepository { }  // Nome não contém "IUserRepository"

// Corrigido
public interface IUserRepository { }
public class UserRepository : IUserRepository { }  // Contém "UserRepository"
```

### Tipos Não Descobertos

**Problema:** Tipos esperados não são encontrados por `GetTypesAssignableFrom<T>()`

**Solução:**
1. Garanta que o assembly está carregado (referenciado no seu projeto)
2. Verifique que tipos são concretos (não abstratos ou interfaces)
3. Verifique que tipos são públicos e acessíveis

### Múltiplas Interfaces por Implementação

**Problema:** Uma classe implementa múltiplas interfaces

**Solução:** O auto-registro encontra a primeira interface contendo o nome da classe. Para controle preciso, registre manualmente:

```csharp
// Auto-registro escolhe uma interface
services.AddServiceFromType<IService>();

// Registro manual para múltiplas interfaces
services.AddScoped<IUserRepository, UserRepository>();
services.AddScoped<IReadOnlyUserRepository, UserRepository>();
```

## Licença

Este projeto está licenciado sob a Licença Apache 2.0 - veja a [LICENSE](https://opensource.org/licenses/Apache-2.0) para detalhes.

## Contribuindo

Contribuições são bem-vindas! Por favor, sinta-se à vontade para enviar um Pull Request.

## Bibliotecas Relacionadas

- **Myth.Flow**: Orquestração de pipeline com padrão Result
- **Myth.Flow.Actions**: CQRS e arquitetura orientada a eventos
- **Myth.Guard**: Validação fluente e integridade de dados
- **Myth.Repository**: Implementação do padrão repositório genérico
- **Myth.Specification**: Padrão de especificação de consulta