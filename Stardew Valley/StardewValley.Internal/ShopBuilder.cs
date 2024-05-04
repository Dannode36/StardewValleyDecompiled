using System;
using System.Collections.Generic;
using System.Linq;
using StardewValley.Extensions;
using StardewValley.GameData.Shops;
using StardewValley.GameData.Tools;

namespace StardewValley.Internal;

/// <summary>Handles building a shop menu from data in <c>Data/Shops</c>.</summary>
/// <remarks>This is an internal implementation class. Most code should use <see cref="M:StardewValley.Utility.TryOpenShopMenu(System.String,System.String,System.Boolean)" /> instead.</remarks>
public static class ShopBuilder
{
	/// <summary>Get the inventory to sell for a shop menu.</summary>
	/// <param name="shopId">The shop ID matching the entry in <c>Data/Shops</c>.</param>
	public static Dictionary<ISalable, ItemStockInformation> GetShopStock(string shopId)
	{
		if (DataLoader.Shops(Game1.content).TryGetValue(shopId, out var shop))
		{
			return GetShopStock(shopId, shop);
		}
		return new Dictionary<ISalable, ItemStockInformation>();
	}

	/// <summary>Get the inventory to sell for a shop menu.</summary>
	/// <param name="shopId">The shop ID in <c>Data\Shops</c>.</param>
	/// <param name="shop">The shop data from <c>Data\Shops</c>.</param>
	public static Dictionary<ISalable, ItemStockInformation> GetShopStock(string shopId, ShopData shop)
	{
		Dictionary<ISalable, ItemStockInformation> stock = new Dictionary<ISalable, ItemStockInformation>();
		List<ShopItemData> items = shop.Items;
		if (items != null && items.Count > 0)
		{
			Random shopRandom = Utility.CreateDaySaveRandom();
			HashSet<string> stockedItemIds = new HashSet<string>();
			ItemQueryContext itemQueryContext = new ItemQueryContext(Game1.currentLocation, Game1.player, shopRandom);
			bool applyPierreStockList = shopId == "SeedShop" && Game1.MasterPlayer.hasOrWillReceiveMail("PierreStocklist");
			HashSet<string> syncKeys = new HashSet<string>();
			foreach (ShopItemData itemData in shop.Items)
			{
				if (!syncKeys.Add(itemData.Id))
				{
					Game1.log.Warn($"Shop {shopId} has multiple items with entry ID '{itemData.Id}'. This may cause unintended behavior.");
				}
				if (!CheckItemCondition(itemData.Condition, applyPierreStockList, out var isItemOutOfSeason))
				{
					continue;
				}
				IList<ItemQueryResult> list = ItemQueryResolver.TryResolve(itemData, itemQueryContext, ItemQuerySearchMode.All, itemData.AvoidRepeat, itemData.AvoidRepeat ? stockedItemIds : null, null, delegate(string query, string message)
				{
					Game1.log.Error($"Failed parsing shop item query '{query}' for the '{shopId}' shop: {message}.");
				});
				int i = 0;
				foreach (ItemQueryResult shopItem in list)
				{
					ISalable item = shopItem.Item;
					item.Stack = shopItem.OverrideStackSize ?? item.Stack;
					float price = GetBasePrice(shopItem, shop, itemData, item, isItemOutOfSeason, itemData.UseObjectDataPrice);
					int availableStock = shopItem.OverrideShopAvailableStock ?? itemData.AvailableStock;
					LimitedStockMode availableStockLimit = itemData.AvailableStockLimit;
					string tradeItemId = shopItem.OverrideTradeItemId ?? itemData.TradeItemId;
					int? tradeItemAmount = ((shopItem.OverrideTradeItemAmount > 0) ? shopItem.OverrideTradeItemAmount : new int?(itemData.TradeItemAmount));
					if (tradeItemId == null || tradeItemAmount < 0)
					{
						tradeItemId = null;
						tradeItemAmount = null;
					}
					if (itemData.IsRecipe)
					{
						item.Stack = 1;
						availableStockLimit = LimitedStockMode.None;
						availableStock = 1;
					}
					if (!itemData.IgnoreShopPriceModifiers)
					{
						price = Utility.ApplyQuantityModifiers(price, shop.PriceModifiers, shop.PriceModifierMode, null, null, item as Item, null, shopRandom);
					}
					price = Utility.ApplyQuantityModifiers(price, itemData.PriceModifiers, itemData.PriceModifierMode, null, null, item as Item, null, shopRandom);
					if (!itemData.IsRecipe)
					{
						availableStock = (int)Utility.ApplyQuantityModifiers(availableStock, itemData.AvailableStockModifiers, itemData.AvailableStockModifierMode, null, null, item as Item, null, shopRandom);
					}
					if (!TrackSeenItems(stockedItemIds, item) || !itemData.AvoidRepeat)
					{
						if (availableStock < 0)
						{
							availableStock = int.MaxValue;
						}
						string syncKey = itemData.Id;
						if (++i > 1)
						{
							syncKey += i;
						}
						int price2 = (int)price;
						int stock2 = availableStock;
						string tradeItem = tradeItemId;
						int? tradeItemCount = tradeItemAmount;
						LimitedStockMode stockMode = availableStockLimit;
						string syncedKey = syncKey;
						Item syncStacksWith = shopItem.SyncStacksWith;
						List<string> actionsOnPurchase = itemData.ActionsOnPurchase;
						stock.Add(item, new ItemStockInformation(price2, stock2, tradeItem, tradeItemCount, stockMode, syncedKey, syncStacksWith, null, actionsOnPurchase));
					}
				}
			}
		}
		Game1.player.team.synchronizedShopStock.UpdateLocalStockWithSyncedQuanitities(shopId, stock);
		return stock;
	}

