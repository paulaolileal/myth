# Myth.Rest

![NuGet Version](https://img.shields.io/nuget/v/Myth.commons?style=for-the-badge&logo=nuget) ![NuGet Version](https://img.shields.io/nuget/vpre/Myth.commons?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)


It is a .NET library for co.

To use it is very simple. Just chain actions to build your request.

# ⭐ Features

- Simple use
- Chained actions
- Works with files
- Highly customizable
- Reusable for multiple requests
- Exception-oriented

# 🕶️ Using

This library is prepared to handle requests for text content or files. Each will be seen below.

## 📄 Requisições de conteúdo

A text content request is a request that sends and/or receives text files. It works with various formats.

To start a text content request, use:

```csharp
Rest.Create()
```

Example of a complete request:

```csharp
var response = await Rest
	.Create( )													// Initializes the request
  	.Configure( config => config								// Default configurations
		.WithBaseUrl( "https://localhost:5001/" )				// Sets the base URL
		.WithContentType( "application/json" )					// Sets the content type
		.WithBodySerialization( CaseStrategy.CamelCase )		// Sets the request body serialization type
		.WithBodyDeserialization( CaseStrategy.SnakeCase ) )	// Sets the response body serialization type
  	.DoGet( "get-success" )										// Defines the action to be performed `get`, `post`, `put`, `patch`, `delete`
  	.OnResult( config => config									// Defines what should happen in case of success
    	.UseTypeForSuccess<IEnumerable<Post>>( ) )				// ... in this case: whenever it's successful, status code >= 200 && < 299, use the type `IEnumerable<Post>`
  	.OnError( error => error									// Defines what should happen in case of error
		.ThrowForNonSuccess( ) )								// ... in this case: whenever it's not successful, throw an exception
  	.BuildAsync( );												// Executes the request
```

### ⚙️ Pre-configuring the request

The `.Configure(...)` is the entry point for request configuration. Many things can be defined to facilitate, see below for functionalities:

- `.WithBaseUrl(param: string)`: Receives the base of the URL to be requested. Example: https://test.com/testing. The base would be https://test.com/
- `.WithContentType(param: string)`: Receives the type of content to be received. Example: application/json
- `.WithBodySerialization(param: CaseStrategy)`: Defines how the json of your request body should be constructed. Example: CaseStrategy.CamelCase: { "myProp": "test" } or CaseStrategy.SnakeCase: { "my_prop": "test" }
- `.WithBodyDeserialization(param: CaseStrategy)`: Defines how the json of the response body should be read.
- `.WithTimeout(param: TimeSpan)`: Determines the maximum time to wait for a request.
- `.WithAuthorization(param: string, param: string)`: Adds a custom authorization header from a scheme and token.
- `.WithBearerAuthorization(param: string)`: Adds a Bearer type authorization header with the informed token.
- `.WithBasicAuthorization(param: string, param: string)`: Adds a Basic type authorization header from the informed user and password.
- `.AddHeader(param: string, param string, param: bool)`: Adds other necessary headers for the request from key and value.
- `.WithClient(param: HttpClient)`: Adds a previously configured http client.

### 🔮 Performing actions

All types of actions expected by REST can be performed.

- `.DoGet( param: string)`: Performs a GET on the informed route.
- `.DoDelete( param: string)`: Performs a DELETE on the informed route.
- `.DoPost<TBody>( param: string, param: TBody)`: Performs a POST on the informed route, sending the serialized body.
- `.DoPut<TBody>( param: string, param: TBody)`: Performs a PUT on the informed route, sending the serialized body.
- `.DoPatch<TBody>( param: string, param: TBody)`:  Performs a PATCH on the informed route, sending the serialized body.

### ✔️ Handling results

It is possible to handle typing for different status codes.

>  It is possible to use a condition to evaluate if this type should be used.

- `.UseTypeForSuccess<TResult>( param: Func<dynamic, bool>? )`: Uses a defined type for all success status codes.
- `.UseTypeFor( param: HttpStatusCode, param: Func<dynamic, bool>? )`: Uses a defined type for a specific status code.
- `.UseEmptyFor( param: HttpStatusCode, param: Func<dynamic, bool>? )`: Defines an empty body for a specific status code. Example: `204 NoContent`
- `.UseTypeFor<TResult( param: IEnumerable<HttpStatusCode>, param: Func<dynamic, bool>? )`: Uses a defined type for a list of status codes.
- `.UseTypeForAll<TResult>( param: Func<dynamic, bool>? )`: Sets the same type for all status codes.

### ❌ Handling errors

It is possible to define which status code should throw exceptions. The exception to be thrown in all cases where the error is expected is `NonSuccessException`.

>  It is also possible to use a condition to evaluate if this type should be used.

- `.ThrowForNonSuccess( param: Func<dynamic, bool>? )`: Throws the exception for all statuses that are not successful.
- `.ThrowFor( param: HttpStatusCode, param: Func<dynamic, bool>? )`: Throws an exception for the defined status code
- `.ThrowForAll( param: Func<dynamic, bool>? )`: Throws an exception for all status codes
- `.NotThrowForNonMappedResult()`: Does not throw an exception if there is no type for the received status code
- `.NotThrowFor( param: HttpStatusCode, param: Func<dynamic, bool>? )`: Does not throw an exception for a defined status code.

## 📁 File Requests

This library also allows and facilitates working with files, performing _download_ and _upload_.

To start a file request use:

```csharp
Rest.File()
```

### ⬇️ Performing downloads

To perform a download, simply use .DoDownload(param: string). All configurations and error handling remain the same as content requests. Here’s an example:

```csharp
var response = await Rest								
	.File( )													// Defines as a file request
	.Configure( conf => conf									// Pre-configures the request
		.WithBaseUrl( "https://localhost:5001" ) );				// Sets the base URL to be used
	.DoDownload( "download-success" )							// Sets the download action with the URL to be used
	.OnError( error => error									// Defines what to do in case of errors
		.ThrowForNonSuccess( ) )								// Always throw exceptions when an error occurs
	.BuildAsync( );												// Executes the request

await response.SaveToFileAsync( directory, fileName, true );	// Saves the downloaded file to a directory on the machine

response.ToStream();											// Returns a stream to be used later
```

### ⬆️ Performing uploads

Uploads follow the same pattern as downloads. The only change is the action to `.DoUpload(param: string, param: File)`.

```csharp
var response = await Rest
	.File( )													// Defines as a file request
	.Configure( conf => conf									// Pre-configures the request
		.WithBaseUrl( "https://localhost:5001" ) )				// Sets the base URL to be used
	.DoUpload( "upload-success", file )							// Sets the upload action with the URL to be used
	.OnError( error => error									// Defines what to do in case of errors
		.ThrowForNonSuccess( ) )								// Always throw exceptions when an error occurs
	.BuildAsync( );												// Executes the request
```

Uploads can use different actions, and for that, just follow the example:

```csharp
...
	.DoUpload("upload-success", file, settings => settings.UsePutAsMethod() )
...
```

You can use:
- `.UsePostAsMethod()`: Default
- `.UsePutAsMethod()`
- `.UsePatchAsMethod()`

# ⚡ Other use cases

## APIs that always return 200 OK

For those terrible cases, where an API always returns 200 OK and the response body will define whether it was really successful or not. We can do it as follows:

Considering a response of the following standard:

```json
{
	"code": 01,
	"success": true,
	"message": "This is a message"
}
```

The request must evaluate the `success` property to know if it was really an error. And for that, we do the following:

```csharp
var response = await Rest
	.Create( )													
  	.Configure( config => config								
		.WithBaseUrl( "https://localhost:5001/" )				
  	.DoGet( "route" )										
  	.OnResult( config => config								
    	.UseTypeFor<ResponseType>( 
			HttpStatusCode.OK, 
			body => body.success == true ) )				
  	.OnError( error => error								
		.ThrowFor( HttpStatusCode.OK, body => body.success == false )
		.ThrowForNonSuccess( ) )							
  	.BuildAsync( );			
```

Thus, if `success` is `true`, the response will be generated. If not, a `NonSuccessException` will be thrown.

## Building a repository

To reuse the same settings in multiple requests, it can be done as follows:

```csharp

public class Test{
	private readonly RestBuilder _client;

	public Test(){
		_client = Rest
			.Create()
			.Configure( conf => conf 
				.UseBaseUrl("https://localhost:5001")
				.WithContentType( "application/json" )					
				.WithBodySerialization( CaseStrategy.CamelCase )		
				.WithBodyDeserialization( CaseStrategy.SnakeCase ) );	
	}

	public async Task<ResponseType> GetTestAsync(CancellationToken cancellationToken){
		var response = await _client
			.DoGet("route")
			.OnResult( config => config									
				.UseTypeForSuccess<ResponseType>())			
			.OnError( error => error									
				.ThrowForNonSuccess( ) )								
			.BuildAsync( cancellationToken );	
				
		return response.GetAs<ResponseType>();
	}

	public async Task PostTestAsync(RequestType request, CancellationToken cancellationToken){
		await _client
			.DoPost("route", request)
			.OnResult( config => config									
				.UseEmptyFor(HttpStatusCode.NoContent))				
			.OnError( error => error									
				.ThrowForNonSuccess( ) )								
			.BuildAsync( cancellationToken );
	}
}
```