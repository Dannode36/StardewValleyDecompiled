using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Movies;

public class MovieCranePrizeData : GenericSpawnItemDataWithCondition
{
	/// <summary>The rarity list to update. This can be 1 (common), 2 (rare), or 3 (deluxe).</summary>
	[ContentSerializer(Optional = true)]
	public int Rarity { get; set; } = 1;

}
