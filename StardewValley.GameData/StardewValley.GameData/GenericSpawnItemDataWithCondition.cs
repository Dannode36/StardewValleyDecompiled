using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>The data for an item to create with support for a game state query, used in data assets like <see cref="T:StardewValley.GameData.Machines.MachineData" /> or <see cref="T:StardewValley.GameData.Shops.ShopData" />.</summary>
public class GenericSpawnItemDataWithCondition : GenericSpawnItemData
{
	/// <summary>A game state query which indicates whether the item should be added. Defaults to always added.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition { get; set; }
}
