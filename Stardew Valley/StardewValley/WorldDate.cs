using System;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley;

/// <summary>An in-game calendar date.</summary>
public class WorldDate : INetObject<NetFields>
{
	/// <summary>The number of months in a year.</summary>
	public const int MonthsPerYear = 4;

	/// <summary>The number of days per month.</summary>
	public const int DaysPerMonth = 28;

	/// <summary>The number of days per year.</summary>
	public const int DaysPerYear = 112;

	/// <summary>The backing field for <see cref="P:StardewValley.WorldDate.Year" />.</summary>
	private readonly NetInt year = new NetInt(1);

	/// <summary>The backing field for <see cref="P:StardewValley.WorldDate.Season" />.</summary>
	private readonly NetEnum<Season> season = new NetEnum<Season>(Season.Spring);

	/// <summary>The backing field for <see cref="P:StardewValley.WorldDate.DayOfMonth" />.</summary>
	private readonly NetInt dayOfMonth = new NetInt(1);

	/// <summary>The calendar year.</summary>
	public int Year
	{
		get
		{
			return year.Value;
		}
		set
		{
			year.Value = value;
		}
	}

	/// <summary>The index of the calendar season (where 0 is spring, 1 is summer, 2 is fall, and 3 is winter).</summary>
	[XmlIgnore]
	public int SeasonIndex => (int)season.Value;

	/// <summary>The calendar day of month.</summary>
	public int DayOfMonth
	{
		get
		{
			return dayOfMonth.Value;
		}
		set
		{
			dayOfMonth.Value = value;
		}
	}

	/// <summary>The day of week.</summary>
	public DayOfWeek DayOfWeek => GetDayOfWeekFor(DayOfMonth);

	/// <summary>The calendar season.</summary>
	[XmlIgnore]
	public Season Season
	{
		get
		{
			return season.Value;
		}
		set
		{
			season.Value = value;
		}
	}

	/// <summary>The unique key for the calendar season (one of <c>spring</c>, <c>summer</c>, <c>fall</c>, or <c>winter</c>).</summary>
	[XmlElement("Season")]
	public string SeasonKey
	{
		get
		{
			return Utility.getSeasonKey(season.Value);
		}
		set
		{
			if (!Utility.TryParseEnum<Season>(value, out var parsedSeason))
			{
				throw new ArgumentException("Can't parse '" + value + "' as a season key.", "value");
			}
			season.Value = parsedSeason;
		}
	}

	/// <summary>The number of days since the game began (starting at 1 for the first day of spring in Y1).</summary>
	[XmlIgnore]
	public int TotalDays
	{
		get
		{
			return ((Year - 1) * 4 + SeasonIndex) * 28 + (DayOfMonth - 1);
		}
		set
		{
			int totalMonths = value / 28;
			DayOfMonth = value % 28 + 1;
			Season = (Season)(totalMonths % 4);
			Year = totalMonths / 4 + 1;
		}
	}

	/// <summary>The number of weeks since the game began (starting at 1 for the first day of spring in Y1).</summary>
	public int TotalWeeks => TotalDays / 7;

	/// <summary>The number of Sundays since the game began (starting at 1 for the first day of spring in Y1).</summary>
	public int TotalSundayWeeks => (TotalDays + 1) / 7;

	public NetFields NetFields { get; } = new NetFields("WorldDate");


