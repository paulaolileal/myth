namespace Myth.Interfaces.Repositories.Results {

    public interface IPaginated {
        public int PageNumber { get; }

        public int PageSize { get; }

        public int TotalPages { get; }

        public int TotalItens { get; }
    }
}