using Bogus;
using FluentAssertions;
using Myth.Rest.Test.Models;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Myth.Rest.Test {

	public class RetryTests : IDisposable {
		private readonly WireMockServer _server;
		private readonly Faker _faker;

		public RetryTests( ) {
			_server = WireMockServer.Start( 5006, true );

			_faker = new Faker( "pt_BR" );
		}

		public void Dispose( ) {
			_server.Stop( );
			_server.Dispose( );
		}

		[Fact]
		public async Task Retry_should_work_with_default( ) {
			// Arrange
			_server
				.Given(
					Request
						.Create( )
						.WithPath( "/retry" )
						.UsingGet( ) )
				.RespondWith(
					Response
						.Create( )
						.WithBodyAsJson( new {
							errorCode = _faker.Random.Int( 1000, 9999 ),
							message = _faker.Lorem.Text( )
						} )
						.WithStatusCode( HttpStatusCode.InternalServerError ) );

			// Act
			var response = await Rest.Create( )
				.Configure( config => config
					.WithBaseUrl( "https://localhost:5006" )
					.WithRetry( )
				)
				.DoGet( "/retry" )
				.OnResult( res => res.UseTypeFor<Error>( HttpStatusCode.InternalServerError ) )
				.OnError( x => x.NotThrowFor( HttpStatusCode.InternalServerError ) )
				.BuildAsync( );

			// Assert
			response.IsSuccessStatusCode( ).Should( ).BeFalse( );
			response.RetriesMade.Should( ).BeGreaterThan( 0 );
		}

		[Fact]
		public async Task Retry_should_work_with_random( ) {
			// Arrange
			_server
				.Given(
					Request
						.Create( )
						.WithPath( "/retry" )
						.UsingGet( ) )
				.RespondWith(
					Response
						.Create( )
						.WithBodyAsJson( new {
							errorCode = _faker.Random.Int( 1000, 9999 ),
							message = _faker.Lorem.Text( )
						} )
						.WithStatusCode( HttpStatusCode.InternalServerError ) );

			// Act
			var response = await Rest.Create( )
				.Configure( config => config
					.WithBaseUrl( "https://localhost:5006" )
					.WithRetry( retry => retry
						.WithMaxAttempts( 5 )
						.UseRandom(
							minDelay: TimeSpan.FromSeconds( 1 ),
							maxDelay: TimeSpan.FromSeconds( 5 )
						)
						.ForServerErrors( )
					)
				)
				.DoGet( "/retry" )
				.OnResult( res => res.UseTypeFor<Error>( HttpStatusCode.InternalServerError ) )
				.OnError( x => x.NotThrowFor( HttpStatusCode.InternalServerError ) )
				.BuildAsync( );

			// Assert
			response.IsSuccessStatusCode( ).Should( ).BeFalse( );
			response.RetriesMade.Should( ).BeGreaterThan( 0 );
		}

		[Fact]
		public async Task Retry_should_work_with_exponential_backoff( ) {
			// Arrange
			_server
				.Given(
					Request
						.Create( )
						.WithPath( "/retry" )
						.UsingGet( ) )
				.RespondWith(
					Response
						.Create( )
						.WithBodyAsJson( new {
							errorCode = _faker.Random.Int( 1000, 9999 ),
							message = _faker.Lorem.Text( )
						} )
						.WithStatusCode( HttpStatusCode.InternalServerError ) );

			// Act
			var response = await Rest.Create( )
				.Configure( config => config
					.WithBaseUrl( "https://localhost:5006" )
					.WithRetry( retry => retry
						.WithMaxAttempts( 4 )
						.UseExponentialBackoff(
							baseDelay: TimeSpan.FromMilliseconds( 500 ),
							multiplier: 1.5,
							maxDelay: TimeSpan.FromSeconds( 10 )
						)
						.ForStatusCodes(
							HttpStatusCode.ServiceUnavailable,
							HttpStatusCode.TooManyRequests,
							HttpStatusCode.InternalServerError
						)
					)
				)
				.DoGet( "/retry" )
				.OnResult( res => res.UseTypeFor<Error>( HttpStatusCode.InternalServerError ) )
				.OnError( x => x.NotThrowFor( HttpStatusCode.InternalServerError ) )
				.BuildAsync( );

			// Assert
			response.IsSuccessStatusCode( ).Should( ).BeFalse( );
			response.RetriesMade.Should( ).BeGreaterThan( 0 );
		}

		[Fact]
		public async Task Retry_should_work_with_jitter( ) {
			// Arrange
			_server
				.Given(
					Request
						.Create( )
						.WithPath( "/retry" )
						.UsingGet( ) )
				.RespondWith(
					Response
						.Create( )
						.WithBodyAsJson( new {
							errorCode = _faker.Random.Int( 1000, 9999 ),
							message = _faker.Lorem.Text( )
						} )
						.WithStatusCode( HttpStatusCode.InternalServerError ) );

			// Act
			var response = await Rest.Create( )
				.Configure( config => config
					.WithBaseUrl( "https://localhost:5006" )
					.WithRetry( retry => retry
						.WithMaxAttempts( 3 )
						.UseExponentialBackoffWithJitter(
							baseDelay: TimeSpan.FromSeconds( 1 ),
							multiplier: 2.0,
							maxDelay: TimeSpan.FromSeconds( 30 )
						)
						.ForServerErrors( )
						.ForExceptions( typeof( TaskCanceledException ), typeof( HttpRequestException ) )
					)
				)
				.DoGet( "/retry" )
				.OnResult( res => res.UseTypeFor<Error>( HttpStatusCode.InternalServerError ) )
				.OnError( x => x.NotThrowFor( HttpStatusCode.InternalServerError ) )
				.BuildAsync( );

			// Assert
			response.IsSuccessStatusCode( ).Should( ).BeFalse( );
			response.RetriesMade.Should( ).BeGreaterThan( 0 );
		}

		[Fact]
		public async Task Retry_should_work_with_fixed_delay( ) {
			// Arrange
			_server
				.Given(
					Request
						.Create( )
						.WithPath( "/retry" )
						.UsingGet( ) )
				.RespondWith(
					Response
						.Create( )
						.WithBodyAsJson( new {
							errorCode = _faker.Random.Int( 1000, 9999 ),
							message = _faker.Lorem.Text( )
						} )
						.WithStatusCode( HttpStatusCode.TooManyRequests ) );

			// Act
			var response = await Rest.Create( )
				.Configure( config => config
					.WithBaseUrl( "https://localhost:5006" )
					.WithRetry( retry => retry
						.WithMaxAttempts( 2 )
						.UseFixedDelay( TimeSpan.FromSeconds( 3 ) )
						.ForStatusCodes( HttpStatusCode.TooManyRequests )
					)
				)
				.DoGet( "/retry" )
				.OnResult( res => res.UseTypeFor<Error>( HttpStatusCode.TooManyRequests ) )
				.OnError( x => x.NotThrowFor( HttpStatusCode.TooManyRequests ) )
				.BuildAsync( );

			// Assert
			response.IsSuccessStatusCode( ).Should( ).BeFalse( );
			response.RetriesMade.Should( ).BeGreaterThan( 0 );
		}

		[Fact]
		public async Task Retry_should_work_with_simple( ) {
			// Arrange
			_server
				.Given(
					Request
						.Create( )
						.WithPath( "/retry" )
						.UsingGet( ) )
				.RespondWith(
					Response
						.Create( )
						.WithBodyAsJson( new {
							errorCode = _faker.Random.Int( 1000, 9999 ),
							message = _faker.Lorem.Text( )
						} )
						.WithStatusCode( HttpStatusCode.ServiceUnavailable ) );

			// Act
			var response = await Rest.Create( )
				.Configure( config => config
					.WithBaseUrl( "https://localhost:5006" )
					.WithRetry( 3, TimeSpan.FromSeconds( 2 ), HttpStatusCode.ServiceUnavailable )
				)
				.DoGet( "/retry" )
				.OnResult( res => res.UseTypeFor<Error>( HttpStatusCode.ServiceUnavailable ) )
				.OnError( x => x.NotThrowFor( HttpStatusCode.ServiceUnavailable ) )
				.BuildAsync( );

			// Assert
			response.IsSuccessStatusCode( ).Should( ).BeFalse( );
			response.RetriesMade.Should( ).BeGreaterThan( 0 );
		}
	}
}