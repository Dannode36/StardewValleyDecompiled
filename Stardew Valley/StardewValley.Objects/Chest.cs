using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Delegates;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Network.ChestHit;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.Objects;

public class Chest : Object
{
	public enum SpecialChestTypes
	{
		None,
		MiniShippingBin,
		JunimoChest,
		AutoLoader,
		Enricher,
		BigChest
	}

	public const int capacity = 36;

	/// <summary>The underlying <see cref="T:StardewValley.Network.ChestHit.ChestHitTimer" /> instance used by <see cref="P:StardewValley.Objects.Chest.HitTimerInstance" />.</summary>
	internal ChestHitTimer hitTimerInstance;

	[XmlElement("currentLidFrame")]
	public readonly NetInt startingLidFrame = new NetInt(501);

	public readonly NetInt lidFrameCount = new NetInt(5);

	private int currentLidFrame;

	[XmlElement("frameCounter")]
	public readonly NetInt frameCounter = new NetInt(-1);

	/// <summary>The backing field for <see cref="P:StardewValley.Objects.Chest.Items" />.</summary>
	[XmlElement("items")]
	public NetRef<Inventory> netItems = new NetRef<Inventory>(new Inventory());

	public readonly NetLongDictionary<Inventory, NetRef<Inventory>> separateWalletItems = new NetLongDictionary<Inventory, NetRef<Inventory>>();

	[XmlElement("tint")]
	public readonly NetColor tint = new NetColor(Color.White);

	[XmlElement("playerChoiceColor")]
	public readonly NetColor playerChoiceColor = new NetColor(Color.Black);

	[XmlElement("playerChest")]
	public readonly NetBool playerChest = new NetBool();

	[XmlElement("fridge")]
	public readonly NetBool fridge = new NetBool();

	/// <summary>Whether this is a gift box. This changes the chest's appearance, and when the player interacts with the chest they'll receive all the items directly and the chest will disappear.</summary>
	[XmlElement("giftbox")]
	public readonly NetBool giftbox = new NetBool();

	/// <summary>If <see cref="F:StardewValley.Objects.Chest.giftbox" /> is true, the sprite index to draw from the <see cref="F:StardewValley.Game1.giftboxName" /> texture.</summary>
	[XmlElement("giftboxIndex")]
	public readonly NetInt giftboxIndex = new NetInt();

	/// <summary>If <see cref="F:StardewValley.Objects.Chest.giftbox" /> is true, whether this is the starter gift for a player in their cabin or farmhouse.</summary>
	public readonly NetBool giftboxIsStarterGift = new NetBool();

	[XmlElement("spriteIndexOverride")]
	public readonly NetInt bigCraftableSpriteIndex = new NetInt(-1);

	[XmlElement("dropContents")]
	public readonly NetBool dropContents = new NetBool(value: false);

	[XmlIgnore]
	public string mailToAddOnItemDump;

	[XmlElement("synchronized")]
	public readonly NetBool synchronized = new NetBool(value: false);

	[XmlIgnore]
	public int _shippingBinFrameCounter;

	[XmlIgnore]
	public bool _farmerNearby;

	[XmlIgnore]
	public NetVector2 kickStartTile = new NetVector2(new Vector2(-1000f, -1000f));

	[XmlIgnore]
	public Vector2? localKickStartTile;

	[XmlIgnore]
	public float kickProgress = -1f;

	[XmlIgnore]
	public readonly NetEvent0 openChestEvent = new NetEvent0();

	[XmlElement("specialChestType")]
	public readonly NetEnum<SpecialChestTypes> specialChestType = new NetEnum<SpecialChestTypes>();

	/// <summary>The backing field for <see cref="P:StardewValley.Objects.Chest.GlobalInventoryId" />.</summary>
	public readonly NetString globalInventoryId = new NetString();

	[XmlIgnore]
	public readonly NetMutex mutex = new NetMutex();

	/// <summary>A read-only <see cref="T:StardewValley.Network.ChestHit.ChestHitTimer" /> that is automatically created or fetched from <see cref="F:StardewValley.Network.ChestHit.ChestHitSynchronizer.SavedTimers" />.</summary>
	private ChestHitTimer HitTimerInstance
	{
		get
		{
			if (hitTimerInstance != null)
			{
				return hitTimerInstance;
			}
			hitTimerInstance = new ChestHitTimer();
			if (Game1.IsMasterGame || Location == null)
			{
				return hitTimerInstance;
			}
			if (!Game1.player.team.chestHit.SavedTimers.TryGetValue(Location.NameOrUniqueName, out var localTimers))
			{
				return hitTimerInstance;
			}
			ulong tileHash = ChestHitSynchronizer.HashPosition((int)TileLocation.X, (int)TileLocation.Y);
			if (localTimers.TryGetValue(tileHash, out var timer))
			{
				hitTimerInstance = timer;
				localTimers.Remove(tileHash);
				if (timer.SavedTime >= 0 && Game1.currentGameTime != null)
				{
					timer.Milliseconds -= (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds - timer.SavedTime;
					timer.SavedTime = -1;
				}
			}
			return hitTimerInstance;
		}
	}

	[XmlIgnore]
	public SpecialChestTypes SpecialChestType
	{
		get
		{
			return specialChestType.Value;
		}
		set
		{
			specialChestType.Value = value;
		}
	}

	/// <summary>If set, the inventory ID in <see cref="F:StardewValley.FarmerTeam.globalInventories" /> to use for this chest instead of its local item list.</summary>
	[XmlIgnore]
	public string GlobalInventoryId
	{
		get
		{
			return globalInventoryId.Value;
		}
		set
		{
			globalInventoryId.Value = value;
		}
	}

	[XmlIgnore]
	public Color Tint
	{
		get
		{
			return tint.Value;
		}
		set
		{
			tint.Value = value;
		}
	}

	[XmlIgnore]
	public Inventory Items => netItems.Value;

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(startingLidFrame, "startingLidFrame").AddField(frameCounter, "frameCounter").AddField(netItems, "netItems")
			.AddField(tint, "tint")
			.AddField(playerChoiceColor, "playerChoiceColor")
			.AddField(playerChest, "playerChest")
			.AddField(fridge, "fridge")
			.AddField(giftbox, "giftbox")
			.AddField(giftboxIndex, "giftboxIndex")
			.AddField(giftboxIsStarterGift, "giftboxIsStarterGift")
			.AddField(mutex.NetFields, "mutex.NetFields")
			.AddField(lidFrameCount, "lidFrameCount")
			.AddField(bigCraftableSpriteIndex, "bigCraftableSpriteIndex")
			.AddField(dropContents, "dropContents")
			.AddField(openChestEvent.NetFields, "openChestEvent.NetFields")
			.AddField(synchronized, "synchronized")
			.AddField(specialChestType, "specialChestType")
			.AddField(kickStartTile, "kickStartTile")
			.AddField(separateWalletItems, "separateWalletItems")
			.AddField(globalInventoryId, "globalInventoryId");
		openChestEvent.onEvent += performOpenChest;
		kickStartTile.fieldChangeVisibleEvent += delegate(NetVector2 field, Vector2 old_value, Vector2 new_value)
		{
			if (Game1.gameMode != 6 && new_value.X != -1000f && new_value.Y != -1000f)
			{
				localKickStartTile = kickStartTile.Value;
				kickProgress = 0f;
			}
		};
	}

