using Microsoft.EntityFrameworkCore;
using Myth.Contexts;
using Myth.Interfaces.Repositories.EntityFramework;

namespace Myth.Repositories.EntityFramework;

public class WriteRepositoryAsync<T>( BaseContext context ) : IWriteRepositoryAsync<T> where T : class {
	private readonly BaseContext _context = context;

	public virtual Task AddAsync( T entity, CancellationToken cancellationToken = default ) =>
	   _context
			.Set<T>( )
			.AddAsync( entity, cancellationToken )
			.AsTask( );

	public virtual Task AddRangeAsync( IEnumerable<T> entity, CancellationToken cancellationToken = default ) =>
		_context
			.Set<T>( )
			.AddRangeAsync( entity, cancellationToken );

	public virtual Task RemoveAsync( T entity, CancellationToken cancellationToken = default ) =>
	   Task.Run( ( ) =>
			_context
			   .Set<T>( )
			   .Remove( entity ),
		   cancellationToken );

	public virtual Task RemoveRangeAsync( IEnumerable<T> entities, CancellationToken cancellationToken = default ) =>
	   Task.Run( ( ) =>
			_context
				.Set<T>( )
				.RemoveRange( entities ),
		   cancellationToken );

	public virtual Task UpdateAsync( T entity, CancellationToken cancellationToken = default ) =>
		AttachAsync( entity, cancellationToken )
			.ContinueWith( ( _ ) => _context.Entry( entity ).State = EntityState.Modified, cancellationToken );

	public virtual Task UpdateRangeAsync( IEnumerable<T> entities, CancellationToken cancellationToken = default ) =>
		AttachRangeAsync( entities, cancellationToken )
			.ContinueWith( task => {
				foreach ( var entity in entities )
					_context.Entry( entity ).State = EntityState.Modified;
			}, cancellationToken );

	public virtual Task AttachAsync( T entity, CancellationToken cancellationToken = default ) =>
		Task.Run( ( ) => {
			if ( _context.Entry( entity ).State == EntityState.Detached )
				_context
					.Set<T>( )
					.Attach( entity );
		}, cancellationToken );

	public virtual Task AttachRangeAsync( IEnumerable<T> entities, CancellationToken cancellationToken = default ) =>
		Task.Run( ( ) => {
			var entitiesToAttach = entities.Where( entity => _context.Entry( entity ).State == EntityState.Detached );
			_context
				.Set<T>( )
				.AttachRange( entitiesToAttach );
		}, cancellationToken );

	public ValueTask DisposeAsync( ) => DisposeAsyncCore( );

	protected virtual async ValueTask DisposeAsyncCore( ) {
		if ( _context is not null )
			await _context.DisposeAsync( );

		GC.SuppressFinalize( this );
	}
}