using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.FruitTrees;

/// <summary>Metadata for a fruit tree type.</summary>
public class FruitTreeData
{
	/// <summary>The rules which override which locations the tree can be planted in, if applicable. These don't override more specific checks (e.g. not being plantable on stone).</summary>
	[ContentSerializer(Optional = true)]
	public List<PlantableRule> PlantableLocationRules;

	/// <summary>A tokenizable string for the fruit tree display name, like 'Cherry' for a cherry tree.</summary>
	/// <remarks>This shouldn't include 'tree', which will be added automatically as needed.</remarks>
	public string DisplayName { get; set; }

	/// <summary>The seasons in which this tree bears fruit.</summary>
	public List<Season> Seasons { get; set; }

	/// <summary>The fruit to produce. The first matching entry will be produced.</summary>
	public List<FruitTreeFruitData> Fruit { get; set; }

	/// <summary>The asset name for the texture for the tree's spritesheet.</summary>
	public string Texture { get; set; }

	/// <summary>The row index within the <see cref="P:StardewValley.GameData.FruitTrees.FruitTreeData.Texture" /> for the tree's sprites.</summary>
	public int TextureSpriteRow { get; set; }

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields { get; set; }
}
