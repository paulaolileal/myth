using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Constants;
using Myth.DependencyInjection;
using Myth.Rest.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Myth.Rest.Test {

	public class RestDependencyInjectionTests : IDisposable {
		private readonly WireMockServer _server;

		public RestDependencyInjectionTests( ) {
			_server = WireMockServer.Start( 8001, true );
		}

		public void Dispose( ) {
			_server.Stop( );
			_server.Dispose( );
		}

		[Theory]
		[InlineData( ServiceLifetime.Scoped )]
		[InlineData( ServiceLifetime.Singleton )]
		[InlineData( ServiceLifetime.Transient )]
		public async Task The_service_collection_should_have_the_rest_in_there( ServiceLifetime lifetime ) {
			// Arrange
			var services = new ServiceCollection( );

			_server
				.Given(
					Request
						.Create( )
						.WithPath( "/test" )
						.UsingGet( ) )
				.RespondWith(
					Response
						.Create( )
						.WithStatusCode( HttpStatusCode.NoContent ) );

			// Act
			services.AddRest( conf => conf
				.WithBaseUrl( "https://localhost:8001/" )
				.WithContentType( "application/json" )
				.WithBodySerialization( CaseStrategy.CamelCase )
				.WithBodyDeserialization( CaseStrategy.CamelCase ),
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

			_server
				.Given(
					Request
						.Create( )
						.WithPath( "/test" )
						.UsingGet( ) )
				.RespondWith(
					Response
						.Create( )
						.WithBody( "This is a test file!", BodyDestinationFormat.Bytes )
						.WithHeader( "ContentType", "plain/text" )
						.WithStatusCode( HttpStatusCode.OK ) );

			// Act
			services.AddRest( conf => conf
				.WithBaseUrl( "https://localhost:8001/" ),
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