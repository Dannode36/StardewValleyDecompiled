using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Constants;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.SpecialOrders;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Locations;

public class MineShaft : GameLocation
{
	public const int mineFrostLevel = 40;

	public const int mineLavaLevel = 80;

	public const int upperArea = 0;

	public const int jungleArea = 10;

	public const int frostArea = 40;

	public const int lavaArea = 80;

	public const int desertArea = 121;

	public const int bottomOfMineLevel = 120;

	public const int quarryMineShaft = 77377;

	public const int numberOfLevelsPerArea = 40;

	public const int mineFeature_barrels = 0;

	public const int mineFeature_chests = 1;

	public const int mineFeature_coalCart = 2;

	public const int mineFeature_elevator = 3;

	public const double chanceForColoredGemstone = 0.008;

	public const double chanceForDiamond = 0.0005;

	public const double chanceForPrismaticShard = 0.0005;

	public const int monsterLimit = 30;

	public static SerializableDictionary<int, MineInfo> permanentMineChanges = new SerializableDictionary<int, MineInfo>();

	public static int numberOfCraftedStairsUsedThisRun;

	public Random mineRandom = new Random();

	private LocalizedContentManager mineLoader = Game1.content.CreateTemporary();

	private int timeUntilElevatorLightUp;

	[XmlIgnore]
	public int loadedMapNumber;

	public int fogTime;

	public NetBool isFogUp = new NetBool();

	public static int timeSinceLastMusic = 200000;

	public bool ladderHasSpawned;

	public bool ghostAdded;

	public bool loadedDarkArea;

	public bool isFallingDownShaft;

	public Vector2 fogPos;

	private readonly NetBool elevatorShouldDing = new NetBool();

	public readonly NetString mapImageSource = new NetString();

	private readonly NetInt netMineLevel = new NetInt();

	private readonly NetIntDelta netStonesLeftOnThisLevel = new NetIntDelta();

	private readonly NetVector2 netTileBeneathLadder = new NetVector2();

	private readonly NetVector2 netTileBeneathElevator = new NetVector2();

	public readonly NetPoint calicoStatueSpot = new NetPoint();

	public readonly NetPoint recentlyActivatedCalicoStatue = new NetPoint();

	private readonly NetPoint netElevatorLightSpot = new NetPoint();

	private readonly NetBool netIsSlimeArea = new NetBool();

	private readonly NetBool netIsMonsterArea = new NetBool();

	private readonly NetBool netIsTreasureRoom = new NetBool();

	private readonly NetBool netIsDinoArea = new NetBool();

	private readonly NetBool netIsQuarryArea = new NetBool();

	private readonly NetBool netAmbientFog = new NetBool();

	private readonly NetColor netLighting = new NetColor(Color.White);

	private readonly NetColor netFogColor = new NetColor();

	private readonly NetVector2Dictionary<bool, NetBool> createLadderAtEvent = new NetVector2Dictionary<bool, NetBool>();

	private readonly NetPointDictionary<bool, NetBool> createLadderDownEvent = new NetPointDictionary<bool, NetBool>();

	private float fogAlpha;

	[XmlIgnore]
	public static ICue bugLevelLoop;

	public readonly NetBool rainbowLights = new NetBool(value: false);

	public readonly NetBool isLightingDark = new NetBool(value: false);

	private LocalizedContentManager mapContent;

	public static List<MineShaft> activeMines = new List<MineShaft>();

	public static HashSet<int> mushroomLevelsGeneratedToday = new HashSet<int>();

	public static int totalCalicoStatuesActivatedToday;

	private int recentCalicoStatueEffect;

	private bool forceFirstTime;

	private static int deepestLevelOnCurrentDesertFestivalRun;

	private int lastLevelsDownFallen;

	private Microsoft.Xna.Framework.Rectangle fogSource = new Microsoft.Xna.Framework.Rectangle(640, 0, 64, 64);

	private List<Vector2> brownSpots = new List<Vector2>();

	private int lifespan;

	private bool hasAddedDesertFestivalStatue;

	public float calicoEggIconTimerShake;

	public static int lowestLevelReached
	{
		get
		{
			if (Game1.netWorldState.Value.LowestMineLevelForOrder >= 0)
			{
				if (Game1.netWorldState.Value.LowestMineLevelForOrder == 120)
				{
					return Math.Max(Game1.netWorldState.Value.LowestMineLevelForOrder, Game1.netWorldState.Value.LowestMineLevelForOrder);
				}
				return Game1.netWorldState.Value.LowestMineLevelForOrder;
			}
			return Game1.netWorldState.Value.LowestMineLevel;
		}
		set
		{
			if (Game1.netWorldState.Value.LowestMineLevelForOrder >= 0 && value <= 120)
			{
				Game1.netWorldState.Value.LowestMineLevelForOrder = value;
			}
			else if (Game1.player.hasSkullKey || value <= 120)
			{
				Game1.netWorldState.Value.LowestMineLevel = value;
			}
		}
	}

	public int mineLevel
	{
		get
		{
			return netMineLevel;
		}
		set
		{
			netMineLevel.Value = value;
		}
	}

	public int stonesLeftOnThisLevel
	{
		get
		{
			return netStonesLeftOnThisLevel.Value;
		}
		set
		{
			netStonesLeftOnThisLevel.Value = value;
		}
	}

	public Vector2 tileBeneathLadder
	{
		get
		{
			return netTileBeneathLadder.Value;
		}
		set
		{
			netTileBeneathLadder.Value = value;
		}
	}

	public Vector2 tileBeneathElevator
	{
		get
		{
			return netTileBeneathElevator.Value;
		}
		set
		{
			netTileBeneathElevator.Value = value;
		}
	}

	public Point ElevatorLightSpot
	{
		get
		{
			return netElevatorLightSpot.Value;
		}
		set
		{
			netElevatorLightSpot.Value = value;
		}
	}

	public bool isSlimeArea
	{
		get
		{
			return netIsSlimeArea;
		}
		set
		{
			netIsSlimeArea.Value = value;
		}
	}

	public bool isDinoArea
	{
		get
		{
			return netIsDinoArea;
		}
		set
		{
			netIsDinoArea.Value = value;
		}
	}

	public bool isMonsterArea
	{
		get
		{
			return netIsMonsterArea;
		}
		set
		{
			netIsMonsterArea.Value = value;
		}
	}

	public bool isQuarryArea
	{
		get
		{
			return netIsQuarryArea;
		}
		set
		{
			netIsQuarryArea.Value = value;
		}
	}

	public bool ambientFog
	{
		get
		{
			return netAmbientFog;
		}
		set
		{
			netAmbientFog.Value = value;
		}
	}

	public Color lighting
	{
		get
		{
			return netLighting.Value;
		}
		set
		{
			netLighting.Value = value;
		}
	}

	public Color fogColor
	{
		get
		{
			return netFogColor.Value;
		}
		set
		{
			netFogColor.Value = value;
		}
	}

	public int EnemyCount => characters.Count((NPC p) => p is Monster);

	public MineShaft()
		: this(0)
	{
	}

	public MineShaft(int level)
	{
		mineLevel = level;
		name.Value = GetLevelName(level);
		mapContent = Game1.game1.xTileContent.CreateTemporary();
		if (!Game1.IsMultiplayer && getMineArea() == 121)
		{
			base.ExtraMillisecondsPerInGameMinute = 200;
		}
	}

	public override string GetLocationContextId()
	{
		if (locationContextId == null)
		{
			locationContextId = ((mineLevel >= 121) ? "Desert" : "Default");
		}
		return base.GetLocationContextId();
	}

	public override bool CanPlaceThisFurnitureHere(Furniture furniture)
	{
		return false;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(netMineLevel, "netMineLevel").AddField(netStonesLeftOnThisLevel, "netStonesLeftOnThisLevel").AddField(netTileBeneathLadder, "netTileBeneathLadder")
			.AddField(netTileBeneathElevator, "netTileBeneathElevator")
			.AddField(netElevatorLightSpot, "netElevatorLightSpot")
			.AddField(netIsSlimeArea, "netIsSlimeArea")
			.AddField(netIsMonsterArea, "netIsMonsterArea")
			.AddField(netIsTreasureRoom, "netIsTreasureRoom")
			.AddField(netIsDinoArea, "netIsDinoArea")
			.AddField(netIsQuarryArea, "netIsQuarryArea")
			.AddField(netAmbientFog, "netAmbientFog")
			.AddField(netLighting, "netLighting")
			.AddField(netFogColor, "netFogColor")
			.AddField(createLadderAtEvent, "createLadderAtEvent")
			.AddField(createLadderDownEvent, "createLadderDownEvent")
			.AddField(mapImageSource, "mapImageSource")
			.AddField(rainbowLights, "rainbowLights")
			.AddField(isLightingDark, "isLightingDark")
			.AddField(elevatorShouldDing, "elevatorShouldDing")
			.AddField(isFogUp, "isFogUp")
			.AddField(calicoStatueSpot, "calicoStatueSpot")
			.AddField(recentlyActivatedCalicoStatue, "recentlyActivatedCalicoStatue");
		isFogUp.fieldChangeEvent += delegate(NetBool field, bool oldValue, bool newValue)
		{
			if (!oldValue && newValue)
			{
				if (Game1.currentLocation == this)
				{
					Game1.changeMusicTrack("none");
				}
				if (Game1.IsClient)
				{
					fogTime = 35000;
				}
			}
			else if (!newValue)
			{
				fogTime = 0;
			}
		};
		createLadderAtEvent.OnValueAdded += delegate(Vector2 v, bool b)
		{
			doCreateLadderAt(v);
		};
		createLadderDownEvent.OnValueAdded += doCreateLadderDown;
		mapImageSource.fieldChangeEvent += delegate(NetString field, string oldValue, string newValue)
		{
			if (newValue != null && newValue != oldValue)
			{
				base.Map.TileSheets[0].ImageSource = newValue;
				base.Map.LoadTileSheets(Game1.mapDisplayDevice);
			}
		};
		recentlyActivatedCalicoStatue.fieldChangeEvent += calicoStatueActivated;
	}

