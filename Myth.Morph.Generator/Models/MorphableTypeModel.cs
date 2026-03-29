using Myth.Morph.Generator.Helpers;

namespace Myth.Morph.Generator.Models;

/// <summary>
/// Represents a single source → destination mapping pair discovered by the generator.
/// One of these is created per <c>IMorphableTo&lt;TDest&gt;</c> or <c>IMorphableFrom&lt;TSrc&gt;</c>
/// interface implementation found in the compilation.
/// </summary>
/// <param name="SourceTypeFullName">
/// Fully-qualified source type name in <c>global::</c> format
/// (e.g. <c>global::MyApp.Entities.UserEntity</c>).
/// </param>
/// <param name="SourceTypeSafeName">
/// Sanitized source type name safe to use as a C# identifier
/// (e.g. <c>MyApp_Entities_UserEntity</c>).
/// </param>
/// <param name="DestinationTypeFullName">
/// Fully-qualified destination type name in <c>global::</c> format.
/// </param>
/// <param name="DestinationTypeSafeName">
/// Sanitized destination type name safe to use as a C# identifier.
/// </param>
/// <param name="Properties">
/// The set of same-name, type-compatible properties to auto-map.
/// Complex nested types are excluded — they are handled by the reflection fallback.
/// </param>
internal sealed record MorphableTypeModel(
	string SourceTypeFullName,
	string SourceTypeSafeName,
	string DestinationTypeFullName,
	string DestinationTypeSafeName,
	EquatableArray<PropertyMappingModel> Properties
) {
	/// <summary>
	/// Unique identifier used as the generated class name and hint file name.
	/// Combines both safe names to avoid collisions in the <c>Myth.Morph.Generated</c> namespace.
	/// </summary>
	public string AutoMapperClassName => $"{SourceTypeSafeName}_{DestinationTypeSafeName}_AutoMapper";
}
