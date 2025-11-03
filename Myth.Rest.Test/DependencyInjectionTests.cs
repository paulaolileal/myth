using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Constants;
using Myth.DependencyInjection;
using Myth.Interfaces;
using Myth.Rest.Test.Base;
using Myth.Testing.Mocks;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Rest.Test {

	public class DependencyInjectionTests : BaseTests, IDisposable {

		public DependencyInjectionTests( ) {
		}

		public void Dispose( ) {
			// No cleanup needed for TestServer
		}

		[Theory]
		[InlineData( ServiceLifetime.Scoped )]
		[InlineData( ServiceLifetime.Singleton )]
		[InlineData( ServiceLifetime.Transient )]
		public async Task The_service_collection_should_have_the_rest_in_there( ServiceLifetime lifetime ) {
			// Arrange
			var services = new ServiceCollection( );

			var client = HttpClientMock.CreateClient( settings => settings
				.ForRoute( "/test" )
				.UsingGet( )
				.RespondWith( HttpStatusCode.NoContent ) );

			// Act
			services.AddRest( conf => conf
				.WithBaseUrl( "https://localhost:8001/" )
				.WithContentType( "application/json" )
				.WithBodySerialization( CaseStrategy.CamelCase )
				.WithBodyDeserialization( CaseStrategy.CamelCase )
				.WithClient( client ),
				lifetime );

			var serviceProvider = services.BuildServiceProvider( );
			var result = serviceProvider.GetService<IRestRequest>( );
			var service = services.First( x => x.ServiceType == typeof( IRestRequest ) );

			var response = await result!
				.DoGet( "test" )
				.OnResult( resp => resp
					.UseEmptyFor( HttpStatusCode.NoContent ) )
				.BuildAsync( );

			// Assert
			result.Should( ).NotBeNull( );
			result.Should( ).BeOfType<RestBuilder>( );
			service.Lifetime.Should( ).Be( lifetime );
			response.IsSuccessStatusCode( ).Should( ).BeTrue( );
		}

		[Theory]
		[InlineData( ServiceLifetime.Scoped )]
		[InlineData( ServiceLifetime.Singleton )]
		[InlineData( ServiceLifetime.Transient )]
		public async Task The_service_collection_should_have_the_rest_file_in_there( ServiceLifetime lifetime ) {
			// Arrange
			var services = new ServiceCollection( );

			var directory = Environment.CurrentDirectory;
			var fileName = "Test1.txt";
			var path = Path.Combine( directory, fileName );

			var client = HttpClientMock.CreateClient( settings => settings
				.ForRoute( "/test" )
				.UsingGet( )
				.RespondWith( HttpStatusCode.OK )
				.WithJsonResponse( "This is a test file!" ) );

			// Act
			services.AddRest( conf => conf
				.WithBaseUrl( "https://localhost:8001/" )
				.WithClient( client ),
				lifetime );

			var serviceProvider = services.BuildServiceProvider( );
			var result = serviceProvider.GetService<IRestRequest>( );
			var service = services.First( x => x.ServiceType == typeof( IRestRequest ) );

			var response = await result!
				.DoDownload( "test" )
				.BuildAsync( );

			// Assert
			result.Should( ).NotBeNull( );
			result.Should( ).BeOfType<RestBuilder>( );
			service.Lifetime.Should( ).Be( lifetime );
			response.IsSuccessStatusCode( ).Should( ).BeTrue( );
		}
	}
}