using StardewValley.GameData;

namespace StardewValley.Triggers;

/// <summary>A cached, pre-parsed representation of a trigger action defined in <c>Data/TriggerActions</c>.</summary>
public class CachedTriggerAction
{
	/// <summary>The original trigger action data from <c>Data/TriggerActions</c>.</summary>
	public TriggerActionData Data { get; }

	/// <summary>The validated actions to invoke.</summary>
	public CachedAction[] Actions { get; }

	/// <summary>The validated space-delimited action strings.</summary>
	public string[] ActionStrings { get; }

	/// <summary>Construct an instance.</summary>
	/// <param name="data">The original trigger action data from <c>Data/TriggerActions</c>.</param>
	/// <param name="actions">The validated actions to invoke.</param>
	public CachedTriggerAction(TriggerActionData data, CachedAction[] actions)
	{
		Data = data;
		Actions = actions;
		if (actions.Length == 0)
		{
			ActionStrings = LegacyShims.EmptyArray<string>();
			return;
		}
		ActionStrings = new string[actions.Length];
		for (int i = 0; i < actions.Length; i++)
		{
			ActionStrings[i] = string.Join(" ", actions[i].Args);
		}
	}
}
