using System;
using System.Threading.Tasks;
using Myth.Flow.Test.Contexts;
using Myth.Flow.Test.Interfaces;
using Myth.Flow.Test.Models;
using Myth.Models;

public class UserCreationService(
	IUserRepository repository,
	IPasswordValidator passwordValidator,
	IUnitOfWork unitOfWork ) {
	private readonly IUserRepository _repository = repository;
	private readonly IPasswordValidator _passwordValidator = passwordValidator;
	private readonly IUnitOfWork _unitOfWork = unitOfWork;

	public async Task<Result<CreateUserContext>> CreateUserAsync( CreateUserContext context ) {
		try {
			await _unitOfWork.BeginTransactionAsync( );

			var passwordHashResult = _passwordValidator.ValidateAndHash( context.Request.Password );

			var user = new User {
				Id = Guid.NewGuid( ),
				Email = context.Request.Email,
				PasswordHash = passwordHashResult.Value!,
				Role = context.Request.Role,
				CreatedAt = DateTime.UtcNow
			};

			context.CreatedUser = await _repository.CreateUserAsync( user );

			await _unitOfWork.CommitAsync( );

			return Result<CreateUserContext>.Success( context );
		} catch ( Exception ex ) {
			await _unitOfWork.RollbackAsync( );
			return Result<CreateUserContext>.Failure( "Failed to create user", ex );
		}
	}
}
