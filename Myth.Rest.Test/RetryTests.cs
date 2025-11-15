using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using Myth.Rest.Test.Base;
using Myth.Rest.Test.Models;
using Myth.Testing.Mocks;
using Xunit;

namespace Myth.Rest.Test;

public class RetryTests : BaseTests, IDisposable {

	public RetryTests( ) {
	}

	public void Dispose( ) {
		// No cleanup needed for TestServer
	}

	[Fact]
	public async Task Retry_should_work_with_default( ) {
		// Arrange
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/retry" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.InternalServerError )
			.WithJsonResponse( _errorFaker.Generate( ) ) );

		// Act
		var response = await Rest.Create( )
			.Configure( config => config
				.WithBaseUrl( "https://localhost:5006" )
				.WithClient( client )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/retry" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.InternalServerError )
			.WithJsonResponse( _errorFaker.Generate( ) ) );

		// Act
		var response = await Rest.Create( )
			.Configure( config => config
				.WithBaseUrl( "https://localhost:5006" )
				.WithClient( client )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/retry" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.InternalServerError )
			.WithJsonResponse( _errorFaker.Generate( ) ) );

		// Act
		var response = await Rest.Create( )
			.Configure( config => config
				.WithBaseUrl( "https://localhost:5006" )
				.WithClient( client )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/retry" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.InternalServerError )
			.WithJsonResponse( _errorFaker.Generate( ) ) );

		// Act
		var response = await Rest.Create( )
			.Configure( config => config
				.WithBaseUrl( "https://localhost:5006" )
				.WithClient( client )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/retry" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.TooManyRequests )
			.WithJsonResponse( _errorFaker.Generate( ) ) );

		// Act
		var response = await Rest.Create( )
			.Configure( config => config
				.WithBaseUrl( "https://localhost:5006" )
				.WithClient( client )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/retry" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.ServiceUnavailable )
			.WithJsonResponse( _errorFaker.Generate( ) ) );

		// Act
		var response = await Rest.Create( )
			.Configure( config => config
				.WithBaseUrl( "https://localhost:5006" )
				.WithClient( client )
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
