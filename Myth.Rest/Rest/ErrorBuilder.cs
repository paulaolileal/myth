using Myth.Extensions;
using Myth.Models.Rest;
using System.Net;

namespace Myth.Rest;

public class ErrorBuilder {
	private readonly ExceptionMappingList _exceptionMapping;
	public bool _throwForNonMappedResult;

	public ErrorBuilder( ) {
		_exceptionMapping = new( );
		_throwForNonMappedResult = true;
	}

	/// <summary>
	/// Clear all maping exceptions
	/// </summary>
	public void Clear( ) => _exceptionMapping.Clear( );

	internal bool TryGet( HttpStatusCode statusCode, dynamic content ) =>
		_exceptionMapping.TryGet( statusCode, content );

	/// <summary>
	/// Throws a exception for all status that is not a success
	/// </summary>
	/// <param name="condition">A condition to check before the throw</param>
	/// <returns>This object</returns>
	public ErrorBuilder ThrowForNonSuccess( Func<dynamic, bool>? condition = null ) {
		foreach ( var statusCode in Enum.GetValues<HttpStatusCode>( ) )
			if ( !statusCode.IsSuccess( ) )
				ThrowFor( statusCode, condition );

		return this;
	}

	/// <summary>
	/// Throws a exception for a specific status code
	/// </summary>
	/// <param name="statusCode">The status code</param>
	/// <param name="condition">A condition to check before the throw</param>
	/// <returns>This object</returns>
	public ErrorBuilder ThrowFor( HttpStatusCode statusCode, Func<dynamic, bool>? condition = null ) {
		_exceptionMapping.Add( statusCode, condition );
		return this;
	}

	/// <summary>
	/// Throws a exception for a spec ific status code
	/// </summary>
	/// <param name="statusCodes"></param>
	/// <param name="condition">A condition to check before the throw</param>
	/// <returns>This object</returns>
	public ErrorBuilder ThrowFor( List<HttpStatusCode> statusCodes, Func<dynamic, bool>? condition = null ) {
		foreach ( var statusCode in statusCodes )
			ThrowFor( statusCode, condition );

		return this;
	}

	/// <summary>
	/// Throws a exception for all status code
	/// </summary>
	/// <param name="condition">A condition to check before the throw</param>
	/// <returns>This object</returns>
	public ErrorBuilder ThrowForAll( Func<dynamic, bool>? condition = null ) {
		foreach ( var statusCode in Enum.GetValues<HttpStatusCode>( ) )
			_exceptionMapping.Add( statusCode, condition );

		return this;
	}

	/// <summary>
	/// Defines that not throw a exception if a status code is not mapped
	/// </summary>
	/// <returns>This object</returns>
	public ErrorBuilder NotThrowForNonMappedResult( ) {
		_throwForNonMappedResult = false;

		return this;
	}

	/// <summary>
	/// Not throws a exception for specific status code
	/// </summary>
	/// <param name="statusCode"></param>
	/// <returns>This object</returns>
	public ErrorBuilder NotThrowFor( HttpStatusCode statusCode ) {
		_exceptionMapping.Remove( statusCode );
		return this;
	}
}