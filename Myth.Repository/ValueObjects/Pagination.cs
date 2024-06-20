using Microsoft.AspNetCore.Mvc;

namespace Myth.ValueObjects {

	public class Pagination : ValueObject {

		/// <summary>
		/// Page number
		/// </summary>
		[FromQuery( Name = "$pagenumber" )]
		public int PageNumber { get; set; }

		/// <summary>
		/// Page size
		/// </summary>
		[FromQuery( Name = "$pagesize" )]
		public int PageSize { get; set; }

		public static readonly Pagination Default = new( );

		public static readonly Pagination All = new( -1, -1 );

		public Pagination( ) {
			PageNumber = 1;
			PageSize = 10;
		}

		public Pagination( int pageNumber, int pageSize ) {
			PageNumber = pageNumber;
			PageSize = pageSize;
		}

		protected override IEnumerable<object> GetAtomicValues( ) {
			yield return PageNumber;
			yield return PageSize;
		}
	}
}