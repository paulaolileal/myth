using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Myth.Exceptions {
	public class JsonParsingException : Exception {
		public JsonParsingException( string? message, Exception? innerException ) : base( message, innerException ) {
		}
	}
}
