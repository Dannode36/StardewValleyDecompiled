using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Machines;

/// <summary>As part of a <see cref="T:StardewValley.GameData.Machines.MachineData" />, a rule which define how to process input items and produce output.</summary>
public class MachineOutputRule
{
	/// <summary>A unique identifier for this item within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_Parsnips</c>.</summary>
	public string Id;

	/// <summary>The rules for when this output rule can be applied.</summary>
	public List<MachineOutputTriggerRule> Triggers;

	/// <summary>If multiple <see cref="F:StardewValley.GameData.Machines.MachineOutputRule.OutputItem" /> entries match, whether to use the first match instead of choosing one randomly.</summary>
	[ContentSerializer(Optional = true)]
	public bool UseFirstValidOutput;

	/// <summary>The items produced by this output rule. If multiple entries match, one will be selected randomly unless you specify <see cref="F:StardewValley.GameData.Machines.MachineOutputRule.UseFirstValidOutput" />.</summary>
	[ContentSerializer(Optional = true)]
	public List<MachineItemOutput> OutputItem;

	/// <summary>The number of in-game minutes until the output is ready to collect.</summary>
	/// <remarks>If both days and minutes are specified, days are used. If neither are specified, the item will be ready instantly.</remarks>
	[ContentSerializer(Optional = true)]
	public int MinutesUntilReady = -1;

	/// <summary>The number of in-game days until the output is ready to collect.</summary>
	/// <remarks><inheritdoc cref="F:StardewValley.GameData.Machines.MachineOutputRule.MinutesUntilReady" select="/Remarks" /></remarks>
	[ContentSerializer(Optional = true)]
	public int DaysUntilReady = -1;

	/// <summary>If set, overrides the machine's main <see cref="F:StardewValley.GameData.Machines.MachineData.InvalidCountMessage" />.</summary>
	[ContentSerializer(Optional = true)]
	public string InvalidCountMessage;

	/// <summary>Whether to regenerate the output right before the player collects it, and return the new item instead of what was originally created by the rule.</summary>
	/// <remarks>This is specialized to support bee houses. If the new item is null, the original item is returned instead.</remarks>
	[ContentSerializer(Optional = true)]
	public bool RecalculateOnCollect;
}
