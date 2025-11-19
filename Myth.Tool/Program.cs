using System.Text.Json;
using Myth.Tool.Models;
using Myth.Tool.Services;

namespace Myth.Tool;

/// <summary>
/// Entry point for the Myth code generation tool
/// </summary>
public class Program {
	private static bool _verbose = false;

	/// <summary>
	/// Main entry point
	/// </summary>
	/// <param name="args">Command line arguments</param>
	/// <returns>Exit code</returns>
	public static async Task<int> Main( string[ ] args ) {
		if ( args.Length == 0 || args[ 0 ] == "--help" || args[ 0 ] == "-h" ) {
			ShowHelp( );
			return 0;
		}

		// Check for verbose flag
		_verbose = args.Contains( "--verbose" ) || args.Contains( "-v" );

		var command = args[ 0 ].ToLowerInvariant( );

		return command switch {
			"setup" => await HandleSetupCommand( args ),
			"create" => await HandleCreateCommand( args ),
			"version" => HandleVersionCommand( ),
			_ => HandleUnknownCommand( command )
		};
	}

	private static async Task<int> HandleSetupCommand( string[ ] args ) {
		try {
			if ( args.Contains( "-h" ) || args.Contains( "--help" ) ) {
				ShowSetupHelp( );
				return 0;
			}

			if ( args.Length < 2 ) {
				WriteError( "Project name is required. Use: myth setup <ProjectName>" );
				return 1;
			}

			var projectName = args[ 1 ];
			var clean = false;

			for ( var i = 2; i < args.Length; i++ ) {
				switch ( args[ i ].ToLowerInvariant( ) ) {
					case "--clean":
						clean = true;
						break;
				}
			}

			if ( string.IsNullOrEmpty( projectName ) ) {
				WriteError( "Project name is required. Use: myth setup <ProjectName>" );
				return 1;
			}

			if ( !IsValidProjectName( projectName ) ) {
				WriteError( "Project name must contain only letters, numbers, dots, hyphens or underscores." );
				return 1;
			}

			WriteVerbose( $"Setting up template for project: {projectName}" );

			// Rename directories
			WriteVerbose( "Renaming folders..." );
			var directories = Directory.GetDirectories( ".", "*Myth.Template*", SearchOption.TopDirectoryOnly );
			foreach ( var dir in directories ) {
				var dirName = Path.GetFileName( dir );
				var newName = dirName.Replace( "Myth.Template", projectName );
				Directory.Move( dir, newName );
				WriteVerbose( $"  {dirName} -> {newName}" );
			}

			// Rename files
			WriteVerbose( "Renaming files..." );
			var files = Directory.GetFiles( ".", "*Myth.Template*", SearchOption.AllDirectories );
			foreach ( var file in files ) {
				try {
					var fileName = Path.GetFileName( file );
					var newName = fileName.Replace( "Myth.Template", projectName );
					var newPath = Path.Combine( Path.GetDirectoryName( file )!, newName );
					File.Move( file, newPath );
					WriteVerbose( $"  {fileName} -> {newName}" );
				} catch ( Exception ex ) {
					WriteVerbose( $"  Warning: Could not rename {Path.GetFileName( file )}: {ex.Message}" );
				}
			}

			// Update content
			WriteVerbose( "Updating content..." );
			var contentFiles = Directory.GetFiles( ".", "*", SearchOption.AllDirectories )
				.Where( f => {
					var ext = Path.GetExtension( f ).ToLower( );
					return ext is ".cs" or ".csproj" or ".slnx" or ".json" or ".resx" or ".md";
				} );

			foreach ( var file in contentFiles ) {
				var content = await File.ReadAllTextAsync( file );
				if ( content.Contains( "Myth.Template" ) ) {
					var newContent = content.Replace( "Myth.Template", projectName );
					await File.WriteAllTextAsync( file, newContent );
					WriteVerbose( $"  Updated: {Path.GetFileName( file )}" );
				}
			}

			// Clean examples if requested
			if ( clean ) {
				WriteVerbose( "Cleaning WeatherForecast examples..." );

				var filesToRemove = new[ ] {
					$"{projectName}.Domain/Models/WeatherForecast.cs",
					$"{projectName}.Domain/Models/Summary.cs",
					$"{projectName}.Domain/Interfaces/IWeatherForecastRepository.cs",
					$"{projectName}.Domain/Specifications/WeatherForecastSpecification.cs",
					$"{projectName}.Data/Contexts/ForecastContext.cs",
					$"{projectName}.Data/Repositories/WeatherForecastRepository.cs",
					$"{projectName}.Data/Mappings/WeatherForecastMap.cs",
					$"{projectName}.Application/InitializeFakeData.cs",
					$"{projectName}.API/Controllers/WeatherForecastController.cs",
					$"{projectName}.Test/WeatherForecastTests.cs"
				};

				foreach ( var file in filesToRemove ) {
					if ( File.Exists( file ) ) {
						File.Delete( file );
						WriteVerbose( $"  Removed: {file}" );
					}
				}

				if ( Directory.Exists( $"{projectName}.Application/WeatherForecasts" ) ) {
					Directory.Delete( $"{projectName}.Application/WeatherForecasts", true );
					WriteVerbose( "  Removed: WeatherForecasts folder" );
				}

				// Create simple AppContext
				WriteVerbose( "Creating base AppContext..." );
				var contextDir = $"{projectName}.Data/Contexts";
				Directory.CreateDirectory( contextDir );

				var contextContent = $$"""
					using Microsoft.EntityFrameworkCore;
					using Myth.Contexts;

					namespace {{projectName}}.Data.Contexts;

					public class AppContext : BaseContext
					{
					    public AppContext(DbContextOptions<AppContext> options) : base(options)
					    {
					    }

					    protected override void OnModelCreating(ModelBuilder modelBuilder)
					    {
					        base.OnModelCreating(modelBuilder);
					        // Configure your models here
					    }
					}
					""";

				await File.WriteAllTextAsync( Path.Combine( contextDir, "AppContext.cs" ), contextContent );
				WriteVerbose( "  Created AppContext.cs" );
			}

			// Reset Git
			WriteVerbose( "Resetting Git..." );
			if ( Directory.Exists( ".git" ) ) {
				Directory.Delete( ".git", true );
			}

			RunCommand( "git", "init" );
			RunCommand( "git", "branch -m main" );
			RunCommand( "git", "add ." );

			var commitMessage = clean
				? $"feat: setup {projectName} from myth-template with clean structure"
				: $"feat: setup {projectName} from myth-template with examples";

			RunCommand( "git", $"commit -m \"{commitMessage}\"" );

			WriteSuccess( $"Done! Project renamed to: {projectName}" );

			if ( clean ) {
				WriteVerbose( "WeatherForecast examples removed, base AppContext created" );
			} else {
				WriteVerbose( "WeatherForecast examples kept for reference" );
			}

			WriteVerbose( "\nNext steps:" );
			WriteVerbose( "1. git remote add origin <repository-url>" );
			WriteVerbose( "2. Update appsettings.json" );
			WriteVerbose( "3. Run 'dotnet build' to verify" );

			return 0;
		} catch ( Exception ex ) {
			WriteError( $"Error: {ex.Message}" );
			return 1;
		}
	}

