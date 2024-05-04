using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Shops;

/// <summary>Metadata for an in-game shop at which the player can buy and sell items.</summary>
public class ShopData
{
	/// <summary>The currency in which all items in the shop should be priced. The valid values are 0 (money), 1 (star tokens), 2 (Qi coins), and 4 (Qi gems).</summary>
	/// <remarks>For item trading, see <see cref="P:StardewValley.GameData.Shops.ShopItemData.TradeItemId" /> instead.</remarks>
	[ContentSerializer(Optional = true)]
	public int Currency;

	/// <summary>How to draw stack size numbers in the shop list by default.</summary>
	/// <remarks>This is overridden in some special cases (e.g. recipes never show a stack count).</remarks>
	[ContentSerializer(Optional = true)]
	public StackSizeVisibility? StackSizeVisibility;

	/// <summary>The sound to play when the shop menu is opened.</summary>
	[ContentSerializer(Optional = true)]
	public string OpenSound;

	/// <summary>The sound to play when an item is purchased normally.</summary>
	[ContentSerializer(Optional = true)]
	public string PurchaseSound;

	/// <summary>The repeating sound to play when accumulating a stack to purchase (e.g. by holding right-click on PC).</summary>
	[ContentSerializer(Optional = true)]
	public string PurchaseRepeatSound;

	/// <summary>Changes to apply to the sell price for all items in the shop, unless <see cref="P:StardewValley.GameData.Shops.ShopItemData.IgnoreShopPriceModifiers" /> is <c>true</c>. These stack with <see cref="P:StardewValley.GameData.Shops.ShopItemData.PriceModifiers" />.</summary>
	/// <remarks>If multiple entries match, they'll be applied sequentially (e.g. two matching rules to double the price will quadruple it).</remarks>
	[ContentSerializer(Optional = true)]
	public List<QuantityModifier> PriceModifiers;

	/// <summary>How multiple <see cref="F:StardewValley.GameData.Shops.ShopData.PriceModifiers" /> should be combined. This only affects that specific field, it won't affect price modifiers under <see cref="F:StardewValley.GameData.Shops.ShopData.Items" />.</summary>
	[ContentSerializer(Optional = true)]
	public QuantityModifier.QuantityModifierMode PriceModifierMode;

	/// <summary>The NPCs who can run the shop. If the <c>Action OpenShop</c> property specifies the <c>[owner tile area]</c> argument, at least one of the listed NPCs must be within that area; else if the <c>[owner tile area]</c> argument was omitted, the first entry in the list is used. The selected NPC's portrait will be shown in the shop UI.</summary>
	[ContentSerializer(Optional = true)]
	public List<ShopOwnerData> Owners;

	/// <summary>The visual theme to apply to the shop UI, or <c>null</c> for the default theme.</summary>
	[ContentSerializer(Optional = true)]
	public List<ShopThemeData> VisualTheme;

	/// <summary>A list of context tags for items which the player can sell to to this shop. Default none.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> SalableItemTags;

	/// <summary>The items to add to the shop inventory.</summary>
	public List<ShopItemData> Items = new List<ShopItemData>();

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;

	/// <summary>The default value for <see cref="P:StardewValley.GameData.Shops.ShopItemData.ApplyProfitMargins" />, if set. This can be true (always apply it), false (never apply it), or null (apply to certain items like saplings). This is applied before any quantity modifiers. Default null.</summary>
	[ContentSerializer(Optional = true)]
	public bool? ApplyProfitMargins { get; set; }
}
