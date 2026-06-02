using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Myth.Repository.Test.Contexts;
using Myth.Repository.Test.Interfaces;
using Myth.Repository.Test.Mocks;
using Myth.Specifications;
using Person = Myth.Repository.Test.Models.Person;

namespace Myth.Repository.Test;

[Collection( "Sequential" )]
public class ReadSpecificationsRepositoryTests( DatabaseMock databaseMock ) : IClassFixture<DatabaseMock> {
	private readonly IRepositoryTest _repository = databaseMock.Repository;
	private readonly ContextTest _context = databaseMock.Context;
	private readonly Faker _faker = databaseMock.Faker;
	private readonly Func<Task<Person>> _mockAsync = databaseMock.MockAsync;
	private readonly Func<int, Task<IEnumerable<Person>>> _mockManyAsync = databaseMock.MockAsync;

	[Fact]
	public async Task Search_should_return_list_of_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );
		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId > 0 );

		// Act
		var result = await _repository.SearchAsync( spec );

		// Assert
		result.Should( ).NotBeNull( );
		result.Should( ).Contain( person );
	}

	[Fact]
	public async Task Search_paginated_should_return_list_of_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );
		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId > 0 );

		// Act
		var result = await _repository.SearchPaginatedAsync( spec );

		// Assert
		result.Should( ).NotBeNull( );
		result.Items.Should( ).HaveCount( result.TotalItems );
		result.Items.Should( ).Contain( person );
	}

	[Fact]
	public async Task Search_paginated_with_parameters_should_return_list_of_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );
		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId > 0 )
			.Skip( 5 )
			.Take( 10 );

		// Act
		var result = await _repository.SearchPaginatedAsync( spec );

		// Assert
		result.Should( ).NotBeNull( );
		result.PageNumber.Should( ).Be( 1 ); // (5/10)+1 = 1
		result.PageSize.Should( ).Be( 10 );
		result.Items.Should( ).HaveCount( 10 );
	}

	[Fact]
	public async Task Where_should_return_list_of_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );
		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId > 0 );

		// Act
		var result = await _repository
			.Where( spec )
			.ToListAsync( );

		// Assert
		result.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Count_should_return_list_of_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );
		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId > 0 );

		// Act
		var result = await _repository.CountAsync( spec );

		// Assert
		result.Should( ).BeGreaterThan( 0 );
	}

	[Fact]
	public async Task Any_should_return_list_of_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );
		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId > 0 );

		// Act
		var result = await _repository.AnyAsync( spec );

		// Assert
		result.Should( ).BeTrue( );
	}

	[Fact]
	public async Task All_should_return_list_of_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );
		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId > 0 );

		// Act
		var result = await _repository.AllAsync( spec );

		// Assert
		result.Should( ).BeTrue( );
	}

	[Fact]
	public async Task First_should_return_a_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );
		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId > 0 );

		// Act
		var result = await _repository.FirstOrDefaultAsync( spec );

		// Assert
		result.Should( ).NotBeNull( );
	}

	[Fact]
	public async Task Last_should_return_a_items( ) {
		// Arrange
		var person = await _mockManyAsync( 100 );
		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId > 0 );

		// Act
		var result = await _repository.LastOrDefaultAsync( spec );

		// Assert
		result.Should( ).NotBeNull( );
	}

	[Fact]
	public async Task SearchAsync_should_return_IReadOnlyList_with_correct_Count( ) {
		// Arrange
		await _mockManyAsync( 10 );
		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId > 0 );

		// Act
		var result = await _repository.SearchAsync( spec );

		// Assert — .Count (property of IReadOnlyList) must equal actual element count
		result.Should( ).NotBeNull( );
		result.Count.Should( ).BeGreaterThan( 0 );
		result.Count.Should( ).Be( result.Count( ) ); // property and LINQ extension must agree
	}

	[Fact]
	public async Task SearchAsNoTrackingAsync_should_return_entities_not_tracked( ) {
		// Arrange
		await _mockManyAsync( 5 );
		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId > 0 );

		// Act
		var result = await _repository.SearchAsNoTrackingAsync( spec );

		// Assert
		result.Should( ).NotBeNull( );
		result.Count.Should( ).BeGreaterThan( 0 );
		result.Should( ).AllSatisfy( p => _context.Entry( p ).State.Should( ).Be( Microsoft.EntityFrameworkCore.EntityState.Detached ) );
	}

	[Fact]
	public async Task SearchAsNoTrackingAsync_should_return_same_data_as_SearchAsync( ) {
		// Arrange
		await _mockManyAsync( 10 );
		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId > 0 );

		// Act
		var tracked = await _repository.SearchAsync( spec );
		var noTracking = await _repository.SearchAsNoTrackingAsync( spec );

		// Assert
		noTracking.Count.Should( ).Be( tracked.Count );
		noTracking.Select( p => p.PersonId ).Should( ).BeEquivalentTo( tracked.Select( p => p.PersonId ) );
	}
}
