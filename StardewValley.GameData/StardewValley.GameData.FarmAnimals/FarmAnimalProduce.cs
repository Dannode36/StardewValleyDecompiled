using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.FarmAnimals;

/// <summary>As part of <see cref="T:StardewValley.GameData.FarmAnimals.FarmAnimalData" />, an item that can be produced by the animal when it's an adult.</summary>
public class FarmAnimalProduce
{
	/// <summary>An ID for this entry within the produce list. This only needs to be unique within the current list.</summary>
	[ContentSerializer(Optional = true)]
	public string Id { get; set; }

	/// <summary>A game state query which indicates whether this item can be produced now. Defaults to always true.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition { get; set; }

	/// <summary>The minimum friendship points with the animal needed to produce this item.</summary>
	[ContentSerializer(Optional = true)]
	public int MinimumFriendship { get; set; }

	/// <summary>The <strong>unqualified</strong> object ID of the item to produce.</summary>
	public string ItemId { get; set; }
}
