using FluentAssertions;
using Myth.Extensions;
using Myth.Specification.Test.Models;
using Myth.Specifications;

namespace Myth.Specification.Test;

public class SpecificationTests {

	[Fact]
	public void And_should_return_filtered_items( ) {
		// Arrange
		var entities = Person.Mock( 100 );

		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId != Guid.Empty );

		// Act
		var result = entities
			.Specify( spec )
			.ToList( );

		// Assert
		result.Should( ).NotBeEmpty( );
	}

	[Fact]
	public void And_if_should_return_filtered_items( ) {
		// Arrange
		var entities = Person.Mock( 100 );

		var spec = SpecBuilder<Person>
			.Create( )
			.AndIf( true, x => x.PersonId != Guid.Empty );

		// Act
		var result = entities
			.Specify( spec )
			.ToList( );

		// Assert
		result.Should( ).NotBeEmpty( );
	}

	[Fact]
	public void Or_should_return_filtered_items( ) {
		// Arrage
		var entities = Person.Mock( 100 );

		var spec = SpecBuilder<Person>
			.Create( )
			.Or( x => x.PersonId != Guid.Empty );

		// Act
		var result = entities
			.Specify( spec )
			.ToList( );

		// Assert
		result.Should( ).NotBeEmpty( );
	}

	[Fact]
	public void Or_if_should_return_filtered_items( ) {
		// Arrange
		var entities = Person.Mock( 100 );

		var spec = SpecBuilder<Person>
			.Create( )
			.OrIf( true, x => x.PersonId != Guid.Empty );

		// Act
		var result = entities
			.Specify( spec )
			.ToList( );

		// Assert
		result.Should( ).NotBeEmpty( );
	}

	[Fact]
	public void Distinct_should_return_filtered_items( ) {
		// Arrange
		var entities = Person.Mock( 100 );

		var spec = SpecBuilder<Person>
			.Create( )
			.DistinctBy( x => x.PersonId );

		// Act
		var result = entities
			.AsQueryable( )
			.Paginate( spec )
			.ToList( );

		// Assert
		result.Should( ).NotBeEmpty( );
	}

	[Fact]
	public void Not_should_return_filtered_items( ) {
		// Arrange
		var entities = Person.Mock( 100 );

		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId == Guid.Empty )
			.Not( );

		// Act
		var result = entities
			.Specify( spec )
			.ToList( );

		// Assert
		result.Should( ).NotBeEmpty( );
	}

	[Fact]
	public void Order_should_return_ordered_items( ) {
		// Arrange
		var entities = Person.Mock( 100 );

		var spec = SpecBuilder<Person>
			.Create( )
			.Order( x => x.Name );

		// Act
		var result = entities
			.AsQueryable( )
			.Sort( spec )
			.ToList( );

		// Assert
		result.Should( ).NotBeEmpty( );

		result.Should( ).BeInAscendingOrder( x => x.Name );
	}

	[Fact]
	public void Order_should_return_reverse_ordered_items( ) {
		// Arrange
		var entities = Person.Mock( 100 );

		var spec = SpecBuilder<Person>
			.Create( )
			.OrderDescending( x => x.Name );

		// Act
		var result = entities
			.AsQueryable( )
			.Sort( spec )
			.ToList( );

		// Assert
		result.Should( ).NotBeEmpty( );

		result.Should( ).BeInDescendingOrder( x => x.Name );
	}

	[Fact]
	public void Skip_should_return_items_skiping_ten( ) {
		// Arrange
		var entities = Person.Mock( 100 );

		var spec = SpecBuilder<Person>
			.Create( )
			.Order( x => x.Name )
			.Skip( 10 )
			.Take( 10 );

		// Act
		var result = entities
			.AsQueryable( )
			.Sort( spec )
			.Paginate( spec )
			.ToList( );

		// Assert
		result.Should( ).NotBeEmpty( );

		result.Should( ).BeInAscendingOrder( x => x.Name );

		result.Should( ).HaveCount( 10 );
	}

	[Fact]
	public void Prepare_should_return_items_full_tratment_of_items( ) {
		// Arrange
		var entities = Person.Mock( 100 );

		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId != Guid.Empty )
			.Order( x => x.Name )
			.Skip( 10 )
			.Take( 10 );

		// Act
		var result = entities
			.AsQueryable( )
			.Specify( spec );

		// Assert
		result.Should( ).NotBeEmpty( );

		result.Should( ).BeInAscendingOrder( x => x.Name );

		result.Should( ).HaveCount( 10 );
	}

	[Fact]
	public void Is_satisfyed_should_return_boolean( ) {
		// Arrange
		var entity = Person.Mock( );

		var spec = SpecBuilder<Person>
			.Create( )
			.And( x => x.PersonId != Guid.Empty )
			.And( x => !string.IsNullOrEmpty( x.Name ) )
			.And( x => !string.IsNullOrEmpty( x.Address ) );

		// Act
		var result = spec.IsSatisfiedBy( entity );

		// Assert
		result.Should( ).BeTrue( );
	}
}