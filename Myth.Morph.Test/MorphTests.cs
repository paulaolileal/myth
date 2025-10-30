using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Interfaces.Repositories.Results;
using Myth.Morph.Test.Models;
using Myth.Morph.Test.Models.Dtos;
using System.Collections.ObjectModel;

namespace Myth.Morph.Test {

	public class MorphTests : BaseTestFixture {
		private readonly Faker _faker = new Faker( );

		[Fact]
		public void AddMorph_Should_RegisterBindRegistry( ) {
			// Act
			var registry = ServiceProvider.GetService<SchemaRegistry>( );

			// Assert
			registry.Should( ).NotBeNull( );
		}

		[Fact]
		public void BindRegistry_Should_RegisterGenericMorphpingList( ) {
			// Arrange
			var registry = ServiceProvider.GetRequiredService<SchemaRegistry>( );

			// Assert
			var hasMorphping = registry.TryResolveGenericConcrete( typeof( IList<string> ), out var concrete );
			hasMorphping.Should( ).BeTrue( );
			concrete.Should( ).Be( typeof( List<string> ) );
		}

		[Fact]
		public void BindRegistry_Should_RegisterGenericMorphpingCollection( ) {
			// Arrange
			var registry = ServiceProvider.GetRequiredService<SchemaRegistry>( );

			// Assert
			var hasMorphping = registry.TryResolveGenericConcrete( typeof( ICollection<string> ), out var concrete );
			hasMorphping.Should( ).BeTrue( );
			concrete.Should( ).Be( typeof( List<string> ) );
		}

		[Fact]
		public void BindRegistry_Should_RegisterGenericMorphpingDictionary( ) {
			// Arrange
			var registry = ServiceProvider.GetRequiredService<SchemaRegistry>( );

			// Assert
			var hasMorphping = registry.TryResolveGenericConcrete( typeof( IDictionary<string, string> ), out var concrete );
			hasMorphping.Should( ).BeTrue( );
			concrete.Should( ).Be( typeof( Dictionary<string, string> ) );
		}

		[Fact]
		public void BindRegistry_Should_RegisterGenericMorphpingSet( ) {
			// Arrange
			var registry = ServiceProvider.GetRequiredService<SchemaRegistry>( );

			// Assert
			var hasMorphping = registry.TryResolveGenericConcrete( typeof( ISet<string> ), out var concrete );
			hasMorphping.Should( ).BeTrue( );
			concrete.Should( ).Be( typeof( HashSet<string> ) );
		}

		[Fact]
		public void BindRegistry_Should_RegisterGenericMorphpingReadOnlyCollection( ) {
			// Arrange
			var registry = ServiceProvider.GetRequiredService<SchemaRegistry>( );

			// Assert
			var hasMorphping = registry.TryResolveGenericConcrete( typeof( IReadOnlyCollection<string> ), out var concrete );
			hasMorphping.Should( ).BeTrue( );
			concrete.Should( ).Be( typeof( ReadOnlyCollection<string> ) );
		}

		[Fact]
		public void BindRegistry_Should_RegisterGenericMorphpingReadOnlyList( ) {
			// Arrange
			var registry = ServiceProvider.GetRequiredService<SchemaRegistry>( );

			// Assert
			var hasMorphping = registry.TryResolveGenericConcrete( typeof( IReadOnlyList<string> ), out var concrete );
			hasMorphping.Should( ).BeTrue( );
			concrete.Should( ).Be( typeof( List<string> ) );
		}

		[Fact]
		public void BindRegistry_Should_RegisterGenericMorphpingReadOnlySet( ) {
			// Arrange
			var registry = ServiceProvider.GetRequiredService<SchemaRegistry>( );

			// Assert
			var hasMorphping = registry.TryResolveGenericConcrete( typeof( IReadOnlySet<string> ), out var concrete );
			hasMorphping.Should( ).BeTrue( );
			concrete.Should( ).Be( typeof( HashSet<string> ) );
		}

