using System;
using System.Runtime.InteropServices;
using System.Text;

namespace StardewValley.NativeClipboard;

/// <summary>A wrapper around SDL's native clipboard API.</summary>
internal abstract class SdlClipboard
{
	/// <summary>The underlying platform that provides the SDL clipboard API.</summary>
	private static SdlClipboard PlatformClipboard;

	/// <summary>The name of the platform providing the native SDL API.</summary>
	protected string PlatformName;

	/// <summary>The platform that the clipboard is running on.</summary>
	internal static readonly ClipboardPlatformType Platform;

	static SdlClipboard()
	{
		Platform = GetPlatformType();
		switch (Platform)
		{
		case ClipboardPlatformType.Linux:
			PlatformClipboard = new LinuxSdlClipboard();
			break;
		case ClipboardPlatformType.OSX:
			PlatformClipboard = new OsxSdlClipboard();
			break;
		case ClipboardPlatformType.Windows:
			PlatformClipboard = new WindowsSdlClipboard();
			break;
		default:
			PlatformClipboard = null;
			break;
		}
	}

	/// <summary>Retrieves the clipboard text from the underlying platform's native SDL API.</summary>
	/// <returns>A string containing the clipboard text, null if the clipboard was empty or if an error occurred.</returns>
	public static string GetText()
	{
		if (PlatformClipboard == null)
		{
			return null;
		}
		IntPtr clipboardPtr;
		try
		{
			clipboardPtr = PlatformClipboard.GetTextImpl();
		}
		catch (Exception)
		{
			return null;
		}
		if (clipboardPtr == IntPtr.Zero)
		{
			return null;
		}
		int length;
		for (length = 0; Marshal.ReadByte(clipboardPtr, length) != 0; length++)
		{
		}
		if (length == 0)
		{
			return null;
		}
		byte[] stringBytes = new byte[length];
		Marshal.Copy(clipboardPtr, stringBytes, 0, length);
		try
		{
			return Encoding.UTF8.GetString(stringBytes, 0, length);
		}
		catch (Exception)
		{
			return null;
		}
	}

	/// <summary>Sets the clipboard text using the underlying platform's native SDL API.</summary>
	/// <param name="text">The string to replace the current clipboard text.</param>
	public static bool SetText(string text)
	{
		if (PlatformClipboard == null)
		{
			return false;
		}
		if (text == null)
		{
			return false;
		}
		byte[] stringBytes = Encoding.UTF8.GetBytes(text);
		IntPtr stringPtr = Marshal.AllocHGlobal(stringBytes.Length + 1);
		try
		{
			Marshal.Copy(stringBytes, 0, stringPtr, stringBytes.Length);
			Marshal.WriteByte(stringPtr, stringBytes.Length, 0);
			int result;
			try
			{
				result = PlatformClipboard.SetTextImpl(stringPtr);
			}
			catch (Exception)
			{
				return false;
			}
			return result == 0;
		}
		finally
		{
			Marshal.FreeHGlobal(stringPtr);
		}
	}

	/// <summary>Determines the platform-specific SDL clipboard API provider based on runtime information.</summary>
	private static ClipboardPlatformType GetPlatformType()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			return ClipboardPlatformType.Linux;
		}
		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			return ClipboardPlatformType.OSX;
		}
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			return ClipboardPlatformType.Windows;
		}
		return ClipboardPlatformType.Unknown;
	}

	/// <summary>Retrieves the clipboard text from the native SDL API.</summary>
	/// <returns>Returns a pointer to a null-terminated C-string, containing the clipboard text. May be empty if an error occurred.</returns>
	protected virtual IntPtr GetTextImpl()
	{
		throw new NotImplementedException("GetClipboardText() for " + PlatformName + " is not provided on this platform!");
	}

	/// <summary>Sets the clipboard text using the native SDL API.</summary>
	/// <param name="text">A pointer to a null-terminated, UTF-8 C-string.</param>
	protected virtual int SetTextImpl(IntPtr text)
	{
		throw new NotImplementedException("SetClipboardText(...) for " + PlatformName + " is not provided on this platform!");
	}
}