	/// <summary>Check a game state query which determines whether an item should be added to a shop menu.</summary>
	/// <param name="conditions">The conditions to check.</param>
	/// <param name="applyPierreMissingStockList">Whether to apply Pierre's Missing Stock List, which allows buying out-of-season crops.</param>
	/// <param name="isOutOfSeason">Whether this is an out-of-season item which is allowed (for a price) because the player found Pierre's Stock List.</param>
	public static bool CheckItemCondition(string conditions, bool applyPierreMissingStockList, out bool isOutOfSeason)
	{
		if (conditions == null || GameStateQuery.CheckConditions(conditions))
		{
			isOutOfSeason = false;
			return true;
		}
		if (applyPierreMissingStockList && GameStateQuery.CheckConditions(conditions, null, null, null, null, null, GameStateQuery.SeasonQueryKeys))
		{
			isOutOfSeason = true;
			return true;
		}
		isOutOfSeason = false;
		return false;
	}

	/// <summary>Get the tool upgrade data to show in the blacksmith shop for a given tool, if any.</summary>
	/// <param name="tool">The tool data to show as an upgrade, if possible.</param>
	/// <param name="player">The player viewing the shop.</param>
	public static ToolUpgradeData GetToolUpgradeData(ToolData tool, Farmer player)
	{
		if (tool == null)
		{
			return null;
		}
		IList<ToolUpgradeData> upgradeFrom = tool.UpgradeFrom;
		if (tool.ConventionalUpgradeFrom != null)
		{
			IList<ToolUpgradeData> conventional = new ToolUpgradeData[1]
			{
				new ToolUpgradeData
				{
					RequireToolId = tool.ConventionalUpgradeFrom,
					Price = GetToolUpgradeConventionalPrice(tool.UpgradeLevel),
					TradeItemId = GetToolUpgradeConventionalTradeItem(tool.UpgradeLevel),
					TradeItemAmount = 5
				}
			};
			IList<ToolUpgradeData> list;
			if (upgradeFrom == null || upgradeFrom.Count <= 0)
			{
				list = conventional;
			}
			else
			{
				IList<ToolUpgradeData> list2 = conventional.Concat(upgradeFrom).ToList();
				list = list2;
			}
			upgradeFrom = list;
		}
		if (upgradeFrom == null)
		{
			return null;
		}
		foreach (ToolUpgradeData upgrade in upgradeFrom)
		{
			if ((upgrade.Condition == null || GameStateQuery.CheckConditions(upgrade.Condition, player.currentLocation, player)) && (upgrade.RequireToolId == null || player.Items.ContainsId(upgrade.RequireToolId)))
			{
				return upgrade;
			}
		}
		return null;
	}

