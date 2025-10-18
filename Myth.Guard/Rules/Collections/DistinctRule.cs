using Myth.Models;
using Myth.Rules.Base;

namespace Myth.Rules.Collections {

	internal sealed class DistinctRule<T> : ValidationRuleBase<IEnumerable<T>> {

		protected override Task<bool> EvaluateAsync( RuleContext<IEnumerable<T>> context ) {
			if ( context.Value == null )
				return Task.FromResult( true );

			var list = context.Value.ToList( );
			return Task.FromResult( list.Count == list.Distinct( ).Count( ) );
		}

		protected override string GetDefaultMessage( IEnumerable<T> value ) {
			return "Collection must not contain duplicates";
		}
	}
}