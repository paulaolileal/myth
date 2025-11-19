using System.Text.Json;
using Myth.Tool.Models;

namespace Myth.Tool.Services;

/// <summary>
/// Service for code generation
/// </summary>
public class CodeGenerationService {
	private readonly TemplateService _templateService;

	/// <summary>
	/// Initializes a new instance of the <see cref="CodeGenerationService"/> class
	/// </summary>
	public CodeGenerationService( ) {
		_templateService = new TemplateService( );
	}

	/// <summary>
	/// Generates code artifacts
	/// </summary>
	/// <param name="context">Generation context</param>
	/// <param name="artifactType">Type of artifact to generate</param>
	/// <returns>Generated artifacts</returns>
	public async Task<List<CodeArtifact>> GenerateAsync( GenerationContext context, string artifactType ) {
		var artifacts = new List<CodeArtifact>( );

		// Determine what templates to use based on the artifact type
		var templates = GetTemplatesForType( artifactType );

		foreach ( var template in templates ) {
			var artifact = await GenerateArtifactAsync( template, context );
			if ( artifact != null ) {
				artifacts.Add( artifact );
			}
		}

		// Write files if not dry run
		if ( !context.DryRun ) {
			foreach ( var artifact in artifacts ) {
				await WriteArtifactAsync( artifact, context.Force );
			}
		}

		return artifacts;
	}


	private static List<string> GetTemplatesForType( string type ) {
		return type.ToLowerInvariant( ) switch {
			"command" => [ "command", "command-handler" ],
			"query" => [ "query", "query-handler" ],
			"event" => [ "event", "event-handler" ],
			"dto" => [ "dto" ],
			"model" => [ "entity" ],
			"repository" => [ "repository-interface", "repository" ],
			"controller" => [ "controller" ],
			_ => [ type ]
		};
	}

	private async Task<CodeArtifact?> GenerateArtifactAsync( string templateName, GenerationContext context ) {
		try {
			var content = _templateService.RenderTemplate( templateName, context );
			var filePath = GetFilePath( templateName, context );

			return new CodeArtifact {
				FilePath = filePath,
				Content = content,
				Type = templateName,
				Exists = File.Exists( filePath )
			};
		} catch ( Exception ex ) {
			Console.WriteLine( $"Failed to generate {templateName}: {ex.Message}" );
			return null;
		}
	}

	private static string GetFilePath( string templateName, GenerationContext context ) {
		var fileName = templateName switch {
			"command" => $"{context.Name}.cs",
			"command-handler" => $"{context.Name}Handler.cs",
			"query" => $"{context.Name}.cs",
			"query-handler" => $"{context.Name}Handler.cs",
			"event" => $"{context.Name}.cs",
			"event-handler" => $"{context.Name}Handler.cs",
			"dto" => $"{context.Name}.cs",
			"entity" => $"{context.Name}.cs",
			"repository-interface" => $"I{context.Name}Repository.cs",
			"repository" => $"{context.Name}Repository.cs",
			"controller" => $"{context.Name}.cs",
			"command-test" => $"Create{context.Aggregate}CommandTests.cs",
			"query-test" => $"Get{context.Aggregate}QueryTests.cs",
			"controller-test" => $"{context.Aggregate}ControllerTests.cs",
			_ => $"{context.Name}.cs"
		};

		var folderPath = GetFolderPath( templateName, context );

		return Path.Combine( context.TargetPath, folderPath, fileName );
	}

	private static string GetFolderPath( string templateName, GenerationContext context ) {
		var structure = context.ProjectStructure;
		var aggregate = context.Aggregate;

		return templateName switch {
			"command" => GetCommandPath( structure, aggregate, context.Name ),
			"command-handler" => GetCommandPath( structure, aggregate, context.Name ),
			"query" => GetQueryPath( structure, aggregate, context.Name ),
			"query-handler" => GetQueryPath( structure, aggregate, context.Name ),
			"event" => GetEventPath( structure, aggregate ),
			"event-handler" => GetEventPath( structure, aggregate ),
			"dto" => GetDtoPath( structure, aggregate ),
			"entity" => GetEntityPath( structure, aggregate ),
			"repository" => GetRepositoryPath( structure, aggregate ),
			"repository-interface" => GetRepositoryInterfacePath( structure, aggregate ),
			"controller" => GetControllerPath( structure ),
			_ => "."
		};
	}

	private static string GetCommandPath( ProjectStructure? structure, string aggregate, string commandName ) {
		var basePath = structure?.ApplicationPath ?? Path.Combine( ".", $"YourProject.Application" );

		return Path.Combine( basePath, aggregate, "Commands" );
	}

	private static string GetQueryPath( ProjectStructure? structure, string aggregate, string queryName ) {
		var basePath = structure?.ApplicationPath ?? Path.Combine( ".", $"YourProject.Application" );

		return Path.Combine( basePath, aggregate, "Queries" );
	}

	private static string GetEventPath( ProjectStructure? structure, string aggregate ) {
		var basePath = structure?.ApplicationPath ?? Path.Combine( ".", $"YourProject.Application" );

		return Path.Combine( basePath, aggregate, "Events" );
	}

	private static string GetDtoPath( ProjectStructure? structure, string aggregate ) {
		var basePath = structure?.ApplicationPath ?? Path.Combine( ".", $"YourProject.Application" );

		return Path.Combine( basePath, aggregate, "DTOs" );
	}

	private static string GetEntityPath( ProjectStructure? structure, string aggregate ) {
		var basePath = structure?.DomainPath ?? Path.Combine( ".", $"YourProject.Domain" );

		return Path.Combine( basePath, "Models" );
	}

	private static string GetRepositoryPath( ProjectStructure? structure, string aggregate ) {
		var basePath = structure?.DataPath ?? Path.Combine( ".", $"YourProject.Data" );

		return Path.Combine( basePath, "Repositories" );
	}

	private static string GetRepositoryInterfacePath( ProjectStructure? structure, string aggregate ) {
		var basePath = structure?.DomainPath ?? Path.Combine( ".", $"YourProject.Domain" );

		return Path.Combine( basePath, "Interfaces" );
	}

	private static string GetControllerPath( ProjectStructure? structure ) {
		var basePath = structure?.ApiPath ?? Path.Combine( ".", $"YourProject.Api" );

		return Path.Combine( basePath, "Controllers" );
	}



	private static async Task WriteArtifactAsync( CodeArtifact artifact, bool force ) {
		if ( File.Exists( artifact.FilePath ) && !force ) {
			Console.WriteLine( $"File already exists: {artifact.FilePath}. Use --force to overwrite." );

			return;
		}

		var directory = Path.GetDirectoryName( artifact.FilePath );
		if ( !string.IsNullOrEmpty( directory ) ) {
			Directory.CreateDirectory( directory );
		}

		await File.WriteAllTextAsync( artifact.FilePath, artifact.Content );
	}

}
