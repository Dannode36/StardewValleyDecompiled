using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>The metadata for a festival like the Night Market which replaces an in-game location for a period of time, which the player can enter/leave anytime, and which doesn't affect the passage of time.</summary>
public class PassiveFestivalData
{
	/// <summary>A tokenizable string for the display name shown on the calendar.</summary>
	public string DisplayName;

	/// <summary>A game state query which indicates whether the festival is enabled (subject to the other fields like <see cref="F:StardewValley.GameData.PassiveFestivalData.StartDay" /> and <see cref="F:StardewValley.GameData.PassiveFestivalData.EndDay" />). Defaults to always enabled.</summary>
	public string Condition;

	/// <summary>Whether the festival appears on the calendar, using the same icon as the Night Market. Default true.</summary>
	[ContentSerializer(Optional = true)]
	public bool ShowOnCalendar = true;

	/// <summary>The season when the festival becomes active.</summary>
	public Season Season;

	/// <summary>The day of month when the festival becomes active.</summary>
	public int StartDay;

	/// <summary>The last day of month when the festival is active.</summary>
	public int EndDay;

	/// <summary>The time of day when the festival opens each day.</summary>
	public int StartTime;

	/// <summary>A tokenizable string for the in-game toast notification shown when the festival begins each day.</summary>
	public string StartMessage;

	/// <summary>If true, the in-game notification for festival start will only play on the first day</summary>
	[ContentSerializer(Optional = true)]
	public bool OnlyShowMessageOnFirstDay;

	/// <summary>The locations to swap for the duration of the festival, where the key is the original location's internal name and the value is the new location's internal name.</summary>
	/// <remarks>Despite the field name, this swaps the full locations, not the location's map asset.</remarks>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> MapReplacements;

	/// <summary>A C# method which applies custom logic when the day starts.</summary>
	/// <remarks>This must be specified in the form <c>{full type name}: {method name}</c>. The method must be static, take zero arguments, and return void.</remarks>
	[ContentSerializer(Optional = true)]
	public string DailySetupMethod;

	/// <summary>A C# method which applies custom logic overnight after the last day of the festival.</summary>
	/// <remarks>This must be specified in the form <c>{full type name}: {method name}</c>. The method must be static, take zero arguments, and return void.</remarks>
	[ContentSerializer(Optional = true)]
	public string CleanupMethod;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
