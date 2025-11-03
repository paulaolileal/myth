using FluentAssertions;
using Myth.Extensions;
using Myth.Specification.Test.Models;
using Myth.Specifications;
using Myth.ValueObjects;

namespace Myth.Specification.Test;

public class WithPaginationTests {

	[Fact]
	public void WithPagination_should_apply_skip_and_take_correctly( ) {
		// Arrange
		var entities = Person.Mock( 100 ).ToList( );
		var pagination = new Pagination( 3, 10 ); // Page 3, 10 items per page

		var spec = SpecBuilder<Person>
			.Create( )
			.WithPagination( pagination );

		// Act
		var result = entities.Specify( spec ).ToList( );

		// Assert
		result.Should( ).HaveCount( 10 );
		spec.ItemsSkiped.Should( ).Be( 20 ); // (3-1) * 10 = 20
		spec.ItemsTaked.Should( ).Be( 10 );
	}

	[Fact]
	public void WithPagination_should_handle_first_page_correctly( ) {
		// Arrange
		var entities = Person.Mock( 100 ).ToList( );
		var pagination = new Pagination( 1, 5 ); // First page, 5 items per page

		var spec = SpecBuilder<Person>
			.Create( )
			.WithPagination( pagination );

		// Act
		var result = entities.Specify( spec ).ToList( );

		// Assert
		result.Should( ).HaveCount( 5 );
		spec.ItemsSkiped.Should( ).Be( 0 ); // (1-1) * 5 = 0
		spec.ItemsTaked.Should( ).Be( 5 );
	}

	[Fact]
	public void WithPagination_should_handle_last_page_with_fewer_items( ) {
		// Arrange
		var entities = Person.Mock( 25 ).ToList( );
		var pagination = new Pagination( 3, 10 ); // Page 3, 10 items per page (only 5 items left)

		var spec = SpecBuilder<Person>
			.Create( )
			.WithPagination( pagination );

		// Act
		var result = entities.Specify( spec ).ToList( );

		// Assert
		result.Should( ).HaveCount( 5 ); // Only 5 items remaining on last page
		spec.ItemsSkiped.Should( ).Be( 20 ); // (3-1) * 10 = 20
		spec.ItemsTaked.Should( ).Be( 10 );
	}

	[Fact]
	public void WithPagination_should_handle_pagination_all_correctly( ) {
		// Arrange
		var entities = Person.Mock( 100 ).ToList( );
		var pagination = Pagination.All; // -1, -1 should return all items

		var spec = SpecBuilder<Person>
			.Create( )
			.WithPagination( pagination );

		// Act
		var result = entities.Specify( spec ).ToList( );

		// Assert
		result.Should( ).HaveCount( 100 ); // All items returned
		spec.ItemsSkiped.Should( ).Be( 0 ); // No items skipped
		spec.ItemsTaked.Should( ).Be( 0 ); // No limit applied
	}

	[Fact]
	public void WithPagination_should_handle_zero_page_number_as_first_page( ) {
		// Arrange
		var entities = Person.Mock( 100 ).ToList( );
		var pagination = new Pagination( 0, 10 ); // Invalid page number (should be treated as 1)

		var spec = SpecBuilder<Person>
			.Create( )
			.WithPagination( pagination );

		// Act
		var result = entities.Specify( spec ).ToList( );

		// Assert
		result.Should( ).HaveCount( 10 );
		spec.ItemsSkiped.Should( ).Be( 0 ); // Treated as page 1: (1-1) * 10 = 0
		spec.ItemsTaked.Should( ).Be( 10 );
	}

	[Fact]
	public void WithPagination_should_handle_negative_page_number_as_first_page( ) {
		// Arrange
		var entities = Person.Mock( 100 ).ToList( );
		var pagination = new Pagination( -5, 10 ); // Invalid page number (should be treated as 1)

		var spec = SpecBuilder<Person>
			.Create( )
			.WithPagination( pagination );

		// Act
		var result = entities.Specify( spec ).ToList( );

		// Assert
		result.Should( ).HaveCount( 10 );
		spec.ItemsSkiped.Should( ).Be( 0 ); // Treated as page 1: (1-1) * 10 = 0
		spec.ItemsTaked.Should( ).Be( 10 );
	}

