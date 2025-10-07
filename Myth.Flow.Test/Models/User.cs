using System;

namespace Myth.Flow.Test.Models {

	public class User {
		public Guid Id { get; set; }
		public string Email { get; set; } = string.Empty;
		public string PasswordHash { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public DateTime CreatedAt { get; set; }
	}
}