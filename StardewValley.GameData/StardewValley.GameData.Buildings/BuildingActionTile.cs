using Microsoft.Xna.Framework;

namespace StardewValley.GameData.Buildings;

/// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, a tile which the player can click to trigger an <c>Action</c> map tile property.</summary>
public class BuildingActionTile
{
	/// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries, this should be prefixed with your mod ID like <c>Example.ModId_Id</c>.</summary>
	public string Id;

	/// <summary>The tile position, relative to the building's top-left corner tile.</summary>
	public Point Tile;

	/// <summary>The tokenizable string for the action to perform, excluding the <c>Action</c> prefix. For example, <c>"Dialogue Hi there @!"</c> to show a message box like <c>"Hi there {player name}!"</c>.</summary>
	public string Action;
}
