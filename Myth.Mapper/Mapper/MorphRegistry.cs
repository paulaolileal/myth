using Myth.Extensions;
using Myth.Interfaces;
using System.Diagnostics;
using System.Reflection;

namespace Myth.Morph {

	public class MorphRegistry {
		private readonly IServiceProvider _sp;
		private readonly Dictionary<(Type, Type), object> _builders = [];
		private readonly Dictionary<Type, Type> _genericInterfaceToConcrete = [];
		private readonly List<Action<Type, Type>> _genericRegisters = [];
		private readonly HashSet<(Type, Type)> _instanceBasedMappings = [];

		public MorphRegistry( IServiceProvider sp ) {
			_sp = sp;
		}

		public void Register<TSource, TDestination>( Action<BinderBuilder<TSource, TDestination>> config ) {
			var builder = new BinderBuilder<TSource, TDestination>( );
			config( builder );
			_builders[ (typeof( TSource ), typeof( TDestination )) ] = builder;
		}

		public void RegisterInstanceBasedMapping( Type sourceType, Type destinationType ) {
			_instanceBasedMappings.Add( (sourceType, destinationType) );
		}

		public void RegisterGenericMapping( Type ifaceGeneric, Type concreteGeneric ) {
			_genericInterfaceToConcrete[ ifaceGeneric ] = concreteGeneric;
		}

