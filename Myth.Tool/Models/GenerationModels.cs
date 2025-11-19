using System.Text.Json;

namespace Myth.Tool.Models;

/// <summary>
/// Context for code generation
/// </summary>
public class GenerationContext {
	/// <summary>
	/// Gets or sets the aggregate name
	/// </summary>
	public required string Aggregate { get; set; }

	/// <summary>
	/// Gets or sets the artifact name
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Gets or sets the target namespace
	/// </summary>
	public required string Namespace { get; set; }

	/// <summary>
	/// Gets or sets the properties
	/// </summary>
	public List<Property> Properties { get; set; } = [ ];

	/// <summary>
	/// Gets or sets the project structure
	/// </summary>
	public ProjectStructure? ProjectStructure { get; set; }

	/// <summary>
	/// Gets or sets generation options
	/// </summary>
	public Dictionary<string, object> Options { get; set; } = [ ];

	/// <summary>
	/// Gets or sets the target path
	/// </summary>
	public required string TargetPath { get; set; }

	/// <summary>
	/// Gets or sets whether this is a dry run
	/// </summary>
	public bool DryRun { get; set; }

	/// <summary>
	/// Gets or sets whether to force overwrite existing files
	/// </summary>
	public bool Force { get; set; }

	/// <summary>
	/// Gets or sets the return type
	/// </summary>
	public string? ReturnType { get; set; }

	/// <summary>
	/// Gets or sets whether validation is enabled
	/// </summary>
	public bool HasValidation { get; set; }

	/// <summary>
	/// Gets or sets whether events should be published
	/// </summary>
	public bool PublishesEvents { get; set; }

	/// <summary>
	/// Gets or sets the events to publish
	/// </summary>
	public List<string> Events { get; set; } = [ ];

	/// <summary>
	/// Gets or sets the repository type (read|write|readwrite)
	/// </summary>
	public string? RepositoryType { get; set; }
}

/// <summary>
/// Represents a property in generated code
/// </summary>
public class Property {
	/// <summary>
	/// Gets or sets the property name
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Gets or sets the property type
	/// </summary>
	public required string Type { get; set; }

	/// <summary>
	/// Gets or sets the access modifier
	/// </summary>
	public string AccessModifier { get; set; } = "get; set;";

	/// <summary>
	/// Gets or sets whether this property is required
	/// </summary>
	public bool IsRequired { get; set; } = true;

	/// <summary>
	/// Gets whether this is a nullable type
	/// </summary>
	public bool IsNullable => Type.EndsWith( "?" ) || Type.StartsWith( "Nullable<" );
}

/// <summary>
/// Project structure configuration
/// </summary>
public class ProjectStructure {
	/// <summary>
	/// Gets or sets the root path
	/// </summary>
	public required string RootPath { get; set; }

	/// <summary>
	/// Gets or sets the API project path
	/// </summary>
	public string? ApiPath { get; set; }

	/// <summary>
	/// Gets or sets the domain project path
	/// </summary>
	public string? DomainPath { get; set; }

	/// <summary>
	/// Gets or sets the application project path
	/// </summary>
	public string? ApplicationPath { get; set; }

	/// <summary>
	/// Gets or sets the data project path
	/// </summary>
	public string? DataPath { get; set; }

	/// <summary>
	/// Gets or sets the test project path
	/// </summary>
	public string? TestPath { get; set; }

	/// <summary>
	/// Gets or sets the base namespace
	/// </summary>
	public string? BaseNamespace { get; set; }

	/// <summary>
	/// Gets namespace for specific layer
	/// </summary>
	/// <param name="layer">Layer name</param>
	/// <returns>Full namespace</returns>
	public string GetNamespace( string layer ) => $"{BaseNamespace}.{layer}";
}


/// <summary>
/// Generated code artifact
/// </summary>
public class CodeArtifact {
	/// <summary>
	/// Gets or sets the file path
	/// </summary>
	public required string FilePath { get; set; }

	/// <summary>
	/// Gets or sets the content
	/// </summary>
	public required string Content { get; set; }

	/// <summary>
	/// Gets or sets the artifact type
	/// </summary>
	public required string Type { get; set; }

	/// <summary>
	/// Gets or sets whether file exists
	/// </summary>
	public bool Exists { get; set; }
}
