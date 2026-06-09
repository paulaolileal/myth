# Melhorias e Oportunidades â€” Myth Ecosystem

Cada seÃ§Ã£o Ã© uma entrada independente. Formato: tÃ­tulo, biblioteca, data, contexto, comportamento atual, problema/lacuna, sugestÃ£o.

---

## SchemaRegistry.MapGenericTypes â€” Items Ã© null ao mapear IPaginated<TSource> â†’ IPaginated<TDest>

**Library:** Myth.Morph
**Discovered:** 2026-06-04
**Status:** âœ… RESOLVED 2026-06-04 â€” `MapGenericTypes` agora detecta tipos sem propriedades gravÃ¡veis (ex.: `Paginated<T>` com `private set`) e usa mapeamento orientado a construtor via `CreateInstanceFromSource`.
**Context:** Chamada `result.To<IPaginated<GetWeatherStationResponse>>()` em query handler retorna `Paginated<GetWeatherStationResponse>` com `Items = null`. O `.Tap()` seguinte que chama `pipeline.CurrentRequest!.Items.Count()` lanÃ§a `NullReferenceException` silenciada pelo pipeline.

**Current behavior:**
`SchemaRegistry.MapGenericTypes` cria a instÃ¢ncia de destino via `CreateInstance(Paginated<GetWeatherStationResponse>)`. Como `Paginated<T>` nÃ£o tem construtor sem parÃ¢metros, `CreateInstance` usa o construtor primÃ¡rio `(int pageNumber, int pageSize, int totalItems, int totalPages, IEnumerable<T> items)` com valores padrÃ£o resolvidos via DI. O parÃ¢metro `IEnumerable<GetWeatherStationResponse> items` nÃ£o Ã© resolvÃ­vel via DI, entÃ£o recebe `null` (retorno de `GetDefault(typeof(IEnumerable<>))`).

Em seguida, `MapPropertiesGeneric` tenta copiar as propriedades de `Paginated<WeatherStation>` para `Paginated<GetWeatherStationResponse>`, mas todas as propriedades de `Paginated<T>` tÃªm `private set` â€” portanto `CanWrite = false` para todas. Nenhuma propriedade Ã© copiada. O objeto destino permanece com `Items = null`, `PageNumber = 0`, `TotalItems = 0`, etc.

```csharp
// SchemaRegistry.CreateInstance â€” parÃ¢metros sem DI recebem GetDefault():
private static object? GetDefault(Type type) =>
    type.IsValueType ? Activator.CreateInstance(type) : null; // IEnumerable<T> â†’ null

// MapPropertiesGeneric â€” private set bloqueia escrita:
var destProperties = destType
    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
    .Where(p => p.CanWrite)  // Paginated<T>: todos private set â†’ nenhum passa
    .ToArray();
```

**Problem / Gap:**
1. `Paginated<T>` Ã© um tipo de valor central do ecossistema e Ã© completamente inutilizÃ¡vel com o mapper genÃ©rico â€” silenciosamente retorna um objeto vazio.
2. Qualquer chamada `somePaginated.To<IPaginated<TDto>>()` produz resultado corrompido sem erros ou warnings.
3. O `.Tap()` que chama `Items.Count()` recebe `NullReferenceException` silenciada pelo pipeline (o Myth Flow swallows exceÃ§Ãµes em `.Tap()`), tornando o diagnÃ³stico ainda mais difÃ­cil.

**Suggested improvement:**
Duas abordagens, da mais simples Ã  mais robusta:

**OpÃ§Ã£o A (mÃ­nimo):** Fazer `Paginated<T>` implementar `IMorphableTo<Paginated<TDest>>` usando um tipo especial, ou adicionar um `IMorphableFrom` estÃ¡tico. Exemplo concreto: `SchemaRegistry` poderia detectar que o destino Ã© `Paginated<T>` (genÃ©rico conhecido) e chamar o construtor com os valores corretos mapeados dos scalars + items mapeados.

**OpÃ§Ã£o B (correto):** Adicionar suporte a tipos "construtor-driven" no `SchemaRegistry`. Quando `CreateInstance` falha para construir com defaults vÃ¡lidos (ex.: `IEnumerable<T>` â†’ null), tentar identificar quais propriedades/parÃ¢metros sÃ£o "coleÃ§Ãµes de elementos" e mapear os elementos antes de construir. PseudocÃ³digo:

```csharp
// Detectar que items precisa de mapeamento de coleÃ§Ã£o:
// 1. Identificar parÃ¢metros cujo tipo Ã© IEnumerable<TElement>
// 2. Encontrar a propriedade source com o mesmo nome
// 3. Mapear os elementos (WeatherStation â†’ GetWeatherStationResponse)
// 4. Construir Paginated<GetWeatherStationResponse>(srcPageNumber, srcPageSize, srcTotal, srcTotalPages, mappedItems)
```

**OpÃ§Ã£o C (paliativo no template):** Em vez de `result.To<IPaginated<GetWeatherStationResponse>>()`, construir o paginated manualmente no handler:

```csharp
var items = result.Items.To<WeatherStation, GetWeatherStationResponse>();
var response = items.AsPaginated(result.TotalItems, result.PageSize, (result.PageNumber - 1) * result.PageSize);
```

---

## SearchAsync â€” comportamento de tracking nÃ£o documentado

**Library:** Myth.Repository.EntityFramework
**Discovered:** 2026-05-28
**Status:** âœ… RESOLVED â€” XMLDoc adicionado em `SearchAsync` documentando change tracking; `SearchAsNoTrackingAsync` implementado.
**Context:** Investigando um bug de soft-delete em cadeia que precisava modificar entidades retornadas por `SearchAsync` e salvar com `SaveChangesAsync`.

**Current behavior:**
`SearchAsync` retorna entidades **rastreadas** (EF Core change tracking ativo). NÃ£o usa `AsNoTracking()`.

