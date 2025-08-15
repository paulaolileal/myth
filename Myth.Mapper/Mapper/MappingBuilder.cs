using Microsoft.Extensions.DependencyInjection;
using Myth.Extensions;
using Myth.Interfaces;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Myth.Mapper {

	/// <summary>
	/// Builder de mapeamento tradicional (TSource, TDestination)
	/// </summary>
	public class MappingBuilder<TSource, TDestination> {
		private readonly List<Action<TSource, TDestination, IServiceProvider>> _mappings = [ ];
		private readonly HashSet<string> _manuallyMappedDestProps = [ ];
		private readonly HashSet<string> _ignoredProperties = [ ];
		private readonly List<Func<TSource, TDestination, IServiceProvider, Task>> _asyncMappings = [ ];

		public MappingBuilder<TSource, TDestination> ForMember<TMember>(
			Expression<Func<TDestination, TMember>> destination,
			Func<TSource, IServiceProvider, TMember> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new ArgumentException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );
			_mappings.Add( ( src, dest, sp ) => SetValue( dest, member, resolver( src, sp ) ) );
			return this;
		}

		public MappingBuilder<TSource, TDestination> ForMember<TMember>(
			Expression<Func<TDestination, TMember>> destination,
			Func<TSource, TMember> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new ArgumentException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );
			_mappings.Add( ( src, dest, sp ) => SetValue( dest, member, resolver( src ) ) );
			return this;
		}

		// Método interno usado pelo sistema de mapeamento genérico
		internal MappingBuilder<TSource, TDestination> ForMemberInternal<TMember>(
			Expression<Func<TDestination, TMember>> destination,
			Func<TSource, IServiceProvider, TMember> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new ArgumentException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );
			_mappings.Add( ( src, dest, sp ) => SetValue( dest, member, resolver( src, sp ) ) );
			return this;
		}

		public MappingBuilder<TSource, TDestination> ForMemberAsync<TMember>(
			Expression<Func<TDestination, TMember>> destination,
			Func<TSource, IServiceProvider, Task<TMember>> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new ArgumentException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );
			_asyncMappings.Add( async ( src, dest, sp ) => {
				var value = await resolver( src, sp );
				SetValue( dest, member, value );
			} );
			return this;
		}

		public MappingBuilder<TSource, TDestination> ForMemberAsync<TMember>(
			Expression<Func<TDestination, TMember>> destination,
			Func<TSource, Task<TMember>> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new ArgumentException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );
			_asyncMappings.Add( async ( src, dest, sp ) => {
				var value = await resolver( src );
				SetValue( dest, member, value );
			} );
			return this;
		}

		internal async Task ApplyAsync( TSource src, TDestination dest, IServiceProvider sp ) {
			// Aplica mapeamentos síncronos primeiro
			foreach ( var map in _mappings )
				map( src, dest, sp );

			// Depois os assíncronos
			foreach ( var asyncMap in _asyncMappings )
				await asyncMap( src, dest, sp );

			// Por último, auto-mapping para propriedades não mapeadas manualmente
			AutoMap( src, dest, sp );
		}

		public MappingBuilder<TSource, TDestination> Ignore<TValue>( Expression<Func<TDestination, TValue>> destSelector ) {
			if ( destSelector.Body is MemberExpression member )
				_ignoredProperties.Add( member.Member.Name );
			return this;
		}

		private void AutoMap( TSource src, TDestination dest, IServiceProvider sp ) {
			var srcType = typeof( TSource );
			var destType = typeof( TDestination );

			// Se estamos lidando com objetos reais, usar seus tipos atuais
			if ( src != null && src.GetType( ) != srcType )
				srcType = src.GetType( );

			if ( dest != null && dest.GetType( ) != destType )
				destType = dest.GetType( );

			var srcMembers = srcType.GetMembers( BindingFlags.Public | BindingFlags.Instance );
			var destMembers = destType.GetMembers( BindingFlags.Public | BindingFlags.Instance );

			foreach ( var destMember in destMembers ) {
				if ( _manuallyMappedDestProps.Contains( destMember.Name ) || _ignoredProperties.Contains( destMember.Name ) )
					continue;

				var srcMember = srcMembers.FirstOrDefault( m => m.Name == destMember.Name );
				if ( srcMember == null )
					continue;

				var srcMemberType = GetMemberType( srcMember );
				var destMemberType = GetMemberType( destMember );

				if ( srcMemberType == null || destMemberType == null )
					continue;

				// Verifica se o membro de destino pode ser escrito
				if ( !CanWriteMember( destMember ) )
					continue;

				object? srcValue = null;
				try {
					srcValue = srcMember switch {
						PropertyInfo p => p.GetValue( src ),
						FieldInfo f => f.GetValue( src ),
						_ => null
					};
				} catch ( Exception ex ) {
					Debug.WriteLine( $"[AutoMap] Erro ao ler '{srcMember.Name}': {ex.Message}" );
					continue;
				}

				if ( srcValue == null ) {
					// Define valor padrão se possível
					if ( destMemberType.IsValueType && Nullable.GetUnderlyingType( destMemberType ) == null ) {
						SetValue( dest, destMember, Activator.CreateInstance( destMemberType ) );
					}
					continue;
				}

				try {
					var mappedValue = MapValue( srcValue, srcMemberType, destMemberType, sp );
					SetValue( dest, destMember, mappedValue );
				} catch ( Exception ex ) {
					Debug.WriteLine( $"[AutoMap] Erro ao mapear '{srcMember.Name}' -> '{destMember.Name}': {ex.Message}" );
				}
			}
		}

		private object? MapValue( object srcValue, Type srcType, Type destType, IServiceProvider sp ) {
			// Se os tipos são iguais, retorna direto
			if ( destType.IsAssignableFrom( srcType ) )
				return srcValue;

			// Tenta conversão direta
			if ( TryConvertDirect( srcValue, destType, out var converted ) )
				return converted;

			// Tenta mapear usando o registry
			var registry = ( MapRegistry? )sp.GetService( typeof( MapRegistry ) );
			if ( registry != null ) {
				try {
					// Verifica se tem mapeamento registrado
					if ( registry.HasMapping( srcType, destType ) ) {
						var mapMethod = typeof( MapRegistry ).GetMethod( "Map" )?.MakeGenericMethod( srcType, destType );
						return mapMethod?.Invoke( registry, [ srcValue ] );
					}

					// Se é uma coleção, tenta mapear os elementos
					if ( TryMapCollection( srcValue, srcType, destType, sp, out var mappedCollection ) )
						return mappedCollection;
				} catch ( Exception ex ) {
					Debug.WriteLine( $"[AutoMap] Falha no mapeamento via registry de {srcType.Name} para {destType.Name}: {ex.Message}" );
				}
			}

			Debug.WriteLine( $"[AutoMap] Não foi possível mapear {srcType.Name} para {destType.Name}" );
			return null;
		}

		private bool TryConvertDirect( object value, Type targetType, out object? result ) {
			result = null;

			try {
				// Handle nullable types
				var underlyingType = Nullable.GetUnderlyingType( targetType );
				if ( underlyingType != null ) {
					if ( value == null ) {
						result = null;
						return true;
					}
					targetType = underlyingType;
				}

				// Try direct conversion
				if ( targetType.IsAssignableFrom( value.GetType( ) ) ) {
					result = value;
					return true;
				}

				// Try Convert.ChangeType for basic types
				if ( targetType.IsPrimitive || targetType == typeof( string ) || targetType == typeof( DateTime ) || targetType == typeof( decimal ) ) {
					result = Convert.ChangeType( value, targetType );
					return true;
				}

				return false;
			} catch {
				return false;
			}
		}

		private bool TryMapCollection( object srcValue, Type srcType, Type destType, IServiceProvider sp, out object? result ) {
			result = null;

			// Verifica se ambos implementam IEnumerable
			if ( !typeof( System.Collections.IEnumerable ).IsAssignableFrom( srcType ) ||
				 !typeof( System.Collections.IEnumerable ).IsAssignableFrom( destType ) )
				return false;

			var srcElementType = GetElementType( srcType );
			var destElementType = GetElementType( destType );

			if ( srcElementType == null || destElementType == null )
				return false;

			try {
				var enumerable = ( System.Collections.IEnumerable )srcValue;
				var items = new List<object?>( );

				foreach ( var item in enumerable ) {
					if ( item == null ) {
						items.Add( null );
						continue;
					}

					var mappedItem = MapValue( item, srcElementType, destElementType, sp );
					items.Add( mappedItem );
				}

				// Cria a coleção de destino
				result = CreateCollection( destType, destElementType, items );
				return result != null;
			} catch {
				return false;
			}
		}

		private Type? GetElementType( Type collectionType ) {
			// Para arrays
			if ( collectionType.IsArray )
				return collectionType.GetElementType( );

			// Para tipos genéricos (List<T>, IEnumerable<T>, etc.)
			if ( collectionType.IsGenericType ) {
				var args = collectionType.GetGenericArguments( );
				return args.Length > 0 ? args[ 0 ] : null;
			}

			// Para interfaces genéricas implementadas
			var genericInterface = collectionType.GetInterfaces( )
				.FirstOrDefault( i => i.IsGenericType && i.GetGenericTypeDefinition( ) == typeof( IEnumerable<> ) );

			return genericInterface?.GetGenericArguments( ).FirstOrDefault( );
		}

		private object? CreateCollection( Type collectionType, Type elementType, List<object?> items ) {
			// Array
			if ( collectionType.IsArray ) {
				var array = Array.CreateInstance( elementType, items.Count );
				for ( int i = 0; i < items.Count; i++ ) {
					array.SetValue( items[ i ], i );
				}
				return array;
			}

			// List<T>
			if ( collectionType.IsGenericType && collectionType.GetGenericTypeDefinition( ) == typeof( List<> ) ) {
				var listType = typeof( List<> ).MakeGenericType( elementType );
				var list = ( System.Collections.IList )Activator.CreateInstance( listType )!;
				foreach ( var item in items ) {
					list.Add( item );
				}
				return list;
			}

			// IEnumerable<T>, ICollection<T>, IList<T> -> converte para List<T>
			if ( collectionType.IsInterface && collectionType.IsGenericType ) {
				var genericDef = collectionType.GetGenericTypeDefinition( );
				if ( genericDef == typeof( IEnumerable<> ) ||
					 genericDef == typeof( ICollection<> ) ||
					 genericDef == typeof( IList<> ) ) {
					var listType = typeof( List<> ).MakeGenericType( elementType );
					var list = ( System.Collections.IList )Activator.CreateInstance( listType )!;
					foreach ( var item in items ) {
						list.Add( item );
					}
					return list;
				}
			}

			// Tenta criar instância direta e adicionar items
			try {
				var instance = Activator.CreateInstance( collectionType );
				if ( instance is System.Collections.IList list ) {
					foreach ( var item in items ) {
						list.Add( item );
					}
					return instance;
				}
			} catch {
				// Falhou, retorna null
			}

			return null;
		}

		private bool CanWriteMember( MemberInfo member ) => member switch {
			PropertyInfo p => p.CanWrite,
			FieldInfo f => !f.IsInitOnly && !f.IsLiteral,
			_ => false
		};

		private static Type? GetMemberType( MemberInfo member ) => member switch {
			PropertyInfo p => p.PropertyType,
			FieldInfo f => f.FieldType,
			_ => null
		};

		private static void SetValue( object target, MemberInfo member, object? value ) {
			try {
				switch ( member ) {
					case PropertyInfo p when p.CanWrite:
					p.SetValue( target, value );
					break;

					case FieldInfo f when !f.IsInitOnly && !f.IsLiteral:
					f.SetValue( target, value );
					break;

					default:
					Debug.WriteLine( $"[AutoMap] Membro '{member.Name}' não é atribuível." );
					break;
				}
			} catch ( Exception ex ) {
				Debug.WriteLine( $"[AutoMap] Falha ao atribuir '{member.Name}': {ex.Message}" );
			}
		}
	}

	/// <summary>
	/// Builder de mapeamento baseado em instância (apenas TDestination)
	/// </summary>
	public class MappingBuilder<TDestination> {
		private readonly List<Action<TDestination, IServiceProvider>> _mappings = [ ];
		private readonly HashSet<string> _manuallyMappedDestProps = [ ];
		private readonly HashSet<string> _ignoredProperties = [ ];
		private readonly List<Func<TDestination, IServiceProvider, Task>> _asyncMappings = [ ];

		public MappingBuilder<TDestination> ForMember<TMember>(
			Expression<Func<TDestination, TMember>> destination,
			Func<IServiceProvider, TMember> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new ArgumentException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );
			_mappings.Add( ( dest, sp ) => SetValue( dest, member, resolver( sp ) ) );
			return this;
		}

		public MappingBuilder<TDestination> ForMember<TMember>(
			Expression<Func<TDestination, TMember>> destination,
			Func<TMember> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new ArgumentException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );
			_mappings.Add( ( dest, sp ) => SetValue( dest, member, resolver( ) ) );
			return this;
		}

		public MappingBuilder<TDestination> ForMemberAsync<TMember>(
			Expression<Func<TDestination, TMember>> destination,
			Func<IServiceProvider, Task<TMember>> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new ArgumentException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );
			_asyncMappings.Add( async ( dest, sp ) => {
				var value = await resolver( sp );
				SetValue( dest, member, value );
			} );
			return this;
		}

		public MappingBuilder<TDestination> ForMemberAsync<TMember>(
			Expression<Func<TDestination, TMember>> destination,
			Func<Task<TMember>> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new ArgumentException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );
			_asyncMappings.Add( async ( dest, sp ) => {
				var value = await resolver( );
				SetValue( dest, member, value );
			} );
			return this;
		}

		public MappingBuilder<TDestination> Ignore<TValue>( Expression<Func<TDestination, TValue>> destSelector ) {
			if ( destSelector.Body is MemberExpression member )
				_ignoredProperties.Add( member.Member.Name );
			return this;
		}

		internal async Task ApplyFromInstanceAsync<TSource>( TSource src, TDestination dest, IServiceProvider sp ) where TSource : IMapTo<TDestination> {
			// Aplica mapeamentos síncronos primeiro
			foreach ( var map in _mappings )
				map( dest, sp );

			// Depois os assíncronos
			foreach ( var asyncMap in _asyncMappings )
				await asyncMap( dest, sp );

			// Por último, auto-mapping para propriedades não mapeadas manualmente
			AutoMapFromInstance( src, dest, sp );
		}

		private void AutoMapFromInstance<TSource>( TSource src, TDestination dest, IServiceProvider sp ) {
			var srcType = src?.GetType( ) ?? typeof( TSource );
			var destType = dest?.GetType( ) ?? typeof( TDestination );

			var srcMembers = srcType.GetMembers( BindingFlags.Public | BindingFlags.Instance );
			var destMembers = destType.GetMembers( BindingFlags.Public | BindingFlags.Instance );

			foreach ( var destMember in destMembers ) {
				if ( _manuallyMappedDestProps.Contains( destMember.Name ) || _ignoredProperties.Contains( destMember.Name ) )
					continue;

				var srcMember = srcMembers.FirstOrDefault( m => m.Name == destMember.Name );
				if ( srcMember == null )
					continue;

				var srcMemberType = GetMemberType( srcMember );
				var destMemberType = GetMemberType( destMember );

				if ( srcMemberType == null || destMemberType == null )
					continue;

				// Verifica se o membro de destino pode ser escrito
				if ( !CanWriteMember( destMember ) )
					continue;

				object? srcValue = null;
				try {
					srcValue = srcMember switch {
						PropertyInfo p => p.GetValue( src ),
						FieldInfo f => f.GetValue( src ),
						_ => null
					};
				} catch ( Exception ex ) {
					Debug.WriteLine( $"[AutoMap] Erro ao ler '{srcMember.Name}': {ex.Message}" );
					continue;
				}

				if ( srcValue == null ) {
					// Define valor padrão se possível
					if ( destMemberType.IsValueType && Nullable.GetUnderlyingType( destMemberType ) == null ) {
						SetValue( dest, destMember, Activator.CreateInstance( destMemberType ) );
					}
					continue;
				}

				try {
					var mappedValue = MapValue( srcValue, srcMemberType, destMemberType, sp );
					SetValue( dest, destMember, mappedValue );
				} catch ( Exception ex ) {
					Debug.WriteLine( $"[AutoMap] Erro ao mapear '{srcMember.Name}' -> '{destMember.Name}': {ex.Message}" );
				}
			}
		}

		private object? MapValue( object srcValue, Type srcType, Type destType, IServiceProvider sp ) {
			// Se os tipos são iguais, retorna direto
			if ( destType.IsAssignableFrom( srcType ) )
				return srcValue;

			// Tenta conversão direta
			if ( TryConvertDirect( srcValue, destType, out var converted ) )
				return converted;

			// Tenta mapear usando extensões
			try {
				using ( var scope = sp.CreateScope( ) ) {
					var method = typeof( MapExtensions ).GetMethod( "MapTo", new[ ] { typeof( object ), typeof( IServiceProvider ) } )
						?.MakeGenericMethod( destType );
					return method?.Invoke( null, [ srcValue, scope.ServiceProvider ] );
				}
			} catch ( Exception ex ) {
				Debug.WriteLine( $"[AutoMap] Falha no mapeamento de {srcType.Name} para {destType.Name}: {ex.Message}" );
			}

			Debug.WriteLine( $"[AutoMap] Não foi possível mapear {srcType.Name} para {destType.Name}" );
			return null;
		}

		private bool TryConvertDirect( object value, Type targetType, out object? result ) {
			result = null;

			try {
				// Handle nullable types
				var underlyingType = Nullable.GetUnderlyingType( targetType );
				if ( underlyingType != null ) {
					if ( value == null ) {
						result = null;
						return true;
					}
					targetType = underlyingType;
				}

				// Try direct conversion
				if ( targetType.IsAssignableFrom( value.GetType( ) ) ) {
					result = value;
					return true;
				}

				// Try Convert.ChangeType for basic types
				if ( targetType.IsPrimitive || targetType == typeof( string ) || targetType == typeof( DateTime ) || targetType == typeof( decimal ) ) {
					result = Convert.ChangeType( value, targetType );
					return true;
				}

				return false;
			} catch {
				return false;
			}
		}

		private bool CanWriteMember( MemberInfo member ) => member switch {
			PropertyInfo p => p.CanWrite,
			FieldInfo f => !f.IsInitOnly && !f.IsLiteral,
			_ => false
		};

		private static Type? GetMemberType( MemberInfo member ) => member switch {
			PropertyInfo p => p.PropertyType,
			FieldInfo f => f.FieldType,
			_ => null
		};

		private static void SetValue( object target, MemberInfo member, object? value ) {
			try {
				switch ( member ) {
					case PropertyInfo p when p.CanWrite:
					p.SetValue( target, value );
					break;

					case FieldInfo f when !f.IsInitOnly && !f.IsLiteral:
					f.SetValue( target, value );
					break;

					default:
					Debug.WriteLine( $"[AutoMap] Membro '{member.Name}' não é atribuível." );
					break;
				}
			} catch ( Exception ex ) {
				Debug.WriteLine( $"[AutoMap] Falha ao atribuir '{member.Name}': {ex.Message}" );
			}
		}
	}
}