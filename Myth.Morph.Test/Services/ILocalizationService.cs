namespace Myth.Morph.Test.Services;

public interface ILocalizationService {

	string GetCountryName( string countryCode );

	string GetLocalizedStatus( string status );
}

public class LocalizationService : ILocalizationService {

	public string GetCountryName( string countryCode ) {
		return countryCode.ToUpper( ) switch {
			"US" => "United States",
			"BR" => "Brazil",
			"CA" => "Canada",
			"UK" => "United Kingdom",
			"DE" => "Germany",
			"FR" => "France",
			_ => "Unknown Country"
		};
	}

	public string GetLocalizedStatus( string status ) {
		return status switch {
			"Verified" => "Email Verified ✓",
			"Pending" => "Email Pending ⏳",
			"Active" => "User Active 🟢",
			"Inactive" => "User Inactive 🔴",
			_ => status
		};
	}
}
