using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Machines;

/// <summary>As part of a <see cref="T:StardewValley.GameData.Machines.MachineOutputRule" />, indicates when the output rule can be applied.</summary>
public class MachineOutputTriggerRule
{
	/// <summary>The backing field for <see cref="P:StardewValley.GameData.Machines.MachineOutputTriggerRule.Id" />.</summary>
	private string IdImpl;

	/// <summary>A unique identifier for this item within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_Parsnips</c>.</summary>
	[ContentSerializer(Optional = true)]
	public string Id
	{
		get
		{
			return IdImpl ?? Trigger.ToString();
		}
		set
		{
			IdImpl = value;
		}
	}

	/// <summary>When this output rule should apply.</summary>
	[ContentSerializer(Optional = true)]
	public MachineOutputTrigger Trigger { get; set; } = MachineOutputTrigger.ItemPlacedInMachine;


	/// <summary>The qualified or unqualified item ID for the item to match, if the trigger is <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.ItemPlacedInMachine" /> or <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.OutputCollected" />.</summary>
	/// <remarks>You can specify any combination of <see cref="P:StardewValley.GameData.Machines.MachineOutputTriggerRule.RequiredItemId" />, <see cref="P:StardewValley.GameData.Machines.MachineOutputTriggerRule.RequiredTags" />, and <see cref="P:StardewValley.GameData.Machines.MachineOutputTriggerRule.Condition" />. The input item must match all specified fields; if none are specified, this conversion will always match.</remarks>
	[ContentSerializer(Optional = true)]
	public string RequiredItemId { get; set; }

	/// <summary>The context tags to match against input items, if the trigger is <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.ItemPlacedInMachine" /> or <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.OutputCollected" />. An item must match all of the listed tags to select this rule. You can negate a tag with ! (like <c>!fossil_item</c> to exclude fossils).</summary>
	/// <inheritdoc cref="P:StardewValley.GameData.Machines.MachineOutputTriggerRule.RequiredItemId" select="Remarks" />
	[ContentSerializer(Optional = true)]
	public List<string> RequiredTags { get; set; }

	/// <summary>The required stack size for the input item, if the trigger is <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.ItemPlacedInMachine" /> or <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.OutputCollected" />.</summary>
	[ContentSerializer(Optional = true)]
	public int RequiredCount { get; set; } = 1;


	/// <summary>A game state query which indicates whether a given input should be matched (if the other requirements are matched too). Item-only tokens are valid for this check if the trigger is <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.ItemPlacedInMachine" /> or <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.OutputCollected" />. Defaults to always true.</summary>
	/// <inheritdoc cref="P:StardewValley.GameData.Machines.MachineOutputTriggerRule.RequiredItemId" select="Remarks" />
	[ContentSerializer(Optional = true)]
	public string Condition { get; set; }
}
