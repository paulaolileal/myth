using Myth.Rules.Base;

namespace Myth.Interfaces.Rules {

	/// <summary>
	/// Base interface for rule configurations
	/// </summary>
	public interface IRuleConfigurator<T> {

		FieldRuleBuilder<T> Respect( Func<T, bool> predicate );

		FieldRuleBuilder<T> RespectAsync( Func<T, CancellationToken, IServiceProvider, Task<bool>> predicate );

		FieldRuleBuilder<T> EqualsTo( T value );

		FieldRuleBuilder<T> NotEqualsTo( T value );

		FieldRuleBuilder<T> BeNull( );

		FieldRuleBuilder<T> NotNull( );

		FieldRuleBuilder<T> BeDefault( );

		FieldRuleBuilder<T> NotDefault( );
	}
}