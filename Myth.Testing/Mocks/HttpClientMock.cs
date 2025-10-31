using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using System;
using System.Net.Http;

namespace Myth.Rest.Test.Base {

	public class HttpClientMock {

		public static HttpClient Mock( Action<HttpClientSettings> settings ) {
			var mockSettings = new HttpClientSettings( );
			settings.Invoke( mockSettings );

			var builder = new WebHostBuilder( )
				.ConfigureServices( services => services.AddRouting( ) )
				.Configure( app => {
					app.UseRouting( );

					app.UseEndpoints( endpoints => {
						RequestDelegate handler = async context => {
							context.Response.ContentType = "application/json";
							context.Response.StatusCode = ( int )mockSettings.StatusCode;
							if ( mockSettings.Response is not null )
								await context.Response.WriteAsync( mockSettings.Response.ToJson( ) );
						};

						var route = mockSettings.Route.ToLowerInvariant( );
						var method = mockSettings.Method.Method.ToUpperInvariant( );

						switch ( mockSettings.Method.Method ) {
							case "GET":
							endpoints.MapGet( route, handler );
							break;

							case "POST":
							endpoints.MapPost( route, handler );
							break;

							case "PUT":
							endpoints.MapPut( route, handler );
							break;

							case "DELETE":
							endpoints.MapDelete( route, handler );
							break;

							case "PATCH":
							endpoints.MapPatch( route, handler );
							break;

							default:
							throw new NotSupportedException( $"HTTP method '{method}' is not supported." );
						}
					} );
				} );

			var testServer = new TestServer( builder );

			return testServer.CreateClient( );
		}
	}
}