using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.WildTrees;

/// <summary>As part of <see cref="T:StardewValley.GameData.WildTrees.WildTreeData" />, a possible item to produce.</summary>
public class WildTreeItemData : GenericSpawnItemDataWithCondition
{
	/// <summary>If set, the specific season when this data should apply. For more complex conditions, see <see cref="P:StardewValley.GameData.GenericSpawnItemDataWithCondition.Condition" />.</summary>
	[ContentSerializer(Optional = true)]
	public Season? Season { get; set; }

	/// <summary>The probability that the item will be produced, as a value between 0 (never) and 1 (always).</summary>
	[ContentSerializer(Optional = true)]
	public float Chance { get; set; } = 1f;

}
