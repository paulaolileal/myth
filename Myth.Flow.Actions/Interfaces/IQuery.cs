namespace Myth.Flow.Interfaces;

/// <summary>
/// Represents a query that retrieves data
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IQuery<TResponse> : IRequest<QueryResult<TResponse>> { }