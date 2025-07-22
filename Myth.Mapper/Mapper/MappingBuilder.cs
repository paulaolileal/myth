using System.Linq.Expressions;
using System.Reflection;

namespace Myth.Mapper {

	public class MappingBuilder<TSource, TDestination> {
		private readonly List<Action<TSource, TDestination, IServiceProvider>> _mappings = [];
		private readonly HashSet<string> _manuallyMappedDestProps = [];
		private readonly HashSet<string> _ignoredProperties = [];

		public MappingBuilder<TSource, TDestination> ForMember<TMember>(
			Expression<Func<TDestination, TMember>> destination,
			Func<TSource, IServiceProvider, TMember> resolver ) {
			var destProp = ( PropertyInfo )( ( MemberExpression )destination.Body ).Member;

			_manuallyMappedDestProps.Add( destProp.Name );
			_mappings.Add( ( src, dest, sp ) => destProp.SetValue( dest, resolver( src, sp ) ) );
			return this;
		}

		internal void Apply( TSource src, TDestination dest, IServiceProvider sp ) {
			foreach ( var map in _mappings )
				map( src, dest, sp );

			AutoMap( src, dest );
		}

		public MappingBuilder<TSource, TDestination> Ignore<TValue>( Expression<Func<TDestination, TValue>> destSelector ) {
			var member = ( destSelector.Body as MemberExpression )?.Member;

			if ( member != null )
				_ignoredProperties.Add( member.Name );

			return this;
		}

		private void AutoMap( TSource src, TDestination dest ) {
			var srcProps = typeof( TSource ).GetProperties( BindingFlags.Public | BindingFlags.Instance );
			var destProps = typeof( TDestination ).GetProperties( BindingFlags.Public | BindingFlags.Instance );

			foreach ( var srcProp in srcProps ) {
				var destProp = destProps.FirstOrDefault( p =>
					p.Name == srcProp.Name &&
					p.PropertyType == srcProp.PropertyType &&
					!_manuallyMappedDestProps.Contains( p.Name ) &&
					!_ignoredProperties.Contains( p.Name )
				);

				if ( destProp != null && destProp.CanWrite ) {
					var value = srcProp.GetValue( src );
					destProp.SetValue( dest, value );
				}
			}
		}
	}
}