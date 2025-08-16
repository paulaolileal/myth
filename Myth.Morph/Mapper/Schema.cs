using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Exceptions;
using Myth.Extensions;
using Myth.Interfaces;
using System.Linq.Expressions;
using System.Reflection;

namespace Myth.Morph {

	/// <summary>
	/// Provides a builder for configuring bindings between a source and a destination object.
	/// </summary>
	/// <remarks>
	/// The <see cref="Schema{TDestination}"/> class allows for the creation of mappings between
	/// properties or fields of a destination object and values resolved from various sources, such as service providers or
	/// custom resolvers. It supports both synchronous and asynchronous bindings, as well as the ability to ignore specific
	/// properties during the mapping process.
	/// </remarks>
	/// <typeparam name="TDestination">The type of the destination object to which bindings will be applied.</typeparam>
	public class Schema<TDestination> {
		private readonly List<Action<TDestination, IServiceProvider>> _mappings = [ ];
		private readonly HashSet<string> _manuallyMappedDestProps = [ ];
		private readonly HashSet<string> _ignoredProperties = [ ];
		private readonly List<Func<TDestination, IServiceProvider, Task>> _asyncMappings = [ ];

		/// <summary>
		/// Configures a binding between a destination property and a value resolved from a service provider.
		/// </summary>
		/// <remarks>
		/// This method allows you to manually map a property of the destination type to a value resolved at
		/// runtime. The <paramref name="resolver"/> function is invoked with the <see cref="IServiceProvider"/> to obtain the
		/// value. The property specified by the <paramref name="destination"/> expression will be marked as manually mapped
		/// and will not be subject to automatic mapping.
		/// </remarks>
		/// <typeparam name="TMember">The type of the destination property.</typeparam>
		/// <param name="destination">An expression specifying the destination property to bind.</param>
		/// <param name="resolver">A function that resolves the value for the destination property using a service provider.</param>
		/// <returns>A <see cref="Schema{TDestination}"/> instance for chaining additional bindings.</returns>
		/// <exception cref="BindException">Thrown if the <paramref name="destination"/> expression does not represent a valid member of the destination type.</exception>
		public Schema<TDestination> Bind<TMember>( Expression<Func<TDestination, TMember>> destination, Func<IServiceProvider, TMember> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new BindException( "Invalid expression for destination." );

			_manuallyMappedDestProps.Add( member.Name );

			_mappings.Add( ( dest, sp ) => {
				var logger = GetLogger( sp );
				logger?.LogTrace( "Applying service provider binding for property {PropertyName}", member.Name );
				SetValue( dest, member, resolver( sp ), logger );
			} );

			return this;
		}

		/// <summary>
		/// Configures a binding between a destination property and a resolver function.
		/// </summary>
		/// <remarks>
		/// This method allows you to manually map a destination property to a value resolver function. The
		/// resolver function is invoked to determine the value to assign to the specified property.
		/// The property will be marked as manually mapped and excluded from automatic mapping.
		/// </remarks>
		/// <typeparam name="TMember">The type of the destination property being bound.</typeparam>
		/// <param name="destination">An expression specifying the destination property to bind. The expression must be a valid member access
		/// expression.</param>
		/// <param name="resolver">A function that resolves the value to be assigned to the destination property.</param>
		/// <returns>A <see cref="Schema{TDestination}"/> instance, allowing for further configuration.</returns>
		/// <exception cref="BindException">Thrown if the <paramref name="destination"/> expression is not a valid member access expression.</exception>
		public Schema<TDestination> Bind<TMember>( Expression<Func<TDestination, TMember>> destination, Func<TMember> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new BindException( "Invalid expression for destination." );

			_manuallyMappedDestProps.Add( member.Name );

			_mappings.Add( ( dest, sp ) => {
				var logger = GetLogger( sp );
				logger?.LogTrace( "Applying direct value binding for property {PropertyName}", member.Name );
				SetValue( dest, member, resolver( ), logger );
			} );

			return this;
		}

