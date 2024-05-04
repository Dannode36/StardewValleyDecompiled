namespace StardewValley.GameData.SpecialOrders;

/// <summary>The period for which a special order is valid.</summary>
public enum QuestDuration
{
	/// <summary>The order is valid until the end of this week.</summary>
	Week,
	/// <summary>The order is valid until the end of this month.</summary>
	Month,
	/// <summary>The order is valid until the end of the next weeks.</summary>
	TwoWeeks,
	/// <summary>The order is valid until the end of tomorrow.</summary>
	TwoDays,
	/// <summary>The order is valid until the end of after tomorrow.</summary>
	ThreeDays,
	/// <summary>The valid is valid until the end of today.</summary>
	OneDay
}
