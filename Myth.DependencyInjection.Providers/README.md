# Myth.DependencyInjection.Providers

[![NuGet Version](https://img.shields.io/nuget/v/Myth.DependencyInjection.Providers?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.DependencyInjection.Providers/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.DependencyInjection.Providers?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.DependencyInjection.Providers/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

It is a .NET library to perform dependency injection for pre-configured third-party libraries.

# ⭐ Features
- Add versioning
- Adds versioned Swagger
- Adds AutoMapper with pagination settings
- Adds methods to make using AutoMapper easier

# 🔮 Usage

To use, simply use the functions you need in the `Startup` of your application.

```csharp
services.AddVersioning( 1 );                      // For versioning
services.AddSwaggerVersioned( settings => {       // To add versioned Swagger
  settings.Title = "API Test";
  settings.Description = "This is an API test";
  settings.Options.UseBasicAuthorization( );
} );
services.AddTypeMapping( );                      // To add type mapping
```

# 👓 Observations

## 🔢 Versioning

For the vesionation to work correctly, the _controllers_ must have the following format:

```csharp
[ApiController]
[ApiVersion( "1.0" )]
[Route( "api/v{version:apiVersion}/[controller]" )]
internal class MyController : ControllerBase {

  ...

}
```

## 🗺️ Type Mapping

The following static methods will be available to use and perform mapping at any time:

- `.MapTo<DestinationType>()`: Maps to the destination type
- `.MapToAsync<SourceType, DestinationType>()`: Maps to the destination type asynchronously