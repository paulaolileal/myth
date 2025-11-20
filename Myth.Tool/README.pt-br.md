<img  style="float: right;" src="myth-tool-logo.png" alt="drawing" width="250"/>

# Myth.Tool - CLI de Geração de Código

Uma ferramenta CLI moderna para gerar código com arquitetura Myth usando padrões CQRS, DDD e Clean Architecture.

## Recursos

- **Configuração de Projeto**: Inicializar estrutura de projeto Myth a partir de templates
- **Modelos de Domínio**: Gerar modelos de domínio com validação e objetos de valor
- **Comandos CQRS**: Criar comandos com handlers e validação
- **Consultas CQRS**: Gerar consultas com handlers e DTOs
- **Eventos CQRS**: Criar eventos de domínio com handlers
- **Padrão Repository**: Gerar repositórios com separação de leitura/escrita
- **Controladores de API**: Criar controladores de API REST com operações CRUD
- **Testes Unitários**: Gerar suítes de teste abrangentes com xUnit e FluentAssertions

## Instalação

Instalar como ferramenta global .NET:

```bash
dotnet tool install -g Myth.Tool
```

Atualizar para a versão mais recente:

```bash
dotnet tool update -g Myth.Tool
```

## Início Rápido

Configurar um novo projeto Myth:

```bash
myth setup MeuProjeto --clean
```

Criar um modelo de domínio:

```bash
myth create model PrevisaoTempo \
  -p PrevisaoTempoId:Guid \
  -p Data:DateOnly \
  -p TemperaturaF:int \
  -p Resumo:string \
  --validate
```

Criar um comando com handler:

```bash
myth create command PrevisaoTempo CriarPrevisaoTempo \
  -p Data:DateOnly \
  -p TemperaturaF:int \
  -p Resumo:string \
  --return Guid \
  --validate
```

## Comandos

### Gerenciamento de Projeto

- `myth setup <NomeProjeto> [--clean]` - Configurar estrutura de projeto Myth a partir de template
- `myth version` - Exibir informações de versão

### Geração de Código

- `myth create model <nome>` - Gerar entidade de domínio
- `myth create command <agregado> <nome>` - Gerar comando CQRS com handler
- `myth create query <agregado> <nome>` - Gerar consulta CQRS com handler
- `myth create event <agregado> <nome>` - Gerar evento de domínio com handler
- `myth create dto <agregado> <nome>` - Gerar objeto de transferência de dados
- `myth create repository <nome>` - Gerar interfaces e implementações de repositório
- `myth create controller <nome>` - Gerar controlador de API com operações CRUD
- `myth create test <controlador>` - Gerar testes unitários para controladores

## Opções

### Opções Globais
- `--dry-run` - Visualizar mudanças sem criar arquivos
- `--force` - Sobrescrever arquivos existentes
- `--path <caminho>` - Especificar diretório de destino
- `--namespace <namespace>` - Namespace personalizado

### Opções de Configuração
- `--clean` - Remover exemplos WeatherForecast e criar contexto base limpo

### Opções de Criação
- `-p <propriedade>` - Adicionar propriedade (formato: Nome:Tipo ou Nome:Tipo:required)
- `--return <tipo>` - Especificar tipo de retorno para comandos/consultas
- `--validate` - Habilitar suporte à validação
- `--events <eventos>` - Especificar eventos para publicar (separados por vírgula)
- `--type <tipo>` - Tipo de repositório: read|write|readwrite (padrão: readwrite)

## Exemplos

### Configuração de Projeto

```bash
# Configurar novo projeto com estrutura limpa
myth setup GerenciamentoPedidos --clean

# Configurar projeto mantendo exemplos
myth setup GerenciamentoPedidos
```

### Geração Completa de CRUD

