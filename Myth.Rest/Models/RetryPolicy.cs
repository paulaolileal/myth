using System.Net;

namespace Myth.Models {

	public class RetryPolicy {
		public int AmountRetries { get; private set; } = 3;
		public RetryStrategy Strategy { get; private set; } = RetryStrategy.ExponentialBackoffWithJitter;
		public TimeSpan MinDelay { get; private set; } = TimeSpan.FromSeconds( 1 );
		public TimeSpan MaxDelay { get; private set; } = TimeSpan.FromSeconds( 30 );
		public double BackoffMultiplier { get; private set; } = 2.0;
		public TimeSpan JitterRange { get; private set; } = TimeSpan.FromMilliseconds( 100 );
		public IList<HttpStatusCode> StatusCodes { get; private set; } = [ ];
		public IList<Type> ExceptionTypes { get; private set; } = [ ];
		public int AmountRetriesMade { get; private set; } = 0;

		private readonly Random _random = new( );

		public enum RetryStrategy {

			/// <summary>
			/// Fixed tyme between retries
			/// </summary>
			FixedDelay,

			/// <summary>
			/// Backoff exponential
			/// </summary>
			ExponentialBackoff,

			/// <summary>
			/// Backoff exponential with jitter
			/// </summary>
			ExponentialBackoffWithJitter,

			/// <summary>
			/// Random time between min and max
			/// </summary>
			RandomDelay
		}

		public bool IsRetryStatusCode( HttpStatusCode statusCode ) =>
			StatusCodes.Contains( statusCode ) || !StatusCodes.Any( );

		public bool IsRetryException( Exception exception ) =>
			ExceptionTypes.Any( type => type.IsAssignableFrom( exception.GetType( ) ) ) || !ExceptionTypes.Any( );

		/// <summary>
		/// Set to use fixed delay between retries
		/// </summary>
		public void Set( int amountRetries, TimeSpan timeBetweenRetry, params HttpStatusCode[ ] statusCodes ) {
			ValidateParameters( amountRetries, timeBetweenRetry );

			AmountRetries = amountRetries;
			Strategy = RetryStrategy.FixedDelay;
			MinDelay = timeBetweenRetry;
			StatusCodes = statusCodes;
		}

		/// <summary>
		/// Set to use delay fixed between retries
		/// </summary>
		public RetryPolicy UseFixedDelay( TimeSpan delay ) {
			ValidateDelay( delay );
			Strategy = RetryStrategy.FixedDelay;
			MinDelay = delay;
			return this;
		}

		/// <summary>
		/// Set to use backoff exponential
		/// </summary>
		public RetryPolicy UseExponentialBackoff( TimeSpan baseDelay, double multiplier = 2.0, TimeSpan? maxDelay = null ) {
			ValidateDelay( baseDelay );
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual( multiplier, 1.0, nameof( multiplier ) );

			Strategy = RetryStrategy.ExponentialBackoff;
			MinDelay = baseDelay;
			BackoffMultiplier = multiplier;
			MaxDelay = maxDelay ?? TimeSpan.FromMinutes( 1 );
			return this;
		}

		/// <summary>
		/// Set to use backoff exponential wiff jitter (default)
		/// </summary>
		public RetryPolicy UseExponentialBackoffWithJitter( TimeSpan baseDelay, double multiplier = 2.0, TimeSpan? maxDelay = null, TimeSpan? jitterRange = null ) {
			ValidateDelay( baseDelay );
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual( multiplier, 1.0, nameof( multiplier ) );

			Strategy = RetryStrategy.ExponentialBackoffWithJitter;
			MinDelay = baseDelay;
			BackoffMultiplier = multiplier;
			MaxDelay = maxDelay ?? TimeSpan.FromMinutes( 1 );
			JitterRange = jitterRange ?? TimeSpan.FromMilliseconds( MinDelay.TotalMilliseconds * 0.1 );
			return this;
		}

