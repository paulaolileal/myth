# Myth.Rest

![NuGet Version](https://img.shields.io/nuget/v/Myth.Repository.EntityFramework?style=for-the-badge&logo=nuget) ![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Repository.EntityFramework?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)


It is a .NET library for defining database access repositories using Entity Framework.

# ⭐ Features
- Definition of base context
- Automatic reading of entity mapping files
- Writing implementation
- Implementation of reading using expressions
- Implementation of reading using specification
- Work with transactions

# 🔮 Usage
To use it, simply inherit the created context from [BaseContext](/Contexts/BaseContext.cs). After that, just create the repositories by passing it as a parameter.

## 🕶️ Reading'

Several methods can be used to read:

- `GetProviderName`: Returns the database connection provider
- `AsQueryable`: Returns a collection for execution in the database
- `AsEnumerable`: Returns a collection for execution in memory]
- `ToListAsync`: Returns all items in the collection
- `Where`: Returns the collection with a filter
- `SearchAsync`: Returns the filtered collection
- `SearchPaginatedAsync`: Returns the filtered and paginated collection
- `CountAsync`: Counts the items in the collection
- `AnyAsync`: Checks if any item in the collection meets a requirement
- `AllAsync`: Checks whether all items in the collection meet a requirement
- `FirstOrDefaultAsync`: Returns the first item in the collection
- `LastOrDefaultAsync`: Returns the last item in the collection

## ✍️ Writing

The following methods can be used for writing:

- `AddAsync`: Adds an item to the collection
- `AddRangeAsync`: Adds multiple items to the collection
- `RemoveAsync`: Removes an item from the collection
- `RemoveRangeAsync`: Removes multiple items from the collection
- `UpdateAsync`: Updates a collection item
- `UpdateRangeAsync`: Updates multiple items in the collection
- `AttachAsync`: Attach an item to the collection
- `AttachRangeAsync`: Attach multiple items to the collection
- `SaveChangesAsync`: Saves all changes
- `ExecuteSqlRawAsync`: Executes a query in the database

## 🪄 Unit of work

The entity's independent functionalities are as follows:

- `SaveChangesAsync`: Saves all changes
- `ExecuteSqlRawAsync`: Executes a query in the database
- `BeginTransactionAsync`: Starts a transaction
- `CommitAsync`: Executes all transaction changes
- `RollbackAsync`: Undoes transaction changes
- `CreateSavepointAsync`: Creates a transaction checkpoint
- `RollbackToSavepointAsync`: Returns to a t'ransaction checkpoint