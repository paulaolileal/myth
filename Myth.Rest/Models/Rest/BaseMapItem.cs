using System.Net;

namespace Myth.Models.Rest {

    internal abstract class BaseMapItem {
        public HttpStatusCode StatusCode { get; set; }
        public Func<string, bool>? Condition { get; set; }

        protected BaseMapItem( HttpStatusCode statusCode, Func<string, bool>? condition ) {
            StatusCode = statusCode;
            Condition = condition;
        }

        public bool TestCondition( string content ) {
            if ( Condition is null )
                return true;

            return Condition.Invoke( content );
        }
    }
}