	/// <summary>Construct an instance.</summary>
	public WorldDate()
	{
		NetFields.SetOwner(this).AddField(year, "year").AddField(season, "season")
			.AddField(dayOfMonth, "dayOfMonth");
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="other">The date to copy.</param>
	public WorldDate(WorldDate other)
		: this()
	{
		Year = other.Year;
		Season = other.Season;
		DayOfMonth = other.DayOfMonth;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="year">The calendar year.</param>
	/// <param name="season">The calendar season.</param>
	/// <param name="dayOfMonth">The calendar day of month.</param>
	public WorldDate(int year, Season season, int dayOfMonth)
		: this()
	{
		Year = year;
		Season = season;
		DayOfMonth = dayOfMonth;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="year">The calendar year.</param>
	/// <param name="seasonKey">The unique key for the calendar season (one of <c>spring</c>, <c>summer</c>, <c>fall</c>, or <c>winter</c>).</param>
	/// <param name="dayOfMonth">The calendar day of month.</param>
	public WorldDate(int year, string seasonKey, int dayOfMonth)
		: this()
	{
		Year = year;
		SeasonKey = seasonKey;
		DayOfMonth = dayOfMonth;
	}

	/// <summary>Get a translated display text for the calendar date.</summary>
	public string Localize()
	{
		return Utility.getDateStringFor(DayOfMonth, SeasonIndex, Year);
	}

	/// <summary>Get a non-translated string representation for debug purposes.</summary>
	public override string ToString()
	{
		return $"Year {Year}, {SeasonKey} {DayOfMonth}, {DayOfWeek}";
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		if (obj is WorldDate other)
		{
			return TotalDays == other.TotalDays;
		}
		return false;
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		return TotalDays;
	}

	/// <summary>Get whether two dates are equal.</summary>
	/// <param name="a">The first date to check.</param>
	/// <param name="b">The second date to check.</param>
	public static bool operator ==(WorldDate a, WorldDate b)
	{
		return a?.TotalDays == b?.TotalDays;
	}

	/// <summary>Get whether two dates are not equal.</summary>
	/// <param name="a">The first date to check.</param>
	/// <param name="b">The second date to check.</param>
	public static bool operator !=(WorldDate a, WorldDate b)
	{
		return a?.TotalDays != b?.TotalDays;
	}

	/// <summary>Get whether one date precedes another.</summary>
	/// <param name="a">The left date to check.</param>
	/// <param name="b">The right date to check.</param>
	public static bool operator <(WorldDate a, WorldDate b)
	{
		return a?.TotalDays < b?.TotalDays;
	}

	/// <summary>Get whether one date postdates another.</summary>
	/// <param name="a">The left date to check.</param>
	/// <param name="b">The right date to check.</param>
	public static bool operator >(WorldDate a, WorldDate b)
	{
		return a?.TotalDays > b?.TotalDays;
	}

	/// <summary>Get whether one date precedes or is equal to another.</summary>
	/// <param name="a">The left date to check.</param>
	/// <param name="b">The right date to check.</param>
	public static bool operator <=(WorldDate a, WorldDate b)
	{
		return a?.TotalDays <= b?.TotalDays;
	}

	/// <summary>Get whether one date postdates or is equal to another.</summary>
	/// <param name="a">The left date to check.</param>
	/// <param name="b">The right date to check.</param>
	public static bool operator >=(WorldDate a, WorldDate b)
	{
		return a?.TotalDays >= b?.TotalDays;
	}

	/// <summary>Get the day of week for a day number.</summary>
	/// <param name="dayOfMonth">The day of month, between 1 and 28.</param>
	public static DayOfWeek GetDayOfWeekFor(int dayOfMonth)
	{
		return (DayOfWeek)(dayOfMonth % 7);
	}

	/// <summary>Get the current in-game date.</summary>
	public static WorldDate Now()
	{
		return new WorldDate(Game1.year, Game1.season, Game1.dayOfMonth);
	}

	/// <summary>Get the in-game date for a number of days played.</summary>
	/// <param name="daysPlayed">The number of days since the game began (starting at 1 for the first day of spring in Y1).</param>
	public static WorldDate ForDaysPlayed(int daysPlayed)
	{
		return new WorldDate
		{
			TotalDays = daysPlayed
		};
	}

	/// <summary>Get the day of week from a string value, if valid.</summary>
	/// <param name="day">The numeric day of month (between 1 and 28), short English day name (like 'Mon'), or full English day name (like 'Monday').</param>
	/// <param name="dayOfWeek">The parsed day of week, if valid.</param>
	/// <returns>Returns whether the day of week was successfully parsed.</returns>
	public static bool TryGetDayOfWeekFor(string day, out DayOfWeek dayOfWeek)
	{
		if (int.TryParse(day, out var numeric))
		{
			dayOfWeek = GetDayOfWeekFor(numeric);
			return true;
		}
		switch (day?.ToLower())
		{
		case "mon":
		case "monday":
			dayOfWeek = DayOfWeek.Monday;
			return true;
		case "tue":
		case "tuesday":
			dayOfWeek = DayOfWeek.Tuesday;
			return true;
		case "wed":
		case "wednesday":
			dayOfWeek = DayOfWeek.Wednesday;
			return true;
		case "thu":
		case "thursday":
			dayOfWeek = DayOfWeek.Thursday;
			return true;
		case "fri":
		case "friday":
			dayOfWeek = DayOfWeek.Friday;
			return true;
		case "sat":
		case "saturday":
			dayOfWeek = DayOfWeek.Saturday;
			return true;
		case "sun":
		case "sunday":
			dayOfWeek = DayOfWeek.Sunday;
			return true;
		default:
			dayOfWeek = DayOfWeek.Sunday;
			return false;
		}
	}
}
