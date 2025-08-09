using Myth.Mapper;

namespace Myth.Extensions {

	public static class MapExtensions {

		public static TDestination MapTo<TDestination>( this object source, IServiceProvider? sp = null ) {
			if ( source == null )
				return default( TDestination )!;

			var srcType = source.GetType( );
			var destType = typeof( TDestination );

			var serviceProvider = sp ?? DefaultProvider.ServiceProvider;
			if ( serviceProvider == null )
				throw new InvalidOperationException( "ServiceProvider não configurado. Chame AddMapper() na configuração do DI ou passe o ServiceProvider como parâmetro." );

			// Garante que o DefaultProvider está configurado
			DefaultProvider.EnsureProvider( serviceProvider );

			var registry = ( MapRegistry? )serviceProvider.GetService( typeof( MapRegistry ) );
			if ( registry == null )
				throw new InvalidOperationException( "MapRegistry não encontrado no DI. Verifique se AddMapper() foi chamado corretamente." );

			var method = typeof( MapRegistry )
				.GetMethod( nameof( MapRegistry.Map ) )!
				.MakeGenericMethod( srcType, destType );

			return ( TDestination )method.Invoke( registry, [ source ] )!;
		}

		public static List<TDestination> MapTo<TDestination>( this IEnumerable<object> sourceList, IServiceProvider? sp = null ) {
			if ( sourceList == null )
				return new List<TDestination>( );

			return sourceList
				.Where( s => s != null )
				.Select( s => s.MapTo<TDestination>( sp ) )
				.ToList( );
		}

		// Extensão específica para tipos genéricos conhecidos
		public static List<TDestination> MapTo<TSource, TDestination>( this IEnumerable<TSource> sourceList, IServiceProvider? sp = null ) {
			if ( sourceList == null )
				return new List<TDestination>( );

			var serviceProvider = sp ?? DefaultProvider.ServiceProvider;
			if ( serviceProvider == null )
				throw new InvalidOperationException( "ServiceProvider não configurado." );

			var registry = ( MapRegistry )serviceProvider.GetService( typeof( MapRegistry ) )!;
			if ( registry == null )
				throw new InvalidOperationException( "MapRegistry não encontrado no DI." );

			var result = new List<TDestination>( );
			foreach ( var item in sourceList ) {
				if ( item != null ) {
					var mapped = registry.Map<TSource, TDestination>( item );
					result.Add( mapped );
				}
			}

			return result;
		}

		// Mapeamento assíncrono
		public static async Task<TDestination> MapToAsync<TDestination>( this object source, IServiceProvider? sp = null ) {
			// Por enquanto, apenas chama o método síncrono
			// Pode ser expandido para suporte a mapeamentos assíncronos no futuro
			return await Task.FromResult( source.MapTo<TDestination>( sp ) );
		}

		public static async Task<List<TDestination>> MapToAsync<TDestination>( this IEnumerable<object> sourceList, IServiceProvider? sp = null ) {
			var tasks = sourceList
				.Where( s => s != null )
				.Select( s => s.MapToAsync<TDestination>( sp ) );

			return ( await Task.WhenAll( tasks ) ).ToList( );
		}

		// Método auxiliar para verificar se um mapeamento existe
		public static bool CanMapTo<TDestination>( this object source, IServiceProvider? sp = null ) {
			if ( source == null )
				return false;

			try {
				var serviceProvider = sp ?? DefaultProvider.ServiceProvider;
				if ( serviceProvider == null )
					return false;

				var registry = ( MapRegistry? )serviceProvider.GetService( typeof( MapRegistry ) );
				if ( registry == null )
					return false;

				return registry.HasMapping( source.GetType( ), typeof( TDestination ) );
			} catch {
				return false;
			}
		}

		public static bool CanMapTo<TSource, TDestination>( this TSource source, IServiceProvider? sp = null ) {
			if ( source == null )
				return false;

			try {
				var serviceProvider = sp ?? DefaultProvider.ServiceProvider;
				if ( serviceProvider == null )
					return false;

				var registry = ( MapRegistry? )serviceProvider.GetService( typeof( MapRegistry ) );
				if ( registry == null )
					return false;

				return registry.HasMapping( typeof( TSource ), typeof( TDestination ) );
			} catch {
				return false;
			}
		}
	}
}