	/// <summary>Get the conventional price for a tool upgrade.</summary>
	/// <param name="level">The level to which the tool is being upgraded.</param>
	public static int GetToolUpgradeConventionalPrice(int level)
	{
		return level switch
		{
			1 => 2000, 
			2 => 5000, 
			3 => 10000, 
			4 => 25000, 
			_ => 2000, 
		};
	}

	/// <summary>Get the unqualified item ID for the conventional material that must be provided for a tool upgrade.</summary>
	/// <param name="level">The level to which the tool is being upgraded.</param>
	private static string GetToolUpgradeConventionalTradeItem(int level)
	{
		return level switch
		{
			1 => "334", 
			2 => "335", 
			3 => "336", 
			4 => "337", 
			_ => "334", 
		};
	}

	/// <summary>Get the owner entries for a shop whose conditions currently match.</summary>
	/// <param name="shop">The shop data to check.</param>
	public static IEnumerable<ShopOwnerData> GetCurrentOwners(ShopData shop)
	{
		return shop?.Owners?.Where((ShopOwnerData owner) => GameStateQuery.CheckConditions(owner.Condition)) ?? LegacyShims.EmptyArray<ShopOwnerData>();
	}

	/// <summary>Get the sell price for a shop item, excluding quantity modifiers.</summary>
	/// <param name="output">The shop item for which to get the base price.</param>
	/// <param name="shopData">The shop data.</param>
	/// <param name="itemData">The shop item's data.</param>
	/// <param name="item">The item instance.</param>
	/// <param name="outOfSeasonPrice">Whether to apply the out-of-season pricing for Pierre's Missing Stock List.</param>
	/// <param name="useObjectDataPrice">If <paramref name="item" /> has type <see cref="F:StardewValley.ItemRegistry.type_object" />, whether to use the raw price in <c>Data/Objects</c> instead of the calculated sell-to-player price.</param>
	public static int GetBasePrice(ItemQueryResult output, ShopData shopData, ShopItemData itemData, ISalable item, bool outOfSeasonPrice, bool useObjectDataPrice = false)
	{
		float price = output.OverrideBasePrice ?? itemData.Price;
		if (price < 0f)
		{
			price = ((itemData.TradeItemId != null) ? 0f : ((!useObjectDataPrice || !item.HasTypeObject() || !(item is Object obj)) ? ((float)item.salePrice(ignoreProfitMargins: true)) : ((float)obj.Price)));
		}
		if (itemData.ApplyProfitMargins ?? shopData.ApplyProfitMargins ?? item.appliesProfitMargins())
		{
			price *= Game1.MasterPlayer.difficultyModifier;
		}
		if (outOfSeasonPrice)
		{
			price *= 1.5f;
		}
		return (int)price;
	}

	/// <summary>Add an item to the list of items already in the shop.</summary>
	/// <param name="stockedItems">The item IDs in the shop.</param>
	/// <param name="item">The item to track.</param>
	/// <returns>Returns whether the item was already in the shop.</returns>
	public static bool TrackSeenItems(HashSet<string> stockedItems, ISalable item)
	{
		string fullyQualifiedId = item.QualifiedItemId;
		if (item is Tool { UpgradeLevel: >0 } tool)
		{
			fullyQualifiedId = fullyQualifiedId + "#" + tool.UpgradeLevel;
		}
		if (item.IsRecipe)
		{
			fullyQualifiedId += "#Recipe";
		}
		return !stockedItems.Add(fullyQualifiedId);
	}
}
