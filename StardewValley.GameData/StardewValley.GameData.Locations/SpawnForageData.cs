using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Locations;

/// <summary>As part of <see cref="T:StardewValley.GameData.Locations.LocationData" />, a forage object that can spawn in the location.</summary>
/// <remarks>
///   Forage spawns have a few special constraints:
///   <list type="bullet">
///     <item><description>Only one item can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.</description></item>
///     <item><description>If this returns a null or non-<c>StardewValley.Object</c> item, the game will skip that spawn opportunity (and log a warning for a non-null invalid item type).</description></item>
///     <item><description>The <see cref="P:StardewValley.GameData.GenericSpawnItemDataWithCondition.Condition" /> field is checked once right before spawning forage, to build the list of possible forage spawns. It's not checked again for each forage spawn; use the <see cref="P:StardewValley.GameData.Locations.SpawnForageData.Chance" /> instead for per-spawn probability.</description></item>
///   </list>
/// </remarks>
public class SpawnForageData : GenericSpawnItemDataWithCondition
{
	/// <summary>The probability that the forage will spawn if it's selected, as a value between 0 (never) and 1 (always). If this check fails, that spawn opportunity will be skipped.</summary>
	[ContentSerializer(Optional = true)]
	public double Chance { get; set; } = 1.0;


	/// <summary>If set, the specific season when the forage should apply. For more complex conditions, see <see cref="P:StardewValley.GameData.GenericSpawnItemDataWithCondition.Condition" />.</summary>
	[ContentSerializer(Optional = true)]
	public Season? Season { get; set; }
}
