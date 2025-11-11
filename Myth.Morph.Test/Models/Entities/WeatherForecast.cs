namespace Myth.Morph.Test.Models.Entities;

public enum Summary {
	Freezing,
	Bracing,
	Chilly,
	Cool,
	Mild,
	Warm,
	Balmy,
	Hot,
	Sweltering,
	Scorching
}

public class WeatherForecast {
	public DateTime Date { get; set; }
	public int TemperatureC { get; set; }
	public Summary Summary { get; set; }
	public string Location { get; set; } = "";
	public double Humidity { get; set; }
	public int Pressure { get; set; }

	public int TemperatureF => 32 + ( int )( TemperatureC / 0.5556 );
}
