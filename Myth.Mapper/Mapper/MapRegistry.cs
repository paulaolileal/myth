using Myth.Extensions;
using Myth.Interfaces;
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
		private readonly HashSet<(Type, Type)> _instanceBasedMappings = new( );

		public MapRegistry( IServiceProvider sp ) {
			_sp = sp;
		}

		public void Register<TSource, TDestination>( Action<MappingBuilder<TSource, TDestination>> config ) {
			var builder = new MappingBuilder<TSource, TDestination>( );
			config( builder );
			_builders[ (typeof( TSource ), typeof( TDestination )) ] = builder;
		}

		public void RegisterInstanceBasedMapping( Type sourceType, Type destinationType ) {
			_instanceBasedMappings.Add( (sourceType, destinationType) );
		}

		public TDestination Map<TSource, TDestination>( TSource source ) {
			var sourceType = typeof( TSource );
			var destinationType = typeof( TDestination );

			// Se o source é null, retorna default
			if ( source == null )
				return default( TDestination )!;

			// Usa o tipo real do objeto se for diferente do tipo genérico
			var actualSourceType = source.GetType( );

			// Verifica se é um mapeamento baseado em instância
			if ( _instanceBasedMappings.Contains( (actualSourceType, destinationType) ) && source is IMapTo<TDestination> mapToInstance ) {
				return MapFromInstance( mapToInstance, destinationType );
			}

			// Determina o tipo concreto de destino ANTES de procurar builder
			var concreteDestinationType = ResolveConcreteDestinationType( destinationType );

			// Primeiro tenta encontrar um builder direto para os tipos originais
			if ( !TryGetBuilder( actualSourceType, destinationType, out var builderObj ) ) {
				// Se não encontrou, tenta com o tipo concreto
				if ( !TryGetBuilder( actualSourceType, concreteDestinationType, out builderObj ) ) {
					// Tenta registrar dinamicamente
					foreach ( var registrar in _genericRegistrars ) {
						registrar( actualSourceType, destinationType );
						registrar( actualSourceType, concreteDestinationType );
					}

					// Tenta novamente após registrar
					if ( !TryGetBuilder( actualSourceType, destinationType, out builderObj ) &&
						!TryGetBuilder( actualSourceType, concreteDestinationType, out builderObj ) ) {
						throw new InvalidOperationException(
							$"No mapping registered from {actualSourceType} to {destinationType}" );
					}
				}
			}

			// Cria a instância do destino usando o tipo concreto
			var dest = CreateInstance( concreteDestinationType );

			// Aplica o mapeamento usando reflection se necessário
			if ( actualSourceType != sourceType || concreteDestinationType != destinationType ) {
				return ApplyMappingDynamically<TDestination>( source, actualSourceType, dest, concreteDestinationType, destinationType );
			}

			// Caso padrão - tipos coincidem
			var builder = ( MappingBuilder<TSource, TDestination> )builderObj;
			builder.ApplyAsync( source, ( TDestination )dest, _sp ).GetAwaiter( ).GetResult( );
			return ( TDestination )dest;
		}

		private TDestination MapFromInstance<TDestination>( IMapTo<TDestination> source, Type destinationType ) {
			// Cria a instância de destino
			var dest = ( TDestination )CreateInstance( destinationType );

			// Cria o builder específico para instâncias
			var instanceBuilder = new MappingBuilder<TDestination>( );

			// Chama o MapTo da instância para configurar o builder
			source.MapTo( instanceBuilder );

			// Aplica o mapeamento
			var applyMethod = typeof( MappingBuilder<TDestination> )
				.GetMethod( "ApplyFromInstanceAsync", BindingFlags.Instance | BindingFlags.NonPublic );

			if ( applyMethod != null ) {
				var genericApplyMethod = applyMethod.MakeGenericMethod( source.GetType( ) );
				var task = ( Task )genericApplyMethod.Invoke( instanceBuilder, new object[ ] { source, dest, _sp } )!;
				task.GetAwaiter( ).GetResult( );
			}

			return dest;
		}

		private bool TryGetBuilder( Type sourceType, Type destinationType, out object? builderObj ) {
			// Procura mapeamento exato
			if ( _builders.TryGetValue( (sourceType, destinationType), out builderObj ) )
				return true;

			// Procura mapeamentos compatíveis (herança/implementação)
			foreach ( var ((src, dst), builder) in _builders ) {
				if ( src.IsAssignableFrom( sourceType ) && dst.IsAssignableFrom( destinationType ) ) {
					builderObj = builder;
					return true;
				}
			}

			// Procura por mapeamentos genéricos compatíveis
			if ( sourceType.IsGenericType && destinationType.IsGenericType ) {
				var srcGenericDef = sourceType.GetGenericTypeDefinition( );
				var dstGenericDef = destinationType.GetGenericTypeDefinition( );

				foreach ( var ((src, dst), builder) in _builders ) {
					if ( src.IsGenericType && dst.IsGenericType ) {
						var builderSrcGeneric = src.GetGenericTypeDefinition( );
						var builderDstGeneric = dst.GetGenericTypeDefinition( );

						if ( builderSrcGeneric == srcGenericDef && builderDstGeneric == dstGenericDef ) {
							builderObj = builder;
							return true;
						}
					}
				}
			}

			builderObj = null;
			return false;
		}

		private Type ResolveConcreteDestinationType( Type destinationType ) {
			// Se é interface genérica, tenta resolver para concreto
			if ( destinationType.IsInterface && TryResolveGenericConcrete( destinationType, out var concrete ) ) {
				return concrete;
			}

			// Se é interface não-genérica, procura implementação registrada no DI
			if ( destinationType.IsInterface ) {
				var serviceImpl = _sp.GetService( destinationType );
				if ( serviceImpl != null )
					return serviceImpl.GetType( );
			}

			return destinationType;
		}

		private TDestination ApplyMappingDynamically<TDestination>(
			object source,
			Type actualSourceType,
			object dest,
			Type concreteDestType,
			Type destinationType ) {
			// Encontra ou cria um builder compatível
			var compatibleBuilder = GetOrCreateCompatibleBuilder( actualSourceType, concreteDestType );

			// Se não conseguiu com o tipo concreto, tenta com o tipo interface
			if ( compatibleBuilder == null && concreteDestType != destinationType ) {
				compatibleBuilder = GetOrCreateCompatibleBuilder( actualSourceType, destinationType );
			}

			if ( compatibleBuilder != null ) {
				// Invoca ApplyAsync dinamicamente
				var applyMethod = compatibleBuilder.GetType( ).GetMethod( "ApplyAsync" );
				var applyTask = ( Task )applyMethod!.Invoke( compatibleBuilder, [ source, dest, _sp ] )!;
				applyTask.GetAwaiter( ).GetResult( );
			}

			return ( TDestination )dest;
		}

		private object? GetOrCreateCompatibleBuilder( Type sourceType, Type destType ) {
			// Primeiro verifica se já existe
			if ( _builders.TryGetValue( (sourceType, destType), out var existing ) )
				return existing;

			// Tenta criar um builder dinamicamente se os tipos são compatíveis
			if ( CanCreateDynamicMapping( sourceType, destType ) ) {
				CreateDynamicMapping( sourceType, destType );
				return _builders.GetValueOrDefault( (sourceType, destType) );
			}

			// Procura builders compatíveis
			return GetBuilderForCompatibleTypes( sourceType, destType );
		}

		private bool CanCreateDynamicMapping( Type sourceType, Type destType ) {
			// Pode criar mapeamento automático se:
			// 1. Ambos são classes ou structs (não interfaces)
			// 2. Ou se conseguir resolver o tipo concreto de destino

			if ( !sourceType.IsInterface && !destType.IsInterface )
				return true;

			if ( destType.IsInterface && TryResolveGenericConcrete( destType, out _ ) )
				return true;

			return false;
		}

		private void CreateDynamicMapping( Type sourceType, Type destType ) {
			// Resolve o tipo concreto se necessário
			var concreteDestType = ResolveConcreteDestinationType( destType );

			// Cria um builder genérico dinamicamente
			var builderType = typeof( MappingBuilder<,> ).MakeGenericType( sourceType, concreteDestType );
			var builder = Activator.CreateInstance( builderType )!;

			// Adiciona mapeamento automático básico (será feito pelo AutoMap)
			_builders[ (sourceType, destType) ] = builder;

			// Se o tipo concreto é diferente do tipo interface, registra ambos
			if ( concreteDestType != destType ) {
				_builders[ (sourceType, concreteDestType) ] = builder;
			}
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

		public bool HasMapping( Type sourceType, Type destinationType ) {
			// Verifica se é um mapeamento baseado em instância
			if ( _instanceBasedMappings.Contains( (sourceType, destinationType) ) )
				return true;

			// Verifica mapeamento direto
			if ( _builders.ContainsKey( (sourceType, destinationType) ) )
				return true;

			// Verifica se consegue resolver o tipo concreto e tem mapeamento para ele
			var concreteDestType = ResolveConcreteDestinationType( destinationType );
			if ( concreteDestType != destinationType && _builders.ContainsKey( (sourceType, concreteDestType) ) )
				return true;

			// Verifica compatibilidade
			return GetBuilderForCompatibleTypes( sourceType, destinationType ) != null ||
				   GetBuilderForCompatibleTypes( sourceType, concreteDestType ) != null;
		}

		public void RegisterGenericEqualTypesMapping( Func<string, string, bool>? memberMatchRule = null ) {
			_genericRegistrars.Add( ( sourceType, destType ) => {
				// Resolve tipos concretos se necessário
				var concreteDestType = ResolveConcreteDestinationType( destType );

				// Se já existe mapeamento, não registra novamente
				if ( _builders.ContainsKey( (sourceType, destType) ) ||
					 _builders.ContainsKey( (sourceType, concreteDestType) ) )
					return;

				// Verifica se são tipos genéricos compatíveis
				if ( !IsCompatibleGenericMapping( sourceType, concreteDestType ) )
					return;

				// Cria builder dinamicamente via reflection
				var builderType = typeof( MappingBuilder<,> ).MakeGenericType( sourceType, concreteDestType );
				var builder = Activator.CreateInstance( builderType )!;

				ConfigureGenericBuilder( builder, builderType, sourceType, concreteDestType, memberMatchRule );

				// Registra para ambos os tipos (interface e concreto)
				_builders[ (sourceType, destType) ] = builder;
				if ( destType != concreteDestType ) {
					_builders[ (sourceType, concreteDestType) ] = builder;
				}
			} );
		}

		private bool IsCompatibleGenericMapping( Type sourceType, Type destType ) {
			// Ambos devem ser genéricos
			if ( !sourceType.IsGenericType || !destType.IsGenericType )
				return false;

			var srcGenericDef = sourceType.GetGenericTypeDefinition( );
			var dstGenericDef = destType.GetGenericTypeDefinition( );

			// Devem ter a mesma definição genérica
			if ( srcGenericDef != dstGenericDef )
				return false;

			var srcArgs = sourceType.GetGenericArguments( );
			var dstArgs = destType.GetGenericArguments( );

			// Devem ter o mesmo número de argumentos genéricos
			return srcArgs.Length == dstArgs.Length;
		}

		private void ConfigureGenericBuilder( object builder, Type builderType, Type sourceType, Type destType, Func<string, string, bool>? memberMatchRule ) {
			var srcProps = sourceType.GetProperties( BindingFlags.Public | BindingFlags.Instance );
			var dstProps = destType.GetProperties( BindingFlags.Public | BindingFlags.Instance );

			foreach ( var destProp in dstProps ) {
				if ( !destProp.CanWrite )
					continue;

				var srcProp = srcProps.FirstOrDefault( p =>
					( memberMatchRule?.Invoke( p.Name, destProp.Name ) ?? p.Name == destProp.Name )
					&& p.PropertyType != null
					&& destProp.PropertyType != null
					&& p.CanRead
				);

				if ( srcProp == null )
					continue;

				try {
					ConfigurePropertyMapping( builder, builderType, sourceType, destType, srcProp, destProp );
				} catch ( Exception ex ) {
					System.Diagnostics.Debug.WriteLine( $"[Mapper] Erro ao configurar mapeamento para propriedade {destProp.Name}: {ex.Message}" );
				}
			}
		}

		private void ConfigurePropertyMapping( object builder, Type builderType, Type sourceType, Type destType, PropertyInfo srcProp, PropertyInfo destProp ) {
			// Cria expressão lambda para a propriedade de destino
			var destParam = Expression.Parameter( destType, "d" );
			var memberAccess = Expression.MakeMemberAccess( destParam, destProp );
			var destLambda = Expression.Lambda( memberAccess, destParam );

			// Cria o resolver de valor
			var valueResolver = CreateGenericValueResolver( srcProp, sourceType, destProp.PropertyType );

			// Usa o método interno específico para evitar ambiguidade
			var forMemberMethod = builderType.GetMethod( "ForMemberInternal", BindingFlags.Instance | BindingFlags.NonPublic );

			if ( forMemberMethod != null ) {
				var genericForMember = forMemberMethod.MakeGenericMethod( destProp.PropertyType );
				genericForMember.Invoke( builder, new object[ ] { destLambda, valueResolver } );
			} else {
				System.Diagnostics.Debug.WriteLine( $"[Mapper] Método ForMemberInternal não encontrado em {builderType.Name}" );
			}
		}

		private Delegate CreateGenericValueResolver( PropertyInfo srcProp, Type srcType, Type destPropType ) {
			var srcParam = Expression.Parameter( srcType, "src" );
			var spParam = Expression.Parameter( typeof( IServiceProvider ), "sp" );

			var srcAccess = Expression.Property( srcParam, srcProp );
			Expression body = srcAccess;

			// Verifica se precisa mapear valor interno
			if ( srcProp.PropertyType != destPropType ) {
				// Verifica se pode fazer conversão direta
				if ( destPropType.IsAssignableFrom( srcProp.PropertyType ) ) {
					// Conversão direta
					body = Expression.Convert( srcAccess, destPropType );
				} else {
					// Precisa usar MapTo
					var mapToMethod = typeof( MapExtensions ).GetMethod( "MapTo", new[ ] { typeof( object ), typeof( IServiceProvider ) } )!
						.MakeGenericMethod( destPropType );
					body = Expression.Call( mapToMethod, Expression.Convert( srcAccess, typeof( object ) ), spParam );
				}
			}

			// Cria o tipo de Func correto: Func<TSource, IServiceProvider, TMember>
			var funcType = typeof( Func<,,> ).MakeGenericType( srcType, typeof( IServiceProvider ), destPropType );
			var lambda = Expression.Lambda( funcType, body, srcParam, spParam );
			return lambda.Compile( );
		}

		/// <summary>
		/// Cria instância de um tipo mesmo que não tenha construtor padrão.
		/// </summary>
		private object CreateInstance( Type type ) {
			// 1 - Tenta via DI primeiro
			var fromSp = _sp.GetService( type );
			if ( fromSp != null )
				return fromSp;

			// 2 - Tenta construtor sem parâmetros
			var ctor = type.GetConstructor( Type.EmptyTypes );
			if ( ctor != null )
				return ctor.Invoke( null );

			// 3 - Pega construtor com menos parâmetros e tenta resolver via DI
			var ctorWithParams = type
				.GetConstructors( )
				.OrderBy( c => c.GetParameters( ).Length )
				.FirstOrDefault( );

			if ( ctorWithParams == null )
				throw new InvalidOperationException( $"Tipo {type} não possui construtor acessível." );

			var args = ctorWithParams
				.GetParameters( )
				.Select( p => {
					// Tenta resolver via DI primeiro
					var serviceValue = _sp.GetService( p.ParameterType );
					if ( serviceValue != null )
						return serviceValue;

					// Senão usa valor padrão
					return p.HasDefaultValue ? p.DefaultValue : GetDefault( p.ParameterType );
				} )
				.ToArray( );

			return ctorWithParams.Invoke( args );
		}

		private static object? GetDefault( Type type )
			=> type.IsValueType ? Activator.CreateInstance( type ) : null;
	}
}