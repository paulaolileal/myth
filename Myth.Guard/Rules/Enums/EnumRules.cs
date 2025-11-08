using Myth.Interfaces.Rules;
using Myth.Rules.Base;

namespace Myth.Rules.Enums {

	/// <summary>
	/// Enum rule configurator
	/// </summary>
	public sealed class EnumRules<T> : RuleConfigurator<T>, IEnumRules<T> where T : struct, Enum {

		public EnumRules( FieldRuleBuilder<T> builder ) : base( builder ) {
		}

		/// <summary>
		/// Adds a validation rule that requires the field value to be a defined member of the specified enumeration type.
		/// </summary>
		/// <remarks>This method is typically used when validating fields that are expected to correspond to an
		/// enumeration value. The rule ensures that the field value matches one of the valid enum members for type
		/// <typeparamref name="T"/>.</remarks>
		/// <returns>A builder instance that can be used to configure additional validation rules for the field.</returns>
		public FieldRuleBuilder<T> BeInEnum( ) {
			Builder.AddRule( new BeInEnumRule<T>( ) );
			return Builder;
		}

		/// <summary>
		/// Adds a validation rule that requires the field value to not be defined in the target enumeration type.
		/// </summary>
		/// <remarks>Use this method when you want to ensure that the field value does not match any value defined in
		/// the enumeration type <typeparamref name="T"/>. This is useful for excluding specific enum values from being
		/// considered valid input.</remarks>
		/// <returns>A <see cref="FieldRuleBuilder{T}"/> instance that can be used to configure additional validation rules for the
		/// field.</returns>
		public FieldRuleBuilder<T> BeNotInEnum( ) {
			Builder.AddRule( new BeNotInEnumRule<T>( ) );
			return Builder;
		}

		/// <summary>
		/// Restricts the field to accept only the specified values.
		/// </summary>
		/// <remarks>Use this method to enforce that the field can only be set to one of the provided values. This is
		/// useful for scenarios where the field must match a predefined set of options.</remarks>
		/// <param name="values">An array of allowed values of type <typeparamref name="T"/>. The field will only accept values contained in this
		/// array.</param>
		/// <returns>The current <see cref="FieldRuleBuilder{T}"/> instance for method chaining.</returns>
		public FieldRuleBuilder<T> Only( params T[ ] values ) {
			Builder.AddRule( new OnlyEnumValuesRule<T>( values ) );
			return Builder;
		}

		/// <summary>
		/// Adds a rule that validates whether the field value is a defined member of the specified enumeration type.
		/// </summary>
		/// <remarks>This method is intended for use with fields of an enumeration type. The rule ensures that the
		/// value assigned to the field corresponds to a valid enum member, preventing invalid or undefined values from being
		/// accepted.</remarks>
		/// <returns>The current <see cref="FieldRuleBuilder{T}"/> instance to allow for method chaining.</returns>
		public FieldRuleBuilder<T> IsValidEnumValue( ) {
			Builder.AddRule( new IsValidEnumValueRule<T>( ) );
			return Builder;
		}
	}
}