	public Chest()
	{
		Name = "Chest";
		type.Value = "interactive";
	}

	public Chest(bool playerChest, Vector2 tileLocation, string itemId = "130")
		: base(tileLocation, itemId)
	{
		Name = "Chest";
		type.Value = "Crafting";
		if (playerChest)
		{
			this.playerChest.Value = playerChest;
			startingLidFrame.Value = base.ParentSheetIndex + 1;
			bigCraftable.Value = true;
			canBeSetDown.Value = true;
		}
		else
		{
			lidFrameCount.Value = 3;
		}
		SetSpecialChestType();
	}

	public Chest(bool playerChest, string itemId = "130")
		: base(Vector2.Zero, itemId)
	{
		Name = "Chest";
		type.Value = "Crafting";
		if (playerChest)
		{
			this.playerChest.Value = playerChest;
			startingLidFrame.Value = base.ParentSheetIndex + 1;
			bigCraftable.Value = true;
			canBeSetDown.Value = true;
		}
		else
		{
			lidFrameCount.Value = 3;
		}
	}

	public Chest(string itemId, Vector2 tile_location, int starting_lid_frame, int lid_frame_count)
		: base(tile_location, itemId)
	{
		playerChest.Value = true;
		startingLidFrame.Value = starting_lid_frame;
		lidFrameCount.Value = lid_frame_count;
		bigCraftable.Value = true;
		canBeSetDown.Value = true;
	}

	public Chest(List<Item> items, Vector2 location, bool giftbox = false, int giftboxIndex = 0, bool giftboxIsStarterGift = false)
	{
		base.name = "Chest";
		type.Value = "interactive";
		this.giftbox.Value = giftbox;
		this.giftboxIndex.Value = giftboxIndex;
		this.giftboxIsStarterGift.Value = giftboxIsStarterGift;
		if (!this.giftbox.Value)
		{
			lidFrameCount.Value = 3;
		}
		if (items != null)
		{
			Items.OverwriteWith(items);
		}
		TileLocation = location;
	}

	public void resetLidFrame()
	{
		currentLidFrame = startingLidFrame;
	}

	public void fixLidFrame()
	{
		if (currentLidFrame == 0)
		{
			currentLidFrame = startingLidFrame;
		}
		if (SpecialChestType == SpecialChestTypes.MiniShippingBin)
		{
			return;
		}
		if ((bool)playerChest)
		{
			if (GetMutex().IsLocked() && !GetMutex().IsLockHeld())
			{
				currentLidFrame = getLastLidFrame();
			}
			else if (!GetMutex().IsLocked())
			{
				currentLidFrame = startingLidFrame;
			}
		}
		else if (currentLidFrame == startingLidFrame.Value && GetMutex().IsLocked() && !GetMutex().IsLockHeld())
		{
			currentLidFrame = getLastLidFrame();
		}
	}

	public int getLastLidFrame()
	{
		return startingLidFrame.Value + lidFrameCount.Value - 1;
	}

	/// <summary>Handles a player hitting this chest.</summary>
	/// <param name="args">The arguments for the chest hit event.</param>
	public void HandleChestHit(ChestHitArgs args)
	{
		if (!Game1.IsMasterGame)
		{
			Game1.log.Warn("Attempted to call Chest::HandleChestHit as a farmhand.");
			return;
		}
		if (TileLocation.X == 0f && TileLocation.Y == 0f)
		{
			TileLocation = Utility.PointToVector2(args.ChestTile);
		}
		GetMutex().RequestLock(delegate
		{
			clearNulls();
			if (isEmpty())
			{
				performRemoveAction();
				if (Location.Objects.Remove(Utility.PointToVector2(args.ChestTile)) && base.Type == "Crafting" && (int)fragility != 2)
				{
					Location.debris.Add(new Debris(base.QualifiedItemId, args.ToolPosition, Utility.PointToVector2(args.StandingPixel)));
				}
				Game1.player.team.chestHit.SignalDelete(Location, args.ChestTile.X, args.ChestTile.Y);
			}
			else if (args.ToolCanHit)
			{
				if (args.HoldDownClick || args.RecentlyHit)
				{
					if (kickStartTile.Value == TileLocation)
					{
						kickStartTile.Value = new Vector2(-1000f, -1000f);
					}
					TryMoveToSafePosition(args.Direction);
					Game1.player.team.chestHit.SignalMove(Location, args.ChestTile.X, args.ChestTile.Y, (int)TileLocation.X, (int)TileLocation.Y);
				}
				else
				{
					kickStartTile.Value = TileLocation;
				}
			}
			GetMutex().ReleaseLock();
		});
	}

