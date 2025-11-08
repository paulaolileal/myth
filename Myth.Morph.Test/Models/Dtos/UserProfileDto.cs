using Myth.Interfaces;
using Myth.Morph.Test.Models.Entities;

namespace Myth.Morph.Test.Models.Dtos {

	public class UserProfileDto : IMorphableFrom<User> {
		public string FullName { get; set; } = "";
		public string EmailStatus { get; set; } = "";
		public int Age { get; set; }
		public string Country { get; set; } = "";
		public string ActivityStatus { get; set; } = "";
		public string InitialsAvatar { get; set; } = "";

		public void MorphFrom( Schema<User> schema ) {
			// Note: In a real IMorphableFrom implementation with service provider access,
			// we would need to modify the Bind method to accept IServiceProvider
			// For now, we'll implement without service provider dependency
			schema
				.Bind( ( ) => FullName, src => $"{src.FirstName} {src.LastName}" )
				.Bind( ( ) => EmailStatus, src => src.IsEmailVerified ? "Verified" : "Pending" )
				.Bind( ( ) => Age, src => DateTime.Now.Year - src.BirthDate.Year )
				.Bind( ( ) => Country, src => src.CountryCode.ToUpper( ) )
				.Bind( ( ) => ActivityStatus, src => ( DateTime.Now - src.LastLoginAt ).Days <= 7 ? "Active" : "Inactive" )
				.Bind( ( ) => InitialsAvatar, src => $"{( src.FirstName.Length > 0 ? src.FirstName[ 0 ] : 'U' )}{( src.LastName.Length > 0 ? src.LastName[ 0 ] : 'U' )}" );
		}
	}
}