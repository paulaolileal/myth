using System;

namespace Myth.Rest.Test.Models {

	public class Post {
		public long Id { get; set; }
		public string Title { get; set; }
		public string Body { get; set; }
		public Guid UserId { get; set; }
	}

	public class Error {
		public long ErrorCode { get; set; }
		public string Message { get; set; } = null!;
	}
}