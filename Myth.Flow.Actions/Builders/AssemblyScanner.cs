using Myth.Interfaces;
using System.Reflection;

namespace Myth.Builders {

	/// <summary>
	/// Default assembly scanner implementation
	/// </summary>
	internal sealed class AssemblyScanner : IAssemblyScanner {

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

		private static Type[ ] GetTypesFromAssembly( Assembly assembly ) {
			try {
				return assembly.GetTypes( );
			} catch ( ReflectionTypeLoadException ex ) {
				return ex.Types.Where( t => t != null ).ToArray( )!;
			}
		}

		private static bool IsCommandHandler( Type type ) {
			if ( !type.IsGenericType )
				return false;

			var genericType = type.GetGenericTypeDefinition( );

			return genericType == typeof( ICommandHandler<> ) ||
				   genericType == typeof( ICommandHandler<,> );
		}

		private static bool IsQueryHandler( Type type ) {
			if ( !type.IsGenericType )
				return false;

			var genericType = type.GetGenericTypeDefinition( );

			return genericType == typeof( IQueryHandler<,> );
		}

		private static bool IsEventHandler( Type type ) {
			if ( !type.IsGenericType )
				return false;

			var genericType = type.GetGenericTypeDefinition( );

			return genericType == typeof( IEventHandler<> );
		}
	}
}