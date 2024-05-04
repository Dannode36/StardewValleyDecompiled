using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Machines;

/// <summary>As part of a <see cref="T:StardewValley.GameData.Machines.MachineData" />, an item produced by this machine.</summary>
/// <remarks>Only one item can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.</remarks>
public class MachineItemOutput : GenericSpawnItemDataWithCondition
{
	/// <summary>Machine-specific data provided to the machine logic, if applicable.</summary>
	/// <remarks>For vanilla machines, this is used by casks to set the <c>AgingMultiplier</c> for each item.</remarks>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomData;

	/// <summary>A C# method which produces the item to output.</summary>
	/// <remarks>
	///   <para><strong>This is an advanced field. Most machines shouldn't use this.</strong> This must be specified in the form <c>{full type name}: {method name}</c> (like <c>StardewValley.Object, Stardew Valley: OutputSolarPanel</c>). The method must be static, take five arguments (<c>Object machine, GameLocation location, Farmer player, Item? inputItem, bool probe</c>), and return the <c>Item</c> instance to output. If this method returns null, the machine won't output anything.</para>
	///
	///   <para>If set, the other fields which change the output item (like <see cref="P:StardewValley.GameData.ISpawnItemData.ItemId" /> or <see cref="P:StardewValley.GameData.Machines.MachineItemOutput.CopyColor" />) are ignored.</para>
	/// </remarks>
	[ContentSerializer(Optional = true)]
	public string OutputMethod { get; set; }

	/// <summary>Whether to inherit the color of the input item if it was a <c>ColoredObject</c>. This mainly affects roe.</summary>
	[ContentSerializer(Optional = true)]
	public bool CopyColor { get; set; }

	/// <summary>Whether to inherit the price of the input item, before modifiers like <see cref="P:StardewValley.GameData.Machines.MachineItemOutput.PriceModifiers" /> are applied. This is ignored if the input or output aren't both object (<c>(O)</c>)-type.</summary>
	[ContentSerializer(Optional = true)]
	public bool CopyPrice { get; set; }

	/// <summary>Whether to inherit the quality of the input item, before modifiers like <see cref="P:StardewValley.GameData.GenericSpawnItemData.QualityModifiers" /> are applied.</summary>
	[ContentSerializer(Optional = true)]
	public bool CopyQuality { get; set; }

	/// <summary>The produced item's preserved item type, if applicable. This sets the equivalent flag on the output item. The valid values are <c>Jelly</c>, <c>Juice</c>, <c>Pickle</c>, <c>Roe</c> or <c>AgedRoe</c>, and <c>Wine</c>. Defaults to none.</summary>
	[ContentSerializer(Optional = true)]
	public string PreserveType { get; set; }

	/// <summary>The produced item's preserved unqualified item ID, if applicable. For example, blueberry wine has its preserved item ID set to the blueberry ID. This can be set to <c>DROP_IN</c> to use the input item's ID. Default none.</summary>
	[ContentSerializer(Optional = true)]
	public string PreserveId { get; set; }

	/// <summary>An amount by which to increment the machine's spritesheet index while it's processing this output. This stacks with <see cref="F:StardewValley.GameData.Machines.MachineData.ShowNextIndexWhileWorking" /> or <see cref="F:StardewValley.GameData.Machines.MachineData.ShowNextIndexWhenReady" />.</summary>
	[ContentSerializer(Optional = true)]
	public int IncrementMachineParentSheetIndex { get; set; }

	/// <summary>Changes to apply to the item price. This is ignored if the output isn't object (<c>(O)</c>)-type.</summary>
	[ContentSerializer(Optional = true)]
	public List<QuantityModifier> PriceModifiers { get; set; }

	/// <summary>How multiple <see cref="P:StardewValley.GameData.Machines.MachineItemOutput.PriceModifiers" /> should be combined.</summary>
	[ContentSerializer(Optional = true)]
	public QuantityModifier.QuantityModifierMode PriceModifierMode { get; set; }
}
