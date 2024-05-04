using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Ionic.Zlib;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Characters;
using StardewValley.GameData.Locations;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.Quests;
using StardewValley.SaveMigrations;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;

namespace StardewValley;

public class SaveGame
{
	public static XmlSerializer serializer = new XmlSerializer(typeof(SaveGame), new Type[5]
	{
		typeof(Character),
		typeof(GameLocation),
		typeof(Item),
		typeof(Quest),
		typeof(TerrainFeature)
	});

	public static XmlSerializer farmerSerializer = new XmlSerializer(typeof(Farmer), new Type[1] { typeof(Item) });

	public static XmlSerializer locationSerializer = new XmlSerializer(typeof(GameLocation), new Type[3]
	{
		typeof(Character),
		typeof(Item),
		typeof(TerrainFeature)
	});

	[InstancedStatic]
	public static bool IsProcessing;

	[InstancedStatic]
	public static bool CancelToTitle;

	public Farmer player;

	public List<Farmer> farmhands;

	public List<GameLocation> locations;

	public string currentSeason;

	public string samBandName;

	public string elliottBookName;

	/// <summary>Obsolete. This is only kept to preserve data from old save files.</summary>
	[XmlArray("mailbox")]
	public List<string> obsolete_mailbox;

	public HashSet<string> broadcastedMail;

	public HashSet<string> constructedBuildings;

	public HashSet<string> worldStateIDs;

	public int lostBooksFound = -1;

	public int goldenWalnuts = -1;

	public int goldenWalnutsFound;

	public int miniShippingBinsObtained;

	public bool mineShrineActivated;

	public bool skullShrineActivated;

	public bool goldenCoconutCracked;

	public bool parrotPlatformsUnlocked;

	public bool farmPerfect;

	public List<string> foundBuriedNuts = new List<string>();

	public List<string> checkedGarbage = new List<string>();

	public int visitsUntilY1Guarantee = -1;

	public Game1.MineChestType shuffleMineChests;

	public int dayOfMonth;

	public int year;

	public int? countdownToWedding;

	public double dailyLuck;

	public ulong uniqueIDForThisGame;

	public bool weddingToday;

	public bool isRaining;

	public bool isDebrisWeather;

	public bool isLightning;

	public bool isSnowing;

	public bool shouldSpawnMonsters;

	public bool hasApplied1_3_UpdateChanges;

	public bool hasApplied1_4_UpdateChanges;

	/// <summary>Obsolete. This is only kept to preserve data from old save files.</summary>
	[XmlElement("stats")]
	public Stats obsolete_stats;

	[InstancedStatic]
	public static SaveGame loaded;

	public float musicVolume;

	public float soundVolume;

	public Object dishOfTheDay;

	public int highestPlayerLimit = -1;

	public int moveBuildingPermissionMode;

	public bool useLegacyRandom;

	public SerializableDictionary<string, LocationWeather> locationWeather;

	public SerializableDictionary<string, BuilderData> builders;

	public SerializableDictionary<string, string> bannedUsers = new SerializableDictionary<string, string>();

	public SerializableDictionary<string, string> bundleData = new SerializableDictionary<string, string>();

	public SerializableDictionary<string, int> limitedNutDrops = new SerializableDictionary<string, int>();

	public long latestID;

	public Options options;

	public SerializableDictionary<long, Options> splitscreenOptions = new SerializableDictionary<long, Options>();

	public SerializableDictionary<string, string> CustomData = new SerializableDictionary<string, string>();

	public SerializableDictionary<int, MineInfo> mine_permanentMineChanges;

	public int mine_lowestLevelReached;

	public string weatherForTomorrow;

	public string whichFarm;

	public int mine_lowestLevelReachedForOrder = -1;

	public int skullCavesDifficulty;

	public int minesDifficulty;

	public int currentGemBirdIndex;

	public NetLeaderboards junimoKartLeaderboards;

	public List<SpecialOrder> specialOrders;

	public List<SpecialOrder> availableSpecialOrders;

	public List<string> completedSpecialOrders;

	public List<string> acceptedSpecialOrderTypes = new List<string>();

	public List<Item> returnedDonations;

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.SaveGame.globalInventories" /> instead.</summary>
	public List<Item> junimoChest;

	/// <inheritdoc cref="F:StardewValley.FarmerTeam.globalInventories" />
	public SerializableDictionary<string, List<Item>> globalInventories = new SerializableDictionary<string, List<Item>>();

	public List<string> collectedNutTracker = new List<string>();

	public SerializableDictionary<FarmerPair, Friendship> farmerFriendships = new SerializableDictionary<FarmerPair, Friendship>();

	public SerializableDictionary<int, long> cellarAssignments = new SerializableDictionary<int, long>();

	public int timesFedRaccoons;

	public int treasureTotemsUsed;

	public int perfectionWaivers;

	public int seasonOfCurrentRaccoonBundle;

	public bool[] raccoonBundles = new bool[2];

	public bool activatedGoldenParrot;

	public int daysPlayedWhenLastRaccoonBundleWasFinished;

	public int lastAppliedSaveFix;

	public string gameVersion = Game1.version;

	public string gameVersionLabel;

	public static XmlSerializer GetSerializer(Type type)
	{
		return new XmlSerializer(type);
	}

	/// <summary>Get whether a fix was applied to the loaded data before it was last saved.</summary>
	/// <param name="fix">The save fix to check.</param>
	public bool HasSaveFix(SaveFixes fix)
	{
		return lastAppliedSaveFix >= (int)fix;
	}

