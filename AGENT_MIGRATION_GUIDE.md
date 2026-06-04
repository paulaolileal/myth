# Myth Ecosystem — Agent Migration Guide

This document targets AI agents working on **projects that consume Myth libraries**. It describes API changes made to the Myth ecosystem and provides concrete instructions for updating consumer code to use the new, safer APIs.

Each section states: what changed, what the old pattern looked like, what the new pattern looks like, and what files to search for in the consumer project.

---

## 1. Myth.Repository — SearchAsync now returns IReadOnlyList

### What changed

`IReadRepositoryAsync<T>.SearchAsync` and `SearchAsNoTrackingAsync` now return `Task<IReadOnlyList<TEntity>>` instead of `Task<IEnumerable<TEntity>>`. The new variant `SearchAsNoTrackingAsync` was also added for read-only use cases.

### Why this matters

`IEnumerable<T>.Count` is a property that returns `1` (the count of the wrapper object, not its elements). `IReadOnlyList<T>.Count` is an O(1) property that returns the actual element count. If any consumer code used `.Count` (property, not `.Count()` extension) on the result of `SearchAsync`, it was silently returning wrong values.

### Instructions for the agent

**Step 1 — Search for usages of `.Count` on SearchAsync results**

Search for this anti-pattern in the consumer project:
```csharp
// Pattern to find: .Count (property) called on result of SearchAsync
var results = await repo.SearchAsync(...);
var total = results.Count; // was wrong with IEnumerable; now correct with IReadOnlyList
```

No code change is needed if `.Count` is already used — it now works correctly. If the consumer was using `.Count()` (extension method), that also continues to work correctly — no change needed.

**Step 2 — Replace read-only SearchAsync calls with SearchAsNoTrackingAsync**

Find all usages of `SearchAsync` where the returned entities are:
- Only read (projected to DTOs, returned to API clients)
- NOT modified and saved via `SaveChangesAsync`

Replace those with `SearchAsNoTrackingAsync`:

```csharp
// ❌ OLD — tracked entities for a read-only projection
var products = await _productRepository.SearchAsync(p => p.IsActive, ct);
return products.Select(p => new ProductDto(p)).ToList();

// ✅ NEW — no tracking overhead for read-only scenarios
var products = await _productRepository.SearchAsNoTrackingAsync(p => p.IsActive, ct);
return products.Select(p => new ProductDto(p)).ToList();
```

Keep `SearchAsync` (tracked) when you modify the entities afterward:

```csharp
// ✅ KEEP SearchAsync — entities are modified, tracked, and saved
var orders = await _orderRepository.SearchAsync(spec, ct);
foreach (var order in orders)
    order.Status = OrderStatus.Shipped;
await _unitOfWork.SaveChangesAsync(ct); // EF Core detects changes automatically
```

**Step 3 — Update custom repository interface method signatures**

If the consumer project defines custom repository interfaces that return `IEnumerable<T>` from methods that call `SearchAsync`, update them:

```csharp
// ❌ OLD
public interface IProductRepository : IReadWriteRepositoryAsync<Product> {
    Task<IEnumerable<Product>> GetActiveAsync(CancellationToken ct = default);
}

// ✅ NEW
public interface IProductRepository : IReadWriteRepositoryAsync<Product> {
    Task<IReadOnlyList<Product>> GetActiveAsync(CancellationToken ct = default);
}
```

**Files to search in the consumer project:**
- All `*Repository.cs` files
- All `I*Repository.cs` interfaces
- Any handler or service that calls `.SearchAsync(`

---

## 2. Myth.Flow.Actions — CommandResult now has StatusCode and semantic factories

### What changed

`CommandResult` and `CommandResult<TResponse>` gained:
- `HttpStatusCode StatusCode { get; }` property (200 on Success, 400 on default Failure)
- `Failure(string, HttpStatusCode)` overload
- Semantic factories: `NotFound()`, `Forbidden()`, `Unauthorized()`, `PaymentRequired()`, `Conflict()`, `UnprocessableEntity()`

### Why this matters

The previous only way to return an HTTP 402, 403, or 409 from a handler was to throw `ValidationException` with a specific status — an abuse of the validation exception for non-validation domain failures.

### Instructions for the agent

**Step 1 — Replace ValidationException throws that are NOT validation errors**

Search for `throw new ValidationException` in handler files (`*Handler.cs`, `*CommandHandler.cs`). Evaluate each occurrence:

- If it validates input fields (email format, required fields, length) → **keep as is**, `ValidationException` is correct here.
- If it signals a domain condition (no credits, not found, access denied, duplicate) → **replace** with the appropriate semantic factory.

```csharp
// ❌ OLD — abusing ValidationException for a domain/business failure
throw new ValidationException(new ValidationResult([
    new ValidationError("credits", "Insufficient credits", HttpStatusCode.PaymentRequired)
]));

// ✅ NEW — express the domain condition explicitly
return CommandResult.PaymentRequired("Insufficient credits to create a workspace");
```