	private static async Task<int> HandleCreateCommand( string[ ] args ) {
		try {
			if ( args.Contains( "-h" ) || args.Contains( "--help" ) ) {
				ShowCreateHelp( );
				return 0;
			}

			if ( args.Length < 3 ) {
				WriteError( "Invalid syntax. Use: myth create <type> <name> [options] OR myth create <type> <aggregate> <name> [options]" );
				return 1;
			}

			var type = args[ 1 ];

			// Handle test subcommand
			if ( type.ToLowerInvariant( ) == "test" ) {
				return await HandleTestCommand( args );
			}

			// Determine if this type requires an aggregate
			var requiresAggregate = type.ToLowerInvariant( ) switch {
				"command" or "query" or "event" or "dto" => true,
				_ => false
			};

			string aggregate;
			string providedName;
			int startOptionsAt;

			if ( requiresAggregate ) {
				if ( args.Length < 4 ) {
					WriteError( $"Type '{type}' requires aggregate. Use: myth create {type} <aggregate> <name> [options]" );
					return 1;
				}
				aggregate = args[ 2 ];
				providedName = args[ 3 ];
				startOptionsAt = 4;
			} else {
				aggregate = args[ 2 ]; // For non-aggregate types, use name as aggregate too
				providedName = args[ 2 ];
				startOptionsAt = 3;
			}

			var properties = new List<Property>( );
			string? returnType = null;
			var hasValidation = false;
			var events = new List<string>( );
			var targetPath = Environment.CurrentDirectory;
			var customNamespace = "";
			var dryRun = false;
			var force = false;
			var repositoryType = "readwrite"; // default

			for ( var i = startOptionsAt; i < args.Length; i++ ) {
				switch ( args[ i ].ToLowerInvariant( ) ) {
					case "-p" when i + 1 < args.Length:
						var propParts = args[ ++i ].Split( ':' );
						if ( propParts.Length >= 2 ) {
							properties.Add( new Property {
								Name = propParts[ 0 ],
								Type = propParts[ 1 ],
								IsRequired = propParts.Length > 2 && propParts[ 2 ] == "required"
							} );
						}
						break;
					case "--return" when i + 1 < args.Length:
						returnType = args[ ++i ];
						break;
					case "--validate":
						hasValidation = true;
						break;
					case "--events" when i + 1 < args.Length:
						events.AddRange( args[ ++i ].Split( ',' ) );
						break;
					case "--path" when i + 1 < args.Length:
						targetPath = args[ ++i ];
						break;
					case "--namespace" when i + 1 < args.Length:
						customNamespace = args[ ++i ];
						break;
					case "--dry-run":
						dryRun = true;
						break;
					case "--force":
						force = true;
						break;
					case "--type" when i + 1 < args.Length:
						repositoryType = args[ ++i ];
						break;
				}
			}

			var structure = DetectProjectStructure( targetPath );
			if ( structure == null ) {
				WriteError( "Unable to detect project structure. Make sure you're in the solution root directory." );
				return 1;
			}

			// Use the provided name (now required)
			var finalName = providedName;
			var context = new GenerationContext {
				Name = EnsureCorrectSuffix( type, finalName ),
				Aggregate = aggregate,
				Namespace = customNamespace.IsNullOrEmpty( ) ? GetNamespaceForType( structure, type ) : customNamespace,
				Properties = properties,
				ReturnType = returnType,
				HasValidation = hasValidation,
				PublishesEvents = events.Count > 0,
				Events = events,
				TargetPath = targetPath,
				ProjectStructure = structure,
				DryRun = dryRun,
				Force = force,
				RepositoryType = repositoryType
			};

			var codeGenerationService = new CodeGenerationService( );
			var artifacts = await codeGenerationService.GenerateAsync( context, type );

			if ( dryRun ) {
				WriteVerbose( "Files that would be created:" );
				foreach ( var artifact in artifacts ) {
					WriteVerbose( $"  Preview: {artifact.FilePath}" );
				}
				WriteVerbose( "\nThis was a dry run. Use without --dry-run to create files." );
			} else {
				WriteVerbose( "Generated files:" );
				foreach ( var artifact in artifacts ) {
					var status = artifact.Exists ? "Updated" : "Created";
					WriteVerbose( $"  {status}: {artifact.FilePath}" );
				}
				WriteSuccess( $"Successfully generated {artifacts.Count} file(s)" );
			}

			return 0;
		} catch ( Exception ex ) {
			WriteError( $"Error: {ex.Message}" );
			return 1;
		}
	}

