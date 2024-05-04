using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Locations;

/// <summary>As part of <see cref="T:StardewValley.GameData.Locations.LocationData" />, an item that can be found by digging an artifact dig spot.</summary>
/// <remarks>Only one item can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.</remarks>
public class ArtifactSpotDropData : GenericSpawnItemDataWithCondition
{
	/// <summary>A probability that this item will be found, as a value between 0 (never) and 1 (always).</summary>
	[ContentSerializer(Optional = true)]
	public double Chance { get; set; } = 1.0;


	/// <summary>Whether the item may drop twice if the player is using a hoe with the Generous enchantment.</summary>
	[ContentSerializer(Optional = true)]
	public bool ApplyGenerousEnchantment { get; set; } = true;


	/// <summary>Whether to split the dropped item stack into multiple floating debris that each have a stack size of one.</summary>
	[ContentSerializer(Optional = true)]
	public bool OneDebrisPerDrop { get; set; } = true;


	/// <summary>The order in which this drop should be checked, where 0 is the default value used by most drops. Drops within each precedence group are checked in the order listed.</summary>
	[ContentSerializer(Optional = true)]
	public int Precedence { get; set; }

	/// <summary>Whether to continue searching for more items after this item is dropped, so the artifact spot may drop multiple items.</summary>
	[ContentSerializer(Optional = true)]
	public bool ContinueOnDrop { get; set; }
}
