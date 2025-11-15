using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Morph.Test.Models.Dtos;
using Myth.Morph.Test.Models.Entities;
using Myth.Morph.Test.Services;
using Myth.ServiceProvider;

namespace Myth.Morph.Test;

public class IMorphableFromTests {
	private readonly IServiceProvider _serviceProvider;

	public IMorphableFromTests( ) {
		// Configure MythServiceProvider for tests with localization service
		MythServiceProvider.Reset( );
		var services = new ServiceCollection( );
		services.AddMorph( );
		services.AddScoped<ILocalizationService, LocalizationService>( );
		var serviceProvider = services.BuildServiceProvider( );
		MythServiceProvider.Initialize( serviceProvider );
		_serviceProvider = serviceProvider;
	}

	[Fact]
	public void WeatherForecast_To_WeatherForecastDto_Should_MapCorrectly( ) {
		// Arrange
		var forecast = new WeatherForecast {
			Date = new DateTime( 2024, 6, 15 ),
			TemperatureC = 25,
			Summary = Summary.Warm,
			Location = "New York",
			Humidity = 65.5,
			Pressure = 1013
		};

		// Act
		var dto = forecast.To<WeatherForecastDto>( _serviceProvider );

		// Assert
		dto.Should( ).NotBeNull( );
		dto.Date.Should( ).Be( forecast.Date );
		dto.SummaryDescription.Should( ).Be( "Warm" );
		dto.SummaryId.Should( ).Be( ( int )Summary.Warm );
		dto.Location.Should( ).Be( "New York" );
		dto.TemperatureCategory.Should( ).Be( "Warm" );
	}

	[Fact]
	public void WeatherForecast_To_WeatherForecastDto_Should_HandleEnumCorrectly( ) {
		// Arrange
		var scenarios = new[ ] {
			new { Summary = Summary.Freezing, Expected = "Freezing" },
			new { Summary = Summary.Scorching, Expected = "Scorching" },
			new { Summary = Summary.Cool, Expected = "Cool" }
		};

		foreach ( var scenario in scenarios ) {
			var forecast = new WeatherForecast {
				Date = DateTime.Now,
				TemperatureC = 20,
				Summary = scenario.Summary,
				Location = "Test"
			};

			// Act
			var dto = forecast.To<WeatherForecastDto>( _serviceProvider );

			// Assert
			dto.SummaryDescription.Should( ).Be( scenario.Expected );
			dto.SummaryId.Should( ).Be( ( int )scenario.Summary );
		}
	}

	[Fact]
	public void WeatherForecast_To_WeatherForecastDto_Should_CategorizeTemperatureCorrectly( ) {
		// Arrange
		var scenarios = new[ ] {
			new { TempC = -5, Expected = "Freezing" },
			new { TempC = 5, Expected = "Cold" },
			new { TempC = 15, Expected = "Cool" },
			new { TempC = 25, Expected = "Warm" },
			new { TempC = 35, Expected = "Hot" }
		};

		foreach ( var scenario in scenarios ) {
			var forecast = new WeatherForecast {
				Date = DateTime.Now,
				TemperatureC = scenario.TempC,
				Summary = Summary.Cool,
				Location = "Test"
			};

			// Act
			var dto = forecast.To<WeatherForecastDto>( _serviceProvider );

			// Assert
			dto.TemperatureCategory.Should( ).Be( scenario.Expected,
				$"because {scenario.TempC}°C should be categorized as {scenario.Expected}" );
		}
	}

	[Fact]
	public void Product_To_ProductSummaryDto_Should_MapComplexExpressionsCorrectly( ) {
		// Arrange
		var product = new Product {
			Id = 123,
			Name = "Test Product",
			Price = 99.99m,
			CreatedAt = DateTime.Now.AddDays( -30 ),
			IsActive = true,
			Category = "Electronics & Gadgets",
			Stock = 5,
			Weight = 2.5m
		};

		// Act
		var dto = product.To<ProductSummaryDto>( _serviceProvider );

		// Assert
		dto.Should( ).NotBeNull( );
		dto.DisplayName.Should( ).Be( "Test Product (123)" );
		dto.FormattedPrice.Should( ).Be( "$99.99" );
		dto.Status.Should( ).Be( "Active" );
		dto.DaysOld.Should( ).Be( 30 );
		dto.StockLevel.Should( ).Be( "Low Stock" );
		dto.CategoryCode.Should( ).Be( "ELECTRONICS_&_GADGETS" );
	}

