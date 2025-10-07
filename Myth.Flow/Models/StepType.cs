namespace Myth.Models {

	/// <summary>
	/// Specifies the type of a pipeline step.
	/// </summary>
	internal enum StepType {

		/// <summary>
		/// Synchronous step.
		/// </summary>
		Sync,

		/// <summary>
		/// Asynchronous step.
		/// </summary>
		Async,

		/// <summary>
		/// Transformation step that changes the context type.
		/// </summary>
		Transform,

		/// <summary>
		/// Tap step for side-effect actions.
		/// </summary>
		Tap,

		/// <summary>
		/// Conditional step that executes based on a predicate.
		/// </summary>
		Conditional
	}
}