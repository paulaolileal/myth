using Bogus;
using FluentAssertions;
using Myth.Constants;
using Myth.Exceptions;
using Myth.Rest.Test.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Myth.Rest.Test;

public class RestContentTests : IDisposable {
	private readonly RestBuilder _restClient;
	private readonly WireMockServer _server;
	private readonly Faker _faker;

	public RestContentTests( ) {
		_server = WireMockServer.Start( 5001, true );

		_faker = new Faker( "pt_BR" );

		_restClient = Rest
			.Create( )
			.Configure( config => config
				.WithBaseUrl( "https://localhost:5001/" )
				.WithContentType( "application/json" )
				.WithBodySerialization( CaseStrategy.CamelCase )
				.WithRetry( 3, TimeSpan.FromSeconds( 10 ), HttpStatusCode.InternalServerError ) );
	}

	public void Dispose( ) {
		_server.Stop( );
		_server.Dispose( );
	}

	[Fact]
	public async Task Get_should_be_success_if_status_code_is_success_and_body_doesnt_care( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-success" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson( new[ ] {
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
					} )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var response = await _restClient
			.DoGet( "get-success" )
			.OnResult( res => res
				.DoNotMap( ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Get );
		response.ElapsedTime.Should( ).BeGreaterThan( TimeSpan.MinValue );
		response.ResultType.Should( ).Be( null );
		response.IsSuccessStatusCode( ).Should( ).BeTrue( );
	}

	[Fact]
	public async Task Get_should_return_list_of_items( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-success" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson( new[ ] {
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
					} )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var response = await _restClient
			.DoGet( "get-success" )
			.OnResult( config => config
				.UseTypeForSuccess<IEnumerable<Post>>( ) )
			.OnError( error => error
				.ThrowForNonSuccess( ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Get );
		response.ElapsedTime.Should( ).BeGreaterThan( TimeSpan.MinValue );
		response.IsSuccessStatusCode( ).Should( ).BeTrue( );

		var result = response.GetAs<IEnumerable<Post>>( );

		result.Should( )
			.NotBeEmpty( ).And
			.AllSatisfy( prop => {
				prop.Id.Should( ).BeGreaterThanOrEqualTo( 0 );
				prop.Title.Should( ).NotBeEmpty( );
				prop.Body.Should( ).NotBeEmpty( );
				prop.UserId.Should( ).NotBeEmpty( );
			} );
	}

	[Fact]
	public async Task Get_should_return_list_of_items_on_non_success( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-non-success" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson( new[ ] {
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
					} )
					.WithStatusCode( HttpStatusCode.BadRequest ) );

		// Act
		var response = await _restClient
			.DoGet( "get-non-success" )
			.OnResult( config => config
				.UseTypeForNonSuccess<IEnumerable<Post>>( ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.BadRequest );
		response.Method.Should( ).Be( HttpMethod.Get );
		response.ElapsedTime.Should( ).BeGreaterThan( TimeSpan.MinValue );
		response.IsSuccessStatusCode( ).Should( ).BeFalse( );

		var result = response.GetAs<IEnumerable<Post>>( );

		result.Should( )
			.NotBeEmpty( ).And
			.AllSatisfy( prop => {
				prop.Id.Should( ).BeGreaterThanOrEqualTo( 0 );
				prop.Title.Should( ).NotBeEmpty( );
				prop.Body.Should( ).NotBeEmpty( );
				prop.UserId.Should( ).NotBeEmpty( );
			} );
	}

	[Fact]
	public async Task Get_should_throw_exception_on_non_success_status_code( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-error" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson( new {
						errorCode = 422,
						message = "Error on get value"
					} )
					.WithStatusCode( HttpStatusCode.UnprocessableEntity ) );

		// Act
		var action = async ( ) => await _restClient
			.DoGet( "get-error" )
			.OnResult( config => config
				.UseTypeForSuccess<IEnumerable<Post>>( ) )
			.OnError( error => error
				.ThrowForNonSuccess( ) )
			.BuildAsync( );

		// Assert
		var exceptionAssertion = await action.Should( ).ThrowAsync<NonSuccessException>( );

		var result = exceptionAssertion.Which;

		result.Method.Should( ).Be( HttpMethod.Get );
		result.StatusCode.Should( ).Be( HttpStatusCode.UnprocessableEntity );
		result.RawMessage.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Get_should_throw_exception_on_diffent_mapped_type( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-success" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson( new[ ] {
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
					} )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var response = await _restClient
			.DoGet( "get-success" )
			.OnResult( config => config
				.UseTypeForSuccess<IEnumerable<Post>>( ) )
			.OnError( error => error
				.ThrowForNonSuccess( ) )
			.BuildAsync( );

		var action = ( ) => response.GetAs<string>( );

		action.Should( ).Throw<DifferentResponseTypeException>( );
	}

	[Fact]
	public async Task Get_should_throw_exception_when_no_action_made( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-success" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson( new[ ] {
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
						new { id = _faker.UniqueIndex,title = _faker.Lorem.Lines(1), body = _faker.Lorem.Text(), userId = _faker.Random.Guid()},
					} )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var action = async ( ) => await _restClient
			.OnResult( config => config
				.UseTypeForSuccess<IEnumerable<Post>>( ) )
			.OnError( error => error
				.ThrowForNonSuccess( ) )
			.BuildAsync( );

		await action.Should( ).ThrowAsync<NoActionMadeException>( );
	}

	[Fact]
	public async Task Get_should_throw_exception_on_non_success_specified_status_code( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-error-specified" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson( new {
						errorCode = 422,
						message = "Error on get value"
					} )
					.WithStatusCode( HttpStatusCode.UnprocessableEntity ) );

		// Act
		var action = async ( ) => await _restClient
			.DoGet( "get-error-specified" )
			.OnResult( config => config
				.UseTypeForSuccess<IEnumerable<Post>>( ) )
			.OnError( error => error
				.ThrowFor( HttpStatusCode.UnprocessableEntity ) )
			.BuildAsync( );

		// Assert
		var exceptionAssertion = await action.Should( ).ThrowAsync<NonSuccessException>( );

		var result = exceptionAssertion.Which;

		result.Method.Should( ).Be( HttpMethod.Get );
		result.StatusCode.Should( ).Be( HttpStatusCode.UnprocessableEntity );
		result.RawMessage.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Get_should_throw_exception_on_non_parsing_type_different( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-error-specified" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson( new {
						errorCode = 422,
						message = "Error on get value"
					} )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var action = async ( ) => await _restClient
			.DoGet( "get-error-specified" )
			.OnResult( config => config
				.UseTypeForSuccess<IEnumerable<string>>( ) )
			.OnError( error => error
				.ThrowFor( HttpStatusCode.UnprocessableEntity ) )
			.BuildAsync( );

		// Assert
		await action.Should( ).ThrowAsync<ParsingTypeException>( );
	}

	[Fact]
	public async Task Get_should_return_result_for_mapped_non_success_status_code( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-non-success" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson( new {
						errorCode = 422,
						message = "Error on get value"
					} )
					.WithStatusCode( HttpStatusCode.UnprocessableEntity ) );

		// Act
		var response = await _restClient
			.DoGet( "get-non-success" )
			.OnResult( config => config
				.UseTypeForSuccess<IEnumerable<Post>>( )
				.UseTypeFor<Error>( HttpStatusCode.UnprocessableEntity ) )
			.OnError( error => error
				.ThrowForNonSuccess( )
				.NotThrowFor( HttpStatusCode.UnprocessableEntity ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.UnprocessableEntity );
		response.Method.Should( ).Be( HttpMethod.Get );

		var result = response.GetAs<Error>( );

		result.Should( ).NotBeNull( );
		result.ErrorCode.Should( ).BeGreaterThanOrEqualTo( 0 );
		result.Message.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Get_should_return_result_for_all_types( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-success-all" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson( new {
						isSuccess = true,
						value = 200
					} )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var response = await _restClient
			.DoGet( "get-success-all" )
			.OnResult( config => config
				.UseTypeForAll<dynamic>( ) )
			.OnError( error => error
				.ThrowForNonSuccess( ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Get );

		var result = response.GetAs<dynamic>( );

		( ( bool )result.isSuccess ).Should( ).BeTrue( );
		( ( int )result.value ).Should( ).Be( 200 );
	}

	[Theory]
	[InlineData( HttpStatusCode.OK )]
	[InlineData( HttpStatusCode.Accepted )]
	[InlineData( HttpStatusCode.SeeOther )]
	public async Task Get_should_return_result_for_many_types( HttpStatusCode statusCode ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-success-many" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson( new {
						isSuccess = true,
						value = 200
					} )
					.WithStatusCode( statusCode ) );

		// Act
		var response = await _restClient
			.DoGet( "get-success-many" )
			.OnResult( config => config
				.UseTypeFor<dynamic>( [ HttpStatusCode.OK, HttpStatusCode.Accepted, HttpStatusCode.SeeOther ] ) )
			.OnError( error => error
				.ThrowForNonSuccess( )
				.NotThrowFor( HttpStatusCode.SeeOther ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).BeOneOf( HttpStatusCode.OK, HttpStatusCode.Accepted, HttpStatusCode.SeeOther );
		response.Method.Should( ).Be( HttpMethod.Get );

		var result = response.GetAs<dynamic>( );

		( ( bool )result.isSuccess ).Should( ).BeTrue( );
		( ( int )result.value ).Should( ).Be( 200 );
	}

	[Fact]
	public async Task Get_should_throw_for_success_when_mapped_for_throw( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-error-specified" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson( new {
						isSuccess = false,
						value = 400
					} )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var action = async ( ) => await _restClient
			.DoGet( "get-error-specified" )
			.OnError( error => error
				.ThrowFor( [ HttpStatusCode.OK ], ( x ) => ( ( bool )x.isSuccess ) == false ) )
		.BuildAsync( );

		// Assert
		var exceptionAssertion = await action.Should( ).ThrowAsync<NonSuccessException>( );

		var result = exceptionAssertion.Which;

		result.Method.Should( ).Be( HttpMethod.Get );
		result.StatusCode.Should( ).Be( HttpStatusCode.OK );
		result.RawMessage.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Get_should_throw_for_all_status_codes( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-error-specified" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson( new {
						isSuccess = false,
						value = 400
					} )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var action = async ( ) => await _restClient
			.DoGet( "get-error-specified" )
			.OnResult( resp => resp
				.UseTypeForAll<Error>( ( x ) => ( ( bool )x.isSuccess == true ) ) )
			.OnError( error => error
				.ThrowForAll( ( x ) => ( ( bool )x.isSuccess ) == false ) )
			.BuildAsync( );

		// Assert
		var exceptionAssertion = await action.Should( ).ThrowAsync<NonSuccessException>( );

		var result = exceptionAssertion.Which;

		result.Method.Should( ).Be( HttpMethod.Get );
		result.StatusCode.Should( ).Be( HttpStatusCode.OK );
		result.RawMessage.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Get_should_throw_for_non_mapped_status_codes( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-error-not-mapped" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson( new {
						isSuccess = false,
						value = 400
					} )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var action = async ( ) => await _restClient
			.DoGet( "get-error-not-mapped" )
			.OnResult( resp => resp
				.UseTypeForAll<Error>( ( x ) => ( ( bool )x.isSuccess == true ) ) )
			.BuildAsync( );

		// Assert
		var exceptionAssertion = await action.Should( ).ThrowAsync<NotMappedResultTypeException>( );
	}

	[Fact]
	public async Task Get_should_return_result_for_simple_request( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-success-simple" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson(
						new { id = _faker.UniqueIndex, title = _faker.Lorem.Lines( 1 ), body = _faker.Lorem.Text( ), userId = _faker.Random.Guid( ) }
					 )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var response = await _restClient
			.DoGet( "get-success-simple" )
			.BuildAsync<Post>( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Get );

		var result = response.GetAs<Post>( );

		result.Id.Should( ).BeGreaterThanOrEqualTo( 0 );
		result.Title.Should( ).NotBeEmpty( );
		result.Body.Should( ).NotBeEmpty( );
		result.UserId.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Get_should_return_result_for_bearer_authorized_endpoint( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-bearer-authorization" )
					.WithHeader( "Authorization", "*", MatchBehaviour.AcceptOnMatch )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson(
						new { id = _faker.UniqueIndex, title = _faker.Lorem.Lines( 1 ), body = _faker.Lorem.Text( ), userId = _faker.Random.Guid( ) }
					 )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var response = await _restClient
			.Configure( conf => conf
				.WithBearerAuthorization( _faker.Random.Word( ) ) )
			.DoGet( "get-bearer-authorization" )
			.OnResult( resp => resp
				.UseTypeForSuccess<Post>( ) )
			.BuildAsync<Post>( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Get );

		var result = response.GetAs<Post>( );

		result.Id.Should( ).BeGreaterThanOrEqualTo( 0 );
		result.Title.Should( ).NotBeEmpty( );
		result.Body.Should( ).NotBeEmpty( );
		result.UserId.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Get_should_return_result_for_basic_authorized_endpoint( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-basic-authorization" )
					.WithHeader( "Authorization", "*", MatchBehaviour.AcceptOnMatch )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson(
						new { id = _faker.UniqueIndex, title = _faker.Lorem.Lines( 1 ), body = _faker.Lorem.Text( ), userId = _faker.Random.Guid( ) }
					 )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var response = await _restClient
			.Configure( conf => conf
				.WithBasicAuthorization( _faker.Internet.UserName( ), _faker.Internet.Password( ) ) )
			.DoGet( "get-basic-authorization" )
			.OnResult( resp => resp
				.UseTypeForSuccess<Post>( ) )
			.BuildAsync<Post>( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Get );

		var result = response.GetAs<Post>( );

		result.Id.Should( ).BeGreaterThanOrEqualTo( 0 );
		result.Title.Should( ).NotBeEmpty( );
		result.Body.Should( ).NotBeEmpty( );
		result.UserId.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Get_should_return_result_using_more_header( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-headers" )
					.WithHeader( "X-API-Version", "*", MatchBehaviour.AcceptOnMatch )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson(
						new { id = _faker.UniqueIndex, title = _faker.Lorem.Lines( 1 ), body = _faker.Lorem.Text( ), userId = _faker.Random.Guid( ) }
					 )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var response = await _restClient
			.Configure( conf => conf
				.AddHeader( "X-API-Version", "1.0" )
				.AddHeader( "X-API-Version", "1.0" ) )
			.DoGet( "get-headers" )
			.OnResult( resp => resp
				.UseTypeForSuccess<Post>( ) )
			.BuildAsync<Post>( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Get );

		var result = response.GetAs<Post>( );

		result.Id.Should( ).BeGreaterThanOrEqualTo( 0 );
		result.Title.Should( ).NotBeEmpty( );
		result.Body.Should( ).NotBeEmpty( );
		result.UserId.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Get_should_return_result_using_timeout( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-timeout" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson(
						new { id = _faker.UniqueIndex, title = _faker.Lorem.Lines( 1 ), body = _faker.Lorem.Text( ), userId = _faker.Random.Guid( ) }
					 )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var response = await _restClient
			.Configure( conf => conf
				.WithTimeout( TimeSpan.FromSeconds( 60 ) ) )
			.DoGet( "get-timeout" )
			.OnResult( resp => resp
				.UseTypeForSuccess<Post>( ) )
			.BuildAsync<Post>( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Get );

		var result = response.GetAs<Post>( );

		result.Id.Should( ).BeGreaterThanOrEqualTo( 0 );
		result.Title.Should( ).NotBeEmpty( );
		result.Body.Should( ).NotBeEmpty( );
		result.UserId.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Get_should_return_result_using_http_client( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-http-client" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson(
						new { id = _faker.UniqueIndex, title = _faker.Lorem.Lines( 1 ), body = _faker.Lorem.Text( ), userId = _faker.Random.Guid( ) }
					 )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var response = await _restClient
			.Configure( conf => conf
				.WithClient( new HttpClient( ) ) )
			.DoGet( "get-http-client" )
			.OnResult( resp => resp
				.UseTypeForSuccess<Post>( ) )
			.BuildAsync<Post>( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Get );

		var result = response.GetAs<Post>( );

		result.Id.Should( ).BeGreaterThanOrEqualTo( 0 );
		result.Title.Should( ).NotBeEmpty( );
		result.Body.Should( ).NotBeEmpty( );
		result.UserId.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Get_should_return_result_using_snake_serialization( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-snake" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson(
						new { id = _faker.UniqueIndex, title = _faker.Lorem.Lines( 1 ), body = _faker.Lorem.Text( ), user_id = _faker.Random.Guid( ) }
					 )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var response = await _restClient
			.Configure( conf => conf
				.WithBodyDeserialization( CaseStrategy.SnakeCase ) )
			.DoGet( "get-snake" )
			.OnResult( resp => resp
				.UseTypeForSuccess<Post>( ) )
			.BuildAsync<Post>( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Get );

		var result = response.GetAs<Post>( );

		result.Id.Should( ).BeGreaterThanOrEqualTo( 0 );
		result.Title.Should( ).NotBeEmpty( );
		result.Body.Should( ).NotBeEmpty( );
		result.UserId.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Post_should_create_item( ) {
		// Arrange
		var request = new Post {
			Title = _faker.Lorem.Lines( 1 ),
			Body = _faker.Lorem.Text( ),
			UserId = _faker.Random.Guid( )
		};

		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/post-success" )
					.UsingPost( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson(
						new { id = _faker.UniqueIndex, title = request.Title, body = request.Body, userId = request.UserId }
					 )
					.WithStatusCode( HttpStatusCode.Created ) );

		// Act
		var response = await _restClient
			.DoPost( "post-success", request )
			.OnResult( config => config
				.UseTypeForSuccess<Post>( ) )
			.OnError( error => error
				.ThrowForNonSuccess( ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.Created );
		response.Method.Should( ).Be( HttpMethod.Post );
		response.IsSuccessStatusCode( ).Should( ).BeTrue( );

		var result = response.GetAs<Post>( );

		result.Should( ).NotBeNull( );
		result.Id.Should( ).BeGreaterThanOrEqualTo( 0 );
		result.Title.Should( ).NotBeEmpty( );
		result.Body.Should( ).NotBeEmpty( );
		result.UserId.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Put_should_update_item( ) {
		// Arrange
		var request = new Post {
			Title = _faker.Lorem.Lines( 1 ),
			Body = _faker.Lorem.Text( ),
			UserId = _faker.Random.Guid( )
		};

		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/put-success" )
					.UsingPut( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson(
						new { id = _faker.UniqueIndex, title = request.Title, body = request.Body, userId = request.UserId }
					 )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var response = await _restClient
			.DoPut( "put-success", request )
			.OnResult( config => config
				.UseTypeForSuccess<Post>( ) )
			.OnError( error => error
				.ThrowForNonSuccess( ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Put );
		response.IsSuccessStatusCode( ).Should( ).BeTrue( );

		var result = response.GetAs<Post>( );

		result.Should( ).NotBeNull( );
		result.Id.Should( ).BeGreaterThanOrEqualTo( 0 );
		result.Title.Should( ).NotBeEmpty( );
		result.Body.Should( ).NotBeEmpty( );
		result.UserId.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Patch_should_update_property_of_item( ) {
		// Arrange
		var request = new Post {
			Body = _faker.Lorem.Text( ),
		};

		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/patch-success" )
					.UsingPatch( ) )
			.RespondWith(
				Response
					.Create( )
					.WithBodyAsJson(
						new { id = _faker.UniqueIndex, title = _faker.Lorem.Lines( 1 ), body = request.Body, userId = _faker.Random.Guid( ) }
					 )
					.WithStatusCode( HttpStatusCode.OK ) );

		// Act
		var response = await _restClient
			.DoPatch( "patch-success", request )
			.OnResult( config => config
				.UseTypeForSuccess<Post>( ) )
			.OnError( error => error
				.ThrowForNonSuccess( ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Patch );
		response.IsSuccessStatusCode( ).Should( ).BeTrue( );

		var result = response.GetAs<Post>( );

		result.Should( ).NotBeNull( );
		result.Id.Should( ).BeGreaterThanOrEqualTo( 0 );
		result.Title.Should( ).NotBeEmpty( );
		result.Body.Should( ).NotBeEmpty( );
		result.UserId.Should( ).NotBeEmpty( );
	}

	[Fact]
	public async Task Delete_should_remove_item( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/delete-success" )
					.UsingDelete( ) )
			.RespondWith(
				Response
					.Create( )
					.WithStatusCode( HttpStatusCode.NoContent ) );

		// Act
		var response = await _restClient
			.DoDelete( "delete-success" )
			.OnResult( resp => resp
				.UseEmptyFor( HttpStatusCode.NoContent ) )
			.OnError( error => error
				.ThrowForNonSuccess( ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.NoContent );
		response.Method.Should( ).Be( HttpMethod.Delete );
		response.IsSuccessStatusCode( ).Should( ).BeTrue( );
	}

	[Fact]
	public async Task Retry_should_retry_before_error( ) {
		// Arrange
		_server
			.Given(
				Request
					.Create( )
					.WithPath( "/get-retry" )
					.UsingGet( ) )
			.RespondWith(
				Response
					.Create( )
					.WithStatusCode( HttpStatusCode.InternalServerError ) );

		// Act
		var response = await _restClient
			.DoGet( "get-retry" )
			.OnResult( resp => resp
				.UseEmptyFor( HttpStatusCode.InternalServerError ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.InternalServerError );
		response.Method.Should( ).Be( HttpMethod.Get );
		response.RetriesMade.Should( ).Be( 3 );
		response.IsSuccessStatusCode( ).Should( ).BeFalse( );
	}
}