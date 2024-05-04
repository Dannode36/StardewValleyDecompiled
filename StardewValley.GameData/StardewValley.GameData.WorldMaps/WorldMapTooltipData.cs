using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.WorldMaps;

/// <summary>A tooltip shown when hovering over parts of a larger <see cref="T:StardewValley.GameData.WorldMaps.WorldMapAreaData" /> on the world map.</summary>
public class WorldMapTooltipData
{
	/// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_TooltipId.</c></summary>
	public string Id;

	/// <summary>If set, a game state query which checks whether the tooltip should be visible. Defaults to always visible.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>If set, a game state query which checks whether the area is known by the player, so the <see cref="F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.Text" /> is shown as-is. If this is false, the tooltip text is replaced with '???'. Defaults to always known.</summary>
	[ContentSerializer(Optional = true)]
	public string KnownCondition;

	/// <summary>The pixel area within the map which can be hovered to show this tooltip, or an empty rectangle if it covers the entire area.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle PixelArea;

	/// <summary>A tokenizable string for the tooltip shown when the mouse is over the area.</summary>
	public string Text;

	/// <summary>The tooltip to the left of this one for controller navigation.</summary>
	/// <remarks>This should be the area and tooltip ID, formatted like <c>areaId/tooltipId</c> (not case-sensitive). If there are multiple possible neighbors, they can be specified in comma-delimited form like <c>areaId/tooltipId, areaId/tooltipId, ...</c>; the first one which exists will be used.</remarks>
	public string LeftNeighbor;

	/// <summary>The tooltip to the right of this one for controller navigation.</summary>
	/// <inheritdoc cref="F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.LeftNeighbor" path="/remarks" />
	public string RightNeighbor;

	/// <summary>The tooltip above this one for controller navigation.</summary>
	/// <inheritdoc cref="F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.LeftNeighbor" path="/remarks" />
	public string UpNeighbor;

	/// <summary>The tooltip below this one for controller navigation.</summary>
	/// <inheritdoc cref="F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.LeftNeighbor" path="/remarks" />
	public string DownNeighbor;
}
