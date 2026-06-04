# Melhorias e Oportunidades — Myth Ecosystem

Cada seção é uma entrada independente. Formato: título, biblioteca, data, contexto, comportamento atual, problema/lacuna, sugestão.

---

## SchemaRegistry.MapGenericTypes — Items é null ao mapear IPaginated<TSource> → IPaginated<TDest>

**Library:** Myth.Morph
**Discovered:** 2026-06-04
**Status:** ✅ RESOLVED 2026-06-04 — `MapGenericTypes` agora detecta tipos sem propriedades graváveis (ex.: `Paginated<T>` com `private set`) e usa mapeamento orientado a construtor via `CreateInstanceFromSource`.
**Context:** Chamada `result.To<IPaginated<GetWeatherStationResponse>>()` em query handler retorna `Paginated<GetWeatherStationResponse>` com `Items = null`. O `.Tap()` seguinte que chama `pipeline.CurrentRequest!.Items.Count()` lança `NullReferenceException` silenciada pelo pipeline.

**Current behavior:**
`SchemaRegistry.MapGenericTypes` cria a instância de destino via `CreateInstance(Paginated<GetWeatherStationResponse>)`. Como `Paginated<T>` não tem construtor sem parâmetros, `CreateInstance` usa o construtor primário `(int pageNumber, int pageSize, int totalItems, int totalPages, IEnumerable<T> items)` com valores padrão resolvidos via DI. O parâmetro `IEnumerable<GetWeatherStationResponse> items` não é resolvível via DI, então recebe `null` (retorno de `GetDefault(typeof(IEnumerable<>))`).

Em seguida, `MapPropertiesGeneric` tenta copiar as propriedades de `Paginated<WeatherStation>` para `Paginated<GetWeatherStationResponse>`, mas todas as propriedades de `Paginated<T>` têm `private set` — portanto `CanWrite = false` para todas. Nenhuma propriedade é copiada. O objeto destino permanece com `Items = null`, `PageNumber = 0`, `TotalItems = 0`, etc.

```csharp
// SchemaRegistry.CreateInstance — parâmetros sem DI recebem GetDefault():
private static object? GetDefault(Type type) =>
    type.IsValueType ? Activator.CreateInstance(type) : null; // IEnumerable<T> → null

// MapPropertiesGeneric — private set bloqueia escrita:
var destProperties = destType
    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
    .Where(p => p.CanWrite)  // Paginated<T>: todos private set → nenhum passa
    .ToArray();
```

**Problem / Gap:**
1. `Paginated<T>` é um tipo de valor central do ecossistema e é completamente inutilizável com o mapper genérico — silenciosamente retorna um objeto vazio.
2. Qualquer chamada `somePaginated.To<IPaginated<TDto>>()` produz resultado corrompido sem erros ou warnings.
3. O `.Tap()` que chama `Items.Count()` recebe `NullReferenceException` silenciada pelo pipeline (o Myth Flow swallows exceções em `.Tap()`), tornando o diagnóstico ainda mais difícil.

**Suggested improvement:**
Duas abordagens, da mais simples à mais robusta:

**Opção A (mínimo):** Fazer `Paginated<T>` implementar `IMorphableTo<Paginated<TDest>>` usando um tipo especial, ou adicionar um `IMorphableFrom` estático. Exemplo concreto: `SchemaRegistry` poderia detectar que o destino é `Paginated<T>` (genérico conhecido) e chamar o construtor com os valores corretos mapeados dos scalars + items mapeados.

**Opção B (correto):** Adicionar suporte a tipos "construtor-driven" no `SchemaRegistry`. Quando `CreateInstance` falha para construir com defaults válidos (ex.: `IEnumerable<T>` → null), tentar identificar quais propriedades/parâmetros são "coleções de elementos" e mapear os elementos antes de construir. Pseudocódigo:

