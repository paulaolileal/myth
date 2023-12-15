using Microsoft.AspNetCore.Mvc;

namespace Myth.ValueObjects {

    public class Pagination : ValueObject {

        [FromQuery( Name = "$pagenumber" )]
        public int PageNumber { get; set; }

        [FromQuery( Name = "$pagesize" )]
        public int PageSize { get; set; }

        public static readonly Pagination Default = new( ) {
            PageNumber = 0,
            PageSize = 3
        };

        public static readonly Pagination All = new( ) {
            PageNumber = -1,
            PageSize = -1
        };

        public Pagination( ) {
        }

        public Pagination( int pageNumber, int pageSize ) {
            PageNumber = pageNumber;
            PageSize = pageSize;
        }

        protected override IEnumerable<object> GetAtomicValues( ) {
            yield return PageNumber;
            yield return PageSize;
        }

        public string Build( ) {
            return $"$pagenumber={PageNumber}&$pagesize={PageSize}";
        }

        public int GetPagesToSkip( ) =>
            PageNumber > 0
            ? ( PageNumber - 1 ) * PageSize
            : 1;
    }
}