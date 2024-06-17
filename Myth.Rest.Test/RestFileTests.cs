using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Myth.Exceptions;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace Myth.Rest.Test {

	public class RestFileTests : IDisposable {
		private readonly RestFileBuilder _restDownloadClient;
		private readonly RestFileBuilder _restUploadClient;
		private readonly WireMockServer _server;
		private readonly Faker _faker;

		public RestFileTests( ) {
			_server = WireMockServer.Start( 4001, true );

			_faker = new Faker( "pt_BR" );

			_restDownloadClient = Rest
				.File( )
				.Configure( conf => conf
					.WithBaseUrl( "https://localhost:4001" ) );

			_restUploadClient = Rest
				.File( )
				.Configure( conf => conf
					.WithBaseUrl( "https://www.csm-testcenter.org" ) );
		}

		public void Dispose( ) {
			_server.Stop( );
			_server.Dispose( );
		}

		[Fact]
		public async Task Download_should_download_item( ) {
			// Arrange
			var directory = Environment.CurrentDirectory;
			var fileName = "Test1.txt";
			var path = Path.Combine( directory, fileName );

			_server
				.Given(
					Request
						.Create( )
						.WithPath( "/download-success" )
						.UsingGet( ) )
				.RespondWith(
					Response
						.Create( )
						.WithBody( "This is a test file!", BodyDestinationFormat.Bytes )
						.WithHeader( "ContentType", "plain/text" )
						.WithStatusCode( HttpStatusCode.OK ) );

			// Act
			var response = await _restDownloadClient
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

			_server
				.Given(
					Request
						.Create( )
						.WithPath( "/download-success" )
						.UsingGet( ) )
				.RespondWith(
					Response
						.Create( )
						.WithBody( "This is a test file!", BodyDestinationFormat.Bytes )
						.WithHeader( "ContentType", "plain/text" )
						.WithStatusCode( HttpStatusCode.OK ) );

			// Act
			var response = await _restDownloadClient
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

			_server
				.Given(
					Request
						.Create( )
						.WithPath( "/download-success" )
						.UsingGet( ) )
				.RespondWith(
					Response
						.Create( )
						.WithBody( "This is a test file!", BodyDestinationFormat.Bytes )
						.WithHeader( "ContentType", "plain/text" )
						.WithStatusCode( HttpStatusCode.OK ) );

			// Act
			var response = await _restDownloadClient
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

			_server
				.Given(
					Request
						.Create( )
						.WithPath( "/download-success" )
						.UsingGet( ) )
				.RespondWith(
					Response
						.Create( )
						.WithBody( "This is a test file!", BodyDestinationFormat.Bytes )
						.WithHeader( "ContentType", "plain/text" )
						.WithStatusCode( HttpStatusCode.OK ) );

			// Act
			var response = await _restDownloadClient
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

			_server
				.Given(
					Request
						.Create( )
						.WithPath( "/download-error" )
						.UsingGet( ) )
				.RespondWith(
					Response
						.Create( )
						.WithBody( "This is a test file!", BodyDestinationFormat.Bytes )
						.WithHeader( "ContentType", "plain/text" )
						.WithStatusCode( HttpStatusCode.NotFound ) );

			// Act
			var action = async ( ) => await _restDownloadClient
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

			// Act
			var response = await _restUploadClient
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
	}
}