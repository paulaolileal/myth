# Myth.DependencyInjection

[![NuGet Version](https://img.shields.io/nuget/v/Myth.DependencyInjection?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.DependencyInjection/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.DependencyInjection?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.DependencyInjection/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

É uma biblioteca .NET para trabalhar com injeção de dependências, tipos e _assemblies_.

# ⭐ Funcionalidades
- Carregamento de assemblies da aplicação
- Localizar tipos da aplicação
- Localizar tipos a partir de uma interface
- Injetar serviços a partir de uma interface automáticamente

# 🔮 Utilização

Para injetar todos os tipos que implementam uma interface utilize o seguinte código.

```csharp
services.AddServiceFromType<IMyInterface>();
```

Para utilizar os tipos definidos pela aplicação ou _assemblies_, utilize o provedor de tipos:

```csharp
TypeProvider.ApplicationAssemblies	// Mostra todos os assemblies da aplicação
TypeProvider.ApplicationTypes		// Mostra todos os tipos da aplicação
```