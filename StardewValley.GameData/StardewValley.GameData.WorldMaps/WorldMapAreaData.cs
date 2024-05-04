using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.WorldMaps;

/// <summary>An area within a larger <see cref="T:StardewValley.GameData.WorldMaps.WorldMapRegionData" /> to draw onto the world map. This can provide textures, tooltips, and world positioning data.</summary>
public class WorldMapAreaData
{
	/// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_AreaId</c>.</summary>
	public string Id;

	/// <summary>If set, a game state query which checks whether the area should be applied. Defaults to always applied.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>The pixel area within the map which is covered by this area.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle PixelArea;

	/// <summary>If set, a tokenizable string for the scroll text shown at the bottom of the map when the player is in the location. Defaults to none.</summary>
	[ContentSerializer(Optional = true)]
	public string ScrollText;

	/// <summary>The image overlays to apply to the map.</summary>
	[ContentSerializer(Optional = true)]
	public List<WorldMapTextureData> Textures = new List<WorldMapTextureData>();

	/// <summary>The tooltips to show when hovering over parts of this area on the world map.</summary>
	[ContentSerializer(Optional = true)]
	public List<WorldMapTooltipData> Tooltips = new List<WorldMapTooltipData>();

	/// <summary>The in-world locations and tile coordinates to match to this map area.</summary>
	[ContentSerializer(Optional = true)]
	public List<WorldMapAreaPositionData> WorldPositions = new List<WorldMapAreaPositionData>();

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
