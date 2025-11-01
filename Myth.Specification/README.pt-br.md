# Myth.Specification

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Specification?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Specification/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Specification?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Specification/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

É uma biblioteca .NET para construções de _queries_ de forma bem legível, mantendo a frente a regra de negócio.

# ⭐ Funcionalidades
- Fácil legibilidade
- Fácil escrita
- Regras de negócios a frente
- Uso simplificado

# 🔮 Utilização
Para utilizar, deve-se seguir o seguinte padrão:

```csharp
var spec = SpecBuilder<Entity>
	.Create()
	.And(...)
	.Or(...)
	.Not()
	...
```

A principal ideia é que cada pedacinho do seu filtro seja construido pensando na regra de negócio.

Suponha que eu tenha uma tabela `Pessoa` e precise filtrar as pessoas com identidade de gênero feminina, que sejam da geração Z e que morem em uma cidade X.

Para isso eu deveria criar uma classe estática com a criação de cada parte desse filtro, da seguinter forma:

```csharp
public static class PersonSpecifications {
	public static ISpec<Person> IsGenerationX(this ISpec<Person> spec) {
		return spec.And(person => person.Birthdate.Year >= 2000);
	}

	public static ISpec<Person> IsIdentifiedAsFemale(this ISpec<Person> spec) {
		return spec.And(person => person.Gender == "female");
	}

	public static ISpec<Person> LivesOnCity(this ISpec<Person> spec, string city) {
		return spec.And(person => person.Address.City == city);
	}
}
```

E então na contrução do meu filtro, pesquisar:

```csharp
public class PersonService {
	private IPersonRepository _personRepository;

	...

	public IEnumerable<Person> GetFemalePersonsOfGenerationXOfCityAsync( string city, CancellationToken cancellationToken ) {		
		var spec = SpecBuilder<Person>
			.Create()
			.IsGenerationX()
			.IsIdentifiedAsFemale()
			.LivesOnCity(city);

		var result = _repository.SearchAsync(spec, cancellationToken);

		return result;
	}
}
```

Assim fica muito claro que, eu procuro por pessoas de identidade de gênero feminina, da geração X e que morem na cidade que eu procuro.

## 🪄 Especificações

As especificações podem ser de três tipos e trabalhadas individualmente ou em grupos.

Aplicando todos os tipos pode ser feito da seguinte forma:

```
var enumerable = Enumerable.Empty<Person>();

var spec = SpecBuilder<Person>
	.Create()
	.And(x => x.PersonId != null)
	.Distinct()
	.Order(x => x.Name)
	.Order(x => x.Address)
	.Skip(10)
	.Take(10);

var result = enumerable
	.Specify(spec)
	.ToList();
```` 

## 🔽 Filtros

Filtros podem ser aplicados diretamente da seguinte forma:

```
var enumerable = Enumerable.Empty<Person>();

var spec = SpecBuilder<Person>
	.Create()
	.And(x => x.PersonId != null);

var result = enumerable
	.Filter(spec)
	.ToList();
```

Os seguintes filtros estão disponíveis:

- `And`
- `AndIf`
- `Or`
- `OrIf`
- `Not`

## ⬇️ Ordenação

Ordenação pode ser aplicada diretamente da seguinte forma:

```
var enumerable = Enumerable.Empty<Person>();

var spec = SpecBuilder<Person>
	.Create()
	.Order(x => x.Name);

var result = enumerable
	.Sort(spec)
	.ToList();
```

As seguintes ordenações estão disponíveis:

- `Order`
- `OrderDescending`
- 
- ## 📃 Paginação e pós processamento

Paginações e pós processamento podem ser aplicados diretamente da seguinte forma:

```
var enumerable = Enumerable.Empty<Person>();

var spec = SpecBuilder<Person>
	.Create()
	.DistinctBy(x => x.Name)
	.Skip(10)
	.Take(10);

var result = enumerable
	.Paginate(spec)
	.ToList();
```

As seguintes funções estão disponíveis:

- `Skip`
- `Take`
- `DistinctBy`
- `WithPagination` - Método conveniente usando o value object Pagination

### WithPagination

Você também pode usar o método conveniente `WithPagination` que aceita um value object `Pagination` do `Myth.Commons`:

```csharp
using Myth.ValueObjects;

var enumerable = Enumerable.Empty<Person>();
var pagination = new Pagination(2, 10); // Página 2, 10 itens por página

var spec = SpecBuilder<Person>
	.Create()
	.And(x => x.PersonId != null)
	.Order(x => x.Name)
	.WithPagination(pagination);

var result = enumerable
	.Specify(spec)
	.ToList();
```

Isso substitui o cálculo manual:
```csharp
// Em vez de:
.Skip((pagination.PageNumber - 1) * pagination.PageSize)
.Take(pagination.PageSize)

// Use:
.WithPagination(pagination)
```

**Casos especiais:**
- `Pagination.All` (-1, -1): Retorna todos os itens sem paginação
- `PageNumber <= 0`: Tratado como página 1
- `PageSize <= 0`: Retorna todos os itens sem paginação

**Combinando com Skip/Take:**
Você ainda pode combinar `WithPagination` com operações adicionais de `Skip` e `Take`:

```csharp
var spec = SpecBuilder<Person>
	.Create()
	.WithPagination(pagination)  // Skip 10, Take 10 (página 2)
	.Skip(5)                     // Pula mais 5
	.Take(3);                    // Limita a 3 itens no total
```