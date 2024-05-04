using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Netcode.Validation;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.Characters;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley;

public class Farm : GameLocation
{
	public class LightningStrikeEvent : NetEventArg
	{
		public Vector2 boltPosition;

		public bool createBolt;

		public bool bigFlash;

		public bool smallFlash;

		public bool destroyedTerrainFeature;

		public void Read(BinaryReader reader)
		{
			createBolt = reader.ReadBoolean();
			bigFlash = reader.ReadBoolean();
			smallFlash = reader.ReadBoolean();
			destroyedTerrainFeature = reader.ReadBoolean();
			boltPosition.X = reader.ReadInt32();
			boltPosition.Y = reader.ReadInt32();
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(createBolt);
			writer.Write(bigFlash);
			writer.Write(smallFlash);
			writer.Write(destroyedTerrainFeature);
			writer.Write((int)boltPosition.X);
			writer.Write((int)boltPosition.Y);
		}
	}

	[XmlIgnore]
	[NonInstancedStatic]
	public static Texture2D houseTextures = Game1.content.Load<Texture2D>("Buildings\\houses");

	/// <summary>Obsolete. This is only kept to preserve data from old save files. Use <see cref="F:StardewValley.Buildings.Building.netBuildingPaintColor" /> instead.</summary>
	[NotNetField]
	public NetRef<BuildingPaintColor> housePaintColor = new NetRef<BuildingPaintColor>();

	public const int default_layout = 0;

	public const int riverlands_layout = 1;

	public const int forest_layout = 2;

	public const int mountains_layout = 3;

	public const int combat_layout = 4;

	public const int fourCorners_layout = 5;

	public const int beach_layout = 6;

	public const int mod_layout = 7;

	public const int layout_max = 7;

	[XmlElement("grandpaScore")]
	public readonly NetInt grandpaScore = new NetInt(0);

	[XmlElement("farmCaveReady")]
	public NetBool farmCaveReady = new NetBool(value: false);

	private TemporaryAnimatedSprite shippingBinLid;

	private Microsoft.Xna.Framework.Rectangle shippingBinLidOpenArea = new Microsoft.Xna.Framework.Rectangle(4480, 832, 256, 192);

	[XmlIgnore]
	private readonly NetRef<Inventory> sharedShippingBin = new NetRef<Inventory>(new Inventory());

	[XmlIgnore]
	public Item lastItemShipped;

	public bool hasSeenGrandpaNote;

	protected Dictionary<string, Dictionary<Point, Tile>> _baseSpouseAreaTiles = new Dictionary<string, Dictionary<Point, Tile>>();

	[XmlIgnore]
	public bool hasMatureFairyRoseTonight;

	[XmlElement("greenhouseUnlocked")]
	public readonly NetBool greenhouseUnlocked = new NetBool();

	[XmlElement("greenhouseMoved")]
	public readonly NetBool greenhouseMoved = new NetBool();

	private readonly NetEvent1Field<Vector2, NetVector2> spawnCrowEvent = new NetEvent1Field<Vector2, NetVector2>();

	public readonly NetEvent1<LightningStrikeEvent> lightningStrikeEvent = new NetEvent1<LightningStrikeEvent>();

	[XmlIgnore]
	public Point? mapGrandpaShrinePosition;

	[XmlIgnore]
	public Point? mapMainMailboxPosition;

	[XmlIgnore]
	public Point? mainFarmhouseEntry;

	[XmlIgnore]
	public Vector2? mapSpouseAreaCorner;

	[XmlIgnore]
	public Vector2? mapShippingBinPosition;

	protected Microsoft.Xna.Framework.Rectangle? _mountainForageRectangle;

	protected bool? _shouldSpawnForestFarmForage;

	protected bool? _shouldSpawnBeachFarmForage;

	protected bool? _oceanCrabPotOverride;

	protected string _fishLocationOverride;

	protected float _fishChanceOverride;

	public Point spousePatioSpot;

	public const int numCropsForCrow = 16;

	public Farm()
	{
	}

	public Farm(string mapPath, string name)
		: base(mapPath, name)
	{
		isAlwaysActive.Value = true;
	}

	public override bool IsBuildableLocation()
	{
		return true;
	}

	/// <inheritdoc />
	public override void AddDefaultBuildings(bool load = true)
	{
		AddDefaultBuilding("Farmhouse", GetStarterFarmhouseLocation(), load);
		AddDefaultBuilding("Greenhouse", GetGreenhouseStartLocation(), load);
		AddDefaultBuilding("Shipping Bin", GetStarterShippingBinLocation(), load);
		AddDefaultBuilding("Pet Bowl", GetStarterPetBowlLocation(), load);
		BuildStartingCabins();
	}

	public override string GetDisplayName()
	{
		return base.GetDisplayName() ?? Game1.content.LoadString("Strings\\StringsFromCSFiles:MapPage.cs.11064", Game1.player.farmName.Value);
	}

	/// <summary>Get the tile position at which the shipping bin should be created when it's missing.</summary>
	public virtual Vector2 GetStarterShippingBinLocation()
	{
		if (!mapShippingBinPosition.HasValue)
		{
			if (!TryGetMapPropertyAs("ShippingBinLocation", out Vector2 position, required: false))
			{
				position = new Vector2(71f, 14f);
			}
			mapShippingBinPosition = position;
		}
		return mapShippingBinPosition.Value;
	}

	/// <summary>Get the tile position at which the pet bowl should be created when it's missing.</summary>
	public virtual Vector2 GetStarterPetBowlLocation()
	{
		if (!TryGetMapPropertyAs("PetBowlLocation", out Vector2 tile, required: false))
		{
			return new Vector2(53f, 7f);
		}
		return tile;
	}

	/// <summary>Get the tile position at which the farmhouse should be created when it's missing.</summary>
	/// <remarks>See also <see cref="M:StardewValley.Farm.GetMainFarmHouseEntry" />.</remarks>
	public virtual Vector2 GetStarterFarmhouseLocation()
	{
		Point entry = GetMainFarmHouseEntry();
		return new Vector2(entry.X - 5, entry.Y - 3);
	}

	/// <summary>Get the tile position at which the greenhouse should be created when it's missing.</summary>
	public virtual Vector2 GetGreenhouseStartLocation()
	{
		if (TryGetMapPropertyAs("GreenhouseLocation", out Vector2 position, required: false))
		{
			return position;
		}
		return Game1.whichFarm switch
		{
			5 => new Vector2(36f, 29f), 
			6 => new Vector2(14f, 14f), 
			_ => new Vector2(25f, 10f), 
		};
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(sharedShippingBin, "sharedShippingBin").AddField(spawnCrowEvent, "spawnCrowEvent").AddField(lightningStrikeEvent, "lightningStrikeEvent")
			.AddField(grandpaScore, "grandpaScore")
			.AddField(greenhouseUnlocked, "greenhouseUnlocked")
			.AddField(greenhouseMoved, "greenhouseMoved")
			.AddField(farmCaveReady, "farmCaveReady");
		spawnCrowEvent.onEvent += doSpawnCrow;
		lightningStrikeEvent.onEvent += doLightningStrike;
		greenhouseMoved.fieldChangeVisibleEvent += delegate
		{
			ClearGreenhouseGrassTiles();
		};
	}

