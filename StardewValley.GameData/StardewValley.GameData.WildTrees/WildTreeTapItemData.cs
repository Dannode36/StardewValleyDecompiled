using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.WildTrees;

/// <summary>As part of <see cref="T:StardewValley.GameData.WildTrees.WildTreeData" />, a possible item to produce for tappers on the tree.</summary>
public class WildTreeTapItemData : WildTreeItemData
{
	/// <summary>If set, the group only applies if the previous item produced by the tapper matches one of these qualified or unqualified item IDs (including <c>null</c> for the initial tap).</summary>
	[ContentSerializer(Optional = true)]
	public List<string> PreviousItemId { get; set; }

	/// <summary>The number of days before the tapper is ready to empty.</summary>
	public int DaysUntilReady { get; set; }

	/// <summary>Changes to apply to the result of <see cref="P:StardewValley.GameData.WildTrees.WildTreeTapItemData.DaysUntilReady" />.</summary>
	[ContentSerializer(Optional = true)]
	public List<QuantityModifier> DaysUntilReadyModifiers { get; set; }

	/// <summary>How multiple <see cref="P:StardewValley.GameData.WildTrees.WildTreeTapItemData.DaysUntilReadyModifiers" /> should be combined.</summary>
	[ContentSerializer(Optional = true)]
	public QuantityModifier.QuantityModifierMode DaysUntilReadyModifierMode { get; set; }
}