	private static async Task<int> HandleTestCommand( string[ ] args ) {
		try {
			if ( args.Contains( "-h" ) || args.Contains( "--help" ) ) {
				ShowTestHelp( );
				return 0;
			}

			if ( args.Length < 3 ) {
				WriteError( "Invalid syntax. Use: myth create test <controller> [options]" );
				return 1;
			}

			var controllerName = args[ 2 ];
			var targetPath = Environment.CurrentDirectory;
			var dryRun = false;
			var force = false;

			for ( var i = 3; i < args.Length; i++ ) {
				switch ( args[ i ].ToLowerInvariant( ) ) {
					case "--path" when i + 1 < args.Length:
						targetPath = args[ ++i ];
						break;
					case "--dry-run":
						dryRun = true;
						break;
					case "--force":
						force = true;
						break;
				}
			}

			var structure = DetectProjectStructure( targetPath );
			if ( structure == null ) {
				WriteError( "Unable to detect project structure. Make sure you're in the solution root directory." );
				return 1;
			}

			var testFileName = $"{controllerName}Tests.cs";
			var testFilePath = Path.Combine( structure.TestPath!, testFileName );

			if ( File.Exists( testFilePath ) && !force ) {
				WriteError( $"File already exists: {testFilePath}. Use --force to overwrite." );
				return 1;
			}

			if ( dryRun ) {
				WriteVerbose( $"Preview: {testFilePath}" );
				WriteVerbose( "\nThis was a dry run. Use without --dry-run to create files." );
				return 0;
			}

			var testContent = GenerateTestContent( controllerName, structure );

			Directory.CreateDirectory( Path.GetDirectoryName( testFilePath )! );
			await File.WriteAllTextAsync( testFilePath, testContent );

			WriteVerbose( $"Created: {testFilePath}" );
			WriteSuccess( "Test file generated successfully" );

			return 0;
		} catch ( Exception ex ) {
			WriteError( $"Error: {ex.Message}" );
			return 1;
		}
	}

