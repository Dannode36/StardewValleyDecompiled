namespace StardewValley.Delegates;

/// <summary>The delegate for an event precondition registered via <see cref="M:StardewValley.Event.RegisterPrecondition(System.String,StardewValley.Delegates.EventPreconditionDelegate)" />.</summary>
/// <param name="location">The location which is checking the event.</param>
/// <param name="eventId">The unique ID for the event being checked.</param>
/// <param name="args">The space-delimited event precondition string, including the precondition name.</param>
public delegate bool EventPreconditionDelegate(GameLocation location, string eventId, string[] args);
