using Myth.Interfaces;
using System.Reflection;

namespace Myth.Builders {

	/// <summary>
	/// Default assembly scanner implementation
	/// </summary>
	internal sealed class AssemblyScanner : IAssemblyScanner {

		/// <summary>
		/// Scans assemblies to find all command and query handler implementations
		/// </summary>
		/// <param name="assemblies">The assemblies to scan for handlers</param>
		/// <returns>A collection of tuples containing interface types and their implementation types</returns>
		public IEnumerable<(Type InterfaceType, Type ImplementationType)> ScanForHandlers( params Assembly[ ] assemblies ) {
			var results = new List<(Type, Type)>( );

			foreach ( var assembly in assemblies ) {
				var types = GetTypesFromAssembly( assembly );

				foreach ( var type in types ) {
					if ( !type.IsClass || type.IsAbstract )
						continue;

					var interfaces = type.GetInterfaces( );

					foreach ( var @interface in interfaces ) {
						if ( IsCommandHandler( @interface ) || IsQueryHandler( @interface ) ) {
							results.Add( (@interface, type) );
						}
					}
				}
			}

			return results;
		}

		/// <summary>
		/// Scans assemblies to find all event handler implementations
		/// </summary>
		/// <param name="assemblies">The assemblies to scan for event handlers</param>
		/// <returns>A collection of tuples containing event types and their handler types</returns>
		public IEnumerable<(Type EventType, Type HandlerType)> ScanForEventHandlers( params Assembly[ ] assemblies ) {
			var results = new List<(Type, Type)>( );

			foreach ( var assembly in assemblies ) {
				var types = GetTypesFromAssembly( assembly );

				foreach ( var type in types ) {
					if ( !type.IsClass || type.IsAbstract )
						continue;

					var interfaces = type.GetInterfaces( );

					foreach ( var @interface in interfaces ) {
						if ( IsEventHandler( @interface ) ) {
							var eventType = @interface.GetGenericArguments( )[ 0 ];
							results.Add( (eventType, type) );
						}
					}
				}
			}

			return results;
		}

		/// <summary>
		/// Safely retrieves all types from an assembly, handling ReflectionTypeLoadException
		/// </summary>
		/// <param name="assembly">The assembly to get types from</param>
		/// <returns>An array of types from the assembly</returns>
		private static Type[ ] GetTypesFromAssembly( Assembly assembly ) {
			try {
				return assembly.GetTypes( );
			} catch ( ReflectionTypeLoadException ex ) {
				return ex.Types.Where( t => t != null ).ToArray( )!;
			}
		}

		/// <summary>
		/// Checks if a type is a command handler interface
		/// </summary>
		/// <param name="type">The type to check</param>
		/// <returns>True if the type is ICommandHandler, otherwise false</returns>
		private static bool IsCommandHandler( Type type ) {
			if ( !type.IsGenericType )
				return false;

			var genericType = type.GetGenericTypeDefinition( );

			return genericType == typeof( ICommandHandler<> ) ||
				   genericType == typeof( ICommandHandler<,> );
		}

		/// <summary>
		/// Checks if a type is a query handler interface
		/// </summary>
		/// <param name="type">The type to check</param>
		/// <returns>True if the type is IQueryHandler, otherwise false</returns>
		private static bool IsQueryHandler( Type type ) {
			if ( !type.IsGenericType )
				return false;

			var genericType = type.GetGenericTypeDefinition( );

			return genericType == typeof( IQueryHandler<,> );
		}

		/// <summary>
		/// Checks if a type is an event handler interface
		/// </summary>
		/// <param name="type">The type to check</param>
		/// <returns>True if the type is IEventHandler, otherwise false</returns>
		private static bool IsEventHandler( Type type ) {
			if ( !type.IsGenericType )
				return false;

			var genericType = type.GetGenericTypeDefinition( );

			return genericType == typeof( IEventHandler<> );
		}
	}
}