using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Network;

namespace StardewValley.Objects;

public class BedFurniture : Furniture
{
	public enum BedType
	{
		Any = -1,
		Single,
		Double,
		Child
	}

	public static string DEFAULT_BED_INDEX = "2048";

	public static string DOUBLE_BED_INDEX = "2052";

	public static string CHILD_BED_INDEX = "2076";

	[XmlIgnore]
	public int bedTileOffset;

	[XmlIgnore]
	protected bool _alreadyAttempingRemoval;

	[XmlIgnore]
	public static bool ignoreContextualBedSpotOffset = false;

	[XmlIgnore]
	protected NetEnum<BedType> _bedType = new NetEnum<BedType>(BedType.Any);

	[XmlIgnore]
	public NetMutex mutex = new NetMutex();

	[XmlElement("bedType")]
	public BedType bedType
	{
		get
		{
			if (_bedType.Value == BedType.Any)
			{
				BedType bed_type = BedType.Single;
				string[] data = getData();
				if (data != null && data.Length > 1)
				{
					string[] tokens = ArgUtility.SplitBySpace(data[1]);
					if (tokens.Length > 1)
					{
						string text = tokens[1];
						if (!(text == "double"))
						{
							if (text == "child")
							{
								bed_type = BedType.Child;
							}
						}
						else
						{
							bed_type = BedType.Double;
						}
					}
				}
				_bedType.Value = bed_type;
			}
			return _bedType.Value;
		}
		set
		{
			_bedType.Value = value;
		}
	}

	public BedFurniture()
	{
	}

	public BedFurniture(string itemId, Vector2 tile, int initialRotations)
		: base(itemId, tile, initialRotations)
	{
	}

	public BedFurniture(string itemId, Vector2 tile)
		: base(itemId, tile)
	{
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(_bedType, "_bedType").AddField(mutex.NetFields, "mutex.NetFields");
	}

	public virtual bool IsBeingSleptIn()
	{
		GameLocation location = Location;
		if (location == null)
		{
			return false;
		}
		if (mutex.IsLocked())
		{
			return true;
		}
		Rectangle bedBounds = GetBoundingBox();
		foreach (Farmer farmer in location.farmers)
		{
			if (farmer.GetBoundingBox().Intersects(bedBounds))
			{
				return true;
			}
		}
		return false;
	}

	public override void DayUpdate()
	{
		base.DayUpdate();
		mutex.ReleaseLock();
	}

	public virtual void ReserveForNPC()
	{
		mutex.RequestLock();
	}

	public override void AttemptRemoval(Action<Furniture> removal_action)
	{
		if (_alreadyAttempingRemoval)
		{
			_alreadyAttempingRemoval = false;
			return;
		}
		_alreadyAttempingRemoval = true;
		mutex.RequestLock(delegate
		{
			_alreadyAttempingRemoval = false;
			if (removal_action != null)
			{
				removal_action(this);
				mutex.ReleaseLock();
			}
		}, delegate
		{
			_alreadyAttempingRemoval = false;
		});
	}

	public static BedFurniture GetBedAtTile(GameLocation location, int x, int y)
	{
		if (location == null)
		{
			return null;
		}
		foreach (Furniture furniture in location.furniture)
		{
			if (Utility.doesRectangleIntersectTile(furniture.GetBoundingBox(), x, y) && furniture is BedFurniture bedFurniture)
			{
				return bedFurniture;
			}
		}
		return null;
	}

	public static void ApplyWakeUpPosition(Farmer who)
	{
		GameLocation lastSleepLocation = ((who.lastSleepLocation.Value != null && Game1.isLocationAccessible(who.lastSleepLocation)) ? Game1.getLocationFromName(who.lastSleepLocation) : null);
		bool allowWakeUpWithoutBed;
		if (who.disconnectDay.Value == Game1.MasterPlayer.stats.DaysPlayed && !Game1.newDaySync.hasInstance())
		{
			who.currentLocation = Game1.getLocationFromName(who.disconnectLocation);
			who.Position = who.disconnectPosition.Value;
		}
		else if (lastSleepLocation != null && (IsBedHere(Game1.getLocationFromName(who.lastSleepLocation), who.lastSleepPoint.Value.X, who.lastSleepPoint.Value.Y) || who.sleptInTemporaryBed.Value || lastSleepLocation is IslandFarmHouse || (lastSleepLocation.TryGetMapPropertyAs("AllowWakeUpWithoutBed", out allowWakeUpWithoutBed, required: false) && allowWakeUpWithoutBed)))
		{
			who.Position = Utility.PointToVector2(who.lastSleepPoint.Value) * 64f;
			who.currentLocation = lastSleepLocation;
			ShiftPositionForBed(who);
		}
		else
		{
			if (lastSleepLocation != null)
			{
				Game1.log.Verbose("Can't wake up in last sleep position '" + lastSleepLocation.NameOrUniqueName + "' because it has no bed and doesn't have the 'AllowWakeUpWithoutBed: true' map property set.");
			}
			FarmHouse home = (FarmHouse)(who.currentLocation = Game1.RequireLocation<FarmHouse>(who.homeLocation.Value));
			who.Position = Utility.PointToVector2(home.GetPlayerBedSpot()) * 64f;
			ShiftPositionForBed(who);
		}
		if (who == Game1.player)
		{
			Game1.currentLocation = who.currentLocation;
		}
	}

