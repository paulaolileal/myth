using System.Net;

namespace Myth.Models {

	/// <summary>
	/// Represents a validation error
	/// </summary>
	public sealed class ValidationError {
		public string Field { get; init; } = string.Empty;
		public string Message { get; init; } = string.Empty;
		public string Code { get; init; } = "VIOLATION";
		public HttpStatusCode StatusCode { get; init; } = HttpStatusCode.BadRequest;

		public ValidationError( ) {
		}

		public ValidationError( string field, string message, string code, HttpStatusCode statusCode ) {
			Field = field;
			Message = message;
			Code = code;
			StatusCode = statusCode;
		}

		public override string ToString( ) => $"Error on field `{Field}` with `{Code}: {Message}";
	}
}