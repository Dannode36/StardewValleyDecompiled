using System.Collections.Generic;
using StardewValley.GameData;

namespace StardewValley.Delegates;

/// <summary>The contextual values for a <see cref="T:StardewValley.Delegates.TriggerActionDelegate" />.</summary>
public readonly struct TriggerActionContext
{
	/// <summary>The trigger for which the action is being invoked, or <c>"Manual"</c> if it's not being invoked via <c>Data/TriggerActions</c>.</summary>
	public readonly string Trigger;

	/// <summary>The contextual arguments provided with the trigger, or an empty array if none were provided. For example, an 'item received' trigger might provide the item instance and index.</summary>
	public readonly object[] TriggerArgs;

	/// <summary>The entry data in <c>Data/TriggerActions</c> for the action being applied, or <c>null</c> if the action is being applied some other way (e.g. <c>$action</c> in dialogue).</summary>
	public readonly TriggerActionData Data;

	/// <summary>The custom fields which can be set by mods for custom trigger action behavior, or <c>null</c> if none were set.</summary>
	public readonly Dictionary<string, object> CustomFields;

	/// <summary>Construct an instance.</summary>
	/// <param name="trigger">The trigger for which the action is being invoked, or <c>"Manual"</c> if it's not being invoked via <c>Data/TriggerActions</c>.</param>
	/// <param name="triggerArgs">The contextual arguments provided with the trigger, or an empty array if none were provided. For example, an 'item received' trigger might provide the item instance and index.</param>
	/// <param name="data">The entry data in <c>Data/TriggerActions</c> for the action being applied, or <c>null</c> if the action is being applied some other way (e.g. <c>$action</c> in dialogue).</param>
	/// <param name="customFields">The custom fields which can be set by mods for custom trigger action behavior.</param>
	public TriggerActionContext(string trigger, object[] triggerArgs, TriggerActionData data, Dictionary<string, object> customFields = null)
	{
		Trigger = trigger;
		TriggerArgs = triggerArgs;
		Data = data;
		CustomFields = customFields;
	}
}
