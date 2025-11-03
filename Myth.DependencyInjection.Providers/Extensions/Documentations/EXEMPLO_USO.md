# Myth Advanced Swagger - Exemplo de Uso

## Configuração Básica (Compatível com código existente)

```csharp
// Program.cs - Configuração básica mantém total compatibilidade
builder.Services.AddDocs(settings => {
    settings.UseTitle("My API")
           .UseDescription("API com funcionalidades avançadas")
           .UseContact("Dev Team", "dev@company.com", "https://company.com")
           .UseBearerAuthorization();
});

app.UseDocs();
```

## Configuração Avançada com Todas as Funcionalidades

```csharp
// Program.cs - Configuração com funcionalidades avançadas
builder.Services.AddDocs(settings => {
    settings
        // Configurações básicas
        .UseTitle("Advanced API Documentation")
        .UseDescription("API com TreeView, busca em tempo real, cache e muito mais")
        .UseContact("DevOps Team", "api@company.com", "https://docs.company.com")

        // Autenticação avançada
        .UseAuthentication(
            enableDropdown: true,
            validateTokens: true,
            requireAuth: false
        )
        .UseBearerAuthorization()

        // TreeView hierárquica
        .UseTreeView(
            enableHierarchy: true,
            tagSeparator: "/"
        )

        // Busca em tempo real
        .UseSearch(
            enableRealTime: true,
            searchFields: SearchFields.Name | SearchFields.Description | SearchFields.Path
        )

        // Tema escuro/claro
        .UseTheme(
            defaultTheme: SwaggerTheme.Auto,
            allowUserToggle: true
        )

        // Cache persistente
        .UseCache(
            enablePersistence: true,
            expirationMinutes: 120,
            enableHistory: true
        )

        // UX melhorada
        .UseUI(
            enableKeyboardShortcuts: true,
            enableDirectExecution: true,
            enableJsonBeautify: true,
            enableModelCollapse: true
        )

        // Monitoramento de performance
        .UsePerformance(
            enableTiming: true,
            enableStatusColors: true,
            enableProgressIndicators: true
        )

        // Ou ativar tudo de uma vez com padrões sensatos
        // .UseAdvancedFeatures()
});

// Middleware opcional para autenticação
if (builder.Environment.IsProduction()) {
    app.UseSwaggerAuthentication();
}

app.UseDocs();
```

## Configuração com API Key

```csharp
builder.Services.AddDocs(settings => {
    settings.UseTitle("Secure API")
           .UseApiKeyAuthorization()
           .UseAuthentication(requireAuth: true);
});
```

## Exemplos de Tags Hierárquicas

Para aproveitar a TreeView hierárquica, organize suas tags usando separadores:

```csharp
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase {

    [HttpGet]
    [Tags("Users/Management")]
    public IActionResult GetUsers() { }

    [HttpPost]
    [Tags("Users/Management/Create")]
    public IActionResult CreateUser() { }

    [HttpGet("profile")]
    [Tags("Users/Profile")]
    public IActionResult GetProfile() { }

    [HttpPut("profile")]
    [Tags("Users/Profile/Update")]
    public IActionResult UpdateProfile() { }
}

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase {

    [HttpGet]
    [Tags("Products/Catalog")]
    public IActionResult GetProducts() { }

    [HttpPost]
    [Tags("Products/Catalog/Create")]
    public IActionResult CreateProduct() { }

    [HttpGet("inventory")]
    [Tags("Products/Inventory")]
    public IActionResult GetInventory() { }
}
```

Isso criará uma estrutura hierárquica como:
```
📋 API Endpoints
├── 🔹 Users (4)
│   ├── 📁 Management (2)
│   │   └── 📁 Create (1)
│   └── 📁 Profile (2)
│       └── 📁 Update (1)
└── 🔹 Products (3)
    ├── 📁 Catalog (2)
    │   └── 📁 Create (1)
    └── 📁 Inventory (1)
```

## Funcionalidades Disponíveis

### 🔍 Busca Avançada
- Busca em tempo real enquanto digita
- Busca por nome, método, descrição, tags, path
- Resultados destacados
- Navegação direta para endpoints

### 🌲 TreeView Hierárquica
- Organização baseada em tags
- Suporte a múltiplos níveis (ex: `Users/Profile/Settings`)
- Contadores de endpoints por categoria
- Expansão/colapso individual

### 🎨 Temas
- Claro, escuro e automático (baseado no sistema)
- Alternância com botão
- Persistência da preferência
- Transições suaves

### 💾 Cache Inteligente
- Parâmetros e corpos de requisição persistem entre sessões
- Histórico de requisições por endpoint
- Botões Load/Save/Clear por operação
- Expiração configurável

### ⚡ Execução Direta
- Sem botão "Try it out" - execução direta
- Validação de campos obrigatórios
- Botões específicos por método HTTP
- Feedback visual de carregamento

### ⏱️ Monitoramento
- Tempo de resposta em tempo real
- Status codes coloridos
- Indicadores de progresso
- Histórico de timings

### ⌨️ Atalhos de Teclado
- `Ctrl+Enter`: Executar requisição atual
- `Ctrl+F`: Focar na busca
- `Ctrl+Shift+T`: Alternar tema
- `Ctrl+Shift+F`: Beautificar JSON
- `Ctrl+Delete`: Limpar formulário

### 🔐 Autenticação Avançada
- Dropdown de métodos (Bearer, Basic, API Key)
- Validação contra ASP.NET Core Authentication
- Armazenamento seguro de credenciais
- Middleware de proteção opcional

## Migração do Código Existente

A implementação é **100% compatível** com código existente:

```csharp
// ANTES - continua funcionando exatamente igual
builder.Services.AddDocs(settings => {
    settings.Title = "My API";
    settings.Description = "API Description";
    settings.Type = AuthorizationType.Bearer;
});

// DEPOIS - mesma funcionalidade com fluent API
builder.Services.AddDocs(settings => {
    settings.UseTitle("My API")
           .UseDescription("API Description")
           .UseBearerAuthorization();
});
```

## Personalização

Todas as funcionalidades podem ser habilitadas/desabilitadas individualmente:

```csharp
builder.Services.AddDocs(settings => {
    settings.UseTitle("Custom API")
           .UseTreeView(enableHierarchy: false)  // Desabilita TreeView
           .UseSearch(enableRealTime: false)     // Desabilita busca em tempo real
           .UseTheme(allowUserToggle: false)     // Remove botão de tema
           .UseCache(enablePersistence: false)   // Desabilita cache
           .UseUI(enableDirectExecution: false); // Remove execução direta
});
```