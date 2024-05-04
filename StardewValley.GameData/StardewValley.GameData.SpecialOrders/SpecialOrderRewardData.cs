using System.Collections.Generic;

namespace StardewValley.GameData.SpecialOrders;

/// <summary>As part of <see cref="T:StardewValley.GameData.SpecialOrders.SpecialOrderData" />, a reward given to the player when they complete this special order..</summary>
public class SpecialOrderRewardData
{
	/// <summary>The name of the C# class which handles the logic for this reward.</summary>
	/// <remarks>The class must be in the <c>StardewValley</c> namespace, and its name must end with <c>Reward</c> (without including it in this field). For example, <c>"Money"</c> will match the <c>StardewValley.MoneyReward</c> type.</remarks>
	public string Type;

	/// <summary>The arbitrary data values understood by the C# class identified by <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderRewardData.Type" />. These may or may not allow <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements" /> tokens, depending on the class.</summary>
	public Dictionary<string, string> Data;
}
