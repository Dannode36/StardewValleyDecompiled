using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Quests;

namespace StardewValley.Network;

public class NetWorldState : INetObject<NetFields>
{
	protected readonly NetLong uniqueIDForThisGame = new NetLong();

	protected readonly NetEnum<ServerPrivacy> serverPrivacy = new NetEnum<ServerPrivacy>();

	protected readonly NetInt whichFarm = new NetInt();

	protected readonly NetString whichModFarm = new NetString();

	protected string _oldModFarmType;

	public readonly NetEnum<Game1.MineChestType> shuffleMineChests = new NetEnum<Game1.MineChestType>(Game1.MineChestType.Default);

	public readonly NetInt minesDifficulty = new NetInt();

	public readonly NetInt skullCavesDifficulty = new NetInt();

	public readonly NetInt highestPlayerLimit = new NetInt(-1);

	public readonly NetInt currentPlayerLimit = new NetInt(-1);

	protected readonly NetInt year = new NetInt(1);

	protected readonly NetEnum<Season> season = new NetEnum<Season>(Season.Spring);

	protected readonly NetInt dayOfMonth = new NetInt(0);

	protected readonly NetInt timeOfDay = new NetInt();

	protected readonly NetInt daysPlayed = new NetInt();

	public readonly NetInt visitsUntilY1Guarantee = new NetInt(-1);

	protected readonly NetBool isPaused = new NetBool();

	protected readonly NetBool isTimePaused = new NetBool
	{
		InterpolationWait = false
	};

	protected readonly NetStringDictionary<LocationWeather, NetRef<LocationWeather>> locationWeather = new NetStringDictionary<LocationWeather, NetRef<LocationWeather>>();

	protected readonly NetBool isRaining = new NetBool();

	protected readonly NetBool isSnowing = new NetBool();

	protected readonly NetBool isLightning = new NetBool();

	protected readonly NetBool isDebrisWeather = new NetBool();

	public readonly NetString weatherForTomorrow = new NetString();

	protected readonly NetBundles bundles = new NetBundles();

	protected readonly NetIntDictionary<bool, NetBool> bundleRewards = new NetIntDictionary<bool, NetBool>();

	protected readonly NetStringDictionary<string, NetString> netBundleData = new NetStringDictionary<string, NetString>();

	protected Dictionary<string, string> _bundleData;

	protected bool _bundleDataDirty = true;

	public readonly NetArray<bool, NetBool> raccoonBundles = new NetArray<bool, NetBool>(2);

	public readonly NetInt seasonOfCurrentRacconBundle = new NetInt(-1);

	public readonly NetBool parrotPlatformsUnlocked = new NetBool();

	public readonly NetBool goblinRemoved = new NetBool();

	public readonly NetBool submarineLocked = new NetBool();

	public readonly NetInt lowestMineLevel = new NetInt();

	public readonly NetInt lowestMineLevelForOrder = new NetInt(-1);

	protected readonly NetVector2Dictionary<string, NetString> museumPieces = new NetVector2Dictionary<string, NetString>();

	protected readonly NetIntDelta lostBooksFound = new NetIntDelta
	{
		Minimum = 0,
		Maximum = 21
	};

	protected readonly NetIntDelta goldenWalnuts = new NetIntDelta
	{
		Minimum = 0
	};

	protected readonly NetIntDelta goldenWalnutsFound = new NetIntDelta
	{
		Minimum = 0
	};

	protected readonly NetBool goldenCoconutCracked = new NetBool();

	protected readonly NetStringHashSet foundBuriedNuts = new NetStringHashSet();

	protected readonly NetIntDelta miniShippingBinsObtained = new NetIntDelta
	{
		Minimum = 0
	};

	protected readonly NetIntDelta perfectionWaivers = new NetIntDelta
	{
		Minimum = 0
	};

	protected readonly NetIntDelta timesFedRaccoons = new NetIntDelta
	{
		Minimum = 0
	};

	protected readonly NetIntDelta treasureTotemsUsed = new NetIntDelta
	{
		Minimum = 0
	};

	public NetLongDictionary<Farmer, NetRef<Farmer>> farmhandData = new NetLongDictionary<Farmer, NetRef<Farmer>>();

