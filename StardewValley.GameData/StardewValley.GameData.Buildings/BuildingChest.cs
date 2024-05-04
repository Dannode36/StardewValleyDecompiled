using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Buildings;

/// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, an input/output inventory that can be accessed from a tile on the building exterior.</summary>
public class BuildingChest
{
	/// <summary>A key for this chest, referenced from the <see cref="F:StardewValley.GameData.Buildings.BuildingData.ItemConversions" /> field. Each chest must have a unique name within one building's chest list (but they don't need to be globally unique).</summary>
	public string Id;

	/// <summary>The inventory type.</summary>
	public BuildingChestType Type;

	/// <summary>The sound to play once when the player clicks the chest.</summary>
	[ContentSerializer(Optional = true)]
	public string Sound;

	/// <summary>A tokenizable string to show when the player tries to add an item to the chest when it isn't a supported item.</summary>
	[ContentSerializer(Optional = true)]
	public string InvalidItemMessage;

	/// <summary>An extra condition that must be met before <see cref="F:StardewValley.GameData.Buildings.BuildingChest.InvalidItemMessage" /> is shown.</summary>
	[ContentSerializer(Optional = true)]
	public string InvalidItemMessageCondition;

	/// <summary>A tokenizable string to show when the player tries to add an item to the chest when they don't have enough in their inventory.</summary>
	[ContentSerializer(Optional = true)]
	public string InvalidCountMessage;

	/// <summary>A tokenizable string to show when the player tries to add an item to the chest when the chest has no more room to accept it.</summary>
	[ContentSerializer(Optional = true)]
	public string ChestFullMessage;

	/// <summary>The chest's position on the building exterior, measured in tiles from the top-left corner of the building. This affects the position of the 'item ready to collect' bubble. If omitted, the bubble is disabled.</summary>
	[ContentSerializer(Optional = true)]
	public Vector2 DisplayTile = new Vector2(-1f, -1f);

	/// <summary>If <see cref="F:StardewValley.GameData.Buildings.BuildingChest.DisplayTile" /> is set, the chest's tile height.</summary>
	[ContentSerializer(Optional = true)]
	public float DisplayHeight;
}
