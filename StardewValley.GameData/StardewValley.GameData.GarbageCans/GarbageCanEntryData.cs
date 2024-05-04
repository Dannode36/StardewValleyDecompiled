using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.GarbageCans;

/// <summary>Metadata for a specific in-game garbage can.</summary>
public class GarbageCanEntryData
{
	/// <summary>The probability that any item will be found when the garbage can is searched, or <c>-1</c> to use <see cref="F:StardewValley.GameData.GarbageCans.GarbageCanData.DefaultBaseChance" />.</summary>
	[ContentSerializer(Optional = true)]
	public float BaseChance = -1f;

	/// <summary>The items that may be found by rummaging in the garbage can.</summary>
	public List<GarbageCanItemData> Items;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
