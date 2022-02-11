using Myth.Interfaces.Repositories.Results;

namespace Myth.Repositories.Results {

    public class Paginated<TEntity> : IPaginated<TEntity> {
        public int PageNumber { get; private set; }

        public int PageSize { get; private set; }

        public int TotalItens { get; private set; }

        public int TotalPages { get; private set; }

        public IEnumerable<TEntity> Itens { get; private set; }

        public Paginated(
            int pageNumber,
            int pageSize,
            int totalItens,
            int totalPages,
            IEnumerable<TEntity> itens ) {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalItens = totalItens;
            TotalPages = totalPages;
            Itens = itens;
        }
    }
}