	public static void ShiftPositionForBed(Farmer who)
	{
		GameLocation location = who.currentLocation;
		BedFurniture bed = GetBedAtTile(location, (int)(who.position.X / 64f), (int)(who.position.Y / 64f));
		if (bed != null)
		{
			who.Position = Utility.PointToVector2(bed.GetBedSpot()) * 64f;
			if (bed.bedType != BedType.Double)
			{
				if (location.map == null)
				{
					location.reloadMap();
				}
				if (!location.CanItemBePlacedHere(new Vector2(bed.TileLocation.X - 1f, bed.TileLocation.Y + 1f)))
				{
					who.faceDirection(3);
				}
				else
				{
					who.position.X -= 64f;
					who.faceDirection(1);
				}
			}
			else
			{
				bool should_wake_up_in_spouse_spot = false;
				if (location is FarmHouse { HasOwner: not false } farmhouse)
				{
					if (farmhouse.owner.team.GetSpouse(farmhouse.owner.UniqueMultiplayerID) == who.UniqueMultiplayerID)
					{
						should_wake_up_in_spouse_spot = true;
					}
					else if (farmhouse.owner != who && !farmhouse.owner.isMarriedOrRoommates())
					{
						should_wake_up_in_spouse_spot = true;
					}
				}
				if (should_wake_up_in_spouse_spot)
				{
					who.position.X += 64f;
					who.faceDirection(3);
				}
				else
				{
					who.position.X -= 64f;
					who.faceDirection(1);
				}
			}
		}
		who.position.Y += 32f;
		(who.NetFields.Root as NetRoot<Farmer>)?.CancelInterpolation();
	}

	public virtual bool CanModifyBed(Farmer who)
	{
		if (who == null)
		{
			return false;
		}
		GameLocation location = who.currentLocation;
		if (location == null)
		{
			return false;
		}
		if (location is FarmHouse farmhouse && farmhouse.owner != who && farmhouse.owner.team.GetSpouse(farmhouse.owner.UniqueMultiplayerID) != who.UniqueMultiplayerID)
		{
			return false;
		}
		return true;
	}

	public override int GetAdditionalFurniturePlacementStatus(GameLocation location, int x, int y, Farmer who = null)
	{
		if (bedType == BedType.Double)
		{
			if (!IsBedsideClear(-1))
			{
				return -1;
			}
		}
		else if (!IsBedsideClear(-1) && !IsBedsideClear(getTilesWide()))
		{
			return -1;
		}
		return base.GetAdditionalFurniturePlacementStatus(location, x, y, who);
		bool IsBedsideClear(int offsetX)
		{
			Vector2 tile = new Vector2(x / 64 + offsetX, y / 64 + 1);
			return location.CanItemBePlacedHere(tile, itemIsPassable: false, CollisionMask.All, ~CollisionMask.Objects, useFarmerTile: false, ignorePassablesExactly: true);
		}
	}

	/// <inheritdoc />
	public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
	{
		_alreadyAttempingRemoval = false;
		Location = location;
		if (!CanModifyBed(who))
		{
			Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Bed_CantMoveOthersBeds"));
			return false;
		}
		if (location is FarmHouse farmhouse && ((bedType == BedType.Child && farmhouse.upgradeLevel < 2) || (bedType == BedType.Double && farmhouse.upgradeLevel < 1)))
		{
			Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Bed_NeedsUpgrade"));
			return false;
		}
		return base.placementAction(location, x, y, who);
	}

	public override void performRemoveAction()
	{
		_alreadyAttempingRemoval = false;
		base.performRemoveAction();
	}

	public override void hoverAction()
	{
		if (!Game1.player.GetBoundingBox().Intersects(GetBoundingBox()))
		{
			base.hoverAction();
		}
	}

