using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Netcode.Validation;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Util;
using xTile.Dimensions;

namespace StardewValley.Buildings;

[XmlInclude(typeof(Barn))]
[XmlInclude(typeof(Coop))]
[XmlInclude(typeof(FishPond))]
[XmlInclude(typeof(GreenhouseBuilding))]
[XmlInclude(typeof(JunimoHut))]
[XmlInclude(typeof(Mill))]
[XmlInclude(typeof(PetBowl))]
[XmlInclude(typeof(ShippingBin))]
[XmlInclude(typeof(Stable))]
[NotImplicitNetField]
public class Building : INetObject<NetFields>, IHaveModData
{
	/// <summary>A unique identifier for this specific building instance.</summary>
	[XmlElement("id")]
	public readonly NetGuid id = new NetGuid();

	[XmlIgnore]
	public Lazy<Texture2D> texture;

	[XmlIgnore]
	public Texture2D paintedTexture;

	public NetString skinId = new NetString();

	/// <summary>The indoor location created for this building, if any.</summary>
	/// <remarks>This is mutually exclusive with <see cref="F:StardewValley.Buildings.Building.nonInstancedIndoorsName" />. Most code should use <see cref="M:StardewValley.Buildings.Building.GetIndoors" /> instead, which handles both.</remarks>
	[XmlElement("indoors")]
	public readonly NetRef<GameLocation> indoors = new NetRef<GameLocation>();

	/// <summary>The unique ID of the separate location treated as the building interior (like <c>FarmHouse</c> for the farmhouse), if any.</summary>
	/// <remarks>This is mutually exclusive with <see cref="F:StardewValley.Buildings.Building.indoors" />. Most code should use <see cref="M:StardewValley.Buildings.Building.GetIndoors" /> instead, which handles both.</remarks>
	public readonly NetString nonInstancedIndoorsName = new NetString();

	[XmlElement("tileX")]
	public readonly NetInt tileX = new NetInt();

	[XmlElement("tileY")]
	public readonly NetInt tileY = new NetInt();

	[XmlElement("tilesWide")]
	public readonly NetInt tilesWide = new NetInt();

	[XmlElement("tilesHigh")]
	public readonly NetInt tilesHigh = new NetInt();

	[XmlElement("maxOccupants")]
	public readonly NetInt maxOccupants = new NetInt();

	[XmlElement("currentOccupants")]
	public readonly NetInt currentOccupants = new NetInt();

	[XmlElement("daysOfConstructionLeft")]
	public readonly NetInt daysOfConstructionLeft = new NetInt();

	[XmlElement("daysUntilUpgrade")]
	public readonly NetInt daysUntilUpgrade = new NetInt();

	[XmlElement("upgradeName")]
	public readonly NetString upgradeName = new NetString();

	[XmlElement("buildingType")]
	public readonly NetString buildingType = new NetString();

	[XmlElement("buildingPaintColor")]
	public NetRef<BuildingPaintColor> netBuildingPaintColor = new NetRef<BuildingPaintColor>();

	[XmlElement("hayCapacity")]
	public NetInt hayCapacity = new NetInt();

	public NetList<Chest, NetRef<Chest>> buildingChests = new NetList<Chest, NetRef<Chest>>();

	/// <summary>The unique name of the location which contains this building.</summary>
	[XmlIgnore]
	public NetString parentLocationName = new NetString();

	[XmlIgnore]
	public bool hasLoaded;

	[XmlIgnore]
	protected Dictionary<string, string> buildingMetadata = new Dictionary<string, string>();

	protected int lastHouseUpgradeLevel = -1;

	protected bool? hasChimney;

	protected Vector2 chimneyPosition = Vector2.Zero;

	protected int chimneyTimer = 500;

	[XmlElement("humanDoor")]
	public readonly NetPoint humanDoor = new NetPoint();

	[XmlElement("animalDoor")]
	public readonly NetPoint animalDoor = new NetPoint();

	/// <summary>A temporary color applied to the building sprite when it's highlighted in a menu.</summary>
	[XmlIgnore]
	public Color color = Color.White;

	[XmlElement("animalDoorOpen")]
	public readonly NetBool animalDoorOpen = new NetBool();

	[XmlElement("animalDoorOpenAmount")]
	public readonly NetFloat animalDoorOpenAmount = new NetFloat
	{
		InterpolationWait = false
	};

	[XmlElement("magical")]
	public readonly NetBool magical = new NetBool();

	/// <summary>Whether this building should fade into semi-transparency when the local player is behind it.</summary>
	[XmlElement("fadeWhenPlayerIsBehind")]
	public readonly NetBool fadeWhenPlayerIsBehind = new NetBool(value: true);

	[XmlElement("owner")]
	public readonly NetLong owner = new NetLong();

	[XmlElement("newConstructionTimer")]
	protected readonly NetInt newConstructionTimer = new NetInt();

	/// <summary>The building's opacity for the local player as a value between 0 (transparent) and 1 (opaque), accounting for <see cref="F:StardewValley.Buildings.Building.fadeWhenPlayerIsBehind" />.</summary>
	[XmlIgnore]
	protected float alpha = 1f;

	[XmlIgnore]
	protected bool _isMoving;

	public static Microsoft.Xna.Framework.Rectangle leftShadow = new Microsoft.Xna.Framework.Rectangle(656, 394, 16, 16);

	public static Microsoft.Xna.Framework.Rectangle middleShadow = new Microsoft.Xna.Framework.Rectangle(672, 394, 16, 16);

	public static Microsoft.Xna.Framework.Rectangle rightShadow = new Microsoft.Xna.Framework.Rectangle(688, 394, 16, 16);

	/// <inheritdoc />
	[XmlIgnore]
	public ModDataDictionary modData { get; } = new ModDataDictionary();


	/// <inheritdoc />
	[XmlElement("modData")]
	public ModDataDictionary modDataForSerialization
	{
		get
		{
			return modData.GetForSerialization();
		}
		set
		{
			modData.SetFromSerialization(value);
		}
	}

	/// <summary>Get whether this is a farmhand cabin.</summary>
	/// <remarks>To check whether a farmhand has claimed it, use <see cref="M:StardewValley.Buildings.Building.GetIndoors" /> to get the <see cref="T:StardewValley.Locations.Cabin" /> or <see cref="T:StardewValley.Locations.FarmHouse" /> instance and call methods like <see cref="P:StardewValley.Locations.FarmHouse.HasOwner" />.</remarks>
	public bool isCabin => buildingType.Value == "Cabin";

	public bool isMoving
	{
		get
		{
			return _isMoving;
		}
		set
		{
			if (_isMoving != value)
			{
				_isMoving = value;
				if (_isMoving)
				{
					OnStartMove();
				}
				if (!_isMoving)
				{
					OnEndMove();
				}
			}
		}
	}

	public NetFields NetFields { get; } = new NetFields("Building");


