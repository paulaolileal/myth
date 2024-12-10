# Myth.DependencyInjection

[![NuGet Version](https://img.shields.io/nuget/v/Myth.DependencyInjection?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.DependencyInjection/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.DependencyInjection?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.DependencyInjection/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

It is a .NET library for working with dependency injection, types and _assemblies_.

# ⭐ Features
- Loading application assemblies
- Find application types
- Find types from an interface
- Inject services from an interface automatically

# 🔮 Usage

To inject all types that implement an interface use the following code.

```csharp
services.AddServiceFromType<IMyInterface>();
```

To use application-defined types or _assemblies_, use the type provider:

```csharp
TypeProvider.ApplicationAssemblies // Shows all application assemblies
TypeProvider.ApplicationTypes // Shows all application types
```