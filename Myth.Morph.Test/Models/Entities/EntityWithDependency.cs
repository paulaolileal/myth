using Microsoft.Extensions.DependencyInjection;
using Myth.Interfaces;
using Myth.Morph.Test.Models.Dtos;
using Myth.Morph.Test.Service;

namespace Myth.Morph.Test.Models;

internal class EntityWithDependency : IMorphableTo<DtoWithDependency> {
	public Guid Id { get; set; }
	public string Text { get; set; }

	public void MorphTo( Schema<DtoWithDependency> schema ) {
		schema
			.Bind(
				dest => dest.Text,
				sp => sp
					.GetRequiredService<IDescriptionResolver>( )
					.Resove( Text ) );
	}
}