```csharp
public virtual async Task<IEnumerable<TEntity>> SearchAsync(ISpec<TEntity> specification, ...) {
    var result = await _context
        .Set<TEntity>()
        .AsQueryable()
        .Specify(specification)
        .ToListAsync(cancellationToken);
    return result.AsEnumerable();
}
```

**Problem / Gap:**
NÃ£o Ã© Ã³bvio pelo nome ou documentaÃ§Ã£o que as entidades retornadas sÃ£o rastreadas. Desenvolvedores podem assumir erroneamente que sÃ£o nÃ£o-rastreadas (como seria com `AsNoTracking`) e chamar `repo.UpdateAsync(entity)` desnecessariamente, ou pior, achar que modificaÃ§Ãµes diretas nÃ£o serÃ£o salvas. O padrÃ£o vÃ¡lido â€” modificar entidades retornadas e chamar `uow.SaveChangesAsync` diretamente â€” nÃ£o aparece em nenhum skill ou README.

**Suggested improvement:**
1. Adicionar XMLDoc no mÃ©todo `SearchAsync` explicitando que entidades sÃ£o rastreadas pelo change tracker.
2. Adicionar overload `SearchAsNoTrackingAsync` para casos de leitura pura (relatÃ³rios, projeÃ§Ãµes), deixando a intenÃ§Ã£o explÃ­cita no nome do mÃ©todo.

---

## SearchAsync retorna IEnumerable, nÃ£o List â€” armadilha do .Count

**Library:** Myth.Repository.EntityFramework
**Discovered:** 2026-05-21
**Status:** âœ… RESOLVED â€” `SearchAsync` e `SearchAsNoTrackingAsync` retornam `IReadOnlyList<TEntity>` (jÃ¡ materializado via `ToListAsync`); XMLDoc documenta que `.Count()` (com parÃªnteses) deve ser usado.
**Context:** Contando resultados de `SearchAsync` para verificar limites de plano.

**Current behavior:**
`SearchAsync` declara retorno como `IEnumerable<TEntity>`, nÃ£o `List<T>`. Chamar `.Count` (propriedade) compila mas retorna sempre 1 porque acessa `IEnumerable.Count` da interface, nÃ£o a contagem real dos elementos.

**Problem / Gap:**
Nenhuma documentaÃ§Ã£o ou skill alerta para esse comportamento. O erro Ã© silencioso â€” compila sem warning, produz valor errado em runtime. VerificaÃ§Ãµes de limite de plano baseadas em `.Count` falham silenciosamente.

```csharp
// WRONG â€” .Count Ã© propriedade de IEnumerable (retorna 1, nÃ£o o tamanho real)
var total = results.Count;

// CORRECT â€” .Count() Ã© extension method do LINQ que itera a coleÃ§Ã£o
var total = results.Count();
```

**Suggested improvement:**
1. Documentar na XMLDoc de `SearchAsync` que o retorno Ã© `IEnumerable<T>` e que `.Count()` (com parÃªnteses) deve ser usado.
2. Considerar mudar o retorno para `IReadOnlyList<TEntity>` para eliminar a ambiguidade, jÃ¡ que a coleÃ§Ã£o jÃ¡ estÃ¡ materializada internamente via `ToListAsync`.

---

## Query/Process com tipo de retorno capturam exceÃ§Ãµes silenciosamente em testes

**Library:** Myth.Flow / Myth.Flow.Actions
**Discovered:** 2026-06-02
**Status:** âœ… RESOLVED 2026-06-04 â€” Documentado no SKILL.md (seÃ§Ã£o "Error Handling" e troubleshooting "Exception from handler not propagating") e nos READMEs (seÃ§Ã£o "Exception Handling in Tests"). O comportamento Ã© intencional: o pipeline garante resultados previsÃ­veis capturando exceÃ§Ãµes em `Result.Failure`. Para exceÃ§Ãµes que devem propagar, usar `UseExceptionFilter<T>()` do Myth.Flow.
**Context:** Implementando testes e2e para `WeatherStationController` usando `BaseDatabaseTests`. Os testes que chamam `.Query<T,R>()` e `.Process<T,R>()` (com tipo de retorno `Guid`, por exemplo) nÃ£o propagam `ValidationException` lanÃ§adas em `.TapAsync()` anterior ao step de execuÃ§Ã£o.

**Current behavior:**
Quando uma `ValidationException` Ã© lanÃ§ada dentro de `.TapAsync()` antes de `.Query<T,R>()` ou `.Process<T,R>()`:
- A exceÃ§Ã£o Ã© capturada pelo framework internamente
- `ExecuteAsync()` retorna um resultado de erro (com `Value = default(T)`)
- Nenhuma exceÃ§Ã£o chega ao chamador

PorÃ©m, quando a exceÃ§Ã£o ocorre antes de `.Process()` (sem tipo de retorno):
- A exceÃ§Ã£o **propagada** normalmente ao chamador

```csharp
// NÃƒO propaga â€” Query captura internamente:
PipelineExtensions.Start(query)
    .TapAsync(pipeline => validator.ValidateAsync(pipeline.CurrentRequest!)) // throws
    .Query<TQuery, TResult>()  // captura a exceÃ§Ã£o
    .ExecuteAsync(ct);  // retorna Result<TResult> com erro, nÃ£o lanÃ§a

// PROPAGA â€” Process void nÃ£o captura:
PipelineExtensions.Start(command)
    .TapAsync(pipeline => validator.ValidateAsync(pipeline.CurrentRequest!)) // throws
    .Process()  // nÃ£o captura
    .ExecuteAsync(ct);  // lanÃ§a a exceÃ§Ã£o
```

**Problem / Gap:**
1. Testes e2e que chamam o controller diretamente (`controller.GetByIdAsync(Guid.Empty)`) nÃ£o conseguem testar cenÃ¡rios de erro de validaÃ§Ã£o para endpoints que usam `.Query<T,R>()` â€” a exceÃ§Ã£o nunca chega ao teste.
2. O comportamento Ã© assimÃ©trico entre `.Process()` void e `.Process<T,R>()`/`.Query<T,R>()`, o que nÃ£o Ã© documentado e cria surpresas.
3. Em testes, `UseExceptionFilter<ValidationException>()` nÃ£o Ã© suficiente para distinguir comportamento esperado de falha silenciosa.

