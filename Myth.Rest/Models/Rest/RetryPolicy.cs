using System.Net;

namespace Myth.Models.Rest {

	public class RetryPolicy {
		public int AmountRetries { get; private set; } = 0;
		public TimeSpan TimeBetweenRetry { get; private set; } = TimeSpan.FromSeconds( 30 );
		public IList<HttpStatusCode> StatusCodes { get; private set; } = [ ];
		public int AmountRetriesMade { get; private set; } = 0;

		public bool IsRetryStatusCode( HttpStatusCode statusCode ) => StatusCodes.Contains( statusCode ) || !StatusCodes.Any( );

		public void Set( int amountRetries, TimeSpan timeBetweenRetry, params HttpStatusCode[ ] statusCodes ) {
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero( amountRetries, nameof( amountRetries ) );
			ArgumentOutOfRangeException.ThrowIfNegativeOrZero( timeBetweenRetry.TotalMilliseconds, nameof( timeBetweenRetry ) );

			AmountRetries = amountRetries;
			TimeBetweenRetry = timeBetweenRetry;
			StatusCodes = statusCodes;
		}

		public void SetRetriesMade( int amountRetriesMade ) => AmountRetriesMade = amountRetriesMade;
	}
}