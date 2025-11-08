using Myth.Interfaces;
using Myth.Morph.Test.Models.Entities;

namespace Myth.Morph.Test.Models.Dtos {

	public class WeatherForecastDto : IMorphableFrom<WeatherForecast> {
		public DateTime Date { get; set; }
		public int TemperatureF { get; set; }
		public string SummaryDescription { get; set; } = "";
		public int SummaryId { get; set; }
		public string Location { get; set; } = "";
		public string TemperatureCategory { get; set; } = "";

		public void MorphFrom( Schema<WeatherForecast> schema ) {
			schema
				.Bind( ( ) => Date, src => src.Date )
				.Bind( ( ) => TemperatureF, src => src.TemperatureF )
				.Bind( ( ) => Location, src => src.Location )
				.Bind( ( ) => SummaryDescription, src => Enum.GetName( src.Summary ) ?? "Unknown" )
				.Bind( ( ) => SummaryId, src => ( int )src.Summary )
				.Bind( ( ) => TemperatureCategory, src => GetTemperatureCategory( src.TemperatureC ) );
		}

		private static string GetTemperatureCategory( int temperatureC ) {
			if ( temperatureC < 0 )
				return "Freezing";
			if ( temperatureC < 10 )
				return "Cold";
			if ( temperatureC < 20 )
				return "Cool";
			if ( temperatureC < 30 )
				return "Warm";
			return "Hot";
		}
	}
}