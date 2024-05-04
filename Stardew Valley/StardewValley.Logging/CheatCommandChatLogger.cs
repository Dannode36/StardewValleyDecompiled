using System;
using Microsoft.Xna.Framework;
using StardewValley.Menus;

namespace StardewValley.Logging;

/// <summary>A logger which copies messages to the chat box, used when entering commands through the chat.</summary>
public class CheatCommandChatLogger : IGameLogger
{
	/// <summary>The chat box to which to copy messages.</summary>
	private readonly ChatBox ChatBox;

	/// <summary>Construct an instance.</summary>
	/// <param name="chatBox">The chat box to which to copy messages.</param>
	public CheatCommandChatLogger(ChatBox chatBox)
	{
		ChatBox = chatBox;
	}

	/// <inheritdoc />
	public void Verbose(string message)
	{
		Game1.log.Verbose(message);
	}

	/// <inheritdoc />
	public void Debug(string message)
	{
		ChatBox.addMessage(message, Color.Gray);
		Game1.log.Debug(message);
	}

	/// <inheritdoc />
	public void Info(string message)
	{
		ChatBox.addInfoMessage(message);
		Game1.log.Info(message);
	}

	/// <inheritdoc />
	public void Warn(string message)
	{
		ChatBox.addErrorMessage(message);
		Game1.log.Warn("[Warn] " + message);
	}

	/// <inheritdoc />
	public void Error(string error, Exception exception = null)
	{
		string message = "[Error] " + error;
		if (exception != null)
		{
			message = message + ": " + exception.Message;
		}
		ChatBox.addErrorMessage(message);
		Game1.log.Error(error, exception);
	}
}