**Suggested improvement:**
1. Documentar explicitamente no skill `myth-flow-actions` e `myth-flow` a diferenÃ§a de comportamento entre `.Process()` void e `.Process<T,R>()`/`.Query<T,R>()` com relaÃ§Ã£o ao tratamento de exceÃ§Ãµes.
2. Considerar expor um `Result<T>` do pipeline que permita ao teste inspecionar erros sem exigir que exceÃ§Ãµes propagadas.
3. Ou: adicionar um mÃ©todo `.ThrowOnError()` ao pipeline que force rethrow de erros capturados, facilitando testes.

---

## IUnitOfWorkRepository.BeginTransactionAsync() falha silenciosamente com InMemory EF

**Library:** Myth.Repository.EntityFramework
**Discovered:** 2026-06-02
**Status:** âœ… RESOLVED â€” `BeginTransactionAsync` captura `InvalidOperationException` de providers sem suporte a transaÃ§Ãµes (InMemory). Commit/Rollback/Savepoint tambÃ©m sÃ£o no-op nesse cenÃ¡rio.
**Context:** Testando o endpoint `PostWithForecastsAsync` que usa `IUnitOfWorkRepository.BeginTransactionAsync()` / `CommitAsync()` / `CreateSavepointAsync()`. O handler falha silenciosamente no ambiente de testes com InMemory EF, e `.Process<T,Guid>()` captura a exceÃ§Ã£o retornando `Guid.Empty`.

**Current behavior:**
`IUnitOfWorkRepository.BeginTransactionAsync()` chama `context.Database.BeginTransactionAsync()` que lanÃ§a `InvalidOperationException` no provider InMemory do EF Core ("Transactions are not supported by the in-memory store"). Essa exceÃ§Ã£o Ã© capturada pelo `.Process<T,Guid>()` do Myth Flow, que retorna `Guid.Empty` como valor padrÃ£o.

**Problem / Gap:**
- A documentaÃ§Ã£o do template diz "O provider InMemory silencia transaÃ§Ãµes" â€” mas na prÃ¡tica a exceÃ§Ã£o Ã© capturada pelo pipeline, nÃ£o pelo UoW.
- O controller retorna `CreatedAtRoute(..., Guid.Empty)` como se tivesse sucesso, mesmo com o handler falhando.
- Torna impossÃ­vel testar o comportamento transacional do handler em ambiente de testes com InMemory.

**Suggested improvement:**
1. `IUnitOfWorkRepository.BeginTransactionAsync()` deve tratar silenciosamente (try/catch) a exceÃ§Ã£o `InvalidOperationException` quando o provider for InMemory, retornando um `NullTransaction` que nÃ£o faz nada.
2. Documentar claramente no skill `myth-repository-entity-framework` quais mÃ©todos do UoW sÃ£o suportados com InMemory vs providers reais.
3. Adicionar mÃ©todo `IsTransactionSupported` ao `IUnitOfWorkRepository` para que handlers possam verificar antes de iniciar transaÃ§Ãµes â€” permitindo testes mais robustos.

---

## ValidationContextKey.Create â€” regras globais nÃ£o executam ao passar contexto explÃ­cito

**Library:** Myth.Guard
**Discovered:** 2026-06-02
**Status:** âœ… RESOLVED â€” `ValidationBuilder.GetRules()` jÃ¡ inclui `_globalRules` primeiro e depois adiciona as regras do contexto especificado. Regras globais sempre executam independente do contexto.
**Context:** Testando `PostAsync` do `WeatherStationController` que chama `validator.ValidateAsync(command, ValidationContextKey.Create)`. Em testes, regras globais (fora de `InContext`) nÃ£o executam quando um context key Ã© passado.

**Current behavior:**
Quando `IValidator.ValidateAsync(obj, ValidationContextKey.Create)` Ã© chamado:
- Apenas as regras dentro de `builder.InContext(ValidationContextKey.Create, ...)` sÃ£o executadas
- Regras globais (fora de qualquer `InContext`) sÃ£o ignoradas

Isso significa que para `CreateWeatherStationCommand { Name = "", Location = "..." }`:
- A regra global `builder.For(Name, rules => rules.NotEmpty().MinLength(2).MaxLength(100))` NÃƒO executa
- Somente o check de unicidade via IScopedService executa (mas retorna `true` para `""` pois nÃ£o existe)
- **Nenhuma exceÃ§Ã£o Ã© lanÃ§ada** â€” mesmo com dados invÃ¡lidos

**Problem / Gap:**
1. ValidaÃ§Ã£o de campo obrigatÃ³rio falha silenciosamente quando context key Ã© especificado.
2. O comportamento nÃ£o estÃ¡ documentado e vai contra o princÃ­pio de least surprise â€” ao especificar um contexto adicional, o desenvolvedor espera que regras globais ainda se apliquem.
3. Torna difÃ­cil testar cenÃ¡rios de erro em endpoints que usam `ValidationContextKey.Create`.

**Suggested improvement:**
1. Documentar claramente no skill `myth-guard`: ao usar `ValidationContextKey`, as regras globais **sempre executam**; `InContext` adiciona regras extras quando o contexto bate.
2. Se o comportamento atual Ã© intencional (sÃ³ contexto especÃ­fico), documentar explicitamente com exemplo de quando usar cada abordagem.
3. Se Ã© um bug, corrigir para que regras globais executem independente de qualquer contexto especificado.

---

## CommandResult.Failure() nÃ£o aceita HttpStatusCode

