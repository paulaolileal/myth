using Myth.Specifications;
using Myth.ValueObjects;
using Myth.Extensions;

// Exemplo de uso da nova funcionalidade WithPagination

// Exemplo 1: Uso básico
var pessoas = GetPessoas(); // Lista de pessoas
var paginacao = new Pagination(2, 10); // Página 2, 10 itens por página

var spec = SpecBuilder<Person>
    .Create()
    .And(p => p.IsActive)
    .Order(p => p.Name)
    .WithPagination(paginacao);

var resultado = pessoas.Specify(spec).ToList();

// Exemplo 2: Usando Pagination.All
var specTodos = SpecBuilder<Person>
    .Create()
    .And(p => p.IsActive)
    .WithPagination(Pagination.All);

var todosPessoas = pessoas.Specify(specTodos).ToList();

// Exemplo 3: Combinando com Skip/Take adicionais
var specCombo = SpecBuilder<Person>
    .Create()
    .WithPagination(paginacao)  // Pula 10, pega 10
    .Skip(5)                    // Pula mais 5
    .Take(3);                   // Limita a 3 itens

var resultadoCombo = pessoas.Specify(specCombo).ToList();

// Exemplo 4: Em vez do cálculo manual
// ANTES:
var specAntigo = SpecBuilder<Person>
    .Create()
    .And(p => p.IsActive)
    .Skip((paginacao.PageNumber - 1) * paginacao.PageSize)
    .Take(paginacao.PageSize);

// AGORA:
var specNovo = SpecBuilder<Person>
    .Create()
    .And(p => p.IsActive)
    .WithPagination(paginacao);