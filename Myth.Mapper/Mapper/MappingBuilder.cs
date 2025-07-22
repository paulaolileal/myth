using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Myth.Mapper {
	public class MappingBuilder<TSource, TDestination> {
		private readonly List<Action<TSource, TDestination, IServiceProvider>> _mappings = new( );
		private readonly HashSet<string> _manuallyMappedDestProps = new( );
		private bool _autoMapRemaining = false;

		public MappingBuilder<TSource, TDestination> ForMember<TMember>(
			Expression<Func<TSource, TMember>> source,
			Expression<Func<TDestination, TMember>> destination,
			Func<TSource, IServiceProvider, TMember> resolver ) {
			var destProp = ( PropertyInfo )( ( MemberExpression )destination.Body ).Member;

			_manuallyMappedDestProps.Add( destProp.Name ); 
			_mappings.Add( ( src, dest, sp ) => destProp.SetValue( dest, resolver( src, sp ) ) );
			return this;
		}

		public MappingBuilder<TSource, TDestination> AutoMapRemaining( ) {
			_autoMapRemaining = true;
			return this;
		}

		internal void Apply( TSource src, TDestination dest, IServiceProvider sp ) {
			foreach ( var map in _mappings )
				map( src, dest, sp );

			if ( _autoMapRemaining )
				AutoMap( src, dest );
		}

		private void AutoMap( TSource src, TDestination dest ) {
			var srcProps = typeof( TSource ).GetProperties( BindingFlags.Public | BindingFlags.Instance );
			var destProps = typeof( TDestination ).GetProperties( BindingFlags.Public | BindingFlags.Instance );

			foreach ( var srcProp in srcProps ) {
				var destProp = destProps.FirstOrDefault( p =>
					p.Name == srcProp.Name &&
					p.PropertyType == srcProp.PropertyType &&
					!_manuallyMappedDestProps.Contains( p.Name )
				);

				if ( destProp != null && destProp.CanWrite ) {
					var value = srcProp.GetValue( src );
					destProp.SetValue( dest, value );
				}
			}
		}
	}

}
