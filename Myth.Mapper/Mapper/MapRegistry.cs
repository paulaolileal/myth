namespace Myth.Mapper {

	public interface IMappingProfile {

		void Apply( IServiceProvider sp );
	}

	public class MapRegistry {
		private readonly IServiceProvider _sp;
		private readonly Dictionary<(Type, Type), object> _builders = new( );

		public MapRegistry( IServiceProvider sp ) {
			_sp = sp;
		}

		public void Register<TSource, TDestination>( Action<MappingBuilder<TSource, TDestination>> config ) {
			var builder = new MappingBuilder<TSource, TDestination>( );
			config( builder );
			_builders[ (typeof( TSource ), typeof( TDestination )) ] = builder;
		}

		public TDestination Map<TSource, TDestination>( TSource source ) {
			if ( !_builders.TryGetValue( (typeof( TSource ), typeof( TDestination )), out var builderObj ) )
				throw new InvalidOperationException( $"No mapping registered from {typeof( TSource )} to {typeof( TDestination )}" );

			var builder = ( MappingBuilder<TSource, TDestination> )builderObj;

			var dest = Activator.CreateInstance<TDestination>( );

			builder.Apply( source, dest, _sp );

			return dest;
		}
	}
}