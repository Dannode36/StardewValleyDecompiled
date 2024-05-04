using StardewValley;

namespace Netcode;

/// <summary>Provides utility methods for implementing net fields.</summary>
internal static class NetHelper
{
	/// <summary>Log a validation warning to the console.</summary>
	/// <param name="message">The warning text to log.</param>
	public static void LogWarning(string message)
	{
		Game1.log.Warn(message);
	}

	/// <summary>Log a validation trace message to the console.</summary>
	/// <param name="message">The warning text to log.</param>
	public static void LogVerbose(string message)
	{
		Game1.log.Verbose(message);
	}
}
