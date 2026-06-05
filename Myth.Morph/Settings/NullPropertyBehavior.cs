namespace Myth.Settings;

/// <summary>
/// Defines how the mapping system handles null source property values during auto-mapping.
/// </summary>
public enum NullPropertyBehavior {

	/// <summary>
	/// Assigns <c>default(T)</c> to non-nullable value-type destination properties when the source value is null.
	/// Reference-type destination properties are left unchanged (null is allowed).
	/// This is the default behavior, preserving backward compatibility.
	/// </summary>
	AssignDefault,

	/// <summary>
	/// Skips the destination property entirely when the source value is null,
	/// leaving it with whatever value it was initialized with (e.g., the default declared in the constructor).
	/// Useful when destination objects have meaningful default values that should not be overwritten.
	/// </summary>
	Skip,

	/// <summary>
	/// Throws a <see cref="Myth.Exceptions.MorphPropertyException"/> when the source value is null
	/// and the destination property is a non-nullable type.
	/// Use this when null source values indicate a programming error that should surface immediately.
	/// </summary>
	Throw
}
