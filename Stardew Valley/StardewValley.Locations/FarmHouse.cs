using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.Monsters;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.TerrainFeatures;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Locations;

public class FarmHouse : DecoratableLocation
{
	[XmlElement("fridge")]
	public readonly NetRef<Chest> fridge = new NetRef<Chest>(new Chest(playerChest: true));

	[XmlIgnore]
	public readonly NetInt synchronizedDisplayedLevel = new NetInt(-1);

	public Point fridgePosition;

	[XmlIgnore]
	public Point spouseRoomSpot;

	private string lastSpouseRoom;

	[XmlIgnore]
	private LocalizedContentManager mapLoader;

	public List<Warp> cellarWarps;

	[XmlElement("cribStyle")]
	public readonly NetInt cribStyle = new NetInt(1)
	{
		InterpolationEnabled = false
	};

	[XmlIgnore]
	public int previousUpgradeLevel = -1;

	private int currentlyDisplayedUpgradeLevel;

	private bool displayingSpouseRoom;

	private Color nightLightingColor = new Color(180, 180, 0);

	private Color rainLightingColor = new Color(90, 90, 0);

	/// <summary>The player who owns this home.</summary>
	[XmlIgnore]
	public virtual Farmer owner => Game1.MasterPlayer;

	/// <summary>Whether the home has an assigned player, regardless of whether they've finished creating their character..</summary>
	/// <remarks>See also <see cref="P:StardewValley.Locations.FarmHouse.IsOwnerActivated" />.</remarks>
	[XmlIgnore]
	[MemberNotNullWhen(true, "owner")]
	public virtual bool HasOwner
	{
		[MemberNotNullWhen(true, "owner")]
		get
		{
			return owner != null;
		}
	}

	/// <summary>The unique ID of the player who owns this home, if any.</summary>
	public virtual long OwnerId => owner?.UniqueMultiplayerID ?? 0;

	/// <summary>Whether the home has an assigned player and they've finished creating their character.</summary>
	/// <remarks>See also <see cref="P:StardewValley.Locations.FarmHouse.HasOwner" />.</remarks>
	[MemberNotNullWhen(true, "owner")]
	public bool IsOwnerActivated
	{
		[MemberNotNullWhen(true, "owner")]
		get
		{
			return owner?.isActive() ?? false;
		}
	}

	/// <summary>Whether the home is owned by the current player.</summary>
	[MemberNotNullWhen(true, "owner")]
	public bool IsOwnedByCurrentPlayer
	{
		[MemberNotNullWhen(true, "owner")]
		get
		{
			return owner?.UniqueMultiplayerID == Game1.player.UniqueMultiplayerID;
		}
	}

	[XmlIgnore]
	public virtual int upgradeLevel
	{
		get
		{
			return owner?.HouseUpgradeLevel ?? 0;
		}
		set
		{
			if (HasOwner)
			{
				owner.houseUpgradeLevel.Value = value;
			}
		}
	}

	public FarmHouse()
	{
		fridge.Value.Location = this;
	}

	public FarmHouse(string m, string name)
		: base(m, name)
	{
		fridge.Value.Location = this;
		ReadWallpaperAndFloorTileData();
		Farm farm = Game1.getFarm();
		AddStarterGiftBox(farm);
		AddStarterFurniture(farm);
		SetStarterFlooring(farm);
		SetStarterWallpaper(farm);
	}

	/// <summary>Place the starter gift box when the farmhouse is first created.</summary>
	/// <param name="farm">The farm instance to which a farmhouse is being added.</param>
	private void AddStarterGiftBox(Farm farm)
	{
		Chest box = new Chest(null, Vector2.Zero, giftbox: true, 0, giftboxIsStarterGift: true);
		string[] fields = farm.GetMapPropertySplitBySpaces("FarmHouseStarterGift");
		for (int i = 0; i < fields.Length; i += 2)
		{
			if (!ArgUtility.TryGet(fields, i, out var giftId, out var error, allowBlank: false) || !ArgUtility.TryGetOptionalInt(fields, i + 1, out var count, out error))
			{
				farm.LogMapPropertyError("FarmHouseStarterGift", fields, error);
			}
			else
			{
				box.Items.Add(ItemRegistry.Create(giftId, count));
			}
		}
		if (!box.Items.Any())
		{
			Item parsnipSeeds = ItemRegistry.Create("(O)472", 15);
			box.Items.Add(parsnipSeeds);
		}
		if (!farm.TryGetMapPropertyAs("FarmHouseStarterSeedsPosition", out Vector2 tile, required: false))
		{
			switch (Game1.whichFarm)
			{
			case 1:
			case 2:
			case 4:
				tile = new Vector2(4f, 7f);
				break;
			case 3:
				tile = new Vector2(2f, 9f);
				break;
			case 6:
				tile = new Vector2(8f, 6f);
				break;
			default:
				tile = new Vector2(3f, 7f);
				break;
			}
		}
		objects.Add(tile, box);
	}