	[Fact]
	public void Product_To_ProductSummaryDto_Should_HandleStockLevelsCorrectly( ) {
		// Arrange
		var scenarios = new[ ] {
			new { Stock = 0, Expected = "Out of Stock" },
			new { Stock = 5, Expected = "Low Stock" },
			new { Stock = 25, Expected = "Medium Stock" },
			new { Stock = 100, Expected = "High Stock" }
		};

		foreach ( var scenario in scenarios ) {
			var product = new Product {
				Id = 1,
				Name = "Test",
				Price = 10m,
				CreatedAt = DateTime.Now,
				IsActive = true,
				Category = "Test",
				Stock = scenario.Stock
			};

			// Act
			var dto = product.To<ProductSummaryDto>( _serviceProvider );

			// Assert
			dto.StockLevel.Should( ).Be( scenario.Expected,
				$"because stock level {scenario.Stock} should be categorized as {scenario.Expected}" );
		}
	}

	[Fact]
	public void User_To_UserProfileDto_Should_MapComplexCalculationsCorrectly( ) {
		// Arrange
		var user = new User {
			Id = 42,
			FirstName = "John",
			LastName = "Doe",
			Email = "john.doe@example.com",
			BirthDate = new DateTime( 1990, 5, 15 ),
			CountryCode = "US",
			IsEmailVerified = true,
			LastLoginAt = DateTime.Now.AddDays( -2 )
		};

		// Act
		var dto = user.To<UserProfileDto>( _serviceProvider );

		// Assert
		dto.Should( ).NotBeNull( );
		dto.FullName.Should( ).Be( "John Doe" );
		dto.EmailStatus.Should( ).Be( "Verified" );
		dto.Age.Should( ).Be( DateTime.Now.Year - 1990 );
		dto.Country.Should( ).Be( "US" );
		dto.ActivityStatus.Should( ).Be( "Active" );
		dto.InitialsAvatar.Should( ).Be( "JD" );
	}

	[Fact]
	public void User_To_UserProfileDto_Should_HandleEdgeCasesCorrectly( ) {
		// Arrange
		var user = new User {
			Id = 1,
			FirstName = "",
			LastName = "",
			Email = "test@example.com",
			BirthDate = new DateTime( 2000, 1, 1 ),
			CountryCode = "br",
			IsEmailVerified = false,
			LastLoginAt = DateTime.Now.AddDays( -10 )
		};

		// Act
		var dto = user.To<UserProfileDto>( _serviceProvider );

		// Assert
		dto.FullName.Should( ).Be( " " ); // FirstName + " " + LastName
		dto.EmailStatus.Should( ).Be( "Pending" );
		dto.Country.Should( ).Be( "BR" ); // Should be uppercased
		dto.ActivityStatus.Should( ).Be( "Inactive" ); // More than 7 days
		dto.InitialsAvatar.Should( ).Be( "UU" ); // Default when names are empty
	}

	[Fact]
	public void IMorphableFrom_Should_WorkWithNullValues( ) {
		// Arrange
		var product = new Product {
			Id = 1,
			Name = null!, // Simulating null value
			Price = 0,
			CreatedAt = DateTime.MinValue,
			IsActive = false,
			Category = null!,
			Stock = 0
		};

		// Act & Assert - Should not throw exception
		var dto = product.To<ProductSummaryDto>( _serviceProvider );
		dto.Should( ).NotBeNull( );
		dto.DisplayName.Should( ).Be( " (1)" ); // Handles null name
		dto.FormattedPrice.Should( ).Be( "$0.00" );
		dto.Status.Should( ).Be( "Inactive" );
		dto.StockLevel.Should( ).Be( "Out of Stock" );
	}

