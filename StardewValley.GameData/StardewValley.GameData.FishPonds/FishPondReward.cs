using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.FishPonds;

/// <summary>As part of <see cref="T:StardewValley.GameData.FishPonds.FishPondData" />, an item that can be produced by the fish pond.</summary>
public class FishPondReward
{
	/// <summary>The minimum population needed before this output becomes available.</summary>
	[ContentSerializer(Optional = true)]
	public int RequiredPopulation;

	/// <summary>The percentage chance that this output is selected, as a value between 0 (never) and 1 (always).If multiple items pass, only the first one will be produced.</summary>
	[ContentSerializer(Optional = true)]
	public float Chance = 1f;

	/// <summary>The item ID or item query to produce.</summary>
	public string ItemId;

	/// <summary>The minimum number of <see cref="F:StardewValley.GameData.FishPonds.FishPondReward.ItemId" /> to produce.</summary>
	[ContentSerializer(Optional = true)]
	public int MinQuantity = 1;

	/// <summary>The maximum number of <see cref="F:StardewValley.GameData.FishPonds.FishPondReward.ItemId" /> to produce.</summary>
	[ContentSerializer(Optional = true)]
	public int MaxQuantity = 1;
}
