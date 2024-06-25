# Myth.Specification

![NuGet Version](https://img.shields.io/nuget/v/Myth.Specification?style=for-the-badge&logo=nuget) ![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Specification?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

It is a .NET library for constructing _queries_ in a very readable way, keeping the business rules in mind.

# ⭐ Features
- Easy readability
- Easy writing
- Business rules ahead
- Simplified use

# 🔮 Usage
To use, the following pattern must be followed:

```csharp
var spec = SpecBuilder<Entity>
	.Create()
	.And(...)
	.Or(...)
	.Not()
  ...
```

The main idea is that every bit of your filter is built with the business rule in mind.

Suppose I have a `Person` table and I need to filter people with a female gender identity, who are from generation Z and who live in city X.

To do this, I should create a static class with the creation of each part of this filter, as follows:

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

And then when building my filter, search:

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

So it's very clear that I'm looking for people with a female gender identity, from generation X and who live in the city I'm looking for.

## 🪄 Specifications

Specifications can be of three types and worked individually or in groups.

Applying all types can be done as follows:

```csharp
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

## 🔽 Filters

Filters can be applied directly as follows:

```csharp
var enumerable = Enumerable.Empty<Person>();

var spec = SpecBuilder<Person>
	.Create()
	.And(x => x.PersonId != null);

var result = enumerable
	.Filter(spec)
	.ToList();
```

The following filters are available:

- `And`
- `AndIf`
- `Or`
- `OrIf`
- `Not`

## ⬇️ Ordering

Ordering can be applied directly as follows:

```csharp
var enumerable = Enumerable.Empty<Person>();

var spec = SpecBuilder<Person>
	.Create()
	.Order(x => x.Name);

var result = enumerable
	.Sort(spec)
	.ToList();
```

The following orderings are available:

- `Order`
- `OrderDescending`
-
- ## 📃 Pagination and post processing

Paginations and post processing can be applied directly as follows:

```csharp
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

The following functions are available:

- `Skip`
- `Take`
- `DistinctBy`