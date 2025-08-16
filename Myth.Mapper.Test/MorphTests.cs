using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Interfaces.Repositories.Results;
using Myth.Morph.Test.Models;
using Myth.Morph.Test.Service;
using Myth.Repositories.Results;
using System.Collections.ObjectModel;

namespace Myth.Morph.Test {

	public class MorphTests {
		private readonly IServiceCollection _services;
		private IServiceProvider _serviceProvider;

		public MorphTests( ) {
			_services = new ServiceCollection( );
			_services.AddLogging( );
			_services.AddSingleton<IDescriptionResolver, DescriptionResolver>( );
			_services.AddMorph( config => {
				config.AddGenericMorph( typeof( IPaginated<> ), typeof( Paginated<> ) );
			} );
			_serviceProvider = _services.BuildServiceProvider( );
		}

		[Fact]
		public void AddMorph_Should_RegisterBindRegistry( ) {
			// Act
			var registry = _serviceProvider.GetService<SchemaRegistry>( );

			// Assert
			registry.Should( ).NotBeNull( );
		}

		[Fact]
		public void BindRegistry_Should_RegisterGenericMorphpingList( ) {
			// Arrange
			var registry = _serviceProvider.GetRequiredService<SchemaRegistry>( );

			// Assert
			var hasMorphping = registry.TryResolveGenericConcrete( typeof( IList<string> ), out var concrete );
			hasMorphping.Should( ).BeTrue( );
			concrete.Should( ).Be( typeof( List<string> ) );
		}

		[Fact]
		public void BindRegistry_Should_RegisterGenericMorphpingCollection( ) {
			// Arrange
			var registry = _serviceProvider.GetRequiredService<SchemaRegistry>( );

			// Assert
			var hasMorphping = registry.TryResolveGenericConcrete( typeof( ICollection<string> ), out var concrete );
			hasMorphping.Should( ).BeTrue( );
			concrete.Should( ).Be( typeof( List<string> ) );
		}

		[Fact]
		public void BindRegistry_Should_RegisterGenericMorphpingDictionary( ) {
			// Arrange
			var registry = _serviceProvider.GetRequiredService<SchemaRegistry>( );

			// Assert
			var hasMorphping = registry.TryResolveGenericConcrete( typeof( IDictionary<string, string> ), out var concrete );
			hasMorphping.Should( ).BeTrue( );
			concrete.Should( ).Be( typeof( Dictionary<string, string> ) );
		}

		[Fact]
		public void BindRegistry_Should_RegisterGenericMorphpingSet( ) {
			// Arrange
			var registry = _serviceProvider.GetRequiredService<SchemaRegistry>( );

			// Assert
			var hasMorphping = registry.TryResolveGenericConcrete( typeof( ISet<string> ), out var concrete );
			hasMorphping.Should( ).BeTrue( );
			concrete.Should( ).Be( typeof( HashSet<string> ) );
		}

		[Fact]
		public void BindRegistry_Should_RegisterGenericMorphpingReadOnlyCollection( ) {
			// Arrange
			var registry = _serviceProvider.GetRequiredService<SchemaRegistry>( );

			// Assert
			var hasMorphping = registry.TryResolveGenericConcrete( typeof( IReadOnlyCollection<string> ), out var concrete );
			hasMorphping.Should( ).BeTrue( );
			concrete.Should( ).Be( typeof( ReadOnlyCollection<string> ) );
		}

		[Fact]
		public void BindRegistry_Should_RegisterGenericMorphpingReadOnlyList( ) {
			// Arrange
			var registry = _serviceProvider.GetRequiredService<SchemaRegistry>( );

			// Assert
			var hasMorphping = registry.TryResolveGenericConcrete( typeof( IReadOnlyList<string> ), out var concrete );
			hasMorphping.Should( ).BeTrue( );
			concrete.Should( ).Be( typeof( List<string> ) );
		}

		[Fact]
		public void BindRegistry_Should_RegisterGenericMorphpingReadOnlySet( ) {
			// Arrange
			var registry = _serviceProvider.GetRequiredService<SchemaRegistry>( );

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
			var result = entity.To<BasicDto>( );

			// Assert
			result.Should( ).BeNull( );
		}

