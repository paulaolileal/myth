using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Myth.ValueObjects.RequestObjects {

    public class Order<TViewModel> {
        public List<string> Orders { get; private set; }

        public Order( ) {
            Orders = new List<string>( );
        }

        public Order( string conditions ) : this( ) {
            Orders = conditions.Split( '&', '?' ).ToList( );
        }

        public void Add<TMember>( Expression<Func<TViewModel, TMember>> destinationMember, bool desc = false ) {
            var memberExpression = ( MemberExpression ) destinationMember.Body;
            var property = memberExpression.Member.Name;

            var @operator = desc ? " desc" : "";

            Orders.Add( $"Order={property}{@operator}" );
        }

        public string Build( ) =>
            string.Join( "&", Orders.ToArray( ) );
    }
}