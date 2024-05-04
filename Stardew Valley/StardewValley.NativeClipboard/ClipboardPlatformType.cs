namespace StardewValley.NativeClipboard;

/// <summary>The platform that provides the clipboard.</summary>
internal enum ClipboardPlatformType
{
	/// <summary>The platform is Linux.</summary>
	Linux,
	/// <summary>The platform is macOS/OSX.</summary>
	OSX,
	/// <summary>The platform is Windows.</summary>
	Windows,
	/// <summary>The platform is unknown.</summary>
	Unknown
}
