namespace StardewValley.Delegates;

/// <summary>A delegate which handles an action which can be triggered via <c>Data/TriggerActions</c>, registered via <see cref="M:StardewValley.Triggers.TriggerActionManager.RegisterAction(System.String,StardewValley.Delegates.TriggerActionDelegate)" />.</summary>
/// <param name="args">The space-delimited action string, including the action name.</param>
/// <param name="context">The trigger action context.</param>
/// <param name="error">An error phrase indicating why applying the action failed (like 'required argument X missing'), if applicable. This should always be set to <c>null</c> when returning true, and a non-empty message when returning false.</param>
/// <returns>Returns whether the action was handled successfully (regardless of whether it did anything).</returns>
public delegate bool TriggerActionDelegate(string[] args, TriggerActionContext context, out string error);
