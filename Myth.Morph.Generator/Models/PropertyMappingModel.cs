namespace Myth.Morph.Generator.Models;

/// <summary>
/// Represents a single auto-mappable property pair between a source and destination type.
/// Used by <see cref="Emitters.AutoMapperEmitter"/> to generate direct property assignments.
/// </summary>
/// <param name="Name">The property name (must be identical on both sides).</param>
/// <param name="SourcePropertyType">Fully-qualified source property type (e.g. <c>global::System.String</c>).</param>
/// <param name="DestinationPropertyType">Fully-qualified destination property type.</param>
/// <param name="RequiresNullCheck">
/// When <c>true</c>, the generated assignment wraps the value in a null-conditional guard
/// (e.g. <c>Nullable&lt;T&gt;</c> → <c>T</c> or nullable-annotated reference → non-nullable destination).
/// </param>
internal sealed record PropertyMappingModel(
    string Name,
    string SourcePropertyType,
    string DestinationPropertyType,
    bool RequiresNullCheck
);
