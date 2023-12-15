using System.Net;

namespace Myth.Models.Rest {

    internal class ExceptionMapItem : BaseMapItem {
        public bool ThrowException { get; set; }

        public ExceptionMapItem( HttpStatusCode statusCode, Func<string, bool>? condition ) : base( statusCode, condition ) {
            Condition = condition;
            ThrowException = true;
        }
    }
}