	public override bool canBeRemoved(Farmer who)
	{
		if (Location == null)
		{
			return false;
		}
		if (!CanModifyBed(who))
		{
			if (!Game1.player.GetBoundingBox().Intersects(GetBoundingBox()))
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Bed_CantMoveOthersBeds"));
			}
			return false;
		}
		if (IsBeingSleptIn())
		{
			if (!Game1.player.GetBoundingBox().Intersects(GetBoundingBox()))
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\UI:Bed_InUse"));
			}
			return false;
		}
		return true;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new BedFurniture(base.ItemId, tileLocation.Value);
	}

	/// <inheritdoc />
	protected override void GetOneCopyFrom(Item source)
	{
		base.GetOneCopyFrom(source);
		if (source is BedFurniture fromBed)
		{
			bedType = fromBed.bedType;
		}
	}

	public virtual Point GetBedSpot()
	{
		return new Point((int)tileLocation.X + 1, (int)tileLocation.Y + 1);
	}

	/// <inheritdoc />
	public override void actionOnPlayerEntryOrPlacement(GameLocation environment, bool dropDown)
	{
		base.actionOnPlayerEntryOrPlacement(environment, dropDown);
		UpdateBedTile(check_bounds: false);
	}

	public virtual void UpdateBedTile(bool check_bounds)
	{
		Rectangle bounding_box = GetBoundingBox();
		if (bedType == BedType.Double)
		{
			bedTileOffset = 1;
		}
		else if (!check_bounds || !bounding_box.Intersects(Game1.player.GetBoundingBox()))
		{
			if (Game1.player.Position.X > (float)bounding_box.Center.X)
			{
				bedTileOffset = 0;
			}
			else
			{
				bedTileOffset = 1;
			}
		}
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		if (Location != null)
		{
			mutex.Update(Game1.getOnlineFarmers());
			UpdateBedTile(check_bounds: true);
		}
		base.updateWhenCurrentLocation(time);
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		if (!isTemporarilyInvisible)
		{
			if (Furniture.isDrawingLocationFurniture)
			{
				ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
				Texture2D texture = dataOrErrorItem.GetTexture();
				Rectangle drawSourceRect = dataOrErrorItem.GetSourceRect();
				spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, drawPosition.Value + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), drawSourceRect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(boundingBox.Value.Top + 1) / 10000f);
				drawSourceRect.X += drawSourceRect.Width;
				spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, drawPosition.Value + ((shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero)), drawSourceRect, Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(boundingBox.Value.Bottom - 1) / 10000f);
			}
			else
			{
				base.draw(spriteBatch, x, y, alpha);
			}
		}
	}

	public override bool AllowPlacementOnThisTile(int x, int y)
	{
		if (bedType == BedType.Child && (float)y == TileLocation.Y + 1f)
		{
			return true;
		}
		return base.AllowPlacementOnThisTile(x, y);
	}

	public override bool IntersectsForCollision(Rectangle rect)
	{
		Rectangle bounds = GetBoundingBox();
		Rectangle current_rect = bounds;
		current_rect.Height = 64;
		if (current_rect.Intersects(rect))
		{
			return true;
		}
		current_rect = bounds;
		current_rect.Y += 128;
		current_rect.Height -= 128;
		if (current_rect.Intersects(rect))
		{
			return true;
		}
		return false;
	}

	public override int GetAdditionalTilePropertyRadius()
	{
		return 1;
	}

	public static bool IsBedHere(GameLocation location, int x, int y)
	{
		if (location == null)
		{
			return false;
		}
		ignoreContextualBedSpotOffset = true;
		if (location.doesTileHaveProperty(x, y, "Bed", "Back") != null)
		{
			ignoreContextualBedSpotOffset = false;
			return true;
		}
		ignoreContextualBedSpotOffset = false;
		return false;
	}

	public override bool DoesTileHaveProperty(int tile_x, int tile_y, string property_name, string layer_name, ref string property_value)
	{
		if (bedType == BedType.Double && (float)tile_x == tileLocation.X - 1f && (float)tile_y == tileLocation.Y + 1f && layer_name == "Back" && property_name == "NoFurniture")
		{
			property_value = "T";
			return true;
		}
		if ((float)tile_x >= tileLocation.X && (float)tile_x < tileLocation.X + (float)getTilesWide() && (float)tile_y == tileLocation.Y + 1f && layer_name == "Back")
		{
			if (property_name == "Bed")
			{
				property_value = "T";
				return true;
			}
			if (bedType != BedType.Child)
			{
				int bed_spot_x = (int)tileLocation.X + bedTileOffset;
				if (ignoreContextualBedSpotOffset)
				{
					bed_spot_x = (int)tileLocation.X + 1;
				}
				if (tile_x == bed_spot_x && property_name == "TouchAction")
				{
					property_value = "Sleep";
					return true;
				}
			}
		}
		return false;
	}
}
