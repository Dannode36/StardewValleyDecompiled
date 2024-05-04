using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>An action that's performed when a trigger is called and its conditions are met.</summary>
public class TriggerActionData
{
	/// <summary>A unique string ID for this action in the global list.</summary>
	public string Id;

	/// <summary>When the action should be checked. This must be a space-delimited list of registered trigger types like <c>DayStarted</c>.</summary>
	public string Trigger;

	/// <summary>If set, a game state query which indicates whether the action should run when the trigger runs.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>Whether to only run this action for the main player.</summary>
	[ContentSerializer(Optional = true)]
	public bool HostOnly;

	/// <summary>The single action to perform.</summary>
	/// <remarks><see cref="F:StardewValley.GameData.TriggerActionData.Action" /> and <see cref="F:StardewValley.GameData.TriggerActionData.Actions" /> can technically be used together, but generally you should pick one or the other.</remarks>
	[ContentSerializer(Optional = true)]
	public string Action;

	/// <summary>The actions to perform.</summary>
	/// <inheritdoc cref="F:StardewValley.GameData.TriggerActionData.Action" path="/remarks" />
	[ContentSerializer(Optional = true)]
	public List<string> Actions;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;

	/// <summary>Whether to mark the action applied when it's applied. If false, the action can repeat immediately when the same trigger is raised, and queries like <c>PLAYER_HAS_RUN_TRIGGER_ACTION</c> will return false for it.</summary>
	[ContentSerializer(Optional = true)]
	public bool MarkActionApplied = true;
}
