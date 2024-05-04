using System;

namespace StardewValley.Logging;

/// <summary>Handles writing messages to the game log for Stardew Valley itself.</summary>
public interface IGameLogger
{
	/// <summary>Log tracing info intended for Stardew Valley developers troubleshooting specific issues.</summary>
	/// <param name="message">The message to log.</param>
	void Verbose(string message);

	/// <summary>Log troubleshooting info intended for developers or players.</summary>
	/// <param name="message">The message to log.</param>
	void Debug(string message);

	/// <summary>Log a message intended for players interacting with the console.</summary>
	/// <param name="message">The message to log.</param>
	void Info(string message);

	/// <summary>Log a potential problem that users should be aware of.</summary>
	void Warn(string message);

	/// <summary>Log an message indicating something has gone wrong.</summary>
	/// <param name="error">The message to log.</param>
	/// <param name="exception">The underlying exception.</param>
	void Error(string error, Exception exception = null);
}
