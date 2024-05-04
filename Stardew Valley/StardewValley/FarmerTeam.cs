using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.GameData.Shops;
using StardewValley.GameData.SpecialOrders;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Minigames;
using StardewValley.Network;
using StardewValley.Network.ChestHit;
using StardewValley.Network.NetEvents;
using StardewValley.Objects;
using StardewValley.SpecialOrders;
using StardewValley.Util;

namespace StardewValley;

public class FarmerTeam : INetObject<NetFields>
{
	public enum RemoteBuildingPermissions
	{
		Off,
		OwnedBuildings,
		On
	}

	public enum SleepAnnounceModes
	{
		All,
		First,
		Off
	}

	/// <summary>The unique ID in <see cref="F:StardewValley.FarmerTeam.globalInventories" /> for Junimo chests.</summary>
	public const string GlobalInventoryId_JunimoChest = "JunimoChests";

	public readonly NetIntDelta money = new NetIntDelta(500)
	{
		Minimum = 0
	};

	public readonly NetLongDictionary<NetIntDelta, NetRef<NetIntDelta>> individualMoney = new NetLongDictionary<NetIntDelta, NetRef<NetIntDelta>>();

	public readonly NetIntDelta totalMoneyEarned = new NetIntDelta(0);

	public readonly NetBool useSeparateWallets = new NetBool();

	public readonly NetBool newLostAndFoundItems = new NetBool();

	public readonly NetBool toggleMineShrineOvernight = new NetBool();

	public readonly NetBool mineShrineActivated = new NetBool();

	public readonly NetBool toggleSkullShrineOvernight = new NetBool();

	public readonly NetBool skullShrineActivated = new NetBool();

	public readonly NetBool farmPerfect = new NetBool();

	public readonly NetList<string, NetString> specialRulesRemovedToday = new NetList<string, NetString>();

	/// <summary>The unqualified item IDs to remove everywhere in the world when the current day ends.</summary>
	public readonly NetList<string, NetString> itemsToRemoveOvernight = new NetList<string, NetString>();

	/// <summary>The mail IDs to remove from all players when the current day ends.</summary>
	public readonly NetList<string, NetString> mailToRemoveOvernight = new NetList<string, NetString>();

	public NetIntDictionary<long, NetLong> cellarAssignments = new NetIntDictionary<long, NetLong>();

	/// <summary>The mail IDs that have been broadcast globally.</summary>
	public NetStringHashSet broadcastedMail = new NetStringHashSet();

	/// <summary>The building type IDs which have been constructed.</summary>
	public readonly NetStringHashSet constructedBuildings = new NetStringHashSet();

	public NetStringHashSet collectedNutTracker = new NetStringHashSet();

	/// <summary>The special order IDs which were previously completed.</summary>
	public NetStringHashSet completedSpecialOrders = new NetStringHashSet();

	/// <summary>The special orders which are currently active.</summary>
	public NetList<SpecialOrder, NetRef<SpecialOrder>> specialOrders = new NetList<SpecialOrder, NetRef<SpecialOrder>>();

	/// <summary>The special orders which are currently available to choose from, across all special order boards.</summary>
	public NetList<SpecialOrder, NetRef<SpecialOrder>> availableSpecialOrders = new NetList<SpecialOrder, NetRef<SpecialOrder>>();

	/// <summary>The order board types for the active special order.</summary>
	public NetStringHashSet acceptedSpecialOrderTypes = new NetStringHashSet();

	public readonly NetCollection<Item> returnedDonations = new NetCollection<Item>();

	/// <summary>The synchronizer that prevents race conditions when multiplayer players hit a chest.</summary>
	internal readonly ChestHitSynchronizer chestHit = new ChestHitSynchronizer();

	/// <summary>The global inventories for special containers like Junimo chests.</summary>
	/// <remarks>
	///   <para>The vanilla keys have constants like <see cref="F:StardewValley.FarmerTeam.GlobalInventoryId_JunimoChest" />.</para>
	///
	///   <para>Most code should call <see cref="M:StardewValley.FarmerTeam.GetOrCreateGlobalInventory(System.String)" /> instead of accessing this field directly.</para>
	/// </remarks>
	public readonly NetStringDictionary<Inventory, NetRef<Inventory>> globalInventories = new NetStringDictionary<Inventory, NetRef<Inventory>>();

	/// <summary>The mutexes which prevent multiple players from opening the same global inventory at once.</summary>
	public readonly NetStringDictionary<NetMutex, NetRef<NetMutex>> globalInventoryMutexes = new NetStringDictionary<NetMutex, NetRef<NetMutex>>();

	public readonly NetFarmerCollection announcedSleepingFarmers = new NetFarmerCollection();

	public readonly NetEnum<SleepAnnounceModes> sleepAnnounceMode = new NetEnum<SleepAnnounceModes>(SleepAnnounceModes.All);

	public readonly NetEnum<RemoteBuildingPermissions> farmhandsCanMoveBuildings = new NetEnum<RemoteBuildingPermissions>(RemoteBuildingPermissions.Off);

	private readonly NetLongDictionary<Proposal, NetRef<Proposal>> proposals = new NetLongDictionary<Proposal, NetRef<Proposal>>();

	public readonly NetList<MovieInvitation, NetRef<MovieInvitation>> movieInvitations = new NetList<MovieInvitation, NetRef<MovieInvitation>>();

	public readonly NetCollection<Item> luauIngredients = new NetCollection<Item>();

	public readonly NetCollection<Item> grangeDisplay = new NetCollection<Item>();

	public readonly NetMutex grangeMutex = new NetMutex();

	public readonly NetMutex returnedDonationsMutex = new NetMutex();

	public readonly NetMutex ordersBoardMutex = new NetMutex();

	public readonly NetMutex qiChallengeBoardMutex = new NetMutex();

	private readonly NetEvent1Field<Rectangle, NetRectangle> festivalPropRemovalEvent = new NetEvent1Field<Rectangle, NetRectangle>();

	public readonly NetEvent1Field<int, NetInt> addQiGemsToTeam = new NetEvent1Field<int, NetInt>();

	public readonly NetEvent1Field<string, NetString> addCharacterEvent = new NetEvent1Field<string, NetString>();

	public readonly NetEvent1Field<string, NetString> requestAddCharacterEvent = new NetEvent1Field<string, NetString>();

	public readonly NetEvent0 requestLeoMove = new NetEvent0();

	/// <summary>An event raised when a mine area needs to kick players.</summary>
	public readonly NetEvent1Field<int, NetInt> kickOutOfMinesEvent = new NetEvent1Field<int, NetInt>();

	public readonly NetEvent1Field<string, NetString> requestNPCGoHome = new NetEvent1Field<string, NetString>
	{
		InterpolationWait = false
	};

	public readonly NetEvent1Field<long, NetLong> requestSpouseSleepEvent = new NetEvent1Field<long, NetLong>
	{
		InterpolationWait = false
	};

	public readonly NetEvent1Field<string, NetString> ringPhoneEvent = new NetEvent1Field<string, NetString>();

	public readonly NetEvent1Field<long, NetLong> requestHorseWarpEvent = new NetEvent1Field<long, NetLong>
	{
		InterpolationWait = false
	};

	public readonly NetEvent1Field<long, NetLong> requestPetWarpHomeEvent = new NetEvent1Field<long, NetLong>
	{
		InterpolationWait = false
	};

	public readonly NetEvent1Field<long, NetLong> requestMovieEndEvent = new NetEvent1Field<long, NetLong>();

	public readonly NetEvent1Field<long, NetLong> endMovieEvent = new NetEvent1Field<long, NetLong>();

	/// <summary>An event raised when a building is placed.</summary>
	public readonly NetEventBinary buildingConstructedEvent = new NetEventBinary();

	/// <summary>An event raised when a building is moved.</summary>
	public readonly NetEventBinary buildingMovedEvent = new NetEventBinary();

