using System.Diagnostics.CodeAnalysis;

namespace Myth.Models.Rest;

[ExcludeFromCodeCoverage]
public class RestUploadSettings {

	public enum UploadMethod {
		POST,
		PUT,
		PATCH
	}

	public UploadMethod Method { get; private set; }

	public void UsePostAsMethod( ) => Method = UploadMethod.POST;

	public void UsePutAsMethod( ) => Method = UploadMethod.PUT;

	public void UsePatchAsMethod( ) => Method = UploadMethod.PATCH;
}