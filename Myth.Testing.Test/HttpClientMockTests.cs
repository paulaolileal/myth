using System.Net;
using System.Text;
using FluentAssertions;
using Myth.Extensions;
using Myth.Mocks;

namespace Myth.Testing.Test;

/// <summary>
/// Example tests demonstrating the use of HttpClientMock for testing HTTP-dependent services
/// </summary>
public class HttpClientMockTests : BaseTests {

	/// <summary>
	/// Example service that depends on an external HTTP API
	/// </summary>
	public class WeatherService( HttpClient httpClient ) {
		private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException( nameof( httpClient ) );

		/// <summary>
		/// Get weather data for a city
		/// </summary>
		/// <param name="city">The city name</param>
		/// <returns>Weather data</returns>
		public async Task<WeatherData?> GetWeatherAsync( string city ) {
			if ( string.IsNullOrWhiteSpace( city ) )
				throw new ArgumentException( "City name is required", nameof( city ) );

			var response = await _httpClient.GetAsync( $"/api/weather/{city}" );

			if ( response.StatusCode == HttpStatusCode.NotFound )
				return null;

			response.EnsureSuccessStatusCode( );

			var content = await response.Content.ReadAsStringAsync( );

			return content.FromJson<WeatherData>( );
		}

		/// <summary>
		/// Get weather data for multiple cities
		/// </summary>
		/// <param name="cities">List of city names</param>
		/// <returns>Weather data for all cities</returns>
		public async Task<List<WeatherData>> GetWeatherForCitiesAsync( IEnumerable<string> cities ) {
			var results = new List<WeatherData>( );

			foreach ( var city in cities ) {
				var weather = await GetWeatherAsync( city );
				if ( weather is not null )
					results.Add( weather );
			}

			return results;
		}

		/// <summary>
		/// Create a weather report
		/// </summary>
		/// <param name="report">The weather report data</param>
		/// <returns>The created report ID</returns>
		public async Task<string> CreateWeatherReportAsync( WeatherReport report ) {
			if ( report is null )
				throw new ArgumentNullException( nameof( report ) );

			var content = new StringContent( report.ToJson( ), Encoding.UTF8, "application/json" );
			var response = await _httpClient.PostAsync( "/api/reports", content );

			response.EnsureSuccessStatusCode( );

			var result = await response.Content.ReadAsStringAsync( );
			var createResult = result.FromJson<CreateReportResult>( )!;

			return createResult.Id;
		}
	}

	/// <summary>
	/// Test getting weather data with successful response
	/// </summary>
	[Fact]
	public async Task GetWeatherAsync_WithValidCity_ShouldReturnWeatherData( ) {
		// Arrange
		var expectedWeather = new WeatherData {
			City = "London",
			Temperature = 20.5,
			Humidity = 65,
			Description = "Partly cloudy"
		};

		var httpClient = HttpClientMock.CreateClient( config => config
			.ForRoute( "/api/weather/{city}" )
			.UsingGet( )
			.RespondWithSuccess( )
			.WithJsonResponse( expectedWeather )
		);

		var weatherService = new WeatherService( httpClient );

		// Act
		var result = await weatherService.GetWeatherAsync( "London" );

		// Assert
		result.Should( ).NotBeNull( );
		result!.City.Should( ).Be( "London" );
		result.Temperature.Should( ).Be( 20.5 );
		result.Humidity.Should( ).Be( 65 );
		result.Description.Should( ).Be( "Partly cloudy" );
	}

	/// <summary>
	/// Test getting weather data for non-existing city
	/// </summary>
	[Fact]
	public async Task GetWeatherAsync_WithNonExistingCity_ShouldReturnNull( ) {
		// Arrange
		var httpClient = HttpClientMock.CreateClient( config => config
			.ForRoute( "/api/weather/{city}" )
			.UsingGet( )
			.RespondWithNotFound( )
		);

		var weatherService = new WeatherService( httpClient );

		// Act
		var result = await weatherService.GetWeatherAsync( "NonExistingCity" );

		// Assert
		result.Should( ).BeNull( );
	}

	/// <summary>
	/// Test handling server errors
	/// </summary>
	[Fact]
	public async Task GetWeatherAsync_WithServerError_ShouldThrowException( ) {
		// Arrange
		var httpClient = HttpClientMock.CreateClient( config => config
			.ForRoute( "/api/weather/{city}" )
			.UsingGet( )
			.RespondWithServerError( )
		);

		var weatherService = new WeatherService( httpClient );

		// Act & Assert
		await Assert.ThrowsAsync<HttpRequestException>(
			( ) => weatherService.GetWeatherAsync( "London" )
		);
	}

	/// <summary>
	/// Test creating weather report
	/// </summary>
	[Fact]
	public async Task CreateWeatherReportAsync_WithValidReport_ShouldReturnReportId( ) {
		// Arrange
		var report = new WeatherReport {
			Title = "Daily Weather Report",
			Cities = new[ ] { "London", "Paris", "Berlin" },
			Date = DateTime.UtcNow
		};

		var expectedResponse = new CreateReportResult {
			Id = "report-123",
			Success = true
		};

		var httpClient = HttpClientMock.CreateClient( config => config
			.ForRoute( "/api/reports" )
			.UsingPost( )
			.RespondWith( HttpStatusCode.Created )
			.WithJsonResponse( expectedResponse )
		);

		var weatherService = new WeatherService( httpClient );

		// Act
		var result = await weatherService.CreateWeatherReportAsync( report );

		// Assert
		result.Should( ).Be( "report-123" );
	}

