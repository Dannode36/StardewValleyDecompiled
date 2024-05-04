using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Buildings;

/// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, an item to place in the building interior when it's constructed or upgraded.</summary>
public class IndoorItemAdd
{
	/// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_Id</c>.</summary>
	public string Id;

	/// <summary>The qualified item ID for the item to place.</summary>
	public string ItemId;

	/// <summary>The tile position at which to place the item.</summary>
	public Point Tile;

	/// <summary>Whether to prevent the player from destroying, picking up, or moving the item.</summary>
	[ContentSerializer(Optional = true)]
	public bool Indestructible;
}
