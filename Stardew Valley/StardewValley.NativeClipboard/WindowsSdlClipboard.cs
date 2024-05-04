using System;
using System.Runtime.InteropServices;

namespace StardewValley.NativeClipboard;

/// <summary>Provides a wrapper around SDL's clipboard API for Windows.</summary>
internal sealed class WindowsSdlClipboard : SdlClipboard
{
	/// <inheritdoc cref="M:StardewValley.NativeClipboard.WindowsSdlClipboard.GetTextImpl" />
	[DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern IntPtr SDL_GetClipboardText();

	/// <inheritdoc cref="M:StardewValley.NativeClipboard.WindowsSdlClipboard.SetTextImpl(System.IntPtr)" />
	[DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
	private static extern int SDL_SetClipboardText(IntPtr text);

	/// <summary>Constructs an instance and sets the providing platform name.</summary>
	public WindowsSdlClipboard()
	{
		PlatformName = "Windows";
	}

	/// <inheritdoc />
	protected override IntPtr GetTextImpl()
	{
		return SDL_GetClipboardText();
	}

	/// <inheritdoc />
	protected override int SetTextImpl(IntPtr text)
	{
		return SDL_SetClipboardText(text);
	}
}