		/// <summary>
		/// Set the random delay between the min and max values
		/// </summary>
		public RetryPolicy UseRandom( TimeSpan minDelay, TimeSpan maxDelay ) {
			ValidateDelay( minDelay );
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual( maxDelay.TotalMilliseconds, minDelay.TotalMilliseconds, nameof( maxDelay ) );

			Strategy = RetryStrategy.RandomDelay;
			MinDelay = minDelay;
			MaxDelay = maxDelay;
			return this;
		}

		/// <summary>
		/// Define the max amount of retries 
		/// </summary>
		public RetryPolicy WithMaxAttempts( int maxAttempts ) {
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero( maxAttempts, nameof( maxAttempts ) );
			AmountRetries = maxAttempts;
			return this;
		}

		/// <summary>
		/// Defines the status codes for retrying 
		/// </summary>
		public RetryPolicy ForStatusCodes( params HttpStatusCode[ ] statusCodes ) {
			StatusCodes = statusCodes;
			return this;
		}

		/// <summary>
		/// Defines exception types when it need to be retried
		/// </summary>
		public RetryPolicy ForExceptions( params Type[ ] exceptionTypes ) {
			foreach ( var type in exceptionTypes ) {
				if ( !typeof( Exception ).IsAssignableFrom( type ) )
					throw new ArgumentException( $"Type {type.Name} must inherit from Exception", nameof( exceptionTypes ) );

				if ( !ExceptionTypes.Contains( type ) )
					ExceptionTypes.Add( type );
			}
			return this;
		}

		/// <summary>
		/// Set the unvailabe server status codes for retry
		/// </summary>
		public RetryPolicy ForServerErrors( ) {
			return ForStatusCodes(
				HttpStatusCode.InternalServerError,
				HttpStatusCode.BadGateway,
				HttpStatusCode.ServiceUnavailable,
				HttpStatusCode.GatewayTimeout,
				HttpStatusCode.TooManyRequests
			);
		}

		/// <summary>
		/// Calculate the delay for the next retry
		/// </summary>
		public TimeSpan CalculateDelay( int attemptNumber ) {
			return Strategy switch {
				RetryStrategy.FixedDelay => MinDelay,
				RetryStrategy.ExponentialBackoff => CalculateExponentialDelay( attemptNumber ),
				RetryStrategy.ExponentialBackoffWithJitter => CalculateExponentialDelayWithJitter( attemptNumber ),
				RetryStrategy.RandomDelay => CalculateRandomDelay( ),
				_ => MinDelay
			};
		}

		private TimeSpan CalculateExponentialDelay( int attemptNumber ) {
			var delay = TimeSpan.FromMilliseconds(
				MinDelay.TotalMilliseconds * Math.Pow( BackoffMultiplier, attemptNumber - 1 )
			);

			return delay > MaxDelay ? MaxDelay : delay;
		}

		private TimeSpan CalculateExponentialDelayWithJitter( int attemptNumber ) {
			var baseDelay = CalculateExponentialDelay( attemptNumber );
			var jitterMs = _random.NextDouble( ) * JitterRange.TotalMilliseconds * 2 - JitterRange.TotalMilliseconds;
			var totalMs = Math.Max( 0, baseDelay.TotalMilliseconds + jitterMs );

			return TimeSpan.FromMilliseconds( totalMs );
		}

		private TimeSpan CalculateRandomDelay( ) {
			var randomMs = _random.NextDouble( ) * ( MaxDelay.TotalMilliseconds - MinDelay.TotalMilliseconds ) + MinDelay.TotalMilliseconds;
			return TimeSpan.FromMilliseconds( randomMs );
		}

		public void SetRetriesMade( int amountRetriesMade ) => AmountRetriesMade = amountRetriesMade;

		private static void ValidateParameters( int amountRetries, TimeSpan delay ) {
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero( amountRetries, nameof( amountRetries ) );
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero( delay.TotalMilliseconds, nameof( delay ) );
		}

		private static void ValidateDelay( TimeSpan delay ) {
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero( delay.TotalMilliseconds, nameof( delay ) );
		}
	}
}