# Myth.DependencyInjection.Providers

![NuGet Version](https://img.shields.io/nuget/v/Myth.DependencyInjection.Providers?style=for-the-badge&logo=nuget) ![NuGet Version](https://img.shields.io/nuget/vpre/Myth.DependencyInjection.Providers?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

É uma biblioteca .NET para realizar a injeção de dependências para bibliotecas terceiras pré-configuradas.

# ⭐ Funcionalidades
- Adiciona versionamento
- Adiciona Swagger versionado
- Adiciona AutoMapper com configurações de paginação
- Adiciona métodos para facilitar o uso do AutoMapper

# 🔮 Utilização

Para utilização basta utilizar no `Startup` da sua aplicação as funções que necessitar.

```csharp
services.AddVersioning( 1 );						// Para versionamento
services.AddSwaggerVersioned( settings => {			// Para adicionar o Swagger versionado
	settings.Title = "API Test";
	settings.Description = "This is an API test";
	settings.Options.UseBasicAuthorization( );
} );
services.AddTypeMapping( );							// Para adicionar mapeamento de tipos
```

# 👓 Observações

## 🔢 Versionamento

Para que o vesionamento funcione corretamente é necesário que as _controllers_ tenham o seguinte formato:

```csharp
[ApiController]
[ApiVersion( "1.0" )]
[Route( "api/v{version:apiVersion}/[controller]" )]
internal class MyController : ControllerBase {
	
	...

}
```

## 🗺️ Mapeamento de tipos

Os seguintes métodos estáticos estarão disponiveis para utilizar e realizar o mapeamento a qualquer momento:

- `.MapTo<DestinationType>()`: Mapeia para o tipo de destino
- `.MapToAsync<SourceType, DestinationType>()`: Mapeia para o tipo de destino de forma assíncrona