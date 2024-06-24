# Myth.Rest

![NuGet Version](https://img.shields.io/nuget/v/Myth.Repository.EntityFramework?style=for-the-badge&logo=nuget) ![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Repository.EntityFramework?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)


É uma biblioteca .NET para definições de repositórios de acessos a bancos de dados usando Entity Framework.

# ⭐ Funcionalidades
- Definição de contexto base
- Leitura automática de arquivos de mapeamento de entidades
- Implementação de escrita
- Implementação de leitura usando expressões
- Implementação de leitura usando specification
- Trabalhar com transações

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