using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Buildings;

/// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, a map tile property to set.</summary>
public class BuildingTileProperty
{
	/// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_Id</c>.</summary>
	public string Id;

	/// <summary>The tile property name to set.</summary>
	public string Name;

	/// <summary>The tile property value to set.</summary>
	[ContentSerializer(Optional = true)]
	public string Value;

	/// <summary>The name of the map layer whose tiles to change.</summary>
	public string Layer;

	/// <summary>The tiles to which to add the property.</summary>
	public Rectangle TileArea;
}
