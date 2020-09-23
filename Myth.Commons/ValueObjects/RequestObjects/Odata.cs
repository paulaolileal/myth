using Myth.ValueObjects.QueryObjects;
using System.Linq;

namespace Myth.ValueObjects.RequestObjects {

    public class Odata<TViewModel> {
        public Filter<TViewModel> Filter { get; private set; }
        public Order<TViewModel> Order { get; private set; }
        public Pagination Pagination { get; private set; }

        public Odata( ) {
            Filter = new Filter<TViewModel>( );
            Order = new Order<TViewModel>( );
            Pagination = new Pagination( );
        }

        public void SetFilter( Filter<TViewModel> filter ) =>
            Filter = filter;

        public void SetOrder( Order<TViewModel> order ) =>
            Order = order;

        public void SetPagination( Pagination pagination ) =>
            Pagination = pagination;

        public string Build( string route = "" ) {
            var query = route;

            if ( !query.Contains( "?" ) )
                query += "?";

            if ( Filter != null )
                query += ShouldAddAnd( query ) + Filter.Build( );

            if ( Order != null )
                query += ShouldAddAnd( query ) + Order.Build( );

            if ( Pagination == null )
                Pagination = Pagination.Default;
            query += ShouldAddAnd( query ) + Pagination.Build( );

            return query;
        }

        public string ShouldAddAnd( string query ) {
            if ( query.LastOrDefault( ) != '&' && query.LastOrDefault( ) != '?' )
                return "&";
            return string.Empty;
        }
    }
}