```csharp
// Detectar que items precisa de mapeamento de coleção:
// 1. Identificar parâmetros cujo tipo é IEnumerable<TElement>
// 2. Encontrar a propriedade source com o mesmo nome
// 3. Mapear os elementos (WeatherStation → GetWeatherStationResponse)
// 4. Construir Paginated<GetWeatherStationResponse>(srcPageNumber, srcPageSize, srcTotal, srcTotalPages, mappedItems)
```

**Opção C (paliativo no template):** Em vez de `result.To<IPaginated<GetWeatherStationResponse>>()`, construir o paginated manualmente no handler:

```csharp
var items = result.Items.To<WeatherStation, GetWeatherStationResponse>();
var response = items.AsPaginated(result.TotalItems, result.PageSize, (result.PageNumber - 1) * result.PageSize);
```

---

## SearchAsync — comportamento de tracking não documentado

**Library:** Myth.Repository.EntityFramework
**Discovered:** 2026-05-28
**Status:** ✅ RESOLVED — XMLDoc adicionado em `SearchAsync` documentando change tracking; `SearchAsNoTrackingAsync` implementado.
**Context:** Investigando um bug de soft-delete em cadeia que precisava modificar entidades retornadas por `SearchAsync` e salvar com `SaveChangesAsync`.

**Current behavior:**
`SearchAsync` retorna entidades **rastreadas** (EF Core change tracking ativo). Não usa `AsNoTracking()`.

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
Não é óbvio pelo nome ou documentação que as entidades retornadas são rastreadas. Desenvolvedores podem assumir erroneamente que são não-rastreadas (como seria com `AsNoTracking`) e chamar `repo.UpdateAsync(entity)` desnecessariamente, ou pior, achar que modificações diretas não serão salvas. O padrão válido — modificar entidades retornadas e chamar `uow.SaveChangesAsync` diretamente — não aparece em nenhum skill ou README.

**Suggested improvement:**
1. Adicionar XMLDoc no método `SearchAsync` explicitando que entidades são rastreadas pelo change tracker.
2. Adicionar overload `SearchAsNoTrackingAsync` para casos de leitura pura (relatórios, projeções), deixando a intenção explícita no nome do método.

---

## SearchAsync retorna IEnumerable, não List — armadilha do .Count

**Library:** Myth.Repository.EntityFramework
**Discovered:** 2026-05-21
**Status:** ✅ RESOLVED — `SearchAsync` e `SearchAsNoTrackingAsync` retornam `IReadOnlyList<TEntity>` (já materializado via `ToListAsync`); XMLDoc documenta que `.Count()` (com parênteses) deve ser usado.
**Context:** Contando resultados de `SearchAsync` para verificar limites de plano.

**Current behavior:**
`SearchAsync` declara retorno como `IEnumerable<TEntity>`, não `List<T>`. Chamar `.Count` (propriedade) compila mas retorna sempre 1 porque acessa `IEnumerable.Count` da interface, não a contagem real dos elementos.

**Problem / Gap:**
Nenhuma documentação ou skill alerta para esse comportamento. O erro é silencioso — compila sem warning, produz valor errado em runtime. Verificações de limite de plano baseadas em `.Count` falham silenciosamente.

```csharp
// WRONG — .Count é propriedade de IEnumerable (retorna 1, não o tamanho real)
var total = results.Count;

// CORRECT — .Count() é extension method do LINQ que itera a coleção
var total = results.Count();
```

**Suggested improvement:**
1. Documentar na XMLDoc de `SearchAsync` que o retorno é `IEnumerable<T>` e que `.Count()` (com parênteses) deve ser usado.
2. Considerar mudar o retorno para `IReadOnlyList<TEntity>` para eliminar a ambiguidade, já que a coleção já está materializada internamente via `ToListAsync`.

---

## Query/Process com tipo de retorno capturam exceções silenciosamente em testes

**Library:** Myth.Flow / Myth.Flow.Actions
**Discovered:** 2026-06-02
**Context:** Implementando testes e2e para `WeatherStationController` usando `BaseDatabaseTests`. Os testes que chamam `.Query<T,R>()` e `.Process<T,R>()` (com tipo de retorno `Guid`, por exemplo) não propagam `ValidationException` lançadas em `.TapAsync()` anterior ao step de execução.

