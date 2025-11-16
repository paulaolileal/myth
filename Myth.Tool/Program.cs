namespace Myth.Tool;

/// <summary>
/// Entry point for the Myth code generation tool
/// </summary>
public class Program {
	/// <summary>
	/// Main entry point
	/// </summary>
	/// <param name="args">Command line arguments</param>
	/// <returns>Exit code</returns>
	public static async Task<int> Main( string[ ] args ) {
		Console.WriteLine( "🔮 Myth Code Generation Tool v1.0.0" );
		Console.WriteLine( );

		if ( args.Length == 0 || args[ 0 ] == "--help" || args[ 0 ] == "-h" ) {
			ShowHelp( );

			return 0;
		}

		var command = args[ 0 ].ToLowerInvariant( );

		return command switch {
			"init" => await HandleInitCommand( args ),
			"version" => HandleVersionCommand( ),
			_ => HandleUnknownCommand( command )
		};
	}

	private static async Task<int> HandleInitCommand( string[ ] args ) {
		Console.WriteLine( "🚀 Initializing Myth project structure..." );
		Console.WriteLine( );

		try {
			// Parse options
			var targetPath = Environment.CurrentDirectory;
			var force = false;

			for ( var i = 1; i < args.Length; i++ ) {
				switch ( args[ i ].ToLowerInvariant( ) ) {
					case "--path" when i + 1 < args.Length:
						targetPath = args[ ++i ];

						break;

					case "--force":
						force = true;

						break;
				}
			}

			Console.WriteLine( $"📁 Target directory: {targetPath}" );
			Console.WriteLine( );

			// Create base directory structure
			var directories = new[ ]
			{
				"YourProject.Api",
				"YourProject.Api/Controllers",
				"YourProject.Domain",
				"YourProject.Domain/Models",
				"YourProject.Domain/Interfaces",
				"YourProject.Domain/Specifications",
				"YourProject.Application",
				"YourProject.Application/Common",
				"YourProject.Data",
				"YourProject.Data/Contexts",
				"YourProject.Data/Repositories",
				"YourProject.Data/Mappings",
				"YourProject.ExternalData",
				"YourProject.Test"
			};

			Console.WriteLine( "Creating directory structure..." );

			foreach ( var directory in directories ) {
				var fullPath = Path.Combine( targetPath, directory );

				Directory.CreateDirectory( fullPath );
				Console.WriteLine( $"✅ Created: {directory}" );
			}

			Console.WriteLine( );

			// Create myth.config.json
			var configContent = """
                {
                  "myth": {
                    "version": "1.0.0",
                    "projectStructure": "clean-architecture",
                    "namespaces": {
                      "domain": "YourProject.Domain",
                      "application": "YourProject.Application",
                      "infrastructure": "YourProject.Data",
                      "api": "YourProject.Api",
                      "test": "YourProject.Test"
                    },
                    "conventions": {
                      "fileNaming": "PascalCase",
                      "folderNaming": "PascalCase",
                      "testSuffix": "Tests"
                    }
                  }
                }
                """;

			var configPath = Path.Combine( targetPath, "myth.config.json" );

			if ( !File.Exists( configPath ) || force ) {
				await File.WriteAllTextAsync( configPath, configContent );
				Console.WriteLine( "✅ Created myth.config.json" );
			} else {
				Console.WriteLine( "⚠️  myth.config.json already exists. Use --force to overwrite." );
			}

			// Create README.md
			var readmeContent = """
                # YourProject

                A Myth-based project with Clean Architecture, CQRS, and DDD patterns.

                ## Project Structure

                ```
                YourProject/
                ├── 🏗️ YourProject.Api/                    # Web API Layer
                ├── 🎯 YourProject.Domain/                 # Domain Layer
                ├── 🔄 YourProject.Application/            # Application Layer
                ├── 💾 YourProject.Data/                   # Data Access Layer
                ├── 🌐 YourProject.ExternalData/           # External Integrations
                └── 🧪 YourProject.Test/                   # Test Projects
                ```

                ## Getting Started

                ### Future Code Generation Commands (Coming Soon)

                ```bash
                # Create domain models
                myth create model User \
                  -p UserId:Guid:get \
                  -p Name:string \
                  -p Email:string \
                  --validate

                # Create CQRS commands
                myth create command User CreateUser \
                  -p Name:string \
                  -p Email:string \
                  --return Guid \
                  --validate

                # Create CQRS queries
                myth create query User GetUser \
                  -p Id:Guid \
                  --return GetUserResponse

                # Generate repositories and controllers
                myth create repository User
                myth create controller User --include-crud
                ```

                ## Manual Setup for Now

                1. Update namespace configuration in `myth.config.json`
                2. Install Myth NuGet packages in your projects:
                   - Myth.Commons
                   - Myth.Flow.Actions
                   - Myth.Guard
                   - Myth.Repository
                   - Myth.Specification
                3. Start building your domain models and business logic

                ## Recommended Project File Structure

                ### Domain Project (YourProject.Domain.csproj)
                ```xml
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                    <ImplicitUsings>enable</ImplicitUsings>
                    <Nullable>enable</Nullable>
                  </PropertyGroup>

                  <ItemGroup>
                    <PackageReference Include="Myth.Commons" Version="1.0.0" />
                    <PackageReference Include="Myth.Guard" Version="1.0.0" />
                  </ItemGroup>
                </Project>
                ```

                ### Application Project (YourProject.Application.csproj)
                ```xml
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                    <ImplicitUsings>enable</ImplicitUsings>
                    <Nullable>enable</Nullable>
                  </PropertyGroup>

                  <ItemGroup>
                    <ProjectReference Include="../YourProject.Domain/YourProject.Domain.csproj" />
                  </ItemGroup>

                  <ItemGroup>
                    <PackageReference Include="Myth.Flow.Actions" Version="1.0.0" />
                    <PackageReference Include="Myth.Morph" Version="1.0.0" />
                  </ItemGroup>
                </Project>
                ```

                ### Data Project (YourProject.Data.csproj)
                ```xml
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFramework>net10.0</TargetFramework>
                    <ImplicitUsings>enable</ImplicitUsings>
                    <Nullable>enable</Nullable>
                  </PropertyGroup>

                  <ItemGroup>
                    <ProjectReference Include="../YourProject.Domain/YourProject.Domain.csproj" />
                  </ItemGroup>

                  <ItemGroup>
                    <PackageReference Include="Myth.Repository.EntityFramework" Version="1.0.0" />
                    <PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.0" />
                    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.0" />
                  </ItemGroup>
                </Project>
                ```

                For more information, visit: https://gitlab.com/dotnet-myth/myth
                """;

			var readmePath = Path.Combine( targetPath, "README.md" );

			if ( !File.Exists( readmePath ) || force ) {
				await File.WriteAllTextAsync( readmePath, readmeContent );
				Console.WriteLine( "✅ Created README.md with getting started guide" );
			} else {
				Console.WriteLine( "⚠️  README.md already exists. Use --force to overwrite." );
			}

			Console.WriteLine( );
			Console.WriteLine( "🎉 Myth project structure initialized successfully!" );
			Console.WriteLine( );
			Console.WriteLine( "Next steps:" );
			Console.WriteLine( "1. Update myth.config.json with your project details" );
			Console.WriteLine( "2. Create .csproj files using the examples in README.md" );
			Console.WriteLine( "3. Install Myth NuGet packages" );
			Console.WriteLine( "4. Start building your domain models" );
			Console.WriteLine( "5. Stay tuned for code generation features in future releases" );

			return 0;

		} catch ( Exception ex ) {
			Console.WriteLine( $"❌ Error: {ex.Message}" );

			return 1;
		}
	}

	private static int HandleVersionCommand( ) {
		Console.WriteLine( "Myth Code Generation Tool" );
		Console.WriteLine( "Version: 1.0.0" );
		Console.WriteLine( "Framework: .NET 10.0" );

		return 0;
	}

	private static int HandleUnknownCommand( string command ) {
		Console.WriteLine( $"❌ Unknown command: {command}" );
		Console.WriteLine( );
		ShowHelp( );

		return 1;
	}

	private static void ShowHelp( ) {
		Console.WriteLine( "Available commands:" );
		Console.WriteLine( "  init                 Initialize Myth project structure" );
		Console.WriteLine( "  version              Show version information" );
		Console.WriteLine( );
		Console.WriteLine( "Options for 'init' command:" );
		Console.WriteLine( "  --path <path>        Target directory (default: current)" );
		Console.WriteLine( "  --force              Overwrite existing files" );
		Console.WriteLine( );
		Console.WriteLine( "Examples:" );
		Console.WriteLine( "  myth init" );
		Console.WriteLine( "  myth init --path ./my-project" );
		Console.WriteLine( "  myth init --path ./my-project --force" );
		Console.WriteLine( );
		Console.WriteLine( "For more information, visit: https://gitlab.com/dotnet-myth/myth" );
	}
}
