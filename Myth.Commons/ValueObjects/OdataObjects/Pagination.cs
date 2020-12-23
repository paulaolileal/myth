using System.Collections.Generic;

namespace Myth.ValueObjects.OdataObjects {

    public class Pagination : ValueObject {

        public int PageNumber { get; set; }

        public int PageSize { get; set; }

        public static readonly Pagination Default = new Pagination {
            PageNumber = 0,
            PageSize = 3
        };

        public static readonly Pagination All = new Pagination {
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
            return $"PageNumber={PageNumber}&PageSize={PageSize}";
        }
    }
}