	private static int HandleVersionCommand( ) {
		Console.WriteLine( );
		Console.WriteLine( "    ╔═══════════════════════════════════════════════╗" );
		Console.WriteLine( "    ║                                               ║" );
		Console.WriteLine( "    ║      ███╗   ███╗██╗   ██╗████████╗██╗  ██╗    ║" );
		Console.WriteLine( "    ║      ████╗ ████║╚██╗ ██╔╝╚══██╔══╝██║  ██║    ║" );
		Console.WriteLine( "    ║      ██╔████╔██║ ╚████╔╝    ██║   ███████║    ║" );
		Console.WriteLine( "    ║      ██║╚██╔╝██║  ╚██╔╝     ██║   ██╔══██║    ║" );
		Console.WriteLine( "    ║      ██║ ╚═╝ ██║   ██║      ██║   ██║  ██║    ║" );
		Console.WriteLine( "    ║      ╚═╝     ╚═╝   ╚═╝      ╚═╝   ╚═╝  ╚═╝    ║" );
		Console.WriteLine( "    ║                                               ║" );
		Console.WriteLine( "    ║      Legendary Code Generation                ║" );
		Console.WriteLine( "    ║                                               ║" );
		Console.WriteLine( "    ╚═══════════════════════════════════════════════╝" );
		Console.WriteLine( );
		Console.WriteLine( "Myth Code Generation Tool" );
		Console.WriteLine( "Version: 1.0.0" );
		Console.WriteLine( "Framework: .NET 10.0" );
		Console.WriteLine( );
		Console.WriteLine( "Resources:" );
		Console.WriteLine( "   Libraries: https://github.com/paulaolileal/myth" );
		Console.WriteLine( "   Templates: https://github.com/paulaolileal/myth-template" );
		Console.WriteLine( );
		Console.WriteLine( "Created by:" );
		Console.WriteLine( "   Paula Leal" );
		Console.WriteLine( "   LinkedIn: https://linkedin.com/in/paulaolileal" );
		Console.WriteLine( );
		Console.WriteLine( "Clean Architecture • CQRS • DDD • Modern .NET" );

		return 0;
	}

