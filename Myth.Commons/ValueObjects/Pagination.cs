using System.Collections.Generic;

namespace Myth.ValueObjects {

    public class Pagination: ValueObject {
        public int PageNumber { get; private set; }
        public int PageSize { get; private set; }

        public Pagination( ) {
            PageNumber = -1;
            PageSize = -1;
        }

        public Pagination( int startIndex, int pageSize ) {
            PageNumber = startIndex;
            PageSize = pageSize;
        }

        public static readonly Pagination Default = new Pagination {
            PageNumber = 0,
            PageSize = 3
        };

        public static readonly Pagination All = new Pagination {
            PageNumber = -1,
            PageSize = -1
        };

        protected override IEnumerable<object> GetAtomicValues( ) {
            yield return PageNumber;
            yield return PageSize;
        }
    }
}