**Current behavior:**
Quando uma `ValidationException` é lançada dentro de `.TapAsync()` antes de `.Query<T,R>()` ou `.Process<T,R>()`:
- A exceção é capturada pelo framework internamente
- `ExecuteAsync()` retorna um resultado de erro (com `Value = default(T)`)
- Nenhuma exceção chega ao chamador

Porém, quando a exceção ocorre antes de `.Process()` (sem tipo de retorno):
- A exceção **propagada** normalmente ao chamador

```csharp
// NÃO propaga — Query captura internamente:
PipelineExtensions.Start(query)
    .TapAsync(pipeline => validator.ValidateAsync(pipeline.CurrentRequest!)) // throws
    .Query<TQuery, TResult>()  // captura a exceção
    .ExecuteAsync(ct);  // retorna Result<TResult> com erro, não lança

// PROPAGA — Process void não captura:
PipelineExtensions.Start(command)
    .TapAsync(pipeline => validator.ValidateAsync(pipeline.CurrentRequest!)) // throws
    .Process()  // não captura
    .ExecuteAsync(ct);  // lança a exceção
```

**Problem / Gap:**
1. Testes e2e que chamam o controller diretamente (`controller.GetByIdAsync(Guid.Empty)`) não conseguem testar cenários de erro de validação para endpoints que usam `.Query<T,R>()` — a exceção nunca chega ao teste.
2. O comportamento é assimétrico entre `.Process()` void e `.Process<T,R>()`/`.Query<T,R>()`, o que não é documentado e cria surpresas.
3. Em testes, `UseExceptionFilter<ValidationException>()` não é suficiente para distinguir comportamento esperado de falha silenciosa.

**Suggested improvement:**
1. Documentar explicitamente no skill `myth-flow-actions` e `myth-flow` a diferença de comportamento entre `.Process()` void e `.Process<T,R>()`/`.Query<T,R>()` com relação ao tratamento de exceções.
2. Considerar expor um `Result<T>` do pipeline que permita ao teste inspecionar erros sem exigir que exceções propagadas.
3. Ou: adicionar um método `.ThrowOnError()` ao pipeline que force rethrow de erros capturados, facilitando testes.

---

## IUnitOfWorkRepository.BeginTransactionAsync() falha silenciosamente com InMemory EF

**Library:** Myth.Repository.EntityFramework
**Discovered:** 2026-06-02
**Status:** ✅ RESOLVED — `BeginTransactionAsync` captura `InvalidOperationException` de providers sem suporte a transações (InMemory). Commit/Rollback/Savepoint também são no-op nesse cenário.
**Context:** Testando o endpoint `PostWithForecastsAsync` que usa `IUnitOfWorkRepository.BeginTransactionAsync()` / `CommitAsync()` / `CreateSavepointAsync()`. O handler falha silenciosamente no ambiente de testes com InMemory EF, e `.Process<T,Guid>()` captura a exceção retornando `Guid.Empty`.

**Current behavior:**
`IUnitOfWorkRepository.BeginTransactionAsync()` chama `context.Database.BeginTransactionAsync()` que lança `InvalidOperationException` no provider InMemory do EF Core ("Transactions are not supported by the in-memory store"). Essa exceção é capturada pelo `.Process<T,Guid>()` do Myth Flow, que retorna `Guid.Empty` como valor padrão.

**Problem / Gap:**
- A documentação do template diz "O provider InMemory silencia transações" — mas na prática a exceção é capturada pelo pipeline, não pelo UoW.
- O controller retorna `CreatedAtRoute(..., Guid.Empty)` como se tivesse sucesso, mesmo com o handler falhando.
- Torna impossível testar o comportamento transacional do handler em ambiente de testes com InMemory.

