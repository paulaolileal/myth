using System;

namespace Myth.Rest.Test.Models;

public class Post : IPost, ICloneable {
	public long Id { get; set; }
	public string Title { get; set; } = null!;
	public string Body { get; set; } = null!;
	public Guid UserId { get; set; }

	public object Clone( ) => new Post {
		Body = Body,
		Id = Id,
		Title = Title,
		UserId = UserId
	};

	public Post Copy( ) => ( Post )Clone( );
}