	private static int HandleUnknownCommand( string command ) {
		WriteError( $"Unknown command: {command}" );
		WriteError( "Use 'myth --help' to see available commands." );
		return 1;
	}

	private static ProjectStructure? DetectProjectStructure( string path ) {
		// Find solution file
		var solutionFiles = Directory.GetFiles( path, "*.sln" ).Concat( Directory.GetFiles( path, "*.slnx" ) ).ToArray( );
		if ( solutionFiles.Length == 0 ) {
			return null;
		}

		var solutionFile = solutionFiles.First( );
		var solutionName = Path.GetFileNameWithoutExtension( solutionFile );

		// Detect project paths based on common naming patterns
		var structure = new ProjectStructure {
			RootPath = path,
			BaseNamespace = solutionName
		};

		structure.ApiPath = FindProjectPath( path, "Api", "API" ) ?? FindProjectPath( path, solutionName + ".Api", solutionName + ".API" );
		structure.ApplicationPath = FindProjectPath( path, "Application" ) ?? FindProjectPath( path, solutionName + ".Application" );
		structure.DomainPath = FindProjectPath( path, "Domain" ) ?? FindProjectPath( path, solutionName + ".Domain" );
		structure.DataPath = FindProjectPath( path, "Data" ) ?? FindProjectPath( path, solutionName + ".Data" );
		structure.TestPath = FindProjectPath( path, "Test" ) ?? FindProjectPath( path, solutionName + ".Test" );

		return structure;
	}

	private static string? FindProjectPath( string basePath, params string[ ] names ) {
		foreach ( var name in names ) {
			var fullPath = Path.Combine( basePath, name );
			if ( Directory.Exists( fullPath ) ) {
				return fullPath;
			}
		}
		return null;
	}

	private static string GetNamespaceForType( ProjectStructure structure, string type ) {
		return type switch {
			"command" or "query" or "event" or "dto" => $"{structure.BaseNamespace}.Application",
			"model" => $"{structure.BaseNamespace}.Domain.Models",
			"repository" => $"{structure.BaseNamespace}.Data.Repositories",
			"controller" => $"{structure.BaseNamespace}.API.Controllers",
			_ => $"{structure.BaseNamespace}.Application"
		};
	}

	private static string EnsureCorrectSuffix( string type, string name ) {
		return type switch {
			"command" => name.EndsWith( "Command", StringComparison.OrdinalIgnoreCase ) ? name : $"{name}Command",
			"query" => name.EndsWith( "Query", StringComparison.OrdinalIgnoreCase ) ? name : $"{name}Query",
			"event" => name.EndsWith( "Event", StringComparison.OrdinalIgnoreCase ) ? name : $"{name}Event",
			"controller" => name.EndsWith( "Controller", StringComparison.OrdinalIgnoreCase ) ? name : $"{name}Controller",
			_ => name
		};
	}

