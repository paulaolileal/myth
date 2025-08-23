# Myth.Morph

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Morph?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Morph/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Morph?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Morph/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

Uma biblioteca .NET poderosa para transformação e mapeamento de objetos. Myth.Morph fornece um sistema flexível e extensível para conversão entre diferentes tipos com suporte para mapeamentos baseados em convenção e personalizados.

O objetivo principal é simplificar cenários de mapeamento de objetos fornecendo alta flexibilidade e desempenho através de integração com injeção de dependência e configuração baseada em esquemas.

# ⭐ Funcionalidades

- **Simples e Intuitivo**: Métodos de extensão fáceis de usar para transformação de objetos
- **Mapeamento Flexível**: Suporte para mapeamentos automáticos, personalizados e baseados em instância
- **Integração com Injeção de Dependência**: Integração completa com Microsoft.Extensions.DependencyInjection
- **Suporte a Tipos Genéricos**: Mapeamento automático para coleções genéricas e interfaces
- **Operações Assíncronas**: Suporte integrado para binding assíncrono de propriedades
- **Integração com Logging**: Logging abrangente através do Microsoft.Extensions.Logging
- **Segurança de Exceções**: Tratamento detalhado de exceções com tipos de exceção personalizados
- **Configuração Baseada em Esquema**: API fluente para configurar mapeamentos complexos

# 🕶️ Como Usar

## 🚀 Início Rápido

### Instalação e Configuração

Primeiro, registre o Myth.Morph no seu container de injeção de dependência:

```csharp
services.AddMorph();
```

### Uso Básico

Transforme objetos usando os métodos de extensão:

```csharp
// Transformação simples
var destino = origem.To<TipoDestino>();

// Transformar com service provider personalizado
var destino = origem.To<TipoDestino>(serviceProvider);

// Transformar coleções
var listaDestino = listaOrigem.To<TipoDestino>();

// Transformação assíncrona
var destino = await origem.ToAsync<TipoDestino>();
```

### Verificar se o Mapeamento Existe

```csharp
// Verificar se um mapeamento existe
bool podeMapear = origem.CanBindTo<TipoDestino>();

// Verificação type-safe
bool podeMapear = origem.CanBindTo<TipoOrigem, TipoDestino>();
```

## 📋 Mapeamento Baseado em Instância

Crie mapeamentos personalizados implementando a interface `IMorphable<TDestination>`:

```csharp
public class UsuarioDto : IMorphable<Usuario>
{
    public string Nome { get; set; }
    public string Email { get; set; }
    public DateTime DataNascimento { get; set; }
    
    public void MorphTo(Schema<Usuario> schema)
    {
        schema
            .Bind(u => u.NomeCompleto, () => Nome)
            .Bind(u => u.EnderecoEmail, () => Email)
            .Bind(u => u.Idade, sp => CalcularIdade(DataNascimento))
            .BindAsync(u => u.Perfil, async sp => 
            {
                var servicoPerfil = sp.GetService<IServicoPerfil>();
                return await servicoPerfil.ObterPerfilAsync(Email);
            })
            .Ignore(u => u.IdInterno);
    }
    
    private int CalcularIdade(DateTime dataNascimento) 
        => DateTime.Today.Year - dataNascimento.Year;
}
```

## ⚙️ Configuração Avançada de Esquema

### Bindings Síncronos

```csharp
public void MorphTo(Schema<Destino> schema)
{
    // Bind com resolvedor de service provider
    schema.Bind(d => d.Propriedade, sp => 
    {
        var servico = sp.GetService<IMeuServico>();
        return servico.ObterValor();
    });
    
    // Bind com resolvedor direto
    schema.Bind(d => d.Propriedade, () => "Valor Direto");
    
    // Ignorar propriedades
    schema.Ignore(d => d.PropriedadeIndesejada);
}
```

### Bindings Assíncronos

```csharp
public void MorphTo(Schema<Destino> schema)
{
    // Binding assíncrono com service provider
    schema.BindAsync(d => d.PropriedadeAssincrona, async sp =>
    {
        var servico = sp.GetService<IServicoAssincrono>();
        return await servico.ObterDadosAsync();
    });
    
    // Binding assíncrono com resolvedor direto
    schema.BindAsync(d => d.PropriedadeAssincrona, async () =>
    {
        await Task.Delay(100);
        return "Valor Assíncrono";
    });
}
```

## 🔧 Opções de Configuração

### Configuração de Assembly

```csharp
services.AddMorph(settings =>
{
    // Adicionar assemblies específicos
    settings.AddAssembly(Assembly.GetExecutingAssembly());
    settings.AddAssemblies(assembly1, assembly2);
    
    // Limpar e adicionar assemblies personalizados
    settings.ClearAssemblies()
            .AddAssembly(assemblyPersonalizado);
});
```

### Mapeamentos de Tipos Genéricos

```csharp
services.AddMorph(settings =>
{
    // Adicionar mapeamentos personalizados de interface para concreto
    settings.AddGenericMorph(typeof(IMinhaInterface<>), typeof(MinhaImplementacao<>));
    
    // Mapeamento genérico type-safe
    settings.AddGenericMapping<IColecaoPersonalizada<>, ColecaoPersonalizada<>>();
    
    // Limpar mapeamentos padrão e adicionar personalizados
    settings.ClearGenericMappings()
            .AddGenericMapping<IList<>, ArrayList>();
});
```

### Mapeamentos Genéricos Padrão

A biblioteca inclui estes mapeamentos padrão:

- `IList<>` → `List<>`
- `ICollection<>` → `List<>`
- `IDictionary<,>` → `Dictionary<,>`
- `ISet<>` → `HashSet<>`
- `IReadOnlyCollection<>` → `ReadOnlyCollection<>`
- `IReadOnlyList<>` → `List<>`
- `IReadOnlySet<>` → `HashSet<>`

