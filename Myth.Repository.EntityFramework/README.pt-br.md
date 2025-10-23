# Myth.Repository.EntityFramework

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Repository.EntityFramework?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Repository.EntityFramework/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Repository.EntityFramework?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Repository.EntityFramework/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

É uma biblioteca .NET para definições de repositórios de acessos a bancos de dados usando Entity Framework.

# ⭐ Funcionalidades
- Definição de contexto base
- Leitura automática de arquivos de mapeamento de entidades
- Implementação de escrita
- Implementação de leitura usando expressões
- Implementação de leitura usando specification
- Trabalhar com transações
- **Registro automático de repositórios com injeção de dependência**

# 🔮 Utilização
Para utilizar basta que o contexto criado herde do [BaseContext](/Contexts/BaseContext.cs). Após isso somente crie os repositórios passando-o como parametro.

## 🕶️ Leitura'

Para leitura diversos métodos podem ser utilizados:

- `GetProviderName`: Retorna o provedor de conexão com o banco de dados
- `AsQueryable`: Retorna uma coleção para execução no banco
- `AsEnumerable`: Retorna uma coleção para execução em memoria]
- `ToListAsync`: Retorna todos os items da coleção
- `Where`: Retorna a coleção com um filtro
- `SearchAsync`: Retorna a coleção filtrada
- `SearchPaginatedAsync`: Retorna a coleção filtrada e paginada
- `CountAsync`: Conta os items da coleção
- `AnyAsync`: Verifica se algum item da coleção atendem um requisito
- `AllAsync`: Verifica se todos os items da coleção atendem um requisito
- `FirstOrDefaultAsync`: Retorna o primeiro item da coleção
- `LastOrDefaultAsync`: Retorna o último item da coleção

## ✍️ Escrita

Para escrita podem ser utilizados os seguinte métodos:

- `AddAsync`: Adiciona um item a coleção
- `AddRangeAsync`: Adiciona multiplos items a coleção
- `RemoveAsync`: Remove um item da coleção
- `RemoveRangeAsync`: Remove multiplos items da coleção
- `UpdateAsync`: Atualiza um item da coleção
- `UpdateRangeAsync`: Atualiza multiplos items da coleção
- `AttachAsync`: Anexa um item a coleção
- `AttachRangeAsync`: Anexa multiplos items a coleção
- `SaveChangesAsync`: Salva todas as alterações
- `ExecuteSqlRawAsync`: Execulta uma query no banco de dados

## 🪄 Unidade de trabalho

As funcionalidades independentes da entidade, são as seguintes:

- `SaveChangesAsync`: Salva todas as alterações
- `ExecuteSqlRawAsync`: Executa uma query no banco de dados
- `BeginTransactionAsync`: Inicia uma transação
- `CommitAsync`: Executa todas as alterações da transação 
- `RollbackAsync`: Desfaz as alterações da transação
- `CreateSavepointAsync`: Cria um checkpoint da transação
- `RollbackToSavepointAsync`: Retorna para um checkpoint da transação

## 🚀 Registro de Injeção de Dependência

A biblioteca oferece registro automático de repositórios para configuração simplificada de injeção de dependência.

### Métodos de Auto-Registro

- `AddRepositories()`: Registra automaticamente todas as implementações de repositório encontradas nos assemblies da aplicação
- `AddRepositoriesFromAssembly(assembly)`: Registra repositórios de um assembly específico
- `AddRepository<TInterface, TImplementation>()`: Registra manualmente um repositório específico
- `AddUnitOfWork<TUnitOfWork>()`: Registra uma implementação de Unit of Work

### Uso Básico

```csharp
// Program.cs - ASP.NET Core
var builder = WebApplication.CreateBuilder(args);

// Adicionar DbContext do Entity Framework
builder.Services.AddDbContext<MeuContexto>(options =>
    options.UseSqlServer(connectionString));

// Registrar automaticamente TODOS os repositórios
builder.Services.AddRepositories();

var app = builder.BuildApp(); // Use BuildApp() ao invés de Build()
```

### Configuração Avançada

```csharp
// Tempo de vida do serviço personalizado
builder.Services.AddRepositories(ServiceLifetime.Transient);

// Registrar de assembly específico
builder.Services.AddRepositoriesFromAssembly(typeof(UsuarioRepository).Assembly);

// Registro manual
builder.Services.AddRepository<IUsuarioRepository, UsuarioRepository>();

// Registrar Unit of Work
builder.Services.AddUnitOfWork<MeuUnitOfWork>();
```

### Exemplo de Implementação de Repositório

```csharp
// 1. Crie sua interface de repositório
public interface IUsuarioRepository : IReadWriteRepositoryAsync<Usuario> {
    // Adicione métodos customizados se necessário
}

// 2. Crie sua implementação de repositório
public class UsuarioRepository : ReadWriteRepositoryAsync<Usuario>, IUsuarioRepository {
    public UsuarioRepository(MeuContexto context) : base(context) { }

    // Implemente métodos customizados se necessário
}

// 3. O repositório será automaticamente registrado por AddRepositories()
```

### Uso em Controllers/Serviços

```csharp
[ApiController]
public class UsuariosController : ControllerBase {
    private readonly IUsuarioRepository _repository;

    public UsuariosController(IUsuarioRepository repository) {
        _repository = repository;
    }

    [HttpGet]
    public async Task<IEnumerable<Usuario>> GetUsuarios() {
        return await _repository.ToListAsync();
    }
}
```

### Comportamento do Registro

- **Tempo de Vida do Serviço**: Scoped por padrão (recomendado para repositórios)
- **Prioridade de Interface**: Interfaces específicas do EntityFramework são registradas antes das interfaces base
- **Filtragem de Testes**: Tipos com "Test", "Mock", "Fake" ou "Stub" em seus nomes são automaticamente excluídos
- **Tipos Genéricos**: Definições de tipos genéricos são automaticamente excluídas