**Suggested improvement:**
1. `IUnitOfWorkRepository.BeginTransactionAsync()` deve tratar silenciosamente (try/catch) a exceção `InvalidOperationException` quando o provider for InMemory, retornando um `NullTransaction` que não faz nada.
2. Documentar claramente no skill `myth-repository-entity-framework` quais métodos do UoW são suportados com InMemory vs providers reais.
3. Adicionar método `IsTransactionSupported` ao `IUnitOfWorkRepository` para que handlers possam verificar antes de iniciar transações — permitindo testes mais robustos.

---

## ValidationContextKey.Create — regras globais não executam ao passar contexto explícito

**Library:** Myth.Guard
**Discovered:** 2026-06-02
**Status:** ✅ RESOLVED — `ValidationBuilder.GetRules()` já inclui `_globalRules` primeiro e depois adiciona as regras do contexto especificado. Regras globais sempre executam independente do contexto.
**Context:** Testando `PostAsync` do `WeatherStationController` que chama `validator.ValidateAsync(command, ValidationContextKey.Create)`. Em testes, regras globais (fora de `InContext`) não executam quando um context key é passado.

**Current behavior:**
Quando `IValidator.ValidateAsync(obj, ValidationContextKey.Create)` é chamado:
- Apenas as regras dentro de `builder.InContext(ValidationContextKey.Create, ...)` são executadas
- Regras globais (fora de qualquer `InContext`) são ignoradas

Isso significa que para `CreateWeatherStationCommand { Name = "", Location = "..." }`:
- A regra global `builder.For(Name, rules => rules.NotEmpty().MinLength(2).MaxLength(100))` NÃO executa
- Somente o check de unicidade via IScopedService executa (mas retorna `true` para `""` pois não existe)
- **Nenhuma exceção é lançada** — mesmo com dados inválidos

**Problem / Gap:**
1. Validação de campo obrigatório falha silenciosamente quando context key é especificado.
2. O comportamento não está documentado e vai contra o princípio de least surprise — ao especificar um contexto adicional, o desenvolvedor espera que regras globais ainda se apliquem.
3. Torna difícil testar cenários de erro em endpoints que usam `ValidationContextKey.Create`.

**Suggested improvement:**
1. Documentar claramente no skill `myth-guard`: ao usar `ValidationContextKey`, as regras globais **sempre executam**; `InContext` adiciona regras extras quando o contexto bate.
2. Se o comportamento atual é intencional (só contexto específico), documentar explicitamente com exemplo de quando usar cada abordagem.
3. Se é um bug, corrigir para que regras globais executem independente de qualquer contexto especificado.

---

## CommandResult.Failure() não aceita HttpStatusCode

**Library:** Myth.Flow.Actions
**Discovered:** 2026-05-20
**Status:** ✅ RESOLVED — `CommandResult.Failure(string, HttpStatusCode, ...)` existe. Métodos semânticos também implementados: `NotFound`, `Forbidden`, `Unauthorized`, `PaymentRequired`, `Conflict`, `UnprocessableEntity`.
**Context:** Implementando handler que precisava retornar 402 Payment Required para usuário sem créditos.

**Current behavior:**
`CommandResult<T>.Failure()` aceita apenas `string message`. Não existe overload com `HttpStatusCode`.

**Problem / Gap:**
Para retornar status HTTP semântico (402, 403, 409…) de dentro de um handler, não é possível usar `CommandResult.Failure()`. A saída forçada é lançar `ValidationException` diretamente com `ValidationError` contendo o status code — padrão que não está documentado nos skills e parece um abuso da exceção de validação para casos que não são erros de validação.

```csharp
// Único caminho possível — não intuitivo
throw new ValidationException(new ValidationResult([
    new ValidationError("field", "message", HttpStatusCode.PaymentRequired)
]));
// Namespace: Myth.Exceptions (não Myth.Guard.Exceptions)
```