	/// <summary>
	/// Test with multiple endpoints
	/// </summary>
	[Fact]
	public async Task MultipleEndpoints_ShouldWorkCorrectly( ) {
		// Arrange
		var londonWeather = new WeatherData {
			City = "London",
			Temperature = 18.0,
			Humidity = 70,
			Description = "Rainy"
		};

		var parisWeather = new WeatherData {
			City = "Paris",
			Temperature = 22.0,
			Humidity = 60,
			Description = "Sunny"
		};

		var createResponse = new CreateReportResult {
			Id = "multi-report-456",
			Success = true
		};

		var httpClient = HttpClientMock.CreateClientWithEndpoints(
			config => config
				.ForRoute( "/api/weather/london" )
				.UsingGet( )
				.RespondWithSuccess( )
				.WithJsonResponse( londonWeather ),

			config => config
				.ForRoute( "/api/weather/paris" )
				.UsingGet( )
				.RespondWithSuccess( )
				.WithJsonResponse( parisWeather ),

			config => config
				.ForRoute( "/api/reports" )
				.UsingPost( )
				.RespondWith( HttpStatusCode.Created )
				.WithJsonResponse( createResponse )
		);

		var weatherService = new WeatherService( httpClient );

		// Act
		var londonResult = await weatherService.GetWeatherAsync( "london" );
		var parisResult = await weatherService.GetWeatherAsync( "paris" );

		var report = new WeatherReport {
			Title = "Multi-City Report",
			Cities = new[ ] { "London", "Paris" },
			Date = DateTime.UtcNow
		};
		var reportId = await weatherService.CreateWeatherReportAsync( report );

		// Assert
		londonResult.Should( ).NotBeNull( );
		londonResult!.City.Should( ).Be( "London" );
		londonResult.Temperature.Should( ).Be( 18.0 );

		parisResult.Should( ).NotBeNull( );
		parisResult!.City.Should( ).Be( "Paris" );
		parisResult.Temperature.Should( ).Be( 22.0 );

		reportId.Should( ).Be( "multi-report-456" );
	}

	/// <summary>
	/// Test error handling with invalid input
	/// </summary>
	[Fact]
	public async Task GetWeatherAsync_WithEmptyCity_ShouldThrowArgumentException( ) {
		// Arrange
		var httpClient = HttpClientMock.CreateClient( config => config
			.ForRoute( "/api/weather/{city}" )
			.UsingGet( )
			.RespondWithSuccess( )
		);

		var weatherService = new WeatherService( httpClient );

		// Act & Assert
		var exception = await Assert.ThrowsAsync<ArgumentException>(
			( ) => weatherService.GetWeatherAsync( string.Empty )
		);

		exception.Message.Should( ).Contain( "City name is required" );
	}

	/// <summary>
	/// Test with different HTTP methods
	/// </summary>
	[Fact]
	public async Task HttpMethods_ShouldWorkCorrectly( ) {
		// Arrange
		var httpClient = HttpClientMock.CreateClientWithEndpoints(
			// GET endpoint
			config => config
				.ForRoute( "/api/test" )
				.UsingGet( )
				.RespondWithSuccess( )
				.WithJsonResponse( new { Message = "GET success" } ),

			// POST endpoint
			config => config
				.ForRoute( "/api/test" )
				.UsingPost( )
				.RespondWith( HttpStatusCode.Created )
				.WithJsonResponse( new { Message = "POST success" } ),

			// PUT endpoint
			config => config
				.ForRoute( "/api/test" )
				.UsingPut( )
				.RespondWithSuccess( )
				.WithJsonResponse( new { Message = "PUT success" } ),

			// DELETE endpoint
			config => config
				.ForRoute( "/api/test" )
				.UsingDelete( )
				.RespondWith( HttpStatusCode.NoContent ),

			// PATCH endpoint
			config => config
				.ForRoute( "/api/test" )
				.UsingPatch( )
				.RespondWithSuccess( )
				.WithJsonResponse( new { Message = "PATCH success" } )
		);

		// Act & Assert
		var getResponse = await httpClient.GetAsync( "/api/test" );
		getResponse.StatusCode.Should( ).Be( HttpStatusCode.OK );

		var postResponse = await httpClient.PostAsync( "/api/test", new StringContent( "{}" ) );
		postResponse.StatusCode.Should( ).Be( HttpStatusCode.Created );

		var putResponse = await httpClient.PutAsync( "/api/test", new StringContent( "{}" ) );
		putResponse.StatusCode.Should( ).Be( HttpStatusCode.OK );

		var deleteResponse = await httpClient.DeleteAsync( "/api/test" );
		deleteResponse.StatusCode.Should( ).Be( HttpStatusCode.NoContent );

		var patchResponse = await httpClient.PatchAsync( "/api/test", new StringContent( "{}" ) );
		patchResponse.StatusCode.Should( ).Be( HttpStatusCode.OK );
	}
}

/// <summary>
/// Weather data model for testing
/// </summary>
public class WeatherData {
	public string City { get; set; } = string.Empty;
	public double Temperature { get; set; }
	public int Humidity { get; set; }
	public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Weather report model for testing
/// </summary>
public class WeatherReport {
	public string Title { get; set; } = string.Empty;
	public string[ ] Cities { get; set; } = Array.Empty<string>( );
	public DateTime Date { get; set; }
}

/// <summary>
/// Create report result model for testing
/// </summary>
public class CreateReportResult {
	public string Id { get; set; } = string.Empty;
	public bool Success { get; set; }
}
