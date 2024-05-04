using System.Collections.Generic;

namespace StardewValley.GameData.GarbageCans;

/// <summary>The data for in-game garbage cans.</summary>
public class GarbageCanData
{
	/// <summary>The default probability that any item will be found when searching a garbage can, unless overridden by <see cref="F:StardewValley.GameData.GarbageCans.GarbageCanEntryData.BaseChance" />.</summary>
	public float DefaultBaseChance = 0.2f;

	/// <summary>The items to try before <see cref="F:StardewValley.GameData.GarbageCans.GarbageCanData.GarbageCans" /> and <see cref="F:StardewValley.GameData.GarbageCans.GarbageCanData.AfterAll" />, subject to the garbage can's base chance.</summary>
	public List<GarbageCanItemData> BeforeAll;

	/// <summary>The items to try if neither <see cref="F:StardewValley.GameData.GarbageCans.GarbageCanData.BeforeAll" /> nor <see cref="F:StardewValley.GameData.GarbageCans.GarbageCanData.GarbageCans" /> returned a value.</summary>
	public List<GarbageCanItemData> AfterAll;

	/// <summary>The metadata for specific garbage can IDs.</summary>
	public Dictionary<string, GarbageCanEntryData> GarbageCans;
}