	[Fact]
	public void IMorphableFrom_Should_PreserveUnmappedProperties( ) {
		// Arrange
		var forecast = new WeatherForecast {
			Date = new DateTime( 2024, 6, 15 ),
			TemperatureC = 25,
			Summary = Summary.Warm,
			Location = "Test Location",
			Humidity = 65.5,
			Pressure = 1013
		};

		// Act
		var dto = forecast.To<WeatherForecastDto>( _serviceProvider );

		// Assert
		// Properties that are auto-mapped (same name) should work
		dto.Date.Should( ).Be( forecast.Date );
		dto.Location.Should( ).Be( forecast.Location );

		// Properties mapped via custom expressions should work
		dto.SummaryDescription.Should( ).Be( "Warm" );
		dto.SummaryId.Should( ).Be( ( int )Summary.Warm );
		dto.TemperatureCategory.Should( ).Be( "Warm" );
	}

	[Theory]
	[InlineData( Summary.Freezing, 0 )]
	[InlineData( Summary.Warm, 5 )]
	[InlineData( Summary.Scorching, 9 )]
	public void IMorphableFrom_Should_HandleAllEnumValues( Summary summary, int expectedId ) {
		// Arrange
		var forecast = new WeatherForecast {
			Date = DateTime.Now,
			TemperatureC = 20,
			Summary = summary,
			Location = "Test"
		};

		// Act
		var dto = forecast.To<WeatherForecastDto>( _serviceProvider );

		// Assert
		dto.SummaryId.Should( ).Be( expectedId );
		dto.SummaryDescription.Should( ).Be( summary.ToString( ) );
	}

	[Fact]
	public void IMorphableFrom_Should_UseServiceProviderForLocalization( ) {
		// Arrange
		var user = new User {
			Id = 1,
			FirstName = "John",
			LastName = "Doe",
			Email = "john.doe@example.com",
			BirthDate = new DateTime( 1990, 5, 15 ),
			CountryCode = "US",
			IsEmailVerified = true,
			LastLoginAt = DateTime.Now.AddDays( -2 )
		};

		// Act
		var dto = user.To<UserProfileDto>( _serviceProvider );

		// Assert - Basic properties should work
		dto.FullName.Should( ).Be( "John Doe" );
		dto.EmailStatus.Should( ).Be( "Verified" );
		dto.Country.Should( ).Be( "US" );
		dto.ActivityStatus.Should( ).Be( "Active" );

		// Service provider dependent properties should be populated
		dto.LocalizedEmailStatus.Should( ).Be( "Email Verified ✓" );
		dto.LocalizedActivityStatus.Should( ).Be( "User Active 🟢" );
		dto.CountryFullName.Should( ).Be( "United States" );
	}

	[Fact]
	public void IMorphableFrom_Should_WorkWithMythServiceProviderWithoutLocalizationService( ) {
		// Arrange - Save current MythServiceProvider state
		var originalServiceProvider = MythServiceProvider.Current;

		try {
			// Reset MythServiceProvider and configure without localization service
			MythServiceProvider.Reset( );
			var minimalServices = new ServiceCollection( );
			minimalServices.AddMorph( );
			// Note: Not adding ILocalizationService
			MythServiceProvider.Initialize( minimalServices.BuildServiceProvider( ) );

			var user = new User {
				Id = 1,
				FirstName = "Jane",
				LastName = "Smith",
				Email = "jane@example.com",
				BirthDate = new DateTime( 1985, 3, 10 ),
				CountryCode = "ca",
				IsEmailVerified = false,
				LastLoginAt = DateTime.Now.AddDays( -10 )
			};

			// Act - Transform using MythServiceProvider without localization service
			var dto = user.To<UserProfileDto>( );

			// Assert - Basic properties should still work
			dto.FullName.Should( ).Be( "Jane Smith" );
			dto.EmailStatus.Should( ).Be( "Pending" );
			dto.Country.Should( ).Be( "CA" );
			dto.ActivityStatus.Should( ).Be( "Inactive" );

			// Service provider dependent properties should be empty (default values)
			dto.LocalizedEmailStatus.Should( ).Be( "" );
			dto.LocalizedActivityStatus.Should( ).Be( "" );
			dto.CountryFullName.Should( ).Be( "" );
		} finally {
			// Restore original service provider
			if ( originalServiceProvider != null ) {
				MythServiceProvider.Initialize( originalServiceProvider );
			} else {
				MythServiceProvider.Reset( );
			}
		}
	}

