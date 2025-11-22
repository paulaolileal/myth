using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Myth.Repository.Test.Contexts;
using Myth.Repository.Test.Interfaces;
using Myth.Repository.Test.Mocks;
using Person = Myth.Repository.Test.Models.Person;

namespace Myth.Repository.Test;

[Collection( "Sequential" )]
public class ReadExpressionsRepositoryTests( DatabaseMock databaseMock ) : IClassFixture<DatabaseMock> {
	private readonly IRepositoryTest _repository = databaseMock.Repository;
	private readonly ContextTest _context = databaseMock.Context;
	private readonly Faker _faker = databaseMock.Faker;
	private readonly Func<Task<Person>> _mockAsync = databaseMock.MockAsync;
	private readonly Func<int, Task<IEnumerable<Person>>> _mockManyAsync = databaseMock.MockAsync;

	[Fact]
	public async Task Search_should_return_list_of_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );

		// Act
		var result = await _repository.SearchAsync( x => x.PersonId > 0 );

		// Assert
		result.Should( ).NotBeNull( );
		result.Should( ).Contain( person );
	}

	[Fact]
	public async Task Search_paginated_should_return_list_of_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );

		// Act
		var result = await _repository.SearchPaginatedAsync( x => x.PersonId > 0 );

		// Assert
		result.Should( ).NotBeNull( );
		result.Items.Should( ).HaveCount( result.TotalItems );
		result.Items.Should( ).Contain( person );
	}

	[Fact]
	public async Task Search_paginated_with_parameters_should_return_list_of_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );

		// Act
		var result = await _repository.SearchPaginatedAsync( x => x.PersonId > 0, 10, 20 );

		// Assert
		result.Should( ).NotBeNull( );
		result.PageNumber.Should( ).Be( 3 );
		result.PageSize.Should( ).Be( 10 );
		result.Items.Should( ).HaveCount( 10 );
	}

	[Fact]
	public async Task Where_should_return_list_of_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );

		// Act
		var result = await _repository
			.Where( x => x.PersonId > 0 )
			.ToListAsync( );

		// Assert
		result.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Count_should_return_list_of_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );

		// Act
		var result = await _repository.CountAsync( x => x.PersonId > 0 );

		// Assert
		result.Should( ).BeGreaterThan( 0 );
	}

	[Fact]
	public async Task Any_should_return_list_of_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );

		// Act
		var result = await _repository.AnyAsync( x => x.PersonId > 0 );

		// Assert
		result.Should( ).BeTrue( );
	}

	[Fact]
	public async Task All_should_return_list_of_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );

		// Act
		var result = await _repository.AllAsync( x => x.PersonId > 0 );

		// Assert
		result.Should( ).BeTrue( );
	}

	[Fact]
	public async Task First_should_return_a_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );

		// Act
		var result = await _repository.FirstOrDefaultAsync( x => x.PersonId > 0 );

		// Assert
		result.Should( ).NotBeNull( );
	}

	[Fact]
	public async Task Last_should_return_a_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );

		// Act
		var result = await _repository.LastOrDefaultAsync( x => x.PersonId > 0 );

		// Assert
		result.Should( ).NotBeNull( );
	}

	[Fact]
	public async Task To_list_should_return_a_all_items( ) {
		// Arrange
		_ = await _mockManyAsync( 100 );

		// Act
		var result = await _repository.ToListAsync( );

		// Assert
		result.Should( ).NotBeNull( );
	}
}
