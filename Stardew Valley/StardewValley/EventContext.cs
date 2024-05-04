using Microsoft.Xna.Framework;

namespace StardewValley;

/// <summary>The context info for a running event.</summary>
public class EventContext
{
	/// <summary>The active event.</summary>
	public Event Event { get; }

	/// <summary>The location in which the event is running.</summary>
	public GameLocation Location { get; }

	/// <summary>The current game execution time.</summary>
	public GameTime Time { get; }

	/// <summary>The space-delimited event command string, including the command name.</summary>
	public string[] Args { get; }

	/// <summary>Construct an instance.</summary>
	/// <param name="event"><inheritdoc cref="P:StardewValley.EventContext.Event" path="/summary" /></param>
	/// <param name="location"><inheritdoc cref="P:StardewValley.EventContext.Location" path="/summary" /></param>
	/// <param name="time"><inheritdoc cref="P:StardewValley.EventContext.Time" path="/summary" /></param>
	/// <param name="args"><inheritdoc cref="P:StardewValley.EventContext.Args" path="/summary" /></param>
	public EventContext(Event @event, GameLocation location, GameTime time, string[] args)
	{
		Event = @event;
		Location = location;
		Time = time;
		Args = args;
	}

	/// <summary>Log an error indicating that the command is invalid.</summary>
	/// <param name="error">The error to log.</param>
	/// <param name="willSkip">Whether the event command will be skipped entirely. If false, the event command will be applied without the argument(s) that failed.</param>
	public void LogError(string error, bool willSkip = false)
	{
		Event.LogCommandError(Args, error, willSkip);
	}

	/// <summary>Log an error indicating that the command format is invalid and skip the current command.</summary>
	/// <param name="error">The error to log.</param>
	/// <param name="hideError">Whether to skip without logging an error message.</param>
	public void LogErrorAndSkip(string error, bool hideError = false)
	{
		Event.LogCommandErrorAndSkip(Args, error, hideError);
	}
}