	public virtual void ClearGreenhouseGrassTiles()
	{
		if (map != null && Game1.gameMode != 6 && greenhouseMoved.Value)
		{
			switch (Game1.whichFarm)
			{
			case 0:
			case 3:
			case 4:
				ApplyMapOverride("Farm_Greenhouse_Dirt", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle((int)GetGreenhouseStartLocation().X, (int)GetGreenhouseStartLocation().Y, 9, 6));
				break;
			case 5:
				ApplyMapOverride("Farm_Greenhouse_Dirt_FourCorners", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle((int)GetGreenhouseStartLocation().X, (int)GetGreenhouseStartLocation().Y, 9, 6));
				break;
			case 1:
			case 2:
				break;
			}
		}
	}

	public static string getMapNameFromTypeInt(int type)
	{
		switch (type)
		{
		case 0:
			return "Farm";
		case 1:
			return "Farm_Fishing";
		case 2:
			return "Farm_Foraging";
		case 3:
			return "Farm_Mining";
		case 4:
			return "Farm_Combat";
		case 5:
			return "Farm_FourCorners";
		case 6:
			return "Farm_Island";
		case 7:
			if (Game1.whichModFarm != null)
			{
				return Game1.whichModFarm.MapName;
			}
			break;
		}
		return "Farm";
	}