	/// <summary>The backing field for <see cref="P:StardewValley.Network.NetWorldState.LocationsWithBuildings" />.</summary>
	public readonly NetStringHashSet locationsWithBuildings = new NetStringHashSet();

	public NetStringDictionary<BuilderData, NetRef<BuilderData>> builders = new NetStringDictionary<BuilderData, NetRef<BuilderData>>();

	public NetStringHashSet activePassiveFestivals = new NetStringHashSet();

	protected readonly NetStringHashSet worldStateIDs = new NetStringHashSet();

	protected readonly NetStringHashSet islandVisitors = new NetStringHashSet();

	protected readonly NetStringHashSet checkedGarbage = new NetStringHashSet();

	public readonly NetRef<Object> dishOfTheDay = new NetRef<Object>();

	private readonly NetBool activatedGoldenParrot = new NetBool();

	private readonly NetInt daysPlayedWhenLastRaccoonBundleWasFinished = new NetInt();

	public readonly NetBool canDriveYourselfToday = new NetBool();

	public readonly NetBool goldenClocksTurnedOff = new NetBool();

	/// <summary>The backing field for <see cref="P:StardewValley.Network.NetWorldState.QuestOfTheDay" />.</summary>
	protected readonly NetRef<Quest> netQuestOfTheDay = new NetRef<Quest>();

	public NetFields NetFields { get; } = new NetFields("NetWorldState");


	public ServerPrivacy ServerPrivacy
	{
		get
		{
			return serverPrivacy.Value;
		}
		set
		{
			serverPrivacy.Value = value;
		}
	}

	public Game1.MineChestType ShuffleMineChests
	{
		get
		{
			return shuffleMineChests.Value;
		}
		set
		{
			shuffleMineChests.Value = value;
		}
	}

	public int MinesDifficulty
	{
		get
		{
			return minesDifficulty;
		}
		set
		{
			minesDifficulty.Value = value;
		}
	}

	public int SkullCavesDifficulty
	{
		get
		{
			return skullCavesDifficulty;
		}
		set
		{
			skullCavesDifficulty.Value = value;
		}
	}

	public int HighestPlayerLimit
	{
		get
		{
			return highestPlayerLimit.Value;
		}
		set
		{
			highestPlayerLimit.Value = value;
		}
	}

	public int CurrentPlayerLimit
	{
		get
		{
			return currentPlayerLimit.Value;
		}
		set
		{
			currentPlayerLimit.Value = value;
		}
	}

	public WorldDate Date => WorldDate.Now();

	public int VisitsUntilY1Guarantee
	{
		get
		{
			return visitsUntilY1Guarantee.Value;
		}
		set
		{
			visitsUntilY1Guarantee.Value = value;
		}
	}

	public bool IsPaused
	{
		get
		{
			return isPaused;
		}
		set
		{
			isPaused.Value = value;
		}
	}

	public bool IsTimePaused
	{
		get
		{
			return isTimePaused;
		}
		set
		{
			isTimePaused.Value = value;
		}
	}

	public NetStringDictionary<LocationWeather, NetRef<LocationWeather>> LocationWeather => locationWeather;

	public string WeatherForTomorrow
	{
		get
		{
			return weatherForTomorrow;
		}
		set
		{
			weatherForTomorrow.Value = value;
		}
	}

	public NetBundles Bundles => bundles;

	public NetIntDictionary<bool, NetBool> BundleRewards => bundleRewards;

	public Dictionary<string, string> BundleData
	{
		get
		{
			if (netBundleData.Length == 0)
			{
				SetBundleData(DataLoader.Bundles(Game1.content));
			}
			if (_bundleDataDirty)
			{
				_bundleDataDirty = false;
				_bundleData = new Dictionary<string, string>();
				foreach (string key in netBundleData.Keys)
				{
					_bundleData[key] = netBundleData[key];
				}
				UpdateBundleDisplayNames();
			}
			return _bundleData;
		}
	}

	public bool ParrotPlatformsUnlocked
	{
		get
		{
			return parrotPlatformsUnlocked.Value;
		}
		set
		{
			parrotPlatformsUnlocked.Value = value;
		}
	}

	public bool IsGoblinRemoved
	{
		get
		{
			return goblinRemoved;
		}
		set
		{
			goblinRemoved.Value = value;
		}
	}

