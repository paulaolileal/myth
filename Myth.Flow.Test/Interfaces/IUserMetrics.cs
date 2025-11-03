public interface IUserMetrics {

	void IncrementUserCreated( );

	int TotalUsersCreated { get; }
}