using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Myth.Exceptions;
using Myth.Extensions;
using Myth.Interfaces;
using Myth.Rest.Test.Base;
using Myth.Testing.Mocks;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Rest.Test;

public class FileTests : BaseTests {
	private readonly IRestRequest _restDownloadClient;
	private readonly IRestRequest _restUploadClient;

	public FileTests( ) {
		_restDownloadClient = Rest
			.Create( )
			.Configure( conf => conf
				.WithBaseUrl( "https://localhost:4001" )
				.WithRetry( 3, TimeSpan.FromSeconds( 10 ) ) );

		_restUploadClient = Rest
			.Create( )
			.Configure( conf => conf
				.WithBaseUrl( "https://www.csm-testcenter.org" )
				.WithRetry( 3, TimeSpan.FromSeconds( 10 ) ) );
	}

	[Fact]
	public async Task Download_should_download_item( ) {
		// Arrange
		var directory = Environment.CurrentDirectory;
		var fileName = "Test1.txt";
		var path = Path.Combine( directory, fileName );

		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/download-success" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( "This is a test file!" ) );

		// Act
		var response = await _restDownloadClient
			.Configure( settings => settings.WithClient( client ) )
			.DoDownload( "download-success" )
			.OnError( error => error
				.ThrowForNonSuccess( ) )
			.BuildAsync( );

		response.Url.Should( ).NotBeNull( );
		response.Method.Should( ).Be( HttpMethod.Get );

		await response.SaveToFileAsync( directory, fileName, true );

		File.Exists( path ).Should( ).BeTrue( );

		File.Delete( path );
	}

	[Fact]
	public async Task Download_should_download_item_and_replace_the_existing( ) {
		// Arrange
		var directory = Environment.CurrentDirectory;
		var fileName = "Test6.txt";
		var path = Path.Combine( directory, fileName );

		File.Create( path ).Dispose( );

		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/download-success" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( "This is a test file!" ) );

		// Act
		var response = await _restDownloadClient
			.Configure( settings => settings.WithClient( client ) )
			.DoDownload( "download-success" )
			.OnError( error => error
				.ThrowForNonSuccess( ) )
			.BuildAsync( );

		response.Url.Should( ).NotBeNull( );
		response.Method.Should( ).Be( HttpMethod.Get );

		await response.SaveToFileAsync( directory, fileName, true );

		File.Exists( path ).Should( ).BeTrue( );

		File.Delete( path );
	}

	[Fact]
	public async Task Download_should_download_and_return_stream( ) {
		// Arrange
		var directory = Environment.CurrentDirectory;
		var fileName = "Test2.txt";
		var path = Path.Combine( directory, fileName );

		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/download-success" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( "This is a test file!" ) );

		// Act
		var response = await _restDownloadClient
			.Configure( settings => settings.WithClient( client ) )
			.DoDownload( "download-success" )
			.OnError( error => error
				.ThrowForNonSuccess( ) )
			.BuildAsync( );

		response.Url.Should( ).NotBeNull( );
		response.Method.Should( ).Be( HttpMethod.Get );

		var stream = response.ToStream( );

		stream.Should( ).NotBeNull( );
		stream.Should( ).BeSeekable( );
		stream.Should( ).BeReadable( );
	}

	[Fact]
	public async Task Download_should_throw_exception_when_file_already_exists( ) {
		// Arrange
		var directory = Environment.CurrentDirectory;
		var fileName = "Test3.txt";
		var path = Path.Combine( directory, fileName );

		File.Create( path ).Dispose( );

		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/download-success" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( "This is a test file!" ) );

		// Act
		var response = await _restDownloadClient
			.Configure( settings => settings.WithClient( client ) )
			.DoDownload( "download-success" )
			.OnError( error => error
				.ThrowForNonSuccess( ) )
			.BuildAsync( );

		response.Url.Should( ).NotBeNull( );
		response.Method.Should( ).Be( HttpMethod.Get );

		var action = async ( ) => await response.SaveToFileAsync( directory, fileName );

		await action.Should( ).ThrowAsync<FileAlreadyExsistsOnDownloadException>( );
	}

	[Fact]
	public async Task Download_should_throw_exception_when_non_status_code( ) {
		// Arrange
		var directory = Environment.CurrentDirectory;
		var fileName = "Test4.txt";
		var path = Path.Combine( directory, fileName );

		File.Create( path ).Dispose( );

		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/download-error" )
			.UsingGet( )
			.RespondWith( HttpStatusCode.NotFound )
			.WithJsonResponse( "This is a test file!" ) );

		// Act
		var action = async ( ) => await _restDownloadClient
			.Configure( settings => settings.WithClient( client ) )
			.DoDownload( "download-error" )
			.OnError( error => error
				.ThrowForNonSuccess( ) )
			.BuildAsync( );

		await action.Should( ).ThrowAsync<NonSuccessException>( );
	}

	[Fact]
	public async Task Upload_should_upload_a_file( ) {
		// Arrange

		// Mock file
		var content = "This is a test file";
		var fileName = "Test5.txt";
		var stream = new MemoryStream( );
		var writer = new StreamWriter( stream );
		writer.Write( content );
		writer.Flush( );
		stream.Position = 0;

		// Mock form file
		var file = new FormFile( stream, 0, stream.Length, "file", fileName ) {
			Headers = new HeaderDictionary( ),
			ContentType = "text/plain"
		};

		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/test" )
			.UsingPost( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( new { success = true } ) );

		// Act
		var response = await _restUploadClient
			.Configure( settings => settings.WithClient( client ) )
			.DoUpload( "test?do=test&subdo=file_upload", file )
			.OnError( error => error
				.ThrowForNonSuccess( ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Post );
		response.IsSuccessStatusCode( ).Should( ).BeTrue( );
	}

	[Fact]
	public async Task Upload_should_upload_a_file_if_he_is_a_content( ) {
		// Arrange

		// Mock file
		var content = "This is a test file";
		var fileName = "Test5.txt";
		var stream = new MemoryStream( );
		var writer = new StreamWriter( stream );
		writer.Write( content );
		writer.Flush( );
		stream.Position = 0;

		// Mock form file
		var file = new FormFile( stream, 0, stream.Length, "file", fileName ) {
			Headers = new HeaderDictionary( ),
			ContentType = "text/plain"
		};

		var client = HttpClientMock.CreateClient( settings => settings
			.ForRoute( "/test" )
			.UsingPost( )
			.RespondWith( HttpStatusCode.OK )
			.WithJsonResponse( new { success = true } ) );

		// Act
		var response = await _restUploadClient
			.Configure( settings => settings.WithClient( client ) )
			.DoUpload( "test?do=test&subdo=file_upload", file.ToMultiPartFormData( ) )
			.OnError( error => error
				.ThrowForNonSuccess( ) )
			.BuildAsync( );

		// Assert
		response.Should( ).NotBeNull( );
		response.StatusCode.Should( ).Be( HttpStatusCode.OK );
		response.Method.Should( ).Be( HttpMethod.Post );
		response.IsSuccessStatusCode( ).Should( ).BeTrue( );
	}
}