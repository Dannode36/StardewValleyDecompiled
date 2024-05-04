using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.GarbageCans;

/// <summary>As part of <see cref="T:StardewValley.GameData.GarbageCans.GarbageCanData" />, an item that can be found by rummaging in the garbage can.</summary>
/// <remarks>Only one item can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.</remarks>
public class GarbageCanItemData : GenericSpawnItemDataWithCondition
{
	/// <summary>Whether to check this item even if the <see cref="F:StardewValley.GameData.GarbageCans.GarbageCanEntryData.BaseChance" /> didn't pass.</summary>
	[ContentSerializer(Optional = true)]
	public bool IgnoreBaseChance { get; set; }

	/// <summary>Whether to treat this item as a 'mega success' if it's selected, which plays a special <c>crit</c> sound and bigger animation.</summary>
	[ContentSerializer(Optional = true)]
	public bool IsMegaSuccess { get; set; }

	/// <summary>Whether to treat this item as an 'double mega success' if it's selected, which plays an explosion sound and dramatic animation.</summary>
	[ContentSerializer(Optional = true)]
	public bool IsDoubleMegaSuccess { get; set; }

	/// <summary>Whether to add the item to the player's inventory directly, opening an item grab menu if they don't have room in their inventory. If false, the item will be dropped on the ground next to the garbage can instead.</summary>
	[ContentSerializer(Optional = true)]
	public bool AddToInventoryDirectly { get; set; }

	/// <summary>Whether to splits stacks into multiple debris items, instead of a single item with a stack size.</summary>
	[ContentSerializer(Optional = true)]
	public bool CreateMultipleDebris { get; set; }
}