	/// <summary>An event raised when a building is demolished.</summary>
	public readonly NetEventBinary buildingDemolishedEvent = new NetEventBinary();

	public readonly NetStringDictionary<int, NetInt> limitedNutDrops = new NetStringDictionary<int, NetInt>();

	/// <summary>An event raised when a nut should be dropped.</summary>
	private readonly NetEvent1<NutDropRequest> requestNutDrop = new NetEvent1<NutDropRequest>();

	/// <summary>An event raised when an action needs to set a simple flag (e.g. event seen or song heard) for a group of players.</summary>
	private readonly NetEvent1<SetSimpleFlagRequest> requestSetSimpleFlag = new NetEvent1<SetSimpleFlagRequest>();

	/// <summary>An event raised to add or remove mail for a group of players.</summary>
	private readonly NetEvent1<SetMailRequest> requestSetMail = new NetEvent1<SetMailRequest>();

	public readonly NetFarmerPairDictionary<Friendship, NetRef<Friendship>> friendshipData = new NetFarmerPairDictionary<Friendship, NetRef<Friendship>>();

	public readonly NetWitnessedLock demolishLock = new NetWitnessedLock();

	public readonly NetMutex buildLock = new NetMutex();

	public readonly NetMutex movieMutex = new NetMutex();

	public readonly NetMutex goldenCoconutMutex = new NetMutex();

	public readonly SynchronizedShopStock synchronizedShopStock = new SynchronizedShopStock();

	public readonly NetLong theaterBuildDate = new NetLong(-1L);

	public readonly NetInt lastDayQueenOfSauceRerunUpdated = new NetInt(0);

	public readonly NetInt queenOfSauceRerunWeek = new NetInt(1);

	public readonly NetDouble sharedDailyLuck = new NetDouble(0.0010000000474974513);

	public readonly NetBool spawnMonstersAtNight = new NetBool(value: false);

	/// <summary>When the game makes a random choice, whether to use a simpler method that's prone to repeating patterns.</summary>
	/// <remarks>This is mainly intended for speedrunning, where full randomization might be undesirable. Most code should use <see cref="P:StardewValley.Game1.UseLegacyRandom" /> instead.</remarks>
	public readonly NetBool useLegacyRandom = new NetBool(value: false);

	public readonly NetInt calicoEggSkullCavernRating = new NetInt(0);

	/// <summary>The highest Calico Egg Rating reached by any player today.</summary>
	public readonly NetInt highestCalicoEggRatingToday = new NetInt(0);

	/// <summary>The Calico Statue effects currently applied, where the key is an effect ID like <see cref="!:DesertFestival.CALICO_STATUE_BAT_INVASION" /> and the key is the number of that effect currently applied.</summary>
	public readonly NetIntDictionary<int, NetInt> calicoStatueEffects = new NetIntDictionary<int, NetInt>();

	public readonly NetLeaderboards junimoKartScores = new NetLeaderboards();

	public PlayerStatusList junimoKartStatus = new PlayerStatusList();

	public PlayerStatusList endOfNightStatus = new PlayerStatusList();

	public PlayerStatusList festivalScoreStatus = new PlayerStatusList();

	public PlayerStatusList sleepStatus = new PlayerStatusList();

	public NetFields NetFields { get; } = new NetFields("FarmerTeam");


