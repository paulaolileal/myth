using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Myth.Contexts;
using Myth.Interfaces.Repositories.Base;
using Myth.Interfaces.Repositories.EntityFramework;
using Myth.Repositories.EntityFramework;
using Myth.Repositories.EntityFramework.Base;
using Myth.ValueProviders;

namespace Myth.Extensions;

public static class RepositoryExtensions {

	/// <summary>
	/// Registers a specific Unit of Work repository implementation in the dependency injection container
	/// </summary>
	/// <typeparam name="TUnitOfWork">The concrete implementation of UnitOfWorkRepository to register</typeparam>
	/// <param name="services">The service collection to add the registration to</param>
	/// <returns>The service collection for method chaining</returns>
	public static IServiceCollection AddUnitOfWork<TUnitOfWork>( this IServiceCollection services ) where TUnitOfWork : BaseUnitOfWorkRepository {
		services.AddScoped<IUnitOfWorkRepository, TUnitOfWork>( );

		return services;
	}

	/// <summary>
	/// Registers a generic Unit of Work repository implementation for the specified context in the dependency injection container
	/// </summary>
	/// <typeparam name="TContext">The specific BaseContext type that the UnitOfWorkRepository will operate on</typeparam>
	/// <param name="services">The service collection to add the registration to</param>
	/// <returns>The service collection for method chaining</returns>
	public static IServiceCollection AddUnitOfWorkForContext<TContext>( this IServiceCollection services ) where TContext : BaseContext {
		services.AddScoped<IUnitOfWorkRepository, UnitOfWorkRepository<TContext>>( );

		return services;
	}

	/// <summary>
	/// Automatically registers all repository implementations found in the application assemblies
	/// </summary>
	/// <param name="services">The service collection to add the registrations to</param>
	/// <param name="lifetime">The service lifetime for repository registrations (default: Scoped)</param>
	/// <returns>The service collection for method chaining</returns>
	public static IServiceCollection AddRepositories( this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped ) {
		var repositoryTypes = GetRepositoryImplementations( );

		foreach ( var repositoryType in repositoryTypes ) {
			var interfaces = GetRepositoryInterfaces( repositoryType );

			var interfaceType = interfaces.First( );

			services.Add( new ServiceDescriptor( interfaceType, repositoryType, lifetime ) );
		}
		return services;
	}

	/// <summary>
	/// Automatically registers all repository implementations found in the specified assembly
	/// </summary>
	/// <param name="services">The service collection to add the registrations to</param>
	/// <param name="assembly">The assembly to scan for repository implementations</param>
	/// <param name="lifetime">The service lifetime for repository registrations (default: Scoped)</param>
	/// <returns>The service collection for method chaining</returns>
	public static IServiceCollection AddRepositoriesFromAssembly( this IServiceCollection services, Assembly assembly, ServiceLifetime lifetime = ServiceLifetime.Scoped ) {
		var repositoryTypes = GetRepositoryImplementationsFromAssembly( assembly );

		foreach ( var repositoryType in repositoryTypes ) {
			var interfaces = GetRepositoryInterfaces( repositoryType );

			var interfaceType = interfaces.First( );

			services.Add( new ServiceDescriptor( interfaceType, repositoryType, lifetime ) );
		}

		return services;
	}

	/// <summary>
	/// Manually registers a specific repository implementation with its interface
	/// </summary>
	/// <typeparam name="TInterface">The repository interface type</typeparam>
	/// <typeparam name="TImplementation">The repository implementation type</typeparam>
	/// <param name="services">The service collection to add the registration to</param>
	/// <param name="lifetime">The service lifetime for the repository registration (default: Scoped)</param>
	/// <returns>The service collection for method chaining</returns>
	public static IServiceCollection AddRepository<TInterface, TImplementation>( this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped )
		where TInterface : class, IRepository
		where TImplementation : class, TInterface {
		services.Add( new ServiceDescriptor( typeof( TInterface ), typeof( TImplementation ), lifetime ) );

		return services;
	}

	/// <summary>
	/// Gets all concrete repository implementation types from application assemblies
	/// </summary>
	/// <returns>Collection of repository implementation types</returns>
	private static IEnumerable<Type> GetRepositoryImplementations( ) {
		return TypeProvider.GetTypesAssignableFrom<IRepository>( )
			.Where( type => type.IsClass && !type.IsAbstract && !type.IsGenericTypeDefinition && !IsTestType( type ) );
	}

	/// <summary>
	/// Gets all concrete repository implementation types from the specified assembly
	/// </summary>
	/// <param name="assembly">The assembly to scan</param>
	/// <returns>Collection of repository implementation types</returns>
	private static IEnumerable<Type> GetRepositoryImplementationsFromAssembly( Assembly assembly ) {
		return assembly.GetTypes( )
			.Where( type => typeof( IRepository ).IsAssignableFrom( type ) )
			.Where( type => type.IsClass && !type.IsAbstract && !type.IsGenericTypeDefinition && !IsTestType( type ) );
	}

	/// <summary>
	/// Gets the most appropriate interfaces for a repository implementation type
	/// </summary>
	/// <param name="repositoryType">The repository implementation type</param>
	/// <returns>Collection of interface types to register</returns>
	private static IEnumerable<Type> GetRepositoryInterfaces( Type repositoryType ) {
		var interfaces = repositoryType.GetInterfaces( );

		// Fallback: Use interface name matching strategy
		var matchedInterface = interfaces.FirstOrDefault( i => i.Name.Contains( repositoryType.Name ) );
		if ( matchedInterface != null )
			return [ matchedInterface ];

		throw new InvalidOperationException( $"The type {repositoryType.Name} must implement a interface with the same name" );
	}

	/// <summary>
	/// Determines if a type is a test-related type that should be excluded from registration
	/// </summary>
	/// <param name="type">The type to check</param>
	/// <returns>True if the type is test-related, false otherwise</returns>
	private static bool IsTestType( Type type ) {
		// Filter based on specific naming patterns rather than namespace
		return type.Name.EndsWith( "Test" ) ||
			   type.Name.EndsWith( "Tests" ) ||
			   type.Name.StartsWith( "Test" ) ||
			   type.Name.Contains( "Mock" ) ||
			   type.Name.Contains( "Fake" ) ||
			   type.Name.Contains( "Stub" );
	}
}