		/// <summary>
		/// Configures an asynchronous binding for a specified destination property.
		/// </summary>
		/// <remarks>
		/// This method enables asynchronous resolution of values for destination properties during the
		/// binding process. The <paramref name="destination"/> expression must represent a valid property of the destination
		/// type. If the expression is invalid, a <see cref="BindException"/> is thrown. The property will be marked
		/// as manually mapped and excluded from automatic mapping.
		/// </remarks>
		/// <typeparam name="TMember">The type of the destination property to bind.</typeparam>
		/// <param name="destination">An expression specifying the destination property to bind.</param>
		/// <param name="resolver">A function that asynchronously resolves the value to be assigned to the destination property. The function
		/// receives an <see cref="IServiceProvider"/> for dependency resolution.</param>
		/// <returns>A <see cref="Schema{TDestination}"/> instance, allowing further configuration of bindings.</returns>
		/// <exception cref="BindException">Thrown if the <paramref name="destination"/> expression does not represent a valid property of the destination
		/// type.</exception>
		public Schema<TDestination> BindAsync<TMember>( Expression<Func<TDestination, TMember>> destination, Func<IServiceProvider, Task<TMember>> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new BindException( "Invalid expression for destination." );

			_manuallyMappedDestProps.Add( member.Name );

			_asyncMappings.Add( async ( dest, sp ) => {
				var logger = GetLogger( sp );
				logger?.LogTrace( "Applying async service provider binding for property {PropertyName}", member.Name );
				var value = await resolver( sp );
				SetValue( dest, member, value, logger );
			} );

			return this;
		}

		/// <summary>
		/// Maps a destination property to an asynchronous resolver function.
		/// </summary>
		/// <remarks>
		/// This method enables asynchronous binding of a destination property to a value resolved at
		/// runtime. The resolver function is executed asynchronously, and its result is assigned to the specified
		/// destination property. The property will be marked as manually mapped and excluded from automatic mapping.
		/// </remarks>
		/// <typeparam name="TMember">The type of the destination property being mapped.</typeparam>
		/// <param name="destination">An expression specifying the destination property to bind. The expression must be a valid member access
		/// expression (e.g., <c>x => x.PropertyName</c>).</param>
		/// <param name="resolver">A function that asynchronously resolves the value to be assigned to the destination property.</param>
		/// <returns>A <see cref="Schema{TDestination}"/> instance, allowing further configuration of bindings.</returns>
		/// <exception cref="BindException">Thrown if the <paramref name="destination"/> expression is not a valid member access expression.</exception>
		public Schema<TDestination> BindAsync<TMember>( Expression<Func<TDestination, TMember>> destination, Func<Task<TMember>> resolver ) {
			if ( destination.Body is not MemberExpression memberExp || memberExp.Member is not MemberInfo member )
				throw new BindException( "Invalid expression for destination." );

			_manuallyMappedDestProps.Add( member.Name );

			_asyncMappings.Add( async ( dest, sp ) => {
				var logger = GetLogger( sp );
				logger?.LogTrace( "Applying async direct value binding for property {PropertyName}", member.Name );
				var value = await resolver( );
				SetValue( dest, member, value, logger );
			} );

			return this;
		}

		/// <summary>
		/// Excludes the specified property from the binding process.
		/// </summary>
		/// <remarks>
		/// This method marks a property to be ignored during both manual and automatic mapping processes.
		/// The property will not be assigned any value during the transformation.
		/// </remarks>
		/// <typeparam name="TValue">The type of the property to be ignored.</typeparam>
		/// <param name="destSelector">An expression that specifies the property of the destination type to ignore. The expression must be a member
		/// access expression.</param>
		/// <returns>The current <see cref="Schema{TDestination}"/> instance, allowing for method chaining.</returns>
		public Schema<TDestination> Ignore<TValue>( Expression<Func<TDestination, TValue>> destSelector ) {
			if ( destSelector.Body is MemberExpression member ) {
				_ignoredProperties.Add( member.Member.Name );
			}

			return this;
		}

		/// <summary>
		/// Applies mappings from the source instance to the destination instance asynchronously.
		/// </summary>
		/// <remarks>
		/// This method applies mappings in three sequential stages:
		/// <list type="number">
		/// <item>First, synchronous mappings are applied.</item>
		/// <item>Second, asynchronous mappings are applied.</item>
		/// <item>Finally, automatic mappings are applied for properties not manually mapped.</item>
		/// </list>
		/// Ensure that both <paramref name="src"/> and <paramref name="dest"/> are properly initialized before calling this method.
		/// </remarks>
		/// <typeparam name="TSource">The type of the source instance, which must implement <see cref="IMorphable{TDestination}"/>.</typeparam>
		/// <param name="src">The source instance from which mappings are applied. Cannot be <see langword="null"/>.</param>
		/// <param name="dest">The destination instance to which mappings are applied. Cannot be <see langword="null"/>.</param>
		/// <param name="sp">The <see cref="IServiceProvider"/> used to resolve dependencies during the mapping process. Cannot be <see
		/// langword="null"/>.</param>
		/// <returns>A task representing the asynchronous mapping operation.</returns>
		internal async Task ApplyFromInstanceAsync<TSource>( TSource src, TDestination dest, IServiceProvider sp ) where TSource : IMorphable<TDestination> {
			var logger = GetLogger( sp );
			logger?.LogDebug( "Starting mapping from {SourceType} to {DestinationType}", typeof( TSource ).Name, typeof( TDestination ).Name );

			// Apply synchronized mappings
			logger?.LogTrace( "Applying {Count} synchronous mappings", _mappings.Count );
			foreach ( var map in _mappings ) {
				try {
					map( dest, sp );
				} catch ( Exception ex ) {
					logger?.LogError( ex, "Error applying synchronous mapping" );
					throw;
				}
			}

			// Apply asynchronous mappings
			logger?.LogTrace( "Applying {Count} asynchronous mappings", _asyncMappings.Count );
			foreach ( var asyncMap in _asyncMappings ) {
				try {
					await asyncMap( dest, sp );
				} catch ( Exception ex ) {
					logger?.LogError( ex, "Error applying asynchronous mapping" );
					throw;
				}
			}

			// Apply auto-mapping for unmapped properties
			logger?.LogTrace( "Starting automatic property mapping" );
			AutoMapFromInstance( src, dest, sp );

			logger?.LogDebug( "Completed mapping from {SourceType} to {DestinationType}", typeof( TSource ).Name, typeof( TDestination ).Name );
		}

