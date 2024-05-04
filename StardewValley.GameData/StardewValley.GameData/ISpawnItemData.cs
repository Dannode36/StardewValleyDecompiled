using System.Collections.Generic;

namespace StardewValley.GameData;

/// <summary>A data entry which specifies item data to create.</summary>
public interface ISpawnItemData
{
	/// <summary>The item(s) to create. This can be either a qualified item ID, or an item query like <c>ALL_ITEMS</c>.</summary>
	string ItemId { get; set; }

	/// <summary>A list of random item IDs to choose from, using the same format as <see cref="P:StardewValley.GameData.ISpawnItemData.ItemId" />. If set, <see cref="P:StardewValley.GameData.ISpawnItemData.ItemId" /> is ignored.</summary>
	List<string> RandomItemId { get; set; }

	/// <summary>The maximum number of item stacks to produce, or <c>null</c> to include all stacks produced by <see cref="P:StardewValley.GameData.ISpawnItemData.ItemId" /> or <see cref="P:StardewValley.GameData.ISpawnItemData.RandomItemId" />.</summary>
	int? MaxItems { get; set; }

	/// <summary>The minimum stack size for the item to create, or <c>-1</c> to keep the default value.</summary>
	/// <remarks>A value in the <see cref="P:StardewValley.GameData.ISpawnItemData.MinStack" /> to <see cref="P:StardewValley.GameData.ISpawnItemData.MaxStack" /> range is chosen randomly. If the maximum is lower than the minimum, the stack is set to <see cref="P:StardewValley.GameData.ISpawnItemData.MinStack" />.</remarks>
	int MinStack { get; set; }

	/// <summary>The maximum stack size for the item to create, or <c>-1</c> to match <see cref="P:StardewValley.GameData.ISpawnItemData.MinStack" />.</summary>
	/// <remarks><inheritdoc cref="P:StardewValley.GameData.ISpawnItemData.MinStack" select="/Remarks" /></remarks>
	int MaxStack { get; set; }

	/// <summary>The quality of the item to create. One of <c>0</c> (normal), <c>1</c> (silver), <c>2</c> (gold), <c>4</c> (iridium), or <c>-1</c> (keep the quality as-is).</summary>
	int Quality { get; set; }

	/// <summary>For objects only, the internal name to set (or <c>null</c> for the item's name in data). This should usually be null.</summary>
	string ObjectInternalName { get; set; }

	/// <summary>For objects only, a tokenizable string for the display name to show (or <c>null</c> for the item's default display name). See remarks on <c>Object.displayNameFormat</c>.</summary>
	string ObjectDisplayName { get; set; }

	/// <summary>For tool items only, the initial upgrade level, or <c>-1</c> to keep the default value.</summary>
	int ToolUpgradeLevel { get; set; }

	/// <summary>Whether to add the crafting/cooking recipe for the item, instead of the item itself.</summary>
	bool IsRecipe { get; set; }

	/// <summary>Changes to apply to the result of <see cref="P:StardewValley.GameData.ISpawnItemData.MinStack" /> and <see cref="P:StardewValley.GameData.ISpawnItemData.MaxStack" />.</summary>
	List<QuantityModifier> StackModifiers { get; set; }

	/// <summary>How multiple <see cref="P:StardewValley.GameData.ISpawnItemData.StackModifiers" /> should be combined.</summary>
	QuantityModifier.QuantityModifierMode StackModifierMode { get; set; }

	/// <summary>Changes to apply to the <see cref="P:StardewValley.GameData.ISpawnItemData.Quality" />.</summary>
	/// <remarks>These operate on the numeric quality values (i.e. <c>0</c> = normal, <c>1</c> = silver, <c>2</c> = gold, and <c>4</c> = iridium). For example, silver × 2 is gold.</remarks>
	List<QuantityModifier> QualityModifiers { get; set; }

	/// <summary>How multiple <see cref="P:StardewValley.GameData.ISpawnItemData.QualityModifiers" /> should be combined.</summary>
	QuantityModifier.QuantityModifierMode QualityModifierMode { get; set; }

	/// <summary>Custom metadata to add to the created item's <c>modData</c> field for mod use.</summary>
	Dictionary<string, string> ModData { get; set; }

	/// <summary>A game state query which indicates whether an item produced from the other fields should be returned (e.g. to filter results from item queries like <c>ALL_ITEMS</c>). Defaults to always true.</summary>
	string PerItemCondition { get; set; }
}
