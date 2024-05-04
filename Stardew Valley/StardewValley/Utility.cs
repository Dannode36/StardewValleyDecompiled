using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Constants;
using StardewValley.Delegates;
using StardewValley.Events;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Characters;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.GameData.Weddings;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Network.NetEvents;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley;

/// <summary>Provides general utility methods for the game code.</summary>
/// <remarks>See also <see cref="T:StardewValley.ItemRegistry" /> for working with item IDs.</remarks>
public class Utility
{
	/// <summary>Indicates the reasons a horse can't be summoned by a player.</summary>
	[Flags]
	public enum HorseWarpRestrictions
	{
		/// <summary>No reasons apply.</summary>
		None = 0,
		/// <summary>The player doesn't own a horse.</summary>
		NoOwnedHorse = 1,
		/// <summary>The player is indoors (horses can't be summoned to an indoors location).</summary>
		Indoors = 2,
		/// <summary>There's no room near the player to place the horse.</summary>
		NoRoom = 4,
		/// <summary>The player's horse is currently in use by another player.</summary>
		InUse = 8
	}

	public static Color[] PRISMATIC_COLORS = new Color[6]
	{
		Color.Red,
		new Color(255, 120, 0),
		new Color(255, 217, 0),
		Color.Lime,
		Color.Cyan,
		Color.Violet
	};

	public static Item recentlyDiscoveredMissingBasicShippedItem;

	public static readonly Vector2[] DirectionsTileVectors = new Vector2[4]
	{
		new Vector2(0f, -1f),
		new Vector2(1f, 0f),
		new Vector2(0f, 1f),
		new Vector2(-1f, 0f)
	};

	public static readonly Vector2[] DirectionsTileVectorsWithDiagonals = new Vector2[8]
	{
		new Vector2(0f, -1f),
		new Vector2(1f, -1f),
		new Vector2(1f, 0f),
		new Vector2(1f, 1f),
		new Vector2(0f, 1f),
		new Vector2(-1f, 1f),
		new Vector2(-1f, 0f),
		new Vector2(-1f, -1f)
	};

	public static readonly RasterizerState ScissorEnabled = new RasterizerState
	{
		ScissorTestEnable = true
	};

	public static Microsoft.Xna.Framework.Rectangle controllerMapSourceRect(Microsoft.Xna.Framework.Rectangle xboxSourceRect)
	{
		return xboxSourceRect;
	}

	public static List<Vector2> removeDuplicates(List<Vector2> list)
	{
		for (int i = 0; i < list.Count; i++)
		{
			for (int j = list.Count - 1; j >= 0; j--)
			{
				if (j != i && list[i].Equals(list[j]))
				{
					list.RemoveAt(j);
				}
			}
		}
		return list;
	}

	/// <summary>Get the reasons a horse can't be summoned to the player currently, if any.</summary>
	/// <param name="who">The player requesting a horse.</param>
	public static HorseWarpRestrictions GetHorseWarpRestrictionsForFarmer(Farmer who)
	{
		HorseWarpRestrictions restrictions = HorseWarpRestrictions.None;
		if (who.horseName.Value == null)
		{
			restrictions |= HorseWarpRestrictions.NoOwnedHorse;
		}
		GameLocation currentLocation = who.currentLocation;
		if (!currentLocation.IsOutdoors)
		{
			restrictions |= HorseWarpRestrictions.Indoors;
		}
		Point playerTile = who.TilePoint;
		Microsoft.Xna.Framework.Rectangle horse_check_rect = new Microsoft.Xna.Framework.Rectangle(playerTile.X * 64, playerTile.Y * 64, 128, 64);
		if (currentLocation.isCollidingPosition(horse_check_rect, Game1.viewport, isFarmer: true, 0, glider: false, who))
		{
			restrictions |= HorseWarpRestrictions.NoRoom;
		}
		foreach (Farmer farmer in Game1.getOnlineFarmers())
		{
			if (farmer.mount != null && farmer.mount.getOwner() == who)
			{
				restrictions |= HorseWarpRestrictions.InUse;
				break;
			}
		}
		return restrictions;
	}