		[Fact]
		public void MorphTo_Should_HandleNullValues( ) {
			// Arrange
			BasicEntity? entity = null;

			// Act
			var result = entity.To<BasicDto>( ServiceProvider );

			// Assert
			result.Should( ).BeNull( );
		}

		[Fact]
		public void MorphTo_Should_HandleCollections( ) {
			// Arrange
			var entities = new List<BasicEntity>( ) {
				new () {
					Description = _faker.Lorem.Text(),
					Enabled = _faker.Random.Bool(),
					EntityId = _faker.Random.Number(),
					Name = _faker.Name.FirstName(),
				},
				new () {
					Description = _faker.Lorem.Text(),
					Enabled =  _faker.Random.Bool(),
					EntityId = _faker.Random.Number(),
					Name = _faker.Name.FirstName()
				}
			};

			// Act
			var result = entities.To<BasicDto>( ServiceProvider );

			// Assert
			result.Should( ).NotBeNull( );
			var firstItem = result.First( );
			var firstEntity = entities.First( );
			firstItem.Should( ).NotBeNull( );
			firstItem.DtoId.Should( ).Be( firstEntity.EntityId );
			firstItem.Name.Should( ).Be( firstEntity.Name );
			firstItem.Description.Should( ).Be( firstEntity.Description );
			firstItem.Enabled.Should( ).Be( !firstEntity.Enabled );

			var secondItem = result.Last( );
			var secondEntity = entities.Last( );
			secondItem.Should( ).NotBeNull( );
			secondItem.DtoId.Should( ).Be( secondEntity.EntityId );
			secondItem.Name.Should( ).Be( secondEntity.Name );
			secondItem.Description.Should( ).Be( secondEntity.Description );
			secondItem.Enabled.Should( ).Be( !secondEntity.Enabled );
		}

		[Fact]
		public void MorphTo_Should_HandleGenerics( ) {
			// Arrange
			var entities = new List<BasicEntity>( ) {
				new () {
					Description = _faker.Lorem.Text(),
					Enabled = _faker.Random.Bool(),
					EntityId = _faker.Random.Number(),
					Name = _faker.Name.FirstName(),
				},
				new () {
					Description = _faker.Lorem.Text(),
					Enabled =  _faker.Random.Bool(),
					EntityId = _faker.Random.Number(),
					Name = _faker.Name.FirstName()
				}
			};

			// Act
			// Act
			var paginatedEntities = entities.AsPaginated( );
			var result = paginatedEntities.To<IPaginated<BasicDto>>( ServiceProvider );

			// Assert
			result.Should( ).NotBeNull( );
			result.Items.Should( ).HaveCount( 2 );
			result.PageNumber.Should( ).Be( 1 );
			result.PageSize.Should( ).Be( 2 );
			result.TotalItems.Should( ).Be( 2 );
			result.TotalPages.Should( ).Be( 1 );

			// Verifica se os itens foram Morpheados corretamente
			var firstItem = result.Items.First( );
			var firstEntity = entities.First( );
			firstItem.Should( ).NotBeNull( );
			firstItem.DtoId.Should( ).Be( firstEntity.EntityId );
			firstItem.Name.Should( ).Be( firstEntity.Name );
			firstItem.Description.Should( ).Be( firstEntity.Description );
			firstItem.Enabled.Should( ).Be( !firstEntity.Enabled );

			var secondItem = result.Items.Last( );
			var secondEntity = entities.Last( );
			secondItem.Should( ).NotBeNull( );
			secondItem.DtoId.Should( ).Be( secondEntity.EntityId );
			secondItem.Name.Should( ).Be( secondEntity.Name );
			secondItem.Description.Should( ).Be( secondEntity.Description );
			secondItem.Enabled.Should( ).Be( !secondEntity.Enabled );
		}

