using Myth.Interfaces.Rules;
using Myth.Rules.Base;

namespace Myth.Rules.Collections; 

/// <summary>
/// Collection rule configurator
/// </summary>
public sealed class CollectionRules<T> : RuleConfigurator<IEnumerable<T>>, ICollectionRules<T> {

	public CollectionRules( FieldRuleBuilder<IEnumerable<T>> builder ) : base( builder ) {
	}

	public FieldRuleBuilder<IEnumerable<T>> NotEmpty( ) {
		Builder.AddRule( new NotEmptyCollectionRule<T>( ) );
		return Builder;
	}

	public FieldRuleBuilder<IEnumerable<T>> CountGreaterThan( int min ) {
		Builder.AddRule( new CountGreaterThanRule<T>( min ) );
		return Builder;
	}

	public FieldRuleBuilder<IEnumerable<T>> CountLessThan( int max ) {
		Builder.AddRule( new CountLessThanRule<T>( max ) );
		return Builder;
	}

	public FieldRuleBuilder<IEnumerable<T>> CountBetween( int min, int max ) {
		Builder.AddRule( new CountBetweenRule<T>( min, max ) );
		return Builder;
	}

	public FieldRuleBuilder<IEnumerable<T>> All( Func<T, bool> predicate ) {
		Builder.AddRule( new AllRule<T>( predicate ) );
		return Builder;
	}

	public FieldRuleBuilder<IEnumerable<T>> Any( Func<T, bool> predicate ) {
		Builder.AddRule( new AnyRule<T>( predicate ) );
		return Builder;
	}

	public FieldRuleBuilder<IEnumerable<T>> None( Func<T, bool> predicate ) {
		Builder.AddRule( new NoneRule<T>( predicate ) );
		return Builder;
	}

	public FieldRuleBuilder<IEnumerable<T>> Distinct( ) {
		Builder.AddRule( new DistinctRule<T>( ) );
		return Builder;
	}

	public FieldRuleBuilder<IEnumerable<T>> DistinctBy<TKey>( Func<T, TKey> keySelector ) {
		Builder.AddRule( new DistinctByRule<T, TKey>( keySelector ) );
		return Builder;
	}
}