	public bool IsSubmarineLocked
	{
		get
		{
			return submarineLocked;
		}
		set
		{
			submarineLocked.Value = value;
		}
	}

	public int LowestMineLevel
	{
		get
		{
			return lowestMineLevel;
		}
		set
		{
			lowestMineLevel.Value = value;
		}
	}

	public int LowestMineLevelForOrder
	{
		get
		{
			return lowestMineLevelForOrder;
		}
		set
		{
			lowestMineLevelForOrder.Value = value;
		}
	}

	public NetVector2Dictionary<string, NetString> MuseumPieces => museumPieces;

	public int LostBooksFound
	{
		get
		{
			return lostBooksFound.Value;
		}
		set
		{
			lostBooksFound.Value = value;
		}
	}

	public int GoldenWalnuts
	{
		get
		{
			return goldenWalnuts.Value;
		}
		set
		{
			goldenWalnuts.Value = value;
		}
	}

	public int GoldenWalnutsFound
	{
		get
		{
			return goldenWalnutsFound.Value;
		}
		set
		{
			goldenWalnutsFound.Value = value;
		}
	}

	public bool GoldenCoconutCracked
	{
		get
		{
			return goldenCoconutCracked.Value;
		}
		set
		{
			goldenCoconutCracked.Value = value;
		}
	}

	public bool ActivatedGoldenParrot
	{
		get
		{
			return activatedGoldenParrot.Value;
		}
		set
		{
			activatedGoldenParrot.Value = value;
		}
	}

	public ISet<string> FoundBuriedNuts => foundBuriedNuts;

	public int MiniShippingBinsObtained
	{
		get
		{
			return miniShippingBinsObtained.Value;
		}
		set
		{
			miniShippingBinsObtained.Value = value;
		}
	}

	public int PerfectionWaivers
	{
		get
		{
			return perfectionWaivers.Value;
		}
		set
		{
			perfectionWaivers.Value = value;
		}
	}

	public int TimesFedRaccoons
	{
		get
		{
			return timesFedRaccoons.Value;
		}
		set
		{
			timesFedRaccoons.Value = value;
		}
	}

	public int TreasureTotemsUsed
	{
		get
		{
			return treasureTotemsUsed.Value;
		}
		set
		{
			treasureTotemsUsed.Value = value;
		}
	}

	public int SeasonOfCurrentRacconBundle
	{
		get
		{
			return seasonOfCurrentRacconBundle.Value;
		}
		set
		{
			seasonOfCurrentRacconBundle.Value = value;
		}
	}

	public int DaysPlayedWhenLastRaccoonBundleWasFinished
	{
		get
		{
			return daysPlayedWhenLastRaccoonBundleWasFinished.Value;
		}
		set
		{
			daysPlayedWhenLastRaccoonBundleWasFinished.Value = value;
		}
	}

	/// <summary>The unique names for locations which contain at least one constructed building.</summary>
	public ISet<string> LocationsWithBuildings => locationsWithBuildings;

	public NetStringDictionary<BuilderData, NetRef<BuilderData>> Builders => builders;

	public ISet<string> ActivePassiveFestivals => activePassiveFestivals;

	public ISet<string> IslandVisitors => islandVisitors;

	public ISet<string> CheckedGarbage => checkedGarbage;

	public Object DishOfTheDay
	{
		get
		{
			return dishOfTheDay.Value;
		}
		set
		{
			dishOfTheDay.Value = value;
		}
	}

	/// <summary>The daily quest that's shown on the billboard, if any.</summary>
	/// <remarks>This is synchronized from the host in multiplayer. See <see cref="M:StardewValley.Network.NetWorldState.SetQuestOfTheDay(StardewValley.Quests.Quest)" /> to set it.</remarks>
	public Quest QuestOfTheDay { get; private set; }