		[Fact]
		public void MorphTo_Should_MorphNestedCollections( ) {
			// Arrange
			var entity = new EntityWithNested {
				Id = 1,
				Items = [
					new() { Id = _faker.Random.Number(), Value = _faker.Lorem.Word() },
					new() { Id = _faker.Random.Number(), Value = _faker.Lorem.Word() }
				]
			};

			// Act
			var dto = entity.To<DtoWithNested>( ServiceProvider );

			// Assert
			dto.Items.Should( ).HaveCount( 2 );
			dto.Items.Should( ).BeEquivalentTo( entity.Items, opts => opts
				.ComparingByMembers<NestedItem>( ) );
		}

		[Fact]
		public void MorphTo_Should_HandleInheritance( ) {
			// Arrange
			var derived = new DerivedEntity {
				BaseProperty = _faker.Lorem.Word( ),
				DerivedProperty = _faker.Lorem.Word( )
			};

			// Act
			var dto = derived.To<DerivedDto>( ServiceProvider );

			// Assert
			dto.BaseProperty.Should( ).Be( derived.BaseProperty );
			dto.DerivedProperty.Should( ).Be( derived.DerivedProperty );
		}

		[Fact]
		public void MorphpingBuilder_Should_IgnoreSpecifiedProperties( ) {
			// Arrange
			var source = new SourceEntity {
				Id = _faker.Random.Number( ),
				Name = _faker.Lorem.Word( ),
				IgnoredValue = _faker.Lorem.Text( )
			};

			// Act
			var result = source.To<DestEntity>( ServiceProvider );

			// Assert
			result.Id.Should( ).Be( source.Id );
			result.Name.Should( ).Be( source.Name );
			result.IgnoredProperty.Should( ).BeNull( );
		}

		[Fact]
		public async Task MorphTo_Should_HandleAsyncMorphpings( ) {
			// Arrange
			var value = _faker.Lorem.Text( );

			var entity = new EntityWithAsync {
				Id = 1,
				AsyncValue = Task.FromResult( value )
			};

			// Act
			var dto = await entity.ToAsync<DtoWithAsync>( ServiceProvider );

			// Assert
			dto.Id.Should( ).Be( 1 );
			dto.Value.Should( ).Be( value );
		}

		[Fact]
		public void CanMorphTo_Should_ReturnCorrectResult( ) {
			// Arrange
			var entity = new BasicEntity {
				EntityId = _faker.Random.Number( )
			};

			// Act
			var canMorph = entity.CanBindTo<BasicDto>( ServiceProvider );
			var cannotMorph = entity.CanBindTo<NonBindableDto>( ServiceProvider );

			// Assert
			canMorph.Should( ).BeTrue( );
			cannotMorph.Should( ).BeFalse( );
		}

		[Fact]
		public void MorphTo_Should_HandleCircularReferences( ) {
			// Arrange
			var parent = new ParentEntity {
				Id = _faker.Random.Number( )
			};

			var child = new ChildEntity {
				Id = _faker.Random.Number( ),
				Parent = parent
			};

			parent.Child = child;

			// Act
			var dto = parent.To<ParentDto>( ServiceProvider );

			// Assert
			dto.Id.Should( ).Be( parent.Id );
			dto.Child.Should( ).NotBeNull( );
			dto.Child.Id.Should( ).Be( child.Id );
			dto.Child.ParentId.Should( ).Be( parent.Id );
		}

		[Fact]
		public void MorphTo_Should_HandleDependencyInjection( ) {
			// Arrange
			var entity = new EntityWithDependency {
				Id = _faker.Random.Guid( ),
				Text = _faker.Lorem.Text( )
			};

			// Act
			var dto = entity.To<DtoWithDependency>( ServiceProvider );

			// Assert
			dto.Id.Should( ).Be( entity.Id );
			dto.Text.Should( ).Be( entity.Text.ToUpper( ) );
		}
	}
}