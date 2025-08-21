# Myth.Rest

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Rest?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Rest/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Rest?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Rest/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

Uma poderosa biblioteca .NET para consumir APIs REST com uma interface fluente e encadeável. Construída com funcionalidades de nível empresarial, incluindo políticas de retry avançadas, circuit breakers, injeção de dependência e tratamento abrangente de erros.

# ⭐ Funcionalidades

- **Interface Fluente**: API simples e encadeável
- **Políticas de Retry Avançadas**: Backoff exponencial, jitter, estratégias customizadas
- **Circuit Breaker**: Previne falhas em cascata em sistemas distribuídos
- **Injeção de Dependência**: Integração completa com DI do ASP.NET Core
- **Padrão Factory**: Gerencia múltiplas configurações de API
- **Operações com Arquivos**: Suporte nativo para uploads e downloads
- **Integração com Logging**: Logging estruturado com Microsoft.Extensions.Logging
- **Tipagem Forte**: Tipagem forte com serialização/deserialização automática
- **Suporte a Fallback**: Degradação elegante com respostas de fallback customizadas
- **Orientado a Exceções**: Tratamento claro de erros e exceções customizadas

# 📦 Instalação

```bash
dotnet add package Myth.Rest
```

# 🚀 Início Rápido

## Uso Básico

```csharp
var response = await Rest
    .Create()
    .Configure(config => config
        .WithBaseUrl("https://api.exemplo.com")
        .WithBearerAuthorization("seu-token")
        .WithRetry())
    .DoGet("usuarios")
    .OnResult(result => result
        .UseTypeForSuccess<List<Usuario>>())
    .OnError(error => error
        .ThrowForNonSuccess())
    .BuildAsync();

var usuarios = response.GetAs<List<Usuario>>();
```

## Configuração de Injeção de Dependência

### Program.cs (Minimal API)
```csharp
builder.Services.AddRest(config => config
    .WithBaseUrl("https://api.exemplo.com")
    .WithBearerAuthorization("token")
    .WithRetry());

// Ou use o padrão factory para múltiplas configurações
builder.Services.AddRestFactory()
    .AddRestConfiguration("api1", config => config
        .WithBaseUrl("https://api1.exemplo.com")
        .WithBearerAuthorization("token1"))
    .AddRestConfiguration("api2", config => config
        .WithBaseUrl("https://api2.exemplo.com")
        .WithBasicAuthorization("usuario", "senha"));
```

### Usando em Controllers/Services
```csharp
public class UsuarioService
{
    private readonly IRestRequest _restClient;
    private readonly IRestFactory _restFactory;

    public UsuarioService(IRestRequest restClient, IRestFactory restFactory)
    {
        _restClient = restClient;
        _restFactory = restFactory;
    }

    public async Task<List<Usuario>> ObterUsuariosAsync()
    {
        var response = await _restClient
            .DoGet("usuarios")
            .OnResult(r => r.UseTypeForSuccess<List<Usuario>>())
            .OnError(e => e.ThrowForNonSuccess())
            .BuildAsync();
            
        return response.GetAs<List<Usuario>>();
    }

    public async Task<Produto[]> ObterProdutosDaApi2Async()
    {
        var response = await _restFactory
            .Create("api2") // Usa configuração nomeada
            .DoGet("produtos")
            .OnResult(r => r.UseTypeForSuccess<Produto[]>())
            .BuildAsync();
            
        return response.GetAs<Produto[]>();
    }
}
```

# 🔧 Configuração

## Configuração Básica

```csharp
.Configure(config => config
    .WithBaseUrl("https://api.exemplo.com")
    .WithTimeout(TimeSpan.FromSeconds(30))
    .WithContentType("application/json")
    .WithBearerAuthorization("seu-bearer-token")
    .WithHeader("X-Custom-Header", "valor")
    .WithBodySerialization(CaseStrategy.CamelCase)
    .WithBodyDeserialization(CaseStrategy.SnakeCase))
```

## Configuração Avançada

### HttpClient Customizado
```csharp
.Configure(config => config
    .WithClient(httpClientCustomizado)
    // ou
    .WithHttpClientFactory(httpClientFactory, "cliente-nomeado"))
```

### Conversores de Tipo
```csharp
.Configure(config => config
    .WithTypeConverter<IUsuarioRepository, UsuarioRepository>()) // Mapeamento interface para tipo concreto
```

### Integração com Logging
```csharp
.Configure(config => config
    .WithLogging(logger, logRequests: true, logResponses: true))
```

## Métodos de Autorização

- `.WithAuthorization(esquema, token)`: Header de autorização customizado
- `.WithBearerAuthorization(token)`: Autenticação Bearer token
- `.WithBasicAuthorization(usuario, senha)`: Autenticação básica (auto-codificada)
- `.WithBasicAuthorization(tokenCodificado)`: Autenticação básica com token pré-codificado