	public NetWorldState()
	{
		RegisterSpecialCurrencies();
		NetFields.SetOwner(this).AddField(uniqueIDForThisGame, "uniqueIDForThisGame").AddField(serverPrivacy, "serverPrivacy")
			.AddField(whichFarm, "whichFarm")
			.AddField(whichModFarm, "whichModFarm")
			.AddField(shuffleMineChests, "shuffleMineChests")
			.AddField(minesDifficulty, "minesDifficulty")
			.AddField(skullCavesDifficulty, "skullCavesDifficulty")
			.AddField(highestPlayerLimit, "highestPlayerLimit")
			.AddField(currentPlayerLimit, "currentPlayerLimit")
			.AddField(year, "year")
			.AddField(season, "season")
			.AddField(dayOfMonth, "dayOfMonth")
			.AddField(timeOfDay, "timeOfDay")
			.AddField(daysPlayed, "daysPlayed")
			.AddField(visitsUntilY1Guarantee, "visitsUntilY1Guarantee")
			.AddField(isPaused, "isPaused")
			.AddField(isTimePaused, "isTimePaused")
			.AddField(locationWeather, "locationWeather")
			.AddField(isRaining, "isRaining")
			.AddField(isSnowing, "isSnowing")
			.AddField(isLightning, "isLightning")
			.AddField(isDebrisWeather, "isDebrisWeather")
			.AddField(weatherForTomorrow, "weatherForTomorrow")
			.AddField(bundles, "bundles")
			.AddField(bundleRewards, "bundleRewards")
			.AddField(netBundleData, "netBundleData")
			.AddField(raccoonBundles, "raccoonBundles")
			.AddField(seasonOfCurrentRacconBundle, "seasonOfCurrentRacconBundle")
			.AddField(parrotPlatformsUnlocked, "parrotPlatformsUnlocked")
			.AddField(goblinRemoved, "goblinRemoved")
			.AddField(submarineLocked, "submarineLocked")
			.AddField(lowestMineLevel, "lowestMineLevel")
			.AddField(lowestMineLevelForOrder, "lowestMineLevelForOrder")
			.AddField(museumPieces, "museumPieces")
			.AddField(lostBooksFound, "lostBooksFound")
			.AddField(goldenWalnuts, "goldenWalnuts")
			.AddField(goldenWalnutsFound, "goldenWalnutsFound")
			.AddField(goldenCoconutCracked, "goldenCoconutCracked")
			.AddField(foundBuriedNuts, "foundBuriedNuts")
			.AddField(miniShippingBinsObtained, "miniShippingBinsObtained")
			.AddField(perfectionWaivers, "perfectionWaivers")
			.AddField(timesFedRaccoons, "timesFedRaccoons")
			.AddField(treasureTotemsUsed, "treasureTotemsUsed")
			.AddField(farmhandData, "farmhandData")
			.AddField(locationsWithBuildings, "locationsWithBuildings")
			.AddField(builders, "builders")
			.AddField(activePassiveFestivals, "activePassiveFestivals")
			.AddField(worldStateIDs, "worldStateIDs")
			.AddField(islandVisitors, "islandVisitors")
			.AddField(checkedGarbage, "checkedGarbage")
			.AddField(dishOfTheDay, "dishOfTheDay")
			.AddField(netQuestOfTheDay, "netQuestOfTheDay")
			.AddField(activatedGoldenParrot, "activatedGoldenParrot")
			.AddField(daysPlayedWhenLastRaccoonBundleWasFinished, "daysPlayedWhenLastRaccoonBundleWasFinished")
			.AddField(canDriveYourselfToday, "canDriveYourselfToday")
			.AddField(goldenClocksTurnedOff, "goldenClocksTurnedOff");
		netBundleData.OnConflictResolve += delegate
		{
			_bundleDataDirty = true;
		};
		netBundleData.OnValueAdded += delegate
		{
			_bundleDataDirty = true;
		};
		netBundleData.OnValueRemoved += delegate
		{
			_bundleDataDirty = true;
		};
		netQuestOfTheDay.fieldChangeVisibleEvent += delegate(NetRef<Quest> field, Quest oldQuest, Quest newQuest)
		{
			if (newQuest == null)
			{
				QuestOfTheDay = null;
				return;
			}
			using MemoryStream memoryStream = new MemoryStream();
			using BinaryWriter writer = new BinaryWriter(memoryStream);
			NetRef<Quest> netRef = new NetRef<Quest>();
			netRef.Value = newQuest;
			netRef.WriteFull(writer);
			memoryStream.Seek(0L, SeekOrigin.Begin);
			using BinaryReader reader = new BinaryReader(memoryStream);
			NetRef<Quest> netRef2 = new NetRef<Quest>();
			netRef2.ReadFull(reader, default(NetVersion));
			QuestOfTheDay = netRef2.Value;
		};
	}

