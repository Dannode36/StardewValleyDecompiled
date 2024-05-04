using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Buildings;

/// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, a tile to treat as part of the building when placing it through a construction menu.</summary>
public class BuildingPlacementTile
{
	/// <summary>The tile positions relative to the top-left corner of the building.</summary>
	public Rectangle TileArea;

	/// <summary>Whether this area allows tiles that would normally not be buildable, so long as they are passable. For example, this is used to ensure that an entrance is accessible.</summary>
	[ContentSerializer(Optional = true)]
	public bool OnlyNeedsToBePassable;
}
