using System.Collections.ObjectModel;
using System.Reflection;

namespace Myth.Settings {

	/// <summary>
	/// Configuration settings for the Myth Morph mapping system.
	/// This class allows customization of assemblies to scan for mappings and
	/// registration of generic type mappings between interfaces and concrete implementations.
	/// </summary>
	public class MorphSettings {

		/// <summary>
		/// Gets or sets the list of assemblies to scan for mapping profiles that implement IMorphable interface.
		/// </summary>
		/// <remarks>
		/// If this list is empty or null, all assemblies in the current AppDomain will be scanned.
		/// This can impact application startup performance for large applications.
		/// </remarks>
		internal List<Assembly> Assemblies { get; set; } = [ ];

		/// <summary>
		/// Gets or sets the list of generic type mappings between interfaces and their concrete implementations.
		/// </summary>
		/// <remarks>
		/// These mappings enable automatic resolution of interface types to concrete types during object transformation.
		/// Default mappings include common collection interfaces like IList&lt;&gt;, ICollection&lt;&gt;, etc.
		/// </remarks>
		internal List<(Type iface, Type concrete)> GenericMappings { get; set; } = [
				(typeof(IList<>), typeof(List<>)),
				(typeof(ICollection<>), typeof(List<>)),
				(typeof(IDictionary<,>), typeof(Dictionary<,>)),
				(typeof(ISet<>), typeof(HashSet<>)),
				(typeof(IReadOnlyCollection<>), typeof(ReadOnlyCollection<>)),
				(typeof(IReadOnlyList<>), typeof(List<>)),
				(typeof(IReadOnlySet<>), typeof(HashSet<>)),
			];

		/// <summary>
		/// Adds an assembly to the list of assemblies to scan for mapping profiles.
		/// </summary>
		/// <param name="assembly">The assembly to add for scanning. Must not be null.</param>
		/// <returns>The current MorphSettings instance for method chaining.</returns>
		/// <exception cref="ArgumentNullException">Thrown when assembly is null.</exception>
		public MorphSettings AddAssembly( Assembly assembly ) {
			if ( assembly == null )
				throw new ArgumentNullException( nameof( assembly ) );

			if ( !Assemblies.Contains( assembly ) )
				Assemblies.Add( assembly );
			return this;
		}

		/// <summary>
		/// Adds multiple assemblies to the list of assemblies to scan for mapping profiles.
		/// </summary>
		/// <param name="assemblies">The assemblies to add for scanning. Individual assemblies must not be null.</param>
		/// <returns>The current MorphSettings instance for method chaining.</returns>
		/// <exception cref="ArgumentNullException">Thrown when assemblies array is null or contains null elements.</exception>
		public MorphSettings AddAssemblies( params Assembly[ ] assemblies ) {
			if ( assemblies == null )
				throw new ArgumentNullException( nameof( assemblies ) );

			if ( assemblies.Any( a => a == null ) )
				throw new ArgumentNullException( nameof( assemblies ), "Assembly array contains null elements" );

			var newAssemblies = assemblies.Except( Assemblies );
			Assemblies.AddRange( newAssemblies );

			return this;
		}

		/// <summary>
		/// Adds a generic type mapping between an interface and its concrete implementation.
		/// </summary>
		/// <remarks>
		/// This method allows registration of mappings between generic interface types and their concrete implementations.
		/// For example, mapping typeof(IPaginated&lt;&gt;) to typeof(Paginated&lt;&gt;) enables automatic resolution
		/// of IPaginated&lt;T&gt; to Paginated&lt;T&gt; during object transformation.
		/// Both types must be generic type definitions (open generic types).
		/// </remarks>
		/// <param name="ifaceGeneric">The generic interface type definition (e.g., typeof(IList&lt;&gt;)).</param>
		/// <param name="concreteGeneric">The concrete generic type definition that implements the interface (e.g., typeof(List&lt;&gt;)).</param>
		/// <returns>The current MorphSettings instance for method chaining.</returns>
		/// <exception cref="ArgumentNullException">Thrown when either parameter is null.</exception>
		/// <exception cref="ArgumentException">Thrown when either type is not a generic type definition.</exception>
		public MorphSettings AddGenericMorph( Type ifaceGeneric, Type concreteGeneric ) {
			if ( ifaceGeneric == null )
				throw new ArgumentNullException( nameof( ifaceGeneric ) );

			if ( concreteGeneric == null )
				throw new ArgumentNullException( nameof( concreteGeneric ) );

			if ( !ifaceGeneric.IsGenericTypeDefinition )
				throw new ArgumentException( "Interface type must be a generic type definition", nameof( ifaceGeneric ) );

			if ( !concreteGeneric.IsGenericTypeDefinition )
				throw new ArgumentException( "Concrete type must be a generic type definition", nameof( concreteGeneric ) );

			GenericMappings.Add( (ifaceGeneric, concreteGeneric) );

			return this;
		}