	public virtual void RegisterSpecialCurrencies()
	{
		if (Game1.specialCurrencyDisplay != null)
		{
			Game1.specialCurrencyDisplay.Register("walnuts", goldenWalnuts);
			Game1.specialCurrencyDisplay.Register("qiGems", Game1.player.netQiGems);
		}
	}

	/// <summary>Sets the quest of the day and synchronizes it to other players. In multiplayer, this can only be called on the host instance.</summary>
	/// <param name="quest">The daily quest to set.</param>
	public void SetQuestOfTheDay(Quest quest)
	{
		if (!Game1.IsMasterGame)
		{
			Game1.log.Warn("Can't set the daily quest from a farmhand instance.");
			Game1.log.Verbose(new StackTrace().ToString());
		}
		else
		{
			netQuestOfTheDay.Value = quest;
		}
	}

	public void SetBundleData(Dictionary<string, string> data)
	{
		_bundleDataDirty = true;
		netBundleData.CopyFrom(data);
		foreach (KeyValuePair<string, string> pair in netBundleData.Pairs)
		{
			string key = pair.Key;
			string value = pair.Value;
			int index = Convert.ToInt32(key.Split('/')[1]);
			int count = ArgUtility.SplitBySpace(value.Split('/')[2]).Length;
			if (!bundles.ContainsKey(index))
			{
				bundles.Add(index, new NetArray<bool, NetBool>(count));
			}
			else if (bundles[index].Length < count)
			{
				NetArray<bool, NetBool> new_array = new NetArray<bool, NetBool>(count);
				for (int i = 0; i < Math.Min(bundles[index].Length, count); i++)
				{
					new_array[i] = bundles[index][i];
				}
				bundles.Remove(index);
				bundles.Add(index, new_array);
			}
			if (!bundleRewards.ContainsKey(index))
			{
				bundleRewards.Add(index, new NetBool(value: false));
			}
		}
	}

	public static bool checkAnywhereForWorldStateID(string id)
	{
		if (!Game1.worldStateIDs.Contains(id))
		{
			return Game1.netWorldState.Value.hasWorldStateID(id);
		}
		return true;
	}

	public static void addWorldStateIDEverywhere(string id)
	{
		Game1.netWorldState.Value.addWorldStateID(id);
		if (!Game1.worldStateIDs.Contains(id))
		{
			Game1.worldStateIDs.Add(id);
		}
	}

	public virtual void UpdateBundleDisplayNames()
	{
		List<string> list = new List<string>(_bundleData.Keys);
		Dictionary<string, string> localizedBundleData = DataLoader.Bundles(Game1.content);
		foreach (string key in list)
		{
			string[] fields = _bundleData[key].Split('/');
			string bundleName = fields[0];
			if (!ArgUtility.HasIndex(fields, 6))
			{
				Array.Resize(ref fields, 7);
			}
			string displayName = null;
			foreach (string value in localizedBundleData.Values)
			{
				string[] localizedFields = value.Split('/');
				if (ArgUtility.Get(localizedFields, 0) == bundleName)
				{
					displayName = ArgUtility.Get(localizedFields, 6);
					break;
				}
			}
			if (displayName == null)
			{
				displayName = Game1.content.LoadStringReturnNullIfNotFound("Strings\\BundleNames:" + bundleName);
			}
			fields[6] = displayName ?? bundleName;
			_bundleData[key] = string.Join("/", fields);
		}
	}

	public bool hasWorldStateID(string id)
	{
		return worldStateIDs.Contains(id);
	}

	public void addWorldStateID(string id)
	{
		worldStateIDs.Add(id);
	}

	public void removeWorldStateID(string id)
	{
		worldStateIDs.Remove(id);
	}

	public void SaveFarmhand(NetFarmerRoot farmhand)
	{
		if (Game1.netWorldState.Value.farmhandData.FieldDict.TryGetValue(farmhand.Value.UniqueMultiplayerID, out var farmhandData))
		{
			farmhand.CloneInto(farmhandData);
		}
		ResetFarmhandState(farmhand.Value);
	}

