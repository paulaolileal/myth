using Myth.Interfaces.Rules;
using Myth.Rules.Base;

namespace Myth.Rules.Dictionaries;

/// <summary>
/// Dictionary rule configurator
/// </summary>
public sealed class DictionaryRules<TKey, TValue>( FieldRuleBuilder<IDictionary<TKey, TValue>> builder )
	: RuleConfigurator<IDictionary<TKey, TValue>>( builder ), IDictionaryRules<TKey, TValue> {

	public FieldRuleBuilder<IDictionary<TKey, TValue>> NotEmpty( ) {
		Builder.AddRule( new NotEmptyDictionaryRule<TKey, TValue>( ) );
		return Builder;
	}

	public FieldRuleBuilder<IDictionary<TKey, TValue>> CountGreaterThan( int min ) {
		Builder.AddRule( new DictionaryCountGreaterThanRule<TKey, TValue>( min ) );
		return Builder;
	}

	public FieldRuleBuilder<IDictionary<TKey, TValue>> CountLessThan( int max ) {
		Builder.AddRule( new DictionaryCountLessThanRule<TKey, TValue>( max ) );
		return Builder;
	}

	public FieldRuleBuilder<IDictionary<TKey, TValue>> CountBetween( int min, int max ) {
		Builder.AddRule( new DictionaryCountBetweenRule<TKey, TValue>( min, max ) );
		return Builder;
	}

	public FieldRuleBuilder<IDictionary<TKey, TValue>> ContainsKey( TKey key ) {
		Builder.AddRule( new ContainsKeyRule<TKey, TValue>( key ) );
		return Builder;
	}

	public FieldRuleBuilder<IDictionary<TKey, TValue>> NotContainsKey( TKey key ) {
		Builder.AddRule( new NotContainsKeyRule<TKey, TValue>( key ) );
		return Builder;
	}

	public FieldRuleBuilder<IDictionary<TKey, TValue>> ContainsValue( TValue value ) {
		Builder.AddRule( new ContainsValueRule<TKey, TValue>( value ) );
		return Builder;
	}

	public FieldRuleBuilder<IDictionary<TKey, TValue>> AllKeys( Func<TKey, bool> predicate ) {
		Builder.AddRule( new AllKeysRule<TKey, TValue>( predicate ) );
		return Builder;
	}

	public FieldRuleBuilder<IDictionary<TKey, TValue>> AllValues( Func<TValue, bool> predicate ) {
		Builder.AddRule( new AllValuesRule<TKey, TValue>( predicate ) );
		return Builder;
	}

	public FieldRuleBuilder<IDictionary<TKey, TValue>> AnyKey( Func<TKey, bool> predicate ) {
		Builder.AddRule( new AnyKeyRule<TKey, TValue>( predicate ) );
		return Builder;
	}

	public FieldRuleBuilder<IDictionary<TKey, TValue>> AnyValue( Func<TValue, bool> predicate ) {
		Builder.AddRule( new AnyValueRule<TKey, TValue>( predicate ) );
		return Builder;
	}

	public FieldRuleBuilder<IDictionary<TKey, TValue>> NoKeys( Func<TKey, bool> predicate ) {
		Builder.AddRule( new NoKeysRule<TKey, TValue>( predicate ) );
		return Builder;
	}

	public FieldRuleBuilder<IDictionary<TKey, TValue>> NoValues( Func<TValue, bool> predicate ) {
		Builder.AddRule( new NoValuesRule<TKey, TValue>( predicate ) );
		return Builder;
	}
}
