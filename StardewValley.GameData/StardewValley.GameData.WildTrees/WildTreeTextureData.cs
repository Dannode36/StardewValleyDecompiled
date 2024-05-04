using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.WildTrees;

/// <summary>As part of <see cref="T:StardewValley.GameData.WildTrees.WildTreeData" />, a possible spritesheet to use for the tree.</summary>
public class WildTreeTextureData
{
	/// <summary>A game state query which indicates whether this spritesheet should be applied for a tree. Defaults to always enabled.</summary>
	/// <remarks>This condition is checked when a tree's texture is loaded. Once it's loaded, the conditions won't be rechecked until the next day.</remarks>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>If set, the specific season when this texture should apply. For more complex conditions, see <see cref="F:StardewValley.GameData.WildTrees.WildTreeTextureData.Condition" />.</summary>
	[ContentSerializer(Optional = true)]
	public Season? Season;

	/// <summary>The asset name for the tree's spritesheet.</summary>
	public string Texture;
}
