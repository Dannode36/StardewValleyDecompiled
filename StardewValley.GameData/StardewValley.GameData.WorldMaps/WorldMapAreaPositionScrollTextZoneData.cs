using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.WorldMaps;

/// <summary>As part of <see cref="T:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData" />, a smaller area within this position which shows a different scroll text.</summary>
public class WorldMapAreaPositionScrollTextZoneData
{
	/// <summary>An ID for this entry within the list. This only needs to be unique within the current position list.</summary>
	public string Id;

	/// <summary>The pixel coordinates for the image area on the map.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle TileArea;

	/// <summary>A tokenizable string for the scroll text shown at the bottom of the map when the player is in this area. Defaults to <see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.ScrollText" />.</summary>
	[ContentSerializer(Optional = true)]
	public string ScrollText;
}