	public void onNewGame()
	{
		if (Game1.whichFarm == 3 || ShouldSpawnMountainOres())
		{
			for (int i = 0; i < 28; i++)
			{
				doDailyMountainFarmUpdate();
			}
		}
		else if (Game1.whichFarm == 5)
		{
			for (int i = 0; i < 10; i++)
			{
				doDailyMountainFarmUpdate();
			}
		}
		else if (Game1.whichFarm == 7 && Game1.whichModFarm.Id == "MeadowlandsFarm")
		{
			for (int x = 47; x < 63; x++)
			{
				objects.Add(new Vector2(x, 20f), new Fence(new Vector2(x, 20f), "322", isGate: false));
			}
			for (int y = 16; y < 20; y++)
			{
				objects.Add(new Vector2(47f, y), new Fence(new Vector2(47f, y), "322", isGate: false));
			}
			for (int y = 7; y < 20; y++)
			{
				objects.Add(new Vector2(62f, y), new Fence(new Vector2(62f, y), "322", y == 13));
			}
			Building b = new Building("Coop", new Vector2(54f, 9f));
			b.FinishConstruction(onGameStart: true);
			b.LoadFromBuildingData(b.GetData(), forUpgrade: false, forConstruction: true);
			FarmAnimal starterChicken = new FarmAnimal("White Chicken", Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
			FarmAnimal starterChicken2 = new FarmAnimal("Brown Chicken", Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
			string[] chickenSplit = Game1.content.LoadString("Strings\\1_6_Strings:StarterChicken_Names").Split('|');
			string chickenNames = chickenSplit[Game1.random.Next(chickenSplit.Length)];
			starterChicken.Name = chickenNames.Split(',')[0].Trim();
			starterChicken2.Name = chickenNames.Split(',')[1].Trim();
			(b.GetIndoors() as AnimalHouse).adoptAnimal(starterChicken);
			(b.GetIndoors() as AnimalHouse).adoptAnimal(starterChicken2);
			buildings.Add(b);
		}
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		UpdatePatio();
		for (int i = characters.Count - 1; i >= 0; i--)
		{
			if (characters[i] is Pet pet && (getTileIndexAt(pet.TilePoint, "Buildings") != -1 || getTileIndexAt(pet.TilePoint.X + 1, pet.TilePoint.Y, "Buildings") != -1 || !CanSpawnCharacterHere(pet.Tile) || !CanSpawnCharacterHere(new Vector2(pet.TilePoint.X + 1, pet.TilePoint.Y))))
			{
				pet.WarpToPetBowl();
			}
		}
		lastItemShipped = null;
		if (characters.Count > 5)
		{
			int slimesEscaped = 0;
			for (int i = characters.Count - 1; i >= 0; i--)
			{
				if (characters[i] is GreenSlime && Game1.random.NextDouble() < 0.035)
				{
					characters.RemoveAt(i);
					slimesEscaped++;
				}
			}
			if (slimesEscaped > 0)
			{
				Game1.multiplayer.broadcastGlobalMessage((slimesEscaped == 1) ? "Strings\\Locations:Farm_1SlimeEscaped" : "Strings\\Locations:Farm_NSlimesEscaped", false, null, slimesEscaped.ToString() ?? "");
			}
		}
		Vector2 key;
		if (Game1.whichFarm == 5)
		{
			if (CanItemBePlacedHere(new Vector2(5f, 32f), itemIsPassable: false, CollisionMask.All, CollisionMask.None) && CanItemBePlacedHere(new Vector2(6f, 32f), itemIsPassable: false, CollisionMask.All, CollisionMask.None) && CanItemBePlacedHere(new Vector2(6f, 33f), itemIsPassable: false, CollisionMask.All, CollisionMask.None) && CanItemBePlacedHere(new Vector2(5f, 33f), itemIsPassable: false, CollisionMask.All, CollisionMask.None))
			{
				resourceClumps.Add(new ResourceClump(600, 2, 2, new Vector2(5f, 32f)));
			}
			if (objects.Length > 0)
			{
				for (int i = 0; i < 6; i++)
				{
					if (Utility.TryGetRandom(objects, out key, out var o) && o.IsWeeds() && o.tileLocation.X < 36f && o.tileLocation.Y < 34f)
					{
						o.SetIdAndSprite(792 + Game1.seasonIndex);
					}
				}
			}
		}
		if (ShouldSpawnBeachFarmForage())
		{
			while (Game1.random.NextDouble() < 0.9)
			{
				Vector2 v = getRandomTile();
				if (!CanItemBePlacedHere(v) || getTileIndexAt((int)v.X, (int)v.Y, "AlwaysFront") != -1)
				{
					continue;
				}
				string whichItem = null;
				if (doesTileHavePropertyNoNull((int)v.X, (int)v.Y, "BeachSpawn", "Back") != "")
				{
					whichItem = "372";
					Game1.stats.Increment("beachFarmSpawns");
					switch (Game1.random.Next(6))
					{
					case 0:
						whichItem = "393";
						break;
					case 1:
						whichItem = "719";
						break;
					case 2:
						whichItem = "718";
						break;
					case 3:
						whichItem = "723";
						break;
					case 4:
					case 5:
						whichItem = "152";
						break;
					}
					if (Game1.stats.DaysPlayed > 1)
					{
						if (Game1.random.NextDouble() < 0.15 || Game1.stats.Get("beachFarmSpawns") % 4 == 0)
						{
							whichItem = Game1.random.Next(922, 925).ToString();
							objects.Add(v, new Object(whichItem, 1)
							{
								Fragility = 2,
								MinutesUntilReady = 3
							});
							whichItem = null;
						}
						else if (Game1.random.NextDouble() < 0.1)
						{
							whichItem = "397";
						}
						else if (Game1.random.NextDouble() < 0.05)
						{
							whichItem = "392";
						}
						else if (Game1.random.NextDouble() < 0.02)
						{
							whichItem = "394";
						}
					}
				}
				else if (Game1.season != Season.Winter && new Microsoft.Xna.Framework.Rectangle(20, 66, 33, 18).Contains((int)v.X, (int)v.Y) && doesTileHavePropertyNoNull((int)v.X, (int)v.Y, "Type", "Back") == "Grass")
				{
					whichItem = Utility.getRandomBasicSeasonalForageItem(Game1.season, (int)Game1.stats.DaysPlayed);
				}
				if (whichItem != null)
				{
					Object obj = ItemRegistry.Create<Object>("(O)" + whichItem);
					obj.CanBeSetDown = false;
					obj.IsSpawnedObject = true;
					dropObject(obj, v * 64f, Game1.viewport, initialPlacement: true);
				}
			}
		}
		if (Game1.whichFarm == 2)
		{
			for (int x = 0; x < 20; x++)
			{
				for (int y = 0; y < map.Layers[0].LayerHeight; y++)
				{
					if (getTileIndexAt(x, y, "Paths") == 21 && CanItemBePlacedHere(new Vector2(x, y), itemIsPassable: false, CollisionMask.All, CollisionMask.None) && CanItemBePlacedHere(new Vector2(x + 1, y), itemIsPassable: false, CollisionMask.All, CollisionMask.None) && CanItemBePlacedHere(new Vector2(x + 1, y + 1), itemIsPassable: false, CollisionMask.All, CollisionMask.None) && CanItemBePlacedHere(new Vector2(x, y + 1), itemIsPassable: false, CollisionMask.All, CollisionMask.None))
					{
						resourceClumps.Add(new ResourceClump(600, 2, 2, new Vector2(x, y)));
					}
				}
			}
		}
		if (ShouldSpawnForestFarmForage() && !Game1.IsWinter)
		{
			while (Game1.random.NextDouble() < 0.75)
			{
				Vector2 v = new Vector2(Game1.random.Next(18), Game1.random.Next(map.Layers[0].LayerHeight));
				if (Game1.random.NextBool() || Game1.whichFarm != 2)
				{
					v = getRandomTile();
				}
				if (CanItemBePlacedHere(v, itemIsPassable: false, CollisionMask.All, CollisionMask.None) && getTileIndexAt((int)v.X, (int)v.Y, "AlwaysFront") == -1 && ((Game1.whichFarm == 2 && v.X < 18f) || doesTileHavePropertyNoNull((int)v.X, (int)v.Y, "Type", "Back").Equals("Grass")))
				{
					Object obj = ItemRegistry.Create<Object>(Game1.season switch
					{
						Season.Spring => Game1.random.Next(4) switch
						{
							0 => "(O)" + 16, 
							1 => "(O)" + 22, 
							2 => "(O)" + 20, 
							_ => "(O)257", 
						}, 
						Season.Summer => Game1.random.Next(4) switch
						{
							0 => "(O)402", 
							1 => "(O)396", 
							2 => "(O)398", 
							_ => "(O)404", 
						}, 
						Season.Fall => Game1.random.Next(4) switch
						{
							0 => "(O)281", 
							1 => "(O)420", 
							2 => "(O)422", 
							_ => "(O)404", 
						}, 
						_ => "(O)792", 
					});
					obj.CanBeSetDown = false;
					obj.IsSpawnedObject = true;
					dropObject(obj, v * 64f, Game1.viewport, initialPlacement: true);
				}
			}
			if (objects.Length > 0)
			{
				for (int i = 0; i < 6; i++)
				{
					if (Utility.TryGetRandom(objects, out key, out var o) && o.IsWeeds())
					{
						o.SetIdAndSprite(792 + Game1.seasonIndex);
					}
				}
			}
		}
		if (Game1.whichFarm == 3 || Game1.whichFarm == 5 || ShouldSpawnMountainOres())
		{
			doDailyMountainFarmUpdate();
		}
		if (terrainFeatures.Length > 0 && Game1.season == Season.Fall && Game1.dayOfMonth > 1 && Game1.random.NextDouble() < 0.05)
		{
			for (int tries = 0; tries < 10; tries++)
			{
				if (Utility.TryGetRandom(terrainFeatures, out var _, out var feature) && feature is Tree tree && (int)tree.growthStage >= 5 && !tree.tapped && !tree.isTemporaryGreenRainTree.Value)
				{
					tree.treeType.Value = "7";
					tree.loadSprite();
					break;
				}
			}
		}
		addCrows();
		if (Game1.season != Season.Winter)
		{
			spawnWeedsAndStones((Game1.season == Season.Summer) ? 30 : 20);
		}
		spawnWeeds(weedsOnly: false);
		HandleGrassGrowth(dayOfMonth);
	}

	public void doDailyMountainFarmUpdate()
	{
		double chance = 1.0;
		while (Game1.random.NextDouble() < chance)
		{
			Vector2 v = (ShouldSpawnMountainOres() ? Utility.getRandomPositionInThisRectangle(_mountainForageRectangle.Value, Game1.random) : ((Game1.whichFarm == 5) ? Utility.getRandomPositionInThisRectangle(new Microsoft.Xna.Framework.Rectangle(51, 67, 11, 3), Game1.random) : Utility.getRandomPositionInThisRectangle(new Microsoft.Xna.Framework.Rectangle(5, 37, 22, 8), Game1.random)));
			if (doesTileHavePropertyNoNull((int)v.X, (int)v.Y, "Type", "Back").Equals("Dirt") && CanItemBePlacedHere(v, itemIsPassable: false, CollisionMask.All, CollisionMask.None))
			{
				string stone_id = "668";
				int health = 2;
				if (Game1.random.NextDouble() < 0.15)
				{
					objects.Add(v, ItemRegistry.Create<Object>("(O)590"));
					continue;
				}
				if (Game1.random.NextBool())
				{
					stone_id = "670";
				}
				if (Game1.random.NextDouble() < 0.1)
				{
					if (Game1.player.MiningLevel >= 8 && Game1.random.NextDouble() < 0.33)
					{
						stone_id = "77";
						health = 7;
					}
					else if (Game1.player.MiningLevel >= 5 && Game1.random.NextBool())
					{
						stone_id = "76";
						health = 5;
					}
					else
					{
						stone_id = "75";
						health = 3;
					}
				}
				if (Game1.random.NextDouble() < 0.21)
				{
					stone_id = "751";
					health = 3;
				}
				if (Game1.player.MiningLevel >= 4 && Game1.random.NextDouble() < 0.15)
				{
					stone_id = "290";
					health = 4;
				}
				if (Game1.player.MiningLevel >= 7 && Game1.random.NextDouble() < 0.1)
				{
					stone_id = "764";
					health = 8;
				}
				if (Game1.player.MiningLevel >= 10 && Game1.random.NextDouble() < 0.01)
				{
					stone_id = "765";
					health = 16;
				}
				objects.Add(v, new Object(stone_id, 10)
				{
					MinutesUntilReady = health
				});
			}
			chance *= 0.75;
		}
	}

	/// <inheritdoc />
	public override bool catchOceanCrabPotFishFromThisSpot(int x, int y)
	{
		if (map != null)
		{
			if (!_oceanCrabPotOverride.HasValue)
			{
				_oceanCrabPotOverride = map.Properties.ContainsKey("FarmOceanCrabPotOverride");
			}
			if (_oceanCrabPotOverride.Value)
			{
				return true;
			}
		}
		return base.catchOceanCrabPotFishFromThisSpot(x, y);
	}

	public void addCrows()
	{
		int numCrops = 0;
		foreach (KeyValuePair<Vector2, TerrainFeature> pair in terrainFeatures.Pairs)
		{
			if (pair.Value is HoeDirt { crop: not null })
			{
				numCrops++;
			}
		}
		List<Vector2> scarecrowPositions = new List<Vector2>();
		foreach (KeyValuePair<Vector2, Object> v in objects.Pairs)
		{
			if (v.Value.IsScarecrow())
			{
				scarecrowPositions.Add(v.Key);
			}
		}
		int potentialCrows = Math.Min(4, numCrops / 16);
		for (int i = 0; i < potentialCrows; i++)
		{
			if (!(Game1.random.NextDouble() < 0.3))
			{
				continue;
			}
			for (int attempts = 0; attempts < 10; attempts++)
			{
				if (!Utility.TryGetRandom(terrainFeatures, out var tile, out var feature) || !(feature is HoeDirt dirt) || (int)dirt.crop?.currentPhase <= 1)
				{
					continue;
				}
				bool scarecrow = false;
				foreach (Vector2 s in scarecrowPositions)
				{
					int radius = objects[s].GetRadiusForScarecrow();
					if (Vector2.Distance(s, tile) < (float)radius)
					{
						scarecrow = true;
						objects[s].SpecialVariable++;
						break;
					}
				}
				if (!scarecrow)
				{
					dirt.destroyCrop(showAnimation: false);
					spawnCrowEvent.Fire(tile);
				}
				break;
			}
		}
	}

	private void doSpawnCrow(Vector2 v)
	{
		if (critters == null && (bool)isOutdoors)
		{
			critters = new List<Critter>();
		}
		critters.Add(new Crow((int)v.X, (int)v.Y));
	}

	public static Point getFrontDoorPositionForFarmer(Farmer who)
	{
		Point entry_point = Game1.getFarm().GetMainFarmHouseEntry();
		entry_point.Y--;
		return entry_point;
	}

	public override void performTenMinuteUpdate(int timeOfDay)
	{
		base.performTenMinuteUpdate(timeOfDay);
		if (timeOfDay >= 1300 && Game1.IsMasterGame)
		{
			foreach (NPC n in new List<Character>(characters))
			{
				if (n.isMarried())
				{
					n.returnHomeFromFarmPosition(this);
				}
			}
		}
		foreach (NPC c in characters)
		{
			if (c.getSpouse() == Game1.player)
			{
				c.checkForMarriageDialogue(timeOfDay, this);
			}
			if (c is Child child)
			{
				child.tenMinuteUpdate();
			}
		}
		if (!Game1.spawnMonstersAtNight || Game1.farmEvent != null || Game1.timeOfDay < 1900 || !(Game1.random.NextDouble() < 0.25 - Game1.player.team.AverageDailyLuck() / 2.0))
		{
			return;
		}
		if (Game1.random.NextDouble() < 0.25)
		{
			if (Equals(Game1.currentLocation))
			{
				spawnFlyingMonstersOffScreen();
			}
		}
		else
		{
			spawnGroundMonsterOffScreen();
		}
	}

	public void spawnGroundMonsterOffScreen()
	{
		for (int i = 0; i < 15; i++)
		{
			Vector2 spawnLocation = getRandomTile();
			if (Utility.isOnScreen(Utility.Vector2ToPoint(spawnLocation), 64, this))
			{
				spawnLocation.X -= Game1.viewport.Width / 64;
			}
			if (!CanItemBePlacedHere(spawnLocation))
			{
				continue;
			}
			int combatLevel = Game1.player.CombatLevel;
			bool success;
			if (combatLevel >= 8 && Game1.random.NextDouble() < 0.15)
			{
				characters.Add(new ShadowBrute(spawnLocation * 64f)
				{
					focusedOnFarmers = true,
					wildernessFarmMonster = true
				});
				success = true;
			}
			else if (Game1.random.NextDouble() < ((Game1.whichFarm == 4) ? 0.66 : 0.33))
			{
				characters.Add(new RockGolem(spawnLocation * 64f, combatLevel)
				{
					wildernessFarmMonster = true
				});
				success = true;
			}
			else
			{
				int virtualMineLevel = 1;
				if (combatLevel >= 10)
				{
					virtualMineLevel = 140;
				}
				else if (combatLevel >= 8)
				{
					virtualMineLevel = 100;
				}
				else if (combatLevel >= 4)
				{
					virtualMineLevel = 41;
				}
				characters.Add(new GreenSlime(spawnLocation * 64f, virtualMineLevel)
				{
					wildernessFarmMonster = true
				});
				success = true;
			}
			if (!success || !Game1.currentLocation.Equals(this))
			{
				break;
			}
			{
				foreach (KeyValuePair<Vector2, Object> v in objects.Pairs)
				{
					if (v.Value?.QualifiedItemId == "(BC)83")
					{
						v.Value.shakeTimer = 1000;
						v.Value.showNextIndex.Value = true;
						Game1.currentLightSources.Add(new LightSource(4, v.Key * 64f + new Vector2(32f, 0f), 1f, Color.Cyan * 0.75f, (int)(v.Key.X * 797f + v.Key.Y * 13f + 666f), LightSource.LightContext.None, 0L));
					}
				}
				break;
			}
		}
	}

	public void spawnFlyingMonstersOffScreen()
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
			spawnLocation.X -= Game1.viewport.Width;
		}
		int combatLevel = Game1.player.CombatLevel;
		bool success;
		if (combatLevel >= 10 && Game1.random.NextDouble() < 0.01 && Game1.player.Items.ContainsId("(W)4"))
		{
			characters.Add(new Bat(spawnLocation * 64f, 9999)
			{
				focusedOnFarmers = true,
				wildernessFarmMonster = true
			});
			success = true;
		}
		else if (combatLevel >= 10 && Game1.random.NextDouble() < 0.25)
		{
			characters.Add(new Bat(spawnLocation * 64f, 172)
			{
				focusedOnFarmers = true,
				wildernessFarmMonster = true
			});
			success = true;
		}
		else if (combatLevel >= 10 && Game1.random.NextDouble() < 0.25)
		{
			characters.Add(new Serpent(spawnLocation * 64f)
			{
				focusedOnFarmers = true,
				wildernessFarmMonster = true
			});
			success = true;
		}
		else if (combatLevel >= 8 && Game1.random.NextBool())
		{
			characters.Add(new Bat(spawnLocation * 64f, 81)
			{
				focusedOnFarmers = true,
				wildernessFarmMonster = true
			});
			success = true;
		}
		else if (combatLevel >= 5 && Game1.random.NextBool())
		{
			characters.Add(new Bat(spawnLocation * 64f, 41)
			{
				focusedOnFarmers = true,
				wildernessFarmMonster = true
			});
			success = true;
		}
		else
		{
			characters.Add(new Bat(spawnLocation * 64f, 1)
			{
				focusedOnFarmers = true,
				wildernessFarmMonster = true
			});
			success = true;
		}
		if (!success || !Game1.currentLocation.Equals(this))
		{
			return;
		}
		foreach (KeyValuePair<Vector2, Object> v in objects.Pairs)
		{
			if (v.Value != null && (bool)v.Value.bigCraftable && v.Value.QualifiedItemId == "(BC)83")
			{
				v.Value.shakeTimer = 1000;
				v.Value.showNextIndex.Value = true;
				Game1.currentLightSources.Add(new LightSource(4, v.Key * 64f + new Vector2(32f, 0f), 1f, Color.Cyan * 0.75f, (int)(v.Key.X * 797f + v.Key.Y * 13f + 666f), LightSource.LightContext.None, 0L));
			}
		}
	}

