using Myth.Rules.Base;

namespace Myth.Interfaces.Rules {

	/// <summary>
	/// Collection-specific validation rules
	/// </summary>
	public interface ICollectionRules<T> : IRuleConfigurator<IEnumerable<T>> {

		FieldRuleBuilder<IEnumerable<T>> NotEmpty( );

		FieldRuleBuilder<IEnumerable<T>> CountGreaterThan( int min );

		FieldRuleBuilder<IEnumerable<T>> CountLessThan( int max );

		FieldRuleBuilder<IEnumerable<T>> CountBetween( int min, int max );

		FieldRuleBuilder<IEnumerable<T>> All( Func<T, bool> predicate );

		FieldRuleBuilder<IEnumerable<T>> Any( Func<T, bool> predicate );

		FieldRuleBuilder<IEnumerable<T>> None( Func<T, bool> predicate );

		FieldRuleBuilder<IEnumerable<T>> Distinct( );

		FieldRuleBuilder<IEnumerable<T>> DistinctBy<TKey>( Func<T, TKey> keySelector );
	}
}