using System;

namespace StardewValley.GameData;

/// <summary>As part of <see cref="T:StardewValley.GameData.PlantableRule" />, indicates which cases the rule applies to.</summary>
[Flags]
public enum PlantableRuleContext
{
	/// <summary>This rule applies when planting into the ground.</summary>
	Ground = 1,
	/// <summary>This rule applies when planting in a garden pot.</summary>
	GardenPot = 2,
	/// <summary>This rule always applies.</summary>
	Any = 3
}