	private static string GenerateTestContent( string controllerName, ProjectStructure structure ) {
		var namespaceName = $"{structure.BaseNamespace}.Test";
		var apiNamespace = $"{structure.BaseNamespace}.API.Controllers";

		return $$"""
			using FluentAssertions;
			using Microsoft.AspNetCore.Mvc;
			using Microsoft.Extensions.DependencyInjection;
			using Myth.Exceptions;
			using Myth.Extensions;
			using Myth.Flow.Actions.Extensions;
			using Myth.Models.Results;
			using {{apiNamespace}};
			using {{structure.BaseNamespace}}.Data.Contexts;
			using Myth.Testing.Extensions;
			using Myth.Testing.Repositories;
			using Myth.ValueObjects;

			namespace {{namespaceName}};

			/// <summary>
			/// Integration test suite for the {{controllerName}}Controller API endpoints.
			/// Tests all CRUD operations including validation scenarios, error handling, and business logic.
			/// Inherits from BaseDatabaseTests to provide in-memory database testing capabilities.
			/// </summary>
			public class {{controllerName}}Tests : BaseDatabaseTests<AppContext> {
				/// <summary>
				/// The controller instance under test, initialized with all required dependencies.
				/// </summary>
				private readonly {{controllerName}}Controller _controller;

				/// <summary>
				/// Initializes a new instance of the {{controllerName}}Tests class.
				/// Sets up the dependency injection container with all required services for testing.
				/// </summary>
				public {{controllerName}}Tests() : base() {
					AddServices(services => {
						services.AddLogging();
						services.AddRepositories();
						services.AddUnitOfWorkForContext<AppContext>();
						services.AddScopedServiceProvider();
						services.AddMorph();
						services.AddGuard();
						services.AddFlow(conf => conf
							.UseExceptionFilter<ValidationException>()
							.UseActions(x => x
								.UseInMemory()
								.ScanAssemblies(typeof({{controllerName}}Controller).Assembly)));
					});

					_controller = CreateInstance<{{controllerName}}Controller>();
				}

				/// <summary>
				/// Test example - replace with actual tests for your controller
				/// </summary>
				[Fact]
				public async Task Example_Test() {
					// Arrange
					// TODO: Set up test data

					// Act
					// TODO: Call controller method

					// Assert
					// TODO: Verify results
					Assert.True(true); // Remove this and add real assertions
				}

				// TODO: Add more specific tests for your controller endpoints
				// Examples:
				// - Test GET endpoints with valid/invalid parameters
				// - Test POST endpoints with valid/invalid data
				// - Test PUT endpoints for updates
				// - Test DELETE endpoints
				// - Test validation scenarios
				// - Test error handling
			}
			""";
	}

	private static bool IsValidProjectName( string name ) {
		return !string.IsNullOrEmpty( name ) && name.All( c => char.IsLetterOrDigit( c ) || c == '.' || c == '_' || c == '-' );
	}

	private static void RunCommand( string command, string arguments ) {
		var process = new System.Diagnostics.Process {
			StartInfo = new System.Diagnostics.ProcessStartInfo {
				FileName = command,
				Arguments = arguments,
				UseShellExecute = false,
				CreateNoWindow = true
			}
		};
		process.Start( );
		process.WaitForExit( );
	}

	private static void WriteVerbose( string message ) {
		if ( _verbose ) {
			Console.WriteLine( message );
		}
	}

	private static void WriteSuccess( string message ) {
		if ( _verbose ) {
			Console.WriteLine( message );
		} else {
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine( "Done." );
			Console.ResetColor( );
		}
	}

	private static void WriteError( string message ) {
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine( message );
		Console.ResetColor( );
	}

	private static void ShowHelp( ) {
		Console.WriteLine( "Myth Code Generation Tool" );
		Console.WriteLine( );
		Console.WriteLine( "Available commands:" );
		Console.WriteLine( "  setup                Setup project from template" );
		Console.WriteLine( "  create               Generate code artifacts" );
		Console.WriteLine( "  version              Show version information" );
		Console.WriteLine( );
		Console.WriteLine( "Global options:" );
		Console.WriteLine( "  --verbose            Show detailed output" );
		Console.WriteLine( "  -h, --help           Show help information" );
		Console.WriteLine( );
		Console.WriteLine( "Use 'myth <command> -h' for help with specific commands." );
	}

	private static void ShowSetupHelp( ) {
		Console.WriteLine( "Setup project from myth-template" );
		Console.WriteLine( );
		Console.WriteLine( "Usage: myth setup <ProjectName> [options]" );
		Console.WriteLine( );
		Console.WriteLine( "Arguments:" );
		Console.WriteLine( "  <ProjectName>        Project name (required)" );
		Console.WriteLine( );
		Console.WriteLine( "Options:" );
		Console.WriteLine( "  --clean              Remove WeatherForecast examples" );
		Console.WriteLine( "  --verbose            Show detailed output" );
		Console.WriteLine( "  -h, --help           Show help information" );
		Console.WriteLine( );
		Console.WriteLine( "Examples:" );
		Console.WriteLine( "  myth setup MyProject" );
		Console.WriteLine( "  myth setup MyProject --clean" );
	}

