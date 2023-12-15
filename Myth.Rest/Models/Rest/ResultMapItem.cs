using System.Net;

namespace Myth.Models.Rest {

    internal class ResultMapItem : BaseMapItem {
        public Type? Type { get; set; }

        public bool ThrowException { get; set; }

        public ResultMapItem( HttpStatusCode statusCode, Func<string, bool>? condition, Type type ) : base( statusCode, condition ) {
            Type = type;
            Condition = condition;
            ThrowException = false;
        }
    }
}