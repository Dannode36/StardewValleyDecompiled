using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Shops;

/// <summary>As part of <see cref="T:StardewValley.GameData.Shops.ShopData" />, an item to add to the shop inventory.</summary>
public class ShopItemData : GenericSpawnItemDataWithCondition
{
	/// <summary>The actions to perform when the item is purchased.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> ActionsOnPurchase;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;

	/// <summary>The qualified or unqualified item ID which must be traded to purchase this item.</summary>
	/// <remarks>If both <see cref="P:StardewValley.GameData.Shops.ShopItemData.TradeItemId" /> and <see cref="P:StardewValley.GameData.Shops.ShopItemData.Price" /> are specified, the player will need to provide both to get the item.</remarks>
	[ContentSerializer(Optional = true)]
	public string TradeItemId { get; set; }

	/// <summary>The number of <see cref="P:StardewValley.GameData.Shops.ShopItemData.TradeItemId" /> needed to purchase this item.</summary>
	[ContentSerializer(Optional = true)]
	public int TradeItemAmount { get; set; } = 1;


	/// <summary>The gold price to purchase the item from the shop. Defaults to the item's normal price, or zero if <see cref="P:StardewValley.GameData.Shops.ShopItemData.TradeItemId" /> is specified.</summary>
	/// <remarks>If both <see cref="P:StardewValley.GameData.Shops.ShopItemData.TradeItemId" /> and <see cref="P:StardewValley.GameData.Shops.ShopItemData.Price" /> are specified, the player will need to provide both to get the item.</remarks>
	[ContentSerializer(Optional = true)]
	public int Price { get; set; } = -1;


	/// <summary>Whether to multiply the price by the game's profit margins, which reduces the price on easier difficulty settings. This can be true (always apply it), false (never apply it), or null (apply to certain items like saplings). This is applied before any quantity modifiers. Default null.</summary>
	[ContentSerializer(Optional = true)]
	public bool? ApplyProfitMargins { get; set; }

	/// <summary>The maximum number of the item which can be purchased in one day. Default unlimited.</summary>
	[ContentSerializer(Optional = true)]
	public int AvailableStock { get; set; } = -1;


	/// <summary>If <see cref="P:StardewValley.GameData.Shops.ShopItemData.AvailableStock" /> is set, how the limit is applied in multiplayer. This has no effect on recipes.</summary>
	[ContentSerializer(Optional = true)]
	public LimitedStockMode AvailableStockLimit { get; set; }

	/// <summary>Whether to avoid adding this item to the shop if it would duplicate one that was already added. If the item is randomized, this will choose a value that hasn't already been added to the shop if possible.</summary>
	[ContentSerializer(Optional = true)]
	public bool AvoidRepeat { get; set; }

	/// <summary>If this data produces an object and <see cref="P:StardewValley.GameData.Shops.ShopItemData.Price" /> is -1, whether to use the raw price in <c>Data/Objects</c> instead of the calculated sell-to-player price.</summary>
	[ContentSerializer(Optional = true)]
	public bool UseObjectDataPrice { get; set; }

	/// <summary>Whether to ignore the <see cref="F:StardewValley.GameData.Shops.ShopData.PriceModifiers" /> for the shop. This has no effect on the item's <see cref="P:StardewValley.GameData.Shops.ShopItemData.PriceModifiers" />. Default false.</summary>
	[ContentSerializer(Optional = true)]
	public bool IgnoreShopPriceModifiers { get; set; }

	/// <summary>Changes to apply to the <see cref="P:StardewValley.GameData.Shops.ShopItemData.Price" />. These stack with <see cref="F:StardewValley.GameData.Shops.ShopData.PriceModifiers" />.</summary>
	/// <remarks>If multiple entries match, they'll be applied sequentially (e.g. two matching rules to double the price will quadruple it).</remarks>
	[ContentSerializer(Optional = true)]
	public List<QuantityModifier> PriceModifiers { get; set; }

	/// <summary>How multiple <see cref="P:StardewValley.GameData.Shops.ShopItemData.PriceModifiers" /> should be combined.</summary>
	[ContentSerializer(Optional = true)]
	public QuantityModifier.QuantityModifierMode PriceModifierMode { get; set; }

	/// <summary>Changes to apply to the <see cref="P:StardewValley.GameData.Shops.ShopItemData.AvailableStock" />.</summary>
	/// <remarks>If multiple entries match, they'll be applied sequentially (e.g. two matching rules to double the available stock will quadruple it).</remarks>
	[ContentSerializer(Optional = true)]
	public List<QuantityModifier> AvailableStockModifiers { get; set; }

	/// <summary>How multiple <see cref="P:StardewValley.GameData.Shops.ShopItemData.AvailableStockModifiers" /> should be combined.</summary>
	[ContentSerializer(Optional = true)]
	public QuantityModifier.QuantityModifierMode AvailableStockModifierMode { get; set; }
}