	public static IEnumerator<int> Save()
	{
		IsProcessing = true;
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			IEnumerator<int> save = getSaveEnumerator();
			while (save.MoveNext())
			{
				yield return save.Current;
			}
			yield return 100;
			yield break;
		}
		Game1.log.Verbose("SaveGame.Save() called.");
		yield return 1;
		IEnumerator<int> loader = getSaveEnumerator();
		Task saveTask = new Task(delegate
		{
			Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
			if (loader != null)
			{
				while (loader.MoveNext() && loader.Current < 100)
				{
				}
			}
		});
		Game1.hooks.StartTask(saveTask, "Save");
		while (!saveTask.IsCanceled && !saveTask.IsCompleted && !saveTask.IsFaulted)
		{
			yield return 1;
		}
		IsProcessing = false;
		if (saveTask.IsFaulted)
		{
			Exception e = saveTask.Exception.GetBaseException();
			Game1.log.Error("saveTask failed with an exception", e);
			if (!(e is TaskCanceledException))
			{
				throw e;
			}
			Game1.ExitToTitle();
		}
		else
		{
			Game1.log.Verbose("SaveGame.Save() completed without exceptions.");
			yield return 100;
		}
	}

	public static string FilterFileName(string fileName)
	{
		string text = fileName;
		for (int i = 0; i < text.Length; i++)
		{
			char c = text[i];
			if (!char.IsLetterOrDigit(c))
			{
				fileName = fileName.Replace(c.ToString() ?? "", "");
			}
		}
		return fileName;
	}

	public static IEnumerator<int> getSaveEnumerator()
	{
		if (CancelToTitle)
		{
			throw new TaskCanceledException();
		}
		yield return 1;
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			allFarmer.UnapplyAllTrinketEffects();
		}
		SaveGame saveData = new SaveGame();
		saveData.player = Game1.player;
		saveData.player.gameVersion = Game1.version;
		saveData.player.gameVersionLabel = Game1.versionLabel;
		saveData.farmhands = new List<Farmer>();
		saveData.farmhands.AddRange(Game1.netWorldState.Value.farmhandData.Values);
		saveData.locations = new List<GameLocation>();
		saveData.locations.AddRange(Game1.locations);
		foreach (GameLocation location in Game1.locations)
		{
			location.cleanupBeforeSave();
		}
		saveData.currentSeason = Game1.currentSeason;
		saveData.samBandName = Game1.samBandName;
		saveData.broadcastedMail = new HashSet<string>(Game1.player.team.broadcastedMail);
		saveData.constructedBuildings = new HashSet<string>(Game1.player.team.constructedBuildings);
		saveData.bannedUsers = Game1.bannedUsers;
		saveData.skullCavesDifficulty = Game1.netWorldState.Value.SkullCavesDifficulty;
		saveData.minesDifficulty = Game1.netWorldState.Value.MinesDifficulty;
		saveData.visitsUntilY1Guarantee = Game1.netWorldState.Value.VisitsUntilY1Guarantee;
		saveData.shuffleMineChests = Game1.netWorldState.Value.ShuffleMineChests;
		saveData.elliottBookName = Game1.elliottBookName;
		saveData.dayOfMonth = Game1.dayOfMonth;
		saveData.year = Game1.year;
		saveData.dailyLuck = Game1.player.team.sharedDailyLuck.Value;
		saveData.isRaining = Game1.isRaining;
		saveData.isLightning = Game1.isLightning;
		saveData.isSnowing = Game1.isSnowing;
		saveData.isDebrisWeather = Game1.isDebrisWeather;
		saveData.shouldSpawnMonsters = Game1.spawnMonstersAtNight;
		saveData.specialOrders = Game1.player.team.specialOrders.ToList();
		saveData.availableSpecialOrders = Game1.player.team.availableSpecialOrders.ToList();
		saveData.completedSpecialOrders = Game1.player.team.completedSpecialOrders.ToList();
		saveData.collectedNutTracker = Game1.player.team.collectedNutTracker.ToList();
		saveData.acceptedSpecialOrderTypes = Game1.player.team.acceptedSpecialOrderTypes.ToList();
		saveData.returnedDonations = Game1.player.team.returnedDonations.ToList();
		saveData.weddingToday = Game1.weddingToday;
		saveData.globalInventories = new SerializableDictionary<string, List<Item>>();
		foreach (KeyValuePair<string, NetRef<Inventory>> pair in Game1.player.team.globalInventories.FieldDict)
		{
			IInventory inventory = pair.Value.Value;
			if (inventory.HasAny())
			{
				saveData.globalInventories[pair.Key] = inventory.ToList();
			}
		}
		if (Game1.whichFarm == 7)
		{
			saveData.whichFarm = Game1.whichModFarm.Id;
		}
		else
		{
			saveData.whichFarm = Game1.whichFarm.ToString();
		}
		saveData.junimoKartLeaderboards = Game1.player.team.junimoKartScores;
		saveData.lastAppliedSaveFix = (int)Game1.lastAppliedSaveFix;
		saveData.locationWeather = SerializableDictionary<string, LocationWeather>.BuildFrom(Game1.netWorldState.Value.LocationWeather.FieldDict, (NetRef<LocationWeather> value) => value.Value);
		saveData.builders = SerializableDictionary<string, BuilderData>.BuildFrom(Game1.netWorldState.Value.Builders.FieldDict, (NetRef<BuilderData> value) => value.Value);
		saveData.cellarAssignments = SerializableDictionary<int, long>.BuildFrom(Game1.player.team.cellarAssignments.FieldDict, (NetLong value) => value.Value);
		saveData.uniqueIDForThisGame = Game1.uniqueIDForThisGame;
		saveData.musicVolume = Game1.options.musicVolumeLevel;
		saveData.soundVolume = Game1.options.soundVolumeLevel;
		saveData.mine_lowestLevelReached = Game1.netWorldState.Value.LowestMineLevel;
		saveData.mine_lowestLevelReachedForOrder = Game1.netWorldState.Value.LowestMineLevelForOrder;
		saveData.currentGemBirdIndex = Game1.currentGemBirdIndex;
		saveData.mine_permanentMineChanges = MineShaft.permanentMineChanges;
		saveData.dishOfTheDay = Game1.dishOfTheDay;
		saveData.latestID = (long)Game1.multiplayer.latestID;
		saveData.highestPlayerLimit = Game1.netWorldState.Value.HighestPlayerLimit;
		saveData.options = Game1.options;
		saveData.splitscreenOptions = Game1.splitscreenOptions;
		saveData.CustomData = Game1.CustomData;
		saveData.worldStateIDs = Game1.worldStateIDs;
		saveData.weatherForTomorrow = Game1.weatherForTomorrow;
		saveData.goldenWalnuts = Game1.netWorldState.Value.GoldenWalnuts;
		saveData.goldenWalnutsFound = Game1.netWorldState.Value.GoldenWalnutsFound;
		saveData.miniShippingBinsObtained = Game1.netWorldState.Value.MiniShippingBinsObtained;
		saveData.goldenCoconutCracked = Game1.netWorldState.Value.GoldenCoconutCracked;
		saveData.parrotPlatformsUnlocked = Game1.netWorldState.Value.ParrotPlatformsUnlocked;
		saveData.farmPerfect = Game1.player.team.farmPerfect.Value;
		saveData.lostBooksFound = Game1.netWorldState.Value.LostBooksFound;
		saveData.foundBuriedNuts = Game1.netWorldState.Value.FoundBuriedNuts.ToList();
		saveData.checkedGarbage = Game1.netWorldState.Value.CheckedGarbage.ToList();
		saveData.mineShrineActivated = Game1.player.team.mineShrineActivated;
		saveData.skullShrineActivated = Game1.player.team.skullShrineActivated;
		saveData.timesFedRaccoons = Game1.netWorldState.Value.TimesFedRaccoons;
		saveData.treasureTotemsUsed = Game1.netWorldState.Value.TreasureTotemsUsed;
		saveData.perfectionWaivers = Game1.netWorldState.Value.PerfectionWaivers;
		saveData.seasonOfCurrentRaccoonBundle = Game1.netWorldState.Value.SeasonOfCurrentRacconBundle;
		saveData.raccoonBundles = Game1.netWorldState.Value.raccoonBundles.ToArray();
		saveData.activatedGoldenParrot = Game1.netWorldState.Value.ActivatedGoldenParrot;
		saveData.daysPlayedWhenLastRaccoonBundleWasFinished = Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished;
		saveData.gameVersion = Game1.version;
		saveData.gameVersionLabel = Game1.versionLabel;
		saveData.limitedNutDrops = SerializableDictionary<string, int>.BuildFrom(Game1.player.team.limitedNutDrops.FieldDict, (NetInt value) => value.Value);
		saveData.bundleData = new SerializableDictionary<string, string>(Game1.netWorldState.Value.BundleData);
		saveData.moveBuildingPermissionMode = (int)Game1.player.team.farmhandsCanMoveBuildings.Value;
		saveData.useLegacyRandom = Game1.player.team.useLegacyRandom.Value;
		saveData.hasApplied1_3_UpdateChanges = Game1.hasApplied1_3_UpdateChanges;
		saveData.hasApplied1_4_UpdateChanges = Game1.hasApplied1_4_UpdateChanges;
		saveData.farmerFriendships = SerializableDictionary<FarmerPair, Friendship>.BuildFrom(Game1.player.team.friendshipData.FieldDict, (NetRef<Friendship> value) => value.Value);
		string tmpString = "_STARDEWVALLEYSAVETMP";
		bool save_backups_and_metadata = true;
		string friendlyName = FilterFileName(Game1.GetSaveGameName());
		string filenameNoTmpString = friendlyName + "_" + Game1.uniqueIDForThisGame;
		string filenameWithTmpString = friendlyName + "_" + Game1.uniqueIDForThisGame + tmpString;
		string save_directory = Path.Combine(Program.GetSavesFolder(), filenameNoTmpString + Path.DirectorySeparatorChar);
		if (Game1.savePathOverride != "")
		{
			save_directory = Game1.savePathOverride;
			if (Game1.savePathOverride != "")
			{
				save_backups_and_metadata = false;
			}
		}
		string fullFilePath = Path.Combine(save_directory, filenameWithTmpString);
		ensureFolderStructureExists();
		string justFarmerFilePath = Path.Combine(save_directory, "SaveGameInfo" + tmpString);
		if (File.Exists(fullFilePath))
		{
			File.Delete(fullFilePath);
		}
		if (save_backups_and_metadata && File.Exists(justFarmerFilePath))
		{
			File.Delete(justFarmerFilePath);
		}
		Stream fstream = null;
		try
		{
			fstream = File.Create(fullFilePath);
		}
		catch (IOException ex)
		{
			if (fstream != null)
			{
				fstream.Close();
				fstream.Dispose();
			}
			Game1.gameMode = 9;
			Game1.debugOutput = Game1.parseText(ex.Message);
			yield break;
		}
		MemoryStream mstream1 = new MemoryStream(1024);
		MemoryStream mstream2 = new MemoryStream(1024);
		if (CancelToTitle)
		{
			throw new TaskCanceledException();
		}
		yield return 2;
		XmlWriterSettings settings = new XmlWriterSettings();
		settings.CloseOutput = false;
		Game1.log.Verbose("Saving without compression...");
		_ = mstream2;
		XmlWriter writer = XmlWriter.Create(mstream1, settings);
		writer.WriteStartDocument();
		serializer.Serialize(writer, saveData);
		writer.WriteEndDocument();
		writer.Flush();
		writer.Close();
		mstream1.Close();
		byte[] buffer1 = mstream1.ToArray();
		if (CancelToTitle)
		{
			throw new TaskCanceledException();
		}
		yield return 2;
		fstream.Write(buffer1, 0, buffer1.Length);
		fstream.Close();
		if (save_backups_and_metadata)
		{
			Game1.player.saveTime = (int)(DateTime.UtcNow - new DateTime(2012, 6, 22)).TotalMinutes;
			try
			{
				fstream = File.Create(justFarmerFilePath);
			}
			catch (IOException ex2)
			{
				fstream?.Close();
				Game1.gameMode = 9;
				Game1.debugOutput = Game1.parseText(ex2.Message);
				yield break;
			}
			writer = XmlWriter.Create(fstream, new XmlWriterSettings
			{
				CloseOutput = false
			});
			writer.WriteStartDocument();
			farmerSerializer.Serialize(writer, Game1.player);
			writer.WriteEndDocument();
			writer.Flush();
			fstream.Close();
		}
		if (CancelToTitle)
		{
			throw new TaskCanceledException();
		}
		yield return 2;
		fullFilePath = Path.Combine(save_directory, filenameNoTmpString);
		justFarmerFilePath = Path.Combine(save_directory, "SaveGameInfo");
		if (save_backups_and_metadata)
		{
			string fullFilePathOld = Path.Combine(save_directory, filenameNoTmpString + "_old");
			string justFarmerFilePathOld = Path.Combine(save_directory, "SaveGameInfo_old");
			if (File.Exists(fullFilePathOld))
			{
				File.Delete(fullFilePathOld);
			}
			if (File.Exists(justFarmerFilePathOld))
			{
				File.Delete(justFarmerFilePathOld);
			}
			try
			{
				File.Move(fullFilePath, fullFilePathOld);
				File.Move(justFarmerFilePath, justFarmerFilePathOld);
			}
			catch (Exception)
			{
			}
		}
		if (File.Exists(fullFilePath))
		{
			File.Delete(fullFilePath);
		}
		if (save_backups_and_metadata && File.Exists(justFarmerFilePath))
		{
			File.Delete(justFarmerFilePath);
		}
		fullFilePath = Path.Combine(save_directory, filenameWithTmpString);
		if (File.Exists(fullFilePath))
		{
			File.Move(fullFilePath, fullFilePath.Replace(tmpString, ""));
		}
		if (save_backups_and_metadata)
		{
			justFarmerFilePath = Path.Combine(save_directory, "SaveGameInfo" + tmpString);
			if (File.Exists(justFarmerFilePath))
			{
				File.Move(justFarmerFilePath, justFarmerFilePath.Replace(tmpString, ""));
			}
		}
		foreach (Farmer allFarmer2 in Game1.getAllFarmers())
		{
			allFarmer2.resetAllTrinketEffects();
		}
		Game1.player.sleptInTemporaryBed.Value = false;
		if (CancelToTitle)
		{
			throw new TaskCanceledException();
		}
		yield return 100;
	}

	public static bool IsNewGameSaveNameCollision(string save_name)
	{
		string filename = FilterFileName(save_name) + "_" + Game1.uniqueIDForThisGame;
		return Directory.Exists(Path.Combine(Program.GetSavesFolder(), filename));
	}

	public static void ensureFolderStructureExists(string tmpString = "")
	{
		string filename = FilterFileName(Game1.GetSaveGameName()) + "_" + Game1.uniqueIDForThisGame + tmpString;
		Directory.CreateDirectory(Path.Combine(Program.GetSavesFolder(), filename));
	}

	public static void Load(string filename)
	{
		Game1.gameMode = 6;
		Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4690");
		Game1.currentLoader = getLoadEnumerator(filename);
	}

	public static void LoadFarmType()
	{
		List<ModFarmType> farm_types = DataLoader.AdditionalFarms(Game1.content);
		Game1.whichFarm = -1;
		if (farm_types != null)
		{
			foreach (ModFarmType farm_type in farm_types)
			{
				if (farm_type.Id == loaded.whichFarm)
				{
					Game1.whichModFarm = farm_type;
					Game1.whichFarm = 7;
					break;
				}
			}
		}
		if (loaded.whichFarm == null)
		{
			Game1.whichFarm = 0;
		}
		if (Game1.whichFarm < 0)
		{
			if (int.TryParse(loaded.whichFarm, out var farmType))
			{
				Game1.whichFarm = farmType;
				return;
			}
			Game1.log.Warn("Ignored unknown farm type '" + loaded.whichFarm + "' which no longer exists in the data.");
			Game1.whichFarm = 0;
			Game1.whichModFarm = null;
		}
	}

	public static IEnumerator<int> getLoadEnumerator(string file)
	{
		Game1.SetSaveName(Path.GetFileNameWithoutExtension(file).Split('_').FirstOrDefault());
		Game1.log.Verbose("getLoadEnumerator('" + file + "')");
		Stopwatch stopwatch = Stopwatch.StartNew();
		Game1.loadingMessage = "Accessing save...";
		SaveGame saveData = new SaveGame();
		IsProcessing = true;
		if (CancelToTitle)
		{
			Game1.ExitToTitle();
		}
		yield return 1;
		Stream stream = null;
		string fullFilePath = file;
		Game1.savePathOverride = Path.GetDirectoryName(file);
		if (Game1.savePathOverride == "")
		{
			fullFilePath = Path.Combine(Program.GetSavesFolder(), file, file);
		}
		if (!File.Exists(fullFilePath))
		{
			fullFilePath += ".xml";
			if (!File.Exists(fullFilePath))
			{
				Game1.gameMode = 9;
				Game1.debugOutput = "File does not exist (-_-)";
				yield break;
			}
		}
		yield return 5;
		try
		{
			byte[] buffer = File.ReadAllBytes(fullFilePath);
			stream = new MemoryStream(buffer, writable: false);
		}
		catch (IOException ex)
		{
			Game1.gameMode = 9;
			Game1.debugOutput = Game1.parseText(ex.Message);
			stream?.Close();
			yield break;
		}
		Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4696");
		yield return 7;
		byte num = (byte)stream.ReadByte();
		stream.Position--;
		if (num == 120)
		{
			Game1.log.Verbose("zlib stream detected...");
			stream = new ZlibStream(stream, CompressionMode.Decompress);
		}
		else
		{
			Game1.log.Verbose("regular stream detected...");
		}
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			loaded = (SaveGame)serializer.Deserialize(stream);
		}
		else
		{
			SaveGame pendingSaveGame = null;
			Task deserializeTask = new Task(delegate
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				pendingSaveGame = (SaveGame)serializer.Deserialize(stream);
			});
			Game1.hooks.StartTask(deserializeTask, "Load_Deserialize");
			while (!deserializeTask.IsCanceled && !deserializeTask.IsCompleted && !deserializeTask.IsFaulted)
			{
				yield return 20;
			}
			if (deserializeTask.IsFaulted)
			{
				Exception e = deserializeTask.Exception.GetBaseException();
				Game1.log.Error("deserializeTask failed with an exception.", e);
				throw e;
			}
			loaded = pendingSaveGame;
		}
		stream.Dispose();
		Game1.hasApplied1_3_UpdateChanges = loaded.hasApplied1_3_UpdateChanges;
		Game1.hasApplied1_4_UpdateChanges = loaded.hasApplied1_4_UpdateChanges;
		Game1.lastAppliedSaveFix = (SaveFixes)loaded.lastAppliedSaveFix;
		Game1.player.team.useLegacyRandom.Value = loaded.useLegacyRandom;
		Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4697");
		if (CancelToTitle)
		{
			Game1.ExitToTitle();
		}
		yield return 20;
		LoadFarmType();
		Game1.year = loaded.year;
		Game1.netWorldState.Value.CurrentPlayerLimit = Game1.multiplayer.playerLimit;
		if (loaded.highestPlayerLimit >= 0)
		{
			Game1.netWorldState.Value.HighestPlayerLimit = loaded.highestPlayerLimit;
		}
		else
		{
			Game1.netWorldState.Value.HighestPlayerLimit = Math.Max(Game1.netWorldState.Value.HighestPlayerLimit, Game1.multiplayer.MaxPlayers);
		}
		Game1.uniqueIDForThisGame = loaded.uniqueIDForThisGame;
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			Game1.game1.loadForNewGame(loadedGame: true);
		}
		else
		{
			Task deserializeTask = new Task(delegate
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				Game1.game1.loadForNewGame(loadedGame: true);
			});
			Game1.hooks.StartTask(deserializeTask, "Load_LoadForNewGame");
			while (!deserializeTask.IsCanceled && !deserializeTask.IsCompleted && !deserializeTask.IsFaulted)
			{
				yield return 24;
			}
			if (deserializeTask.IsFaulted)
			{
				Exception e = deserializeTask.Exception.GetBaseException();
				Game1.log.Error("loadNewGameTask failed with an exception.", e);
				throw e;
			}
			if (CancelToTitle)
			{
				Game1.ExitToTitle();
			}
			yield return 25;
		}
		Game1.weatherForTomorrow = (int.TryParse(loaded.weatherForTomorrow, out var legacyWeather) ? Utility.LegacyWeatherToWeather(legacyWeather) : loaded.weatherForTomorrow);
		Game1.dayOfMonth = loaded.dayOfMonth;
		Game1.year = loaded.year;
		Game1.currentSeason = loaded.currentSeason;
		Game1.worldStateIDs = loaded.worldStateIDs;
		Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4698");
		if (loaded.mine_permanentMineChanges != null)
		{
			MineShaft.permanentMineChanges = loaded.mine_permanentMineChanges;
			Game1.netWorldState.Value.LowestMineLevel = loaded.mine_lowestLevelReached;
			Game1.netWorldState.Value.LowestMineLevelForOrder = loaded.mine_lowestLevelReachedForOrder;
		}
		Game1.currentGemBirdIndex = loaded.currentGemBirdIndex;
		if (loaded.bundleData.Count > 0)
		{
			if (!loaded.HasSaveFix(SaveFixes.StandardizeBundleFields))
			{
				SaveMigrator_1_6.StandardizeBundleFields(loaded.bundleData);
			}
			Game1.netWorldState.Value.SetBundleData(loaded.bundleData);
			foreach (string key in Game1.netWorldState.Value.BundleData.Keys)
			{
				saveData.bundleData[key] = Game1.netWorldState.Value.BundleData[key];
			}
		}
		if (CancelToTitle)
		{
			Game1.ExitToTitle();
		}
		yield return 26;
		Game1.isRaining = loaded.isRaining;
		Game1.isLightning = loaded.isLightning;
		Game1.isSnowing = loaded.isSnowing;
		Game1.isGreenRain = Utility.isGreenRainDay();
		if (Game1.IsMasterGame)
		{
			Game1.netWorldState.Value.UpdateFromGame1();
		}
		if (loaded.locationWeather != null)
		{
			Game1.netWorldState.Value.LocationWeather.Clear();
			foreach (KeyValuePair<string, LocationWeather> pair in loaded.locationWeather)
			{
				Game1.netWorldState.Value.LocationWeather[pair.Key] = pair.Value;
			}
		}
		if (loaded.builders != null)
		{
			foreach (string key in loaded.builders.Keys)
			{
				Game1.netWorldState.Value.Builders[key] = loaded.builders[key];
			}
		}
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			loadDataToFarmer(loaded.player);
		}
		else
		{
			Task deserializeTask = new Task(delegate
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				loadDataToFarmer(loaded.player);
			});
			Game1.hooks.StartTask(deserializeTask, "Load_Farmer");
			while (!deserializeTask.IsCanceled && !deserializeTask.IsCompleted && !deserializeTask.IsFaulted)
			{
				yield return 1;
			}
			if (deserializeTask.IsFaulted)
			{
				Exception e = deserializeTask.Exception.GetBaseException();
				Game1.log.Error("loadFarmerTask failed with an exception", e);
				throw e;
			}
		}
		Game1.player = loaded.player;
		Game1.player.team.useLegacyRandom.Value = loaded.useLegacyRandom;
		Game1.netWorldState.Value.farmhandData.Clear();
		if (Game1.lastAppliedSaveFix < SaveFixes.MigrateFarmhands)
		{
			SaveMigrator_1_6.MigrateFarmhands(loaded.locations);
		}
		if (loaded.farmhands != null)
		{
			foreach (Farmer farmhand in loaded.farmhands)
			{
				Game1.netWorldState.Value.farmhandData[farmhand.UniqueMultiplayerID] = farmhand;
			}
		}
		foreach (Farmer value in Game1.netWorldState.Value.farmhandData.Values)
		{
			loadDataToFarmer(value);
		}
		if (Game1.MasterPlayer.hasOrWillReceiveMail("leoMoved") && Game1.getLocationFromName("Mountain") is Mountain mountain)
		{
			mountain.reloadMap();
			mountain.ApplyTreehouseIfNecessary();
			if (mountain.treehouseDoorDirty)
			{
				mountain.treehouseDoorDirty = false;
				WarpPathfindingCache.PopulateCache();
			}
		}
		foreach (FarmerPair key in loaded.farmerFriendships.Keys)
		{
			Game1.player.team.friendshipData[key] = loaded.farmerFriendships[key];
		}
		Game1.spawnMonstersAtNight = loaded.shouldSpawnMonsters;
		Game1.player.team.limitedNutDrops.Clear();
		if (Game1.netWorldState != null && Game1.netWorldState.Value != null)
		{
			Game1.netWorldState.Value.RegisterSpecialCurrencies();
		}
		if (loaded.limitedNutDrops != null)
		{
			foreach (string key in loaded.limitedNutDrops.Keys)
			{
				if (loaded.limitedNutDrops[key] > 0)
				{
					Game1.player.team.limitedNutDrops[key] = loaded.limitedNutDrops[key];
				}
			}
		}
		Game1.player.team.completedSpecialOrders.Clear();
		Game1.player.team.completedSpecialOrders.AddRange(loaded.completedSpecialOrders);
		Game1.player.team.specialOrders.Clear();
		foreach (SpecialOrder order in loaded.specialOrders)
		{
			if (order != null)
			{
				Game1.player.team.specialOrders.Add(order);
			}
		}
		Game1.player.team.availableSpecialOrders.Clear();
		foreach (SpecialOrder order in loaded.availableSpecialOrders)
		{
			if (order != null)
			{
				Game1.player.team.availableSpecialOrders.Add(order);
			}
		}
		Game1.player.team.acceptedSpecialOrderTypes.Clear();
		Game1.player.team.acceptedSpecialOrderTypes.AddRange(loaded.acceptedSpecialOrderTypes);
		Game1.player.team.collectedNutTracker.Clear();
		Game1.player.team.collectedNutTracker.AddRange(loaded.collectedNutTracker);
		Game1.player.team.globalInventories.Clear();
		foreach (KeyValuePair<string, List<Item>> pair in loaded.globalInventories)
		{
			Game1.player.team.GetOrCreateGlobalInventory(pair.Key).AddRange(pair.Value);
		}
		List<Item> list = loaded.junimoChest;
		if (list != null && list.Count > 0)
		{
			Game1.player.team.GetOrCreateGlobalInventory("JunimoChests").AddRange(loaded.junimoChest);
		}
		Game1.player.team.returnedDonations.Clear();
		foreach (Item donated_item in loaded.returnedDonations)
		{
			Game1.player.team.returnedDonations.Add(donated_item);
		}
		if (loaded.obsolete_stats != null)
		{
			Game1.player.stats = loaded.obsolete_stats;
		}
		if (loaded.obsolete_mailbox != null && !Game1.player.mailbox.Any())
		{
			Game1.player.mailbox.AddRange(loaded.obsolete_mailbox);
		}
		Game1.random = Utility.CreateDaySaveRandom(1.0);
		Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4699");
		if (CancelToTitle)
		{
			Game1.ExitToTitle();
		}
		yield return 36;
		Game1.UpdatePassiveFestivalStates();
		if (loaded.cellarAssignments != null)
		{
			foreach (int key in loaded.cellarAssignments.Keys)
			{
				Game1.player.team.cellarAssignments[key] = loaded.cellarAssignments[key];
			}
		}
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			loadDataToLocations(loaded.locations);
		}
		else
		{
			Task deserializeTask = new Task(delegate
			{
				Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
				loadDataToLocations(loaded.locations);
			});
			Game1.hooks.StartTask(deserializeTask, "Load_Locations");
			while (!deserializeTask.IsCanceled && !deserializeTask.IsCompleted && !deserializeTask.IsFaulted)
			{
				yield return 1;
			}
			if (deserializeTask.IsFaulted)
			{
				Exception e = deserializeTask.Exception.GetBaseException();
				Game1.log.Error("loadLocationsTask failed with an exception", e);
				throw deserializeTask.Exception.GetBaseException();
			}
		}
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			int farmerMoney = farmer.Money;
			if (!Game1.player.team.individualMoney.TryGetValue(farmer.UniqueMultiplayerID, out var moneyField))
			{
				moneyField = (Game1.player.team.individualMoney[farmer.UniqueMultiplayerID] = new NetIntDelta(farmerMoney));
			}
			moneyField.Value = farmerMoney;
		}
		Game1.updateCellarAssignments();
		foreach (GameLocation location in Game1.locations)
		{
			foreach (Building building in location.buildings)
			{
				GameLocation indoors = building.GetIndoors();
				if (indoors != null)
				{
					if (indoors is FarmHouse house)
					{
						house.updateCellarWarps();
					}
					indoors.parentLocationName.Value = location.NameOrUniqueName;
				}
			}
			if (location is FarmHouse farmHouse)
			{
				farmHouse.updateCellarWarps();
			}
		}
		foreach (Farmer farmhand in Game1.netWorldState.Value.farmhandData.Values)
		{
			Game1.netWorldState.Value.ResetFarmhandState(farmhand);
		}
		if (CancelToTitle)
		{
			Game1.ExitToTitle();
		}
		yield return 50;
		yield return 51;
		Game1.isDebrisWeather = loaded.isDebrisWeather;
		if (Game1.isDebrisWeather)
		{
			Game1.populateDebrisWeatherArray();
		}
		else
		{
			Game1.debrisWeather.Clear();
		}
		yield return 53;
		Game1.player.team.sharedDailyLuck.Value = loaded.dailyLuck;
		yield return 54;
		yield return 55;
		Game1.setGraphicsForSeason(onLoad: true);
		yield return 56;
		Game1.samBandName = loaded.samBandName;
		Game1.elliottBookName = loaded.elliottBookName;
		yield return 63;
		Game1.weddingToday = loaded.weddingToday;
		Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4700");
		yield return 64;
		Game1.loadingMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:SaveGame.cs.4701");
		if (CancelToTitle)
		{
			Game1.ExitToTitle();
		}
		yield return 79;
		Game1.options.musicVolumeLevel = loaded.musicVolume;
		Game1.options.soundVolumeLevel = loaded.soundVolume;
		yield return 83;
		if (loaded.countdownToWedding.HasValue && loaded.countdownToWedding.Value != 0 && !string.IsNullOrEmpty(loaded.player.spouse))
		{
			WorldDate weddingDate = WorldDate.Now();
			weddingDate.TotalDays += loaded.countdownToWedding.Value;
			Friendship friendship = loaded.player.friendshipData[loaded.player.spouse];
			friendship.Status = FriendshipStatus.Engaged;
			friendship.WeddingDate = weddingDate;
		}
		yield return 85;
		yield return 87;
		yield return 88;
		yield return 95;
		Game1.fadeToBlack = true;
		Game1.fadeIn = false;
		Game1.fadeToBlackAlpha = 0.99f;
		if (Game1.player.mostRecentBed.X <= 0f)
		{
			Game1.player.Position = new Vector2(192f, 384f);
		}
		Game1.addNewFarmBuildingMaps();
		GameLocation last_sleep_location = null;
		if (Game1.player.lastSleepLocation.Value != null && Game1.isLocationAccessible(Game1.player.lastSleepLocation.Value))
		{
			last_sleep_location = Game1.getLocationFromName(Game1.player.lastSleepLocation);
		}
		bool apply_default_bed_position = true;
		if (last_sleep_location != null && (Game1.player.sleptInTemporaryBed.Value || last_sleep_location.GetFurnitureAt(Utility.PointToVector2(Game1.player.lastSleepPoint.Value)) is BedFurniture))
		{
			Game1.currentLocation = last_sleep_location;
			Game1.player.currentLocation = Game1.currentLocation;
			Game1.player.Position = Utility.PointToVector2(Game1.player.lastSleepPoint.Value) * 64f;
			apply_default_bed_position = false;
		}
		if (apply_default_bed_position)
		{
			Game1.currentLocation = Game1.RequireLocation("FarmHouse");
		}
		Game1.currentLocation.map.LoadTileSheets(Game1.mapDisplayDevice);
		Game1.player.CanMove = true;
		Game1.player.ReequipEnchantments();
		if (loaded.junimoKartLeaderboards != null)
		{
			Game1.player.team.junimoKartScores.LoadScores(loaded.junimoKartLeaderboards.GetScores());
		}
		Game1.options = loaded.options;
		Game1.splitscreenOptions = loaded.splitscreenOptions;
		Game1.CustomData = loaded.CustomData;
		if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.zh)
		{
			Game1.options.loadChineseFonts();
		}
		else
		{
			Game1.options.dialogueFontScale = 1f;
		}
		Game1.player.team.broadcastedMail.Clear();
		if (loaded.broadcastedMail != null)
		{
			Game1.player.team.broadcastedMail.AddRange(loaded.broadcastedMail);
		}
		Game1.player.team.constructedBuildings.Clear();
		if (loaded.constructedBuildings != null)
		{
			Game1.player.team.constructedBuildings.AddRange(loaded.constructedBuildings);
		}
		if (Game1.options == null)
		{
			Game1.options = new Options();
			Game1.options.LoadDefaultOptions();
		}
		else
		{
			Game1.options.platformClampValues();
			Game1.options.SaveDefaultOptions();
		}
		try
		{
			StartupPreferences startupPreferences = new StartupPreferences();
			startupPreferences.loadPreferences(async: false, applyLanguage: false);
			Game1.options.gamepadMode = startupPreferences.gamepadMode;
		}
		catch (Exception)
		{
		}
		Game1.initializeVolumeLevels();
		Game1.multiplayer.latestID = (ulong)loaded.latestID;
		Game1.netWorldState.Value.SkullCavesDifficulty = loaded.skullCavesDifficulty;
		Game1.netWorldState.Value.MinesDifficulty = loaded.minesDifficulty;
		Game1.netWorldState.Value.VisitsUntilY1Guarantee = loaded.visitsUntilY1Guarantee;
		Game1.netWorldState.Value.ShuffleMineChests = loaded.shuffleMineChests;
		Game1.netWorldState.Value.DishOfTheDay = loaded.dishOfTheDay;
		if (Game1.IsRainingHere())
		{
			Game1.changeMusicTrack("rain", track_interruptable: true);
		}
		Game1.updateWeatherIcon();
		Game1.netWorldState.Value.MiniShippingBinsObtained = loaded.miniShippingBinsObtained;
		Game1.netWorldState.Value.LostBooksFound = loaded.lostBooksFound;
		Game1.netWorldState.Value.GoldenWalnuts = loaded.goldenWalnuts;
		Game1.netWorldState.Value.GoldenWalnutsFound = loaded.goldenWalnutsFound;
		Game1.netWorldState.Value.GoldenCoconutCracked = loaded.goldenCoconutCracked;
		Game1.netWorldState.Value.FoundBuriedNuts.Clear();
		Game1.netWorldState.Value.FoundBuriedNuts.AddRange(loaded.foundBuriedNuts);
		Game1.netWorldState.Value.CheckedGarbage.Clear();
		Game1.netWorldState.Value.CheckedGarbage.AddRange(loaded.checkedGarbage);
		IslandSouth.SetupIslandSchedules();
		Game1.netWorldState.Value.TimesFedRaccoons = loaded.timesFedRaccoons;
		Game1.netWorldState.Value.TreasureTotemsUsed = loaded.treasureTotemsUsed;
		Game1.netWorldState.Value.PerfectionWaivers = loaded.perfectionWaivers;
		Game1.netWorldState.Value.SeasonOfCurrentRacconBundle = loaded.seasonOfCurrentRaccoonBundle;
		Game1.netWorldState.Value.raccoonBundles.Set(loaded.raccoonBundles);
		Game1.netWorldState.Value.ActivatedGoldenParrot = loaded.activatedGoldenParrot;
		Game1.netWorldState.Value.DaysPlayedWhenLastRaccoonBundleWasFinished = loaded.daysPlayedWhenLastRaccoonBundleWasFinished;
		Game1.PerformPassiveFestivalSetup();
		Game1.player.team.farmhandsCanMoveBuildings.Value = (FarmerTeam.RemoteBuildingPermissions)loaded.moveBuildingPermissionMode;
		Game1.player.team.mineShrineActivated.Value = loaded.mineShrineActivated;
		Game1.player.team.skullShrineActivated.Value = loaded.skullShrineActivated;
		if (Game1.multiplayerMode == 2)
		{
			if (Program.sdk.Networking != null && Game1.options.serverPrivacy == ServerPrivacy.InviteOnly)
			{
				Game1.options.setServerMode("invite");
			}
			else if (Program.sdk.Networking != null && Game1.options.serverPrivacy == ServerPrivacy.FriendsOnly)
			{
				Game1.options.setServerMode("friends");
			}
			else
			{
				Game1.options.setServerMode("friends");
			}
		}
		Game1.bannedUsers = loaded.bannedUsers;
		bool need_lost_book_recount = false;
		if (loaded.lostBooksFound < 0)
		{
			need_lost_book_recount = true;
		}
		loaded = null;
		Game1.currentLocation.lastTouchActionLocation = Game1.player.Tile;
		if (Game1.player.horseName.Value == null)
		{
			Horse horse = Utility.findHorse(Guid.Empty);
			if (horse != null && horse.displayName != "")
			{
				Game1.player.horseName.Value = horse.displayName;
				horse.ownerId.Value = Game1.player.UniqueMultiplayerID;
			}
		}
		SaveMigrator.ApplySaveFixes();
		if (need_lost_book_recount)
		{
			SaveMigrator_1_4.RecalculateLostBookCount();
		}
		foreach (Item item in Game1.player.Items)
		{
			(item as Object)?.reloadSprite();
		}
		foreach (Trinket trinketItem in Game1.player.trinketItems)
		{
			trinketItem.reloadSprite();
		}
		Game1.gameMode = 3;
		Game1.AddNPCs();
		Game1.AddModNPCs();
		Game1.RefreshQuestOfTheDay();
		try
		{
			Game1.fixProblems();
		}
		catch (Exception)
		{
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			LevelUpMenu.AddMissedProfessionChoices(allFarmer);
			LevelUpMenu.AddMissedLevelRecipes(allFarmer);
			allFarmer.LearnDefaultRecipes();
			LevelUpMenu.RevalidateHealth(allFarmer);
		}
		if (Game1.player.Items.ContainsId("(W)62") || Game1.player.Items.ContainsId("(W)63") || Game1.player.Items.ContainsId("(W)64"))
		{
			Game1.getAchievement(42);
		}
		Utility.ForEachBuilding(delegate(Building building)
		{
			if (building is Stable stable)
			{
				stable.grabHorse();
			}
			else
			{
				GameLocation indoors2 = building.GetIndoors();
				if (!(indoors2 is Cabin cabin))
				{
					if (indoors2 is Shed shed)
					{
						shed.updateLayout();
						building.updateInteriorWarps(shed);
					}
				}
				else
				{
					cabin.updateFarmLayout();
				}
			}
			return true;
		});
		Game1.UpdateHorseOwnership();
		Game1.UpdateFarmPerfection();
		Game1.doMorningStuff();
		if (apply_default_bed_position && Game1.player.currentLocation is FarmHouse)
		{
			Game1.player.Position = Utility.PointToVector2((Game1.player.currentLocation as FarmHouse).GetPlayerBedSpot()) * 64f;
		}
		BedFurniture.ShiftPositionForBed(Game1.player);
		Game1.stats.checkForAchievements();
		if (Game1.IsMasterGame)
		{
			Game1.netWorldState.Value.UpdateFromGame1();
		}
		Game1.log.Verbose("getLoadEnumerator() exited, elapsed = '" + stopwatch.Elapsed.ToString() + "'");
		if (CancelToTitle)
		{
			Game1.ExitToTitle();
		}
		IsProcessing = false;
		Game1.player.currentLocation.lastTouchActionLocation = Game1.player.Tile;
		Game1.player.currentLocation.resetForPlayerEntry();
		Game1.player.sleptInTemporaryBed.Value = false;
		Game1.player.showToolUpgradeAvailability();
		Game1.player.resetAllTrinketEffects();
		Game1.dayTimeMoneyBox.questsDirty = true;
		yield return 100;
	}

	public static void loadDataToFarmer(Farmer target)
	{
		target.gameVersion = target.gameVersion;
		target.Items.OverwriteWith(target.Items);
		target.canMove = true;
		target.Sprite = new FarmerSprite(null);
		target.songsHeard.Add("title_day");
		target.songsHeard.Add("title_night");
		target.maxItems.Value = target.maxItems;
		for (int i = 0; i < (int)target.maxItems; i++)
		{
			if (target.Items.Count <= i)
			{
				target.Items.Add(null);
			}
		}
		if (target.FarmerRenderer == null)
		{
			target.FarmerRenderer = new FarmerRenderer(target.getTexture(), target);
		}
		target.changeGender(target.IsMale);
		target.changeAccessory(target.accessory);
		target.changeShirt(target.shirt);
		target.changePantsColor(target.GetPantsColor());
		target.changeSkinColor(target.skin);
		target.changeHairColor(target.hairstyleColor.Value);
		target.changeHairStyle(target.hair);
		target.changeShoeColor(target.shoes);
		target.changeEyeColor(target.newEyeColor.Value);
		target.Stamina = target.Stamina;
		target.health = target.health;
		target.maxStamina.Value = target.maxStamina.Value;
		target.mostRecentBed = target.mostRecentBed;
		target.Position = target.mostRecentBed;
		target.position.X -= 64f;
		if (!Game1.hasApplied1_3_UpdateChanges)
		{
			SaveMigrator_1_3.MigrateFriendshipData(target);
		}
		target.questLog.RemoveWhere((Quest quest) => quest == null);
		target.ConvertClothingOverrideToClothesItems();
		target.UpdateClothing();
		target._lastEquippedTool = target.CurrentTool;
	}

	public static void loadDataToLocations(List<GameLocation> fromLocations)
	{
		Dictionary<string, string> formerLocationNames = GetFormerLocationNames();
		if (formerLocationNames.Count > 0)
		{
			foreach (GameLocation fromLocation2 in fromLocations)
			{
				foreach (NPC npc in fromLocation2.characters)
				{
					string curHome = npc.DefaultMap;
					if (curHome != null && formerLocationNames.TryGetValue(curHome, out var newHome))
					{
						Game1.log.Debug($"Updated {npc.Name}'s home from '{curHome}' to '{newHome}'.");
						npc.DefaultMap = newHome;
					}
				}
			}
		}
		Game1.netWorldState.Value.ParrotPlatformsUnlocked = loaded.parrotPlatformsUnlocked;
		Game1.player.team.farmPerfect.Value = loaded.farmPerfect;
		List<GameLocation> loadedLocations = new List<GameLocation>();
		foreach (GameLocation fromLocation in fromLocations)
		{
			GameLocation realLocation = Game1.getLocationFromName(fromLocation.name);
			if (realLocation == null)
			{
				if (fromLocation is Cellar)
				{
					realLocation = Game1.CreateGameLocation("Cellar");
					if (realLocation == null)
					{
						Game1.log.Error("Couldn't create 'Cellar' location. Was it removed from Data/Locations?");
						continue;
					}
					realLocation.name.Value = fromLocation.name.Value;
					Game1.locations.Add(realLocation);
				}
				if (realLocation == null && formerLocationNames.TryGetValue(fromLocation.name, out var realLocationName))
				{
					Game1.log.Debug($"Mapped legacy location '{fromLocation.Name}' to '{realLocationName}'.");
					realLocation = Game1.getLocationFromName(realLocationName);
				}
				if (realLocation == null)
				{
					Game1.log.Warn("Ignored unknown location '" + fromLocation.NameOrUniqueName + "' in save data.");
					continue;
				}
			}
			if (!(realLocation is Farm farm))
			{
				if (!(realLocation is FarmHouse farmHouse))
				{
					if (!(realLocation is Forest forest))
					{
						if (!(realLocation is MovieTheater theater))
						{
							if (!(realLocation is Town town))
							{
								if (!(realLocation is Beach beach))
								{
									if (!(realLocation is Woods woods))
									{
										if (!(realLocation is CommunityCenter communityCenter))
										{
											if (realLocation is ShopLocation shopLocation && fromLocation is ShopLocation fromShopLocation)
											{
												shopLocation.itemsFromPlayerToSell.MoveFrom(fromShopLocation.itemsFromPlayerToSell);
												shopLocation.itemsToStartSellingTomorrow.MoveFrom(fromShopLocation.itemsToStartSellingTomorrow);
											}
										}
										else if (fromLocation is CommunityCenter fromCommunityCenter)
										{
											communityCenter.areasComplete.Set(fromCommunityCenter.areasComplete);
										}
									}
									else if (fromLocation is Woods fromWoods)
									{
										woods.hasUnlockedStatue.Value = fromWoods.hasUnlockedStatue.Value;
									}
								}
								else if (fromLocation is Beach fromBeach)
								{
									beach.bridgeFixed.Value = fromBeach.bridgeFixed;
								}
							}
							else if (fromLocation is Town fromTown)
							{
								town.daysUntilCommunityUpgrade.Value = fromTown.daysUntilCommunityUpgrade;
							}
						}
						else if (fromLocation is MovieTheater fromTheater)
						{
							theater.dayFirstEntered.Set(fromTheater.dayFirstEntered);
						}
					}
					else if (fromLocation is Forest fromForest)
					{
						forest.stumpFixed.Value = fromForest.stumpFixed;
						forest.obsolete_log = fromForest.obsolete_log;
					}
				}
				else if (fromLocation is FarmHouse fromFarmHouse)
				{
					farmHouse.setMapForUpgradeLevel(farmHouse.upgradeLevel);
					farmHouse.fridge.Value = fromFarmHouse.fridge.Value;
					farmHouse.ReadWallpaperAndFloorTileData();
				}
			}
			else if (fromLocation is Farm fromFarm)
			{
				farm.greenhouseUnlocked.Value = fromFarm.greenhouseUnlocked.Value;
				farm.greenhouseMoved.Value = fromFarm.greenhouseMoved.Value;
				farm.hasSeenGrandpaNote = fromFarm.hasSeenGrandpaNote;
				farm.grandpaScore.Value = fromFarm.grandpaScore;
				farm.UpdatePatio();
			}
			realLocation.TransferDataFromSavedLocation(fromLocation);
			realLocation.animals.MoveFrom(fromLocation.animals);
			realLocation.buildings.Set(fromLocation.buildings);
			realLocation.characters.Set(fromLocation.characters);
			realLocation.furniture.Set(fromLocation.furniture);
			realLocation.largeTerrainFeatures.Set(fromLocation.largeTerrainFeatures);
			realLocation.miniJukeboxCount.Value = fromLocation.miniJukeboxCount.Value;
			realLocation.miniJukeboxTrack.Value = fromLocation.miniJukeboxTrack.Value;
			realLocation.netObjects.Set(fromLocation.netObjects.Pairs);
			realLocation.numberOfSpawnedObjectsOnMap = fromLocation.numberOfSpawnedObjectsOnMap;
			realLocation.piecesOfHay.Value = fromLocation.piecesOfHay;
			realLocation.resourceClumps.Set(new List<ResourceClump>(fromLocation.resourceClumps));
			realLocation.terrainFeatures.Set(fromLocation.terrainFeatures.Pairs);
			if (!loaded.HasSaveFix(SaveFixes.MigrateBuildingsToData))
			{
				SaveMigrator_1_6.ConvertBuildingsToData(realLocation);
			}
			loadedLocations.Add(realLocation);
		}
		MigrateVillagersByFormerName();
		foreach (GameLocation realLocation in loadedLocations)
		{
			realLocation.AddDefaultBuildings(load: false);
			foreach (Building b in realLocation.buildings)
			{
				b.load();
				if (b.GetIndoorsType() == IndoorsType.Instanced)
				{
					b.GetIndoors()?.addLightGlows();
				}
			}
			foreach (FarmAnimal value in realLocation.animals.Values)
			{
				value.reload(null);
			}
			foreach (Furniture item in realLocation.furniture)
			{
				item.updateDrawPosition();
			}
			foreach (LargeTerrainFeature largeTerrainFeature in realLocation.largeTerrainFeatures)
			{
				largeTerrainFeature.Location = realLocation;
				largeTerrainFeature.loadSprite();
			}
			foreach (TerrainFeature value2 in realLocation.terrainFeatures.Values)
			{
				value2.Location = realLocation;
				value2.loadSprite();
				if (value2 is HoeDirt hoe_dirt)
				{
					hoe_dirt.updateNeighbors();
				}
			}
			foreach (KeyValuePair<Vector2, Object> v in realLocation.objects.Pairs)
			{
				v.Value.initializeLightSource(v.Key);
				v.Value.reloadSprite();
			}
			realLocation.addLightGlows();
			if (!(realLocation is IslandLocation islandLocation))
			{
				if (realLocation is FarmCave farmCave)
				{
					farmCave.UpdateReadyFlag();
				}
			}
			else
			{
				islandLocation.AddAdditionalWalnutBushes();
			}
		}
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			if (location.characters.Count > 0)
			{
				NPC[] array = location.characters.ToArray();
				foreach (NPC obj in array)
				{
					initializeCharacter(obj, location);
					obj.reloadSprite();
				}
			}
			return true;
		});
		Game1.player.currentLocation = Utility.getHomeOfFarmer(Game1.player);
	}

	public static void initializeCharacter(NPC c, GameLocation location)
	{
		c.currentLocation = location;
		c.reloadData();
		if (!c.DefaultPosition.Equals(Vector2.Zero))
		{
			c.Position = c.DefaultPosition;
		}
	}

	/// <summary>Migrate villager NPCs based on their <see cref="F:StardewValley.GameData.Characters.CharacterData.FormerCharacterNames" /> field.</summary>
	public static void MigrateVillagersByFormerName()
	{
		Dictionary<string, string> formerNpcNames = GetFormerNpcNames((string newName, CharacterData _) => Game1.getCharacterFromName(newName) == null);
		if (formerNpcNames.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<string, string> pair in formerNpcNames)
		{
			string oldName = pair.Key;
			string newName = pair.Value;
			NPC npc = Game1.getCharacterFromName(oldName);
			if (npc == null)
			{
				continue;
			}
			npc.Name = newName;
			foreach (Farmer player in Game1.getAllFarmers())
			{
				if (player.spouse == oldName)
				{
					player.spouse = newName;
				}
				if (player.friendshipData.TryGetValue(oldName, out var friendship))
				{
					player.friendshipData.Remove(oldName);
					player.friendshipData.TryAdd(newName, friendship);
				}
				if (player.giftedItems.TryGetValue(oldName, out var giftedItems))
				{
					player.giftedItems.Remove(oldName);
					player.giftedItems.TryAdd(newName, giftedItems);
				}
			}
			Game1.log.Debug($"Migrated legacy NPC '{oldName}' in save data to '{newName}'.");
		}
	}

	/// <summary>Get a lookup of former → new location names based on their <see cref="F:StardewValley.GameData.Locations.LocationData.FormerLocationNames" /> field.</summary>
	public static Dictionary<string, string> GetFormerLocationNames()
	{
		Dictionary<string, string> formerNames = new Dictionary<string, string>();
		Dictionary<string, LocationData> locationData = DataLoader.Locations(Game1.content);
		foreach (KeyValuePair<string, LocationData> pair in locationData)
		{
			LocationData data = pair.Value;
			List<string> formerLocationNames = data.FormerLocationNames;
			if (formerLocationNames == null || formerLocationNames.Count <= 0)
			{
				continue;
			}
			foreach (string formerName in data.FormerLocationNames)
			{
				string conflictingId;
				if (locationData.ContainsKey(formerName))
				{
					Game1.log.Error($"Location '{pair.Key}' in Data/Locations has former name '{formerName}', which can't be added because there's a location with that ID in Data/Locations.");
				}
				else if (formerNames.TryGetValue(formerName, out conflictingId))
				{
					if (conflictingId != pair.Key)
					{
						Game1.log.Error($"Location '{pair.Key}' in Data/Locations has former name '{formerName}', which can't be added because that name is already mapped to '{conflictingId}'.");
					}
				}
				else
				{
					formerNames[formerName] = pair.Key;
				}
			}
		}
		return formerNames;
	}

	/// <summary>Get a lookup of former → new NPC names based on their <see cref="F:StardewValley.GameData.Characters.CharacterData.FormerCharacterNames" /> field.</summary>
	/// <param name="filter">A filter to apply to the list of NPCs with former names.</param>
	public static Dictionary<string, string> GetFormerNpcNames(Func<string, CharacterData, bool> filter)
	{
		Dictionary<string, string> formerNames = new Dictionary<string, string>();
		foreach (KeyValuePair<string, CharacterData> pair in Game1.characterData)
		{
			CharacterData data = pair.Value;
			List<string> formerCharacterNames = data.FormerCharacterNames;
			if (formerCharacterNames == null || formerCharacterNames.Count <= 0 || !filter(pair.Key, data))
			{
				continue;
			}
			foreach (string formerName in data.FormerCharacterNames)
			{
				string conflictingId;
				if (Game1.characterData.ContainsKey(formerName))
				{
					Game1.log.Error($"NPC '{pair.Key}' in Data/Characters has former name '{formerName}', which can't be added because there's an NPC with that ID in Data/Characters.");
				}
				else if (formerNames.TryGetValue(formerName, out conflictingId))
				{
					if (conflictingId != pair.Key)
					{
						Game1.log.Error($"NPC '{pair.Key}' in Data/Characters has former name '{formerName}', which can't be added because that name is already mapped to '{conflictingId}'.");
					}
				}
				else
				{
					formerNames[formerName] = pair.Key;
				}
			}
		}
		return formerNames;
	}
}
