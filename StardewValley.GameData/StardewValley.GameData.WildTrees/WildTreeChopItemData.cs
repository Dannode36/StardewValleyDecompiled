using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.WildTrees;

/// <summary>As part of <see cref="T:StardewValley.GameData.WildTrees.WildTreeData" />, a possible item to drop when the tree is chopped down.</summary>
public class WildTreeChopItemData : WildTreeItemData
{
	/// <summary>The minimum growth stage at which to produce this item.</summary>
	[ContentSerializer(Optional = true)]
	public WildTreeGrowthStage? MinSize { get; set; }

	/// <summary>The maximum growth stage at which to produce this item.</summary>
	[ContentSerializer(Optional = true)]
	public WildTreeGrowthStage? MaxSize { get; set; }

	/// <summary>Whether to drop this item if the item is a stump (true), not a stump (false), or both (null).</summary>
	[ContentSerializer(Optional = true)]
	public bool? ForStump { get; set; } = false;


	/// <summary>Get whether the given tree growth stage is valid for <see cref="P:StardewValley.GameData.WildTrees.WildTreeChopItemData.MinSize" /> and <see cref="P:StardewValley.GameData.WildTrees.WildTreeChopItemData.MaxSize" />.</summary>
	/// <param name="size">The tree growth stage.</param>
	/// <param name="isStump">Whether the tree is a stump.</param>
	public bool IsValidForGrowthStage(int size, bool isStump)
	{
		if (size == 4)
		{
			size = 3;
		}
		if ((int?)size < (int?)MinSize)
		{
			return false;
		}
		if ((int?)size > (int?)MaxSize)
		{
			return false;
		}
		if (ForStump.HasValue && ForStump != isStump)
		{
			return false;
		}
		return true;
	}
}
