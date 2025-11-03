using Myth.Models;

namespace Myth.Rules.Base {

	/// <summary>
	/// Generic custom rule
	/// </summary>
	internal sealed class CustomRule<T> : ValidationRuleBase<T> {
		private readonly Func<T, bool>? _syncPredicate;
		private readonly Func<T, CancellationToken, IServiceProvider, Task<bool>>? _asyncPredicate;

		public CustomRule( Func<T, bool> predicate ) {
			_syncPredicate = predicate;
		}

		public CustomRule( Func<T, CancellationToken, IServiceProvider, Task<bool>> predicate ) {
			_asyncPredicate = predicate;
		}

		protected override async Task<bool> EvaluateAsync( RuleContext<T> context ) {
			if ( _syncPredicate != null )
				return _syncPredicate( context.Value );

			if ( _asyncPredicate != null )
				return await _asyncPredicate( context.Value, context.CancellationToken, context.ServiceProvider );

			return true;
		}

		protected override string GetDefaultMessage( T value ) {
			return "Custom validation failed";
		}
	}
}