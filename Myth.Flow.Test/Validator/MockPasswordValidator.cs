using Myth.Flow.Test.Interfaces;
using Myth.Models;
using System.Linq;

public class MockPasswordValidator : IPasswordValidator {

	public Result<string> ValidateAndHash( string password ) {
		if ( password.Length < 8 ) {
			return Result<string>.Failure( "Password must be at least 8 characters" );
		}
		if ( !password.Any( char.IsDigit ) ) {
			return Result<string>.Failure( "Password must contain at least one digit" );
		}
		if ( !password.Any( char.IsUpper ) ) {
			return Result<string>.Failure( "Password must contain at least one uppercase letter" );
		}

		return Result<string>.Success( $"hashed_{password}" );
	}
}