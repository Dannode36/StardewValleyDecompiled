using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.WorldMaps;

/// <summary>A large-scale part of the world like the Valley, containing all the areas drawn together as part of the combined map view.</summary>
public class WorldMapRegionData
{
	/// <summary>The base texture to draw as the base texture, if any. The first matching texture is applied.</summary>
	public List<WorldMapTextureData> BaseTexture = new List<WorldMapTextureData>();

	/// <summary>Maps neighbor IDs for controller support in fields like <see cref="F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.LeftNeighbor" /> to the specific values to use. This allows using simplified IDs like <c>Beach/FishShop</c> instead of <c>Beach/FishShop_DefaultHours, Beach/FishShop_ExtendedHours</c>. Aliases cannot be recursive.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> MapNeighborIdAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

	/// <summary>The areas to draw on top of the <see cref="F:StardewValley.GameData.WorldMaps.WorldMapRegionData.BaseTexture" />. These can provide tooltips, scroll text, and character marker positioning data.</summary>
	public List<WorldMapAreaData> MapAreas = new List<WorldMapAreaData>();
}
