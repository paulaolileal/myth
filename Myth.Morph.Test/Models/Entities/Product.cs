namespace Myth.Morph.Test.Models.Entities;

public class Product {
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public decimal Price { get; set; }
	public DateTime CreatedAt { get; set; }
	public bool IsActive { get; set; }
	public string Category { get; set; } = "";
	public int Stock { get; set; }
	public decimal Weight { get; set; }
}
