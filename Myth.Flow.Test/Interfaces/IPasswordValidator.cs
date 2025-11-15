using Myth.Models;

namespace Myth.Flow.Test.Interfaces;

public interface IPasswordValidator {

	Result<string> ValidateAndHash( string password );
}