	public void ResetFarmhandState(Farmer farmhand)
	{
		farmhand.farmName.Value = Game1.MasterPlayer.farmName.Value;
		if (TryAssignFarmhandHome(farmhand))
		{
			FarmHouse farmhandHome = Utility.getHomeOfFarmer(farmhand);
			if (farmhand.lastSleepLocation.Value == null || farmhand.lastSleepLocation.Value == farmhandHome.NameOrUniqueName)
			{
				farmhand.currentLocation = farmhandHome;
				farmhand.Position = Utility.PointToVector2(farmhandHome.GetPlayerBedSpot()) * 64f;
			}
		}
		else
		{
			farmhand.userID.Value = "";
			farmhand.homeLocation.Value = null;
			Game1.otherFarmers.Remove(farmhand.UniqueMultiplayerID);
		}
		farmhand.resetState();
	}

	/// <summary>Assign a farmhand to a cabin if their current home is invalid.</summary>
	/// <param name="farmhand">The farmhand instance.</param>
	/// <returns>Returns whether the farmhand has a valid home (either already assigned or just assigned).</returns>
	public bool TryAssignFarmhandHome(Farmer farmhand)
	{
		if (farmhand.IsMainPlayer || Game1.getLocationFromName(farmhand.homeLocation.Value) is Cabin)
		{
			return true;
		}
		if (farmhand.currentLocation is Cabin curLocation && curLocation.CanAssignTo(farmhand))
		{
			curLocation.AssignFarmhand(farmhand);
			return true;
		}
		if (Game1.getLocationFromName(farmhand.lastSleepLocation.Value) is Cabin lastSleptCabin && lastSleptCabin.CanAssignTo(farmhand))
		{
			lastSleptCabin.AssignFarmhand(farmhand);
			return true;
		}
		bool found = false;
		Utility.ForEachBuilding(delegate(Building building)
		{
			if (building.GetIndoors() is Cabin cabin && cabin.CanAssignTo(farmhand))
			{
				cabin.AssignFarmhand(farmhand);
				found = true;
				return false;
			}
			return true;
		});
		return found;
	}

	public void UpdateFromGame1()
	{
		year.Value = Game1.year;
		season.Value = Game1.season;
		dayOfMonth.Value = Game1.dayOfMonth;
		timeOfDay.Value = Game1.timeOfDay;
		LocationWeather weatherForLocation = GetWeatherForLocation("Default");
		weatherForLocation.WeatherForTomorrow = Game1.weatherForTomorrow;
		weatherForLocation.IsRaining = Game1.isRaining;
		weatherForLocation.IsSnowing = Game1.isSnowing;
		weatherForLocation.IsDebrisWeather = Game1.isDebrisWeather;
		weatherForLocation.IsGreenRain = Game1.isGreenRain;
		isDebrisWeather.Value = Game1.isDebrisWeather;
		whichFarm.Value = Game1.whichFarm;
		weatherForTomorrow.Value = Game1.weatherForTomorrow;
		daysPlayed.Value = (int)Game1.stats.DaysPlayed;
		uniqueIDForThisGame.Value = (long)Game1.uniqueIDForThisGame;
		if (Game1.whichFarm != 7 || Game1.whichModFarm == null)
		{
			whichModFarm.Value = null;
		}
		else
		{
			whichModFarm.Value = Game1.whichModFarm.Id;
		}
		currentPlayerLimit.Value = Game1.multiplayer.playerLimit;
		highestPlayerLimit.Value = Math.Max(highestPlayerLimit.Value, Game1.multiplayer.playerLimit);
		worldStateIDs.Clear();
		worldStateIDs.AddRange(Game1.worldStateIDs);
	}

	public LocationWeather GetWeatherForLocation(string locationContextId)
	{
		if (!this.locationWeather.TryGetValue(locationContextId, out var weather))
		{
			weather = (this.locationWeather[locationContextId] = new LocationWeather());
			if (Game1.locationContextData.TryGetValue(locationContextId, out var contextData))
			{
				weather.UpdateDailyWeather(locationContextId, contextData, Game1.random);
				weather.UpdateDailyWeather(locationContextId, contextData, Game1.random);
			}
		}
		return weather;
	}

