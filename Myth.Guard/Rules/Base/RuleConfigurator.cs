using Myth.Interfaces.Rules;
using Myth.Rules.Generics;

namespace Myth.Rules.Base {

	/// <summary>
	/// Generic rule configurator
	/// </summary>
	public class RuleConfigurator<T> : IRuleConfigurator<T> {
		protected readonly FieldRuleBuilder<T> Builder;

		public RuleConfigurator( FieldRuleBuilder<T> builder ) {
			Builder = builder;
		}

		public FieldRuleBuilder<T> Respect( Func<T, bool> predicate ) {
			Builder.AddRule( new CustomRule<T>( predicate ) );
			return Builder;
		}

		public FieldRuleBuilder<T> RespectAsync( Func<T, CancellationToken, IServiceProvider, Task<bool>> predicate ) {
			Builder.AddRule( new CustomRule<T>( predicate ) );
			return Builder;
		}

		public FieldRuleBuilder<T> EqualsTo( T value ) {
			Builder.AddRule( new EqualsRule<T>( value ) );
			return Builder;
		}

		public FieldRuleBuilder<T> NotEqualsTo( T value ) {
			Builder.AddRule( new NotEqualsRule<T>( value ) );
			return Builder;
		}

		public FieldRuleBuilder<T> BeNull( ) {
			Builder.AddRule( new BeNullRule<T>( ) );
			return Builder;
		}

		public FieldRuleBuilder<T> NotNull( ) {
			Builder.AddRule( new NotNullRule<T>( ) );
			return Builder;
		}

		public FieldRuleBuilder<T> BeDefault( ) {
			Builder.AddRule( new BeDefaultRule<T>( ) );
			return Builder;
		}

		public FieldRuleBuilder<T> NotDefault( ) {
			Builder.AddRule( new NotDefaultRule<T>( ) );
			return Builder;
		}
	}
}