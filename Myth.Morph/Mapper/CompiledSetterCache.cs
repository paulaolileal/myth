using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace Myth.Morph;

/// <summary>
/// Cache of compiled getter and setter delegates built via <see cref="Expression.Lambda{TDelegate}"/>.
/// Replaces <c>PropertyInfo.GetValue</c> / <c>SetValue</c> reflection in hot mapping paths.
/// Each delegate is compiled once per member and reused across all mapping calls.
/// </summary>
internal static class CompiledSetterCache {
	private static readonly ConcurrentDictionary<MemberInfo, Action<object, object?>> _setters = new( );
	private static readonly ConcurrentDictionary<MemberInfo, Func<object, object?>> _getters = new( );

	/// <summary>
	/// Returns a compiled setter for the given property or field.
	/// Compiled and cached on first access per member.
	/// </summary>
	internal static Action<object, object?> GetSetter( MemberInfo member )
		=> _setters.GetOrAdd( member, BuildSetter );

	/// <summary>
	/// Returns a compiled getter for the given property or field.
	/// Compiled and cached on first access per member.
	/// </summary>
	internal static Func<object, object?> GetGetter( MemberInfo member )
		=> _getters.GetOrAdd( member, BuildGetter );

	private static Action<object, object?> BuildSetter( MemberInfo member ) {
		var targetParam = Expression.Parameter( typeof( object ), "target" );
		var valueParam = Expression.Parameter( typeof( object ), "value" );

		var (memberAccess, memberType) = ResolveMemberAccess( member, targetParam );
		var convertedValue = Expression.Convert( valueParam, memberType );
		var assignment = Expression.Assign( memberAccess, convertedValue );

		return Expression.Lambda<Action<object, object?>>( assignment, targetParam, valueParam ).Compile( );
	}

	private static Func<object, object?> BuildGetter( MemberInfo member ) {
		var sourceParam = Expression.Parameter( typeof( object ), "source" );

		var (memberAccess, _) = ResolveMemberAccess( member, sourceParam );
		var boxed = Expression.Convert( memberAccess, typeof( object ) );

		return Expression.Lambda<Func<object, object?>>( boxed, sourceParam ).Compile( );
	}

	private static (MemberExpression Access, Type MemberType) ResolveMemberAccess( MemberInfo member, ParameterExpression instanceParam ) {
		var declaringType = member.DeclaringType
			?? throw new InvalidOperationException( $"Member '{member.Name}' has no declaring type." );

		var castInstance = Expression.Convert( instanceParam, declaringType );

		return member switch {
			PropertyInfo prop => (Expression.Property( castInstance, prop ), prop.PropertyType),
			FieldInfo field => (Expression.Field( castInstance, field ), field.FieldType),
			_ => throw new NotSupportedException( $"Member kind '{member.MemberType}' is not supported for compiled accessor." )
		};
	}

	/// <summary>Clears all cached delegates. For testing purposes only.</summary>
	internal static void Clear( ) {
		_setters.Clear( );
		_getters.Clear( );
	}
}