	public void calicoStatueActivated(NetPoint field, Point oldVector, Point newVector)
	{
		if (newVector == Point.Zero)
		{
			return;
		}
		if (Game1.currentLocation != null && Game1.currentLocation.Equals(this))
		{
			Game1.playSound("openBox");
			temporarySprites.AddRange(Utility.sparkleWithinArea(new Microsoft.Xna.Framework.Rectangle((newVector.X - 1) * 64, (newVector.Y - 3) * 64, 192, 192), 20, Color.White, 50, 500));
			calicoEggIconTimerShake = 1500f;
			setMapTileIndex(newVector.X, newVector.Y, 285, "Buildings");
			setMapTileIndex(newVector.X, newVector.Y - 1, 269, "Front");
			setMapTileIndex(newVector.X, newVector.Y - 2, 253, "Front");
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Microsoft.Xna.Framework.Rectangle(20, 0, 19, 21), new Vector2(newVector.X * 64 - 4, ((float)newVector.Y - 2.5f) * 64f), flipped: false, 0f, Color.White)
			{
				motion = new Vector2(0f, -1f),
				yStopCoordinate = (int)(((float)newVector.Y - 3.25f) * 64f),
				scale = 4f,
				animationLength = 1,
				delayBeforeAnimationStart = 1500,
				totalNumberOfLoops = 10,
				interval = 300f,
				drawAboveAlwaysFront = true
			});
		}
		if (!Game1.IsMasterGame)
		{
			return;
		}
		Game1.player.team.calicoEggSkullCavernRating.Value++;
		totalCalicoStatuesActivatedToday++;
		Random r = Utility.CreateDaySaveRandom(totalCalicoStatuesActivatedToday);
		if (r.NextBool(0.51 + Game1.player.team.AverageDailyLuck(this)))
		{
			if (!tryToAddCalicoStatueEffect(r, 0.15, 10) && !tryToAddCalicoStatueEffect(r, 0.01, 17, effectCanStack: true) && !tryToAddCalicoStatueEffect(r, 0.05, 12, effectCanStack: true) && !tryToAddCalicoStatueEffect(r, 0.1, 15, effectCanStack: true) && !tryToAddCalicoStatueEffect(r, 0.2, 16, effectCanStack: true) && !tryToAddCalicoStatueEffect(r, 0.1, 14, effectCanStack: true) && !tryToAddCalicoStatueEffect(r, 0.5, 11, effectCanStack: true))
			{
				Game1.player.team.AddCalicoStatueEffect(13);
				signalCalicoStatueActivation(13);
			}
			return;
		}
		if (r.NextBool(0.2))
		{
			for (int tries = 0; tries < 30; tries++)
			{
				int which = r.Next(4);
				if (!Game1.player.team.calicoStatueEffects.ContainsKey(which))
				{
					Game1.player.team.AddCalicoStatueEffect(which);
					signalCalicoStatueActivation(which);
					return;
				}
			}
		}
		if (!tryToAddCalicoStatueEffect(r, 0.1, 4) && !tryToAddCalicoStatueEffect(r, 0.1, 9) && !tryToAddCalicoStatueEffect(r, 0.1, 5) && !tryToAddCalicoStatueEffect(r, 0.1, 6) && !tryToAddCalicoStatueEffect(r, 0.2, 7, effectCanStack: true) && !tryToAddCalicoStatueEffect(r, 0.2, 8, effectCanStack: true))
		{
			Game1.player.team.AddCalicoStatueEffect(13);
			signalCalicoStatueActivation(13);
		}
	}

	private void signalCalicoStatueActivation(int whichEffect)
	{
		recentCalicoStatueEffect = whichEffect;
		if (Game1.IsMultiplayer)
		{
			Game1.multiplayer.globalChatInfoMessage("CalicoStatue_Activated", TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:DF_Mine_CalicoStatue_Description_" + whichEffect));
		}
	}

	private bool tryToAddCalicoStatueEffect(Random r, double chance, int which, bool effectCanStack = false)
	{
		if (r.NextBool(chance) && (effectCanStack || !Game1.player.team.calicoStatueEffects.ContainsKey(which)))
		{
			Game1.player.team.AddCalicoStatueEffect(which);
			signalCalicoStatueActivation(which);
			return true;
		}
		return false;
	}

	public override bool AllowMapModificationsInResetState()
	{
		return true;
	}

	protected override LocalizedContentManager getMapLoader()
	{
		return mapContent;
	}

	private void setElevatorLit()
	{
		setMapTileIndex(ElevatorLightSpot.X, ElevatorLightSpot.Y, 48, "Buildings");
		Game1.currentLightSources.Add(new LightSource(4, new Vector2(ElevatorLightSpot.X, ElevatorLightSpot.Y) * 64f, 2f, Color.Black, ElevatorLightSpot.X + ElevatorLightSpot.Y * 1000, LightSource.LightContext.None, 0L));
		elevatorShouldDing.Value = false;
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		bool num = Game1.currentLocation == this;
		if ((Game1.isMusicContextActiveButNotPlaying() || Game1.getMusicTrackName().Contains("Ambient")) && Game1.random.NextDouble() < 0.00195)
		{
			localSound("cavedrip");
		}
		if (timeUntilElevatorLightUp > 0)
		{
			timeUntilElevatorLightUp -= time.ElapsedGameTime.Milliseconds;
			if (timeUntilElevatorLightUp <= 0)
			{
				int? pitch = 0;
				localSound("crystal", null, pitch);
				setElevatorLit();
			}
		}
		if (calicoEggIconTimerShake > 0f)
		{
			calicoEggIconTimerShake -= (float)time.ElapsedGameTime.TotalMilliseconds;
		}
		if (num)
		{
			if ((bool)isFogUp && Game1.shouldTimePass())
			{
				if (bugLevelLoop == null || bugLevelLoop.IsStopped)
				{
					Game1.playSound("bugLevelLoop", out bugLevelLoop);
				}
				if (fogAlpha < 1f)
				{
					if (Game1.shouldTimePass())
					{
						fogAlpha += 0.01f;
					}
					if (bugLevelLoop != null)
					{
						bugLevelLoop.SetVariable("Volume", fogAlpha * 100f);
						bugLevelLoop.SetVariable("Frequency", fogAlpha * 25f);
					}
				}
				else if (bugLevelLoop != null)
				{
					float f = (float)Math.Max(0.0, Math.Min(100.0, Math.Sin((double)((float)fogTime / 10000f) % (Math.PI * 200.0))));
					bugLevelLoop.SetVariable("Frequency", Math.Max(0f, Math.Min(100f, fogAlpha * 25f + f * 10f)));
				}
			}
			else if (fogAlpha > 0f)
			{
				if (Game1.shouldTimePass())
				{
					fogAlpha -= 0.01f;
				}
				if (bugLevelLoop != null)
				{
					bugLevelLoop.SetVariable("Volume", fogAlpha * 100f);
					bugLevelLoop.SetVariable("Frequency", Math.Max(0f, bugLevelLoop.GetVariable("Frequency") - 0.01f));
					if (fogAlpha <= 0f)
					{
						bugLevelLoop.Stop(AudioStopOptions.Immediate);
						bugLevelLoop = null;
					}
				}
			}
			if (fogAlpha > 0f || ambientFog)
			{
				fogPos = Game1.updateFloatingObjectPositionForMovement(current: new Vector2(Game1.viewport.X, Game1.viewport.Y), w: fogPos, previous: Game1.previousViewportPosition, speed: -1f);
				fogPos.X = (fogPos.X + 0.5f) % 256f;
				fogPos.Y = (fogPos.Y + 0.5f) % 256f;
			}
		}
		base.UpdateWhenCurrentLocation(time);
	}

	public override void cleanupBeforePlayerExit()
	{
		base.cleanupBeforePlayerExit();
		if (bugLevelLoop != null)
		{
			bugLevelLoop.Stop(AudioStopOptions.Immediate);
			bugLevelLoop = null;
		}
		if (!Game1.IsMultiplayer && mineLevel == 20)
		{
			Game1.changeMusicTrack("none");
		}
	}

	public Vector2 mineEntrancePosition(Farmer who)
	{
		if (!who.ridingMineElevator || tileBeneathElevator.Equals(Vector2.Zero))
		{
			return tileBeneathLadder;
		}
		return tileBeneathElevator;
	}

	private void generateContents()
	{
		ladderHasSpawned = false;
		loadLevel(mineLevel);
		chooseLevelType();
		findLadder();
		populateLevel();
	}

	public void chooseLevelType()
	{
		fogTime = 0;
		if (bugLevelLoop != null)
		{
			bugLevelLoop.Stop(AudioStopOptions.Immediate);
			bugLevelLoop = null;
		}
		ambientFog = false;
		rainbowLights.Value = false;
		isLightingDark.Value = false;
		Random r = Utility.CreateDaySaveRandom(Game1.stats.DaysPlayed, mineLevel, 4 * mineLevel);
		lighting = new Color(80, 80, 40);
		if (getMineArea() == 80)
		{
			lighting = new Color(100, 100, 50);
		}
		if (GetAdditionalDifficulty() > 0)
		{
			if (getMineArea() == 40)
			{
				lighting = new Color(230, 200, 90);
				ambientFog = true;
				fogColor = new Color(0, 80, 255) * 0.55f;
				if (mineLevel < 50)
				{
					lighting = new Color(100, 80, 40);
					ambientFog = false;
				}
			}
		}
		else if (r.NextDouble() < 0.3 && mineLevel > 2)
		{
			isLightingDark.Value = true;
			lighting = new Color(120, 120, 40);
			if (r.NextDouble() < 0.3)
			{
				lighting = new Color(150, 150, 60);
			}
		}
		if (r.NextDouble() < 0.15 && mineLevel > 5 && mineLevel != 120)
		{
			isLightingDark.Value = true;
			switch (getMineArea())
			{
			case 0:
			case 10:
				lighting = new Color(110, 110, 70);
				break;
			case 40:
				lighting = Color.Black;
				if (GetAdditionalDifficulty() > 0)
				{
					lighting = new Color(237, 212, 185);
				}
				break;
			case 80:
				lighting = new Color(90, 130, 70);
				break;
			}
		}
		if (r.NextDouble() < 0.035 && getMineArea() == 80 && mineLevel % 5 != 0 && !mushroomLevelsGeneratedToday.Contains(mineLevel))
		{
			rainbowLights.Value = true;
			mushroomLevelsGeneratedToday.Add(mineLevel);
		}
		if (isDarkArea() && mineLevel < 120)
		{
			isLightingDark.Value = true;
			lighting = ((getMineArea() == 80) ? new Color(70, 100, 100) : new Color(150, 150, 120));
			if (getMineArea() == 0)
			{
				ambientFog = true;
				fogColor = Color.Black;
			}
		}
		if (mineLevel == 100)
		{
			lighting = new Color(140, 140, 80);
		}
		if (getMineArea() == 121)
		{
			lighting = new Color(110, 110, 40);
			if (r.NextDouble() < 0.05)
			{
				lighting = (r.NextBool() ? new Color(30, 30, 0) : new Color(150, 150, 50));
			}
		}
		if (getMineArea() == 77377)
		{
			isLightingDark.Value = false;
			rainbowLights.Value = false;
			ambientFog = true;
			fogColor = Color.White * 0.4f;
			lighting = new Color(80, 80, 30);
		}
	}

	public static void yearUpdate()
	{
		permanentMineChanges.RemoveWhere((KeyValuePair<int, MineInfo> x) => x.Key > 120 || x.Key % 5 != 0);
		if (permanentMineChanges.ContainsKey(5))
		{
			permanentMineChanges[5].platformContainersLeft = 6;
		}
		if (permanentMineChanges.ContainsKey(45))
		{
			permanentMineChanges[45].platformContainersLeft = 6;
		}
		if (permanentMineChanges.ContainsKey(85))
		{
			permanentMineChanges[85].platformContainersLeft = 6;
		}
	}

	private bool canAdd(int typeOfFeature, int numberSoFar)
	{
		if (permanentMineChanges.TryGetValue(mineLevel, out var changes))
		{
			switch (typeOfFeature)
			{
			case 0:
				return changes.platformContainersLeft > numberSoFar;
			case 1:
				return changes.chestsLeft > numberSoFar;
			case 2:
				return changes.coalCartsLeft > numberSoFar;
			case 3:
				return changes.elevator == 0;
			}
		}
		return true;
	}

	public void updateMineLevelData(int feature, int amount = 1)
	{
		if (!permanentMineChanges.TryGetValue(mineLevel, out var changes))
		{
			changes = (permanentMineChanges[mineLevel] = new MineInfo());
			if (mineLevel == 5 || mineLevel == 45 || mineLevel == 85)
			{
				forceFirstTime = true;
			}
		}
		switch (feature)
		{
		case 0:
			changes.platformContainersLeft += amount;
			break;
		case 1:
			changes.chestsLeft += amount;
			break;
		case 2:
			changes.coalCartsLeft += amount;
			break;
		case 3:
			changes.elevator += amount;
			break;
		}
	}

	public void chestConsumed()
	{
		Game1.player.chestConsumedMineLevels[mineLevel] = true;
	}

	public bool isLevelSlimeArea()
	{
		return isSlimeArea;
	}

	public void checkForMapAlterations(int x, int y)
	{
		if (getTileIndexAt(x, y, "Buildings") == 194 && !canAdd(2, 0))
		{
			setMapTileIndex(x, y, 195, "Buildings");
			setMapTileIndex(x, y - 1, 179, "Front");
		}
	}

	public void findLadder()
	{
		int found = 0;
		tileBeneathElevator = Vector2.Zero;
		bool lookForWater = mineLevel % 20 == 0;
		lightGlows.Clear();
		Layer buildingsLayer = map.RequireLayer("Buildings");
		for (int y = 0; y < buildingsLayer.LayerHeight; y++)
		{
			for (int x = 0; x < buildingsLayer.LayerWidth; x++)
			{
				int tileIndex = buildingsLayer.GetTileIndexAt(x, y);
				if (tileIndex != -1)
				{
					switch (tileIndex)
					{
					case 115:
						tileBeneathLadder = new Vector2(x, y + 1);
						sharedLights[x + y * 999] = new LightSource(4, new Vector2(x, y - 2) * 64f + new Vector2(32f, 0f), 0.25f, new Color(0, 20, 50), x + y * 999, LightSource.LightContext.None, 0L);
						sharedLights[x + y * 998] = new LightSource(4, new Vector2(x, y - 1) * 64f + new Vector2(32f, 0f), 0.5f, new Color(0, 20, 50), x + y * 998, LightSource.LightContext.None, 0L);
						sharedLights[x + y * 997] = new LightSource(4, new Vector2(x, y) * 64f + new Vector2(32f, 0f), 0.75f, new Color(0, 20, 50), x + y * 997, LightSource.LightContext.None, 0L);
						sharedLights[x + y * 1000] = new LightSource(4, new Vector2(x, y + 1) * 64f + new Vector2(32f, 0f), 1f, new Color(0, 20, 50), x + y * 1000, LightSource.LightContext.None, 0L);
						found++;
						break;
					case 112:
						tileBeneathElevator = new Vector2(x, y + 1);
						found++;
						break;
					}
					if (lighting.Equals(Color.White) && found == 2 && !lookForWater)
					{
						return;
					}
					if (!lighting.Equals(Color.White) && (tileIndex == 97 || tileIndex == 113 || tileIndex == 65 || tileIndex == 66 || tileIndex == 81 || tileIndex == 82 || tileIndex == 48))
					{
						sharedLights[x + y * 1000] = new LightSource(4, new Vector2(x, y) * 64f, 2.5f, new Color(0, 50, 100), x + y * 1000, LightSource.LightContext.None, 0L);
						switch (tileIndex)
						{
						case 66:
							lightGlows.Add(new Vector2(x, y) * 64f + new Vector2(0f, 64f));
							break;
						case 97:
						case 113:
							lightGlows.Add(new Vector2(x, y) * 64f + new Vector2(32f, 32f));
							break;
						}
					}
				}
				if (Game1.IsMasterGame && isWaterTile(x, y) && getMineArea() == 80 && Game1.random.NextDouble() < 0.1)
				{
					sharedLights[x + y * 1000] = new LightSource(4, new Vector2(x, y) * 64f, 2f, new Color(0, 220, 220), x + y * 1000, LightSource.LightContext.None, 0L);
				}
			}
		}
		if (isFallingDownShaft)
		{
			Vector2 p = default(Vector2);
			while (!isTileClearForMineObjects(p))
			{
				p.X = Game1.random.Next(1, map.Layers[0].LayerWidth);
				p.Y = Game1.random.Next(1, map.Layers[0].LayerHeight);
			}
			tileBeneathLadder = p;
			Game1.player.showFrame(5);
		}
		isFallingDownShaft = false;
	}

	public override void performTenMinuteUpdate(int timeOfDay)
	{
		base.performTenMinuteUpdate(timeOfDay);
		if (mustKillAllMonstersToAdvance() && EnemyCount == 0)
		{
			Vector2 p = new Vector2((int)tileBeneathLadder.X, (int)tileBeneathLadder.Y);
			if (getTileIndexAt(Utility.Vector2ToPoint(p), "Buildings") == -1)
			{
				createLadderAt(p, "newArtifact");
				if (mustKillAllMonstersToAdvance() && Game1.player.currentLocation == this)
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MineShaft.cs.9484"));
				}
			}
		}
		if ((bool)isFogUp || map == null || mineLevel % 5 == 0 || !(Game1.random.NextDouble() < 0.1) || AnyOnlineFarmerHasBuff("23"))
		{
			return;
		}
		if (mineLevel > 10 && !mustKillAllMonstersToAdvance() && Game1.random.NextDouble() < 0.11 && getMineArea() != 77377)
		{
			isFogUp.Value = true;
			fogTime = 35000 + Game1.random.Next(-5, 6) * 1000;
			switch (getMineArea())
			{
			case 121:
				fogColor = Color.BlueViolet * 1f;
				break;
			case 0:
			case 10:
				if (GetAdditionalDifficulty() > 0)
				{
					fogColor = (isDarkArea() ? new Color(255, 150, 0) : (Color.Cyan * 0.75f));
				}
				else
				{
					fogColor = (isDarkArea() ? Color.Khaki : (Color.Green * 0.75f));
				}
				break;
			case 40:
				fogColor = Color.Blue * 0.75f;
				break;
			case 80:
				fogColor = Color.Red * 0.5f;
				break;
			}
		}
		else
		{
			spawnFlyingMonsterOffScreen();
		}
	}

	public void spawnFlyingMonsterOffScreen()
	{
		Vector2 spawnLocation = Vector2.Zero;
		switch (Game1.random.Next(4))
		{
		case 0:
			spawnLocation.X = Game1.random.Next(map.Layers[0].LayerWidth);
			break;
		case 3:
			spawnLocation.Y = Game1.random.Next(map.Layers[0].LayerHeight);
			break;
		case 1:
			spawnLocation.X = map.Layers[0].LayerWidth - 1;
			spawnLocation.Y = Game1.random.Next(map.Layers[0].LayerHeight);
			break;
		case 2:
			spawnLocation.Y = map.Layers[0].LayerHeight - 1;
			spawnLocation.X = Game1.random.Next(map.Layers[0].LayerWidth);
			break;
		}
		if (Utility.isOnScreen(spawnLocation * 64f, 64))
		{
			spawnLocation.X -= Game1.viewport.Width / 64;
		}
		switch (getMineArea())
		{
		case 0:
			if (mineLevel > 10 && isDarkArea())
			{
				characters.Add(BuffMonsterIfNecessary(new Bat(spawnLocation * 64f, mineLevel)
				{
					focusedOnFarmers = true
				}));
				playSound("batScreech");
			}
			break;
		case 10:
			if (GetAdditionalDifficulty() > 0)
			{
				characters.Add(BuffMonsterIfNecessary(new BlueSquid(spawnLocation * 64f)
				{
					focusedOnFarmers = true
				}));
			}
			else
			{
				characters.Add(BuffMonsterIfNecessary(new Fly(spawnLocation * 64f)
				{
					focusedOnFarmers = true
				}));
			}
			break;
		case 40:
			characters.Add(BuffMonsterIfNecessary(new Bat(spawnLocation * 64f, mineLevel)
			{
				focusedOnFarmers = true
			}));
			playSound("batScreech");
			break;
		case 80:
			characters.Add(BuffMonsterIfNecessary(new Bat(spawnLocation * 64f, mineLevel)
			{
				focusedOnFarmers = true
			}));
			playSound("batScreech");
			break;
		case 121:
			if (mineLevel < 171 || Game1.random.NextBool())
			{
				characters.Add(BuffMonsterIfNecessary((GetAdditionalDifficulty() > 0) ? new Serpent(spawnLocation * 64f, "Royal Serpent")
				{
					focusedOnFarmers = true
				} : new Serpent(spawnLocation * 64f)
				{
					focusedOnFarmers = true
				}));
				playSound("serpentDie");
			}
			else
			{
				characters.Add(BuffMonsterIfNecessary(new Bat(spawnLocation * 64f, mineLevel)
				{
					focusedOnFarmers = true
				}));
				playSound("batScreech");
			}
			break;
		case 77377:
			characters.Add(new Bat(spawnLocation * 64f, 77377)
			{
				focusedOnFarmers = true
			});
			playSound("rockGolemHit");
			break;
		}
	}

	public override void drawLightGlows(SpriteBatch b)
	{
		Color c;
		switch (getMineArea())
		{
		case 0:
			c = (isDarkArea() ? (Color.PaleGoldenrod * 0.5f) : (Color.PaleGoldenrod * 0.33f));
			break;
		case 80:
			c = (isDarkArea() ? (Color.Pink * 0.4f) : (Color.Red * 0.33f));
			break;
		case 40:
			c = Color.White * 0.65f;
			if (GetAdditionalDifficulty() > 0)
			{
				c = ((mineLevel % 40 >= 30) ? (new Color(220, 240, 255) * 0.8f) : (new Color(230, 225, 100) * 0.8f));
			}
			break;
		case 121:
			c = Color.White * 0.8f;
			if (isDinoArea)
			{
				c = Color.Orange * 0.5f;
			}
			break;
		default:
			c = Color.PaleGoldenrod * 0.33f;
			break;
		}
		foreach (Vector2 v in lightGlows)
		{
			if ((bool)rainbowLights)
			{
				switch ((int)(v.X / 64f + v.Y / 64f) % 4)
				{
				case 0:
					c = Color.Red * 0.5f;
					break;
				case 1:
					c = Color.Yellow * 0.5f;
					break;
				case 2:
					c = Color.Cyan * 0.33f;
					break;
				case 3:
					c = Color.Lime * 0.45f;
					break;
				}
			}
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, v), new Microsoft.Xna.Framework.Rectangle(88, 1779, 30, 30), c, 0f, new Vector2(15f, 15f), 8f + (float)(96.0 * Math.Sin((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(v.X * 777f) + (double)(v.Y * 9746f)) % 3140.0 / 1000.0) / 50.0), SpriteEffects.None, 1f);
		}
	}

	public Monster BuffMonsterIfNecessary(Monster monster)
	{
		if (monster != null && monster.GetBaseDifficultyLevel() < GetAdditionalDifficulty())
		{
			monster.BuffForAdditionalDifficulty(GetAdditionalDifficulty() - monster.GetBaseDifficultyLevel());
			if (monster is GreenSlime slime)
			{
				if (mineLevel < 40)
				{
					slime.color.Value = new Color(Game1.random.Next(40, 70), Game1.random.Next(100, 190), 255);
				}
				else if (mineLevel < 80)
				{
					slime.color.Value = new Color(0, 180, 120);
				}
				else if (mineLevel < 120)
				{
					slime.color.Value = new Color(Game1.random.Next(180, 250), 20, 120);
				}
				else
				{
					slime.color.Value = new Color(Game1.random.Next(120, 180), 20, 255);
				}
			}
			setMonsterTextureToDangerousVersion(monster);
		}
		return monster;
	}

	private void setMonsterTextureToDangerousVersion(Monster monster)
	{
		string newAssetName = string.Concat(monster.Sprite.textureName, "_dangerous");
		if (!Game1.content.DoesAssetExist<Texture2D>(newAssetName))
		{
			return;
		}
		try
		{
			monster.Sprite.LoadTexture(newAssetName);
		}
		catch (Exception e)
		{
			Game1.log.Error($"Failed loading '{newAssetName}' texture for dangerous {monster.Name}.", e);
		}
	}

	public override Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
	{
		if (!(who?.CurrentTool is FishingRod r) || !r.QualifiedItemId.Contains("TrainingRod"))
		{
			string fish = null;
			double chanceMultiplier = 1.0;
			chanceMultiplier += 0.4 * (double)who.FishingLevel;
			chanceMultiplier += (double)waterDepth * 0.1;
			string baitName = "";
			if (who?.CurrentTool is FishingRod rod)
			{
				if (rod.HasCuriosityLure())
				{
					chanceMultiplier += 5.0;
				}
				baitName = rod.GetBait()?.Name ?? "";
			}
			switch (getMineArea())
			{
			case 0:
			case 10:
				chanceMultiplier += (double)(baitName.Contains("Stonefish") ? 10 : 0);
				if (Game1.random.NextDouble() < 0.02 + 0.01 * chanceMultiplier)
				{
					fish = "(O)158";
				}
				break;
			case 40:
				chanceMultiplier += (double)(baitName.Contains("Ice Pip") ? 10 : 0);
				if (Game1.random.NextDouble() < 0.015 + 0.009 * chanceMultiplier)
				{
					fish = "(O)161";
				}
				break;
			case 80:
				chanceMultiplier += (double)(baitName.Contains("Lava Eel") ? 10 : 0);
				if (Game1.random.NextDouble() < 0.01 + 0.008 * chanceMultiplier)
				{
					fish = "(O)162";
				}
				break;
			}
			int quality = 0;
			if (Game1.random.NextDouble() < (double)((float)who.FishingLevel / 10f))
			{
				quality = 1;
			}
			if (Game1.random.NextDouble() < (double)((float)who.FishingLevel / 50f + (float)who.LuckLevel / 100f))
			{
				quality = 2;
			}
			if (fish != null)
			{
				return ItemRegistry.Create(fish, 1, quality);
			}
			if (getMineArea() == 80)
			{
				if (Game1.random.NextDouble() < 0.05 + (double)who.LuckLevel * 0.05)
				{
					return ItemRegistry.Create("(O)CaveJelly");
				}
				return ItemRegistry.Create("(O)" + Game1.random.Next(167, 173));
			}
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, "UndergroundMine");
		}
		return ItemRegistry.Create("(O)" + Game1.random.Next(167, 173));
	}

	private void adjustLevelChances(ref double stoneChance, ref double monsterChance, ref double itemChance, ref double gemStoneChance)
	{
		if (mineLevel == 1)
		{
			monsterChance = 0.0;
			itemChance = 0.0;
			gemStoneChance = 0.0;
		}
		else if (mineLevel % 5 == 0 && getMineArea() != 121)
		{
			itemChance = 0.0;
			gemStoneChance = 0.0;
			if (mineLevel % 10 == 0)
			{
				monsterChance = 0.0;
			}
		}
		if (mustKillAllMonstersToAdvance())
		{
			monsterChance = 0.025;
			itemChance = 0.001;
			stoneChance = 0.0;
			gemStoneChance = 0.0;
			if (isDinoArea)
			{
				itemChance *= 4.0;
			}
		}
		monsterChance += 0.02 * (double)GetAdditionalDifficulty();
		bool num = AnyOnlineFarmerHasBuff("23");
		bool has_spawn_monsters_buff = AnyOnlineFarmerHasBuff("24");
		if (num && getMineArea() != 121)
		{
			if (!has_spawn_monsters_buff)
			{
				monsterChance = 0.0;
			}
		}
		else if (has_spawn_monsters_buff)
		{
			monsterChance *= 2.0;
		}
		gemStoneChance /= 2.0;
		if (isQuarryArea || getMineArea() == 77377)
		{
			gemStoneChance = 0.001;
			itemChance = 0.0001;
			stoneChance *= 2.0;
			monsterChance = 0.02;
		}
		if (GetAdditionalDifficulty() > 0 && getMineArea() == 40)
		{
			monsterChance *= 0.6600000262260437;
		}
		if (Utility.GetDayOfPassiveFestival("DesertFestival") <= 0 || getMineArea() != 121)
		{
			return;
		}
		double finalModifier = 1.0;
		int[] calicoStatueInvasionIds = DesertFestival.CalicoStatueInvasionIds;
		foreach (int invasionId in calicoStatueInvasionIds)
		{
			if (Game1.player.team.calicoStatueEffects.TryGetValue(invasionId, out var invasionAmount))
			{
				monsterChance += (double)invasionAmount * 0.01;
			}
		}
		if (Game1.player.team.calicoStatueEffects.TryGetValue(7, out var monsterSurgeAmount))
		{
			finalModifier += (double)monsterSurgeAmount * 0.2;
		}
		monsterChance *= finalModifier;
	}

	public bool AnyOnlineFarmerHasBuff(string which_buff)
	{
		if (which_buff == "23" && GetAdditionalDifficulty() > 0)
		{
			return false;
		}
		foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
		{
			if (onlineFarmer.hasBuff(which_buff))
			{
				return true;
			}
		}
		return false;
	}

	private void populateLevel()
	{
		objects.Clear();
		terrainFeatures.Clear();
		resourceClumps.Clear();
		debris.Clear();
		characters.Clear();
		ghostAdded = false;
		stonesLeftOnThisLevel = 0;
		if (mineLevel == 77377)
		{
			resourceClumps.Add(new ResourceClump(148, 2, 2, new Vector2(47f, 37f), null, "TileSheets\\Objects_2"));
			resourceClumps.Add(new ResourceClump(148, 2, 2, new Vector2(36f, 12f), null, "TileSheets\\Objects_2"));
		}
		double stoneChance = (double)mineRandom.Next(10, 30) / 100.0;
		double monsterChance = 0.002 + (double)mineRandom.Next(200) / 10000.0;
		double itemChance = 0.0025;
		double gemStoneChance = 0.003;
		adjustLevelChances(ref stoneChance, ref monsterChance, ref itemChance, ref gemStoneChance);
		int barrelsAdded = 0;
		bool firstTime = !permanentMineChanges.ContainsKey(mineLevel) || forceFirstTime;
		float df_barrelExtra = 0f;
		if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && mineLevel > 131)
		{
			df_barrelExtra += 1f - 130f / (float)mineLevel;
		}
		if (mineLevel > 1 && (mineLevel % 5 != 0 || mineLevel >= 121) && (mineRandom.NextBool() || isDinoArea))
		{
			Layer backLayer = map.RequireLayer("Back");
			int numBarrels = mineRandom.Next(5) + (int)(Game1.player.team.AverageDailyLuck(this) * 20.0);
			if (isDinoArea)
			{
				numBarrels += map.Layers[0].LayerWidth * map.Layers[0].LayerHeight / 40;
			}
			for (int i = 0; i < numBarrels; i++)
			{
				Point p;
				Point motion;
				if (mineRandom.NextDouble() < 0.33 + (double)(df_barrelExtra / 2f))
				{
					p = new Point(mineRandom.Next(backLayer.LayerWidth), 0);
					motion = new Point(0, 1);
				}
				else if (mineRandom.NextBool())
				{
					p = new Point(0, mineRandom.Next(backLayer.LayerHeight));
					motion = new Point(1, 0);
				}
				else
				{
					p = new Point(backLayer.LayerWidth - 1, mineRandom.Next(backLayer.LayerHeight));
					motion = new Point(-1, 0);
				}
				while (isTileOnMap(p.X, p.Y))
				{
					p.X += motion.X;
					p.Y += motion.Y;
					if (!isTileClearForMineObjects(p.X, p.Y))
					{
						continue;
					}
					Vector2 objectPos = new Vector2(p.X, p.Y);
					if (isDinoArea)
					{
						terrainFeatures.Add(objectPos, new CosmeticPlant(mineRandom.Next(3)));
					}
					else if (!mustKillAllMonstersToAdvance())
					{
						if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && getMineArea() == 121 && !hasAddedDesertFestivalStatue && getTileIndexAt((int)objectPos.X, (int)objectPos.Y - 1, "Buildings") != -1)
						{
							calicoStatueSpot.Value = p;
							hasAddedDesertFestivalStatue = true;
						}
						else
						{
							objects.Add(objectPos, BreakableContainer.GetBarrelForMines(objectPos, this));
						}
					}
					break;
				}
			}
		}
		bool spawned_prismatic_jelly = false;
		if (mineLevel % 10 != 0 || (getMineArea() == 121 && !isForcedChestLevel(mineLevel) && !netIsTreasureRoom.Value))
		{
			Layer backLayer = map.RequireLayer("Back");
			for (int i = 0; i < backLayer.LayerWidth; i++)
			{
				for (int j = 0; j < backLayer.LayerHeight; j++)
				{
					checkForMapAlterations(i, j);
					if (isTileClearForMineObjects(i, j))
					{
						if (mineRandom.NextDouble() <= stoneChance)
						{
							Vector2 objectPos = new Vector2(i, j);
							if (base.Objects.ContainsKey(objectPos))
							{
								continue;
							}
							if (getMineArea() == 40 && mineRandom.NextDouble() < 0.15)
							{
								int which = mineRandom.Next(319, 322);
								if (GetAdditionalDifficulty() > 0 && mineLevel % 40 < 30)
								{
									which = mineRandom.Next(313, 316);
								}
								base.Objects.Add(objectPos, new Object(which.ToString(), 1)
								{
									Fragility = 2,
									CanBeGrabbed = true
								});
								continue;
							}
							if ((bool)rainbowLights && mineRandom.NextDouble() < 0.55)
							{
								if (mineRandom.NextDouble() < 0.25)
								{
									string which = ((mineRandom.Next(5) != 0) ? "(O)420" : "(O)422");
									Object obj = ItemRegistry.Create<Object>(which);
									obj.IsSpawnedObject = true;
									base.Objects.Add(objectPos, obj);
								}
								continue;
							}
							Object litter = createLitterObject(0.001, 5E-05, gemStoneChance, objectPos);
							if (litter != null)
							{
								base.Objects.Add(objectPos, litter);
								if (litter.IsBreakableStone())
								{
									stonesLeftOnThisLevel++;
								}
							}
						}
						else if (mineRandom.NextDouble() <= monsterChance && getDistanceFromStart(i, j) > 5f)
						{
							Monster monsterToAdd = null;
							if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && getMineArea() == 121)
							{
								int[] calicoStatueInvasionIds = DesertFestival.CalicoStatueInvasionIds;
								foreach (int invasionId in calicoStatueInvasionIds)
								{
									if (!Game1.player.team.calicoStatueEffects.TryGetValue(invasionId, out var amount))
									{
										continue;
									}
									for (int invasion = 0; invasion < amount; invasion++)
									{
										if (mineRandom.NextBool(0.15))
										{
											Vector2 position = new Vector2(i, j) * 64f;
											switch (invasionId)
											{
											case 3:
												monsterToAdd = new Bat(position, mineLevel);
												break;
											case 0:
												monsterToAdd = new Ghost(position, "Carbon Ghost");
												break;
											case 1:
												monsterToAdd = new Serpent(position);
												break;
											case 2:
												monsterToAdd = ((!(mineRandom.NextDouble() < 0.33)) ? ((Monster)new Skeleton(position, mineRandom.NextBool())) : ((Monster)new Bat(position, 77377)));
												monsterToAdd.BuffForAdditionalDifficulty(1);
												break;
											}
											break;
										}
									}
								}
							}
							if (monsterToAdd == null)
							{
								monsterToAdd = BuffMonsterIfNecessary(getMonsterForThisLevel(mineLevel, i, j));
							}
							if (!(monsterToAdd is GreenSlime slime))
							{
								if (!(monsterToAdd is Leaper))
								{
									if (!(monsterToAdd is Grub))
									{
										if (monsterToAdd is DustSpirit)
										{
											if (mineRandom.NextDouble() < 0.6)
											{
												tryToAddMonster(BuffMonsterIfNecessary(new DustSpirit(Vector2.Zero)), i - 1, j);
											}
											if (mineRandom.NextDouble() < 0.6)
											{
												tryToAddMonster(BuffMonsterIfNecessary(new DustSpirit(Vector2.Zero)), i + 1, j);
											}
											if (mineRandom.NextDouble() < 0.6)
											{
												tryToAddMonster(BuffMonsterIfNecessary(new DustSpirit(Vector2.Zero)), i, j - 1);
											}
											if (mineRandom.NextDouble() < 0.6)
											{
												tryToAddMonster(BuffMonsterIfNecessary(new DustSpirit(Vector2.Zero)), i, j + 1);
											}
										}
									}
									else
									{
										if (mineRandom.NextDouble() < 0.4)
										{
											tryToAddMonster(BuffMonsterIfNecessary(new Grub(Vector2.Zero)), i - 1, j);
										}
										if (mineRandom.NextDouble() < 0.4)
										{
											tryToAddMonster(BuffMonsterIfNecessary(new Grub(Vector2.Zero)), i + 1, j);
										}
										if (mineRandom.NextDouble() < 0.4)
										{
											tryToAddMonster(BuffMonsterIfNecessary(new Grub(Vector2.Zero)), i, j - 1);
										}
										if (mineRandom.NextDouble() < 0.4)
										{
											tryToAddMonster(BuffMonsterIfNecessary(new Grub(Vector2.Zero)), i, j + 1);
										}
									}
								}
								else
								{
									float partner_chance = (float)(GetAdditionalDifficulty() + 1) * 0.3f;
									if (mineRandom.NextDouble() < (double)partner_chance)
									{
										tryToAddMonster(BuffMonsterIfNecessary(new Leaper(Vector2.Zero)), i - 1, j);
									}
									if (mineRandom.NextDouble() < (double)partner_chance)
									{
										tryToAddMonster(BuffMonsterIfNecessary(new Leaper(Vector2.Zero)), i + 1, j);
									}
									if (mineRandom.NextDouble() < (double)partner_chance)
									{
										tryToAddMonster(BuffMonsterIfNecessary(new Leaper(Vector2.Zero)), i, j - 1);
									}
									if (mineRandom.NextDouble() < (double)partner_chance)
									{
										tryToAddMonster(BuffMonsterIfNecessary(new Leaper(Vector2.Zero)), i, j + 1);
									}
								}
							}
							else
							{
								if (!spawned_prismatic_jelly && Game1.random.NextDouble() <= Math.Max(0.01, 0.012 + Game1.player.team.AverageDailyLuck(this) / 10.0) && Game1.player.team.SpecialOrderActive("Wizard2"))
								{
									slime.makePrismatic();
									spawned_prismatic_jelly = true;
								}
								if (GetAdditionalDifficulty() > 0 && mineRandom.NextDouble() < (double)Math.Min((float)GetAdditionalDifficulty() * 0.1f, 0.5f))
								{
									if (mineRandom.NextDouble() < 0.009999999776482582)
									{
										slime.stackedSlimes.Value = 4;
									}
									else
									{
										slime.stackedSlimes.Value = 2;
									}
								}
							}
							if (mineRandom.NextDouble() < 0.00175)
							{
								monsterToAdd.hasSpecialItem.Value = true;
							}
							if (monsterToAdd.GetBoundingBox().Width <= 64 || isTileClearForMineObjects(i + 1, j))
							{
								characters.Add(monsterToAdd);
							}
						}
						else if (mineRandom.NextDouble() <= itemChance)
						{
							Vector2 objectPos = new Vector2(i, j);
							base.Objects.Add(objectPos, getRandomItemForThisLevel(mineLevel, objectPos));
						}
						else if (mineRandom.NextDouble() <= 0.005 && !isDarkArea() && !mustKillAllMonstersToAdvance() && (GetAdditionalDifficulty() <= 0 || (getMineArea() == 40 && mineLevel % 40 < 30)))
						{
							if (!isTileClearForMineObjects(i + 1, j) || !isTileClearForMineObjects(i, j + 1) || !isTileClearForMineObjects(i + 1, j + 1))
							{
								continue;
							}
							Vector2 objectPos = new Vector2(i, j);
							int whichClump = mineRandom.Choose(752, 754);
							if (getMineArea() == 40)
							{
								if (GetAdditionalDifficulty() > 0)
								{
									whichClump = 600;
									if (mineRandom.NextDouble() < 0.1)
									{
										whichClump = 602;
									}
								}
								else
								{
									whichClump = mineRandom.Choose(756, 758);
								}
							}
							resourceClumps.Add(new ResourceClump(whichClump, 2, 2, objectPos));
						}
						else if (GetAdditionalDifficulty() > 0)
						{
							if (getMineArea() == 40 && mineLevel % 40 < 30 && mineRandom.NextDouble() < 0.01 && getTileIndexAt(i, j - 1, "Buildings") != -1)
							{
								terrainFeatures.Add(new Vector2(i, j), new Tree("8", 5));
							}
							else if (getMineArea() == 40 && mineLevel % 40 < 30 && mineRandom.NextDouble() < 0.1 && (getTileIndexAt(i, j - 1, "Buildings") != -1 || getTileIndexAt(i - 1, j, "Buildings") != -1 || getTileIndexAt(i, j + 1, "Buildings") != -1 || getTileIndexAt(i + 1, j, "Buildings") != -1 || terrainFeatures.ContainsKey(new Vector2(i - 1, j)) || terrainFeatures.ContainsKey(new Vector2(i + 1, j)) || terrainFeatures.ContainsKey(new Vector2(i, j - 1)) || terrainFeatures.ContainsKey(new Vector2(i, j + 1))))
							{
								terrainFeatures.Add(new Vector2(i, j), new Grass((mineLevel >= 50) ? 6 : 5, (mineLevel >= 50) ? 1 : mineRandom.Next(1, 5)));
							}
							else if (getMineArea() == 80 && !isDarkArea() && mineRandom.NextDouble() < 0.1 && (getTileIndexAt(i, j - 1, "Buildings") != -1 || getTileIndexAt(i - 1, j, "Buildings") != -1 || getTileIndexAt(i, j + 1, "Buildings") != -1 || getTileIndexAt(i + 1, j, "Buildings") != -1 || terrainFeatures.ContainsKey(new Vector2(i - 1, j)) || terrainFeatures.ContainsKey(new Vector2(i + 1, j)) || terrainFeatures.ContainsKey(new Vector2(i, j - 1)) || terrainFeatures.ContainsKey(new Vector2(i, j + 1))))
							{
								terrainFeatures.Add(new Vector2(i, j), new Grass(4, mineRandom.Next(1, 5)));
							}
						}
					}
					else if (isContainerPlatform(i, j) && CanItemBePlacedHere(new Vector2(i, j)) && mineRandom.NextDouble() < 0.4 && (firstTime || canAdd(0, barrelsAdded)))
					{
						Vector2 objectPos = new Vector2(i, j);
						objects.Add(objectPos, BreakableContainer.GetBarrelForMines(objectPos, this));
						barrelsAdded++;
						if (firstTime)
						{
							updateMineLevelData(0);
						}
					}
					else
					{
						if (!(mineRandom.NextDouble() <= monsterChance) || !CanSpawnCharacterHere(new Vector2(i, j)) || !isTileOnClearAndSolidGround(i, j) || !(getDistanceFromStart(i, j) > 5f) || (AnyOnlineFarmerHasBuff("23") && getMineArea() != 121))
						{
							continue;
						}
						Monster monsterToAdd = BuffMonsterIfNecessary(getMonsterForThisLevel(mineLevel, i, j));
						if (monsterToAdd.GetBoundingBox().Width <= 64 || isTileClearForMineObjects(i + 1, j))
						{
							if (mineRandom.NextDouble() < 0.01)
							{
								monsterToAdd.hasSpecialItem.Value = true;
							}
							characters.Add(monsterToAdd);
						}
					}
				}
			}
			if (stonesLeftOnThisLevel > 35)
			{
				int tries = stonesLeftOnThisLevel / 35;
				for (int i = 0; i < tries; i++)
				{
					if (!Utility.TryGetRandom(objects, out var stone, out var obj) || !obj.IsBreakableStone())
					{
						continue;
					}
					int radius = mineRandom.Next(3, 8);
					bool monsterSpot = mineRandom.NextDouble() < 0.1;
					for (int x = (int)stone.X - radius / 2; (float)x < stone.X + (float)(radius / 2); x++)
					{
						for (int y = (int)stone.Y - radius / 2; (float)y < stone.Y + (float)(radius / 2); y++)
						{
							Vector2 tile = new Vector2(x, y);
							if (!objects.ContainsKey(tile) || !objects[tile].IsBreakableStone())
							{
								continue;
							}
							objects.Remove(tile);
							stonesLeftOnThisLevel--;
							if (getDistanceFromStart(x, y) > 5f && monsterSpot && mineRandom.NextDouble() < 0.12)
							{
								Monster monster = BuffMonsterIfNecessary(getMonsterForThisLevel(mineLevel, x, y));
								if (monster.GetBoundingBox().Width <= 64 || isTileClearForMineObjects(x + 1, y))
								{
									characters.Add(monster);
								}
							}
						}
					}
				}
			}
			tryToAddAreaUniques();
			if (mineRandom.NextDouble() < 0.95 && !mustKillAllMonstersToAdvance() && mineLevel > 1 && mineLevel % 5 != 0 && shouldCreateLadderOnThisLevel())
			{
				Vector2 possibleSpot = new Vector2(mineRandom.Next(backLayer.LayerWidth), mineRandom.Next(backLayer.LayerHeight));
				if (isTileClearForMineObjects(possibleSpot))
				{
					createLadderDown((int)possibleSpot.X, (int)possibleSpot.Y);
				}
			}
			if (mustKillAllMonstersToAdvance() && EnemyCount <= 1)
			{
				characters.Add(new Bat(tileBeneathLadder * 64f + new Vector2(256f, 256f)));
			}
		}
		if ((!mustKillAllMonstersToAdvance() || isDinoArea) && mineLevel % 5 != 0 && mineLevel > 2 && !isForcedChestLevel(mineLevel) && !netIsTreasureRoom.Value)
		{
			tryToAddOreClumps();
			if ((bool)isLightingDark)
			{
				tryToAddOldMinerPath();
			}
		}
	}

	public void placeAppropriateOreAt(Vector2 tile)
	{
		if (CanItemBePlacedHere(tile, itemIsPassable: false, CollisionMask.All, CollisionMask.None))
		{
			objects.Add(tile, getAppropriateOre(tile));
		}
	}

	public Object getAppropriateOre(Vector2 tile)
	{
		Object ore = new Object("751", 1)
		{
			MinutesUntilReady = 3
		};
		switch (getMineArea())
		{
		case 0:
		case 10:
			if (GetAdditionalDifficulty() > 0)
			{
				ore = new Object("849", 1)
				{
					MinutesUntilReady = 6
				};
			}
			break;
		case 40:
			if (GetAdditionalDifficulty() > 0)
			{
				ore = new ColoredObject("290", 1, new Color(150, 225, 160))
				{
					MinutesUntilReady = 6,
					TileLocation = tile,
					ColorSameIndexAsParentSheetIndex = true,
					Flipped = mineRandom.NextBool()
				};
			}
			else if (mineRandom.NextDouble() < 0.8)
			{
				ore = new Object("290", 1)
				{
					MinutesUntilReady = 4
				};
			}
			break;
		case 80:
			if (mineRandom.NextDouble() < 0.8)
			{
				ore = new Object("764", 1)
				{
					MinutesUntilReady = 8
				};
			}
			break;
		case 121:
			if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && mineRandom.NextBool(0.25 + (double)((float)((int)Game1.player.team.calicoEggSkullCavernRating * 5) / 100f)))
			{
				ore = new Object("CalicoEggStone_" + mineRandom.Next(3), 1)
				{
					MinutesUntilReady = 8
				};
				break;
			}
			ore = new Object("764", 1)
			{
				MinutesUntilReady = 8
			};
			if (mineRandom.NextDouble() < 0.02)
			{
				ore = new Object("765", 1)
				{
					MinutesUntilReady = 16
				};
			}
			break;
		}
		if (mineRandom.NextDouble() < 0.25 && getMineArea() != 40 && GetAdditionalDifficulty() <= 0)
		{
			ore = new Object(mineRandom.Choose("668", "670"), 1)
			{
				MinutesUntilReady = 2
			};
		}
		return ore;
	}

	public void tryToAddOreClumps()
	{
		if (!(mineRandom.NextDouble() < 0.55 + Game1.player.team.AverageDailyLuck(this)))
		{
			return;
		}
		Vector2 endPoint = getRandomTile();
		for (int tries = 0; tries < 1 || mineRandom.NextDouble() < 0.25 + Game1.player.team.AverageDailyLuck(this); tries++)
		{
			if (CanItemBePlacedHere(endPoint, itemIsPassable: false, CollisionMask.All, CollisionMask.None) && isTileOnClearAndSolidGround(endPoint) && doesTileHaveProperty((int)endPoint.X, (int)endPoint.Y, "Diggable", "Back") == null)
			{
				Object ore = getAppropriateOre(endPoint);
				if (ore.QualifiedItemId == "(O)670")
				{
					ore = new Object("668", 1);
				}
				bool hasVariant = ore.QualifiedItemId == "(O)668";
				if (ore.QualifiedItemId.Contains("CalicoEgg"))
				{
					Utility.recursiveObjectPlacement(ore, (int)endPoint.X, (int)endPoint.Y, 0.949999988079071, 0.30000001192092896, this, "Dirt", 0, 0.05000000074505806, 1, new List<string> { "CalicoEggStone_0", "CalicoEggStone_1", "CalicoEggStone_2" });
				}
				else
				{
					Utility.recursiveObjectPlacement(ore, (int)endPoint.X, (int)endPoint.Y, 0.949999988079071, 0.30000001192092896, this, "Dirt", hasVariant ? 1 : 0, 0.05000000074505806, (!hasVariant) ? 1 : 2);
				}
			}
			endPoint = getRandomTile();
		}
	}

	public void tryToAddOldMinerPath()
	{
		Vector2 endPoint = getRandomTile();
		int tries = 0;
		while (!isTileOnClearAndSolidGround(endPoint) && tries < 8)
		{
			endPoint = getRandomTile();
			tries++;
		}
		if (!isTileOnClearAndSolidGround(endPoint))
		{
			return;
		}
		Stack<Point> path = PathFindController.findPath(Utility.Vector2ToPoint(tileBeneathLadder), Utility.Vector2ToPoint(endPoint), PathFindController.isAtEndPoint, this, Game1.player, 500);
		if (path == null)
		{
			return;
		}
		while (path.Count > 0)
		{
			Point p = path.Pop();
			removeObjectsAndSpawned(p.X, p.Y, 1, 1);
			if (path.Count <= 0 || !(mineRandom.NextDouble() < 0.2))
			{
				continue;
			}
			Vector2 torchPosition = ((path.Peek().X == p.X) ? new Vector2(p.X + mineRandom.Choose(-1, 1), p.Y) : new Vector2(p.X, p.Y + mineRandom.Choose(-1, 1)));
			if (!torchPosition.Equals(Vector2.Zero) && CanItemBePlacedHere(torchPosition) && isTileOnClearAndSolidGround(torchPosition))
			{
				if (mineRandom.NextBool())
				{
					new Torch().placementAction(this, (int)torchPosition.X * 64, (int)torchPosition.Y * 64, null);
				}
				else
				{
					placeAppropriateOreAt(torchPosition);
				}
			}
		}
	}

	public void tryToAddAreaUniques()
	{
		if ((getMineArea() != 10 && getMineArea() != 80 && (getMineArea() != 40 || !(mineRandom.NextDouble() < 0.1))) || isDarkArea() || mustKillAllMonstersToAdvance())
		{
			return;
		}
		int tries = mineRandom.Next(7, 24);
		int baseWeedIndex = ((getMineArea() == 80) ? 316 : ((getMineArea() == 40) ? 319 : 313));
		Color tintColor = Color.White;
		int indexRandomizeRange = 2;
		if (GetAdditionalDifficulty() > 0)
		{
			if (getMineArea() == 10)
			{
				baseWeedIndex = 674;
				tintColor = new Color(30, 120, 255);
			}
			else if (getMineArea() == 40)
			{
				if (mineLevel % 40 >= 30)
				{
					baseWeedIndex = 319;
				}
				else
				{
					baseWeedIndex = 882;
					tintColor = new Color(100, 180, 220);
				}
			}
			else if (getMineArea() == 80)
			{
				return;
			}
		}
		Layer backLayer = map.RequireLayer("Back");
		for (int i = 0; i < tries; i++)
		{
			Vector2 tile = new Vector2(mineRandom.Next(backLayer.LayerWidth), mineRandom.Next(backLayer.LayerHeight));
			if (tintColor.Equals(Color.White))
			{
				Utility.recursiveObjectPlacement(new Object(baseWeedIndex.ToString(), 1)
				{
					Fragility = 2,
					CanBeGrabbed = true
				}, (int)tile.X, (int)tile.Y, 1.0, (float)mineRandom.Next(10, 40) / 100f, this, "Dirt", indexRandomizeRange, 0.29);
				continue;
			}
			Utility.recursiveObjectPlacement(new ColoredObject(baseWeedIndex.ToString(), 1, tintColor)
			{
				Fragility = 2,
				CanBeGrabbed = true,
				CanBeSetDown = true,
				TileLocation = tile,
				ColorSameIndexAsParentSheetIndex = true
			}, (int)tile.X, (int)tile.Y, 1.0, (float)mineRandom.Next(10, 40) / 100f, this, "Dirt", indexRandomizeRange, 0.29);
		}
	}

	public bool tryToAddMonster(Monster m, int tileX, int tileY)
	{
		if (isTileClearForMineObjects(tileX, tileY) && !IsTileOccupiedBy(new Vector2(tileX, tileY)))
		{
			m.setTilePosition(tileX, tileY);
			characters.Add(m);
			return true;
		}
		return false;
	}

	public bool isContainerPlatform(int x, int y)
	{
		return getTileIndexAt(x, y, "Back") == 257;
	}

	public bool mustKillAllMonstersToAdvance()
	{
		if (!isSlimeArea && !isMonsterArea)
		{
			return isDinoArea;
		}
		return true;
	}

	public void createLadderAt(Vector2 p, string sound = "hoeHit")
	{
		if (shouldCreateLadderOnThisLevel())
		{
			playSound(sound);
			createLadderAtEvent[p] = true;
		}
	}

	public bool shouldCreateLadderOnThisLevel()
	{
		if (mineLevel != 77377)
		{
			return mineLevel != 120;
		}
		return false;
	}

	private void doCreateLadderAt(Vector2 p)
	{
		string startSound = ((Game1.currentLocation == this) ? "sandyStep" : null);
		updateMap();
		setMapTileIndex((int)p.X, (int)p.Y, 173, "Buildings");
		temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f, Color.White * 0.5f)
		{
			interval = 80f
		});
		temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f - new Vector2(16f, 16f), Color.White * 0.5f)
		{
			delayBeforeAnimationStart = 150,
			interval = 80f,
			scale = 0.75f,
			startSound = startSound
		});
		temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f + new Vector2(32f, 16f), Color.White * 0.5f)
		{
			delayBeforeAnimationStart = 300,
			interval = 80f,
			scale = 0.75f,
			startSound = startSound
		});
		temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f - new Vector2(32f, -16f), Color.White * 0.5f)
		{
			delayBeforeAnimationStart = 450,
			interval = 80f,
			scale = 0.75f,
			startSound = startSound
		});
		temporarySprites.Add(new TemporaryAnimatedSprite(5, p * 64f - new Vector2(-16f, 16f), Color.White * 0.5f)
		{
			delayBeforeAnimationStart = 600,
			interval = 80f,
			scale = 0.75f,
			startSound = startSound
		});
		if (Game1.player.currentLocation == this)
		{
			Game1.player.TemporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle((int)p.X * 64, (int)p.Y * 64, 64, 64));
		}
	}

	public bool recursiveTryToCreateLadderDown(Vector2 centerTile, string sound = "hoeHit", int maxIterations = 16)
	{
		int iterations = 0;
		Queue<Vector2> positionsToCheck = new Queue<Vector2>();
		positionsToCheck.Enqueue(centerTile);
		List<Vector2> closedList = new List<Vector2>();
		for (; iterations < maxIterations; iterations++)
		{
			if (positionsToCheck.Count <= 0)
			{
				break;
			}
			Vector2 currentPoint = positionsToCheck.Dequeue();
			closedList.Add(currentPoint);
			if (!IsTileOccupiedBy(currentPoint) && isTileOnClearAndSolidGround(currentPoint) && doesTileHaveProperty((int)currentPoint.X, (int)currentPoint.Y, "Type", "Back") != null && doesTileHaveProperty((int)currentPoint.X, (int)currentPoint.Y, "Type", "Back").Equals("Stone"))
			{
				createLadderAt(currentPoint);
				return true;
			}
			Vector2[] directionsTileVectors = Utility.DirectionsTileVectors;
			foreach (Vector2 v in directionsTileVectors)
			{
				if (!closedList.Contains(currentPoint + v))
				{
					positionsToCheck.Enqueue(currentPoint + v);
				}
			}
		}
		return false;
	}

	public override void monsterDrop(Monster monster, int x, int y, Farmer who)
	{
		if ((bool)monster.hasSpecialItem)
		{
			Game1.createItemDebris(getSpecialItemForThisMineLevel(mineLevel, x / 64, y / 64), monster.Position, Game1.random.Next(4), monster.currentLocation);
		}
		else if (mineLevel > 121 && who != null && who.getFriendshipHeartLevelForNPC("Krobus") >= 10 && (int)who.houseUpgradeLevel >= 1 && !who.isMarriedOrRoommates() && !who.isEngaged() && Game1.random.NextDouble() < 0.001)
		{
			Game1.createItemDebris(ItemRegistry.Create("(O)808"), monster.Position, Game1.random.Next(4), monster.currentLocation);
		}
		else
		{
			base.monsterDrop(monster, x, y, who);
		}
		double extraLadderChance = ((who != null && who.hasBuff("dwarfStatue_1")) ? 0.07 : 0.0);
		if ((mustKillAllMonstersToAdvance() || !(Game1.random.NextDouble() < 0.15 + extraLadderChance)) && (!mustKillAllMonstersToAdvance() || EnemyCount > 1))
		{
			return;
		}
		Vector2 p = new Vector2(x, y) / 64f;
		p.X = (int)p.X;
		p.Y = (int)p.Y;
		monster.IsInvisible = true;
		if (!IsTileOccupiedBy(p) && isTileOnClearAndSolidGround(p) && doesTileHaveProperty((int)p.X, (int)p.Y, "Type", "Back") != null && doesTileHaveProperty((int)p.X, (int)p.Y, "Type", "Back").Equals("Stone"))
		{
			createLadderAt(p);
		}
		else if (mustKillAllMonstersToAdvance() && EnemyCount <= 1)
		{
			p = new Vector2((int)tileBeneathLadder.X, (int)tileBeneathLadder.Y);
			createLadderAt(p, "newArtifact");
			if (mustKillAllMonstersToAdvance() && who.IsLocalPlayer && who.currentLocation == this)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:MineShaft.cs.9484"));
			}
		}
	}

	public Item GetReplacementChestItem(int floor)
	{
		List<Item> valid_items = null;
		if (Game1.netWorldState.Value.ShuffleMineChests == Game1.MineChestType.Remixed)
		{
			valid_items = new List<Item>();
			switch (floor)
			{
			case 10:
				valid_items.Add(ItemRegistry.Create("(B)506"));
				valid_items.Add(ItemRegistry.Create("(B)507"));
				valid_items.Add(ItemRegistry.Create("(W)12"));
				valid_items.Add(ItemRegistry.Create("(W)17"));
				valid_items.Add(ItemRegistry.Create("(W)22"));
				valid_items.Add(ItemRegistry.Create("(W)31"));
				break;
			case 20:
				valid_items.Add(ItemRegistry.Create("(W)11"));
				valid_items.Add(ItemRegistry.Create("(W)24"));
				valid_items.Add(ItemRegistry.Create("(W)20"));
				valid_items.Add(new Ring("517"));
				valid_items.Add(new Ring("519"));
				break;
			case 50:
				valid_items.Add(ItemRegistry.Create("(B)509"));
				valid_items.Add(ItemRegistry.Create("(B)510"));
				valid_items.Add(ItemRegistry.Create("(B)508"));
				valid_items.Add(ItemRegistry.Create("(W)1"));
				valid_items.Add(ItemRegistry.Create("(W)43"));
				break;
			case 60:
				valid_items.Add(ItemRegistry.Create("(W)21"));
				valid_items.Add(ItemRegistry.Create("(W)44"));
				valid_items.Add(ItemRegistry.Create("(W)6"));
				valid_items.Add(ItemRegistry.Create("(W)18"));
				valid_items.Add(ItemRegistry.Create("(W)27"));
				break;
			case 80:
				valid_items.Add(ItemRegistry.Create("(B)512"));
				valid_items.Add(ItemRegistry.Create("(B)511"));
				valid_items.Add(ItemRegistry.Create("(W)10"));
				valid_items.Add(ItemRegistry.Create("(W)7"));
				valid_items.Add(ItemRegistry.Create("(W)46"));
				valid_items.Add(ItemRegistry.Create("(W)19"));
				break;
			case 90:
				valid_items.Add(ItemRegistry.Create("(W)8"));
				valid_items.Add(ItemRegistry.Create("(W)52"));
				valid_items.Add(ItemRegistry.Create("(W)45"));
				valid_items.Add(ItemRegistry.Create("(W)5"));
				valid_items.Add(ItemRegistry.Create("(W)60"));
				break;
			case 110:
				valid_items.Add(ItemRegistry.Create("(B)514"));
				valid_items.Add(ItemRegistry.Create("(B)878"));
				valid_items.Add(ItemRegistry.Create("(W)50"));
				valid_items.Add(ItemRegistry.Create("(W)28"));
				break;
			}
		}
		if (valid_items != null && valid_items.Count > 0)
		{
			return Utility.CreateRandom((double)Game1.uniqueIDForThisGame * 512.0, floor).ChooseFrom(valid_items);
		}
		return null;
	}

	private void addLevelChests()
	{
		List<Item> chestItem = new List<Item>();
		Vector2 chestSpot = new Vector2(9f, 9f);
		Color tint = Color.White;
		if (mineLevel < 121 && mineLevel % 20 == 0 && mineLevel % 40 != 0)
		{
			chestSpot.Y += 4f;
		}
		Item replacement_item = GetReplacementChestItem(mineLevel);
		bool force_treasure_room = false;
		if (replacement_item != null)
		{
			chestItem.Add(replacement_item);
		}
		else
		{
			switch (mineLevel)
			{
			case 5:
				Game1.player.completeQuest("14");
				if (!Game1.player.hasOrWillReceiveMail("guildQuest"))
				{
					Game1.addMailForTomorrow("guildQuest");
				}
				break;
			case 10:
				chestItem.Add(ItemRegistry.Create("(B)506"));
				break;
			case 20:
				chestItem.Add(ItemRegistry.Create("(W)11"));
				break;
			case 40:
				Game1.player.completeQuest("17");
				chestItem.Add(ItemRegistry.Create("(W)32"));
				break;
			case 50:
				chestItem.Add(ItemRegistry.Create("(B)509"));
				break;
			case 60:
				chestItem.Add(ItemRegistry.Create("(W)21"));
				break;
			case 70:
				chestItem.Add(ItemRegistry.Create("(W)33"));
				break;
			case 80:
				chestItem.Add(ItemRegistry.Create("(B)512"));
				break;
			case 90:
				chestItem.Add(ItemRegistry.Create("(W)8"));
				break;
			case 100:
				chestItem.Add(new Object("434", 1));
				break;
			case 110:
				chestItem.Add(ItemRegistry.Create("(B)514"));
				break;
			case 120:
				Game1.player.completeQuest("18");
				Game1.getSteamAchievement("Achievement_TheBottom");
				if (!Game1.player.hasSkullKey)
				{
					Game1.player.chestConsumedMineLevels.Remove(120);
					chestItem.Add(new SpecialItem(4));
					tint = Color.Pink;
				}
				break;
			case 220:
				if (Game1.player.secretNotesSeen.Contains(10) && !Game1.player.mailReceived.Contains("qiCave"))
				{
					Game1.eventUp = true;
					Game1.displayHUD = false;
					Game1.player.CanMove = false;
					Game1.player.showNotCarrying();
					currentEvent = new Event(Game1.content.LoadString((numberOfCraftedStairsUsedThisRun <= 10) ? "Data\\ExtraDialogue:SkullCavern_100_event_honorable" : "Data\\ExtraDialogue:SkullCavern_100_event"));
					currentEvent.exitLocation = new LocationRequest(base.Name, isStructure: false, this);
					Game1.player.chestConsumedMineLevels[mineLevel] = true;
				}
				else
				{
					force_treasure_room = true;
				}
				break;
			case 320:
			case 420:
				force_treasure_room = true;
				break;
			}
		}
		if (netIsTreasureRoom.Value || force_treasure_room)
		{
			chestItem.Add(getTreasureRoomItem());
		}
		if (mineLevel == 320)
		{
			chestSpot.X += 1f;
		}
		if (chestItem.Count > 0 && !Game1.player.chestConsumedMineLevels.ContainsKey(mineLevel))
		{
			overlayObjects[chestSpot] = new Chest(chestItem, chestSpot)
			{
				Tint = tint
			};
			if (getMineArea() == 121 && force_treasure_room)
			{
				(overlayObjects[chestSpot] as Chest).SetBigCraftableSpriteIndex(344);
			}
		}
		if (mineLevel == 320 || mineLevel == 420)
		{
			overlayObjects[chestSpot + new Vector2(-2f, 0f)] = new Chest(new List<Item> { getTreasureRoomItem() }, chestSpot + new Vector2(-2f, 0f))
			{
				Tint = new Color(255, 210, 200)
			};
			(overlayObjects[chestSpot + new Vector2(-2f, 0f)] as Chest).SetBigCraftableSpriteIndex(344);
		}
		if (mineLevel == 420)
		{
			overlayObjects[chestSpot + new Vector2(2f, 0f)] = new Chest(new List<Item> { getTreasureRoomItem() }, chestSpot + new Vector2(2f, 0f))
			{
				Tint = new Color(216, 255, 240)
			};
			(overlayObjects[chestSpot + new Vector2(2f, 0f)] as Chest).SetBigCraftableSpriteIndex(344);
		}
	}

	private bool isForcedChestLevel(int level)
	{
		if (level != 220 && level != 320)
		{
			return level == 420;
		}
		return true;
	}

	public static Item getTreasureRoomItem()
	{
		if (Game1.player.stats.Get(StatKeys.Mastery(0)) != 0 && Game1.random.NextDouble() < 0.02)
		{
			return ItemRegistry.Create("(O)GoldenAnimalCracker");
		}
		if (Trinket.CanSpawnTrinket(Game1.player) && Game1.random.NextDouble() < 0.045)
		{
			return Trinket.GetRandomTrinket();
		}
		switch (Game1.random.Next(26))
		{
		case 0:
			return ItemRegistry.Create("(O)288", 5);
		case 1:
			return ItemRegistry.Create("(O)287", 10);
		case 2:
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("volcanoShortcutUnlocked") || !(Game1.random.NextDouble() < 0.66))
			{
				return ItemRegistry.Create("(O)275", 5);
			}
			return ItemRegistry.Create("(O)848", 5 + Game1.random.Next(1, 4) * 5);
		case 3:
			return ItemRegistry.Create("(O)773", Game1.random.Next(2, 5));
		case 4:
			return ItemRegistry.Create("(O)749", 5 + ((Game1.random.NextDouble() < 0.25) ? 5 : 0));
		case 5:
			return ItemRegistry.Create("(O)688", 5);
		case 6:
			return ItemRegistry.Create("(O)681", Game1.random.Next(1, 4));
		case 7:
			return ItemRegistry.Create("(O)" + Game1.random.Next(628, 634));
		case 8:
			return ItemRegistry.Create("(O)645", Game1.random.Next(1, 3));
		case 9:
			return ItemRegistry.Create("(O)621", 4);
		case 10:
			if (!(Game1.random.NextDouble() < 0.33))
			{
				return ItemRegistry.Create("(O)" + Game1.random.Next(472, 499), Game1.random.Next(1, 5) * 5);
			}
			return ItemRegistry.Create("(O)802", 15);
		case 11:
			return ItemRegistry.Create("(O)286", 15);
		case 12:
			if (!(Game1.random.NextDouble() < 0.5))
			{
				return ItemRegistry.Create("(O)437");
			}
			return ItemRegistry.Create("(O)265");
		case 13:
			return ItemRegistry.Create("(O)439");
		case 14:
			if (!(Game1.random.NextDouble() < 0.33))
			{
				return ItemRegistry.Create("(O)349", Game1.random.Next(2, 5));
			}
			return ItemRegistry.Create("(O)" + ((Game1.random.NextDouble() < 0.5) ? 226 : 732), 5);
		case 15:
			return ItemRegistry.Create("(O)337", Game1.random.Next(2, 4));
		case 16:
			if (!(Game1.random.NextDouble() < 0.33))
			{
				return ItemRegistry.Create("(O)" + Game1.random.Next(235, 245), 5);
			}
			return ItemRegistry.Create("(O)" + ((Game1.random.NextDouble() < 0.5) ? 226 : 732), 5);
		case 17:
			return ItemRegistry.Create("(O)74");
		case 18:
			return ItemRegistry.Create("(BC)21");
		case 19:
			return ItemRegistry.Create("(BC)25");
		case 20:
			return ItemRegistry.Create("(BC)165");
		case 21:
			return ItemRegistry.Create(Game1.random.NextBool() ? "(H)38" : "(H)37");
		case 22:
			if (Game1.player.mailReceived.Contains("sawQiPlane"))
			{
				return ItemRegistry.Create((Game1.player.stats.Get(StatKeys.Mastery(2)) != 0) ? "(O)GoldenMysteryBox" : "(O)MysteryBox", 5);
			}
			return ItemRegistry.Create("(O)749", 5 + ((Game1.random.NextDouble() < 0.25) ? 5 : 0));
		case 23:
			return ItemRegistry.Create("(H)65");
		case 24:
			return ItemRegistry.Create("(BC)272");
		case 25:
			return ItemRegistry.Create("(H)83");
		default:
			return ItemRegistry.Create("(O)288", 5);
		}
	}

	public static Item getSpecialItemForThisMineLevel(int level, int x, int y)
	{
		Random r = Utility.CreateRandom(level, Game1.stats.DaysPlayed, x, (double)y * 10000.0);
		if (Game1.mine == null)
		{
			return ItemRegistry.Create("(O)388");
		}
		if (Game1.mine.GetAdditionalDifficulty() > 0)
		{
			if (r.NextDouble() < 0.02)
			{
				return ItemRegistry.Create("(BC)272");
			}
			switch (r.Next(7))
			{
			case 0:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)61"), r);
			case 1:
				return ItemRegistry.Create("(O)910");
			case 2:
				return ItemRegistry.Create("(O)913");
			case 3:
				return ItemRegistry.Create("(O)915");
			case 4:
				return new Ring("527");
			case 5:
				return ItemRegistry.Create("(O)858");
			case 6:
			{
				Item treasureRoomItem = getTreasureRoomItem();
				treasureRoomItem.Stack = 1;
				return treasureRoomItem;
			}
			}
		}
		if (level < 20)
		{
			switch (r.Next(6))
			{
			case 0:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)16"), r);
			case 1:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)24"), r);
			case 2:
				return ItemRegistry.Create("(B)504");
			case 3:
				return ItemRegistry.Create("(B)505");
			case 4:
				return new Ring("516");
			case 5:
				return new Ring("518");
			}
		}
		else if (level < 40)
		{
			switch (r.Next(7))
			{
			case 0:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)22"), r);
			case 1:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)24"), r);
			case 2:
				return ItemRegistry.Create("(B)504");
			case 3:
				return ItemRegistry.Create("(B)505");
			case 4:
				return new Ring("516");
			case 5:
				return new Ring("518");
			case 6:
				return ItemRegistry.Create("(W)15");
			}
		}
		else if (level < 60)
		{
			switch (r.Next(7))
			{
			case 0:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)6"), r);
			case 1:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)26"), r);
			case 2:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)15"), r);
			case 3:
				return ItemRegistry.Create("(B)510");
			case 4:
				return new Ring("517");
			case 5:
				return new Ring("519");
			case 6:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)27"), r);
			}
		}
		else if (level < 80)
		{
			switch (r.Next(7))
			{
			case 0:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)26"), r);
			case 1:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)27"), r);
			case 2:
				return ItemRegistry.Create("(B)508");
			case 3:
				return ItemRegistry.Create("(B)510");
			case 4:
				return new Ring("517");
			case 5:
				return new Ring("519");
			case 6:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)19"), r);
			}
		}
		else if (level < 100)
		{
			switch (r.Next(8))
			{
			case 0:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)48"), r);
			case 1:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)48"), r);
			case 2:
				return ItemRegistry.Create("(B)511");
			case 3:
				return ItemRegistry.Create("(B)513");
			case 4:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)18"), r);
			case 5:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)28"), r);
			case 6:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)52"), r);
			case 7:
			{
				MeleeWeapon obj = (MeleeWeapon)MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)3"), r);
				obj.AddEnchantment(new CrusaderEnchantment());
				return obj;
			}
			}
		}
		else if (level < 120)
		{
			switch (r.Next(8))
			{
			case 0:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)19"), r);
			case 1:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)50"), r);
			case 2:
				return ItemRegistry.Create("(B)511");
			case 3:
				return ItemRegistry.Create("(B)513");
			case 4:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)18"), r);
			case 5:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)46"), r);
			case 6:
				return new Ring("887");
			case 7:
			{
				MeleeWeapon obj2 = (MeleeWeapon)MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)3"), r);
				obj2.AddEnchantment(new CrusaderEnchantment());
				return obj2;
			}
			}
		}
		else
		{
			switch (r.Next(12))
			{
			case 0:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)45"), r);
			case 1:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)50"), r);
			case 2:
				return ItemRegistry.Create("(B)511");
			case 3:
				return ItemRegistry.Create("(B)513");
			case 4:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)18"), r);
			case 5:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)28"), r);
			case 6:
				return MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)52"), r);
			case 7:
				return ItemRegistry.Create("(O)787");
			case 8:
				return ItemRegistry.Create("(B)878");
			case 9:
				return ItemRegistry.Create("(O)856");
			case 10:
				return new Ring("859");
			case 11:
				return new Ring("887");
			}
		}
		return new Object("78", 1);
	}

	public override bool IsLocationSpecificOccupantOnTile(Vector2 tileLocation)
	{
		if (tileBeneathLadder.Equals(tileLocation))
		{
			return true;
		}
		if (tileBeneathElevator != Vector2.Zero && tileBeneathElevator.Equals(tileLocation))
		{
			return true;
		}
		return base.IsLocationSpecificOccupantOnTile(tileLocation);
	}

	public bool isDarkArea()
	{
		if (loadedDarkArea || mineLevel % 40 > 30)
		{
			return getMineArea() != 40;
		}
		return false;
	}

	public bool isTileClearForMineObjects(Vector2 v)
	{
		if (tileBeneathLadder.Equals(v) || tileBeneathElevator.Equals(v))
		{
			return false;
		}
		if (!CanItemBePlacedHere(v, itemIsPassable: false, CollisionMask.All, CollisionMask.None))
		{
			return false;
		}
		if (IsTileOccupiedBy(v, CollisionMask.Characters))
		{
			return false;
		}
		if (IsTileOccupiedBy(v, CollisionMask.Flooring | CollisionMask.TerrainFeatures))
		{
			return false;
		}
		string s = doesTileHaveProperty((int)v.X, (int)v.Y, "Type", "Back");
		if (s == null || !s.Equals("Stone"))
		{
			return false;
		}
		if (!isTileOnClearAndSolidGround(v))
		{
			return false;
		}
		if (objects.ContainsKey(v))
		{
			return false;
		}
		if (Utility.PointToVector2(calicoStatueSpot.Value).Equals(v))
		{
			return false;
		}
		return true;
	}

	public override string getFootstepSoundReplacement(string footstep)
	{
		if (GetAdditionalDifficulty() > 0 && getMineArea() == 40 && mineLevel % 40 < 30 && footstep == "stoneStep")
		{
			return "grassyStep";
		}
		return base.getFootstepSoundReplacement(footstep);
	}

	public bool isTileOnClearAndSolidGround(Vector2 v)
	{
		if (map.RequireLayer("Back").Tiles[(int)v.X, (int)v.Y] == null)
		{
			return false;
		}
		if (map.RequireLayer("Front").Tiles[(int)v.X, (int)v.Y] != null || map.RequireLayer("Buildings").Tiles[(int)v.X, (int)v.Y] != null)
		{
			return false;
		}
		if (getTileIndexAt((int)v.X, (int)v.Y, "Back") == 77)
		{
			return false;
		}
		return true;
	}

	public bool isTileOnClearAndSolidGround(int x, int y)
	{
		if (map.RequireLayer("Back").Tiles[x, y] == null)
		{
			return false;
		}
		if (map.RequireLayer("Front").Tiles[x, y] != null)
		{
			return false;
		}
		if (getTileIndexAt(x, y, "Back") == 77)
		{
			return false;
		}
		return true;
	}

	public bool isTileClearForMineObjects(int x, int y)
	{
		return isTileClearForMineObjects(new Vector2(x, y));
	}

	public void loadLevel(int level)
	{
		forceFirstTime = false;
		hasAddedDesertFestivalStatue = false;
		isMonsterArea = false;
		isSlimeArea = false;
		loadedDarkArea = false;
		isQuarryArea = false;
		isDinoArea = false;
		mineLoader.Unload();
		mineLoader.Dispose();
		mineLoader = Game1.content.CreateTemporary();
		if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && Game1.IsMasterGame && level > deepestLevelOnCurrentDesertFestivalRun && getMineArea() == 121)
		{
			if (level % 5 == 0)
			{
				Game1.player.team.calicoEggSkullCavernRating.Value++;
			}
			deepestLevelOnCurrentDesertFestivalRun = level;
		}
		int mapNumberToLoad = ((level % 40 % 20 == 0 && level % 40 != 0) ? 20 : ((level % 10 == 0) ? 10 : level));
		mapNumberToLoad %= 40;
		if (level == 120)
		{
			mapNumberToLoad = 120;
		}
		if (getMineArea(level) == 121)
		{
			MineShaft last_level = null;
			foreach (MineShaft mine in activeMines)
			{
				if (mine != null && mine.mineLevel > 120 && mine.mineLevel < level && (last_level == null || mine.mineLevel > last_level.mineLevel))
				{
					last_level = mine;
				}
			}
			for (mapNumberToLoad = mineRandom.Next(40); mapNumberToLoad == last_level?.loadedMapNumber; mapNumberToLoad = mineRandom.Next(40))
			{
			}
			while (mapNumberToLoad % 5 == 0)
			{
				mapNumberToLoad = mineRandom.Next(40);
			}
			if (isForcedChestLevel(level))
			{
				mapNumberToLoad = 10;
			}
			else if (level >= 130)
			{
				double chance = 0.01;
				chance += Game1.player.team.AverageDailyLuck(this) / 10.0 + Game1.player.team.AverageLuckLevel(this) / 100.0;
				if (Game1.random.NextDouble() < chance)
				{
					netIsTreasureRoom.Value = true;
					mapNumberToLoad = 10;
				}
			}
		}
		else if (getMineArea() == 77377 && mineLevel == 77377)
		{
			mapNumberToLoad = 77377;
		}
		bool preventMonsterLevel = false;
		if (lowestLevelReached >= 120 && mapNumberToLoad != 10 && mapNumberToLoad % 5 != 0 && mineLevel > 1 && mineLevel != 77377)
		{
			Random alt_random = Utility.CreateDaySaveRandom(1293857 + mineLevel * 400);
			double chance = 0.06;
			if (mineLevel > 120)
			{
				chance += Math.Min(0.06, (double)mineLevel / 10000.0);
			}
			if (alt_random.NextDouble() < chance)
			{
				int[] source = new int[4] { 40, 47, 50, 51 };
				mapNumberToLoad = alt_random.Next(40, 61);
				if (source.Contains(mapNumberToLoad) && alt_random.NextDouble() < 0.75)
				{
					mapNumberToLoad = alt_random.Next(40, 61);
				}
				if (mapNumberToLoad == 53 && getMineArea() != 121)
				{
					mapNumberToLoad = alt_random.Next(52, 61);
				}
				if (mapNumberToLoad == 40 && getMineArea() != 0 && getMineArea() != 80)
				{
					mapNumberToLoad = alt_random.Next(52, 61);
				}
				if (source.Contains(mapNumberToLoad))
				{
					preventMonsterLevel = true;
				}
			}
		}
		mapPath.Value = "Maps\\Mines\\" + mapNumberToLoad;
		loadedMapNumber = mapNumberToLoad;
		updateMap();
		Random r = Utility.CreateDaySaveRandom(level * 100);
		if ((!AnyOnlineFarmerHasBuff("23") || getMineArea() == 121) && r.NextDouble() < 0.044 && mapNumberToLoad % 5 != 0 && mapNumberToLoad % 40 > 5 && mapNumberToLoad % 40 < 30 && mapNumberToLoad % 40 != 19 && !preventMonsterLevel)
		{
			if (r.NextBool())
			{
				isMonsterArea = true;
			}
			else
			{
				isSlimeArea = true;
			}
			if (getMineArea() == 121 && mineLevel > 126 && r.NextBool())
			{
				isDinoArea = true;
				isSlimeArea = false;
				isMonsterArea = false;
			}
		}
		else if (mineLevel < 121 && r.NextDouble() < 0.044 && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccCraftsRoom") && Game1.MasterPlayer.hasOrWillReceiveMail("VisitedQuarryMine") && mapNumberToLoad % 40 > 1 && mapNumberToLoad % 5 != 0)
		{
			isQuarryArea = true;
			if (r.NextDouble() < 0.25 && !preventMonsterLevel)
			{
				isMonsterArea = true;
			}
		}
		if (isQuarryArea || getMineArea(level) == 77377)
		{
			mapImageSource.Value = "Maps\\Mines\\mine_quarryshaft";
			int numBrownSpots = map.Layers[0].LayerWidth * map.Layers[0].LayerHeight / 100;
			isQuarryArea = true;
			isSlimeArea = false;
			isMonsterArea = false;
			isDinoArea = false;
			for (int i = 0; i < numBrownSpots; i++)
			{
				brownSpots.Add(new Vector2(mineRandom.Next(0, map.Layers[0].LayerWidth), mineRandom.Next(0, map.Layers[0].LayerHeight)));
			}
		}
		else if (isDinoArea)
		{
			mapImageSource.Value = "Maps\\Mines\\mine_dino";
		}
		else if (isSlimeArea)
		{
			mapImageSource.Value = "Maps\\Mines\\mine_slime";
		}
		else if (getMineArea() == 0 || getMineArea() == 10 || (getMineArea(level) != 0 && getMineArea(level) != 10))
		{
			if (getMineArea(level) == 40)
			{
				mapImageSource.Value = "Maps\\Mines\\mine_frost";
				if (level >= 70)
				{
					mapImageSource.Value += "_dark";
					loadedDarkArea = true;
				}
			}
			else if (getMineArea(level) == 80)
			{
				mapImageSource.Value = "Maps\\Mines\\mine_lava";
				if (level >= 110 && level != 120)
				{
					mapImageSource.Value += "_dark";
					loadedDarkArea = true;
				}
			}
			else if (getMineArea(level) == 121)
			{
				mapImageSource.Value = "Maps\\Mines\\mine_desert";
				if (mapNumberToLoad % 40 >= 30)
				{
					mapImageSource.Value += "_dark";
					loadedDarkArea = true;
				}
			}
		}
		if (mapNumberToLoad == 45)
		{
			loadedDarkArea = true;
			if (mapImageSource.Value == null)
			{
				mapImageSource.Value = "Maps\\Mines\\mine_dark";
			}
			else if (!mapImageSource.Value.EndsWith("dark"))
			{
				mapImageSource.Value += "_dark";
			}
		}
		if (GetAdditionalDifficulty() > 0)
		{
			string map_image_source = "Maps\\Mines\\mine";
			if (mapImageSource.Value != null)
			{
				map_image_source = mapImageSource.Value;
			}
			if (map_image_source.EndsWith("_dark"))
			{
				map_image_source = map_image_source.Remove(map_image_source.Length - "_dark".Length);
			}
			string base_map_image_source = map_image_source;
			if (level % 40 >= 30)
			{
				loadedDarkArea = true;
			}
			if (loadedDarkArea)
			{
				map_image_source += "_dark";
			}
			map_image_source += "_dangerous";
			try
			{
				mapImageSource.Value = map_image_source;
				Game1.temporaryContent.Load<Texture2D>(mapImageSource.Value);
			}
			catch (ContentLoadException)
			{
				map_image_source = base_map_image_source + "_dangerous";
				try
				{
					mapImageSource.Value = map_image_source;
					Game1.temporaryContent.Load<Texture2D>(mapImageSource.Value);
				}
				catch (ContentLoadException)
				{
					map_image_source = base_map_image_source;
					if (loadedDarkArea)
					{
						map_image_source += "_dark";
					}
					try
					{
						mapImageSource.Value = map_image_source;
						Game1.temporaryContent.Load<Texture2D>(mapImageSource.Value);
						goto end_IL_0839;
					}
					catch (ContentLoadException)
					{
						mapImageSource.Value = base_map_image_source;
						goto end_IL_0839;
					}
					end_IL_0839:;
				}
			}
		}
		ApplyDiggableTileFixes();
		if (!isSideBranch())
		{
			lowestLevelReached = Math.Max(lowestLevelReached, level);
			if (mineLevel % 5 == 0 && getMineArea() != 121)
			{
				prepareElevator();
			}
		}
	}

	private void addBlueFlamesToChallengeShrine()
	{
		temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(8.75f, 5.8f) * 64f + new Vector2(32f, -32f), flipped: false, 0f, Color.White)
		{
			interval = 50f,
			totalNumberOfLoops = 99999,
			animationLength = 4,
			light = true,
			lightID = 888,
			id = 888,
			lightRadius = 2f,
			scale = 4f,
			yPeriodic = true,
			lightcolor = new Color(100, 0, 0),
			yPeriodicLoopTime = 1000f,
			yPeriodicRange = 4f,
			layerDepth = 0.04544f
		});
		temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2(10.75f, 5.8f) * 64f + new Vector2(32f, -32f), flipped: false, 0f, Color.White)
		{
			interval = 50f,
			totalNumberOfLoops = 99999,
			animationLength = 4,
			light = true,
			lightID = 889,
			id = 889,
			lightRadius = 2f,
			scale = 4f,
			lightcolor = new Color(100, 0, 0),
			yPeriodic = true,
			yPeriodicLoopTime = 1100f,
			yPeriodicRange = 4f,
			layerDepth = 0.04544f
		});
		Game1.playSound("fireball");
	}

	public static void CheckForQiChallengeCompletion()
	{
		if (Game1.player.deepestMineLevel >= 145 && Game1.player.hasQuest("20") && !Game1.player.hasOrWillReceiveMail("QiChallengeComplete"))
		{
			Game1.player.completeQuest("20");
			Game1.addMailForTomorrow("QiChallengeComplete");
		}
	}

	private void prepareElevator()
	{
		Point elevatorSpot = (ElevatorLightSpot = Utility.findTile(this, 80, "Buildings"));
		if (elevatorSpot.X >= 0)
		{
			if (canAdd(3, 0))
			{
				elevatorShouldDing.Value = true;
				updateMineLevelData(3);
			}
			else
			{
				setMapTileIndex(elevatorSpot.X, elevatorSpot.Y, 48, "Buildings");
			}
		}
	}

	public void enterMineShaft()
	{
		DelayedAction.playSoundAfterDelay("fallDown", 800, this);
		DelayedAction.playSoundAfterDelay("clubSmash", 1800);
		Random random = Utility.CreateRandom(mineLevel, Game1.uniqueIDForThisGame, Game1.Date.TotalDays);
		int levelsDown = random.Next(3, 9);
		if (random.NextDouble() < 0.1)
		{
			levelsDown = levelsDown * 2 - 1;
		}
		if (mineLevel < 220 && mineLevel + levelsDown > 220)
		{
			levelsDown = 220 - mineLevel;
		}
		lastLevelsDownFallen = levelsDown;
		Game1.player.health = Math.Max(1, Game1.player.health - levelsDown * 3);
		isFallingDownShaft = true;
		Game1.globalFadeToBlack(afterFall, 0.045f);
		Game1.player.CanMove = false;
		Game1.player.jump();
		if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && Game1.IsMasterGame && lastLevelsDownFallen + mineLevel > deepestLevelOnCurrentDesertFestivalRun && isFallingDownShaft && (lastLevelsDownFallen + mineLevel) / 5 > mineLevel / 5)
		{
			Game1.player.team.calicoEggSkullCavernRating.Value += (lastLevelsDownFallen + mineLevel) / 5 - mineLevel / 5;
		}
	}

	private void afterFall()
	{
		Game1.drawObjectDialogue(Game1.content.LoadString((lastLevelsDownFallen > 7) ? "Strings\\Locations:Mines_FallenFar" : "Strings\\Locations:Mines_Fallen", lastLevelsDownFallen));
		Game1.messagePause = true;
		Game1.enterMine(mineLevel + lastLevelsDownFallen);
		Game1.fadeToBlackAlpha = 1f;
		Game1.player.faceDirection(2);
		Game1.player.showFrame(5);
	}

	/// <inheritdoc />
	public override bool ShouldExcludeFromNpcPathfinding()
	{
		return true;
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		if (who.IsLocalPlayer)
		{
			switch (getTileIndexAt(tileLocation, "Buildings"))
			{
			case 284:
				if (mineLevel > 120 && mineLevel != 77377)
				{
					recentlyActivatedCalicoStatue.Value = new Point(tileLocation.X, tileLocation.Y);
					return true;
				}
				break;
			case 112:
				if (mineLevel <= 120)
				{
					Game1.activeClickableMenu = new MineElevatorMenu();
					return true;
				}
				break;
			case 115:
			{
				Response[] options = new Response[2]
				{
					new Response("Leave", Game1.content.LoadString("Strings\\Locations:Mines_LeaveMine")).SetHotKey(Keys.Y),
					new Response("Do", Game1.content.LoadString("Strings\\Locations:Mines_DoNothing")).SetHotKey(Keys.Escape)
				};
				createQuestionDialogue(" ", options, "ExitMine");
				return true;
			}
			case 173:
				Game1.enterMine(mineLevel + 1);
				playSound("stairsdown");
				return true;
			case 174:
			{
				Response[] options = new Response[2]
				{
					new Response("Jump", Game1.content.LoadString("Strings\\Locations:Mines_ShaftJumpIn")).SetHotKey(Keys.Y),
					new Response("Do", Game1.content.LoadString("Strings\\Locations:Mines_DoNothing")).SetHotKey(Keys.Escape)
				};
				createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Mines_Shaft"), options, "Shaft");
				return true;
			}
			case 194:
				playSound("openBox");
				playSound("Ship");
				map.RequireLayer("Buildings").Tiles[tileLocation].TileIndex++;
				map.RequireLayer("Front").Tiles[tileLocation.X, tileLocation.Y - 1].TileIndex++;
				Game1.createRadialDebris(this, 382, tileLocation.X, tileLocation.Y, 6, resource: false, -1, item: true);
				updateMineLevelData(2, -1);
				return true;
			case 315:
			case 316:
			case 317:
				if (Game1.player.team.SpecialOrderRuleActive("MINE_HARD") || Game1.player.team.specialRulesRemovedToday.Contains("MINE_HARD"))
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ChallengeShrine_OnQiChallenge"));
				}
				else if (Game1.player.team.toggleMineShrineOvernight.Value)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:ChallengeShrine_AlreadyActive"));
				}
				else
				{
					createQuestionDialogue(Game1.player.team.mineShrineActivated.Value ? Game1.content.LoadString("Strings\\Locations:ChallengeShrine_AlreadyHard") : Game1.content.LoadString("Strings\\Locations:ChallengeShrine_NotYetHard"), createYesNoResponses(), "ShrineOfChallenge");
				}
				break;
			}
		}
		return base.checkAction(tileLocation, viewport, who);
	}

	public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
	{
		if (isQuarryArea)
		{
			return "";
		}
		if (Game1.random.NextDouble() < 0.15)
		{
			string objectId = "(O)330";
			if (Game1.random.NextDouble() < 0.07)
			{
				if (Game1.random.NextDouble() < 0.75)
				{
					switch (Game1.random.Next(5))
					{
					case 0:
						objectId = "(O)96";
						break;
					case 1:
						objectId = ((!who.hasOrWillReceiveMail("lostBookFound")) ? "(O)770" : ((Game1.netWorldState.Value.LostBooksFound < 21) ? "(O)102" : "(O)770"));
						break;
					case 2:
						objectId = "(O)110";
						break;
					case 3:
						objectId = "(O)112";
						break;
					case 4:
						objectId = "(O)585";
						break;
					}
				}
				else if (Game1.random.NextDouble() < 0.75)
				{
					switch (getMineArea())
					{
					case 0:
					case 10:
						objectId = Game1.random.Choose("(O)121", "(O)97");
						break;
					case 40:
						objectId = Game1.random.Choose("(O)122", "(O)336");
						break;
					case 80:
						objectId = "(O)99";
						break;
					}
				}
				else
				{
					objectId = Game1.random.Choose("(O)126", "(O)127");
				}
			}
			else if (Game1.random.NextDouble() < 0.19)
			{
				objectId = (Game1.random.NextBool() ? "(O)390" : getOreIdForLevel(mineLevel, Game1.random));
			}
			else if (Game1.random.NextDouble() < 0.45)
			{
				objectId = "(O)330";
			}
			else if (Game1.random.NextDouble() < 0.12)
			{
				if (Game1.random.NextDouble() < 0.25)
				{
					objectId = "(O)749";
				}
				else
				{
					switch (getMineArea())
					{
					case 0:
					case 10:
						objectId = "(O)535";
						break;
					case 40:
						objectId = "(O)536";
						break;
					case 80:
						objectId = "(O)537";
						break;
					}
				}
			}
			else
			{
				objectId = "(O)78";
			}
			Game1.createObjectDebris(objectId, xLocation, yLocation, who.UniqueMultiplayerID, this);
			bool num = who?.CurrentTool is Hoe && who.CurrentTool.hasEnchantmentOfType<GenerousEnchantment>();
			float generousChance = 0.25f;
			if (num && Game1.random.NextDouble() < (double)generousChance)
			{
				Game1.createObjectDebris(objectId, xLocation, yLocation, who.UniqueMultiplayerID, this);
			}
			return "";
		}
		return "";
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		base.drawAboveAlwaysFrontLayer(b);
		b.End();
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		foreach (NPC character in characters)
		{
			if (character is Monster monster)
			{
				monster.drawAboveAllLayers(b);
			}
		}
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (fogAlpha > 0f || ambientFog)
		{
			Vector2 v = default(Vector2);
			for (float x = -256 + (int)(fogPos.X % 256f); x < (float)Game1.graphics.GraphicsDevice.Viewport.Width; x += 256f)
			{
				for (float y = -256 + (int)(fogPos.Y % 256f); y < (float)Game1.graphics.GraphicsDevice.Viewport.Height; y += 256f)
				{
					v.X = (int)x;
					v.Y = (int)y;
					b.Draw(Game1.mouseCursors, v, fogSource, (fogAlpha > 0f) ? (fogColor * fogAlpha) : fogColor, 0f, Vector2.Zero, 4.001f, SpriteEffects.None, 1f);
				}
			}
		}
		if (Game1.game1.takingMapScreenshot || isSideBranch())
		{
			return;
		}
		Color col = ((getMineArea() == 0 || (isDarkArea() && getMineArea() != 121)) ? SpriteText.color_White : ((getMineArea() == 10) ? SpriteText.color_Green : ((getMineArea() == 40) ? SpriteText.color_Cyan : ((getMineArea() == 80) ? SpriteText.color_Red : SpriteText.color_Purple))));
		string txt = (mineLevel + ((getMineArea() == 121) ? (-120) : 0)).ToString() ?? "";
		Microsoft.Xna.Framework.Rectangle tsarea = Game1.game1.GraphicsDevice.Viewport.GetTitleSafeArea();
		int height = SpriteText.getHeightOfString(txt);
		SpriteText.drawString(b, txt, tsarea.Left + 16, tsarea.Top + 16, 999999, -1, height, 1f, 1f, junimoText: false, 2, "", col);
		int text_width = SpriteText.getWidthOfString(txt);
		if (mustKillAllMonstersToAdvance())
		{
			b.Draw(Game1.mouseCursors, new Vector2(tsarea.Left + 16 + text_width + 16, tsarea.Top + 16) + new Vector2(4f, 6f) * 4f, new Microsoft.Xna.Framework.Rectangle(192, 324, 7, 10), Color.White, 0f, new Vector2(3f, 5f), 4f + Game1.dialogueButtonScale / 25f, SpriteEffects.None, 1f);
		}
		if (Utility.GetDayOfPassiveFestival("DesertFestival") <= 0)
		{
			return;
		}
		int buffs = 0;
		foreach (IClickableMenu onScreenMenu in Game1.onScreenMenus)
		{
			if (onScreenMenu is BuffsDisplay _bd)
			{
				buffs = _bd.getNumBuffs();
			}
		}
		Vector2 eggPos = new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width - 300f * ((float)Game1.graphics.GraphicsDevice.Viewport.Width / (float)Game1.uiViewport.Width) - 100f, tsarea.Top + 64 + 16 + (buffs - 1) / 5 * 16 * 4) + new Vector2(4f, 6f) * 4f;
		if (calicoEggIconTimerShake > 0f)
		{
			eggPos += new Vector2(Game1.random.Next(-4, 5), Game1.random.Next(-4, 5));
			b.DrawString(Game1.dialogueFont, "+1", eggPos + new Vector2(eggPos.X - 32f, eggPos.Y + 32f), Color.White);
		}
		b.Draw(Game1.mouseCursors_1_6, eggPos, new Microsoft.Xna.Framework.Rectangle(0, 0, 19, 21), Color.White, 0f, new Vector2(3f, 5f), 4f, SpriteEffects.None, 1f);
		SpriteText.drawString(b, ((int)Game1.player.team.calicoEggSkullCavernRating + 1).ToString() ?? "", (int)eggPos.X + 28 - SpriteText.getWidthOfString(((int)Game1.player.team.calicoEggSkullCavernRating + 1).ToString() ?? "") / 2, (int)eggPos.Y + 4);
	}

	/// <inheritdoc />
	public override void checkForMusic(GameTime time)
	{
		if (Game1.player.freezePause <= 0 && !isFogUp && mineLevel != 120)
		{
			string trackName = null;
			switch (getMineArea())
			{
			case 0:
			case 10:
			case 121:
			case 77377:
				trackName = "Upper_Ambient";
				break;
			case 40:
				trackName = "Frost_Ambient";
				break;
			case 80:
				trackName = "Lava_Ambient";
				break;
			}
			if (GetAdditionalDifficulty() > 0 && getMineArea() == 40 && mineLevel < 70)
			{
				trackName = "jungle_ambience";
			}
			if (Game1.getMusicTrackName() == "none" || Game1.isMusicContextActiveButNotPlaying() || (Game1.getMusicTrackName().EndsWith("_Ambient") && Game1.getMusicTrackName() != trackName))
			{
				Game1.changeMusicTrack(trackName);
			}
			timeSinceLastMusic = Math.Min(335000, timeSinceLastMusic + time.ElapsedGameTime.Milliseconds);
		}
	}

	public string getMineSong()
	{
		if (mineLevel < 40)
		{
			return "EarthMine";
		}
		if (mineLevel < 80)
		{
			return "FrostMine";
		}
		if (getMineArea() == 121)
		{
			if (Game1.random.NextDouble() < 0.75)
			{
				return "LavaMine";
			}
			return "EarthMine";
		}
		return "LavaMine";
	}

	public int GetAdditionalDifficulty()
	{
		if (mineLevel == 77377)
		{
			return 0;
		}
		if (mineLevel > 120)
		{
			return Game1.netWorldState.Value.SkullCavesDifficulty;
		}
		return Game1.netWorldState.Value.MinesDifficulty;
	}

	public bool isPlayingSongFromDifferentArea()
	{
		if (Game1.getMusicTrackName() != getMineSong())
		{
			return Game1.getMusicTrackName().EndsWith("Mine");
		}
		return false;
	}

	public void playMineSong()
	{
		string track_for_area = getMineSong();
		if ((Game1.getMusicTrackName() == "none" || Game1.isMusicContextActiveButNotPlaying() || Game1.getMusicTrackName().Contains("Ambient")) && !isDarkArea() && mineLevel != 77377)
		{
			Game1.changeMusicTrack(track_for_area);
			timeSinceLastMusic = 0;
		}
	}

	protected override void resetLocalState()
	{
		addLevelChests();
		base.resetLocalState();
		if (Game1.IsPlayingBackgroundMusic)
		{
			Game1.changeMusicTrack("none");
		}
		if ((bool)elevatorShouldDing)
		{
			timeUntilElevatorLightUp = 1500;
		}
		else if (mineLevel % 5 == 0 && getMineArea() != 121)
		{
			setElevatorLit();
		}
		if (!isSideBranch(mineLevel))
		{
			Game1.player.deepestMineLevel = Math.Max(Game1.player.deepestMineLevel, mineLevel);
			if (Game1.player.team.specialOrders != null)
			{
				foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
				{
					specialOrder.onMineFloorReached?.Invoke(Game1.player, mineLevel);
				}
			}
			Game1.player.autoGenerateActiveDialogueEvent("mineArea_" + getMineArea());
		}
		if (mineLevel == 77377)
		{
			Game1.addMailForTomorrow("VisitedQuarryMine", noLetter: true, sendToEveryone: true);
		}
		if (getMineArea() == 121 && Game1.player.team.calicoStatueEffects.ContainsKey(10) && !Game1.player.hasBuff("CalicoStatueSpeed"))
		{
			DesertFestival.addCalicoStatueSpeedBuff();
		}
		CheckForQiChallengeCompletion();
		if (mineLevel == 120)
		{
			Farmer player = Game1.player;
			int timesReachedMineBottom = player.timesReachedMineBottom + 1;
			player.timesReachedMineBottom = timesReachedMineBottom;
		}
		Vector2 vector = mineEntrancePosition(Game1.player);
		Game1.xLocationAfterWarp = (int)vector.X;
		Game1.yLocationAfterWarp = (int)vector.Y;
		if (Game1.IsClient)
		{
			Game1.player.Position = new Vector2(Game1.xLocationAfterWarp * 64, Game1.yLocationAfterWarp * 64 - (Game1.player.Sprite.getHeight() - 32) + 16);
		}
		forceViewportPlayerFollow = true;
		switch (mineLevel)
		{
		case 20:
			if (!Game1.IsMultiplayer && IsRainingHere() && Game1.player.eventsSeen.Contains("901756") && !Game1.IsMultiplayer)
			{
				characters.Clear();
				NPC a = new NPC(new AnimatedSprite("Characters\\Abigail", 0, 16, 32), new Vector2(896f, 644f), "SeedShop", 3, "AbigailMine", datable: true, Game1.content.Load<Texture2D>("Portraits\\Abigail"))
				{
					displayName = NPC.GetDisplayName("Abigail")
				};
				Random r = Utility.CreateRandom(Game1.stats.DaysPlayed);
				if (Game1.player.mailReceived.Add("AbigailInMineFirst"))
				{
					a.setNewDialogue("Strings\\Characters:AbigailInMineFirst");
					a.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(0, 300),
						new FarmerSprite.AnimationFrame(1, 300),
						new FarmerSprite.AnimationFrame(2, 300),
						new FarmerSprite.AnimationFrame(3, 300)
					});
				}
				else if (r.NextDouble() < 0.15)
				{
					a.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(16, 500),
						new FarmerSprite.AnimationFrame(17, 500),
						new FarmerSprite.AnimationFrame(18, 500),
						new FarmerSprite.AnimationFrame(19, 500)
					});
					a.setNewDialogue("Strings\\Characters:AbigailInMineFlute");
					Game1.changeMusicTrack("AbigailFlute");
				}
				else
				{
					a.setNewDialogue("Strings\\Characters:AbigailInMine" + r.Next(5));
					a.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(0, 300),
						new FarmerSprite.AnimationFrame(1, 300),
						new FarmerSprite.AnimationFrame(2, 300),
						new FarmerSprite.AnimationFrame(3, 300)
					});
				}
				characters.Add(a);
			}
			break;
		case 120:
			if (GetAdditionalDifficulty() > 0 && !Game1.player.hasOrWillReceiveMail("reachedBottomOfHardMines"))
			{
				Game1.addMailForTomorrow("reachedBottomOfHardMines", noLetter: true, sendToEveryone: true);
			}
			if (GetAdditionalDifficulty() > 0)
			{
				Game1.getAchievement(41);
			}
			if (Game1.player.hasOrWillReceiveMail("reachedBottomOfHardMines"))
			{
				setMapTileIndex(9, 6, 315, "Buildings");
				setMapTileIndex(10, 6, 316, "Buildings");
				setMapTileIndex(11, 6, 317, "Buildings");
				setTileProperty(9, 6, "Buildings", "Action", "None");
				setTileProperty(10, 6, "Buildings", "Action", "None");
				setTileProperty(11, 6, "Buildings", "Action", "None");
				setMapTileIndex(9, 5, 299, "Front");
				setMapTileIndex(10, 5, 300, "Front");
				setMapTileIndex(11, 5, 301, "Front");
				if ((Game1.player.team.mineShrineActivated.Value && !Game1.player.team.toggleMineShrineOvernight.Value) || (!Game1.player.team.mineShrineActivated.Value && Game1.player.team.toggleMineShrineOvernight.Value))
				{
					DelayedAction.functionAfterDelay(addBlueFlamesToChallengeShrine, 1000);
				}
			}
			break;
		}
		ApplyDiggableTileFixes();
		if (isMonsterArea || isSlimeArea)
		{
			Random r = Utility.CreateRandom(Game1.stats.DaysPlayed);
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:Mines_" + r.Choose("Infested", "Overrun")));
		}
		bool num = mineLevel % 20 == 0;
		bool foundAnyWater = false;
		if (num)
		{
			waterTiles = new WaterTiles(map.Layers[0].LayerWidth, map.Layers[0].LayerHeight);
			waterColor.Value = ((getMineArea() == 80) ? (Color.Red * 0.8f) : (new Color(50, 100, 200) * 0.5f));
			for (int y = 0; y < map.RequireLayer("Buildings").LayerHeight; y++)
			{
				for (int x = 0; x < map.RequireLayer("Buildings").LayerWidth; x++)
				{
					string water_property = doesTileHaveProperty(x, y, "Water", "Back");
					if (water_property != null)
					{
						foundAnyWater = true;
						if (water_property == "I")
						{
							waterTiles.waterTiles[x, y] = new WaterTiles.WaterTileData(is_water: true, is_visible: false);
						}
						else
						{
							waterTiles[x, y] = true;
						}
						if (getMineArea() == 80 && Game1.random.NextDouble() < 0.1)
						{
							sharedLights[x + y * 1000] = new LightSource(4, new Vector2(x, y) * 64f, 2f, new Color(0, 220, 220), x + y * 1000, LightSource.LightContext.None, 0L);
						}
					}
				}
			}
		}
		if (!foundAnyWater)
		{
			waterTiles = null;
		}
		if (getMineArea(mineLevel) != getMineArea(mineLevel - 1) || mineLevel == 120 || isPlayingSongFromDifferentArea())
		{
			Game1.changeMusicTrack("none");
		}
		if (GetAdditionalDifficulty() > 0 && mineLevel == 70)
		{
			Game1.changeMusicTrack("none");
		}
		if (mineLevel == 77377 && Game1.player.mailReceived.Contains("gotGoldenScythe"))
		{
			setMapTileIndex(29, 4, 245, "Front");
			setMapTileIndex(30, 4, 246, "Front");
			setMapTileIndex(29, 5, 261, "Front");
			setMapTileIndex(30, 5, 262, "Front");
			setMapTileIndex(29, 6, 277, "Buildings");
			setMapTileIndex(30, 56, 278, "Buildings");
		}
		if (calicoStatueSpot.Value != Point.Zero)
		{
			if (recentlyActivatedCalicoStatue.Value != Point.Zero)
			{
				setMapTileIndex(calicoStatueSpot.X, calicoStatueSpot.Y, 285, "Buildings");
				setMapTileIndex(calicoStatueSpot.X, calicoStatueSpot.Y - 1, 269, "Front");
				setMapTileIndex(calicoStatueSpot.X, calicoStatueSpot.Y - 2, 253, "Front");
			}
			else
			{
				setMapTileIndex(calicoStatueSpot.X, calicoStatueSpot.Y, 284, "Buildings");
				setMapTileIndex(calicoStatueSpot.X, calicoStatueSpot.Y - 1, 268, "Front");
				setMapTileIndex(calicoStatueSpot.X, calicoStatueSpot.Y - 2, 252, "Front");
			}
		}
		if (mineLevel > 1 && (mineLevel == 2 || (mineLevel % 5 != 0 && timeSinceLastMusic > 150000 && Game1.random.NextBool())))
		{
			playMineSong();
		}
	}

	public virtual void ApplyDiggableTileFixes()
	{
		if (map != null && (GetAdditionalDifficulty() <= 0 || getMineArea() == 40 || !isDarkArea()))
		{
			TileSheet tileSheet = map.TileSheets[0];
			tileSheet.TileIndexProperties[165].TryAdd("Diggable", "true");
			tileSheet.TileIndexProperties[181].TryAdd("Diggable", "true");
			tileSheet.TileIndexProperties[183].TryAdd("Diggable", "true");
		}
	}

	public void createLadderDown(int x, int y, bool forceShaft = false)
	{
		createLadderDownEvent[new Point(x, y)] = forceShaft || (getMineArea() == 121 && !mustKillAllMonstersToAdvance() && mineRandom.NextDouble() < 0.2);
	}

	private void doCreateLadderDown(Point point, bool shaft)
	{
		updateMap();
		int x = point.X;
		int y = point.Y;
		if (shaft)
		{
			map.RequireLayer("Buildings").Tiles[x, y] = new StaticTile(map.RequireLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 174);
		}
		else
		{
			ladderHasSpawned = true;
			map.RequireLayer("Buildings").Tiles[x, y] = new StaticTile(map.RequireLayer("Buildings"), map.TileSheets[0], BlendMode.Alpha, 173);
		}
		if (Game1.player.currentLocation == this)
		{
			Game1.player.TemporaryPassableTiles.Add(new Microsoft.Xna.Framework.Rectangle(x * 64, y * 64, 64, 64));
		}
	}

	public void checkStoneForItems(string stoneId, int x, int y, Farmer who)
	{
		long farmerId = who?.UniqueMultiplayerID ?? 0;
		int farmerLuckLevel = who?.LuckLevel ?? 0;
		double num = who?.DailyLuck ?? 0.0;
		int farmerMiningLevel = who?.MiningLevel ?? 0;
		double chanceModifier = num / 2.0 + (double)farmerMiningLevel * 0.005 + (double)farmerLuckLevel * 0.001;
		Random r = Utility.CreateDaySaveRandom(x * 1000, y, mineLevel);
		r.NextDouble();
		double oreModifier = ((stoneId == 40.ToString() || stoneId == 42.ToString()) ? 1.2 : 0.8);
		stonesLeftOnThisLevel--;
		double chanceForLadderDown = 0.02 + 1.0 / (double)Math.Max(1, stonesLeftOnThisLevel) + (double)farmerLuckLevel / 100.0 + Game1.player.DailyLuck / 5.0;
		if (EnemyCount == 0)
		{
			chanceForLadderDown += 0.04;
		}
		if (who != null && who.hasBuff("dwarfStatue_1"))
		{
			chanceForLadderDown *= 1.25;
		}
		if (!ladderHasSpawned && !mustKillAllMonstersToAdvance() && (stonesLeftOnThisLevel == 0 || r.NextDouble() < chanceForLadderDown) && shouldCreateLadderOnThisLevel())
		{
			createLadderDown(x, y);
		}
		if (breakStone(stoneId, x, y, who, r))
		{
			return;
		}
		if (stoneId == 44.ToString())
		{
			int whichGem = r.Next(59, 70);
			whichGem += whichGem % 2;
			bool reachedBottom = false;
			foreach (Farmer allFarmer in Game1.getAllFarmers())
			{
				if (allFarmer.timesReachedMineBottom > 0)
				{
					reachedBottom = true;
					break;
				}
			}
			if (!reachedBottom)
			{
				if (mineLevel < 40 && whichGem != 66 && whichGem != 68)
				{
					whichGem = r.Choose(66, 68);
				}
				else if (mineLevel < 80 && (whichGem == 64 || whichGem == 60))
				{
					whichGem = r.Choose(66, 70, 68, 62);
				}
			}
			Game1.createObjectDebris("(O)" + whichGem, x, y, farmerId, this);
			Game1.stats.OtherPreciousGemsFound++;
			return;
		}
		int excavatorMultiplier = ((who == null || !who.professions.Contains(22)) ? 1 : 2);
		double dwarfStatueMultiplier = ((who != null && who.hasBuff("dwarfStatue_4")) ? 1.25 : 1.0);
		if (r.NextDouble() < 0.022 * (1.0 + chanceModifier) * (double)excavatorMultiplier * dwarfStatueMultiplier)
		{
			string id = "(O)" + (535 + ((getMineArea() == 40) ? 1 : ((getMineArea() == 80) ? 2 : 0)));
			if (getMineArea() == 121)
			{
				id = "(O)749";
			}
			if (who != null && who.professions.Contains(19) && r.NextBool())
			{
				Game1.createObjectDebris(id, x, y, farmerId, this);
			}
			Game1.createObjectDebris(id, x, y, farmerId, this);
			who?.gainExperience(5, 20 * getMineArea());
		}
		if (mineLevel > 20 && r.NextDouble() < 0.005 * (1.0 + chanceModifier) * (double)excavatorMultiplier * dwarfStatueMultiplier)
		{
			if (who != null && who.professions.Contains(19) && r.NextBool())
			{
				Game1.createObjectDebris("(O)749", x, y, farmerId, this);
			}
			Game1.createObjectDebris("(O)749", x, y, farmerId, this);
			who?.gainExperience(5, 40 * getMineArea());
		}
		if (r.NextDouble() < 0.05 * (1.0 + chanceModifier) * oreModifier)
		{
			int burrowerMultiplier = ((who == null || !who.professions.Contains(21)) ? 1 : 2);
			double addedCoalChance = ((who != null && who.hasBuff("dwarfStatue_2")) ? 0.1 : 0.0);
			if (r.NextDouble() < 0.25 * (double)burrowerMultiplier + addedCoalChance)
			{
				Game1.createObjectDebris("(O)382", x, y, farmerId, this);
				Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(25, new Vector2(64 * x, 64 * y), Color.White, 8, Game1.random.NextBool(), 80f, 0, -1, -1f, 128));
			}
			Game1.createObjectDebris(getOreIdForLevel(mineLevel, r), x, y, farmerId, this);
			who?.gainExperience(3, 5);
		}
		else if (r.NextBool())
		{
			Game1.createDebris(14, x, y, 1, this);
		}
	}

	public string getOreIdForLevel(int mineLevel, Random r)
	{
		if (getMineArea(mineLevel) == 77377)
		{
			return "(O)380";
		}
		if (mineLevel < 40)
		{
			if (mineLevel >= 20 && r.NextDouble() < 0.1)
			{
				return "(O)380";
			}
			return "(O)378";
		}
		if (mineLevel < 80)
		{
			if (mineLevel >= 60 && r.NextDouble() < 0.1)
			{
				return "(O)384";
			}
			if (!(r.NextDouble() < 0.75))
			{
				return "(O)378";
			}
			return "(O)380";
		}
		if (mineLevel < 120)
		{
			if (!(r.NextDouble() < 0.75))
			{
				if (!(r.NextDouble() < 0.75))
				{
					return "(O)378";
				}
				return "(O)380";
			}
			return "(O)384";
		}
		if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && r.NextDouble() < 0.13 + (double)((float)((int)Game1.player.team.calicoEggSkullCavernRating * 5) / 1000f))
		{
			return "CalicoEgg";
		}
		if (r.NextDouble() < 0.01 + (double)((float)(mineLevel - 120) / 2000f))
		{
			return "(O)386";
		}
		if (!(r.NextDouble() < 0.75))
		{
			if (!(r.NextDouble() < 0.75))
			{
				return "(O)378";
			}
			return "(O)380";
		}
		return "(O)384";
	}

	public bool shouldUseSnowTextureHoeDirt()
	{
		if (isSlimeArea)
		{
			return false;
		}
		if (GetAdditionalDifficulty() > 0 && (mineLevel < 40 || (mineLevel >= 70 && mineLevel < 80)))
		{
			return true;
		}
		if (GetAdditionalDifficulty() <= 0 && getMineArea() == 40)
		{
			return true;
		}
		return false;
	}

	public int getMineArea(int level = -1)
	{
		if (level == -1)
		{
			level = mineLevel;
		}
		if (!isQuarryArea)
		{
			switch (level)
			{
			case 77377:
				break;
			case 80:
			case 81:
			case 82:
			case 83:
			case 84:
			case 85:
			case 86:
			case 87:
			case 88:
			case 89:
			case 90:
			case 91:
			case 92:
			case 93:
			case 94:
			case 95:
			case 96:
			case 97:
			case 98:
			case 99:
			case 100:
			case 101:
			case 102:
			case 103:
			case 104:
			case 105:
			case 106:
			case 107:
			case 108:
			case 109:
			case 110:
			case 111:
			case 112:
			case 113:
			case 114:
			case 115:
			case 116:
			case 117:
			case 118:
			case 119:
			case 120:
				return 80;
			default:
				if (level > 120)
				{
					return 121;
				}
				if (level >= 40)
				{
					return 40;
				}
				if (level > 10 && mineLevel < 30)
				{
					return 10;
				}
				return 0;
			}
		}
		return 77377;
	}

	public bool isSideBranch(int level = -1)
	{
		if (level == -1)
		{
			level = mineLevel;
		}
		return level == 77377;
	}

	public byte getWallAt(int x, int y)
	{
		return byte.MaxValue;
	}

	public Color getLightingColor(GameTime time)
	{
		return lighting;
	}

	public Object getRandomItemForThisLevel(int level, Vector2 tile)
	{
		string id = "80";
		if (mineRandom.NextDouble() < 0.05 && level > 80)
		{
			id = "422";
		}
		else if (mineRandom.NextDouble() < 0.1 && level > 20 && getMineArea() != 40)
		{
			id = "420";
		}
		else if (mineRandom.NextDouble() < 0.25 || GetAdditionalDifficulty() > 0)
		{
			switch (getMineArea())
			{
			case 0:
			case 10:
				if (GetAdditionalDifficulty() > 0 && !isDarkArea())
				{
					switch (mineRandom.Next(6))
					{
					case 0:
					case 6:
						id = "152";
						break;
					case 1:
						id = "393";
						break;
					case 2:
						id = "397";
						break;
					case 3:
						id = "372";
						break;
					case 4:
						id = "392";
						break;
					}
					if (mineRandom.NextDouble() < 0.005)
					{
						id = "797";
					}
					else if (mineRandom.NextDouble() < 0.08)
					{
						id = "394";
					}
				}
				else
				{
					id = "86";
				}
				break;
			case 40:
				if (GetAdditionalDifficulty() > 0 && mineLevel % 40 < 30)
				{
					switch (mineRandom.Next(4))
					{
					case 0:
					case 3:
						id = "259";
						break;
					case 1:
						id = "404";
						break;
					case 2:
						id = "420";
						break;
					}
					if (mineRandom.NextDouble() < 0.08)
					{
						id = "422";
					}
				}
				else
				{
					id = "84";
				}
				break;
			case 80:
				id = "82";
				break;
			case 121:
				id = ((mineRandom.NextDouble() < 0.3) ? "86" : ((mineRandom.NextDouble() < 0.3) ? "84" : "82"));
				break;
			}
		}
		else
		{
			id = "80";
		}
		if (isDinoArea)
		{
			id = "259";
			if (mineRandom.NextDouble() < 0.06)
			{
				id = "107";
			}
		}
		return new Object(id, 1)
		{
			IsSpawnedObject = true
		};
	}

	public bool shouldShowDarkHoeDirt()
	{
		if (getMineArea() == 121 && !isDinoArea)
		{
			return false;
		}
		return true;
	}

	public string getRandomGemRichStoneForThisLevel(int level)
	{
		int whichGem = mineRandom.Next(59, 70);
		whichGem += whichGem % 2;
		if (Game1.player.timesReachedMineBottom == 0)
		{
			if (level < 40 && whichGem != 66 && whichGem != 68)
			{
				whichGem = mineRandom.Choose(66, 68);
			}
			else if (level < 80 && (whichGem == 64 || whichGem == 60))
			{
				whichGem = mineRandom.Choose(66, 70, 68, 62);
			}
		}
		return whichGem switch
		{
			66 => "8", 
			68 => "10", 
			60 => "12", 
			70 => "6", 
			64 => "4", 
			62 => "14", 
			_ => 40.ToString(), 
		};
	}

	public float getDistanceFromStart(int xTile, int yTile)
	{
		float distance = Utility.distance(xTile, tileBeneathLadder.X, yTile, tileBeneathLadder.Y);
		if (tileBeneathElevator != Vector2.Zero)
		{
			distance = Math.Min(distance, Utility.distance(xTile, tileBeneathElevator.X, yTile, tileBeneathElevator.Y));
		}
		return distance;
	}

	public Monster getMonsterForThisLevel(int level, int xTile, int yTile)
	{
		Vector2 position = new Vector2(xTile, yTile) * 64f;
		float distanceFromLadder = getDistanceFromStart(xTile, yTile);
		if (isSlimeArea)
		{
			if (GetAdditionalDifficulty() <= 0)
			{
				if (mineRandom.NextDouble() < 0.2)
				{
					return new BigSlime(position, getMineArea());
				}
				return new GreenSlime(position, mineLevel);
			}
			if (mineLevel < 20)
			{
				return new GreenSlime(position, mineLevel);
			}
			if (mineLevel < 30)
			{
				return new BlueSquid(position);
			}
			if (mineLevel < 40)
			{
				return new RockGolem(position, this);
			}
			if (mineLevel < 50)
			{
				if (mineRandom.NextDouble() < 0.15 && distanceFromLadder >= 10f)
				{
					return new Fly(position);
				}
				return new Grub(position);
			}
			if (mineLevel < 70)
			{
				return new Leaper(position);
			}
		}
		else if (isDinoArea)
		{
			if (mineRandom.NextDouble() < 0.1)
			{
				return new Bat(position, 999);
			}
			if (mineRandom.NextDouble() < 0.1)
			{
				return new Fly(position, hard: true);
			}
			return new DinoMonster(position);
		}
		if (getMineArea() == 0 || getMineArea() == 10)
		{
			if (mineRandom.NextDouble() < 0.25 && !mustKillAllMonstersToAdvance())
			{
				return new Bug(position, mineRandom.Next(4), this);
			}
			if (level < 15)
			{
				if (doesTileHaveProperty(xTile, yTile, "Diggable", "Back") != null)
				{
					return new Duggy(position);
				}
				if (mineRandom.NextDouble() < 0.15)
				{
					return new RockCrab(position);
				}
				return new GreenSlime(position, level);
			}
			if (level <= 30)
			{
				if (doesTileHaveProperty(xTile, yTile, "Diggable", "Back") != null)
				{
					return new Duggy(position);
				}
				if (mineRandom.NextDouble() < 0.15)
				{
					return new RockCrab(position);
				}
				if (mineRandom.NextDouble() < 0.05 && distanceFromLadder > 10f && GetAdditionalDifficulty() <= 0)
				{
					return new Fly(position);
				}
				if (mineRandom.NextDouble() < 0.45)
				{
					return new GreenSlime(position, level);
				}
				if (GetAdditionalDifficulty() <= 0)
				{
					return new Grub(position);
				}
				if (distanceFromLadder > 9f)
				{
					return new BlueSquid(position);
				}
				if (mineRandom.NextDouble() < 0.01)
				{
					return new RockGolem(position, this);
				}
				return new GreenSlime(position, level);
			}
			if (level <= 40)
			{
				if (mineRandom.NextDouble() < 0.1 && distanceFromLadder > 10f)
				{
					return new Bat(position, level);
				}
				if (GetAdditionalDifficulty() > 0 && mineRandom.NextDouble() < 0.1)
				{
					return new Ghost(position, "Carbon Ghost");
				}
				return new RockGolem(position, this);
			}
		}
		else if (getMineArea() == 40)
		{
			if (mineLevel >= 70 && (mineRandom.NextDouble() < 0.75 || GetAdditionalDifficulty() > 0))
			{
				if (mineRandom.NextDouble() < 0.75 || GetAdditionalDifficulty() <= 0)
				{
					return new Skeleton(position, GetAdditionalDifficulty() > 0 && mineRandom.NextBool());
				}
				return new Bat(position, 77377);
			}
			if (mineRandom.NextDouble() < 0.3)
			{
				return new DustSpirit(position, mineRandom.NextDouble() < 0.8);
			}
			if (mineRandom.NextDouble() < 0.3 && distanceFromLadder > 10f)
			{
				return new Bat(position, mineLevel);
			}
			if (!ghostAdded && mineLevel > 50 && mineRandom.NextDouble() < 0.3 && distanceFromLadder > 10f)
			{
				ghostAdded = true;
				if (GetAdditionalDifficulty() > 0)
				{
					return new Ghost(position, "Putrid Ghost");
				}
				return new Ghost(position);
			}
			if (GetAdditionalDifficulty() > 0)
			{
				if (mineRandom.NextDouble() < 0.01)
				{
					RockCrab rockCrab = new RockCrab(position);
					rockCrab.makeStickBug();
					return rockCrab;
				}
				if (mineLevel >= 50)
				{
					return new Leaper(position);
				}
				if (mineRandom.NextDouble() < 0.7)
				{
					return new Grub(position);
				}
				return new GreenSlime(position, mineLevel);
			}
		}
		else if (getMineArea() == 80)
		{
			if (isDarkArea() && mineRandom.NextDouble() < 0.25)
			{
				return new Bat(position, mineLevel);
			}
			if (mineRandom.NextDouble() < ((GetAdditionalDifficulty() > 0) ? 0.05 : 0.15))
			{
				return new GreenSlime(position, getMineArea());
			}
			if (mineRandom.NextDouble() < 0.15)
			{
				return new MetalHead(position, getMineArea());
			}
			if (mineRandom.NextDouble() < 0.25)
			{
				return new ShadowBrute(position);
			}
			if (GetAdditionalDifficulty() > 0 && mineRandom.NextDouble() < 0.25)
			{
				return new Shooter(position, "Shadow Sniper");
			}
			if (mineRandom.NextDouble() < 0.25)
			{
				return new ShadowShaman(position);
			}
			if (mineRandom.NextDouble() < 0.25)
			{
				return new RockCrab(position, "Lava Crab");
			}
			if (mineRandom.NextDouble() < 0.2 && distanceFromLadder > 8f && mineLevel >= 90 && getTileIndexAt(xTile, yTile, "Back") != -1 && getTileIndexAt(xTile, yTile, "Front") == -1)
			{
				return new SquidKid(position);
			}
		}
		else
		{
			if (getMineArea() == 121)
			{
				if (loadedDarkArea)
				{
					if (mineRandom.NextDouble() < 0.18 && distanceFromLadder > 8f)
					{
						return new Ghost(position, "Carbon Ghost");
					}
					Mummy mummy = new Mummy(position);
					if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && getMineArea() == 121 && Game1.player.team.calicoStatueEffects.ContainsKey(9))
					{
						mummy.BuffForAdditionalDifficulty(2);
						mummy.speed *= 2;
						setMonsterTextureToDangerousVersion(mummy);
					}
					return mummy;
				}
				if (mineLevel % 20 == 0 && distanceFromLadder > 10f)
				{
					return new Bat(position, mineLevel);
				}
				if (mineLevel % 16 == 0 && !mustKillAllMonstersToAdvance())
				{
					if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && getMineArea() == 121 && Game1.player.team.calicoStatueEffects.ContainsKey(4))
					{
						return new Bug(position, mineRandom.Next(4), "Assassin Bug");
					}
					return new Bug(position, mineRandom.Next(4), this);
				}
				if (mineRandom.NextDouble() < 0.33 && distanceFromLadder > 10f)
				{
					if (GetAdditionalDifficulty() <= 0)
					{
						return new Serpent(position);
					}
					return new Serpent(position, "Royal Serpent");
				}
				if (mineRandom.NextDouble() < 0.33 && distanceFromLadder > 10f && mineLevel >= 171)
				{
					return new Bat(position, mineLevel);
				}
				if (mineLevel >= 126 && distanceFromLadder > 10f && mineRandom.NextDouble() < 0.04 && !mustKillAllMonstersToAdvance())
				{
					return new DinoMonster(position);
				}
				if (mineRandom.NextDouble() < 0.33 && !mustKillAllMonstersToAdvance())
				{
					if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && getMineArea() == 121 && Game1.player.team.calicoStatueEffects.ContainsKey(4))
					{
						return new Bug(position, mineRandom.Next(4), "Assassin Bug");
					}
					return new Bug(position, mineRandom.Next(4), this);
				}
				if (mineRandom.NextDouble() < 0.25)
				{
					return new GreenSlime(position, level);
				}
				if (mineLevel >= 146 && mineRandom.NextDouble() < 0.25)
				{
					return new RockCrab(position, "Iridium Crab");
				}
				if (GetAdditionalDifficulty() > 0 && mineRandom.NextDouble() < 0.2 && distanceFromLadder > 8f && getTileIndexAt(xTile, yTile, "Back") != -1 && getTileIndexAt(xTile, yTile, "Front") == -1)
				{
					return new SquidKid(position);
				}
				return new BigSlime(position, this);
			}
			if (getMineArea() == 77377)
			{
				if ((mineLevel == 77377 && yTile > 59) || (mineLevel != 77377 && mineLevel % 2 == 0))
				{
					GreenSlime slime = new GreenSlime(position, 77377);
					Vector2 tile = new Vector2(xTile, yTile);
					bool brown = false;
					for (int i = 0; i < brownSpots.Count; i++)
					{
						if (Vector2.Distance(tile, brownSpots[i]) < 4f)
						{
							brown = true;
							break;
						}
					}
					if (brown)
					{
						int red = Game1.random.Next(120, 200);
						slime.color.Value = new Color(red, red / 2, red / 4);
						while (Game1.random.NextDouble() < 0.33)
						{
							slime.objectsToDrop.Add("378");
						}
						slime.Health = (int)((float)slime.Health * 0.5f);
						slime.Speed += 2;
					}
					else
					{
						int colorBase = Game1.random.Next(120, 200);
						slime.color.Value = new Color(colorBase, colorBase, colorBase);
						while (Game1.random.NextDouble() < 0.33)
						{
							slime.objectsToDrop.Add("380");
						}
						slime.Speed = 1;
					}
					return slime;
				}
				if (yTile < 51 || mineLevel != 77377)
				{
					if (xTile >= 70)
					{
						Monster skel = new Skeleton(position, Game1.random.NextBool());
						skel.BuffForAdditionalDifficulty(mineRandom.Next(1, 3));
						setMonsterTextureToDangerousVersion(skel);
						return skel;
					}
					return new Bat(position, 77377);
				}
				return new Bat(position, 77377)
				{
					focusedOnFarmers = true
				};
			}
		}
		return new GreenSlime(position, level);
	}

	private Object createLitterObject(double chanceForPurpleStone, double chanceForMysticStone, double gemStoneChance, Vector2 tile)
	{
		Color stoneColor = Color.White;
		int stoneHealth = 1;
		if (GetAdditionalDifficulty() > 0 && mineLevel % 5 != 0 && mineRandom.NextDouble() < (double)GetAdditionalDifficulty() * 0.001 + (double)((float)mineLevel / 100000f) + Game1.player.team.AverageDailyLuck(this) / 13.0 + Game1.player.team.AverageLuckLevel(this) * 0.0001500000071246177)
		{
			return new Object("95", 1)
			{
				MinutesUntilReady = 25
			};
		}
		int whichStone;
		if (getMineArea() == 0 || getMineArea() == 10)
		{
			whichStone = mineRandom.Next(31, 42);
			if (mineLevel % 40 < 30 && whichStone >= 33 && whichStone < 38)
			{
				whichStone = mineRandom.Choose(32, 38);
			}
			else if (mineLevel % 40 >= 30)
			{
				whichStone = mineRandom.Choose(34, 36);
			}
			if (GetAdditionalDifficulty() > 0)
			{
				whichStone = mineRandom.Next(33, 37);
				stoneHealth = 5;
				if (Game1.random.NextDouble() < 0.33)
				{
					whichStone = 846;
				}
				else
				{
					stoneColor = new Color(Game1.random.Next(60, 90), Game1.random.Next(150, 200), Game1.random.Next(190, 240));
				}
				if (isDarkArea())
				{
					whichStone = mineRandom.Next(32, 39);
					int tone = Game1.random.Next(130, 160);
					stoneColor = new Color(tone, tone, tone);
				}
				if (mineLevel != 1 && mineLevel % 5 != 0 && mineRandom.NextDouble() < 0.029)
				{
					return new Object("849", 1)
					{
						MinutesUntilReady = 6
					};
				}
				if (stoneColor.Equals(Color.White))
				{
					return new Object(whichStone.ToString(), 1)
					{
						MinutesUntilReady = stoneHealth
					};
				}
			}
			else if (mineLevel != 1 && mineLevel % 5 != 0 && mineRandom.NextDouble() < 0.029)
			{
				return new Object("751", 1)
				{
					MinutesUntilReady = 3
				};
			}
		}
		else if (getMineArea() == 40)
		{
			whichStone = mineRandom.Next(47, 54);
			stoneHealth = 3;
			if (GetAdditionalDifficulty() > 0 && mineLevel % 40 < 30)
			{
				whichStone = mineRandom.Next(39, 42);
				stoneHealth = 5;
				stoneColor = new Color(170, 255, 160);
				if (isDarkArea())
				{
					whichStone = mineRandom.Next(32, 39);
					int tone = Game1.random.Next(130, 160);
					stoneColor = new Color(tone, tone, tone);
				}
				if (mineRandom.NextDouble() < 0.15)
				{
					return new ColoredObject((294 + mineRandom.Choose(1, 0)).ToString(), 1, new Color(170, 140, 155))
					{
						MinutesUntilReady = 6,
						CanBeSetDown = true,
						ColorSameIndexAsParentSheetIndex = true,
						Flipped = mineRandom.NextBool()
					};
				}
				if (mineLevel != 1 && mineLevel % 5 != 0 && mineRandom.NextDouble() < 0.029)
				{
					return new ColoredObject("290", 1, new Color(150, 225, 160))
					{
						MinutesUntilReady = 6,
						CanBeSetDown = true,
						ColorSameIndexAsParentSheetIndex = true,
						Flipped = mineRandom.NextBool()
					};
				}
				if (stoneColor.Equals(Color.White))
				{
					return new Object(whichStone.ToString(), 1)
					{
						MinutesUntilReady = stoneHealth
					};
				}
			}
			else if (mineLevel % 5 != 0 && mineRandom.NextDouble() < 0.029)
			{
				return new Object("290", 1)
				{
					MinutesUntilReady = 4
				};
			}
		}
		else if (getMineArea() == 80)
		{
			stoneHealth = 4;
			whichStone = ((mineRandom.NextDouble() < 0.3 && !isDarkArea()) ? ((!mineRandom.NextBool()) ? 32 : 38) : ((mineRandom.NextDouble() < 0.3) ? mineRandom.Next(55, 58) : ((!mineRandom.NextBool()) ? 762 : 760)));
			if (GetAdditionalDifficulty() > 0)
			{
				whichStone = ((!mineRandom.NextBool()) ? 32 : 38);
				stoneHealth = 5;
				stoneColor = new Color(Game1.random.Next(140, 190), Game1.random.Next(90, 120), Game1.random.Next(210, 255));
				if (isDarkArea())
				{
					whichStone = mineRandom.Next(32, 39);
					int tone = Game1.random.Next(130, 160);
					stoneColor = new Color(tone, tone, tone);
				}
				if (mineLevel != 1 && mineLevel % 5 != 0 && mineRandom.NextDouble() < 0.029)
				{
					return new Object("764", 1)
					{
						MinutesUntilReady = 7
					};
				}
				if (stoneColor.Equals(Color.White))
				{
					return new Object(whichStone.ToString(), 1)
					{
						MinutesUntilReady = stoneHealth
					};
				}
			}
			else if (mineLevel % 5 != 0 && mineRandom.NextDouble() < 0.029)
			{
				return new Object("764", 1)
				{
					MinutesUntilReady = 8
				};
			}
		}
		else
		{
			if (getMineArea() == 77377)
			{
				stoneHealth = 5;
				bool foundSomething = false;
				foreach (Vector2 v in Utility.getAdjacentTileLocations(tile))
				{
					if (objects.ContainsKey(v))
					{
						foundSomething = true;
						break;
					}
				}
				if (!foundSomething && mineRandom.NextDouble() < 0.45)
				{
					return null;
				}
				bool brownSpot = false;
				for (int i = 0; i < brownSpots.Count; i++)
				{
					if (Vector2.Distance(tile, brownSpots[i]) < 4f)
					{
						brownSpot = true;
						break;
					}
					if (Vector2.Distance(tile, brownSpots[i]) < 6f)
					{
						return null;
					}
				}
				if (tile.X > 50f)
				{
					whichStone = Game1.random.Choose(668, 670);
					if (mineRandom.NextDouble() < 0.09 + Game1.player.team.AverageDailyLuck(this) / 2.0)
					{
						return new Object(Game1.random.Choose("BasicCoalNode0", "BasicCoalNode1"), 1)
						{
							MinutesUntilReady = 5
						};
					}
					if (mineRandom.NextDouble() < 0.25)
					{
						return null;
					}
				}
				else if (brownSpot)
				{
					whichStone = mineRandom.Choose(32, 38);
					if (mineRandom.NextDouble() < 0.01)
					{
						return new Object("751", 1)
						{
							MinutesUntilReady = 3
						};
					}
				}
				else
				{
					whichStone = mineRandom.Choose(34, 36);
					if (mineRandom.NextDouble() < 0.01)
					{
						return new Object("290", 1)
						{
							MinutesUntilReady = 3
						};
					}
				}
				return new Object(whichStone.ToString(), 1)
				{
					MinutesUntilReady = stoneHealth
				};
			}
			stoneHealth = 5;
			whichStone = (mineRandom.NextBool() ? ((!mineRandom.NextBool()) ? 32 : 38) : ((!mineRandom.NextBool()) ? 42 : 40));
			int skullCavernMineLevel = mineLevel - 120;
			double chanceForOre = 0.02 + (double)skullCavernMineLevel * 0.0005;
			if (mineLevel >= 130)
			{
				chanceForOre += 0.01 * (double)((float)(Math.Min(100, skullCavernMineLevel) - 10) / 10f);
			}
			double iridiumBoost = 0.0;
			if (mineLevel >= 130)
			{
				iridiumBoost += 0.001 * (double)((float)(skullCavernMineLevel - 10) / 10f);
			}
			iridiumBoost = Math.Min(iridiumBoost, 0.004);
			if (skullCavernMineLevel > 100)
			{
				iridiumBoost += (double)skullCavernMineLevel / 1000000.0;
			}
			if (!netIsTreasureRoom.Value && mineRandom.NextDouble() < chanceForOre)
			{
				double chanceForIridium = (double)Math.Min(100, skullCavernMineLevel) * (0.0003 + iridiumBoost);
				double chanceForGold = 0.01 + (double)(mineLevel - Math.Min(150, skullCavernMineLevel)) * 0.0005;
				double chanceForIron = Math.Min(0.5, 0.1 + (double)(mineLevel - Math.Min(200, skullCavernMineLevel)) * 0.005);
				if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0 && mineRandom.NextBool(0.13 + (double)((float)((int)Game1.player.team.calicoEggSkullCavernRating * 5) / 1000f)))
				{
					return new Object("CalicoEggStone_" + mineRandom.Next(3), 1)
					{
						MinutesUntilReady = 8
					};
				}
				if (mineRandom.NextDouble() < chanceForIridium)
				{
					return new Object("765", 1)
					{
						MinutesUntilReady = 16
					};
				}
				if (mineRandom.NextDouble() < chanceForGold)
				{
					return new Object("764", 1)
					{
						MinutesUntilReady = 8
					};
				}
				if (mineRandom.NextDouble() < chanceForIron)
				{
					return new Object("290", 1)
					{
						MinutesUntilReady = 4
					};
				}
				return new Object("751", 1)
				{
					MinutesUntilReady = 2
				};
			}
		}
		double averageDailyLuck = Game1.player.team.AverageDailyLuck(this);
		double averageMiningLevel = Game1.player.team.AverageSkillLevel(3, Game1.currentLocation);
		double chanceModifier = averageDailyLuck + averageMiningLevel * 0.005;
		if (mineLevel > 50 && mineRandom.NextDouble() < 0.00025 + (double)mineLevel / 120000.0 + 0.0005 * chanceModifier / 2.0)
		{
			whichStone = 2;
			stoneHealth = 10;
		}
		else if (gemStoneChance != 0.0 && mineRandom.NextDouble() < gemStoneChance + gemStoneChance * chanceModifier + (double)mineLevel / 24000.0)
		{
			return new Object(getRandomGemRichStoneForThisLevel(mineLevel), 1)
			{
				MinutesUntilReady = 5
			};
		}
		if (mineRandom.NextDouble() < chanceForPurpleStone / 2.0 + chanceForPurpleStone * averageMiningLevel * 0.008 + chanceForPurpleStone * (averageDailyLuck / 2.0))
		{
			whichStone = 44;
		}
		if (mineLevel > 100 && mineRandom.NextDouble() < chanceForMysticStone + chanceForMysticStone * averageMiningLevel * 0.008 + chanceForMysticStone * (averageDailyLuck / 2.0))
		{
			whichStone = 46;
		}
		whichStone += whichStone % 2;
		if (mineRandom.NextDouble() < 0.1 && getMineArea() != 40)
		{
			if (!stoneColor.Equals(Color.White))
			{
				return new ColoredObject(mineRandom.Choose("668", "670"), 1, stoneColor)
				{
					MinutesUntilReady = 2,
					ColorSameIndexAsParentSheetIndex = true,
					Flipped = mineRandom.NextBool()
				};
			}
			return new Object(mineRandom.Choose("668", "670"), 1)
			{
				MinutesUntilReady = 2,
				Flipped = mineRandom.NextBool()
			};
		}
		if (!stoneColor.Equals(Color.White))
		{
			return new ColoredObject(whichStone.ToString(), 1, stoneColor)
			{
				MinutesUntilReady = stoneHealth,
				ColorSameIndexAsParentSheetIndex = true,
				Flipped = mineRandom.NextBool()
			};
		}
		return new Object(whichStone.ToString(), 1)
		{
			MinutesUntilReady = stoneHealth
		};
	}

	public static void OnLeftMines()
	{
		if (!Game1.IsClient && !Game1.IsMultiplayer)
		{
			clearInactiveMines(keepUntickedLevels: false);
		}
		Game1.player.buffs.Remove("CalicoStatueSpeed");
	}

	public static void clearActiveMines()
	{
		activeMines.RemoveAll(delegate(MineShaft mine)
		{
			mine.mapContent.Dispose();
			return true;
		});
	}

	private static void clearInactiveMines(bool keepUntickedLevels = true)
	{
		int maxMineLevel = -1;
		int maxSkullLevel = -1;
		string[] disconnectLevels = (from fh in Game1.getAllFarmhands()
			select ((int)fh.disconnectDay != Game1.MasterPlayer.stats.DaysPlayed) ? null : fh.disconnectLocation.Value).ToArray();
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			if (farmer.locationBeforeForcedEvent.Value == null || !IsGeneratedLevel(farmer.locationBeforeForcedEvent.Value, out var player_mine_level))
			{
				continue;
			}
			if (player_mine_level > 120)
			{
				if (player_mine_level < 77377)
				{
					maxSkullLevel = Math.Max(maxSkullLevel, player_mine_level);
				}
			}
			else
			{
				maxMineLevel = Math.Max(maxMineLevel, player_mine_level);
			}
		}
		foreach (MineShaft mine in activeMines)
		{
			if (!mine.farmers.Any() && !disconnectLevels.Contains(mine.NameOrUniqueName))
			{
				continue;
			}
			if (mine.mineLevel > 120)
			{
				if (mine.mineLevel < 77377)
				{
					maxSkullLevel = Math.Max(maxSkullLevel, mine.mineLevel);
				}
			}
			else
			{
				maxMineLevel = Math.Max(maxMineLevel, mine.mineLevel);
			}
		}
		activeMines.RemoveAll(delegate(MineShaft mine)
		{
			if (mine.mineLevel == 77377)
			{
				return false;
			}
			if (disconnectLevels.Contains(mine.NameOrUniqueName))
			{
				return false;
			}
			if (mine.mineLevel > 120)
			{
				if (mine.mineLevel <= maxSkullLevel)
				{
					return false;
				}
			}
			else if (mine.mineLevel <= maxMineLevel)
			{
				return false;
			}
			if (mine.lifespan == 0 && keepUntickedLevels)
			{
				return false;
			}
			mine.mapContent.Dispose();
			return true;
		});
		if (activeMines.Count == 0)
		{
			Game1.player.team.calicoEggSkullCavernRating.Value = 0;
			Game1.player.team.calicoStatueEffects.Clear();
			deepestLevelOnCurrentDesertFestivalRun = 0;
		}
	}

	public static void UpdateMines10Minutes(int timeOfDay)
	{
		clearInactiveMines();
		if (Game1.IsClient)
		{
			return;
		}
		foreach (MineShaft mine in activeMines)
		{
			if (mine.farmers.Any())
			{
				mine.performTenMinuteUpdate(timeOfDay);
			}
			mine.lifespan++;
		}
	}

	protected override void updateCharacters(GameTime time)
	{
		if (farmers.Any())
		{
			base.updateCharacters(time);
		}
	}

	public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
	{
		base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
		if (!Game1.shouldTimePass() || !isFogUp)
		{
			return;
		}
		int oldTime = fogTime;
		fogTime -= (int)time.ElapsedGameTime.TotalMilliseconds;
		if (!Game1.IsMasterGame)
		{
			return;
		}
		if (fogTime > 5000 && oldTime % 4000 < fogTime % 4000)
		{
			spawnFlyingMonsterOffScreen();
		}
		if (fogTime <= 0)
		{
			isFogUp.Value = false;
			if (isDarkArea())
			{
				netFogColor.Value = Color.Black;
			}
			else if (GetAdditionalDifficulty() > 0 && getMineArea() == 40 && !isDarkArea())
			{
				netFogColor.Value = default(Color);
			}
		}
	}

	public static void UpdateMines(GameTime time)
	{
		foreach (MineShaft mine in activeMines)
		{
			if (mine.farmers.Any())
			{
				mine.UpdateWhenCurrentLocation(time);
			}
			mine.updateEvenIfFarmerIsntHere(time);
		}
	}

	/// <summary>Get the location name for a generated mine level.</summary>
	/// <param name="level">The mine level.</param>
	public static string GetLevelName(int level)
	{
		return "UndergroundMine" + level;
	}

	/// <summary>Get whether a location is a generated mine level.</summary>
	/// <param name="location">The location to check.</param>
	/// <param name="level">The parsed mine level, if applicable.</param>
	public static bool IsGeneratedLevel(GameLocation location, out int level)
	{
		if (location is MineShaft mine)
		{
			level = mine.mineLevel;
			return true;
		}
		level = 0;
		return false;
	}

	/// <summary>Get whether a location name is a generated mine level.</summary>
	/// <param name="locationName">The location name to check.</param>
	/// <param name="level">The parsed mine level, if applicable.</param>
	public static bool IsGeneratedLevel(string locationName, out int level)
	{
		if (locationName == null || !locationName.StartsWith("UndergroundMine", StringComparison.OrdinalIgnoreCase))
		{
			level = 0;
			return false;
		}
		return int.TryParse(locationName.Substring("UndergroundMine".Length), out level);
	}

	public static MineShaft GetMine(string name)
	{
		foreach (MineShaft mine in activeMines)
		{
			if (mine.Name.Equals(name))
			{
				return mine;
			}
		}
		if (!IsGeneratedLevel(name, out var mineLevel))
		{
			Game1.log.Warn("Failed parsing mine level from location name '" + name + "', defaulting to level 0.");
			mineLevel = 0;
		}
		MineShaft newMine = new MineShaft(mineLevel);
		activeMines.Add(newMine);
		newMine.generateContents();
		return newMine;
	}

	public static void ForEach(Action<MineShaft> action)
	{
		foreach (MineShaft mine in activeMines)
		{
			action(mine);
		}
	}
}