	/// <summary>Place the starter furniture when the farmhouse is first created.</summary>
	/// <param name="farm">The farm instance to which a farmhouse is being added.</param>
	private void AddStarterFurniture(Farm farm)
	{
		furniture.Add(new BedFurniture(BedFurniture.DEFAULT_BED_INDEX, new Vector2(9f, 8f)));
		string[] fields = farm.GetMapPropertySplitBySpaces("FarmHouseFurniture");
		if (fields.Any())
		{
			for (int i = 0; i < fields.Length; i += 4)
			{
				if (!ArgUtility.TryGetInt(fields, i, out var index, out var error) || !ArgUtility.TryGetVector2(fields, i + 1, out var tile, out error) || !ArgUtility.TryGetInt(fields, i + 3, out var rotations, out error))
				{
					farm.LogMapPropertyError("FarmHouseFurniture", fields, error);
					continue;
				}
				Furniture newFurniture = ItemRegistry.Create<Furniture>("(F)" + index);
				newFurniture.InitializeAtTile(tile);
				newFurniture.isOn.Value = true;
				for (int rotation = 0; rotation < rotations; rotation++)
				{
					newFurniture.rotate();
				}
				Furniture targetFurniture = GetFurnitureAt(tile);
				if (targetFurniture != null)
				{
					targetFurniture.heldObject.Value = newFurniture;
				}
				else
				{
					furniture.Add(newFurniture);
				}
			}
			return;
		}
		switch (Game1.whichFarm)
		{
		case 0:
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1120").SetPlacement(5, 4).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1364")));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1376").SetPlacement(1, 10));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)0").SetPlacement(4, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1466").SetPlacement(1, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1614").SetPlacement(3, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1618").SetPlacement(6, 8));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1602").SetPlacement(5, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1792").SetPlacement(getFireplacePoint()));
			break;
		case 1:
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1122").SetPlacement(1, 6).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1367")));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)3").SetPlacement(1, 5));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1680").SetPlacement(5, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1673").SetPlacement(1, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1673").SetPlacement(3, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1676").SetPlacement(5, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1737").SetPlacement(6, 8));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1742").SetPlacement(5, 5));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1675").SetPlacement(10, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1792").SetPlacement(getFireplacePoint()));
			objects.Add(new Vector2(4f, 4f), ItemRegistry.Create<Object>("(BC)FishSmoker"));
			break;
		case 2:
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1134").SetPlacement(1, 7).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1748")));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)3").SetPlacement(1, 6));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1680").SetPlacement(6, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1296").SetPlacement(1, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1682").SetPlacement(3, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1777").SetPlacement(6, 5));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1745").SetPlacement(6, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1747").SetPlacement(5, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1296").SetPlacement(10, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1792").SetPlacement(getFireplacePoint()));
			break;
		case 3:
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1218").SetPlacement(1, 6).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1368")));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1755").SetPlacement(1, 5));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1755").SetPlacement(3, 6, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1680").SetPlacement(5, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1751").SetPlacement(5, 10));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1749").SetPlacement(3, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1753").SetPlacement(5, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1742").SetPlacement(5, 5));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1794").SetPlacement(getFireplacePoint()));
			break;
		case 4:
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1680").SetPlacement(1, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1628").SetPlacement(1, 5));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1393").SetPlacement(3, 4).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1369")));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1678").SetPlacement(10, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1812").SetPlacement(3, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1630").SetPlacement(1, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1811").SetPlacement(6, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1389").SetPlacement(10, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1758").SetPlacement(1, 10));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1794").SetPlacement(getFireplacePoint()));
			break;
		case 5:
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1466").SetPlacement(1, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1614").SetPlacement(3, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1614").SetPlacement(6, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1601").SetPlacement(10, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)202").SetPlacement(3, 4, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1124").SetPlacement(4, 4, 1).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1379")));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)202").SetPlacement(6, 4, 3));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1378").SetPlacement(10, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1377").SetPlacement(1, 9));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1445").SetPlacement(1, 10));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1618").SetPlacement(2, 9));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1792").SetPlacement(getFireplacePoint()));
			break;
		case 6:
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1680").SetPlacement(4, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1614").SetPlacement(7, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1294").SetPlacement(3, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1283").SetPlacement(1, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1614").SetPlacement(8, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)202").SetPlacement(7, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1294").SetPlacement(10, 4));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)6").SetPlacement(2, 6, 1));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)6").SetPlacement(5, 7, 3));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1124").SetPlacement(3, 6).SetHeldObject(ItemRegistry.Create<Furniture>("(F)1362")));
			furniture.Add(ItemRegistry.Create<Furniture>("(F)1228").SetPlacement(2, 9));
			break;
		}
	}

	/// <summary>Set the initial flooring when the farmhouse is first created, if any.</summary>
	/// <param name="farm">The farm instance to which a farmhouse is being added.</param>
	private void SetStarterFlooring(Farm farm, string styleToOverride = null)
	{
		string id = farm.getMapProperty("FarmHouseFlooring");
		if (id == null)
		{
			switch (Game1.whichFarm)
			{
			case 1:
				id = "1";
				break;
			case 2:
				id = "34";
				break;
			case 3:
				id = "18";
				break;
			case 4:
				id = "4";
				break;
			case 5:
				id = "5";
				break;
			case 6:
				id = "35";
				break;
			}
		}
		if (id != null)
		{
			if (styleToOverride != null)
			{
				OverrideSpecificFlooring(id, null, styleToOverride);
			}
			else
			{
				SetFloor(id, null);
			}
		}
	}

	public override void ReadWallpaperAndFloorTileData()
	{
		base.ReadWallpaperAndFloorTileData();
		if (upgradeLevel < 3 && Game1.getLocationFromName("Farm", isStructure: false) is Farm farm)
		{
			SetStarterWallpaper(farm, "0");
			SetStarterFlooring(farm, "0");
		}
	}

	/// <summary>Set the initial wallpaper when the farmhouse is first created, if any.</summary>
	/// <param name="farm">The farm instance to which a farmhouse is being added.</param>
	private void SetStarterWallpaper(Farm farm, string styleToOverride = null)
	{
		string id = farm.getMapProperty("FarmHouseWallpaper");
		if (id == null)
		{
			switch (Game1.whichFarm)
			{
			case 1:
				id = "11";
				break;
			case 2:
				id = "92";
				break;
			case 3:
				id = "12";
				break;
			case 4:
				id = "95";
				break;
			case 5:
				id = "65";
				break;
			case 6:
				id = "106";
				break;
			}
		}
		if (id != null)
		{
			if (styleToOverride != null)
			{
				OverrideSpecificWallpaper(id, null, styleToOverride);
			}
			else
			{
				SetWallpaper(id, null);
			}
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(fridge, "fridge").AddField(cribStyle, "cribStyle").AddField(synchronizedDisplayedLevel, "synchronizedDisplayedLevel");
		cribStyle.fieldChangeVisibleEvent += delegate
		{
			if (map != null)
			{
				if (_appliedMapOverrides != null && _appliedMapOverrides.Contains("crib"))
				{
					_appliedMapOverrides.Remove("crib");
				}
				UpdateChildRoom();
				ReadWallpaperAndFloorTileData();
				setWallpapers();
				setFloors();
			}
		};
		fridge.fieldChangeEvent += delegate(NetRef<Chest> field, Chest oldValue, Chest newValue)
		{
			newValue.Location = this;
		};
	}

	public List<Child> getChildren()
	{
		return characters.OfType<Child>().ToList();
	}

	public int getChildrenCount()
	{
		int count = 0;
		foreach (NPC character in characters)
		{
			if (character is Child)
			{
				count++;
			}
		}
		return count;
	}

	public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false, bool skipCollisionEffects = false)
	{
		return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding);
	}

	public override void performTenMinuteUpdate(int timeOfDay)
	{
		base.performTenMinuteUpdate(timeOfDay);
		foreach (NPC c in characters)
		{
			if (c.isMarried())
			{
				if (c.getSpouse() == Game1.player)
				{
					c.checkForMarriageDialogue(timeOfDay, this);
				}
				if (Game1.IsMasterGame && Game1.timeOfDay >= 2200 && Game1.IsMasterGame && c.TilePoint != getSpouseBedSpot(c.Name) && (timeOfDay == 2200 || (c.controller == null && timeOfDay % 100 % 30 == 0)))
				{
					Point bed_spot = getSpouseBedSpot(c.Name);
					c.controller = null;
					PathFindController.endBehavior end_behavior = null;
					bool found_bed = GetSpouseBed() != null;
					if (found_bed)
					{
						end_behavior = spouseSleepEndFunction;
					}
					c.controller = new PathFindController(c, this, bed_spot, 0, end_behavior);
					if (c.controller.pathToEndPoint == null || !isTileOnMap(c.controller.pathToEndPoint.Last()))
					{
						c.controller = null;
					}
					else if (found_bed)
					{
						foreach (Furniture item in furniture)
						{
							if (item is BedFurniture bed && bed.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(bed_spot.X * 64, bed_spot.Y * 64, 64, 64)))
							{
								bed.ReserveForNPC();
								break;
							}
						}
					}
				}
			}
			if (c is Child child)
			{
				child.tenMinuteUpdate();
			}
		}
	}

	public static void spouseSleepEndFunction(Character c, GameLocation location)
	{
		if (!(c is NPC npc))
		{
			return;
		}
		if (DataLoader.AnimationDescriptions(Game1.content).ContainsKey(npc.name.Value.ToLower() + "_sleep"))
		{
			npc.playSleepingAnimation();
		}
		Microsoft.Xna.Framework.Rectangle npcBounds = npc.GetBoundingBox();
		foreach (Furniture item in location.furniture)
		{
			if (item is BedFurniture bed && bed.GetBoundingBox().Intersects(npcBounds))
			{
				bed.ReserveForNPC();
				break;
			}
		}
		if (Game1.random.NextDouble() < 0.1)
		{
			if (Game1.random.NextDouble() < 0.8)
			{
				npc.showTextAboveHead(Game1.content.LoadString("Strings\\1_6_Strings:Spouse_Goodnight0", npc.getTermOfSpousalEndearment(Game1.random.NextDouble() < 0.1)));
			}
			else
			{
				npc.showTextAboveHead(Game1.content.LoadString("Strings\\1_6_Strings:Spouse_Goodnight1"));
			}
		}
	}

	public virtual Point getFrontDoorSpot()
	{
		foreach (Warp warp in warps)
		{
			if (warp.TargetName == "Farm")
			{
				if (this is Cabin)
				{
					return new Point(warp.TargetX, warp.TargetY);
				}
				if (warp.TargetX == 64 && warp.TargetY == 15)
				{
					return Game1.getFarm().GetMainFarmHouseEntry();
				}
				return new Point(warp.TargetX, warp.TargetY);
			}
		}
		return Game1.getFarm().GetMainFarmHouseEntry();
	}

	public virtual Point getPorchStandingSpot()
	{
		Point p = Game1.getFarm().GetMainFarmHouseEntry();
		p.X += 2;
		return p;
	}

	public Point getKitchenStandingSpot()
	{
		if (TryGetMapPropertyAs("KitchenStandingLocation", out Point position, required: false))
		{
			return position;
		}
		switch (upgradeLevel)
		{
		case 1:
			return new Point(4, 5);
		case 2:
		case 3:
			return new Point(22, 24);
		default:
			return new Point(-1000, -1000);
		}
	}

	public virtual BedFurniture GetSpouseBed()
	{
		if (HasOwner)
		{
			if (owner.getSpouse()?.Name == "Krobus")
			{
				return null;
			}
			if (owner.hasCurrentOrPendingRoommate() && GetBed(BedFurniture.BedType.Single) != null)
			{
				return GetBed(BedFurniture.BedType.Single);
			}
		}
		return GetBed(BedFurniture.BedType.Double);
	}

	public Point getSpouseBedSpot(string spouseName)
	{
		if (spouseName == "Krobus")
		{
			NPC characterFromName = Game1.getCharacterFromName(name);
			if (characterFromName != null && characterFromName.isRoommate())
			{
				goto IL_0035;
			}
		}
		if (GetSpouseBed() != null)
		{
			BedFurniture spouseBed = GetSpouseBed();
			Point bed_spot = GetSpouseBed().GetBedSpot();
			if (spouseBed.bedType == BedFurniture.BedType.Double)
			{
				bed_spot.X++;
			}
			return bed_spot;
		}
		goto IL_0035;
		IL_0035:
		return GetSpouseRoomSpot();
	}

	public Point GetSpouseRoomSpot()
	{
		if (upgradeLevel == 0)
		{
			return new Point(-1000, -1000);
		}
		return spouseRoomSpot;
	}

	public BedFurniture GetBed(BedFurniture.BedType bed_type = BedFurniture.BedType.Any, int index = 0)
	{
		foreach (Furniture item in furniture)
		{
			if (item is BedFurniture bed && (bed_type == BedFurniture.BedType.Any || bed.bedType == bed_type))
			{
				if (index == 0)
				{
					return bed;
				}
				index--;
			}
		}
		return null;
	}

	public Point GetPlayerBedSpot()
	{
		return GetPlayerBed()?.GetBedSpot() ?? getEntryLocation();
	}

	public BedFurniture GetPlayerBed()
	{
		if (upgradeLevel == 0)
		{
			return GetBed(BedFurniture.BedType.Single);
		}
		return GetBed(BedFurniture.BedType.Double);
	}

	public Point getBedSpot(BedFurniture.BedType bed_type = BedFurniture.BedType.Any)
	{
		return GetBed(bed_type)?.GetBedSpot() ?? new Point(-1000, -1000);
	}

	public Point getEntryLocation()
	{
		if (TryGetMapPropertyAs("EntryLocation", out Point position, required: false))
		{
			return position;
		}
		switch (upgradeLevel)
		{
		case 0:
			return new Point(3, 11);
		case 1:
			return new Point(9, 11);
		case 2:
		case 3:
			return new Point(27, 30);
		default:
			return new Point(-1000, -1000);
		}
	}

	public BedFurniture GetChildBed(int index)
	{
		return GetBed(BedFurniture.BedType.Child, index);
	}

	public Point GetChildBedSpot(int index)
	{
		return GetChildBed(index)?.GetBedSpot() ?? Point.Zero;
	}

	public override bool isTilePlaceable(Vector2 v, bool itemIsPassable = false)
	{
		if (isTileOnMap(v) && getTileIndexAt((int)v.X, (int)v.Y, "Back") == 0 && getTileSheetIDAt((int)v.X, (int)v.Y, "Back") == "indoor")
		{
			return false;
		}
		return base.isTilePlaceable(v, itemIsPassable);
	}

	public Point getRandomOpenPointInHouse(Random r, int buffer = 0, int tries = 30)
	{
		for (int numTries = 0; numTries < tries; numTries++)
		{
			Point point = new Point(r.Next(map.Layers[0].LayerWidth), r.Next(map.Layers[0].LayerHeight));
			Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(point.X - buffer, point.Y - buffer, 1 + buffer * 2, 1 + buffer * 2);
			bool obstacleFound = false;
			foreach (Point point2 in rect.GetPoints())
			{
				int x = point2.X;
				int y = point2.Y;
				obstacleFound = getTileIndexAt(x, y, "Back") == -1 || !CanItemBePlacedHere(new Vector2(x, y)) || isTileOnWall(x, y);
				if (getTileIndexAt(x, y, "Back") == 0 && getTileSheetIDAt(x, y, "Back") == "indoor")
				{
					obstacleFound = true;
				}
				if (obstacleFound)
				{
					break;
				}
			}
			if (!obstacleFound)
			{
				return point;
			}
		}
		return Point.Zero;
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		switch (getTileIndexAt(tileLocation, "Buildings"))
		{
		case 173:
			fridge.Value.fridge.Value = true;
			fridge.Value.checkForAction(who);
			return true;
		case 2173:
			if (Game1.player.eventsSeen.Contains("463391") && Game1.player.spouse == "Emily" && getTemporarySpriteByID(5858585) is EmilysParrot parrot)
			{
				parrot.doAction();
			}
			return true;
		default:
			return base.checkAction(tileLocation, viewport, who);
		}
	}

	public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
	{
		base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
		if (!HasOwner || !Game1.IsMasterGame)
		{
			return;
		}
		foreach (NPC spouse in characters)
		{
			if (spouse.getSpouse()?.UniqueMultiplayerID != OwnerId || Game1.timeOfDay >= 1500 || !(Game1.random.NextDouble() < 0.0006) || spouse.controller != null || spouse.Schedule != null || !(spouse.TilePoint != getSpouseBedSpot(Game1.player.spouse)) || furniture.Count <= 0)
			{
				continue;
			}
			Furniture f = furniture[Game1.random.Next(furniture.Count)];
			Microsoft.Xna.Framework.Rectangle b = f.boundingBox.Value;
			Vector2 possibleLocation = new Vector2(b.X / 64, b.Y / 64);
			if (f.furniture_type.Value == 15 || f.furniture_type.Value == 12)
			{
				continue;
			}
			int tries = 0;
			int facingDirection = -3;
			for (; tries < 3; tries++)
			{
				int xMove = Game1.random.Next(-1, 2);
				int yMove = Game1.random.Next(-1, 2);
				possibleLocation.X += xMove;
				if (xMove == 0)
				{
					possibleLocation.Y += yMove;
				}
				switch (xMove)
				{
				case -1:
					facingDirection = 1;
					break;
				case 1:
					facingDirection = 3;
					break;
				default:
					switch (yMove)
					{
					case -1:
						facingDirection = 2;
						break;
					case 1:
						facingDirection = 0;
						break;
					}
					break;
				}
				if (CanItemBePlacedHere(possibleLocation))
				{
					break;
				}
			}
			if (tries < 3)
			{
				spouse.controller = new PathFindController(spouse, this, new Point((int)possibleLocation.X, (int)possibleLocation.Y), facingDirection, clearMarriageDialogues: false);
			}
		}
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		if (wasUpdated)
		{
			return;
		}
		base.UpdateWhenCurrentLocation(time);
		fridge.Value.updateWhenCurrentLocation(time);
		if (!Game1.player.isMarriedOrRoommates() || Game1.player.spouse == null)
		{
			return;
		}
		NPC spouse = getCharacterFromName(Game1.player.spouse);
		if (spouse == null || spouse.isEmoting)
		{
			return;
		}
		Vector2 spousePos = spouse.Tile;
		Vector2[] adjacentTilesOffsets = Character.AdjacentTilesOffsets;
		foreach (Vector2 offset in adjacentTilesOffsets)
		{
			Vector2 v = spousePos + offset;
			if (isCharacterAtTile(v) is Monster monster)
			{
				Microsoft.Xna.Framework.Rectangle monsterBounds = monster.GetBoundingBox();
				Point centerPixel = monsterBounds.Center;
				spouse.faceGeneralDirection(v * new Vector2(64f, 64f));
				Game1.showSwordswipeAnimation(spouse.FacingDirection, spouse.Position, 60f, flip: false);
				localSound("swordswipe");
				spouse.shake(500);
				spouse.showTextAboveHead(Game1.content.LoadString("Strings\\Locations:FarmHouse_SpouseAttacked" + (Game1.random.Next(12) + 1)));
				monster.takeDamage(50, (int)Utility.getAwayFromPositionTrajectory(monsterBounds, spouse.Position).X, (int)Utility.getAwayFromPositionTrajectory(monsterBounds, spouse.Position).Y, isBomb: false, 1.0, Game1.player);
				if (monster.Health <= 0)
				{
					debris.Add(new Debris(monster.Sprite.textureName, Game1.random.Next(6, 16), Utility.PointToVector2(centerPixel)));
					monsterDrop(monster, centerPixel.X, centerPixel.Y, owner);
					characters.Remove(monster);
					Game1.stats.MonstersKilled++;
					Game1.player.changeFriendship(-10, spouse);
				}
				else
				{
					monster.shedChunks(4);
				}
				spouse.CurrentDialogue.Clear();
				spouse.CurrentDialogue.Push(spouse.TryGetDialogue("Spouse_MonstersInHouse") ?? new Dialogue(spouse, "Data\\ExtraDialogue:Spouse_MonstersInHouse"));
			}
		}
	}

	public Point getFireplacePoint()
	{
		switch (upgradeLevel)
		{
		case 0:
			return new Point(8, 4);
		case 1:
			return new Point(26, 4);
		case 2:
		case 3:
			return new Point(17, 23);
		default:
			return new Point(-50, -50);
		}
	}

	/// <summary>Get whether the player who owns this home is married to or roommates with an NPC.</summary>
	public bool HasNpcSpouseOrRoommate()
	{
		if (owner?.spouse != null)
		{
			return owner.isMarriedOrRoommates();
		}
		return false;
	}

	/// <summary>Get whether the player who owns this home is married to or roommates with the given NPC.</summary>
	/// <param name="spouseName">The NPC name.</param>
	public bool HasNpcSpouseOrRoommate(string spouseName)
	{
		if (spouseName != null && owner?.spouse == spouseName)
		{
			return owner.isMarriedOrRoommates();
		}
		return false;
	}

	public virtual void showSpouseRoom()
	{
		bool showSpouse = HasNpcSpouseOrRoommate();
		bool num = displayingSpouseRoom;
		displayingSpouseRoom = showSpouse;
		updateMap();
		if (num && !displayingSpouseRoom)
		{
			Point corner = GetSpouseRoomCorner();
			Microsoft.Xna.Framework.Rectangle sourceArea = CharacterSpouseRoomData.DefaultMapSourceRect;
			if (NPC.TryGetData(owner.spouse, out var spouseData))
			{
				sourceArea = spouseData.SpouseRoom?.MapSourceRect ?? sourceArea;
			}
			Microsoft.Xna.Framework.Rectangle spouseRoomBounds = new Microsoft.Xna.Framework.Rectangle(corner.X, corner.Y, sourceArea.Width, sourceArea.Height);
			spouseRoomBounds.X--;
			List<Item> collected_items = new List<Item>();
			Microsoft.Xna.Framework.Rectangle room_bounds = new Microsoft.Xna.Framework.Rectangle(spouseRoomBounds.X * 64, spouseRoomBounds.Y * 64, spouseRoomBounds.Width * 64, spouseRoomBounds.Height * 64);
			foreach (Furniture placed_furniture in new List<Furniture>(furniture))
			{
				if (placed_furniture.GetBoundingBox().Intersects(room_bounds))
				{
					if (placed_furniture is StorageFurniture storage_furniture)
					{
						collected_items.AddRange(storage_furniture.heldItems);
						storage_furniture.heldItems.Clear();
					}
					if (placed_furniture.heldObject.Value != null)
					{
						collected_items.Add(placed_furniture.heldObject.Value);
						placed_furniture.heldObject.Value = null;
					}
					collected_items.Add(placed_furniture);
					furniture.Remove(placed_furniture);
				}
			}
			for (int x = spouseRoomBounds.X; x <= spouseRoomBounds.Right; x++)
			{
				for (int y = spouseRoomBounds.Y; y <= spouseRoomBounds.Bottom; y++)
				{
					Object tile_object = getObjectAtTile(x, y);
					if (tile_object == null || tile_object is Furniture)
					{
						continue;
					}
					tile_object.performRemoveAction();
					if (!(tile_object is Fence fence))
					{
						if (!(tile_object is IndoorPot garden_pot))
						{
							if (tile_object is Chest chest)
							{
								collected_items.AddRange(chest.Items);
								chest.Items.Clear();
							}
						}
						else if (garden_pot.hoeDirt.Value?.crop != null)
						{
							garden_pot.hoeDirt.Value.destroyCrop(showAnimation: false);
						}
					}
					else
					{
						tile_object = new Object(fence.ItemId, 1);
					}
					tile_object.heldObject.Value = null;
					tile_object.minutesUntilReady.Value = -1;
					tile_object.readyForHarvest.Value = false;
					collected_items.Add(tile_object);
					objects.Remove(new Vector2(x, y));
				}
			}
			if (upgradeLevel >= 2)
			{
				Utility.createOverflowChest(this, new Vector2(39f, 32f), collected_items);
			}
			else
			{
				Utility.createOverflowChest(this, new Vector2(21f, 10f), collected_items);
			}
		}
		loadObjects();
		if (upgradeLevel == 3)
		{
			AddCellarTiles();
			createCellarWarps();
			Game1.player.craftingRecipes.TryAdd("Cask", 0);
		}
		if (showSpouse)
		{
			loadSpouseRoom();
		}
		lastSpouseRoom = owner?.spouse;
	}

	public virtual void AddCellarTiles()
	{
		if (_appliedMapOverrides.Contains("cellar"))
		{
			_appliedMapOverrides.Remove("cellar");
		}
		ApplyMapOverride("FarmHouse_Cellar", "cellar");
	}

	/// <summary>Get the cellar location linked to this cabin, or <c>null</c> if there is none.</summary>
	public Cellar GetCellar()
	{
		string cellarName = GetCellarName();
		if (cellarName == null)
		{
			return null;
		}
		return Game1.RequireLocation<Cellar>(cellarName);
	}

	/// <summary>Get the name of the cellar location linked to this cabin, or <c>null</c> if there is none.</summary>
	public string GetCellarName()
	{
		int cellar_number = -1;
		if (HasOwner)
		{
			foreach (int i in Game1.player.team.cellarAssignments.Keys)
			{
				if (Game1.player.team.cellarAssignments[i] == OwnerId)
				{
					cellar_number = i;
				}
			}
		}
		switch (cellar_number)
		{
		case 0:
		case 1:
			return "Cellar";
		case -1:
			return null;
		default:
			return "Cellar" + cellar_number;
		}
	}

	protected override void resetSharedState()
	{
		base.resetSharedState();
		if (HasOwner)
		{
			if (Game1.timeOfDay >= 2200 && owner.spouse != null && getCharacterFromName(owner.spouse) != null && !owner.isEngaged())
			{
				Game1.player.team.requestSpouseSleepEvent.Fire(owner.UniqueMultiplayerID);
			}
			if (Game1.timeOfDay >= 2000 && IsOwnedByCurrentPlayer && Game1.getFarm().farmers.Count <= 1)
			{
				Game1.player.team.requestPetWarpHomeEvent.Fire(owner.UniqueMultiplayerID);
			}
		}
		if (!Game1.IsMasterGame)
		{
			return;
		}
		Farm farm = Game1.getFarm();
		for (int i = characters.Count - 1; i >= 0; i--)
		{
			if (characters[i] is Pet { TilePoint: var tile } pet)
			{
				Microsoft.Xna.Framework.Rectangle bounds = pet.GetBoundingBox();
				if (!isTileOnMap(tile.X, tile.Y) || getTileIndexAt(bounds.Left / 64, tile.Y, "Buildings") != -1 || getTileIndexAt(bounds.Right / 64, tile.Y, "Buildings") != -1)
				{
					pet.WarpToPetBowl();
					break;
				}
			}
		}
		for (int i = characters.Count - 1; i >= 0; i--)
		{
			for (int j = i - 1; j >= 0; j--)
			{
				if (i < characters.Count && j < characters.Count && (characters[j].Equals(characters[i]) || (characters[j].Name.Equals(characters[i].Name) && characters[j].IsVillager && characters[i].IsVillager)) && j != i)
				{
					characters.RemoveAt(j);
				}
			}
			for (int j = farm.characters.Count - 1; j >= 0; j--)
			{
				if (i < characters.Count && j < characters.Count && farm.characters[j].Equals(characters[i]))
				{
					farm.characters.RemoveAt(j);
				}
			}
		}
	}

	public void UpdateForRenovation()
	{
		updateFarmLayout();
		setWallpapers();
		setFloors();
	}

	public void updateFarmLayout()
	{
		if (currentlyDisplayedUpgradeLevel != upgradeLevel)
		{
			setMapForUpgradeLevel(upgradeLevel);
		}
		_ApplyRenovations();
		if (displayingSpouseRoom != HasNpcSpouseOrRoommate() || lastSpouseRoom != owner?.spouse)
		{
			showSpouseRoom();
		}
		UpdateChildRoom();
		ReadWallpaperAndFloorTileData();
	}

	protected virtual void _ApplyRenovations()
	{
		bool hasOwner = HasOwner;
		if (upgradeLevel >= 2)
		{
			if (_appliedMapOverrides.Contains("bedroom_open"))
			{
				_appliedMapOverrides.Remove("bedroom_open");
			}
			if (hasOwner && owner.mailReceived.Contains("renovation_bedroom_open"))
			{
				ApplyMapOverride("FarmHouse_Bedroom_Open", "bedroom_open");
			}
			else
			{
				ApplyMapOverride("FarmHouse_Bedroom_Normal", "bedroom_open");
			}
			if (_appliedMapOverrides.Contains("southernroom_open"))
			{
				_appliedMapOverrides.Remove("southernroom_open");
			}
			if (hasOwner && owner.mailReceived.Contains("renovation_southern_open"))
			{
				ApplyMapOverride("FarmHouse_SouthernRoom_Add", "southernroom_open");
			}
			else
			{
				ApplyMapOverride("FarmHouse_SouthernRoom_Remove", "southernroom_open");
			}
			if (_appliedMapOverrides.Contains("cornerroom_open"))
			{
				_appliedMapOverrides.Remove("cornerroom_open");
			}
			if (hasOwner && owner.mailReceived.Contains("renovation_corner_open"))
			{
				ApplyMapOverride("FarmHouse_CornerRoom_Add", "cornerroom_open");
				if (displayingSpouseRoom)
				{
					setMapTile(49, 19, 229, "Front", null, 2);
				}
			}
			else
			{
				ApplyMapOverride("FarmHouse_CornerRoom_Remove", "cornerroom_open");
				if (displayingSpouseRoom)
				{
					setMapTile(49, 19, 87, "Front", null, 2);
				}
			}
			if (_appliedMapOverrides.Contains("diningroom_open"))
			{
				_appliedMapOverrides.Remove("diningroom_open");
			}
			if (hasOwner && owner.mailReceived.Contains("renovation_dining_open"))
			{
				ApplyMapOverride("FarmHouse_DiningRoom_Add", "diningroom_open");
			}
			else
			{
				ApplyMapOverride("FarmHouse_DiningRoom_Remove", "diningroom_open");
			}
			if (_appliedMapOverrides.Contains("cubby_open"))
			{
				_appliedMapOverrides.Remove("cubby_open");
			}
			if (hasOwner && owner.mailReceived.Contains("renovation_cubby_open"))
			{
				ApplyMapOverride("FarmHouse_Cubby_Add", "cubby_open");
			}
			else
			{
				ApplyMapOverride("FarmHouse_Cubby_Remove", "cubby_open");
			}
			if (_appliedMapOverrides.Contains("farupperroom_open"))
			{
				_appliedMapOverrides.Remove("farupperroom_open");
			}
			if (hasOwner && owner.mailReceived.Contains("renovation_farupperroom_open"))
			{
				ApplyMapOverride("FarmHouse_FarUpperRoom_Add", "farupperroom_open");
			}
			else
			{
				ApplyMapOverride("FarmHouse_FarUpperRoom_Remove", "farupperroom_open");
			}
			if (_appliedMapOverrides.Contains("extendedcorner_open"))
			{
				_appliedMapOverrides.Remove("extendedcorner_open");
			}
			if (hasOwner && owner.mailReceived.Contains("renovation_extendedcorner_open"))
			{
				ApplyMapOverride("FarmHouse_ExtendedCornerRoom_Add", "extendedcorner_open");
			}
			else if (hasOwner && owner.mailReceived.Contains("renovation_corner_open"))
			{
				ApplyMapOverride("FarmHouse_ExtendedCornerRoom_Remove", "extendedcorner_open");
			}
			if (_appliedMapOverrides.Contains("diningroomwall_open"))
			{
				_appliedMapOverrides.Remove("diningroomwall_open");
			}
			if (hasOwner && owner.mailReceived.Contains("renovation_diningroomwall_open"))
			{
				ApplyMapOverride("FarmHouse_DiningRoomWall_Add", "diningroomwall_open");
			}
			else if (hasOwner && owner.mailReceived.Contains("renovation_dining_open"))
			{
				ApplyMapOverride("FarmHouse_DiningRoomWall_Remove", "diningroomwall_open");
			}
		}
		if (!TryGetMapProperty("AdditionalRenovations", out var propertyValue))
		{
			return;
		}
		string[] array = propertyValue.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string[] data_split = ArgUtility.SplitBySpace(array[i]);
			if (data_split.Length < 4)
			{
				continue;
			}
			string map_patch_id = data_split[0];
			string required_mail = data_split[1];
			string add_map_override = data_split[2];
			string remove_map_override = data_split[3];
			Microsoft.Xna.Framework.Rectangle? destination_rect = null;
			if (data_split.Length >= 8)
			{
				try
				{
					Microsoft.Xna.Framework.Rectangle rectangle = default(Microsoft.Xna.Framework.Rectangle);
					rectangle.X = int.Parse(data_split[4]);
					rectangle.Y = int.Parse(data_split[5]);
					rectangle.Width = int.Parse(data_split[6]);
					rectangle.Height = int.Parse(data_split[7]);
					destination_rect = rectangle;
				}
				catch (Exception)
				{
					destination_rect = null;
				}
			}
			if (_appliedMapOverrides.Contains(map_patch_id))
			{
				_appliedMapOverrides.Remove(map_patch_id);
			}
			if (hasOwner && owner.mailReceived.Contains(required_mail))
			{
				ApplyMapOverride(add_map_override, map_patch_id, null, destination_rect);
			}
			else
			{
				ApplyMapOverride(remove_map_override, map_patch_id, null, destination_rect);
			}
		}
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		updateFarmLayout();
		setWallpapers();
		setFloors();
		if (HasNpcSpouseOrRoommate("Sebastian") && Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
		{
			Point frog_spot = GetSpouseRoomCorner();
			frog_spot.X++;
			frog_spot.Y += 6;
			Vector2 spot = Utility.PointToVector2(frog_spot);
			removeTile((int)spot.X, (int)spot.Y - 1, "Front");
			removeTile((int)spot.X + 1, (int)spot.Y - 1, "Front");
			removeTile((int)spot.X + 2, (int)spot.Y - 1, "Front");
		}
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (HasNpcSpouseOrRoommate("Emily") && Game1.player.eventsSeen.Contains("463391"))
		{
			Vector2 parrotSpot = new Vector2(2064f, 160f);
			int num = upgradeLevel;
			if ((uint)(num - 2) <= 1u)
			{
				parrotSpot = new Vector2(3408f, 1376f);
			}
			temporarySprites.Add(new EmilysParrot(parrotSpot));
		}
		if (Game1.player.currentLocation == null || (!Game1.player.currentLocation.Equals(this) && !Game1.player.currentLocation.name.Value.StartsWith("Cellar")))
		{
			Game1.player.Position = Utility.PointToVector2(getEntryLocation()) * 64f;
			Game1.xLocationAfterWarp = Game1.player.TilePoint.X;
			Game1.yLocationAfterWarp = Game1.player.TilePoint.Y;
			Game1.player.currentLocation = this;
		}
		foreach (NPC n in characters)
		{
			if (n is Child child)
			{
				child.resetForPlayerEntry(this);
			}
			if (Game1.IsMasterGame && Game1.timeOfDay >= 2000 && !(n is Pet))
			{
				n.controller = null;
				n.Halt();
			}
		}
		if (IsOwnedByCurrentPlayer && Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).HasValue && Game1.player.team.IsMarried(Game1.player.UniqueMultiplayerID) && !Game1.player.mailReceived.Contains("CF_Spouse"))
		{
			Vector2 chestPosition = Utility.PointToVector2(getEntryLocation()) + new Vector2(0f, -1f);
			Chest chest = new Chest(new List<Item> { ItemRegistry.Create("(O)434") }, chestPosition, giftbox: true, 1);
			overlayObjects[chestPosition] = chest;
		}
		if (IsOwnedByCurrentPlayer && !Game1.player.activeDialogueEvents.ContainsKey("pennyRedecorating"))
		{
			int whichQuilt = -1;
			if (Game1.player.mailReceived.Contains("pennyQuilt0"))
			{
				whichQuilt = 0;
			}
			else if (Game1.player.mailReceived.Contains("pennyQuilt1"))
			{
				whichQuilt = 1;
			}
			else if (Game1.player.mailReceived.Contains("pennyQuilt2"))
			{
				whichQuilt = 2;
			}
			if (whichQuilt != -1 && !Game1.player.mailReceived.Contains("pennyRefurbished"))
			{
				List<Object> objectsPickedUp = new List<Object>();
				foreach (Furniture item in furniture)
				{
					if (item is BedFurniture { bedType: BedFurniture.BedType.Double } bed_furniture)
					{
						string bedId = null;
						if (owner.mailReceived.Contains("pennyQuilt0"))
						{
							bedId = "2058";
						}
						if (owner.mailReceived.Contains("pennyQuilt1"))
						{
							bedId = "2064";
						}
						if (owner.mailReceived.Contains("pennyQuilt2"))
						{
							bedId = "2070";
						}
						if (bedId != null)
						{
							Vector2 tile_location = bed_furniture.TileLocation;
							bed_furniture.performRemoveAction();
							objectsPickedUp.Add(bed_furniture);
							Guid guid = furniture.GuidOf(bed_furniture);
							furniture.Remove(guid);
							furniture.Add(new BedFurniture(bedId, new Vector2(tile_location.X, tile_location.Y)));
						}
						break;
					}
				}
				Game1.player.mailReceived.Add("pennyRefurbished");
				Microsoft.Xna.Framework.Rectangle roomToRedecorate = ((upgradeLevel >= 2) ? new Microsoft.Xna.Framework.Rectangle(38, 20, 11, 13) : new Microsoft.Xna.Framework.Rectangle(20, 1, 8, 10));
				for (int x = roomToRedecorate.X; x <= roomToRedecorate.Right; x++)
				{
					for (int y = roomToRedecorate.Y; y <= roomToRedecorate.Bottom; y++)
					{
						if (getObjectAtTile(x, y) == null)
						{
							continue;
						}
						Object o = getObjectAtTile(x, y);
						if (o != null && !(o is Chest) && !(o is StorageFurniture) && !(o is IndoorPot) && !(o is BedFurniture))
						{
							if (o.heldObject.Value != null && ((o as Furniture)?.IsTable() ?? false))
							{
								Object held_object = o.heldObject.Value;
								o.heldObject.Value = null;
								objectsPickedUp.Add(held_object);
							}
							o.performRemoveAction();
							if (o is Fence fence)
							{
								o = new Object(fence.ItemId, 1);
							}
							objectsPickedUp.Add(o);
							objects.Remove(new Vector2(x, y));
							if (o is Furniture curFurniture)
							{
								furniture.Remove(curFurniture);
							}
						}
					}
				}
				decoratePennyRoom(whichQuilt, objectsPickedUp);
			}
		}
		if (!HasNpcSpouseOrRoommate("Sebastian") || !Game1.netWorldState.Value.hasWorldStateID("sebastianFrog"))
		{
			return;
		}
		Point frog_spot = GetSpouseRoomCorner();
		frog_spot.X++;
		frog_spot.Y += 6;
		Vector2 spot = Utility.PointToVector2(frog_spot);
		temporarySprites.Add(new TemporaryAnimatedSprite
		{
			texture = Game1.mouseCursors,
			sourceRect = new Microsoft.Xna.Framework.Rectangle(641, 1534, 48, 37),
			animationLength = 1,
			sourceRectStartingPos = new Vector2(641f, 1534f),
			interval = 5000f,
			totalNumberOfLoops = 9999,
			position = spot * 64f + new Vector2(0f, -5f) * 4f,
			scale = 4f,
			layerDepth = (spot.Y + 2f + 0.1f) * 64f / 10000f
		});
		if (Game1.random.NextDouble() < 0.85)
		{
			Texture2D crittersText2 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
			base.TemporarySprites.Add(new SebsFrogs
			{
				texture = crittersText2,
				sourceRect = new Microsoft.Xna.Framework.Rectangle(64, 224, 16, 16),
				animationLength = 1,
				sourceRectStartingPos = new Vector2(64f, 224f),
				interval = 100f,
				totalNumberOfLoops = 9999,
				position = spot * 64f + new Vector2(Game1.random.Choose(22, 25), Game1.random.Choose(2, 1)) * 4f,
				scale = 4f,
				flipped = Game1.random.NextBool(),
				layerDepth = (spot.Y + 2f + 0.11f) * 64f / 10000f,
				Parent = this
			});
		}
		if (!Game1.player.activeDialogueEvents.ContainsKey("sebastianFrog2") && Game1.random.NextBool())
		{
			Texture2D crittersText2 = Game1.temporaryContent.Load<Texture2D>("TileSheets\\critters");
			base.TemporarySprites.Add(new SebsFrogs
			{
				texture = crittersText2,
				sourceRect = new Microsoft.Xna.Framework.Rectangle(64, 240, 16, 16),
				animationLength = 1,
				sourceRectStartingPos = new Vector2(64f, 240f),
				interval = 150f,
				totalNumberOfLoops = 9999,
				position = spot * 64f + new Vector2(8f, 3f) * 4f,
				scale = 4f,
				layerDepth = (spot.Y + 2f + 0.11f) * 64f / 10000f,
				flipped = Game1.random.NextBool(),
				pingPong = false,
				Parent = this
			});
			if (Game1.random.NextDouble() < 0.1 && Game1.timeOfDay > 610)
			{
				DelayedAction.playSoundAfterDelay("croak", 1000);
			}
		}
	}

	private void addFurnitureIfSpaceIsFreePenny(List<Object> objectsToStoreInChests, Furniture f, Furniture heldObject = null)
	{
		bool fail = false;
		foreach (Furniture furniture in base.furniture)
		{
			if (f.GetBoundingBox().Intersects(furniture.GetBoundingBox()))
			{
				fail = true;
				break;
			}
		}
		if (objects.ContainsKey(f.TileLocation))
		{
			fail = true;
		}
		if (!fail)
		{
			base.furniture.Add(f);
			if (heldObject != null)
			{
				f.heldObject.Value = heldObject;
			}
		}
		else
		{
			objectsToStoreInChests.Add(f);
			if (heldObject != null)
			{
				objectsToStoreInChests.Add(heldObject);
			}
		}
	}

	private void decoratePennyRoom(int whichStyle, List<Object> objectsToStoreInChests)
	{
		List<Chest> chests = new List<Chest>();
		List<Vector2> chest_positions = new List<Vector2>();
		Color chest_color = default(Color);
		switch (whichStyle)
		{
		case 0:
			if (upgradeLevel == 1)
			{
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916").SetPlacement(20, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1914").SetPlacement(21, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1915").SetPlacement(22, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1914").SetPlacement(23, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916").SetPlacement(24, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1682").SetPlacement(26, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1747").SetPlacement(25, 4));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1395").SetPlacement(26, 4), ItemRegistry.Create<Furniture>("(F)1363"));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1443").SetPlacement(27, 4));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1664").SetPlacement(27, 5, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1978").SetPlacement(21, 6));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1124").SetPlacement(26, 9), ItemRegistry.Create<Furniture>("(F)1368"));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)6").SetPlacement(25, 10, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1296").SetPlacement(28, 10));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1747").SetPlacement(24, 10));
				SetWallpaper("107", "Bedroom");
				SetFloor("2", "Bedroom");
				chest_color = new Color(85, 85, 255);
				chest_positions.Add(new Vector2(21f, 10f));
				chest_positions.Add(new Vector2(22f, 10f));
			}
			else
			{
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916").SetPlacement(38, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1914").SetPlacement(39, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1604").SetPlacement(41, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1915").SetPlacement(43, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916").SetPlacement(45, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1914").SetPlacement(47, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916").SetPlacement(48, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1443").SetPlacement(38, 23));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1747").SetPlacement(39, 23));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1395").SetPlacement(40, 23), ItemRegistry.Create<Furniture>("(F)1363"));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)714").SetPlacement(46, 23));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1443").SetPlacement(48, 23));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1978").SetPlacement(42, 25));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1664").SetPlacement(47, 25, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1664").SetPlacement(38, 27, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1124").SetPlacement(46, 31), ItemRegistry.Create<Furniture>("(F)1368"));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)416").SetPlacement(40, 32, 2));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1296").SetPlacement(38, 32));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)6").SetPlacement(45, 32, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1296").SetPlacement(48, 32));
				SetWallpaper("107", "Bedroom");
				SetFloor("2", "Bedroom");
				chest_color = new Color(85, 85, 255);
				chest_positions.Add(new Vector2(38f, 24f));
				chest_positions.Add(new Vector2(39f, 24f));
			}
			break;
		case 1:
			if (upgradeLevel == 1)
			{
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1678").SetPlacement(20, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814").SetPlacement(21, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814").SetPlacement(22, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814").SetPlacement(23, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1907").SetPlacement(24, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1400").SetPlacement(25, 4), ItemRegistry.Create<Furniture>("(F)1365"));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1866").SetPlacement(26, 4));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1909").SetPlacement(27, 6, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1451").SetPlacement(21, 6));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1138").SetPlacement(27, 9), ItemRegistry.Create<Furniture>("(F)1378"));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)12").SetPlacement(26, 10, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1758").SetPlacement(24, 10));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1618").SetPlacement(21, 9));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1390").SetPlacement(22, 10));
				SetWallpaper("84", "Bedroom");
				SetFloor("35", "Bedroom");
				chest_color = new Color(255, 85, 85);
				chest_positions.Add(new Vector2(21f, 10f));
				chest_positions.Add(new Vector2(23f, 10f));
			}
			else
			{
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1678").SetPlacement(39, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1907").SetPlacement(40, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814").SetPlacement(42, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814").SetPlacement(43, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1814").SetPlacement(44, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1907").SetPlacement(45, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1916").SetPlacement(48, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1758").SetPlacement(38, 23));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1400").SetPlacement(40, 23), ItemRegistry.Create<Furniture>("(F)1365"));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1390").SetPlacement(46, 23));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1866").SetPlacement(47, 23));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1387").SetPlacement(38, 24));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1909").SetPlacement(47, 24, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)719").SetPlacement(38, 25, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1451").SetPlacement(42, 25));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1909").SetPlacement(38, 27, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1389").SetPlacement(47, 29));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1377").SetPlacement(48, 29));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1758").SetPlacement(41, 30));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)424").SetPlacement(42, 30, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1618").SetPlacement(44, 30));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)536").SetPlacement(47, 30, 3));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1138").SetPlacement(38, 31), ItemRegistry.Create<Furniture>("(F)1378"));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1383").SetPlacement(41, 31));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1449").SetPlacement(48, 32));
				SetWallpaper("84", "Bedroom");
				SetFloor("35", "Bedroom");
				chest_color = new Color(255, 85, 85);
				chest_positions.Add(new Vector2(39f, 23f));
				chest_positions.Add(new Vector2(43f, 25f));
			}
			break;
		case 2:
			if (upgradeLevel == 1)
			{
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1673").SetPlacement(20, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1547").SetPlacement(21, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1675").SetPlacement(24, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1900").SetPlacement(25, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1393").SetPlacement(25, 4), ItemRegistry.Create<Furniture>("(F)1367"));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1798").SetPlacement(26, 4));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1902").SetPlacement(25, 5));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1751").SetPlacement(22, 6));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1122").SetPlacement(26, 9), ItemRegistry.Create<Furniture>("(F)1378"));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)197").SetPlacement(28, 9, 3));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)3").SetPlacement(25, 10, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1294").SetPlacement(20, 10));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1294").SetPlacement(24, 10));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1964").SetPlacement(21, 8));
				SetWallpaper("95", "Bedroom");
				SetFloor("1", "Bedroom");
				chest_color = new Color(85, 85, 85);
				chest_positions.Add(new Vector2(22f, 10f));
				chest_positions.Add(new Vector2(23f, 10f));
			}
			else
			{
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1673").SetPlacement(38, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1675").SetPlacement(40, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1547").SetPlacement(42, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1900").SetPlacement(45, 20));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1751").SetPlacement(38, 23));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1393").SetPlacement(40, 23), ItemRegistry.Create<Furniture>("(F)1367"));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1798").SetPlacement(47, 23));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1902").SetPlacement(46, 24));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1964").SetPlacement(42, 25));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1294").SetPlacement(38, 26));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)3").SetPlacement(46, 29));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1294").SetPlacement(38, 30));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)1122").SetPlacement(46, 30), ItemRegistry.Create<Furniture>("(F)1369"));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)197").SetPlacement(48, 30, 3));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)709").SetPlacement(38, 31, 1));
				addFurnitureIfSpaceIsFreePenny(objectsToStoreInChests, ItemRegistry.Create<Furniture>("(F)3").SetPlacement(47, 32, 2));
				SetWallpaper("95", "Bedroom");
				SetFloor("1", "Bedroom");
				chest_color = new Color(85, 85, 85);
				chest_positions.Add(new Vector2(39f, 23f));
				chest_positions.Add(new Vector2(46f, 23f));
			}
			break;
		}
		if (objectsToStoreInChests != null)
		{
			foreach (Object o in objectsToStoreInChests)
			{
				if (chests.Count == 0)
				{
					chests.Add(new Chest(playerChest: true));
				}
				bool found_chest_to_stash_in = false;
				foreach (Chest item in chests)
				{
					if (item.addItem(o) == null)
					{
						found_chest_to_stash_in = true;
					}
				}
				if (!found_chest_to_stash_in)
				{
					Chest new_chest = new Chest(playerChest: true);
					chests.Add(new_chest);
					new_chest.addItem(o);
				}
			}
		}
		for (int i = 0; i < chests.Count; i++)
		{
			Chest chest = chests[i];
			chest.playerChoiceColor.Value = chest_color;
			Vector2 chest_position = chest_positions[Math.Min(i, chest_positions.Count - 1)];
			PlaceInNearbySpace(chest_position, chest);
		}
	}

	public void PlaceInNearbySpace(Vector2 tileLocation, Object o)
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
			if (CanItemBePlacedHere(current))
			{
				break;
			}
			closed_list.Add(current);
			foreach (Vector2 v in Utility.getAdjacentTileLocations(current))
			{
				if (!closed_list.Contains(v))
				{
					open_list.Enqueue(v);
				}
			}
		}
		if (!current.Equals(Vector2.Zero) && CanItemBePlacedHere(current))
		{
			o.TileLocation = current;
			objects.Add(current, o);
		}
	}

	public virtual void RefreshFloorObjectNeighbors()
	{
		foreach (Vector2 key in terrainFeatures.Keys)
		{
			if (terrainFeatures[key] is Flooring flooring)
			{
				flooring.OnAdded(this, key);
			}
		}
	}

	public void moveObjectsForHouseUpgrade(int whichUpgrade)
	{
		previousUpgradeLevel = upgradeLevel;
		overlayObjects.Clear();
		switch (whichUpgrade)
		{
		case 0:
			if (upgradeLevel == 1)
			{
				shiftContents(-6, 0);
			}
			break;
		case 1:
			switch (upgradeLevel)
			{
			case 0:
				shiftContents(6, 0);
				break;
			case 2:
				shiftContents(-3, 0);
				break;
			}
			break;
		case 2:
		case 3:
			switch (upgradeLevel)
			{
			case 1:
				shiftContents(18, 19);
				foreach (Furniture v in furniture)
				{
					if (v.tileLocation.X >= 25f && v.tileLocation.X <= 28f && v.tileLocation.Y >= 20f && v.tileLocation.Y <= 21f)
					{
						v.TileLocation = new Vector2(v.tileLocation.X - 3f, v.tileLocation.Y - 9f);
					}
				}
				moveFurniture(42, 23, 16, 14);
				moveFurniture(43, 23, 17, 14);
				moveFurniture(44, 23, 18, 14);
				moveFurniture(43, 24, 22, 14);
				moveFurniture(44, 24, 23, 14);
				moveFurniture(42, 24, 19, 14);
				moveFurniture(43, 25, 20, 14);
				moveFurniture(44, 26, 21, 14);
				break;
			case 0:
				shiftContents(24, 19);
				break;
			}
			break;
		}
	}

	protected override LocalizedContentManager getMapLoader()
	{
		if (mapLoader == null)
		{
			mapLoader = Game1.game1.xTileContent.CreateTemporary();
		}
		return mapLoader;
	}

	protected override void _updateAmbientLighting()
	{
		if (Game1.isStartingToGetDarkOut(this) || lightLevel.Value > 0f)
		{
			int time = Game1.timeOfDay + Game1.gameTimeInterval / (Game1.realMilliSecondsPerGameMinute + base.ExtraMillisecondsPerInGameMinute);
			float lerp = 1f - Utility.Clamp((float)Utility.CalculateMinutesBetweenTimes(time, Game1.getTrulyDarkTime(this)) / 120f, 0f, 1f);
			Game1.ambientLight = new Color((byte)Utility.Lerp(Game1.isRaining ? rainLightingColor.R : 0, (int)nightLightingColor.R, lerp), (byte)Utility.Lerp(Game1.isRaining ? rainLightingColor.G : 0, (int)nightLightingColor.G, lerp), (byte)Utility.Lerp(0f, (int)nightLightingColor.B, lerp));
		}
		else
		{
			Game1.ambientLight = (Game1.isRaining ? rainLightingColor : Color.White);
		}
	}

	public override void drawAboveFrontLayer(SpriteBatch b)
	{
		base.drawAboveFrontLayer(b);
		if (fridge.Value.mutex.IsLocked())
		{
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(fridgePosition.X, fridgePosition.Y - 1) * 64f), new Microsoft.Xna.Framework.Rectangle(0, 192, 16, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((fridgePosition.Y + 1) * 64 + 1) / 10000f);
		}
	}

	public override void updateMap()
	{
		bool showSpouse = HasNpcSpouseOrRoommate();
		mapPath.Value = "Maps\\FarmHouse" + ((upgradeLevel == 0) ? "" : ((upgradeLevel == 3) ? "2" : (upgradeLevel.ToString() ?? ""))) + (showSpouse ? "_marriage" : "");
		base.updateMap();
	}

	public virtual void setMapForUpgradeLevel(int level)
	{
		upgradeLevel = level;
		int previous_synchronized_displayed_level = synchronizedDisplayedLevel.Value;
		currentlyDisplayedUpgradeLevel = level;
		synchronizedDisplayedLevel.Value = level;
		bool showSpouse = HasNpcSpouseOrRoommate();
		if (displayingSpouseRoom && !showSpouse)
		{
			displayingSpouseRoom = false;
		}
		updateMap();
		RefreshFloorObjectNeighbors();
		if (showSpouse)
		{
			showSpouseRoom();
		}
		loadObjects();
		if (level == 3)
		{
			AddCellarTiles();
			createCellarWarps();
			if (!Game1.player.craftingRecipes.ContainsKey("Cask"))
			{
				Game1.player.craftingRecipes.Add("Cask", 0);
			}
		}
		bool need_bed_upgrade = false;
		if (previousUpgradeLevel == 0 && upgradeLevel >= 0)
		{
			need_bed_upgrade = true;
		}
		if (previousUpgradeLevel >= 0)
		{
			if (previousUpgradeLevel < 2 && upgradeLevel >= 2)
			{
				for (int x = 0; x < map.Layers[0].TileWidth; x++)
				{
					for (int y = 0; y < map.Layers[0].TileHeight; y++)
					{
						if (doesTileHaveProperty(x, y, "DefaultChildBedPosition", "Back") != null)
						{
							string bedId = BedFurniture.CHILD_BED_INDEX;
							furniture.Add(new BedFurniture(bedId, new Vector2(x, y)));
							break;
						}
					}
				}
			}
			Furniture bed_furniture = null;
			if (previousUpgradeLevel == 0)
			{
				foreach (Furniture item in furniture)
				{
					if (item is BedFurniture { bedType: BedFurniture.BedType.Single } bed)
					{
						bed_furniture = bed;
						break;
					}
				}
			}
			else
			{
				foreach (Furniture item2 in furniture)
				{
					if (item2 is BedFurniture { bedType: BedFurniture.BedType.Double } bed)
					{
						bed_furniture = bed;
						break;
					}
				}
			}
			if (upgradeLevel != 3 || need_bed_upgrade)
			{
				for (int x = 0; x < map.Layers[0].TileWidth; x++)
				{
					for (int y = 0; y < map.Layers[0].TileHeight; y++)
					{
						if (doesTileHaveProperty(x, y, "DefaultBedPosition", "Back") == null)
						{
							continue;
						}
						string bedId = BedFurniture.DEFAULT_BED_INDEX;
						if (previousUpgradeLevel != 1 || bed_furniture == null || (bed_furniture.tileLocation.X == 39f && bed_furniture.tileLocation.Y == 22f))
						{
							if (bed_furniture != null)
							{
								bedId = bed_furniture.ItemId;
							}
							if (previousUpgradeLevel == 0 && bed_furniture != null)
							{
								bed_furniture.performRemoveAction();
								Guid guid = furniture.GuidOf(bed_furniture);
								furniture.Remove(guid);
								bedId = Utility.GetDoubleWideVersionOfBed(bedId);
								furniture.Add(new BedFurniture(bedId, new Vector2(x, y)));
							}
							else if (bed_furniture != null)
							{
								bed_furniture.performRemoveAction();
								Guid guid = furniture.GuidOf(bed_furniture);
								furniture.Remove(guid);
								furniture.Add(new BedFurniture(bed_furniture.ItemId, new Vector2(x, y)));
							}
						}
						break;
					}
				}
			}
			previousUpgradeLevel = -1;
		}
		if (previous_synchronized_displayed_level != level)
		{
			lightGlows.Clear();
		}
		fridgePosition = default(Point);
		bool found_fridge = false;
		for (int x = 0; x < map.RequireLayer("Buildings").LayerWidth; x++)
		{
			for (int y = 0; y < map.RequireLayer("Buildings").LayerHeight; y++)
			{
				if (getTileIndexAt(x, y, "Buildings") == 173)
				{
					fridgePosition = new Point(x, y);
					found_fridge = true;
					break;
				}
			}
			if (found_fridge)
			{
				break;
			}
		}
	}

	public void createCellarWarps()
	{
		updateCellarWarps();
	}

	public void updateCellarWarps()
	{
		Layer back_layer = map.RequireLayer("Back");
		string cellarName = GetCellarName();
		if (cellarName == null)
		{
			return;
		}
		for (int x = 0; x < back_layer.LayerWidth; x++)
		{
			for (int y = 0; y < back_layer.LayerHeight; y++)
			{
				string[] touchAction = GetTilePropertySplitBySpaces("TouchAction", "Back", x, y);
				if (ArgUtility.Get(touchAction, 0) == "Warp" && ArgUtility.Get(touchAction, 1, "").StartsWith("Cellar"))
				{
					touchAction[1] = cellarName;
					setTileProperty(x, y, "Back", "TouchAction", string.Join(" ", touchAction));
				}
			}
		}
		if (cellarWarps == null)
		{
			return;
		}
		foreach (Warp warp in cellarWarps)
		{
			if (!warps.Contains(warp))
			{
				warps.Add(warp);
			}
			warp.TargetName = cellarName;
		}
	}

	public virtual Point GetSpouseRoomCorner()
	{
		if (TryGetMapPropertyAs("SpouseRoomPosition", out Point position, required: false))
		{
			return position;
		}
		if (upgradeLevel != 1)
		{
			return new Point(50, 20);
		}
		return new Point(29, 1);
	}

	public virtual void loadSpouseRoom()
	{
		string obj = ((owner?.spouse != null && owner.isMarriedOrRoommates()) ? owner.spouse : null);
		CharacterData spouseData;
		CharacterSpouseRoomData roomData = ((!NPC.TryGetData(obj, out spouseData)) ? null : spouseData?.SpouseRoom);
		spouseRoomSpot = GetSpouseRoomCorner();
		spouseRoomSpot.X += 3;
		spouseRoomSpot.Y += 4;
		if (obj == null)
		{
			return;
		}
		string assetName = roomData?.MapAsset ?? "spouseRooms";
		Microsoft.Xna.Framework.Rectangle sourceArea = roomData?.MapSourceRect ?? CharacterSpouseRoomData.DefaultMapSourceRect;
		Point corner = GetSpouseRoomCorner();
		Microsoft.Xna.Framework.Rectangle areaToRefurbish = new Microsoft.Xna.Framework.Rectangle(corner.X, corner.Y, sourceArea.Width, sourceArea.Height);
		Map refurbishedMap = Game1.game1.xTileContent.Load<Map>("Maps\\" + assetName);
		Point fromOrigin = sourceArea.Location;
		map.Properties.Remove("Light");
		map.Properties.Remove("DayTiles");
		map.Properties.Remove("NightTiles");
		List<KeyValuePair<Point, Tile>> bottom_row_tiles = new List<KeyValuePair<Point, Tile>>();
		Layer front_layer = map.RequireLayer("Front");
		for (int x = areaToRefurbish.Left; x < areaToRefurbish.Right; x++)
		{
			Point point = new Point(x, areaToRefurbish.Bottom - 1);
			Tile tile = front_layer.Tiles[point.X, point.Y];
			if (tile != null)
			{
				bottom_row_tiles.Add(new KeyValuePair<Point, Tile>(point, tile));
			}
		}
		if (_appliedMapOverrides.Contains("spouse_room"))
		{
			_appliedMapOverrides.Remove("spouse_room");
		}
		ApplyMapOverride(assetName, "spouse_room", new Microsoft.Xna.Framework.Rectangle(fromOrigin.X, fromOrigin.Y, areaToRefurbish.Width, areaToRefurbish.Height), areaToRefurbish);
		Layer refurbishedBuildingsLayer = refurbishedMap.RequireLayer("Buildings");
		Layer refurbishedFrontLayer = refurbishedMap.RequireLayer("Front");
		for (int x = 0; x < areaToRefurbish.Width; x++)
		{
			for (int y = 0; y < areaToRefurbish.Height; y++)
			{
				int tileIndex = refurbishedBuildingsLayer.GetTileIndexAt(fromOrigin.X + x, fromOrigin.Y + y);
				if (tileIndex != -1)
				{
					adjustMapLightPropertiesForLamp(tileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Buildings");
				}
				if (y < areaToRefurbish.Height - 1)
				{
					tileIndex = refurbishedFrontLayer.GetTileIndexAt(fromOrigin.X + x, fromOrigin.Y + y);
					if (tileIndex != -1)
					{
						adjustMapLightPropertiesForLamp(tileIndex, areaToRefurbish.X + x, areaToRefurbish.Y + y, "Front");
					}
				}
			}
		}
		foreach (Point tile in areaToRefurbish.GetPoints())
		{
			if (getTileIndexAt(tile, "Paths") == 7)
			{
				spouseRoomSpot = tile;
				break;
			}
		}
		Point spouse_room_spot = GetSpouseRoomSpot();
		setTileProperty(spouse_room_spot.X, spouse_room_spot.Y, "Back", "NoFurniture", "T");
		foreach (KeyValuePair<Point, Tile> kvp in bottom_row_tiles)
		{
			front_layer.Tiles[kvp.Key.X, kvp.Key.Y] = kvp.Value;
		}
	}

	public virtual Microsoft.Xna.Framework.Rectangle? GetCribBounds()
	{
		if (upgradeLevel < 2)
		{
			return null;
		}
		return new Microsoft.Xna.Framework.Rectangle(30, 12, 3, 4);
	}

	public virtual void UpdateChildRoom()
	{
		Microsoft.Xna.Framework.Rectangle? crib_location = GetCribBounds();
		if (crib_location.HasValue)
		{
			if (_appliedMapOverrides.Contains("crib"))
			{
				_appliedMapOverrides.Remove("crib");
			}
			ApplyMapOverride("FarmHouse_Crib_" + cribStyle.Value, "crib", null, crib_location);
		}
	}

	public void playerDivorced()
	{
		displayingSpouseRoom = false;
	}

	public virtual List<Microsoft.Xna.Framework.Rectangle> getForbiddenPetWarpTiles()
	{
		List<Microsoft.Xna.Framework.Rectangle> forbidden_tiles = new List<Microsoft.Xna.Framework.Rectangle>();
		switch (upgradeLevel)
		{
		case 0:
			forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(2, 8, 3, 4));
			break;
		case 1:
			forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(8, 8, 3, 4));
			forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(17, 8, 4, 3));
			break;
		case 2:
		case 3:
			forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(26, 27, 3, 4));
			forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(35, 27, 4, 3));
			forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(27, 15, 4, 3));
			forbidden_tiles.Add(new Microsoft.Xna.Framework.Rectangle(26, 17, 2, 6));
			break;
		}
		return forbidden_tiles;
	}

	public bool canPetWarpHere(Vector2 tile_position)
	{
		foreach (Microsoft.Xna.Framework.Rectangle forbiddenPetWarpTile in getForbiddenPetWarpTiles())
		{
			if (forbiddenPetWarpTile.Contains((int)tile_position.X, (int)tile_position.Y))
			{
				return false;
			}
		}
		return true;
	}

	public override List<Microsoft.Xna.Framework.Rectangle> getWalls()
	{
		List<Microsoft.Xna.Framework.Rectangle> walls = new List<Microsoft.Xna.Framework.Rectangle>();
		switch (upgradeLevel)
		{
		case 0:
			walls.Add(new Microsoft.Xna.Framework.Rectangle(1, 1, 10, 3));
			break;
		case 1:
			walls.Add(new Microsoft.Xna.Framework.Rectangle(1, 1, 17, 3));
			walls.Add(new Microsoft.Xna.Framework.Rectangle(18, 6, 2, 2));
			walls.Add(new Microsoft.Xna.Framework.Rectangle(20, 1, 9, 3));
			break;
		case 2:
		case 3:
		{
			bool hasOwner = HasOwner;
			walls.Add(new Microsoft.Xna.Framework.Rectangle(1, 1, 12, 3));
			walls.Add(new Microsoft.Xna.Framework.Rectangle(15, 1, 13, 3));
			walls.Add(new Microsoft.Xna.Framework.Rectangle(13, 3, 2, 2));
			walls.Add(new Microsoft.Xna.Framework.Rectangle(1, 10, 10, 3));
			walls.Add(new Microsoft.Xna.Framework.Rectangle(13, 10, 8, 3));
			int bedroomWidthReduction = ((hasOwner && owner.hasOrWillReceiveMail("renovation_corner_open")) ? (-3) : 0);
			if (hasOwner && owner.hasOrWillReceiveMail("renovation_bedroom_open"))
			{
				walls.Add(new Microsoft.Xna.Framework.Rectangle(21, 15, 0, 2));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(21, 10, 13 + bedroomWidthReduction, 3));
			}
			else
			{
				walls.Add(new Microsoft.Xna.Framework.Rectangle(21, 15, 2, 2));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(23, 10, 11 + bedroomWidthReduction, 3));
			}
			if (hasOwner && owner.hasOrWillReceiveMail("renovation_southern_open"))
			{
				walls.Add(new Microsoft.Xna.Framework.Rectangle(23, 24, 3, 3));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(31, 24, 3, 3));
			}
			else
			{
				walls.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
			}
			if (hasOwner && owner.hasOrWillReceiveMail("renovation_corner_open"))
			{
				walls.Add(new Microsoft.Xna.Framework.Rectangle(30, 1, 9, 3));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(28, 3, 2, 2));
			}
			else
			{
				walls.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
				walls.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
			}
			foreach (Microsoft.Xna.Framework.Rectangle item in walls)
			{
				item.Offset(15, 10);
			}
			break;
		}
		}
		return walls;
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		if (l is FarmHouse farmhouse)
		{
			cribStyle.Value = farmhouse.cribStyle.Value;
		}
		base.TransferDataFromSavedLocation(l);
	}

	public override List<Microsoft.Xna.Framework.Rectangle> getFloors()
	{
		List<Microsoft.Xna.Framework.Rectangle> floors = new List<Microsoft.Xna.Framework.Rectangle>();
		switch (upgradeLevel)
		{
		case 0:
			floors.Add(new Microsoft.Xna.Framework.Rectangle(1, 3, 10, 9));
			break;
		case 1:
			floors.Add(new Microsoft.Xna.Framework.Rectangle(1, 3, 6, 9));
			floors.Add(new Microsoft.Xna.Framework.Rectangle(7, 3, 11, 9));
			floors.Add(new Microsoft.Xna.Framework.Rectangle(18, 8, 2, 2));
			floors.Add(new Microsoft.Xna.Framework.Rectangle(20, 3, 9, 8));
			break;
		case 2:
		case 3:
		{
			bool hasOwner = HasOwner;
			floors.Add(new Microsoft.Xna.Framework.Rectangle(1, 3, 12, 6));
			floors.Add(new Microsoft.Xna.Framework.Rectangle(15, 3, 13, 6));
			floors.Add(new Microsoft.Xna.Framework.Rectangle(13, 5, 2, 2));
			floors.Add(new Microsoft.Xna.Framework.Rectangle(0, 12, 10, 11));
			floors.Add(new Microsoft.Xna.Framework.Rectangle(10, 12, 11, 9));
			if (hasOwner && owner.mailReceived.Contains("renovation_bedroom_open"))
			{
				floors.Add(new Microsoft.Xna.Framework.Rectangle(21, 17, 0, 2));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(21, 12, 14, 11));
			}
			else
			{
				floors.Add(new Microsoft.Xna.Framework.Rectangle(21, 17, 2, 2));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(23, 12, 12, 11));
			}
			if (hasOwner && owner.hasOrWillReceiveMail("renovation_southern_open"))
			{
				floors.Add(new Microsoft.Xna.Framework.Rectangle(23, 26, 11, 8));
			}
			else
			{
				floors.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
			}
			if (hasOwner && owner.hasOrWillReceiveMail("renovation_corner_open"))
			{
				floors.Add(new Microsoft.Xna.Framework.Rectangle(28, 5, 2, 3));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(30, 3, 9, 6));
			}
			else
			{
				floors.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
				floors.Add(new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
			}
			foreach (Microsoft.Xna.Framework.Rectangle item in floors)
			{
				item.Offset(15, 10);
			}
			break;
		}
		}
		return floors;
	}

	public virtual bool CanModifyCrib()
	{
		if (!HasOwner)
		{
			return false;
		}
		if (owner.isMarriedOrRoommates() && owner.GetSpouseFriendship().DaysUntilBirthing != -1)
		{
			return false;
		}
		foreach (Child child in owner.getChildren())
		{
			if (child.Age < 3)
			{
				return false;
			}
		}
		return true;
	}
}
