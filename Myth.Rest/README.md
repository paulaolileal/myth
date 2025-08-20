# Myth.Rest

[![NuGet Version](https://img.shields.io/nuget/v/Myth.Rest?style=for-the-badge&logo=nuget)](https://www.nuget.org/packages/Myth.Rest/) [![NuGet Version](https://img.shields.io/nuget/vpre/Myth.Rest?style=for-the-badge&logo=nuget&color=rgb(255%2C%20185%2C%200))](https://www.nuget.org/packages/Myth.Rest/absoluteLatest)

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg?style=for-the-badge)](https://opensource.org/licenses/Apache-2.0)

[![pt-br](https://img.shields.io/badge/lang-pt--br-green.svg?style=for-the-badge)](/README.pt-br.md) [![en](https://img.shields.io/badge/lang-en-red.svg?style=for-the-badge)](/README.md)

It is a .NET library for consuming REST APIs as a client. The main goal is to simplify consumption and enable working with RESTFUL.

To use it is very simple. Just chain actions to build your request.

# ⭐ Features

- Simple use
- Chained actions
- Works with files
- Highly customizable
- Reusable for multiple requests
- Exception-oriented
- **Advanced retry policies with multiple strategies**

# 🕶️ Using

This library is prepared to handle requests for text content or files. Each will be seen below.

## 📄 Requests

To start a text content request, use:

```csharp
Rest.Create()
```

Example of a complete request:

```csharp
var response = await Rest
	.Create( )							// Initializes the request
  	.Configure( config => config					// Default configurations
		.WithBaseUrl( "https://localhost:5001/" )		// Sets the base URL
		.WithContentType( "application/json" )			// Sets the content type
		.WithBodySerialization( CaseStrategy.CamelCase )	// Sets the request body serialization type
		.WithBodyDeserialization( CaseStrategy.SnakeCase )	// Sets the response body serialization type
		.WithRetry( )						// Defines intelligent retry policy (recommended)
		.WithTypeConverter<Interface, Type>			// Defines a conversion between the interface and the concrete type on deserialize responses
  	.DoGet( "get-success" )						// Defines the action to be performed `get`, `post`, `put`, `patch`, `delete`
  	.OnResult( config => config					// Defines what should happen in case of success
    	.UseTypeForSuccess<IEnumerable<Post>>( ) )			// ... in this case: whenever it's successful, status code >= 200 && < 299, use the type `IEnumerable<Post>`
  	.OnError( error => error					// Defines what should happen in case of error
		.ThrowForNonSuccess( ) )                                // Always throw exceptions when an error occurs
  	.BuildAsync( );							// Executes the request
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
- `.WithRetry( )` or `.WithRetry( retry => retry... )`: Defines retry policies for failed requests (see detailed section below)
- `.WithTypeConverter<Interface, Type>`: Defines a conversion between the interface and the concrete type on deserialize responses

#### 🔄 Retry Policies

The library offers sophisticated retry mechanisms following industry standards like AWS SDK, Google Cloud SDK, and Polly:

##### Smart Default (Recommended)
```csharp
.WithRetry() // 3 attempts, exponential backoff with jitter, server errors only
```

##### Custom Retry Strategies
```csharp
.WithRetry(retry => retry
	.WithMaxAttempts(5)
	.UseExponentialBackoffWithJitter(
		baseDelay: TimeSpan.FromSeconds(1),
		multiplier: 2.0,
		maxDelay: TimeSpan.FromSeconds(30)
	)
	.ForServerErrors()
	.ForExceptions(typeof(TaskCanceledException))
)
```

##### Available Strategies:

**🎯 Exponential Backoff with Jitter** (Recommended)
- **When to use**: Most production scenarios
- **How it works**: Delays increase exponentially (1s, 2s, 4s...) with random jitter to prevent thundering herd
- **Used by**: AWS SDK, Google Cloud SDK

```csharp
.UseExponentialBackoffWithJitter(
	baseDelay: TimeSpan.FromSeconds(1),
	multiplier: 2.0,
	maxDelay: TimeSpan.FromSeconds(30)
)
```

**📈 Exponential Backoff**
- **When to use**: When you want predictable delays without randomness
- **How it works**: Delays increase exponentially (1s, 2s, 4s, 8s...)

```csharp
.UseExponentialBackoff(
	baseDelay: TimeSpan.FromSeconds(1),
	multiplier: 2.0,
	maxDelay: TimeSpan.FromSeconds(30)
)
```

**🎲 Random Delay**
- **When to use**: High-traffic scenarios where you want to spread load randomly
- **How it works**: Each retry uses a random delay between min and max values

```csharp
.UseRandom(
	minDelay: TimeSpan.FromSeconds(1),
	maxDelay: TimeSpan.FromSeconds(5)
)
```

**⏱️ Fixed Delay**
- **When to use**: Simple scenarios or when you need predictable timing
- **How it works**: Same delay between all retries

```csharp
.UseFixedDelay(TimeSpan.FromSeconds(2))
```

##### Configuration Options:
- `.WithMaxAttempts(n)`: Maximum retry attempts
- `.ForServerErrors()`: Retry for 5xx status codes and 429 Too Many Requests
- `.ForStatusCodes(...)`: Retry for specific HTTP status codes
- `.ForExceptions(...)`: Retry for specific exception types

##### Backward Compatibility:
```csharp
.WithRetry(3, TimeSpan.FromSeconds(2), HttpStatusCode.ServiceUnavailable) // Still works
```

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
- `.UseFallback<T>(param: HttpStatusCode, param: T)`: Uses a fallback value for a specific status code. This is useful when you want to return a default value instead of throwing an exception.

### ⬇️ Performing downloads

To perform a download, simply use .DoDownload(param: string). All configurations and error handling remain the same as content requests. Here's an example:

```csharp
var response = await Rest								
	.Create( )													// Defines as a file request
	.Configure( conf => conf					// Pre-configures the request
		.WithBaseUrl( "https://localhost:5001" )		// Sets the base URL to be used
		.WithRetry( ) )						// Smart retry for downloads
	.DoDownload( "download-success" )				// Sets the download action with the URL to be used
	.OnError( error => error					// Defines what to do in case of errors
		.ThrowForNonSuccess( ) )				// Always throw exceptions when an error occurs
	.BuildAsync( );							// Executes the request

await response.SaveToFileAsync( directory, fileName, true );	        // Saves the downloaded file to a directory on the machine

response.ToStream();							// Returns a stream to be used later
```

### ⬆️ Performing uploads

Uploads follow the same pattern as downloads. The only change is the action to `.DoUpload(param: string, param: File)`.

```csharp
var response = await Rest
	.Create( )													// Defines as a file request
	.Configure( conf => conf			                // Pre-configures the request
		.WithBaseUrl( "https://localhost:5001" )	        // Sets the base URL to be used
		.WithRetry( retry => retry			        // Custom retry for uploads
			.WithMaxAttempts(2)				// Fewer retries for uploads
			.UseFixedDelay(TimeSpan.FromSeconds(5))		// Longer delays for large files
		) )
	.DoUpload( "upload-success", file )			        // Sets the upload action with the URL to be used
	.OnError( error => error					// Defines what to do in case of errors
		.ThrowForNonSuccess( ) )                                // Always throw exceptions when an error occurs
	.BuildAsync( );                                                 // Executes the request
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
		.WithRetry( retry => retry								// Even with 200 OK, retry on business logic errors
			.WithMaxAttempts(2)
			.UseFixedDelay(TimeSpan.FromSeconds(1))
		) )				
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
				.WithBodyDeserialization( CaseStrategy.SnakeCase )
				.WithRetry( retry => retry							// Centralized retry policy
					.WithMaxAttempts(3)
					.UseExponentialBackoffWithJitter(TimeSpan.FromSeconds(1))
					.ForServerErrors()
				) );	
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

## Advanced Retry Scenarios

### E-commerce API with Different Strategies
```csharp
// Critical operations - Conservative retry
var orderClient = Rest
	.Create()
	.Configure(config => config
		.WithBaseUrl("https://api.shop.com")
		.WithRetry(retry => retry
			.WithMaxAttempts(2)
			.UseExponentialBackoff(TimeSpan.FromSeconds(2))
			.ForStatusCodes(HttpStatusCode.ServiceUnavailable, HttpStatusCode.TooManyRequests)
		)
	);

// Read operations - Aggressive retry
var catalogClient = Rest.Create()
	.Configure(config => config
		.WithBaseUrl("https://api.shop.com")
		.WithRetry(retry => retry
			.WithMaxAttempts(5)
			.UseRandom(TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(3))
			.ForServerErrors()
			.ForExceptions(typeof(TaskCanceledException))
		)
	);
```