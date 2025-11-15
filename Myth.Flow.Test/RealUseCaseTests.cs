using System;
using System.Threading.Tasks;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Flow.Test.Contexts;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using Myth.ServiceProvider;
using Xunit;

namespace Myth.Flow.Test;

public class CreateUserTests : BaseTestFixture {
	private readonly MockUserRepository _repository = new MockUserRepository( );
	private readonly MockUnitOfWork _unitOfWork = new MockUnitOfWork( );
	private readonly MockEventPublisher _eventPublisher = new MockEventPublisher( );
	private readonly MockUserMetrics _metrics = new MockUserMetrics( );
	private readonly Faker<CreateUserRequest> _requestFaker;

	public CreateUserTests( ) : base( ) {
		// Initialize Bogus faker
		_requestFaker = new Faker<CreateUserRequest>( )
			.CustomInstantiator( f => new CreateUserRequest(
				Email: f.Internet.Email( ),
				Password: GenerateValidPassword( f ),
				Role: f.PickRandom( "admin", "common", "maintainer" )
			) );
	}

	protected override void ConfigureServices( IServiceCollection services ) {
		// Register mocks
		services.AddSingleton<IUserRepository>( _repository );
		services.AddSingleton<IPasswordValidator>( new MockPasswordValidator( ) );
		services.AddSingleton<IRoleValidator>( new MockRoleValidator( ) );
		services.AddSingleton<IUnitOfWork>( _unitOfWork );
		services.AddSingleton<IEventPublisher>( _eventPublisher );
		services.AddSingleton<IUserMetrics>( _metrics );

		// Register pipeline services
		services.AddSingleton<UserValidationService>( );
		services.AddSingleton<UserCreationService>( );
		services.AddSingleton<UserEventService>( );
		services.AddSingleton<UserObservabilityService>( );

		// Register logging
		services.AddLogging( );
	}

	private static string GenerateValidPassword( Faker faker ) {
		// Generate password that meets requirements: 8+ chars, 1 digit, 1 uppercase
		var password = faker.Internet.Password(
			length: 10,
			memorable: false,
			prefix: "Pass1" );
		return password;
	}