**Library:** Myth.Flow.Actions
**Discovered:** 2026-05-20
**Status:** âœ… RESOLVED â€” `CommandResult.Failure(string, HttpStatusCode, ...)` existe. MÃ©todos semÃ¢nticos tambÃ©m implementados: `NotFound`, `Forbidden`, `Unauthorized`, `PaymentRequired`, `Conflict`, `UnprocessableEntity`.
**Context:** Implementando handler que precisava retornar 402 Payment Required para usuÃ¡rio sem crÃ©ditos.

**Current behavior:**
`CommandResult<T>.Failure()` aceita apenas `string message`. NÃ£o existe overload com `HttpStatusCode`.

**Problem / Gap:**
Para retornar status HTTP semÃ¢ntico (402, 403, 409â€¦) de dentro de um handler, nÃ£o Ã© possÃ­vel usar `CommandResult.Failure()`. A saÃ­da forÃ§ada Ã© lanÃ§ar `ValidationException` diretamente com `ValidationError` contendo o status code â€” padrÃ£o que nÃ£o estÃ¡ documentado nos skills e parece um abuso da exceÃ§Ã£o de validaÃ§Ã£o para casos que nÃ£o sÃ£o erros de validaÃ§Ã£o.

```csharp
// Ãšnico caminho possÃ­vel â€” nÃ£o intuitivo
throw new ValidationException(new ValidationResult([
    new ValidationError("field", "message", HttpStatusCode.PaymentRequired)
]));
// Namespace: Myth.Exceptions (nÃ£o Myth.Guard.Exceptions)
```

**Suggested improvement:**
1. Adicionar overload `CommandResult<T>.Failure(string message, HttpStatusCode statusCode)`.
2. Ou expor um factory method semÃ¢ntico: `CommandResult<T>.PaymentRequired(string message)`, `CommandResult<T>.Forbidden(string message)`.
3. Documentar no skill de `myth-flow-actions` o padrÃ£o atual com `ValidationException` enquanto o overload nÃ£o existe.

---

## QueryResult nÃ£o tem .NotFound() â€” anti-padrÃ£o forÃ§ado

**Library:** Myth.Flow.Actions
**Discovered:** 2026-05-20
**Status:** âœ… RESOLVED â€” `QueryResult<T>.NotFound()`, `Forbidden()`, `Unauthorized()`, `Failure(string, HttpStatusCode)` implementados.
**Context:** Implementando queries onde a entidade pode nÃ£o existir (ex: buscar projeto por ID que o usuÃ¡rio nÃ£o tem acesso).

**Current behavior:**
`QueryResult<T>` sÃ³ tem `.Success(value)`. NÃ£o existe `.NotFound()`, `.Failure()` ou qualquer forma de representar ausÃªncia sem usar exceÃ§Ã£o.

**Problem / Gap:**
Para "nÃ£o encontrado" em queries, o Ãºnico caminho Ã© `return QueryResult<T>.Success(null!)` e checar `null` no controller para retornar 404. Isso faz o tipo mentir â€” `Success` com valor nulo nÃ£o Ã© sucesso. O controller fica responsÃ¡vel por lÃ³gica de domÃ­nio (o que Ã© um retorno vÃ¡lido vs nÃ£o encontrado), violando a separaÃ§Ã£o de responsabilidades.

```csharp
// Handler â€” forÃ§ado a retornar Success com null
return QueryResult<ProjectDto>.Success(null!);

// Controller â€” precisa checar null manualmente
var result = await dispatcher.DispatchQueryAsync(query, ct);
if (result.Value is null) return NotFound();
return Ok(result.Value);
```

**Suggested improvement:**
1. Adicionar `QueryResult<T>.NotFound()` com status HTTP 404 implÃ­cito.
2. Adicionar `QueryResult<T>.Forbidden()` para casos de acesso negado.
3. O controller poderia entÃ£o usar um helper `result.ToActionResult()` que mapeie automaticamente os estados para respostas HTTP.

---

## Namespaces inconsistentes entre IQuery, ICommand e seus handlers

**Library:** Myth.Interfaces / Myth.Flow.Actions
**Discovered:** 2026-05-15
**Status:** âœ… RESOLVED â€” `ICommand`, `IQuery`, `ICommandHandler`, `IQueryHandler`, `IDispatcher` consolidados em `Myth.Interfaces`. Documentado na seÃ§Ã£o "Namespace quick reference" do `AGENT_MIGRATION_GUIDE.md`. Confirmado pelo `myth-template`: `ICommand<Guid>` importado via `using Myth.Interfaces;` sem erros.
**Context:** Criando novos handlers CQRS e recebendo erros de compilaÃ§Ã£o por namespace errado.

**Current behavior:**
Os tipos centrais do CQRS estÃ£o espalhados em namespaces diferentes sem lÃ³gica aparente:

| Tipo | Namespace |
|---|---|
| `IQuery<TResult>` | `Myth.Interfaces` |
| `ICommand<TResult>` | `Myth.Flow.Actions` |
| `ICommandHandler<TCommand, TResult>` | `Myth.Interfaces` |
| `IQueryHandler<TQuery, TResult>` | `Myth.Interfaces` |
| `ValidationException` | `Myth.Exceptions` |

**Problem / Gap:**
`ICommand` estÃ¡ em `Myth.Flow.Actions` mas `ICommandHandler` estÃ¡ em `Myth.Interfaces`. Um desenvolvedor que aprende pelo skill espera que command e handler estejam no mesmo namespace. O erro de compilaÃ§Ã£o gerado nÃ£o diz qual namespace estÃ¡ errado â€” simplesmente "type not found". Custo: tempo perdido toda vez que um novo handler Ã© criado.

**Suggested improvement:**
1. Consolidar todos os tipos CQRS em `Myth.Flow.Actions` (ou manter em `Myth.Interfaces` com re-exports via `using`).
2. Adicionar tabela de namespaces no skill `myth-flow-actions` como referÃªncia rÃ¡pida.
3. Considerar um arquivo de convenience `using Myth.Cqrs` que importe todos os tipos necessÃ¡rios de uma vez.

---

## ValidationException em Myth.Exceptions, nÃ£o Myth.Guard.Exceptions

