using System.Collections.Generic;

namespace StardewValley.GameData.SpecialOrders;

/// <summary>As part of <see cref="T:StardewValley.GameData.SpecialOrders.SpecialOrderData" />, a randomized token which can be referenced by other special order fields.</summary>
/// <remarks>See remarks on <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements" /> for usage details.</remarks>
public class RandomizedElement
{
	/// <summary>The token name used to reference it.</summary>
	public string Name;

	/// <summary>The possible values to randomly choose from. If multiple values match, one is chosen randomly.</summary>
	public List<RandomizedElementItem> Values;
}