	public FarmerTeam()
	{
		NetFields.SetOwner(this).AddField(money, "money").AddField(totalMoneyEarned, "totalMoneyEarned")
			.AddField(proposals, "proposals")
			.AddField(luauIngredients, "luauIngredients")
			.AddField(grangeDisplay, "grangeDisplay")
			.AddField(grangeMutex.NetFields, "grangeMutex.NetFields")
			.AddField(festivalPropRemovalEvent, "festivalPropRemovalEvent")
			.AddField(friendshipData, "friendshipData")
			.AddField(demolishLock.NetFields, "demolishLock.NetFields")
			.AddField(buildLock.NetFields, "buildLock.NetFields")
			.AddField(movieInvitations, "movieInvitations")
			.AddField(movieMutex.NetFields, "movieMutex.NetFields")
			.AddField(requestMovieEndEvent, "requestMovieEndEvent")
			.AddField(endMovieEvent, "endMovieEvent")
			.AddField(requestSpouseSleepEvent, "requestSpouseSleepEvent")
			.AddField(requestNPCGoHome, "requestNPCGoHome")
			.AddField(useSeparateWallets, "useSeparateWallets")
			.AddField(individualMoney, "individualMoney")
			.AddField(announcedSleepingFarmers.NetFields, "announcedSleepingFarmers.NetFields")
			.AddField(sleepAnnounceMode, "sleepAnnounceMode")
			.AddField(theaterBuildDate, "theaterBuildDate")
			.AddField(buildingConstructedEvent, "buildingConstructedEvent")
			.AddField(buildingMovedEvent, "buildingMovedEvent")
			.AddField(buildingDemolishedEvent, "buildingDemolishedEvent")
			.AddField(queenOfSauceRerunWeek, "queenOfSauceRerunWeek")
			.AddField(lastDayQueenOfSauceRerunUpdated, "lastDayQueenOfSauceRerunUpdated")
			.AddField(broadcastedMail, "broadcastedMail")
			.AddField(constructedBuildings, "constructedBuildings")
			.AddField(sharedDailyLuck, "sharedDailyLuck")
			.AddField(spawnMonstersAtNight, "spawnMonstersAtNight")
			.AddField(useLegacyRandom, "useLegacyRandom")
			.AddField(junimoKartScores.NetFields, "junimoKartScores.NetFields")
			.AddField(cellarAssignments, "cellarAssignments")
			.AddField(synchronizedShopStock.NetFields, "synchronizedShopStock.NetFields")
			.AddField(junimoKartStatus.NetFields, "junimoKartStatus.NetFields")
			.AddField(endOfNightStatus.NetFields, "endOfNightStatus.NetFields")
			.AddField(festivalScoreStatus.NetFields, "festivalScoreStatus.NetFields")
			.AddField(sleepStatus.NetFields, "sleepStatus.NetFields")
			.AddField(farmhandsCanMoveBuildings, "farmhandsCanMoveBuildings")
			.AddField(requestPetWarpHomeEvent, "requestPetWarpHomeEvent")
			.AddField(ringPhoneEvent, "ringPhoneEvent")
			.AddField(specialOrders, "specialOrders")
			.AddField(returnedDonations, "returnedDonations")
			.AddField(returnedDonationsMutex.NetFields, "returnedDonationsMutex.NetFields")
			.AddField(goldenCoconutMutex.NetFields, "goldenCoconutMutex.NetFields")
			.AddField(requestNutDrop, "requestNutDrop")
			.AddField(requestSetSimpleFlag, "requestSetSimpleFlag")
			.AddField(requestSetMail, "requestSetMail")
			.AddField(limitedNutDrops, "limitedNutDrops")
			.AddField(availableSpecialOrders, "availableSpecialOrders")
			.AddField(acceptedSpecialOrderTypes, "acceptedSpecialOrderTypes")
			.AddField(ordersBoardMutex.NetFields, "ordersBoardMutex.NetFields")
			.AddField(qiChallengeBoardMutex.NetFields, "qiChallengeBoardMutex.NetFields")
			.AddField(completedSpecialOrders, "completedSpecialOrders")
			.AddField(addCharacterEvent, "addCharacterEvent")
			.AddField(requestAddCharacterEvent, "requestAddCharacterEvent")
			.AddField(requestLeoMove, "requestLeoMove")
			.AddField(collectedNutTracker, "collectedNutTracker")
			.AddField(itemsToRemoveOvernight, "itemsToRemoveOvernight")
			.AddField(mailToRemoveOvernight, "mailToRemoveOvernight")
			.AddField(newLostAndFoundItems, "newLostAndFoundItems")
			.AddField(globalInventories, "globalInventories")
			.AddField(globalInventoryMutexes, "globalInventoryMutexes")
			.AddField(requestHorseWarpEvent, "requestHorseWarpEvent")
			.AddField(kickOutOfMinesEvent, "kickOutOfMinesEvent")
			.AddField(toggleMineShrineOvernight, "toggleMineShrineOvernight")
			.AddField(mineShrineActivated, "mineShrineActivated")
			.AddField(toggleSkullShrineOvernight, "toggleSkullShrineOvernight")
			.AddField(skullShrineActivated, "skullShrineActivated")
			.AddField(specialRulesRemovedToday, "specialRulesRemovedToday")
			.AddField(addQiGemsToTeam, "addQiGemsToTeam")
			.AddField(farmPerfect, "farmPerfect")
			.AddField(calicoEggSkullCavernRating, "calicoEggSkullCavernRating")
			.AddField(highestCalicoEggRatingToday, "highestCalicoEggRatingToday")
			.AddField(calicoStatueEffects, "calicoStatueEffects");
		newLostAndFoundItems.Interpolated(interpolate: false, wait: false);
		junimoKartStatus.sortMode = PlayerStatusList.SortMode.NumberSortDescending;
		festivalScoreStatus.sortMode = PlayerStatusList.SortMode.NumberSortDescending;
		endOfNightStatus.displayMode = PlayerStatusList.DisplayMode.Icons;
		endOfNightStatus.AddSpriteDefinition("sleep", "LooseSprites\\PlayerStatusList", 0, 0, 16, 16);
		endOfNightStatus.AddSpriteDefinition("level", "LooseSprites\\PlayerStatusList", 16, 0, 16, 16);
		endOfNightStatus.AddSpriteDefinition("shipment", "LooseSprites\\PlayerStatusList", 32, 0, 16, 16);
		endOfNightStatus.AddSpriteDefinition("ready", "LooseSprites\\PlayerStatusList", 48, 0, 16, 16);
		endOfNightStatus.iconAnimationFrames = 4;
		festivalPropRemovalEvent.onEvent += delegate(Rectangle rect)
		{
			if (Game1.CurrentEvent != null)
			{
				Game1.CurrentEvent.removeFestivalProps(rect);
			}
		};
		toggleSkullShrineOvernight.fieldChangeEvent += delegate(NetBool field, bool oldVal, bool newVal)
		{
			if ((newVal || Game1.player.team.skullShrineActivated.Value) && Game1.currentLocation.NameOrUniqueName == "SkullCave")
			{
				Game1.currentLocation.MakeMapModifications(force: true);
			}
		};
		requestSpouseSleepEvent.onEvent += OnRequestSpouseSleepEvent;
		requestNPCGoHome.onEvent += OnRequestNPCGoHome;
		requestPetWarpHomeEvent.onEvent += OnRequestPetWarpHomeEvent;
		requestMovieEndEvent.onEvent += OnRequestMovieEndEvent;
		endMovieEvent.onEvent += OnEndMovieEvent;
		buildingConstructedEvent.AddReaderHandler(OnBuildingConstructedEvent);
		buildingMovedEvent.AddReaderHandler(OnBuildingMovedEvent);
		buildingDemolishedEvent.AddReaderHandler(OnBuildingDemolishedEvent);
		ringPhoneEvent.onEvent += OnRingPhoneEvent;
		requestNutDrop.onEvent += OnRequestNutDrop;
		requestSetSimpleFlag.onEvent += OnRequestPlayerAction;
		requestSetMail.onEvent += OnRequestPlayerAction;
		requestAddCharacterEvent.onEvent += OnRequestAddCharacterEvent;
		addCharacterEvent.onEvent += OnAddCharacterEvent;
		requestLeoMove.onEvent += OnRequestLeoMoveEvent;
		requestHorseWarpEvent.onEvent += OnRequestHorseWarp;
		calicoEggSkullCavernRating.fieldChangeEvent += OnCalicoEggRatingChanged;
		calicoStatueEffects.OnValueAdded += delegate(int key, int _)
		{
			OnCalicoStatueEffectAdded(key);
		};
		calicoStatueEffects.OnValueTargetUpdated += delegate(int key, int oldValue, int newValue)
		{
			OnCalicoStatueEffectAdded(key);
		};
		kickOutOfMinesEvent.onEvent += OnKickOutOfMinesEvent;
		addQiGemsToTeam.onEvent += _AddQiGemsToTeam;
		constructedBuildings.OnValueAdded += delegate(string addedValue)
		{
			if (Game1.hasStartedDay)
			{
				Game1.player.checkForQuestComplete(null, -1, -1, null, addedValue, 8);
			}
		};
	}

	public void AddCalicoStatueEffect(int effectId)
	{
		if (!calicoStatueEffects.TryAdd(effectId, 1))
		{
			calicoStatueEffects[effectId]++;
		}
	}