## 🏗️ Exemplos de Mapeamento Complexo

### Integração com Padrão Repository

```csharp
public class ServicoUsuario
{
    private readonly IServiceProvider _serviceProvider;
    
    public ServicoUsuario(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    
    public async Task<UsuarioDto> ObterUsuarioAsync(int idUsuario)
    {
        var usuario = await ObterUsuarioDoBanco(idUsuario);
        return usuario.To<UsuarioDto>(_serviceProvider);
    }
    
    public async Task<IEnumerable<UsuarioDto>> ObterUsuariosAsync()
    {
        var usuarios = await ObterUsuariosDoBanco();
        return await usuarios.ToAsync<UsuarioDto>(_serviceProvider);
    }
}
```

### Transformação de Objetos Complexos

```csharp
public class PedidoDto : IMorphable<Pedido>
{
    public int Id { get; set; }
    public string NomeCliente { get; set; }
    public List<ItemPedidoDto> Itens { get; set; }
    public decimal ValorTotal { get; set; }
    
    public void MorphTo(Schema<Pedido> schema)
    {
        schema
            .Bind(p => p.IdPedido, () => Id)
            .Bind(p => p.Cliente, sp =>
            {
                var servicoCliente = sp.GetService<IServicoCliente>();
                return servicoCliente.ObterClientePorNome(NomeCliente);
            })
            .BindAsync(p => p.ItensPedido, async sp =>
            {
                // Transformar coleção assincronamente
                return await Itens.ToAsync<ItemPedido>(sp);
            })
            .Bind(p => p.Total, () => ValorTotal)
            .BindAsync(p => p.InfoEntrega, async sp =>
            {
                var servicoEntrega = sp.GetService<IServicoEntrega>();
                return await servicoEntrega.CalcularEntregaAsync(Id);
            })
            .Ignore(p => p.NotasInternas);
    }
}
```

## 🎯 Casos de Uso

### Transformação de Respostas de API

```csharp
// Transformar respostas de API para modelos de domínio
public async Task<Usuario> ObterUsuarioDaApi(int idUsuario)
{
    var respostaApi = await httpClient.GetAsync($"users/{idUsuario}");
    var usuarioDto = await respostaApi.Content.ReadFromJsonAsync<UsuarioApiDto>();
    
    return usuarioDto.To<Usuario>();
}
```

### Mapeamento de Entidades de Banco de Dados

```csharp
// Transformar entidades de banco para DTOs
public async Task<IEnumerable<ProdutoDto>> ObterProdutosAsync()
{
    var entidades = await dbContext.Produtos.ToListAsync();
    return entidades.To<ProdutoDto>();
}
```

### Integração com Event Sourcing

```csharp
public class UsuarioCriadoEvento : IMorphable<Usuario>
{
    public string IdUsuario { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public DateTime CriadoEm { get; set; }
    
    public void MorphTo(Schema<Usuario> schema)
    {
        schema
            .Bind(u => u.Id, () => IdUsuario)
            .Bind(u => u.NomeCompleto, () => Nome)
            .Bind(u => u.EnderecoEmail, () => Email)
            .Bind(u => u.DataCriacao, () => CriadoEm)
            .Bind(u => u.Ativo, () => true);
    }
}
```

# 🚨 Tratamento de Exceções

A biblioteca fornece tratamento detalhado de exceções:

## Tipos de Exceção

- **`BinderNotFoundException`**: Lançada quando não existe mapeamento entre tipos de origem e destino
- **`BindException`**: Lançada quando operações de binding de propriedade ou campo falham
- **`InvalidMorphConfigurationException`**: Lançada quando o sistema Morph não está configurado adequadamente

## Exemplo de Tratamento de Exceções

```csharp
try
{
    var resultado = origem.To<TipoDestino>();
}
catch (BinderNotFoundException ex)
{
    // Tratar mapeamento ausente
    logger.LogError($"Nenhum mapeamento encontrado: {ex.Message}");
}
catch (BindException ex)
{
    // Tratar erro de binding
    logger.LogError($"Falha no binding: {ex.Message}");
}
catch (InvalidMorphConfigurationException ex)
{
    // Tratar erro de configuração
    logger.LogError($"Problema de configuração: {ex.Message}");
}
```

# 📊 Dicas de Performance

1. **Reutilize o Service Provider**: Passe a mesma instância do service provider ao transformar múltiplos objetos
2. **Transformação de Coleções**: Use métodos de coleção específicos por tipo para melhor performance
3. **Escaneamento de Assemblies**: Limite assemblies na configuração para reduzir tempo de inicialização
4. **Operações Assíncronas**: Use métodos assíncronos para operações I/O-bound nos bindings

# 🛠️ Solução de Problemas

## Problemas Comuns

### Erro "ServiceProvider not configured"
```csharp
// Certifique-se de que AddMorph() foi chamado na configuração de DI
services.AddMorph();
```

### Erro "No mapping found"
```csharp
// Verifique se o tipo de origem implementa IMorphable<TDestination>
public class MinhaOrigem : IMorphable<MeuDestino>
{
    public void MorphTo(Schema<MeuDestino> schema) { /* implementação */ }
}
```

### Problemas de Mapeamento de Coleções Genéricas
```csharp
// Registre mapeamentos genéricos apropriados
services.AddMorph(settings =>
{
    settings.AddGenericMapping<IMinhaColecao<>, MinhaColecao<>>();
});
```

# 📝 Contribuindo

Agradecemos contribuições! Por favor, leia nossas diretrizes de contribuição e sinta-se à vontade para enviar pull requests.

# 📄 Licença

Este projeto é licenciado sob a Licença Apache 2.0 - veja o arquivo LICENSE para detalhes.