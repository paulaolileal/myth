using Myth.Exceptions;
using System.Net;

namespace Myth.Models.Rest {

    internal class MappingResultList : List<BaseMapItem> {

        private void AddItem( BaseMapItem item ) {
            if ( this.Any( x =>
                x.StatusCode == item.StatusCode &&
                item.GetType( ) != x.GetType( ) ) )
                throw new StatusMapException( );

            base.Add( item );
        }

        public void AddResultMap( HttpStatusCode statusCode, Func<string, bool>? condition, Type type ) {
            var result = new ResultMapItem( statusCode, condition, type );

            AddItem( result );
        }

        public void AddExceptionMap( HttpStatusCode statusCode, Func<string, bool>? condition ) {
            var result = new ExceptionMapItem( statusCode, condition );

            AddItem( result );
        }

        public bool GetResultMap( HttpStatusCode statusCode, string content, out Type? type ) {
            var item = this.FirstOrDefault( x =>
                x.StatusCode == statusCode &&
                x.GetType( ) == typeof( ResultMapItem ) )
                as ResultMapItem;

            type = item?.Type;

            return item is not null && item.TestCondition( content );
        }

        public bool GetExceptiontMap( HttpStatusCode statusCode, string content ) {
            var item = this.FirstOrDefault( x =>
                x.StatusCode == statusCode &&
                x.GetType( ) == typeof( ExceptionMapItem ) )
                as ExceptionMapItem;

            return item is not null && item.TestCondition( content );
        }
    }
}