	/// <summary>Get the error message to show for a warp issue returned by <see cref="M:StardewValley.Utility.GetHorseWarpRestrictionsForFarmer(StardewValley.Farmer)" />.</summary>
	/// <param name="issue">The current issues preventing a warp, if any.</param>
	/// <returns>Returns the error message to display, or <c>null</c> if none apply.</returns>
	public static string GetHorseWarpErrorMessage(HorseWarpRestrictions issue)
	{
		if (issue.HasFlag(HorseWarpRestrictions.NoOwnedHorse))
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_NoHorse");
		}
		if (issue.HasFlag(HorseWarpRestrictions.Indoors))
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_InvalidLocation");
		}
		if (issue.HasFlag(HorseWarpRestrictions.NoRoom))
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_NoClearance");
		}
		if (issue.HasFlag(HorseWarpRestrictions.InUse))
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:HorseFlute_InUse");
		}
		return null;
	}

	public static Microsoft.Xna.Framework.Rectangle ConstrainScissorRectToScreen(Microsoft.Xna.Framework.Rectangle scissor_rect)
	{
		if (scissor_rect.Top < 0)
		{
			int amount_to_trim = -scissor_rect.Top;
			scissor_rect.Height -= amount_to_trim;
			scissor_rect.Y += amount_to_trim;
		}
		if (scissor_rect.Bottom > Game1.viewport.Height)
		{
			int amount_to_trim = scissor_rect.Bottom - Game1.viewport.Height;
			scissor_rect.Height -= amount_to_trim;
		}
		if (scissor_rect.Left < 0)
		{
			int amount_to_trim = -scissor_rect.Left;
			scissor_rect.Width -= amount_to_trim;
			scissor_rect.X += amount_to_trim;
		}
		if (scissor_rect.Right > Game1.viewport.Width)
		{
			int amount_to_trim = scissor_rect.Right - Game1.viewport.Width;
			scissor_rect.Width -= amount_to_trim;
		}
		return scissor_rect;
	}

	public static void RecordAnimalProduce(FarmAnimal animal, string produce)
	{
		if (animal.type.Contains("Cow"))
		{
			Game1.stats.CowMilkProduced++;
		}
		else if (animal.type.Contains("Sheep"))
		{
			Game1.stats.SheepWoolProduced++;
		}
		else if (animal.type.Contains("Goat"))
		{
			Game1.stats.GoatMilkProduced++;
		}
	}

	public static double getRandomDouble(double min, double max, Random random = null)
	{
		if (random == null)
		{
			random = Game1.random;
		}
		double range = max - min;
		return random.NextDouble() * range + min;
	}

	public static Vector2 getRandom360degreeVector(float speed)
	{
		Vector2 motion = new Vector2(0f, -1f);
		motion = Vector2.Transform(motion, Matrix.CreateRotationZ((float)getRandomDouble(0.0, Math.PI * 2.0)));
		motion.Normalize();
		return motion * speed;
	}

	public static Point Vector2ToPoint(Vector2 v)
	{
		return new Point((int)v.X, (int)v.Y);
	}

	public static Item getRaccoonSeedForCurrentTimeOfYear(Farmer who, Random r, int stackOverride = -1)
	{
		int number = r.Next(2, 4);
		while (r.NextDouble() < 0.1 + who.team.AverageDailyLuck())
		{
			number++;
		}
		Item i = null;
		Season season = Game1.season;
		if (Game1.dayOfMonth > ((season == Season.Spring) ? 23 : 20))
		{
			season = (Season)((int)(season + 1) % 4);
		}
		switch (season)
		{
		case Season.Spring:
			i = ItemRegistry.Create("(O)CarrotSeeds");
			break;
		case Season.Summer:
			i = ItemRegistry.Create("(O)SummerSquashSeeds");
			break;
		case Season.Fall:
			i = ItemRegistry.Create("(O)BroccoliSeeds");
			break;
		case Season.Winter:
			i = ItemRegistry.Create("(O)PowdermelonSeeds");
			break;
		}
		i.Stack = ((stackOverride == -1) ? number : stackOverride);
		return i;
	}

	public static Vector2 PointToVector2(Point p)
	{
		return new Vector2(p.X, p.Y);
	}

	public static int getStartTimeOfFestival()
	{
		if (Game1.weatherIcon == 1)
		{
			return Convert.ToInt32(ArgUtility.SplitBySpaceAndGet(Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + Game1.currentSeason + Game1.dayOfMonth)["conditions"].Split('/')[1], 0));
		}
		return -1;
	}

	public static bool doesMasterPlayerHaveMailReceivedButNotMailForTomorrow(string mailID)
	{
		if (Game1.MasterPlayer.mailReceived.Contains(mailID) || Game1.MasterPlayer.mailReceived.Contains(mailID + "%&NL&%"))
		{
			if (!Game1.MasterPlayer.mailForTomorrow.Contains(mailID))
			{
				return !Game1.MasterPlayer.mailForTomorrow.Contains(mailID + "%&NL&%");
			}
			return false;
		}
		return false;
	}

	/// <summary>Get whether there's a festival scheduled for today in any location.</summary>
	/// <remarks>This doesn't match passive festivals like the Night Market; see <see cref="M:StardewValley.Utility.IsPassiveFestivalDay" /> for those.</remarks>
	public static bool isFestivalDay()
	{
		return isFestivalDay(Game1.dayOfMonth, Game1.season, null);
	}

	/// <summary>Get whether there's a festival scheduled for today in the given location context.</summary>
	/// <param name="locationContext">The location context to check, usually matching a constant like <see cref="F:StardewValley.LocationContexts.DefaultId" />, or <c>null</c> for any context.</param>
	/// <inheritdoc cref="M:StardewValley.Utility.isFestivalDay" path="/remarks" />
	public static bool isFestivalDay(string locationContext)
	{
		return isFestivalDay(Game1.dayOfMonth, Game1.season, locationContext);
	}

	/// <summary>Get whether there's a festival scheduled on the given day in any location. This doesn't match passive festivals like the Night Market.</summary>
	/// <param name="day">The day of month to check.</param>
	/// <param name="season">The season key to check.</param>
	/// <inheritdoc cref="M:StardewValley.Utility.isFestivalDay" path="/remarks" />
	public static bool isFestivalDay(int day, Season season)
	{
		return isFestivalDay(day, season, null);
	}

	/// <summary>Get whether there's a festival scheduled on the given day and in the given location context. This doesn't match passive festivals like the Night Market.</summary>
	/// <param name="day">The day of month to check.</param>
	/// <param name="season">The season key to check.</param>
	/// <param name="locationContext">The location context to check, usually matching a constant like <see cref="F:StardewValley.LocationContexts.DefaultId" />, or <c>null</c> for any context.</param>
	/// <inheritdoc cref="M:StardewValley.Utility.isFestivalDay" path="/remarks" />
	public static bool isFestivalDay(int day, Season season, string locationContext)
	{
		string festivalId = $"{getSeasonKey(season)}{day}";
		if (!DataLoader.Festivals_FestivalDates(Game1.temporaryContent).ContainsKey(festivalId))
		{
			return false;
		}
		if (locationContext != null)
		{
			if (!Event.tryToLoadFestivalData(festivalId, out var _, out var _, out var locationName, out var _, out var _))
			{
				return false;
			}
			GameLocation location = Game1.getLocationFromName(locationName);
			if (location == null)
			{
				return false;
			}
			if (location.GetLocationContextId() != locationContext)
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Perform an action for each location in the game.</summary>
	/// <param name="action">The action to perform for each location. This should return true (continue iterating) or false (stop).</param>
	/// <param name="includeInteriors">Whether to include instanced building interiors that aren't in <see cref="P:StardewValley.Game1.locations" /> directly.</param>
	/// <param name="includeGenerated">Whether to include temporary generated locations like mine or volcano dungeon levels.</param>
	public static void ForEachLocation(Func<GameLocation, bool> action, bool includeInteriors = true, bool includeGenerated = false)
	{
		GameLocation currentLocation = Game1.currentLocation;
		string currentLocationName = currentLocation?.NameOrUniqueName;
		foreach (GameLocation rawLocation in Game1.locations)
		{
			GameLocation location = ((rawLocation.NameOrUniqueName == currentLocationName && currentLocation != null) ? currentLocation : rawLocation);
			if (!action(location))
			{
				return;
			}
			if (!includeInteriors)
			{
				continue;
			}
			bool shouldContinue = true;
			location.ForEachInstancedInterior(delegate(GameLocation interior)
			{
				if (action(interior))
				{
					return true;
				}
				shouldContinue = false;
				return false;
			});
			if (!shouldContinue)
			{
				return;
			}
		}
		if (!includeGenerated)
		{
			return;
		}
		foreach (MineShaft rawLevel in MineShaft.activeMines)
		{
			GameLocation level = ((rawLevel.NameOrUniqueName == currentLocationName && currentLocation != null) ? currentLocation : rawLevel);
			if (!action(level))
			{
				return;
			}
		}
		foreach (VolcanoDungeon rawLevel in VolcanoDungeon.activeLevels)
		{
			GameLocation level = ((rawLevel.NameOrUniqueName == currentLocationName && currentLocation != null) ? currentLocation : rawLevel);
			if (!action(level))
			{
				break;
			}
		}
	}

	/// <summary>Perform an action for each building in the game.</summary>
	/// <param name="action">The action to perform for each building. This should return true (continue iterating) or false (stop).</param>
	/// <param name="ignoreUnderConstruction">Whether to ignore buildings which haven't been fully constructed yet.</param>
	public static void ForEachBuilding(Func<Building, bool> action, bool ignoreUnderConstruction = true)
	{
		ForEachLocation(delegate(GameLocation location)
		{
			foreach (Building current in location.buildings)
			{
				if ((!ignoreUnderConstruction || !current.isUnderConstruction()) && !action(current))
				{
					return false;
				}
			}
			return true;
		}, includeInteriors: false);
	}

	public static List<Pet> getAllPets()
	{
		List<Pet> pets = new List<Pet>();
		foreach (NPC character in Game1.getFarm().characters)
		{
			if (character is Pet pet)
			{
				pets.Add(pet);
			}
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			foreach (NPC character2 in getHomeOfFarmer(allFarmer).characters)
			{
				if (character2 is Pet pet)
				{
					pets.Add(pet);
				}
			}
		}
		return pets;
	}

	/// <summary>Perform an action for each non-playable character in the game (including villagers, horses, pets, monsters, player children, etc).</summary>
	/// <param name="action">The action to perform for each character. This should return true (continue iterating) or false (stop).</param>
	/// <param name="includeEventActors">Whether to match temporary event actors.</param>
	/// <remarks>See also <see cref="M:StardewValley.Utility.ForEachVillager(System.Func{StardewValley.NPC,System.Boolean},System.Boolean)" />.</remarks>
	public static void ForEachCharacter(Func<NPC, bool> action, bool includeEventActors = false)
	{
		ForEachLocation(delegate(GameLocation location)
		{
			foreach (NPC current in location.characters)
			{
				if ((includeEventActors || !current.EventActor) && !action(current))
				{
					return false;
				}
			}
			return true;
		}, includeInteriors: true, includeGenerated: true);
	}

	/// <summary>Perform an action for each villager NPC in the game.</summary>
	/// <param name="action">The action to perform for each character. This should return true (continue iterating) or false (stop).</param>
	/// <param name="includeEventActors">Whether to match temporary event actors.</param>
	/// <remarks>See also <see cref="M:StardewValley.Utility.ForEachCharacter(System.Func{StardewValley.NPC,System.Boolean},System.Boolean)" />.</remarks>
	public static void ForEachVillager(Func<NPC, bool> action, bool includeEventActors = false)
	{
		ForEachLocation(delegate(GameLocation location)
		{
			foreach (NPC current in location.characters)
			{
				if ((includeEventActors || !current.EventActor) && current.IsVillager && !action(current))
				{
					return false;
				}
			}
			return true;
		});
	}

	/// <summary>Perform an action for each building in the game.</summary>
	/// <typeparam name="TBuilding">The expected building type.</typeparam>
	/// <param name="action">The action to perform for each building. This should return true (continue iterating) or false (stop).</param>
	/// <param name="ignoreUnderConstruction">Whether to ignore buildings which haven't been fully constructed yet.</param>
	public static void ForEachBuilding<TBuilding>(Func<TBuilding, bool> action, bool ignoreUnderConstruction = true) where TBuilding : Building
	{
		ForEachLocation(delegate(GameLocation location)
		{
			foreach (Building building in location.buildings)
			{
				if (building is TBuilding val && (!ignoreUnderConstruction || !val.isUnderConstruction()) && !action(val))
				{
					return false;
				}
			}
			return true;
		}, includeInteriors: false);
	}

	/// <summary>Perform an action for each planted crop in the game.</summary>
	/// <param name="action">The action to perform for each crop. This should return true (continue iterating) or false (stop).</param>
	public static void ForEachCrop(Func<Crop, bool> action)
	{
		ForEachLocation(delegate(GameLocation location)
		{
			foreach (TerrainFeature value in location.terrainFeatures.Values)
			{
				Crop crop = (value as HoeDirt)?.crop;
				if (crop != null && !action(crop))
				{
					return false;
				}
			}
			foreach (Object value2 in location.objects.Values)
			{
				Crop crop2 = (value2 as IndoorPot)?.hoeDirt.Value?.crop;
				if (crop2 != null && !action(crop2))
				{
					return false;
				}
			}
			return true;
		});
	}

	/// <summary>Perform an action for each item in the game world, including items within items (e.g. in a chest or on a table), hats placed on children, items in player inventories, etc.</summary>
	/// <param name="action">The action to perform for each item. This should return true (continue iterating) or false (stop).</param>
	/// <returns>Returns whether to continue iterating if needed (i.e. returns false if the last <paramref name="action" /> call did).</returns>
	public static bool ForEachItem(Func<Item, bool> action)
	{
		return ForEachItemHelper.ForEachItemInWorld((Item item, Action remove, Action<Item> replaceWith) => action(item));
	}

	/// <summary>Perform an action for each item in the game world, including items within items (e.g. in a chest or on a table), hats placed on children, items in player inventories, etc.</summary>
	/// <param name="handler">The action to perform for each item.</param>
	/// <returns>Returns whether to continue iterating if needed (i.e. returns false if the last <paramref name="handler" /> call did).</returns>
	public static bool ForEachItem(ForEachItemDelegate handler)
	{
		return ForEachItemHelper.ForEachItemInWorld(handler);
	}

	/// <summary>Perform an action for each item within a location, including items within items (e.g. in a chest or on a table), hats placed on children, items in player inventories, etc.</summary>
	/// <param name="location">The location whose items to iterate.</param>
	/// <param name="action">The action to perform for each item. This should return true (continue iterating) or false (stop).</param>
	/// <returns>Returns whether to continue iterating if needed (i.e. returns false if the last <paramref name="action" /> call did).</returns>
	public static bool ForEachItemIn(GameLocation location, Func<Item, bool> action)
	{
		return ForEachItemHelper.ForEachItemInLocation(location, (Item item, Action remove, Action<Item> replaceWith) => action(item));
	}

	/// <summary>Perform an action for each item within a location, including items within items (e.g. in a chest or on a table), hats placed on children, items in player inventories, etc.</summary>
	/// <param name="location">The location whose items to iterate.</param>
	/// <param name="handler">The action to perform for each item.</param>
	/// <returns>Returns whether to continue iterating if needed (i.e. returns false if the last <paramref name="handler" /> call did).</returns>
	public static bool ForEachItemIn(GameLocation location, ForEachItemDelegate handler)
	{
		return ForEachItemHelper.ForEachItemInLocation(location, handler);
	}

	public static int getNumObjectsOfIndexWithinRectangle(Microsoft.Xna.Framework.Rectangle r, string[] indexes, GameLocation location)
	{
		int count = 0;
		Vector2 v = Vector2.Zero;
		for (int y = r.Y; y < r.Bottom + 1; y++)
		{
			v.Y = y;
			for (int x = r.X; x < r.Right + 1; x++)
			{
				v.X = x;
				if (!location.objects.TryGetValue(v, out var obj))
				{
					continue;
				}
				foreach (string itemId in indexes)
				{
					if (itemId == null || ItemRegistry.HasItemId(obj, itemId))
					{
						count++;
						break;
					}
				}
			}
		}
		return count;
	}

	/// <summary>Try to parse a string as a valid enum value.</summary>
	/// <typeparam name="TEnum">The enum type.</typeparam>
	/// <param name="value">The raw value to parse. This is not case-sensitive.</param>
	/// <param name="parsed">The parsed enum value, if valid.</param>
	/// <returns>Returns whether the value was successfully parsed as an enum.</returns>
	public static bool TryParseEnum<TEnum>(string value, out TEnum parsed) where TEnum : struct
	{
		if (Enum.TryParse<TEnum>(value, ignoreCase: true, out parsed))
		{
			if (typeof(TEnum).IsEnumDefined(parsed))
			{
				return true;
			}
			if (typeof(TEnum).GetCustomAttribute<FlagsAttribute>() != null && !long.TryParse(parsed.ToString(), out var _))
			{
				return true;
			}
		}
		parsed = default(TEnum);
		return false;
	}

	/// <summary>Get an enum value if it's valid, else get a default value.</summary>
	/// <typeparam name="TEnum">The enum type.</typeparam>
	/// <param name="value">The unvalidated enum value.</param>
	/// <param name="defaultValue">The value to return if invalid.</param>
	/// <returns>Returns <paramref name="value" /> if it matches one of the enum constants, else <paramref name="defaultValue" />.</returns>
	public static TEnum GetEnumOrDefault<TEnum>(TEnum value, TEnum defaultValue) where TEnum : struct
	{
		if (!typeof(TEnum).IsEnumDefined(value))
		{
			return defaultValue;
		}
		return value;
	}

	/// <summary>Trim whitespace at the start and end of each line in the given text.</summary>
	/// <param name="text">The text whose lines to trim.</param>
	public static string TrimLines(string text)
	{
		text = text?.Trim();
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		string[] lines = LegacyShims.SplitAndTrim(text, '\n');
		if (lines.Length <= 1)
		{
			return text;
		}
		return string.Join("\n", lines);
	}

	public static bool IsLegacyIdAbove(string itemId, int lowerBound)
	{
		if (int.TryParse(itemId, out var legacyId))
		{
			return legacyId > lowerBound;
		}
		return false;
	}

	public static bool IsLegacyIdBetween(string itemId, int lowerBound, int upperBound)
	{
		if (int.TryParse(itemId, out var legacyId) && legacyId >= lowerBound)
		{
			return legacyId <= upperBound;
		}
		return false;
	}

	/// <summary>Find the best match for a search term based on fuzzy compare rules.</summary>
	/// <param name="query">The fuzzy search query to match.</param>
	/// <param name="terms">The terms from which to choose a match.</param>
	/// <returns>Returns the best match for the query, or <c>null</c> if no match was found.</returns>
	public static string fuzzySearch(string query, ICollection<string> terms)
	{
		int? bestPriority = null;
		string bestMatch = null;
		foreach (string term in terms)
		{
			int? priority = fuzzyCompare(query, term);
			if (priority.HasValue && (!bestPriority.HasValue || priority < bestPriority))
			{
				bestPriority = priority;
				bestMatch = term;
			}
		}
		return bestMatch;
	}

	/// <summary>Get whether a term is a fuzzy match for a search query.</summary>
	/// <param name="query">The fuzzy search query to match.</param>
	/// <param name="term">The actual value to compare against the query.</param>
	/// <returns>Returns the numeric match priority (where lower values are a better match), or <c>null</c> if the term doesn't match the query.</returns>
	public static int? fuzzyCompare(string query, string term)
	{
		if (query.Trim() == term.Trim())
		{
			return 0;
		}
		string formattedQuery = FormatForFuzzySearch(query);
		string formattedTerm = FormatForFuzzySearch(term);
		if (formattedQuery == formattedTerm)
		{
			return 1;
		}
		if (formattedTerm.StartsWith(formattedQuery))
		{
			return 2;
		}
		if (formattedTerm.Contains(formattedQuery))
		{
			return 3;
		}
		return null;
		static string FormatForFuzzySearch(string value)
		{
			string minimalFormatted = value.Trim().ToLowerInvariant().Replace(" ", "");
			string formatted = minimalFormatted.Replace("(", "").Replace(")", "").Replace("'", "")
				.Replace(".", "")
				.Replace("!", "")
				.Replace("?", "")
				.Replace("-", "");
			if (formatted.Length != 0)
			{
				return formatted;
			}
			return minimalFormatted;
		}
	}

	public static Item fuzzyItemSearch(string query, int stack_count = 1, bool useLocalizedNames = false)
	{
		Dictionary<string, string> items = new Dictionary<string, string>();
		foreach (IItemDataDefinition itemType in ItemRegistry.ItemTypes)
		{
			foreach (string itemId in itemType.GetAllIds())
			{
				ParsedItemData itemData = itemType.GetData(itemId);
				string itemName = (useLocalizedNames ? itemData.DisplayName : itemData.InternalName);
				if (!items.ContainsKey(itemName))
				{
					items[itemName] = itemType.Identifier + itemId;
				}
			}
		}
		ParsedItemData stoneData = ItemRegistry.GetData("(O)390");
		if (stoneData != null)
		{
			string stoneName = (useLocalizedNames ? stoneData.DisplayName : stoneData.InternalName);
			items[stoneName] = "(O)390";
		}
		string result = fuzzySearch(query, items.Keys);
		if (result != null)
		{
			return ItemRegistry.Create(items[result], stack_count);
		}
		return null;
	}

	public static GameLocation fuzzyLocationSearch(string query)
	{
		Dictionary<string, GameLocation> name_bank = new Dictionary<string, GameLocation>();
		ForEachLocation(delegate(GameLocation location)
		{
			name_bank[location.NameOrUniqueName] = location;
			return true;
		});
		string location_name = fuzzySearch(query, name_bank.Keys);
		if (location_name == null)
		{
			return null;
		}
		return name_bank[location_name];
	}

	public static string AOrAn(string text)
	{
		if (text != null && text.Length > 0)
		{
			char letter = text.ToLowerInvariant()[0];
			if (letter == 'a' || letter == 'e' || letter == 'i' || letter == 'o' || letter == 'u')
			{
				if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.hu)
				{
					return "az";
				}
				return "an";
			}
		}
		return "a";
	}

	/// <summary>Get the default tile position where the player should be placed when they arrive in a location, if arriving from a warp that didn't specify a tile position.</summary>
	/// <param name="locationName">The <see cref="P:StardewValley.GameLocation.Name" /> value for the target location.</param>
	/// <param name="x">The default X tile position.</param>
	/// <param name="y">The default Y tile position.</param>
	public static void getDefaultWarpLocation(string locationName, ref int x, ref int y)
	{
		GameLocation location = Game1.getLocationFromName(locationName);
		if (location != null && location.TryGetMapPropertyAs("DefaultWarpLocation", out Point position, required: false))
		{
			x = position.X;
			y = position.Y;
			return;
		}
		if (location is Farm farm)
		{
			Point tile = farm.GetMainFarmHouseEntry();
			if (tile != Point.Zero)
			{
				x = tile.X;
				y = tile.Y;
			}
		}
		Point? arrivalTile = GameLocation.GetData(locationName)?.DefaultArrivalTile;
		if (arrivalTile.HasValue)
		{
			x = arrivalTile.Value.X;
			y = arrivalTile.Value.Y;
			return;
		}
		switch (locationName)
		{
		case "Barn":
		case "Barn2":
		case "Barn3":
			x = 11;
			y = 13;
			return;
		case "Coop":
		case "Coop2":
		case "Coop3":
			x = 2;
			y = 8;
			return;
		case "Farm":
			x = 64;
			y = 15;
			return;
		case "SlimeHutch":
			x = 8;
			y = 18;
			return;
		}
		if (location != null && location.TryGetMapProperty("Warp", out var warps))
		{
			string[] warpExtract = warps.Split(' ');
			Vector2 warpLoc = recursiveFindOpenTileForCharacter(tileLocation: new Vector2(Convert.ToInt32(warpExtract[0]), Convert.ToInt32(warpExtract[1])), c: Game1.player, l: Game1.getLocationFromName(locationName), maxIterations: 10, allowOffMap: false);
			x = (int)warpLoc.X;
			y = (int)warpLoc.Y;
		}
	}

	public static FarmAnimal fuzzyAnimalSearch(string query)
	{
		List<FarmAnimal> animals = new List<FarmAnimal>();
		ForEachLocation(delegate(GameLocation location)
		{
			animals.AddRange(location.Animals.Values);
			return true;
		});
		Dictionary<string, FarmAnimal> name_bank = new Dictionary<string, FarmAnimal>();
		foreach (FarmAnimal animal in animals)
		{
			name_bank[animal.Name] = animal;
		}
		string character_name = fuzzySearch(query, name_bank.Keys);
		if (character_name == null)
		{
			return null;
		}
		return name_bank[character_name];
	}

	public static NPC fuzzyCharacterSearch(string query, bool must_be_villager = true)
	{
		Dictionary<string, NPC> name_bank = new Dictionary<string, NPC>();
		ForEachCharacter(delegate(NPC character)
		{
			if (!must_be_villager || character.IsVillager)
			{
				name_bank[character.Name] = character;
			}
			return true;
		});
		string character_name = fuzzySearch(query, name_bank.Keys);
		if (character_name == null)
		{
			return null;
		}
		return name_bank[character_name];
	}

	public static Color GetPrismaticColor(int offset = 0, float speedMultiplier = 1f)
	{
		float interval = 1500f;
		int current_index = ((int)((float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds * speedMultiplier / interval) + offset) % PRISMATIC_COLORS.Length;
		int next_index = (current_index + 1) % PRISMATIC_COLORS.Length;
		float position = (float)Game1.currentGameTime.TotalGameTime.TotalMilliseconds * speedMultiplier / interval % 1f;
		Color prismatic_color = default(Color);
		prismatic_color.R = (byte)(Lerp((float)(int)PRISMATIC_COLORS[current_index].R / 255f, (float)(int)PRISMATIC_COLORS[next_index].R / 255f, position) * 255f);
		prismatic_color.G = (byte)(Lerp((float)(int)PRISMATIC_COLORS[current_index].G / 255f, (float)(int)PRISMATIC_COLORS[next_index].G / 255f, position) * 255f);
		prismatic_color.B = (byte)(Lerp((float)(int)PRISMATIC_COLORS[current_index].B / 255f, (float)(int)PRISMATIC_COLORS[next_index].B / 255f, position) * 255f);
		prismatic_color.A = (byte)(Lerp((float)(int)PRISMATIC_COLORS[current_index].A / 255f, (float)(int)PRISMATIC_COLORS[next_index].A / 255f, position) * 255f);
		return prismatic_color;
	}

	public static Color Get2PhaseColor(Color color1, Color color2, int offset = 0, float speedMultiplier = 1f, float timeOffset = 0f)
	{
		float interval = 1500f;
		int num = ((int)((float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)timeOffset) * speedMultiplier / interval) + offset) % 2;
		float position = (float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)timeOffset) * speedMultiplier / interval % 1f;
		Color prismatic_color = default(Color);
		Color a = ((num == 0) ? color1 : color2);
		Color b = ((num == 0) ? color2 : color1);
		prismatic_color.R = (byte)(Lerp((float)(int)a.R / 255f, (float)(int)b.R / 255f, position) * 255f);
		prismatic_color.G = (byte)(Lerp((float)(int)a.G / 255f, (float)(int)b.G / 255f, position) * 255f);
		prismatic_color.B = (byte)(Lerp((float)(int)a.B / 255f, (float)(int)b.B / 255f, position) * 255f);
		prismatic_color.A = (byte)(Lerp((float)(int)a.A / 255f, (float)(int)b.A / 255f, position) * 255f);
		return prismatic_color;
	}

	public static bool IsNormalObjectAtParentSheetIndex(Item item, string itemId)
	{
		if (item.HasTypeObject() && item.GetType() == typeof(Object))
		{
			return item.ItemId == itemId;
		}
		return false;
	}

	public static Microsoft.Xna.Framework.Rectangle getSafeArea()
	{
		Microsoft.Xna.Framework.Rectangle area = Game1.game1.GraphicsDevice.Viewport.GetTitleSafeArea();
		if (Game1.game1.GraphicsDevice.GetRenderTargets().Length == 0)
		{
			float oneOverZoomLevel = 1f / Game1.options.zoomLevel;
			if (Game1.uiMode)
			{
				oneOverZoomLevel = 1f / Game1.options.uiScale;
			}
			area.X = (int)((float)area.X * oneOverZoomLevel);
			area.Y = (int)((float)area.Y * oneOverZoomLevel);
			area.Width = (int)((float)area.Width * oneOverZoomLevel);
			area.Height = (int)((float)area.Height * oneOverZoomLevel);
		}
		return area;
	}

	/// <summary>
	/// Return the adjusted renderPos such that bounds implied by renderSize
	/// is within the TitleSafeArea.
	///
	/// If it already is, renderPos is returned unmodified.
	/// </summary>
	public static Vector2 makeSafe(Vector2 renderPos, Vector2 renderSize)
	{
		int x = (int)renderPos.X;
		int y = (int)renderPos.Y;
		int w = (int)renderSize.X;
		int h = (int)renderSize.Y;
		makeSafe(ref x, ref y, w, h);
		return new Vector2(x, y);
	}

	public static void makeSafe(ref Vector2 position, int width, int height)
	{
		int x = (int)position.X;
		int y = (int)position.Y;
		makeSafe(ref x, ref y, width, height);
		position.X = x;
		position.Y = y;
	}

	public static void makeSafe(ref Microsoft.Xna.Framework.Rectangle bounds)
	{
		makeSafe(ref bounds.X, ref bounds.Y, bounds.Width, bounds.Height);
	}

	public static void makeSafe(ref int x, ref int y, int width, int height)
	{
		Microsoft.Xna.Framework.Rectangle area = getSafeArea();
		if (x < area.Left)
		{
			x = area.Left;
		}
		if (y < area.Top)
		{
			y = area.Top;
		}
		if (x + width > area.Right)
		{
			x = area.Right - width;
		}
		if (y + height > area.Bottom)
		{
			y = area.Bottom - height;
		}
	}

	public static int makeSafeMarginY(int marginy)
	{
		Viewport vp = Game1.game1.GraphicsDevice.Viewport;
		Microsoft.Xna.Framework.Rectangle area = getSafeArea();
		int m = area.Top - vp.Bounds.Top;
		if (m > marginy)
		{
			marginy = m;
		}
		m = vp.Bounds.Bottom - area.Bottom;
		if (m > marginy)
		{
			marginy = m;
		}
		return marginy;
	}

	public static int CompareGameVersions(string version, string other_version, bool ignore_platform_specific = false)
	{
		string[] split = version.Split('.');
		string[] other_split = other_version.Split('.');
		for (int i = 0; i < Math.Max(split.Length, other_split.Length); i++)
		{
			float version_number = 0f;
			float other_version_number = 0f;
			if (i < split.Length)
			{
				float.TryParse(split[i], out version_number);
			}
			if (i < other_split.Length)
			{
				float.TryParse(other_split[i], out other_version_number);
			}
			if (version_number != other_version_number || (i == 2 && ignore_platform_specific))
			{
				return version_number.CompareTo(other_version_number);
			}
		}
		return 0;
	}

	public static float getFarmerItemsShippedPercent(Farmer who = null)
	{
		if (who == null)
		{
			who = Game1.player;
		}
		recentlyDiscoveredMissingBasicShippedItem = null;
		int farmerShipped = 0;
		int total = 0;
		foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
		{
			int category = data.Category;
			if (category != -7 && category != -2 && Object.isPotentialBasicShipped(data.ItemId, data.Category, data.ObjectType))
			{
				total++;
				if (who.basicShipped.ContainsKey(data.ItemId))
				{
					farmerShipped++;
				}
				else if (recentlyDiscoveredMissingBasicShippedItem == null)
				{
					recentlyDiscoveredMissingBasicShippedItem = ItemRegistry.Create(data.QualifiedItemId);
				}
			}
		}
		return (float)farmerShipped / (float)total;
	}

	public static bool hasFarmerShippedAllItems()
	{
		return getFarmerItemsShippedPercent() >= 1f;
	}

	public static NPC getTodaysBirthdayNPC()
	{
		NPC match = null;
		ForEachVillager(delegate(NPC n)
		{
			if (n.isBirthday())
			{
				match = n;
			}
			return match == null;
		});
		return match;
	}

	/// <summary>Create a <see cref="T:System.Random" /> instance using the save ID and days played as a seed.</summary>
	/// <param name="seedA">The first extra value to add to the RNG seed, if any.</param>
	/// <param name="seedB">The second extra value to add to the RNG seed, if any.</param>
	/// <param name="seedC">The third extra value to add to the RNG seed, if any.</param>
	public static Random CreateDaySaveRandom(double seedA = 0.0, double seedB = 0.0, double seedC = 0.0)
	{
		return CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame / 2, seedA, seedB, seedC);
	}

	/// <summary>Get an RNG seeded with the same value when called within the specified period.</summary>
	/// <param name="interval">The time interval within which the random seed should be consistent.</param>
	/// <param name="key">A key which identifies the random instance being created, if any. Instances with a different key will have a different seed.</param>
	/// <param name="random">The created RNG, if valid.</param>
	/// <param name="error">An error indicating why the RNG could not be created, if applicable.</param>
	/// <returns>Returns whether the interval is valid and the RNG was created.</returns>
	public static bool TryCreateIntervalRandom(string interval, string key, out Random random, out string error)
	{
		int seed = ((key != null) ? Game1.hash.GetDeterministicHashCode(key) : 0);
		error = null;
		double intervalSeed;
		switch (interval.ToLower())
		{
		case "tick":
			intervalSeed = Game1.ticks;
			break;
		case "day":
			intervalSeed = Game1.stats.DaysPlayed;
			break;
		case "season":
			intervalSeed = Game1.hash.GetDeterministicHashCode(Game1.currentSeason + Game1.year);
			break;
		case "year":
			intervalSeed = Game1.hash.GetDeterministicHashCode("year" + Game1.year);
			break;
		default:
			error = "invalid interval '" + interval + "'; expected one of 'tick', 'day', 'season', or 'year'";
			random = null;
			return false;
		}
		random = CreateRandom(seed, Game1.uniqueIDForThisGame, intervalSeed);
		return true;
	}

	/// <summary>Create a <see cref="T:System.Random" /> instance which safely combines the given seed values.</summary>
	/// <param name="seedA">The first seed value to combine.</param>
	/// <param name="seedB">The second seed value to combine.</param>
	/// <param name="seedC">The third seed value to combine.</param>
	/// <param name="seedD">The fourth seed value to combine.</param>
	/// <param name="seedE">The fifth seed value to combine.</param>
	public static Random CreateRandom(double seedA, double seedB = 0.0, double seedC = 0.0, double seedD = 0.0, double seedE = 0.0)
	{
		return new Random(CreateRandomSeed(seedA, seedB, seedC, seedD, seedE));
	}

	/// <summary>Safely combine seed values for use as a <see cref="T:System.Random" /> seed.</summary>
	/// <param name="seedA">The first seed value to combine.</param>
	/// <param name="seedB">The second seed value to combine.</param>
	/// <param name="seedC">The third seed value to combine.</param>
	/// <param name="seedD">The fourth seed value to combine.</param>
	/// <param name="seedE">The fifth seed value to combine.</param>
	public static int CreateRandomSeed(double seedA, double seedB, double seedC = 0.0, double seedD = 0.0, double seedE = 0.0)
	{
		if (Game1.UseLegacyRandom)
		{
			return (int)((seedA % 2147483647.0 + seedB % 2147483647.0 + seedC % 2147483647.0 + seedD % 2147483647.0 + seedE % 2147483647.0) % 2147483647.0);
		}
		return Game1.hash.GetDeterministicHashCode((int)(seedA % 2147483647.0), (int)(seedB % 2147483647.0), (int)(seedC % 2147483647.0), (int)(seedD % 2147483647.0), (int)(seedE % 2147483647.0));
	}

	/// <summary>Get a random entry from a dictionary.</summary>
	/// <typeparam name="TKey">The dictionary key type.</typeparam>
	/// <typeparam name="TValue">The dictionary value type.</typeparam>
	/// <param name="dictionary">The list whose entries to get.</param>
	/// <param name="key">The random entry's key, if found.</param>
	/// <param name="value">The random entry's value, if found.</param>
	/// <param name="random">The RNG to use, or <c>null</c> for <see cref="F:StardewValley.Game1.random" />.</param>
	/// <returns>Returns whether an entry was found.</returns>
	public static bool TryGetRandom<TKey, TValue>(IDictionary<TKey, TValue> dictionary, out TKey key, out TValue value, Random random = null)
	{
		if (dictionary == null || dictionary.Count == 0)
		{
			key = default(TKey);
			value = default(TValue);
			return false;
		}
		if (random == null)
		{
			random = Game1.random;
		}
		KeyValuePair<TKey, TValue> pair = dictionary.ElementAt(random.Next(dictionary.Count));
		key = pair.Key;
		value = pair.Value;
		return true;
	}

	/// <inheritdoc cref="M:StardewValley.Utility.TryGetRandom``2(System.Collections.Generic.IDictionary{``0,``1},``0@,``1@,System.Random)" />
	public static bool TryGetRandom<TKey, TValue, TField, TSerialDict, TSelf>(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> dictionary, out TKey key, out TValue value, Random random = null) where TField : class, INetObject<INetSerializable>, new() where TSerialDict : IDictionary<TKey, TValue>, new() where TSelf : NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>
	{
		if (dictionary == null || dictionary.Length == 0)
		{
			key = default(TKey);
			value = default(TValue);
			return false;
		}
		if (random == null)
		{
			random = Game1.random;
		}
		KeyValuePair<TKey, TValue> pair = dictionary.Pairs.ElementAt(random.Next(dictionary.Length));
		key = pair.Key;
		value = pair.Value;
		return true;
	}

	/// <inheritdoc cref="M:StardewValley.Utility.TryGetRandom``2(System.Collections.Generic.IDictionary{``0,``1},``0@,``1@,System.Random)" />
	public static bool TryGetRandom(OverlaidDictionary dictionary, out Vector2 key, out Object value, Random random = null)
	{
		if (dictionary == null || dictionary.Length == 0)
		{
			key = Vector2.Zero;
			value = null;
			return false;
		}
		if (random == null)
		{
			random = Game1.random;
		}
		KeyValuePair<Vector2, Object> pair = dictionary.Pairs.ElementAt(random.Next(dictionary.Length));
		key = pair.Key;
		value = pair.Value;
		return true;
	}

	/// <summary>Get a random entry from a list, ignoring specific values.</summary>
	/// <typeparam name="T">The list item type.</typeparam>
	/// <param name="list">The values to choose from.</param>
	/// <param name="except">The values to ignore in the <paramref name="list" />.</param>
	/// <param name="random">The random number generator to use.</param>
	/// <param name="selected">The selected value.</param>
	/// <returns>Returns whether a value was selected.</returns>
	public static bool TryGetRandomExcept<T>(IList<T> list, ISet<T> except, Random random, out T selected)
	{
		if (list == null || list.Count == 0)
		{
			selected = default(T);
			return false;
		}
		if (except == null || except.Count == 0)
		{
			selected = random.ChooseFrom(list);
			return true;
		}
		T[] filtered = list.Except(except).ToArray();
		selected = random.ChooseFrom(filtered);
		return true;
	}

	public static string getRandomSingleTileFurniture(Random r)
	{
		return r.Next(3) switch
		{
			0 => "(F)" + r.Next(10) * 3, 
			1 => "(F)" + r.Next(1376, 1391), 
			_ => "(F)" + (r.Next(6) * 2 + 1391), 
		};
	}

	public static void improveFriendshipWithEveryoneInRegion(Farmer who, int amount, string region)
	{
		ForEachLocation(delegate(GameLocation l)
		{
			foreach (NPC current in l.characters)
			{
				if (current.GetData()?.HomeRegion == region && who.friendshipData.ContainsKey(current.Name))
				{
					who.changeFriendship(amount, current);
				}
			}
			return true;
		});
	}

	/// <summary>Get a random Winter Star gift which an NPC can give to players.</summary>
	/// <param name="who">The NPC giving the gift.</param>
	public static Item getGiftFromNPC(NPC who)
	{
		Random giftRandom = CreateRandom(Game1.uniqueIDForThisGame / 2, Game1.year, Game1.dayOfMonth, Game1.seasonIndex, who.TilePoint.X);
		List<Item> gifts = new List<Item>();
		CharacterData data = who.GetData();
		List<GenericSpawnItemDataWithCondition> winterStarGifts = data.WinterStarGifts;
		if (winterStarGifts != null && winterStarGifts.Count > 0)
		{
			ItemQueryContext itemQueryContext = new ItemQueryContext(Game1.currentLocation, Game1.player, giftRandom);
			foreach (GenericSpawnItemDataWithCondition entry in data.WinterStarGifts)
			{
				if (GameStateQuery.CheckConditions(entry.Condition, null, null, null, null, giftRandom))
				{
					Item result = ItemQueryResolver.TryResolveRandomItem(entry, itemQueryContext, avoidRepeat: false, null, null, null, delegate(string query, string error)
					{
						Game1.log.Error($"{who.Name} failed parsing item query '{query}' for winter star gift entry '{entry.Id}': {error}");
					});
					if (result != null)
					{
						gifts.Add(result);
					}
				}
			}
		}
		if (gifts.Count == 0)
		{
			if (who.Age == 2)
			{
				gifts.AddRange(new Item[4]
				{
					ItemRegistry.Create("(O)330"),
					ItemRegistry.Create("(O)103"),
					ItemRegistry.Create("(O)394"),
					ItemRegistry.Create("(O)" + giftRandom.Next(535, 538))
				});
			}
			else
			{
				gifts.AddRange(new Item[14]
				{
					ItemRegistry.Create("(O)608"),
					ItemRegistry.Create("(O)651"),
					ItemRegistry.Create("(O)611"),
					ItemRegistry.Create("(O)517"),
					ItemRegistry.Create("(O)466", 10),
					ItemRegistry.Create("(O)422"),
					ItemRegistry.Create("(O)392"),
					ItemRegistry.Create("(O)348"),
					ItemRegistry.Create("(O)346"),
					ItemRegistry.Create("(O)341"),
					ItemRegistry.Create("(O)221"),
					ItemRegistry.Create("(O)64"),
					ItemRegistry.Create("(O)60"),
					ItemRegistry.Create("(O)70")
				});
			}
		}
		return giftRandom.ChooseFrom(gifts);
	}

	public static NPC getTopRomanticInterest(Farmer who)
	{
		NPC topSpot = null;
		int highestFriendPoints = -1;
		ForEachVillager(delegate(NPC n)
		{
			if (who.friendshipData.ContainsKey(n.Name) && (bool)n.datable && who.getFriendshipLevelForNPC(n.Name) > highestFriendPoints)
			{
				topSpot = n;
				highestFriendPoints = who.getFriendshipLevelForNPC(n.Name);
			}
			return true;
		});
		return topSpot;
	}

	public static Color getRandomRainbowColor(Random r = null)
	{
		return (r?.Next(8) ?? Game1.random.Next(8)) switch
		{
			0 => Color.Red, 
			1 => Color.Orange, 
			2 => Color.Yellow, 
			3 => Color.Lime, 
			4 => Color.Cyan, 
			5 => new Color(0, 100, 255), 
			6 => new Color(152, 96, 255), 
			7 => new Color(255, 100, 255), 
			_ => Color.White, 
		};
	}

	public static NPC getTopNonRomanticInterest(Farmer who)
	{
		NPC topSpot = null;
		int highestFriendPoints = -1;
		ForEachVillager(delegate(NPC n)
		{
			if (who.friendshipData.ContainsKey(n.Name) && !n.datable && who.getFriendshipLevelForNPC(n.Name) > highestFriendPoints)
			{
				topSpot = n;
				highestFriendPoints = who.getFriendshipLevelForNPC(n.Name);
			}
			return true;
		});
		return topSpot;
	}

	/// <summary>Get which of a player's skills has the highest number of experience points.</summary>
	/// <param name="who">The player whose skills to check.</param>
	public static int getHighestSkill(Farmer who)
	{
		int topSkillExperience = 0;
		int topSkill = 0;
		for (int i = 0; i < who.experiencePoints.Length; i++)
		{
			int experiencePoints = who.experiencePoints[i];
			if (who.experiencePoints[i] > topSkillExperience)
			{
				topSkillExperience = experiencePoints;
				topSkill = i;
			}
		}
		return topSkill;
	}

	public static int getNumberOfFriendsWithinThisRange(Farmer who, int minFriendshipPoints, int maxFriendshipPoints, bool romanceOnly = false)
	{
		int number = 0;
		ForEachVillager(delegate(NPC n)
		{
			int? num = who.tryGetFriendshipLevelForNPC(n.Name);
			if (num.HasValue && num.Value >= minFriendshipPoints && num.Value <= maxFriendshipPoints && (!romanceOnly || (bool)n.datable))
			{
				number++;
			}
			return true;
		});
		return number;
	}

	public static bool highlightLuauSoupItems(Item i)
	{
		if (i is Object obj)
		{
			if (((int)obj.edibility == -300 || obj.Category == -7) && !(obj.QualifiedItemId == "(O)789"))
			{
				return obj.QualifiedItemId == "(O)71";
			}
			return true;
		}
		return false;
	}

	public static bool highlightSmallObjects(Item i)
	{
		if (i is Object obj)
		{
			return !obj.bigCraftable;
		}
		return false;
	}

	public static bool highlightSantaObjects(Item i)
	{
		if (!i.canBeTrashed() || !i.canBeGivenAsGift())
		{
			return false;
		}
		return highlightSmallObjects(i);
	}

	public static bool highlightShippableObjects(Item i)
	{
		if (i is Object obj)
		{
			return obj.canBeShipped();
		}
		return false;
	}

	public static int getFarmerNumberFromFarmer(Farmer who)
	{
		if (who != null)
		{
			if (who.IsMainPlayer)
			{
				return 1;
			}
			int farmerNumber = 2;
			foreach (Farmer item in from f in Game1.otherFarmers.Values
				orderby f.UniqueMultiplayerID
				where !f.IsMainPlayer
				select f)
			{
				if (item.UniqueMultiplayerID == who.UniqueMultiplayerID)
				{
					return farmerNumber;
				}
				farmerNumber++;
			}
		}
		return -1;
	}

	public static Farmer getFarmerFromFarmerNumber(int number)
	{
		if (number <= 1)
		{
			return Game1.MasterPlayer;
		}
		int curNumber = 2;
		foreach (Farmer player in from f in Game1.otherFarmers.Values
			orderby f.UniqueMultiplayerID
			where !f.IsMainPlayer
			select f)
		{
			if (curNumber == number)
			{
				return player;
			}
			curNumber++;
		}
		return null;
	}

	public static string getLoveInterest(string who)
	{
		return who switch
		{
			"Haley" => "Alex", 
			"Sam" => "Penny", 
			"Alex" => "Haley", 
			"Penny" => "Sam", 
			"Leah" => "Elliott", 
			"Harvey" => "Maru", 
			"Maru" => "Harvey", 
			"Elliott" => "Leah", 
			"Abigail" => "Sebastian", 
			"Sebastian" => "Abigail", 
			"Emily" => "Shane", 
			"Shane" => "Emily", 
			_ => "", 
		};
	}

	public static string ParseGiftReveals(string str)
	{
		string original = str;
		try
		{
			while (true)
			{
				int reveal_taste_location = str.IndexOf("%revealtaste");
				if (reveal_taste_location < 0)
				{
					break;
				}
				int tokenEnd = reveal_taste_location + "%revealtaste".Length;
				for (int i = tokenEnd; i < str.Length; i++)
				{
					char ch = str[i];
					if (char.IsWhiteSpace(ch) || ch == '#' || ch == '%' || ch == '$' || ch == '{' || ch == '^' || ch == '*')
					{
						break;
					}
					tokenEnd = i;
				}
				string match = str.Substring(reveal_taste_location, tokenEnd - reveal_taste_location + 1);
				string[] parts = match.Split(':');
				if (parts.Length == 3 && parts[0] == "%revealtaste")
				{
					string npcName = parts[1].Trim();
					NPC npc = Game1.getCharacterFromName(npcName);
					ItemMetadata itemData = ItemRegistry.GetMetadata(parts[2].Trim());
					if (itemData == null)
					{
						Game1.log.Warn($"Failed to parse gift taste reveal '{match}' in dialogue '{str}'. There is no item with that ID.");
					}
					else
					{
						Game1.player.revealGiftTaste(npc?.Name ?? npcName, itemData.LocalItemId);
					}
					str = str.Remove(reveal_taste_location, match.Length);
					continue;
				}
				int token_start = reveal_taste_location + "%revealtaste".Length;
				int token_end = reveal_taste_location + 1;
				if (token_end >= str.Length)
				{
					token_end = str.Length - 1;
				}
				for (; token_end < str.Length && (str[token_end] < '0' || str[token_end] > '9'); token_end++)
				{
				}
				string character_name = str.Substring(token_start, token_end - token_start);
				token_start = token_end;
				for (; token_end < str.Length && str[token_end] >= '0' && str[token_end] <= '9'; token_end++)
				{
				}
				string itemId = str.Substring(token_start, token_end - token_start);
				str = str.Remove(reveal_taste_location, token_end - reveal_taste_location);
				NPC target = Game1.getCharacterFromName(character_name);
				Game1.player.revealGiftTaste(target?.Name ?? character_name, itemId);
			}
		}
		catch (Exception e)
		{
			Game1.log.Error("Error parsing gift taste reveals in string '" + original + "'.", e);
		}
		return str;
	}

	public static void Shuffle<T>(Random rng, List<T> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			int k = rng.Next(n--);
			T temp = list[n];
			list[n] = list[k];
			list[k] = temp;
		}
	}

	public static void Shuffle<T>(Random rng, T[] array)
	{
		int n = array.Length;
		while (n > 1)
		{
			int k = rng.Next(n--);
			T temp = array[n];
			array[n] = array[k];
			array[k] = temp;
		}
	}

	/// <summary>Get the unique key for a season (one of <c>spring</c>, <c>summer</c>, <c>fall</c>, or <c>winter</c>).</summary>
	/// <param name="season">The season value.</param>
	public static string getSeasonKey(Season season)
	{
		return season switch
		{
			Season.Spring => "spring", 
			Season.Summer => "summer", 
			Season.Fall => "fall", 
			Season.Winter => "winter", 
			_ => season.ToString().ToLower(), 
		};
	}

	public static int getSeasonNumber(string whichSeason)
	{
		if (TryParseEnum<Season>(whichSeason, out var season))
		{
			return (int)season;
		}
		if (whichSeason.Equals("autumn", StringComparison.OrdinalIgnoreCase))
		{
			return 2;
		}
		return -1;
	}

	/// <summary>
	/// uses Game1.random so this will not be the same each time it's called in the same context.
	/// </summary>
	/// <param name="startTile"></param>
	/// <param name="number"></param>
	/// <returns></returns>
	public static List<Vector2> getPositionsInClusterAroundThisTile(Vector2 startTile, int number)
	{
		Queue<Vector2> openList = new Queue<Vector2>();
		List<Vector2> tiles = new List<Vector2>();
		Vector2 currentTile = startTile;
		openList.Enqueue(currentTile);
		while (tiles.Count < number)
		{
			currentTile = openList.Dequeue();
			tiles.Add(currentTile);
			if (!tiles.Contains(new Vector2(currentTile.X + 1f, currentTile.Y)))
			{
				openList.Enqueue(new Vector2(currentTile.X + 1f, currentTile.Y));
			}
			if (!tiles.Contains(new Vector2(currentTile.X - 1f, currentTile.Y)))
			{
				openList.Enqueue(new Vector2(currentTile.X - 1f, currentTile.Y));
			}
			if (!tiles.Contains(new Vector2(currentTile.X, currentTile.Y + 1f)))
			{
				openList.Enqueue(new Vector2(currentTile.X, currentTile.Y + 1f));
			}
			if (!tiles.Contains(new Vector2(currentTile.X, currentTile.Y - 1f)))
			{
				openList.Enqueue(new Vector2(currentTile.X, currentTile.Y - 1f));
			}
		}
		return tiles;
	}

	public static bool doesPointHaveLineOfSightInMine(GameLocation mine, Vector2 start, Vector2 end, int visionDistance)
	{
		if (Vector2.Distance(start, end) > (float)visionDistance)
		{
			return false;
		}
		foreach (Point p in GetPointsOnLine((int)start.X, (int)start.Y, (int)end.X, (int)end.Y))
		{
			if (mine.getTileIndexAt(p, "Buildings") != -1)
			{
				return false;
			}
		}
		return true;
	}

	public static void addSprinklesToLocation(GameLocation l, int sourceXTile, int sourceYTile, int tilesWide, int tilesHigh, int totalSprinkleDuration, int millisecondsBetweenSprinkles, Color sprinkleColor, string sound = null, bool motionTowardCenter = false)
	{
		Microsoft.Xna.Framework.Rectangle area = new Microsoft.Xna.Framework.Rectangle(sourceXTile - tilesWide / 2, sourceYTile - tilesHigh / 2, tilesWide, tilesHigh);
		Random r = new Random();
		int numSprinkles = totalSprinkleDuration / millisecondsBetweenSprinkles;
		for (int i = 0; i < numSprinkles; i++)
		{
			Vector2 currentSprinklePosition = getRandomPositionInThisRectangle(area, r) * 64f;
			l.temporarySprites.Add(new TemporaryAnimatedSprite(r.Next(10, 12), currentSprinklePosition, sprinkleColor, 8, flipped: false, 50f)
			{
				layerDepth = 1f,
				delayBeforeAnimationStart = millisecondsBetweenSprinkles * i,
				interval = 100f,
				startSound = sound,
				motion = (motionTowardCenter ? getVelocityTowardPoint(currentSprinklePosition, new Vector2(sourceXTile, sourceYTile) * 64f, Vector2.Distance(new Vector2(sourceXTile, sourceYTile) * 64f, currentSprinklePosition) / 64f) : Vector2.Zero),
				xStopCoordinate = sourceXTile,
				yStopCoordinate = sourceYTile
			});
		}
	}

	public static void addRainbowStarExplosion(GameLocation l, Vector2 origin, int numStars)
	{
		List<TemporaryAnimatedSprite> sprites = new List<TemporaryAnimatedSprite>();
		float radialStep = (float)Math.PI * 2f / (float)Math.Max(1, numStars - 1);
		Vector2 radPosition = new Vector2(0f, -4f);
		double r = Game1.random.NextDouble() * Math.PI * 2.0;
		for (int i = 0; i < numStars; i++)
		{
			sprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 640, 64, 64), origin + radPosition, flipped: false, 0.03f, GetPrismaticColor(Game1.random.Next(99999)))
			{
				motion = getVectorDirection(origin, origin + radPosition, normalize: true) * 0.06f * 150f,
				acceleration = -getVectorDirection(origin, origin + radPosition, normalize: true) * 0.06f * 6f,
				totalNumberOfLoops = 1,
				animationLength = 8,
				interval = 50f,
				drawAboveAlwaysFront = true,
				rotation = -(float)Math.PI / 2f - radialStep * (float)i
			});
			radPosition.X = 4f * (float)Math.Sin((double)(radialStep * (float)(i + 1)) + r);
			radPosition.Y = 4f * (float)Math.Cos((double)(radialStep * (float)(i + 1)) + r);
		}
		sprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 320, 64, 64), origin + radPosition, flipped: false, 0.03f, Color.White)
		{
			totalNumberOfLoops = 1,
			animationLength = 8,
			interval = 60f,
			drawAboveAlwaysFront = true
		});
		l.temporarySprites.AddRange(sprites);
	}

	public static Vector2 getVectorDirection(Vector2 start, Vector2 finish, bool normalize = false)
	{
		Vector2 v = new Vector2(finish.X - start.X, finish.Y - start.Y);
		if (normalize)
		{
			v.Normalize();
		}
		return v;
	}

	public static TemporaryAnimatedSpriteList getStarsAndSpirals(GameLocation l, int sourceXTile, int sourceYTile, int tilesWide, int tilesHigh, int totalSprinkleDuration, int millisecondsBetweenSprinkles, Color sprinkleColor, string sound = null, bool motionTowardCenter = false)
	{
		Microsoft.Xna.Framework.Rectangle area = new Microsoft.Xna.Framework.Rectangle(sourceXTile - tilesWide / 2, sourceYTile - tilesHigh / 2, tilesWide, tilesHigh);
		Random r = CreateRandom(sourceXTile * 7, sourceYTile * 77, Game1.currentGameTime.TotalGameTime.TotalSeconds);
		int numSprinkles = totalSprinkleDuration / millisecondsBetweenSprinkles;
		TemporaryAnimatedSpriteList tempSprites = new TemporaryAnimatedSpriteList();
		for (int i = 0; i < numSprinkles; i++)
		{
			Vector2 currentSprinklePosition = getRandomPositionInThisRectangle(area, r) * 64f;
			tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", r.NextBool() ? new Microsoft.Xna.Framework.Rectangle(359, 1437, 14, 14) : new Microsoft.Xna.Framework.Rectangle(377, 1438, 9, 9), currentSprinklePosition, flipped: false, 0.01f, sprinkleColor)
			{
				xPeriodic = true,
				xPeriodicLoopTime = r.Next(2000, 3000),
				xPeriodicRange = r.Next(-64, 64),
				motion = new Vector2(0f, -2f),
				rotationChange = (float)Math.PI / (float)r.Next(4, 64),
				delayBeforeAnimationStart = millisecondsBetweenSprinkles * i,
				layerDepth = 1f,
				scaleChange = 0.04f,
				scaleChangeChange = -0.0008f,
				scale = 4f
			});
		}
		return tempSprites;
	}

	public static void addStarsAndSpirals(GameLocation l, int sourceXTile, int sourceYTile, int tilesWide, int tilesHigh, int totalSprinkleDuration, int millisecondsBetweenSprinkles, Color sprinkleColor, string sound = null, bool motionTowardCenter = false)
	{
		l.temporarySprites.AddRange(getStarsAndSpirals(l, sourceXTile, sourceYTile, tilesWide, tilesHigh, totalSprinkleDuration, millisecondsBetweenSprinkles, sprinkleColor, sound, motionTowardCenter));
	}

	public static Vector2 snapDrawPosition(Vector2 draw_position)
	{
		return new Vector2((int)draw_position.X, (int)draw_position.Y);
	}

	public static Vector2 clampToTile(Vector2 nonTileLocation)
	{
		nonTileLocation.X -= nonTileLocation.X % 64f;
		nonTileLocation.Y -= nonTileLocation.Y % 64f;
		return nonTileLocation;
	}

	public static float distance(float x1, float x2, float y1, float y2)
	{
		return (float)Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
	}

	public static bool couldSeePlayerInPeripheralVision(Farmer player, Character c)
	{
		Point playerPixel = player.StandingPixel;
		Point targetPixel = c.StandingPixel;
		switch (c.FacingDirection)
		{
		case 0:
			if (playerPixel.Y < targetPixel.Y + 32)
			{
				return true;
			}
			break;
		case 1:
			if (playerPixel.X > targetPixel.X - 32)
			{
				return true;
			}
			break;
		case 2:
			if (playerPixel.Y > targetPixel.Y - 32)
			{
				return true;
			}
			break;
		case 3:
			if (playerPixel.X < targetPixel.X + 32)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public static IEnumerable<Point> GetPointsOnLine(int x0, int y0, int x1, int y1)
	{
		return GetPointsOnLine(x0, y0, x1, y1, ignoreSwap: false);
	}

	public static List<Vector2> getBorderOfThisRectangle(Microsoft.Xna.Framework.Rectangle r)
	{
		List<Vector2> border = new List<Vector2>();
		for (int i = r.X; i < r.Right; i++)
		{
			border.Add(new Vector2(i, r.Y));
		}
		for (int i = r.Y + 1; i < r.Bottom; i++)
		{
			border.Add(new Vector2(r.Right - 1, i));
		}
		for (int i = r.Right - 2; i >= r.X; i--)
		{
			border.Add(new Vector2(i, r.Bottom - 1));
		}
		for (int i = r.Bottom - 2; i >= r.Y + 1; i--)
		{
			border.Add(new Vector2(r.X, i));
		}
		return border;
	}

	public static Monster getClosestMonsterWithinRange(GameLocation location, Vector2 originPoint, int range)
	{
		return findClosestMonsterWithinRange(location, originPoint, range);
	}

	public static Monster findClosestMonsterWithinRange(GameLocation location, Vector2 originPoint, int range, bool ignoreUntargetables = false)
	{
		Monster closest_monster = null;
		float closest_distance = range + 1;
		foreach (NPC n in location.characters)
		{
			if (n is Monster && (!ignoreUntargetables || !(n is Spiker)))
			{
				float distance = Vector2.Distance(originPoint, n.getStandingPosition());
				if (distance <= (float)range && distance < closest_distance && !(n as Monster).IsInvisible)
				{
					closest_monster = n as Monster;
					closest_distance = distance;
				}
			}
		}
		return closest_monster;
	}

	public static Microsoft.Xna.Framework.Rectangle getTranslatedRectangle(Microsoft.Xna.Framework.Rectangle r, int xTranslate, int yTranslate = 0)
	{
		return translateRect(r, xTranslate, yTranslate);
	}

	public static Microsoft.Xna.Framework.Rectangle translateRect(Microsoft.Xna.Framework.Rectangle r, int xTranslate, int yTranslate = 0)
	{
		r.X += xTranslate;
		r.Y += yTranslate;
		return r;
	}

	public static Point getTranslatedPoint(Point p, int direction, int movementAmount)
	{
		return direction switch
		{
			0 => new Point(p.X, p.Y - movementAmount), 
			2 => new Point(p.X, p.Y + movementAmount), 
			1 => new Point(p.X + movementAmount, p.Y), 
			3 => new Point(p.X - movementAmount, p.Y), 
			_ => p, 
		};
	}

	public static Vector2 getTranslatedVector2(Vector2 p, int direction, float movementAmount)
	{
		return direction switch
		{
			0 => new Vector2(p.X, p.Y - movementAmount), 
			2 => new Vector2(p.X, p.Y + movementAmount), 
			1 => new Vector2(p.X + movementAmount, p.Y), 
			3 => new Vector2(p.X - movementAmount, p.Y), 
			_ => p, 
		};
	}

	public static IEnumerable<Point> GetPointsOnLine(int x0, int y0, int x1, int y1, bool ignoreSwap)
	{
		bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
		if (steep)
		{
			int t = x0;
			x0 = y0;
			y0 = t;
			t = x1;
			x1 = y1;
			y1 = t;
		}
		if (!ignoreSwap && x0 > x1)
		{
			int t = x0;
			x0 = x1;
			x1 = t;
			t = y0;
			y0 = y1;
			y1 = t;
		}
		int dx = x1 - x0;
		int dy = Math.Abs(y1 - y0);
		int error = dx / 2;
		int ystep = ((y0 < y1) ? 1 : (-1));
		int y = y0;
		for (int x = x0; x <= x1; x++)
		{
			yield return new Point(steep ? y : x, steep ? x : y);
			error -= dy;
			if (error < 0)
			{
				y += ystep;
				error += dx;
			}
		}
	}

	public static Vector2 getRandomAdjacentOpenTile(Vector2 tile, GameLocation location)
	{
		List<Vector2> l = getAdjacentTileLocations(tile);
		int iter = 0;
		int which = Game1.random.Next(l.Count);
		Vector2 v = l[which];
		for (; iter < 4; iter++)
		{
			if (!location.IsTileBlockedBy(v))
			{
				break;
			}
			which = (which + 1) % l.Count;
			v = l[which];
		}
		if (iter >= 4)
		{
			return Vector2.Zero;
		}
		return v;
	}

	public static void CollectSingleItemOrShowChestMenu(Chest chest, object context = null)
	{
		int item_count = 0;
		Item item_to_grab = null;
		IInventory items = chest.Items;
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i] != null)
			{
				item_count++;
				if (item_count == 1)
				{
					item_to_grab = items[i];
				}
				if (item_count == 2)
				{
					item_to_grab = null;
					break;
				}
			}
		}
		if (item_count == 0)
		{
			return;
		}
		if (item_to_grab != null)
		{
			int old_stack_amount = item_to_grab.Stack;
			if (Game1.player.addItemToInventory(item_to_grab) == null)
			{
				Game1.playSound("coin");
				items.Remove(item_to_grab);
				chest.clearNulls();
				return;
			}
			if (item_to_grab.Stack != old_stack_amount)
			{
				Game1.playSound("coin");
			}
		}
		Game1.activeClickableMenu = new ItemGrabMenu(items, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, chest.grabItemFromInventory, null, chest.grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, null, -1, context);
	}

	public static bool CollectOrDrop(Item item, int direction)
	{
		if (item != null)
		{
			item = Game1.player.addItemToInventory(item);
			if (item != null)
			{
				if (direction != -1)
				{
					Game1.createItemDebris(item, Game1.player.getStandingPosition(), direction);
				}
				else
				{
					Game1.createItemDebris(item, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
				}
				return false;
			}
			return true;
		}
		return true;
	}

	public static bool CollectOrDrop(Item item)
	{
		return CollectOrDrop(item, -1);
	}

	public static List<string> getExes(Farmer farmer)
	{
		List<string> exes = new List<string>();
		foreach (string key in farmer.friendshipData.Keys)
		{
			if (farmer.friendshipData[key].IsDivorced())
			{
				exes.Add(key);
			}
		}
		return exes;
	}

	public static void fixAllAnimals()
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		List<GameLocation> animalLocations = new List<GameLocation>();
		HashSet<long> uniqueAnimals = new HashSet<long>();
		List<long> animalsToRemove = new List<long>();
		ForEachLocation(delegate(GameLocation f)
		{
			if (f.animals.Length == 0 && f.buildings.Count == 0)
			{
				return true;
			}
			animalLocations.Clear();
			animalLocations.Add(f);
			foreach (Building building in f.buildings)
			{
				GameLocation indoors = building.GetIndoors();
				if (indoors != null && indoors.animals.Length > 0)
				{
					animalLocations.Add(indoors);
				}
			}
			bool flag = false;
			bool flag2 = false;
			foreach (GameLocation current in animalLocations)
			{
				AnimalHouse animalHouse = current as AnimalHouse;
				animalsToRemove.Clear();
				foreach (KeyValuePair<long, NetRef<FarmAnimal>> current2 in current.animals.FieldDict)
				{
					if (current2.Value?.Value == null)
					{
						animalsToRemove.Add(current2.Key);
					}
					else
					{
						if (current2.Value.Value.home == null)
						{
							flag = true;
						}
						if (!uniqueAnimals.Add(current2.Value.Value.myID.Value))
						{
							animalsToRemove.Add(current2.Key);
						}
					}
				}
				flag2 = flag2 || animalsToRemove.Count > 0;
				foreach (long current3 in animalsToRemove)
				{
					long animalId = current.animals[current3].myID.Value;
					current.animals.Remove(current3);
					animalHouse?.animalsThatLiveHere.RemoveWhere((long id) => id == animalId);
				}
			}
			foreach (Building current4 in f.buildings)
			{
				if (current4.GetIndoors() is AnimalHouse animalHouse2)
				{
					foreach (long item in animalHouse2.animalsThatLiveHere)
					{
						FarmAnimal animal = getAnimal(item);
						if (animal != null)
						{
							if (animal.home == null)
							{
								flag = true;
							}
							animal.home = current4;
						}
					}
				}
			}
			if (!flag && !flag2)
			{
				return true;
			}
			List<FarmAnimal> allFarmAnimals = f.getAllFarmAnimals();
			allFarmAnimals.RemoveAll((FarmAnimal a) => a.home != null);
			foreach (FarmAnimal a in allFarmAnimals)
			{
				foreach (Building building2 in f.buildings)
				{
					building2.GetIndoors()?.animals.RemoveWhere((KeyValuePair<long, FarmAnimal> pair) => pair.Value.Equals(a));
				}
				f.animals.RemoveWhere((KeyValuePair<long, FarmAnimal> pair) => pair.Value.Equals(a));
			}
			foreach (Building b in f.buildings)
			{
				if (b.GetIndoors() is AnimalHouse animalHouse3)
				{
					animalHouse3.animalsThatLiveHere.RemoveWhere((long id) => getAnimal(id)?.home != b);
				}
			}
			foreach (FarmAnimal current5 in allFarmAnimals)
			{
				foreach (Building current6 in f.buildings)
				{
					if (current5.CanLiveIn(current6) && current6.GetIndoors() is AnimalHouse animalHouse4 && !animalHouse4.isFull())
					{
						animalHouse4.adoptAnimal(current5);
						break;
					}
				}
			}
			foreach (FarmAnimal current7 in allFarmAnimals)
			{
				if (current7.home == null)
				{
					current7.Position = recursiveFindOpenTileForCharacter(current7, f, new Vector2(40f, 40f), 200) * 64f;
					f.animals.TryAdd(current7.myID.Value, current7);
				}
			}
			return true;
		}, includeInteriors: false);
	}

	/// <summary>Create a generated event to marry a player's current NPC or player spouse.</summary>
	/// <param name="farmer">The player getting married.</param>
	public static Event getWeddingEvent(Farmer farmer)
	{
		Farmer spouseFarmer = null;
		long? spouseFarmerId = farmer.team.GetSpouse(farmer.UniqueMultiplayerID);
		if (spouseFarmerId.HasValue)
		{
			spouseFarmer = Game1.getFarmerMaybeOffline(spouseFarmerId.Value);
		}
		string spouseActor = ((spouseFarmer != null) ? ("farmer" + getFarmerNumberFromFarmer(spouseFarmer)) : farmer.spouse);
		WeddingData data = DataLoader.Weddings(Game1.content);
		List<WeddingAttendeeData> contextualAttendees = new List<WeddingAttendeeData>();
		if (data.Attendees != null)
		{
			List<string> exes = getExes(farmer);
			foreach (WeddingAttendeeData attendee in data.Attendees.Values)
			{
				if (!exes.Contains(attendee.Id) && !(attendee.Id == farmer.spouse) && GameStateQuery.CheckConditions(attendee.Condition, null, farmer) && (attendee.IgnoreUnlockConditions || !NPC.TryGetData(attendee.Id, out var characterData) || GameStateQuery.CheckConditions(characterData.UnlockConditions, null, farmer)))
				{
					contextualAttendees.Add(attendee);
				}
			}
		}
		if (!data.EventScript.TryGetValue(spouseFarmerId?.ToString() ?? farmer.spouse, out var weddingEventString) && !data.EventScript.TryGetValue("default", out weddingEventString))
		{
			throw new InvalidOperationException("The Data/Weddings asset has no wedding script with the 'default' script key.");
		}
		weddingEventString = TokenParser.ParseText(weddingEventString, null, ParseWeddingToken, farmer);
		return new Event(weddingEventString, null, "-2", farmer);
		bool ParseWeddingToken(string[] query, out string replacement, Random random, Farmer player)
		{
			switch (ArgUtility.Get(query, 0)?.ToLower())
			{
			case "spouseactor":
				replacement = spouseActor;
				return true;
			case "setupcontextualweddingattendees":
			{
				StringBuilder sb = new StringBuilder();
				foreach (WeddingAttendeeData attendee in contextualAttendees)
				{
					sb.Append(" ");
					sb.Append(attendee.Setup);
				}
				replacement = sb.ToString();
				return true;
			}
			case "contextualweddingcelebrations":
			{
				StringBuilder sb = new StringBuilder();
				foreach (WeddingAttendeeData attendee in contextualAttendees)
				{
					if (attendee.Celebration != null)
					{
						sb.Append(attendee.Celebration);
						sb.Append("/");
					}
				}
				replacement = sb.ToString();
				return true;
			}
			default:
				replacement = null;
				return false;
			}
		}
	}

	/// <summary>Draw a box to the screen.</summary>
	/// <param name="b">The sprite batch being drawn.</param>
	/// <param name="pixelArea">The pixel area of the box to draw.</param>
	/// <param name="borderWidth">The width of the border to draw.</param>
	/// <param name="borderColor">The color of the border to draw, or <c>null</c> for black.</param>
	/// <param name="backgroundColor">The background color to draw, or <c>null</c> for none.</param>
	public static void DrawSquare(SpriteBatch b, Microsoft.Xna.Framework.Rectangle pixelArea, int borderWidth, Color? borderColor = null, Color? backgroundColor = null)
	{
		if (backgroundColor.HasValue)
		{
			b.Draw(Game1.staminaRect, pixelArea, backgroundColor.Value);
		}
		if (borderWidth > 0)
		{
			Color color = borderColor ?? Color.Black;
			b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(pixelArea.X, pixelArea.Y, pixelArea.Width, borderWidth), color);
			b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(pixelArea.X, pixelArea.Y + pixelArea.Height - borderWidth, pixelArea.Width, borderWidth), color);
			b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(pixelArea.X, pixelArea.Y, borderWidth, pixelArea.Height), color);
			b.Draw(Game1.staminaRect, new Microsoft.Xna.Framework.Rectangle(pixelArea.X + pixelArea.Width - borderWidth, pixelArea.Y, borderWidth, pixelArea.Height), color);
		}
	}

	/// <summary>Draw a missing-texture image to the screen.</summary>
	/// <param name="spriteBatch">The sprite batch being drawn.</param>
	/// <param name="screenArea">The pixel area within the <see cref="F:StardewValley.Game1.viewport" /> to cover with the error texture.</param>
	/// <param name="layerDepth">The layer depth at which to draw the error texture in the <paramref name="spriteBatch" />.</param>
	public static void DrawErrorTexture(SpriteBatch spriteBatch, Microsoft.Xna.Framework.Rectangle screenArea, float layerDepth)
	{
		spriteBatch.Draw(Game1.mouseCursors, screenArea, new Microsoft.Xna.Framework.Rectangle(320, 496, 16, 16), Color.White, 0f, Vector2.Zero, SpriteEffects.None, layerDepth);
	}

	public static void drawTinyDigits(int toDraw, SpriteBatch b, Vector2 position, float scale, float layerDepth, Color c)
	{
		int xPosition = 0;
		int currentValue = toDraw;
		int numDigits = 0;
		do
		{
			numDigits++;
		}
		while ((toDraw /= 10) >= 1);
		int digitStrip = (int)Math.Pow(10.0, numDigits - 1);
		bool significant = false;
		for (int i = 0; i < numDigits; i++)
		{
			int currentDigit = currentValue / digitStrip % 10;
			if (currentDigit > 0 || i == numDigits - 1)
			{
				significant = true;
			}
			if (significant)
			{
				b.Draw(Game1.mouseCursors, position + new Vector2(xPosition, 0f), new Microsoft.Xna.Framework.Rectangle(368 + currentDigit * 5, 56, 5, 7), c, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
			}
			xPosition += (int)(5f * scale) - 1;
			digitStrip /= 10;
		}
	}

	public static int getWidthOfTinyDigitString(int toDraw, float scale)
	{
		int numDigits = 0;
		do
		{
			numDigits++;
		}
		while ((toDraw /= 10) >= 1);
		return (int)((float)(numDigits * 5) * scale);
	}

	public static bool isMale(string who)
	{
		if (NPC.TryGetData(who, out var data))
		{
			return data.Gender == Gender.Male;
		}
		return true;
	}

	public static int GetMaximumHeartsForCharacter(Character character)
	{
		if (character == null)
		{
			return 0;
		}
		int max_hearts = 10;
		if (character is NPC npc && (bool)npc.datable)
		{
			max_hearts = 8;
		}
		if (Game1.player.friendshipData.TryGetValue(character.Name, out var friendship))
		{
			if (friendship.IsMarried())
			{
				max_hearts = 14;
			}
			else if (friendship.IsDating())
			{
				max_hearts = 10;
			}
		}
		return max_hearts;
	}

	/// <summary>Get whether an item exists anywhere in the world.</summary>
	/// <param name="itemId">The qualified or unqualified item ID.</param>
	public static bool doesItemExistAnywhere(string itemId)
	{
		itemId = ItemRegistry.QualifyItemId(itemId);
		if (itemId == null)
		{
			return false;
		}
		bool itemFound = false;
		ForEachItem(delegate(Item item)
		{
			if (item.QualifiedItemId == itemId)
			{
				itemFound = true;
			}
			return !itemFound;
		});
		return itemFound;
	}

	internal static void CollectGarbage(string filePath = "", int lineNumber = 0)
	{
		GC.Collect(0, GCCollectionMode.Forced);
	}

	public static List<string> possibleCropsAtThisTime(Season season, bool firstWeek)
	{
		List<string> firstWeekCrops = null;
		List<string> secondWeekCrops = null;
		switch (season)
		{
		case Season.Spring:
			firstWeekCrops = new List<string> { "24", "192" };
			if (Game1.year > 1)
			{
				firstWeekCrops.Add("250");
			}
			if (doesAnyFarmerHaveMail("ccVault"))
			{
				firstWeekCrops.Add("248");
			}
			secondWeekCrops = new List<string> { "190", "188" };
			if (doesAnyFarmerHaveMail("ccVault"))
			{
				secondWeekCrops.Add("252");
			}
			secondWeekCrops.AddRange(firstWeekCrops);
			break;
		case Season.Summer:
			firstWeekCrops = new List<string> { "264", "262", "260" };
			secondWeekCrops = new List<string> { "254", "256" };
			if (Game1.year > 1)
			{
				firstWeekCrops.Add("266");
			}
			if (doesAnyFarmerHaveMail("ccVault"))
			{
				secondWeekCrops.AddRange(new string[2] { "258", "268" });
			}
			secondWeekCrops.AddRange(firstWeekCrops);
			break;
		case Season.Fall:
			firstWeekCrops = new List<string> { "272", "278" };
			secondWeekCrops = new List<string> { "270", "276", "280" };
			if (Game1.year > 1)
			{
				secondWeekCrops.Add("274");
			}
			if (doesAnyFarmerHaveMail("ccVault"))
			{
				firstWeekCrops.Add("284");
				secondWeekCrops.Add("282");
			}
			secondWeekCrops.AddRange(firstWeekCrops);
			break;
		}
		if (!firstWeek)
		{
			return secondWeekCrops;
		}
		return firstWeekCrops;
	}

	public static float RandomFloat(float min, float max, Random random = null)
	{
		if (random == null)
		{
			random = Game1.random;
		}
		return Lerp(min, max, (float)random.NextDouble());
	}

	public static float Clamp(float value, float min, float max)
	{
		if (max < min)
		{
			float num = min;
			min = max;
			max = num;
		}
		if (value < min)
		{
			value = min;
		}
		if (value > max)
		{
			value = max;
		}
		return value;
	}

	public static Color MakeCompletelyOpaque(Color color)
	{
		if (color.A >= byte.MaxValue)
		{
			return color;
		}
		color.A = byte.MaxValue;
		return color;
	}

	public static int Clamp(int value, int min, int max)
	{
		if (max < min)
		{
			int num = min;
			min = max;
			max = num;
		}
		if (value < min)
		{
			value = min;
		}
		if (value > max)
		{
			value = max;
		}
		return value;
	}

	public static float Lerp(float a, float b, float t)
	{
		return a + t * (b - a);
	}

	public static float MoveTowards(float from, float to, float delta)
	{
		if (Math.Abs(to - from) <= delta)
		{
			return to;
		}
		return from + (float)Math.Sign(to - from) * delta;
	}

	public static Color MultiplyColor(Color a, Color b)
	{
		return new Color((float)(int)a.R / 255f * ((float)(int)b.R / 255f), (float)(int)a.G / 255f * ((float)(int)b.G / 255f), (float)(int)a.B / 255f * ((float)(int)b.B / 255f), (float)(int)a.A / 255f * ((float)(int)b.A / 255f));
	}

	/// <summary>Get the number of minutes until 6am tomorrow.</summary>
	/// <param name="currentTime">The starting time of day, in 26-hour format.</param>
	public static int CalculateMinutesUntilMorning(int currentTime)
	{
		return CalculateMinutesUntilMorning(currentTime, 1);
	}

	/// <summary>Get the number of minutes until 6am on a given day.</summary>
	/// <param name="currentTime">The starting time of day, in 26-hour format.</param>
	/// <param name="daysElapsed">The day offset (e.g. 1 for tomorrow).</param>
	public static int CalculateMinutesUntilMorning(int currentTime, int daysElapsed)
	{
		if (daysElapsed < 1)
		{
			return 0;
		}
		return ConvertTimeToMinutes(2600) - ConvertTimeToMinutes(currentTime) + 400 + (daysElapsed - 1) * 1600;
	}

	/// <summary>Get the number of minutes between two times.</summary>
	/// <param name="startTime">The starting time of day, in 26-hour format.</param>
	/// <param name="endTime">The ending time of day, in 26-hour format.</param>
	public static int CalculateMinutesBetweenTimes(int startTime, int endTime)
	{
		return ConvertTimeToMinutes(endTime) - ConvertTimeToMinutes(startTime);
	}

	/// <summary>Apply a minute offset to a time of day.</summary>
	/// <param name="timestamp">The initial time of day, in 26-hour format.</param>
	/// <param name="minutes_to_add">The number of minutes to add to the time.</param>
	public static int ModifyTime(int timestamp, int minutes_to_add)
	{
		timestamp = ConvertTimeToMinutes(timestamp);
		timestamp += minutes_to_add;
		return ConvertMinutesToTime(timestamp);
	}

	/// <summary>Get the time of day given the number of minutes since midnight.</summary>
	/// <param name="minutes">The number of minutes since midnight.</param>
	public static int ConvertMinutesToTime(int minutes)
	{
		return minutes / 60 * 100 + minutes % 60;
	}

	/// <summary>Get the number of minutes since midnight for a time.</summary>
	/// <param name="time_stamp">The time of day, in 26-hour format.</param>
	public static int ConvertTimeToMinutes(int time_stamp)
	{
		return time_stamp / 100 * 60 + time_stamp % 100;
	}

	public static int getSellToStorePriceOfItem(Item i, bool countStack = true)
	{
		if (i != null)
		{
			return i.sellToStorePrice(-1L) * ((!countStack) ? 1 : i.Stack);
		}
		return 0;
	}

	/// <summary>Get a list of secret notes or journal scraps that have not been seen.</summary>
	/// <param name="who">The farmer to check for unseen secret notes or journal scraps.</param>
	/// <param name="journal">Whether to get journal scraps (true) or secret notes (false).</param>
	/// <param name="totalNotes">The total number of secret notes or journal scraps (depending on <paramref name="journal" />), including seen ones.</param>
	public static int[] GetUnseenSecretNotes(Farmer who, bool journal, out int totalNotes)
	{
		Func<int, bool> query = ((!journal) ? ((Func<int, bool>)((int id) => id < GameLocation.JOURNAL_INDEX)) : ((Func<int, bool>)((int id) => id >= GameLocation.JOURNAL_INDEX)));
		int[] allNotes = DataLoader.SecretNotes(Game1.content).Keys.Where(query).ToArray();
		totalNotes = allNotes.Length;
		return allNotes.Except(who.secretNotesSeen.Where(query)).ToArray();
	}

	public static bool HasAnyPlayerSeenSecretNote(int note_number)
	{
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			if (allFarmer.secretNotesSeen.Contains(note_number))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HasAnyPlayerSeenEvent(string eventId)
	{
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			if (allFarmer.eventsSeen.Contains(eventId))
			{
				return true;
			}
		}
		return false;
	}

	public static bool HaveAllPlayersSeenEvent(string eventId)
	{
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			if (!allFarmer.eventsSeen.Contains(eventId))
			{
				return false;
			}
		}
		return true;
	}

	public static List<string> GetAllPlayerUnlockedCookingRecipes()
	{
		List<string> unlocked_recipes = new List<string>();
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			foreach (string recipe in allFarmer.cookingRecipes.Keys)
			{
				if (!unlocked_recipes.Contains(recipe))
				{
					unlocked_recipes.Add(recipe);
				}
			}
		}
		return unlocked_recipes;
	}

	public static List<string> GetAllPlayerUnlockedCraftingRecipes()
	{
		List<string> unlocked_recipes = new List<string>();
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			foreach (string recipe in allFarmer.craftingRecipes.Keys)
			{
				if (!unlocked_recipes.Contains(recipe))
				{
					unlocked_recipes.Add(recipe);
				}
			}
		}
		return unlocked_recipes;
	}

	public static int GetAllPlayerFriendshipLevel(NPC npc)
	{
		int highest_friendship_points = -1;
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			if (allFarmer.friendshipData.TryGetValue(npc.Name, out var friendship) && friendship.Points > highest_friendship_points)
			{
				highest_friendship_points = friendship.Points;
			}
		}
		return highest_friendship_points;
	}

	public static int GetAllPlayerReachedBottomOfMines()
	{
		int highest_value = 0;
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			if (farmer.timesReachedMineBottom > highest_value)
			{
				highest_value = farmer.timesReachedMineBottom;
			}
		}
		return highest_value;
	}

	public static int GetAllPlayerDeepestMineLevel()
	{
		int highest_value = 0;
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			if (farmer.deepestMineLevel > highest_value)
			{
				highest_value = farmer.deepestMineLevel;
			}
		}
		return highest_value;
	}

	public static string LegacyWeatherToWeather(int legacyWeather)
	{
		return legacyWeather switch
		{
			2 => "Wind", 
			4 => "Festival", 
			3 => "Storm", 
			1 => "Rain", 
			5 => "Snow", 
			6 => "Wedding", 
			_ => "Sun", 
		};
	}

	public static string getRandomBasicSeasonalForageItem(Season season, int randomSeedAddition = -1)
	{
		Random r = CreateRandom(Game1.uniqueIDForThisGame, randomSeedAddition);
		string[] possibleItems = LegacyShims.EmptyArray<string>();
		switch (season)
		{
		case Season.Spring:
			possibleItems = new string[4] { "16", "18", "20", "22" };
			break;
		case Season.Summer:
			possibleItems = new string[3] { "396", "398", "402" };
			break;
		case Season.Fall:
			possibleItems = new string[4] { "404", "406", "408", "410" };
			break;
		case Season.Winter:
			possibleItems = new string[4] { "412", "414", "416", "418" };
			break;
		}
		return r.ChooseFrom(possibleItems) ?? "0";
	}

	public static string getRandomPureSeasonalItem(Season season, int randomSeedAddition)
	{
		Random r = CreateRandom(Game1.uniqueIDForThisGame, randomSeedAddition);
		string[] possibleItems = LegacyShims.EmptyArray<string>();
		switch (season)
		{
		case Season.Spring:
			possibleItems = new string[15]
			{
				"16", "18", "20", "22", "129", "131", "132", "136", "137", "142",
				"143", "145", "147", "148", "152"
			};
			break;
		case Season.Summer:
			possibleItems = new string[16]
			{
				"128", "130", "131", "132", "136", "138", "142", "144", "145", "146",
				"149", "150", "155", "396", "398", "402"
			};
			break;
		case Season.Fall:
			possibleItems = new string[17]
			{
				"404", "406", "408", "410", "129", "131", "132", "136", "137", "139",
				"140", "142", "143", "148", "150", "154", "155"
			};
			break;
		case Season.Winter:
			possibleItems = new string[17]
			{
				"412", "414", "416", "418", "130", "131", "132", "136", "140", "141",
				"143", "144", "146", "147", "150", "151", "154"
			};
			break;
		}
		return r.ChooseFrom(possibleItems) ?? "0";
	}

	public static Item CreateFlavoredItem(string baseID, string preservesID, int quality = 0, int stack = 1)
	{
		if (ItemQueryResolver.TryResolve("FLAVORED_ITEM " + baseID + " " + preservesID, new ItemQueryContext(Game1.currentLocation, Game1.player, Game1.random)).FirstOrDefault()?.Item is Item i)
		{
			i.Quality = quality;
			i.Stack = stack;
			return i;
		}
		return null;
	}

	public static string getRandomItemFromSeason(Season season, bool forQuest, Random random)
	{
		List<string> possibleItems = new List<string> { "68", "66", "78", "80", "86", "152", "167", "153", "420" };
		List<string> all_unlocked_crafting_recipes = new List<string>(Game1.player.craftingRecipes.Keys);
		List<string> all_unlocked_cooking_recipes = new List<string>(Game1.player.cookingRecipes.Keys);
		if (forQuest)
		{
			all_unlocked_crafting_recipes = GetAllPlayerUnlockedCraftingRecipes();
			all_unlocked_cooking_recipes = GetAllPlayerUnlockedCookingRecipes();
		}
		if ((forQuest && (MineShaft.lowestLevelReached > 40 || GetAllPlayerReachedBottomOfMines() >= 1)) || (!forQuest && (Game1.player.deepestMineLevel > 40 || Game1.player.timesReachedMineBottom >= 1)))
		{
			possibleItems.AddRange(new string[5] { "62", "70", "72", "84", "422" });
		}
		if ((forQuest && (MineShaft.lowestLevelReached > 80 || GetAllPlayerReachedBottomOfMines() >= 1)) || (!forQuest && (Game1.player.deepestMineLevel > 80 || Game1.player.timesReachedMineBottom >= 1)))
		{
			possibleItems.AddRange(new string[3] { "64", "60", "82" });
		}
		if (doesAnyFarmerHaveMail("ccVault"))
		{
			possibleItems.AddRange(new string[4] { "88", "90", "164", "165" });
		}
		if (all_unlocked_crafting_recipes.Contains("Furnace"))
		{
			possibleItems.AddRange(new string[4] { "334", "335", "336", "338" });
		}
		if (all_unlocked_crafting_recipes.Contains("Quartz Globe"))
		{
			possibleItems.Add("339");
		}
		switch (season)
		{
		case Season.Spring:
			possibleItems.AddRange(new string[17]
			{
				"16", "18", "20", "22", "129", "131", "132", "136", "137", "142",
				"143", "145", "147", "148", "152", "167", "267"
			});
			break;
		case Season.Summer:
			possibleItems.AddRange(new string[16]
			{
				"128", "130", "132", "136", "138", "142", "144", "145", "146", "149",
				"150", "155", "396", "398", "402", "267"
			});
			break;
		case Season.Fall:
			possibleItems.AddRange(new string[18]
			{
				"404", "406", "408", "410", "129", "131", "132", "136", "137", "139",
				"140", "142", "143", "148", "150", "154", "155", "269"
			});
			break;
		case Season.Winter:
			possibleItems.AddRange(new string[17]
			{
				"412", "414", "416", "418", "130", "131", "132", "136", "140", "141",
				"144", "146", "147", "150", "151", "154", "269"
			});
			break;
		}
		if (forQuest)
		{
			foreach (string s in all_unlocked_cooking_recipes)
			{
				if (random.NextDouble() < 0.4)
				{
					continue;
				}
				List<string> cropsAvailableNow = possibleCropsAtThisTime(Game1.season, Game1.dayOfMonth <= 7);
				if (!DataLoader.CookingRecipes(Game1.content).TryGetValue(s, out var rawCraftingData))
				{
					continue;
				}
				string[] fields = rawCraftingData.Split('/');
				string[] ingredientsSplit = ArgUtility.SplitBySpace(ArgUtility.Get(fields, 0));
				bool ingredientsAvailable = true;
				for (int i = 0; i < ingredientsSplit.Length; i++)
				{
					if (!possibleItems.Contains(ingredientsSplit[i]) && !isCategoryIngredientAvailable(ingredientsSplit[i]) && (cropsAvailableNow == null || !cropsAvailableNow.Contains(ingredientsSplit[i])))
					{
						ingredientsAvailable = false;
						break;
					}
				}
				if (ingredientsAvailable)
				{
					string itemId = ArgUtility.Get(fields, 2);
					if (itemId != null)
					{
						possibleItems.Add(itemId);
					}
				}
			}
		}
		return random.ChooseFrom(possibleItems);
	}

	public static string getRandomItemFromSeason(Season season, int randomSeedAddition, bool forQuest, bool changeDaily = true)
	{
		Random r = CreateRandom(Game1.uniqueIDForThisGame, changeDaily ? Game1.stats.DaysPlayed : 0u, randomSeedAddition);
		return getRandomItemFromSeason(season, forQuest, r);
	}

	private static bool isCategoryIngredientAvailable(string category)
	{
		if (category != null && category.StartsWith('-'))
		{
			if (category == "-5" || category == "-6")
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static void farmerHeardSong(string trackName)
	{
		if (string.IsNullOrWhiteSpace(trackName))
		{
			return;
		}
		HashSet<string> songs = Game1.player.songsHeard;
		switch (trackName)
		{
		case "EarthMine":
			songs.Add("Crystal Bells");
			songs.Add("Cavern");
			songs.Add("Secret Gnomes");
			break;
		case "FrostMine":
			songs.Add("Cloth");
			songs.Add("Icicles");
			songs.Add("XOR");
			break;
		case "LavaMine":
			songs.Add("Of Dwarves");
			songs.Add("Near The Planet Core");
			songs.Add("Overcast");
			songs.Add("tribal");
			break;
		case "VolcanoMines":
			songs.Add("VolcanoMines1");
			songs.Add("VolcanoMines2");
			break;
		default:
			if (trackName != "none" && trackName != "rain" && trackName != "silence")
			{
				songs.Add(trackName);
			}
			break;
		}
	}

	public static float getMaxedFriendshipPercent(Farmer who = null)
	{
		if (who == null)
		{
			who = Game1.player;
		}
		int maxedFriends = 0;
		int totalFriends = 0;
		foreach (KeyValuePair<string, CharacterData> pair in Game1.characterData)
		{
			string npcName = pair.Key;
			CharacterData data = pair.Value;
			if (!data.PerfectionScore || GameStateQuery.IsImmutablyFalse(data.CanSocialize))
			{
				continue;
			}
			totalFriends++;
			if (who.friendshipData.TryGetValue(npcName, out var friendship))
			{
				int maxPoints = (data.CanBeRomanced ? 8 : 10) * 250;
				if (friendship != null && friendship.Points >= maxPoints)
				{
					maxedFriends++;
				}
			}
		}
		return (float)maxedFriends / ((float)totalFriends * 1f);
	}

	public static float getCookedRecipesPercent(Farmer who = null)
	{
		if (who == null)
		{
			who = Game1.player;
		}
		Dictionary<string, string> recipes = DataLoader.CookingRecipes(Game1.content);
		float numberOfRecipesCooked = 0f;
		foreach (KeyValuePair<string, string> v in recipes)
		{
			string recipeKey = v.Key;
			if (who.cookingRecipes.ContainsKey(recipeKey))
			{
				string recipe = ArgUtility.SplitBySpaceAndGet(ArgUtility.Get(v.Value.Split('/'), 2), 0);
				if (who.recipesCooked.ContainsKey(recipe))
				{
					numberOfRecipesCooked += 1f;
				}
			}
		}
		return numberOfRecipesCooked / (float)recipes.Count;
	}

	public static float getCraftedRecipesPercent(Farmer who = null)
	{
		if (who == null)
		{
			who = Game1.player;
		}
		Dictionary<string, string> recipes = DataLoader.CraftingRecipes(Game1.content);
		float numberOfRecipesMade = 0f;
		foreach (string s in recipes.Keys)
		{
			if (!(s == "Wedding Ring") && who.craftingRecipes.TryGetValue(s, out var timesCrafted) && timesCrafted > 0)
			{
				numberOfRecipesMade += 1f;
			}
		}
		return numberOfRecipesMade / ((float)recipes.Count - 1f);
	}

	public static float getFishCaughtPercent(Farmer who = null)
	{
		if (who == null)
		{
			who = Game1.player;
		}
		float fishCaught = 0f;
		float totalFish = 0f;
		foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
		{
			if (data.ObjectType == "Fish" && !(data.RawData is ObjectData { ExcludeFromFishingCollection: not false }))
			{
				totalFish += 1f;
				if (who.fishCaught.ContainsKey(data.QualifiedItemId))
				{
					fishCaught += 1f;
				}
			}
		}
		return fishCaught / totalFish;
	}

	public static KeyValuePair<Farmer, bool> GetFarmCompletion(Func<Farmer, bool> check)
	{
		if (check(Game1.player))
		{
			return new KeyValuePair<Farmer, bool>(Game1.player, value: true);
		}
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			if (farmer != Game1.player && farmer.isCustomized.Value && check(farmer))
			{
				return new KeyValuePair<Farmer, bool>(farmer, value: true);
			}
		}
		return new KeyValuePair<Farmer, bool>(Game1.player, value: false);
	}

	public static KeyValuePair<Farmer, float> GetFarmCompletion(Func<Farmer, float> check)
	{
		Farmer highest_farmer = Game1.player;
		float highest_value = check(Game1.player);
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			if (farmer != Game1.player && farmer.isCustomized.Value)
			{
				float current_value = check(farmer);
				if (current_value > highest_value)
				{
					highest_farmer = farmer;
					highest_value = current_value;
				}
			}
		}
		return new KeyValuePair<Farmer, float>(highest_farmer, highest_value);
	}

	/// <summary>Get the overall perfection score for this save, accounting for all players.</summary>
	/// <remarks>See also <see cref="M:StardewValley.Game1.UpdateFarmPerfection" /> for the overnight changes if perfection was reached.</remarks>
	/// <returns>Returns a number between 0 (no perfection requirements met) and 1 (all requirements met).</returns>
	public static float percentGameComplete()
	{
		float total = 0f;
		float num = 0f + GetFarmCompletion((Farmer farmer) => getFarmerItemsShippedPercent(farmer)).Value * 15f;
		total += 15f;
		float num2 = num + Math.Min(GetObeliskTypesBuilt(), 4f);
		total += 4f;
		float num3 = num2 + (float)(Game1.IsBuildingConstructed("Gold Clock") ? 10 : 0);
		total += 10f;
		float num4 = num3 + (float)(GetFarmCompletion((Farmer farmer) => farmer.hasCompletedAllMonsterSlayerQuests.Value).Value ? 10 : 0);
		total += 10f;
		float NPCFriendPercent = GetFarmCompletion((Farmer farmer) => getMaxedFriendshipPercent(farmer)).Value;
		float num5 = num4 + NPCFriendPercent * 11f;
		total += 11f;
		float farmerLevelPercent = GetFarmCompletion((Farmer farmer) => Math.Min(farmer.Level, 25f) / 25f).Value;
		float num6 = num5 + farmerLevelPercent * 5f;
		total += 5f;
		float num7 = num6 + (float)(GetFarmCompletion((Farmer farmer) => foundAllStardrops(farmer)).Value ? 10 : 0);
		total += 10f;
		float num8 = num7 + GetFarmCompletion((Farmer farmer) => getCookedRecipesPercent(farmer)).Value * 10f;
		total += 10f;
		float num9 = num8 + GetFarmCompletion((Farmer farmer) => getCraftedRecipesPercent(farmer)).Value * 10f;
		total += 10f;
		float num10 = num9 + GetFarmCompletion((Farmer farmer) => getFishCaughtPercent(farmer)).Value * 10f;
		total += 10f;
		float totalNuts = 130f;
		float walnutsFound = Math.Min(Game1.netWorldState.Value.GoldenWalnutsFound, totalNuts);
		float num11 = num10 + walnutsFound / totalNuts * 5f;
		total += 5f;
		return num11 / total;
	}

	/// <summary>Get the number of unique obelisk building types constructed anywhere in the world.</summary>
	public static int GetObeliskTypesBuilt()
	{
		return (Game1.IsBuildingConstructed("Water Obelisk") ? 1 : 0) + (Game1.IsBuildingConstructed("Earth Obelisk") ? 1 : 0) + (Game1.IsBuildingConstructed("Desert Obelisk") ? 1 : 0) + (Game1.IsBuildingConstructed("Island Obelisk") ? 1 : 0);
	}

	private static int itemsShippedPercent()
	{
		return (int)((float)Game1.player.basicShipped.Length / 92f * 5f);
	}

	public static int getTrashReclamationPrice(Item i, Farmer f)
	{
		float sellPercentage = 0.15f * (float)f.trashCanLevel;
		if (i.canBeTrashed())
		{
			if (i is Wallpaper || i is Furniture)
			{
				return -1;
			}
			if ((i is Object obj && !obj.bigCraftable) || i is MeleeWeapon || i is Ring || i is Boots)
			{
				return (int)((float)i.Stack * ((float)i.sellToStorePrice(-1L) * sellPercentage));
			}
		}
		return -1;
	}

	/// <summary>Get the help-wanted quest to show on Pierre's bulletin board today, if any.</summary>
	public static Quest getQuestOfTheDay()
	{
		if (Game1.stats.DaysPlayed <= 1)
		{
			return null;
		}
		double d = CreateDaySaveRandom(100.0, Game1.stats.DaysPlayed * 777).NextDouble();
		if (d < 0.08)
		{
			return new ResourceCollectionQuest();
		}
		if (d < 0.2 && MineShaft.lowestLevelReached > 0 && Game1.stats.DaysPlayed > 5)
		{
			return new SlayMonsterQuest();
		}
		if (d < 0.5)
		{
			return null;
		}
		if (d < 0.6)
		{
			return new FishingQuest();
		}
		if (d < 0.66 && Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth).Equals("Mon"))
		{
			bool foundOne = false;
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				foreach (Quest item in allFarmer.questLog)
				{
					if (item is SocializeQuest)
					{
						foundOne = true;
						break;
					}
				}
				if (foundOne)
				{
					break;
				}
			}
			if (!foundOne)
			{
				return new SocializeQuest();
			}
			return new ItemDeliveryQuest();
		}
		return new ItemDeliveryQuest();
	}

	/// <summary>Get a MonoGame color from a string representation.</summary>
	/// <param name="rawColor">The raw color value to parse. This can be a <see cref="T:Microsoft.Xna.Framework.Color" /> property name (like <c>SkyBlue</c>), RGB or RGBA hex code (like <c>#AABBCC</c> or <c>#AABBCCDD</c>), or 8-bit RGB or RGBA code (like <c>34 139 34</c> or <c>34 139 34 255</c>).</param>
	/// <returns>Returns the matching color (if any), else <c>null</c>.</returns>
	public static Color? StringToColor(string rawColor)
	{
		rawColor = rawColor?.Trim();
		if (string.IsNullOrEmpty(rawColor))
		{
			return null;
		}
		if (rawColor.StartsWith('#'))
		{
			byte alpha = byte.MaxValue;
			if ((rawColor.Length == 7 || rawColor.Length == 9) && byte.TryParse(rawColor.Substring(1, 2), NumberStyles.HexNumber, null, out var red) && byte.TryParse(rawColor.Substring(3, 2), NumberStyles.HexNumber, null, out var green) && byte.TryParse(rawColor.Substring(5, 2), NumberStyles.HexNumber, null, out var blue) && (rawColor.Length == 7 || byte.TryParse(rawColor.Substring(7, 2), NumberStyles.HexNumber, null, out alpha)))
			{
				return new Color(red, green, blue, alpha);
			}
		}
		else if (rawColor.Contains(' '))
		{
			string[] parts = ArgUtility.SplitBySpace(rawColor);
			if ((parts.Length == 3 || parts.Length == 4) && ArgUtility.TryGetInt(parts, 0, out var red, out var error) && ArgUtility.TryGetInt(parts, 1, out var green, out error) && ArgUtility.TryGetInt(parts, 2, out var blue, out error) && ArgUtility.TryGetOptionalInt(parts, 3, out var alpha, out error, 255))
			{
				return new Color(red, green, blue, alpha);
			}
		}
		else
		{
			PropertyInfo property = typeof(Color).GetProperty(rawColor, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			if (property != null)
			{
				return (Color)property.GetValue(null, null);
			}
		}
		Game1.log.Warn("Can't parse '" + rawColor + "' as a color because it's not a hexadecimal code, RGB code, or color name.");
		return null;
	}

	public static Color getOppositeColor(Color color)
	{
		return new Color(255 - color.R, 255 - color.G, 255 - color.B);
	}

	public static void drawLightningBolt(Vector2 strikePosition, GameLocation l)
	{
		Microsoft.Xna.Framework.Rectangle lightningSourceRect = new Microsoft.Xna.Framework.Rectangle(644, 1078, 37, 57);
		Vector2 drawPosition = strikePosition + new Vector2(-lightningSourceRect.Width * 4 / 2, -lightningSourceRect.Height * 4);
		while (drawPosition.Y > (float)(-lightningSourceRect.Height * 4))
		{
			l.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", lightningSourceRect, 9999f, 1, 999, drawPosition, flicker: false, Game1.random.NextBool(), (strikePosition.Y + 32f) / 10000f + 0.001f, 0.025f, Color.White, 4f, 0f, 0f, 0f)
			{
				light = true,
				lightRadius = 2f,
				delayBeforeAnimationStart = 200,
				lightcolor = Color.Black
			});
			drawPosition.Y -= lightningSourceRect.Height * 4;
		}
	}

	/// <summary>Get a translated display text for a calendar date.</summary>
	/// <param name="day">The calendar day of month.</param>
	/// <param name="season">The calendar season.</param>
	/// <param name="year">The calendar year.</param>
	public static string getDateStringFor(int day, int season, int year)
	{
		if (day <= 0)
		{
			day += 28;
			season--;
			if (season < 0)
			{
				season = 3;
				year--;
			}
		}
		else if (day > 28)
		{
			day -= 28;
			season++;
			if (season > 3)
			{
				season = 0;
				year++;
			}
		}
		if (year == 0)
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5677");
		}
		return Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5678", day, (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es) ? getSeasonNameFromNumber(season).ToLower() : getSeasonNameFromNumber(season), year);
	}

	public static string getDateString(int offset = 0)
	{
		int dayOfMonth = Game1.dayOfMonth;
		int currentSeason = Game1.seasonIndex;
		int currentYear = Game1.year;
		return getDateStringFor(dayOfMonth + offset, currentSeason, currentYear);
	}

	public static string getYesterdaysDate()
	{
		return getDateString(-1);
	}

	public static string getSeasonNameFromNumber(int number)
	{
		return number switch
		{
			0 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5680"), 
			1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5681"), 
			2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5682"), 
			3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Utility.cs.5683"), 
			_ => "", 
		};
	}

	public static string getNumberEnding(int number)
	{
		if (number % 100 > 10 && number % 100 < 20)
		{
			return "th";
		}
		switch (number % 10)
		{
		case 0:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
		case 9:
			return "th";
		case 1:
			return "st";
		case 2:
			return "nd";
		case 3:
			return "rd";
		default:
			return "";
		}
	}

	public static void killAllStaticLoopingSoundCues()
	{
		Intro.roadNoise?.Stop(AudioStopOptions.Immediate);
		Fly.buzz?.Stop(AudioStopOptions.Immediate);
		Railroad.trainLoop?.Stop(AudioStopOptions.Immediate);
		BobberBar.reelSound?.Stop(AudioStopOptions.Immediate);
		BobberBar.unReelSound?.Stop(AudioStopOptions.Immediate);
		FishingRod.reelSound?.Stop(AudioStopOptions.Immediate);
		Game1.loopingLocationCues.StopAll();
	}

	public static void consolidateStacks(IList<Item> objects)
	{
		for (int i = 0; i < objects.Count; i++)
		{
			if (!(objects[i] is Object o))
			{
				continue;
			}
			for (int j = i + 1; j < objects.Count; j++)
			{
				if (objects[j] != null && o.canStackWith(objects[j]))
				{
					o.Stack = objects[j].addToStack(o);
					if (o.Stack <= 0)
					{
						break;
					}
				}
			}
		}
		for (int i = objects.Count - 1; i >= 0; i--)
		{
			if (objects[i] != null && objects[i].Stack <= 0)
			{
				objects.RemoveAt(i);
			}
		}
	}

	public static void performLightningUpdate(int time_of_day)
	{
		Random random = CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, time_of_day);
		if (random.NextDouble() < 0.125 + Game1.player.team.AverageDailyLuck() + Game1.player.team.AverageLuckLevel() / 100.0)
		{
			Farm.LightningStrikeEvent lightningEvent = new Farm.LightningStrikeEvent();
			lightningEvent.bigFlash = true;
			Farm farm = Game1.getFarm();
			List<Vector2> lightningRods = new List<Vector2>();
			foreach (KeyValuePair<Vector2, Object> v in farm.objects.Pairs)
			{
				if (v.Value.QualifiedItemId == "(BC)9")
				{
					lightningRods.Add(v.Key);
				}
			}
			if (lightningRods.Count > 0)
			{
				for (int i = 0; i < 2; i++)
				{
					Vector2 v = random.ChooseFrom(lightningRods);
					if (farm.objects[v].heldObject.Value == null)
					{
						farm.objects[v].heldObject.Value = ItemRegistry.Create<Object>("(O)787");
						farm.objects[v].minutesUntilReady.Value = CalculateMinutesUntilMorning(Game1.timeOfDay);
						farm.objects[v].shakeTimer = 1000;
						lightningEvent.createBolt = true;
						lightningEvent.boltPosition = v * 64f + new Vector2(32f, 0f);
						farm.lightningStrikeEvent.Fire(lightningEvent);
						return;
					}
				}
			}
			if (random.NextDouble() < 0.25 - Game1.player.team.AverageDailyLuck() - Game1.player.team.AverageLuckLevel() / 100.0)
			{
				try
				{
					if (TryGetRandom(farm.terrainFeatures, out var tile, out var feature))
					{
						if (feature is FruitTree fruitTree)
						{
							fruitTree.struckByLightningCountdown.Value = 4;
							fruitTree.shake(tile, doEvenIfStillShaking: true);
							lightningEvent.createBolt = true;
							lightningEvent.boltPosition = tile * 64f + new Vector2(32f, -128f);
						}
						else
						{
							Crop crop = (feature as HoeDirt)?.crop;
							bool num = crop != null && !crop.dead;
							if (feature.performToolAction(null, 50, tile))
							{
								lightningEvent.destroyedTerrainFeature = true;
								lightningEvent.createBolt = true;
								farm.terrainFeatures.Remove(tile);
								lightningEvent.boltPosition = tile * 64f + new Vector2(32f, -128f);
							}
							if (num && (bool)crop.dead)
							{
								lightningEvent.createBolt = true;
								lightningEvent.boltPosition = tile * 64f + new Vector2(32f, 0f);
							}
						}
					}
				}
				catch (Exception)
				{
				}
			}
			farm.lightningStrikeEvent.Fire(lightningEvent);
		}
		else if (random.NextDouble() < 0.1)
		{
			Farm.LightningStrikeEvent lightningEvent = new Farm.LightningStrikeEvent();
			lightningEvent.smallFlash = true;
			Farm farm = Game1.getFarm();
			farm.lightningStrikeEvent.Fire(lightningEvent);
		}
	}

	/// <summary>Apply overnight lightning strikes after the player goes to sleep.</summary>
	/// <param name="timeWentToSleep">The time of day when the player went to sleep, in 26-hour format.</param>
	public static void overnightLightning(int timeWentToSleep)
	{
		if (Game1.IsMasterGame)
		{
			int numberOfLoops = (2300 - timeWentToSleep) / 100;
			for (int i = 1; i <= numberOfLoops; i++)
			{
				performLightningUpdate(timeWentToSleep + i * 100);
			}
		}
	}

	public static List<Vector2> getAdjacentTileLocations(Vector2 tileLocation)
	{
		return new List<Vector2>
		{
			new Vector2(-1f, 0f) + tileLocation,
			new Vector2(1f, 0f) + tileLocation,
			new Vector2(0f, 1f) + tileLocation,
			new Vector2(0f, -1f) + tileLocation
		};
	}

	public static Vector2[] getAdjacentTileLocationsArray(Vector2 tileLocation)
	{
		return new Vector2[4]
		{
			new Vector2(-1f, 0f) + tileLocation,
			new Vector2(1f, 0f) + tileLocation,
			new Vector2(0f, 1f) + tileLocation,
			new Vector2(0f, -1f) + tileLocation
		};
	}

	public static Vector2[] getSurroundingTileLocationsArray(Vector2 tileLocation)
	{
		return new Vector2[8]
		{
			new Vector2(-1f, 0f) + tileLocation,
			new Vector2(1f, 0f) + tileLocation,
			new Vector2(0f, 1f) + tileLocation,
			new Vector2(0f, -1f) + tileLocation,
			new Vector2(-1f, -1f) + tileLocation,
			new Vector2(1f, -1f) + tileLocation,
			new Vector2(1f, 1f) + tileLocation,
			new Vector2(-1f, 1f) + tileLocation
		};
	}

	public static Crop findCloseFlower(GameLocation location, Vector2 startTileLocation, int range = -1, Func<Crop, bool> additional_check = null)
	{
		Queue<Vector2> openList = new Queue<Vector2>();
		HashSet<Vector2> closedList = new HashSet<Vector2>();
		openList.Enqueue(startTileLocation);
		for (int attempts = 0; range >= 0 || (range < 0 && attempts <= 150); attempts++)
		{
			if (openList.Count <= 0)
			{
				break;
			}
			Vector2 currentTile = openList.Dequeue();
			HoeDirt dirt = location.GetHoeDirtAtTile(currentTile);
			if (dirt?.crop != null)
			{
				ParsedItemData data = ItemRegistry.GetData(dirt.crop.indexOfHarvest.Value);
				if (data != null && data.Category == -80 && (int)dirt.crop.currentPhase >= dirt.crop.phaseDays.Count - 1 && !dirt.crop.dead && (additional_check == null || additional_check(dirt.crop)))
				{
					return dirt.crop;
				}
			}
			foreach (Vector2 v in getAdjacentTileLocations(currentTile))
			{
				if (!closedList.Contains(v) && (range < 0 || Math.Abs(v.X - startTileLocation.X) + Math.Abs(v.Y - startTileLocation.Y) <= (float)range))
				{
					openList.Enqueue(v);
				}
			}
			closedList.Add(currentTile);
		}
		return null;
	}

	public static void recursiveFenceBuild(Vector2 position, int direction, GameLocation location, Random r)
	{
		if (!(r.NextDouble() < 0.04) && !location.objects.ContainsKey(position) && location.isTileLocationOpen(new Location((int)position.X, (int)position.Y)))
		{
			location.objects.Add(position, new Fence(position, "322", isGate: false));
			int directionToBuild = direction;
			if (r.NextDouble() < 0.16)
			{
				directionToBuild = r.Next(4);
			}
			if (directionToBuild == (direction + 2) % 4)
			{
				directionToBuild = (directionToBuild + 1) % 4;
			}
			switch (direction)
			{
			case 0:
				recursiveFenceBuild(position + new Vector2(0f, -1f), directionToBuild, location, r);
				break;
			case 1:
				recursiveFenceBuild(position + new Vector2(1f, 0f), directionToBuild, location, r);
				break;
			case 3:
				recursiveFenceBuild(position + new Vector2(-1f, 0f), directionToBuild, location, r);
				break;
			case 2:
				recursiveFenceBuild(position + new Vector2(0f, 1f), directionToBuild, location, r);
				break;
			}
		}
	}

	public static bool addAnimalToFarm(FarmAnimal animal)
	{
		if (animal?.Sprite == null)
		{
			return false;
		}
		foreach (Building b in Game1.currentLocation.buildings)
		{
			if (animal.CanLiveIn(b) && b.GetIndoors() is AnimalHouse animalHouse && !animalHouse.isFull())
			{
				animalHouse.adoptAnimal(animal);
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// "Standard" description is as follows:
	/// (Item type [Object (O), BigObject (BO), Weapon (W), Ring (R), Hat (H), Boot (B), Blueprint (BL), Big Object Blueprint(BBL)], follwed by item index, then stack amount)
	/// </summary>
	/// <returns>the described Item object</returns>
	[Obsolete("This is only intended for backwards compatibility with older data. Most code should use ItemRegistry instead.")]
	public static Item getItemFromStandardTextDescription(string description, Farmer who, char delimiter = ' ')
	{
		string[] array = description.Split(delimiter);
		string type = array[0];
		string id = array[1];
		int stock = Convert.ToInt32(array[2]);
		return getItemFromStandardTextDescription(type, id, stock, who);
	}

	/// <summary>
	/// "Standard" description is as follows:
	/// (Item type [Object (O), BigObject (BO), Weapon (W), Ring (R), Hat (H), Boot (B), Blueprint (BL), Big Object Blueprint(BBL)], follwed by item index, then stack amount)
	/// </summary>
	/// <returns>the described Item object</returns>
	[Obsolete("This is only intended for backwards compatibility with older data. Most code should use ItemRegistry instead.")]
	public static Item getItemFromStandardTextDescription(string type, string itemId, int stock, Farmer who)
	{
		Item item = null;
		switch (type)
		{
		case "Furniture":
		case "F":
			item = ItemRegistry.Create("(F)" + itemId);
			break;
		case "Object":
		case "O":
		case "Ring":
		case "R":
			item = ItemRegistry.Create("(O)" + itemId);
			break;
		case "BigObject":
		case "BO":
			item = ItemRegistry.Create("(BC)" + itemId);
			break;
		case "Boot":
		case "B":
			item = ItemRegistry.Create("(B)" + itemId);
			break;
		case "Weapon":
		case "W":
			item = ItemRegistry.Create("(W)" + itemId);
			break;
		case "Blueprint":
		case "BL":
			item = ItemRegistry.Create("(O)" + itemId);
			item.IsRecipe = true;
			break;
		case "Hat":
		case "H":
			item = ItemRegistry.Create("(H)" + itemId);
			break;
		case "BigBlueprint":
		case "BBl":
		case "BBL":
			item = ItemRegistry.Create("(BC)" + itemId);
			item.IsRecipe = true;
			break;
		case "C":
		{
			item = (int.TryParse(itemId, out var index) ? ItemRegistry.Create(((index >= 1000) ? "(S)" : "(P)") + itemId) : ItemRegistry.Create(itemId));
			break;
		}
		}
		item.Stack = stock;
		if (who != null && item.IsRecipe && who.knowsRecipe(item.Name))
		{
			return null;
		}
		return item;
	}

	[Obsolete("This is only intended for backwards compatibility with older data. Most code should use ItemRegistry instead.")]
	public static string getStandardDescriptionFromItem(Item item, int stack, char delimiter = ' ')
	{
		return getStandardDescriptionFromItem(item.TypeDefinitionId, item.ItemId, item.isRecipe, item is Ring, stack, delimiter);
	}

	[Obsolete("This is only intended for backwards compatibility with older data. Most code should use ItemRegistry instead.")]
	public static string getStandardDescriptionFromItem(string typeDefinitionId, string itemId, bool isRecipe, bool isRing, int stack, char delimiter = ' ')
	{
		string identifier;
		switch (typeDefinitionId)
		{
		case "(F)":
			identifier = "F";
			break;
		case "(BC)":
			identifier = (isRecipe ? "BBL" : "BO");
			break;
		case "(O)":
			identifier = ((!isRing) ? (isRecipe ? "BL" : "O") : "R");
			break;
		case "(B)":
			identifier = "B";
			break;
		case "(W)":
			identifier = "W";
			break;
		case "(H)":
			identifier = "H";
			break;
		case "(S)":
		case "(P)":
			identifier = "C";
			break;
		default:
			identifier = "";
			break;
		}
		return identifier + delimiter + itemId + delimiter + stack;
	}

	public static TemporaryAnimatedSpriteList sparkleWithinArea(Microsoft.Xna.Framework.Rectangle bounds, int numberOfSparkles, Color sparkleColor, int delayBetweenSparkles = 100, int delayBeforeStarting = 0, string sparkleSound = "")
	{
		return getTemporarySpritesWithinArea(new int[2] { 10, 11 }, bounds, numberOfSparkles, sparkleColor, delayBetweenSparkles, delayBeforeStarting, sparkleSound);
	}

	public static TemporaryAnimatedSpriteList getTemporarySpritesWithinArea(int[] temporarySpriteRowNumbers, Microsoft.Xna.Framework.Rectangle bounds, int numberOfsprites, Color color, int delayBetweenSprites = 100, int delayBeforeStarting = 0, string sound = "")
	{
		TemporaryAnimatedSpriteList sparkles = new TemporaryAnimatedSpriteList();
		for (int i = 0; i < numberOfsprites; i++)
		{
			sparkles.Add(new TemporaryAnimatedSprite(Game1.random.Choose(temporarySpriteRowNumbers), new Vector2(Game1.random.Next(bounds.X, bounds.Right), Game1.random.Next(bounds.Y, bounds.Bottom)), color)
			{
				delayBeforeAnimationStart = delayBeforeStarting + delayBetweenSprites * i,
				startSound = ((sound.Length > 0) ? sound : null)
			});
		}
		return sparkles;
	}

	public static Vector2 getAwayFromPlayerTrajectory(Microsoft.Xna.Framework.Rectangle monsterBox, Farmer who)
	{
		Point monsterPixel = monsterBox.Center;
		Point playerPixel = who.StandingPixel;
		Vector2 offset = new Vector2(-(playerPixel.X - monsterPixel.X), playerPixel.Y - monsterPixel.Y);
		if (offset.Length() <= 0f)
		{
			switch (who.FacingDirection)
			{
			case 3:
				offset = new Vector2(-1f, 0f);
				break;
			case 1:
				offset = new Vector2(1f, 0f);
				break;
			case 0:
				offset = new Vector2(0f, 1f);
				break;
			case 2:
				offset = new Vector2(0f, -1f);
				break;
			}
		}
		offset.Normalize();
		offset.X *= 50 + Game1.random.Next(-20, 20);
		offset.Y *= 50 + Game1.random.Next(-20, 20);
		return offset;
	}

	/// <summary>Get the cue names that can be played from a jukebox for the current player.</summary>
	/// <param name="player">The player for whom to get music.</param>
	/// <param name="location">The location for whom to get music.</param>
	/// <remarks>See also <see cref="M:StardewValley.Utility.getSongTitleFromCueName(System.String)" />.</remarks>
	public static List<string> GetJukeboxTracks(Farmer player, GameLocation location)
	{
		Dictionary<string, string> cueNamesByAlternativeId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (KeyValuePair<string, JukeboxTrackData> entry in Game1.jukeboxTrackData)
		{
			List<string> alternativeTrackIds = entry.Value.AlternativeTrackIds;
			if (alternativeTrackIds == null || alternativeTrackIds.Count <= 0)
			{
				continue;
			}
			foreach (string id in entry.Value.AlternativeTrackIds)
			{
				if (id != null)
				{
					cueNamesByAlternativeId[id] = entry.Key;
				}
			}
		}
		List<string> tracks = new List<string>();
		HashSet<string> seen = new HashSet<string>();
		foreach (KeyValuePair<string, JukeboxTrackData> entry in Game1.jukeboxTrackData)
		{
			if (entry.Value.Available ?? false)
			{
				tracks.Add(entry.Key);
				seen.Add(entry.Key);
			}
		}
		foreach (string heardId in player.songsHeard)
		{
			string cueName = cueNamesByAlternativeId.GetValueOrDefault(heardId) ?? heardId;
			if (IsValidTrackName(cueName) && seen.Add(cueName) && (!Game1.jukeboxTrackData.TryGetValue(cueName, out var data) || !((!data.Available) ?? false)))
			{
				tracks.Add(cueName);
			}
		}
		return tracks;
	}

	/// <summary>Get whether an audio cue name is valid for the jukebox, regardless of whether it's disabled in <see cref="F:StardewValley.Game1.jukeboxTrackData" />.</summary>
	/// <param name="name">The audio cue name to check.</param>
	/// <remarks>This only checks whether the cue *could* be played by the jukebox. To check whether it's actually available, see <see cref="M:StardewValley.Utility.GetJukeboxTracks(StardewValley.Farmer,StardewValley.GameLocation)" />.</remarks>
	public static bool IsValidTrackName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return false;
		}
		string lowerName = name.ToLower();
		if (lowerName.Contains("ambience") || lowerName.Contains("ambient") || lowerName.Contains("bigdrums") || lowerName.Contains("clubloop"))
		{
			return false;
		}
		if (!Game1.soundBank.Exists(name))
		{
			return false;
		}
		return true;
	}

	/// <summary>Get the jukebox display name for a cue name.</summary>
	/// <param name="cueName">The cue name being played.</param>
	/// <remarks>See also <see cref="M:StardewValley.Utility.GetJukeboxTracks(StardewValley.Farmer,StardewValley.GameLocation)" />.</remarks>
	public static string getSongTitleFromCueName(string cueName)
	{
		if (!string.IsNullOrWhiteSpace(cueName))
		{
			string text = cueName.ToLowerInvariant();
			if (text == "turn_off")
			{
				return Game1.content.LoadString("Strings\\UI:Mini_JukeBox_Off");
			}
			if (text == "random")
			{
				return Game1.content.LoadString("Strings\\StringsFromCSFiles:JukeboxRandomTrack");
			}
			if (Game1.jukeboxTrackData.TryGetValue(cueName, out var data))
			{
				return TokenParser.ParseText(data.Name) ?? cueName;
			}
			foreach (JukeboxTrackData entry in Game1.jukeboxTrackData.Values)
			{
				if (entry.AlternativeTrackIds?.Contains<string>(cueName, StringComparer.OrdinalIgnoreCase) ?? false)
				{
					return TokenParser.ParseText(entry.Name) ?? cueName;
				}
			}
		}
		return cueName;
	}

	public static bool isOffScreenEndFunction(PathNode currentNode, Point endPoint, GameLocation location, Character c)
	{
		if (!isOnScreen(new Vector2(currentNode.x * 64, currentNode.y * 64), 32))
		{
			return true;
		}
		return false;
	}

	public static Vector2 getAwayFromPositionTrajectory(Microsoft.Xna.Framework.Rectangle monsterBox, Vector2 position)
	{
		float num = 0f - (position.X - (float)monsterBox.Center.X);
		float ySlope = position.Y - (float)monsterBox.Center.Y;
		float total = Math.Abs(num) + Math.Abs(ySlope);
		if (total < 1f)
		{
			total = 5f;
		}
		float x = num / total * 20f;
		ySlope = ySlope / total * 20f;
		return new Vector2(x, ySlope);
	}

	public static bool tileWithinRadiusOfPlayer(int xTile, int yTile, int tileRadius, Farmer f)
	{
		Point point = new Point(xTile, yTile);
		Vector2 playerTile = f.Tile;
		if (Math.Abs((float)point.X - playerTile.X) <= (float)tileRadius)
		{
			return Math.Abs((float)point.Y - playerTile.Y) <= (float)tileRadius;
		}
		return false;
	}

	public static bool withinRadiusOfPlayer(int x, int y, int tileRadius, Farmer f)
	{
		Point point = new Point(x / 64, y / 64);
		Vector2 playerTile = f.Tile;
		if (Math.Abs((float)point.X - playerTile.X) <= (float)tileRadius)
		{
			return Math.Abs((float)point.Y - playerTile.Y) <= (float)tileRadius;
		}
		return false;
	}

	public static bool isThereAnObjectHereWhichAcceptsThisItem(GameLocation location, Item item, int x, int y)
	{
		if (item is Tool)
		{
			return false;
		}
		Vector2 tileLocation = new Vector2(x / 64, y / 64);
		foreach (Building building in location.buildings)
		{
			if (building.occupiesTile(tileLocation) && building.performActiveObjectDropInAction(Game1.player, probe: true))
			{
				return true;
			}
		}
		if (location.Objects.TryGetValue(tileLocation, out var obj) && obj.heldObject.Value == null && obj.performObjectDropInAction((Object)item, probe: true, Game1.player))
		{
			return true;
		}
		return false;
	}

	public static FarmAnimal getAnimal(long id)
	{
		FarmAnimal match = null;
		ForEachLocation(delegate(GameLocation location)
		{
			if (location.animals.TryGetValue(id, out var value))
			{
				match = value;
				return false;
			}
			return true;
		});
		return match;
	}

	public static bool isWallpaperOffLimitsForSale(string index)
	{
		if (index.StartsWith("MoreWalls"))
		{
			return true;
		}
		return false;
	}

	public static bool isFlooringOffLimitsForSale(string index)
	{
		return false;
	}

	/// <summary>Open a menu to buy items from a shop, if it exists, using the specified NPC regardless of whether they're present.</summary>
	/// <param name="shopId">The shop ID matching the entry in <c>Data/Shops</c>.</param>
	/// <param name="ownerName">The internal name of the NPC running the shop, or <c>null</c> to open the shop with no NPC portrait/dialogue.</param>
	/// <param name="playOpenSound">Whether to play the open-menu sound.</param>
	/// <returns>Returns whether the shop menu was opened.</returns>
	public static bool TryOpenShopMenu(string shopId, string ownerName, bool playOpenSound = true)
	{
		if (!DataLoader.Shops(Game1.content).TryGetValue(shopId, out var shop))
		{
			return false;
		}
		if (!TryParseEnum<ShopOwnerType>(ownerName, out var ownerType))
		{
			ownerType = ShopOwnerType.NamedNpc;
		}
		ShopOwnerData[] owners = ShopBuilder.GetCurrentOwners(shop).ToArray();
		NPC owner;
		ShopOwnerData ownerData;
		switch (ownerType)
		{
		case ShopOwnerType.Any:
			owner = null;
			ownerData = owners.FirstOrDefault((ShopOwnerData p) => p.Type == ownerType) ?? owners.FirstOrDefault((ShopOwnerData p) => p.Type != ShopOwnerType.None);
			break;
		case ShopOwnerType.AnyOrNone:
			owner = null;
			ownerData = owners.FirstOrDefault((ShopOwnerData p) => p.Type == ownerType) ?? owners.FirstOrDefault();
			break;
		case ShopOwnerType.None:
			owner = null;
			ownerData = owners.FirstOrDefault((ShopOwnerData p) => p.Type == ownerType) ?? owners.FirstOrDefault((ShopOwnerData p) => p.Type == ShopOwnerType.AnyOrNone);
			break;
		default:
			if (ownerName == null)
			{
				owner = null;
				ownerData = owners.FirstOrDefault((ShopOwnerData p) => p.Type == ShopOwnerType.AnyOrNone || p.Type == ShopOwnerType.None);
				break;
			}
			owner = Game1.getCharacterFromName(ownerName);
			ownerData = (from p in owners
				orderby p.Type == ShopOwnerType.NamedNpc descending, p.Type != ShopOwnerType.None descending
				select p).FirstOrDefault((ShopOwnerData p) => p.IsValid(ownerName));
			break;
		}
		Game1.activeClickableMenu = new ShopMenu(shopId, shop, ownerData, owner, null, null, playOpenSound);
		return true;
	}

	/// <summary>Open a menu to buy items from a shop, if it exists and an NPC who can run it is within the specified range.</summary>
	/// <param name="shopId">The shop ID matching the entry in <c>Data/Shops</c>.</param>
	/// <param name="location">The location in which to open the shop menu.</param>
	/// <param name="ownerArea">The tile area to search for an NPC who can run the shop (or <c>null</c> to search the entire location). If no NPC within the area matches the shop's <see cref="F:StardewValley.GameData.Shops.ShopData.Owners" />, the shop won't be opened (unless <paramref name="forceOpen" /> is <c>true</c>).</param>
	/// <param name="maxOwnerY">The maximum Y tile position for an owner NPC, or <c>null</c> for no maximum. This is used for shops that only work if the NPC is behind the counter.</param>
	/// <param name="forceOpen">Whether to open the menu regardless of whether an owner NPC was found.</param>
	/// <param name="playOpenSound">Whether to play the open-menu sound.</param>
	/// <param name="showClosedMessage">Custom logic to handle the closed message if it shouldn't be shown directly.</param>
	/// <returns>Returns whether the shop menu was opened.</returns>
	public static bool TryOpenShopMenu(string shopId, GameLocation location, Microsoft.Xna.Framework.Rectangle? ownerArea = null, int? maxOwnerY = null, bool forceOpen = false, bool playOpenSound = true, Action<string> showClosedMessage = null)
	{
		if (!DataLoader.Shops(Game1.content).TryGetValue(shopId, out var shop))
		{
			return false;
		}
		IList<NPC> characters = location.currentEvent?.actors;
		if (characters == null)
		{
			characters = location.characters;
		}
		NPC owner = null;
		ShopOwnerData ownerData = null;
		ShopOwnerData[] currentOwners = ShopBuilder.GetCurrentOwners(shop).ToArray();
		ShopOwnerData[] array = currentOwners;
		foreach (ShopOwnerData curOwner in array)
		{
			if (forceOpen && curOwner.ClosedMessage != null)
			{
				continue;
			}
			foreach (NPC npc in characters)
			{
				if (curOwner.IsValid(npc.Name))
				{
					Point tile = npc.TilePoint;
					if ((!ownerArea.HasValue || ownerArea.Value.Contains(tile)) && (!maxOwnerY.HasValue || tile.Y <= maxOwnerY))
					{
						owner = npc;
						ownerData = curOwner;
						break;
					}
				}
			}
			if (ownerData != null)
			{
				break;
			}
		}
		if (ownerData == null)
		{
			ownerData = currentOwners.FirstOrDefault((ShopOwnerData p) => (p.Type == ShopOwnerType.AnyOrNone || p.Type == ShopOwnerType.None) && (!forceOpen || p.ClosedMessage == null));
		}
		if (forceOpen && ownerData == null)
		{
			array = currentOwners;
			foreach (ShopOwnerData entry in array)
			{
				if (entry.Type == ShopOwnerType.Any)
				{
					ownerData = entry;
					owner = characters.FirstOrDefault((NPC p) => p.IsVillager);
					if (owner == null)
					{
						ForEachVillager(delegate(NPC npc)
						{
							owner = npc;
							return false;
						});
					}
				}
				else
				{
					owner = Game1.getCharacterFromName(entry.Name);
					if (owner != null)
					{
						ownerData = entry;
					}
				}
				if (ownerData != null)
				{
					break;
				}
			}
		}
		if (ownerData != null && ownerData.ClosedMessage != null)
		{
			string closedMessage = TokenParser.ParseText(ownerData.ClosedMessage);
			if (showClosedMessage != null)
			{
				showClosedMessage(closedMessage);
			}
			else
			{
				Game1.drawObjectDialogue(closedMessage);
			}
			return false;
		}
		if (ownerData != null || forceOpen)
		{
			Game1.activeClickableMenu = new ShopMenu(shopId, shop, ownerData, owner);
			return true;
		}
		return false;
	}

	/// <summary>Apply a set of modifiers to a value.</summary>
	/// <param name="value">The base value to which to apply modifiers.</param>
	/// <param name="modifiers">The modifiers to apply.</param>
	/// <param name="mode">How multiple quantity modifiers should be combined.</param>
	/// <param name="location">The location for which to check queries, or <c>null</c> for the current location.</param>
	/// <param name="player">The player for which to check queries, or <c>null</c> for the current player.</param>
	/// <param name="targetItem">The target item (e.g. machine output or tree fruit) for which to check queries, or <c>null</c> if not applicable.</param>
	/// <param name="inputItem">The input item (e.g. machine input) for which to check queries, or <c>null</c> if not applicable.</param>
	/// <param name="random">The random number generator to use, or <c>null</c> for <see cref="F:StardewValley.Game1.random" />.</param>
	public static float ApplyQuantityModifiers(float value, IList<QuantityModifier> modifiers, QuantityModifier.QuantityModifierMode mode = QuantityModifier.QuantityModifierMode.Stack, GameLocation location = null, Farmer player = null, Item targetItem = null, Item inputItem = null, Random random = null)
	{
		if (modifiers == null || !modifiers.Any())
		{
			return value;
		}
		if (random == null)
		{
			random = Game1.random;
		}
		float? newValue = null;
		foreach (QuantityModifier modifier in modifiers)
		{
			float amount = modifier.Amount;
			List<float> randomAmount = modifier.RandomAmount;
			if (randomAmount != null && randomAmount.Any())
			{
				amount = random.ChooseFrom(modifier.RandomAmount);
			}
			if (!GameStateQuery.CheckConditions(modifier.Condition, location, player, targetItem, inputItem, random))
			{
				continue;
			}
			switch (mode)
			{
			case QuantityModifier.QuantityModifierMode.Minimum:
			{
				float applied = QuantityModifier.Apply(value, modifier.Modification, amount);
				if (!newValue.HasValue || applied < newValue)
				{
					newValue = applied;
				}
				break;
			}
			case QuantityModifier.QuantityModifierMode.Maximum:
			{
				float applied = QuantityModifier.Apply(value, modifier.Modification, amount);
				if (!newValue.HasValue || applied > newValue)
				{
					newValue = applied;
				}
				break;
			}
			default:
				newValue = QuantityModifier.Apply(newValue ?? value, modifier.Modification, amount);
				break;
			}
		}
		return newValue ?? value;
	}

	public static bool IsForbiddenDishOfTheDay(string id)
	{
		switch (id)
		{
		case "346":
		case "196":
		case "216":
		case "224":
		case "206":
		case "395":
			return true;
		default:
			return !ItemRegistry.Exists(id);
		}
	}

	public static bool removeLightSource(int identifier)
	{
		return Game1.currentLightSources.RemoveWhere((LightSource light) => (int)light.identifier == identifier) > 0;
	}

	public static Horse findHorseForPlayer(long uid)
	{
		Horse match = null;
		ForEachLocation(delegate(GameLocation location)
		{
			foreach (NPC character in location.characters)
			{
				if (character is Horse horse && horse.ownerId.Value == uid)
				{
					match = horse;
					return false;
				}
			}
			return true;
		}, includeInteriors: true, includeGenerated: true);
		return match;
	}

	public static Horse findHorse(Guid horseId)
	{
		Horse match = null;
		ForEachLocation(delegate(GameLocation location)
		{
			foreach (NPC character in location.characters)
			{
				if (character is Horse horse && horse.HorseId == horseId)
				{
					match = horse;
					return false;
				}
			}
			return true;
		}, includeInteriors: true, includeGenerated: true);
		return match;
	}

	public static void addDirtPuffs(GameLocation location, int tileX, int tileY, int tilesWide, int tilesHigh, int number = 5)
	{
		for (int x = tileX; x < tileX + tilesWide; x++)
		{
			for (int y = tileY; y < tileY + tilesHigh; y++)
			{
				for (int i = 0; i < number; i++)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.random.Choose(46, 12), new Vector2(x, y) * 64f + new Vector2(Game1.random.Next(-16, 32), Game1.random.Next(-16, 32)), Color.White, 10, Game1.random.NextBool())
					{
						delayBeforeAnimationStart = Math.Max(0, Game1.random.Next(-200, 400)),
						motion = new Vector2(0f, -1f),
						interval = Game1.random.Next(50, 80)
					});
				}
				location.temporarySprites.Add(new TemporaryAnimatedSprite(14, new Vector2(x, y) * 64f + new Vector2(Game1.random.Next(-16, 32), Game1.random.Next(-16, 32)), Color.White, 10, Game1.random.NextBool()));
			}
		}
	}

	public static void addSmokePuff(GameLocation l, Vector2 v, int delay = 0, float baseScale = 2f, float scaleChange = 0.02f, float alpha = 0.75f, float alphaFade = 0.002f)
	{
		TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), v, flipped: false, alphaFade, Color.Gray);
		sprite.alpha = alpha;
		sprite.motion = new Vector2(0f, -0.5f);
		sprite.acceleration = new Vector2(0.002f, 0f);
		sprite.interval = 99999f;
		sprite.layerDepth = 1f;
		sprite.scale = baseScale;
		sprite.scaleChange = scaleChange;
		sprite.rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f;
		sprite.delayBeforeAnimationStart = delay;
		l.temporarySprites.Add(sprite);
	}

	public static LightSource getLightSource(int identifier)
	{
		foreach (LightSource l in Game1.currentLightSources)
		{
			if ((int)l.identifier == identifier)
			{
				return l;
			}
		}
		return null;
	}

	public static int SortAllFurnitures(Furniture a, Furniture b)
	{
		string leftId = a.QualifiedItemId;
		string rightId = b.QualifiedItemId;
		if (leftId != rightId)
		{
			if (leftId == "(F)1226" || leftId == "(F)1308")
			{
				return -1;
			}
			if (rightId == "(F)1226" || rightId == "(F)1308")
			{
				return 1;
			}
		}
		if (a.furniture_type != b.furniture_type)
		{
			return a.furniture_type.Value.CompareTo(b.furniture_type.Value);
		}
		if ((int)a.furniture_type == 12 && (int)b.furniture_type == 12)
		{
			bool num = a.Name.StartsWith("Floor Divider ");
			bool b_is_floor_divider = b.Name.StartsWith("Floor Divider ");
			if (num != b_is_floor_divider)
			{
				if (b_is_floor_divider)
				{
					return -1;
				}
				return 1;
			}
		}
		return a.ItemId.CompareTo(b.ItemId);
	}

	public static bool doesAnyFarmerHaveOrWillReceiveMail(string id)
	{
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			if (allFarmer.hasOrWillReceiveMail(id))
			{
				return true;
			}
		}
		return false;
	}

	public static string loadStringShort(string fileWithinStringsFolder, string key)
	{
		return Game1.content.LoadString("Strings\\" + fileWithinStringsFolder + ":" + key);
	}

	public static bool doesAnyFarmerHaveMail(string id)
	{
		if (Game1.player.mailReceived.Contains(id))
		{
			return true;
		}
		foreach (Farmer value in Game1.otherFarmers.Values)
		{
			if (value.mailReceived.Contains(id))
			{
				return true;
			}
		}
		return false;
	}

	public static FarmEvent pickFarmEvent()
	{
		return Game1.hooks.OnUtility_PickFarmEvent(delegate
		{
			Random random = CreateDaySaveRandom();
			for (int i = 0; i < 10; i++)
			{
				random.NextDouble();
			}
			if (Game1.weddingToday)
			{
				return (FarmEvent)null;
			}
			foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
			{
				Friendship spouseFriendship = onlineFarmer.GetSpouseFriendship();
				if (spouseFriendship != null && spouseFriendship.IsMarried() && spouseFriendship.WeddingDate == Game1.Date)
				{
					return (FarmEvent)null;
				}
			}
			if (Game1.stats.DaysPlayed == 31)
			{
				return new SoundInTheNightEvent(4);
			}
			if (Game1.MasterPlayer.mailForTomorrow.Contains("leoMoved%&NL&%") || Game1.MasterPlayer.mailForTomorrow.Contains("leoMoved"))
			{
				return new WorldChangeEvent(14);
			}
			if (Game1.player.mailForTomorrow.Contains("jojaPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaPantry"))
			{
				return new WorldChangeEvent(0);
			}
			if (Game1.player.mailForTomorrow.Contains("ccPantry%&NL&%") || Game1.player.mailForTomorrow.Contains("ccPantry"))
			{
				return new WorldChangeEvent(1);
			}
			if (Game1.player.mailForTomorrow.Contains("jojaVault%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaVault"))
			{
				return new WorldChangeEvent(6);
			}
			if (Game1.player.mailForTomorrow.Contains("ccVault%&NL&%") || Game1.player.mailForTomorrow.Contains("ccVault"))
			{
				return new WorldChangeEvent(7);
			}
			if (Game1.player.mailForTomorrow.Contains("jojaBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaBoilerRoom"))
			{
				return new WorldChangeEvent(2);
			}
			if (Game1.player.mailForTomorrow.Contains("ccBoilerRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("ccBoilerRoom"))
			{
				return new WorldChangeEvent(3);
			}
			if (Game1.player.mailForTomorrow.Contains("jojaCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaCraftsRoom"))
			{
				return new WorldChangeEvent(4);
			}
			if (Game1.player.mailForTomorrow.Contains("ccCraftsRoom%&NL&%") || Game1.player.mailForTomorrow.Contains("ccCraftsRoom"))
			{
				return new WorldChangeEvent(5);
			}
			if (Game1.player.mailForTomorrow.Contains("jojaFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaFishTank"))
			{
				return new WorldChangeEvent(8);
			}
			if (Game1.player.mailForTomorrow.Contains("ccFishTank%&NL&%") || Game1.player.mailForTomorrow.Contains("ccFishTank"))
			{
				return new WorldChangeEvent(9);
			}
			if (Game1.player.mailForTomorrow.Contains("ccMovieTheaterJoja%&NL&%") || Game1.player.mailForTomorrow.Contains("jojaMovieTheater"))
			{
				return new WorldChangeEvent(10);
			}
			if (Game1.player.mailForTomorrow.Contains("ccMovieTheater%&NL&%") || Game1.player.mailForTomorrow.Contains("ccMovieTheater"))
			{
				return new WorldChangeEvent(11);
			}
			if (Game1.MasterPlayer.eventsSeen.Contains("191393") && (Game1.isRaining || Game1.isLightning) && !Game1.MasterPlayer.mailReceived.Contains("abandonedJojaMartAccessible") && !Game1.MasterPlayer.mailReceived.Contains("ccMovieTheater"))
			{
				return new WorldChangeEvent(12);
			}
			if (Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatTicketMachine") && Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatHull") && Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatAnchor") && !Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed"))
			{
				return new WorldChangeEvent(13);
			}
			if (Game1.MasterPlayer.hasOrWillReceiveMail("activateGoldenParrotsTonight") && !Game1.netWorldState.Value.ActivatedGoldenParrot)
			{
				return new WorldChangeEvent(15);
			}
			if (Game1.player.mailReceived.Contains("ccPantry") && random.NextDouble() < 0.1 && !Game1.MasterPlayer.mailReceived.Contains("raccoonTreeFallen"))
			{
				return new SoundInTheNightEvent(5);
			}
			if (!Game1.player.mailReceived.Contains("sawQiPlane"))
			{
				foreach (Farmer onlineFarmer2 in Game1.getOnlineFarmers())
				{
					if (onlineFarmer2.mailReceived.Contains("gotFirstBillboardPrizeTicket") || Game1.stats.DaysPlayed > 50)
					{
						return new QiPlaneEvent();
					}
				}
			}
			double num = (Game1.getFarm().hasMatureFairyRoseTonight ? 0.007 : 0.0);
			Game1.getFarm().hasMatureFairyRoseTonight = false;
			if (random.NextDouble() < 0.01 + num && !Game1.IsWinter && Game1.dayOfMonth != 1)
			{
				return new FairyEvent();
			}
			if (random.NextDouble() < 0.01 && Game1.stats.DaysPlayed > 20)
			{
				return new WitchEvent();
			}
			if (random.NextDouble() < 0.01 && Game1.stats.DaysPlayed > 5)
			{
				return new SoundInTheNightEvent(1);
			}
			if (random.NextDouble() < 0.005)
			{
				return new SoundInTheNightEvent(3);
			}
			if (random.NextDouble() < 0.008 && Game1.year > 1 && !Game1.MasterPlayer.mailReceived.Contains("Got_Capsule"))
			{
				Game1.player.team.RequestSetMail(PlayerActionTarget.Host, "Got_Capsule", MailType.Received, add: true);
				return new SoundInTheNightEvent(0);
			}
			return (FarmEvent)null;
		});
	}

	public static bool hasFinishedJojaRoute()
	{
		bool foundJoja = false;
		if (Game1.MasterPlayer.mailReceived.Contains("jojaVault"))
		{
			foundJoja = true;
		}
		else if (!Game1.MasterPlayer.mailReceived.Contains("ccVault"))
		{
			return false;
		}
		if (Game1.MasterPlayer.mailReceived.Contains("jojaPantry"))
		{
			foundJoja = true;
		}
		else if (!Game1.MasterPlayer.mailReceived.Contains("ccPantry"))
		{
			return false;
		}
		if (Game1.MasterPlayer.mailReceived.Contains("jojaBoilerRoom"))
		{
			foundJoja = true;
		}
		else if (!Game1.MasterPlayer.mailReceived.Contains("ccBoilerRoom"))
		{
			return false;
		}
		if (Game1.MasterPlayer.mailReceived.Contains("jojaCraftsRoom"))
		{
			foundJoja = true;
		}
		else if (!Game1.MasterPlayer.mailReceived.Contains("ccCraftsRoom"))
		{
			return false;
		}
		if (Game1.MasterPlayer.mailReceived.Contains("jojaFishTank"))
		{
			foundJoja = true;
		}
		else if (!Game1.MasterPlayer.mailReceived.Contains("ccFishTank"))
		{
			return false;
		}
		if (foundJoja || Game1.MasterPlayer.mailReceived.Contains("JojaMember"))
		{
			return true;
		}
		return false;
	}

	public static FarmEvent pickPersonalFarmEvent()
	{
		Random r = CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame / 2, 470124797.0, Game1.player.UniqueMultiplayerID);
		if (Game1.weddingToday)
		{
			return null;
		}
		NPC npcSpouse = Game1.player.getSpouse();
		bool isMarriedOrRoommates = Game1.player.isMarriedOrRoommates();
		if (isMarriedOrRoommates && Game1.player.GetSpouseFriendship().DaysUntilBirthing <= 0 && Game1.player.GetSpouseFriendship().NextBirthingDate != null)
		{
			if (npcSpouse != null)
			{
				return new BirthingEvent();
			}
			long spouseID = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
			if (Game1.otherFarmers.ContainsKey(spouseID))
			{
				return new PlayerCoupleBirthingEvent();
			}
		}
		else
		{
			if (isMarriedOrRoommates)
			{
				bool? flag = npcSpouse?.canGetPregnant();
				if (flag.HasValue && flag.GetValueOrDefault() && Game1.player.currentLocation == Game1.getLocationFromName(Game1.player.homeLocation) && r.NextDouble() < 0.05 && GameStateQuery.CheckConditions(npcSpouse.GetData()?.SpouseWantsChildren))
				{
					return new QuestionEvent(1);
				}
			}
			if (isMarriedOrRoommates && Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).HasValue && Game1.player.GetSpouseFriendship().NextBirthingDate == null && r.NextDouble() < 0.05)
			{
				long spouseID = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
				if (Game1.otherFarmers.TryGetValue(spouseID, out var farmerSpouse))
				{
					Farmer spouse = farmerSpouse;
					if (spouse.currentLocation == Game1.player.currentLocation && (spouse.currentLocation == Game1.getLocationFromName(spouse.homeLocation) || spouse.currentLocation == Game1.getLocationFromName(Game1.player.homeLocation)) && playersCanGetPregnantHere(spouse.currentLocation as FarmHouse))
					{
						return new QuestionEvent(3);
					}
				}
			}
		}
		if (r.NextBool())
		{
			return new QuestionEvent(2);
		}
		return new SoundInTheNightEvent(2);
	}

	private static bool playersCanGetPregnantHere(FarmHouse farmHouse)
	{
		List<Child> kids = farmHouse.getChildren();
		if (farmHouse.cribStyle.Value <= 0)
		{
			return false;
		}
		if (farmHouse.getChildrenCount() < 2 && farmHouse.upgradeLevel >= 2 && kids.Count < 2)
		{
			if (kids.Count != 0)
			{
				return kids[0].Age > 2;
			}
			return true;
		}
		return false;
	}

	public static string capitalizeFirstLetter(string s)
	{
		if (s == null || s.Length < 1)
		{
			return "";
		}
		return s[0].ToString().ToUpper() + ((s.Length > 1) ? s.Substring(1) : "");
	}

	public static void repositionLightSource(int identifier, Vector2 position)
	{
		foreach (LightSource l in Game1.currentLightSources)
		{
			if ((int)l.identifier == identifier)
			{
				l.position.Value = position;
			}
		}
	}

	public static bool areThereAnyOtherAnimalsWithThisName(string name)
	{
		bool found = false;
		if (name != null)
		{
			ForEachLocation(delegate(GameLocation location)
			{
				foreach (FarmAnimal value in location.animals.Values)
				{
					if (value.displayName == name)
					{
						found = true;
						return false;
					}
				}
				return true;
			});
		}
		return found;
	}

	public static string getNumberWithCommas(int number)
	{
		StringBuilder s = new StringBuilder(number.ToString() ?? "");
		string comma;
		switch (LocalizedContentManager.CurrentLanguageCode)
		{
		case LocalizedContentManager.LanguageCode.pt:
		case LocalizedContentManager.LanguageCode.es:
		case LocalizedContentManager.LanguageCode.de:
		case LocalizedContentManager.LanguageCode.hu:
			comma = ".";
			break;
		case LocalizedContentManager.LanguageCode.ru:
			comma = " ";
			break;
		case LocalizedContentManager.LanguageCode.mod:
			comma = LocalizedContentManager.CurrentModLanguage?.NumberComma ?? ",";
			break;
		default:
			comma = ",";
			break;
		}
		for (int i = s.Length - 4; i >= 0; i -= 3)
		{
			s.Insert(i + 1, comma);
		}
		return s.ToString();
	}

	protected static bool _HasBuildingOrUpgrade(GameLocation location, string buildingId)
	{
		if (location.getNumberBuildingsConstructed(buildingId) > 0)
		{
			return true;
		}
		foreach (KeyValuePair<string, BuildingData> pair in Game1.buildingData)
		{
			string curId = pair.Key;
			BuildingData building = pair.Value;
			if (!(curId == buildingId) && building.BuildingToUpgrade == buildingId && _HasBuildingOrUpgrade(location, curId))
			{
				return true;
			}
		}
		return false;
	}

	public static List<int> getDaysOfBooksellerThisSeason()
	{
		Random r = CreateRandom(Game1.year * 11, Game1.uniqueIDForThisGame, Game1.seasonIndex);
		int[] possible_days = null;
		List<int> days = new List<int>();
		switch (Game1.season)
		{
		case Season.Spring:
			possible_days = new int[5] { 11, 12, 21, 22, 25 };
			break;
		case Season.Summer:
			possible_days = new int[5] { 9, 12, 18, 25, 27 };
			break;
		case Season.Fall:
			possible_days = new int[8] { 4, 7, 8, 9, 12, 19, 22, 25 };
			break;
		case Season.Winter:
			possible_days = new int[6] { 5, 11, 12, 19, 22, 24 };
			break;
		}
		int index1 = r.Next(possible_days.Length);
		days.Add(possible_days[index1]);
		days.Add(possible_days[(index1 + possible_days.Length / 2) % possible_days.Length]);
		return days;
	}

	/// <summary>Get whether there's green rain scheduled for today.</summary>
	public static bool isGreenRainDay()
	{
		return isGreenRainDay(Game1.dayOfMonth, Game1.season);
	}

	/// <summary>Get whether there's green rain scheduled on the given day.</summary>
	/// <param name="day">The day of month to check.</param>
	/// <param name="season">The season key to check.</param>
	public static bool isGreenRainDay(int day, Season season)
	{
		if (season == Season.Summer)
		{
			Random r = CreateRandom(Game1.year * 777, Game1.uniqueIDForThisGame);
			int[] possible_days = new int[8] { 5, 6, 7, 14, 15, 16, 18, 23 };
			return day == r.ChooseFrom(possible_days);
		}
		return false;
	}

	public static List<Object> getPurchaseAnimalStock(GameLocation location)
	{
		List<Object> stock = new List<Object>();
		foreach (KeyValuePair<string, FarmAnimalData> pair in Game1.farmAnimalData)
		{
			FarmAnimalData data = pair.Value;
			if (data.PurchasePrice >= 0 && GameStateQuery.CheckConditions(data.UnlockCondition))
			{
				Object o = new Object("100", 1, isRecipe: false, data.PurchasePrice)
				{
					Name = pair.Key,
					Type = null
				};
				if (data.RequiredBuilding != null && !_HasBuildingOrUpgrade(location, data.RequiredBuilding))
				{
					o.Type = ((data.ShopMissingBuildingDescription == null) ? "" : TokenParser.ParseText(data.ShopMissingBuildingDescription));
				}
				o.displayNameFormat = data.ShopDisplayName;
				stock.Add(o);
			}
		}
		return stock;
	}

	public static string SanitizeName(string name)
	{
		return Regex.Replace(name, "[^a-zA-Z0-9]", string.Empty);
	}

	public static void FixChildNameCollisions()
	{
		List<NPC> all_characters = getAllCharacters();
		foreach (NPC character in all_characters)
		{
			if (!(character is Child))
			{
				continue;
			}
			string old_character_name = character.Name;
			string character_name = character.Name;
			bool collision_found;
			do
			{
				collision_found = false;
				if (Game1.characterData.ContainsKey(character_name))
				{
					character_name += " ";
					collision_found = true;
					continue;
				}
				foreach (NPC n in all_characters)
				{
					if (n != character && n.name.Equals(character_name))
					{
						character_name += " ";
						collision_found = true;
					}
				}
			}
			while (collision_found);
			if (!(character_name != character.Name))
			{
				continue;
			}
			character.Name = character_name;
			character.displayName = null;
			foreach (Farmer farmer in Game1.getAllFarmers())
			{
				if (farmer.friendshipData != null && farmer.friendshipData.TryGetValue(old_character_name, out var oldFriendship))
				{
					farmer.friendshipData[character_name] = oldFriendship;
					farmer.friendshipData.Remove(old_character_name);
				}
			}
		}
	}

	public static Vector2 getCornersOfThisRectangle(ref Microsoft.Xna.Framework.Rectangle r, int corner)
	{
		return corner switch
		{
			1 => new Vector2(r.Right - 1, r.Y), 
			2 => new Vector2(r.Right - 1, r.Bottom - 1), 
			3 => new Vector2(r.X, r.Bottom - 1), 
			_ => new Vector2(r.X, r.Y), 
		};
	}

	public static Vector2 GetCurvePoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	{
		float cx = 3f * (p1.X - p0.X);
		float cy = 3f * (p1.Y - p0.Y);
		float bx = 3f * (p2.X - p1.X) - cx;
		float by = 3f * (p2.Y - p1.Y) - cy;
		float num = p3.X - p0.X - cx - bx;
		float ay = p3.Y - p0.Y - cy - by;
		float Cube = t * t * t;
		float Square = t * t;
		float x = num * Cube + bx * Square + cx * t + p0.X;
		float resY = ay * Cube + by * Square + cy * t + p0.Y;
		return new Vector2(x, resY);
	}

	public static GameLocation getGameLocationOfCharacter(NPC n)
	{
		return n.currentLocation;
	}

	public static int[] parseStringToIntArray(string s, char delimiter = ' ')
	{
		string[] split = s.Split(delimiter);
		int[] result = new int[split.Length];
		for (int i = 0; i < split.Length; i++)
		{
			result[i] = Convert.ToInt32(split[i]);
		}
		return result;
	}

	public static void drawLineWithScreenCoordinates(int x1, int y1, int x2, int y2, SpriteBatch b, Color color1, float layerDepth = 1f, int thickness = 1)
	{
		Vector2 vector = new Vector2(x2, y2);
		Vector2 start = new Vector2(x1, y1);
		Vector2 edge = vector - start;
		float angle = (float)Math.Atan2(edge.Y, edge.X);
		b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), thickness), null, color1, angle, new Vector2(0f, 0f), SpriteEffects.None, layerDepth);
		b.Draw(Game1.fadeToBlackRect, new Microsoft.Xna.Framework.Rectangle((int)start.X, (int)start.Y + 1, (int)edge.Length(), thickness), null, color1, angle, new Vector2(0f, 0f), SpriteEffects.None, layerDepth);
	}

	public static Farmer isThereAFarmerWithinDistance(Vector2 tileLocation, int tilesAway, GameLocation location)
	{
		return GetPlayersWithinDistance(tileLocation, tilesAway, location).FirstOrDefault();
	}

	public static Character isThereAFarmerOrCharacterWithinDistance(Vector2 tileLocation, int tilesAway, GameLocation environment)
	{
		Character result = GetNpcsWithinDistance(tileLocation, tilesAway, environment).FirstOrDefault();
		if (result == null)
		{
			result = GetPlayersWithinDistance(tileLocation, tilesAway, environment).FirstOrDefault();
		}
		return result;
	}

	/// <summary>Get all NPCs within a given distance of a tile.</summary>
	/// <param name="centerTile">The tile location around which to find NPCs.</param>
	/// <param name="tilesAway">The maximum tile distance (including diagonal) within which to match NPCs.</param>
	/// <param name="location">The location to search.</param>
	public static IEnumerable<NPC> GetNpcsWithinDistance(Vector2 centerTile, int tilesAway, GameLocation location)
	{
		foreach (NPC npc in location.characters)
		{
			if (Vector2.Distance(npc.Tile, centerTile) <= (float)tilesAway)
			{
				yield return npc;
			}
		}
	}

	/// <summary>Get all players within a given distance of a tile.</summary>
	/// <param name="centerTile">The tile location around which to find NPCs.</param>
	/// <param name="tilesAway">The maximum tile distance (including diagonal) within which to match NPCs.</param>
	/// <param name="location">The location to search.</param>
	public static IEnumerable<Farmer> GetPlayersWithinDistance(Vector2 centerTile, int tilesAway, GameLocation location)
	{
		foreach (Farmer player in location.farmers)
		{
			if (Vector2.Distance(player.Tile, centerTile) <= (float)tilesAway)
			{
				yield return player;
			}
		}
	}

	public static Color getRedToGreenLerpColor(float power)
	{
		return new Color((int)((power <= 0.5f) ? 255f : ((1f - power) * 2f * 255f)), (int)Math.Min(255f, power * 2f * 255f), 0);
	}

	public static FarmHouse getHomeOfFarmer(Farmer who)
	{
		return Game1.RequireLocation<FarmHouse>(who.homeLocation);
	}

	public static Vector2 getRandomPositionOnScreen()
	{
		return new Vector2(Game1.random.Next(Game1.viewport.Width), Game1.random.Next(Game1.viewport.Height));
	}

	public static Vector2 getRandomPositionOnScreenNotOnMap()
	{
		Vector2 output = Vector2.Zero;
		int tries;
		for (tries = 0; tries < 30; tries++)
		{
			if (!output.Equals(Vector2.Zero) && !Game1.currentLocation.isTileOnMap((output + new Vector2(Game1.viewport.X, Game1.viewport.Y)) / 64f))
			{
				break;
			}
			output = getRandomPositionOnScreen();
		}
		if (tries >= 30)
		{
			return new Vector2(-1000f, -1000f);
		}
		return output;
	}

	public static Microsoft.Xna.Framework.Rectangle getRectangleCenteredAt(Vector2 v, int size)
	{
		return new Microsoft.Xna.Framework.Rectangle((int)v.X - size / 2, (int)v.Y - size / 2, size, size);
	}

	public static void checkForBooksReadAchievement()
	{
		if (!Game1.player.achievements.Contains(35) && Game1.player.stats.Get("Book_Trash") != 0 && Game1.player.stats.Get("Book_Crabbing") != 0 && Game1.player.stats.Get("Book_Bombs") != 0 && Game1.player.stats.Get("Book_Roe") != 0 && Game1.player.stats.Get("Book_WildSeeds") != 0 && Game1.player.stats.Get("Book_Woodcutting") != 0 && Game1.player.stats.Get("Book_Defense") != 0 && Game1.player.stats.Get("Book_Friendship") != 0 && Game1.player.stats.Get("Book_Void") != 0 && Game1.player.stats.Get("Book_Speed") != 0 && Game1.player.stats.Get("Book_Marlon") != 0 && Game1.player.stats.Get("Book_PriceCatalogue") != 0 && Game1.player.stats.Get("Book_Diamonds") != 0 && Game1.player.stats.Get("Book_Mystery") != 0 && Game1.player.stats.Get("Book_AnimalCatalogue") != 0 && Game1.player.stats.Get("Book_Speed2") != 0 && Game1.player.stats.Get("Book_Artifact") != 0 && Game1.player.stats.Get("Book_Horse") != 0 && Game1.player.stats.Get("Book_Grass") != 0)
		{
			Game1.getAchievement(35);
		}
	}

	public static bool checkForCharacterInteractionAtTile(Vector2 tileLocation, Farmer who)
	{
		NPC character = Game1.currentLocation.isCharacterAtTile(tileLocation);
		if (character != null && !character.IsMonster && !character.IsInvisible)
		{
			if (character.SimpleNonVillagerNPC && character.nonVillagerNPCTimesTalked != -1)
			{
				Game1.mouseCursor = Game1.cursor_talk;
			}
			else if (Game1.currentLocation is MovieTheater)
			{
				Game1.mouseCursor = Game1.cursor_talk;
			}
			else if (character.Name == "Pierre" && who.ActiveObject?.QualifiedItemId == "(O)897" && character.tryToReceiveActiveObject(who, probe: true))
			{
				Game1.mouseCursor = Game1.cursor_gift;
			}
			else
			{
				bool? flag = who.ActiveItem?.canBeGivenAsGift();
				if (flag.HasValue && flag.GetValueOrDefault() && character.CanReceiveGifts() && !who.isRidingHorse() && who.friendshipData.ContainsKey(character.Name) && !Game1.eventUp)
				{
					Game1.mouseCursor = (character.tryToReceiveActiveObject(who, probe: true) ? Game1.cursor_gift : Game1.cursor_default);
				}
				else if (character.canTalk())
				{
					if (character.CurrentDialogue == null || character.CurrentDialogue.Count <= 0)
					{
						if (Game1.player.spouse != null && character.Name != null && character.Name == Game1.player.spouse && character.shouldSayMarriageDialogue.Value)
						{
							NetList<MarriageDialogueReference, NetRef<MarriageDialogueReference>> currentMarriageDialogue = character.currentMarriageDialogue;
							if (currentMarriageDialogue != null && currentMarriageDialogue.Count > 0)
							{
								goto IL_01fb;
							}
						}
						if (!character.hasTemporaryMessageAvailable() && (!who.hasClubCard || !character.Name.Equals("Bouncer") || !who.IsLocalPlayer) && (!character.Name.Equals("Henchman") || !character.currentLocation.Name.Equals("WitchSwamp") || who.hasOrWillReceiveMail("henchmanGone")))
						{
							goto IL_020d;
						}
					}
					goto IL_01fb;
				}
			}
			goto IL_020d;
		}
		return false;
		IL_020d:
		if (Game1.eventUp && Game1.CurrentEvent != null && !Game1.CurrentEvent.playerControlSequence)
		{
			Game1.mouseCursor = Game1.cursor_default;
		}
		Game1.currentLocation.checkForSpecialCharacterIconAtThisTile(tileLocation);
		if (Game1.mouseCursor == Game1.cursor_gift || Game1.mouseCursor == Game1.cursor_talk)
		{
			if (tileWithinRadiusOfPlayer((int)tileLocation.X, (int)tileLocation.Y, 1, who))
			{
				Game1.mouseCursorTransparency = 1f;
			}
			else
			{
				Game1.mouseCursorTransparency = 0.5f;
			}
		}
		return true;
		IL_01fb:
		if (!character.isOnSilentTemporaryMessage())
		{
			Game1.mouseCursor = Game1.cursor_talk;
		}
		goto IL_020d;
	}

	public static bool canGrabSomethingFromHere(int x, int y, Farmer who)
	{
		if (Game1.currentLocation == null)
		{
			return false;
		}
		Vector2 tileLocation = new Vector2(x / 64, y / 64);
		if (Game1.currentLocation.isObjectAt(x, y))
		{
			Game1.currentLocation.getObjectAt(x, y).hoverAction();
		}
		if (checkForCharacterInteractionAtTile(tileLocation, who))
		{
			return false;
		}
		if (checkForCharacterInteractionAtTile(tileLocation + new Vector2(0f, 1f), who))
		{
			return false;
		}
		if (who.IsLocalPlayer)
		{
			if (who.onBridge.Value)
			{
				return false;
			}
			if (Game1.currentLocation != null)
			{
				foreach (Furniture f in Game1.currentLocation.furniture)
				{
					if (f.GetBoundingBox().Contains(Vector2ToPoint(tileLocation * 64f)) && f.IsTable() && f.heldObject.Value != null)
					{
						return true;
					}
				}
			}
			TerrainFeature terrainFeature;
			if (Game1.currentLocation.Objects.TryGetValue(tileLocation, out var obj))
			{
				if ((bool)obj.readyForHarvest || (bool)obj.isSpawnedObject || (obj is IndoorPot pot && pot.hoeDirt.Value.readyForHarvest()))
				{
					Game1.mouseCursor = Game1.cursor_harvest;
					if (!withinRadiusOfPlayer(x, y, 1, who))
					{
						Game1.mouseCursorTransparency = 0.5f;
						return false;
					}
					return true;
				}
			}
			else if (Game1.currentLocation.terrainFeatures.TryGetValue(tileLocation, out terrainFeature) && terrainFeature is HoeDirt dirt && dirt.readyForHarvest())
			{
				Game1.mouseCursor = Game1.cursor_harvest;
				if (!withinRadiusOfPlayer(x, y, 1, who))
				{
					Game1.mouseCursorTransparency = 0.5f;
					return false;
				}
				return true;
			}
		}
		return false;
	}

	public static int getStringCountInList(List<string> strings, string whichStringToCheck)
	{
		int num = 0;
		if (strings != null)
		{
			foreach (string @string in strings)
			{
				if (@string == whichStringToCheck)
				{
					num++;
				}
			}
		}
		return num;
	}

	public static Microsoft.Xna.Framework.Rectangle getSourceRectWithinRectangularRegion(int regionX, int regionY, int regionWidth, int sourceIndex, int sourceWidth, int sourceHeight)
	{
		int sourceRectWidthsOfRegion = regionWidth / sourceWidth;
		return new Microsoft.Xna.Framework.Rectangle(regionX + sourceIndex % sourceRectWidthsOfRegion * sourceWidth, regionY + sourceIndex / sourceRectWidthsOfRegion * sourceHeight, sourceWidth, sourceHeight);
	}

	public static void drawWithShadow(SpriteBatch b, Texture2D texture, Vector2 position, Microsoft.Xna.Framework.Rectangle sourceRect, Color color, float rotation, Vector2 origin, float scale = -1f, bool flipped = false, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, float shadowIntensity = 0.35f)
	{
		if (scale == -1f)
		{
			scale = 4f;
		}
		if (layerDepth == -1f)
		{
			layerDepth = position.Y / 10000f;
		}
		if (horizontalShadowOffset == -1)
		{
			horizontalShadowOffset = -4;
		}
		if (verticalShadowOffset == -1)
		{
			verticalShadowOffset = 4;
		}
		b.Draw(texture, position + new Vector2(horizontalShadowOffset, verticalShadowOffset), sourceRect, Color.Black * shadowIntensity * ((float)(int)color.A / 255f), rotation, origin, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth - 0.0001f);
		b.Draw(texture, position, sourceRect, color, rotation, origin, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
	}

	public static void drawTextWithShadow(SpriteBatch b, StringBuilder text, SpriteFont font, Vector2 position, Color color, float scale = 1f, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, float shadowIntensity = 1f, int numShadows = 3)
	{
		if (layerDepth == -1f)
		{
			layerDepth = position.Y / 10000f;
		}
		bool longWords = Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.de;
		if (horizontalShadowOffset == -1)
		{
			horizontalShadowOffset = ((font.Equals(Game1.smallFont) || longWords) ? (-2) : (-3));
		}
		if (verticalShadowOffset == -1)
		{
			verticalShadowOffset = ((font.Equals(Game1.smallFont) || longWords) ? 2 : 3);
		}
		if (text == null)
		{
			throw new ArgumentNullException("text");
		}
		b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, verticalShadowOffset), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0001f);
		switch (numShadows)
		{
		case 2:
			b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, 0f), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0002f);
			break;
		case 3:
			b.DrawString(font, text, position + new Vector2(0f, verticalShadowOffset), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0003f);
			break;
		}
		b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
	}

	public static void drawTextWithShadow(SpriteBatch b, string text, SpriteFont font, Vector2 position, Color color, float scale = 1f, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, float shadowIntensity = 1f, int numShadows = 3)
	{
		if (layerDepth == -1f)
		{
			layerDepth = position.Y / 10000f;
		}
		bool longWords = Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.de || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ko;
		if (horizontalShadowOffset == -1)
		{
			horizontalShadowOffset = ((font.Equals(Game1.smallFont) || longWords) ? (-2) : (-3));
		}
		if (verticalShadowOffset == -1)
		{
			verticalShadowOffset = ((font.Equals(Game1.smallFont) || longWords) ? 2 : 3);
		}
		if (text == null)
		{
			text = "";
		}
		b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, verticalShadowOffset), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0001f);
		switch (numShadows)
		{
		case 2:
			b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, 0f), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0002f);
			break;
		case 3:
			b.DrawString(font, text, position + new Vector2(0f, verticalShadowOffset), Game1.textShadowDarkerColor * shadowIntensity, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0003f);
			break;
		}
		b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
	}

	public static void drawTextWithColoredShadow(SpriteBatch b, string text, SpriteFont font, Vector2 position, Color color, Color shadowColor, float scale = 1f, float layerDepth = -1f, int horizontalShadowOffset = -1, int verticalShadowOffset = -1, int numShadows = 3)
	{
		if (layerDepth == -1f)
		{
			layerDepth = position.Y / 10000f;
		}
		bool longWords = Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru || Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.de;
		if (horizontalShadowOffset == -1)
		{
			horizontalShadowOffset = ((font.Equals(Game1.smallFont) || longWords) ? (-2) : (-3));
		}
		if (verticalShadowOffset == -1)
		{
			verticalShadowOffset = ((font.Equals(Game1.smallFont) || longWords) ? 2 : 3);
		}
		if (text == null)
		{
			text = "";
		}
		b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, verticalShadowOffset), shadowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0001f);
		switch (numShadows)
		{
		case 2:
			b.DrawString(font, text, position + new Vector2(horizontalShadowOffset, 0f), shadowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0002f);
			break;
		case 3:
			b.DrawString(font, text, position + new Vector2(0f, verticalShadowOffset), shadowColor, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth - 0.0003f);
			break;
		}
		b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
	}

	public static void drawBoldText(SpriteBatch b, string text, SpriteFont font, Vector2 position, Color color, float scale = 1f, float layerDepth = -1f, int boldnessOffset = 1)
	{
		if (layerDepth == -1f)
		{
			layerDepth = position.Y / 10000f;
		}
		b.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
		b.DrawString(font, text, position + new Vector2(boldnessOffset, 0f), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
		b.DrawString(font, text, position + new Vector2(boldnessOffset, boldnessOffset), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
		b.DrawString(font, text, position + new Vector2(0f, boldnessOffset), color, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
	}

	protected static bool _HasNonMousePlacementLeeway(int x, int y, Item item, Farmer f)
	{
		if (!Game1.isCheckingNonMousePlacement)
		{
			return false;
		}
		Point start_point = f.TilePoint;
		if (!withinRadiusOfPlayer(x, y, 2, f))
		{
			return false;
		}
		if (item.Category == -74)
		{
			return true;
		}
		foreach (Point p in GetPointsOnLine(start_point.X, start_point.Y, x / 64, y / 64))
		{
			if (!(p == start_point) && !item.canBePlacedHere(f.currentLocation, new Vector2(p.X, p.Y), ~(CollisionMask.Characters | CollisionMask.Farmers)))
			{
				return false;
			}
		}
		return true;
	}

	public static bool isPlacementForbiddenHere(GameLocation location)
	{
		if (location == null)
		{
			return true;
		}
		return isPlacementForbiddenHere(location.name);
	}

	public static bool TryGetPassiveFestivalData(string festivalId, out PassiveFestivalData data)
	{
		if (festivalId == null)
		{
			data = null;
			return false;
		}
		return DataLoader.PassiveFestivals(Game1.content).TryGetValue(festivalId, out data);
	}

	/// <summary>Get the passive festival which is active on a given date.</summary>
	/// <param name="dayOfMonth">The day of month to check.</param>
	/// <param name="season">The season to check.</param>
	/// <param name="locationContextId">The location context to check, or <c>null</c> for any location context.</param>
	/// <param name="id">The passive festival ID, if found.</param>
	/// <param name="data">The passive festival data, if found.</param>
	/// <param name="ignoreConditionsCheck">Whether to ignore the custom passive festival conditions, if any.</param>
	public static bool TryGetPassiveFestivalDataForDay(int dayOfMonth, Season season, string locationContextId, out string id, out PassiveFestivalData data, bool ignoreConditionsCheck = false)
	{
		bool checkDateAndConditions = true;
		ICollection<string> possibleIds;
		if (dayOfMonth == Game1.dayOfMonth && season == Game1.season)
		{
			possibleIds = Game1.netWorldState.Value.ActivePassiveFestivals;
			checkDateAndConditions = false;
		}
		else
		{
			possibleIds = DataLoader.PassiveFestivals(Game1.content).Keys;
		}
		foreach (string curId in possibleIds)
		{
			id = curId;
			if (!TryGetPassiveFestivalData(id, out data) || (checkDateAndConditions && (dayOfMonth < data.StartDay || dayOfMonth > data.EndDay || season != data.Season || (!ignoreConditionsCheck && !GameStateQuery.CheckConditions(data.Condition)))))
			{
				continue;
			}
			if (locationContextId != null)
			{
				if (data.MapReplacements == null)
				{
					continue;
				}
				foreach (string key in data.MapReplacements.Keys)
				{
					if (Game1.getLocationFromName(key)?.GetLocationContextId() == locationContextId)
					{
						return true;
					}
				}
				continue;
			}
			return true;
		}
		id = null;
		data = null;
		return false;
	}

	/// <summary>Get whether there's a passive festival scheduled for today.</summary>
	/// <remarks>This doesn't match active festivals like the Flower Dance; see <see cref="M:StardewValley.Utility.isFestivalDay" /> for those.</remarks>
	public static bool IsPassiveFestivalDay()
	{
		string id;
		PassiveFestivalData data;
		return TryGetPassiveFestivalDataForDay(Game1.dayOfMonth, Game1.season, null, out id, out data);
	}

	/// <summary>Get whether there's a passive festival scheduled for the given day.</summary>
	/// <param name="day">The day of month to check.</param>
	/// <param name="season">The season to check.</param>
	/// <param name="locationContextId">The location context to check, or <c>null</c> for any location context.</param>
	/// <remarks>This doesn't match active festivals like the Flower Dance; see <see cref="M:StardewValley.Utility.isFestivalDay(System.Int32,StardewValley.Season)" /> for those.</remarks>
	public static bool IsPassiveFestivalDay(int dayOfMonth, Season season, string locationContextId)
	{
		string id;
		PassiveFestivalData data;
		return TryGetPassiveFestivalDataForDay(dayOfMonth, season, locationContextId, out id, out data);
	}

	/// <summary>Get whether a given passive festival is scheduled for today.</summary>
	/// <param name="festivalId">The passive festival ID.</param>
	/// <remarks>This doesn't match active festivals like the Flower Dance; see <see cref="M:StardewValley.Utility.isFestivalDay" /> for those.</remarks>
	public static bool IsPassiveFestivalDay(string festivalId)
	{
		return Game1.netWorldState.Value.ActivePassiveFestivals.Contains(festivalId);
	}

	public static bool IsPassiveFestivalOpen(string festivalId)
	{
		if (IsPassiveFestivalDay(festivalId) && TryGetPassiveFestivalData(festivalId, out var festival))
		{
			return Game1.timeOfDay >= festival.StartTime;
		}
		return false;
	}

	public static int GetDayOfPassiveFestival(string festivalId)
	{
		if (!IsPassiveFestivalDay(festivalId) || !TryGetPassiveFestivalData(festivalId, out var festival))
		{
			return -1;
		}
		return Game1.dayOfMonth - festival.StartDay + 1;
	}

	public static bool isPlacementForbiddenHere(string location_name)
	{
		if (location_name == "AbandonedJojaMart")
		{
			return true;
		}
		foreach (string activePassiveFestival in Game1.netWorldState.Value.ActivePassiveFestivals)
		{
			if (!TryGetPassiveFestivalData(activePassiveFestival, out var festival) || festival.MapReplacements == null)
			{
				continue;
			}
			foreach (string festivalLocationName in festival.MapReplacements.Values)
			{
				if (location_name == festivalLocationName)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static void transferPlacedObjectsFromOneLocationToAnother(GameLocation source, GameLocation destination, Vector2? overflow_chest_position = null, GameLocation overflow_chest_location = null)
	{
		if (source == null)
		{
			return;
		}
		List<Item> invalid_objects = new List<Item>();
		foreach (Vector2 position in new List<Vector2>(source.objects.Keys))
		{
			if (source.objects[position] == null)
			{
				continue;
			}
			Object source_object = source.objects[position];
			bool valid = true;
			if (destination == null)
			{
				valid = false;
			}
			if (valid && destination.objects.ContainsKey(position))
			{
				valid = false;
			}
			if (valid && !destination.CanItemBePlacedHere(position))
			{
				valid = false;
			}
			source.objects.Remove(position);
			if (valid && destination != null)
			{
				destination.objects[position] = source_object;
				continue;
			}
			invalid_objects.Add(source_object);
			if (!(source_object is Chest source_chest))
			{
				continue;
			}
			List<Item> chest_items = new List<Item>(source_chest.Items);
			source_chest.Items.Clear();
			foreach (Item chest_item in chest_items)
			{
				if (chest_item != null)
				{
					invalid_objects.Add(chest_item);
				}
			}
		}
		if (overflow_chest_position.HasValue)
		{
			if (overflow_chest_location != null)
			{
				createOverflowChest(overflow_chest_location, overflow_chest_position.Value, invalid_objects);
			}
			else if (destination != null)
			{
				createOverflowChest(destination, overflow_chest_position.Value, invalid_objects);
			}
		}
	}

	public static void createOverflowChest(GameLocation destination, Vector2 overflow_chest_location, List<Item> overflow_items)
	{
		List<Chest> chests = new List<Chest>();
		foreach (Item overflow_object in overflow_items)
		{
			if (chests.Count == 0)
			{
				chests.Add(new Chest(playerChest: true));
			}
			bool found_chest_to_stash_in = false;
			foreach (Chest item in chests)
			{
				if (item.addItem(overflow_object) == null)
				{
					found_chest_to_stash_in = true;
					break;
				}
			}
			if (!found_chest_to_stash_in)
			{
				Chest new_chest = new Chest(playerChest: true);
				new_chest.addItem(overflow_object);
				chests.Add(new_chest);
			}
		}
		for (int i = 0; i < chests.Count; i++)
		{
			Chest chest = chests[i];
			_placeOverflowChestInNearbySpace(destination, overflow_chest_location, chest);
		}
	}

	protected static void _placeOverflowChestInNearbySpace(GameLocation location, Vector2 tileLocation, Object o)
	{
		if (o == null || tileLocation.Equals(Vector2.Zero))
		{
			return;
		}
		int attempts = 0;
		Queue<Vector2> open_list = new Queue<Vector2>();
		HashSet<Vector2> closed_list = new HashSet<Vector2>();
		open_list.Enqueue(tileLocation);
		Vector2 current = Vector2.Zero;
		for (; attempts < 100; attempts++)
		{
			current = open_list.Dequeue();
			if (location.CanItemBePlacedHere(current))
			{
				break;
			}
			closed_list.Add(current);
			foreach (Vector2 v in getAdjacentTileLocations(current))
			{
				if (!closed_list.Contains(v))
				{
					open_list.Enqueue(v);
				}
			}
		}
		if (!current.Equals(Vector2.Zero) && location.CanItemBePlacedHere(current))
		{
			o.TileLocation = current;
			location.objects.Add(current, o);
		}
	}

	public static bool isWithinTileWithLeeway(int x, int y, Item item, Farmer f)
	{
		if (!withinRadiusOfPlayer(x, y, 1, f))
		{
			return _HasNonMousePlacementLeeway(x, y, item, f);
		}
		return true;
	}

	public static bool playerCanPlaceItemHere(GameLocation location, Item item, int x, int y, Farmer f, bool show_error = false)
	{
		if (isPlacementForbiddenHere(location))
		{
			return false;
		}
		if (item == null || item is Tool || Game1.eventUp || (bool)f.bathingClothes || f.onBridge.Value)
		{
			return false;
		}
		if (isWithinTileWithLeeway(x, y, item, f) || (item is Wallpaper && location is DecoratableLocation) || (item is Furniture curFurniture && location.CanPlaceThisFurnitureHere(curFurniture)))
		{
			if (item is Furniture furniture && !location.CanFreePlaceFurniture() && !furniture.IsCloseEnoughToFarmer(f, x / 64, y / 64))
			{
				return false;
			}
			Vector2 tileLocation = new Vector2(x / 64, y / 64);
			if (item.canBePlacedHere(location, tileLocation, CollisionMask.All, show_error))
			{
				return item.isPlaceable();
			}
		}
		return false;
	}

	public static string GetDoubleWideVersionOfBed(string bedId)
	{
		if (int.TryParse(bedId, out var bed_index))
		{
			return (bed_index + 4).ToString();
		}
		if (bedId == "BluePinstripeBed")
		{
			return "BluePinstripeDoubleBed";
		}
		return BedFurniture.DOUBLE_BED_INDEX;
	}

	public static int getDirectionFromChange(Vector2 current, Vector2 previous)
	{
		if (current.X > previous.X)
		{
			return 1;
		}
		if (current.X < previous.X)
		{
			return 3;
		}
		if (current.Y > previous.Y)
		{
			return 2;
		}
		if (current.Y < previous.Y)
		{
			return 0;
		}
		return -1;
	}

	public static bool doesRectangleIntersectTile(Microsoft.Xna.Framework.Rectangle r, int tileX, int tileY)
	{
		Microsoft.Xna.Framework.Rectangle tileRect = new Microsoft.Xna.Framework.Rectangle(tileX * 64, tileY * 64, 64, 64);
		return r.Intersects(tileRect);
	}

	public static bool IsHospitalVisitDay(string character_name)
	{
		try
		{
			Dictionary<string, string> dictionary = Game1.content.Load<Dictionary<string, string>>("Characters\\schedules\\" + character_name);
			string day_key = Game1.currentSeason + "_" + Game1.dayOfMonth;
			if (dictionary.TryGetValue(day_key, out var scheduleScript) && scheduleScript.Contains("Hospital"))
			{
				return true;
			}
		}
		catch (Exception)
		{
		}
		return false;
	}

	/// <summary>Get all characters of any type (including villagers, horses, pets, monsters, player children, etc).</summary>
	/// <remarks>This creates a new list each time it's called, which is inefficient for hot paths. Consider using <see cref="M:StardewValley.Utility.ForEachCharacter(System.Func{StardewValley.NPC,System.Boolean},System.Boolean)" /> if you don't need an actual list (e.g. you just need to iterate them once).</remarks>
	public static List<NPC> getAllCharacters()
	{
		List<NPC> list = new List<NPC>();
		ForEachCharacter(delegate(NPC npc)
		{
			list.Add(npc);
			return true;
		});
		return list;
	}

	/// <summary>Get all villager NPCs (excluding horses, pets, monsters, player children, etc).</summary>
	/// <remarks>This creates a new list each time it's called, which is inefficient for hot paths. Consider using <see cref="M:StardewValley.Utility.ForEachVillager(System.Func{StardewValley.NPC,System.Boolean},System.Boolean)" /> if you don't need an actual list (e.g. you just need to iterate them once).</remarks>
	public static List<NPC> getAllVillagers()
	{
		List<NPC> list = new List<NPC>();
		ForEachVillager(delegate(NPC npc)
		{
			list.Add(npc);
			return true;
		});
		return list;
	}

	/// <summary>Apply special conversion rules when equipping an item. For example, this is used to convert a Copper Pan tool into a hat.</summary>
	/// <param name="placedItem">The item being equipped.</param>
	public static Item PerformSpecialItemPlaceReplacement(Item placedItem)
	{
		Item newItem;
		switch (placedItem?.QualifiedItemId)
		{
		case "(T)Pan":
			newItem = ItemRegistry.Create("(H)71");
			break;
		case "(T)SteelPan":
			newItem = ItemRegistry.Create("(H)SteelPanHat");
			break;
		case "(T)GoldPan":
			newItem = ItemRegistry.Create("(H)GoldPanHat");
			break;
		case "(T)IridiumPan":
			newItem = ItemRegistry.Create("(H)IridiumPanHat");
			break;
		case "(O)71":
			newItem = ItemRegistry.Create("(P)15");
			break;
		default:
			return placedItem;
		}
		newItem.modData.CopyFrom(placedItem.modData);
		if (newItem is Hat newHat && placedItem is Tool fromTool)
		{
			newHat.enchantments.AddRange(fromTool.enchantments);
			newHat.previousEnchantments.AddRange(fromTool.previousEnchantments);
		}
		return newItem;
	}

	/// <summary>Apply special conversion rules when un-equipping an item. For example, this is used to convert a Copper Pan hat back into a tool.</summary>
	/// <param name="placedItem">The item being equipped.</param>
	public static Item PerformSpecialItemGrabReplacement(Item heldItem)
	{
		Item newItem;
		switch (heldItem?.QualifiedItemId)
		{
		case "(P)15":
		{
			Object @object = ItemRegistry.Create<Object>("(O)71");
			@object.questItem.Value = true;
			@object.questId.Value = "102";
			newItem = @object;
			break;
		}
		case "(H)71":
			newItem = ItemRegistry.Create("(T)Pan");
			break;
		case "(H)SteelPanHat":
			newItem = ItemRegistry.Create("(T)SteelPan");
			break;
		case "(H)GoldPanHat":
			newItem = ItemRegistry.Create("(T)GoldPan");
			break;
		case "(H)IridiumPanHat":
			newItem = ItemRegistry.Create("(T)IridiumPan");
			break;
		default:
			return heldItem;
		}
		newItem.modData.CopyFrom(heldItem.modData);
		if (newItem is Pan newPan && heldItem is Hat fromHat)
		{
			newPan.enchantments.AddRange(fromHat.enchantments);
			newPan.previousEnchantments.AddRange(fromHat.previousEnchantments);
		}
		return newItem;
	}

	/// <summary>Perform an action for every item stored in chests or storage furniture, or placed on furniture.</summary>
	/// <param name="action">The action to perform.</param>
	/// <remarks>See also <see cref="M:StardewValley.Utility.ForEachItem(System.Func{StardewValley.Item,System.Boolean})" /> to iterate all items, regardless of where they are.</remarks>
	public static void iterateChestsAndStorage(Action<Item> action)
	{
		ForEachLocation(delegate(GameLocation l)
		{
			Chest fridge = l.GetFridge(onlyUnlocked: false);
			fridge?.ForEachItem(Handle);
			foreach (Object current in l.objects.Values)
			{
				if (current != fridge)
				{
					if (current is Chest)
					{
						current.ForEachItem(Handle);
					}
					else if (current.heldObject.Value is Chest chest)
					{
						chest.ForEachItem(Handle);
					}
				}
			}
			foreach (Furniture item2 in l.furniture)
			{
				item2.ForEachItem(Handle);
			}
			foreach (Building building in l.buildings)
			{
				foreach (Chest buildingChest in building.buildingChests)
				{
					buildingChest.ForEachItem(Handle);
				}
			}
			return true;
		});
		foreach (Item item in Game1.player.team.returnedDonations)
		{
			if (item != null)
			{
				action(item);
			}
		}
		foreach (Inventory value in Game1.player.team.globalInventories.Values)
		{
			foreach (Item item in (IEnumerable<Item>)value)
			{
				if (item != null)
				{
					action(item);
				}
			}
		}
		foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
		{
			foreach (Item item in specialOrder.donatedItems)
			{
				if (item != null)
				{
					action(item);
				}
			}
		}
		bool Handle(Item item, Action remove, Action<Item> replaceWith)
		{
			action(item);
			return true;
		}
	}

	public static Item removeItemFromInventory(int whichItemIndex, IList<Item> items)
	{
		if (whichItemIndex >= 0 && whichItemIndex < items.Count && items[whichItemIndex] != null)
		{
			Item tmp = items[whichItemIndex];
			if (whichItemIndex == Game1.player.CurrentToolIndex && items.Equals(Game1.player.Items))
			{
				tmp?.actionWhenStopBeingHeld(Game1.player);
			}
			items[whichItemIndex] = null;
			return tmp;
		}
		return null;
	}

	/// <summary>Get a random available NPC listed in <c>Data/Characters</c> whose <see cref="F:StardewValley.GameData.Characters.CharacterData.HomeRegion" /> is <see cref="F:StardewValley.NPC.region_town" />.</summary>
	/// <param name="random">The RNG with which to choose an NPC.</param>
	/// <remarks>See also <see cref="M:StardewValley.Utility.getRandomNpcFromHomeRegion(System.String,System.Random)" />.</remarks>
	public static NPC getRandomTownNPC(Random random = null)
	{
		return getRandomNpcFromHomeRegion("Town", random);
	}

	/// <summary>Get a random available NPC listed in <c>Data/Characters</c> with a given <see cref="F:StardewValley.GameData.Characters.CharacterData.HomeRegion" />.</summary>
	/// <param name="region">The region to match.</param>
	/// <param name="random">The RNG with which to choose an NPC.</param>
	public static NPC getRandomNpcFromHomeRegion(string region, Random random = null)
	{
		return GetRandomNpc((string name, CharacterData data) => data.HomeRegion == region, random);
	}

	/// <summary>Get a random available NPC listed in <c>Data/Characters</c> which can give or receive gifts at the Feast of the Winter Star.</summary>
	/// <param name="ignoreNpc">Whether to exclude an NPC from the selection.</param>
	public static NPC GetRandomWinterStarParticipant(Func<string, bool> ignoreNpc = null)
	{
		return GetRandomNpc(delegate(string name, CharacterData data)
		{
			Func<string, bool> func = ignoreNpc;
			return (func == null || !func(name)) && ((data.WinterStarParticipant == null) ? (data.HomeRegion == "Town") : GameStateQuery.CheckConditions(data.WinterStarParticipant));
		}, CreateRandom(Game1.uniqueIDForThisGame / 2, Game1.year, Game1.player.UniqueMultiplayerID));
	}

	/// <summary>Get a random available NPC listed in <c>Data/Characters</c>.</summary>
	/// <param name="match">A predicate matching the NPCs to include, or <c>null</c> to allow any valid match.</param>
	/// <param name="random">The RNG with which to choose an NPC.</param>
	/// <param name="mustBeSocial">Whether to only include NPCs whose <see cref="P:StardewValley.NPC.CanSocialize" /> property is true.</param>
	public static NPC GetRandomNpc(Func<string, CharacterData, bool> match = null, Random random = null, bool mustBeSocial = true)
	{
		List<string> npcNames = new List<string>();
		foreach (KeyValuePair<string, CharacterData> entry in Game1.characterData)
		{
			if (match == null || match(entry.Key, entry.Value))
			{
				npcNames.Add(entry.Key);
			}
		}
		random = random ?? Game1.random;
		while (npcNames.Count > 0)
		{
			int index = random.Next(npcNames.Count);
			NPC npc = Game1.getCharacterFromName(npcNames[index]);
			if (npc != null && (!mustBeSocial || npc.CanSocialize))
			{
				return npc;
			}
			npcNames.RemoveAt(index);
		}
		return null;
	}

	public static bool foundAllStardrops(Farmer who = null)
	{
		if (who == null)
		{
			who = Game1.player;
		}
		if (who.mailReceived.Contains("gotMaxStamina"))
		{
			return true;
		}
		if (who.hasOrWillReceiveMail("CF_Fair") && who.hasOrWillReceiveMail("CF_Fish") && (who.hasOrWillReceiveMail("CF_Mines") || (who.chestConsumedMineLevels.ContainsKey(100) && who.chestConsumedMineLevels[100])) && who.hasOrWillReceiveMail("CF_Sewer") && who.hasOrWillReceiveMail("museumComplete") && who.hasOrWillReceiveMail("CF_Spouse"))
		{
			return who.hasOrWillReceiveMail("CF_Statue");
		}
		return false;
	}

	public static int numStardropsFound(Farmer who = null)
	{
		if (who == null)
		{
			who = Game1.player;
		}
		int num = 0;
		if (who.hasOrWillReceiveMail("CF_Fair"))
		{
			num++;
		}
		if (who.hasOrWillReceiveMail("CF_Fish"))
		{
			num++;
		}
		if (who.hasOrWillReceiveMail("CF_Mines") || (who.chestConsumedMineLevels.ContainsKey(100) && who.chestConsumedMineLevels[100]))
		{
			num++;
		}
		if (who.hasOrWillReceiveMail("CF_Sewer"))
		{
			num++;
		}
		if (who.hasOrWillReceiveMail("museumComplete"))
		{
			num++;
		}
		if (who.hasOrWillReceiveMail("CF_Spouse"))
		{
			num++;
		}
		if (who.hasOrWillReceiveMail("CF_Statue"))
		{
			num++;
		}
		return num;
	}

	/// <summary>
	/// Can range from 0 to 21.
	///
	///    if (points &gt;= 12) 4
	///     if (points &gt;= 8) 3
	///   if (points &gt;= 4)  2
	///    else 1
	/// those are the number of candles that will be light on grandpa's shrine.
	/// </summary>
	/// <returns></returns>
	public static int getGrandpaScore()
	{
		int points = 0;
		if (Game1.player.totalMoneyEarned >= 50000)
		{
			points++;
		}
		if (Game1.player.totalMoneyEarned >= 100000)
		{
			points++;
		}
		if (Game1.player.totalMoneyEarned >= 200000)
		{
			points++;
		}
		if (Game1.player.totalMoneyEarned >= 300000)
		{
			points++;
		}
		if (Game1.player.totalMoneyEarned >= 500000)
		{
			points++;
		}
		if (Game1.player.totalMoneyEarned >= 1000000)
		{
			points += 2;
		}
		if (Game1.player.achievements.Contains(5))
		{
			points++;
		}
		if (Game1.player.hasSkullKey)
		{
			points++;
		}
		bool num = Game1.isLocationAccessible("CommunityCenter");
		if (num || Game1.player.hasCompletedCommunityCenter())
		{
			points++;
		}
		if (num)
		{
			points += 2;
		}
		if (Game1.player.isMarriedOrRoommates() && getHomeOfFarmer(Game1.player).upgradeLevel >= 2)
		{
			points++;
		}
		if (Game1.player.hasRustyKey)
		{
			points++;
		}
		if (Game1.player.achievements.Contains(26))
		{
			points++;
		}
		if (Game1.player.achievements.Contains(34))
		{
			points++;
		}
		int numberOfFriendsWithinThisRange = getNumberOfFriendsWithinThisRange(Game1.player, 1975, 999999);
		if (numberOfFriendsWithinThisRange >= 5)
		{
			points++;
		}
		if (numberOfFriendsWithinThisRange >= 10)
		{
			points++;
		}
		int level = Game1.player.Level;
		if (level >= 15)
		{
			points++;
		}
		if (level >= 25)
		{
			points++;
		}
		if (Game1.player.mailReceived.Contains("petLoveMessage"))
		{
			points++;
		}
		return points;
	}

	public static int getGrandpaCandlesFromScore(int score)
	{
		if (score >= 12)
		{
			return 4;
		}
		if (score >= 8)
		{
			return 3;
		}
		if (score >= 4)
		{
			return 2;
		}
		return 1;
	}

	public static bool canItemBeAddedToThisInventoryList(Item i, IList<Item> list, int listMaxSpace = -1)
	{
		if (listMaxSpace != -1 && list.Count < listMaxSpace)
		{
			return true;
		}
		int stack = i.Stack;
		foreach (Item it in list)
		{
			if (it == null)
			{
				return true;
			}
			if (it.canStackWith(i) && it.getRemainingStackSpace() > 0)
			{
				stack -= it.getRemainingStackSpace();
				if (stack <= 0)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>Parse a raw direction string into a number matching one of the constants like <see cref="F:StardewValley.Game1.up" />.</summary>
	/// <param name="direction">The raw direction value. This can be a case-insensitive name (<c>up</c>, <c>down</c>, <c>left</c>, or <c>right</c>) or a numeric value matching a contant like <see cref="F:StardewValley.Game1.up" />.</param>
	/// <param name="parsed">The parsed value matching a constant like <see cref="F:StardewValley.Game1.up" />, or <c>-1</c> if not valid.</param>
	/// <returns>Returns whether the value was successfully parsed.</returns>
	public static bool TryParseDirection(string direction, out int parsed)
	{
		if (string.IsNullOrWhiteSpace(direction))
		{
			parsed = -1;
			return false;
		}
		if (direction.Equals("up", StringComparison.OrdinalIgnoreCase))
		{
			parsed = 0;
			return true;
		}
		if (direction.Equals("down", StringComparison.OrdinalIgnoreCase))
		{
			parsed = 2;
			return true;
		}
		if (direction.Equals("left", StringComparison.OrdinalIgnoreCase))
		{
			parsed = 3;
			return true;
		}
		if (direction.Equals("right", StringComparison.OrdinalIgnoreCase))
		{
			parsed = 1;
			return true;
		}
		if (int.TryParse(direction, out parsed))
		{
			int num = parsed;
			if ((uint)num <= 3u)
			{
				return true;
			}
		}
		parsed = -1;
		return false;
	}

	public static int GetNumberOfItemThatCanBeAddedToThisInventoryList(Item item, IList<Item> list, int listMaxItems)
	{
		int addableStacks = 0;
		foreach (Item existingStack in list)
		{
			if (existingStack == null)
			{
				addableStacks += item.maximumStackSize();
			}
			else if (existingStack != null && existingStack.canStackWith(item) && existingStack.getRemainingStackSpace() > 0)
			{
				addableStacks += existingStack.getRemainingStackSpace();
			}
		}
		for (int i = 0; i < listMaxItems - list.Count; i++)
		{
			addableStacks += item.maximumStackSize();
		}
		return addableStacks;
	}

	/// <summary>Add an item to an inventory list if there's room for it.</summary>
	/// <param name="i">The item to add.</param>
	/// <param name="list">The inventory list to add it to.</param>
	/// <param name="listMaxSpace">The maximum number of item slots allowed in the <paramref name="list" />, or <c>-1</c> for no limit.</param>
	/// <returns>If the item was fully added to the inventory, returns <c>null</c>. Else returns the input item with its stack reduced to the amount that couldn't be added.</returns>
	public static Item addItemToThisInventoryList(Item i, IList<Item> list, int listMaxSpace = -1)
	{
		if (i.Stack == 0)
		{
			i.Stack = 1;
		}
		foreach (Item it in list)
		{
			if (it != null && it.canStackWith(i) && it.getRemainingStackSpace() > 0)
			{
				if (i is Object obj)
				{
					obj.stack.Value = it.addToStack(i);
				}
				else
				{
					i.Stack = it.addToStack(i);
				}
				if (i.Stack <= 0)
				{
					return null;
				}
			}
		}
		for (int j = list.Count - 1; j >= 0; j--)
		{
			if (list[j] == null)
			{
				if (i.Stack <= i.maximumStackSize())
				{
					list[j] = i;
					return null;
				}
				list[j] = i.getOne();
				list[j].Stack = i.maximumStackSize();
				if (i is Object obj)
				{
					obj.stack.Value -= i.maximumStackSize();
				}
				else
				{
					i.Stack -= i.maximumStackSize();
				}
			}
		}
		while (listMaxSpace != -1 && list.Count < listMaxSpace)
		{
			if (i.Stack > i.maximumStackSize())
			{
				Item tmp = i.getOne();
				tmp.Stack = i.maximumStackSize();
				if (i is Object obj)
				{
					obj.stack.Value -= i.maximumStackSize();
				}
				else
				{
					i.Stack -= i.maximumStackSize();
				}
				list.Add(tmp);
				continue;
			}
			list.Add(i);
			return null;
		}
		return i;
	}

	/// <summary>Add an item to an inventory list at a specific index position. If there's already an item at that position, the stacks are merged (if possible) else they're swapped.</summary>
	/// <param name="item">The item to add.</param>
	/// <param name="position">The index position within the list at which to add the item.</param>
	/// <param name="items">The inventory list to add it to.</param>
	/// <param name="onAddFunction">The callback to invoke when an item is added to the inventory.</param>
	/// <returns>If the item was fully added to the inventory, returns <c>null</c>. If it replaced an item stack previously at that position, returns the replaced item stack. Else returns the input item with its stack reduced to the amount that couldn't be added.</returns>
	public static Item addItemToInventory(Item item, int position, IList<Item> items, ItemGrabMenu.behaviorOnItemSelect onAddFunction = null)
	{
		bool isCurrentPlayer = items.Equals(Game1.player.Items);
		if (isCurrentPlayer)
		{
			Game1.player.GetItemReceiveBehavior(item, out var needsInventorySpace, out var _);
			if (!needsInventorySpace)
			{
				Game1.player.OnItemReceived(item, item.Stack, null);
				return null;
			}
		}
		if (position >= 0 && position < items.Count)
		{
			if (items[position] == null)
			{
				items[position] = item;
				if (isCurrentPlayer)
				{
					Game1.player.OnItemReceived(item, item.Stack, null);
				}
				onAddFunction?.Invoke(item, null);
				return null;
			}
			if (item.canStackWith(items[position]))
			{
				int originalStack = item.Stack;
				int stackLeft = items[position].addToStack(item);
				if (isCurrentPlayer)
				{
					Game1.player.OnItemReceived(item, originalStack - stackLeft, items[position]);
				}
				if (stackLeft <= 0)
				{
					return null;
				}
				item.Stack = stackLeft;
				onAddFunction?.Invoke(item, null);
				return item;
			}
			Item tmp = items[position];
			if (position == Game1.player.CurrentToolIndex && items.Equals(Game1.player.Items) && tmp != null)
			{
				tmp.actionWhenStopBeingHeld(Game1.player);
				item.actionWhenBeingHeld(Game1.player);
			}
			items[position] = item;
			if (isCurrentPlayer)
			{
				Game1.player.OnItemReceived(item, item.Stack, null);
			}
			onAddFunction?.Invoke(item, null);
			return tmp;
		}
		return item;
	}

	/// <summary>
	/// called on monster kill, breakable container open, tree chop, tree shake w/ seed, diggable spots. ChanceModifier is adjusted per each source to account for the frequency of source hits
	/// </summary>
	public static bool trySpawnRareObject(Farmer who, Vector2 position, GameLocation location, double chanceModifier = 1.0, double dailyLuckWeight = 1.0, int groundLevel = -1, Random random = null)
	{
		if (random == null)
		{
			random = Game1.random;
		}
		double luckMod = 1.0;
		if (who != null)
		{
			luckMod = 1.0 + who.team.AverageDailyLuck() * dailyLuckWeight;
		}
		if (who != null && who.stats.Get(StatKeys.Mastery(0)) != 0 && random.NextDouble() < 0.001 * chanceModifier * luckMod)
		{
			Game1.createItemDebris(ItemRegistry.Create("(O)GoldenAnimalCracker"), position, -1, location, groundLevel);
		}
		if (Game1.stats.DaysPlayed > 2 && random.NextDouble() < 0.002 * chanceModifier)
		{
			Game1.createItemDebris(getRandomCosmeticItem(Game1.random), position, -1, location, groundLevel);
		}
		if (Game1.stats.DaysPlayed > 2 && random.NextDouble() < 0.0006 * chanceModifier)
		{
			Game1.createItemDebris(ItemRegistry.Create("(O)SkillBook_" + Game1.random.Next(5)), position, -1, location, groundLevel);
		}
		return false;
	}

	public static bool spawnObjectAround(Vector2 tileLocation, Object o, GameLocation l, bool playSound = true, Action<Object> modifyObject = null)
	{
		if (o == null || l == null || tileLocation.Equals(Vector2.Zero))
		{
			return false;
		}
		int attempts = 0;
		Queue<Vector2> openList = new Queue<Vector2>();
		HashSet<Vector2> closedList = new HashSet<Vector2>();
		openList.Enqueue(tileLocation);
		Vector2 current = Vector2.Zero;
		for (; attempts < 100; attempts++)
		{
			current = openList.Dequeue();
			if (l.CanItemBePlacedHere(current))
			{
				break;
			}
			closedList.Add(current);
			Vector2[] array = (from a in getAdjacentTileLocations(current)
				orderby Guid.NewGuid()
				select a).ToArray();
			foreach (Vector2 v in array)
			{
				if (!closedList.Contains(v))
				{
					openList.Enqueue(v);
				}
			}
		}
		o.isSpawnedObject.Value = true;
		o.canBeGrabbed.Value = true;
		o.TileLocation = current;
		modifyObject?.Invoke(o);
		if (!current.Equals(Vector2.Zero) && l.CanItemBePlacedHere(current))
		{
			l.objects.Add(current, o);
			if (playSound)
			{
				l.playSound("coin");
			}
			if (l.Equals(Game1.currentLocation))
			{
				l.temporarySprites.Add(new TemporaryAnimatedSprite(5, current * 64f, Color.White));
			}
			return true;
		}
		return false;
	}

	public static bool IsGeode(Item item, bool disallow_special_geodes = false)
	{
		if (item.HasTypeObject() && (!disallow_special_geodes || !item.HasContextTag("geode_crusher_ignored")))
		{
			if (!item.QualifiedItemId.Contains("MysteryBox"))
			{
				if (Game1.objectData.TryGetValue(item.ItemId, out var data))
				{
					if (!data.GeodeDropsDefaultItems)
					{
						List<ObjectGeodeDropData> geodeDrops = data.GeodeDrops;
						if (geodeDrops == null)
						{
							return false;
						}
						return geodeDrops.Count > 0;
					}
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public static Item getRandomCosmeticItem(Random r)
	{
		if (r.NextDouble() < 0.2)
		{
			if (r.NextDouble() < 0.05)
			{
				return ItemRegistry.Create("(F)1369");
			}
			Item item = null;
			switch (r.Next(3))
			{
			case 0:
				item = ItemRegistry.Create(getRandomSingleTileFurniture(r));
				break;
			case 1:
				item = ItemRegistry.Create("(F)" + r.Next(1362, 1370));
				break;
			case 2:
				item = ItemRegistry.Create("(F)" + r.Next(1376, 1391));
				break;
			}
			if (item == null || item.Name.Contains("Error"))
			{
				item = ItemRegistry.Create("(F)1369");
			}
			return item;
		}
		if (r.NextDouble() < 0.25)
		{
			List<string> hats = new List<string>
			{
				"(H)45", "(H)46", "(H)47", "(H)49", "(H)52", "(H)53", "(H)54", "(H)55", "(H)57", "(H)58",
				"(H)59", "(H)62", "(H)63", "(H)68", "(H)69", "(H)70", "(H)84", "(H)85", "(H)87", "(H)88",
				"(H)89", "(H)90"
			};
			return ItemRegistry.Create(hats[r.Next(hats.Count)]);
		}
		return ItemRegistry.Create("(S)" + getRandomIntWithExceptions(r, 1112, 1291, new List<int>
		{
			1038, 1041, 1129, 1130, 1132, 1133, 1136, 1152, 1176, 1177,
			1201, 1202, 1127
		}));
	}

	public static int getRandomIntWithExceptions(Random r, int minValue, int maxValueExclusive, List<int> exceptions)
	{
		if (r == null)
		{
			r = Game1.random;
		}
		int value = r.Next(minValue, maxValueExclusive);
		while (exceptions != null && exceptions.Contains(value))
		{
			value = r.Next(minValue, maxValueExclusive);
		}
		return value;
	}

	public static bool tryRollMysteryBox(double baseChance, Random r = null)
	{
		if (!Game1.MasterPlayer.mailReceived.Contains("sawQiPlane"))
		{
			return false;
		}
		if (r == null)
		{
			r = Game1.random;
		}
		baseChance = ((Game1.player.stats.Get("Book_Mystery") == 0) ? (baseChance * 0.66) : (baseChance * 0.88));
		return r.NextDouble() < baseChance;
	}

	public static Item getTreasureFromGeode(Item geode)
	{
		if (!IsGeode(geode))
		{
			return null;
		}
		try
		{
			string geodeId = geode.QualifiedItemId;
			Random r = CreateRandom(geodeId.Contains("MysteryBox") ? Game1.stats.Get("MysteryBoxesOpened") : Game1.stats.GeodesCracked, Game1.uniqueIDForThisGame / 2, (int)Game1.player.uniqueMultiplayerID.Value / 2);
			int prewarm_amount = r.Next(1, 10);
			for (int i = 0; i < prewarm_amount; i++)
			{
				r.NextDouble();
			}
			prewarm_amount = r.Next(1, 10);
			for (int i = 0; i < prewarm_amount; i++)
			{
				r.NextDouble();
			}
			if (geodeId.Contains("MysteryBox"))
			{
				if (Game1.stats.Get("MysteryBoxesOpened") > 10 || geodeId == "(O)GoldenMysteryBox")
				{
					double rareMod = ((!(geodeId == "(O)GoldenMysteryBox")) ? 1 : 2);
					if (geodeId == "(O)GoldenMysteryBox" && Game1.player.stats.Get(StatKeys.Mastery(0)) != 0 && r.NextBool(0.005))
					{
						return ItemRegistry.Create("(O)GoldenAnimalCracker");
					}
					if (geodeId == "(O)GoldenMysteryBox" && r.NextBool(0.005))
					{
						return ItemRegistry.Create("(BC)272");
					}
					if (r.NextBool(0.002 * rareMod))
					{
						return ItemRegistry.Create("(O)279");
					}
					if (r.NextBool(0.004 * rareMod))
					{
						return ItemRegistry.Create("(O)74");
					}
					if (r.NextBool(0.008 * rareMod))
					{
						return ItemRegistry.Create("(O)166");
					}
					if (r.NextBool(0.01 * rareMod + (Game1.player.mailReceived.Contains("GotMysteryBook") ? 0.0 : (0.0004 * (double)Game1.stats.Get("MysteryBoxesOpened")))))
					{
						if (!Game1.player.mailReceived.Contains("GotMysteryBook"))
						{
							Game1.player.mailReceived.Add("GotMysteryBook");
							return ItemRegistry.Create("(O)Book_Mystery");
						}
						return ItemRegistry.Create(r.Choose("(O)PurpleBook", "(O)Book_Mystery"));
					}
					if (r.NextBool(0.01 * rareMod))
					{
						return ItemRegistry.Create(r.Choose("(O)797", "(O)373"));
					}
					if (r.NextBool(0.01 * rareMod))
					{
						return ItemRegistry.Create("(H)MysteryHat");
					}
					if (r.NextBool(0.01 * rareMod))
					{
						return ItemRegistry.Create("(S)MysteryShirt");
					}
					if (r.NextBool(0.01 * rareMod))
					{
						return ItemRegistry.Create("(WP)MoreWalls:11");
					}
					if (r.NextBool(0.1) || geodeId == "(O)GoldenMysteryBox")
					{
						switch (r.Next(15))
						{
						case 0:
							return ItemRegistry.Create("(O)288", 5);
						case 1:
							return ItemRegistry.Create("(O)253", 3);
						case 2:
							if (Game1.player.GetUnmodifiedSkillLevel(1) >= 6 && r.NextBool())
							{
								return ItemRegistry.Create(r.Choose("(O)687", "(O)695"));
							}
							return ItemRegistry.Create("(O)242", 2);
						case 3:
							return ItemRegistry.Create("(O)204", 2);
						case 4:
							return ItemRegistry.Create("(O)369", 20);
						case 5:
							return ItemRegistry.Create("(O)466", 20);
						case 6:
							return ItemRegistry.Create("(O)773", 2);
						case 7:
							return ItemRegistry.Create("(O)688", 3);
						case 8:
							return ItemRegistry.Create("(O)" + r.Next(628, 634));
						case 9:
							return ItemRegistry.Create("(O)" + Crop.getRandomLowGradeCropForThisSeason(Game1.season), 20);
						case 10:
							if (r.NextBool())
							{
								return ItemRegistry.Create("(W)60");
							}
							return ItemRegistry.Create(r.Choose("(O)533", "(O)534"));
						case 11:
							return ItemRegistry.Create("(O)621");
						case 12:
							return ItemRegistry.Create("(O)MysteryBox", r.Next(3, 5));
						case 13:
							return ItemRegistry.Create("(O)SkillBook_" + r.Next(5));
						case 14:
							return getRaccoonSeedForCurrentTimeOfYear(Game1.player, r, 8);
						}
					}
				}
				switch (r.Next(14))
				{
				case 0:
					return ItemRegistry.Create("(O)395", 3);
				case 1:
					return ItemRegistry.Create("(O)287", 5);
				case 2:
					return ItemRegistry.Create("(O)" + Crop.getRandomLowGradeCropForThisSeason(Game1.season), 8);
				case 3:
					return ItemRegistry.Create("(O)" + r.Next(727, 734));
				case 4:
					return ItemRegistry.Create("(O)" + getRandomIntWithExceptions(r, 194, 240, new List<int> { 217 }));
				case 5:
					return ItemRegistry.Create("(O)709", 10);
				case 6:
					return ItemRegistry.Create("(O)369", 10);
				case 7:
					return ItemRegistry.Create("(O)466", 10);
				case 8:
					return ItemRegistry.Create("(O)688");
				case 9:
					return ItemRegistry.Create("(O)689");
				case 10:
					return ItemRegistry.Create("(O)770", 10);
				case 11:
					return ItemRegistry.Create("(O)MixedFlowerSeeds", 10);
				case 12:
					if (r.NextBool(0.4))
					{
						return r.Next(4) switch
						{
							0 => ItemRegistry.Create<Ring>("(O)525"), 
							1 => ItemRegistry.Create<Ring>("(O)529"), 
							2 => ItemRegistry.Create<Ring>("(O)888"), 
							_ => ItemRegistry.Create<Ring>("(O)" + r.Next(531, 533)), 
						};
					}
					return ItemRegistry.Create("(O)MysteryBox", 2);
				case 13:
					return ItemRegistry.Create("(O)690");
				default:
					return ItemRegistry.Create("(O)382");
				}
			}
			if (r.NextBool(0.1) && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
			{
				return ItemRegistry.Create("(O)890", (!r.NextBool(0.25)) ? 1 : 5);
			}
			if (Game1.objectData.TryGetValue(geode.ItemId, out var data))
			{
				List<ObjectGeodeDropData> geodeDrops = data.GeodeDrops;
				if (geodeDrops != null && geodeDrops.Count > 0 && (!data.GeodeDropsDefaultItems || r.NextBool()))
				{
					foreach (ObjectGeodeDropData drop in data.GeodeDrops.OrderBy((ObjectGeodeDropData p) => p.Precedence))
					{
						if (!r.NextBool(drop.Chance) || (drop.Condition != null && !GameStateQuery.CheckConditions(drop.Condition, null, null, null, null, r)))
						{
							continue;
						}
						Item item = ItemQueryResolver.TryResolveRandomItem(drop, new ItemQueryContext(null, null, r), avoidRepeat: false, null, null, null, delegate(string query, string error)
						{
							Game1.log.Error($"Geode item '{geode.QualifiedItemId}' failed parsing item query '{query}' for {"GeodeDrops"} entry '{drop.Id}': {error}");
						});
						if (item != null)
						{
							if (drop.SetFlagOnPickup != null)
							{
								item.SetFlagOnPickup = drop.SetFlagOnPickup;
							}
							return item;
						}
					}
				}
			}
			int amount = r.Next(3) * 2 + 1;
			if (r.NextBool(0.1))
			{
				amount = 10;
			}
			if (r.NextBool(0.01))
			{
				amount = 20;
			}
			if (r.NextBool())
			{
				switch (r.Next(4))
				{
				case 0:
				case 1:
					return ItemRegistry.Create("(O)390", amount);
				case 2:
					return ItemRegistry.Create("(O)330");
				default:
					return geodeId switch
					{
						"(O)749" => ItemRegistry.Create("(O)" + (82 + r.Next(3) * 2)), 
						"(O)535" => ItemRegistry.Create("(O)86"), 
						"(O)536" => ItemRegistry.Create("(O)84"), 
						_ => ItemRegistry.Create("(O)82"), 
					};
				}
			}
			if (!(geodeId == "(O)535"))
			{
				if (geodeId == "(O)536")
				{
					return r.Next(4) switch
					{
						0 => ItemRegistry.Create("(O)378", amount), 
						1 => ItemRegistry.Create("(O)380", amount), 
						2 => ItemRegistry.Create("(O)382", amount), 
						_ => ItemRegistry.Create((Game1.player.deepestMineLevel > 75) ? "(O)384" : "(O)380", amount), 
					};
				}
				return r.Next(5) switch
				{
					0 => ItemRegistry.Create("(O)378", amount), 
					1 => ItemRegistry.Create("(O)380", amount), 
					2 => ItemRegistry.Create("(O)382", amount), 
					3 => ItemRegistry.Create("(O)384", amount), 
					_ => ItemRegistry.Create("(O)386", amount / 2 + 1), 
				};
			}
			return r.Next(3) switch
			{
				0 => ItemRegistry.Create("(O)378", amount), 
				1 => ItemRegistry.Create((Game1.player.deepestMineLevel > 25) ? "(O)380" : "(O)378", amount), 
				_ => ItemRegistry.Create("(O)382", amount), 
			};
		}
		catch (Exception e)
		{
			Game1.log.Error("Geode '" + geode?.QualifiedItemId + "' failed creating treasure.", e);
		}
		return ItemRegistry.Create("(O)390");
	}

	public static Vector2 snapToInt(Vector2 v)
	{
		v.X = (int)v.X;
		v.Y = (int)v.Y;
		return v;
	}

	public static Vector2 GetNearbyValidPlacementPosition(Farmer who, GameLocation location, Item item, int x, int y)
	{
		if (!Game1.isCheckingNonMousePlacement)
		{
			return new Vector2(x, y);
		}
		int item_width = 1;
		int item_length = 1;
		Point direction = default(Point);
		Microsoft.Xna.Framework.Rectangle bounding_box = new Microsoft.Xna.Framework.Rectangle(0, 0, item_width * 64, item_length * 64);
		if (item is Furniture furniture)
		{
			item_width = furniture.getTilesWide();
			item_length = furniture.getTilesHigh();
			bounding_box.Width = furniture.boundingBox.Value.Width;
			bounding_box.Height = furniture.boundingBox.Value.Height;
		}
		switch (who.FacingDirection)
		{
		case 0:
			direction.X = 0;
			direction.Y = -1;
			y -= (item_length - 1) * 64;
			break;
		case 2:
			direction.X = 0;
			direction.Y = 1;
			break;
		case 3:
			direction.X = -1;
			direction.Y = 0;
			x -= (item_width - 1) * 64;
			break;
		case 1:
			direction.X = 1;
			direction.Y = 0;
			break;
		}
		int scan_distance = 2;
		if (item is Object obj && obj.isPassable() && (obj.Category == -74 || obj.isSapling() || obj.Category == -19))
		{
			x = (int)who.GetToolLocation().X / 64 * 64;
			y = (int)who.GetToolLocation().Y / 64 * 64;
			direction.X = who.TilePoint.X - x / 64;
			direction.Y = who.TilePoint.Y - y / 64;
			int magnitude = (int)Math.Sqrt(Math.Pow(direction.X, 2.0) + Math.Pow(direction.Y, 2.0));
			if (magnitude > 0)
			{
				direction.X /= magnitude;
				direction.Y /= magnitude;
			}
			scan_distance = magnitude + 1;
		}
		bool is_passable = (item as Object)?.isPassable() ?? false;
		x = x / 64 * 64;
		y = y / 64 * 64;
		Microsoft.Xna.Framework.Rectangle playerBounds = who.GetBoundingBox();
		for (int offset = 0; offset < scan_distance; offset++)
		{
			int checked_x = x + direction.X * offset * 64;
			int checked_y = y + direction.Y * offset * 64;
			bounding_box.X = checked_x;
			bounding_box.Y = checked_y;
			if ((!playerBounds.Intersects(bounding_box) && !is_passable) || playerCanPlaceItemHere(location, item, checked_x, checked_y, who))
			{
				return new Vector2(checked_x, checked_y);
			}
		}
		return new Vector2(x, y);
	}

	public static bool tryToPlaceItem(GameLocation location, Item item, int x, int y)
	{
		if (item == null || item is Tool)
		{
			return false;
		}
		Vector2 tileLocation = new Vector2(x / 64, y / 64);
		if (playerCanPlaceItemHere(location, item, x, y, Game1.player))
		{
			if (item is Furniture)
			{
				Game1.player.ActiveObject = null;
			}
			if (((Object)item).placementAction(location, x, y, Game1.player))
			{
				Game1.player.reduceActiveItemByOne();
			}
			else if (item is Furniture furniture)
			{
				Game1.player.ActiveObject = furniture;
			}
			else if (item is Wallpaper)
			{
				return false;
			}
			return true;
		}
		if (isPlacementForbiddenHere(location) && item != null && item.isPlaceable())
		{
			if (Game1.didPlayerJustClickAtAll(ignoreNonMouseHeldInput: true))
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Object.cs.13053"));
			}
		}
		else if (item is Furniture furniture && Game1.didPlayerJustLeftClick(ignoreNonMouseHeldInput: true))
		{
			switch (furniture.GetAdditionalFurniturePlacementStatus(location, x, y, Game1.player))
			{
			case 1:
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12629"));
				break;
			case 2:
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12632"));
				break;
			case 3:
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12633"));
				break;
			case 4:
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Furniture.cs.12632"));
				break;
			}
		}
		if (item.Category == -19 && location.terrainFeatures.TryGetValue(tileLocation, out var terrainFeature) && terrainFeature is HoeDirt dirt)
		{
			switch (dirt.CheckApplyFertilizerRules(item.QualifiedItemId))
			{
			case HoeDirtFertilizerApplyStatus.HasThisFertilizer:
				return false;
			case HoeDirtFertilizerApplyStatus.HasAnotherFertilizer:
				if (Game1.didPlayerJustClickAtAll(ignoreNonMouseHeldInput: true))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916-2"));
				}
				return false;
			case HoeDirtFertilizerApplyStatus.CropAlreadySprouted:
				if (Game1.didPlayerJustClickAtAll(ignoreNonMouseHeldInput: true))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13916"));
				}
				return false;
			}
		}
		playerCanPlaceItemHere(location, item, x, y, Game1.player, show_error: true);
		return false;
	}

	public static bool pointInRectangles(List<Microsoft.Xna.Framework.Rectangle> rectangles, int x, int y)
	{
		foreach (Microsoft.Xna.Framework.Rectangle rectangle in rectangles)
		{
			if (rectangle.Contains(x, y))
			{
				return true;
			}
		}
		return false;
	}

	public static Keys mapGamePadButtonToKey(Buttons b)
	{
		return b switch
		{
			Buttons.A => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.actionButton), 
			Buttons.X => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.useToolButton), 
			Buttons.B => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.menuButton), 
			Buttons.Back => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.journalButton), 
			Buttons.Start => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.menuButton), 
			Buttons.Y => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.menuButton), 
			Buttons.DPadUp => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveUpButton), 
			Buttons.DPadRight => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveRightButton), 
			Buttons.DPadDown => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveDownButton), 
			Buttons.DPadLeft => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveLeftButton), 
			Buttons.LeftThumbstickUp => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveUpButton), 
			Buttons.LeftThumbstickRight => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveRightButton), 
			Buttons.LeftThumbstickDown => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveDownButton), 
			Buttons.LeftThumbstickLeft => Game1.options.getFirstKeyboardKeyFromInputButtonList(Game1.options.moveLeftButton), 
			_ => Keys.None, 
		};
	}

	public static ButtonCollection getPressedButtons(GamePadState padState, GamePadState oldPadState)
	{
		return new ButtonCollection(ref padState, ref oldPadState);
	}

	public static bool thumbstickIsInDirection(int direction, GamePadState padState)
	{
		if (Game1.currentMinigame != null)
		{
			return true;
		}
		return direction switch
		{
			0 => Math.Abs(padState.ThumbSticks.Left.X) < padState.ThumbSticks.Left.Y, 
			1 => padState.ThumbSticks.Left.X > Math.Abs(padState.ThumbSticks.Left.Y), 
			2 => Math.Abs(padState.ThumbSticks.Left.X) < Math.Abs(padState.ThumbSticks.Left.Y), 
			3 => Math.Abs(padState.ThumbSticks.Left.X) > Math.Abs(padState.ThumbSticks.Left.Y), 
			_ => false, 
		};
	}

	public static ButtonCollection getHeldButtons(GamePadState padState)
	{
		return new ButtonCollection(ref padState);
	}

	/// <summary>
	/// return true if music becomes muted
	/// </summary>
	/// <returns></returns>
	public static bool toggleMuteMusic()
	{
		if (Game1.options.musicVolumeLevel != 0f)
		{
			disableMusic();
			return true;
		}
		enableMusic();
		return false;
	}

	public static void enableMusic()
	{
		Game1.options.musicVolumeLevel = 0.75f;
		Game1.musicCategory.SetVolume(0.75f);
		Game1.musicPlayerVolume = 0.75f;
		Game1.options.ambientVolumeLevel = 0.75f;
		Game1.ambientCategory.SetVolume(0.75f);
		Game1.ambientPlayerVolume = 0.75f;
	}

	public static void disableMusic()
	{
		Game1.options.musicVolumeLevel = 0f;
		Game1.musicCategory.SetVolume(0f);
		Game1.options.ambientVolumeLevel = 0f;
		Game1.ambientCategory.SetVolume(0f);
		Game1.ambientPlayerVolume = 0f;
		Game1.musicPlayerVolume = 0f;
	}

	public static Vector2 getVelocityTowardPlayer(Point startingPoint, float speed, Farmer f)
	{
		Microsoft.Xna.Framework.Rectangle playerBounds = f.GetBoundingBox();
		return getVelocityTowardPoint(startingPoint, new Vector2(playerBounds.X, playerBounds.Y), speed);
	}

	/// <summary>Get a timestamp with hours and minutes from a milliseconds count, like <c>27:46</c> for 100,000,000 milliseconds.</summary>
	/// <param name="milliseconds">The number of milliseconds.</param>
	public static string getHoursMinutesStringFromMilliseconds(ulong milliseconds)
	{
		return milliseconds / 3600000 + ":" + ((milliseconds % 3600000 / 60000 < 10) ? "0" : "") + milliseconds % 3600000 / 60000;
	}

	/// <summary>Get a timestamp with minutes and seconds from a milliseconds count, like <c>1:40</c> for 100,000 milliseconds.</summary>
	/// <param name="milliseconds">The number of milliseconds.</param>
	public static string getMinutesSecondsStringFromMilliseconds(int milliseconds)
	{
		return milliseconds / 60000 + ":" + ((milliseconds % 60000 / 1000 < 10) ? "0" : "") + milliseconds % 60000 / 1000;
	}

	public static Vector2 getVelocityTowardPoint(Vector2 startingPoint, Vector2 endingPoint, float speed)
	{
		double xDif = endingPoint.X - startingPoint.X;
		double yDif = endingPoint.Y - startingPoint.Y;
		if (Math.Abs(xDif) < 0.1 && Math.Abs(yDif) < 0.1)
		{
			return new Vector2(0f, 0f);
		}
		double total = Math.Sqrt(Math.Pow(xDif, 2.0) + Math.Pow(yDif, 2.0));
		xDif /= total;
		yDif /= total;
		return new Vector2((float)(xDif * (double)speed), (float)(yDif * (double)speed));
	}

	public static Vector2 getVelocityTowardPoint(Point startingPoint, Vector2 endingPoint, float speed)
	{
		return getVelocityTowardPoint(new Vector2(startingPoint.X, startingPoint.Y), endingPoint, speed);
	}

	public static Vector2 getRandomPositionInThisRectangle(Microsoft.Xna.Framework.Rectangle r, Random random)
	{
		return new Vector2(random.Next(r.X, r.X + r.Width), random.Next(r.Y, r.Y + r.Height));
	}

	public static Vector2 getTopLeftPositionForCenteringOnScreen(xTile.Dimensions.Rectangle viewport, int width, int height, int xOffset = 0, int yOffset = 0)
	{
		return new Vector2(viewport.Width / 2 - width / 2 + xOffset, viewport.Height / 2 - height / 2 + yOffset);
	}

	public static Vector2 getTopLeftPositionForCenteringOnScreen(int width, int height, int xOffset = 0, int yOffset = 0)
	{
		return getTopLeftPositionForCenteringOnScreen(Game1.uiViewport, width, height, xOffset, yOffset);
	}

	public static void recursiveFindPositionForCharacter(NPC c, GameLocation l, Vector2 tileLocation, int maxIterations)
	{
		int iterations = 0;
		Queue<Vector2> positionsToCheck = new Queue<Vector2>();
		positionsToCheck.Enqueue(tileLocation);
		List<Vector2> closedList = new List<Vector2>();
		Microsoft.Xna.Framework.Rectangle boundsSize = c.GetBoundingBox();
		for (; iterations < maxIterations; iterations++)
		{
			if (positionsToCheck.Count <= 0)
			{
				break;
			}
			Vector2 currentPoint = positionsToCheck.Dequeue();
			closedList.Add(currentPoint);
			c.Position = new Vector2(currentPoint.X * 64f + 32f - (float)(boundsSize.Width / 2), currentPoint.Y * 64f - (float)boundsSize.Height);
			if (!l.isCollidingPosition(c.GetBoundingBox(), Game1.viewport, isFarmer: false, 0, glider: false, c, pathfinding: true))
			{
				if (!l.characters.Contains(c))
				{
					l.characters.Add(c);
					c.currentLocation = l;
				}
				break;
			}
			Vector2[] directionsTileVectors = DirectionsTileVectors;
			foreach (Vector2 v in directionsTileVectors)
			{
				if (!closedList.Contains(currentPoint + v))
				{
					positionsToCheck.Enqueue(currentPoint + v);
				}
			}
		}
	}

	public static Pet findPet(Guid guid)
	{
		foreach (NPC character in Game1.getFarm().characters)
		{
			if (character is Pet pet && pet.petId.Value.Equals(guid))
			{
				return pet;
			}
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			foreach (NPC character2 in getHomeOfFarmer(allFarmer).characters)
			{
				if (character2 is Pet pet && pet.petId.Value.Equals(guid))
				{
					return pet;
				}
			}
		}
		return null;
	}

	public static Vector2 recursiveFindOpenTileForCharacter(Character c, GameLocation l, Vector2 tileLocation, int maxIterations, bool allowOffMap = true)
	{
		int iterations = 0;
		Queue<Vector2> positionsToCheck = new Queue<Vector2>();
		positionsToCheck.Enqueue(tileLocation);
		List<Vector2> closedList = new List<Vector2>();
		Vector2 originalPosition = c.Position;
		int width = c.GetBoundingBox().Width;
		for (; iterations < maxIterations; iterations++)
		{
			if (positionsToCheck.Count <= 0)
			{
				break;
			}
			Vector2 currentPoint = positionsToCheck.Dequeue();
			closedList.Add(currentPoint);
			c.Position = new Vector2(currentPoint.X * 64f + 32f - (float)(width / 2), currentPoint.Y * 64f + 4f);
			Microsoft.Xna.Framework.Rectangle boundingBox = c.GetBoundingBox();
			c.Position = originalPosition;
			if (!l.isCollidingPosition(boundingBox, Game1.viewport, c is Farmer, 0, glider: false, c, pathfinding: false, projectile: false, ignoreCharacterRequirement: false, skipCollisionEffects: true) && (allowOffMap || l.isTileOnMap(currentPoint)))
			{
				return currentPoint;
			}
			Vector2[] directionsTileVectors = DirectionsTileVectors;
			for (int i = 0; i < directionsTileVectors.Length; i++)
			{
				Vector2 v = directionsTileVectors[i];
				if (!closedList.Contains(currentPoint + v) && l.isTilePlaceable(currentPoint + v) && (!(l is DecoratableLocation) || !(l as DecoratableLocation).isTileOnWall((int)(v.X + currentPoint.X), (int)(v.Y + currentPoint.Y))))
				{
					positionsToCheck.Enqueue(currentPoint + v);
				}
			}
		}
		return Vector2.Zero;
	}

	public static List<Vector2> recursiveFindOpenTiles(GameLocation l, Vector2 tileLocation, int maxOpenTilesToFind = 24, int maxIterations = 50)
	{
		int iterations = 0;
		Queue<Vector2> positionsToCheck = new Queue<Vector2>();
		positionsToCheck.Enqueue(tileLocation);
		List<Vector2> closedList = new List<Vector2>();
		List<Vector2> successList = new List<Vector2>();
		for (; iterations < maxIterations; iterations++)
		{
			if (positionsToCheck.Count <= 0)
			{
				break;
			}
			if (successList.Count >= maxOpenTilesToFind)
			{
				break;
			}
			Vector2 currentPoint = positionsToCheck.Dequeue();
			closedList.Add(currentPoint);
			if (l.CanItemBePlacedHere(currentPoint))
			{
				successList.Add(currentPoint);
			}
			Vector2[] directionsTileVectors = DirectionsTileVectors;
			foreach (Vector2 v in directionsTileVectors)
			{
				if (!closedList.Contains(currentPoint + v))
				{
					positionsToCheck.Enqueue(currentPoint + v);
				}
			}
		}
		return successList;
	}

	public static void spreadAnimalsAround(Building b, GameLocation environment)
	{
		try
		{
			GameLocation indoors = b.GetIndoors();
			if (indoors != null)
			{
				spreadAnimalsAround(b, environment, indoors.animals.Values);
			}
		}
		catch (Exception)
		{
		}
	}

	public static void spreadAnimalsAround(Building b, GameLocation environment, IEnumerable<FarmAnimal> animalsList)
	{
		if (!b.HasIndoors())
		{
			return;
		}
		Queue<FarmAnimal> animals = new Queue<FarmAnimal>(animalsList);
		int iterations = 0;
		Queue<Vector2> positionsToCheck = new Queue<Vector2>();
		positionsToCheck.Enqueue(new Vector2((int)b.tileX + b.animalDoor.X, (int)b.tileY + b.animalDoor.Y + 1));
		while (animals.Count > 0 && iterations < 40 && positionsToCheck.Count > 0)
		{
			Vector2 currentPoint = positionsToCheck.Dequeue();
			FarmAnimal animal = animals.Peek();
			Microsoft.Xna.Framework.Rectangle boundsSize = animal.GetBoundingBox();
			animal.Position = new Vector2(currentPoint.X * 64f + 32f - (float)(boundsSize.Width / 2), currentPoint.Y * 64f - 32f - (float)(boundsSize.Height / 2));
			if (!environment.isCollidingPosition(animal.GetBoundingBox(), Game1.viewport, isFarmer: false, 0, glider: false, animal, pathfinding: true))
			{
				environment.animals.Add(animal.myID.Value, animal);
				animals.Dequeue();
			}
			if (animals.Count > 0)
			{
				animal = animals.Peek();
				boundsSize = animal.GetBoundingBox();
				Vector2[] directionsTileVectors = DirectionsTileVectors;
				for (int i = 0; i < directionsTileVectors.Length; i++)
				{
					Vector2 v = directionsTileVectors[i];
					animal.Position = new Vector2((currentPoint.X + v.X) * 64f + 32f - (float)(boundsSize.Width / 2), (currentPoint.Y + v.Y) * 64f - 32f - (float)(boundsSize.Height / 2));
					if (!environment.isCollidingPosition(animal.GetBoundingBox(), Game1.viewport, isFarmer: false, 0, glider: false, animal, pathfinding: true))
					{
						positionsToCheck.Enqueue(currentPoint + v);
					}
				}
			}
			iterations++;
		}
	}

	public static Point findTile(GameLocation location, int tileIndex, string layerId)
	{
		Layer layer = location.map.RequireLayer(layerId);
		for (int y = 0; y < layer.LayerHeight; y++)
		{
			for (int x = 0; x < layer.LayerWidth; x++)
			{
				if (location.getTileIndexAt(x, y, layerId) == tileIndex)
				{
					return new Point(x, y);
				}
			}
		}
		return new Point(-1, -1);
	}

	public static bool[] horizontalOrVerticalCollisionDirections(Microsoft.Xna.Framework.Rectangle boundingBox, Character c, bool projectile = false)
	{
		bool[] directions = new bool[2];
		Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(boundingBox.X, boundingBox.Y, boundingBox.Width, boundingBox.Height);
		rect.Width = 1;
		rect.X = boundingBox.Center.X;
		if (c != null)
		{
			if (Game1.currentLocation.isCollidingPosition(rect, Game1.viewport, isFarmer: false, -1, projectile, c, pathfinding: false, projectile))
			{
				directions[1] = true;
			}
		}
		else if (Game1.currentLocation.isCollidingPosition(rect, Game1.viewport, isFarmer: false, -1, projectile, c, pathfinding: false, projectile))
		{
			directions[1] = true;
		}
		rect.Width = boundingBox.Width;
		rect.X = boundingBox.X;
		rect.Height = 1;
		rect.Y = boundingBox.Center.Y;
		if (c != null)
		{
			if (Game1.currentLocation.isCollidingPosition(rect, Game1.viewport, isFarmer: false, -1, projectile, c, pathfinding: false, projectile))
			{
				directions[0] = true;
			}
		}
		else if (Game1.currentLocation.isCollidingPosition(rect, Game1.viewport, isFarmer: false, -1, projectile, c, pathfinding: false, projectile))
		{
			directions[0] = true;
		}
		return directions;
	}

	public static Color getBlendedColor(Color c1, Color c2)
	{
		return new Color(Game1.random.NextBool() ? Math.Max(c1.R, c2.R) : ((c1.R + c2.R) / 2), Game1.random.NextBool() ? Math.Max(c1.G, c2.G) : ((c1.G + c2.G) / 2), Game1.random.NextBool() ? Math.Max(c1.B, c2.B) : ((c1.B + c2.B) / 2));
	}

	public static Character checkForCharacterWithinArea(Type kindOfCharacter, Vector2 positionToAvoid, GameLocation location, Microsoft.Xna.Framework.Rectangle area)
	{
		foreach (NPC n in location.characters)
		{
			if (n.GetType().Equals(kindOfCharacter) && n.GetBoundingBox().Intersects(area) && !n.Position.Equals(positionToAvoid))
			{
				return n;
			}
		}
		return null;
	}

	public static int getNumberOfCharactersInRadius(GameLocation l, Point position, int tileRadius)
	{
		Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(position.X - tileRadius * 64, position.Y - tileRadius * 64, (tileRadius * 2 + 1) * 64, (tileRadius * 2 + 1) * 64);
		int count = 0;
		foreach (NPC n in l.characters)
		{
			if (rect.Contains(Vector2ToPoint(n.Position)))
			{
				count++;
			}
		}
		return count;
	}

	public static List<Vector2> getListOfTileLocationsForBordersOfNonTileRectangle(Microsoft.Xna.Framework.Rectangle rectangle)
	{
		return new List<Vector2>
		{
			new Vector2(rectangle.Left / 64, rectangle.Top / 64),
			new Vector2(rectangle.Right / 64, rectangle.Top / 64),
			new Vector2(rectangle.Left / 64, rectangle.Bottom / 64),
			new Vector2(rectangle.Right / 64, rectangle.Bottom / 64),
			new Vector2(rectangle.Left / 64, rectangle.Center.Y / 64),
			new Vector2(rectangle.Right / 64, rectangle.Center.Y / 64),
			new Vector2(rectangle.Center.X / 64, rectangle.Bottom / 64),
			new Vector2(rectangle.Center.X / 64, rectangle.Top / 64),
			new Vector2(rectangle.Center.X / 64, rectangle.Center.Y / 64)
		};
	}

	public static void makeTemporarySpriteJuicier(TemporaryAnimatedSprite t, GameLocation l, int numAddOns = 4, int xRange = 64, int yRange = 64)
	{
		t.position.Y -= 8f;
		l.temporarySprites.Add(t);
		for (int i = 0; i < numAddOns; i++)
		{
			TemporaryAnimatedSprite clone = t.getClone();
			clone.delayBeforeAnimationStart = i * 100;
			clone.position += new Vector2(Game1.random.Next(-xRange / 2, xRange / 2 + 1), Game1.random.Next(-yRange / 2, yRange / 2 + 1));
			clone.layerDepth += 1E-06f;
			l.temporarySprites.Add(clone);
		}
	}

	public static void recursiveObjectPlacement(Object o, int tileX, int tileY, double growthRate, double decay, GameLocation location, string terrainToExclude = "", int objectIndexAddRange = 0, double failChance = 0.0, int objectIndeAddRangeMultiplier = 1, List<string> itemIDVariations = null)
	{
		if (o == null)
		{
			return;
		}
		if (!int.TryParse(o.ItemId, out var parsedIndex))
		{
			parsedIndex = -1;
		}
		if (!location.isTileLocationOpen(new Location(tileX, tileY)) || location.IsTileOccupiedBy(new Vector2(tileX, tileY)) || location.getTileIndexAt(tileX, tileY, "Back") == -1 || (!terrainToExclude.Equals("") && (location.doesTileHaveProperty(tileX, tileY, "Type", "Back") == null || location.doesTileHaveProperty(tileX, tileY, "Type", "Back").Equals(terrainToExclude))))
		{
			return;
		}
		Vector2 objectPos = new Vector2(tileX, tileY);
		if (!Game1.random.NextBool(failChance * 2.0))
		{
			string itemId = o.ItemId;
			if (parsedIndex >= 0)
			{
				itemId = (parsedIndex + Game1.random.Next(objectIndexAddRange + 1) * objectIndeAddRangeMultiplier).ToString();
			}
			if (o is ColoredObject coloredObj)
			{
				location.objects.Add(objectPos, new ColoredObject(itemId, 1, coloredObj.color.Value)
				{
					Fragility = o.fragility,
					MinutesUntilReady = o.MinutesUntilReady,
					Name = o.name,
					CanBeSetDown = o.CanBeSetDown,
					CanBeGrabbed = o.CanBeGrabbed,
					IsSpawnedObject = o.IsSpawnedObject,
					TileLocation = objectPos,
					ColorSameIndexAsParentSheetIndex = coloredObj.ColorSameIndexAsParentSheetIndex
				});
			}
			else
			{
				location.objects.Add(objectPos, new Object(itemId, 1)
				{
					Fragility = o.fragility,
					MinutesUntilReady = o.MinutesUntilReady,
					CanBeSetDown = o.canBeSetDown,
					CanBeGrabbed = o.canBeGrabbed,
					IsSpawnedObject = o.isSpawnedObject
				});
			}
		}
		growthRate -= decay;
		if (Game1.random.NextDouble() < growthRate)
		{
			recursiveObjectPlacement(o, tileX + 1, tileY, growthRate, decay, location, terrainToExclude, objectIndexAddRange, failChance, objectIndeAddRangeMultiplier, itemIDVariations);
		}
		if (Game1.random.NextDouble() < growthRate)
		{
			recursiveObjectPlacement(o, tileX - 1, tileY, growthRate, decay, location, terrainToExclude, objectIndexAddRange, failChance, objectIndeAddRangeMultiplier, itemIDVariations);
		}
		if (Game1.random.NextDouble() < growthRate)
		{
			recursiveObjectPlacement(o, tileX, tileY + 1, growthRate, decay, location, terrainToExclude, objectIndexAddRange, failChance, objectIndeAddRangeMultiplier, itemIDVariations);
		}
		if (Game1.random.NextDouble() < growthRate)
		{
			recursiveObjectPlacement(o, tileX, tileY - 1, growthRate, decay, location, terrainToExclude, objectIndexAddRange, failChance, objectIndeAddRangeMultiplier, itemIDVariations);
		}
	}

	public static void recursiveFarmGrassPlacement(int tileX, int tileY, double growthRate, double decay, GameLocation farm)
	{
		if (farm.isTileLocationOpen(new Location(tileX, tileY)) && !farm.IsTileOccupiedBy(new Vector2(tileX, tileY)) && farm.doesTileHaveProperty(tileX, tileY, "Diggable", "Back") != null)
		{
			Vector2 objectPos = new Vector2(tileX, tileY);
			if (Game1.random.NextDouble() < 0.05)
			{
				farm.objects.Add(new Vector2(tileX, tileY), ItemRegistry.Create<Object>(Game1.random.Choose("(O)674", "(O)675")));
			}
			else
			{
				farm.terrainFeatures.Add(objectPos, new Grass(1, 4 - (int)((1.0 - growthRate) * 4.0)));
			}
			growthRate -= decay;
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveFarmGrassPlacement(tileX + 1, tileY, growthRate, decay, farm);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveFarmGrassPlacement(tileX - 1, tileY, growthRate, decay, farm);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveFarmGrassPlacement(tileX, tileY + 1, growthRate, decay, farm);
			}
			if (Game1.random.NextDouble() < growthRate)
			{
				recursiveFarmGrassPlacement(tileX, tileY - 1, growthRate, decay, farm);
			}
		}
	}

	public static void recursiveTreePlacement(int tileX, int tileY, double growthRate, int growthStage, double skipChance, GameLocation l, Microsoft.Xna.Framework.Rectangle clearPatch, bool sparse)
	{
		if (clearPatch.Contains(tileX, tileY))
		{
			return;
		}
		Vector2 location = new Vector2(tileX, tileY);
		if (l.doesTileHaveProperty((int)location.X, (int)location.Y, "Diggable", "Back") == null || l.IsNoSpawnTile(location) || !l.isTileLocationOpen(new Location((int)location.X, (int)location.Y)) || l.IsTileOccupiedBy(location) || (sparse && (l.IsTileOccupiedBy(new Vector2(tileX, tileY + -1)) || l.IsTileOccupiedBy(new Vector2(tileX, tileY + 1)) || l.IsTileOccupiedBy(new Vector2(tileX + 1, tileY)) || l.IsTileOccupiedBy(new Vector2(tileX + -1, tileY)) || l.IsTileOccupiedBy(new Vector2(tileX + 1, tileY + 1)))))
		{
			return;
		}
		if (!Game1.random.NextBool(skipChance))
		{
			if (sparse && location.X < 70f && (location.X < 48f || location.Y > 26f) && Game1.random.NextDouble() < 0.07)
			{
				(l as Farm).resourceClumps.Add(new ResourceClump(Game1.random.Choose(672, 600, 602), 2, 2, location));
			}
			else
			{
				l.terrainFeatures.Add(location, new Tree(Game1.random.Next(1, 4).ToString(), (growthStage < 5) ? Game1.random.Next(5) : 5));
			}
			growthRate -= 0.05;
		}
		if (Game1.random.NextDouble() < growthRate)
		{
			recursiveTreePlacement(tileX + Game1.random.Next(1, 3), tileY, growthRate, growthStage, skipChance, l, clearPatch, sparse);
		}
		if (Game1.random.NextDouble() < growthRate)
		{
			recursiveTreePlacement(tileX - Game1.random.Next(1, 3), tileY, growthRate, growthStage, skipChance, l, clearPatch, sparse);
		}
		if (Game1.random.NextDouble() < growthRate)
		{
			recursiveTreePlacement(tileX, tileY + Game1.random.Next(1, 3), growthRate, growthStage, skipChance, l, clearPatch, sparse);
		}
		if (Game1.random.NextDouble() < growthRate)
		{
			recursiveTreePlacement(tileX, tileY - Game1.random.Next(1, 3), growthRate, growthStage, skipChance, l, clearPatch, sparse);
		}
	}

	public static void recursiveRemoveTerrainFeatures(int tileX, int tileY, double growthRate, double decay, GameLocation l)
	{
		Vector2 location = new Vector2(tileX, tileY);
		l.terrainFeatures.Remove(location);
		growthRate -= decay;
		if (Game1.random.NextDouble() < growthRate)
		{
			recursiveRemoveTerrainFeatures(tileX + 1, tileY, growthRate, decay, l);
		}
		if (Game1.random.NextDouble() < growthRate)
		{
			recursiveRemoveTerrainFeatures(tileX - 1, tileY, growthRate, decay, l);
		}
		if (Game1.random.NextDouble() < growthRate)
		{
			recursiveRemoveTerrainFeatures(tileX, tileY + 1, growthRate, decay, l);
		}
		if (Game1.random.NextDouble() < growthRate)
		{
			recursiveRemoveTerrainFeatures(tileX, tileY - 1, growthRate, decay, l);
		}
	}

	public static IEnumerator<int> generateNewFarm(bool skipFarmGeneration)
	{
		return generateNewFarm(skipFarmGeneration, loadForNewGame: true);
	}

	public static IEnumerator<int> generateNewFarm(bool skipFarmGeneration, bool loadForNewGame)
	{
		Game1.fadeToBlack = false;
		Game1.fadeToBlackAlpha = 1f;
		Game1.debrisWeather.Clear();
		Game1.viewport.X = -9999;
		Game1.changeMusicTrack("none");
		if (loadForNewGame)
		{
			Game1.game1.loadForNewGame();
		}
		Game1.currentLocation = Game1.RequireLocation("Farmhouse");
		Game1.currentLocation.currentEvent = new Event("none/-600 -600/farmer 4 8 2/warp farmer 4 8/end beginGame");
		Game1.gameMode = 2;
		yield return 100;
	}

	/// <summary>Get the pixel distance between a position in the world and the player's screen viewport, where 0 is within the viewport.</summary>
	/// <param name="pixelPosition">The pixel position.</param>
	public static float distanceFromScreen(Vector2 pixelPosition)
	{
		float x = pixelPosition.X - (float)Game1.viewport.X;
		float y = pixelPosition.Y - (float)Game1.viewport.Y;
		float x2 = MathHelper.Clamp(x, 0f, Game1.viewport.Width - 1);
		float screenY = MathHelper.Clamp(y, 0f, Game1.viewport.Height - 1);
		return distance(x2, x, screenY, y);
	}

	/// <summary>Get whether a pixel position is within the current player's screen viewport.</summary>
	/// <param name="positionNonTile">The pixel position.</param>
	/// <param name="acceptableDistanceFromScreen">The maximum pixel distance outside the screen viewport to allow.</param>
	public static bool isOnScreen(Vector2 positionNonTile, int acceptableDistanceFromScreen)
	{
		positionNonTile.X -= Game1.viewport.X;
		positionNonTile.Y -= Game1.viewport.Y;
		if (positionNonTile.X > (float)(-acceptableDistanceFromScreen) && positionNonTile.X < (float)(Game1.viewport.Width + acceptableDistanceFromScreen) && positionNonTile.Y > (float)(-acceptableDistanceFromScreen))
		{
			return positionNonTile.Y < (float)(Game1.viewport.Height + acceptableDistanceFromScreen);
		}
		return false;
	}

	/// <summary>Get whether a tile position is within the current player's screen viewport.</summary>
	/// <param name="positionTile">The tile position.</param>
	/// <param name="acceptableDistanceFromScreenNonTile">The maximum tile distance outside the screen viewport to allow.</param>
	/// <param name="location">The location whose position to check.</param>
	public static bool isOnScreen(Point positionTile, int acceptableDistanceFromScreenNonTile, GameLocation location = null)
	{
		if (location != null && !location.Equals(Game1.currentLocation))
		{
			return false;
		}
		if (positionTile.X * 64 > Game1.viewport.X - acceptableDistanceFromScreenNonTile && positionTile.X * 64 < Game1.viewport.X + Game1.viewport.Width + acceptableDistanceFromScreenNonTile && positionTile.Y * 64 > Game1.viewport.Y - acceptableDistanceFromScreenNonTile)
		{
			return positionTile.Y * 64 < Game1.viewport.Y + Game1.viewport.Height + acceptableDistanceFromScreenNonTile;
		}
		return false;
	}

	public static void clearObjectsInArea(Microsoft.Xna.Framework.Rectangle r, GameLocation l)
	{
		for (int x = r.Left; x < r.Right; x += 64)
		{
			for (int y = r.Top; y < r.Bottom; y += 64)
			{
				l.removeEverythingFromThisTile(x / 64, y / 64);
			}
		}
	}

	public static void trashItem(Item item)
	{
		if (item is Object && Game1.player.specialItems.Contains(item.ItemId))
		{
			Game1.player.specialItems.Remove(item.ItemId);
		}
		if (getTrashReclamationPrice(item, Game1.player) > 0)
		{
			Game1.player.Money += getTrashReclamationPrice(item, Game1.player);
		}
		Game1.playSound("trashcan");
	}

	public static FarmAnimal GetBestHarvestableFarmAnimal(IEnumerable<FarmAnimal> animals, Tool tool, Microsoft.Xna.Framework.Rectangle toolRect)
	{
		FarmAnimal fallbackAnimal = null;
		foreach (FarmAnimal animal in animals)
		{
			if (animal.GetHarvestBoundingBox().Intersects(toolRect))
			{
				if (animal.CanGetProduceWithTool(tool) && animal.currentProduce.Value != null && animal.isAdult())
				{
					return animal;
				}
				fallbackAnimal = animal;
			}
		}
		return fallbackAnimal;
	}

	public static long RandomLong(Random r = null)
	{
		if (r == null)
		{
			r = new Random();
		}
		byte[] bytes = new byte[8];
		r.NextBytes(bytes);
		return BitConverter.ToInt64(bytes, 0);
	}

	public static ulong NewUniqueIdForThisGame()
	{
		DateTime epoc = new DateTime(2012, 6, 22);
		return (ulong)(long)(DateTime.UtcNow - epoc).TotalSeconds;
	}

	public static string FilterDirtyWords(string words)
	{
		return Program.sdk.FilterDirtyWords(words);
	}

	/// <summary>
	/// This is used to filter out special characters from user entered 
	/// names to avoid crashes and other bugs in Dialogue.cs parsing.
	///
	/// The characters are replaced with spaces.
	/// </summary>
	public static string FilterUserName(string name)
	{
		return name;
	}

	public static bool IsHorizontalDirection(int direction)
	{
		if (direction != 3)
		{
			return direction == 1;
		}
		return true;
	}

	public static bool IsVerticalDirection(int direction)
	{
		if (direction != 0)
		{
			return direction == 2;
		}
		return true;
	}

	public static Microsoft.Xna.Framework.Rectangle ExpandRectangle(Microsoft.Xna.Framework.Rectangle rect, int facingDirection, int pixels)
	{
		switch (facingDirection)
		{
		case 0:
			rect.Height += pixels;
			rect.Y -= pixels;
			break;
		case 1:
			rect.Width += pixels;
			break;
		case 2:
			rect.Height += pixels;
			break;
		case 3:
			rect.Width += pixels;
			rect.X -= pixels;
			break;
		}
		return rect;
	}

	public static int GetOppositeFacingDirection(int facingDirection)
	{
		return facingDirection switch
		{
			0 => 2, 
			1 => 3, 
			2 => 0, 
			3 => 1, 
			_ => 0, 
		};
	}

	public static void RGBtoHSL(int r, int g, int b, out double h, out double s, out double l)
	{
		double double_r = (double)r / 255.0;
		double double_g = (double)g / 255.0;
		double double_b = (double)b / 255.0;
		double max = double_r;
		if (max < double_g)
		{
			max = double_g;
		}
		if (max < double_b)
		{
			max = double_b;
		}
		double min = double_r;
		if (min > double_g)
		{
			min = double_g;
		}
		if (min > double_b)
		{
			min = double_b;
		}
		double diff = max - min;
		l = (max + min) / 2.0;
		if (Math.Abs(diff) < 1E-05)
		{
			s = 0.0;
			h = 0.0;
			return;
		}
		if (l <= 0.5)
		{
			s = diff / (max + min);
		}
		else
		{
			s = diff / (2.0 - max - min);
		}
		double r_dist = (max - double_r) / diff;
		double g_dist = (max - double_g) / diff;
		double b_dist = (max - double_b) / diff;
		if (double_r == max)
		{
			h = b_dist - g_dist;
		}
		else if (double_g == max)
		{
			h = 2.0 + r_dist - b_dist;
		}
		else
		{
			h = 4.0 + g_dist - r_dist;
		}
		h *= 60.0;
		if (h < 0.0)
		{
			h += 360.0;
		}
	}

	public static void HSLtoRGB(double h, double s, double l, out int r, out int g, out int b)
	{
		double p2 = ((!(l <= 0.5)) ? (l + s - l * s) : (l * (1.0 + s)));
		double p1 = 2.0 * l - p2;
		double double_r;
		double double_g;
		double double_b;
		if (s == 0.0)
		{
			double_r = l;
			double_g = l;
			double_b = l;
		}
		else
		{
			double_r = QQHtoRGB(p1, p2, h + 120.0);
			double_g = QQHtoRGB(p1, p2, h);
			double_b = QQHtoRGB(p1, p2, h - 120.0);
		}
		r = (int)(double_r * 255.0);
		g = (int)(double_g * 255.0);
		b = (int)(double_b * 255.0);
	}

	private static double QQHtoRGB(double q1, double q2, double hue)
	{
		if (hue > 360.0)
		{
			hue -= 360.0;
		}
		else if (hue < 0.0)
		{
			hue += 360.0;
		}
		if (hue < 60.0)
		{
			return q1 + (q2 - q1) * hue / 60.0;
		}
		if (hue < 180.0)
		{
			return q2;
		}
		if (hue < 240.0)
		{
			return q1 + (q2 - q1) * (240.0 - hue) / 60.0;
		}
		return q1;
	}

	public static float ModifyCoordinateFromUIScale(float coordinate)
	{
		return coordinate * Game1.options.uiScale / Game1.options.zoomLevel;
	}

	public static Vector2 ModifyCoordinatesFromUIScale(Vector2 coordinates)
	{
		return coordinates * Game1.options.uiScale / Game1.options.zoomLevel;
	}

	public static float ModifyCoordinateForUIScale(float coordinate)
	{
		return coordinate / Game1.options.uiScale * Game1.options.zoomLevel;
	}

	public static Vector2 ModifyCoordinatesForUIScale(Vector2 coordinates)
	{
		return coordinates / Game1.options.uiScale * Game1.options.zoomLevel;
	}

	public static bool ShouldIgnoreValueChangeCallback()
	{
		if (Game1.gameMode != 3)
		{
			return true;
		}
		if (Game1.client != null && !Game1.client.readyToPlay)
		{
			return true;
		}
		if (Game1.client != null && Game1.locationRequest != null)
		{
			return true;
		}
		return false;
	}

	/// <summary>Constrain an index to a range by wrapping out-of-bounds values to the other side (e.g. last index + 1 is the first index).</summary>
	/// <param name="index">The index to constrain.</param>
	/// <param name="count">The number of values in the range.</param>
	public static int WrapIndex(int index, int count)
	{
		return (index + count) % count;
	}
}
