using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.WorldMaps;

/// <summary>As part of <see cref="T:StardewValley.GameData.WorldMaps.WorldMapAreaData" />, a set of in-game locations and tile positions to match to the area.</summary>
public class WorldMapAreaPositionData
{
	/// <summary>The backing field for <see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.Id" />.</summary>
	private string IdImpl;

	/// <summary>If set, the smaller areas within this position which show a different scroll text.</summary>
	[ContentSerializer(Optional = true)]
	public List<WorldMapAreaPositionScrollTextZoneData> ScrollTextZones = new List<WorldMapAreaPositionScrollTextZoneData>();

	/// <summary>An ID for this entry within the list. This only needs to be unique within the current position list. Defaults to <see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.LocationName" />, if set.</summary>
	[ContentSerializer(Optional = true)]
	public string Id
	{
		get
		{
			return IdImpl ?? LocationName ?? LocationNames?.FirstOrDefault() ?? LocationContext;
		}
		set
		{
			IdImpl = value;
		}
	}

	/// <summary>If set, a game state query which checks whether this position should be applied. Defaults to always applied.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition { get; set; }

	/// <summary>The location context in which this world position applies.</summary>
	[ContentSerializer(Optional = true)]
	public string LocationContext { get; set; }

	/// <summary>The location name to which this world position applies. Any location within the mines and the Skull Cavern will be <c>Mines</c> and <c>SkullCave</c> respectively, and festivals use the map asset name (e.g. <c>Town-EggFestival</c>).</summary>
	[ContentSerializer(Optional = true)]
	public string LocationName { get; set; }

	/// <summary>A list of location names in which this world position applies (see <see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.LocationName" /> for details).</summary>
	[ContentSerializer(Optional = true)]
	public List<string> LocationNames { get; set; } = new List<string>();


	/// <summary>The tile area for the zone within the in-game location, or an empty rectangle for the entire map.</summary>
	/// <remarks><see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.TileArea" /> and <see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.MapPixelArea" /> are used to calculate the position of a player within the map view, given their real position in-game. For example, let's say an area has tile positions (0, 0) through (10, 20), and map pixel positions (200, 200) through (300, 400). If the player is standing on tile (5, 10) in-game (in the exact middle of the location), the game would place their marker at pixel (250, 300) on the map (in the exact middle of the map area).</remarks>
	[ContentSerializer(Optional = true)]
	public Rectangle TileArea { get; set; }

	/// <summary>The tile area within which the player is considered to be within the zone, even if they're beyond the <see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.TileArea" />. Positions outside the <see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.TileArea" /> will be snapped to the nearest valid position.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle? ExtendedTileArea { get; set; }

	/// <summary>The pixel coordinates for the image area on the map.</summary>
	/// <remarks>See remarks on <see cref="P:StardewValley.GameData.WorldMaps.WorldMapAreaPositionData.TileArea" />.</remarks>
	[ContentSerializer(Optional = true)]
	public Rectangle MapPixelArea { get; set; }

	/// <summary>A tokenizable string for the scroll text shown at the bottom of the map when the player is in this area. Defaults to <see cref="F:StardewValley.GameData.WorldMaps.WorldMapAreaData.ScrollText" />.</summary>
	[ContentSerializer(Optional = true)]
	public string ScrollText { get; set; }
}