	public override bool performToolAction(Tool t)
	{
		if (t?.getLastFarmerToUse() != null && t.getLastFarmerToUse() != Game1.player)
		{
			return false;
		}
		if ((bool)playerChest)
		{
			if (t == null)
			{
				return false;
			}
			if (t is MeleeWeapon || !t.isHeavyHitter())
			{
				return false;
			}
			if (base.performToolAction(t))
			{
				GameLocation location = Location;
				Farmer player = t.getLastFarmerToUse();
				if (player != null)
				{
					Vector2 c = TileLocation;
					if (c.X == 0f && c.Y == 0f)
					{
						bool found = false;
						foreach (KeyValuePair<Vector2, Object> pair in location.objects.Pairs)
						{
							if (pair.Value == this)
							{
								c.X = (int)pair.Key.X;
								c.Y = (int)pair.Key.Y;
								found = true;
								break;
							}
						}
						if (!found)
						{
							c = player.GetToolLocation() / 64f;
							c.X = (int)c.X;
							c.Y = (int)c.Y;
						}
					}
					if (!GetMutex().IsLocked())
					{
						ChestHitArgs args = new ChestHitArgs();
						args.Location = location;
						args.ChestTile = new Point((int)TileLocation.X, (int)TileLocation.Y);
						args.ToolPosition = player.GetToolLocation();
						args.StandingPixel = player.StandingPixel;
						args.Direction = player.FacingDirection;
						args.HoldDownClick = t != player.CurrentTool;
						args.ToolCanHit = t.isHeavyHitter() && !(t is MeleeWeapon);
						args.RecentlyHit = HitTimerInstance.Milliseconds > 0;
						if (args.ToolCanHit)
						{
							shakeTimer = 100;
							HitTimerInstance.Milliseconds = 10000;
						}
						if (args.ChestTile.X == 0 && args.ChestTile.Y == 0)
						{
							if (location.getObjectAtTile((int)c.X, (int)c.Y) != this)
							{
								return false;
							}
							args.ChestTile = new Point((int)c.X, (int)c.Y);
						}
						Game1.player.team.chestHit.Sync(args);
					}
				}
			}
			return false;
		}
		if (t is Pickaxe && currentLidFrame == getLastLidFrame() && (int)frameCounter == -1 && isEmpty())
		{
			Location.playSound("woodWhack");
			for (int i = 0; i < 8; i++)
			{
				Game1.multiplayer.broadcastSprites(Location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", (Game1.random.NextDouble() < 0.5) ? new Microsoft.Xna.Framework.Rectangle(598, 1275, 13, 4) : new Microsoft.Xna.Framework.Rectangle(598, 1275, 13, 4), 999f, 1, 0, tileLocation.Value * 64f + new Vector2(32f, 64f), flicker: false, Game1.random.NextDouble() < 0.5, (tileLocation.Y * 64f + 64f) / 10000f, 0.01f, new Color(204, 132, 87), 4f, 0f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 8f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 64f)
				{
					motion = new Vector2((float)Game1.random.Next(-25, 26) / 10f, Game1.random.Next(-11, -8)),
					acceleration = new Vector2(0f, 0.3f)
				});
			}
			Game1.createRadialDebris(Location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 7), resource: false, -1, item: false, new Color(204, 132, 87));
			return true;
		}
		return false;
	}

	/// <summary>Try to shove this chest onto an unoccupied nearby tile.</summary>
	/// <param name="preferDirection">The direction in which to move the chest if possible, matching a constant like <see cref="F:StardewValley.Game1.up" />.</param>
	/// <returns>Returns whether the chest was successfully moved to an unoccupied space.</returns>
	public bool TryMoveToSafePosition(int? preferDirection = null)
	{
		GameLocation location = Location;
		Vector2? prioritizeDirection = preferDirection switch
		{
			1 => new Vector2(1f, 0f), 
			3 => new Vector2(-1f, 0f), 
			0 => new Vector2(0f, -1f), 
			_ => new Vector2(0f, 1f), 
		};
		return TryMoveRecursively(tileLocation.Value, 0, prioritizeDirection);
		bool TryMoveRecursively(Vector2 tile_position, int depth, Vector2? prioritize_direction)
		{
			List<Vector2> offsets = new List<Vector2>();
			offsets.AddRange(new Vector2[4]
			{
				new Vector2(1f, 0f),
				new Vector2(-1f, 0f),
				new Vector2(0f, -1f),
				new Vector2(0f, 1f)
			});
			Utility.Shuffle(Game1.random, offsets);
			if (prioritize_direction.HasValue)
			{
				offsets.Remove(-prioritize_direction.Value);
				offsets.Insert(0, -prioritize_direction.Value);
				offsets.Remove(prioritize_direction.Value);
				offsets.Insert(0, prioritize_direction.Value);
			}
			foreach (Vector2 offset in offsets)
			{
				Vector2 new_position = tile_position + offset;
				if (canBePlacedHere(location, new_position) && location.CanItemBePlacedHere(new_position))
				{
					if (location.objects.ContainsKey(TileLocation) && !location.objects.ContainsKey(new_position))
					{
						location.objects.Remove(TileLocation);
						kickStartTile.Value = TileLocation;
						TileLocation = new_position;
						location.objects[new_position] = this;
					}
					return true;
				}
			}
			Utility.Shuffle(Game1.random, offsets);
			if (prioritize_direction.HasValue)
			{
				offsets.Remove(-prioritize_direction.Value);
				offsets.Insert(0, -prioritize_direction.Value);
				offsets.Remove(prioritize_direction.Value);
				offsets.Insert(0, prioritize_direction.Value);
			}
			if (depth < 3)
			{
				foreach (Vector2 offset in offsets)
				{
					Vector2 new_position = tile_position + offset;
					if (location.isPointPassable(new Location((int)(new_position.X + 0.5f) * 64, (int)(new_position.Y + 0.5f) * 64), Game1.viewport) && TryMoveRecursively(new_position, depth + 1, prioritize_direction))
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	/// <inheritdoc />
	public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
	{
		localKickStartTile = null;
		kickProgress = -1f;
		return base.placementAction(location, x, y, who);
	}

	/// <summary>Set the special chest type based on the chest's item ID.</summary>
	public void SetSpecialChestType()
	{
		switch (base.QualifiedItemId)
		{
		case "(BC)BigChest":
		case "(BC)BigStoneChest":
			SpecialChestType = SpecialChestTypes.BigChest;
			break;
		case "(BC)248":
			SpecialChestType = SpecialChestTypes.MiniShippingBin;
			break;
		case "(BC)256":
			SpecialChestType = SpecialChestTypes.JunimoChest;
			break;
		case "(BC)275":
			SpecialChestType = SpecialChestTypes.AutoLoader;
			break;
		}
	}

	public void destroyAndDropContents(Vector2 pointToDropAt)
	{
		GameLocation location = Location;
		if (location == null)
		{
			return;
		}
		List<Item> item_list = new List<Item>();
		item_list.AddRange(Items);
		if (SpecialChestType == SpecialChestTypes.MiniShippingBin)
		{
			foreach (Inventory separate_wallet_item_list in separateWalletItems.Values)
			{
				item_list.AddRange(separate_wallet_item_list);
			}
		}
		if (item_list.Count > 0)
		{
			location.playSound("throwDownITem");
		}
		foreach (Item item in item_list)
		{
			if (item != null)
			{
				Game1.createItemDebris(item, pointToDropAt, Game1.random.Next(4), location);
			}
		}
		Items.Clear();
		separateWalletItems.Clear();
		clearNulls();
	}

	/// <inheritdoc />
	public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
	{
		if (dropInItem != null && dropInItem.QualifiedItemId != base.QualifiedItemId && dropInItem.HasContextTag("swappable_chest") && HasContextTag("swappable_chest") && Location != null)
		{
			if (!probe)
			{
				if (GetMutex().IsLocked())
				{
					return false;
				}
				GameLocation location = Location;
				Chest otherChest = new Chest(playerChest: true, TileLocation, dropInItem.ItemId);
				location.Objects.Remove(TileLocation);
				otherChest.netItems.Value = netItems.Value;
				location.Objects.Add(TileLocation, otherChest);
				Game1.createMultipleItemDebris(ItemRegistry.Create(base.QualifiedItemId), TileLocation * 64f + new Vector2(32f), -1);
				Location.playSound("axchop");
			}
			return true;
		}
		return base.performObjectDropInAction(dropInItem, probe, who);
	}

	public void dumpContents()
	{
		GameLocation location = Location;
		if (location == null)
		{
			return;
		}
		IInventory items = Items;
		if (synchronized.Value && (GetMutex().IsLocked() || !Game1.IsMasterGame) && !GetMutex().IsLockHeld())
		{
			return;
		}
		if (items.Count > 0 && (GetMutex().IsLockHeld() || !playerChest))
		{
			if (giftbox.Value && giftboxIsStarterGift.Value && location is FarmHouse house)
			{
				if (!house.IsOwnedByCurrentPlayer)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Objects:ParsnipSeedPackage_SomeoneElse"));
					return;
				}
				Game1.player.addQuest((Game1.whichModFarm?.Id == "MeadowlandsFarm") ? "132" : "6");
				Game1.dayTimeMoneyBox.PingQuestLog();
			}
			foreach (Item item in items)
			{
				if (item == null)
				{
					continue;
				}
				item.SetTempData("FromStarterGiftBox", value: true);
				if (item.QualifiedItemId == "(O)434")
				{
					if (Game1.player.mailReceived.Add((location is FarmHouse) ? "CF_Spouse" : "CF_Mines"))
					{
						Game1.player.eatObject(items[0] as Object, overrideFullness: true);
					}
				}
				else if (dropContents.Value)
				{
					Game1.createItemDebris(item, tileLocation.Value * 64f, -1, location);
					if (location is VolcanoDungeon)
					{
						switch (bigCraftableSpriteIndex.Value)
						{
						case 223:
							Game1.player.team.RequestLimitedNutDrops("VolcanoNormalChest", location, (int)tileLocation.Value.X * 64, (int)tileLocation.Value.Y * 64, 1);
							break;
						case 227:
							Game1.player.team.RequestLimitedNutDrops("VolcanoRareChest", location, (int)tileLocation.Value.X * 64, (int)tileLocation.Value.Y * 64, 1);
							break;
						}
					}
				}
				else if (!synchronized.Value || GetMutex().IsLockHeld())
				{
					item.onDetachedFromParent();
					if (Game1.activeClickableMenu is ItemGrabMenu grabMenu)
					{
						grabMenu.ItemsToGrabMenu.actualInventory.Add(item);
					}
					else
					{
						Game1.player.addItemByMenuIfNecessaryElseHoldUp(item);
					}
					if (mailToAddOnItemDump != null)
					{
						Game1.player.mailReceived.Add(mailToAddOnItemDump);
					}
					if (location is Caldera || Game1.player.currentLocation is Caldera)
					{
						Game1.player.mailReceived.Add("CalderaTreasure");
					}
				}
			}
			items.Clear();
			clearNulls();
			Game1.mine?.chestConsumed();
			IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
			ItemGrabMenu grabMenu = activeClickableMenu as ItemGrabMenu;
			if (grabMenu != null)
			{
				ItemGrabMenu itemGrabMenu = grabMenu;
				itemGrabMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(itemGrabMenu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
				{
					grabMenu.DropRemainingItems();
				});
			}
		}
		Game1.player.gainExperience(5, 25 + Game1.CurrentMineLevel);
		if ((bool)giftbox)
		{
			TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite("LooseSprites\\Giftbox", new Microsoft.Xna.Framework.Rectangle(0, (int)giftboxIndex * 32, 16, 32), 80f, 11, 1, tileLocation.Value * 64f - new Vector2(0f, 52f), flicker: false, flipped: false, tileLocation.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f)
			{
				destroyable = false,
				holdLastFrame = true
			};
			if (location.netObjects.ContainsKey(tileLocation.Value) && location.netObjects[tileLocation.Value] == this)
			{
				Game1.multiplayer.broadcastSprites(location, sprite);
				location.removeObject(tileLocation.Value, showDestroyedObject: false);
			}
			else
			{
				location.temporarySprites.Add(sprite);
			}
		}
	}

	public NetMutex GetMutex()
	{
		if (GlobalInventoryId != null)
		{
			return Game1.player.team.GetOrCreateGlobalInventoryMutex(GlobalInventoryId);
		}
		if (specialChestType.Value == SpecialChestTypes.JunimoChest)
		{
			return Game1.player.team.GetOrCreateGlobalInventoryMutex("JunimoChests");
		}
		return mutex;
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		GameLocation location = Location;
		IInventory items = GetItemsForPlayer();
		if ((bool)giftbox)
		{
			Game1.player.Halt();
			Game1.player.freezePause = 1000;
			location.playSound("Ship");
			dumpContents();
		}
		else if ((bool)playerChest)
		{
			if (!Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
			{
				return false;
			}
			GetMutex().RequestLock(delegate
			{
				if (SpecialChestType == SpecialChestTypes.MiniShippingBin)
				{
					OpenMiniShippingMenu();
				}
				else
				{
					frameCounter.Value = 5;
					Game1.playSound(fridge ? "doorCreak" : "openChest");
					Game1.player.Halt();
					Game1.player.freezePause = 1000;
				}
			});
		}
		else if (!playerChest)
		{
			if (currentLidFrame == startingLidFrame.Value && (int)frameCounter <= -1)
			{
				location.playSound("openChest");
				if (synchronized.Value)
				{
					GetMutex().RequestLock(openChestEvent.Fire);
				}
				else
				{
					performOpenChest();
				}
			}
			else if (currentLidFrame == getLastLidFrame() && items.Count > 0 && !synchronized.Value)
			{
				Item item = items[0];
				items.RemoveAt(0);
				if (Game1.mine != null)
				{
					Game1.mine.chestConsumed();
				}
				who.addItemByMenuIfNecessaryElseHoldUp(item);
				IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
				ItemGrabMenu grab_menu = activeClickableMenu as ItemGrabMenu;
				if (grab_menu != null)
				{
					ItemGrabMenu itemGrabMenu = grab_menu;
					itemGrabMenu.behaviorBeforeCleanup = (Action<IClickableMenu>)Delegate.Combine(itemGrabMenu.behaviorBeforeCleanup, (Action<IClickableMenu>)delegate
					{
						grab_menu.DropRemainingItems();
					});
				}
			}
		}
		if (items.Count == 0 && (!playerChest || giftbox.Value))
		{
			location.removeObject(TileLocation, showDestroyedObject: false);
			location.playSound("woodWhack");
			for (int i = 0; i < 8; i++)
			{
				Game1.multiplayer.broadcastSprites(Location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", (Game1.random.NextDouble() < 0.5) ? new Microsoft.Xna.Framework.Rectangle(598, 1275, 13, 4) : new Microsoft.Xna.Framework.Rectangle(598, 1275, 13, 4), 999f, 1, 0, tileLocation.Value * 64f + new Vector2(32f, 64f), flicker: false, Game1.random.NextDouble() < 0.5, (tileLocation.Y * 64f + 64f) / 10000f, 0.01f, new Color(204, 132, 87), 4f, 0f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 8f, (float)Game1.random.Next(-5, 6) * (float)Math.PI / 64f)
				{
					motion = new Vector2((float)Game1.random.Next(-25, 26) / 10f, Game1.random.Next(-11, -8)),
					acceleration = new Vector2(0f, 0.3f)
				});
			}
			Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 7), resource: false, -1, item: false, new Color(204, 132, 87));
		}
		return true;
	}

	public virtual void OpenMiniShippingMenu()
	{
		Game1.playSound("shwip");
		ShowMenu();
	}

	public virtual void performOpenChest()
	{
		frameCounter.Value = 5;
	}

	public virtual void grabItemFromChest(Item item, Farmer who)
	{
		if (who.couldInventoryAcceptThisItem(item))
		{
			GetItemsForPlayer().Remove(item);
			clearNulls();
			ShowMenu();
		}
	}

	public virtual Item addItem(Item item)
	{
		item.resetState();
		clearNulls();
		IInventory item_list = GetItemsForPlayer();
		for (int i = 0; i < item_list.Count; i++)
		{
			if (item_list[i] != null && item_list[i].canStackWith(item))
			{
				item.Stack = item_list[i].addToStack(item);
				if (item.Stack <= 0)
				{
					return null;
				}
			}
		}
		if (item_list.Count < GetActualCapacity())
		{
			item_list.Add(item);
			return null;
		}
		return item;
	}

	public virtual int GetActualCapacity()
	{
		switch (SpecialChestType)
		{
		case SpecialChestTypes.MiniShippingBin:
		case SpecialChestTypes.JunimoChest:
			return 9;
		case SpecialChestTypes.Enricher:
			return 1;
		case SpecialChestTypes.BigChest:
			return 70;
		default:
			return 36;
		}
	}

	/// <summary>If there's an object below this chest, try to auto-load its inventory from this chest.</summary>
	/// <param name="who">The player who interacted with the chest.</param>
	public virtual void CheckAutoLoad(Farmer who)
	{
		GameLocation location = Location;
		Vector2 tile = TileLocation;
		if (location != null && location.objects.TryGetValue(new Vector2(tile.X, tile.Y + 1f), out var beneath_object))
		{
			beneath_object?.AttemptAutoLoad(who);
		}
	}

	public virtual void ShowMenu()
	{
		ItemGrabMenu oldMenu = Game1.activeClickableMenu as ItemGrabMenu;
		switch (SpecialChestType)
		{
		case SpecialChestTypes.MiniShippingBin:
			Game1.activeClickableMenu = new ItemGrabMenu(GetItemsForPlayer(), reverseGrab: false, showReceivingMenu: true, Utility.highlightShippableObjects, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: false, 1, fridge ? null : this, -1, this);
			break;
		case SpecialChestTypes.JunimoChest:
			Game1.activeClickableMenu = new ItemGrabMenu(GetItemsForPlayer(), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, fridge ? null : this, -1, this);
			break;
		case SpecialChestTypes.AutoLoader:
		{
			ItemGrabMenu itemGrabMenu = new ItemGrabMenu(GetItemsForPlayer(), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, fridge ? null : this, -1, this);
			itemGrabMenu.exitFunction = (IClickableMenu.onExit)Delegate.Combine(itemGrabMenu.exitFunction, (IClickableMenu.onExit)delegate
			{
				CheckAutoLoad(Game1.player);
			});
			Game1.activeClickableMenu = itemGrabMenu;
			break;
		}
		case SpecialChestTypes.Enricher:
			Game1.activeClickableMenu = new ItemGrabMenu(GetItemsForPlayer(), reverseGrab: false, showReceivingMenu: true, Object.HighlightFertilizers, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, fridge ? null : this, -1, this);
			break;
		default:
			Game1.activeClickableMenu = new ItemGrabMenu(GetItemsForPlayer(), reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, grabItemFromInventory, null, grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, fridge ? null : this, -1, this);
			break;
		}
		if (oldMenu != null && Game1.activeClickableMenu is ItemGrabMenu newMenu)
		{
			newMenu.inventory.moveItemSound = oldMenu.inventory.moveItemSound;
			newMenu.inventory.highlightMethod = oldMenu.inventory.highlightMethod;
		}
	}

	public virtual void grabItemFromInventory(Item item, Farmer who)
	{
		if (item.Stack == 0)
		{
			item.Stack = 1;
		}
		Item tmp = addItem(item);
		if (tmp == null)
		{
			who.removeItemFromInventory(item);
		}
		else
		{
			tmp = who.addItemToInventory(tmp);
		}
		clearNulls();
		int oldID = ((Game1.activeClickableMenu.currentlySnappedComponent != null) ? Game1.activeClickableMenu.currentlySnappedComponent.myID : (-1));
		ShowMenu();
		(Game1.activeClickableMenu as ItemGrabMenu).heldItem = tmp;
		if (oldID != -1)
		{
			Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
			Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
		}
	}

	public IInventory GetItemsForPlayer()
	{
		return GetItemsForPlayer(Game1.player.UniqueMultiplayerID);
	}

	public IInventory GetItemsForPlayer(long id)
	{
		if (GlobalInventoryId != null)
		{
			return Game1.player.team.GetOrCreateGlobalInventory(GlobalInventoryId);
		}
		switch (SpecialChestType)
		{
		case SpecialChestTypes.MiniShippingBin:
			if (Game1.player.team.useSeparateWallets.Value && SpecialChestType == SpecialChestTypes.MiniShippingBin && Game1.player.team.useSeparateWallets.Value)
			{
				if (!separateWalletItems.TryGetValue(id, out var items))
				{
					items = (separateWalletItems[id] = new Inventory());
				}
				return items;
			}
			break;
		case SpecialChestTypes.JunimoChest:
			return Game1.player.team.GetOrCreateGlobalInventory("JunimoChests");
		}
		return Items;
	}

	public virtual bool isEmpty()
	{
		if (SpecialChestType == SpecialChestTypes.MiniShippingBin && Game1.player.team.useSeparateWallets.Value)
		{
			foreach (Inventory value in separateWalletItems.Values)
			{
				if (value.HasAny())
				{
					return false;
				}
			}
			return true;
		}
		return !GetItemsForPlayer().HasAny();
	}

	public virtual void clearNulls()
	{
		GetItemsForPlayer().RemoveEmptySlots();
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		GameLocation environment = Location;
		if (environment == null)
		{
			return;
		}
		if (synchronized.Value)
		{
			openChestEvent.Poll();
		}
		if (localKickStartTile.HasValue)
		{
			if (Game1.currentLocation == environment)
			{
				if (kickProgress == 0f)
				{
					if (Utility.isOnScreen((localKickStartTile.Value + new Vector2(0.5f, 0.5f)) * 64f, 64))
					{
						Game1.playSound("clubhit");
					}
					shakeTimer = 100;
				}
			}
			else
			{
				localKickStartTile = null;
				kickProgress = -1f;
			}
			if (kickProgress >= 0f)
			{
				float move_duration = 0.25f;
				kickProgress += (float)(time.ElapsedGameTime.TotalSeconds / (double)move_duration);
				if (kickProgress >= 1f)
				{
					kickProgress = -1f;
					localKickStartTile = null;
				}
			}
		}
		else
		{
			kickProgress = -1f;
		}
		fixLidFrame();
		mutex.Update(environment);
		if (shakeTimer > 0)
		{
			shakeTimer -= time.ElapsedGameTime.Milliseconds;
			if (shakeTimer <= 0)
			{
				health = 10;
			}
		}
		hitTimerInstance?.Update(time);
		if ((bool)playerChest)
		{
			if (SpecialChestType == SpecialChestTypes.MiniShippingBin)
			{
				UpdateFarmerNearby();
				if (_shippingBinFrameCounter > -1)
				{
					_shippingBinFrameCounter--;
					if (_shippingBinFrameCounter <= 0)
					{
						_shippingBinFrameCounter = 5;
						if (_farmerNearby && currentLidFrame < getLastLidFrame())
						{
							currentLidFrame++;
						}
						else if (!_farmerNearby && currentLidFrame > startingLidFrame.Value)
						{
							currentLidFrame--;
						}
						else
						{
							_shippingBinFrameCounter = -1;
						}
					}
				}
				if (Game1.activeClickableMenu == null && GetMutex().IsLockHeld())
				{
					GetMutex().ReleaseLock();
				}
			}
			else if ((int)frameCounter > -1 && currentLidFrame < getLastLidFrame() + 1)
			{
				frameCounter.Value--;
				if ((int)frameCounter <= 0 && GetMutex().IsLockHeld())
				{
					if (currentLidFrame == getLastLidFrame())
					{
						ShowMenu();
						frameCounter.Value = -1;
					}
					else
					{
						frameCounter.Value = 5;
						currentLidFrame++;
					}
				}
			}
			else if ((((int)frameCounter == -1 && currentLidFrame > (int)startingLidFrame) || currentLidFrame >= getLastLidFrame()) && Game1.activeClickableMenu == null && GetMutex().IsLockHeld())
			{
				GetMutex().ReleaseLock();
				currentLidFrame = getLastLidFrame();
				frameCounter.Value = 2;
				environment.localSound("doorCreakReverse");
			}
		}
		else
		{
			if ((int)frameCounter <= -1 || currentLidFrame > getLastLidFrame())
			{
				return;
			}
			frameCounter.Value--;
			if ((int)frameCounter > 0)
			{
				return;
			}
			if (currentLidFrame == getLastLidFrame())
			{
				dumpContents();
				frameCounter.Value = -1;
				return;
			}
			frameCounter.Value = 10;
			currentLidFrame++;
			if (currentLidFrame == getLastLidFrame())
			{
				frameCounter.Value += 5;
			}
		}
	}

	public virtual void UpdateFarmerNearby(bool animate = true)
	{
		GameLocation location = Location;
		bool should_open = false;
		Vector2 curTile = tileLocation.Value;
		foreach (Farmer farmer in location.farmers)
		{
			Point playerTile = farmer.TilePoint;
			if (Math.Abs((float)playerTile.X - curTile.X) <= 1f && Math.Abs((float)playerTile.Y - curTile.Y) <= 1f)
			{
				should_open = true;
				break;
			}
		}
		if (should_open == _farmerNearby)
		{
			return;
		}
		_farmerNearby = should_open;
		_shippingBinFrameCounter = 5;
		if (!animate)
		{
			_shippingBinFrameCounter = -1;
			if (_farmerNearby)
			{
				currentLidFrame = getLastLidFrame();
			}
			else
			{
				currentLidFrame = startingLidFrame.Value;
			}
		}
		else if (Game1.gameMode != 6)
		{
			if (_farmerNearby)
			{
				location.localSound("doorCreak");
			}
			else
			{
				location.localSound("doorCreakReverse");
			}
		}
	}

	/// <inheritdoc />
	public override void actionOnPlayerEntry()
	{
		base.actionOnPlayerEntry();
		fixLidFrame();
		if (specialChestType.Value == SpecialChestTypes.MiniShippingBin)
		{
			UpdateFarmerNearby(animate: false);
		}
		kickProgress = -1f;
		localKickStartTile = null;
		if (!playerChest && GetItemsForPlayer().Count == 0)
		{
			currentLidFrame = getLastLidFrame();
		}
	}

	public virtual void SetBigCraftableSpriteIndex(int sprite_index, int starting_lid_frame = -1, int lid_frame_count = 3)
	{
		bigCraftableSpriteIndex.Value = sprite_index;
		if (starting_lid_frame >= 0)
		{
			startingLidFrame.Value = starting_lid_frame;
		}
		else
		{
			startingLidFrame.Value = sprite_index + 1;
		}
		lidFrameCount.Value = lid_frame_count;
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		float draw_x = x;
		float draw_y = y;
		if (localKickStartTile.HasValue)
		{
			draw_x = Utility.Lerp(localKickStartTile.Value.X, draw_x, kickProgress);
			draw_y = Utility.Lerp(localKickStartTile.Value.Y, draw_y, kickProgress);
		}
		float base_sort_order = Math.Max(0f, ((draw_y + 1f) * 64f - 24f) / 10000f) + draw_x * 1E-05f;
		if (localKickStartTile.HasValue)
		{
			spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((draw_x + 0.5f) * 64f, (draw_y + 0.5f) * 64f)), Game1.shadowTexture.Bounds, Color.Black * 0.5f, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 0.0001f);
			draw_y -= (float)Math.Sin((double)kickProgress * Math.PI) * 0.5f;
		}
		if ((bool)playerChest && (base.QualifiedItemId == "(BC)130" || base.QualifiedItemId == "(BC)232" || base.QualifiedItemId.Equals("(BC)BigChest") || base.QualifiedItemId.Equals("(BC)BigStoneChest")))
		{
			Texture2D texture;
			if (playerChoiceColor.Value.Equals(Color.Black))
			{
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
				texture = itemData.GetTexture();
				spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), itemData.GetSourceRect(), tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
				spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), itemData.GetSourceRect(0, currentLidFrame), tint.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
				return;
			}
			ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			texture = dataOrErrorItem.GetTexture();
			int spriteIndex = base.ParentSheetIndex;
			int lidIndex = currentLidFrame + 8;
			int coloredLidIndex = currentLidFrame;
			if (base.QualifiedItemId == "(BC)130")
			{
				spriteIndex = 168;
				lidIndex = currentLidFrame + 46;
				coloredLidIndex = currentLidFrame + 38;
			}
			else if (base.QualifiedItemId.Equals("(BC)BigChest"))
			{
				spriteIndex = 312;
				lidIndex = currentLidFrame + 16;
				coloredLidIndex = currentLidFrame + 8;
			}
			Microsoft.Xna.Framework.Rectangle drawRect = dataOrErrorItem.GetSourceRect(0, spriteIndex);
			Microsoft.Xna.Framework.Rectangle lidRect = dataOrErrorItem.GetSourceRect(0, lidIndex);
			Microsoft.Xna.Framework.Rectangle coloredLidRect = dataOrErrorItem.GetSourceRect(0, coloredLidIndex);
			spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, (draw_y - 1f) * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), drawRect, playerChoiceColor.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
			spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, draw_y * 64f + 20f)), new Microsoft.Xna.Framework.Rectangle(0, spriteIndex / 8 * 32 + 53, 16, 11), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 2E-05f);
			spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, (draw_y - 1f) * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), lidRect, Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 2E-05f);
			spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, (draw_y - 1f) * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), coloredLidRect, playerChoiceColor.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
			return;
		}
		if ((bool)playerChest)
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			Texture2D texture = itemData.GetTexture();
			spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), itemData.GetSourceRect(), tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
			spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (draw_y - 1f) * 64f)), itemData.GetSourceRect(0, currentLidFrame), tint.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
			return;
		}
		if ((bool)giftbox)
		{
			spriteBatch.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(16f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 5f, SpriteEffects.None, 1E-07f);
			if (GetItemsForPlayer().Count > 0)
			{
				int textureY = (int)giftboxIndex * 32;
				spriteBatch.Draw(Game1.giftboxTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f + (float)((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), draw_y * 64f - 52f)), new Microsoft.Xna.Framework.Rectangle(0, textureY, 16, 32), tint.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
			}
			return;
		}
		int sprite_index = 500;
		Texture2D sprite_sheet = Game1.objectSpriteSheet;
		int sprite_sheet_height = 16;
		int y_offset = 0;
		if (bigCraftableSpriteIndex.Value >= 0)
		{
			sprite_index = bigCraftableSpriteIndex.Value;
			sprite_sheet = Game1.bigCraftableSpriteSheet;
			sprite_sheet_height = 32;
			y_offset = -64;
		}
		if (bigCraftableSpriteIndex.Value < 0)
		{
			spriteBatch.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(16f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 5f, SpriteEffects.None, 1E-07f);
		}
		spriteBatch.Draw(sprite_sheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_x * 64f, draw_y * 64f + (float)y_offset)), Game1.getSourceRectForStandardTileSheet(sprite_sheet, sprite_index, 16, sprite_sheet_height), tint.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order);
		Vector2 lidPosition = new Vector2(draw_x * 64f, draw_y * 64f + (float)y_offset);
		if (bigCraftableSpriteIndex.Value < 0)
		{
			switch (currentLidFrame)
			{
			case 501:
				lidPosition.Y -= 32f;
				break;
			case 502:
				lidPosition.Y -= 40f;
				break;
			case 503:
				lidPosition.Y -= 60f;
				break;
			}
		}
		spriteBatch.Draw(sprite_sheet, Game1.GlobalToLocal(Game1.viewport, lidPosition), Game1.getSourceRectForStandardTileSheet(sprite_sheet, currentLidFrame, 16, sprite_sheet_height), tint.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, base_sort_order + 1E-05f);
	}

	public virtual void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f, bool local = false)
	{
		if (!playerChest)
		{
			return;
		}
		ParsedItemData itemData;
		if (playerChoiceColor.Equals(Color.Black))
		{
			itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			spriteBatch.Draw(itemData.GetTexture(), local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (y - 1) * 64)), itemData.GetSourceRect(), tint.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.89f : ((float)(y * 64 + 4) / 10000f));
			return;
		}
		itemData = ItemRegistry.GetData(base.QualifiedItemId);
		if (itemData != null)
		{
			int drawIndex = base.ParentSheetIndex;
			int overlayIndex = currentLidFrame + 8;
			int coloredLidIndex = currentLidFrame;
			if (base.QualifiedItemId == "(BC)130")
			{
				drawIndex = 168;
				overlayIndex = currentLidFrame + 46;
				coloredLidIndex = currentLidFrame + 38;
			}
			else if (base.QualifiedItemId.Equals("(BC)BigChest"))
			{
				drawIndex = 312;
				overlayIndex = currentLidFrame + 16;
				coloredLidIndex = currentLidFrame + 8;
			}
			else if (base.QualifiedItemId.Equals("(BC)BigStoneChest"))
			{
				overlayIndex = currentLidFrame + 8;
				coloredLidIndex = currentLidFrame;
			}
			Microsoft.Xna.Framework.Rectangle drawRect = itemData.GetSourceRect(0, drawIndex);
			Microsoft.Xna.Framework.Rectangle lidRect = itemData.GetSourceRect(0, overlayIndex);
			Microsoft.Xna.Framework.Rectangle coloredLidRect = itemData.GetSourceRect(0, coloredLidIndex);
			Texture2D texture = itemData.GetTexture();
			spriteBatch.Draw(texture, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), drawRect, playerChoiceColor.Value * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.9f : ((float)(y * 64 + 4) / 10000f));
			spriteBatch.Draw(texture, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), coloredLidRect, playerChoiceColor.Value * alpha * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.9f : ((float)(y * 64 + 5) / 10000f));
			spriteBatch.Draw(texture, local ? new Vector2(x, y + 20) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 + 20)), new Microsoft.Xna.Framework.Rectangle(0, drawIndex / 8 * 32 + 53, 16, 11), Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.91f : ((float)(y * 64 + 6) / 10000f));
			spriteBatch.Draw(texture, local ? new Vector2(x, y - 64) : Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y - 1) * 64 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), lidRect, Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, local ? 0.91f : ((float)(y * 64 + 6) / 10000f));
		}
	}

	/// <inheritdoc />
	public override bool ForEachItem(ForEachItemDelegate handler)
	{
		if (base.ForEachItem(handler))
		{
			return ForEachItemHelper.ApplyToList(Items, handler);
		}
		return false;
	}
}