	[Fact]
	public void IMorphableFrom_Should_HandleMissingServiceWithGracefulDegradation( ) {
		// Arrange - Testing graceful degradation when ILocalizationService is not available in MythServiceProvider
		var originalServiceProvider = MythServiceProvider.Current;

		try {
			// Reset and configure MythServiceProvider without localization service
			MythServiceProvider.Reset( );
			var servicesWithoutLocalization = new ServiceCollection( );
			servicesWithoutLocalization.AddMorph( );
			// Note: Not adding ILocalizationService to test graceful degradation
			MythServiceProvider.Initialize( servicesWithoutLocalization.BuildServiceProvider( ) );

			var user = new User {
				Id = 1,
				FirstName = "Bob",
				LastName = "Johnson",
				Email = "bob@example.com",
				BirthDate = new DateTime( 1980, 8, 20 ),
				CountryCode = "uk",
				IsEmailVerified = true,
				LastLoginAt = DateTime.Now.AddDays( -1 )
			};

			// Act - Use MythServiceProvider directly (no service provider parameter needed)
			var dto = user.To<UserProfileDto>( );

			// Assert - Basic properties should work
			dto.FullName.Should( ).Be( "Bob Johnson" );
			dto.EmailStatus.Should( ).Be( "Verified" );
			dto.Country.Should( ).Be( "UK" );
			dto.ActivityStatus.Should( ).Be( "Active" );

			// Service provider dependent properties should be empty when service is not available
			dto.LocalizedEmailStatus.Should( ).Be( "" );
			dto.LocalizedActivityStatus.Should( ).Be( "" );
			dto.CountryFullName.Should( ).Be( "" );
		} finally {
			// Restore original service provider
			if ( originalServiceProvider != null ) {
				MythServiceProvider.Initialize( originalServiceProvider );
			} else {
				MythServiceProvider.Reset( );
			}
		}
	}

	[Fact]
	public void IMorphableFrom_Should_SupportComplexServiceProviderScenarios( ) {
		// Arrange
		var user = new User {
			Id = 42,
			FirstName = "Alice",
			LastName = "Wonder",
			Email = "alice@wonderland.com",
			BirthDate = new DateTime( 1992, 12, 25 ),
			CountryCode = "BR",
			IsEmailVerified = false,
			LastLoginAt = DateTime.Now.AddDays( -15 )
		};

		// Act
		var dto = user.To<UserProfileDto>( _serviceProvider );

		// Assert
		dto.FullName.Should( ).Be( "Alice Wonder" );
		dto.EmailStatus.Should( ).Be( "Pending" );
		dto.Age.Should( ).Be( DateTime.Now.Year - 1992 );
		dto.Country.Should( ).Be( "BR" );
		dto.ActivityStatus.Should( ).Be( "Inactive" );
		dto.InitialsAvatar.Should( ).Be( "AW" );

		// Service provider enhanced properties
		dto.LocalizedEmailStatus.Should( ).Be( "Email Pending ⏳" );
		dto.LocalizedActivityStatus.Should( ).Be( "User Inactive 🔴" );
		dto.CountryFullName.Should( ).Be( "Brazil" );
	}

	[Fact]
	public void SimpleUserDto_Should_MapAutomatically_WhenNoCustomMappings( ) {
		// Arrange
		var user = new User {
			Id = 123,
			FirstName = "John",
			LastName = "Doe",
			Email = "john.doe@example.com",
			BirthDate = new DateTime( 1985, 5, 15 ),
			CountryCode = "US",
			IsEmailVerified = true,
			LastLoginAt = new DateTime( 2024, 6, 1, 10, 30, 0 )
		};

		// Act
		var dto = user.To<SimpleUserDto>( _serviceProvider );

		// Assert - All properties should be automatically mapped
		dto.Should( ).NotBeNull( );
		dto.Id.Should( ).Be( user.Id );
		dto.FirstName.Should( ).Be( user.FirstName );
		dto.LastName.Should( ).Be( user.LastName );
		dto.Email.Should( ).Be( user.Email );
		dto.BirthDate.Should( ).Be( user.BirthDate );
		dto.CountryCode.Should( ).Be( user.CountryCode );
		dto.IsEmailVerified.Should( ).Be( user.IsEmailVerified );
		dto.LastLoginAt.Should( ).Be( user.LastLoginAt );
	}
}
