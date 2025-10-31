using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.Testing.Repositories {
	/// <summary>
	/// A base class to unit tests who needs database context
	/// </summary>
	/// <typeparam name="TContext">The DbContext type to be used in tests</typeparam>
	public abstract class BaseDatabaseTests<TContext> : BaseTests where TContext : DbContext {
		/// <summary>
		/// Initializes a new instance of the BaseDatabaseTests class with database context
		/// </summary>
		/// <param name="fakerCulture">The culture to use for Faker data generation</param>
		/// <remarks>
		/// Add a database context in memory
		/// </remarks>
		public BaseDatabaseTests( string fakerCulture = "en_US" ) : base( fakerCulture ) {
			SetupDatabase( );
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

			_serviceProvider = _services.BuildServiceProvider( );
		}

		/// <summary>
		/// Ensures the database is created and cleaned before each test
		/// </summary>
		/// <remarks>
		/// Call this method at the beginning of each test method to ensure clean state
		/// </remarks>
		protected virtual async Task InitializeDatabaseAsync( ) {
			var context = GetRequiredService<TContext>( );

			await context.Database.EnsureCreatedAsync( );
			context.ChangeTracker.Clear( );
		}

		/// <summary>
		/// Ensures the database is removed after each test
		/// </summary>
		/// <remarks>
		/// Call this method at the end of each test method or use it in test cleanup
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
		protected virtual Task SaveChangesAsync( CancellationToken cancellationToken = default ) {
			var context = GetRequiredService<TContext>( );

			return context.SaveChangesAsync( cancellationToken );
		}

		/// <summary>
		/// Get the database context for direct access
		/// </summary>
		/// <returns>The database context instance</returns>
		protected TContext GetContext( ) {
			return GetRequiredService<TContext>( );
		}

		/// <summary>
		/// Dispose database resources
		/// </summary>
		public override void Dispose( ) {
			try {
				var context = GetService<TContext>( );
				context?.Database?.EnsureDeleted( );
			} catch {
				// Ignore exceptions during disposal
			}

			base.Dispose( );
		}
	}
}