**Suggested improvement:**
1. Adicionar overload `CommandResult<T>.Failure(string message, HttpStatusCode statusCode)`.
2. Ou expor um factory method semântico: `CommandResult<T>.PaymentRequired(string message)`, `CommandResult<T>.Forbidden(string message)`.
3. Documentar no skill de `myth-flow-actions` o padrão atual com `ValidationException` enquanto o overload não existe.

---

## QueryResult não tem .NotFound() — anti-padrão forçado

**Library:** Myth.Flow.Actions
**Discovered:** 2026-05-20
**Status:** ✅ RESOLVED — `QueryResult<T>.NotFound()`, `Forbidden()`, `Unauthorized()`, `Failure(string, HttpStatusCode)` implementados.
**Context:** Implementando queries onde a entidade pode não existir (ex: buscar projeto por ID que o usuário não tem acesso).

**Current behavior:**
`QueryResult<T>` só tem `.Success(value)`. Não existe `.NotFound()`, `.Failure()` ou qualquer forma de representar ausência sem usar exceção.

**Problem / Gap:**
Para "não encontrado" em queries, o único caminho é `return QueryResult<T>.Success(null!)` e checar `null` no controller para retornar 404. Isso faz o tipo mentir — `Success` com valor nulo não é sucesso. O controller fica responsável por lógica de domínio (o que é um retorno válido vs não encontrado), violando a separação de responsabilidades.

```csharp
// Handler — forçado a retornar Success com null
return QueryResult<ProjectDto>.Success(null!);

// Controller — precisa checar null manualmente
var result = await dispatcher.DispatchQueryAsync(query, ct);
if (result.Value is null) return NotFound();
return Ok(result.Value);
```

**Suggested improvement:**
1. Adicionar `QueryResult<T>.NotFound()` com status HTTP 404 implícito.
2. Adicionar `QueryResult<T>.Forbidden()` para casos de acesso negado.
3. O controller poderia então usar um helper `result.ToActionResult()` que mapeie automaticamente os estados para respostas HTTP.

---

## Namespaces inconsistentes entre IQuery, ICommand e seus handlers

**Library:** Myth.Interfaces / Myth.Flow.Actions
**Discovered:** 2026-05-15
**Status:** ✅ RESOLVED — `ICommand`, `IQuery`, `ICommandHandler`, `IQueryHandler`, `IDispatcher` consolidados em `Myth.Interfaces`. Documentado na seção "Namespace quick reference" do `AGENT_MIGRATION_GUIDE.md`. Confirmado pelo `myth-template`: `ICommand<Guid>` importado via `using Myth.Interfaces;` sem erros.
**Context:** Criando novos handlers CQRS e recebendo erros de compilação por namespace errado.

**Current behavior:**
Os tipos centrais do CQRS estão espalhados em namespaces diferentes sem lógica aparente:

| Tipo | Namespace |
|---|---|
| `IQuery<TResult>` | `Myth.Interfaces` |
| `ICommand<TResult>` | `Myth.Flow.Actions` |
| `ICommandHandler<TCommand, TResult>` | `Myth.Interfaces` |
| `IQueryHandler<TQuery, TResult>` | `Myth.Interfaces` |
| `ValidationException` | `Myth.Exceptions` |

**Problem / Gap:**
`ICommand` está em `Myth.Flow.Actions` mas `ICommandHandler` está em `Myth.Interfaces`. Um desenvolvedor que aprende pelo skill espera que command e handler estejam no mesmo namespace. O erro de compilação gerado não diz qual namespace está errado — simplesmente "type not found". Custo: tempo perdido toda vez que um novo handler é criado.

**Suggested improvement:**
1. Consolidar todos os tipos CQRS em `Myth.Flow.Actions` (ou manter em `Myth.Interfaces` com re-exports via `using`).
2. Adicionar tabela de namespaces no skill `myth-flow-actions` como referência rápida.
3. Considerar um arquivo de convenience `using Myth.Cqrs` que importe todos os tipos necessários de uma vez.

---

## ValidationException em Myth.Exceptions, não Myth.Guard.Exceptions

