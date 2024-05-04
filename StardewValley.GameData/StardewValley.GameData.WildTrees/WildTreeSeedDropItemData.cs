using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.WildTrees;

/// <summary>As part of <see cref="T:StardewValley.GameData.WildTrees.WildTreeData" />, a possible item to produce when dropping the tree seed.</summary>
public class WildTreeSeedDropItemData : WildTreeItemData
{
	/// <summary>If this item is dropped, whether to continue as if it hadn't been dropped for the remaining drop candidates.</summary>
	[ContentSerializer(Optional = true)]
	public bool ContinueOnDrop { get; set; }
}
