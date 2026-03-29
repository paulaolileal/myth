namespace Myth.Exceptions;

/// <summary>
/// Exception thrown when a property mapping operation fails during object transformation.
/// Provides rich context including source and destination types and the property name that caused the failure,
/// making it easier to diagnose mapping errors compared to a generic <see cref="Exception"/>.
/// </summary>
/// <remarks>
/// This exception is raised in two scenarios:
/// <list type="bullet">
/// <item>When a manual binding (via <c>Schema.Bind</c>) throws during execution.</item>
/// <item>When auto-mapping encounters a null source value for a non-nullable destination property
/// and <see cref="Myth.Settings.NullPropertyBehavior.Throw"/> is configured.</item>
/// </list>
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="MorphPropertyException"/> class.
/// </remarks>
/// <param name="sourceType">The source type involved in the failed mapping.</param>
/// <param name="destinationType">The destination type involved in the failed mapping.</param>
/// <param name="propertyName">The name of the property that caused the failure.</param>
/// <param name="message">A message describing the mapping error.</param>
/// <param name="innerException">The original exception that caused this failure, if any.</param>
public class MorphPropertyException(
	Type sourceType,
	Type destinationType,
	string propertyName,
	string message,
	Exception? innerException = null ) : Exception( message, innerException ) {

	/// <summary>Gets the source type involved in the failed mapping.</summary>
	public Type SourceType { get; } = sourceType;

	/// <summary>Gets the destination type involved in the failed mapping.</summary>
	public Type DestinationType { get; } = destinationType;

	/// <summary>Gets the name of the property that caused the mapping failure.</summary>
	public string PropertyName { get; } = propertyName;

	/// <summary>
	/// Creates a <see cref="MorphPropertyException"/> with a standardized message from a mapping failure.
	/// </summary>
	/// <param name="sourceType">The source type involved in the failed mapping.</param>
	/// <param name="destinationType">The destination type involved in the failed mapping.</param>
	/// <param name="propertyName">The name of the property that caused the failure.</param>
	/// <param name="inner">The original exception that caused the failure.</param>
	/// <returns>A new <see cref="MorphPropertyException"/> with contextual information.</returns>
	public static MorphPropertyException Create(
		Type sourceType,
		Type destinationType,
		string propertyName,
		Exception inner )
		=> new(
			sourceType,
			destinationType,
			propertyName,
			$"Mapping failed for property '{propertyName}' ({sourceType.Name} -> {destinationType.Name}): {inner.Message}",
			inner );

	/// <summary>
	/// Creates a <see cref="MorphPropertyException"/> for a null source value violation.
	/// </summary>
	/// <param name="sourceType">The source type involved in the failed mapping.</param>
	/// <param name="destinationType">The destination type involved in the failed mapping.</param>
	/// <param name="propertyName">The name of the destination property that received a null source value.</param>
	/// <returns>A new <see cref="MorphPropertyException"/> describing the null violation.</returns>
	public static MorphPropertyException NullSourceValue(
		Type sourceType,
		Type destinationType,
		string propertyName )
		=> new(
			sourceType,
			destinationType,
			propertyName,
			$"Source value is null for non-nullable property '{propertyName}' ({sourceType.Name} -> {destinationType.Name})." );
}
