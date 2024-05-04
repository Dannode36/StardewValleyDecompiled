using System;
using System.Collections.Generic;

namespace StardewValley.Delegates;

/// <summary>The contextual values for a <see cref="T:StardewValley.Delegates.GameStateQueryDelegate" />.</summary>
public readonly struct GameStateQueryContext
{
	/// <summary>The location for which to check the query.</summary>
	public readonly GameLocation Location;

	/// <summary>The player for which to check the query.</summary>
	public readonly Farmer Player;

	/// <summary>The target item (e.g. machine output or tree fruit) for which to check the query, or <c>null</c> if not applicable.</summary>
	public readonly Item TargetItem;

	/// <summary>The input item (e.g. machine input) for which to check the query, or <c>null</c> if not applicable.</summary>
	public readonly Item InputItem;

	/// <summary>The RNG to use for randomization.</summary>
	public readonly Random Random;

	/// <summary>The query keys to ignore when checking conditions (like <c>LOCATION_SEASON</c>), or <c>null</c> to check all of them.</summary>
	public readonly HashSet<string> IgnoreQueryKeys;

	/// <summary>The custom fields which can be set by mods for custom game state query behavior, or <c>null</c> if none were set.</summary>
	public readonly Dictionary<string, object> CustomFields;

	/// <summary>Construct an instance.</summary>
	/// <param name="location">The location for which to check the query.</param>
	/// <param name="player">The player for which to check the query.</param>
	/// <param name="targetItem">The target item (e.g. machine output or tree fruit) for which to check the query, or <c>null</c> if not applicable.</param>
	/// <param name="inputItem">The input item (e.g. machine input) for which to check the query, or <c>null</c> if not applicable.</param>
	/// <param name="random">The RNG to use for randomization.</param>
	/// <param name="ignoreQueryKeys">The query keys to ignore when checking conditions (like <c>LOCATION_SEASON</c>), or <c>null</c> to check all of them.</param>
	/// <param name="customFields">The custom fields which can be set by mods for custom game state query behavior.</param>
	public GameStateQueryContext(GameLocation location, Farmer player, Item targetItem, Item inputItem, Random random, HashSet<string> ignoreQueryKeys = null, Dictionary<string, object> customFields = null)
	{
		Location = location ?? player?.currentLocation ?? Game1.currentLocation;
		Player = player ?? Game1.player;
		TargetItem = targetItem;
		InputItem = inputItem;
		Random = random ?? Game1.random;
		IgnoreQueryKeys = ignoreQueryKeys;
		CustomFields = customFields;
	}
}
