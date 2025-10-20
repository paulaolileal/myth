using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Collections {

	internal sealed class DistinctByRule<T, TKey> : ValidationRuleBase<IEnumerable<T>> {
		private readonly Func<T, TKey> _keySelector;

		public DistinctByRule( Func<T, TKey> keySelector ) {
			_keySelector = keySelector;
		}

		protected override Task<bool> EvaluateAsync( RuleContext<IEnumerable<T>> context ) {
			if ( context.Value == null )
				return Task.FromResult( true );

			var list = context.Value.ToList( );
			return Task.FromResult( list.Count == list.DistinctBy( _keySelector ).Count( ) );
		}

		protected override string GetDefaultMessage( IEnumerable<T> value ) {
			return "Collection must not contain duplicates based on the specified property";
		}
	}
}