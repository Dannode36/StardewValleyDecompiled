using System;
using System.Collections.Generic;

namespace StardewValley.Internal;

/// <summary>The game context for an item search query.</summary>
public class ItemQueryContext
{
	/// <summary>The location to use for location-dependent queries like season.</summary>
	public GameLocation Location { get; }

	/// <summary>The player for which to perform the search.</summary>
	public Farmer Player { get; }

	/// <summary>The instance to use for randomization, or <c>null</c> to create one dynamically.</summary>
	public Random Random { get; }

	/// <summary>The full item query string.</summary>
	public string QueryString { get; internal set; }

	/// <summary>The context for the item query which triggered this query, if any.</summary>
	/// <remarks>
	///   <para>For example, <c>LOST_BOOK_OR_ITEM RANDOM_ITEMS (O)</c> contains a fallback <c>RANDOM_ITEMS (O)</c> query which is parsed if the player already found every lost book. In that case, the former is the parent context for the latter.</para>
	///   <para>This is used to detect and break circular references.</para>
	/// </remarks>
	public ItemQueryContext ParentContext { get; }

	/// <summary>The custom fields which can be set by mods for custom item query behavior, or <c>null</c> if none were set.</summary>
	public Dictionary<string, object> CustomFields { get; set; }

	/// <summary>Construct an instance.</summary>
	public ItemQueryContext()
		: this(null, null, null)
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="parentContext">The context for the item query which triggered this query, if any.</param>
	public ItemQueryContext(ItemQueryContext parentContext)
		: this(parentContext?.Location, parentContext?.Player, parentContext?.Random)
	{
		ParentContext = parentContext;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="location">The location to use for location-dependent queries like season.</param>
	/// <param name="player">The player for which to perform the search.</param>
	/// <param name="random">The instance to use for randomization, or <c>null</c> to create one dynamically.</param>
	public ItemQueryContext(GameLocation location, Farmer player, Random random)
	{
		Location = location ?? Game1.currentLocation;
		Player = player ?? Game1.player;
		Random = random;
	}
}
