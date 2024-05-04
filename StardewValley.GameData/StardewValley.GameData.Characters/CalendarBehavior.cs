namespace StardewValley.GameData.Characters;

/// <summary>How an NPC's birthday is shown on the calendar.</summary>
public enum CalendarBehavior
{
	/// <summary>They always appear on the calendar.</summary>
	AlwaysShown,
	/// <summary>Until the player meets them, they don't appear on the calendar.</summary>
	HiddenUntilMet,
	/// <summary>They never appear on the calendar.</summary>
	HiddenAlways
}