	/// <summary>Construct an instance.</summary>
	public Building()
	{
		id.Value = Guid.NewGuid();
		resetTexture();
		initNetFields();
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="type">The building type ID in <see cref="F:StardewValley.Game1.buildingData" />.</param>
	/// <param name="tile">The top-left tile position of the building.</param>
	public Building(string type, Vector2 tile)
		: this()
	{
		tileX.Value = (int)tile.X;
		tileY.Value = (int)tile.Y;
		buildingType.Value = type;
		BuildingData data = ReloadBuildingData();
		daysOfConstructionLeft.Value = data?.BuildDays ?? 0;
	}

	/// <summary>Get whether the building has any skins that can be applied to it currently.</summary>
	/// <param name="ignoreSeparateConstructionEntries">Whether to ignore skins with <see cref="F:StardewValley.GameData.Buildings.BuildingSkin.ShowAsSeparateConstructionEntry" /> set to true.</param>
	public virtual bool CanBeReskinned(bool ignoreSeparateConstructionEntries = false)
	{
		BuildingData data = GetData();
		if (skinId.Value != null)
		{
			return true;
		}
		if (data?.Skins != null)
		{
			foreach (BuildingSkin skin in data.Skins)
			{
				if (!(skin.Id == skinId.Value) && (!ignoreSeparateConstructionEntries || !skin.ShowAsSeparateConstructionEntry) && GameStateQuery.CheckConditions(skin.Condition, GetParentLocation()))
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>Get whether animals within this building can get pregnant and produce offspring.</summary>
	public bool AllowsAnimalPregnancy()
	{
		return GetData()?.AllowAnimalPregnancy ?? false;
	}

	/// <summary>Get whether players can repaint this building.</summary>
	public virtual bool CanBePainted()
	{
		if (this is GreenhouseBuilding && !Game1.getFarm().greenhouseUnlocked.Value)
		{
			return false;
		}
		if ((isCabin || HasIndoorsName("Farmhouse")) && GetIndoors() is FarmHouse { upgradeLevel: <2 })
		{
			return false;
		}
		return GetPaintDataKey() != null;
	}

	/// <summary>Get the building's current skin, if applicable.</summary>
	public BuildingSkin GetSkin()
	{
		return GetSkin(skinId.Value, GetData());
	}

	/// <summary>Get a building skin from data, if it exists.</summary>
	/// <param name="skinId">The building skin ID to find.</param>
	/// <param name="data">The building data to search.</param>
	/// <returns>Returns the matching building skin if found, else <c>null</c>.</returns>
	public static BuildingSkin GetSkin(string skinId, BuildingData data)
	{
		if (skinId != null && data?.Skins != null)
		{
			foreach (BuildingSkin skin in data.Skins)
			{
				if (skin.Id == skinId)
				{
					return skin;
				}
			}
		}
		return null;
	}

	/// <summary>Get the key in <c>Data/PaintData</c> for the building, if it has any.</summary>
	public virtual string GetPaintDataKey()
	{
		Dictionary<string, string> asset = DataLoader.PaintData(Game1.content);
		return GetPaintDataKey(asset);
	}

	/// <summary>Get the key in <c>Data/PaintData</c> for the building, if it has any.</summary>
	/// <param name="paintData">The loaded <c>Data/PaintData</c> asset.</param>
	public virtual string GetPaintDataKey(Dictionary<string, string> paintData)
	{
		if (skinId.Value != null && paintData.ContainsKey(skinId.Value))
		{
			return skinId.Value;
		}
		string text = buildingType;
		string lookupName = ((text == "Farmhouse") ? "House" : ((!(text == "Cabin")) ? ((string)buildingType) : "Stone Cabin"));
		if (!paintData.ContainsKey(lookupName))
		{
			return null;
		}
		return lookupName;
	}

	public string GetMetadata(string key)
	{
		if (buildingMetadata == null)
		{
			buildingMetadata = new Dictionary<string, string>();
			BuildingData data = GetData();
			if (data != null)
			{
				foreach (KeyValuePair<string, string> kvp in data.Metadata)
				{
					buildingMetadata[kvp.Key] = kvp.Value;
				}
				BuildingSkin skin = GetSkin(skinId.Value, data);
				if (skin != null)
				{
					foreach (KeyValuePair<string, string> kvp in skin.Metadata)
					{
						buildingMetadata[kvp.Key] = kvp.Value;
					}
				}
			}
		}
		if (!buildingMetadata.TryGetValue(key, out key))
		{
			return null;
		}
		return key;
	}

	/// <summary>Get the location which contains this building.</summary>
	public GameLocation GetParentLocation()
	{
		return Game1.getLocationFromName(parentLocationName.Value);
	}

	/// <summary>Get whether the building is in <see cref="P:StardewValley.Game1.currentLocation" />.</summary>
	public bool IsInCurrentLocation()
	{
		if (Game1.currentLocation != null)
		{
			return Game1.currentLocation.NameOrUniqueName == parentLocationName.Value;
		}
		return false;
	}

	public virtual bool hasCarpenterPermissions()
	{
		if (Game1.IsMasterGame)
		{
			return true;
		}
		if (owner.Value == Game1.player.UniqueMultiplayerID)
		{
			return true;
		}
		if (GetIndoors() is FarmHouse { IsOwnedByCurrentPlayer: not false })
		{
			return true;
		}
		return false;
	}

	protected virtual void initNetFields()
	{
		NetFields.SetOwner(this).AddField(id, "id").AddField(indoors, "indoors")
			.AddField(nonInstancedIndoorsName, "nonInstancedIndoorsName")
			.AddField(tileX, "tileX")
			.AddField(tileY, "tileY")
			.AddField(tilesWide, "tilesWide")
			.AddField(tilesHigh, "tilesHigh")
			.AddField(maxOccupants, "maxOccupants")
			.AddField(currentOccupants, "currentOccupants")
			.AddField(daysOfConstructionLeft, "daysOfConstructionLeft")
			.AddField(daysUntilUpgrade, "daysUntilUpgrade")
			.AddField(buildingType, "buildingType")
			.AddField(humanDoor, "humanDoor")
			.AddField(animalDoor, "animalDoor")
			.AddField(magical, "magical")
			.AddField(fadeWhenPlayerIsBehind, "fadeWhenPlayerIsBehind")
			.AddField(animalDoorOpen, "animalDoorOpen")
			.AddField(owner, "owner")
			.AddField(newConstructionTimer, "newConstructionTimer")
			.AddField(netBuildingPaintColor, "netBuildingPaintColor")
			.AddField(buildingChests, "buildingChests")
			.AddField(animalDoorOpenAmount, "animalDoorOpenAmount")
			.AddField(hayCapacity, "hayCapacity")
			.AddField(parentLocationName, "parentLocationName")
			.AddField(upgradeName, "upgradeName")
			.AddField(skinId, "skinId")
			.AddField(modData, "modData");
		buildingType.fieldChangeVisibleEvent += delegate(NetString a, string b, string c)
		{
			hasChimney = null;
			bool forUpgrade = b != null && b != c;
			ReloadBuildingData(forUpgrade);
		};
		skinId.fieldChangeVisibleEvent += delegate
		{
			hasChimney = null;
			buildingMetadata = null;
			resetTexture();
		};
		buildingType.fieldChangeVisibleEvent += delegate
		{
			hasChimney = null;
			buildingMetadata = null;
			resetTexture();
		};
		indoors.fieldChangeVisibleEvent += delegate
		{
			UpdateIndoorParent();
		};
		parentLocationName.fieldChangeVisibleEvent += delegate
		{
			UpdateIndoorParent();
		};
		if (netBuildingPaintColor.Value == null)
		{
			netBuildingPaintColor.Value = new BuildingPaintColor();
		}
	}

	public virtual void UpdateIndoorParent()
	{
		GameLocation interior = GetIndoors();
		if (interior != null)
		{
			interior.parentLocationName.Value = parentLocationName.Value;
		}
	}

	/// <summary>Get the building's data from <see cref="F:StardewValley.Game1.buildingData" />, if found.</summary>
	public virtual BuildingData GetData()
	{
		if (!TryGetData(buildingType.Value, out var data))
		{
			return null;
		}
		return data;
	}

	/// <summary>Try to get a building's data from <see cref="F:StardewValley.Game1.buildingData" />.</summary>
	/// <param name="buildingType">The building type (i.e. the key in <see cref="F:StardewValley.Game1.buildingData" />).</param>
	/// <param name="data">The building data, if found.</param>
	/// <returns>Returns whether the building data was found.</returns>
	public static bool TryGetData(string buildingType, out BuildingData data)
	{
		if (buildingType == null)
		{
			data = null;
			return false;
		}
		return Game1.buildingData.TryGetValue(buildingType, out data);
	}

	/// <summary>Reload the building's data from <see cref="F:StardewValley.Game1.buildingData" /> and reapply it to the building's fields.</summary>
	/// <param name="forUpgrade">Whether the building is being upgraded.</param>
	/// <param name="forConstruction">Whether the building is being constructed.</param>
	/// <returns>Returns the loaded building data, if any.</returns>
	/// <remarks>See also <see cref="M:StardewValley.Buildings.Building.LoadFromBuildingData(StardewValley.GameData.Buildings.BuildingData,System.Boolean,System.Boolean)" />.</remarks>
	public virtual BuildingData ReloadBuildingData(bool forUpgrade = false, bool forConstruction = false)
	{
		BuildingData data = GetData();
		if (data != null)
		{
			LoadFromBuildingData(data, forUpgrade, forConstruction);
		}
		return data;
	}

	/// <summary>Reapply the loaded data to the building's fields.</summary>
	/// <param name="data">The building data to load.</param>
	/// <param name="forUpgrade">Whether the building is being upgraded.</param>
	/// <param name="forConstruction">Whether the building is being constructed.</param>
	/// <remarks>This doesn't reload the underlying data; see <see cref="M:StardewValley.Buildings.Building.ReloadBuildingData(System.Boolean,System.Boolean)" /> if you need to do that.</remarks>
	public virtual void LoadFromBuildingData(BuildingData data, bool forUpgrade = false, bool forConstruction = false)
	{
		if (data == null)
		{
			return;
		}
		tilesWide.Value = data.Size.X;
		tilesHigh.Value = data.Size.Y;
		humanDoor.X = data.HumanDoor.X;
		humanDoor.Y = data.HumanDoor.Y;
		animalDoor.Value = data.AnimalDoor.Location;
		if (data.MaxOccupants >= 0)
		{
			maxOccupants.Value = data.MaxOccupants;
		}
		hayCapacity.Value = data.HayCapacity;
		magical.Value = data.Builder == "Wizard";
		fadeWhenPlayerIsBehind.Value = data.FadeWhenBehind;
		foreach (KeyValuePair<string, string> pair in data.ModData)
		{
			modData[pair.Key] = pair.Value;
		}
		GetIndoors()?.InvalidateCachedMultiplayerMap(Game1.multiplayer.cachedMultiplayerMaps);
		if (!Game1.IsMasterGame)
		{
			return;
		}
		if (hasLoaded || forConstruction)
		{
			if (nonInstancedIndoorsName.Value == null)
			{
				string mapPath = data.IndoorMap;
				string mapType = typeof(GameLocation).ToString();
				if (data.IndoorMapType != null)
				{
					mapType = data.IndoorMapType;
				}
				if (mapPath != null)
				{
					mapPath = "Maps\\" + mapPath;
					if (indoors.Value == null)
					{
						indoors.Value = createIndoors(data, data.IndoorMap);
						InitializeIndoor(data, forConstruction, forUpgrade);
					}
					else if (indoors.Value.mapPath.Value == mapPath)
					{
						if (forUpgrade)
						{
							InitializeIndoor(data, forConstruction, forUpgrade: true);
						}
					}
					else
					{
						if (indoors.Value.GetType().ToString() != mapType)
						{
							load();
						}
						else
						{
							indoors.Value.mapPath.Value = mapPath;
							indoors.Value.updateMap();
						}
						updateInteriorWarps(indoors.Value);
						InitializeIndoor(data, forConstruction, forUpgrade);
					}
				}
			}
			else
			{
				updateInteriorWarps();
			}
		}
		if (!(hasLoaded || forConstruction))
		{
			return;
		}
		HashSet<string> validChests = new HashSet<string>();
		if (data.Chests != null)
		{
			foreach (BuildingChest buildingChest in data.Chests)
			{
				validChests.Add(buildingChest.Id);
			}
		}
		for (int i = buildingChests.Count - 1; i >= 0; i--)
		{
			if (!validChests.Contains(buildingChests[i].Name))
			{
				buildingChests.RemoveAt(i);
			}
		}
		if (data.Chests == null)
		{
			return;
		}
		foreach (BuildingChest buildingChest in data.Chests)
		{
			if (GetBuildingChest(buildingChest.Id) == null)
			{
				Chest newChest = new Chest(playerChest: true)
				{
					Name = buildingChest.Id
				};
				buildingChests.Add(newChest);
			}
		}
	}

	/// <summary>Create a building instance from its type ID.</summary>
	/// <param name="typeId">The building type ID in <c>Data/Buildings</c>.</param>
	/// <param name="tile">The top-left tile position of the building.</param>
	public static Building CreateInstanceFromId(string typeId, Vector2 tile)
	{
		if (typeId != null && Game1.buildingData.TryGetValue(typeId, out var data))
		{
			Type type = ((data.BuildingType != null) ? Type.GetType(data.BuildingType) : null);
			if (type != null && type != typeof(Building))
			{
				try
				{
					return (Building)Activator.CreateInstance(type, typeId, tile);
				}
				catch (MissingMethodException)
				{
					try
					{
						Building obj = (Building)Activator.CreateInstance(type, tile);
						obj.buildingType.Value = typeId;
						return obj;
					}
					catch (Exception e)
					{
						Game1.log.Error("Error trying to instantiate building for type '" + typeId + "'", e);
					}
				}
			}
		}
		return new Building(typeId, tile);
	}

	public virtual void InitializeIndoor(BuildingData data, bool forConstruction, bool forUpgrade)
	{
		if (data == null)
		{
			return;
		}
		GameLocation interior = GetIndoors();
		if (interior == null)
		{
			return;
		}
		if (interior is AnimalHouse animalHouse && data.MaxOccupants > 0)
		{
			animalHouse.animalLimit.Value = data.MaxOccupants;
		}
		if (forUpgrade && data.IndoorItemMoves != null)
		{
			foreach (IndoorItemMove move in data.IndoorItemMoves)
			{
				for (int x = 0; x < move.Size.X; x++)
				{
					for (int y = 0; y < move.Size.Y; y++)
					{
						interior.moveObject(move.Source.X + x, move.Source.Y + y, move.Destination.X + x, move.Destination.Y + y, move.UnlessItemId);
					}
				}
			}
		}
		if (!(forConstruction || forUpgrade) || data.IndoorItems == null)
		{
			return;
		}
		foreach (IndoorItemAdd item in data.IndoorItems)
		{
			Vector2 tileVector = Utility.PointToVector2(item.Tile);
			if (!interior.IsTileBlockedBy(tileVector, CollisionMask.Furniture | CollisionMask.Objects) && ItemRegistry.Create(item.ItemId) is Object newObj)
			{
				if (item.Indestructible)
				{
					newObj.fragility.Value = 2;
				}
				newObj.TileLocation = tileVector;
				if (newObj is Furniture newFurniture)
				{
					interior.furniture.Add(newFurniture);
				}
				else
				{
					interior.objects.Add(tileVector, newObj);
				}
			}
		}
	}

	public BuildingItemConversion GetItemConversionForItem(Item item, Chest chest)
	{
		if (item == null || chest == null)
		{
			return null;
		}
		BuildingData data = GetData();
		if (data?.ItemConversions != null)
		{
			foreach (BuildingItemConversion conversion in data.ItemConversions)
			{
				if (!(conversion.SourceChest == chest.Name))
				{
					continue;
				}
				bool fail = false;
				foreach (string requiredTag in conversion.RequiredTags)
				{
					if (!item.HasContextTag(requiredTag))
					{
						fail = true;
						break;
					}
				}
				if (!fail)
				{
					return conversion;
				}
			}
		}
		return null;
	}

	public bool IsValidObjectForChest(Item item, Chest chest)
	{
		return GetItemConversionForItem(item, chest) != null;
	}

	public bool PerformBuildingChestAction(string name, Farmer who)
	{
		Chest chest = GetBuildingChest(name);
		if (chest == null)
		{
			return false;
		}
		BuildingChest chestData = GetBuildingChestData(name);
		if (chestData == null)
		{
			return false;
		}
		switch (chestData.Type)
		{
		case BuildingChestType.Chest:
			((MenuWithInventory)(Game1.activeClickableMenu = new ItemGrabMenu(chest.Items, reverseGrab: false, showReceivingMenu: true, (Item item) => IsValidObjectForChest(item, chest), chest.grabItemFromInventory, null, chest.grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, null, -1, this))).inventory.moveItemSound = chestData.Sound;
			return true;
		case BuildingChestType.Load:
			if (who?.ActiveObject != null)
			{
				if (!IsValidObjectForChest(who.ActiveObject, chest))
				{
					if (chestData.InvalidItemMessage != null && (chestData.InvalidItemMessageCondition == null || GameStateQuery.CheckConditions(chestData.InvalidItemMessageCondition, GetParentLocation(), who, who.ActiveObject, who.ActiveObject)))
					{
						Game1.showRedMessage(TokenParser.ParseText(chestData.InvalidItemMessage));
					}
					return false;
				}
				BuildingItemConversion conversion = GetItemConversionForItem(who.ActiveObject, chest);
				Utility.consolidateStacks(chest.Items);
				chest.clearNulls();
				int roomForItem = Utility.GetNumberOfItemThatCanBeAddedToThisInventoryList(who.ActiveObject, chest.Items, 36);
				if (who.ActiveObject.Stack > conversion.RequiredCount && roomForItem < conversion.RequiredCount)
				{
					Game1.showRedMessage(TokenParser.ParseText(chestData.ChestFullMessage));
					return false;
				}
				int acceptAmount = Math.Min(roomForItem, who.ActiveObject.Stack) / conversion.RequiredCount * conversion.RequiredCount;
				if (acceptAmount == 0)
				{
					if (chestData.InvalidCountMessage != null)
					{
						Game1.showRedMessage(TokenParser.ParseText(chestData.InvalidCountMessage));
					}
					return false;
				}
				Item one = who.ActiveObject.getOne();
				Object heldStack = (Object)who.ActiveObject.ConsumeStack(acceptAmount);
				who.ActiveObject = null;
				if (heldStack != null)
				{
					who.ActiveObject = heldStack;
				}
				one.Stack = acceptAmount;
				Utility.addItemToThisInventoryList(one, chest.Items, 36);
				if (chestData.Sound != null)
				{
					Game1.playSound(chestData.Sound);
				}
			}
			return true;
		case BuildingChestType.Collect:
			Utility.CollectSingleItemOrShowChestMenu(chest);
			return true;
		default:
			return false;
		}
	}

	public BuildingChest GetBuildingChestData(string name)
	{
		return GetBuildingChestData(GetData(), name);
	}

	public static BuildingChest GetBuildingChestData(BuildingData data, string name)
	{
		if (data == null)
		{
			return null;
		}
		foreach (BuildingChest buildingChestData in data.Chests)
		{
			if (buildingChestData.Id == name)
			{
				return buildingChestData;
			}
		}
		return null;
	}

	public Chest GetBuildingChest(string name)
	{
		foreach (Chest buildingChest in buildingChests)
		{
			if (buildingChest.Name == name)
			{
				return buildingChest;
			}
		}
		return null;
	}

	public virtual string textureName()
	{
		BuildingData data = GetData();
		return GetSkin(skinId.Value, data)?.Texture ?? data?.Texture ?? ("Buildings\\" + buildingType);
	}

	public virtual void resetTexture()
	{
		texture = new Lazy<Texture2D>(delegate
		{
			if (paintedTexture != null)
			{
				paintedTexture.Dispose();
				paintedTexture = null;
			}
			string text = textureName();
			Texture2D texture2D;
			try
			{
				texture2D = Game1.content.Load<Texture2D>(text);
			}
			catch
			{
				return Game1.content.Load<Texture2D>("Buildings\\Error");
			}
			paintedTexture = BuildingPainter.Apply(texture2D, text + "_PaintMask", netBuildingPaintColor.Value);
			if (paintedTexture != null)
			{
				texture2D = paintedTexture;
			}
			return texture2D;
		});
	}

	public int getTileSheetIndexForStructurePlacementTile(int x, int y)
	{
		if (x == humanDoor.X && y == humanDoor.Y)
		{
			return 2;
		}
		if (x == animalDoor.X && y == animalDoor.Y)
		{
			return 4;
		}
		return 0;
	}

	public virtual void performTenMinuteAction(int timeElapsed)
	{
	}

	public virtual void resetLocalState()
	{
		alpha = 1f;
		color = Color.White;
		isMoving = false;
	}

	public virtual bool CanLeftClick(int x, int y)
	{
		Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(x, y, 1, 1);
		return intersects(r);
	}

	public virtual bool leftClicked()
	{
		return false;
	}

	public virtual void ToggleAnimalDoor(Farmer who)
	{
		BuildingData data = GetData();
		string sound = ((!animalDoorOpen.Value) ? data?.AnimalDoorCloseSound : data?.AnimalDoorOpenSound);
		if (sound != null)
		{
			who.currentLocation.playSound(sound);
		}
		animalDoorOpen.Value = !animalDoorOpen;
	}

	public virtual bool OnUseHumanDoor(Farmer who)
	{
		return true;
	}

	public virtual bool doAction(Vector2 tileLocation, Farmer who)
	{
		if (who.isRidingHorse())
		{
			return false;
		}
		if (who.IsLocalPlayer && occupiesTile(tileLocation) && (int)daysOfConstructionLeft > 0)
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:UnderConstruction"));
		}
		else
		{
			if (who.ActiveObject != null && who.ActiveObject.IsFloorPathItem() && who.currentLocation != null && !who.currentLocation.terrainFeatures.ContainsKey(tileLocation))
			{
				return false;
			}
			GameLocation interior = GetIndoors();
			if (who.IsLocalPlayer && tileLocation.X == (float)(humanDoor.X + (int)tileX) && tileLocation.Y == (float)(humanDoor.Y + (int)tileY) && interior != null)
			{
				if (who.mount != null)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:DismountBeforeEntering"));
					return false;
				}
				if (who.team.demolishLock.IsLocked())
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:CantEnter"));
					return false;
				}
				if (OnUseHumanDoor(who))
				{
					who.currentLocation.playSound("doorClose", tileLocation);
					bool isStructure = indoors.Value != null;
					Game1.warpFarmer(interior.NameOrUniqueName, interior.warps[0].X, interior.warps[0].Y - 1, Game1.player.FacingDirection, isStructure);
				}
				return true;
			}
			BuildingData data = GetData();
			if (data != null)
			{
				Microsoft.Xna.Framework.Rectangle door = getRectForAnimalDoor(data);
				door.Width /= 64;
				door.Height /= 64;
				door.X /= 64;
				door.Y /= 64;
				if ((int)daysOfConstructionLeft <= 0 && door != Microsoft.Xna.Framework.Rectangle.Empty && door.Contains(Utility.Vector2ToPoint(tileLocation)) && Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
				{
					ToggleAnimalDoor(who);
					return true;
				}
				if (who.IsLocalPlayer && occupiesTile(tileLocation, applyTilePropertyRadius: true) && !isTilePassable(tileLocation))
				{
					string tileAction = data.GetActionAtTile((int)tileLocation.X - tileX.Value, (int)tileLocation.Y - tileY.Value);
					if (tileAction != null)
					{
						tileAction = TokenParser.ParseText(tileAction);
						if (who.currentLocation.performAction(tileAction, who, new Location((int)tileLocation.X, (int)tileLocation.Y)))
						{
							return true;
						}
					}
				}
			}
			else
			{
				if (who.IsLocalPlayer && !isTilePassable(tileLocation) && TryPerformObeliskWarp(buildingType.Value, who))
				{
					return true;
				}
				if (who.IsLocalPlayer && who.ActiveObject != null && !isTilePassable(tileLocation))
				{
					return performActiveObjectDropInAction(who, probe: false);
				}
			}
		}
		return false;
	}

	public static bool TryPerformObeliskWarp(string buildingType, Farmer who)
	{
		switch (buildingType)
		{
		case "Desert Obelisk":
			PerformObeliskWarp("Desert", 35, 43, force_dismount: true, who);
			return true;
		case "Water Obelisk":
			PerformObeliskWarp("Beach", 20, 4, force_dismount: false, who);
			return true;
		case "Earth Obelisk":
			PerformObeliskWarp("Mountain", 31, 20, force_dismount: false, who);
			return true;
		case "Island Obelisk":
			PerformObeliskWarp("IslandSouth", 11, 11, force_dismount: false, who);
			return true;
		default:
			return false;
		}
	}

	public static void PerformObeliskWarp(string destination, int warp_x, int warp_y, bool force_dismount, Farmer who)
	{
		if (force_dismount && who.isRidingHorse() && who.mount != null)
		{
			who.mount.checkAction(who, who.currentLocation);
			return;
		}
		for (int i = 0; i < 12; i++)
		{
			who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), flicker: false, Game1.random.NextBool()));
		}
		who.currentLocation.playSound("wand");
		Game1.displayFarmer = false;
		Game1.player.temporarilyInvincible = true;
		Game1.player.temporaryInvincibilityTimer = -2000;
		Game1.player.freezePause = 1000;
		Game1.flashAlpha = 1f;
		Microsoft.Xna.Framework.Rectangle playerBounds = who.GetBoundingBox();
		DelayedAction.fadeAfterDelay(delegate
		{
			obeliskWarpForReal(destination, warp_x, warp_y, who);
		}, 1000);
		new Microsoft.Xna.Framework.Rectangle(playerBounds.X, playerBounds.Y, 64, 64).Inflate(192, 192);
		int j = 0;
		Point playerTile = who.TilePoint;
		for (int x = playerTile.X + 8; x >= playerTile.X - 8; x--)
		{
			who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(x, playerTile.Y) * 64f, Color.White, 8, flipped: false, 50f)
			{
				layerDepth = 1f,
				delayBeforeAnimationStart = j * 25,
				motion = new Vector2(-0.25f, 0f)
			});
			j++;
		}
	}

	private static void obeliskWarpForReal(string destination, int warp_x, int warp_y, Farmer who)
	{
		Game1.warpFarmer(destination, warp_x, warp_y, flip: false);
		Game1.fadeToBlackAlpha = 0.99f;
		Game1.screenGlow = false;
		Game1.player.temporarilyInvincible = false;
		Game1.player.temporaryInvincibilityTimer = 0;
		Game1.displayFarmer = true;
	}

	public virtual bool isActionableTile(int xTile, int yTile, Farmer who)
	{
		BuildingData data = GetData();
		if (data != null)
		{
			Vector2 tileLocation = new Vector2(xTile, yTile);
			if (occupiesTile(tileLocation, applyTilePropertyRadius: true) && !isTilePassable(tileLocation) && data.GetActionAtTile(xTile - tileX.Value, yTile - tileY.Value) != null)
			{
				return true;
			}
		}
		if (humanDoor.X >= 0 && xTile == (int)tileX + humanDoor.X && yTile == (int)tileY + humanDoor.Y)
		{
			return true;
		}
		Microsoft.Xna.Framework.Rectangle door = getRectForAnimalDoor(data);
		door.Width /= 64;
		door.Height /= 64;
		door.X /= 64;
		door.Y /= 64;
		if (door != Microsoft.Xna.Framework.Rectangle.Empty)
		{
			return door.Contains(new Point(xTile, yTile));
		}
		return false;
	}

	/// <summary>Handle the building being moved within its location by any player.</summary>
	public virtual void performActionOnBuildingPlacement()
	{
		GameLocation location = GetParentLocation();
		if (location == null)
		{
			return;
		}
		for (int y = 0; y < (int)tilesHigh; y++)
		{
			for (int x = 0; x < (int)tilesWide; x++)
			{
				Vector2 currentGlobalTilePosition = new Vector2((int)tileX + x, (int)tileY + y);
				if (!location.terrainFeatures.ContainsKey(currentGlobalTilePosition) || !(location.terrainFeatures[currentGlobalTilePosition] is Flooring) || GetData() == null || !GetData().AllowsFlooringUnderneath)
				{
					location.terrainFeatures.Remove(currentGlobalTilePosition);
				}
			}
		}
		foreach (BuildingPlacementTile additionalPlacementTile in GetAdditionalPlacementTiles())
		{
			bool onlyNeedsToBePassable = additionalPlacementTile.OnlyNeedsToBePassable;
			foreach (Point areaTile in additionalPlacementTile.TileArea.GetPoints())
			{
				Vector2 currentGlobalTilePosition = new Vector2((int)tileX + areaTile.X, (int)tileY + areaTile.Y);
				if ((!onlyNeedsToBePassable || (location.terrainFeatures.TryGetValue(currentGlobalTilePosition, out var feature) && !feature.isPassable())) && (!location.terrainFeatures.ContainsKey(currentGlobalTilePosition) || !(location.terrainFeatures[currentGlobalTilePosition] is Flooring) || GetData() == null || !GetData().AllowsFlooringUnderneath))
				{
					location.terrainFeatures.Remove(currentGlobalTilePosition);
				}
			}
		}
	}

	/// <summary>Handle the building being constructed.</summary>
	/// <param name="location">The location containing the building.</param>
	/// <param name="who">The player that constructed the building.</param>
	public virtual void performActionOnConstruction(GameLocation location, Farmer who)
	{
		BuildingData data = GetData();
		LoadFromBuildingData(data, forUpgrade: false, forConstruction: true);
		Vector2 buildingCenter = new Vector2((float)(int)tileX + (float)(int)tilesWide * 0.5f, (float)(int)tileY + (float)(int)tilesHigh * 0.5f);
		location.localSound("axchop", buildingCenter);
		newConstructionTimer.Value = (((bool)magical || (int)daysOfConstructionLeft <= 0) ? 2000 : 1000);
		if (data?.AddMailOnBuild != null)
		{
			foreach (string item in data.AddMailOnBuild)
			{
				Game1.addMail(item, noLetter: false, sendToEveryone: true);
			}
		}
		if (!magical)
		{
			location.localSound("axchop", buildingCenter);
			for (int x = tileX; x < (int)tileX + (int)tilesWide; x++)
			{
				for (int y = tileY; y < (int)tileY + (int)tilesHigh; y++)
				{
					for (int i = 0; i < 5; i++)
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
			for (int i = 0; i < 8; i++)
			{
				DelayedAction.playSoundAfterDelay("dirtyHit", 250 + i * 150, location, buildingCenter, -1, local: true);
			}
		}
		else
		{
			for (int i = 0; i < 8; i++)
			{
				DelayedAction.playSoundAfterDelay("dirtyHit", 100 + i * 210, location, buildingCenter, -1, local: true);
			}
			if (Game1.player == who)
			{
				Game1.flashAlpha = 2f;
			}
			location.localSound("wand", buildingCenter);
			Microsoft.Xna.Framework.Rectangle mainSourceRect = getSourceRect();
			Microsoft.Xna.Framework.Rectangle sourceRectForMenu = getSourceRectForMenu() ?? mainSourceRect;
			int y = 0;
			for (int bottomEdge = mainSourceRect.Height / 16 * 2; y <= bottomEdge; y++)
			{
				int x = 0;
				for (int rightEdge = sourceRectForMenu.Width / 16 * 2; x < rightEdge; x++)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 40f, 4, 2, new Vector2((int)tileX, (int)tileY) * 64f + new Vector2(x * 64 / 2, y * 64 / 2 - mainSourceRect.Height * 4 + (int)tilesHigh * 64) + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), flicker: false, flipped: false)
					{
						layerDepth = (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + (float)x / 10000f,
						pingPong = true,
						delayBeforeAnimationStart = (mainSourceRect.Height / 16 * 2 - y) * 100,
						scale = 4f,
						alphaFade = 0.01f,
						color = Color.AliceBlue
					});
					location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 40f, 4, 2, new Vector2((int)tileX, (int)tileY) * 64f + new Vector2(x * 64 / 2, y * 64 / 2 - mainSourceRect.Height * 4 + (int)tilesHigh * 64) + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), flicker: false, flipped: false)
					{
						layerDepth = (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + (float)x / 10000f + 0.0001f,
						pingPong = true,
						delayBeforeAnimationStart = (mainSourceRect.Height / 16 * 2 - y) * 100,
						scale = 4f,
						alphaFade = 0.01f,
						color = Color.AliceBlue
					});
				}
			}
		}
		if (GetIndoors() is Cabin { HasOwner: false } cabin)
		{
			cabin.CreateFarmhand();
			if (Game1.IsMasterGame)
			{
				hasLoaded = true;
			}
		}
	}

	/// <summary>Handle the building being demolished.</summary>
	/// <param name="location">The location which previously contained the building.</param>
	public virtual void performActionOnDemolition(GameLocation location)
	{
		if (GetIndoors() is Cabin cabin)
		{
			cabin.DeleteFarmhand();
		}
		if (indoors.Value != null)
		{
			Game1.multiplayer.broadcastRemoveLocationFromLookup(indoors.Value);
			indoors.Value = null;
		}
	}

	/// <summary>Perform an action for each item within the building instance, excluding those in the interior location.</summary>
	/// <param name="action">The action to perform for each item.  This should return true (continue iterating) or false (stop).</param>
	/// <returns>Returns whether to continue iterating.</returns>
	/// <remarks>For items in the interior location, use <see cref="M:StardewValley.Utility.ForEachItemIn(StardewValley.GameLocation,System.Func{StardewValley.Item,System.Boolean})" /> instead.</remarks>
	public virtual bool ForEachItemExcludingInterior(Func<Item, bool> action)
	{
		return ForEachItemExcludingInterior((Item item, Action remove, Action<Item> replaceWith) => action(item));
	}

	/// <summary>Perform an action for each item within the building instance, excluding those in the interior location.</summary>
	/// <param name="handler">The action to perform for each item.</param>
	/// <returns>Returns whether to continue iterating.</returns>
	/// <remarks>For items in the interior location, use <see cref="M:StardewValley.Utility.ForEachItemIn(StardewValley.GameLocation,System.Func{StardewValley.Item,System.Boolean})" /> instead.</remarks>
	public virtual bool ForEachItemExcludingInterior(ForEachItemDelegate handler)
	{
		foreach (Chest buildingChest in buildingChests)
		{
			if (!buildingChest.ForEachItem(handler))
			{
				return false;
			}
		}
		return true;
	}

	public virtual void BeforeDemolish()
	{
		List<Item> quest_items = new List<Item>();
		ForEachItemExcludingInterior(delegate(Item item)
		{
			CollectQuestItem(item);
			return true;
		});
		if (indoors.Value != null)
		{
			Utility.ForEachItemIn(indoors.Value, delegate(Item item)
			{
				CollectQuestItem(item);
				return true;
			});
			if (indoors.Value is Cabin cabin)
			{
				Cellar cellar = cabin.GetCellar();
				if (cellar != null)
				{
					Utility.ForEachItemIn(cellar, delegate(Item item)
					{
						CollectQuestItem(item);
						return true;
					});
				}
			}
		}
		if (quest_items.Count > 0)
		{
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:NewLostAndFoundItems"));
			for (int i = 0; i < quest_items.Count; i++)
			{
				Game1.player.team.returnedDonations.Add(quest_items[i]);
			}
		}
		void CollectQuestItem(Item item)
		{
			if (item is Object obj && obj.questItem.Value)
			{
				Item clone = obj.getOne();
				clone.Stack = obj.Stack;
				quest_items.Add(clone);
			}
		}
	}

	public virtual void performActionOnUpgrade(GameLocation location)
	{
		if (location is Farm farm)
		{
			farm.UnsetFarmhouseValues();
		}
	}

	public virtual string isThereAnythingtoPreventConstruction(GameLocation location, Vector2 tile_location)
	{
		return null;
	}

	public virtual bool performActiveObjectDropInAction(Farmer who, bool probe)
	{
		return false;
	}

	public virtual void performToolAction(Tool t, int tileX, int tileY)
	{
	}

	public virtual void updateWhenFarmNotCurrentLocation(GameTime time)
	{
		if (indoors.Value != null && Game1.currentLocation != indoors.Value)
		{
			indoors.Value.netAudio.Update();
		}
		netBuildingPaintColor.Value?.Poll(resetTexture);
		if ((int)newConstructionTimer > 0)
		{
			newConstructionTimer.Value -= time.ElapsedGameTime.Milliseconds;
			if ((int)newConstructionTimer <= 0 && (bool)magical)
			{
				daysOfConstructionLeft.Value = 0;
			}
		}
		if (!Game1.IsMasterGame)
		{
			return;
		}
		BuildingData data = GetData();
		if (data == null)
		{
			return;
		}
		if (animalDoorOpen.Value)
		{
			if (animalDoorOpenAmount.Value < 1f)
			{
				animalDoorOpenAmount.Value = ((data.AnimalDoorOpenDuration > 0f) ? Utility.MoveTowards(animalDoorOpenAmount.Value, 1f, (float)time.ElapsedGameTime.TotalSeconds / data.AnimalDoorOpenDuration) : 1f);
			}
		}
		else if (animalDoorOpenAmount.Value > 0f)
		{
			animalDoorOpenAmount.Value = ((data.AnimalDoorCloseDuration > 0f) ? Utility.MoveTowards(animalDoorOpenAmount.Value, 0f, (float)time.ElapsedGameTime.TotalSeconds / data.AnimalDoorCloseDuration) : 0f);
		}
	}

	public virtual void Update(GameTime time)
	{
		if (!hasLoaded && Game1.IsMasterGame && Game1.hasLoadedGame)
		{
			ReloadBuildingData(forUpgrade: false, forConstruction: true);
			load();
		}
		UpdateTransparency();
		if (isUnderConstruction())
		{
			return;
		}
		if (!hasChimney.HasValue)
		{
			string chimneyString = GetMetadata("ChimneyPosition");
			if (chimneyString != null)
			{
				hasChimney = true;
				string[] split = ArgUtility.SplitBySpace(chimneyString);
				chimneyPosition.X = int.Parse(split[0]);
				chimneyPosition.Y = int.Parse(split[1]);
			}
			else
			{
				hasChimney = false;
			}
		}
		GameLocation interior = GetIndoors();
		if (interior is FarmHouse { upgradeLevel: var upgradeLevel } && lastHouseUpgradeLevel != upgradeLevel)
		{
			lastHouseUpgradeLevel = upgradeLevel;
			string chimneyString = null;
			for (int i = 1; i <= lastHouseUpgradeLevel; i++)
			{
				string currentChimneyString = GetMetadata("ChimneyPosition" + (i + 1));
				if (currentChimneyString != null)
				{
					chimneyString = currentChimneyString;
				}
			}
			if (chimneyString != null)
			{
				hasChimney = true;
				string[] split = ArgUtility.SplitBySpace(chimneyString);
				chimneyPosition.X = int.Parse(split[0]);
				chimneyPosition.Y = int.Parse(split[1]);
			}
		}
		if (hasChimney != true || interior == null)
		{
			return;
		}
		chimneyTimer -= time.ElapsedGameTime.Milliseconds;
		if (chimneyTimer <= 0)
		{
			if (interior.hasActiveFireplace())
			{
				GameLocation parentLocation = GetParentLocation();
				Microsoft.Xna.Framework.Rectangle mainSourceRect = getSourceRect();
				Vector2 cornerPosition = new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64 - mainSourceRect.Height * 4);
				BuildingData data = GetData();
				Vector2 cornerOffset = ((data != null) ? (data.DrawOffset * 4f) : Vector2.Zero);
				TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(cornerPosition.X + cornerOffset.X, cornerPosition.Y + cornerOffset.Y) + chimneyPosition * 4f + new Vector2(-8f, -12f), flipped: false, 0.002f, Color.Gray);
				sprite.alpha = 0.75f;
				sprite.motion = new Vector2(0f, -0.5f);
				sprite.acceleration = new Vector2(0.002f, 0f);
				sprite.interval = 99999f;
				sprite.layerDepth = 1f;
				sprite.scale = 2f;
				sprite.scaleChange = 0.02f;
				sprite.rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f;
				parentLocation.temporarySprites.Add(sprite);
			}
			chimneyTimer = 500;
		}
	}

	/// <summary>Update the building transparency on tick for the local player's position.</summary>
	public virtual void UpdateTransparency()
	{
		if (fadeWhenPlayerIsBehind.Value)
		{
			Microsoft.Xna.Framework.Rectangle sourceRect = getSourceRectForMenu() ?? getSourceRect();
			Microsoft.Xna.Framework.Rectangle boundingBox = new Microsoft.Xna.Framework.Rectangle((int)tileX * 64, ((int)tileY + (-(sourceRect.Height / 16) + (int)tilesHigh)) * 64, (int)tilesWide * 64, (sourceRect.Height / 16 - (int)tilesHigh) * 64 + 32);
			if (Game1.player.GetBoundingBox().Intersects(boundingBox))
			{
				if (alpha > 0.4f)
				{
					alpha = Math.Max(0.4f, alpha - 0.04f);
				}
				return;
			}
		}
		if (alpha < 1f)
		{
			alpha = Math.Min(1f, alpha + 0.05f);
		}
	}

	public virtual void showUpgradeAnimation(GameLocation location)
	{
		color = Color.White;
		location.temporarySprites.Add(new TemporaryAnimatedSprite(46, getUpgradeSignLocation() + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), Color.Beige, 10, Game1.random.NextBool(), 75f)
		{
			motion = new Vector2(0f, -0.5f),
			acceleration = new Vector2(-0.02f, 0.01f),
			delayBeforeAnimationStart = Game1.random.Next(100),
			layerDepth = 0.89f
		});
		location.temporarySprites.Add(new TemporaryAnimatedSprite(46, getUpgradeSignLocation() + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), Color.Beige, 10, Game1.random.NextBool(), 75f)
		{
			motion = new Vector2(0f, -0.5f),
			acceleration = new Vector2(-0.02f, 0.01f),
			delayBeforeAnimationStart = Game1.random.Next(40),
			layerDepth = 0.89f
		});
	}

	public virtual Vector2 getUpgradeSignLocation()
	{
		BuildingData data = GetData();
		Vector2 signOffset = data?.UpgradeSignTile ?? new Vector2(0.5f, 0f);
		float signHeight = data?.UpgradeSignHeight ?? 8f;
		return new Vector2(((float)(int)tileX + signOffset.X) * 64f, ((float)(int)tileY + signOffset.Y) * 64f - signHeight * 4f);
	}

	public virtual void showDestroyedAnimation(GameLocation location)
	{
		for (int x = tileX; x < (int)tileX + (int)tilesWide; x++)
		{
			for (int y = tileY; y < (int)tileY + (int)tilesHigh; y++)
			{
				location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(x * 64, y * 64) + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), flicker: false, Game1.random.NextBool())
				{
					delayBeforeAnimationStart = Game1.random.Next(300)
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(x * 64, y * 64) + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), flicker: false, Game1.random.NextBool())
				{
					delayBeforeAnimationStart = 250 + Game1.random.Next(300)
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(x, y) * 64f + new Vector2(32f, -32f) + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-16, 16)), flipped: false, 0f, Color.White)
				{
					interval = 30f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					scale = 4f,
					alphaFade = 0.01f
				});
			}
		}
	}

	/// <summary>Instantly finish constructing or upgrading the building, if applicable.</summary>
	public void FinishConstruction(bool onGameStart = false)
	{
		bool changed = false;
		if (daysOfConstructionLeft.Value > 0)
		{
			Game1.player.team.constructedBuildings.Add(buildingType);
			if (buildingType.Value == "Slime Hutch")
			{
				Game1.player.mailReceived.Add("slimeHutchBuilt");
			}
			daysOfConstructionLeft.Value = 0;
			changed = true;
		}
		if (daysUntilUpgrade.Value > 0)
		{
			string nextUpgrade = upgradeName.Value ?? "Well";
			Game1.player.team.constructedBuildings.Add(nextUpgrade);
			buildingType.Value = nextUpgrade;
			ReloadBuildingData(forUpgrade: true);
			daysUntilUpgrade.Value = 0;
			OnUpgraded();
			changed = true;
		}
		if (changed)
		{
			Game1.netWorldState.Value.UpdateUnderConstruction();
			resetTexture();
		}
		if (onGameStart)
		{
			return;
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			allFarmer.autoGenerateActiveDialogueEvent("structureBuilt_" + buildingType);
		}
	}

	public virtual void dayUpdate(int dayOfMonth)
	{
		if ((int)daysOfConstructionLeft > 0 && !Utility.isFestivalDay(dayOfMonth, Game1.season))
		{
			if ((int)daysOfConstructionLeft == 1)
			{
				FinishConstruction();
			}
			else
			{
				daysOfConstructionLeft.Value--;
			}
			return;
		}
		if ((int)daysUntilUpgrade > 0 && !Utility.isFestivalDay(dayOfMonth, Game1.season))
		{
			if (daysUntilUpgrade.Value == 1)
			{
				FinishConstruction();
			}
			else
			{
				daysUntilUpgrade.Value--;
			}
		}
		GameLocation interior = GetIndoors();
		if (interior is AnimalHouse animalHouse)
		{
			currentOccupants.Value = animalHouse.animals.Length;
		}
		if (GetIndoorsType() == IndoorsType.Instanced)
		{
			interior?.DayUpdate(dayOfMonth);
		}
		BuildingData data = GetData();
		if (data == null || !(data.ItemConversions?.Count > 0))
		{
			return;
		}
		ItemQueryContext itemQueryContext = new ItemQueryContext(GetParentLocation(), null, null);
		foreach (BuildingItemConversion conversion in data.ItemConversions)
		{
			CheckItemConversionRule(conversion, itemQueryContext);
		}
	}

	protected virtual void CheckItemConversionRule(BuildingItemConversion conversion, ItemQueryContext itemQueryContext)
	{
		int convertAmount = 0;
		int currentCount = 0;
		Chest sourceChest = GetBuildingChest(conversion.SourceChest);
		Chest destinationChest = GetBuildingChest(conversion.DestinationChest);
		if (sourceChest == null)
		{
			return;
		}
		foreach (Item item in sourceChest.Items)
		{
			if (item == null)
			{
				continue;
			}
			bool fail = false;
			foreach (string requiredTag in conversion.RequiredTags)
			{
				if (!item.HasContextTag(requiredTag))
				{
					fail = true;
					break;
				}
			}
			if (fail)
			{
				continue;
			}
			currentCount += item.Stack;
			if (currentCount >= conversion.RequiredCount)
			{
				int conversions = currentCount / conversion.RequiredCount;
				if (conversion.MaxDailyConversions >= 0)
				{
					conversions = Math.Min(conversions, conversion.MaxDailyConversions - convertAmount);
				}
				convertAmount += conversions;
				currentCount -= conversions * conversion.RequiredCount;
			}
			if (conversion.MaxDailyConversions >= 0 && convertAmount >= conversion.MaxDailyConversions)
			{
				break;
			}
		}
		if (convertAmount == 0)
		{
			return;
		}
		int totalConversions = 0;
		for (int j = 0; j < convertAmount; j++)
		{
			bool conversionCreatedItem = false;
			for (int i = 0; i < conversion.ProducedItems.Count; i++)
			{
				GenericSpawnItemDataWithCondition producedItem = conversion.ProducedItems[i];
				if (GameStateQuery.CheckConditions(producedItem.Condition, GetParentLocation()))
				{
					Item item = ItemQueryResolver.TryResolveRandomItem(producedItem, itemQueryContext);
					int producedCount = item.Stack;
					Item item2 = destinationChest.addItem(item);
					if (item2 == null || item2.Stack != producedCount)
					{
						conversionCreatedItem = true;
					}
				}
			}
			if (conversionCreatedItem)
			{
				totalConversions++;
			}
		}
		if (totalConversions <= 0)
		{
			return;
		}
		int requiredAmount = totalConversions * conversion.RequiredCount;
		for (int i = 0; i < sourceChest.Items.Count; i++)
		{
			Item item = sourceChest.Items[i];
			if (item == null)
			{
				continue;
			}
			bool fail = false;
			foreach (string requiredTag in conversion.RequiredTags)
			{
				if (!item.HasContextTag(requiredTag))
				{
					fail = true;
					break;
				}
			}
			if (!fail)
			{
				int consumedAmount = Math.Min(requiredAmount, item.Stack);
				sourceChest.Items[i] = item.ConsumeStack(consumedAmount);
				requiredAmount -= consumedAmount;
				if (requiredAmount <= 0)
				{
					break;
				}
			}
		}
	}

	public virtual void OnUpgraded()
	{
		GetIndoors()?.OnParentBuildingUpgraded(this);
		BuildingData data = GetData();
		if (data?.AddMailOnBuild == null)
		{
			return;
		}
		foreach (string item in data.AddMailOnBuild)
		{
			Game1.addMail(item, noLetter: false, sendToEveryone: true);
		}
	}

	public virtual Microsoft.Xna.Framework.Rectangle getSourceRect()
	{
		BuildingData data = GetData();
		if (data != null)
		{
			Microsoft.Xna.Framework.Rectangle rect = data.SourceRect;
			if (rect == Microsoft.Xna.Framework.Rectangle.Empty)
			{
				return texture.Value.Bounds;
			}
			GameLocation interior = GetIndoors();
			if (interior is FarmHouse farmhouse)
			{
				if (interior is Cabin)
				{
					rect.X += rect.Width * Math.Min(farmhouse.upgradeLevel, 2);
				}
				else
				{
					rect.Y += rect.Height * Math.Min(farmhouse.upgradeLevel, 2);
				}
			}
			rect = ApplySourceRectOffsets(rect);
			if (buildingType.Value == "Greenhouse" && GetParentLocation() is Farm farm && !farm.greenhouseUnlocked)
			{
				rect.Y -= rect.Height;
			}
			return rect;
		}
		if (isCabin)
		{
			return new Microsoft.Xna.Framework.Rectangle(((GetIndoors() is Cabin cabin) ? Math.Min(cabin.upgradeLevel, 2) : 0) * 80, 0, 80, 112);
		}
		return texture.Value.Bounds;
	}

	public virtual Microsoft.Xna.Framework.Rectangle ApplySourceRectOffsets(Microsoft.Xna.Framework.Rectangle source)
	{
		BuildingData data = GetData();
		if (data != null && data.SeasonOffset != Point.Zero)
		{
			int seasonOffset = Game1.seasonIndex;
			source.X += data.SeasonOffset.X * seasonOffset;
			source.Y += data.SeasonOffset.Y * seasonOffset;
		}
		return source;
	}

	public virtual Microsoft.Xna.Framework.Rectangle? getSourceRectForMenu()
	{
		return null;
	}

	public virtual void updateInteriorWarps(GameLocation interior = null)
	{
		interior = interior ?? GetIndoors();
		if (interior == null)
		{
			return;
		}
		GameLocation parentLocation = GetParentLocation();
		foreach (Warp warp in interior.warps)
		{
			if (warp.TargetName == "Farm" || (parentLocation != null && warp.TargetName == parentLocation.NameOrUniqueName))
			{
				warp.TargetName = parentLocation?.NameOrUniqueName ?? warp.TargetName;
				warp.TargetX = humanDoor.X + (int)tileX;
				warp.TargetY = humanDoor.Y + (int)tileY + 1;
			}
		}
	}

	/// <summary>Get whether the building has an interior location.</summary>
	public bool HasIndoors()
	{
		if (indoors.Value == null)
		{
			return nonInstancedIndoorsName.Value != null;
		}
		return true;
	}

	/// <summary>Get whether the building has an interior location with the given unique name.</summary>
	/// <param name="name">The name to check.</param>
	public bool HasIndoorsName(string name)
	{
		string actualName = GetIndoorsName();
		if (actualName != null)
		{
			return string.Equals(actualName, name, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	/// <summary>Get the unique name of the location within this building, if it's linked to an instanced or non-instanced interior.</summary>
	public string GetIndoorsName()
	{
		return indoors.Value?.NameOrUniqueName ?? nonInstancedIndoorsName.Value;
	}

	/// <summary>Get the type of indoors location this building has.</summary>
	public IndoorsType GetIndoorsType()
	{
		if (indoors.Value != null)
		{
			return IndoorsType.Instanced;
		}
		if (nonInstancedIndoorsName.Value != null)
		{
			return IndoorsType.Global;
		}
		return IndoorsType.None;
	}

	/// <summary>Get the location within this building, if it's linked to an instanced or non-instanced interior.</summary>
	public GameLocation GetIndoors()
	{
		if (indoors.Value != null)
		{
			return indoors.Value;
		}
		if (nonInstancedIndoorsName.Value != null)
		{
			return Game1.getLocationFromName(nonInstancedIndoorsName.Value);
		}
		return null;
	}

	protected virtual GameLocation createIndoors(BuildingData data, string nameOfIndoorsWithoutUnique)
	{
		GameLocation lcl_indoors = null;
		if (data != null && !string.IsNullOrEmpty(data.IndoorMap))
		{
			Type locationType = typeof(GameLocation);
			if (data.IndoorMapType != null)
			{
				Exception exception = null;
				try
				{
					locationType = Type.GetType(data.IndoorMapType);
				}
				catch (Exception ex)
				{
					exception = ex;
				}
				if ((object)locationType == null || exception != null)
				{
					Game1.log.Error($"Error constructing interior type '{data.IndoorMapType}' for building '{buildingType.Value}'" + ((exception != null) ? "." : ": that type doesn't exist."));
					locationType = typeof(GameLocation);
				}
			}
			string mapAssetName = "Maps\\" + data.IndoorMap;
			try
			{
				lcl_indoors = (GameLocation)Activator.CreateInstance(locationType, mapAssetName, buildingType.Value);
			}
			catch (Exception)
			{
				try
				{
					lcl_indoors = (GameLocation)Activator.CreateInstance(locationType, mapAssetName);
				}
				catch (Exception e)
				{
					Game1.log.Error($"Error trying to instantiate indoors for '{buildingType}'", e);
					lcl_indoors = new GameLocation("Maps\\" + nameOfIndoorsWithoutUnique, buildingType);
				}
			}
		}
		if (lcl_indoors != null)
		{
			lcl_indoors.uniqueName.Value = nameOfIndoorsWithoutUnique + GuidHelper.NewGuid();
			lcl_indoors.IsFarm = true;
			lcl_indoors.isStructure.Value = true;
			updateInteriorWarps(lcl_indoors);
		}
		return lcl_indoors;
	}

	public virtual Point getPointForHumanDoor()
	{
		return new Point((int)tileX + humanDoor.Value.X, (int)tileY + humanDoor.Value.Y);
	}

	public virtual Microsoft.Xna.Framework.Rectangle getRectForHumanDoor()
	{
		return new Microsoft.Xna.Framework.Rectangle(getPointForHumanDoor().X * 64, getPointForHumanDoor().Y * 64, 64, 64);
	}

	public Microsoft.Xna.Framework.Rectangle getRectForAnimalDoor()
	{
		return getRectForAnimalDoor(GetData());
	}

	public virtual Microsoft.Xna.Framework.Rectangle getRectForAnimalDoor(BuildingData data)
	{
		if (data != null)
		{
			Microsoft.Xna.Framework.Rectangle rect = data.AnimalDoor;
			return new Microsoft.Xna.Framework.Rectangle((rect.X + (int)tileX) * 64, (rect.Y + (int)tileY) * 64, rect.Width * 64, rect.Height * 64);
		}
		return new Microsoft.Xna.Framework.Rectangle((animalDoor.X + (int)tileX) * 64, ((int)tileY + animalDoor.Y) * 64, 64, 64);
	}

	public virtual void load()
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		BuildingData data = GetData();
		if (!hasLoaded)
		{
			hasLoaded = true;
			if (data != null)
			{
				if (data.NonInstancedIndoorLocation == null && nonInstancedIndoorsName.Value != null)
				{
					GameLocation interior = GetIndoors();
					if (interior != null)
					{
						interior.parentLocationName.Value = null;
					}
					nonInstancedIndoorsName.Value = null;
				}
				else if (data.NonInstancedIndoorLocation != null)
				{
					bool nonInstancedLocationAlreadyUsed = false;
					Utility.ForEachBuilding(delegate(Building building)
					{
						if (building.HasIndoorsName(data.NonInstancedIndoorLocation))
						{
							nonInstancedLocationAlreadyUsed = true;
							return false;
						}
						return true;
					});
					if (!nonInstancedLocationAlreadyUsed)
					{
						nonInstancedIndoorsName.Value = Game1.RequireLocation(data.NonInstancedIndoorLocation).NameOrUniqueName;
					}
				}
			}
			LoadFromBuildingData(data);
		}
		if (nonInstancedIndoorsName.Value != null)
		{
			UpdateIndoorParent();
		}
		else
		{
			string nameOfIndoorsWithoutUnique = data?.IndoorMap ?? indoors.Value?.Name;
			GameLocation indoorInstance = createIndoors(data, nameOfIndoorsWithoutUnique);
			if (indoorInstance != null && indoors.Value != null)
			{
				indoorInstance.characters.Set(indoors.Value.characters);
				indoorInstance.netObjects.MoveFrom(indoors.Value.netObjects);
				indoorInstance.terrainFeatures.MoveFrom(indoors.Value.terrainFeatures);
				indoorInstance.IsFarm = true;
				indoorInstance.IsOutdoors = false;
				indoorInstance.isStructure.Value = true;
				indoorInstance.miniJukeboxCount.Set(indoors.Value.miniJukeboxCount.Value);
				indoorInstance.miniJukeboxTrack.Set(indoors.Value.miniJukeboxTrack.Value);
				NetString uniqueName = indoorInstance.uniqueName;
				NetString uniqueName2 = indoors.Value.uniqueName;
				uniqueName.Value = (((object)uniqueName2 != null) ? ((string)uniqueName2) : (nameOfIndoorsWithoutUnique + ((int)tileX * 2000 + (int)tileY)));
				indoorInstance.numberOfSpawnedObjectsOnMap = indoors.Value.numberOfSpawnedObjectsOnMap;
				indoorInstance.animals.MoveFrom(indoors.Value.animals);
				if (indoors.Value is AnimalHouse house && indoorInstance is AnimalHouse houseInstance)
				{
					houseInstance.animalsThatLiveHere.Set(house.animalsThatLiveHere);
				}
				foreach (KeyValuePair<long, FarmAnimal> pair in indoorInstance.animals.Pairs)
				{
					pair.Value.reload(this);
				}
				indoorInstance.furniture.Set(indoors.Value.furniture);
				foreach (Furniture item in indoorInstance.furniture)
				{
					item.updateDrawPosition();
				}
				if (indoors.Value is Cabin cabin && indoorInstance is Cabin cabinInstance)
				{
					cabinInstance.fridge.Value = cabin.fridge.Value;
					cabinInstance.farmhandReference.Value = cabin.farmhandReference.Value;
				}
				indoorInstance.TransferDataFromSavedLocation(indoors.Value);
				indoors.Value = indoorInstance;
			}
			updateInteriorWarps();
			if (indoors.Value != null)
			{
				for (int i = indoors.Value.characters.Count - 1; i >= 0; i--)
				{
					SaveGame.initializeCharacter(indoors.Value.characters[i], indoors.Value);
				}
				foreach (TerrainFeature value in indoors.Value.terrainFeatures.Values)
				{
					value.loadSprite();
				}
				foreach (KeyValuePair<Vector2, Object> v in indoors.Value.objects.Pairs)
				{
					v.Value.initializeLightSource(v.Key);
					v.Value.reloadSprite();
				}
			}
		}
		if (data != null)
		{
			humanDoor.X = data.HumanDoor.X;
			humanDoor.Y = data.HumanDoor.Y;
		}
	}

	/// <summary>Get the extra tiles to treat as part of the building when placing it through a construction menu, if any. For example, the farmhouse uses this to make sure the stairs are clear.</summary>
	public IEnumerable<BuildingPlacementTile> GetAdditionalPlacementTiles()
	{
		IEnumerable<BuildingPlacementTile> enumerable = GetData()?.AdditionalPlacementTiles;
		return enumerable ?? LegacyShims.EmptyArray<BuildingPlacementTile>();
	}

	public bool isUnderConstruction(bool ignoreUpgrades = true)
	{
		if (!ignoreUpgrades && daysUntilUpgrade.Value > 0)
		{
			return true;
		}
		return (int)daysOfConstructionLeft > 0;
	}

	/// <summary>Get whether the building's bounds covers a given tile coordinate.</summary>
	/// <param name="tile">The tile position to check.</param>
	/// <param name="applyTilePropertyRadius">Whether to check the extra tiles around the building itself for which it may add tile properties.</param>
	public bool occupiesTile(Vector2 tile, bool applyTilePropertyRadius = false)
	{
		return occupiesTile((int)tile.X, (int)tile.Y, applyTilePropertyRadius);
	}

	/// <summary>Get whether the building's bounds covers a given tile coordinate.</summary>
	/// <param name="x">The X tile position to check.</param>
	/// <param name="y">The Y tile position to check</param>
	/// <param name="applyTilePropertyRadius">Whether to check the extra tiles around the building itself for which it may add tile properties.</param>
	public virtual bool occupiesTile(int x, int y, bool applyTilePropertyRadius = false)
	{
		int additionalRadius = (applyTilePropertyRadius ? GetAdditionalTilePropertyRadius() : 0);
		int leftX = tileX.Value;
		int topY = tileY.Value;
		int width = tilesWide.Value;
		int height = tilesHigh.Value;
		if (x >= leftX - additionalRadius && x < leftX + width + additionalRadius && y >= topY - additionalRadius)
		{
			return y < topY + height + additionalRadius;
		}
		return false;
	}

	public virtual bool isTilePassable(Vector2 tile)
	{
		bool occupied = occupiesTile(tile);
		if (occupied && isUnderConstruction())
		{
			return false;
		}
		BuildingData data = GetData();
		if (data != null && occupiesTile(tile, applyTilePropertyRadius: true))
		{
			return data.IsTilePassable((int)tile.X - tileX.Value, (int)tile.Y - tileY.Value);
		}
		return !occupied;
	}

	public virtual bool isTileOccupiedForPlacement(Vector2 tile, Object to_place)
	{
		if (!isTilePassable(tile))
		{
			return true;
		}
		return false;
	}

	/// <summary>If this building is fishable, get the color of the water at the given tile position.</summary>
	/// <param name="tile">The tile position.</param>
	/// <returns>Returns the water color to use, or <c>null</c> to use the location's default water color.</returns>
	public virtual Color? GetWaterColor(Vector2 tile)
	{
		return null;
	}

	public virtual bool isTileFishable(Vector2 tile)
	{
		return false;
	}

	/// <summary>Whether watering cans can be refilled from any tile covered by this building.</summary>
	/// <remarks>If this is false, watering cans may still be refillable based on tile data (e.g. the <c>WaterSource</c> back tile property).</remarks>
	public virtual bool CanRefillWateringCan()
	{
		return false;
	}

	/// <summary>Create a pixel rectangle for the building's ground footprint within its location.</summary>
	public Microsoft.Xna.Framework.Rectangle GetBoundingBox()
	{
		return new Microsoft.Xna.Framework.Rectangle((int)tileX * 64, (int)tileY * 64, (int)tilesWide * 64, (int)tilesHigh * 64);
	}

	public virtual bool intersects(Microsoft.Xna.Framework.Rectangle boundingBox)
	{
		Microsoft.Xna.Framework.Rectangle buildingRect = GetBoundingBox();
		int additionalRadius = GetAdditionalTilePropertyRadius();
		if (additionalRadius > 0)
		{
			buildingRect.Inflate(additionalRadius * 64, additionalRadius * 64);
		}
		if (buildingRect.Intersects(boundingBox))
		{
			int y = boundingBox.Top / 64;
			for (int maxY = boundingBox.Bottom / 64; y <= maxY; y++)
			{
				int x = boundingBox.Left / 64;
				for (int maxX = boundingBox.Right / 64; x <= maxX; x++)
				{
					if (!isTilePassable(new Vector2(x, y)))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public virtual void drawInMenu(SpriteBatch b, int x, int y)
	{
		BuildingData data = GetData();
		if (data != null)
		{
			x += (int)(data.DrawOffset.X * 4f);
			y += (int)(data.DrawOffset.Y * 4f);
		}
		float baseSortY = (int)tilesHigh * 64;
		float sortY = baseSortY;
		if (data != null)
		{
			sortY -= data.SortTileOffset * 64f;
		}
		sortY /= 10000f;
		if (ShouldDrawShadow(data))
		{
			drawShadow(b, x, y);
		}
		Microsoft.Xna.Framework.Rectangle mainSourceRect = getSourceRect();
		b.Draw(texture.Value, new Vector2(x, y), mainSourceRect, color, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, sortY);
		if (data?.DrawLayers == null)
		{
			return;
		}
		foreach (BuildingDrawLayer drawLayer in data.DrawLayers)
		{
			if (drawLayer.OnlyDrawIfChestHasContents == null)
			{
				sortY = baseSortY - drawLayer.SortTileOffset * 64f;
				sortY += 1f;
				if (drawLayer.DrawInBackground)
				{
					sortY = 0f;
				}
				sortY /= 10000f;
				Microsoft.Xna.Framework.Rectangle sourceRect = drawLayer.GetSourceRect((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds);
				sourceRect = ApplySourceRectOffsets(sourceRect);
				Texture2D layerTexture = texture.Value;
				if (drawLayer.Texture != null)
				{
					layerTexture = Game1.content.Load<Texture2D>(drawLayer.Texture);
				}
				b.Draw(layerTexture, new Vector2(x, y) + drawLayer.DrawPosition * 4f, sourceRect, Color.White, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, sortY);
			}
		}
	}

	public virtual void drawBackground(SpriteBatch b)
	{
		if (isMoving || (int)daysOfConstructionLeft > 0 || (int)newConstructionTimer > 0)
		{
			return;
		}
		BuildingData data = GetData();
		if (data?.DrawLayers == null)
		{
			return;
		}
		Vector2 drawOrigin = new Vector2(0f, getSourceRect().Height);
		Vector2 drawPosition = new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64);
		foreach (BuildingDrawLayer drawLayer in data.DrawLayers)
		{
			if (!drawLayer.DrawInBackground)
			{
				continue;
			}
			if (drawLayer.OnlyDrawIfChestHasContents != null)
			{
				Chest chest = GetBuildingChest(drawLayer.OnlyDrawIfChestHasContents);
				if (chest == null || chest.isEmpty())
				{
					continue;
				}
			}
			Microsoft.Xna.Framework.Rectangle sourceRect = drawLayer.GetSourceRect((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds);
			sourceRect = ApplySourceRectOffsets(sourceRect);
			Vector2 drawOffset = Vector2.Zero;
			if (drawLayer.AnimalDoorOffset != Point.Zero)
			{
				drawOffset = new Vector2((float)drawLayer.AnimalDoorOffset.X * animalDoorOpenAmount.Value, (float)drawLayer.AnimalDoorOffset.Y * animalDoorOpenAmount.Value);
			}
			Texture2D layerTexture = texture.Value;
			if (drawLayer.Texture != null)
			{
				layerTexture = Game1.content.Load<Texture2D>(drawLayer.Texture);
			}
			b.Draw(layerTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + (drawOffset - drawOrigin + drawLayer.DrawPosition) * 4f), sourceRect, color * alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0f);
		}
	}

	public virtual void draw(SpriteBatch b)
	{
		if (isMoving)
		{
			return;
		}
		if ((int)daysOfConstructionLeft > 0 || (int)newConstructionTimer > 0)
		{
			drawInConstruction(b);
			return;
		}
		BuildingData data = GetData();
		if (ShouldDrawShadow(data))
		{
			drawShadow(b);
		}
		float baseSortY = ((int)tileY + (int)tilesHigh) * 64;
		float sortY = baseSortY;
		if (data != null)
		{
			sortY -= data.SortTileOffset * 64f;
		}
		sortY /= 10000f;
		Vector2 drawPosition = new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64);
		Vector2 drawOffset = Vector2.Zero;
		if (data != null)
		{
			drawOffset = data.DrawOffset * 4f;
		}
		Microsoft.Xna.Framework.Rectangle mainSourceRect = getSourceRect();
		Vector2 drawOrigin = new Vector2(0f, mainSourceRect.Height);
		b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, drawPosition + drawOffset), mainSourceRect, color * alpha, 0f, drawOrigin, 4f, SpriteEffects.None, sortY);
		if ((bool)magical && buildingType.Value.Equals("Gold Clock"))
		{
			if ((bool)Game1.netWorldState.Value.goldenClocksTurnedOff)
			{
				b.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 68, (int)tileY * 64 - 56)), new Microsoft.Xna.Framework.Rectangle(498, 368, 13, 9), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.0001f);
			}
			else
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 92, (int)tileY * 64 - 40)), Town.hourHandSource, Color.White * alpha, (float)(Math.PI * 2.0 * (double)((float)(Game1.timeOfDay % 1200) / 1200f) + (double)((float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes / 23f)), new Vector2(2.5f, 8f), 3f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.0001f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 92, (int)tileY * 64 - 40)), Town.minuteHandSource, Color.White * alpha, (float)(Math.PI * 2.0 * (double)((float)(Game1.timeOfDay % 1000 % 100 % 60) / 60f) + (double)((float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 1.02f)), new Vector2(2.5f, 12f), 3f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.00011f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + 92, (int)tileY * 64 - 40)), Town.clockNub, Color.White * alpha, 0f, new Vector2(2f, 2f), 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.00012f);
			}
		}
		if (data != null)
		{
			foreach (Chest chest in buildingChests)
			{
				BuildingChest chestData = GetBuildingChestData(data, chest.Name);
				if (chestData.DisplayTile.X != -1f && chestData.DisplayTile.Y != -1f && chest.Items.Count > 0 && chest.Items[0] != null)
				{
					sortY = ((float)(int)tileY + chestData.DisplayTile.Y + 1f) * 64f;
					sortY += 1f;
					float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2) - chestData.DisplayHeight * 64f;
					float drawX = ((float)(int)tileX + chestData.DisplayTile.X) * 64f;
					float drawY = ((float)(int)tileY + chestData.DisplayTile.Y - 1f) * 64f;
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(drawX, drawY + yOffset)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, sortY / 10000f);
					ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(chest.Items[0].QualifiedItemId);
					b.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(drawX + 32f + 4f, drawY + 32f + yOffset)), itemData.GetSourceRect(), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (sortY + 1f) / 10000f);
				}
			}
			if (data.DrawLayers != null)
			{
				foreach (BuildingDrawLayer drawLayer in data.DrawLayers)
				{
					if (drawLayer.DrawInBackground)
					{
						continue;
					}
					if (drawLayer.OnlyDrawIfChestHasContents != null)
					{
						Chest chest = GetBuildingChest(drawLayer.OnlyDrawIfChestHasContents);
						if (chest == null || chest.isEmpty())
						{
							continue;
						}
					}
					sortY = baseSortY - drawLayer.SortTileOffset * 64f;
					sortY += 1f;
					sortY /= 10000f;
					Microsoft.Xna.Framework.Rectangle sourceRect = drawLayer.GetSourceRect((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds);
					sourceRect = ApplySourceRectOffsets(sourceRect);
					drawOffset = Vector2.Zero;
					if (drawLayer.AnimalDoorOffset != Point.Zero)
					{
						drawOffset = new Vector2((float)drawLayer.AnimalDoorOffset.X * animalDoorOpenAmount.Value, (float)drawLayer.AnimalDoorOffset.Y * animalDoorOpenAmount.Value);
					}
					Texture2D layerTexture = texture.Value;
					if (drawLayer.Texture != null)
					{
						layerTexture = Game1.content.Load<Texture2D>(drawLayer.Texture);
					}
					b.Draw(layerTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + (drawOffset - drawOrigin + drawLayer.DrawPosition) * 4f), sourceRect, color * alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, sortY);
				}
			}
		}
		if ((int)daysUntilUpgrade <= 0)
		{
			return;
		}
		if (data != null)
		{
			if (data.UpgradeSignTile.X >= 0f)
			{
				sortY = ((float)(int)tileY + data.UpgradeSignTile.Y + 1f) * 64f;
				sortY += 2f;
				sortY /= 10000f;
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, getUpgradeSignLocation()), new Microsoft.Xna.Framework.Rectangle(367, 309, 16, 15), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, sortY);
			}
		}
		else if (GetIndoors() is Shed)
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, getUpgradeSignLocation()), new Microsoft.Xna.Framework.Rectangle(367, 309, 16, 15), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.0001f);
		}
	}

	public bool ShouldDrawShadow(BuildingData data)
	{
		return data?.DrawShadow ?? true;
	}

	public virtual void drawShadow(SpriteBatch b, int localX = -1, int localY = -1)
	{
		Microsoft.Xna.Framework.Rectangle sourceRectForMenu = getSourceRectForMenu() ?? getSourceRect();
		Vector2 basePosition = ((localX == -1) ? Game1.GlobalToLocal(new Vector2((int)tileX * 64, ((int)tileY + (int)tilesHigh) * 64)) : new Vector2(localX, localY + sourceRectForMenu.Height * 4));
		b.Draw(Game1.mouseCursors, basePosition, leftShadow, Color.White * ((localX == -1) ? alpha : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
		for (int x = 1; x < (int)tilesWide - 1; x++)
		{
			b.Draw(Game1.mouseCursors, basePosition + new Vector2(x * 64, 0f), middleShadow, Color.White * ((localX == -1) ? alpha : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
		}
		b.Draw(Game1.mouseCursors, basePosition + new Vector2(((int)tilesWide - 1) * 64, 0f), rightShadow, Color.White * ((localX == -1) ? alpha : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
	}

	public virtual void OnStartMove()
	{
	}

	public virtual void OnEndMove()
	{
		Game1.player.team.SendBuildingMovedEvent(GetParentLocation(), this);
	}

	public Point getPorchStandingSpot()
	{
		if (isCabin)
		{
			return new Point((int)tileX + 1, (int)tileY + (int)tilesHigh - 1);
		}
		return new Point(0, 0);
	}

	public virtual bool doesTileHaveProperty(int tile_x, int tile_y, string property_name, string layer_name, ref string property_value)
	{
		BuildingData data = GetData();
		if (data != null && (int)daysOfConstructionLeft <= 0 && data.HasPropertyAtTile(tile_x - tileX.Value, tile_y - tileY.Value, property_name, layer_name, ref property_value))
		{
			return true;
		}
		if (property_name == "NoSpawn" && layer_name == "Back" && occupiesTile(tile_x, tile_y))
		{
			property_value = "All";
			return true;
		}
		return false;
	}

	public Point getMailboxPosition()
	{
		if (isCabin)
		{
			return new Point((int)tileX + (int)tilesWide - 1, (int)tileY + (int)tilesHigh - 1);
		}
		return new Point(68, 16);
	}

	/// <summary>Get the number of extra tiles around the building for which it may add tile properties, but without hiding tile properties from the underlying ground that aren't overwritten by the building data.</summary>
	public virtual int GetAdditionalTilePropertyRadius()
	{
		return GetData()?.AdditionalTilePropertyRadius ?? 0;
	}

	public void removeOverlappingBushes(GameLocation location)
	{
		for (int x = tileX; x < (int)tileX + (int)tilesWide; x++)
		{
			for (int y = tileY; y < (int)tileY + (int)tilesHigh; y++)
			{
				if (location.isTerrainFeatureAt(x, y))
				{
					LargeTerrainFeature large_feature = location.getLargeTerrainFeatureAt(x, y);
					if (large_feature is Bush)
					{
						location.largeTerrainFeatures.Remove(large_feature);
					}
				}
			}
		}
	}

	public virtual void drawInConstruction(SpriteBatch b)
	{
		int drawPercentage = Math.Min(16, Math.Max(0, (int)(16f - (float)(int)newConstructionTimer / 1000f * 16f)));
		float drawPercentageReal = (float)(2000 - (int)newConstructionTimer) / 2000f;
		if ((bool)magical || (int)daysOfConstructionLeft <= 0)
		{
			BuildingData data = GetData();
			if (ShouldDrawShadow(data))
			{
				drawShadow(b);
			}
			Microsoft.Xna.Framework.Rectangle mainSourceRect = getSourceRect();
			Microsoft.Xna.Framework.Rectangle sourceRectForMenu = getSourceRectForMenu() ?? mainSourceRect;
			int yPos = (int)((float)(mainSourceRect.Height * 4) * (1f - drawPercentageReal));
			float baseSortY = ((int)tileY + (int)tilesHigh) * 64;
			float sortY = baseSortY;
			if (data != null)
			{
				sortY -= data.SortTileOffset * 64f;
			}
			sortY /= 10000f;
			Vector2 drawPosition = new Vector2((int)tileX * 64, (int)tileY * 64 + (int)tilesHigh * 64);
			Vector2 drawOffset = Vector2.Zero;
			if (data != null)
			{
				drawOffset = data.DrawOffset * 4f;
			}
			Vector2 offset = new Vector2(0f, yPos + 4 - yPos % 4);
			Vector2 drawOrigin = new Vector2(0f, mainSourceRect.Height);
			b.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, drawPosition + offset + drawOffset), new Microsoft.Xna.Framework.Rectangle(mainSourceRect.Left, mainSourceRect.Bottom - (int)(drawPercentageReal * (float)mainSourceRect.Height), sourceRectForMenu.Width, (int)((float)mainSourceRect.Height * drawPercentageReal)), color * alpha, 0f, new Vector2(0f, mainSourceRect.Height), 4f, SpriteEffects.None, sortY);
			if (data?.DrawLayers != null)
			{
				foreach (BuildingDrawLayer drawLayer in data.DrawLayers)
				{
					if (drawLayer.OnlyDrawIfChestHasContents != null)
					{
						continue;
					}
					sortY = baseSortY - drawLayer.SortTileOffset * 64f;
					sortY += 1f;
					sortY /= 10000f;
					Microsoft.Xna.Framework.Rectangle sourceRect = drawLayer.GetSourceRect((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds);
					sourceRect = ApplySourceRectOffsets(sourceRect);
					float cutoffPixels = (float)(yPos / 4) - drawLayer.DrawPosition.Y;
					drawOffset = Vector2.Zero;
					if (!(cutoffPixels > (float)sourceRect.Height))
					{
						if (cutoffPixels > 0f)
						{
							drawOffset.Y += cutoffPixels;
							sourceRect.Y += (int)cutoffPixels;
							sourceRect.Height -= (int)cutoffPixels;
						}
						Texture2D layerTexture = texture.Value;
						if (drawLayer.Texture != null)
						{
							layerTexture = Game1.content.Load<Texture2D>(drawLayer.Texture);
						}
						b.Draw(layerTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + (drawOffset - drawOrigin + drawLayer.DrawPosition) * 4f), sourceRect, color * alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, sortY);
					}
				}
			}
			if ((bool)magical)
			{
				for (int i = 0; i < (int)tilesWide * 4; i++)
				{
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + i * 16, (float)((int)tileY * 64 - mainSourceRect.Height * 4 + (int)tilesHigh * 64) + (float)(mainSourceRect.Height * 4) * (1f - drawPercentageReal))) + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2) - ((i % 2 == 0) ? 32 : 8)), new Microsoft.Xna.Framework.Rectangle(536 + ((int)newConstructionTimer + i * 4) % 56 / 8 * 8, 1945, 8, 8), (i % 2 == 1) ? (Color.Pink * alpha) : (Color.LightPink * alpha), 0f, new Vector2(0f, 0f), 4f + (float)Game1.random.Next(100) / 100f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.0001f);
					if (i % 2 == 0)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 + i * 16, (float)((int)tileY * 64 - mainSourceRect.Height * 4 + (int)tilesHigh * 64) + (float)(mainSourceRect.Height * 4) * (1f - drawPercentageReal))) + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2) + ((i % 2 == 0) ? 32 : 8)), new Microsoft.Xna.Framework.Rectangle(536 + ((int)newConstructionTimer + i * 4) % 56 / 8 * 8, 1945, 8, 8), Color.White * alpha, 0f, new Vector2(0f, 0f), 4f + (float)Game1.random.Next(100) / 100f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.0001f);
					}
				}
				return;
			}
			for (int i = 0; i < (int)tilesWide * 4; i++)
			{
				b.Draw(Game1.animations, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 - 16 + i * 16, (float)((int)tileY * 64 - mainSourceRect.Height * 4 + (int)tilesHigh * 64) + (float)(mainSourceRect.Height * 4) * (1f - drawPercentageReal))) + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2) - ((i % 2 == 0) ? 32 : 8)), new Microsoft.Xna.Framework.Rectangle(((int)newConstructionTimer + i * 20) % 304 / 38 * 64, 768, 64, 64), Color.White * alpha * ((float)(int)newConstructionTimer / 500f), 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.0001f);
				if (i % 2 == 0)
				{
					b.Draw(Game1.animations, Game1.GlobalToLocal(Game1.viewport, new Vector2((int)tileX * 64 - 16 + i * 16, (float)((int)tileY * 64 - mainSourceRect.Height * 4 + (int)tilesHigh * 64) + (float)(mainSourceRect.Height * 4) * (1f - drawPercentageReal))) + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2) - ((i % 2 == 0) ? 32 : 8)), new Microsoft.Xna.Framework.Rectangle(((int)newConstructionTimer + i * 20) % 400 / 50 * 64, 2944, 64, 64), Color.White * alpha * ((float)(int)newConstructionTimer / 500f), 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, (float)(((int)tileY + (int)tilesHigh) * 64) / 10000f + 0.0001f);
				}
			}
			return;
		}
		bool drawFloor = (int)daysOfConstructionLeft == 1;
		for (int x = tileX; x < (int)tileX + (int)tilesWide; x++)
		{
			for (int y = tileY; y < (int)tileY + (int)tilesHigh; y++)
			{
				if (x == (int)tileX + (int)tilesWide / 2 && y == (int)tileY + (int)tilesHigh - 1)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16 - 4), new Microsoft.Xna.Framework.Rectangle(367, 277, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(367, 309, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 64 - 1) / 10000f);
				}
				else if (x == (int)tileX && y == (int)tileY)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(351, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(351, 293, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 64 - 1) / 10000f);
				}
				else if (x == (int)tileX + (int)tilesWide - 1 && y == (int)tileY)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(383, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(383, 293, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 64 - 1) / 10000f);
				}
				else if (x == (int)tileX + (int)tilesWide - 1 && y == (int)tileY + (int)tilesHigh - 1)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(383, 277, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(383, 325, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
				}
				else if (x == (int)tileX && y == (int)tileY + (int)tilesHigh - 1)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(351, 277, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(351, 325, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
				}
				else if (x == (int)tileX + (int)tilesWide - 1)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(383, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(383, 309, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
				}
				else if (y == (int)tileY + (int)tilesHigh - 1)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(367, 277, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(367, 325, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
				}
				else if (x == (int)tileX)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(351, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(351, 309, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
				}
				else if (y == (int)tileY)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(367, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + (((int)newConstructionTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(367, 293, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 64 - 1) / 10000f);
				}
				else if (drawFloor)
				{
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(367, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
				}
			}
		}
	}
}
