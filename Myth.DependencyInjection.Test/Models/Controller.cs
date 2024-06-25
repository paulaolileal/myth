using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace Myth.DependencyInjection.Test.Models {

	[ApiController]
	[ApiVersion( "1.0" )]
	[Route( "api/v{version:apiVersion}/[controller]" )]
	internal class Controller : ControllerBase {
	}
}