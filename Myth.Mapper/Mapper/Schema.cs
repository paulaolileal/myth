using Microsoft.Extensions.DependencyInjection;
using Myth.Exceptions;
using Myth.Extensions;
using Myth.Interfaces;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

namespace Myth.Morph {

	/// <summary>
	/// Provides a builder for configuring bindings between a source and a destination object.
	/// </summary>
	/// <remarks>The <see cref="Schema{TDestination}"/> class allows for the creation of mappings between
	/// properties or fields of a destination object and values resolved from various sources, such as service providers or
	/// custom resolvers. It supports both synchronous and asynchronous bindings, as well as the ability to ignore specific
	/// properties.</remarks>
	/// <typeparam name="TDestination">The type of the destination object to which bindings will be applied.</typeparam>
	public class Schema<TDestination> {
		private readonly List<Action<TDestination, IServiceProvider>> _mappings = [ ];
		private readonly HashSet<string> _manuallyMappedDestProps = [ ];
		private readonly HashSet<string> _ignoredProperties = [ ];
		private readonly List<Func<TDestination, IServiceProvider, Task>> _asyncMappings = [ ];

		/// <summary>
		/// Configures a binding between a destination property and a value resolved from a service provider.
		/// </summary>
		/// <remarks>This method allows you to manually map a property of the destination type to a value resolved at
		/// runtime. The <paramref name="resolver"/> function is invoked with the <see cref="IServiceProvider"/> to obtain the
		/// value.</remarks>
		/// <typeparam name="TMember">The type of the destination property.</typeparam>
		/// <param name="destination">An expression specifying the destination property to bind.</param>
		/// <param name="resolver">A function that resolves the value for the destination property using a service provider.</param>
		/// <returns>A <see cref="Schema{TDestination}"/> instance for chaining additional bindings.</returns>
		/// <exception cref="BindException">Thrown if the <paramref name="destination"/> expression does not represent a valid member of the destination type.</exception>
		public Schema<TDestination> Bind<TMember>( Expression<Func<TDestination, TMember>> destination, Func<IServiceProvider, TMember> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new BindException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );

			_mappings.Add( ( dest, sp ) => SetValue( dest, member, resolver( sp ) ) );

			return this;
		}

		/// <summary>
		/// Configures a binding between a destination property and a resolver function.
		/// </summary>
		/// <remarks>This method allows you to manually map a destination property to a value resolver function. The
		/// resolver function is invoked to determine the value to assign to the specified property.</remarks>
		/// <typeparam name="TMember">The type of the destination property being bound.</typeparam>
		/// <param name="destination">An expression specifying the destination property to bind.  The expression must be a valid member access
		/// expression.</param>
		/// <param name="resolver">A function that resolves the value to be assigned to the destination property.</param>
		/// <returns>A <see cref="Schema{TDestination}"/> instance, allowing for further configuration.</returns>
		/// <exception cref="BindException">Thrown if the <paramref name="destination"/> expression is not a valid member access expression.</exception>
		public Schema<TDestination> Bind<TMember>( Expression<Func<TDestination, TMember>> destination, Func<TMember> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new BindException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );

			_mappings.Add( ( dest, sp ) => SetValue( dest, member, resolver( ) ) );

