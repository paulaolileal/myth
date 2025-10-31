using Xunit;

namespace Myth.Testing.Extensions {

	/// <summary>
	/// Extension methods to improve testing experience
	/// </summary>
	public static class TestExtensions {

		/// <summary>
		/// Executes an async test action and handles common async patterns
		/// </summary>
		/// <param name="asyncAction">The async action to execute</param>
		/// <returns>A task representing the asynchronous operation</returns>
		public static async Task RunAsync( Func<Task> asyncAction ) {
			await asyncAction( );
		}

		/// <summary>
		/// Executes an async test function and returns the result
		/// </summary>
		/// <typeparam name="T">The return type</typeparam>
		/// <param name="asyncFunc">The async function to execute</param>
		/// <returns>The result of the async function</returns>
		public static async Task<T> RunAsync<T>( Func<Task<T>> asyncFunc ) {
			return await asyncFunc( );
		}

		/// <summary>
		/// Asserts that an async action throws a specific exception type
		/// </summary>
		/// <typeparam name="TException">The expected exception type</typeparam>
		/// <param name="asyncAction">The async action that should throw</param>
		/// <returns>The thrown exception</returns>
		public static async Task<TException> AssertThrowsAsync<TException>( Func<Task> asyncAction ) where TException : Exception {
			return await Assert.ThrowsAsync<TException>( asyncAction );
		}

		/// <summary>
		/// Asserts that an async action throws any exception
		/// </summary>
		/// <param name="asyncAction">The async action that should throw</param>
		/// <returns>The thrown exception</returns>
		public static async Task<Exception> AssertThrowsAsync( Func<Task> asyncAction ) {
			return await Assert.ThrowsAsync<Exception>( asyncAction );
		}

		/// <summary>
		/// Asserts that an async action does not throw any exception
		/// </summary>
		/// <param name="asyncAction">The async action that should not throw</param>
		/// <returns>A task representing the asynchronous operation</returns>
		public static async Task AssertDoesNotThrowAsync( Func<Task> asyncAction ) {
			var exception = await Record.ExceptionAsync( asyncAction );
			Assert.Null( exception );
		}

		/// <summary>
		/// Creates a timeout task for async operations
		/// </summary>
		/// <param name="timeout">The timeout duration</param>
		/// <returns>A task that represents the timeout</returns>
		public static Task CreateTimeoutTask( TimeSpan timeout ) {
			return Task.Delay( timeout );
		}

		/// <summary>
		/// Executes an async action with a timeout
		/// </summary>
		/// <param name="asyncAction">The async action to execute</param>
		/// <param name="timeout">The timeout duration</param>
		/// <returns>A task representing the asynchronous operation</returns>
		/// <exception cref="TimeoutException">Thrown when the operation times out</exception>
		public static async Task WithTimeoutAsync( Func<Task> asyncAction, TimeSpan timeout ) {
			var task = asyncAction( );
			var timeoutTask = CreateTimeoutTask( timeout );

			var completedTask = await Task.WhenAny( task, timeoutTask );

			if ( completedTask == timeoutTask )
				throw new TimeoutException( $"Operation timed out after {timeout.TotalMilliseconds}ms" );

			await task; // Re-await to get potential exceptions
		}

		/// <summary>
		/// Executes an async function with a timeout
		/// </summary>
		/// <typeparam name="T">The return type</typeparam>
		/// <param name="asyncFunc">The async function to execute</param>
		/// <param name="timeout">The timeout duration</param>
		/// <returns>The result of the async function</returns>
		/// <exception cref="TimeoutException">Thrown when the operation times out</exception>
		public static async Task<T> WithTimeoutAsync<T>( Func<Task<T>> asyncFunc, TimeSpan timeout ) {
			var task = asyncFunc( );
			var timeoutTask = CreateTimeoutTask( timeout );

			var completedTask = await Task.WhenAny( task, timeoutTask );

			if ( completedTask == timeoutTask )
				throw new TimeoutException( $"Operation timed out after {timeout.TotalMilliseconds}ms" );

			return await task; // Re-await to get potential exceptions and result
		}
	}
}