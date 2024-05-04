namespace StardewValley.GameData.Buildings;

/// <summary>The inventory type for a building chest.</summary>
public enum BuildingChestType
{
	/// <summary>A normal chest which can both provide output and accept input.</summary>
	Chest,
	/// <summary>Provides items for the player to collect. Clicking the tile will do nothing (if empty), grab the item directly (if it only contains one item), else show a grab-only inventory UI.</summary>
	Collect,
	/// <summary>Lets the player add items for the building to process.</summary>
	Load
}
