using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Machines;

/// <summary>As part of a <see cref="T:StardewValley.GameData.Machines.MachineData" />, an extra item required before the machine starts.</summary>
public class MachineItemAdditionalConsumedItems
{
	/// <summary>The qualified or unqualified item ID for the required item.</summary>
	public string ItemId;

	/// <summary>The required stack size for the item matching <see cref="F:StardewValley.GameData.Machines.MachineItemAdditionalConsumedItems.ItemId" />.</summary>
	[ContentSerializer(Optional = true)]
	public int RequiredCount = 1;

	/// <summary>If set, overrides the machine's main <see cref="F:StardewValley.GameData.Machines.MachineData.InvalidCountMessage" />.</summary>
	public string InvalidCountMessage;
}
