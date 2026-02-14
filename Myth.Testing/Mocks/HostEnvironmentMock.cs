using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Myth.Mocks;

internal class HostEnvironmentMock : IHostEnvironment {
	public string ApplicationName { get; set; } = "ApplicationTest.Api";
	public IFileProvider ContentRootFileProvider { get; set; } = new PhysicalFileProvider( Directory.GetCurrentDirectory( ) );
	public string ContentRootPath { get; set; } = Path.GetDirectoryName( Directory.GetCurrentDirectory( ) )!;
	public string EnvironmentName { get; set; } = "Test";
}
