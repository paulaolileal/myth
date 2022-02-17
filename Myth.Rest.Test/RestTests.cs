using Myth.Rest.Test.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Myth.Rest.Test {

    public class RestTests {
        private readonly RestBuilder _restClient;

        public RestTests( ) {
            _restClient = RestBuilder
                .Create( config => config
                    .WithBaseUrl( "https://jsonplaceholder.typicode.com/" )
                    .WithContentType( "application/json" ) );
        }

        [Fact]
        public async Task Get_should_return_list_of_itens( ) {
            // Act
            var response = await _restClient
                .DoGet( "posts" )
                    .When( config => config
                        .NonSuccessStatusCodeThrows( true )
                        .StatusIs<IEnumerable<Post>>( HttpStatusCode.OK ) )
                    .BuildResultAsync( );

            // Assert
            Assert.NotNull( response );
            var actual = ( IEnumerable<Post> ) response.Result!;
            Assert.True( actual.Any( ) );
            Assert.Equal( HttpStatusCode.OK, response.StatusCode );
            Assert.Equal( HttpMethod.Get, response.Method );
        }

        [Fact]
        public async Task Post_should_create_item( ) {
            // Arrange
            var body = new Post {
                Title = "foo",
                Body = "bar",
                UserId = 1
            };

            // Act
            var response = await _restClient
                .DoPost( "posts", body )
                    .When( config => config
                        .NonSuccessStatusCodeThrows( true )
                        .StatusIs<Post>( HttpStatusCode.Created ) )
                    .BuildResultAsync( );

            // Assert
            Assert.NotNull( response );
            var actual = ( Post ) response.Result!;
            Assert.Equal( 101, actual.Id );
            Assert.Equal( "foo", actual.Title );
            Assert.Equal( "bar", actual.Body );
            Assert.Equal( 1, actual.UserId );
            Assert.Equal( HttpStatusCode.Created, response.StatusCode );
            Assert.Equal( HttpMethod.Post, response.Method );
        }

        [Fact]
        public async Task Put_should_update_item( ) {
            // Arrange
            var body = new Post {
                Title = "foo",
                Body = "bar",
                UserId = 1
            };

            // Act
            var response = await _restClient
                .DoPut( "posts/1", body )
                    .When( config => config
                        .NonSuccessStatusCodeThrows( true )
                        .StatusIs<Post>( HttpStatusCode.OK ) )
                    .BuildResultAsync( );

            // Assert
            Assert.NotNull( response );
            var actual = ( Post ) response.Result!;
            Assert.Equal( 1, actual.Id );
            Assert.Equal( "foo", actual.Title );
            Assert.Equal( "bar", actual.Body );
            Assert.Equal( 1, actual.UserId );
            Assert.Equal( HttpStatusCode.OK, response.StatusCode );
            Assert.Equal( HttpMethod.Put, response.Method );
        }

        [Fact]
        public async Task Patch_should_update_property_of_item( ) {
            // Arrange
            var body = new Post {
                Title = "foo"
            };

            // Act
            var response = await _restClient
                .DoPatch( "posts/1", body )
                    .When( config => config
                        .NonSuccessStatusCodeThrows( true )
                        .StatusIs<Post>( HttpStatusCode.OK ) )
                    .BuildResultAsync( );

            // Assert
            Assert.NotNull( response );
            var actual = ( Post ) response.Result!;
            Assert.Equal( 0, actual.UserId );
            Assert.Equal( HttpStatusCode.OK, response.StatusCode );
            Assert.Equal( HttpMethod.Patch, response.Method );
        }

        [Fact]
        public async Task Delete_should_remove_item( ) {
            // Act
            var response = await _restClient
                .DoDelete( "posts/1" )
                    .When( config => config
                        .NonSuccessStatusCodeThrows( true ) )
                    .BuildResultAsync( );

            // Assert
            Assert.NotNull( response );
            Assert.Null( response.Result );
            Assert.Null( response.ResultType );
            Assert.Equal( HttpStatusCode.OK, response.StatusCode );
            Assert.Equal( HttpMethod.Delete, response.Method );
        }
    }
}