# 🔄 Políticas de Retry

A biblioteca fornece mecanismos de retry de nível empresarial seguindo padrões da indústria.

## Padrão Inteligente (Recomendado)
```csharp
.WithRetry() // 3 tentativas, backoff exponencial com jitter, apenas erros de servidor
```

## Estratégias de Retry Personalizadas

### Backoff Exponencial com Jitter (Recomendado)
```csharp
.WithRetry(retry => retry
    .WithMaxAttempts(5)
    .UseExponentialBackoffWithJitter(
        baseDelay: TimeSpan.FromSeconds(1),
        multiplier: 2.0,
        maxDelay: TimeSpan.FromSeconds(30),
        jitterRange: TimeSpan.FromMilliseconds(100))
    .ForServerErrors()
    .ForExceptions(typeof(TaskCanceledException)))
```

### Outras Estratégias

**Backoff Exponencial**
```csharp
.UseExponentialBackoff(TimeSpan.FromSeconds(1), multiplier: 2.0)
```

**Delay Fixo**
```csharp
.UseFixedDelay(TimeSpan.FromSeconds(2))
```

**Delay Aleatório**
```csharp
.UseRandom(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5))
```

### Opções de Configuração de Retry
```csharp
.WithRetry(retry => retry
    .WithMaxAttempts(3)
    .ForServerErrors()                    // Códigos 5xx e 429
    .ForStatusCodes(HttpStatusCode.BadGateway, HttpStatusCode.ServiceUnavailable)
    .ForExceptions(typeof(HttpRequestException), typeof(TaskCanceledException)))
```

# ⚡ Circuit Breaker

Previne falhas em cascata em sistemas distribuídos:

```csharp
var circuitBreaker = new CircuitBreaker(
    failureThreshold: 5,
    timeout: TimeSpan.FromMinutes(1),
    halfOpenRetryTimeout: TimeSpan.FromSeconds(30));

.Configure(config => config
    .WithCircuitBreaker(circuitBreaker))
```

O circuit breaker tem três estados:
- **Fechado (Closed)**: Operação normal
- **Aberto (Open)**: Falhas excederam o limite, requisições são bloqueadas
- **Meio-Aberto (Half-Open)**: Testando se o serviço se recuperou

# 🎯 Operações HTTP

## Requisições GET
```csharp
.DoGet("usuarios/{id}")
.DoGet("produtos?categoria=eletronicos")
```

## Requisições POST/PUT/PATCH
```csharp
.DoPost("usuarios", novoUsuario)
.DoPut("usuarios/123", usuarioAtualizado)
.DoPatch("usuarios/123", atualizacaoParcial)
```

## Requisições DELETE
```csharp
.DoDelete("usuarios/123")
```

# 📁 Operações com Arquivos

## Downloads
```csharp
var response = await Rest
    .Create()
    .Configure(config => config
        .WithBaseUrl("https://api.exemplo.com")
        .WithRetry())
    .DoDownload("arquivos/documento.pdf")
    .OnError(error => error.ThrowForNonSuccess())
    .BuildAsync();

// Salvar em arquivo
await response.SaveToFileAsync("./downloads", "documento.pdf", replaceExisting: true);

// Ou obter como stream
var stream = response.ToStream();
```

## Uploads

### Upload de Stream
```csharp
.DoUpload("arquivos/upload", fileStream, "application/pdf")
```

### Upload de array de bytes
```csharp
.DoUpload("arquivos/upload", bytesDoArquivo, "image/jpeg")
```

### Upload de IFormFile (ASP.NET Core)
```csharp
.DoUpload("arquivos/upload", formFile)
```

### Upload com método HTTP customizado
```csharp
.DoUpload("arquivos/upload", arquivo, settings => settings.UsePutAsMethod())
// Disponíveis: UsePostAsMethod(), UsePutAsMethod(), UsePatchAsMethod()
```

# ✅ Tratamento de Resultados

## Mapeamento de Tipos por Status Code
```csharp
.OnResult(result => result
    .UseTypeForSuccess<Usuario>()                           // Códigos 2xx
    .UseTypeFor<RespostaErro>(HttpStatusCode.BadRequest)
    .UseTypeFor<List<ErroValidacao>>(HttpStatusCode.UnprocessableEntity)
    .UseEmptyFor(HttpStatusCode.NoContent))                 // Resposta vazia para 204
```

## Mapeamento Condicional de Tipos
```csharp
.OnResult(result => result
    .UseTypeFor<RespostaSucesso>(
        HttpStatusCode.OK, 
        body => body.sucesso == true)
    .UseTypeFor<RespostaErro>(
        HttpStatusCode.OK, 
        body => body.sucesso == false))
```

