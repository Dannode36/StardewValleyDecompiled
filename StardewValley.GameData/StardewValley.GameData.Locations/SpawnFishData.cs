using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Locations;

/// <summary>As part of <see cref="T:StardewValley.GameData.Locations.LocationData" />, an item that can be found by fishing in the location.</summary>
/// <remarks>
///   Fish spawns have a few special constraints:
///   <list type="bullet">
///     <item><description>Only one fish can be produced at a time. If this uses an item query which returns multiple items, one will be chosen at random.</description></item>
///     <item><description>This must return an item of type <c>StardewValley.Object</c> or one of its subclasses.</description></item>
///     <item><description>Entries using an item query (instead of an item ID) are ignored for the fishing TV channel hints.</description></item>
///   </list>
/// </remarks>
public class SpawnFishData : GenericSpawnItemDataWithCondition
{
	/// <summary>The probability that the fish will spawn, as a value between 0 (never) and 1 (always).</summary>
	[ContentSerializer(Optional = true)]
	public float Chance { get; set; } = 1f;


	/// <summary>If set, the specific season when the fish should apply. For more complex conditions, see <see cref="P:StardewValley.GameData.GenericSpawnItemDataWithCondition.Condition" />.</summary>
	[ContentSerializer(Optional = true)]
	public Season? Season { get; set; }

	/// <summary>If set, the fish area (as defined by <see cref="F:StardewValley.GameData.Locations.LocationData.FishAreas" /> in which the fish can be caught. If omitted, it can be caught in all areas.</summary>
	[ContentSerializer(Optional = true)]
	public string FishAreaId { get; set; }

	/// <summary>If set, the tile area within the location where the bobber must land to catch the fish.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle? BobberPosition { get; set; }

	/// <summary>If set, the tile area within the location where the player must be standing to catch the fish.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle? PlayerPosition { get; set; }

	/// <summary>The minimum fishing level needed for the fish to appear.</summary>
	[ContentSerializer(Optional = true)]
	public int MinFishingLevel { get; set; }

	/// <summary>The minimum distance from the shore (measured in tiles) at which the fish can be caught, where zero is water directly adjacent to shore.</summary>
	[ContentSerializer(Optional = true)]
	public int MinDistanceFromShore { get; set; }

	/// <summary>The maximum distance from the shore (measured in tiles) at which the fish can be caught, where zero is water directly adjacent to shore, or -1 for no maximum.</summary>
	[ContentSerializer(Optional = true)]
	public int MaxDistanceFromShore { get; set; } = -1;


	/// <summary>Whether to increase the <see cref="P:StardewValley.GameData.Locations.SpawnFishData.Chance" /> by an amount equal to the player's daily luck.</summary>
	[ContentSerializer(Optional = true)]
	public bool ApplyDailyLuck { get; set; }

	/// <summary>A flat increase to the spawn chance when the player has the Curiosity Lure equipped, or <c>-1</c> to apply the default behavior. This affects both the <see cref="P:StardewValley.GameData.Locations.SpawnFishData.Chance" /> field and the <c>Data\Fish</c> chance, if applicable.</summary>
	[ContentSerializer(Optional = true)]
	public float CuriosityLureBuff { get; set; } = -1f;


	/// <summary>A flat increase to the spawn chance when the player has a specific bait equipped which targets this fish.</summary>
	[ContentSerializer(Optional = true)]
	public float SpecificBaitBuff { get; set; }

	/// <summary>A multiplier applied to the spawn chance when the player has a specific bait equipped which targets this fish.</summary>
	[ContentSerializer(Optional = true)]
	public float SpecificBaitMultiplier { get; set; } = 1.66f;


	/// <summary>The maximum number of times this fish can be caught by each player.</summary>
	[ContentSerializer(Optional = true)]
	public int CatchLimit { get; set; } = -1;


	/// <summary>Whether the player can catch this fish using a training rod. This can be <c>true</c> (always allowed), <c>false</c> (never allowed), or <c>null</c> (apply default logic, i.e. allowed for difficulty ratings under 50).</summary>
	[ContentSerializer(Optional = true)]
	public bool? CanUseTrainingRod { get; set; }