	[Fact]
	public async Task CreateUser_WithValidData_ShouldSucceed( ) {
		// Arrange
		var request = _requestFaker.Generate( );
		var context = new CreateUserContext { Request = request };

		// Act
		var result = await Pipeline.Start( context )
			.WithTelemetry( "CreateUser" )
			.WithRetry( maxAttempts: 2, backoffMs: 100 )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserValidationService>( )!.ValidateAsync( ctx ) )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserCreationService>( )!.CreateUserAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserEventService>( )!.PublishUserCreatedAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserObservabilityService>( )!.ObserveUserCreationAsync( ctx ) )
			.Transform<CreateUserResponse>( ctx => new CreateUserResponse(
				ctx.CreatedUser!.Id,
				ctx.CreatedUser.Email,
				ctx.CreatedUser.Role,
				ctx.CreatedUser.CreatedAt
			) )
			.ExecuteAsync( );

		// Assert
		Assert.True( result.IsSuccess );
		Assert.NotNull( result.Value );
		Assert.Equal( request.Email, result.Value.Email );
		Assert.Equal( request.Role, result.Value.Role );
		Assert.NotEqual( Guid.Empty, result.Value.Id );

		// Verify transaction was committed
		Assert.True( _unitOfWork.TransactionCommitted );
		Assert.False( _unitOfWork.TransactionRolledBack );

		// Verify event was published
		Assert.Single( _eventPublisher.PublishedEvents );
		var publishedEvent = Assert.IsType<UserCreatedEvent>( _eventPublisher.PublishedEvents[ 0 ] );
		Assert.Equal( result.Value.Id, publishedEvent.UserId );
		Assert.Equal( result.Value.Email, publishedEvent.Email );

		// Verify metrics were incremented
		Assert.Equal( 1, _metrics.TotalUsersCreated );
	}

	[Fact]
	public async Task CreateUser_WithExistingEmail_ShouldFail( ) {
		// Arrange
		var request = _requestFaker.Generate( );
		_repository.AddExistingEmail( request.Email );
		var context = new CreateUserContext { Request = request };

		// Act
		var result = await Pipeline.Start( context )
			.WithTelemetry( "CreateUser" )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserValidationService>( )!.ValidateAsync( ctx ) )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserCreationService>( )!.CreateUserAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserEventService>( )!.PublishUserCreatedAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserObservabilityService>( )!.ObserveUserCreationAsync( ctx ) )
			.Transform<CreateUserResponse>( ctx => new CreateUserResponse(
				ctx.CreatedUser!.Id,
				ctx.CreatedUser.Email,
				ctx.CreatedUser.Role,
				ctx.CreatedUser.CreatedAt
			) )
			.ExecuteAsync( );

		// Assert
		Assert.False( result.IsSuccess );
		Assert.Contains( "already in use", result.ErrorMessage );

		// Verify transaction was not committed
		Assert.False( _unitOfWork.TransactionCommitted );

		// Verify no events were published
		Assert.Empty( _eventPublisher.PublishedEvents );

		// Verify metrics were not incremented
		Assert.Equal( 0, _metrics.TotalUsersCreated );
	}

	[Fact]
	public async Task CreateUser_WithInvalidEmail_ShouldFail( ) {
		// Arrange
		var faker = new Faker( );
		var request = new CreateUserRequest(
			Email: "invalid-email-format",
			Password: GenerateValidPassword( faker ),
			Role: "admin"
		);
		var context = new CreateUserContext { Request = request };

		// Act
		var result = await Pipeline.Start( context )
			.WithTelemetry( "CreateUser" )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserValidationService>( )!.ValidateAsync( ctx ) )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserCreationService>( )!.CreateUserAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserEventService>( )!.PublishUserCreatedAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserObservabilityService>( )!.ObserveUserCreationAsync( ctx ) )
			.Transform<CreateUserResponse>( ctx => new CreateUserResponse(
				ctx.CreatedUser!.Id,
				ctx.CreatedUser.Email,
				ctx.CreatedUser.Role,
				ctx.CreatedUser.CreatedAt
			) )
			.ExecuteAsync( );

		// Assert
		Assert.False( result.IsSuccess );
		Assert.Contains( "Invalid email format", result.ErrorMessage );
		Assert.Empty( _eventPublisher.PublishedEvents );
		Assert.Equal( 0, _metrics.TotalUsersCreated );
	}

	[Fact]
	public async Task CreateUser_WithWeakPassword_ShouldFail( ) {
		// Arrange
		var faker = new Faker( );
		var request = new CreateUserRequest(
			Email: faker.Internet.Email( ),
			Password: "weak", // Too short, no digit, no uppercase
			Role: "admin"
		);
		var context = new CreateUserContext { Request = request };

		// Act
		var result = await Pipeline.Start( context )
			.WithTelemetry( "CreateUser" )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserValidationService>( )!.ValidateAsync( ctx ) )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserCreationService>( )!.CreateUserAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserEventService>( )!.PublishUserCreatedAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserObservabilityService>( )!.ObserveUserCreationAsync( ctx ) )
			.Transform<CreateUserResponse>( ctx => new CreateUserResponse(
				ctx.CreatedUser!.Id,
				ctx.CreatedUser.Email,
				ctx.CreatedUser.Role,
				ctx.CreatedUser.CreatedAt
			) )
			.ExecuteAsync( );

		// Assert
		Assert.False( result.IsSuccess );
		Assert.Contains( "Password must be at least 8 characters", result.ErrorMessage );
		Assert.Empty( _eventPublisher.PublishedEvents );
		Assert.Equal( 0, _metrics.TotalUsersCreated );
	}

	[Fact]
	public async Task CreateUser_WithPasswordMissingDigit_ShouldFail( ) {
		// Arrange
		var faker = new Faker( );
		var request = new CreateUserRequest(
			Email: faker.Internet.Email( ),
			Password: "NoDigitPassword", // No digit
			Role: "admin"
		);
		var context = new CreateUserContext { Request = request };

		// Act
		var result = await Pipeline.Start( context )
			.WithTelemetry( "CreateUser" )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserValidationService>( )!.ValidateAsync( ctx ) )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserCreationService>( )!.CreateUserAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserEventService>( )!.PublishUserCreatedAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserObservabilityService>( )!.ObserveUserCreationAsync( ctx ) )
			.Transform<CreateUserResponse>( ctx => new CreateUserResponse(
				ctx.CreatedUser!.Id,
				ctx.CreatedUser.Email,
				ctx.CreatedUser.Role,
				ctx.CreatedUser.CreatedAt
			) )
			.ExecuteAsync( );

		// Assert
		Assert.False( result.IsSuccess );
		Assert.Contains( "Password must contain at least one digit", result.ErrorMessage );
		Assert.Empty( _eventPublisher.PublishedEvents );
		Assert.Equal( 0, _metrics.TotalUsersCreated );
	}

	[Fact]
	public async Task CreateUser_WithPasswordMissingUppercase_ShouldFail( ) {
		// Arrange
		var faker = new Faker( );
		var request = new CreateUserRequest(
			Email: faker.Internet.Email( ),
			Password: "nouppercase123", // No uppercase
			Role: "admin"
		);
		var context = new CreateUserContext { Request = request };

		// Act
		var result = await Pipeline.Start( context )
			.WithTelemetry( "CreateUser" )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserValidationService>( )!.ValidateAsync( ctx ) )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserCreationService>( )!.CreateUserAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserEventService>( )!.PublishUserCreatedAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserObservabilityService>( )!.ObserveUserCreationAsync( ctx ) )
			.Transform<CreateUserResponse>( ctx => new CreateUserResponse(
				ctx.CreatedUser!.Id,
				ctx.CreatedUser.Email,
				ctx.CreatedUser.Role,
				ctx.CreatedUser.CreatedAt
			) )
			.ExecuteAsync( );

		// Assert
		Assert.False( result.IsSuccess );
		Assert.Contains( "Password must contain at least one uppercase letter", result.ErrorMessage );
		Assert.Empty( _eventPublisher.PublishedEvents );
		Assert.Equal( 0, _metrics.TotalUsersCreated );
	}

	[Fact]
	public async Task CreateUser_WithInvalidRole_ShouldFail( ) {
		// Arrange
		var faker = new Faker( );
		var request = new CreateUserRequest(
			Email: faker.Internet.Email( ),
			Password: GenerateValidPassword( faker ),
			Role: "superadmin" // Invalid role
		);
		var context = new CreateUserContext { Request = request };

		// Act
		var result = await Pipeline.Start( context )
			.WithTelemetry( "CreateUser" )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserValidationService>( )!.ValidateAsync( ctx ) )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserCreationService>( )!.CreateUserAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserEventService>( )!.PublishUserCreatedAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserObservabilityService>( )!.ObserveUserCreationAsync( ctx ) )
			.Transform<CreateUserResponse>( ctx => new CreateUserResponse(
				ctx.CreatedUser!.Id,
				ctx.CreatedUser.Email,
				ctx.CreatedUser.Role,
				ctx.CreatedUser.CreatedAt
			) )
			.ExecuteAsync( );

		// Assert
		Assert.False( result.IsSuccess );
		Assert.Contains( "Invalid role", result.ErrorMessage );
		Assert.Contains( "admin, common, or maintainer", result.ErrorMessage );
		Assert.Empty( _eventPublisher.PublishedEvents );
		Assert.Equal( 0, _metrics.TotalUsersCreated );
	}

	[Fact]
	public async Task CreateUser_WhenDatabaseFails_ShouldRollbackTransaction( ) {
		// Arrange
		var services = new ServiceCollection( );
		var failingRepository = new MockUserRepository( shouldFailOnCreate: true );
		var unitOfWork = new MockUnitOfWork( );
		var eventPublisher = new MockEventPublisher( );
		var metrics = new MockUserMetrics( );

		services.AddSingleton<IUserRepository>( failingRepository );
		services.AddSingleton<IPasswordValidator>( new MockPasswordValidator( ) );
		services.AddSingleton<IRoleValidator>( new MockRoleValidator( ) );
		services.AddSingleton<IUnitOfWork>( unitOfWork );
		services.AddSingleton<IEventPublisher>( eventPublisher );
		services.AddSingleton<IUserMetrics>( metrics );
		services.AddSingleton<UserValidationService>( );
		services.AddSingleton<UserCreationService>( );
		services.AddSingleton<UserEventService>( );
		services.AddSingleton<UserObservabilityService>( );
		services.AddLogging( );
		services.AddFlow( );

		var serviceProvider = services.BuildServiceProvider( );
		MythServiceProvider.Initialize( serviceProvider );

		var request = _requestFaker.Generate( );
		var context = new CreateUserContext { Request = request };

		// Act
		var result = await Pipeline.Start( context )
			.WithTelemetry( "CreateUser" )
			.WithRetry( maxAttempts: 2, backoffMs: 50 )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserValidationService>( )!.ValidateAsync( ctx ) )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserCreationService>( )!.CreateUserAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserEventService>( )!.PublishUserCreatedAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserObservabilityService>( )!.ObserveUserCreationAsync( ctx ) )
			.Transform<CreateUserResponse>( ctx => new CreateUserResponse(
				ctx.CreatedUser!.Id,
				ctx.CreatedUser.Email,
				ctx.CreatedUser.Role,
				ctx.CreatedUser.CreatedAt
			) )
			.ExecuteAsync( );

		// Assert
		Assert.False( result.IsSuccess );
		Assert.Contains( "Failed to create user", result.ErrorMessage );

		// Verify transaction was rolled back
		Assert.True( unitOfWork.TransactionRolledBack );
		Assert.False( unitOfWork.TransactionCommitted );

		// Verify no events were published
		Assert.Empty( eventPublisher.PublishedEvents );

		// Verify metrics were not incremented
		Assert.Equal( 0, metrics.TotalUsersCreated );

		serviceProvider.Dispose( );
	}

	[Fact]
	public async Task CreateUser_WithMultipleValidUsers_ShouldIncrementMetricsCorrectly( ) {
		// Arrange
		var requests = _requestFaker.Generate( 3 );

		// Act & Assert
		for ( int i = 0; i < requests.Count; i++ ) {
			var context = new CreateUserContext { Request = requests[ i ] };

			var result = await Pipeline.Start( context )
				.WithTelemetry( $"CreateUser_{i}" )
				.StepResultAsync(
					ctx => ServiceProvider.GetService<UserValidationService>( )!.ValidateAsync( ctx ) )
				.StepResultAsync(
					ctx => ServiceProvider.GetService<UserCreationService>( )!.CreateUserAsync( ctx ) )
				.TapAsync(
					ctx => ServiceProvider.GetService<UserEventService>( )!.PublishUserCreatedAsync( ctx ) )
				.TapAsync(
					ctx => ServiceProvider.GetService<UserObservabilityService>( )!.ObserveUserCreationAsync( ctx ) )
				.Transform<CreateUserResponse>( ctx => new CreateUserResponse(
					ctx.CreatedUser!.Id,
					ctx.CreatedUser.Email,
					ctx.CreatedUser.Role,
					ctx.CreatedUser.CreatedAt
				) )
				.ExecuteAsync( );

			Assert.True( result.IsSuccess );
			Assert.Equal( i + 1, _metrics.TotalUsersCreated );
		}

		// Final verification
		Assert.Equal( 3, _metrics.TotalUsersCreated );
		Assert.Equal( 3, _eventPublisher.PublishedEvents.Count );
	}

	[Theory]
	[InlineData( "admin" )]
	[InlineData( "common" )]
	[InlineData( "maintainer" )]
	[InlineData( "Admin" )] // Test case insensitivity
	[InlineData( "COMMON" )] // Test case insensitivity
	public async Task CreateUser_WithValidRoles_ShouldSucceed( string role ) {
		// Arrange
		var faker = new Faker( );
		var request = new CreateUserRequest(
			Email: faker.Internet.Email( ),
			Password: GenerateValidPassword( faker ),
			Role: role
		);
		var context = new CreateUserContext { Request = request };

		// Act
		var result = await Pipeline.Start( context )
			.WithTelemetry( "CreateUser" )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserValidationService>( )!.ValidateAsync( ctx ) )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserCreationService>( )!.CreateUserAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserEventService>( )!.PublishUserCreatedAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserObservabilityService>( )!.ObserveUserCreationAsync( ctx ) )
			.Transform<CreateUserResponse>( ctx => new CreateUserResponse(
				ctx.CreatedUser!.Id,
				ctx.CreatedUser.Email,
				ctx.CreatedUser.Role,
				ctx.CreatedUser.CreatedAt
			) )
			.ExecuteAsync( );

		// Assert
		Assert.True( result.IsSuccess );
		Assert.Equal( role, result.Value!.Role );
	}

	[Theory]
	[InlineData( "guest" )]
	[InlineData( "superuser" )]
	[InlineData( "root" )]
	[InlineData( "" )]
	[InlineData( "  " )]
	public async Task CreateUser_WithInvalidRoles_ShouldFail( string role ) {
		// Arrange
		var faker = new Faker( );
		var request = new CreateUserRequest(
			Email: faker.Internet.Email( ),
			Password: GenerateValidPassword( faker ),
			Role: role
		);
		var context = new CreateUserContext { Request = request };

		// Act
		var result = await Pipeline.Start( context )
			.WithTelemetry( "CreateUser" )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserValidationService>( )!.ValidateAsync( ctx ) )
			.StepResultAsync(
				ctx => ServiceProvider.GetService<UserCreationService>( )!.CreateUserAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserEventService>( )!.PublishUserCreatedAsync( ctx ) )
			.TapAsync(
				ctx => ServiceProvider.GetService<UserObservabilityService>( )!.ObserveUserCreationAsync( ctx ) )
			.Transform<CreateUserResponse>( ctx => new CreateUserResponse(
				ctx.CreatedUser!.Id,
				ctx.CreatedUser.Email,
				ctx.CreatedUser.Role,
				ctx.CreatedUser.CreatedAt
			) )
			.ExecuteAsync( );

		// Assert
		Assert.False( result.IsSuccess );
		Assert.Contains( "Invalid role", result.ErrorMessage );
	}
}
