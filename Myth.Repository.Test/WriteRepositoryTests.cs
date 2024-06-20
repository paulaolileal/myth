using Bogus;
using FluentAssertions;
using Myth.Repository.Test.Contexts;
using Myth.Repository.Test.Interfaces;
using Myth.Repository.Test.Mocks;
using Person = Myth.Repository.Test.Models.Person;

namespace Myth.Repository.Test {

	public class WriteRepositoryTests : IClassFixture<DatabaseMock> {
		private readonly IRepositoryTest _repository;
		private readonly ContextTest _context;
		private readonly Faker _faker;
		private readonly Func<Task<Person>> _mockAsync;
		private readonly Func<int, Task<IEnumerable<Person>>> _mockManyAsync;

		public WriteRepositoryTests( DatabaseMock databaseMock ) {
			_repository = databaseMock.Repository;
			_context = databaseMock.Context;
			_faker = databaseMock.Faker;
			_mockAsync = databaseMock.MockAsync;
			_mockManyAsync = databaseMock.MockAsync;
		}

		[Fact]
		public async Task Add_should_add_one_in_context( ) {
			// Arrange
			var person = Person.Mock( );

			// Act
			await _repository.AddAsync( person );

			await _repository.SaveChangesAsync( );

			// Assert
			var result = _context.Persons;

			result.Should( ).ContainEquivalentOf( person );
		}

		[Fact]
		public async Task Add_range_should_add_multiples_in_context( ) {
			// Arrange
			var persons = Person.Mock( 100 );

			// Act
			await _repository.AddRangeAsync( persons );

			await _repository.SaveChangesAsync( );

			// Assert
			var result = _context.Persons;

			result.Should( ).Contain( persons );
		}

		[Fact]
		public async Task Remove_should_remove_an_item_in_context( ) {
			// Arrange
			var person = await _mockAsync( );

			// Act
			await _repository.RemoveAsync( person );

			await _repository.SaveChangesAsync( );

			// Assert
			var result = _context.Persons;

			result.Should( ).NotContain( person );
		}

		[Fact]
		public async Task Remove_range_should_remove_multiple_items_in_context( ) {
			// Arrange
			var persons = await _mockManyAsync( 100 );

			// Act
			await _repository.RemoveRangeAsync( persons );

			await _repository.SaveChangesAsync( );

			// Assert
			var result = _context.Persons;

			result.Should( ).NotContain( persons );
		}

		[Fact]
		public async Task Update_should_update_an_item_in_context( ) {
			// Arrange
			var person = await _mockAsync( );

			// Act
			person.Name = _faker.Person.FullName;
			await _repository.UpdateAsync( person );

			await _repository.SaveChangesAsync( );

			// Assert
			var result = _context.Persons;

			result.Should( ).Contain( person );
		}

		[Fact]
		public async Task Update_range_should_update_many_items_in_context( ) {
			// Arrange
			var persons = await _mockManyAsync( 100 );

			// Act
			foreach ( var person in persons )
				person.Name = _faker.Person.FullName;
			await _repository.UpdateRangeAsync( persons );

			await _repository.SaveChangesAsync( );

			// Assert
			var result = _context.Persons;

			result.Should( ).Contain( persons );
		}

		[Fact]
		public async Task Get_provider_name_should_return_a_name( ) {
			// Act
			var result = _repository.GetProviderName( );

			// Assert
			result.Should( ).NotBeEmpty( );
		}
	}
}