using Myth.Interfaces;
using Myth.Morph.Test.Models.Entities;

namespace Myth.Morph.Test.Models.Dtos {

	public class ProductSummaryDto : IMorphableFrom<Product> {
		public string DisplayName { get; set; } = "";
		public string FormattedPrice { get; set; } = "";
		public string Status { get; set; } = "";
		public int DaysOld { get; set; }
		public string StockLevel { get; set; } = "";
		public string CategoryCode { get; set; } = "";

		public void MorphFrom( Schema<Product> schema ) {
			schema
				.Bind( ( ) => DisplayName, src => $"{src.Name ?? ""} ({src.Id})" )
				.Bind( ( ) => FormattedPrice, src => $"${src.Price.ToString( "F2", System.Globalization.CultureInfo.InvariantCulture )}" )
				.Bind( ( ) => Status, src => src.IsActive ? "Active" : "Inactive" )
				.Bind( ( ) => DaysOld, src => ( DateTime.Now - src.CreatedAt ).Days )
				.Bind( ( ) => StockLevel, src => GetStockLevel( src.Stock ) )
				.Bind( ( ) => CategoryCode, src => ( src.Category ?? "" ).ToUpper( ).Replace( " ", "_" ) );
		}

		private static string GetStockLevel( int stock ) {
			if ( stock == 0 )
				return "Out of Stock";
			if ( stock < 10 )
				return "Low Stock";
			if ( stock < 50 )
				return "Medium Stock";
			return "High Stock";
		}
	}
}