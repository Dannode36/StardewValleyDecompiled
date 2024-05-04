using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.WorldMaps;

/// <summary>As part of a larger <see cref="T:StardewValley.GameData.WorldMaps.WorldMapAreaData" />, an image overlay to apply to the map.</summary>
public class WorldMapTextureData
{
	/// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_OverlayId</c>.</summary>
	public string Id;

	/// <summary>If set, a game state query which checks whether the overlay should be applied. Defaults to always applied.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>The asset name for the texture to draw when the area is applied to the map.</summary>
	public string Texture;

	/// <summary>The pixel area within the <see cref="F:StardewValley.GameData.WorldMaps.WorldMapTextureData.Texture" /> to draw, or an empty rectangle to draw the entire image.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle SourceRect;

	/// <summary>The pixel area within the map area to draw the texture to. If this is an empty rectangle, defaults to the entire map (for a base texture) or <see cref="F:StardewValley.GameData.WorldMaps.WorldMapAreaData.PixelArea" /> (for a map area texture).</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle MapPixelArea;
}