	public virtual void requestGrandpaReevaluation()
	{
		grandpaScore.Value = 0;
		if (Game1.IsMasterGame)
		{
			Game1.player.eventsSeen.Remove("558292");
			Game1.player.eventsSeen.Add("321777");
		}
		removeTemporarySpritesWithID(6666);
	}

	public override void OnMapLoad(Map map)
	{
		CacheOffBasePatioArea();
		base.OnMapLoad(map);
	}

	/// <inheritdoc />
	public override void OnBuildingMoved(Building building)
	{
		base.OnBuildingMoved(building);
		if (building.HasIndoorsName("FarmHouse"))
		{
			UnsetFarmhouseValues();
		}
		if (building is GreenhouseBuilding)
		{
			greenhouseMoved.Value = true;
		}
		if (building.GetIndoors() is FarmHouse house && house.HasNpcSpouseOrRoommate())
		{
			NPC npc = getCharacterFromName(house.owner.spouse);
			if (npc != null && !npc.shouldPlaySpousePatioAnimation.Value)
			{
				Game1.player.team.requestNPCGoHome.Fire(npc.Name);
			}
		}
	}

	/// <inheritdoc />
	public override bool ShouldExcludeFromNpcPathfinding()
	{
		return true;
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		Point shrine_position = GetGrandpaShrinePosition();
		if (tileLocation.X >= shrine_position.X - 1 && tileLocation.X <= shrine_position.X + 1 && tileLocation.Y == shrine_position.Y)
		{
			if (!hasSeenGrandpaNote)
			{
				Game1.addMail("hasSeenGrandpaNote", noLetter: true);
				hasSeenGrandpaNote = true;
				Game1.activeClickableMenu = new LetterViewerMenu(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaNote", Game1.player.Name).Replace('\n', '^'));
				return true;
			}
			if (Game1.year >= 3 && (int)grandpaScore > 0 && (int)grandpaScore < 4)
			{
				if (who.ActiveObject?.QualifiedItemId == "(O)72" && (int)grandpaScore < 4)
				{
					who.reduceActiveItemByOne();
					playSound("stoneStep");
					playSound("fireball");
					DelayedAction.playSoundAfterDelay("yoba", 800, this);
					DelayedAction.showDialogueAfterDelay(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaShrine_PlaceDiamond"), 1200);
					Game1.multiplayer.broadcastGrandpaReevaluation();
					Game1.player.freezePause = 1200;
					return true;
				}
				if (who.ActiveObject == null || who.ActiveObject.QualifiedItemId != "(O)72")
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Farm_GrandpaShrine_DiamondSlot"));
					return true;
				}
			}
			else
			{
				if ((int)grandpaScore >= 4 && !Utility.doesItemExistAnywhere("(BC)160"))
				{
					who.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(BC)160"), grandpaStatueCallback);
					return true;
				}
				if ((int)grandpaScore == 0 && Game1.year >= 3)
				{
					Game1.player.eventsSeen.Remove("558292");
					Game1.player.eventsSeen.Add("321777");
				}
			}
		}
		if (base.checkAction(tileLocation, viewport, who))
		{
			return true;
		}
		return false;
	}

	public void grandpaStatueCallback(Item item, Farmer who)
	{
		if (item is Object { QualifiedItemId: "(BC)160" })
		{
			who?.mailReceived.Add("grandpaPerfect");
		}
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		Farm fromFarm = (Farm)l;
		base.TransferDataFromSavedLocation(l);
		housePaintColor.Value = fromFarm.housePaintColor.Value;
		farmCaveReady.Value = fromFarm.farmCaveReady.Value;
		if (fromFarm.hasSeenGrandpaNote)
		{
			Game1.addMail("hasSeenGrandpaNote", noLetter: true);
		}
		UnsetFarmhouseValues();
	}

	public IInventory getShippingBin(Farmer who)
	{
		if ((bool)Game1.player.team.useSeparateWallets)
		{
			return who.personalShippingBin.Value;
		}
		return sharedShippingBin.Value;
	}

	public void shipItem(Item i, Farmer who)
	{
		if (i != null)
		{
			who.removeItemFromInventory(i);
			getShippingBin(who).Add(i);
			if (i is Object obj)
			{
				showShipment(obj, playThrowSound: false);
			}
			lastItemShipped = i;
			if (Game1.player.ActiveObject == null)
			{
				Game1.player.showNotCarrying();
				Game1.player.Halt();
			}
		}
	}

	public void UnsetFarmhouseValues()
	{
		mainFarmhouseEntry = null;
		mapMainMailboxPosition = null;
	}

	public void showShipment(Object o, bool playThrowSound = true)
	{
		if (playThrowSound)
		{
			localSound("backpackIN");
		}
		DelayedAction.playSoundAfterDelay("Ship", playThrowSound ? 250 : 0);
		int temp = Game1.random.Next();
		temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(524, 218, 34, 22), new Vector2(71f, 13f) * 64f + new Vector2(0f, 5f) * 4f, flipped: false, 0f, Color.White)
		{
			interval = 100f,
			totalNumberOfLoops = 1,
			animationLength = 3,
			pingPong = true,
			scale = 4f,
			layerDepth = 0.09601f,
			id = temp,
			extraInfoForEndBehavior = temp,
			endFunction = base.removeTemporarySpritesWithID
		});
		temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(524, 230, 34, 10), new Vector2(71f, 13f) * 64f + new Vector2(0f, 17f) * 4f, flipped: false, 0f, Color.White)
		{
			interval = 100f,
			totalNumberOfLoops = 1,
			animationLength = 3,
			pingPong = true,
			scale = 4f,
			layerDepth = 0.0963f,
			id = temp,
			extraInfoForEndBehavior = temp
		});
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(o.QualifiedItemId);
		temporarySprites.Add(new TemporaryAnimatedSprite(itemData.TextureName, itemData.GetSourceRect(), new Vector2(71f, 13f) * 64f + new Vector2(8 + Game1.random.Next(6), 2f) * 4f, flipped: false, 0f, Color.White)
		{
			interval = 9999f,
			scale = 4f,
			alphaFade = 0.045f,
			layerDepth = 0.096225f,
			motion = new Vector2(0f, 0.3f),
			acceleration = new Vector2(0f, 0.2f),
			scaleChange = -0.05f
		});
	}

	public override Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string location = null)
	{
		if (_fishLocationOverride == null)
		{
			_fishLocationOverride = "";
			string[] fields = GetMapPropertySplitBySpaces("FarmFishLocationOverride");
			if (fields.Length != 0)
			{
				if (!ArgUtility.TryGet(fields, 0, out var targetLocation, out var error) || !ArgUtility.TryGetFloat(fields, 1, out var chance, out error))
				{
					LogMapPropertyError("FarmFishLocationOverride", fields, error);
				}
				else
				{
					_fishLocationOverride = targetLocation;
					_fishChanceOverride = chance;
				}
			}
		}
		if (_fishChanceOverride > 0f && Game1.random.NextDouble() < (double)_fishChanceOverride)
		{
			return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile, _fishLocationOverride);
		}
		return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile);
	}

	protected override void resetSharedState()
	{
		base.resetSharedState();
		if (!greenhouseUnlocked.Value && Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccPantry"))
		{
			greenhouseUnlocked.Value = true;
		}
		for (int i = characters.Count - 1; i >= 0; i--)
		{
			if (Game1.timeOfDay >= 1300 && characters[i].isMarried() && characters[i].controller == null)
			{
				characters[i].Halt();
				characters[i].drawOffset = Vector2.Zero;
				characters[i].Sprite.StopAnimation();
				FarmHouse farmHouse = Game1.RequireLocation<FarmHouse>(characters[i].getSpouse().homeLocation.Value);
				Game1.warpCharacter(characters[i], characters[i].getSpouse().homeLocation.Value, farmHouse.getKitchenStandingSpot());
				break;
			}
		}
	}

	public virtual void UpdatePatio()
	{
		if (Game1.MasterPlayer.isMarriedOrRoommates() && Game1.MasterPlayer.spouse != null)
		{
			addSpouseOutdoorArea(Game1.MasterPlayer.spouse);
		}
		else
		{
			addSpouseOutdoorArea("");
		}
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		ClearGreenhouseGrassTiles();
		UpdatePatio();
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		hasSeenGrandpaNote = Game1.player.hasOrWillReceiveMail("hasSeenGrandpaNote");
		if (Game1.player.mailReceived.Add("button_tut_2"))
		{
			Game1.onScreenMenus.Add(new ButtonTutorialMenu(1));
		}
		for (int i = characters.Count - 1; i >= 0; i--)
		{
			if (characters[i] is Child child)
			{
				child.resetForPlayerEntry(this);
			}
		}
		addGrandpaCandles();
		if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && !Game1.player.mailReceived.Contains("Farm_Eternal_Parrots") && !IsRainingHere())
		{
			for (int i = 0; i < 20; i++)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Microsoft.Xna.Framework.Rectangle(49, 24 * Game1.random.Next(4), 24, 24), new Vector2(Game1.viewport.MaxCorner.X, Game1.viewport.Location.Y + Game1.random.Next(64, Game1.viewport.Height / 2)), flipped: false, 0f, Color.White)
				{
					scale = 4f,
					motion = new Vector2(-5f + (float)Game1.random.Next(-10, 11) / 10f, 4f + (float)Game1.random.Next(-10, 11) / 10f),
					acceleration = new Vector2(0f, -0.02f),
					animationLength = 3,
					interval = 100f,
					pingPong = true,
					totalNumberOfLoops = 999,
					delayBeforeAnimationStart = i * 250,
					drawAboveAlwaysFront = true,
					startSound = "batFlap"
				});
			}
			DelayedAction.playSoundAfterDelay("parrot_squawk", 1000);
			DelayedAction.playSoundAfterDelay("parrot_squawk", 4000);
			DelayedAction.playSoundAfterDelay("parrot", 3000);
			DelayedAction.playSoundAfterDelay("parrot", 5500);
			DelayedAction.playSoundAfterDelay("parrot_squawk", 7000);
			for (int i = 0; i < 20; i++)
			{
				DelayedAction.playSoundAfterDelay("batFlap", 5000 + i * 250);
			}
			Game1.player.mailReceived.Add("Farm_Eternal_Parrots");
		}
	}

	public virtual Vector2 GetSpouseOutdoorAreaCorner()
	{
		if (!mapSpouseAreaCorner.HasValue)
		{
			if (!TryGetMapPropertyAs("SpouseAreaLocation", out Vector2 position, required: false))
			{
				position = new Vector2(69f, 6f);
			}
			mapSpouseAreaCorner = position;
		}
		return mapSpouseAreaCorner.Value;
	}

	public virtual void CacheOffBasePatioArea()
	{
		_baseSpouseAreaTiles = new Dictionary<string, Dictionary<Point, Tile>>();
		List<string> layers_to_cache = new List<string>();
		foreach (Layer layer in map.Layers)
		{
			layers_to_cache.Add(layer.Id);
		}
		foreach (string layer_name in layers_to_cache)
		{
			Layer original_layer = map.GetLayer(layer_name);
			Dictionary<Point, Tile> tiles = new Dictionary<Point, Tile>();
			_baseSpouseAreaTiles[layer_name] = tiles;
			Vector2 spouse_area_corner = GetSpouseOutdoorAreaCorner();
			for (int x = (int)spouse_area_corner.X; x < (int)spouse_area_corner.X + 4; x++)
			{
				for (int y = (int)spouse_area_corner.Y; y < (int)spouse_area_corner.Y + 4; y++)
				{
					if (original_layer == null)
					{
						tiles[new Point(x, y)] = null;
					}
					else
					{
						tiles[new Point(x, y)] = original_layer.Tiles[x, y];
					}
				}
			}
		}
	}

	public virtual void ReapplyBasePatioArea()
	{
		foreach (string layer in _baseSpouseAreaTiles.Keys)
		{
			Layer map_layer = map.GetLayer(layer);
			foreach (Point location in _baseSpouseAreaTiles[layer].Keys)
			{
				Tile base_tile = _baseSpouseAreaTiles[layer][location];
				if (map_layer != null)
				{
					map_layer.Tiles[location.X, location.Y] = base_tile;
				}
			}
		}
	}

	public void addSpouseOutdoorArea(string spouseName)
	{
		ReapplyBasePatioArea();
		Point patio_corner = Utility.Vector2ToPoint(GetSpouseOutdoorAreaCorner());
		spousePatioSpot = new Point(patio_corner.X + 2, patio_corner.Y + 3);
		CharacterData spouseData;
		CharacterSpousePatioData patioData = (NPC.TryGetData(spouseName, out spouseData) ? spouseData.SpousePatio : null);
		if (patioData == null)
		{
			return;
		}
		string assetName = patioData.MapAsset ?? "spousePatios";
		Microsoft.Xna.Framework.Rectangle sourceArea = patioData.MapSourceRect;
		int width = Math.Min(sourceArea.Width, 4);
		int height = Math.Min(sourceArea.Height, 4);
		Point corner = patio_corner;
		Microsoft.Xna.Framework.Rectangle areaToRefurbish = new Microsoft.Xna.Framework.Rectangle(corner.X, corner.Y, width, height);
		Point fromOrigin = sourceArea.Location;
		if (_appliedMapOverrides.Contains("spouse_patio"))
		{
			_appliedMapOverrides.Remove("spouse_patio");
		}
		ApplyMapOverride(assetName, "spouse_patio", new Microsoft.Xna.Framework.Rectangle(fromOrigin.X, fromOrigin.Y, areaToRefurbish.Width, areaToRefurbish.Height), areaToRefurbish);
		foreach (Point tile in areaToRefurbish.GetPoints())
		{
			if (getTileIndexAt(tile, "Paths") == 7)
			{
				spousePatioSpot = tile;
				break;
			}
		}
	}

	public void addGrandpaCandles()
	{
		Point grandpa_shrine_location = GetGrandpaShrinePosition();
		if ((int)grandpaScore > 0)
		{
			Microsoft.Xna.Framework.Rectangle candleSource = new Microsoft.Xna.Framework.Rectangle(577, 1985, 2, 5);
			removeTemporarySpritesWithIDLocal(6666);
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2((grandpa_shrine_location.X - 1) * 64 + 20, (grandpa_shrine_location.Y - 1) * 64 + 20), flicker: false, flipped: false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((grandpa_shrine_location.X - 1) * 64 + 12, (grandpa_shrine_location.Y - 1) * 64 - 4), flipped: false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 7,
				light = true,
				id = 6666,
				lightRadius = 1f,
				scale = 3f,
				layerDepth = 0.038500004f,
				delayBeforeAnimationStart = 0
			});
			if ((int)grandpaScore > 1)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2((grandpa_shrine_location.X - 1) * 64 + 40, (grandpa_shrine_location.Y - 2) * 64 + 24), flicker: false, flipped: false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((grandpa_shrine_location.X - 1) * 64 + 36, (grandpa_shrine_location.Y - 2) * 64), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 7,
					light = true,
					id = 6666,
					lightRadius = 1f,
					scale = 3f,
					layerDepth = 0.038500004f,
					delayBeforeAnimationStart = 50
				});
			}
			if ((int)grandpaScore > 2)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2((grandpa_shrine_location.X + 1) * 64 + 20, (grandpa_shrine_location.Y - 2) * 64 + 24), flicker: false, flipped: false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((grandpa_shrine_location.X + 1) * 64 + 16, (grandpa_shrine_location.Y - 2) * 64), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 7,
					light = true,
					id = 6666,
					lightRadius = 1f,
					scale = 3f,
					layerDepth = 0.038500004f,
					delayBeforeAnimationStart = 100
				});
			}
			if ((int)grandpaScore > 3)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", candleSource, 99999f, 1, 9999, new Vector2((grandpa_shrine_location.X + 1) * 64 + 40, (grandpa_shrine_location.Y - 1) * 64 + 20), flicker: false, flipped: false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
				temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(536, 1945, 8, 8), new Vector2((grandpa_shrine_location.X + 1) * 64 + 36, (grandpa_shrine_location.Y - 1) * 64 - 4), flipped: false, 0f, Color.White)
				{
					interval = 50f,
					totalNumberOfLoops = 99999,
					animationLength = 7,
					light = true,
					id = 6666,
					lightRadius = 1f,
					scale = 3f,
					layerDepth = 0.038500004f,
					delayBeforeAnimationStart = 150
				});
			}
		}
		if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal"))
		{
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(176, 157, 15, 16), 99999f, 1, 9999, new Vector2(grandpa_shrine_location.X * 64 + 4, (grandpa_shrine_location.Y - 2) * 64 - 24), flicker: false, flipped: false, (float)((grandpa_shrine_location.Y - 1) * 64) / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f));
		}
	}

	private void openShippingBinLid()
	{
		if (shippingBinLid != null)
		{
			if (shippingBinLid.pingPongMotion != 1 && Game1.currentLocation == this)
			{
				localSound("doorCreak");
			}
			shippingBinLid.pingPongMotion = 1;
			shippingBinLid.paused = false;
		}
	}

	private void closeShippingBinLid()
	{
		if (shippingBinLid != null && shippingBinLid.currentParentTileIndex > 0)
		{
			if (shippingBinLid.pingPongMotion != -1 && Game1.currentLocation == this)
			{
				localSound("doorCreakReverse");
			}
			shippingBinLid.pingPongMotion = -1;
			shippingBinLid.paused = false;
		}
	}

	private void updateShippingBinLid(GameTime time)
	{
		if (isShippingBinLidOpen(requiredToBeFullyOpen: true) && shippingBinLid.pingPongMotion == 1)
		{
			shippingBinLid.paused = true;
		}
		else if (shippingBinLid.currentParentTileIndex == 0 && shippingBinLid.pingPongMotion == -1)
		{
			if (!shippingBinLid.paused && Game1.currentLocation == this)
			{
				localSound("woodyStep");
			}
			shippingBinLid.paused = true;
		}
		shippingBinLid.update(time);
	}

	private bool isShippingBinLidOpen(bool requiredToBeFullyOpen = false)
	{
		if (shippingBinLid != null && shippingBinLid.currentParentTileIndex >= ((!requiredToBeFullyOpen) ? 1 : (shippingBinLid.animationLength - 1)))
		{
			return true;
		}
		return false;
	}

	public override void pokeTileForConstruction(Vector2 tile)
	{
		base.pokeTileForConstruction(tile);
		foreach (NPC character in characters)
		{
			if (character is Pet pet && pet.Tile == tile)
			{
				pet.FacingDirection = Game1.random.Next(0, 4);
				pet.faceDirection(pet.FacingDirection);
				pet.CurrentBehavior = "Walk";
				pet.forceUpdateTimer = 2000;
				pet.setMovingInFacingDirection();
			}
		}
	}

	public override bool shouldShadowBeDrawnAboveBuildingsLayer(Vector2 p)
	{
		if (doesTileHaveProperty((int)p.X, (int)p.Y, "NoSpawn", "Back") == "All" && doesTileHaveProperty((int)p.X, (int)p.Y, "Type", "Back") == "Wood")
		{
			return true;
		}
		return base.shouldShadowBeDrawnAboveBuildingsLayer(p);
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (Game1.mailbox.Count > 0)
		{
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			Point mailbox_position = Game1.player.getMailboxPosition();
			float draw_layer = (float)((mailbox_position.X + 1) * 64) / 10000f + (float)(mailbox_position.Y * 64) / 10000f;
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(mailbox_position.X * 64, (float)(mailbox_position.Y * 64 - 96 - 48) + yOffset)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer + 1E-06f);
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(mailbox_position.X * 64 + 32 + 4, (float)(mailbox_position.Y * 64 - 64 - 24 - 8) + yOffset)), new Microsoft.Xna.Framework.Rectangle(189, 423, 15, 13), Color.White, 0f, new Vector2(7f, 6f), 4f, SpriteEffects.None, draw_layer + 1E-05f);
		}
		shippingBinLid?.draw(b);
		if (!hasSeenGrandpaNote)
		{
			Point grandpa_shrine = GetGrandpaShrinePosition();
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((grandpa_shrine.X + 1) * 64, grandpa_shrine.Y * 64)), new Microsoft.Xna.Framework.Rectangle(575, 1972, 11, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(grandpa_shrine.Y * 64) / 10000f + 1E-06f);
		}
	}

	public virtual Point GetMainMailboxPosition()
	{
		if (!mapMainMailboxPosition.HasValue)
		{
			if (!TryGetMapPropertyAs("MailboxLocation", out Point position, required: false))
			{
				position = new Point(68, 16);
			}
			mapMainMailboxPosition = position;
			Building farmhouse = GetMainFarmHouse();
			BuildingData buildingData = farmhouse?.GetData();
			if (buildingData?.ActionTiles != null)
			{
				foreach (BuildingActionTile action in buildingData.ActionTiles)
				{
					if (action.Action == "Mailbox")
					{
						mapMainMailboxPosition = new Point((int)farmhouse.tileX + action.Tile.X, (int)farmhouse.tileY + action.Tile.Y);
						break;
					}
				}
			}
		}
		return mapMainMailboxPosition.Value;
	}

	public virtual Point GetGrandpaShrinePosition()
	{
		if (!mapGrandpaShrinePosition.HasValue)
		{
			if (!TryGetMapPropertyAs("GrandpaShrineLocation", out Point position, required: false))
			{
				position = new Point(8, 7);
			}
			mapGrandpaShrinePosition = position;
		}
		return mapGrandpaShrinePosition.Value;
	}

	/// <summary>Get the door tile position for the farmhouse.</summary>
	/// <remarks>See also <see cref="M:StardewValley.Farm.GetStarterFarmhouseLocation" />.</remarks>
	public virtual Point GetMainFarmHouseEntry()
	{
		if (!mainFarmhouseEntry.HasValue)
		{
			if (!TryGetMapPropertyAs("FarmHouseEntry", out Point position, required: false))
			{
				position = new Point(64, 15);
			}
			mainFarmhouseEntry = position;
			Building farmhouse = GetMainFarmHouse();
			if (farmhouse != null)
			{
				mainFarmhouseEntry = new Point((int)farmhouse.tileX + farmhouse.humanDoor.X, (int)farmhouse.tileY + farmhouse.humanDoor.Y + 1);
			}
		}
		return mainFarmhouseEntry.Value;
	}

	/// <summary>Get the main player's farmhouse, if found.</summary>
	public virtual Building GetMainFarmHouse()
	{
		return getBuildingByType("Farmhouse");
	}

	public override void ResetForEvent(Event ev)
	{
		base.ResetForEvent(ev);
		if (ev.id != "-2")
		{
			Point main_farmhouse_entry = getFrontDoorPositionForFarmer(ev.farmer);
			main_farmhouse_entry.Y++;
			int offset_x = main_farmhouse_entry.X - 64;
			int offset_y = main_farmhouse_entry.Y - 15;
			ev.eventPositionTileOffset = new Vector2(offset_x, offset_y);
		}
	}

	public override void updateEvenIfFarmerIsntHere(GameTime time, bool skipWasUpdatedFlush = false)
	{
		spawnCrowEvent.Poll();
		lightningStrikeEvent.Poll();
		base.updateEvenIfFarmerIsntHere(time, skipWasUpdatedFlush);
	}

	public bool isTileOpenBesidesTerrainFeatures(Vector2 tile)
	{
		Microsoft.Xna.Framework.Rectangle boundingBox = new Microsoft.Xna.Framework.Rectangle((int)tile.X * 64, (int)tile.Y * 64, 64, 64);
		foreach (Building building in buildings)
		{
			if (building.intersects(boundingBox))
			{
				return false;
			}
		}
		foreach (ResourceClump resourceClump in resourceClumps)
		{
			if (resourceClump.getBoundingBox().Intersects(boundingBox))
			{
				return false;
			}
		}
		foreach (KeyValuePair<long, FarmAnimal> pair in animals.Pairs)
		{
			if (pair.Value.Tile == tile)
			{
				return true;
			}
		}
		if (!objects.ContainsKey(tile))
		{
			return isTilePassable(new Location((int)tile.X, (int)tile.Y), Game1.viewport);
		}
		return false;
	}

	private void doLightningStrike(LightningStrikeEvent lightning)
	{
		if (lightning.smallFlash)
		{
			if (Game1.currentLocation.IsOutdoors && !Game1.newDay && Game1.currentLocation.IsLightningHere())
			{
				Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
				if (Game1.random.NextBool())
				{
					DelayedAction.screenFlashAfterDelay((float)(0.3 + Game1.random.NextDouble()), Game1.random.Next(500, 1000));
				}
				DelayedAction.playSoundAfterDelay("thunder_small", Game1.random.Next(500, 1500));
			}
		}
		else if (lightning.bigFlash && Game1.currentLocation.IsOutdoors && Game1.currentLocation.IsLightningHere() && !Game1.newDay)
		{
			Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
			Game1.playSound("thunder");
		}
		if (lightning.createBolt && Game1.currentLocation.name.Equals("Farm"))
		{
			if (lightning.destroyedTerrainFeature)
			{
				temporarySprites.Add(new TemporaryAnimatedSprite(362, 75f, 6, 1, lightning.boltPosition, flicker: false, flipped: false));
			}
			Utility.drawLightningBolt(lightning.boltPosition, this);
		}
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		if (wasUpdated && Game1.gameMode != 0)
		{
			return;
		}
		base.UpdateWhenCurrentLocation(time);
		if (shippingBinLid == null)
		{
			return;
		}
		bool opening = false;
		foreach (Farmer farmer in farmers)
		{
			if (farmer.GetBoundingBox().Intersects(shippingBinLidOpenArea))
			{
				openShippingBinLid();
				opening = true;
			}
		}
		if (!opening)
		{
			closeShippingBinLid();
		}
		updateShippingBinLid(time);
	}

	public bool ShouldSpawnMountainOres()
	{
		if (!_mountainForageRectangle.HasValue)
		{
			_mountainForageRectangle = (TryGetMapPropertyAs("SpawnMountainFarmOreRect", out Microsoft.Xna.Framework.Rectangle area, required: false) ? area : Microsoft.Xna.Framework.Rectangle.Empty);
		}
		return _mountainForageRectangle.Value.Width > 0;
	}

	public bool ShouldSpawnForestFarmForage()
	{
		if (map != null)
		{
			if (!_shouldSpawnForestFarmForage.HasValue)
			{
				_shouldSpawnForestFarmForage = map.Properties.ContainsKey("SpawnForestFarmForage");
			}
			if (_shouldSpawnForestFarmForage.Value)
			{
				return true;
			}
		}
		return Game1.whichFarm == 2;
	}

	public bool ShouldSpawnBeachFarmForage()
	{
		if (map != null)
		{
			if (!_shouldSpawnBeachFarmForage.HasValue)
			{
				_shouldSpawnBeachFarmForage = map.Properties.ContainsKey("SpawnBeachFarmForage");
			}
			if (_shouldSpawnBeachFarmForage.Value)
			{
				return true;
			}
		}
		return Game1.whichFarm == 6;
	}

	public bool SpawnsForage()
	{
		if (!ShouldSpawnForestFarmForage())
		{
			return ShouldSpawnBeachFarmForage();
		}
		return true;
	}

	public bool doesFarmCaveNeedHarvesting()
	{
		return farmCaveReady.Value;
	}
}
