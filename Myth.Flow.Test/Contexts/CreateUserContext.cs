using Myth.Flow.Test.Models;

namespace Myth.Flow.Test.Contexts {

	public class CreateUserContext {
		public CreateUserRequest Request { get; set; } = null!;
		public User? CreatedUser { get; set; }
		public string? ValidationError { get; set; }
	}
}