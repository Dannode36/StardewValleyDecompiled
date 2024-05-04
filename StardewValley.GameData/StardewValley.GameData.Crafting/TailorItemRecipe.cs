using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Crafting;

/// <summary>A clothing item that can be tailored from ingredients using Emily's sewing machine.</summary>
public class TailorItemRecipe
{
	/// <summary>The backing field for <see cref="P:StardewValley.GameData.Crafting.TailorItemRecipe.Id" />.</summary>
	private string IdImpl;

	/// <summary>The context tags for the first item required, or <c>null</c> for none required. If this lists multiple context tags, an item must match all of them.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> FirstItemTags;

	/// <summary>The context tags for the second item required, or <c>null</c> for none required. If this lists multiple context tags, an item must match all of them.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> SecondItemTags;

	/// <summary>Whether tailoring the item destroys the item matched by <see cref="F:StardewValley.GameData.Crafting.TailorItemRecipe.SecondItemTags" />.</summary>
	[ContentSerializer(Optional = true)]
	public bool SpendRightItem = true;

	/// <summary>The item ID to produce by default.</summary>
	/// <remarks>Ignored if <see cref="F:StardewValley.GameData.Crafting.TailorItemRecipe.CraftedItemIds" /> has any values, or for female players if <see cref="F:StardewValley.GameData.Crafting.TailorItemRecipe.CraftedItemIdFeminine" /> is set.</remarks>
	[ContentSerializer(Optional = true)]
	public string CraftedItemId;

	/// <summary>The item IDs to produce by default.</summary>
	/// <remarks>Ignored for female players if <see cref="F:StardewValley.GameData.Crafting.TailorItemRecipe.CraftedItemIdFeminine" /> is set.</remarks>
	[ContentSerializer(Optional = true)]
	public List<string> CraftedItemIds;

	/// <summary>If set, the item ID to produce if the player is female.</summary>
	[ContentSerializer(Optional = true)]
	public string CraftedItemIdFeminine;

	/// <summary>A unique identifier for this entry. This only needs to be unique within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ItemName</c>.</summary>
	[ContentSerializer(Optional = true)]
	public string Id
	{
		get
		{
			if (IdImpl != null)
			{
				return IdImpl;
			}
			List<string> craftedItemIds = CraftedItemIds;
			if (craftedItemIds != null && craftedItemIds.Any())
			{
				return string.Join(",", CraftedItemIds);
			}
			return CraftedItemId;
		}
		set
		{
			IdImpl = value;
		}
	}
}
