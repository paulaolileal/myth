using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace Myth.Testing.Repositories {

	/// <summary>
	/// A base class to unit tests who needs database context
	/// </summary>
	/// <typeparam name="TContext">The DbContext type to be used in tests</typeparam>
	/// <remarks>
	/// This class automatically initializes the database on first access and cleans up on disposal.
	/// Manual calls to InitializeDatabaseAsync() and CleanupDatabaseAsync() are optional.
	/// </remarks>
	public abstract class BaseDatabaseTests<TContext> : BaseTests where TContext : DbContext {

		private readonly Lazy<Task> _databaseInitializer;
		private bool _isDatabaseInitialized = false;
		private readonly object _initializationLock = new object( );

		/// <summary>
		/// Initializes a new instance of the BaseDatabaseTests class with database context
		/// </summary>
		/// <param name="fakerCulture">The culture to use for Faker data generation</param>
		/// <remarks>
		/// Configures an in-memory database context and sets up lazy initialization.
		/// The database is automatically initialized on first access and cleaned up on disposal.
		/// </remarks>
		public BaseDatabaseTests( string fakerCulture = "en_US" ) : base( fakerCulture ) {
			SetupDatabase( );
			_databaseInitializer = new Lazy<Task>( InitializeDatabaseInternalAsync );
		}

		/// <summary>
		/// Setup the database context for testing
		/// </summary>
		private void SetupDatabase( ) {
			_services
				.AddDbContext<TContext>( options => options
					.UseInMemoryDatabase( databaseName: $"TestDB_{Guid.NewGuid( )}" )
					.ConfigureWarnings( conf => conf.Ignore( InMemoryEventId.TransactionIgnoredWarning ) )
					.UseLazyLoadingProxies( ),
						contextLifetime: ServiceLifetime.Scoped,
						optionsLifetime: ServiceLifetime.Singleton );

			RebuildServiceProvider( );
		}

		/// <summary>
		/// Ensures automatic database initialization on first access
		/// </summary>
		/// <returns>A task representing the asynchronous operation</returns>
		private async Task InitializeDatabaseInternalAsync( ) {
			lock ( _initializationLock ) {
				if ( _isDatabaseInitialized )
					return;
			}

			await InitializeDatabaseAsync( );

			lock ( _initializationLock ) {
				_isDatabaseInitialized = true;
			}
		}

		/// <summary>
		/// Ensures the database is created and cleaned before each test
		/// </summary>
		/// <remarks>
		/// This method is called automatically on first database access.
		/// You can also call it manually for explicit control over timing.
		/// </remarks>
		protected virtual async Task InitializeDatabaseAsync( ) {
			var context = GetRequiredService<TContext>( );

			await context.Database.EnsureCreatedAsync( );
			context.ChangeTracker.Clear( );
		}

		/// <summary>
		/// Ensures database initialization before accessing database-related operations
		/// </summary>
		/// <returns>A task representing the asynchronous operation</returns>
		private async Task EnsureDatabaseInitializedAsync( ) {
			await _databaseInitializer.Value;
		}

		/// <summary>
		/// Ensures the database is removed after each test
		/// </summary>
		/// <remarks>
		/// This method is called automatically during DisposeAsync().
		/// You can also call it manually for explicit cleanup control.
		/// </remarks>
		protected virtual async Task CleanupDatabaseAsync( ) {
			var context = GetRequiredService<TContext>( );

			await context.Database.EnsureDeletedAsync( );
		}

		/// <summary>
		/// Save the database context
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>A task representing the asynchronous operation</returns>
		protected virtual async Task SaveChangesAsync( CancellationToken cancellationToken = default ) {
			await EnsureDatabaseInitializedAsync( );
			var context = GetRequiredService<TContext>( );

			await context.SaveChangesAsync( cancellationToken );
		}

		/// <summary>
		/// Get the database context for direct access
		/// </summary>
		/// <returns>The database context instance</returns>
		/// <remarks>
		/// The database is automatically initialized when this method is called for the first time.
		/// </remarks>
		protected TContext GetContext( ) {
			// Note: For synchronous context access, we use a fire-and-forget approach
			// The calling code should be responsible for proper async patterns
			_ = EnsureDatabaseInitializedAsync( );
			return GetRequiredService<TContext>( );
		}

		/// <summary>
		/// Get the database context for direct access with automatic initialization
		/// </summary>
		/// <returns>A task containing the database context instance</returns>
		/// <remarks>
		/// This async version ensures proper database initialization before returning the context.
		/// Prefer this method in async scenarios for guaranteed initialization.
		/// </remarks>
		protected async Task<TContext> GetContextAsync( ) {
			await EnsureDatabaseInitializedAsync( );
			return GetRequiredService<TContext>( );
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
				context?.Database?.EnsureDeleted( );
			} catch {
				// Ignore exceptions during disposal
			}

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
				// Only perform cleanup if the database was initialized
				if ( _isDatabaseInitialized ) {
					await CleanupDatabaseAsync( );
				}
			} catch {
				// Ignore exceptions during disposal to prevent masking test failures
			}

			await base.DisposeAsync( );
		}
	}
}