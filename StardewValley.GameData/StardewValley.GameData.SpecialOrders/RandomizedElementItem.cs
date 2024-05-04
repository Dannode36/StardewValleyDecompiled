using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.SpecialOrders;

/// <summary>As part of <see cref="T:StardewValley.GameData.SpecialOrders.RandomizedElement" />, a possible value for the token.</summary>
public class RandomizedElementItem
{
	/// <summary>A set of hardcoded tags that check conditions like the season, received mail, etc.</summary>
	[ContentSerializer(Optional = true)]
	public string RequiredTags = "";

	/// <summary>The token value to set if this item is selected.</summary>
	public string Value = "";
}