			return this;
		}

		/// <summary>
		/// Configures an asynchronous binding for a specified destination property.
		/// </summary>
		/// <remarks>This method enables asynchronous resolution of values for destination properties during the
		/// binding process. The <paramref name="destination"/> expression must represent a valid property of the destination
		/// type. If the expression is invalid, a <see cref="BindException"/> is thrown.</remarks>
		/// <typeparam name="TMember">The type of the destination property to bind.</typeparam>
		/// <param name="destination">An expression specifying the destination property to bind.</param>
		/// <param name="resolver">A function that asynchronously resolves the value to be assigned to the destination property. The function
		/// receives an <see cref="IServiceProvider"/> for dependency resolution.</param>
		/// <returns>A <see cref="Schema{TDestination}"/> instance, allowing further configuration of bindings.</returns>
		/// <exception cref="BindException">Thrown if the <paramref name="destination"/> expression does not represent a valid property of the destination
		/// type.</exception>
		public Schema<TDestination> BindAsync<TMember>( Expression<Func<TDestination, TMember>> destination, Func<IServiceProvider, Task<TMember>> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new BindException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );

			_asyncMappings.Add( async ( dest, sp ) => {
				var value = await resolver( sp );
				SetValue( dest, member, value );
			} );

			return this;
		}

		/// <summary>
		/// Maps a destination property to an asynchronous resolver function.
		/// </summary>
		/// <remarks>
		/// This method enables asynchronous binding of a destination property to a value resolved at
		/// runtime. The resolver function is executed asynchronously, and its result is assigned to the specified 
		/// destination property.
		/// </remarks>
		/// <typeparam name="TMember">The type of the destination property being mapped.</typeparam>
		/// <param name="destination">An expression specifying the destination property to bind. The expression must be a valid  member access
		/// expression (e.g., <c>x => x.PropertyName</c>).</param>
		/// <param name="resolver">A function that asynchronously resolves the value to be assigned to the destination property.</param>
		/// <returns>A <see cref="Schema{TDestination}"/> instance, allowing further configuration of bindings.</returns>
		/// <exception cref="BindException">Thrown if the <paramref name="destination"/> expression is not a valid member access expression.</exception>
		public Schema<TDestination> BindAsync<TMember>( Expression<Func<TDestination, TMember>> destination, Func<Task<TMember>> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new BindException( "Expressão inválida para destino." );

			_manuallyMappedDestProps.Add( member.Name );

			_asyncMappings.Add( async ( dest, sp ) => {
				var value = await resolver( );
				SetValue( dest, member, value );
			} );

			return this;
		}

		/// <summary>
		/// Excludes the specified property from the binding process.
		/// </summary>
		/// <typeparam name="TValue">The type of the property to be ignored.</typeparam>
		/// <param name="destSelector">An expression that specifies the property of the destination type to ignore. The expression must be a member
		/// access expression.</param>
		/// <returns>The current <see cref="Schema{TDestination}"/> instance, allowing for method chaining.</returns>
		public Schema<TDestination> Ignore<TValue>( Expression<Func<TDestination, TValue>> destSelector ) {
			if ( destSelector.Body is MemberExpression member )
				_ignoredProperties.Add( member.Member.Name );

			return this;
		}

		/// <summary>
		/// Applies mappings from the source instance to the destination instance asynchronously.
		/// </summary>
		/// <remarks>This method applies mappings in three stages: <list type="number"> <item>First, synchronous
		/// mappings are applied.</item> <item>Second, asynchronous mappings are applied.</item> <item>Finally, automatic
		/// mappings are applied for properties not manually mapped.</item> </list> Ensure that both <paramref name="src"/>
		/// and <paramref name="dest"/> are properly initialized before calling this method.</remarks>
		/// <typeparam name="TSource">The type of the source instance, which must implement <see cref="IMorphable{TDestination}"/>.</typeparam>
		/// <param name="src">The source instance from which mappings are applied. Cannot be <see langword="null"/>.</param>
		/// <param name="dest">The destination instance to which mappings are applied. Cannot be <see langword="null"/>.</param>
		/// <param name="sp">The <see cref="IServiceProvider"/> used to resolve dependencies during the mapping process. Cannot be <see
		/// langword="null"/>.</param>
		/// <returns></returns>
		internal async Task ApplyFromInstanceAsync<TSource>( TSource src, TDestination dest, IServiceProvider sp ) where TSource : IMorphable<TDestination> {
			// Apply synced maps 
			foreach ( var map in _mappings )
				map( dest, sp );

			// Apply async maps
			foreach ( var asyncMap in _asyncMappings )
				await asyncMap( dest, sp );

			// Apply auto-map for not mpapped properties
			AutoMapFromInstance( src, dest, sp );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="TSource"></typeparam>
		/// <param name="src"></param>
		/// <param name="dest"></param>
		/// <param name="sp"></param>
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