**Library:** Myth.Guard / Myth.Exceptions
**Discovered:** 2026-05-20
**Status:** âœ… RESOLVED â€” Documentado na seÃ§Ã£o "Namespace quick reference" do `AGENT_MIGRATION_GUIDE.md` com aviso explÃ­cito: `using Myth.Exceptions; // ValidationException â† NOT Myth.Guard.Exceptions`. Todos os exemplos do guide usam o namespace correto.
**Context:** LanÃ§ando ValidationException em handler para retornar 402, import errado gerou erro de compilaÃ§Ã£o confuso.

**Current behavior:**
`ValidationException` estÃ¡ no namespace `Myth.Exceptions`, mas o skill `myth-guard` nÃ£o menciona isso. A expectativa natural Ã© que esteja em `Myth.Guard` ou `Myth.Guard.Exceptions`.

**Problem / Gap:**
Desenvolvedores que trabalham com validaÃ§Ã£o via `myth-guard` assumem que `ValidationException` estÃ¡ em `Myth.Guard.Exceptions`. O namespace real (`Myth.Exceptions`) nÃ£o aparece em nenhum exemplo do skill. O erro de compilaÃ§Ã£o diz "type not found" sem sugerir o namespace correto.

**Suggested improvement:**
1. Adicionar `using Myth.Exceptions;` em todos os exemplos de cÃ³digo do skill `myth-guard` que lanÃ§am `ValidationException`.
2. Ou mover `ValidationException` para `Myth.Guard.Exceptions` (breaking change, mas mais intuitivo).
3. Documentar a tabela completa de namespaces de exceÃ§Ã£o no skill.

---

## FluentRuleBuilder nÃ£o tem MaxLength e NotDefault nÃ£o estÃ¡ documentado

**Library:** Myth.Guard
**Discovered:** 2026-05-21
**Status:** âœ… RESOLVED â€” `MaxLength(int)` existe como alias de `MaximumLength(int)` em `FluentRuleBuilderExtensions`. `NotDefault()` implementado no `FluentRuleBuilder` base com suporte a nullable.
**Context:** Validando campos string com tamanho mÃ¡ximo e campos Guid obrigatÃ³rios em commands.

**Current behavior:**
O `FluentRuleBuilder` (usado em `Validate()` via `ValidationBuilder<T>`) nÃ£o possui mÃ©todo `MaxLength()`. Para validar Guid obrigatÃ³rio, o mÃ©todo correto Ã© `NotDefault()`, nÃ£o `NotEmpty()` â€” mas isso nÃ£o estÃ¡ documentado no skill.

**Problem / Gap:**
- Chamar `NotEmpty()` em um `Guid` compila mas nÃ£o valida corretamente (Guid.Empty passa).
- Para `MaxLength`, o desenvolvedor precisa usar `.Must(v => v.Length <= 200)` manualmente, perdendo a mensagem de erro padronizada.

```csharp
// WRONG â€” compila mas nÃ£o valida Guid.Empty
RuleFor(x => x.OrganizationId).NotEmpty();

// CORRECT â€” rejeita Guid.Empty corretamente
RuleFor(x => x.OrganizationId).NotDefault();

// MaxLength â€” workaround necessÃ¡rio, sem mÃ©todo nativo
RuleFor(x => x.Name).Must(v => v?.Length <= 100).WithMessage("Max 100 chars");
```

**Suggested improvement:**
1. Adicionar `MaxLength(int max)` no `FluentRuleBuilder` com mensagem de erro padrÃ£o.
2. Documentar `NotDefault()` no skill `myth-guard` com exemplo explÃ­cito para `Guid` e outros value types.
3. Adicionar seÃ§Ã£o "Common Validation Pitfalls" no skill com `NotDefault` vs `NotEmpty`.

---

## AGENT_MIGRATION_GUIDE â€” SeÃ§Ã£o 6: exemplo de teste insuficiente para pipelines com mÃºltiplos Transforms

**Library:** Myth.Flow / AGENT_MIGRATION_GUIDE.md
**Discovered:** 2026-06-04
**Status:** âœ… RESOLVED 2026-06-04 â€” SeÃ§Ã£o 6 do `AGENT_MIGRATION_GUIDE.md` atualizada para usar `GetBaseException()` em vez de `.InnerException`, com nota explicativa sobre aninhamento proporcional de `PipelineException` por step.
**Context:** Aplicando a migraÃ§Ã£o descrita na seÃ§Ã£o 6 do `AGENT_MIGRATION_GUIDE.md` ao projeto `myth-template`. Os testes de `PostAsync` (que usa `.Process<T,R>()` + `.Transform()` + `.Publish()`) continuavam falhando mesmo apÃ³s aplicar o padrÃ£o de `InnerException` descrito no guide.

**Current behavior (guide):**
O guide instrui usar:
```csharp
// âœ… NEW test assertion â€” PipelineException with inner exception
var thrown = await act.Should().ThrowAsync<PipelineException>();
thrown.Which.InnerException.Should().BeOfType<ArgumentException>();
```

Isso funciona quando hÃ¡ **um Ãºnico** Transform no pipeline. Mas quando existem mÃºltiplos steps que usam Transform internamente (ex: `.Process<T,R>()` + `.Transform(result => ...)` + `.Publish()`), a exceÃ§Ã£o Ã© envolvida vÃ¡rias vezes:

```
PipelineException (from .Publish())
  â†’ PipelineException (from .Transform())
    â†’ PipelineException (from .Process<T,R>())
      â†’ ValidationException (original)
```

Nesse caso, `.Which.InnerException` Ã© outro `PipelineException`, nÃ£o a `ValidationException`.

**Problem / Gap:**
O exemplo do guide assume aninhamento Ãºnico, mas na prÃ¡tica pipelines com mÃºltiplos steps de Transform produzem aninhamento profundo. O padrÃ£o recomendado com `.InnerException` nÃ£o funciona de forma genÃ©rica.

