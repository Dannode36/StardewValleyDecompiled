using System;
using Microsoft.Xna.Framework;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Network;

namespace StardewValley;

/// <summary>The low-level handlers for vanilla event preconditions. Most code should use <see cref="M:StardewValley.Event.CheckPrecondition(StardewValley.GameLocation,System.String,System.String)" /> instead.</summary>
public class Preconditions
{
	/// <summary>The current farmer has seen any of the given events.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "e" })]
	public static bool SawEvent(GameLocation location, string eventId, string[] args)
	{
		for (int i = 1; i < args.Length; i++)
		{
			if (!ArgUtility.TryGet(args, i, out var id, out var error, allowBlank: false))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			if (Game1.player.eventsSeen.Contains(id))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>The current farmer hasn't received a pet yet, and (if specified) has this pet preference.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "h" })]
	public static bool MissingPet(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGetOptional(args, 1, out var petType, out var error, null, allowBlank: false))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		if (!Game1.player.hasPet())
		{
			if (petType != null)
			{
				return string.Equals(petType, Game1.player.whichPetType, StringComparison.OrdinalIgnoreCase);
			}
			return true;
		}
		return false;
	}

	/// <summary>The current farmer is the host.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "H" })]
	public static bool IsHost(GameLocation location, string eventId, string[] args)
	{
		return Game1.IsMasterGame;
	}

	/// <summary>The host farmer has this mail.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "Hn" })]
	public static bool HostMail(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGet(args, 1, out var mailId, out var error, allowBlank: false))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		return Game1.MasterPlayer.mailReceived.Contains(mailId);
	}

