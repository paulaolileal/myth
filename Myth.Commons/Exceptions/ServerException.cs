using System;

namespace Myth.Exceptions {

    public class ServerException : Exception {

        public string InnerMessage { get; set; }

        public Exception Stack { get; set; }

        public ServerException( string message, string stackTrace ) : base( message, new Exception( stackTrace ) ) {
            Stack = new Exception( stackTrace );
        }
    }
}