using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Interfaces.Repositories.Results;
using Myth.Morph.Test.Models;
using Myth.Morph.Test.Service;
using Myth.Repositories.Results;
using Myth.Settings;

namespace Myth.Morph.Test {
	public class MorphTests2 {
		private readonly IServiceCollection _services;
		private IServiceProvider _serviceProvider;

		public MorphTests2( ) {
			_services = new ServiceCollection( );
			_services.AddSingleton<IDescriptionResolver, DescriptionResolver>( );
			_services.AddMorph( config => {
				config.AddGenericMorph( typeof( IPaginated<> ), typeof( Paginated<> ) );
				config.AddGenericMorph( typeof( IList<> ), typeof( List<> ) );
			} );
			_serviceProvider = _services.BuildServiceProvider( );
		}

		[Fact]
		public async Task MapExtensions_Should_HandleAsyncMappingsAsync( ) {
			// Arrange
			var entities = new List<BasicEntity> {
				new() { EntityId = 1, Name = "Test1" },
				new() { EntityId = 2, Name = "Test2" }
			};

			// Act
			var result = await entities.ToAsync<BasicDto>( _serviceProvider );

			// Assert
			result.Should( ).HaveCount( 2 );
			result.First( ).DtoId.Should( ).Be( 1 );
			result.Last( ).DtoId.Should( ).Be( 2 );
		}

		[Fact]
		public void MapRegistry_Should_HandleComplexPropertyMapping( ) {
			// Arrange
			var entity = new EntityWithNested {
				Id = 1,
				Items = [
					new() { Id = 1, Value = "Test" }
				]
			};

			// Act
			var registry = _serviceProvider.GetRequiredService<MorphRegistry>( );
			var result = registry.Morph<EntityWithNested, DtoWithNested>( entity );

			// Assert
			result.Should( ).NotBeNull( );
			result.Items.Should( ).HaveCount( 1 );
			result.Items.First( ).Value.Should( ).Be( "Test" );
		}

		[Fact]
		public void MapTo_Should_HandleMultiLevelMapping( ) {
			// Arrange
			var child = new ChildEntity { Id = 1 };
			var parent = new ParentEntity {
				Id = 2,
				Child = child
			};
			child.Parent = parent;

			// Act
			var dto = parent.To<ParentDto>( );

			// Assert
			dto.Should( ).NotBeNull( );
			dto.Id.Should( ).Be( 2 );
			dto.Child.Should( ).NotBeNull( );
			dto.Child.Id.Should( ).Be( 1 );
			dto.Child.ParentId.Should( ).Be( 2 );
		}

		[Fact]
		public void BinderBuilder_Should_HandleCustomMappings( ) {
			// Arrange
			var registry = new MorphRegistry( _serviceProvider );

			registry.Register<SourceEntity, DestEntity>( builder => {
				builder.Bind(
					dest => dest.Name,
					src => $"Custom: {src.Name}"
				);
			} );

			var source = new SourceEntity {
				Id = 1,
				Name = "Test"
			};

			// Act
			var result = registry.Morph<SourceEntity, DestEntity>( source );

			// Assert
			result.Should( ).NotBeNull( );
			result.Name.Should( ).Be( "Custom: Test" );
		}

		[Fact]
		public void ServiceCollection_Should_HandleMultipleConfigurations( ) {
			// Arrange
			var services = new ServiceCollection( );

			// Act
			services.AddMorph( config => {
				config.AddAssembly( typeof( BasicEntity ).Assembly )
					 .AddGenericMorph( typeof( IList<> ), typeof( List<> ) );
			} );

			var sp = services.BuildServiceProvider( );
			var registry = sp.GetRequiredService<MorphRegistry>( );

			// Assert
			registry.Should( ).NotBeNull( );
			registry.HasMapping( typeof( BasicEntity ), typeof( BasicDto ) )
				   .Should( ).BeTrue( );
		}

		[Fact]
		public void MapTo_Should_HandleViewModelMappings( ) {
			// Arrange
			var dto = new BasicDto {
				DtoId = 1,
				Name = "Test",
				Enabled = true
			};

			// Act
			var viewModel = dto.To<ViewModel>( );

			// Assert
			viewModel.Should( ).NotBeNull( );
			viewModel.ViewModelId.Should( ).Be( dto.DtoId );
			viewModel.Name.Should( ).Be( dto.Name );
			viewModel.Enabled.Should( ).Be( dto.Enabled );
		}

	}
}