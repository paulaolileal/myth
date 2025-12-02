using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Exceptions;
using Myth.Interfaces;

namespace Myth.Morph;

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

	// For IMorphableFrom pattern - reverse bindings where TDestination is the source type
	private readonly List<Action<object, object, IServiceProvider>> _reverseMappings = [ ];

	private readonly List<Func<object, object, IServiceProvider, Task>> _asyncReverseMappings = [ ];

	// Executors for different mapping patterns
	private ToMappingExecutor<TDestination>? _toExecutor;

	private FromMappingExecutor<TDestination>? _fromExecutor;

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
	/// Configures a binding from a source property to a destination value for IMorphableFrom pattern.
	/// This method is used when the destination object defines how it should be created from the source.
	/// </summary>
	/// <typeparam name="TValue">The type of the value being mapped.</typeparam>
	/// <param name="sourceSelector">An expression that specifies the property of the source type to read from.</param>
	/// <param name="destinationPropertyGetter">A function that references the destination property (used to capture the property name).</param>
	/// <returns>The current <see cref="Schema{TDestination}"/> instance, allowing for method chaining.</returns>

	/// <summary>
	/// Configures a reverse binding for IMorphableFrom pattern where destination property is set from source expression.
	/// This method is used when the destination object defines how it should be created from the source.
	/// The syntax is: schema.Bind(() => destinationProperty, src => sourceExpression)
	/// </summary>
	/// <typeparam name="TValue">The type of the value being mapped.</typeparam>
	/// <param name="destinationPropertyGetter">An expression that references the destination property to be set.</param>
	/// <param name="sourceExpression">An expression that calculates a value from the source object.</param>
	/// <returns>The current <see cref="Schema{TDestination}"/> instance, allowing for method chaining.</returns>
	public Schema<TDestination> Bind<TValue>( Expression<Func<TValue>> destinationPropertyGetter, Expression<Func<TDestination, TValue>> sourceExpression ) {
		var compiledSourceExpression = sourceExpression.Compile( );

		// Extract the destination property name from the destinationPropertyGetter expression
		string destinationPropertyName = null;
		if ( destinationPropertyGetter.Body is MemberExpression memberExpr ) {
			destinationPropertyName = memberExpr.Member.Name;
		}

		if ( destinationPropertyName == null ) {
			throw new BindException( "Invalid destination property expression. Use () => PropertyName syntax." );
		}

		// Mark this property as manually mapped to prevent auto-mapping from overriding
		_manuallyMappedDestProps.Add( destinationPropertyName );

		_reverseMappings.Add( ( source, destination, sp ) => {
			var logger = GetLogger( sp );
			try {
				// Calculate the value from the source using the expression
				var calculatedValue = compiledSourceExpression( ( TDestination )source );

				// Set the value on the destination object
				var destType = destination.GetType( );
				var property = destType.GetProperty( destinationPropertyName );
				if ( property != null && property.CanWrite ) {
					property.SetValue( destination, calculatedValue );
					logger?.LogTrace( "Set property {PropertyName} to value {Value}", destinationPropertyName, calculatedValue );
				} else {
					logger?.LogWarning( "Property {PropertyName} not found or not writable on type {DestinationType}", destinationPropertyName, destType.Name );
				}
			} catch ( Exception ex ) {
				logger?.LogError( ex, "Error applying reverse binding for property {PropertyName}", destinationPropertyName );
				throw;
			}
		} );

		return this;
	}

	/// <summary>
	/// Configures a binding for the IMorphableFrom pattern with service provider access.
	/// This method is used when the destination object needs to access services during mapping.
	/// The syntax is: schema.Bind(() => destinationProperty, (src, sp) => sp.GetService<IService>().Method(src.Property))
	/// </summary>
	/// <typeparam name="TValue">The type of the value being mapped.</typeparam>
	/// <param name="destinationPropertyGetter">An expression that references the destination property to be set.</param>
	/// <param name="sourceExpression">A function that calculates a value from the source object with service provider access.</param>
	/// <returns>The current <see cref="Schema{TDestination}"/> instance, allowing for method chaining.</returns>
	public Schema<TDestination> Bind<TValue>( Expression<Func<TValue>> destinationPropertyGetter, Func<TDestination, IServiceProvider, TValue> sourceExpression ) {
		// Extract the destination property name from the destinationPropertyGetter expression
		string destinationPropertyName = null;
		if ( destinationPropertyGetter.Body is MemberExpression memberExpr ) {
			destinationPropertyName = memberExpr.Member.Name;
		}

		if ( destinationPropertyName == null ) {
			throw new BindException( "Invalid destination property expression. Use () => PropertyName syntax." );
		}

		// Mark this property as manually mapped to prevent auto-mapping from overriding
		_manuallyMappedDestProps.Add( destinationPropertyName );

		_reverseMappings.Add( ( source, destination, sp ) => {
			var logger = GetLogger( sp );
			try {
				// Calculate the value from the source using the expression with service provider
				var calculatedValue = sourceExpression( ( TDestination )source, sp );

				// Set the value on the destination object
				var destType = destination.GetType( );
				var property = destType.GetProperty( destinationPropertyName );
				if ( property != null && property.CanWrite ) {
					property.SetValue( destination, calculatedValue );
					logger?.LogTrace( "Set property {PropertyName} to value {Value} using service provider", destinationPropertyName, calculatedValue );
				} else {
					logger?.LogWarning( "Property {PropertyName} not found or not writable on type {DestinationType}", destinationPropertyName, destType.Name );
				}
			} catch ( Exception ex ) {
				logger?.LogError( ex, "Error applying service provider binding for property {PropertyName}", destinationPropertyName );
				throw;
			}
		} );

		return this;
	}

	/// <summary>
	/// Configures an asynchronous binding for the IMorphableFrom pattern using MythServiceProvider access.
	/// This method is used when the destination object defines how it should be created from the source asynchronously.
	/// The syntax is: schema.BindAsync(() => destinationProperty, async src => await CalculateValueAsync(src))
	/// </summary>
	/// <typeparam name="TValue">The type of the value being mapped.</typeparam>
	/// <param name="destinationPropertyGetter">An expression that references the destination property to be set.</param>
	/// <param name="sourceExpressionAsync">An async function that calculates a value from the source object using MythServiceProvider for services.</param>
	/// <returns>The current <see cref="Schema{TDestination}"/> instance, allowing for method chaining.</returns>
	public Schema<TDestination> BindAsync<TValue>( Expression<Func<TValue>> destinationPropertyGetter, Func<TDestination, Task<TValue>> sourceExpressionAsync ) {
		// Extract the destination property name from the destinationPropertyGetter expression
		string destinationPropertyName = null;
		if ( destinationPropertyGetter.Body is MemberExpression memberExpr ) {
			destinationPropertyName = memberExpr.Member.Name;
		}

		if ( destinationPropertyName == null ) {
			throw new BindException( "Invalid destination property expression. Use () => PropertyName syntax." );
		}

		// Mark this property as manually mapped to prevent auto-mapping from overriding
		_manuallyMappedDestProps.Add( destinationPropertyName );

		_asyncReverseMappings.Add( async ( source, destination, sp ) => {
			var logger = GetLogger( sp );
			try {
				// Calculate the value from the source using the async expression (no service provider parameter)
				var calculatedValue = await sourceExpressionAsync( ( TDestination )source );

				// Set the value on the destination object
				var destType = destination.GetType( );
				var property = destType.GetProperty( destinationPropertyName );
				if ( property != null && property.CanWrite ) {
					property.SetValue( destination, calculatedValue );
					logger?.LogTrace( "Set property {PropertyName} to async value {Value}", destinationPropertyName, calculatedValue );
				} else {
					logger?.LogWarning( "Property {PropertyName} not found or not writable on type {DestinationType}", destinationPropertyName, destType.Name );
				}
			} catch ( Exception ex ) {
				logger?.LogError( ex, "Error applying async reverse binding for property {PropertyName}", destinationPropertyName );
				throw;
			}
		} );

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
	internal async Task ApplyFromInstanceAsync<TSource>( TSource src, TDestination dest, IServiceProvider sp ) where TSource : IMorphableTo<TDestination> {
		var logger = GetLogger( sp );
		logger?.LogDebug( "Starting IMorphableTo mapping from {SourceType} to {DestinationType}", typeof( TSource ).Name, typeof( TDestination ).Name );

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

		// Apply auto-mapping using executor
		logger?.LogTrace( "Starting automatic property mapping using ToMappingExecutor" );
		GetToExecutor( sp ).ApplyMapping( src, dest, sp, _manuallyMappedDestProps, _ignoredProperties );

		logger?.LogDebug( "Completed IMorphableTo mapping from {SourceType} to {DestinationType}", typeof( TSource ).Name, typeof( TDestination ).Name );
	}

	/// <summary>
	/// Applies mappings to create a destination instance from a source, where the destination defines how to be created from the source.
	/// This method is used for IMorphableFrom pattern where the destination knows how to transform itself from the source.
	/// </summary>
	/// <typeparam name="TDestination">The destination type that implements IMorphableFrom.</typeparam>
	/// <param name="src">The source instance from which to create the destination. Cannot be <see langword="null"/>.</param>
	/// <param name="dest">The destination instance to which mappings are applied. Cannot be <see langword="null"/>.</param>
	/// <param name="sp">The <see cref="IServiceProvider"/> used to resolve dependencies during the mapping process. Cannot be <see
	/// langword="null"/>.</param>
	/// <returns>A task representing the asynchronous mapping operation.</returns>
	internal async Task ApplyToInstanceAsync<TSource>( TSource src, TDestination dest, IServiceProvider sp ) {
		var logger = GetLogger( sp );
		logger?.LogDebug( "Starting mapping from {SourceType} to {DestinationType} using IMorphableFrom", typeof( TSource ).Name, typeof( TDestination ).Name );

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

		// Apply auto-mapping using executor
		logger?.LogTrace( "Starting automatic property mapping using FromMappingExecutor" );
		GetFromExecutor( sp ).ApplyMapping( src, dest, sp, _manuallyMappedDestProps, _ignoredProperties );

		logger?.LogDebug( "Completed mapping from {SourceType} to {DestinationType} using IMorphableFrom", typeof( TSource ).Name, typeof( TDestination ).Name );
	}

	/// <summary>
	/// Gets or creates the ToMappingExecutor instance for this schema.
	/// </summary>
	/// <param name="sp">The service provider for logger resolution.</param>
	/// <returns>The ToMappingExecutor instance.</returns>
	private ToMappingExecutor<TDestination> GetToExecutor( IServiceProvider sp ) {
		if ( _toExecutor == null ) {
			var logger = GetLogger( sp );
			var typeResolverLogger = logger != null ? sp.GetService<ILogger<TypeResolver>>( ) : null;
			var typeResolver = new TypeResolver( typeResolverLogger );
			_toExecutor = new ToMappingExecutor<TDestination>( logger, typeResolver );
		}
		return _toExecutor;
	}

	/// <summary>
	/// Gets or creates the FromMappingExecutor instance for this schema.
	/// </summary>
	/// <param name="sp">The service provider for logger resolution.</param>
	/// <returns>The FromMappingExecutor instance.</returns>
	private FromMappingExecutor<TDestination> GetFromExecutor( IServiceProvider sp ) {
		if ( _fromExecutor == null ) {
			var logger = GetLogger( sp );
			var typeResolverLogger = logger != null ? sp.GetService<ILogger<TypeResolver>>( ) : null;
			var typeResolver = new TypeResolver( typeResolverLogger );
			_fromExecutor = new FromMappingExecutor<TDestination>( logger, typeResolver );
		}
		return _fromExecutor;
	}

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
					logger?.LogTrace( "Successfully set property {PropertyName} to value of type {ValueType}",
						p.Name, value?.GetType( ).Name ?? "null" );
					break;

				case FieldInfo f when !f.IsInitOnly && !f.IsLiteral:
					f.SetValue( target, value );
					logger?.LogTrace( "Successfully set field {FieldName} to value of type {ValueType}",
						f.Name, value?.GetType( ).Name ?? "null" );
					break;

				default:
					logger?.LogWarning( "Member {MemberName} is not assignable", member.Name );
					break;
			}
		} catch ( Exception ex ) {
			logger?.LogError( ex, "Failed to assign value to member {MemberName}", member.Name );
			throw;
		}
	}

	/// <summary>
	/// Applies configured bindings from source to destination for IMorphableFrom pattern.
	/// This method processes reverse mappings where expressions calculate values from the source
	/// and assign them to the destination object.
	/// </summary>
	/// <param name="source">The source object to read values from.</param>
	/// <param name="destination">The destination object to write values to.</param>
	/// <param name="serviceProvider">The service provider for dependency resolution.</param>
	public void ApplyFromSourceToDestination( object source, object destination, IServiceProvider serviceProvider ) {
		var logger = GetLogger( serviceProvider );
		logger?.LogDebug( "Applying reverse mappings from {SourceType} to {DestinationType}", source.GetType( ).Name, destination.GetType( ).Name );

		// Apply all configured reverse mappings
		foreach ( var mapping in _reverseMappings ) {
			try {
				mapping( source, destination, serviceProvider );
			} catch ( Exception ex ) {
				logger?.LogError( ex, "Error applying reverse mapping" );
				throw;
			}
		}

		// Apply auto-mapping using executor
		// Note: For IMorphableFrom pattern, TDestination is actually the SOURCE type (User),
		// and destination parameter is the actual DTO we're mapping TO
		logger?.LogTrace( "Starting automatic property mapping using FromMappingExecutor" );

		// Create a FromMappingExecutor for the actual destination type
		var actualDestinationType = destination.GetType( );
		var logger2 = GetLogger( serviceProvider );
		var typeResolverLogger = logger2 != null ? serviceProvider.GetService<ILogger<TypeResolver>>( ) : null;
		var typeResolver = new TypeResolver( typeResolverLogger );

		// Use reflection to create and invoke the executor for the actual destination type
		var executorType = typeof( FromMappingExecutor<> ).MakeGenericType( actualDestinationType );
		var executor = Activator.CreateInstance( executorType, logger2, typeResolver )!;

		var applyMethod = executorType.GetMethod( nameof( FromMappingExecutor<object>.ApplyMapping ) );
		if ( applyMethod != null ) {
			applyMethod.Invoke( executor, [ source, destination, serviceProvider, _manuallyMappedDestProps, _ignoredProperties ] );
		}

		logger?.LogDebug( "Completed reverse mapping with {MappingCount} mappings", _reverseMappings.Count );
	}

	/// <summary>
	/// Applies reverse mappings from source to destination asynchronously, including async reverse mappings.
	/// This method is used for the IMorphableFrom pattern when async operations are needed.
	/// </summary>
	/// <param name="source">The source object to map from.</param>
	/// <param name="destination">The destination object to map to.</param>
	/// <param name="serviceProvider">The service provider for dependency injection.</param>
	/// <returns>A task representing the asynchronous mapping operation.</returns>
	public async Task ApplyFromSourceToDestinationAsync( object source, object destination, IServiceProvider serviceProvider ) {
		var logger = GetLogger( serviceProvider );
		logger?.LogDebug( "Applying reverse mappings asynchronously from {SourceType} to {DestinationType}", source.GetType( ).Name, destination.GetType( ).Name );

		// Apply synchronous reverse mappings first
		foreach ( var mapping in _reverseMappings ) {
			try {
				mapping( source, destination, serviceProvider );
			} catch ( Exception ex ) {
				logger?.LogError( ex, "Error applying synchronous reverse mapping" );
				throw;
			}
		}

		// Apply asynchronous reverse mappings
		foreach ( var asyncMapping in _asyncReverseMappings ) {
			try {
				await asyncMapping( source, destination, serviceProvider );
			} catch ( Exception ex ) {
				logger?.LogError( ex, "Error applying asynchronous reverse mapping" );
				throw;
			}
		}

		// Apply auto-mapping using executor
		logger?.LogTrace( "Starting automatic property mapping using FromMappingExecutor" );

		// Create a FromMappingExecutor for the actual destination type
		var actualDestinationType = destination.GetType( );
		var logger2 = GetLogger( serviceProvider );
		var typeResolverLogger = logger2 != null ? serviceProvider.GetService<ILogger<TypeResolver>>( ) : null;
		var typeResolver = new TypeResolver( typeResolverLogger );

		// Use reflection to create and invoke the executor for the actual destination type
		var executorType = typeof( FromMappingExecutor<> ).MakeGenericType( actualDestinationType );
		var executor = Activator.CreateInstance( executorType, logger2, typeResolver )!;

		var applyMethod = executorType.GetMethod( nameof( FromMappingExecutor<object>.ApplyMapping ) );
		if ( applyMethod != null ) {
			applyMethod.Invoke( executor, [ source, destination, serviceProvider, _manuallyMappedDestProps, _ignoredProperties ] );
		}

		logger?.LogDebug( "Completed async reverse mapping with {SyncMappingCount} sync and {AsyncMappingCount} async mappings", _reverseMappings.Count, _asyncReverseMappings.Count );
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