**Discovered fix:**
Usar `GetBaseException()` que percorre toda a cadeia de InnerException atÃ© encontrar a exceÃ§Ã£o raiz â€” funciona independentemente da profundidade:

```csharp
// âœ… Robust â€” works for any nesting depth
var thrown = await act.Should().ThrowAsync<PipelineException>();
var response = thrown.Which.GetBaseException().Should().BeOfType<ValidationException>().Which;
response.Message.Should().NotBeEmpty();
response.ValidationResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
```

**Suggested improvement:**
1. Atualizar a seÃ§Ã£o 6 do `AGENT_MIGRATION_GUIDE.md` para usar `GetBaseException()` em vez de `.InnerException` diretamente.
2. Adicionar nota explicando que mÃºltiplos Transform steps causam aninhamento proporcional de PipelineExceptions.
3. Mencionar que `.Process<T,R>()`, `.Query<T,R>()`, `.Transform()`, e `.Publish()` TODOS usam Transform internamente â€” nÃ£o apenas o `.Transform()` explÃ­cito.

---

## PadrÃ£o de validaÃ§Ã£o de entidade em Validate() nÃ£o tem primitivo Myth

**Library:** Myth.Guard
**Discovered:** 2026-05-21
**Status:** âœ… RESOLVED â€” `RespectAsync(async (value, ct, sp) => ...)` e `RespectAsync<TEntity>(async (value, entity, ct, sp) => ...)` jÃ¡ existem no `FluentRuleBuilder`. Documentados no SKILL.md (seÃ§Ã£o "Async Business Rule Validation") e README (seÃ§Ã£o "Async Validation with Service Provider") com exemplos de entity existence check, unique constraint, plan limits e credit check.
**Context:** Implementando validaÃ§Ã£o de existÃªncia de entidade, verificaÃ§Ã£o de crÃ©ditos e limites de plano dentro do mÃ©todo `Validate()` de commands.

**Current behavior:**
Myth.Guard provÃª `ValidationBuilder<T>` para regras sÃ­ncronas simples. Para validaÃ§Ãµes assÃ­ncronas que consultam o banco (entity existence, credit balance, plan limits, permissions), nÃ£o existe nenhum primitivo â€” o projeto precisou criar:
- `EntityValidationService` â€” serviÃ§o que encapsula queries de existÃªncia
- `RulesExtensions` â€” extension methods com nomes semÃ¢nticos (`WorkspaceMustExistAsync()`, `UserHasSufficientAiCreditsAsync()`, etc.)
- Acesso via reflection para ler propriedades do command genÃ©rico (`OrganizationId`, `ProjectId`, `Count`) em regras genÃ©ricas compartilhadas

**Problem / Gap:**
Todo projeto que usa Myth.Guard para validaÃ§Ã£o sÃ­ncrona vai precisar reinventar esse padrÃ£o para validaÃ§Ãµes assÃ­ncronas. NÃ£o existe guia, interface ou base class no ecosystem para isso. O uso de reflection para acessar propriedades do command em regras genÃ©ricas Ã© frÃ¡gil e quebra silenciosamente se a propriedade for renomeada.

**Suggested improvement:**
1. Adicionar suporte a regras assÃ­ncronas no `ValidationBuilder<T>`: `.RuleForAsync(x => x.WorkspaceId, async id => await repo.ExistsAsync(id))`.
2. Criar interface `IAsyncValidationRule<TCommand>` que handlers de validaÃ§Ã£o possam implementar e registrar no DI, separando validaÃ§Ã£o de negÃ³cio da definiÃ§Ã£o do command.
3. Documentar o padrÃ£o `EntityValidationService` + `RulesExtensions` como arquitetura recomendada enquanto o suporte nativo nÃ£o existe.

---

## WriteRepositoryAsync.UpdateAsync â€” DbUpdateConcurrencyException no EF InMemory ao atualizar entidade em estado Added

**Library:** Myth.Repository.EntityFramework
**Discovered:** 2026-06-05
**Status:** âœ… FIXED 2026-06-05 (v4.4.2) â€” `UpdateAsync` e `UpdateRangeAsync` agora verificam se o estado atual Ã© `Added` antes de mudar para `Modified`. Se jÃ¡ for `Added`, o estado Ã© mantido (as modificaÃ§Ãµes jÃ¡ estÃ£o rastreadas pelo change tracker).

**Context:** Implementando testes e2e para `RegisterCommandHandler` (MindCircle). O handler chama `AddAsync(project)` e depois `UpdateAsync(project)` para atribuir o workspace ao projeto â€” tudo dentro do mesmo scope de DbContext, antes do `SaveChangesAsync`.

**Current behavior (before fix):**
```csharp
// WriteRepositoryAsync.UpdateAsync
public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default) =>
    AttachAsync(entity, cancellationToken)
        .ContinueWith((_) => _context.Entry(entity).State = EntityState.Modified, cancellationToken);
```

Quando `AddAsync(entity)` Ã© chamado antes de `SaveChangesAsync`, o entity fica com `EntityState.Added`. Em seguida, `UpdateAsync(entity)` muda para `EntityState.Modified`. No `SaveChangesAsync` com EF InMemory, o provider tenta fazer UPDATE em uma entidade que ainda nÃ£o existe no store (nunca foi inserida), lanÃ§ando:

```
Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException:
Attempted to update or delete an entity that does not exist in the store.
```

O Dispatcher captura essa exceÃ§Ã£o e retorna `CommandResult.Failure()` com `Value = null`. O controller acessa `result.Value!.Prop` e lanÃ§a `NullReferenceException`.

