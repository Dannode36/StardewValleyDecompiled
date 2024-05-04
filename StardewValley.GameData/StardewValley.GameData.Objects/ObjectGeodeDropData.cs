using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Objects;

/// <summary>As part of <see cref="T:StardewValley.GameData.Objects.ObjectData" />, an item that can be found by breaking the item as a geode.</summary>
/// <remarks>Only one item can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.</remarks>
public class ObjectGeodeDropData : GenericSpawnItemDataWithCondition
{
	/// <summary>A probability that this item will be found, as a value between 0 (never) and 1 (always).</summary>
	[ContentSerializer(Optional = true)]
	public double Chance { get; set; } = 1.0;


	/// <summary>The mail flag to set for the current player when this item is picked up by the player.</summary>
	[ContentSerializer(Optional = true)]
	public string SetFlagOnPickup { get; set; }

	/// <summary>The order in which this drop should be checked, where 0 is the default value used by most drops. Drops within each precedence group are checked in the order listed.</summary>
	[ContentSerializer(Optional = true)]
	public int Precedence { get; set; }
}