**Mapping table:**

| Old pattern | New factory |
|------------|-------------|
| `throw ValidationException(...HttpStatusCode.NotFound)` | `return CommandResult.NotFound("...")` |
| `throw ValidationException(...HttpStatusCode.Forbidden)` | `return CommandResult.Forbidden("...")` |
| `throw ValidationException(...HttpStatusCode.Unauthorized)` | `return CommandResult.Unauthorized("...")` |
| `throw ValidationException(...HttpStatusCode.PaymentRequired)` | `return CommandResult.PaymentRequired("...")` |
| `throw ValidationException(...HttpStatusCode.Conflict)` | `return CommandResult.Conflict("...")` |

**Step 2 — Update controllers to use result.StatusCode**

If controllers currently hardcode HTTP status codes, replace with `result.StatusCode`:

```csharp
// ❌ OLD — controller must know what each failure means
var result = await _dispatcher.DispatchCommandAsync(command, ct);
if (!result.IsSuccess)
    return result.ErrorMessage?.Contains("not found") == true ? NotFound() : BadRequest(result.ErrorMessage);

// ✅ NEW — handler communicates status code; controller just maps it
var result = await _dispatcher.DispatchCommandAsync(command, ct);
return result.IsSuccess
    ? Ok()
    : StatusCode((int)result.StatusCode, result.ErrorMessage);
```

**Files to search in the consumer project:**
- All `*Handler.cs` / `*CommandHandler.cs` files
- All `*Controller.cs` files that dispatch commands

---

## 3. Myth.Flow.Actions — QueryResult now has NotFound and Forbidden

### What changed

`QueryResult<TData>` gained:
- `HttpStatusCode StatusCode { get; }` property
- `NotFound()`, `Forbidden()`, `Unauthorized()` semantic factories

### Why this matters

The previous pattern `QueryResult<T>.Success(null!)` lies about the result state — a successful result with null data is semantically incorrect when the resource simply does not exist.

### Instructions for the agent

**Step 1 — Replace Success(null!) with NotFound()**

Search for `QueryResult` + `Success(null` in handler files:

```csharp
// ❌ OLD — lies about success when entity is absent
var project = await _repo.FirstOrDefaultAsync(p => p.Id == query.Id, ct);
if (project is null)
    return QueryResult<ProjectDto>.Success(null!);

// ✅ NEW — explicit about the absent state
var project = await _repo.FirstOrDefaultAsync(p => p.Id == query.Id, ct);
if (project is null)
    return QueryResult<ProjectDto>.NotFound($"Project {query.Id} not found");
```

**Step 2 — Update controllers that check for null Data**

After replacing `Success(null!)` with `NotFound()`, the controller null check becomes redundant:

```csharp
// ❌ OLD — controller interprets null as not found
var result = await _dispatcher.DispatchQueryAsync<GetProjectQuery, ProjectDto>(query, null, ct);
if (result.Value is null) return NotFound();
return Ok(result.Value);

// ✅ NEW — handler signals not found; controller maps StatusCode
var result = await _dispatcher.DispatchQueryAsync<GetProjectQuery, ProjectDto>(query, null, ct);
return result.IsSuccess
    ? Ok(result.Data)
    : StatusCode((int)result.StatusCode, result.ErrorMessage);
```

**Files to search in the consumer project:**
- All `*QueryHandler.cs` / `*Handler.cs` files for queries
- All `*Controller.cs` / `*Endpoint.cs` files that dispatch queries

---

## 4. Myth.Guard — MaxLength and MinLength are now available

### What changed

`FluentRuleBuilder<string>` and `IStandaloneValidationBuilder<string>` now expose `MaxLength(int)` and `MinLength(int)` as aliases for `MaximumLength(int)` and `MinimumLength(int)`.

### Why this matters

Developers who could not find the canonical `MaximumLength` name used `.Must(v => v.Length <= N)` as a workaround, losing the standard error message.

### Instructions for the agent

**Step 1 — Replace manual .Must length checks**

Search for `.Must(v => v.Length` in entity `Validate()` methods and `Sentry.For()` chains:

```csharp
// ❌ OLD — manual workaround without standard error message
builder.For(x.Name, r => r.Must(v => v?.Length <= 100).WithMessage("Max 100 chars"));
builder.For(x.Description, r => r.Must(v => v?.Length >= 10).WithMessage("Min 10 chars"));

// ✅ NEW — built-in rules with standard messages
builder.For(x.Name, r => r.MaxLength(100));
builder.For(x.Description, r => r.MinLength(10));
```

Both `MaxLength`/`MinLength` (shorter) and `MaximumLength`/`MinimumLength` (canonical) work identically. Either form is correct going forward.

**Files to search in the consumer project:**
- All `IValidatable<T>` implementors (entities and DTOs with `Validate()` methods)
- All `Sentry.For()` chains