**Library:** Myth.Guard / Myth.Exceptions
**Discovered:** 2026-05-20
**Status:** ✅ RESOLVED — Documentado na seção "Namespace quick reference" do `AGENT_MIGRATION_GUIDE.md` com aviso explícito: `using Myth.Exceptions; // ValidationException ← NOT Myth.Guard.Exceptions`. Todos os exemplos do guide usam o namespace correto.
**Context:** Lançando ValidationException em handler para retornar 402, import errado gerou erro de compilação confuso.

**Current behavior:**
`ValidationException` está no namespace `Myth.Exceptions`, mas o skill `myth-guard` não menciona isso. A expectativa natural é que esteja em `Myth.Guard` ou `Myth.Guard.Exceptions`.

**Problem / Gap:**
Desenvolvedores que trabalham com validação via `myth-guard` assumem que `ValidationException` está em `Myth.Guard.Exceptions`. O namespace real (`Myth.Exceptions`) não aparece em nenhum exemplo do skill. O erro de compilação diz "type not found" sem sugerir o namespace correto.

**Suggested improvement:**
1. Adicionar `using Myth.Exceptions;` em todos os exemplos de código do skill `myth-guard` que lançam `ValidationException`.
2. Ou mover `ValidationException` para `Myth.Guard.Exceptions` (breaking change, mas mais intuitivo).
3. Documentar a tabela completa de namespaces de exceção no skill.

---

## FluentRuleBuilder não tem MaxLength e NotDefault não está documentado

**Library:** Myth.Guard
**Discovered:** 2026-05-21
**Status:** ✅ RESOLVED — `MaxLength(int)` existe como alias de `MaximumLength(int)` em `FluentRuleBuilderExtensions`. `NotDefault()` implementado no `FluentRuleBuilder` base com suporte a nullable.
**Context:** Validando campos string com tamanho máximo e campos Guid obrigatórios em commands.

**Current behavior:**
O `FluentRuleBuilder` (usado em `Validate()` via `ValidationBuilder<T>`) não possui método `MaxLength()`. Para validar Guid obrigatório, o método correto é `NotDefault()`, não `NotEmpty()` — mas isso não está documentado no skill.

**Problem / Gap:**
- Chamar `NotEmpty()` em um `Guid` compila mas não valida corretamente (Guid.Empty passa).
- Para `MaxLength`, o desenvolvedor precisa usar `.Must(v => v.Length <= 200)` manualmente, perdendo a mensagem de erro padronizada.

```csharp
// WRONG — compila mas não valida Guid.Empty
RuleFor(x => x.OrganizationId).NotEmpty();

// CORRECT — rejeita Guid.Empty corretamente
RuleFor(x => x.OrganizationId).NotDefault();

// MaxLength — workaround necessário, sem método nativo
RuleFor(x => x.Name).Must(v => v?.Length <= 100).WithMessage("Max 100 chars");
```

**Suggested improvement:**
1. Adicionar `MaxLength(int max)` no `FluentRuleBuilder` com mensagem de erro padrão.
2. Documentar `NotDefault()` no skill `myth-guard` com exemplo explícito para `Guid` e outros value types.
3. Adicionar seção "Common Validation Pitfalls" no skill com `NotDefault` vs `NotEmpty`.

---

## AGENT_MIGRATION_GUIDE — Seção 6: exemplo de teste insuficiente para pipelines com múltiplos Transforms

**Library:** Myth.Flow / AGENT_MIGRATION_GUIDE.md
**Discovered:** 2026-06-04
**Status:** ✅ RESOLVED 2026-06-04 — Seção 6 do `AGENT_MIGRATION_GUIDE.md` atualizada para usar `GetBaseException()` em vez de `.InnerException`, com nota explicativa sobre aninhamento proporcional de `PipelineException` por step.
**Context:** Aplicando a migração descrita na seção 6 do `AGENT_MIGRATION_GUIDE.md` ao projeto `myth-template`. Os testes de `PostAsync` (que usa `.Process<T,R>()` + `.Transform()` + `.Publish()`) continuavam falhando mesmo após aplicar o padrão de `InnerException` descrito no guide.