## Múltiplos Status Codes
```csharp
.OnResult(result => result
    .UseTypeFor<RespostaErro>(new[] { 
        HttpStatusCode.BadRequest, 
        HttpStatusCode.Conflict,
        HttpStatusCode.UnprocessableEntity 
    }))
```

# ❌ Tratamento de Erros

## Tratamento Básico de Erros
```csharp
.OnError(error => error
    .ThrowForNonSuccess()                        // Lança para qualquer status não-2xx
    .ThrowFor(HttpStatusCode.Unauthorized)       // Lança para status específico
    .NotThrowFor(HttpStatusCode.NotFound))       // Não lança para 404
```

## Tratamento Condicional de Erros
```csharp
.OnError(error => error
    .ThrowFor(HttpStatusCode.BadRequest, 
        body => body.codigoErro == "ERRO_VALIDACAO"))
```

## Respostas de Fallback
```csharp
.OnError(error => error
    .UseFallback(HttpStatusCode.ServiceUnavailable, new { mensagem = "Serviço temporariamente indisponível" })
    .UseFallback(HttpStatusCode.NotFound, "{}"))
```

# 🏗️ Padrões Avançados

## Padrão Repository
```csharp
public class UsuarioRepository
{
    private readonly IRestRequest _client;

    public UsuarioRepository(IRestRequest client)
    {
        _client = client;
    }

    public async Task<Usuario> ObterUsuarioAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await _client
            .DoGet($"usuarios/{id}")
            .OnResult(r => r.UseTypeForSuccess<Usuario>())
            .OnError(e => e
                .ThrowForNonSuccess()
                .UseFallback(HttpStatusCode.NotFound, new Usuario { Id = id, Nome = "Desconhecido" }))
            .BuildAsync(cancellationToken);

        return response.GetAs<Usuario>();
    }

    public async Task<Usuario> CriarUsuarioAsync(CriarUsuarioRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _client
            .DoPost("usuarios", request)
            .OnResult(r => r
                .UseTypeFor<Usuario>(HttpStatusCode.Created)
                .UseTypeFor<RespostaErroValidacao>(HttpStatusCode.BadRequest))
            .OnError(e => e.ThrowForNonSuccess())
            .BuildAsync(cancellationToken);

        return response.GetAs<Usuario>();
    }
}
```

## Padrão Factory Multi-API
```csharp
public class ServicoApi
{
    private readonly IRestFactory _restFactory;

    public ServicoApi(IRestFactory restFactory)
    {
        _restFactory = restFactory;
    }

    public async Task<PerfilUsuario> ObterPerfilUsuarioAsync(int idUsuario)
    {
        // Obter usuário da API 1
        var responseUsuario = await _restFactory
            .Create("apiUsuario")
            .DoGet($"usuarios/{idUsuario}")
            .OnResult(r => r.UseTypeForSuccess<Usuario>())
            .BuildAsync();

        // Obter preferências da API 2
        var responsePreferencias = await _restFactory
            .Create("apiPreferencias")  
            .DoGet($"preferencias/{idUsuario}")
            .OnResult(r => r.UseTypeForSuccess<PreferenciasUsuario>())
            .OnError(e => e.UseFallback(HttpStatusCode.NotFound, new PreferenciasUsuario()))
            .BuildAsync();

        return new PerfilUsuario
        {
            Usuario = responseUsuario.GetAs<Usuario>(),
            Preferencias = responsePreferencias.GetAs<PreferenciasUsuario>()
        };
    }
}
```

## APIs que Sempre Retornam 200 OK

Para APIs que retornam erros de lógica de negócio como 200 OK:

```csharp
var response = await Rest
    .Create()
    .Configure(config => config
        .WithBaseUrl("https://api-legada.com")
        .WithRetry(retry => retry
            .WithMaxAttempts(2)
            .UseFixedDelay(TimeSpan.FromSeconds(1))))
    .DoGet("usuarios")
    .OnResult(result => result
        .UseTypeFor<List<Usuario>>(HttpStatusCode.OK, body => body.sucesso == true))
    .OnError(error => error
        .ThrowFor(HttpStatusCode.OK, body => body.sucesso == false)
        .ThrowForNonSuccess())
    .BuildAsync();
```

# 🔧 Cenários Empresariais

## E-commerce com Diferentes Estratégias de Retry
```csharp
// Operações críticas - Retry conservador
services.AddRestConfiguration("pedidos", config => config
    .WithBaseUrl("https://api-pedidos.com")
    .WithRetry(retry => retry
        .WithMaxAttempts(2)
        .UseExponentialBackoff(TimeSpan.FromSeconds(2))
        .ForStatusCodes(HttpStatusCode.ServiceUnavailable, HttpStatusCode.TooManyRequests)));

// Operações de leitura - Retry agressivo
services.AddRestConfiguration("catalogo", config => config
    .WithBaseUrl("https://api-catalogo.com")
    .WithRetry(retry => retry
        .WithMaxAttempts(5)
        .UseExponentialBackoffWithJitter(TimeSpan.FromSeconds(1))
        .ForServerErrors()
        .ForExceptions(typeof(TaskCanceledException))));
```