**Problem / Gap:**
1. O padrÃ£o Add â†’ Update â†’ SaveChanges Ã© comum em handlers que criam entidades relacionadas e precisam atualizar referÃªncias (ex: `project.SetWorkspace(workspaceId)` apÃ³s criar a workspace). Com PostgreSQL isso funciona porque as operaÃ§Ãµes sÃ£o agrupadas em uma transaÃ§Ã£o real.
2. Com EF InMemory (usado em testes), nÃ£o hÃ¡ transaÃ§Ã£o. O provider processa cada operaÃ§Ã£o como atÃ´mica: Modified â†’ UPDATE imediato no store em memÃ³ria, mas a entidade ainda nÃ£o foi inserida.
3. O erro Ã© capturado silenciosamente pelo Dispatcher, tornando o diagnÃ³stico muito difÃ­cil â€” apenas aparece como `NullReferenceException` no controller, sem a causa raiz.

**Fix applied:**
```csharp
// UpdateAsync â€” nÃ£o sobrescreve Added com Modified
public virtual Task UpdateAsync(T entity, CancellationToken cancellationToken = default) =>
    AttachAsync(entity, cancellationToken)
        .ContinueWith((_) => {
            var entry = _context.Entry(entity);
            if (entry.State != EntityState.Added)
                entry.State = EntityState.Modified;
        }, cancellationToken);

// UpdateRangeAsync â€” mesma correÃ§Ã£o para cada entidade
public virtual Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default) =>
    AttachRangeAsync(entities, cancellationToken)
        .ContinueWith(task => {
            foreach (var entity in entities) {
                var entry = _context.Entry(entity);
                if (entry.State != EntityState.Added)
                    entry.State = EntityState.Modified;
            }
        }, cancellationToken);
```

**Reasoning:** Se a entidade jÃ¡ estÃ¡ em `Added`, o change tracker jÃ¡ captura todas as modificaÃ§Ãµes em memÃ³ria. Mudar para `Modified` Ã© desnecessÃ¡rio e destrutivo com InMemory. O fix Ã© seguro em PostgreSQL tambÃ©m â€” entidades `Added` serÃ£o inseridas corretamente no `SaveChangesAsync`, com todas as propriedades modificadas aplicadas.

**Suggested improvement:**
JÃ¡ corrigido em v4.4.2. Considerar adicionar um teste unitÃ¡rio ao `Myth.Repository.Test` que cobre o cenÃ¡rio Add â†’ UpdateAsync â†’ SaveChangesAsync com InMemory para evitar regressÃ£o.

## BeStatusCodeCreated — nao suporta CreatedAtRouteResult

**Library:** Myth.Testing
**Discovered:** 2026-06-05
**Status:** ✅ RESOLVED 2026-06-06 — `BeStatusCodeCreated` agora usa `BeObjectResultStatusCode<ObjectResult>` em vez de `BeObjectResultStatusCode<CreatedResult>`, aceitando qualquer `ObjectResult` com status 201. `BeStatusCodeUnprocessableEntity` também corrigida (usava `HttpStatusCode.OK` no path ObjectResult — typo).

**Context:** Escrevendo testes e2e para ProjectsController, cujo CreateProjectAsync retorna CreatedAtRoute() (nao Created()).

**Current behavior:**
BeStatusCodeCreated() em FluentAssertionExtensions tenta fazer cast para CreatedResult.
BeObjectResultStatusCode<CreatedResult> chama .As<CreatedResult>() sobre um CreatedAtRouteResult, que retorna null (o tipo nao bate). A chamada a .StatusCode entao lanca NullReferenceException.

**Problem / Gap:**
CreatedAtRouteResult herda de ObjectResult (com StatusCode = 201), mas NAO herda de CreatedResult. O helper assume que todo retorno 201 e CreatedResult, o que nao e verdade para CreatedAtRoute().
O erro e silencioso - ao inves de "expected 201 but got 200", o desenvolvedor recebe uma NullReferenceException generica que nao indica a causa raiz.

**Suggested improvement:**
Fazer BeStatusCodeCreated aceitar qualquer ObjectResult com status 201, em vez de fazer cast especifico para CreatedResult.
Tambem documentar que a extensao so funciona com Created() e nao com CreatedAtRoute()/CreatedAtAction().

## PipelineExtensions.TapAsync silencia ValidationException — Ok(null) em vez de PipelineException

**Library:** Myth.Flow.Actions
**Discovered:** 2026-06-05
**Status:** ✅ RESOLVED 2026-06-06 — Adicionada interface `IStatusCodeException` em `Myth.Commons`. `ValidationException` (Myth.Guard) e `PipelineException` (Myth.Flow) implementam a interface. `PipelineBuilder.ExecuteAsync` agora usa `(ex as IStatusCodeException)?.StatusCode` no catch final, preservando o StatusCode de qualquer exceção que implemente a interface. `PipelineException(message, inner)` ctor atualizado para propagar StatusCode via `IStatusCodeException` (não só de inner `PipelineException`).
**Context:** Implementando testes E2E para AIController — GenerateSuggestionsAsync usa TapAsync para executar validator.ValidateAsync() antes do Process<,>.

**Current behavior:** Quando validator.ValidateAsync() lanca ValidationException dentro do TapAsync, o pipeline captura a excecao e converte o estado em falha (Result.Failure). O controller, que nao verifica result.IsSuccess, executa return Ok(result.Value) onde result.Value e null. Resultado: HTTP 200 com body null em vez de propagar o erro.

**Problem / Gap:** O contrato implícito de PipelineExtensions.TapAsync e ser um side-effect. Quando uma ValidationException e lancada dentro do TapAsync, o comportamento esperado e que a excecao propague como PipelineException (igual ao que acontece com Process). No entanto, o pipeline silencia a excecao — tornando o erro invisivel para o controller. Isso e especialmente problematico quando a validacao inclui regras de negocio (CountMustNotExceedAiSuggestionPlanLimitAsync, UserCanCreatePersonalWorkspaceAsync) que retornam HTTP 4xx no ambiente de producao via UseGuard middleware.

**Suggested improvement:**
1. O TapAsync deveria re-lancar ValidationException/PipelineException em vez de silencia-las.
2. OU documentar explicitamente que TapAsync nao deve ser usado para validacoes que lancam excecao — validator.ValidateAsync() deve ser chamado fora do pipeline ou em um Step dedicado.
3. OU adicionar uma variante TapValidateAsync que propaga erros corretamente.


