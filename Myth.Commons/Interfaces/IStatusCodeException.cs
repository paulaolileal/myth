using System.Net;

namespace Myth.Interfaces;

/// <summary>
/// Marks exceptions that carry an HTTP status code, allowing the pipeline to preserve it in <see cref="Myth.Models.Result{T}.StatusCode"/>.
/// </summary>
public interface IStatusCodeException {
	HttpStatusCode? StatusCode { get; }
}