	public void WriteToGame1(bool onLoad = false)
	{
		if (Game1.farmEvent != null)
		{
			return;
		}
		LocationWeather weatherForLocation = GetWeatherForLocation("Default");
		Game1.weatherForTomorrow = weatherForLocation.WeatherForTomorrow;
		Game1.isRaining = weatherForLocation.IsRaining;
		Game1.isSnowing = weatherForLocation.IsSnowing;
		Game1.isLightning = weatherForLocation.IsLightning;
		Game1.isDebrisWeather = weatherForLocation.IsDebrisWeather;
		Game1.isGreenRain = weatherForLocation.IsGreenRain;
		Game1.weatherForTomorrow = weatherForTomorrow.Value;
		Game1.worldStateIDs = new HashSet<string>(worldStateIDs);
		if (!Game1.IsServer)
		{
			bool newSeason = Game1.season != season.Value;
			Game1.year = year.Value;
			Game1.season = season.Value;
			Game1.dayOfMonth = dayOfMonth.Value;
			Game1.timeOfDay = timeOfDay.Value;
			Game1.whichFarm = whichFarm.Value;
			if (Game1.whichFarm != 7)
			{
				Game1.whichModFarm = null;
			}
			else if (_oldModFarmType != whichModFarm.Value)
			{
				_oldModFarmType = whichModFarm.Value;
				Game1.whichModFarm = null;
				List<ModFarmType> farm_types = DataLoader.AdditionalFarms(Game1.content);
				if (farm_types != null)
				{
					foreach (ModFarmType farm_type in farm_types)
					{
						if (farm_type.Id == whichModFarm.Value)
						{
							Game1.whichModFarm = farm_type;
							break;
						}
					}
				}
				if (Game1.whichModFarm == null)
				{
					throw new Exception(whichModFarm.Value + " is not a valid farm type.");
				}
			}
			Game1.stats.DaysPlayed = (uint)daysPlayed.Value;
			Game1.uniqueIDForThisGame = (ulong)uniqueIDForThisGame.Value;
			if (newSeason)
			{
				Game1.setGraphicsForSeason(onLoad);
			}
		}
		Game1.updateWeatherIcon();
		if (IsGoblinRemoved)
		{
			Game1.player.removeQuest("27");
		}
	}

	/// <summary>Get cached info about the building being constructed by an NPC.</summary>
	/// <param name="builderName">The internal name of the NPC constructing buildings.</param>
	public BuilderData GetBuilderData(string builderName)
	{
		if (!builders.TryGetValue(builderName, out var data))
		{
			return null;
		}
		return data;
	}

	/// <summary>Mark a building as being under construction.</summary>
	/// <param name="builderName">The internal name of the NPC constructing it.</param>
	/// <param name="building">The building being constructed.</param>
	public void MarkUnderConstruction(string builderName, Building building)
	{
		int buildDays = building.daysOfConstructionLeft.Value;
		int upgradeDays = building.daysUntilUpgrade.Value;
		int daysUntilFinished = Math.Max(buildDays, upgradeDays);
		if (daysUntilFinished != 0)
		{
			builders[builderName] = new BuilderData(building.buildingType.Value, daysUntilFinished, building.parentLocationName.Value, new Point(building.tileX, building.tileY), upgradeDays > 0 && buildDays <= 0);
		}
	}

	/// <summary>Remove constructed buildings from the cached list of buildings under construction.</summary>
	public void UpdateUnderConstruction()
	{
		KeyValuePair<string, BuilderData>[] array = builders.Pairs.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			KeyValuePair<string, BuilderData> pair = array[i];
			string builderName = pair.Key;
			BuilderData data = pair.Value;
			GameLocation location = Game1.getLocationFromName(data.buildingLocation);
			if (location == null)
			{
				builders.Remove(builderName);
				continue;
			}
			Building building = location.getBuildingAt(Utility.PointToVector2(data.buildingTile.Value));
			if (building == null || !building.isUnderConstruction(ignoreUpgrades: false))
			{
				builders.Remove(builderName);
			}
		}
	}

	/// <summary>Add or remove the location from the <see cref="P:StardewValley.Network.NetWorldState.LocationsWithBuildings" /> cache.</summary>
	/// <param name="location">The location to update.</param>
	public void UpdateBuildingCache(GameLocation location)
	{
		string name = location.NameOrUniqueName;
		if (location.buildings.Count > 0)
		{
			locationsWithBuildings.Add(name);
		}
		else
		{
			locationsWithBuildings.Remove(name);
		}
	}
}