		[Fact]
		public void MorphTo_Should_HandleCollections( ) {
			// Arrange
			var entities = new List<BasicEntity>( ) {
				new () {
					Description = "Test",
					Enabled = true,
					EntityId = 1,
					Name = "EntityA"
				},
				new () {
					Description = "Test",
					Enabled = true,
					EntityId = 1,
					Name = "EntityA"
				}
			};

			// Act
			var result = entities.To<BasicDto>( );

			// Assert
			result.Should( ).NotBeNull( );
		}

		[Fact]
		public void MorphTo_Should_HandleGenerics( ) {
			// Arrange
			var entities = new List<BasicEntity> {
				new BasicEntity {
					Description = "Test",
					Enabled = true,
					EntityId = 1,
					Name = "EntityA"
				},
				new BasicEntity {
					Description = "Test",
					Enabled = true,
					EntityId = 2,
					Name = "EntityB"
				}
			};

			// Act
			// Act
			var paginatedEntities = entities.AsPaginated( );
			var result = paginatedEntities.To<IPaginated<BasicDto>>( _serviceProvider );

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
					new() { Id = 1, Value = "One" },
					new() { Id = 2, Value = "Two" }
				]
			};

			// Act
			var dto = entity.To<DtoWithNested>( );

			// Assert
			dto.Items.Should( ).HaveCount( 2 );
			dto.Items.Should( ).BeEquivalentTo( entity.Items, opts =>
				opts.ComparingByMembers<NestedItem>( ) );
		}

		[Fact]
		public void MorphTo_Should_HandleInheritance( ) {
			// Arrange
			var derived = new DerivedEntity {
				BaseProperty = "Base",
				DerivedProperty = "Derived"
			};

			// Act
			var dto = derived.To<DerivedDto>( );

			// Assert
			dto.BaseProperty.Should( ).Be( "Base" );
			dto.DerivedProperty.Should( ).Be( "Derived" );
		}

		[Fact]
		public void MorphpingBuilder_Should_IgnoreSpecifiedProperties( ) {
			// Arrange
			var source = new SourceEntity {
				Id = 1,
				Name = "Test",
				IgnoredValue = "Should not Morph"
			};

			// Act
			var result = source.To<DestEntity>( );

			// Assert
			result.Id.Should( ).Be( 1 );
			result.Name.Should( ).Be( "Test" );
			result.IgnoredProperty.Should( ).BeNull( );
		}

		[Fact]
		public async Task MorphTo_Should_HandleAsyncMorphpings( ) {
			// Arrange
			var entity = new EntityWithAsync {
				Id = 1,
				AsyncValue = Task.FromResult( "Async Result" )
			};

			// Act
			var dto = await entity.ToAsync<DtoWithAsync>( );

			// Assert
			dto.Id.Should( ).Be( 1 );
			dto.Value.Should( ).Be( "Async Result" );
		}

		[Fact]
		public void CanMorphTo_Should_ReturnCorrectResult( ) {
			// Arrange
			var entity = new BasicEntity {
				EntityId = 1
			};

			// Act
			var canMorph = entity.CanBindTo<BasicDto>( );
			var cannotMorph = entity.CanBindTo<NonBindableDto>( );

			// Assert
			canMorph.Should( ).BeTrue( );
			cannotMorph.Should( ).BeFalse( );
		}

		[Fact]
		public void MorphTo_Should_HandleCircularReferences( ) {
			// Arrange
			var parent = new ParentEntity {
				Id = 1
			};

			var child = new ChildEntity {
				Id = 2,
				Parent = parent
			};

			parent.Child = child;

			// Act
			var dto = parent.To<ParentDto>( );

			// Assert
			dto.Id.Should( ).Be( 1 );
			dto.Child.Should( ).NotBeNull( );
			dto.Child.Id.Should( ).Be( 2 );
			dto.Child.ParentId.Should( ).Be( 1 );
		}
	}
}