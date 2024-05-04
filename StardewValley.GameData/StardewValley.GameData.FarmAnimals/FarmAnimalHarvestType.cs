namespace StardewValley.GameData.FarmAnimals;

/// <summary>How produced items are collected from an animal.</summary>
public enum FarmAnimalHarvestType
{
	/// <summary>The item is placed on the ground in the animal's home building overnight.</summary>
	DropOvernight,
	/// <summary>The item is collected from the animal directly based on the <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.HarvestTool" /> field.</summary>
	HarvestWithTool,
	/// <summary>The farm animal digs it up with an animation like pigs finding truffles.</summary>
	DigUp
}