		public TDestination Morph<TSource, TDestination>( TSource source ) {
			var sourceType = typeof( TSource );
			var destinationType = typeof( TDestination );

			// Se o source é null, retorna default
			if ( source == null )
				return default!;

			// Usa o tipo real do objeto se for diferente do tipo genérico
			var actualSourceType = source.GetType( );

			// Verifica se é um mapeamento baseado em instância
			if ( _instanceBasedMappings.Contains( (actualSourceType, destinationType) ) && 
				 source is IMorphTo<TDestination> mapToInstance ) {
				return MapFromInstance( mapToInstance, destinationType );
			}

			// Determina o tipo concreto de destino
			var concreteDestinationType = ResolveConcreteDestinationType( destinationType );

			// Verifica se é um mapeamento de tipos genéricos
			if ( IsGenericMapping( actualSourceType, destinationType ) ) {
				return MapGenericTypes<TSource, TDestination>( source, actualSourceType, destinationType, concreteDestinationType );
			}

			// Primeiro tenta encontrar um builder direto para os tipos originais
			if ( !TryGetBuilder( actualSourceType, destinationType, out var builderObj ) ) {
				// Se não encontrou, tenta com o tipo concreto
				if ( !TryGetBuilder( actualSourceType, concreteDestinationType, out builderObj ) ) {
					// Tenta registrar dinamicamente
					foreach ( var register in _genericRegisters ) {
						register( actualSourceType, destinationType );
						register( actualSourceType, concreteDestinationType );
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
			var builder = ( BinderBuilder<TSource, TDestination> )builderObj;
			builder.ApplyAsync( source, ( TDestination )dest, _sp ).GetAwaiter( ).GetResult( );
			return ( TDestination )dest;
		}

		private bool IsGenericMapping( Type sourceType, Type destinationType ) {
			// Verifica se ambos são tipos genéricos
			if ( !sourceType.IsGenericType || !destinationType.IsGenericType )
				return false;

			var srcGenericDef = sourceType.GetGenericTypeDefinition( );
			var dstGenericDef = destinationType.GetGenericTypeDefinition( );

			// Verifica se temos um mapeamento genérico registrado
			return _genericInterfaceToConcrete.ContainsKey( dstGenericDef );
		}

		private TDestination MapGenericTypes<TSource, TDestination>(
			TSource source,
			Type actualSourceType,
			Type destinationType,
			Type concreteDestinationType ) {
			// Cria uma instância do tipo concreto
			var dest = CreateInstance( concreteDestinationType );

			// Mapeia as propriedades usando reflection
			MapPropertiesGeneric( source, dest, actualSourceType, concreteDestinationType );

			return ( TDestination )dest;
		}

		private void MapPropertiesGeneric( object source, object dest, Type sourceType, Type destType ) {
			var srcProperties = sourceType.GetProperties( BindingFlags.Public | BindingFlags.Instance )
				.Where( p => p.CanRead )
				.ToArray( );

			var destProperties = destType.GetProperties( BindingFlags.Public | BindingFlags.Instance )
				.Where( p => p.CanWrite )
				.ToArray( );

			foreach ( var destProp in destProperties ) {
				var srcProp = srcProperties.FirstOrDefault( p => p.Name == destProp.Name );
				if ( srcProp == null )
					continue;

				try {
					var srcValue = srcProp.GetValue( source );
					if ( srcValue == null )
						continue;

					var mappedValue = MapPropertyValue( srcValue, srcProp.PropertyType, destProp.PropertyType );
					destProp.SetValue( dest, mappedValue );
				} catch ( Exception ex ) {
					Debug.WriteLine( $"[Morph] Erro ao mapear propriedade {destProp.Name}: {ex.Message}" );
				}
			}
		}

		private object? MapPropertyValue( object value, Type sourceType, Type destType ) {
			// Se os tipos são iguais ou compatíveis, retorna direto
			if ( destType.IsAssignableFrom( sourceType ) ) {
				return value;
			}

			// Se é uma coleção genérica, mapeia os elementos
			if ( IsGenericCollection( sourceType ) && IsGenericCollection( destType ) ) {
				return MapGenericCollection( value, sourceType, destType );
			}

			// Tenta usar o sistema de mapeamento padrão
			try {
				var mapToMethod = typeof( MorphExtensions )
					.GetMethod( nameof(MorphExtensions.To), [typeof( object ), typeof( IServiceProvider )] )?
					.MakeGenericMethod( destType );

				return mapToMethod?.Invoke( null, new object[ ] { value, _sp } );
			} catch {
				// Se falhar, retorna o valor original se for compatível
				return destType.IsAssignableFrom( value.GetType( ) ) ? value : null;
			}
		}

		private bool IsGenericCollection( Type type ) {
			return type.IsGenericType &&
				   ( type.GetGenericTypeDefinition( ) == typeof( IEnumerable<> ) ||
					type.GetInterfaces( ).Any( i => i.IsGenericType && i.GetGenericTypeDefinition( ) == typeof( IEnumerable<> ) ) );
		}

		private object? MapGenericCollection( object sourceCollection, Type sourceType, Type destType ) {
			var sourceElementType = GetGenericArgumentType( sourceType );
			var destElementType = GetGenericArgumentType( destType );

			if ( sourceElementType == null || destElementType == null )
				return null;

			var enumerable = ( System.Collections.IEnumerable )sourceCollection;
			var mappedItems = new List<object?>( );

			foreach ( var item in enumerable ) {
				if ( item == null ) {
					mappedItems.Add( null );
					continue;
				}

				var mappedItem = MapPropertyValue( item, sourceElementType, destElementType );
				mappedItems.Add( mappedItem );
			}

			// Cria a coleção de destino apropriada
			return CreateGenericCollection( destType, destElementType, mappedItems );
		}

		private Type? GetGenericArgumentType( Type type ) {
			if ( type.IsGenericType ) {
				var args = type.GetGenericArguments( );
				return args.Length > 0 ? args[ 0 ] : null;
			}

			// Procura em interfaces implementadas
			var genericInterface = type.GetInterfaces( )
				.FirstOrDefault( i => i.IsGenericType && i.GetGenericTypeDefinition( ) == typeof( IEnumerable<> ) );

			return genericInterface?.GetGenericArguments( ).FirstOrDefault( );
		}

		private object? CreateGenericCollection( Type collectionType, Type elementType, List<object?> items ) {
			// Se é um array
			if ( collectionType.IsArray ) {
				var array = Array.CreateInstance( elementType, items.Count );

				for ( int i = 0; i < items.Count; i++ ) 
					array.SetValue( items[ i ], i );
				
				return array;
			}

			// Se é uma interface genérica (IEnumerable<T>, ICollection<T>, etc.), cria uma List<T>
			if ( collectionType.IsInterface && collectionType.IsGenericType ) {
				var listType = typeof( List<> ).MakeGenericType( elementType );
				var list = ( System.Collections.IList )Activator.CreateInstance( listType )!;
				
				foreach ( var item in items ) 
					list.Add( item );
				
				return list;
			}

			// Para tipos concretos, tenta criar instância direta
			try {
				var instance = CreateInstance( collectionType );
				if ( instance is System.Collections.IList list ) {
					foreach ( var item in items ) 
						list.Add( item );
					
					return instance;
				}
			} catch {
				// Se falhar, retorna uma List<T>
				var listType = typeof( List<> ).MakeGenericType( elementType );
				var list = ( System.Collections.IList )Activator.CreateInstance( listType )!;
				
				foreach ( var item in items ) 
					list.Add( item );
				
				return list;
			}

			return null;
		}

		private TDestination MapFromInstance<TDestination>( IMorphTo<TDestination> source, Type destinationType ) {
			// Resolve o tipo concreto se necessário
			var concreteDestinationType = ResolveConcreteDestinationType( destinationType );

			// Cria a instância de destino
			var dest = ( TDestination )CreateInstance( concreteDestinationType );

			// Cria o builder específico para instâncias
			var instanceBuilder = new BinderBuilder<TDestination>( );

			// Chama o MapTo da instância para configurar o builder
			source.Binder( instanceBuilder );

			// Aplica o mapeamento
			var applyMethod = typeof( BinderBuilder<TDestination> )
				.GetMethod( "ApplyFromInstanceAsync", BindingFlags.Instance | BindingFlags.NonPublic );

			if ( applyMethod != null ) {
				var genericApplyMethod = applyMethod.MakeGenericMethod( source.GetType( ) );
				var task = ( Task )genericApplyMethod.Invoke( instanceBuilder, [source, dest, _sp] )!;
				task
					.GetAwaiter( )
					.GetResult( );
			}

			return dest;
		}

		private Type ResolveConcreteDestinationType( Type destinationType ) {
			// Se é interface genérica, tenta resolver para concreto
			if ( destinationType.IsInterface && TryResolveGenericConcrete( destinationType, out var concrete ) ) 
				return concrete;
			

			// Se é interface não-genérica, procura implementação registrada no DI
			if ( destinationType.IsInterface ) {
				var serviceImpl = _sp.GetService( destinationType );
				if ( serviceImpl != null )
					return serviceImpl.GetType( );
			}

			return destinationType;
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

			builderObj = null;
			return false;
		}

		private TDestination ApplyMappingDynamically<TDestination>(
			object source,
			Type actualSourceType,
			object dest,
			Type concreteDestType,
			Type destinationType ) {
			// Encontra ou cria um builder compatível
			var compatibleBuilder = GetOrCreateCompatibleBuilder( actualSourceType, concreteDestType );

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

			return null;
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
			var builderType = typeof( BinderBuilder<,> ).MakeGenericType( sourceType, concreteDestType );
			var builder = Activator.CreateInstance( builderType )!;

			// Adiciona mapeamento automático básico (será feito pelo AutoMap)
			_builders[ (sourceType, destType) ] = builder;

			// Se o tipo concreto é diferente do tipo interface, registra ambos
			if ( concreteDestType != destType ) {
				_builders[ (sourceType, concreteDestType) ] = builder;
			}
		}

		public bool HasMapping( Type sourceType, Type destinationType ) {
			// Verifica se é um mapeamento baseado em instância
			if ( _instanceBasedMappings.Contains( (sourceType, destinationType) ) )
				return true;

			// Verifica mapeamento direto
			if ( _builders.ContainsKey( (sourceType, destinationType) ) )
				return true;

			// Verifica se é um mapeamento genérico
			if ( IsGenericMapping( sourceType, destinationType ) )
				return true;

			// Verifica se consegue resolver o tipo concreto e tem mapeamento para ele
			var concreteDestType = ResolveConcreteDestinationType( destinationType );
			if ( concreteDestType != destinationType && _builders.ContainsKey( (sourceType, concreteDestType) ) )
				return true;

			return false;
		}

		public void RegisterGenericEqualTypesMapping( Func<string, string, bool>? memberMatchRule = null ) {
			_genericRegisters.Add( ( sourceType, destType ) => {
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
				var builderType = typeof( BinderBuilder<,> ).MakeGenericType( sourceType, concreteDestType );
				var builder = Activator.CreateInstance( builderType )!;

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

			if ( ctorWithParams is null )
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

		private static object? GetDefault( Type type ) => 
			type.IsValueType 
			? Activator.CreateInstance( type ) 
			: null;
	}
}