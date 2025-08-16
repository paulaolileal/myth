using Microsoft.Extensions.DependencyInjection;
using Myth.Exceptions;
using Myth.Extensions;
using Myth.Interfaces;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Myth.Morph {

	/// <summary>
	/// Builder de mapeamento baseado em instância (apenas TDestination)
	/// </summary>
	public class BinderBuilder<TDestination> {
		private readonly List<Action<TDestination, IServiceProvider>> _mappings = [ ];
		private readonly HashSet<string> _manuallyMappedDestProps = [ ];
		private readonly HashSet<string> _ignoredProperties = [ ];
		private readonly List<Func<TDestination, IServiceProvider, Task>> _asyncMappings = [ ];

		public BinderBuilder<TDestination> Bind<TMember>( Expression<Func<TDestination, TMember>> destination, Func<IServiceProvider, TMember> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new BindException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );

			_mappings.Add( ( dest, sp ) => SetValue( dest, member, resolver( sp ) ) );

			return this;
		}

		public BinderBuilder<TDestination> Bind<TMember>( Expression<Func<TDestination, TMember>> destination, Func<TMember> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new BindException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );

			_mappings.Add( ( dest, sp ) => SetValue( dest, member, resolver( ) ) );

			return this;
		}

		public BinderBuilder<TDestination> BindAsync<TMember>( Expression<Func<TDestination, TMember>> destination, Func<IServiceProvider, Task<TMember>> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new BindException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );

			_asyncMappings.Add( async ( dest, sp ) => {
				var value = await resolver( sp );
				SetValue( dest, member, value );
			} );

			return this;
		}

		public BinderBuilder<TDestination> BindAsync<TMember>( Expression<Func<TDestination, TMember>> destination, Func<Task<TMember>> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new BindException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );

			_asyncMappings.Add( async ( dest, sp ) => {
				var value = await resolver( );
				SetValue( dest, member, value );
			} );

			return this;
		}

		public BinderBuilder<TDestination> Ignore<TValue>( Expression<Func<TDestination, TValue>> destSelector ) {
			if ( destSelector.Body is MemberExpression member )
				_ignoredProperties.Add( member.Member.Name );

			return this;
		}

		internal async Task ApplyFromInstanceAsync<TSource>( TSource src, TDestination dest, IServiceProvider sp ) where TSource : IMorphTo<TDestination> {
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
					Debug.WriteLine( $"[Morph] Erro ao ler '{srcMember.Name}': {ex.Message}" );

					continue;
				}

				if ( srcValue == null ) {
					// Define valor padrão se possível
					if ( destMemberType.IsValueType && Nullable.GetUnderlyingType( destMemberType ) == null )
						SetValue( dest, destMember, Activator.CreateInstance( destMemberType ) );

					continue;
				}

				try {
					var mappedValue = MapValue( srcValue, srcMemberType, destMemberType, sp );

					SetValue( dest, destMember, mappedValue );
				} catch ( Exception ex ) {
					Debug.WriteLine( $"[Morph] Erro ao mapear '{srcMember.Name}' -> '{destMember.Name}': {ex.Message}" );
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
				using var scope = sp.CreateScope( );

				var method = typeof( MorphExtensions )
					.GetMethod( "MapTo", [ typeof( object ), typeof( IServiceProvider ) ] )?
					.MakeGenericMethod( destType );

				return method?.Invoke( null, [ srcValue, scope.ServiceProvider ] );
			} catch ( Exception ex ) {
				Debug.WriteLine( $"[Morph] Falha no mapeamento de {srcType.Name} para {destType.Name}: {ex.Message}" );
			}

			Debug.WriteLine( $"[Morph] Não foi possível mapear {srcType.Name} para {destType.Name}" );

			return null;
		}

		private static bool TryConvertDirect( object value, Type targetType, out object? result ) {
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
				if ( targetType.IsPrimitive ||
					 targetType == typeof( string ) ||
					 targetType == typeof( DateTime ) ||
					 targetType == typeof( decimal ) ) {
					result = Convert.ChangeType( value, targetType );

					return true;
				}

				return false;
			} catch {
				return false;
			}
		}

		private bool CanWriteMember( MemberInfo member ) =>
			member switch {
				PropertyInfo p => p.CanWrite,
				FieldInfo f => !f.IsInitOnly && !f.IsLiteral,
				_ => false
			};

		private static Type? GetMemberType( MemberInfo member ) =>
			member switch {
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
					Debug.WriteLine( $"[Bind] Membro '{member.Name}' não é atribuível." );
					break;
				}
			} catch ( Exception ex ) {
				Debug.WriteLine( $"[Bind] Falha ao atribuir '{member.Name}': {ex.Message}" );
			}
		}
	}
}