using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Myth.Constants;
using Myth.Exceptions;
using Myth.Rest.Test.Base;
using Myth.Rest.Test.Models;
using Myth.Testing.Mocks;
using Xunit;

namespace Myth.Rest.Test;

public class ContentTests : BaseTests {
	private readonly Faker<Post> _postFaker;

	public ContentTests( ) {
		_postFaker = new Faker<Post>( )
			.RuleFor( prop => prop.Id, r => r.UniqueIndex )
			.RuleFor( prop => prop.Title, r => r.Lorem.Lines( 1 ) )
			.RuleFor( prop => prop.Body, r => r.Lorem.Text( ) )
			.RuleFor( prop => prop.UserId, r => r.Random.Guid( ) );
	}

	[Fact]
	public async Task Get_should_be_success_if_status_code_is_success_and_body_doesnt_care( ) {
		// Arrange
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-success" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( _postFaker.Generate( 5 ) ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-success" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( _postFaker.Generate( 5 ) ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-non-success" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.BadRequest )
			.WithJsonResponse( _postFaker.Generate( 5 ) ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-error" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.UnprocessableEntity )
			.WithJsonResponse( _errorFaker.Generate( ) ) );

		// Act
		var action = async ( ) => await _restClient
			.Configure( settings => settings.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-success" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( _postFaker.Generate( 5 ) ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
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
	public async Task Get_should_throw_exception_on_non_success_specified_status_code( ) {
		// Arrange
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-error-specified" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.UnprocessableEntity )
			.WithJsonResponse( _errorFaker.Generate( ) ) );

		// Act
		var action = async ( ) => await _restClient
			.Configure( settings => settings.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-error-specified" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( _errorFaker.Generate( ) ) );

		// Act
		var action = async ( ) => await _restClient
			.Configure( settings => settings.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-non-success" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.UnprocessableEntity )
			.WithJsonResponse( _errorFaker.Generate( ) ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-success-all" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( new {
				isSuccess = true,
				value = 200
			} ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-success-many" )
			.UsingGet( )
			.RespondWith( statusCode )
			.WithJsonResponse( new {
				isSuccess = true,
				value = 200
			} ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-error-specified" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( new {
				isSuccess = false,
				value = 400
			} ) );

		// Act
		var action = async ( ) => await _restClient
			.Configure( settings => settings.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-error-specified" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( new {
				isSuccess = false,
				value = 400
			} ) );

		// Act
		var action = async ( ) => await _restClient
			.Configure( settings => settings.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-error-not-mapped" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( new {
				isSuccess = false,
				value = 400
			} ) );

		// Act
		var action = async ( ) => await _restClient
			.Configure( settings => settings.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-success-simple" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( _postFaker.Generate( ) ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-bearer-authorization" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( _postFaker.Generate( ) ) );

		// Act
		var response = await _restClient
			.Configure( conf => conf
				.WithBearerAuthorization( _faker.Random.Word( ) )
				.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-basic-authorization" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( _postFaker.Generate( ) ) );

		// Act
		var response = await _restClient
			.Configure( conf => conf
				.WithBasicAuthorization( _faker.Internet.UserName( ), _faker.Internet.Password( ) )
				.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-headers" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( _postFaker.Generate( ) ) );

		// Act
		var response = await _restClient
			.Configure( conf => conf
				.WithHeader( "X-API-Version", "1.0" )
				.WithHeader( "X-API-Version", "1.0" )
				.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-timeout" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( _postFaker.Generate( ) ) );

		// Act
		var response = await _restClient
			.Configure( conf => conf
				.WithTimeout( TimeSpan.FromSeconds( 60 ) )
				.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-http-client" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( _postFaker.Generate( ) ) );

		// Act
		var response = await _restClient
			.Configure( conf => conf
				.WithClient( client ) )
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
		var snakePost = _postFaker.Generate( );
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-snake" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( new { id = snakePost.Id, title = snakePost.Title, body = snakePost.Body, user_id = snakePost.UserId } ) );

		// Act
		var response = await _restClient
			.Configure( conf => conf
				.WithBodyDeserialization( CaseStrategy.SnakeCase )
				.WithClient( client ) )
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

		var responseBody = request.Copy( );
		responseBody.Id = _faker.Random.Number( );

		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/post-success" )
			.UsingPost( )
			.RespondWith( HttpStatusCode.Created )
			.WithJsonResponse( responseBody ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
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

		var responseBody = request.Copy( );
		responseBody.Id = _faker.Random.Number( );

		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/put-success" )
			.UsingPut( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( responseBody ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
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

		var responsePost = _postFaker.Generate( );
		responsePost.Body = request.Body;

		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/patch-success" )
			.UsingPatch( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( responsePost ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/delete-success" )
			.UsingDelete( )
			.RespondWith( HttpStatusCode.NoContent ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
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
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-retry" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.InternalServerError ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
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

	[Fact]
	public async Task When_map_interface_to_concrete_the_result_should_be_mapped( ) {
		// Arrange
		var mockedPost = _postFaker.Generate( );

		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/get-interface" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( mockedPost ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
			.DoGet( "get-interface" )
			.OnResult( resp => resp
				.UseTypeForSuccess<IPost>( ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Get );
		response.IsSuccessStatusCode( ).Should( ).BeTrue( );

		var result = response.GetAs<IPost>( );
		result.Id.Should( ).Be( mockedPost.Id );
		result.Title.Should( ).Be( mockedPost.Title );
		result.Body.Should( ).Be( mockedPost.Body );
		result.UserId.Should( ).Be( mockedPost.UserId );
	}

	[Fact]
	public async Task Fallback_value_should_should_be_used_in_case_of_errors( ) {
		// Arrange
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/fallback" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.InternalServerError ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
			.DoGet( "fallback" )
			.OnResult( resp => resp
				.UseTypeForSuccess<Post>( ) )
			.OnError( error => error
				.UseFallback(
					HttpStatusCode.OK,
					new Post {
						Id = -1,
						Title = "Fallback Title",
						Body = "Fallback Body",
						UserId = Guid.Empty
					} ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Get );
		response.FallbackUsed.Should( ).BeTrue( );
		response.IsSuccessStatusCode( ).Should( ).BeTrue( );
	}

	[Fact]
	public async Task Not_throw_for_non_mapped_should_continue_map_if_the_response_is_mapped( ) {
		// Arrange
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/notmapped" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( _postFaker.Generate( ) ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
			.DoGet( "notmapped" )
			.OnResult( resp => resp
				.UseTypeForSuccess<Post>( ) )
			.OnError( error => error
				.NotThrowForNonMappedResult( ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.ResultType.Should( ).NotBeNull( );
		response.Result.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Get );
		response.IsSuccessStatusCode( ).Should( ).BeTrue( );
	}

	[Fact]
	public async Task Not_throw_for_non_mapped_should_continue_map_if_the_response_is_not_mapped( ) {
		// Arrange
		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/notmapped" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.InternalServerError )
			.WithJsonResponse( new {
				message = "Error"
			} ) );

		// Act
		var response = await _restClient
			.Configure( settings => settings.WithClient( client ) )
			.DoGet( "notmapped" )
			.OnResult( resp => resp
				.UseTypeForSuccess<Post>( ) )
			.OnError( error => error
				.NotThrowForNonMappedResult( ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.InternalServerError );
		response.Method.Should( ).Be( HttpMethod.Get );
		response.IsSuccessStatusCode( ).Should( ).BeFalse( );
	}
}