	/// <summary>Whether this is a 'boss fish' in the fishing minigame. This shows a crowned fish sprite in the minigame, multiplies the XP gained by five, and hides it from the F.I.B.S. TV channel.</summary>
	[ContentSerializer(Optional = true)]
	public bool IsBossFish { get; set; }

	/// <summary>The mail flag to set for the current player when this fish is successfully caught.</summary>
	[ContentSerializer(Optional = true)]
	public string SetFlagOnCatch { get; set; }

	/// <summary>Whether the player must fish with Magic Bait for this fish to spawn.</summary>
	[ContentSerializer(Optional = true)]
	public bool RequireMagicBait { get; set; }

	/// <summary>The order in which this fish should be checked, where 0 is the default value used by most fish. Fish within each precedence group are shuffled randomly.</summary>
	[ContentSerializer(Optional = true)]
	public int Precedence { get; set; }

	/// <summary>Whether to ignore any fish requirements listed for the ID in <c>Data/Fish</c>.</summary>
	/// <remarks>The <c>Data/Fish</c> requirements are ignored regardless of this field for non-object (<c>(O)</c>)-type items, or objects with an ID not listed in <c>Data/Fish</c>.</remarks>
	[ContentSerializer(Optional = true)]
	public bool IgnoreFishDataRequirements { get; set; }

	/// <summary>Whether this fish can be spawned in another location via the <c>LOCATION_FISH</c> item query.</summary>
	[ContentSerializer(Optional = true)]
	public bool CanBeInherited { get; set; } = true;


	/// <summary>Changes to apply to the <see cref="P:StardewValley.GameData.Locations.SpawnFishData.Chance" />.</summary>
	[ContentSerializer(Optional = true)]
	public List<QuantityModifier> ChanceModifiers { get; set; }

	/// <summary>How multiple <see cref="P:StardewValley.GameData.Locations.SpawnFishData.ChanceModifiers" /> should be combined.</summary>
	[ContentSerializer(Optional = true)]
	public QuantityModifier.QuantityModifierMode ChanceModifierMode { get; set; }

	/// <summary>How much to increase the <see cref="P:StardewValley.GameData.Locations.SpawnFishData.Chance" /> per player's Luck level</summary>
	[ContentSerializer(Optional = true)]
	public float ChanceBoostPerLuckLevel { get; set; }

	/// <summary>If true, the chance roll will use a seed value based on the number of fish caught.</summary>
	[ContentSerializer(Optional = true)]
	public bool UseFishCaughtSeededRandom { get; set; }

	/// <summary>Get the probability that the fish will spawn, adjusted for modifiers and equipment.</summary>
	/// <param name="hasCuriosityLure">Whether the player has the Curiosity Lure equipped.</param>
	/// <param name="dailyLuck">The player's daily luck value.</param>
	/// <param name="luckLevel">The player's current luck level.</param>
	/// <param name="applyModifiers">Apply quantity modifiers to the given value.</param>
	/// <param name="isTargetedWithBait">Whether the player has a specific bait equipped which targets this fish.</param>
	/// <returns>Returns a value between 0 (never) and 1 (always).</returns>
	public float GetChance(bool hasCuriosityLure, double dailyLuck, int luckLevel, Func<float, IList<QuantityModifier>, QuantityModifier.QuantityModifierMode, float> applyModifiers, bool isTargetedWithBait = false)
	{
		float chance = Chance;
		if (hasCuriosityLure && CuriosityLureBuff > 0f)
		{
			chance += CuriosityLureBuff;
		}
		if (ApplyDailyLuck)
		{
			chance += (float)dailyLuck;
		}
		List<QuantityModifier> chanceModifiers = ChanceModifiers;
		if (chanceModifiers != null && chanceModifiers.Count > 0)
		{
			chance = applyModifiers(chance, ChanceModifiers, ChanceModifierMode);
		}
		if (isTargetedWithBait)
		{
			chance = chance * SpecificBaitMultiplier + SpecificBaitBuff;
		}
		return chance + ChanceBoostPerLuckLevel * (float)luckLevel;
	}
}
