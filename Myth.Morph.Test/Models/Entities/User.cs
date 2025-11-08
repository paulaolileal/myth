namespace Myth.Morph.Test.Models.Entities {

	public class User {
		public int Id { get; set; }
		public string FirstName { get; set; } = "";
		public string LastName { get; set; } = "";
		public string Email { get; set; } = "";
		public DateTime BirthDate { get; set; }
		public string CountryCode { get; set; } = "";
		public bool IsEmailVerified { get; set; }
		public DateTime LastLoginAt { get; set; }
	}
}