	[Fact]
	public void WithPagination_should_handle_zero_page_size_by_returning_all_items( ) {
		// Arrange
		var entities = Person.Mock( 50 ).ToList( );
		var pagination = new Pagination( 2, 0 ); // Zero page size should return all items

		var spec = SpecBuilder<Person>
			.Create( )
			.WithPagination( pagination );

		// Act
		var result = entities.Specify( spec ).ToList( );

		// Assert
		result.Should( ).HaveCount( 50 ); // All items returned
		spec.ItemsSkiped.Should( ).Be( 0 ); // No pagination applied
		spec.ItemsTaked.Should( ).Be( 0 ); // No pagination applied
	}

	[Fact]
	public void WithPagination_should_handle_negative_page_size_by_returning_all_items( ) {
		// Arrange
		var entities = Person.Mock( 50 ).ToList( );
		var pagination = new Pagination( 2, -10 ); // Negative page size should return all items

		var spec = SpecBuilder<Person>
			.Create( )
			.WithPagination( pagination );

		// Act
		var result = entities.Specify( spec ).ToList( );

		// Assert
		result.Should( ).HaveCount( 50 ); // All items returned
		spec.ItemsSkiped.Should( ).Be( 0 ); // No pagination applied
		spec.ItemsTaked.Should( ).Be( 0 ); // No pagination applied
	}

	[Fact]
	public void WithPagination_should_work_with_filters_and_sorting( ) {
		// Arrange
		var entities = Person.Mock( 100 ).ToList( );
		var pagination = new Pagination( 2, 5 ); // Page 2, 5 items per page

		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId != Guid.Empty )
			.Order( x => x.Name )
			.WithPagination( pagination );

		// Act
		var result = entities.Specify( spec ).ToList( );

		// Assert
		result.Should( ).HaveCount( 5 );
		spec.ItemsSkiped.Should( ).Be( 5 ); // (2-1) * 5 = 5
		spec.ItemsTaked.Should( ).Be( 5 );
	}

	[Fact]
	public void WithPagination_should_combine_with_existing_skip_and_take( ) {
		// Arrange
		var entities = Person.Mock( 100 ).ToList( );
		var pagination = new Pagination( 2, 10 ); // Page 2, 10 items per page (skips 10, takes 10)

		var spec = SpecBuilder<Person>
			.Create( )
			.WithPagination( pagination )
			.Skip( 5 ) // Additional skip of 5 items
			.Take( 3 ); // Limit to 3 items

		// Act
		var result = entities.Specify( spec ).ToList( );

		// Assert
		result.Should( ).HaveCount( 3 ); // Limited by Take(3)
		spec.ItemsSkiped.Should( ).Be( 15 ); // 10 (pagination) + 5 (additional skip) = 15
		spec.ItemsTaked.Should( ).Be( 13 ); // 10 (pagination) + 3 (additional take) = 13
	}

	[Fact]
	public void WithPagination_should_use_default_pagination_correctly( ) {
		// Arrange
		var entities = Person.Mock( 100 ).ToList( );
		var pagination = Pagination.Default; // Page 1, 10 items per page

		var spec = SpecBuilder<Person>
			.Create( )
			.WithPagination( pagination );

		// Act
		var result = entities.Specify( spec ).ToList( );

		// Assert
		result.Should( ).HaveCount( 10 );
		spec.ItemsSkiped.Should( ).Be( 0 ); // (1-1) * 10 = 0
		spec.ItemsTaked.Should( ).Be( 10 );
	}

	[Fact]
	public void WithPagination_should_handle_empty_collection( ) {
		// Arrange
		var entities = new List<Person>( );
		var pagination = new Pagination( 2, 10 );

		var spec = SpecBuilder<Person>
			.Create( )
			.WithPagination( pagination );

		// Act
		var result = entities.Specify( spec ).ToList( );

		// Assert
		result.Should( ).BeEmpty( );
		spec.ItemsSkiped.Should( ).Be( 10 ); // (2-1) * 10 = 10
		spec.ItemsTaked.Should( ).Be( 10 );
	}
}