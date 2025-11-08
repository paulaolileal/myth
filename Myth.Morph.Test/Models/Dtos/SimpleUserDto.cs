using Myth.Interfaces;
using Myth.Morph.Test.Models.Entities;

namespace Myth.Morph.Test.Models.Dtos {

	public class SimpleUserDto : IMorphableFrom<User> {
		public int Id { get; set; }
		public string FirstName { get; set; } = "";
		public string LastName { get; set; } = "";
		public string Email { get; set; } = "";
		public DateTime BirthDate { get; set; }
		public string CountryCode { get; set; } = "";
		public bool IsEmailVerified { get; set; }
		public DateTime LastLoginAt { get; set; }

		public void MorphFrom( Schema<User> schema ) {
			// No custom mappings - should use automatic mapping for all properties
		}
	}
}