	private void OnCalicoStatueEffectAdded(int key)
	{
		switch (key)
		{
		case 16:
			if (!Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)CalicoEgg", 10)))
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)CalicoEgg", 10), Game1.player.getStandingPosition(), 0, Game1.player.currentLocation);
			}
			break;
		case 15:
			if (!Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)CalicoEgg", 25)))
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)CalicoEgg", 25), Game1.player.getStandingPosition(), 0, Game1.player.currentLocation);
			}
			break;
		case 12:
			if (!Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)CalicoEgg", 50)))
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)CalicoEgg", 50), Game1.player.getStandingPosition(), 0, Game1.player.currentLocation);
			}
			break;
		case 17:
			if (!Game1.player.addItemToInventoryBool(ItemRegistry.Create("(O)CalicoEgg", 100)))
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)CalicoEgg", 100), Game1.player.getStandingPosition(), 0, Game1.player.currentLocation);
			}
			break;
		case 11:
			Game1.player.health = Game1.player.maxHealth;
			Game1.player.stamina = (int)Game1.player.maxStamina;
			break;
		case 10:
			if (Game1.player.currentLocation is MineShaft && Game1.mine.getMineArea() == 121)
			{
				DesertFestival.addCalicoStatueSpeedBuff();
			}
			break;
		}
		if (!(Game1.currentLocation is MineShaft) || Game1.mine.getMineArea() != 121)
		{
			return;
		}
		string description = Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_CalicoStatue_Description_" + key);
		Point newVector = Game1.mine.calicoStatueSpot.Value;
		foreach (Vector2 v in Utility.getAdjacentTileLocations(Vector2.Zero))
		{
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(null, Rectangle.Empty, new Vector2((float)(newVector.X * 64 + 32) - (float)SpriteText.getWidthOfString(description) / 2f, (newVector.Y - 3) * 64) + v * 4f, flipped: false, 0f, Color.Black)
			{
				text = description,
				extraInfoForEndBehavior = -777,
				layerDepth = 0.99f,
				motion = new Vector2(0f, -1f),
				yStopCoordinate = (newVector.Y - 4) * 64,
				animationLength = 1,
				delayBeforeAnimationStart = 500,
				totalNumberOfLoops = 10,
				interval = 300f,
				drawAboveAlwaysFront = true
			});
		}
		Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(null, Rectangle.Empty, new Vector2((float)(newVector.X * 64 + 32) - (float)SpriteText.getWidthOfString(description) / 2f, (newVector.Y - 3) * 64), flipped: false, 0f, Color.White)
		{
			text = description,
			extraInfoForEndBehavior = -777,
			layerDepth = 1f,
			motion = new Vector2(0f, -1f),
			yStopCoordinate = (newVector.Y - 4) * 64,
			animationLength = 1,
			delayBeforeAnimationStart = 500,
			totalNumberOfLoops = 10,
			interval = 300f,
			drawAboveAlwaysFront = true
		});
	}

	private void OnCalicoEggRatingChanged(NetInt field, int oldValue, int newValue)
	{
		if (newValue > oldValue && Game1.currentLocation is MineShaft)
		{
			if (Game1.mine != null)
			{
				Game1.mine.calicoEggIconTimerShake = 1500f;
			}
			DelayedAction.playSoundAfterDelay("yoba", 800);
		}
		if (Game1.IsMasterGame && Game1.hasStartedDay && newValue > Game1.player.team.highestCalicoEggRatingToday.Value)
		{
			Game1.player.team.highestCalicoEggRatingToday.Value = newValue;
		}
	}

	protected virtual void _AddQiGemsToTeam(int amount)
	{
		Game1.player.QiGems += amount;
	}

	/// <summary>Kick the player out of a mine area.</summary>
	/// <param name="mineshaftType">The type of mine from which to kick players, or <see cref="F:StardewValley.Locations.MineShaft.bottomOfMineLevel" /> for any area in the regular mines.</param>
	public virtual void OnKickOutOfMinesEvent(int mineshaftType)
	{
		if (Game1.currentLocation is MineShaft mineshaft && ((mineshaftType == 120) ? (mineshaft.mineLevel <= mineshaftType) : (mineshaft.getMineArea() == mineshaftType)))
		{
			switch (mineshaftType)
			{
			case 77377:
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.warpFarmer(Game1.getLocationRequest("Mine"), 67, 10, 2);
				break;
			case 121:
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.warpFarmer(Game1.getLocationRequest("SkullCave"), 3, 4, 2);
				break;
			default:
				Game1.player.completelyStopAnimatingOrDoingAction();
				Game1.warpFarmer(Game1.getLocationRequest("Mine"), 18, 4, 2);
				break;
			}
		}
	}

	public virtual void OnRequestHorseWarp(long uid)
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		Farmer farmer = Game1.getFarmer(uid);
		Horse horse = null;
		Utility.ForEachBuilding(delegate(Stable stable)
		{
			Horse stableHorse = stable.getStableHorse();
			if (stableHorse != null && stableHorse.getOwner() == farmer)
			{
				horse = stableHorse;
				return false;
			}
			return true;
		});
		if (horse == null || Utility.GetHorseWarpRestrictionsForFarmer(farmer) != 0)
		{
			return;
		}
		horse.mutex.RequestLock(delegate
		{
			horse.mutex.ReleaseLock();
			GameLocation currentLocation = horse.currentLocation;
			Vector2 tile = horse.Tile;
			for (int i = 0; i < 8; i++)
			{
				Game1.multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite(10, new Vector2(tile.X + Utility.RandomFloat(-1f, 1f), tile.Y + Utility.RandomFloat(-1f, 0f)) * 64f, Color.White, 8, flipped: false, 50f)
				{
					layerDepth = 1f,
					motion = new Vector2(Utility.RandomFloat(-0.5f, 0.5f), Utility.RandomFloat(-0.5f, 0.5f))
				});
			}
			currentLocation.playSound("wand", horse.Tile);
			currentLocation = farmer.currentLocation;
			tile = farmer.Tile;
			currentLocation.playSound("wand", tile);
			for (int j = 0; j < 8; j++)
			{
				Game1.multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite(10, new Vector2(tile.X + Utility.RandomFloat(-1f, 1f), tile.Y + Utility.RandomFloat(-1f, 0f)) * 64f, Color.White, 8, flipped: false, 50f)
				{
					layerDepth = 1f,
					motion = new Vector2(Utility.RandomFloat(-0.5f, 0.5f), Utility.RandomFloat(-0.5f, 0.5f))
				});
			}
			Game1.warpCharacter(horse, farmer.currentLocation, tile);
			int num = 0;
			for (int num2 = (int)tile.X + 3; num2 >= (int)tile.X - 3; num2--)
			{
				Game1.multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite(6, new Vector2(num2, tile.Y) * 64f, Color.White, 8, flipped: false, 50f)
				{
					layerDepth = 1f,
					delayBeforeAnimationStart = num * 25,
					motion = new Vector2(-0.25f, 0f)
				});
				num++;
			}
		});
	}

	public virtual void OnRequestLeoMoveEvent()
	{
		if (Game1.IsMasterGame)
		{
			Game1.player.team.requestAddCharacterEvent.Fire("Leo");
			NPC leo = Game1.getCharacterFromName("Leo");
			if (leo != null)
			{
				leo.reloadDefaultLocation();
				leo.faceDirection(2);
				leo.InvalidateMasterSchedule();
				leo.ClearSchedule();
				leo.controller = null;
				leo.temporaryController = null;
				Game1.warpCharacter(leo, Game1.RequireLocation("Mountain"), new Vector2(16f, 8f));
				leo.Halt();
				leo.ignoreScheduleToday = false;
			}
		}
	}

	public virtual void MarkCollectedNut(string key)
	{
		collectedNutTracker.Add(key);
	}

	public int GetIndividualMoney(Farmer who)
	{
		return GetMoney(who).Value;
	}

	public void AddIndividualMoney(Farmer who, int value)
	{
		GetMoney(who).Value += value;
	}

	public void SetIndividualMoney(Farmer who, int value)
	{
		GetMoney(who).Value = value;
	}

	public NetIntDelta GetMoney(Farmer who)
	{
		if ((bool)useSeparateWallets)
		{
			if (!individualMoney.TryGetValue(who.UniqueMultiplayerID, out var value))
			{
				NetLongDictionary<NetIntDelta, NetRef<NetIntDelta>> netLongDictionary = individualMoney;
				long uniqueMultiplayerID = who.UniqueMultiplayerID;
				NetIntDelta obj = new NetIntDelta(500)
				{
					Minimum = 0
				};
				value = obj;
				netLongDictionary[uniqueMultiplayerID] = obj;
			}
			return value;
		}
		return money;
	}

	public bool SpecialOrderActive(string special_order_key)
	{
		foreach (SpecialOrder order in specialOrders)
		{
			if (order.questKey == special_order_key && order.questState.Value == SpecialOrderStatus.InProgress)
			{
				return true;
			}
		}
		return false;
	}

	public bool SpecialOrderRuleActive(string special_rule, SpecialOrder order_to_ignore = null)
	{
		foreach (SpecialOrder order in specialOrders)
		{
			if (order == order_to_ignore || order.questState.Value != 0 || order.specialRule.Value == null)
			{
				continue;
			}
			string[] array = order.specialRule.Value.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Trim() == special_rule)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>Add a special order to the player team if it's not already active, or log a warning if it doesn't exist.</summary>
	/// <param name="id">The special order ID in <c>Data/SpecialOrders</c>.</param>
	/// <param name="generationSeed">The seed to use for randomizing the special order, or <c>null</c> for a random seed.</param>
	/// <param name="forceRepeatable">Whether to consider the special order repeatable regardless of <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderData.Repeatable" />.</param>
	public void AddSpecialOrder(string id, int? generationSeed = null, bool forceRepeatable = false)
	{
		if (specialOrders.Any((SpecialOrder p) => p.questKey == id))
		{
			return;
		}
		SpecialOrder order = SpecialOrder.GetSpecialOrder(id, generationSeed);
		if (order == null)
		{
			Game1.log.Warn("Can't add special order with ID '" + id + "' because no such ID was found.");
			return;
		}
		if (completedSpecialOrders.Contains(order.questKey.Value) && !forceRepeatable)
		{
			SpecialOrderData data = order.GetData();
			if (data == null || !data.Repeatable)
			{
				return;
			}
		}
		specialOrders.Add(order);
	}

	public SpecialOrder GetAvailableSpecialOrder(int index = 0, string type = "")
	{
		foreach (SpecialOrder order in availableSpecialOrders)
		{
			if (order.orderType.Value == type)
			{
				if (index <= 0)
				{
					return order;
				}
				index--;
			}
		}
		return null;
	}

	public void CheckReturnedDonations()
	{
		returnedDonationsMutex.RequestLock(delegate
		{
			returnedDonations.RemoveWhere((Item item) => item == null);
			Dictionary<ISalable, ItemStockInformation> dictionary = new Dictionary<ISalable, ItemStockInformation>();
			foreach (Item current in returnedDonations)
			{
				dictionary[current] = new ItemStockInformation(0, 1, null, null, LimitedStockMode.None);
			}
			Game1.activeClickableMenu = new ShopMenu("ReturnedDonations", dictionary, 0, null, OnDonatedItemWithdrawn, OnReturnedDonationDeposited)
			{
				source = this,
				behaviorBeforeCleanup = delegate
				{
					returnedDonationsMutex.ReleaseLock();
				}
			};
		});
	}

	public bool OnDonatedItemWithdrawn(ISalable salable, Farmer who, int amount)
	{
		if (salable is Item item && (salable.Stack <= 0 || salable.maximumStackSize() <= 1))
		{
			returnedDonations.Remove(item);
		}
		return false;
	}

	public bool OnReturnedDonationDeposited(ISalable deposited_salable)
	{
		return false;
	}

	public void OnRequestMovieEndEvent(long uid)
	{
		if (Game1.IsMasterGame)
		{
			Game1.RequireLocation<MovieTheater>("MovieTheater").RequestEndMovie(uid);
		}
	}

	public void OnRequestPetWarpHomeEvent(long uid)
	{
		if (Game1.IsMasterGame)
		{
			Farmer farmer = Game1.getFarmerMaybeOffline(uid);
			if (farmer == null)
			{
				farmer = Game1.MasterPlayer;
			}
			Pet pet = Game1.getCharacterFromName<Pet>(farmer.getPetName(), mustBeVillager: false);
			if (!(pet?.currentLocation is FarmHouse))
			{
				pet?.warpToFarmHouse(farmer);
			}
		}
	}

	public void OnRequestNPCGoHome(string npc_name)
	{
		if (Game1.IsMasterGame)
		{
			NPC npc = Game1.getCharacterFromName(npc_name);
			if (npc.defaultMap != null)
			{
				npc.doingEndOfRouteAnimation.Value = false;
				npc.nextEndOfRouteMessage = null;
				npc.endOfRouteMessage.Value = null;
				npc.controller = null;
				npc.temporaryController = null;
				npc.Halt();
				Game1.warpCharacter(npc, npc.defaultMap, npc.DefaultPosition / 64f);
				npc.ignoreScheduleToday = true;
			}
		}
	}

	public void OnRequestSpouseSleepEvent(long uid)
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		Farmer farmer = Game1.getFarmerMaybeOffline(uid);
		if (farmer == null)
		{
			return;
		}
		NPC spouse = Game1.getCharacterFromName(farmer.spouse);
		if (spouse != null && !spouse.isSleeping.Value)
		{
			FarmHouse farm_house = Utility.getHomeOfFarmer(farmer);
			Game1.warpCharacter(spouse, farm_house, new Vector2(farm_house.getSpouseBedSpot(farmer.spouse).X, farm_house.getSpouseBedSpot(farmer.spouse).Y));
			spouse.NetFields.CancelInterpolation();
			spouse.Halt();
			spouse.faceDirection(0);
			spouse.controller = null;
			spouse.temporaryController = null;
			spouse.ignoreScheduleToday = true;
			if (farm_house.GetSpouseBed() != null)
			{
				FarmHouse.spouseSleepEndFunction(spouse, farm_house);
			}
		}
	}

	public virtual void OnRequestAddCharacterEvent(string character_name)
	{
		if (Game1.IsMasterGame && Game1.AddCharacterIfNecessary(character_name))
		{
			addCharacterEvent.Fire(character_name);
		}
	}

	public virtual void OnAddCharacterEvent(string character_name)
	{
		if (!Game1.IsMasterGame)
		{
			Game1.AddCharacterIfNecessary(character_name, bypassConditions: true);
		}
	}

	/// <summary>Request a nut drop that only happens a set number of times.</summary>
	/// <param name="key">The key for the limited pool of nut drops.</param>
	/// <param name="location">The location where the nut will be dropped.</param>
	/// <param name="x">The x component of the coordinate where we will drop the nut in <paramref name="location" />.</param>
	/// <param name="y">The y component of the coordinate where we will drop the nut in <paramref name="location" />.</param>
	/// <param name="limit">The max amount of nuts that should be dropped from the pool specified by <paramref name="key" />.</param>
	/// <param name="rewardAmount">The amount of nuts that should be dropped. Defaults to 1.</param>
	public void RequestLimitedNutDrops(string key, GameLocation location, int x, int y, int limit, int rewardAmount = 1)
	{
		if (!limitedNutDrops.TryGetValue(key, out var count) || count < limit)
		{
			requestNutDrop.Fire(new NutDropRequest(key, location?.NameOrUniqueName, new Point(x, y), limit, rewardAmount));
		}
	}

	public int GetDroppedLimitedNutCount(string key)
	{
		if (!limitedNutDrops.TryGetValue(key, out var count))
		{
			return 0;
		}
		return count;
	}

	protected void OnRequestNutDrop(NutDropRequest request)
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		int count = GetDroppedLimitedNutCount(request.Key);
		if (count >= request.Limit)
		{
			return;
		}
		int award_amount = request.RewardAmount;
		award_amount = Math.Min(request.Limit - count, award_amount);
		limitedNutDrops[request.Key] = count + award_amount;
		GameLocation location = null;
		if (request.LocationName != "null")
		{
			location = Game1.getLocationFromName(request.LocationName);
		}
		if (location != null)
		{
			for (int i = 0; i < award_amount; i++)
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)73"), new Vector2(request.Tile.X, request.Tile.Y), -1, location);
			}
		}
		else
		{
			Game1.netWorldState.Value.GoldenWalnutsFound += award_amount;
			Game1.netWorldState.Value.GoldenWalnuts += award_amount;
		}
	}

	/// <summary>Sends a request to set or unset a simple flag for a group of players.</summary>
	/// <param name="flag">The flag type to set for the players.</param>
	/// <param name="target">The players for which to perform the action.</param>
	/// <param name="flagId">The flag ID to update.</param>
	/// <param name="flagState">The flag state to set.</param>
	/// <param name="onlyPlayerId">The specific player ID to apply this event to, or <c>null</c> to apply it to all players matching <paramref name="target" />.</param>
	public void RequestSetSimpleFlag(SimpleFlagType flag, PlayerActionTarget target, string flagId, bool flagState, long? onlyPlayerId = null)
	{
		RequestPlayerAction(new SetSimpleFlagRequest(flag, target, flagId, flagState, onlyPlayerId), requestSetSimpleFlag);
	}

	/// <summary>Sends a request to add mail for a group of players.</summary>
	/// <param name="playerTarget">The players to update.</param>
	/// <param name="mailId">The mail to add.</param>
	/// <param name="mailType">When the mail should be received.</param>
	/// <param name="add">Whether to add the mail; else it'll be removed.</param>
	/// <param name="onlyPlayerId">The specific player ID to apply this event to, or <c>null</c> to apply it to all players matching <paramref name="playerTarget" />.</param>
	public void RequestSetMail(PlayerActionTarget playerTarget, string mailId, MailType mailType, bool add, long? onlyPlayerId = null)
	{
		RequestPlayerAction(new SetMailRequest(playerTarget, mailId, mailType, add, onlyPlayerId), requestSetMail);
	}

	public void OnRingPhoneEvent(string callId)
	{
		Phone.Ring(callId);
	}

	public void OnEndMovieEvent(long uid)
	{
		if (Game1.player.UniqueMultiplayerID != uid)
		{
			return;
		}
		Game1.player.lastSeenMovieWeek.Set(Game1.Date.TotalWeeks);
		if (Game1.CurrentEvent != null)
		{
			Event currentEvent = Game1.CurrentEvent;
			currentEvent.onEventFinished = (Action)Delegate.Combine(currentEvent.onEventFinished, (Action)delegate
			{
				Game1.warpFarmer(Game1.getLocationRequest("MovieTheater"), 13, 4, 2);
				Game1.fadeToBlackAlpha = 1f;
			});
			Game1.CurrentEvent.endBehaviors();
		}
	}

	/// <summary>Notify all players that a building has been constructed.</summary>
	/// <param name="location">The location containing the building.</param>
	/// <param name="building">The building that was constructed.</param>
	/// <param name="who">The player that constructed the building.</param>
	/// <remarks>This is received via <see cref="M:StardewValley.FarmerTeam.OnBuildingConstructedEvent(System.IO.BinaryReader)" /> on all players, including the one who sent it.</remarks>
	public void SendBuildingConstructedEvent(GameLocation location, Building building, Farmer who)
	{
		buildingConstructedEvent.Fire(delegate(BinaryWriter writer)
		{
			writer.Write(location.NameOrUniqueName);
			writer.WriteGuid(building.id.Value);
			writer.Write(who.UniqueMultiplayerID);
		});
	}

	/// <summary>Receive an event indicating that a building has been constructed.</summary>
	/// <param name="reader">The event argument reader.</param>
	/// <remarks>This receives an event sent via <see cref="M:StardewValley.FarmerTeam.SendBuildingConstructedEvent(StardewValley.GameLocation,StardewValley.Buildings.Building,StardewValley.Farmer)" />.</remarks>
	public void OnBuildingConstructedEvent(BinaryReader reader)
	{
		string name = reader.ReadString();
		Guid buildingId = reader.ReadGuid();
		long farmerId = reader.ReadInt64();
		GameLocation location = Game1.getLocationFromName(name);
		Building building = location?.getBuildingById(buildingId);
		Farmer who = Game1.getFarmer(farmerId);
		if (building != null)
		{
			location.OnBuildingConstructed(building, who);
		}
	}

	/// <summary>Notify all players that a building has been moved.</summary>
	/// <param name="location">The location containing the building.</param>
	/// <param name="building">The building that was moved.</param>
	/// <remarks>This is received via <see cref="M:StardewValley.FarmerTeam.OnBuildingMovedEvent(System.IO.BinaryReader)" /> on all players, including the one who sent it.</remarks>
	public void SendBuildingMovedEvent(GameLocation location, Building building)
	{
		buildingMovedEvent.Fire(delegate(BinaryWriter writer)
		{
			writer.Write(location.NameOrUniqueName);
			writer.WriteGuid(building.id.Value);
		});
	}

	/// <summary>Receive an event indicating that a building has been moved.</summary>
	/// <param name="reader">The event argument reader.</param>
	/// <remarks>This receives an event sent via <see cref="M:StardewValley.FarmerTeam.SendBuildingMovedEvent(StardewValley.GameLocation,StardewValley.Buildings.Building)" />.</remarks>
	public void OnBuildingMovedEvent(BinaryReader reader)
	{
		string name = reader.ReadString();
		Guid buildingId = reader.ReadGuid();
		GameLocation location = Game1.getLocationFromName(name);
		Building building = location?.getBuildingById(buildingId);
		if (building != null)
		{
			location.OnBuildingMoved(building);
		}
	}

	/// <summary>Notify all players that a building has been demolished.</summary>
	/// <param name="location">The location containing the building.</param>
	/// <param name="building">The building that was demolished.</param>
	/// <remarks>This is received via <see cref="M:StardewValley.FarmerTeam.OnBuildingDemolishedEvent(System.IO.BinaryReader)" /> on all players, including the one who sent it.</remarks>
	public void SendBuildingDemolishedEvent(GameLocation location, Building building)
	{
		buildingDemolishedEvent.Fire(delegate(BinaryWriter writer)
		{
			writer.Write(location.NameOrUniqueName);
			writer.Write(building.buildingType.Value);
			writer.WriteGuid(building.id.Value);
		});
	}

	/// <summary>Receive an event indicating that a building has been demolished.</summary>
	/// <param name="reader">The event argument reader.</param>
	/// <remarks>This receives an event sent via <see cref="M:StardewValley.FarmerTeam.SendBuildingDemolishedEvent(StardewValley.GameLocation,StardewValley.Buildings.Building)" />.</remarks>
	public void OnBuildingDemolishedEvent(BinaryReader reader)
	{
		string name = reader.ReadString();
		string buildingType = reader.ReadString();
		Guid buildingId = reader.ReadGuid();
		Game1.getLocationFromName(name).OnBuildingDemolished(buildingType, buildingId);
	}

	/// <summary>Fully remove a farmhand player from the save. This will permanently remove their data if the game is saved.</summary>
	/// <param name="farmhand">The player to delete.</param>
	public void DeleteFarmhand(Farmer farmhand)
	{
		friendshipData.RemoveWhere((KeyValuePair<FarmerPair, Friendship> pair) => pair.Key.Contains(farmhand.UniqueMultiplayerID));
		Game1.netWorldState.Value.farmhandData.Remove(farmhand.UniqueMultiplayerID);
	}

	public Friendship GetFriendship(long farmer1, long farmer2)
	{
		FarmerPair pair = FarmerPair.MakePair(farmer1, farmer2);
		if (!friendshipData.ContainsKey(pair))
		{
			friendshipData.Add(pair, new Friendship());
		}
		return friendshipData[pair];
	}

	public void AddAnyBroadcastedMail()
	{
		foreach (string item in broadcastedMail)
		{
			Multiplayer.PartyWideMessageQueue mail_queue = Multiplayer.PartyWideMessageQueue.SeenMail;
			string mail_key = item;
			if (mail_key.StartsWith("%&SM&%"))
			{
				mail_key = mail_key.Substring("%&SM&%".Length);
				mail_queue = Multiplayer.PartyWideMessageQueue.SeenMail;
			}
			else if (mail_key.StartsWith("%&MFT&%"))
			{
				mail_key = mail_key.Substring("%&MFT&%".Length);
				mail_queue = Multiplayer.PartyWideMessageQueue.MailForTomorrow;
			}
			if (mail_queue == Multiplayer.PartyWideMessageQueue.SeenMail)
			{
				if (mail_key.Contains("%&NL&%") || mail_key.StartsWith("NightMarketYear"))
				{
					mail_key = mail_key.Replace("%&NL&%", "");
					Game1.player.mailReceived.Add(mail_key);
				}
				else if (!Game1.player.hasOrWillReceiveMail(mail_key))
				{
					Game1.player.mailbox.Add(mail_key);
				}
			}
			else if (!Game1.MasterPlayer.mailForTomorrow.Contains(mail_key))
			{
				if (!Game1.player.hasOrWillReceiveMail(mail_key))
				{
					if (mail_key.Contains("%&NL&%"))
					{
						string stripped = mail_key.Replace("%&NL&%", "");
						Game1.player.mailReceived.Add(stripped);
					}
					else if (!Game1.player.mailbox.Contains(mail_key))
					{
						Game1.player.mailbox.Add(mail_key);
					}
				}
			}
			else if (!Game1.player.hasOrWillReceiveMail(mail_key))
			{
				Game1.player.mailForTomorrow.Add(mail_key);
			}
		}
	}

	public bool IsMarried(long farmer)
	{
		foreach (KeyValuePair<FarmerPair, Friendship> kvpair in friendshipData.Pairs)
		{
			if (kvpair.Key.Contains(farmer) && kvpair.Value.IsMarried())
			{
				return true;
			}
		}
		return false;
	}

	public bool IsEngaged(long farmer)
	{
		foreach (KeyValuePair<FarmerPair, Friendship> kvpair in friendshipData.Pairs)
		{
			if (kvpair.Key.Contains(farmer) && kvpair.Value.IsEngaged())
			{
				return true;
			}
		}
		return false;
	}

	public long? GetSpouse(long farmer)
	{
		foreach (KeyValuePair<FarmerPair, Friendship> kvpair in friendshipData.Pairs)
		{
			if (kvpair.Key.Contains(farmer) && (kvpair.Value.IsEngaged() || kvpair.Value.IsMarried()))
			{
				return kvpair.Key.GetOther(farmer);
			}
		}
		return null;
	}

	public void FestivalPropsRemoved(Rectangle rect)
	{
		festivalPropRemovalEvent.Fire(rect);
	}

	public void SendProposal(Farmer receiver, ProposalType proposalType, Item gift = null)
	{
		Proposal proposal = new Proposal();
		proposal.sender.Value = Game1.player;
		proposal.receiver.Value = receiver;
		proposal.proposalType.Value = proposalType;
		proposal.gift.Value = gift;
		proposals[Game1.player.UniqueMultiplayerID] = proposal;
	}

	public Proposal GetOutgoingProposal()
	{
		if (proposals.TryGetValue(Game1.player.UniqueMultiplayerID, out var proposal))
		{
			return proposal;
		}
		return null;
	}

	public void RemoveOutgoingProposal()
	{
		proposals.Remove(Game1.player.UniqueMultiplayerID);
	}

	public Proposal GetIncomingProposal()
	{
		foreach (Proposal proposal in proposals.Values)
		{
			if (proposal.receiver.Value == Game1.player && proposal.response.Value == ProposalResponse.None)
			{
				return proposal;
			}
		}
		return null;
	}

	private bool locationsMatch(GameLocation location1, GameLocation location2)
	{
		if (location1 == null || location2 == null)
		{
			return false;
		}
		if (location1.Name == location2.Name)
		{
			return true;
		}
		if ((location1 is Mine || (MineShaft.IsGeneratedLevel(location1, out var mineLevel) && mineLevel < 121)) && (location2 is Mine || (MineShaft.IsGeneratedLevel(location2, out mineLevel) && mineLevel < 121)))
		{
			return true;
		}
		if ((location1.Name.Equals("SkullCave") || (MineShaft.IsGeneratedLevel(location1, out mineLevel) && mineLevel >= 121)) && (location2.Name.Equals("SkullCave") || (MineShaft.IsGeneratedLevel(location2, out mineLevel) && mineLevel >= 121)))
		{
			return true;
		}
		return false;
	}

	public double AverageDailyLuck(GameLocation inThisLocation = null)
	{
		double sum = 0.0;
		int count = 0;
		foreach (Farmer farmer in Game1.getOnlineFarmers())
		{
			if (inThisLocation == null || locationsMatch(inThisLocation, farmer.currentLocation))
			{
				sum += farmer.DailyLuck;
				count++;
			}
		}
		return sum / (double)Math.Max(count, 1);
	}

	public double AverageLuckLevel(GameLocation inThisLocation = null)
	{
		double sum = 0.0;
		int count = 0;
		foreach (Farmer farmer in Game1.getOnlineFarmers())
		{
			if (inThisLocation == null || locationsMatch(inThisLocation, farmer.currentLocation))
			{
				sum += (double)farmer.LuckLevel;
				count++;
			}
		}
		return sum / (double)Math.Max(count, 1);
	}

	public double AverageSkillLevel(int skillIndex, GameLocation inThisLocation = null)
	{
		double sum = 0.0;
		int count = 0;
		foreach (Farmer farmer in Game1.getOnlineFarmers())
		{
			if (inThisLocation == null || locationsMatch(inThisLocation, farmer.currentLocation))
			{
				sum += (double)farmer.GetSkillLevel(skillIndex);
				count++;
			}
		}
		return sum / (double)Math.Max(count, 1);
	}

	public void Update()
	{
		requestLeoMove.Poll();
		requestMovieEndEvent.Poll();
		endMovieEvent.Poll();
		ringPhoneEvent.Poll();
		festivalPropRemovalEvent.Poll();
		buildingConstructedEvent.Poll();
		buildingMovedEvent.Poll();
		buildingDemolishedEvent.Poll();
		requestSpouseSleepEvent.Poll();
		requestNPCGoHome.Poll();
		requestHorseWarpEvent.Poll();
		kickOutOfMinesEvent.Poll();
		requestPetWarpHomeEvent.Poll();
		requestNutDrop.Poll();
		requestSetSimpleFlag.Poll();
		requestSetMail.Poll();
		requestAddCharacterEvent.Poll();
		addCharacterEvent.Poll();
		addQiGemsToTeam.Poll();
		grangeMutex.Update(Game1.getOnlineFarmers());
		returnedDonationsMutex.Update(Game1.getOnlineFarmers());
		ordersBoardMutex.Update(Game1.getOnlineFarmers());
		qiChallengeBoardMutex.Update(Game1.getOnlineFarmers());
		chestHit.Update();
		foreach (NetMutex value in globalInventoryMutexes.Values)
		{
			value.Update(Game1.getOnlineFarmers());
		}
		demolishLock.Update();
		buildLock.Update(Game1.getOnlineFarmers());
		movieMutex.Update(Game1.getOnlineFarmers());
		goldenCoconutMutex.Update(Game1.getOnlineFarmers());
		if (grangeMutex.IsLockHeld() && Game1.activeClickableMenu == null)
		{
			grangeMutex.ReleaseLock();
		}
		foreach (SpecialOrder specialOrder in specialOrders)
		{
			specialOrder.Update();
		}
		Game1.netReady.Update();
		if (Game1.IsMasterGame && proposals.Length > 0)
		{
			proposals.RemoveWhere((KeyValuePair<long, Proposal> pair) => !playerIsOnline(pair.Key) || !playerIsOnline(pair.Value.receiver.UID));
		}
		Proposal proposal = GetIncomingProposal();
		if (proposal != null && proposal.canceled.Value)
		{
			proposal.cancelConfirmed.Value = true;
		}
		if (Game1.dialogueUp)
		{
			return;
		}
		if (proposal != null)
		{
			if (!handleIncomingProposal(proposal))
			{
				proposal.responseMessageKey.Value = genderedKey("Strings\\UI:Proposal_PlayerBusy", Game1.player);
				proposal.response.Value = ProposalResponse.Rejected;
			}
		}
		else if (Game1.activeClickableMenu == null && GetOutgoingProposal() != null)
		{
			Game1.activeClickableMenu = new PendingProposalDialog();
		}
	}

	private string genderedKey(string baseKey, Farmer farmer)
	{
		return baseKey + (farmer.IsMale ? "_Male" : "_Female");
	}

	private bool handleIncomingProposal(Proposal proposal)
	{
		if (Game1.gameMode != 3 || Game1.activeClickableMenu != null || Game1.currentMinigame != null)
		{
			return proposal.proposalType.Value == ProposalType.Baby;
		}
		if (Game1.currentLocation == null)
		{
			return false;
		}
		if (proposal.proposalType.Value != ProposalType.Dance && Game1.CurrentEvent != null)
		{
			return false;
		}
		string additionalVar = "";
		string responseYes = null;
		string responseNo = null;
		string questionKey;
		switch (proposal.proposalType.Value)
		{
		case ProposalType.Dance:
			if (Game1.CurrentEvent == null || !Game1.CurrentEvent.isSpecificFestival("spring24"))
			{
				return false;
			}
			questionKey = "Strings\\UI:AskedToDance";
			responseYes = "Strings\\UI:AskedToDance_Accepted";
			responseNo = "Strings\\UI:AskedToDance_Rejected";
			if (Game1.player.dancePartner.Value != null)
			{
				return false;
			}
			break;
		case ProposalType.Marriage:
			if (Game1.player.isMarriedOrRoommates() || Game1.player.isEngaged())
			{
				proposal.response.Value = ProposalResponse.Rejected;
				proposal.responseMessageKey.Value = genderedKey("Strings\\UI:AskedToMarry_NotSingle", Game1.player);
				return true;
			}
			questionKey = "Strings\\UI:AskedToMarry";
			responseYes = "Strings\\UI:AskedToMarry_Accepted";
			responseNo = "Strings\\UI:AskedToMarry_Rejected";
			break;
		case ProposalType.Gift:
			if (proposal.gift.Value == null)
			{
				return false;
			}
			if (!Game1.player.couldInventoryAcceptThisItem(proposal.gift.Value))
			{
				proposal.response.Value = ProposalResponse.Rejected;
				proposal.responseMessageKey.Value = genderedKey("Strings\\UI:GiftPlayerItem_NoInventorySpace", Game1.player);
				return true;
			}
			questionKey = "Strings\\UI:GivenGift";
			additionalVar = proposal.gift.Value.DisplayName;
			break;
		case ProposalType.Baby:
			if (proposal.sender.Value.IsMale != Game1.player.IsMale)
			{
				questionKey = "Strings\\UI:AskedToHaveBaby";
				responseYes = "Strings\\UI:AskedToHaveBaby_Accepted";
				responseNo = "Strings\\UI:AskedToHaveBaby_Rejected";
			}
			else
			{
				questionKey = "Strings\\UI:AskedToAdoptBaby";
				responseYes = "Strings\\UI:AskedToAdoptBaby_Accepted";
				responseNo = "Strings\\UI:AskedToAdoptBaby_Rejected";
			}
			break;
		default:
			return false;
		}
		questionKey = genderedKey(questionKey, proposal.sender.Value);
		if (responseYes != null)
		{
			responseYes = genderedKey(responseYes, Game1.player);
		}
		if (responseNo != null)
		{
			responseNo = genderedKey(responseNo, Game1.player);
		}
		string question = Game1.content.LoadString(questionKey, proposal.sender.Value.Name, additionalVar);
		Game1.currentLocation.createQuestionDialogue(question, Game1.currentLocation.createYesNoResponses(), delegate(Farmer _, string answer)
		{
			if (proposal.canceled.Value)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:ProposalWithdrawn", proposal.sender.Value.Name));
				proposal.response.Value = ProposalResponse.Rejected;
				proposal.responseMessageKey.Value = responseNo;
			}
			else if (answer == "Yes")
			{
				proposal.response.Value = ProposalResponse.Accepted;
				proposal.responseMessageKey.Value = responseYes;
				if (proposal.proposalType.Value == ProposalType.Gift || proposal.proposalType.Value == ProposalType.Marriage)
				{
					Item value = proposal.gift.Value;
					proposal.gift.Value = null;
					value = Game1.player.addItemToInventory(value);
					if (value != null)
					{
						Game1.currentLocation.debris.Add(new Debris(value, Game1.player.Position));
					}
				}
				switch (proposal.proposalType.Value)
				{
				case ProposalType.Dance:
					Game1.player.dancePartner.Value = proposal.sender.Value;
					break;
				case ProposalType.Marriage:
				{
					Friendship friendship2 = GetFriendship(proposal.sender.Value.UniqueMultiplayerID, Game1.player.UniqueMultiplayerID);
					friendship2.Status = FriendshipStatus.Engaged;
					friendship2.Proposer = proposal.sender.Value.UniqueMultiplayerID;
					WorldDate worldDate2 = new WorldDate(Game1.Date);
					worldDate2.TotalDays += 3;
					while (!Game1.canHaveWeddingOnDay(worldDate2.DayOfMonth, worldDate2.Season))
					{
						worldDate2.TotalDays++;
					}
					friendship2.WeddingDate = worldDate2;
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:PlayerWeddingArranged"));
					Game1.multiplayer.globalChatInfoMessage("Engaged", Game1.player.Name, proposal.sender.Value.Name);
					break;
				}
				case ProposalType.Baby:
				{
					Friendship friendship = GetFriendship(proposal.sender.Value.UniqueMultiplayerID, Game1.player.UniqueMultiplayerID);
					WorldDate worldDate = new WorldDate(Game1.Date);
					worldDate.TotalDays += 14;
					friendship.NextBirthingDate = worldDate;
					break;
				}
				}
				Game1.player.doEmote(20);
			}
			else
			{
				proposal.response.Value = ProposalResponse.Rejected;
				proposal.responseMessageKey.Value = responseNo;
			}
		});
		return true;
	}

	public bool playerIsOnline(long uid)
	{
		if (Game1.MasterPlayer.UniqueMultiplayerID == uid)
		{
			return true;
		}
		if (Game1.serverHost != null && Game1.serverHost.Value.UniqueMultiplayerID == uid)
		{
			return true;
		}
		if (Game1.otherFarmers.ContainsKey(uid))
		{
			return !Game1.multiplayer.isDisconnecting(uid);
		}
		return false;
	}

	/// <summary>Get a global inventory from <see cref="F:StardewValley.FarmerTeam.globalInventories" />, creating it if needed.</summary>
	/// <param name="id">The inventory ID to get, usually matching a constant like <see cref="F:StardewValley.FarmerTeam.GlobalInventoryId_JunimoChest" />.</param>
	public Inventory GetOrCreateGlobalInventory(string id)
	{
		if (!globalInventories.TryGetValue(id, out var inventory))
		{
			inventory = (globalInventories[id] = new Inventory());
		}
		return inventory;
	}

	/// <summary>Get the mutex which restricts access to a global inventory, creating it if needed.</summary>
	/// <param name="id">The inventory ID to get, usually matching a constant like <see cref="F:StardewValley.FarmerTeam.GlobalInventoryId_JunimoChest" />.</param>
	public NetMutex GetOrCreateGlobalInventoryMutex(string id)
	{
		if (!globalInventoryMutexes.TryGetValue(id, out var mutex))
		{
			mutex = (globalInventoryMutexes[id] = new NetMutex());
		}
		return mutex;
	}

	public void NewDay()
	{
		Game1.netReady.Reset();
		chestHit.Reset();
		if (Game1.IsClient)
		{
			return;
		}
		luauIngredients.Clear();
		if (grangeDisplay.Count > 0)
		{
			for (int i = 0; i < grangeDisplay.Count; i++)
			{
				Item item = grangeDisplay[i];
				grangeDisplay[i] = null;
				if (item != null)
				{
					returnedDonations.Add(item);
					newLostAndFoundItems.Value = true;
				}
			}
		}
		grangeDisplay.Clear();
		movieInvitations.Clear();
		synchronizedShopStock.Clear();
	}

	/// <summary>Synchronizes a request to perform an action on a group of players.</summary>
	/// <param name="request">The data of the requested action to synchronize.</param>
	/// <param name="event">The net event used to send the synchronization data.</param>
	private void RequestPlayerAction<T>(T request, NetEvent1<T> @event) where T : BasePlayerActionRequest, new()
	{
		if (request.OnlyForLocalPlayer())
		{
			request.PerformAction(Game1.player);
		}
		else
		{
			@event.Fire(request);
		}
	}

	/// <summary>Handles a request to perform an action on a group of players.</summary>
	/// <param name="request">The arguments for the event.</param>
	private void OnRequestPlayerAction(BasePlayerActionRequest request)
	{
		if (request.MatchesPlayer(Game1.player))
		{
			request.PerformAction(Game1.player);
		}
		if (request.Target != PlayerActionTarget.All || !Game1.IsMasterGame)
		{
			return;
		}
		foreach (Farmer farmhand in Game1.getOfflineFarmhands())
		{
			if (request.MatchesPlayer(farmhand))
			{
				request.PerformAction(farmhand);
			}
		}
	}
}