## Comunicação entre Microserviços
```csharp
services.AddRestFactory()
    .AddRestConfiguration("servicoUsuario", config => config
        .WithBaseUrl("https://servico-usuario:8080")
        .WithCircuitBreaker(new CircuitBreaker(5, TimeSpan.FromMinutes(1)))
        .WithRetry())
    .AddRestConfiguration("servicoPedido", config => config
        .WithBaseUrl("https://servico-pedido:8080") 
        .WithCircuitBreaker(new CircuitBreaker(3, TimeSpan.FromMinutes(2)))
        .WithRetry(retry => retry
            .WithMaxAttempts(2)
            .UseFixedDelay(TimeSpan.FromSeconds(3))));
```

## Padrão de Repositório Reutilizável

Para reaproveitar configurações em múltiplas requisições:

```csharp
public class TesteService
{
    private readonly IRestRequest _client;

    public TesteService(IRestRequest client)
    {
        _client = client;
    }

    public async Task<TipoResposta> ObterTesteAsync(CancellationToken cancellationToken)
    {
        var response = await _client
            .DoGet("rota")
            .OnResult(config => config
                .UseTypeForSuccess<TipoResposta>())
            .OnError(error => error
                .ThrowForNonSuccess())
            .BuildAsync(cancellationToken);

        return response.GetAs<TipoResposta>();
    }

    public async Task PostTesteAsync(TipoRequisicao request, CancellationToken cancellationToken)
    {
        await _client
            .DoPost("rota", request)
            .OnResult(config => config
                .UseEmptyFor(HttpStatusCode.NoContent))
            .OnError(error => error
                .ThrowForNonSuccess())
            .BuildAsync(cancellationToken);
    }
}
```

# 📊 Informações da Resposta

Toda resposta contém metadados abrangentes:

```csharp
var response = await Rest.Create()...BuildAsync();

Console.WriteLine($"Status: {response.StatusCode}");
Console.WriteLine($"URL: {response.Url}");
Console.WriteLine($"Método: {response.Method}");
Console.WriteLine($"Tempo Decorrido: {response.ElapsedTime}");
Console.WriteLine($"Tentativas Feitas: {response.RetriesMade}");
Console.WriteLine($"Fallback Usado: {response.FallbackUsed}");
Console.WriteLine($"É Sucesso: {response.IsSuccessStatusCode()}");

// Obter resultado tipado
var usuario = response.GetAs<Usuario>();

// Obter conteúdo bruto
var jsonString = response.ToString();
var bytes = response.ToByteArray();
var stream = response.ToStream();
```

# ⚠️ Tipos de Exceção

A biblioteca fornece exceções específicas para diferentes cenários:

- `NonSuccessException`: Lançada para códigos de status de erro HTTP
- `NotMappedResultTypeException`: Quando nenhum mapeamento de tipo é encontrado para um status code
- `DifferentResponseTypeException`: Ao tentar converter para tipo errado
- `ParsingTypeException`: Quando a deserialização JSON falha
- `FileAlreadyExistsOnDownloadException`: Quando arquivo de download já existe
- `NoActionMadeException`: Quando nenhuma ação HTTP foi definida
- `CircuitBreakerOpenException`: Quando o circuit breaker está aberto

# 🎛️ Diagnósticos e Monitoramento

A biblioteca inclui diagnósticos integrados usando a API Activity do .NET:

```csharp
// Activities são criadas automaticamente com tags:
// - http.url
// - http.method
// - Tempo da operação
```

A integração com OpenTelemetry e outras ferramentas de observabilidade é perfeita.

# 📋 Melhores Práticas

1. **Use Injeção de Dependência**: Registre clientes REST como serviços para melhor testabilidade
2. **Configure Políticas de Retry**: Sempre use políticas de retry para cenários de produção
3. **Implemente Circuit Breakers**: Previna falhas em cascata em microserviços
4. **Trate Erros com Elegância**: Use fallbacks para operações não-críticas
5. **Use Respostas Tipadas**: Aproveite a tipagem forte para melhor manutenibilidade do código
6. **Configure Timeouts**: Defina timeouts apropriados para seus cenários
7. **Registre Requisições/Respostas**: Habilite logging para depuração e monitoramento
8. **Use Configurações Nomeadas**: Use o padrão factory para múltiplas integrações de API

# 📄 Licença

Este projeto está licenciado sob a Licença Apache 2.0 - veja o arquivo LICENSE para detalhes.