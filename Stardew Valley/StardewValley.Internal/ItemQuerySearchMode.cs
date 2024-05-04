namespace StardewValley.Internal;

/// <summary>The filter to apply to an item query's search results.</summary>
public enum ItemQuerySearchMode
{
	/// <summary>Return all matches.</summary>
	All,
	/// <summary>Return all matches which are a concrete <see cref="T:StardewValley.Item" /> (instead of a different <see cref="T:StardewValley.ISalable" /> type).</summary>
	AllOfTypeItem,
	/// <summary>Return the first match which is a concrete <see cref="T:StardewValley.Item" /> (instead of a different <see cref="T:StardewValley.ISalable" /> type).</summary>
	FirstOfTypeItem,
	/// <summary>Return a random match which is a concrete <see cref="T:StardewValley.Item" /> (instead of a different <see cref="T:StardewValley.ISalable" /> type).</summary>
	RandomOfTypeItem
}
