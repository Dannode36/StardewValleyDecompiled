namespace StardewValley.GameData.WildTrees;

/// <summary>The growth state for a tree.</summary>
/// <remarks>These mainly exist to make content edits more readable. Most code should use the constants like <c>Tree.seedStage</c>, which have the same values.</remarks>
public enum WildTreeGrowthStage
{
	Seed = 0,
	Sprout = 1,
	Sapling = 2,
	Bush = 3,
	Tree = 5
}
