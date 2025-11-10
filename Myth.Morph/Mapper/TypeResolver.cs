using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Myth.Morph {

	/// <summary>
	/// Provides centralized type resolution utilities for handling inheritance hierarchies and type compatibility.
	/// This class is completely generic and technology-agnostic, working with any inheritance structure.
	/// </summary>
	public class TypeResolver {
		private readonly ILogger<TypeResolver>? _logger;
		private readonly ConcurrentDictionary<Type, Type> _actualTypeCache = new( );
		private readonly ConcurrentDictionary<Type, List<Type>> _inheritanceHierarchyCache = new( );
		private readonly int _maxInheritanceDepth;
		private readonly bool _includeInterfacesInFallback;
		private readonly bool _resolveToBaseType;

		/// <summary>
		/// Initializes a new instance of the TypeResolver class.
		/// </summary>
		/// <param name="logger">Optional logger for diagnostic information.</param>
		/// <param name="maxInheritanceDepth">Maximum depth to traverse in the inheritance hierarchy. -1 for unlimited.</param>
		/// <param name="includeInterfacesInFallback">Whether to include interfaces in the inheritance hierarchy.</param>
		/// <param name="resolveToBaseType">If true, resolves derived types to their root base type for mapping resolution.</param>
		public TypeResolver( ILogger<TypeResolver>? logger = null, int maxInheritanceDepth = -1, bool includeInterfacesInFallback = false, bool resolveToBaseType = false ) {
			_logger = logger;
			_maxInheritanceDepth = maxInheritanceDepth;
			_includeInterfacesInFallback = includeInterfacesInFallback;
			_resolveToBaseType = resolveToBaseType;
			_logger?.LogDebug( "TypeResolver initialized with maxInheritanceDepth={MaxDepth}, includeInterfaces={IncludeInterfaces}, resolveToBaseType={ResolveToBaseType}",
				maxInheritanceDepth, includeInterfacesInFallback, resolveToBaseType );
		}

		/// <summary>
		/// Gets the actual type for mapping resolution.
		/// If resolveToBaseType is enabled, resolves derived types to their root base type.
		/// Otherwise, returns the type as-is for direct mapping.
		/// </summary>
		/// <param name="type">The type to resolve.</param>
		/// <returns>The resolved type for mapping.</returns>
		public Type GetActualType( Type type ) {
			if ( type == null )
				throw new ArgumentNullException( nameof( type ) );

			return _actualTypeCache.GetOrAdd( type, t => {
				var originalType = t;

				// If resolveToBaseType is enabled, resolve any derived type to its root base
				if ( _resolveToBaseType && t.BaseType != null && t.BaseType != typeof( object ) ) {
					_logger?.LogTrace( "ResolveToBaseType enabled, resolving {DerivedType} to base type", t.Name );
					var baseType = GetRootBaseType( t );
					_logger?.LogDebug( "Resolved derived type {DerivedType} to root base type {BaseType}", originalType.Name, baseType.Name );
					return baseType;
				}

				_logger?.LogTrace( "Type {TypeName} used as-is for mapping", t.Name );
				return t;
			} );
		}

		/// <summary>
		/// Gets the root base type in the inheritance hierarchy, excluding System.Object.
		/// </summary>
		/// <param name="type">The type to get the root base for.</param>
		/// <returns>The root base type, or the type itself if it has no base besides object.</returns>
		private Type GetRootBaseType( Type type ) {
			var baseType = type.BaseType;

			// If no meaningful base type, return the type itself
			if ( baseType == null || baseType == typeof( object ) )
				return type;

			// Traverse to the root
			while ( baseType.BaseType != null && baseType.BaseType != typeof( object ) ) {
				baseType = baseType.BaseType;
			}

			return baseType;
		}

		/// <summary>
		/// Checks if a type has a meaningful base type (not just System.Object).
		/// This is useful for determining if type resolution should traverse the inheritance hierarchy.
		/// </summary>
		/// <param name="type">The type to check.</param>
		/// <returns>True if the type has a base type other than System.Object, false otherwise.</returns>
		public bool HasBaseType( Type type ) {
			if ( type == null )
				return false;

			var hasBase = type.BaseType != null && type.BaseType != typeof( object );

			if ( hasBase ) {
				_logger?.LogTrace( "Type {TypeName} has base type {BaseType}", type.Name, type.BaseType?.Name );
			}

			return hasBase;
		}

		/// <summary>
		/// Gets the inheritance hierarchy for a given type, including base classes and optionally interfaces.
		/// Results are cached for performance.
		/// </summary>
		/// <param name="type">The type to get the hierarchy for.</param>
		/// <returns>A list of types in the inheritance hierarchy, ordered from most specific to most general.</returns>
		public List<Type> GetInheritanceHierarchy( Type type ) {
			if ( type == null )
				throw new ArgumentNullException( nameof( type ) );

			return _inheritanceHierarchyCache.GetOrAdd( type, t => {
				_logger?.LogTrace( "Building inheritance hierarchy for type {TypeName}", t.Name );

				var hierarchy = new List<Type>( );

				// First, resolve the actual type (remove proxies)
				var actualType = GetActualType( t );
				if ( actualType != t ) {
					_logger?.LogTrace( "Using actual type {ActualType} instead of {OriginalType} for hierarchy", actualType.Name, t.Name );
					t = actualType;
				}

				var currentType = t.BaseType;
				var depth = 0;

				// Add base classes
				while ( currentType != null && currentType != typeof( object ) ) {
					if ( _maxInheritanceDepth >= 0 && depth >= _maxInheritanceDepth ) {
						_logger?.LogTrace( "Reached maximum inheritance depth {MaxDepth} for type {Type}", _maxInheritanceDepth, t.Name );
						break;
					}

					_logger?.LogTrace( "Adding base type {BaseType} to hierarchy at depth {Depth}", currentType.Name, depth );
					hierarchy.Add( currentType );
					currentType = currentType.BaseType;
					depth++;
				}

				// Add interfaces if enabled
				if ( _includeInterfacesInFallback ) {
					var interfaces = t.GetInterfaces( );
					_logger?.LogTrace( "Adding {InterfaceCount} interfaces to hierarchy for type {Type}", interfaces.Length, t.Name );

					foreach ( var iface in interfaces ) {
						if ( !hierarchy.Contains( iface ) ) {
							hierarchy.Add( iface );
							_logger?.LogTrace( "Added interface {InterfaceName} to hierarchy", iface.Name );
						}
					}
				}

				_logger?.LogDebug( "Built inheritance hierarchy for {Type}: {HierarchyCount} types found", t.Name, hierarchy.Count );
				return hierarchy;
			} );
		}

		/// <summary>
		/// Checks if a source type can be assigned to a destination type, considering inheritance and proxies.
		/// </summary>
		/// <param name="sourceType">The source type.</param>
		/// <param name="destinationType">The destination type.</param>
		/// <returns>True if the source type is compatible with the destination type.</returns>
		public bool IsCompatible( Type sourceType, Type destinationType ) {
			if ( sourceType == null || destinationType == null )
				return false;

			var actualSourceType = GetActualType( sourceType );
			var actualDestType = GetActualType( destinationType );

			_logger?.LogTrace( "Checking compatibility: {SourceType} -> {DestType} (actual: {ActualSource} -> {ActualDest})",
				sourceType.Name, destinationType.Name, actualSourceType.Name, actualDestType.Name );

			var isCompatible = actualDestType.IsAssignableFrom( actualSourceType );
			_logger?.LogTrace( "Compatibility result: {IsCompatible}", isCompatible );

			return isCompatible;
		}

		/// <summary>
		/// Finds a matching type in the hierarchy that implements a specific generic interface.
		/// </summary>
		/// <param name="type">The type to search from.</param>
		/// <param name="genericInterfaceDefinition">The generic interface definition to find (e.g., typeof(IMorphableFrom<>)).</param>
		/// <returns>The matched interface type, or null if not found.</returns>
		public Type? FindGenericInterface( Type type, Type genericInterfaceDefinition ) {
			if ( type == null || genericInterfaceDefinition == null )
				return null;

			if ( !genericInterfaceDefinition.IsGenericTypeDefinition ) {
				_logger?.LogWarning( "Type {TypeName} is not a generic type definition", genericInterfaceDefinition.Name );
				return null;
			}

			_logger?.LogTrace( "Searching for generic interface {InterfaceName} in type {TypeName}",
				genericInterfaceDefinition.Name, type.Name );

			var actualType = GetActualType( type );
			var interfaces = actualType.GetInterfaces( );

			foreach ( var iface in interfaces ) {
				if ( iface.IsGenericType && iface.GetGenericTypeDefinition( ) == genericInterfaceDefinition ) {
					_logger?.LogDebug( "Found matching interface {InterfaceName} in type {TypeName}",
						iface.Name, actualType.Name );
					return iface;
				}
			}

			_logger?.LogTrace( "Generic interface {InterfaceName} not found in type {TypeName}",
				genericInterfaceDefinition.Name, actualType.Name );
			return null;
		}

		/// <summary>
		/// Clears all internal caches. Useful for testing or when type definitions change at runtime.
		/// </summary>
		public void ClearCaches( ) {
			_actualTypeCache.Clear( );
			_inheritanceHierarchyCache.Clear( );
			_logger?.LogDebug( "TypeResolver caches cleared" );
		}
	}
}