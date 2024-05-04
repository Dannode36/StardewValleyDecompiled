using StardewValley.Delegates;

namespace StardewValley.Triggers;

/// <summary>Parsed metadata for an action that can be triggered via data assets like <see cref="M:StardewValley.DataLoader.TriggerActions(StardewValley.LocalizedContentManager)" />.</summary>
public class CachedAction
{
	/// <summary>The space-delimited action string, including the action name.</summary>
	public string[] Args { get; }

	/// <summary>The handler which performs the action for the action specified by <see cref="P:StardewValley.Triggers.CachedAction.Args" />.</summary>
	public TriggerActionDelegate Handler { get; }

	/// <summary>An error phrase indicating why parsing the action failed (like 'unknown action X'), if applicable.</summary>
	public string Error { get; }

	/// <summary>Whether <see cref="P:StardewValley.Triggers.CachedAction.Handler" /> is the null handler which does nothing when called.</summary>
	public bool IsNullHandler { get; }

	/// <summary>Construct an instance.</summary>
	/// <param name="args">The space-delimited action string, including the action name.</param>
	/// <param name="handler">The handler which performs the action for the action specified by <see cref="P:StardewValley.Triggers.CachedAction.Args" />.</param>
	/// <param name="error">An error phrase indicating why parsing the action failed (like 'unknown action X'), if applicable.</param>
	/// <param name="isNullHandler">Whether <paramref name="handler" /> is the null handler which does nothing when called.</param>
	public CachedAction(string[] args, TriggerActionDelegate handler, string error, bool isNullHandler)
	{
		Args = args;
		Handler = handler;
		Error = error;
		IsNullHandler = isNullHandler;
	}
}
