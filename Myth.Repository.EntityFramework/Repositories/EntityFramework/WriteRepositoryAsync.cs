using Microsoft.EntityFrameworkCore;
using Myth.Contexts;
using Myth.Interfaces.Repositories.EntityFramework;

namespace Myth.Repositories.EntityFramework;

public class WriteRepositoryAsync<T>( BaseContext context ) : IWriteRepositoryAsync<T> where T : class {
	private readonly BaseContext _context = context;

	/// <summary>
	/// Asynchronously adds a new entity to the repository
	/// </summary>
	/// <param name="entity">The entity to add</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous add operation</returns>
	public virtual Task AddAsync( T entity, CancellationToken cancellationToken = default ) =>
	   _context
			.Set<T>( )
			.AddAsync( entity, cancellationToken )
			.AsTask( );

	/// <summary>
	/// Asynchronously adds a collection of entities to the repository
	/// </summary>
	/// <param name="entity">The collection of entities to add</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous add operation</returns>
	public virtual Task AddRangeAsync( IEnumerable<T> entity, CancellationToken cancellationToken = default ) =>
		_context
			.Set<T>( )
			.AddRangeAsync( entity, cancellationToken );

	/// <summary>
	/// Asynchronously removes an entity from the repository
	/// </summary>
	/// <param name="entity">The entity to remove</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous remove operation</returns>
	public virtual Task RemoveAsync( T entity, CancellationToken cancellationToken = default ) =>
	   Task.Run( ( ) =>
			_context
			   .Set<T>( )
			   .Remove( entity ),
		   cancellationToken );

	/// <summary>
	/// Asynchronously removes a collection of entities from the repository
	/// </summary>
	/// <param name="entities">The collection of entities to remove</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous remove operation</returns>
	public virtual Task RemoveRangeAsync( IEnumerable<T> entities, CancellationToken cancellationToken = default ) =>
	   Task.Run( ( ) =>
			_context
				.Set<T>( )
				.RemoveRange( entities ),
		   cancellationToken );

	/// <summary>
	/// Asynchronously updates an existing entity in the repository
	/// </summary>
	/// <param name="entity">The entity to update</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous update operation</returns>
	public virtual Task UpdateAsync( T entity, CancellationToken cancellationToken = default ) =>
		AttachAsync( entity, cancellationToken )
			.ContinueWith( ( _ ) => _context.Entry( entity ).State = EntityState.Modified, cancellationToken );

	/// <summary>
	/// Asynchronously updates a collection of existing entities in the repository
	/// </summary>
	/// <param name="entities">The collection of entities to update</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous update operation</returns>
	public virtual Task UpdateRangeAsync( IEnumerable<T> entities, CancellationToken cancellationToken = default ) =>
		AttachRangeAsync( entities, cancellationToken )
			.ContinueWith( task => {
				foreach ( var entity in entities )
					_context.Entry( entity ).State = EntityState.Modified;
			}, cancellationToken );

	/// <summary>
	/// Asynchronously attaches an entity to the context if it's detached
	/// </summary>
	/// <param name="entity">The entity to attach</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous attach operation</returns>
	public virtual Task AttachAsync( T entity, CancellationToken cancellationToken = default ) =>
		Task.Run( ( ) => {
			if ( _context.Entry( entity ).State == EntityState.Detached )
				_context
					.Set<T>( )
					.Attach( entity );
		}, cancellationToken );

	/// <summary>
	/// Asynchronously attaches a collection of entities to the context if they are detached
	/// </summary>
	/// <param name="entities">The collection of entities to attach</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests</param>
	/// <returns>A task that represents the asynchronous attach operation</returns>
	public virtual Task AttachRangeAsync( IEnumerable<T> entities, CancellationToken cancellationToken = default ) =>
		Task.Run( ( ) => {
			var entitiesToAttach = entities.Where( entity => _context.Entry( entity ).State == EntityState.Detached );
			_context
				.Set<T>( )
				.AttachRange( entitiesToAttach );
		}, cancellationToken );

	/// <summary>
	/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources asynchronously
	/// </summary>
	/// <returns>A task that represents the asynchronous dispose operation</returns>
	public ValueTask DisposeAsync( ) => DisposeAsyncCore( );

	/// <summary>
	/// Core implementation of the asynchronous dispose pattern
	/// </summary>
	/// <returns>A task that represents the asynchronous dispose operation</returns>
	protected virtual async ValueTask DisposeAsyncCore( ) {
		if ( _context is not null )
			await _context.DisposeAsync( );

		GC.SuppressFinalize( this );
	}
}