		/// <summary>
		/// Automatically maps properties from the source instance to the destination instance.
		/// </summary>
		/// <remarks>
		/// This method performs automatic property mapping by matching property names between source and destination types.
		/// Properties that have been manually mapped or explicitly ignored are skipped. The method attempts to handle
		/// type conversions and nested object mappings through the Morph system.
		/// </remarks>
		/// <typeparam name="TSource">The type of the source instance.</typeparam>
		/// <param name="src">The source instance to map from.</param>
		/// <param name="dest">The destination instance to map to.</param>
		/// <param name="sp">The service provider for dependency resolution.</param>
		private void AutoMapFromInstance<TSource>( TSource src, TDestination dest, IServiceProvider sp ) {
			var logger = GetLogger( sp );

			var srcType = src?.GetType( ) ?? typeof( TSource );
			var destType = dest?.GetType( ) ?? typeof( TDestination );

			logger?.LogTrace( "Starting automatic mapping between {SourceType} and {DestinationType}", srcType.Name, destType.Name );

			var srcMembers = srcType.GetMembers( BindingFlags.Public | BindingFlags.Instance );
			var destMembers = destType.GetMembers( BindingFlags.Public | BindingFlags.Instance );

			var mappedCount = 0;
			var skippedCount = 0;
			var errorCount = 0;

			foreach ( var destMember in destMembers ) {
				// Skip manually mapped or ignored properties
				if ( _manuallyMappedDestProps.Contains( destMember.Name ) || _ignoredProperties.Contains( destMember.Name ) ) {
					skippedCount++;
					continue;
				}

				var srcMember = srcMembers.FirstOrDefault( m => m.Name == destMember.Name );
				if ( srcMember == null ) {
					continue;
				}

				var srcMemberType = GetMemberType( srcMember );
				var destMemberType = GetMemberType( destMember );

				if ( srcMemberType == null || destMemberType == null ) {
					continue;
				}

				// Check if destination member can be written
				if ( !CanWriteMember( destMember ) ) {
					continue;
				}

				object? srcValue = null;
				try {
					srcValue = srcMember switch {
						PropertyInfo p => p.GetValue( src ),
						FieldInfo f => f.GetValue( src ),
						_ => null
					};
				} catch ( Exception ex ) {
					errorCount++;
					logger?.LogWarning( ex, "Error reading value from source member '{MemberName}'", srcMember.Name );
					continue;
				}

				if ( srcValue == null ) {
					// Set default value if possible
					if ( destMemberType.IsValueType && Nullable.GetUnderlyingType( destMemberType ) == null ) {
						SetValue( dest, destMember, Activator.CreateInstance( destMemberType ), logger );
					}
					continue;
				}

				try {
					var mappedValue = MapValue( srcValue, srcMemberType, destMemberType, sp );
					SetValue( dest, destMember, mappedValue, logger );
					mappedCount++;
				} catch ( Exception ex ) {
					errorCount++;
					logger?.LogWarning( ex, "Error mapping '{SourceMember}' -> '{DestMember}'", srcMember.Name, destMember.Name );
				}
			}

			logger?.LogTrace( "Automatic mapping completed. Mapped: {MappedCount}, Skipped: {SkippedCount}, Errors: {ErrorCount}",
				mappedCount, skippedCount, errorCount );
		}

