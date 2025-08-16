using Myth.Morph;

namespace Myth.Extensions {

	public static class MorphExtensions {

		public static TDestination To<TDestination>( this object source, IServiceProvider? sp = null ) {
			if ( source is null )
				return default!;

			var srcType = source.GetType( );
			var destType = typeof( TDestination );

			var serviceProvider = sp ?? DefaultProvider.ServiceProvider;
			if ( serviceProvider is null )
				throw new InvalidOperationException( $"ServiceProvider não configurado. Chame {nameof(ServiceCollectionExtensions.AddMorph)}() na configuração do DI ou passe o ServiceProvider como parâmetro." );

			// Garante que o DefaultProvider está configurado
			DefaultProvider.EnsureProvider( serviceProvider );

			var registry = ( MorphRegistry? )serviceProvider.GetService( typeof( MorphRegistry ) );
			if ( registry is null )
				throw new InvalidOperationException( $"{nameof(MorphRegistry)} não encontrado no DI. Verifique se {nameof(ServiceCollectionExtensions.AddMorph)}() foi chamado corretamente." );

			var method = typeof( MorphRegistry )
				.GetMethod( nameof( MorphRegistry.Morph ) )!
				.MakeGenericMethod( srcType, destType );

			return ( TDestination )method.Invoke( registry, [ source ] )!;
		}

		public static List<TDestination> To<TDestination>( this IEnumerable<object> sourceList, IServiceProvider? sp = null ) {
			if ( sourceList is null )
				return [];

			return sourceList
				.Where( s => s != null )
				.Select( s => s.To<TDestination>( sp ) )
				.ToList( );
		}

		// Extensão específica para tipos genéricos conhecidos
		public static List<TDestination> To<TSource, TDestination>( this IEnumerable<TSource> sourceList, IServiceProvider? sp = null ) {
			if ( sourceList is null )
				return [];

			var serviceProvider = sp ?? DefaultProvider.ServiceProvider;
			if ( serviceProvider is null )
				throw new InvalidOperationException( "ServiceProvider não configurado." );

			var registry = ( MorphRegistry )serviceProvider.GetService( typeof( MorphRegistry ) )!;
			if ( registry is null )
				throw new InvalidOperationException( $"{nameof( MorphRegistry )} não encontrado no DI." );

			var result = new List<TDestination>( );
			foreach ( var item in sourceList ) {
				if ( item != null ) {
					var mapped = registry.Morph<TSource, TDestination>( item );
					result.Add( mapped );
				}
			}

			return result;
		}

		// Mapeamento assíncrono
		public static async Task<TDestination> ToAsync<TDestination>( this object source, IServiceProvider? sp = null ) {
			return await Task.FromResult( source.To<TDestination>( sp ) );
		}

		public static async Task<List<TDestination>> ToAsync<TDestination>( this IEnumerable<object> sourceList, IServiceProvider? sp = null ) {
			var tasks = sourceList
				.Where( s => s != null )
				.Select( s => s.ToAsync<TDestination>( sp ) );

			return ( await Task.WhenAll( tasks ) ).ToList( );
		}

		// Método auxiliar para verificar se um mapeamento existe
		public static bool CanBindTo<TDestination>( this object source, IServiceProvider? sp = null ) {
			if ( source is null )
				return false;

			try {
				var serviceProvider = sp ?? DefaultProvider.ServiceProvider;
				if ( serviceProvider is null )
					return false;

				var registry = ( MorphRegistry? )serviceProvider.GetService( typeof( MorphRegistry ) );
				if ( registry is null )
					return false;

				return registry.HasMapping( source.GetType( ), typeof( TDestination ) );
			} catch {
				return false;
			}
		}

		public static bool CanBindTo<TSource, TDestination>( this TSource source, IServiceProvider? sp = null ) {
			if ( source is null )
				return false;

			try {
				var serviceProvider = sp ?? DefaultProvider.ServiceProvider;
				if ( serviceProvider is null )
					return false;

				var registry = ( MorphRegistry? )serviceProvider.GetService( typeof( MorphRegistry ) );
				if ( registry is null )
					return false;

				return registry.HasMapping( typeof( TSource ), typeof( TDestination ) );
			} catch {
				return false;
			}
		}
	}
}