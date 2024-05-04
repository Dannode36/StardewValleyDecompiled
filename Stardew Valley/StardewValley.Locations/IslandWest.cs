using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class IslandWest : IslandLocation
{
	[XmlElement("addedSlimesToday")]
	private readonly NetBool addedSlimesToday = new NetBool();

	[XmlElement("sandDuggy")]
	public NetRef<SandDuggy> sandDuggy = new NetRef<SandDuggy>();

	[XmlElement("farmhouseRestored")]
	public readonly NetBool farmhouseRestored = new NetBool
	{
		InterpolationWait = false
	};

	[XmlElement("farmhouseMailbox")]
	public readonly NetBool farmhouseMailbox = new NetBool
	{
		InterpolationWait = false
	};

	[XmlElement("farmObelisk")]
	public readonly NetBool farmObelisk = new NetBool
	{
		InterpolationWait = false
	};

	public Point shippingBinPosition = new Point(90, 39);

	private TemporaryAnimatedSprite shippingBinLid;

	private Microsoft.Xna.Framework.Rectangle shippingBinLidOpenArea;

	public override void SetBuriedNutLocations()
	{
		buriedNutPoints.Add(new Point(21, 81));
		buriedNutPoints.Add(new Point(62, 76));
		buriedNutPoints.Add(new Point(39, 24));
		buriedNutPoints.Add(new Point(88, 14));
		buriedNutPoints.Add(new Point(43, 74));
		buriedNutPoints.Add(new Point(30, 75));
		base.SetBuriedNutLocations();
	}

	/// <inheritdoc />
	public override bool CanPlantSeedsHere(string itemId, int tileX, int tileY, bool isGardenPot, out string deniedMessage)
	{
		if (getTileSheetIDAt(tileX, tileY, "Back") != "untitled tile sheet2")
		{
			deniedMessage = null;
			return false;
		}
		return base.CanPlantSeedsHere(itemId, tileX, tileY, isGardenPot, out deniedMessage);
	}

	/// <inheritdoc />
	public override bool CanPlantTreesHere(string itemId, int tileX, int tileY, out string deniedMessage)
	{
		if (getTileSheetIDAt(tileX, tileY, "Back") == "untitled tile sheet2" || Object.isWildTreeSeed(itemId))
		{
			switch (doesTileHavePropertyNoNull(tileX, tileY, "Type", "Back"))
			{
			case "Dirt":
			case "Grass":
			case "":
				return CheckItemPlantRules(itemId, isGardenPot: false, defaultAllowed: true, out deniedMessage);
			}
		}
		return base.CanPlantTreesHere(itemId, tileX, tileY, out deniedMessage);
	}

	public IslandWest()
	{
	}

	public override bool performToolAction(Tool t, int tileX, int tileY)
	{
		sandDuggy.Value?.PerformToolAction(t, tileX, tileY);
		return base.performToolAction(t, tileX, tileY);
	}

	public override List<Vector2> GetAdditionalWalnutBushes()
	{
		return new List<Vector2>
		{
			new Vector2(54f, 18f),
			new Vector2(25f, 30f),
			new Vector2(15f, 3f)
		};
	}

	public override void draw(SpriteBatch b)
	{
		sandDuggy.Value?.Draw(b);
		if (farmhouseRestored.Value)
		{
			shippingBinLid?.draw(b);
		}
		if (farmhouseMailbox.Value && Game1.mailbox.Count > 0)
		{
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			Point mailbox_position = new Point(81, 40);
			float draw_layer = (float)((mailbox_position.X + 1) * 64) / 10000f + (float)(mailbox_position.Y * 64) / 10000f;
			float xOffset = -8f;
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(mailbox_position.X * 64) + xOffset, (float)(mailbox_position.Y * 64 - 96 - 48) + yOffset)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer + 1E-06f);
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(mailbox_position.X * 64 + 32 + 4) + xOffset, (float)(mailbox_position.Y * 64 - 64 - 24 - 8) + yOffset)), new Microsoft.Xna.Framework.Rectangle(189, 423, 15, 13), Color.White, 0f, new Vector2(7f, 6f), 4f, SpriteEffects.None, draw_layer + 1E-05f);
		}
		base.draw(b);
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		sandDuggy.Value?.Update(time);
		if (farmhouseRestored.Value && shippingBinLid != null)
		{
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
		base.UpdateWhenCurrentLocation(time);
	}

	public IslandWest(string map, string name)
		: base(map, name)
	{
		sandDuggy.Value = new SandDuggy(this, new Point[4]
		{
			new Point(37, 87),
			new Point(41, 86),
			new Point(45, 86),
			new Point(48, 87)
		});
		parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(72, 37), new Microsoft.Xna.Framework.Rectangle(71, 29, 3, 8), 20, delegate
		{
			Game1.createItemDebris(ItemRegistry.Create("(O)886"), new Vector2(72f, 37f) * 64f + new Vector2(32f), 2);
			Game1.addMailForTomorrow("Island_W_Obelisk", noLetter: true, sendToEveryone: true);
			farmObelisk.Value = true;
		}, () => farmObelisk.Value, "Obelisk", "Island_UpgradeHouse_Mailbox"));
		parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(81, 40), new Microsoft.Xna.Framework.Rectangle(80, 39, 3, 2), 5, delegate
		{
			Game1.addMailForTomorrow("Island_UpgradeHouse_Mailbox", noLetter: true, sendToEveryone: true);
			farmhouseMailbox.Value = true;
		}, () => farmhouseMailbox.Value, "House_Mailbox", "Island_UpgradeHouse"));
		parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(81, 40), new Microsoft.Xna.Framework.Rectangle(74, 36, 7, 4), 20, delegate
		{
			Game1.addMailForTomorrow("Island_UpgradeHouse", noLetter: true, sendToEveryone: true);
			farmhouseRestored.Value = true;
		}, () => farmhouseRestored.Value, "House"));
		parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(72, 10), new Microsoft.Xna.Framework.Rectangle(73, 5, 3, 5), 10, delegate
		{
			Game1.addMailForTomorrow("Island_UpgradeParrotPlatform", noLetter: true, sendToEveryone: true);
			Game1.netWorldState.Value.ParrotPlatformsUnlocked = true;
		}, () => Game1.netWorldState.Value.ParrotPlatformsUnlocked, "ParrotPlatforms"));
	}

	/// <inheritdoc />
	public override bool performAction(string[] action, Farmer who, Location tileLocation)
	{
		if (ArgUtility.Get(action, 0) == "FarmObelisk")
		{
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
			DelayedAction.fadeAfterDelay(delegate
			{
				if (!Game1.getFarm().TryGetMapPropertyAs("WarpTotemEntry", out Point parsed, required: false))
				{
					parsed = Game1.whichFarm switch
					{
						6 => new Point(82, 29), 
						5 => new Point(48, 39), 
						_ => new Point(48, 7), 
					};
				}
				Game1.warpFarmer("Farm", parsed.X, parsed.Y, flip: false);
				Game1.fadeToBlackAlpha = 0.99f;
				Game1.screenGlow = false;
				Game1.player.temporarilyInvincible = false;
				Game1.player.temporaryInvincibilityTimer = 0;
				Game1.displayFarmer = true;
			}, 1000);
			Microsoft.Xna.Framework.Rectangle playerBounds = who.GetBoundingBox();
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
			return true;
		}
		return base.performAction(action, who, tileLocation);
	}

	public override bool leftClick(int x, int y, Farmer who)
	{
		if (farmhouseRestored.Value && who.ActiveObject != null && x / 64 >= shippingBinPosition.X && x / 64 <= shippingBinPosition.X + 1 && y / 64 >= shippingBinPosition.Y - 1 && y / 64 <= shippingBinPosition.Y && who.ActiveObject.canBeShipped() && Vector2.Distance(who.Tile, new Vector2((float)shippingBinPosition.X + 0.5f, shippingBinPosition.Y)) <= 2f)
		{
			Game1.getFarm().getShippingBin(who).Add(who.ActiveObject);
			Game1.getFarm().lastItemShipped = who.ActiveObject;
			who.showNotCarrying();
			showShipment(who.ActiveObject);
			who.ActiveObject = null;
			return true;
		}
		return base.leftClick(x, y, who);
	}

	public void showShipment(Object o, bool playThrowSound = true)
	{
		if (playThrowSound)
		{
			localSound("backpackIN");
		}
		DelayedAction.playSoundAfterDelay("Ship", playThrowSound ? 250 : 0);
		int temp = Game1.random.Next();
		temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(524, 218, 34, 22), new Vector2(90f, 38f) * 64f + new Vector2(0f, 5f) * 4f, flipped: false, 0f, Color.White)
		{
			interval = 100f,
			totalNumberOfLoops = 1,
			animationLength = 3,
			pingPong = true,
			scale = 4f,
			layerDepth = 0.25601003f,
			id = temp,
			extraInfoForEndBehavior = temp,
			endFunction = base.removeTemporarySpritesWithID
		});
		temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(524, 230, 34, 10), new Vector2(90f, 38f) * 64f + new Vector2(0f, 17f) * 4f, flipped: false, 0f, Color.White)
		{
			interval = 100f,
			totalNumberOfLoops = 1,
			animationLength = 3,
			pingPong = true,
			scale = 4f,
			layerDepth = 0.2563f,
			id = temp,
			extraInfoForEndBehavior = temp
		});
		TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite(null, default(Microsoft.Xna.Framework.Rectangle), new Vector2(90f, 38f) * 64f + new Vector2(8 + Game1.random.Next(6), 2f) * 4f, flipped: false, 0f, Color.White)
		{
			interval = 9999f,
			alphaFade = 0.045f,
			layerDepth = 0.25622502f,
			motion = new Vector2(0f, 0.3f),
			acceleration = new Vector2(0f, 0.2f),
			scaleChange = -0.05f
		};
		sprite.CopyAppearanceFromItemId(o.QualifiedItemId);
		temporarySprites.Add(sprite);
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		if (farmhouseRestored.Value && tileLocation.X >= shippingBinPosition.X && tileLocation.X <= shippingBinPosition.X + 1 && tileLocation.Y >= shippingBinPosition.Y - 1 && tileLocation.Y <= shippingBinPosition.Y)
		{
			ItemGrabMenu itemGrabMenu = new ItemGrabMenu(null, reverseGrab: true, showReceivingMenu: false, Utility.highlightShippableObjects, Game1.getFarm().shipItem, "", null, snapToBottom: true, canBeExitedWithKey: true, playRightClickSound: false, allowRightClick: true, showOrganizeButton: false, 0, null, -1, this);
			itemGrabMenu.initializeUpperRightCloseButton();
			itemGrabMenu.setBackgroundTransparency(b: false);
			itemGrabMenu.setDestroyItemOnClick(b: true);
			itemGrabMenu.initializeShippingBin();
			Game1.activeClickableMenu = itemGrabMenu;
			playSound("shwip");
			if (Game1.player.FacingDirection == 1)
			{
				Game1.player.Halt();
			}
			Game1.player.showCarrying();
			return true;
		}
		if (getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings") != -1 && getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings") == 1470)
		{
			if (!IsQiWalnutRoomDoorUnlocked(out var actualFoundWalnutsCount))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:qiNutDoor", actualFoundWalnutsCount));
			}
			else
			{
				Game1.playSound("doorClose");
				Game1.warpFarmer("QiNutRoom", 7, 8, 0);
			}
			return true;
		}
		NPC birdie = getCharacterFromName("Birdie");
		if (birdie != null && !birdie.IsInvisible && (birdie.Tile == new Vector2(tileLocation.X, tileLocation.Y) || birdie.Tile == new Vector2(tileLocation.X - 1, tileLocation.Y)))
		{
			if (who.mailReceived.Add("birdieQuestBegun"))
			{
				who.Halt();
				Game1.globalFadeToBlack(delegate
				{
					startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandSecret_Event_BirdieIntro"), null, "-888999"));
				});
				return true;
			}
			if (who.hasQuest("130") && who.ActiveObject?.QualifiedItemId == "(O)870" && who.mailReceived.Add("birdieQuestFinished"))
			{
				who.Halt();
				Game1.globalFadeToBlack(delegate
				{
					who.reduceActiveItemByOne();
					startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandSecret_Event_BirdieFinished"), null, "-666777"));
				});
				return true;
			}
			if (who.mailReceived.Contains("birdieQuestFinished"))
			{
				if (who.ActiveObject != null)
				{
					Game1.DrawDialogue(birdie, "Data\\ExtraDialogue:Birdie_NoGift");
				}
				else
				{
					Dialogue possible = Dialogue.TryGetDialogue(birdie, "Data\\ExtraDialogue:Birdie" + Game1.dayOfMonth);
					if (possible != null)
					{
						Game1.DrawDialogue(possible);
					}
					else
					{
						Game1.DrawDialogue(birdie, "Data\\ExtraDialogue:Birdie" + Game1.dayOfMonth % 7);
					}
				}
			}
		}
		return base.checkAction(tileLocation, viewport, who);
	}

	public static bool IsQiWalnutRoomDoorUnlocked(out int actualFoundWalnutsCount)
	{
		actualFoundWalnutsCount = Math.Max(0, Game1.netWorldState.Value.GoldenWalnutsFound - 1);
		return actualFoundWalnutsCount >= 100;
	}

	public override bool isActionableTile(int xTile, int yTile, Farmer who)
	{
		if (!Game1.eventUp)
		{
			NPC birdie = getCharacterFromName("Birdie");
			if (birdie != null && !birdie.IsInvisible && birdie.Tile == new Vector2(xTile - 1, yTile) && (!who.mailReceived.Contains("birdieQuestBegun") || who.mailReceived.Contains("birdieQuestFinished")))
			{
				Game1.isSpeechAtCurrentCursorTile = true;
				return true;
			}
		}
		return base.isActionableTile(xTile, yTile, who);
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(addedSlimesToday, "addedSlimesToday").AddField(farmhouseRestored, "farmhouseRestored").AddField(sandDuggy, "sandDuggy")
			.AddField(farmhouseMailbox, "farmhouseMailbox")
			.AddField(farmObelisk, "farmObelisk");
		farmhouseRestored.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
		{
			if (newValue && mapPath.Value != null)
			{
				ApplyFarmHouseRestore();
			}
		};
		farmhouseMailbox.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
		{
			if (newValue && mapPath.Value != null)
			{
				ApplyFarmHouseRestore();
			}
		};
		farmObelisk.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
		{
			if (newValue && mapPath.Value != null)
			{
				ApplyFarmObeliskBuild();
			}
		};
	}

	public void ApplyFarmObeliskBuild()
	{
		if (map != null && !_appliedMapOverrides.Contains("Island_W_Obelisk"))
		{
			ApplyMapOverride("Island_W_Obelisk", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(71, 29, 3, 9));
		}
	}

	public void ApplyFarmHouseRestore()
	{
		if (map != null)
		{
			if (!_appliedMapOverrides.Contains("Island_House_Restored"))
			{
				ApplyMapOverride("Island_House_Restored", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(74, 33, 7, 9));
				ApplyMapOverride("Island_House_Bin", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(shippingBinPosition.X, shippingBinPosition.Y - 1, 2, 2));
				ApplyMapOverride("Island_House_Cave", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(95, 30, 3, 4));
			}
			if (farmhouseMailbox.Value)
			{
				setMapTileIndex(81, 40, 771, "Buildings");
				setMapTileIndex(81, 39, 739, "Front");
				setTileProperty(81, 40, "Buildings", "Action", "Mailbox");
			}
		}
	}

	public override void monsterDrop(Monster monster, int x, int y, Farmer who)
	{
		base.monsterDrop(monster, x, y, who);
		if (!Game1.MasterPlayer.hasOrWillReceiveMail("tigerSlimeNut"))
		{
			int numTigerSlimes = 0;
			foreach (NPC n in characters)
			{
				if (n is GreenSlime && n.name == "Tiger Slime")
				{
					numTigerSlimes++;
				}
			}
			if (numTigerSlimes == 1)
			{
				Game1.addMailForTomorrow("tigerSlimeNut", noLetter: true, sendToEveryone: true);
				Game1.player.team.RequestLimitedNutDrops("TigerSlimeNut", this, x, y, 1);
			}
		}
		if (Game1.random.NextDouble() < 0.01)
		{
			long farmerId = who?.UniqueMultiplayerID ?? 0;
			Game1.createObjectDebris("(O)826", x, y, farmerId, this);
		}
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		if (l is IslandWest location)
		{
			farmhouseRestored.Value = location.farmhouseRestored;
			farmhouseMailbox.Value = location.farmhouseMailbox;
			farmObelisk.Value = location.farmObelisk.Value;
			sandDuggy.Value.whacked.Value = location.sandDuggy.Value.whacked.Value;
		}
		base.TransferDataFromSavedLocation(l);
	}

	public override void spawnObjects()
	{
		base.spawnObjects();
		Microsoft.Xna.Framework.Rectangle musselNodeSpawnArea = new Microsoft.Xna.Framework.Rectangle(57, 78, 43, 8);
		if (Utility.getNumObjectsOfIndexWithinRectangle(musselNodeSpawnArea, new string[1] { "(O)25" }, this) < 10)
		{
			Vector2 spawn = Utility.getRandomPositionInThisRectangle(musselNodeSpawnArea, Game1.random);
			if (CanItemBePlacedHere(spawn, itemIsPassable: false, CollisionMask.All, CollisionMask.None))
			{
				objects.Add(spawn, new Object("25", 1)
				{
					MinutesUntilReady = 8,
					Flipped = Game1.random.NextBool()
				});
			}
		}
		Microsoft.Xna.Framework.Rectangle tidePoolsArea = new Microsoft.Xna.Framework.Rectangle(20, 71, 28, 16);
		if (Utility.getNumObjectsOfIndexWithinRectangle(tidePoolsArea, new string[2] { "(O)393", "(O)397" }, this) < 5)
		{
			Vector2 spawn = Utility.getRandomPositionInThisRectangle(tidePoolsArea, Game1.random);
			if (CanItemBePlacedHere(spawn, itemIsPassable: false, CollisionMask.All, CollisionMask.None))
			{
				Object obj = ItemRegistry.Create<Object>((Game1.random.NextDouble() < 0.1) ? "(O)397" : "(O)393");
				obj.IsSpawnedObject = true;
				obj.CanBeGrabbed = true;
				objects.Add(spawn, obj);
			}
		}
	}

	public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
	{
		if (xLocation == 18 && yLocation == 42 && who.secretNotesSeen.Contains(1004))
		{
			Game1.player.team.RequestLimitedNutDrops("Island_W_BuriedTreasureNut", this, xLocation * 64, yLocation * 64, 1);
			if (!Game1.player.hasOrWillReceiveMail("Island_W_BuriedTreasure"))
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)877"), new Vector2(xLocation, yLocation) * 64f, 1);
				Game1.addMailForTomorrow("Island_W_BuriedTreasure", noLetter: true);
			}
		}
		else if (xLocation == 104 && yLocation == 74 && who.secretNotesSeen.Contains(1006))
		{
			Game1.player.team.RequestLimitedNutDrops("Island_W_BuriedTreasureNut2", this, xLocation * 64, yLocation * 64, 1);
			if (!Game1.player.hasOrWillReceiveMail("Island_W_BuriedTreasure2"))
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)797"), new Vector2(xLocation, yLocation) * 64f, 1);
				Game1.addMailForTomorrow("Island_W_BuriedTreasure2", noLetter: true);
			}
		}
		return base.checkForBuriedItem(xLocation, yLocation, explosion, detectOnly, who);
	}

	protected override bool breakStone(string stoneId, int x, int y, Farmer who, Random r)
	{
		if (r.NextDouble() < ((stoneId == "25") ? 0.025 : 0.01))
		{
			long farmerId = who?.UniqueMultiplayerID ?? 0;
			Game1.createObjectDebris("(O)826", x, y, farmerId, this);
		}
		return base.breakStone(stoneId, x, y, who, r);
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		for (int i = 0; i < characters.Count; i++)
		{
			if (characters[i] is Monster)
			{
				characters.RemoveAt(i);
				i--;
			}
		}
		addedSlimesToday.Value = false;
		List<Vector2> gingerTiles = new List<Vector2>();
		foreach (Vector2 v in terrainFeatures.Keys)
		{
			if (terrainFeatures[v] is HoeDirt { crop: not null } dirt && dirt.crop.forageCrop.Value)
			{
				gingerTiles.Add(v);
			}
		}
		foreach (Vector2 v in gingerTiles)
		{
			terrainFeatures.Remove(v);
		}
		List<Microsoft.Xna.Framework.Rectangle> gingerLocations = new List<Microsoft.Xna.Framework.Rectangle>();
		gingerLocations.Add(new Microsoft.Xna.Framework.Rectangle(31, 43, 7, 6));
		gingerLocations.Add(new Microsoft.Xna.Framework.Rectangle(37, 62, 6, 5));
		gingerLocations.Add(new Microsoft.Xna.Framework.Rectangle(48, 42, 5, 4));
		gingerLocations.Add(new Microsoft.Xna.Framework.Rectangle(71, 12, 5, 4));
		gingerLocations.Add(new Microsoft.Xna.Framework.Rectangle(50, 59, 1, 1));
		gingerLocations.Add(new Microsoft.Xna.Framework.Rectangle(47, 64, 1, 1));
		gingerLocations.Add(new Microsoft.Xna.Framework.Rectangle(36, 58, 1, 1));
		gingerLocations.Add(new Microsoft.Xna.Framework.Rectangle(56, 48, 1, 1));
		gingerLocations.Add(new Microsoft.Xna.Framework.Rectangle(29, 46, 1, 1));
		for (int i = 0; i < 5; i++)
		{
			Microsoft.Xna.Framework.Rectangle r = gingerLocations[Game1.random.Next(gingerLocations.Count)];
			Vector2 origin = new Vector2(Game1.random.Next(r.X, r.Right), Game1.random.Next(r.Y, r.Bottom));
			foreach (Vector2 v in Utility.recursiveFindOpenTiles(this, origin, 16))
			{
				string s = doesTileHaveProperty((int)v.X, (int)v.Y, "Diggable", "Back");
				if (!terrainFeatures.ContainsKey(v) && s != null && Game1.random.NextDouble() < (double)(1f - Vector2.Distance(origin, v) * 0.35f))
				{
					HoeDirt d = new HoeDirt(0, new Crop(forageCrop: true, "2", (int)v.X, (int)v.Y, this));
					d.state.Value = 2;
					terrainFeatures.Add(v, d);
				}
			}
		}
		if (Game1.MasterPlayer.mailReceived.Contains("Island_Turtle"))
		{
			spawnWeedsAndStones(20, weedsOnly: true);
			if (Game1.dayOfMonth % 7 == 1)
			{
				spawnWeedsAndStones(20, weedsOnly: true, spawnFromOldWeeds: false);
			}
		}
	}

	/// <inheritdoc />
	public override double GetDirtDecayChance(Vector2 tile)
	{
		if (getTileSheetIDAt((int)tile.X, (int)tile.Y, "Back") != "untitled tile sheet2")
		{
			return 1.0;
		}
		return base.GetDirtDecayChance(tile);
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		if (farmhouseRestored.Value)
		{
			ApplyFarmHouseRestore();
		}
		if (farmObelisk.Value)
		{
			ApplyFarmObeliskBuild();
		}
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		shippingBinLidOpenArea = new Microsoft.Xna.Framework.Rectangle((shippingBinPosition.X - 1) * 64, (shippingBinPosition.Y - 1) * 64, 256, 192);
		shippingBinLid = new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(134, 226, 30, 25), new Vector2(shippingBinPosition.X, shippingBinPosition.Y - 1) * 64f + new Vector2(2f, -7f) * 4f, flipped: false, 0f, Color.White)
		{
			holdLastFrame = true,
			destroyable = false,
			interval = 20f,
			animationLength = 13,
			paused = true,
			scale = 4f,
			layerDepth = (float)((shippingBinPosition.Y + 1) * 64) / 10000f + 0.0001f,
			pingPong = true,
			pingPongMotion = 0
		};
		sandDuggy.Value?.ResetForPlayerEntry();
		NPC n = getCharacterFromName("Birdie");
		if (n != null)
		{
			if (n.Sprite.SourceRect.Width < 32)
			{
				n.extendSourceRect(16, 0);
			}
			n.Sprite.SpriteWidth = 32;
			n.Sprite.ignoreSourceRectUpdates = false;
			n.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(8, 1000, 0, secondaryArm: false, flip: false),
				new FarmerSprite.AnimationFrame(9, 1000, 0, secondaryArm: false, flip: false)
			});
			n.Sprite.loop = true;
			n.HideShadow = true;
			n.IsInvisible = IsRainingHere();
		}
		if (Game1.timeOfDay > 1700)
		{
			temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(23f, 58f) * 64f + new Vector2(-16f, -32f), flipped: false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 4,
				light = true,
				lightID = 987654,
				id = 987654,
				lightRadius = 2f,
				scale = 4f,
				layerDepth = 0.37824f
			});
			AmbientLocationSounds.addSound(new Vector2(23f, 58f), 1);
		}
		if (AreMoonlightJelliesOut())
		{
			addMoonlightJellies(100, Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, -24917.0), new Microsoft.Xna.Framework.Rectangle(35, 0, 60, 60));
		}
	}

	protected override void resetSharedState()
	{
		base.resetSharedState();
		if ((bool)addedSlimesToday)
		{
			return;
		}
		addedSlimesToday.Value = true;
		Random rand = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, 12.0);
		Microsoft.Xna.Framework.Rectangle spawnArea = new Microsoft.Xna.Framework.Rectangle(28, 24, 19, 8);
		for (int tries = 5; tries > 0; tries--)
		{
			Vector2 tile = Utility.getRandomPositionInThisRectangle(spawnArea, rand);
			if (CanItemBePlacedHere(tile))
			{
				GreenSlime m = new GreenSlime(tile * 64f, 0);
				m.makeTigerSlime();
				characters.Add(m);
			}
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
		TemporaryAnimatedSprite temporaryAnimatedSprite = shippingBinLid;
		if (temporaryAnimatedSprite != null && temporaryAnimatedSprite.currentParentTileIndex > 0)
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
}
