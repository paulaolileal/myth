using System.Collections.ObjectModel;
using System.Reflection;

namespace Myth.Settings {

	public class MorphSettings {

		/// <summary>
		/// Assemblies para procurar mapeamentos via IMapTo
		/// </summary>
		internal List<Assembly> Assemblies { get; set; } = [ ];

		/// <summary>
		/// Registros de mapeamentos genéricos (ex: interface -> implementação concreta)
		/// </summary>
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
		/// Adiciona um assembly para procurar perfis de mapeamento
		/// </summary>
		public MorphSettings AddAssembly( Assembly assembly ) {
			if ( !Assemblies.Contains( assembly ) )
				Assemblies.Add( assembly );
			return this;
		}

		/// <summary>
		/// Adiciona multipplos assemblies para procurar perfis de mapeamento
		/// </summary>
		public MorphSettings AddAssemblies( params Assembly[ ] assemblies ) {
			var newAssemblies = assemblies.Except( Assemblies );
			Assemblies.AddRange( newAssemblies );

			return this;
		}

		/// <summary>
		/// Adiciona um mapeamento genérico (ex: typeof(IPaginated&lt;&gt;), typeof(Paginated&lt;&gt;))
		/// </summary>
		public MorphSettings AddGenericMorph( Type ifaceGeneric, Type concreteGeneric ) {
			GenericMappings.Add( (ifaceGeneric, concreteGeneric) );

			return this;
		}

		/// <summary>
		/// Adiciona mapeamento genérico de forma type-safe
		/// </summary>
		public MorphSettings AddGenericMapping<TInterface, TConcrete>( )
			where TInterface : class
			where TConcrete : class, TInterface {
			var ifaceType = typeof( TInterface );
			var concreteType = typeof( TConcrete );

			// Verifica se são tipos genéricos
			if ( !ifaceType.IsGenericTypeDefinition || !concreteType.IsGenericTypeDefinition )
				throw new ArgumentException( "Ambos os tipos devem ser definições de tipos genéricos (ex: typeof(IList<>))" );

			GenericMappings.Add( (ifaceType, concreteType) );

			return this;
		}
	}
}