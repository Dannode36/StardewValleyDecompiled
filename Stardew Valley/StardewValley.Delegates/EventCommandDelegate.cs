namespace StardewValley.Delegates;

/// <summary>The delegate for an event command registered via <see cref="M:StardewValley.Event.RegisterCommand(System.String,StardewValley.Delegates.EventCommandDelegate)" />.</summary>
/// <param name="event">The event running the command.</param>
/// <param name="args">The space-delimited event command string, including the command name.</param>
/// <param name="context">The context for the active event.</param>
public delegate void EventCommandDelegate(Event @event, string[] args, EventContext context);
