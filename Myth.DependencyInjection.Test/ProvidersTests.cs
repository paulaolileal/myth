using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Myth.DependencyInjection.Providers.Extensions;
using Myth.DependencyInjection.Test.Models;
using Myth.Extensions;
using Myth.ValueProviders;

namespace Myth.DependencyInjection.Test {

	public class ProvidersTests {

		[Fact]
		public void Add_swagger_versioned_should_not_throw_exception( ) {
			// Arrange
			var services = new ServiceCollection( );

			services.AddLogging( );
			services.AddControllers( );
			services.AddVersioning( 1 );

			// Act
			var action = ( ) => services.AddSwaggerVersioned( settings => {
				settings.Title = "API Test";
				settings.Description = "This is an API test";
				settings.Options.UseBasicAuthorization( );
			} );

			var app = new ApplicationBuilder( services.BuildServiceProvider( ) );
			var action2 = ( ) => app.UseSwaggerVersioned( );

			// Assert
			action.Should( ).NotThrow<Exception>( );
			action2.Should( ).NotThrow<Exception>( );
		}

		[Fact]
		public void Add_mapping_type_should_not_throw_exception( ) {
			// Arrange
			var services = new ServiceCollection( );

			// Act
			services.AddTypeMapping( opt => opt.CreateMap<Models.Test, Person>( ) );
			var model = new Models.Test( );
			var result = model.MapTo<Person>( );

			// Assert
			result.Should( ).NotBeNull( );
		}

		[Fact]
		public async Task Add_mapping_type_async_should_not_throw_exceptionAsync( ) {
			// Arrange
			var services = new ServiceCollection( );

			// Act
			services.AddTypeMapping( opt => opt.CreateMap<Models.Test, Person>( ) );
			var model = Task.FromResult( new Models.Test( ) );
			var result = await model.MapToAsync<Models.Test, Person>( );

			// Assert
			result.Should( ).NotBeNull( );
		}
	}
}