**Current behavior (guide):**
O guide instrui usar:
```csharp
// ✅ NEW test assertion — PipelineException with inner exception
var thrown = await act.Should().ThrowAsync<PipelineException>();
thrown.Which.InnerException.Should().BeOfType<ArgumentException>();
```

Isso funciona quando há **um único** Transform no pipeline. Mas quando existem múltiplos steps que usam Transform internamente (ex: `.Process<T,R>()` + `.Transform(result => ...)` + `.Publish()`), a exceção é envolvida várias vezes:

```
PipelineException (from .Publish())
  → PipelineException (from .Transform())
    → PipelineException (from .Process<T,R>())
      → ValidationException (original)
```

Nesse caso, `.Which.InnerException` é outro `PipelineException`, não a `ValidationException`.

**Problem / Gap:**
O exemplo do guide assume aninhamento único, mas na prática pipelines com múltiplos steps de Transform produzem aninhamento profundo. O padrão recomendado com `.InnerException` não funciona de forma genérica.

**Discovered fix:**
Usar `GetBaseException()` que percorre toda a cadeia de InnerException até encontrar a exceção raiz — funciona independentemente da profundidade:

```csharp
// ✅ Robust — works for any nesting depth
var thrown = await act.Should().ThrowAsync<PipelineException>();
var response = thrown.Which.GetBaseException().Should().BeOfType<ValidationException>().Which;
response.Message.Should().NotBeEmpty();
response.ValidationResult.StatusCode.Should().Be(HttpStatusCode.BadRequest);
```

**Suggested improvement:**
1. Atualizar a seção 6 do `AGENT_MIGRATION_GUIDE.md` para usar `GetBaseException()` em vez de `.InnerException` diretamente.
2. Adicionar nota explicando que múltiplos Transform steps causam aninhamento proporcional de PipelineExceptions.
3. Mencionar que `.Process<T,R>()`, `.Query<T,R>()`, `.Transform()`, e `.Publish()` TODOS usam Transform internamente — não apenas o `.Transform()` explícito.

---

## Padrão de validação de entidade em Validate() não tem primitivo Myth

**Library:** Myth.Guard
**Discovered:** 2026-05-21
**Context:** Implementando validação de existência de entidade, verificação de créditos e limites de plano dentro do método `Validate()` de commands.

**Current behavior:**
Myth.Guard provê `ValidationBuilder<T>` para regras síncronas simples. Para validações assíncronas que consultam o banco (entity existence, credit balance, plan limits, permissions), não existe nenhum primitivo — o projeto precisou criar:
- `EntityValidationService` — serviço que encapsula queries de existência
- `RulesExtensions` — extension methods com nomes semânticos (`WorkspaceMustExistAsync()`, `UserHasSufficientAiCreditsAsync()`, etc.)
- Acesso via reflection para ler propriedades do command genérico (`OrganizationId`, `ProjectId`, `Count`) em regras genéricas compartilhadas

**Problem / Gap:**
Todo projeto que usa Myth.Guard para validação síncrona vai precisar reinventar esse padrão para validações assíncronas. Não existe guia, interface ou base class no ecosystem para isso. O uso de reflection para acessar propriedades do command em regras genéricas é frágil e quebra silenciosamente se a propriedade for renomeada.

**Suggested improvement:**
1. Adicionar suporte a regras assíncronas no `ValidationBuilder<T>`: `.RuleForAsync(x => x.WorkspaceId, async id => await repo.ExistsAsync(id))`.
2. Criar interface `IAsyncValidationRule<TCommand>` que handlers de validação possam implementar e registrar no DI, separando validação de negócio da definição do command.
3. Documentar o padrão `EntityValidationService` + `RulesExtensions` como arquitetura recomendada enquanto o suporte nativo não existe.
