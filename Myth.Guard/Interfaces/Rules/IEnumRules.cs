using Myth.Rules.Base;

namespace Myth.Interfaces.Rules {

	/// <summary>
	/// Provides validation rules specifically designed for enumeration values.
	/// </summary>
	/// <typeparam name="T">The enum type being validated.</typeparam>
	public interface IEnumRules<T> : IRuleConfigurator<T> where T : struct, Enum {

		/// <summary>
		/// Validates that the enum value is a valid defined member of the enumeration.
		/// This rule ensures the value is not an undefined enum value.
		/// </summary>
		/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// public enum Status { Active, Inactive, Pending }
		/// builder.For(x => x.UserStatus, r => r.BeInEnum()); // Must be Active, Inactive, or Pending
		/// </code>
		/// </example>
		FieldRuleBuilder<T> BeInEnum( );

		/// <summary>
		/// Validates that the enum value is not a valid defined member of the enumeration.
		/// This rule ensures the value is an undefined enum value (typically used for testing invalid scenarios).
		/// </summary>
		/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
		/// <example>
		/// <code>
		/// public enum Status { Active, Inactive, Pending }
		/// builder.For(x => x.InvalidStatus, r => r.BeNotInEnum()); // Must not be a valid Status value
		/// </code>
		/// </example>
		FieldRuleBuilder<T> BeNotInEnum( );

		/// <summary>
		/// Validates that the enum value matches one of the specified allowed values.
		/// This rule restricts the enum to only specific allowed members.
		/// </summary>
		/// <param name="values">The allowed enum values.</param>
		/// <returns>A <see cref="FieldRuleBuilder{T}"/> for method chaining.</returns>
		/// <exception cref="ArgumentNullException">Thrown when values array is null.</exception>
		/// <exception cref="ArgumentException">Thrown when values array is empty.</exception>
		/// <example>
		/// <code>
		/// public enum Priority { Low, Medium, High, Critical }
		/// builder.For(x => x.TaskPriority, r => r.Only(Priority.High, Priority.Critical)); // Only High or Critical allowed
		/// builder.For(x => x.UserRole, r => r.Only(Role.Admin, Role.User)); // Only Admin or User roles allowed
		/// </code>
		/// </example>
		FieldRuleBuilder<T> Only( params T[ ] values );
	}
}