```bash
# Gerar modelo de domínio
myth create model Usuario \
  -p Id:Guid \
  -p Nome:string:required \
  -p Email:string:required \
  -p CriadoEm:DateTime \
  --validate

# Gerar comando
myth create command Usuario CriarUsuario \
  -p Nome:string:required \
  -p Email:string:required \
  --return Guid \
  --validate \
  --events UsuarioCriado

# Gerar consulta
myth create query Usuario ObterUsuario \
  -p Id:Guid:required \
  --return ObterUsuarioResponse

# Gerar repositório
myth create repository Usuario --type readwrite

# Gerar controlador
myth create controller Usuario

# Gerar testes
myth create test UsuarioController
```

### Exemplos Avançados

```bash
# Criar repositório somente leitura
myth create repository UsuarioLeitura --type read

# Criar comando com múltiplos eventos
myth create command Pedido ProcessarPedido \
  -p PedidoId:Guid:required \
  -p Status:string:required \
  --return bool \
  --events PedidoProcessado,StatusPedidoAlterado

# Criar DTO para transferência de dados complexos
myth create dto Pedido CriarPedidoRequest \
  -p ClienteId:Guid:required \
  -p Itens:"List<ItemPedidoDto>":required \
  -p DataEntrega:DateTime
```

## Estrutura do Projeto

A ferramenta gera código seguindo os princípios de Clean Architecture:

```
SeuProjeto/
├── 🏗️ SeuProjeto.Api/                     # Camada de API Web
│   └── Controllers/                       # Controladores de API REST
├── 🎯 SeuProjeto.Domain/                  # Camada de Domínio
│   ├── Entities/                          # Modelos de Domínio
│   └── Events/                            # Eventos de Domínio
├── 🔄 SeuProjeto.Application/             # Camada de Aplicação
│   ├── Commands/                          # Comandos CQRS
│   ├── Queries/                           # Consultas CQRS
│   ├── Handlers/                          # Handlers de Comando/Consulta/Evento
│   └── DTOs/                              # Objetos de Transferência de Dados
├── 💾 SeuProjeto.Data/                    # Camada de Acesso a Dados
│   ├── Repositories/                      # Implementações de Repositório
│   └── Context/                           # Contexto do Entity Framework
└── 🧪 SeuProjeto.Test/                    # Projetos de Teste
    └── Controllers/                       # Testes Unitários
```

## Recursos do Código Gerado

### Integração com o Framework Myth

O código gerado utiliza:
- **Myth.Flow** - Padrão Pipeline para orquestração de lógica de negócio
- **Myth.Flow.Actions** - Implementação CQRS (ICommand, IQuery, IEvent, IDispatcher)
- **Myth.Guard** - Validação fluente (IValidatable, ValidationBuilder)
- **Myth.Repository** - Padrão Repository com separação de leitura/escrita
- **Myth.Commons** - Extensões e utilitários comuns

### Handlers de Comando
- Implementam `ICommandHandler<TCommand>` ou `ICommandHandler<TCommand, TResponse>`
- Suporte à validação via Myth.Guard
- Publicação de eventos via IDispatcher
- Integração com pipeline para fluxos de trabalho complexos

### Handlers de Consulta
- Implementam `IQueryHandler<TQuery, TResponse>`
- Integração com repositório para acesso a dados
- Suporte para cache e otimização

### Eventos de Domínio
- Implementam interface `IEvent`
- Handlers de evento implementam `IEventHandler<TEvent>`
- Suporte ao processamento assíncrono de eventos

### Repositórios
- Suporte à separação de leitura/escrita
- Padrão de repositório genérico
- Integração com Entity Framework Core
- Padrão Unit of Work

### Controladores
- Operações CRUD completas
- Padrões async/await
- Códigos de status HTTP adequados
- DTOs de request/response

### Testes
- Framework xUnit com FluentAssertions
- Teste de banco de dados em memória
- Mocking de serviços e injeção de dependência
- Herda de `BaseDatabaseTests<TContext>`

## Contribuindo

Esta ferramenta é parte do framework Myth. Para contribuições e problemas, visite o [repositório Myth](https://gitlab.com/dotnet-myth/myth).

## Licença

Licenciado sob a Licença Apache 2.0.