using Myth.Models;

namespace Myth.Rules.Base;

/// <summary>
/// Generic custom rule
/// </summary>
internal sealed class CustomRule<T> : ValidationRuleBase<T> {
	private readonly Func<T, bool>? _syncPredicate;
	private readonly Func<T, CancellationToken, IServiceProvider, Task<bool>>? _asyncPredicate;
	private readonly Func<T, object, bool>? _syncEntityPredicate;
	private readonly Func<T, object, CancellationToken, IServiceProvider, Task<bool>>? _asyncEntityPredicate;

	public CustomRule( Func<T, bool> predicate ) {
		_syncPredicate = predicate;
	}

	public CustomRule( Func<T, CancellationToken, IServiceProvider, Task<bool>> predicate ) {
		_asyncPredicate = predicate;
	}

	/// <summary>
	/// Creates a custom rule with access to the entire entity being validated
	/// </summary>
	/// <param name="predicate">Synchronous predicate that receives both the property value and the parent entity</param>
	public CustomRule( Func<T, object, bool> predicate ) {
		_syncEntityPredicate = predicate;
	}

	/// <summary>
	/// Creates a custom rule with access to the entire entity being validated
	/// </summary>
	/// <param name="predicate">Asynchronous predicate that receives the property value, parent entity, cancellation token, and service provider</param>
	public CustomRule( Func<T, object, CancellationToken, IServiceProvider, Task<bool>> predicate ) {
		_asyncEntityPredicate = predicate;
	}

	protected override async Task<bool> EvaluateAsync( RuleContext<T> context ) {
		if ( _syncPredicate != null )
			return _syncPredicate( context.Value );

		if ( _asyncPredicate != null )
			return await _asyncPredicate( context.Value, context.CancellationToken, context.ServiceProvider );

		if ( _syncEntityPredicate != null )
			return _syncEntityPredicate( context.Value, context.Entity! );

		if ( _asyncEntityPredicate != null )
			return await _asyncEntityPredicate( context.Value, context.Entity!, context.CancellationToken, context.ServiceProvider );

		return true;
	}

	protected override string GetDefaultMessage( T value ) {
		return "Custom validation failed";
	}
}
