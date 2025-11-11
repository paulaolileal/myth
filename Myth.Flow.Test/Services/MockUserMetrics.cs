public class MockUserMetrics : IUserMetrics {
	public int TotalUsersCreated { get; private set; }

	public void IncrementUserCreated( ) {
		TotalUsersCreated++;
	}
}
