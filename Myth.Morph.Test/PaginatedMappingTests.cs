using Bogus;
using FluentAssertions;
using Myth.Extensions;
using Myth.Interfaces.Results;
using Myth.Models.Results;
using Myth.Morph.Test.Models.Dtos;
using Myth.Morph.Test.Models.Entities;

namespace Myth.Morph.Test;

public class PaginatedMappingTests : BaseTestFixture {
	private readonly Faker _faker = new Faker( );

	[Fact]
	public void To_PaginatedDto_Should_MapAllScalarProperties( ) {
		var users = Enumerable.Range( 1, 3 ).Select( i => new User {
			Id = i,
			FirstName = _faker.Name.FirstName( ),
			LastName = _faker.Name.LastName( ),
			Email = _faker.Internet.Email( ),
			BirthDate = _faker.Date.Past( 30 ),
			CountryCode = _faker.Address.CountryCode( ),
			IsEmailVerified = _faker.Random.Bool( ),
			LastLoginAt = _faker.Date.Recent( )
		} ).ToList( );

		var source = new Paginated<User>( pageNumber: 2, pageSize: 3, totalItems: 10, totalPages: 4, items: users );

		var result = source.To<IPaginated<SimpleUserDto>>( );

		result.Should( ).NotBeNull( );
		result.PageNumber.Should( ).Be( 2 );
		result.PageSize.Should( ).Be( 3 );
		result.TotalItems.Should( ).Be( 10 );
		result.TotalPages.Should( ).Be( 4 );
	}

	[Fact]
	public void To_PaginatedDto_Should_MapItems( ) {
		var users = Enumerable.Range( 1, 3 ).Select( i => new User {
			Id = i,
			FirstName = _faker.Name.FirstName( ),
			LastName = _faker.Name.LastName( ),
			Email = _faker.Internet.Email( ),
			BirthDate = _faker.Date.Past( 30 ),
			CountryCode = _faker.Address.CountryCode( ),
			IsEmailVerified = _faker.Random.Bool( ),
			LastLoginAt = _faker.Date.Recent( )
		} ).ToList( );

		var source = new Paginated<User>( pageNumber: 1, pageSize: 3, totalItems: 3, totalPages: 1, items: users );

		var result = source.To<IPaginated<SimpleUserDto>>( );

		result.Items.Should( ).NotBeNull( );
		result.Items.Should( ).HaveCount( 3 );

		var resultList = result.Items.ToList( );
		for ( var i = 0; i < users.Count; i++ ) {
			resultList[ i ].FirstName.Should( ).Be( users[ i ].FirstName );
			resultList[ i ].LastName.Should( ).Be( users[ i ].LastName );
			resultList[ i ].Email.Should( ).Be( users[ i ].Email );
		}
	}

	[Fact]
	public void To_PaginatedDto_WithEmptyItems_Should_ReturnEmptyCollection( ) {
		var source = new Paginated<User>( pageNumber: 1, pageSize: 10, totalItems: 0, totalPages: 0, items: [ ] );

		var result = source.To<IPaginated<SimpleUserDto>>( );

		result.Should( ).NotBeNull( );
		result.TotalItems.Should( ).Be( 0 );
		result.Items.Should( ).NotBeNull( );
		result.Items.Should( ).BeEmpty( );
	}

	[Fact]
	public void To_PaginatedDto_From_IPaginatedSource_Should_MapCorrectly( ) {
		var users = new List<User> {
			new( ) { Id = 1, FirstName = "Alice", Email = "alice@example.com" }
		};

		IPaginated<User> source = new Paginated<User>( pageNumber: 1, pageSize: 1, totalItems: 1, totalPages: 1, items: users );

		var result = source.To<IPaginated<SimpleUserDto>>( );

		result.Should( ).NotBeNull( );
		result.PageNumber.Should( ).Be( 1 );
		result.Items.Should( ).HaveCount( 1 );
		result.Items.First( ).FirstName.Should( ).Be( "Alice" );
	}
}
