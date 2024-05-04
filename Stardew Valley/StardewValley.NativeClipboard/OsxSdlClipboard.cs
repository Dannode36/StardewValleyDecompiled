namespace StardewValley.NativeClipboard;

/// <summary>Provides a wrapper around SDL's clipboard API for OSX.</summary>
internal sealed class OsxSdlClipboard : SdlClipboard
{
	/// <summary>Constructs an instance and sets the providing platform name.</summary>
	public OsxSdlClipboard()
	{
		PlatformName = "OSX";
	}
}