	private static void ShowCreateHelp( ) {
		Console.WriteLine( "Generate code artifacts" );
		Console.WriteLine( );
		Console.WriteLine( "Usage:" );
		Console.WriteLine( "  myth create <type> <aggregate> <name> [options]    (for command, query, event, dto)" );
		Console.WriteLine( "  myth create <type> <name> [options]               (for model, repository, controller)" );
		Console.WriteLine( );
		Console.WriteLine( "Types:" );
		Console.WriteLine( "  command              Generate command and handler" );
		Console.WriteLine( "  query                Generate query and handler" );
		Console.WriteLine( "  event                Generate event and handler" );
		Console.WriteLine( "  dto                  Generate DTO" );
		Console.WriteLine( "  model                Generate domain model" );
		Console.WriteLine( "  repository           Generate repository and interface" );
		Console.WriteLine( "  controller           Generate controller" );
		Console.WriteLine( "  test                 Generate test file for controller" );
		Console.WriteLine( );
		Console.WriteLine( "Options:" );
		Console.WriteLine( "  -p <name:type>       Add property (e.g., -p Name:string)" );
		Console.WriteLine( "  --return <type>      Set return type (for commands/queries)" );
		Console.WriteLine( "  --validate           Add validation support" );
		Console.WriteLine( "  --events <events>    Events to publish (comma-separated)" );
		Console.WriteLine( "  --type <type>        Repository type: read|write|readwrite (default: readwrite)" );
		Console.WriteLine( "  --path <path>        Target directory" );
		Console.WriteLine( "  --namespace <ns>     Custom namespace" );
		Console.WriteLine( "  --dry-run            Preview without creating files" );
		Console.WriteLine( "  --force              Overwrite existing files" );
		Console.WriteLine( "  --verbose            Show detailed output" );
		Console.WriteLine( "  -h, --help           Show help information" );
		Console.WriteLine( );
		Console.WriteLine( "Arguments:" );
		Console.WriteLine( "  <type>               Artifact type to generate" );
		Console.WriteLine( "  <aggregate>          Aggregate/domain name (required for command, query, event, dto)" );
		Console.WriteLine( "  <name>               Specific name for the artifact" );
		Console.WriteLine( );
		Console.WriteLine( "Examples:" );
		Console.WriteLine( "  # With aggregate:" );
		Console.WriteLine( "  myth create command User CreateUser -p Name:string --validate" );
		Console.WriteLine( "  myth create query User GetUser -p Id:Guid --return GetUserResponse" );
		Console.WriteLine( "  myth create dto User GetUserDto -p Name:string" );
		Console.WriteLine( "  myth create event User UserCreated -p UserId:Guid" );
		Console.WriteLine( );
		Console.WriteLine( "  # Without aggregate:" );
		Console.WriteLine( "  myth create model User -p Name:string -p Email:string" );
		Console.WriteLine( "  myth create repository UserRepository --type readwrite" );
		Console.WriteLine( "  myth create controller UserController" );
		Console.WriteLine( "  myth create test UserController" );
	}

	private static void ShowTestHelp( ) {
		Console.WriteLine( "Generate test file for controller" );
		Console.WriteLine( );
		Console.WriteLine( "Usage: myth create test <controller> [options]" );
		Console.WriteLine( );
		Console.WriteLine( "Options:" );
		Console.WriteLine( "  --path <path>        Target directory" );
		Console.WriteLine( "  --dry-run            Preview without creating files" );
		Console.WriteLine( "  --force              Overwrite existing files" );
		Console.WriteLine( "  --verbose            Show detailed output" );
		Console.WriteLine( "  -h, --help           Show help information" );
		Console.WriteLine( );
		Console.WriteLine( "Examples:" );
		Console.WriteLine( "  myth create test WeatherForecast" );
		Console.WriteLine( "  myth create test User --force" );
	}
}

public static class StringExtensions {
	public static bool IsNullOrEmpty( this string? value ) {
		return string.IsNullOrEmpty( value );
	}
}