---

## 5. Myth.Guard — Using NotDefault() for Guid fields (pitfall documentation)

### What changed

No code changed — this is a documentation clarification of an existing behavior. `NotDefault()` already existed but was undocumented for Guid use cases.

### Why this matters

`NotEmpty()` on a `Guid` compiles without warning but does **not** reject `Guid.Empty`. This is a silent runtime bug.

### Instructions for the agent

**Step 1 — Search for NotEmpty() on Guid fields**

Find all `NotEmpty()` calls on `Guid` or `Guid?` properties in `Validate()` methods:

```csharp
// ❌ WRONG — compiles but Guid.Empty passes validation
builder.For(x.WorkspaceId, r => r.NotEmpty());
builder.For(x.OrganizationId, r => r.NotEmpty());

// ✅ CORRECT — NotDefault() rejects Guid.Empty
builder.For(x.WorkspaceId, r => r.NotDefault());
builder.For(x.OrganizationId, r => r.NotDefault());
```

**How to identify Guid fields:** look for properties of type `Guid`, `Guid?`, or any id field ending in `Id` or `Guid`.

**Files to search in the consumer project:**
- All `IValidatable<T>` implementors — specifically Command classes, DTO classes

---

## 6. Myth.Flow — Transform/TransformAsync sempre envolvem exceções em PipelineException

### What changed

`Transform` and `TransformAsync` previously re-threw filtered exceptions (those in `ExceptionTypesToPropagate`) directly without wrapping, while wrapping all other exceptions in `PipelineException`. Now **all** exceptions thrown inside a Transform step — whether filtered or not — are wrapped in `PipelineException` before propagating.

`ShouldPropagateException` was also updated to walk the full `InnerException` chain, so a `PipelineException` whose inner exception is a filtered type still propagates correctly.

### Why this matters

Consumer code that caught the raw filtered type after a `Transform` step was relying on an implementation accident, not a contract. The library contract is: any exception that escapes a pipeline step is a `PipelineException`. Breaking that contract made error handling unpredictable and silently hid the transform context (step name, index, type mapping) in the stack trace.

### Instructions for the agent

**Step 1 — Find catch blocks that catch raw exception types after Transform/TransformAsync**

Search for `try`/`catch` blocks around pipelines that contain `.Transform(` or `.TransformAsync(`:

```csharp
// ❌ OLD — catching raw type that was accidentally propagated
try {
    var result = await Pipeline.Start(dto)
        .TapAsync(ctx => ThrowIfInvalid(ctx))
        .Transform(ctx => new ResponseDto(ctx))
        .ExecuteAsync();
} catch (ArgumentException ex) {
    // This no longer works — Transform now wraps in PipelineException
}
```

**Step 2 — Replace raw catch with PipelineException + inner exception check**

```csharp
// ✅ NEW — catch PipelineException and inspect InnerException
try {
    var result = await Pipeline.Start(dto)
        .TapAsync(ctx => ThrowIfInvalid(ctx))
        .Transform(ctx => new ResponseDto(ctx))
        .ExecuteAsync();
} catch (PipelineException ex) when (ex.InnerException is ArgumentException argEx) {
    // Handle the original ArgumentException via argEx
    _logger.LogError(argEx, "Validation error in transform");
}
```

**Step 3 — Verify that filtered exception propagation tests assert PipelineException**

In test files for pipelines that use `Transform`/`TransformAsync` with `ExceptionTypesToPropagate`:

```csharp
// ❌ OLD test assertion — raw type
await act.Should().ThrowAsync<ArgumentException>().WithMessage("...");

// ✅ NEW test assertion — PipelineException with inner exception
var thrown = await act.Should().ThrowAsync<PipelineException>();
thrown.Which.InnerException.Should().BeOfType<ArgumentException>();
thrown.Which.InnerException!.Message.Should().Be("...");
```

**Files to search in the consumer project:**
- Any `catch (SomeException)` block around code that calls `.ExecuteAsync()` on a pipeline that includes `.Transform(` or `.TransformAsync(`
- Test files that assert `ThrowAsync<SomeDomainException>()` on pipelines with Transform steps

---

## Namespace quick reference (for import errors)

When writing or fixing handlers, these are the correct `using` statements:

```csharp
using Myth.Interfaces;          // ICommand, IQuery, ICommandHandler, IQueryHandler, IDispatcher
using Myth.Models;              // CommandResult, CommandResult<T>, QueryResult<T>
using Myth.Exceptions;          // ValidationException  ← NOT Myth.Guard.Exceptions
using Myth.Guard;               // ValidationBuilder<T>, FluentRuleBuilder<T>, Sentry, Validate
using Myth.Interfaces;          // IValidatable<T>, IValidator
```

> **Common mistake:** `ValidationException` lives in `Myth.Exceptions`, not `Myth.Guard.Exceptions`. The `using Myth.Exceptions;` import is required when throwing or catching it in handlers.
