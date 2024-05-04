namespace StardewValley.NativeClipboard;

/// <summary>Provides a wrapper around SDL's clipboard API for Linux.</summary>
internal sealed class LinuxSdlClipboard : SdlClipboard
{
	/// <summary>Constructs an instance and sets the providing platform name.</summary>
	public LinuxSdlClipboard()
	{
		PlatformName = "Linux";
	}
}