		/// <summary>
		/// Adds a generic type mapping in a type-safe manner using generic type parameters.
		/// </summary>
		/// <remarks>
		/// This method provides a type-safe way to register generic mappings by ensuring at compile time
		/// that the concrete type implements the interface type. Both types must be generic type definitions.
		/// This is particularly useful when you want to ensure type safety during configuration.
		/// </remarks>
		/// <typeparam name="TInterface">The interface type that must be a generic type definition. Must be a class.</typeparam>
		/// <typeparam name="TConcrete">The concrete type that implements TInterface and must be a generic type definition. Must be a class.</typeparam>
		/// <returns>The current MorphSettings instance for method chaining.</returns>
		/// <exception cref="ArgumentException">Thrown when either type is not a generic type definition.</exception>
		/// <example>
		/// <code>
		/// settings.AddGenericMapping&lt;IList&lt;&gt;, List&lt;&gt;&gt;();
		/// settings.AddGenericMapping&lt;IMyInterface&lt;&gt;, MyImplementation&lt;&gt;&gt;();
		/// </code>
		/// </example>
		public MorphSettings AddGenericMapping<TInterface, TConcrete>( )
			where TInterface : class
			where TConcrete : class, TInterface {
			var ifaceType = typeof( TInterface );
			var concreteType = typeof( TConcrete );

			// Verify that both are generic type definitions
			if ( !ifaceType.IsGenericTypeDefinition || !concreteType.IsGenericTypeDefinition )
				throw new ArgumentException( "Both types must be generic type definitions (e.g., typeof(IList<>))" );

			GenericMappings.Add( (ifaceType, concreteType) );

			return this;
		}

		/// <summary>
		/// Removes all registered assemblies from the scanning list.
		/// </summary>
		/// <remarks>
		/// After calling this method, if no assemblies are added back, the system will default
		/// to scanning all assemblies in the current AppDomain, which may impact performance.
		/// </remarks>
		/// <returns>The current MorphSettings instance for method chaining.</returns>
		public MorphSettings ClearAssemblies( ) {
			Assemblies.Clear( );
			return this;
		}

		/// <summary>
		/// Removes all registered generic mappings, including the default ones.
		/// </summary>
		/// <remarks>
		/// After calling this method, you will need to manually add any required generic mappings
		/// including the common collection mappings that are provided by default.
		/// Use this method only if you want full control over which interface-to-concrete mappings are available.
		/// </remarks>
		/// <returns>The current MorphSettings instance for method chaining.</returns>
		public MorphSettings ClearGenericMappings( ) {
			GenericMappings.Clear( );
			return this;
		}

		/// <summary>
		/// Gets the total number of assemblies configured for scanning.
		/// </summary>
		/// <returns>The number of assemblies in the scanning list.</returns>
		public int GetAssemblyCount( ) => Assemblies.Count;

		/// <summary>
		/// Gets the total number of registered generic mappings.
		/// </summary>
		/// <returns>The number of generic mappings registered.</returns>
		public int GetGenericMappingCount( ) => GenericMappings.Count;

		/// <summary>
		/// Checks if a specific assembly is already registered for scanning.
		/// </summary>
		/// <param name="assembly">The assembly to check.</param>
		/// <returns>True if the assembly is registered, false otherwise.</returns>
		/// <exception cref="ArgumentNullException">Thrown when assembly is null.</exception>
		public bool ContainsAssembly( Assembly assembly ) {
			if ( assembly == null )
				throw new ArgumentNullException( nameof( assembly ) );

			return Assemblies.Contains( assembly );
		}

		/// <summary>
		/// Checks if a specific generic mapping is already registered.
		/// </summary>
		/// <param name="interfaceType">The interface type to check.</param>
		/// <param name="concreteType">The concrete type to check.</param>
		/// <returns>True if the mapping is registered, false otherwise.</returns>
		/// <exception cref="ArgumentNullException">Thrown when either parameter is null.</exception>
		public bool ContainsGenericMapping( Type interfaceType, Type concreteType ) {
			if ( interfaceType == null )
				throw new ArgumentNullException( nameof( interfaceType ) );

			if ( concreteType == null )
				throw new ArgumentNullException( nameof( concreteType ) );

			return GenericMappings.Any( mapping => mapping.iface == interfaceType && mapping.concrete == concreteType );
		}
	}
}