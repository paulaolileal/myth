namespace Myth.Interfaces;

/// <summary>
/// Base marker interface for all requests (commands and queries)
/// </summary>
public interface IRequest { }

/// <summary>
/// Represents a request with a response
/// </summary>
/// <typeparam name="TResponse">Response type</typeparam>
public interface IRequest<out TResponse> : IRequest { }
