# Myth.Rest

![NuGet Version](https://img.shields.io/nuget/v/Myth.repository?style=for-the-badge&logo=nuget) ![NuGet Version](https://img.shields.io/nuget/vpre/Myth.repository?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)


É uma biblioteca .NET para definições de repositórios de acessos a bancos de dados usando Entity Framework.

# ⭐ Funcionalidades
- Definição de contexto base
- Leitura automática de arquivos de mapeamento de entidades
- Implementação de escrita
- Implementação de leitura usando expressões
- Implementação de leitura usando specification

# 🔮 Utilização
Para utilizar basta que o contexto criado herde do [BaseContext](/Contexts/BaseContext.cs). Após isso somente crie os repositórios passando-o como parametro.

## 🕶️ Leitura

Para leitura diversos métodos podem ser utilizados:

- `GetProviderName`
- `AsQueryable`
- `AsEnumerable`
- `ToListAsync`
- `Where`
- `SearchAsync`
- `SearchPaginatedAsync`
- `CountAsync`
- `AnyAsync`
- `AllAsync`
- `FirstOrDefaultAsync`
- `LastOrDefaultAsync`

## ✍️ Escrita

Para escrita podem ser utilizados os seguinte métodos:

- `AddAsync`
- `AddRangeAsync`
- `RemoveAsync`
- `RemoveRangeAsync`
- `UpdateAsync`
- `UpdateRangeAsync`
- `AttachAsync`
- `AttachRangeAsync`
- `SaveChangesAsync`
- `ExecuteSqlRawAsync`