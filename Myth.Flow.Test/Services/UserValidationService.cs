using Myth.Flow.Test.Contexts;
using Myth.Flow.Test.Interfaces;
using Myth.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

public class UserValidationService {
	private readonly IUserRepository _repository;
	private readonly IPasswordValidator _passwordValidator;
	private readonly IRoleValidator _roleValidator;

	public UserValidationService(
		IUserRepository repository,
		IPasswordValidator passwordValidator,
		IRoleValidator roleValidator ) {
		_repository = repository;
		_passwordValidator = passwordValidator;
		_roleValidator = roleValidator;
	}

	public async Task<Result<CreateUserContext>> ValidateAsync( CreateUserContext context ) {
		// Validate email uniqueness
		if ( await _repository.EmailExistsAsync( context.Request.Email ) ) {
			return Result<CreateUserContext>.Failure(
				$"Email '{context.Request.Email}' is already in use" );
		}

		// Validate email format
		if ( !new EmailAddressAttribute( ).IsValid( context.Request.Email ) ) {
			return Result<CreateUserContext>.Failure( "Invalid email format" );
		}

		// Validate password
		var passwordResult = _passwordValidator.ValidateAndHash( context.Request.Password );
		if ( passwordResult.IsFailure ) {
			return Result<CreateUserContext>.Failure( passwordResult.ErrorMessage! );
		}

		// Validate role
		if ( !_roleValidator.IsValidRole( context.Request.Role ) ) {
			return Result<CreateUserContext>.Failure(
				$"Invalid role '{context.Request.Role}'. Must be: admin, common, or maintainer" );
		}

		return Result<CreateUserContext>.Success( context );
	}
}