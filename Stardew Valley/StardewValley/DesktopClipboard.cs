using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using StardewValley.NativeClipboard;
using TextCopy;

namespace StardewValley;

public class DesktopClipboard
{
	public const bool IsAvailable = true;

	public static bool GetText(ref string output)
	{
		output = SdlClipboard.GetText();
		if (output != null)
		{
			return true;
		}
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			output = "";
			output = ClipboardService.GetText();
			return true;
		}
		if (externalGetText("xclip", "-o", ref output) || externalGetText("pbpaste", "", ref output))
		{
			return true;
		}
		return false;
	}

	public static bool SetText(string text)
	{
		if (text == null)
		{
			text = "";
		}
		if (SdlClipboard.SetText(text))
		{
			return true;
		}
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
		{
			ClipboardService.SetText(text);
			return true;
		}
		if (!externalSetText("xclip", "-selection clipboard", text))
		{
			return externalSetText("pbcopy", "", text);
		}
		return true;
	}

	private static bool externalSetText(string executable, string arguments, string text)
	{
		ProcessStartInfo psi = new ProcessStartInfo(executable, arguments)
		{
			RedirectStandardInput = true,
			UseShellExecute = false
		};
		try
		{
			using Process process = Process.Start(psi);
			process.StandardInput.Write(text);
			process.StandardInput.Close();
			process.WaitForExit();
			return process.ExitCode == 0;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private static bool externalGetText(string executable, string arguments, ref string output)
	{
		ProcessStartInfo psi = new ProcessStartInfo(executable, arguments)
		{
			RedirectStandardOutput = true,
			UseShellExecute = false
		};
		try
		{
			using Process process = Process.Start(psi);
			string temp = process.StandardOutput.ReadToEnd();
			process.StandardOutput.Close();
			process.WaitForExit();
			if (process.ExitCode == 0)
			{
				output = temp;
			}
			else
			{
				output = "";
			}
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}
}