		/// <summary>
		/// Maps a value from the source type to the destination type, handling type conversions and nested mappings.
		/// </summary>
		/// <param name="srcValue">The source value to map.</param>
		/// <param name="srcType">The type of the source value.</param>
		/// <param name="destType">The target destination type.</param>
		/// <param name="sp">The service provider for dependency resolution.</param>
		/// <returns>The mapped value, or null if mapping is not possible.</returns>
		private object? MapValue( object srcValue, Type srcType, Type destType, IServiceProvider sp ) {
			var logger = GetLogger( sp );

			// If types are compatible, return directly
			if ( destType.IsAssignableFrom( srcType ) ) {
				logger?.LogTrace( "Direct assignment from {SourceType} to {DestType}", srcType.Name, destType.Name );
				return srcValue;
			}

			// Try direct conversion
			if ( TryConvertDirect( srcValue, destType, out var converted ) ) {
				logger?.LogTrace( "Direct conversion successful from {SourceType} to {DestType}", srcType.Name, destType.Name );
				return converted;
			}

			// Try mapping using extensions
			try {
				using var scope = sp.CreateScope( );

				var method = typeof( MorphExtensions )
					.GetMethod( nameof( MorphExtensions.To ), [ typeof( object ), typeof( IServiceProvider ) ] )?
					.MakeGenericMethod( destType );

				var result = method?.Invoke( null, [ srcValue, scope.ServiceProvider ] );
				logger?.LogTrace( "Successfully mapped {SourceType} to {DestType} using MorphExtensions", srcType.Name, destType.Name );
				return result;
			} catch ( Exception ex ) {
				logger?.LogDebug( ex, "Failed to map {SourceType} to {DestType} using MorphExtensions", srcType.Name, destType.Name );
			}

			logger?.LogDebug( "Unable to map {SourceType} to {DestType}", srcType.Name, destType.Name );
			return null;
		}

		/// <summary>
		/// Attempts to perform direct type conversion using built-in .NET conversion methods.
		/// </summary>
		/// <param name="value">The value to convert.</param>
		/// <param name="targetType">The target type for conversion.</param>
		/// <param name="result">The converted result, if successful.</param>
		/// <returns>True if conversion was successful, false otherwise.</returns>
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

				// Try direct assignment
				if ( targetType.IsAssignableFrom( value.GetType( ) ) ) {
					result = value;
					return true;
				}

				// Try Convert.ChangeType for primitive and common types
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

		/// <summary>
		/// Determines whether a member (property or field) can be written to.
		/// </summary>
		/// <param name="member">The member to check.</param>
		/// <returns>True if the member can be written to, false otherwise.</returns>
		private bool CanWriteMember( MemberInfo member ) =>
			member switch {
				PropertyInfo p => p.CanWrite,
				FieldInfo f => !f.IsInitOnly && !f.IsLiteral,
				_ => false
			};

		/// <summary>
		/// Gets the type of a member (property or field).
		/// </summary>
		/// <param name="member">The member to get the type for.</param>
		/// <returns>The type of the member, or null if not a property or field.</returns>
		private static Type? GetMemberType( MemberInfo member ) =>
			member switch {
				PropertyInfo p => p.PropertyType,
				FieldInfo f => f.FieldType,
				_ => null
			};

		/// <summary>
		/// Sets the value of a member (property or field) on the target object.
		/// </summary>
		/// <param name="target">The target object to set the value on.</param>
		/// <param name="member">The member to set the value for.</param>
		/// <param name="value">The value to set.</param>
		/// <param name="logger">The logger instance for recording operations.</param>
		private static void SetValue( object target, MemberInfo member, object? value, ILogger? logger ) {
			try {
				switch ( member ) {
					case PropertyInfo p when p.CanWrite:
					p.SetValue( target, value );
					logger?.LogTrace( "Successfully set property '{PropertyName}' to value of type {ValueType}", p.Name, value?.GetType( ).Name ?? "null" );
					break;

					case FieldInfo f when !f.IsInitOnly && !f.IsLiteral:
					f.SetValue( target, value );
					logger?.LogTrace( "Successfully set field '{FieldName}' to value of type {ValueType}", f.Name, value?.GetType( ).Name ?? "null" );
					break;

					default:
					logger?.LogWarning( "Member '{MemberName}' is not assignable", member.Name );
					break;
				}
			} catch ( Exception ex ) {
				logger?.LogError( ex, "Failed to assign value to member '{MemberName}'", member.Name );
				throw;
			}
		}

		/// <summary>
		/// Gets a logger instance from the service provider if available.
		/// </summary>
		/// <param name="sp">The service provider to resolve the logger from.</param>
		/// <returns>An ILogger instance or null if not available.</returns>
		private static ILogger? GetLogger( IServiceProvider sp ) {
			try {
				return sp.GetService<ILogger<Schema<TDestination>>>( );
			} catch {
				return null;
			}
		}
	}
}