## ChoosePlanCommand validator silently rejects B2C plans (plus/pro)

**Library:** Myth.Guard + application validator
**Discovered:** 2026-06-07
**Status:** 📋 OUT OF SCOPE — `ChoosePlanCommand` e `SubscriptionPlan` são de aplicação específica, fora das bibliotecas Myth. A melhoria é responsabilidade da aplicação consumidora.
**Context:** Trying to set up Plus/Pro subscription in E2E tests via `ChoosePlanAsync("plus")`.

**Current behavior:** `ChoosePlanCommand.Validate()` uses `.Respect(plan => plan is "free" or "team" or "growth" or "enterprise")` — explicitly excluding "plus" and "pro". Meanwhile `SubscriptionPlan` constant class only defines Free/Plus/Pro values. The "team/growth/enterprise" string values the validator accepts have no corresponding `SubscriptionPlan` constant.

**Problem / Gap:** Complete inconsistency between validator-accepted plan strings and the `SubscriptionPlan` constant class. B2C plans (plus/pro) must be set via Stripe webhook, not `ChoosePlanAsync`. In tests, this forces direct DB seeding via `ApplicationContext` to set Plus/Pro subscriptions.

**Suggested improvement:** Document clearly that `ChoosePlanAsync` is only for B2B/enterprise onboarding, or add a test-friendly override for setting B2C subscription state.

## CreateInstance<TInterface>() throws for interfaces — use concrete types

**Library:** Myth.Testing (BaseTests.CreateInstance<T>)
**Discovered:** 2026-06-07
**Status:** ✅ RESOLVED 2026-06-09 — `GetService<T>()` e `GetRequiredService<T>()` já existem em `BaseTests`. XMLDoc de `CreateInstance<T>` atualizado para orientar o uso de `GetRequiredService<T>()` para interfaces e tipos abstratos.
**Context:** Tried `CreateInstance<IAiCreditCouponRepository>()` to seed test data.

**Current behavior:** `CreateInstance<T>()` calls `ActivatorUtilities.CreateInstance(serviceProvider, typeof(T))` which tries to INSTANTIATE the type directly rather than resolve from DI. This fails with "Instances of abstract classes cannot be created" for interfaces and abstract classes.

**Problem / Gap:** To get a registered service, you must use `CreateInstance<ConcreteClass>()`. There's no `GetService<T>()` or `ServiceProvider.GetRequiredService<T>()` method exposed on BaseTests.

**Suggested improvement:** Expose a `GetService<T>()` method on `BaseTests` that resolves from the service provider without instantiation (i.e., calls `ServiceProvider.GetRequiredService<T>()`). This would allow tests to resolve repository interfaces and other services cleanly.

## GetPublicLinkAsync return type incompatible with BeStatusCodeOk

**Library:** Myth.Extensions (FluentAssertionExtensions)
**Discovered:** 2026-06-07
**Status:** ✅ RESOLVED 2026-06-09 — `BeStatusCodeOk` agora usa `BeObjectResultStatusCode<ObjectResult>` em vez de `BeObjectResultStatusCode<OkObjectResult>`, aceitando qualquer `ObjectResult` com status 200. Null guards adicionados em `BeStatusCode<T>`, `BeObjectResultStatusCode<T>` e `BeContentResult` — type mismatch agora produz mensagem de assertion útil em vez de `NullReferenceException`.
**Context:** Testing `ProjectsController.GetPublicLinkAsync` which uses `StatusCode((int)result.StatusCode, value)` pattern.

**Current behavior:** `BeStatusCodeOk()` expects an `ObjectResult` as `assertions.Subject`. When the controller uses `StatusCode(int, object)` but `result.StatusCode` is a `QueryResult` status that doesn't map cleanly, the assertion throws `NullReferenceException` inside `BeObjectResultStatusCode<T>`.

**Problem / Gap:** `BeStatusCodeOk()` should handle `ObjectResult` with non-200 status codes gracefully (return failed assertion, not NullReferenceException).

**Suggested improvement:** Add null guard in `BeObjectResultStatusCode<T>` before accessing status code.

---

## Myth.Flow — Process<TCmd>() (void) does not wrap ValidationException in PipelineException

**Library:** Myth.Flow.Actions
**Discovered:** 2026-06-07
**Status:** ✅ RESOLVED 2026-06-09 — `Process<TCmd>()` void agora chama `.Transform(state => state)` internamente antes de adicionar seu step de dispatch, exatamente como `Process<TCmd, TResult>()` usa `.Transform()` para mudar o tipo. Isso garante que exceções dos steps anteriores (ex.: `TapAsync`) sejam envolvidas em `PipelineException` consistentemente, sem alterar o comportamento do `ExceptionFilter` no `ExecuteAsync` (que continua relançando o tipo original para pipelines sem Transform).
**Context:** Fixing E2E test `DeleteEdge_WhenNotExists_ShouldThrowPipelineException` — test expected `PipelineException` but received `ValidationException` directly.

**Current behavior:** When a pipeline step uses `.Process<TCommand>()` (the void/`ICommand` overload), a `ValidationException` thrown during `.TapAsync(validator.ValidateAsync)` propagates as `ValidationException` directly to the caller. When `.Process<TCommand, TResult>()` (the typed overload) is used, the exception is wrapped as `PipelineException` with the original `ValidationException` as inner exception.

**Problem / Gap:** The asymmetry is invisible at the call site. Code that switches from `ICommand<T>` to `ICommand` (for void operations) silently changes the exception type visible to callers and breaks tests/middleware that catch `PipelineException`. The developer has no warning about this behavior difference.

**Suggested improvement:** Ensure that `Process<TCommand>()` wraps thrown exceptions in `PipelineException` consistently with `Process<TCommand, TResult>()`. Alternatively, document this asymmetry explicitly in XMLDoc on both overloads so developers know to use `ICommand<bool>` for commands that need consistent exception wrapping.
