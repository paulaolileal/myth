using System.Net;

namespace Myth.Models {

	/// <summary>
	/// Represents the result of a validation
	/// </summary>
	public sealed class ValidationResult {
		private readonly List<ValidationError> _errors = new( );

		public IReadOnlyList<ValidationError> Errors => _errors;
		public bool IsValid => _errors.Count == 0;

		public HttpStatusCode StatusCode => Errors.Any( )
			? Errors.Max( e => e.StatusCode )
			: HttpStatusCode.OK;

		internal void AddError( ValidationError error ) {
			_errors.Add( error );
		}

		internal void AddErrors( IEnumerable<ValidationError> errors ) {
			_errors.AddRange( errors );
		}
	}
}