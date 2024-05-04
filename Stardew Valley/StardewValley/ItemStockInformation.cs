using System.Collections.Generic;
using StardewValley.GameData.Shops;

namespace StardewValley;

public struct ItemStockInformation
{
	public int Price;

	public int Stock;

	public string TradeItem;

	public int? TradeItemCount;

	public LimitedStockMode LimitedStockMode;

	public string SyncedKey;

	/// <summary>If set, the stack count will be synchronized with the given item's. This is very specialized and only used for objects whose available stock are tracked separately from the normal shop stock tracking.</summary>
	public ISalable ItemToSyncStack;

	/// <summary>Override how the item's stack number is drawn in the shop menu, or <c>null</c> for the default behavior.</summary>
	public StackDrawType? StackDrawType;

	/// <summary>The actions to perform when the item is purchased.</summary>
	public List<string> ActionsOnPurchase;

	public ItemStockInformation(int price, int stock, string tradeItem = null, int? tradeItemCount = null, LimitedStockMode stockMode = LimitedStockMode.Global, string syncedKey = null, ISalable itemToSyncStack = null, StackDrawType? stackDrawType = null, List<string> actionsOnPurchase = null)
	{
		Price = price;
		Stock = stock;
		TradeItem = tradeItem;
		TradeItemCount = tradeItemCount;
		LimitedStockMode = stockMode;
		SyncedKey = syncedKey;
		ItemToSyncStack = itemToSyncStack;
		StackDrawType = stackDrawType;
		ActionsOnPurchase = actionsOnPurchase;
	}
}
