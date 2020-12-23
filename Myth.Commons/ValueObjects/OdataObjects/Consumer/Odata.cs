using System.Linq;

namespace Myth.ValueObjects.OdataObjects.Consumer {

    public class Odata<TViewModel> {
        public Filter<TViewModel> Filter { get; private set; } = new Filter<TViewModel>( );

        public Order<TViewModel> Order { get; private set; } = new Order<TViewModel>( );

        public Pagination Pagination { get; private set; } = new Pagination( );

        public Odata( Filter<TViewModel> filter = null, Order<TViewModel> order = null, Pagination pagination = null ) {
            if ( filter != null )
                Filter = filter;

            if ( order != null )
                Order = order;

            if ( pagination != null )
                Pagination = pagination;
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