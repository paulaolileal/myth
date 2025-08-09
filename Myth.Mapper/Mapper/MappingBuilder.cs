using Myth.Mapper;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;

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
		foreach ( var map in _mappings )
			map( src, dest, sp );

		foreach ( var asyncMap in _asyncMappings )
			await asyncMap( src, dest, sp );

		AutoMap( src, dest, sp );
	}

	public MappingBuilder<TSource, TDestination> Ignore<TValue>( Expression<Func<TDestination, TValue>> destSelector ) {
		if ( destSelector.Body is MemberExpression member )
			_ignoredProperties.Add( member.Member.Name );
		return this;
	}

	private void AutoMap( TSource src, TDestination dest, IServiceProvider sp ) {
		var srcMembers = typeof( TSource ).GetMembers( BindingFlags.Public | BindingFlags.Instance );
		var destMembers = typeof( TDestination ).GetMembers( BindingFlags.Public | BindingFlags.Instance );

		foreach ( var destMember in destMembers ) {
			if ( _manuallyMappedDestProps.Contains( destMember.Name ) || _ignoredProperties.Contains( destMember.Name ) )
				continue;

			var srcMember = srcMembers.FirstOrDefault( m => m.Name == destMember.Name );
			if ( srcMember == null )
				continue;

			var srcType = GetMemberType( srcMember );
			var destType = GetMemberType( destMember );

			if ( srcType == null || destType == null )
				continue;

			object? srcValue = null;
			try {
				srcValue = srcMember switch {
					PropertyInfo p => p.GetValue( src ),
					FieldInfo f => f.GetValue( src ),
					_ => null
				};
			} catch ( Exception ex ) {
				Debug.WriteLine( $"[Map] Erro ao ler '{srcMember.Name}': {ex.Message}" );
				continue;
			}

			if ( srcValue == null )
				continue;

			try {
				var registry = ( MapRegistry )sp.GetService( typeof( MapRegistry ) );

				if ( registry != null && registry.HasMapping( srcType, destType ) ) {
					var method = typeof( MapRegistry ).GetMethod( "Map" )?.MakeGenericMethod( srcType, destType );
					var mappedValue = method?.Invoke( null, [ srcValue, sp ] );
					SetValue( dest, destMember, mappedValue );
				} else if ( destType == srcType ) {
					SetValue( dest, destMember, srcValue );
				} else {
					Debug.WriteLine( $"[Map] Ignorado: tipos incompatíveis ou sem mapeamento explícito '{srcMember.Name}' ({srcType.Name} -> {destType.Name})" );
				}
			} catch ( Exception ex ) {
				Debug.WriteLine( $"[Map] Erro ao mapear '{srcMember.Name}' -> '{destMember.Name}': {ex.Message}" );
			}
		}
	}

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

				case FieldInfo f:
				f.SetValue( target, value );
				break;

				default:
				Debug.WriteLine( $"[Map] Membro '{member.Name}' não é atribuível." );
				break;
			}
		} catch ( Exception ex ) {
			Debug.WriteLine( $"[Map] Falha ao atribuir '{member.Name}': {ex.Message}" );
		}
	}
}