	/// <summary>The host farmer does NOT have this mail.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[Obsolete("New events should use !HostMail instead.")]
	[OtherNames(new string[] { "Hl" })]
	public static bool NotHostMail(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGet(args, 1, out var mailId, out var error, allowBlank: false))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		return !Game1.MasterPlayer.mailReceived.Contains(mailId);
	}

	/// <summary>This world state ID is active anywhere.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "*" })]
	public static bool WorldState(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGet(args, 1, out var worldStateId, out var error, allowBlank: false))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		return NetWorldState.checkAnywhereForWorldStateID(worldStateId);
	}

	/// <summary>Either the host or current farmer have this mail.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "*n" })]
	public static bool HostOrLocalMail(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGet(args, 1, out var mailId, out var error, allowBlank: false))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		if (!Game1.MasterPlayer.mailReceived.Contains(mailId))
		{
			return Game1.player.mailReceived.Contains(mailId);
		}
		return true;
	}

	/// <summary>Neither the host nor current farmer have this mail.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[Obsolete("New events should use !HostOrLocalMail instead.")]
	[OtherNames(new string[] { "*l" })]
	public static bool NotHostOrLocalMail(GameLocation location, string eventId, string[] args)
	{
		return !HostOrLocalMail(location, eventId, args);
	}

	/// <summary>The current farmer has earned at least this much money, including spent money.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "m" })]
	public static bool EarnedMoney(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGetInt(args, 1, out var minMoney, out var error))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		return Game1.player.totalMoneyEarned >= minMoney;
	}

	/// <summary>The current farmer has at least this much money, not including spent money.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "M" })]
	public static bool HasMoney(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGetInt(args, 1, out var minMoney, out var error))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		return Game1.player.Money >= minMoney;
	}

	/// <summary>The current farmer has at least this many free slots in their inventory.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "c" })]
	public static bool FreeInventorySlots(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGetInt(args, 1, out var minFreeSpots, out var error))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		return Game1.player.freeSpotsInInventory() >= minFreeSpots;
	}

	/// <summary>The community center or Joja warehouse have been completed.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "C" })]
	public static bool CommunityCenterOrWarehouseDone(GameLocation location, string eventId, string[] args)
	{
		if (!Game1.MasterPlayer.eventsSeen.Contains("191393") && !Game1.MasterPlayer.eventsSeen.Contains("502261"))
		{
			return Game1.MasterPlayer.hasCompletedCommunityCenter();
		}
		return true;
	}

	/// <summary>The community center or Joja warehouse have NOT been completed.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[Obsolete("New events should use !CommunityCenterOrWarehouseDone instead.")]
	[OtherNames(new string[] { "X" })]
	public static bool NotCommunityCenterOrWarehouseDone(GameLocation location, string eventId, string[] args)
	{
		return !CommunityCenterOrWarehouseDone(location, eventId, args);
	}

	/// <summary>The current farmer is dating the given NPC.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "D" })]
	public static bool Dating(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGet(args, 1, out var npcName, out var error, allowBlank: false))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		if (Game1.player.friendshipData.TryGetValue(npcName, out var friendship))
		{
			return friendship.IsDating();
		}
		return false;
	}

	/// <summary>The main farmer has played at least this many days.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "j" })]
	public static bool DaysPlayed(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGetInt(args, 1, out var minDaysPlayed, out var error))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		return Game1.stats.DaysPlayed > minDaysPlayed;
	}

	/// <summary>All Joja bundles has been completed.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "J" })]
	public static bool JojaBundlesDone(GameLocation location, string eventId, string[] args)
	{
		return Utility.hasFinishedJojaRoute();
	}

	/// <summary>The current farmer has at least this many friendship points with all of the given NPCs.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "f" })]
	public static bool Friendship(GameLocation location, string eventId, string[] args)
	{
		for (int i = 1; i < args.Length; i += 2)
		{
			if (!ArgUtility.TryGet(args, i, out var npcName, out var error, allowBlank: false) || !ArgUtility.TryGetInt(args, i + 1, out var minPoints, out error))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			if (!Game1.player.friendshipData.TryGetValue(npcName, out var friendship) || friendship.Points < minPoints)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Today is a festival day.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	public static bool FestivalDay(GameLocation location, string eventId, string[] args)
	{
		return Utility.isFestivalDay();
	}

	/// <summary>Today is NOT a festival day.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[Obsolete("New events should use !FestivalDay instead.")]
	[OtherNames(new string[] { "F" })]
	public static bool NotFestivalDay(GameLocation location, string eventId, string[] args)
	{
		return !FestivalDay(location, eventId, args);
	}

	/// <summary>A random check with the given probability matches.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "r" })]
	public static bool Random(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGetFloat(args, 1, out var probability, out var error))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		return Game1.random.NextDouble() <= (double)probability;
	}

	/// <summary>The current farmer has shipped at least this many of each given item ID.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "s" })]
	public static bool Shipped(GameLocation location, string eventId, string[] args)
	{
		for (int i = 1; i < args.Length; i += 2)
		{
			if (!ArgUtility.TryGet(args, i, out var itemId, out var error, allowBlank: false) || !ArgUtility.TryGetInt(args, i + 1, out var minShipped, out error))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			if (!Game1.player.basicShipped.TryGetValue(itemId, out var countShipped) || countShipped < minShipped)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>The current farmer has seen this secret note.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "S" })]
	public static bool SawSecretNote(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGetInt(args, 1, out var secretNoteId, out var error))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		return Game1.player.secretNotesSeen.Contains(secretNoteId);
	}

	/// <summary>The current farmer has selected all of the given dialogue answer IDs.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "q" })]
	public static bool ChoseDialogueAnswers(GameLocation location, string eventId, string[] args)
	{
		for (int i = 1; i < args.Length; i++)
		{
			if (!ArgUtility.TryGet(args, i, out var answerId, out var error, allowBlank: false))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			if (!Game1.player.DialogueQuestionsAnswered.Contains(answerId))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>The current farmer has received this mail.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "n" })]
	public static bool LocalMail(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGet(args, 1, out var mailId, out var error, allowBlank: false))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		return Game1.player.mailReceived.Contains(mailId);
	}

	/// <summary>All players have found at least this many golden walnuts combined, including spent walnuts.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "N" })]
	public static bool GoldenWalnuts(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGetInt(args, 1, out var minWalnuts, out var error))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		return Game1.netWorldState.Value.GoldenWalnutsFound >= minWalnuts;
	}

	/// <summary>The current farmer has NOT received this mail.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[Obsolete("New events should use !LocalMail instead.")]
	[OtherNames(new string[] { "l" })]
	public static bool NotLocalMail(GameLocation location, string eventId, string[] args)
	{
		return !LocalMail(location, eventId, args);
	}

	/// <summary>The current location is a farmhouse or cabin, and it has been upgraded to the max level (level 2 or greater).</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "L" })]
	public static bool InUpgradedHouse(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGetOptionalInt(args, 1, out var minUpgradeLevel, out var error, 2))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		FarmHouse obj = location as FarmHouse;
		if (obj == null)
		{
			return false;
		}
		return obj.upgradeLevel >= minUpgradeLevel;
	}

	/// <summary>The current time of day is between the given values inclusively.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "t" })]
	public static bool Time(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGetInt(args, 1, out var minTime, out var error) || !ArgUtility.TryGetInt(args, 2, out var maxTime, out error))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		if (Game1.timeOfDay >= minTime)
		{
			return Game1.timeOfDay <= maxTime;
		}
		return false;
	}

	/// <summary>The weather in the current location's context is <c>rainy</c>, <c>sunny</c>, or the given weather ID.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "w" })]
	public static bool Weather(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGet(args, 1, out var weather, out var error, allowBlank: false))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		if (!(weather == "rainy"))
		{
			if (weather == "sunny")
			{
				return !location.IsRainingHere();
			}
			return weather == location.GetWeather().Weather;
		}
		return location.IsRainingHere();
	}

	/// <summary>The current day of week is one of these values (in the form <c>Mon</c>, <c>Tue</c>, etc).</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	public static bool DayOfWeek(GameLocation location, string eventId, string[] args)
	{
		DayOfWeek actualDay = Game1.Date.DayOfWeek;
		for (int i = 1; i < args.Length; i++)
		{
			if (!ArgUtility.TryGet(args, i, out var rawDayName, out var error, allowBlank: false))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			if (!WorldDate.TryGetDayOfWeekFor(rawDayName, out var expectedDay))
			{
				return Event.LogPreconditionError(location, eventId, args, "can't parse '" + rawDayName + "' as a day of week");
			}
			if (actualDay == expectedDay)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>The current day of week is NOT one of these values (in the form <c>Mon</c>, <c>Tue</c>, etc).</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[Obsolete("New events should use !DayOfWeek instead.")]
	[OtherNames(new string[] { "d" })]
	public static bool NotDayOfWeek(GameLocation location, string eventId, string[] args)
	{
		return !DayOfWeek(location, eventId, args);
	}

	/// <summary>The current farmer is married to or engaged with this NPC.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "O" })]
	public static bool Spouse(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGet(args, 1, out var npcName, out var error, allowBlank: false))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		return Game1.player.spouse == npcName;
	}

	/// <summary>The current farmer is NOT married to or engaged with this NPC.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[Obsolete("New events should use !Spouse instead.")]
	[OtherNames(new string[] { "o" })]
	public static bool NotSpouse(GameLocation location, string eventId, string[] args)
	{
		return !Spouse(location, eventId, args);
	}

	/// <summary>The current farmer is roommates with any NPC.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "R" })]
	public static bool Roommate(GameLocation location, string eventId, string[] args)
	{
		return Game1.player.hasCurrentOrPendingRoommate();
	}

	/// <summary>The current farmer is NOT roommates with any NPC.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[Obsolete("New events should use !Roommate instead.")]
	[OtherNames(new string[] { "Rf" })]
	public static bool NotRoommate(GameLocation location, string eventId, string[] args)
	{
		return !Roommate(location, eventId, args);
	}

	/// <summary>The given NPC is present and visible in any location.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "v" })]
	public static bool NpcVisible(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGet(args, 1, out var npcName, out var error, allowBlank: false))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		NPC characterFromName = Game1.getCharacterFromName(npcName);
		if (characterFromName == null)
		{
			return false;
		}
		return !characterFromName.IsInvisible;
	}

	/// <summary>The given NPC is present and visible in the current location.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "p" })]
	public static bool NpcVisibleHere(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGet(args, 1, out var npcName, out var error, allowBlank: false))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		foreach (NPC n in location.characters)
		{
			if (n.Name == npcName && !n.IsInvisible)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>The current calendar season is one of the given values.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	public static bool Season(GameLocation location, string eventId, string[] args)
	{
		for (int i = 1; i < args.Length; i++)
		{
			if (!ArgUtility.TryGetEnum<Season>(args, 1, out var season, out var error))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			if (Game1.season == season)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>The current calendar season is NOT one of the given values.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[Obsolete("New events should use !Season instead.")]
	[OtherNames(new string[] { "z" })]
	public static bool NotSeason(GameLocation location, string eventId, string[] args)
	{
		return !Season(location, eventId, args);
	}

	/// <summary>The current farmer has a spouse bed in their house.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "B" })]
	public static bool SpouseBed(GameLocation location, string eventId, string[] args)
	{
		return Utility.getHomeOfFarmer(Game1.player).GetSpouseBed() != null;
	}

	/// <summary>The current farmer has reached the bottom of the mines (i.e. level 120) at least this many times.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "b" })]
	public static bool ReachedMineBottom(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGetOptionalInt(args, 1, out var minTimes, out var error, 1))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		return Game1.player.timesReachedMineBottom >= minTimes;
	}

	/// <summary>The current year is exactly 1 (if specified 1) or at least the given value (if specified any other value).</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "y" })]
	public static bool Year(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGetInt(args, 1, out var desiredYear, out var error))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		if (desiredYear != 1)
		{
			return Game1.year >= desiredYear;
		}
		return Game1.year == 1;
	}

	/// <summary>The current farmer has this gender.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "g" })]
	public static bool Gender(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGet(args, 1, out var gender, out var error, allowBlank: false))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		bool male = gender.ToLower() == "male";
		return Game1.player.IsMale == male;
	}

	/// <summary>The current farmer has this item ID in their inventory.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "i" })]
	public static bool HasItem(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGet(args, 1, out var itemId, out var error, allowBlank: false))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		if (!Game1.player.Items.ContainsId(itemId))
		{
			if (Game1.player.ActiveObject != null)
			{
				return ItemRegistry.HasItemId(Game1.player.ActiveObject, itemId);
			}
			return false;
		}
		return true;
	}

	/// <summary>The current farmer has NOT seen this event.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[Obsolete("New events should use !SawEvent instead.")]
	[OtherNames(new string[] { "k" })]
	public static bool NotSawEvent(GameLocation location, string eventId, string[] args)
	{
		return !SawEvent(location, eventId, args);
	}

	/// <summary>The current farmer is standing on one of these tile positions.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "a" })]
	public static bool Tile(GameLocation location, string eventId, string[] args)
	{
		Point actualTile = (Game1.isWarping ? new Point(Game1.xLocationAfterWarp, Game1.yLocationAfterWarp) : Game1.player.TilePoint);
		for (int i = 1; i < args.Length - 1; i += 2)
		{
			if (!ArgUtility.TryGetPoint(args, i, out var tile, out var error))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			if (tile == actualTile)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>The current player has this active dialogue event.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	public static bool ActiveDialogueEvent(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGet(args, 1, out var id, out var error, allowBlank: false))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		return Game1.player.activeDialogueEvents.ContainsKey(id);
	}

	/// <summary>The current player does NOT have this active dialogue event.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[Obsolete("New events should use !ActiveDialogueEvent instead.")]
	[OtherNames(new string[] { "A" })]
	public static bool NotActiveDialogueEvent(GameLocation location, string eventId, string[] args)
	{
		return !ActiveDialogueEvent(location, eventId, args);
	}

	/// <summary>Send the specified mail and end the event. This is a way to send mail without actually starting the event, it's not a regular event precondition.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[Obsolete("This is a deprecated way to send mail using a hidden pseudo-event. Newer code should use Data/TriggerActions instead.")]
	[OtherNames(new string[] { "x" })]
	public static bool SendMail(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGet(args, 1, out var mailId, out var error, allowBlank: false) || !ArgUtility.TryGetOptionalBool(args, 2, out var inMailboxToday, out error))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		if (inMailboxToday)
		{
			Game1.player.mailbox.Add(mailId);
		}
		else
		{
			Game1.addMailForTomorrow(mailId);
		}
		Game1.player.eventsSeen.Add(eventId);
		return false;
	}

	/// <summary>Today is one of the given days of month.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "u" })]
	public static bool DayOfMonth(GameLocation location, string eventId, string[] args)
	{
		bool foundDay = false;
		for (int i = 1; i < args.Length; i++)
		{
			if (!ArgUtility.TryGetInt(args, i, out var day, out var error))
			{
				return Event.LogPreconditionError(location, eventId, args, error);
			}
			if (Game1.dayOfMonth == day)
			{
				foundDay = true;
				break;
			}
		}
		return foundDay;
	}

	/// <summary>A festival day will occur within the given number of days.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	public static bool UpcomingFestival(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGetInt(args, 1, out var numberOfDays, out var error))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		Season season = Game1.season;
		int seasonIndex = Game1.seasonIndex;
		int day = Game1.dayOfMonth;
		for (int i = 0; i < numberOfDays; i++)
		{
			if (Utility.isFestivalDay(day, season))
			{
				return true;
			}
			day++;
			if (day > 28)
			{
				day = 1;
				season = (Season)((seasonIndex + 1) % 4);
			}
		}
		return false;
	}

	/// <summary>There is no festival planned within the given number of days.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[Obsolete("New events should use !UpcomingFestival instead.")]
	[OtherNames(new string[] { "U" })]
	public static bool NotUpcomingFestival(GameLocation location, string eventId, string[] args)
	{
		return !UpcomingFestival(location, eventId, args);
	}

	/// <summary>A game state query matches.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	[OtherNames(new string[] { "G" })]
	public static bool GameStateQuery(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGetRemainder(args, 1, out var query, out var _))
		{
			return Event.LogPreconditionError(location, eventId, args, "must specify a game state query");
		}
		return StardewValley.GameStateQuery.CheckConditions(query, location);
	}

	/// <summary>The current farmer has a minimum skill level.</summary>
	/// <inheritdoc cref="T:StardewValley.Delegates.EventPreconditionDelegate" />
	public static bool Skill(GameLocation location, string eventId, string[] args)
	{
		if (!ArgUtility.TryGet(args, 1, out var name, out var error, allowBlank: false) || !ArgUtility.TryGetInt(args, 2, out var minSkillLevel, out error))
		{
			return Event.LogPreconditionError(location, eventId, args, error);
		}
		int whichSkill = Farmer.getSkillNumberFromName(name);
		return Game1.player.GetUnmodifiedSkillLevel(whichSkill) >= minSkillLevel;
	}
}
