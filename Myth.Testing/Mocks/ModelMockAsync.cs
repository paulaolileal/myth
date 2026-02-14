using System;
using System.Collections.Generic;
using System.Text;
using Bogus;
using Microsoft.EntityFrameworkCore;

namespace Myth.Mocks;

/// <summary>
/// Provides a base class for generating and managing mock model entities within a specified Entity Framework
/// database context for testing or seeding purposes.
/// </summary>
/// <remarks>This class is intended for use in test scenarios or data seeding workflows where automated
/// generation of model entities is required. Derived classes should implement the logic for generating specific
/// model types. This type is internal and not intended for use outside of the containing assembly.</remarks>
/// <typeparam name="TContext">The type of the Entity Framework database context in which the model entities are managed. Must derive from
/// DbContext.</typeparam>
/// <typeparam name="TModel">The type of the model entity to generate and manage. Must be a reference type.</typeparam>
/// <param name="context">The database context instance used to access and persist model entities.</param>
/// <param name="faker">The Faker instance used to generate randomized data for model entities.</param>
public abstract class ModelMockAsync<TContext, TModel>( TContext context, Faker faker )
	where TContext : DbContext
	where TModel : class {
	protected readonly Faker _faker = faker;

	protected readonly TContext _context = context;

	protected DbSet<TModel> _collection = context.Set<TModel>( );

	/// <summary>
	/// Asynchronously generates a collection of model instances.
	/// </summary>
	/// <param name="amount">The number of model instances to generate. Must be greater than zero.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains an enumerable collection of
	/// generated model instances.</returns>
	public abstract Task<IEnumerable<TModel>> GenerateAsync( int amount, IDictionary<string, object>? metadata = null );

	/// <summary>
	/// Asynchronously generates a single instance of the model.
	/// </summary>
	/// <returns>A task that represents the asynchronous operation. The task result contains the generated model instance.</returns>
	public virtual async Task<TModel> GenerateAsync( IDictionary<string, object>? metadata = null ) => ( await GenerateAsync( 1, metadata ) ).First( );

	/// <summary>
	/// Asynchronously saves all changes made in this context to the underlying database.
	/// </summary>
	/// <remarks>If the context has no changes, no database operations are performed. This method is
	/// typically used to persist changes made to tracked entities. Calling this method multiple times in succession
	/// may result in multiple database transactions.</remarks>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous save operation.</param>
	/// <returns>A task that represents the asynchronous save operation.</returns>
	protected virtual async Task SaveChangesAsync( CancellationToken cancellationToken = default ) => await _context.SaveChangesAsync( cancellationToken );
}
