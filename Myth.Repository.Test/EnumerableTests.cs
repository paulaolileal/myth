using FluentAssertions;
using Myth.Extensions;
using Myth.Repository.Test.Models;
using Myth.ValueObjects;

namespace Myth.Repository.Test;

public class EnumerableTests {

	[Fact]
	public void Paginated_should_paginate_a_list_of_items( ) {
		// Arrange
		var persons = Person.Mock( 100 );
		var pagination = Pagination.Default;

		// Act
		var result = persons.AsPaginated( pagination );

		// Assert
		result.Should( ).NotBeNull( );
		result.TotalItems.Should( ).Be( 100 );
		result.PageSize.Should( ).Be( 10 );
		result.PageNumber.Should( ).Be( 1 );
		result.Items.Should( ).HaveCount( pagination.PageSize );
	}

	[Fact]
	public void Paginated_should_paginate_a_list_of_items_and_return_all( ) {
		// Arrange
		var persons = Person.Mock( 100 );
		var pagination = Pagination.All;

		// Act
		var result = persons.AsPaginated( pagination );

		// Assert
		result.Should( ).NotBeNull( );
		result.TotalItems.Should( ).Be( 100 );
		result.PageSize.Should( ).Be( 100 );
		result.PageNumber.Should( ).Be( 1 );
		result.Items.Should( ).HaveCount( 100 );
	}
}