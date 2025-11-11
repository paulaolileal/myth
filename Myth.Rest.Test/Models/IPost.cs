using System;

namespace Myth.Rest.Test.Models;

internal interface IPost {
	long Id { get; set; }
	string Title { get; set; }
	string Body { get; set; }
	Guid UserId { get; set; }
}
