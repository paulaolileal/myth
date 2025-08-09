using Myth.Extensions;
using System.Linq.Expressions;
using System.Reflection;

namespace Myth.Mapper {

	public interface IMappingProfile {

		void Apply( IServiceProvider sp );
	}

	public class MapRegistry {
		private readonly IServiceProvider _sp;
		private readonly Dictionary<(Type, Type), object> _builders = new( );
		private readonly Dictionary<Type, Type> _genericInterfaceToConcrete = new( );
		private readonly List<Action<Type, Type>> _genericRegistrars = new( );

		public MapRegistry( IServiceProvider sp ) {
			_sp = sp;
		}

		public void Register<TSource, TDestination>( Action<MappingBuilder<TSource, TDestination>> config ) {
			var builder = new MappingBuilder<TSource, TDestination>( );
			config( builder );
			_builders[ (typeof( TSource ), typeof( TDestination )) ] = builder;
		}

		public TDestination Map<TSource, TDestination>( TSource source ) {
			if ( !_builders.TryGetValue( (typeof( TSource ), typeof( TDestination )), out var builderObj ) ) {
				// tenta registrar dinamicamente
				foreach ( var registrar in _genericRegistrars )
					registrar( typeof( TSource ), typeof( TDestination ) );

				if ( !_builders.TryGetValue( (typeof( TSource ), typeof( TDestination )), out builderObj ) )
					throw new InvalidOperationException(
						$"No mapping registered from {typeof( TSource )} to {typeof( TDestination )}" );
			}

			var builder = ( MappingBuilder<TSource, TDestination> )builderObj;
			var dest = ( TDestination )CreateInstance( typeof( TDestination ) );
			builder.ApplyAsync( source, dest, _sp ).GetAwaiter( ).GetResult( );
			return dest;
		}

		public void RegisterGenericMapping( Type ifaceGeneric, Type concreteGeneric ) {
			_genericInterfaceToConcrete[ ifaceGeneric ] = concreteGeneric;
		}

		public bool TryResolveGenericConcrete( Type iface, out Type concrete ) {
			if ( iface.IsGenericType ) {
				var genericDef = iface.GetGenericTypeDefinition( );
				if ( _genericInterfaceToConcrete.TryGetValue( genericDef, out var concreteDef ) ) {
					var args = iface.GetGenericArguments( );
					concrete = concreteDef.MakeGenericType( args );
					return true;
				}
			}
			concrete = null!;
			return false;
		}

		public object? GetBuilderForCompatibleTypes( Type sourceType, Type destinationType ) {
			// Procura mapeamento exato
			if ( _builders.TryGetValue( (sourceType, destinationType), out var builder ) )
				return builder;

			// Procura mapeamentos compatíveis
			foreach ( var ((src, dst), b) in _builders ) {
				if ( src.IsAssignableFrom( sourceType ) && dst.IsAssignableFrom( destinationType ) )
					return b;
			}

			return null;
		}

		public bool HasMapping<TSource, TDestination>( ) {
			return _builders.ContainsKey( (typeof( TSource ), typeof( TDestination )) );
		}

		public bool HasMapping( Type TSource, Type TDestination ) {
			return _builders.ContainsKey( (TSource, TDestination) );
		}

		public void RegisterGenericEqualTypesMapping( Func<string, string, bool>? memberMatchRule = null ) {
			_genericRegistrars.Add( ( sourceType, destType ) => {
				if ( !sourceType.IsGenericType || !destType.IsGenericType )
					return;

				if ( sourceType.GetGenericTypeDefinition( ) != destType.GetGenericTypeDefinition( ) )
					return;

				var srcArgs = sourceType.GetGenericArguments( );
				var dstArgs = destType.GetGenericArguments( );

				if ( srcArgs.Length != dstArgs.Length )
					return;

				// Cria builder dinamicamente via reflection
				var builderType = typeof( MappingBuilder<,> ).MakeGenericType( sourceType, destType );
				var builder = Activator.CreateInstance( builderType )!;

				var srcProps = sourceType.GetProperties( );
				var dstProps = destType.GetProperties( );

				foreach ( var destProp in dstProps ) {
					if ( !destProp.CanWrite )
						continue;

					var srcProp = srcProps.FirstOrDefault( p =>
						( memberMatchRule?.Invoke( p.Name, destProp.Name ) ?? p.Name == destProp.Name )
						&& p.PropertyType != null
						&& destProp.PropertyType != null
					);
					if ( srcProp == null )
						continue;

					// Cria expressão ForMember para cada propriedade compatível
					var mapMethod = builderType.GetMethod( "ForMember" )!;
					var destParam = Expression.Parameter( destType, "d" );
					var memberAccess = Expression.MakeMemberAccess( destParam, destProp );
					var lambda = Expression.Lambda( memberAccess, destParam );

					var funcType = typeof( Func<,,> ).MakeGenericType( sourceType, typeof( IServiceProvider ), destProp.PropertyType );
					var valueGetter = CreateGenericValueResolver( srcProp, sourceType, destProp.PropertyType );

					mapMethod.MakeGenericMethod( destProp.PropertyType )
						.Invoke( builder, new object[ ] { lambda, valueGetter } );
				}

				_builders[ (sourceType, destType) ] = builder;
			} );
		}

		private Delegate CreateGenericValueResolver( PropertyInfo srcProp, Type srcType, Type destPropType ) {
			var srcParam = Expression.Parameter( srcType, "src" );
			var spParam = Expression.Parameter( typeof( IServiceProvider ), "sp" );

			var srcAccess = Expression.Property( srcParam, srcProp );
			Expression body = srcAccess;

			// Verifica se precisa mapear valor interno
			if ( srcProp.PropertyType != destPropType ) {
				// srcProp.PropertyType → destPropType precisa ser mapeado
				var mapToMethod = typeof( MapExtensions ).GetMethod( "MapTo" )!.MakeGenericMethod( destPropType );
				body = Expression.Call( mapToMethod, Expression.Convert( srcAccess, typeof( object ) ), spParam );
			}

			var lambda = Expression.Lambda( body, srcParam, spParam );
			return lambda.Compile( );
		}

		/// <summary>
		/// Cria instância de um tipo mesmo que não tenha construtor padrão.
		/// </summary>
		private object CreateInstance( Type type ) {
			// 1 - Tenta via DI
			var fromSp = _sp.GetService( type );
			if ( fromSp != null )
				return fromSp;

			// 2 - Tenta construtor sem parâmetros
			var ctor = type.GetConstructor( Type.EmptyTypes );
			if ( ctor != null )
				return ctor.Invoke( null );

			// 3 - Pega construtor com menos parâmetros
			var ctorWithParams = type
				.GetConstructors( )
				.OrderBy( c => c.GetParameters( ).Length )
				.FirstOrDefault( );

			if ( ctorWithParams == null )
				throw new InvalidOperationException( $"Tipo {type} não possui construtor acessível." );

			var args = ctorWithParams
				.GetParameters( )
				.Select( p => p.HasDefaultValue ? p.DefaultValue : GetDefault( p.ParameterType ) )
				.ToArray( );

			return ctorWithParams.Invoke( args );
		}

		private static object? GetDefault( Type type )
			=> type.IsValueType ? Activator.CreateInstance( type ) : null;
	}
}