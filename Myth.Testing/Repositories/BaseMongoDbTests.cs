using EphemeralMongo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Myth.Repositories;

/// <summary>
/// A base class for unit tests that need MongoDB context
/// </summary>
/// <typeparam name="TContext">The DbContext type to be used in tests</typeparam>
/// <remarks>
/// This class automatically initializes a real MongoDB instance in memory for testing
/// and cleans up on disposal. Uses EphemeralMongo to provide a real MongoDB server
/// for integration tests.
/// </remarks>
public abstract class BaseMongoDbTests<TContext> : BaseTests
	where TContext : DbContext {
	private readonly IMongoRunner _mongoRunner;
	private readonly SemaphoreSlim _initializationSemaphore = new( 1, 1 );
	private bool _isDatabaseInitialized = false;
	protected string DatabaseName { get; }

	/// <summary>
	/// Initializes a new instance of the BaseMongoDbTests class with MongoDB context
	/// </summary>
	/// <param name="fakerCulture">The culture to use for Faker data generation</param>
	/// <remarks>
	/// Configures a real MongoDB instance in memory using EphemeralMongo.
	/// The database is automatically initialized on first access and cleaned up on disposal.
	/// </remarks>
	public BaseMongoDbTests( string fakerCulture = "en_US" )
		: base( fakerCulture ) {
		DatabaseName = $"TestDB_{Guid.NewGuid( ):N}";

		// Start in-memory MongoDB
		_mongoRunner = MongoRunner.Run(
			new MongoRunnerOptions {
				UseSingleNodeReplicaSet = true,
				StandardOutputLogger = message => Console.WriteLine( message ),
				StandardErrorLogger = message => Console.WriteLine( $"Error: {message}" ),
			}
		);

		SetupDatabase( );
	}

	/// <summary>
	/// Setup the database context for testing with MongoDB
	/// </summary>
	private void SetupDatabase( ) {
		var connectionString = _mongoRunner.ConnectionString;
		var client = new MongoClient( connectionString );

		_services.AddSingleton<IMongoClient>( client );

		_services.AddDbContext<TContext>(
			options =>
				options
					.UseMongoDB( client, DatabaseName )
					.ConfigureWarnings( warnings =>
						warnings.Ignore(
							Microsoft
								.EntityFrameworkCore
								.Diagnostics
								.CoreEventId
								.ManyServiceProvidersCreatedWarning
						)
					),
			contextLifetime: ServiceLifetime.Scoped,
			optionsLifetime: ServiceLifetime.Singleton
		);

		RebuildServiceProvider( );
	}

	private async Task EnsureDatabaseInitializedAsync( ) {
		if ( _isDatabaseInitialized )
			return;

		await _initializationSemaphore.WaitAsync( );
		try {
			if ( _isDatabaseInitialized )
				return;

			await InitializeDatabaseAsync( );
			_isDatabaseInitialized = true;
		} finally {
			_initializationSemaphore.Release( );
		}
	}

	/// <summary>
	/// Ensures automatic database initialization on first access
	/// </summary>
	/// <returns>A task representing the asynchronous operation</returns>
	protected virtual async Task InitializeDatabaseAsync( ) {
		using var scope = _serviceProvider.CreateScope( );
		var context = scope.ServiceProvider.GetRequiredService<TContext>( );

		await context.Database.EnsureCreatedAsync( );

		var collections = context
			.Model.GetEntityTypes( )
			.Select( et => et.GetCollectionName( ) )
			.Where( name => !string.IsNullOrEmpty( name ) )
			.Distinct( );

		var client = GetRequiredService<IMongoClient>( );
		var database = client.GetDatabase( DatabaseName );

		foreach ( var collectionName in collections ) {
			try {
				await database.CreateCollectionAsync( collectionName );
			} catch {
				// Collection already exists
			}
		}
	}

	/// <summary>
	/// Ensures the database is removed after each test
	/// </summary>
	/// <remarks>
	/// This method is called automatically during DisposeAsync().
	/// You can also call it manually for explicit cleanup control.
	/// </remarks>
	protected virtual async Task CleanupDatabaseAsync( ) {
		try {
			var client = GetRequiredService<IMongoClient>( );
			var database = client.GetDatabase( DatabaseName );

			var collections = await ( await database.ListCollectionNamesAsync( ) ).ToListAsync( );
			foreach ( var collectionName in collections )
				await database.DropCollectionAsync( collectionName );
		} catch {
			// Ignore cleanup errors
		}
	}

	/// <summary>
	/// Save the database context
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>A task representing the asynchronous operation</returns>
	protected virtual async Task SaveChangesAsync( CancellationToken cancellationToken = default ) {
		await EnsureDatabaseInitializedAsync( );
		var context = GetRequiredService<TContext>( );

		for ( int i = 0; i < 3; i++ ) {
			try {
				await context.SaveChangesAsync( cancellationToken );

				ClearContext( context );

				return;
			} catch ( Exception ) {
				Thread.Sleep( 3000 );
			}
		}
	}

	private void ClearContext( TContext context ) {
		context.ChangeTracker.Clear( );

		foreach ( var entry in context.ChangeTracker.Entries( ).ToList( ) ) {
			entry.State = EntityState.Detached;
		}
	}

	/// <summary>
	/// Get the database context for direct access
	/// </summary>
	/// <returns>The database context instance with cleared change tracker</returns>
	/// <remarks>
	/// The database is automatically initialized when this method is called for the first time.
	/// The change tracker is cleared to ensure fresh data from the database in test scenarios.
	/// </remarks>
	protected TContext GetContext( ) {
		EnsureDatabaseInitializedAsync( ).ConfigureAwait( true ).GetAwaiter( ).GetResult( );

		var context = GetRequiredService<TContext>( );

		ClearContext( context );

		return context;
	}

	/// <summary>
	/// Get the database context for direct access with automatic initialization
	/// </summary>
	/// <returns>A task containing the database context instance with cleared change tracker</returns>
	/// <remarks>
	/// This async version ensures proper database initialization before returning the context.
	/// The change tracker is cleared to ensure fresh data from the database in test scenarios.
	/// Prefer this method in async scenarios for guaranteed initialization.
	/// </remarks>
	protected async Task<TContext> GetContextAsync( ) {
		await EnsureDatabaseInitializedAsync( );
		var context = GetRequiredService<TContext>( );

		ClearContext( context );

		return context;
	}

	/// <summary>
	/// Dispose database resources
	/// </summary>
	/// <remarks>
	/// Synchronously disposes database resources. For async scenarios, prefer DisposeAsync().
	/// </remarks>
	public override void Dispose( ) {
		try {
			var context = GetService<TContext>( );
			if ( context != null ) {
				ClearContext( context );
				context.Dispose( );
			}
		} catch { }

		_initializationSemaphore?.Dispose( );
		_mongoRunner?.Dispose( );
		base.Dispose( );
	}

	/// <summary>
	/// Asynchronously dispose database resources
	/// </summary>
	/// <returns>A ValueTask representing the asynchronous disposal operation</returns>
	/// <remarks>
	/// Automatically performs database cleanup before disposing base resources.
	/// This method should be preferred over Dispose() when working in async scenarios.
	/// </remarks>
	public override async ValueTask DisposeAsync( ) {
		try {
			if ( _isDatabaseInitialized )
				await CleanupDatabaseAsync( );

			var context = GetService<TContext>( );
			if ( context != null ) {
				ClearContext( context );
				await context.DisposeAsync( );
			}
		} catch { }

		_initializationSemaphore?.Dispose( );
		_mongoRunner?.Dispose( );
		await base.DisposeAsync( );
	}
}
