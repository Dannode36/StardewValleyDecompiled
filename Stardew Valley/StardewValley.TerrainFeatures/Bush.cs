using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Tools;

namespace StardewValley.TerrainFeatures;

public class Bush : LargeTerrainFeature
{
	public const float shakeRate = (float)Math.PI / 200f;

	public const float shakeDecayRate = 0.0030679617f;

	public const int smallBush = 0;

	public const int mediumBush = 1;

	public const int largeBush = 2;

	public const int greenTeaBush = 3;

	public const int walnutBush = 4;

	public const int daysToMatureGreenTeaBush = 20;

	/// <summary>The type of bush, usually matching a constant like <see cref="F:StardewValley.TerrainFeatures.Bush.smallBush" />.</summary>
	[XmlElement("size")]
	public readonly NetInt size = new NetInt();

	[XmlElement("datePlanted")]
	public readonly NetInt datePlanted = new NetInt();

	[XmlElement("tileSheetOffset")]
	public readonly NetInt tileSheetOffset = new NetInt();

	public float health;

	[XmlElement("flipped")]
	public readonly NetBool flipped = new NetBool();

	/// <summary>Whether this is a cosmetic bush which produces no berries.</summary>
	[XmlElement("townBush")]
	public readonly NetBool townBush = new NetBool();

	/// <summary>Whether this bush is planted in a garden pot.</summary>
	public readonly NetBool inPot = new NetBool();

	[XmlElement("drawShadow")]
	public readonly NetBool drawShadow = new NetBool(value: true);

	private bool shakeLeft;

	private float shakeRotation;

	private float maxShake;

	[XmlIgnore]
	public float shakeTimer;

	[XmlIgnore]
	public readonly NetRectangle sourceRect = new NetRectangle();

	[XmlIgnore]
	public NetMutex uniqueSpawnMutex = new NetMutex();

	public static Lazy<Texture2D> texture = new Lazy<Texture2D>(() => Game1.content.Load<Texture2D>("TileSheets\\bushes"));

	public static Rectangle shadowSourceRect = new Rectangle(663, 1011, 41, 30);

	private float yDrawOffset;

	public Bush()
		: base(needsTick: true)
	{
	}

	public Bush(Vector2 tileLocation, int size, GameLocation location, int datePlantedOverride = -1)
		: this()
	{
		Tile = tileLocation;
		this.size.Value = size;
		Location = location;
		townBush.Value = location is Town && (size == 0 || size == 1 || size == 2) && tileLocation.X % 5f != 0f;
		if (location.map.RequireLayer("Front").Tiles[(int)tileLocation.X, (int)tileLocation.Y] != null)
		{
			drawShadow.Value = false;
		}
		datePlanted.Value = ((datePlantedOverride == -1) ? ((int)Game1.stats.DaysPlayed) : datePlantedOverride);
		switch (size)
		{
		case 3:
			drawShadow.Value = false;
			break;
		case 4:
			tileSheetOffset.Value = 1;
			break;
		}
		GameLocation old_location = Location;
		Location = location;
		loadSprite();
		Location = old_location;
		flipped.Value = Game1.random.NextBool();
	}

	public override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(size, "size").AddField(tileSheetOffset, "tileSheetOffset").AddField(flipped, "flipped")
			.AddField(townBush, "townBush")
			.AddField(drawShadow, "drawShadow")
			.AddField(sourceRect, "sourceRect")
			.AddField(datePlanted, "datePlanted")
			.AddField(inPot, "inPot")
			.AddField(uniqueSpawnMutex.NetFields, "uniqueSpawnMutex.NetFields");
	}

	public int getAge()
	{
		return (int)Game1.stats.DaysPlayed - datePlanted.Value;
	}

	public void setUpSourceRect()
	{
		Season season = ((!IsSheltered()) ? Location.GetSeason() : Season.Spring);
		int seasonNumber = (int)season;
		switch (size.Value)
		{
		case 0:
			sourceRect.Value = new Rectangle(seasonNumber * 16 * 2 + (int)tileSheetOffset * 16, 224, 16, 32);
			break;
		case 1:
		{
			if (townBush.Value)
			{
				sourceRect.Value = new Rectangle(seasonNumber * 16 * 2, 96, 32, 32);
				break;
			}
			int xOffset = seasonNumber * 16 * 4 + (int)tileSheetOffset * 16 * 2;
			sourceRect.Value = new Rectangle(xOffset % texture.Value.Bounds.Width, xOffset / texture.Value.Bounds.Width * 3 * 16, 32, 48);
			break;
		}
		case 2:
			if (townBush.Value && (season == Season.Spring || season == Season.Summer))
			{
				sourceRect.Value = new Rectangle(48, 176, 48, 48);
				break;
			}
			switch (season)
			{
			case Season.Spring:
			case Season.Summer:
				sourceRect.Value = new Rectangle(0, 128, 48, 48);
				break;
			case Season.Fall:
				sourceRect.Value = new Rectangle(48, 128, 48, 48);
				break;
			case Season.Winter:
				sourceRect.Value = new Rectangle(0, 176, 48, 48);
				break;
			}
			break;
		case 3:
		{
			int age = getAge();
			switch (season)
			{
			case Season.Spring:
				sourceRect.Value = new Rectangle(Math.Min(2, age / 10) * 16 + (int)tileSheetOffset * 16, 256, 16, 32);
				break;
			case Season.Summer:
				sourceRect.Value = new Rectangle(64 + Math.Min(2, age / 10) * 16 + (int)tileSheetOffset * 16, 256, 16, 32);
				break;
			case Season.Fall:
				sourceRect.Value = new Rectangle(Math.Min(2, age / 10) * 16 + (int)tileSheetOffset * 16, 288, 16, 32);
				break;
			case Season.Winter:
				sourceRect.Value = new Rectangle(64 + Math.Min(2, age / 10) * 16 + (int)tileSheetOffset * 16, 288, 16, 32);
				break;
			}
			break;
		}
		case 4:
			sourceRect.Value = new Rectangle(tileSheetOffset.Value * 32, 320, 32, 32);
			break;
		}
	}

	/// <summary>Whether this bush is in a greenhouse or indoor pot.</summary>
	public bool IsSheltered()
	{
		if (!Location.SeedsIgnoreSeasonsHere())
		{
			if (inPot.Value)
			{
				return !Location.IsOutdoors;
			}
			return false;
		}
		return true;
	}

	/// <summary>Get whether this bush is in season to produce items, regardless of whether it has any currently.</summary>
	public bool inBloom()
	{
		if ((int)size == 4)
		{
			return tileSheetOffset.Value == 1;
		}
		Season season = Location.GetSeason();
		int dayOfMonth = Game1.dayOfMonth;
		if ((int)size == 3)
		{
			bool inBloom = getAge() >= 20 && dayOfMonth >= 22 && (season != Season.Winter || IsSheltered());
			if (inBloom && Location != null && Location.IsFarm)
			{
				foreach (Farmer allFarmer in Game1.getAllFarmers())
				{
					allFarmer.autoGenerateActiveDialogueEvent("cropMatured_815");
				}
			}
			return inBloom;
		}
		switch (season)
		{
		case Season.Spring:
			if (dayOfMonth > 14)
			{
				return dayOfMonth < 19;
			}
			return false;
		case Season.Fall:
			if (dayOfMonth > 7)
			{
				return dayOfMonth < 12;
			}
			return false;
		default:
			return false;
		}
	}

	public override bool isActionable()
	{
		return true;
	}

	public override void loadSprite()
	{
		Vector2 tilePosition = Tile;
		Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, tilePosition.X, (double)tilePosition.Y * 777.0);
		double extra = ((r.NextDouble() < 0.5) ? 0.0 : ((double)r.Next(6) / 100.0));
		if ((int)size != 4)
		{
			if ((int)size == 1 && (int)tileSheetOffset == 0 && r.NextDouble() < 0.2 + extra && inBloom())
			{
				tileSheetOffset.Value = 1;
			}
			else if (Game1.GetSeasonForLocation(Location) != Season.Summer && !inBloom())
			{
				tileSheetOffset.Value = 0;
			}
		}
		if ((int)size == 3)
		{
			tileSheetOffset.Value = (inBloom() ? 1 : 0);
		}
		setUpSourceRect();
	}

	public override Rectangle getBoundingBox()
	{
		Vector2 tileLocation = Tile;
		switch ((long)size.Value)
		{
		case 0L:
		case 3L:
			return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
		case 1L:
		case 4L:
			return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 128, 64);
		case 2L:
			return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 192, 64);
		default:
			return Rectangle.Empty;
		}
	}

	public override Rectangle getRenderBounds()
	{
		Vector2 tileLocation = Tile;
		switch ((long)size.Value)
		{
		case 0L:
		case 3L:
			return new Rectangle((int)tileLocation.X * 64, (int)(tileLocation.Y - 1f) * 64, 64, 160);
		case 1L:
		case 4L:
			return new Rectangle((int)tileLocation.X * 64, (int)(tileLocation.Y - 2f) * 64, 128, 256);
		case 2L:
			return new Rectangle((int)tileLocation.X * 64, (int)(tileLocation.Y - 2f) * 64, 192, 256);
		default:
			return Rectangle.Empty;
		}
	}

	public override bool performUseAction(Vector2 tileLocation)
	{
		GameLocation location = Location;
		base.NeedsUpdate = true;
		if (Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
		{
			shakeTimer = 0f;
		}
		if (shakeTimer <= 0f)
		{
			Season season = location.GetSeason();
			if (maxShake == 0f && ((int)size != 3 || season != Season.Winter || IsSheltered()))
			{
				location.localSound("leafrustle");
			}
			GameLocation old_location = Location;
			Location = location;
			shake(tileLocation, doEvenIfStillShaking: false);
			Location = old_location;
			shakeTimer = 500f;
		}
		return true;
	}

	public override bool tickUpdate(GameTime time)
	{
		if (shakeTimer > 0f)
		{
			shakeTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if ((int)size == 4)
		{
			uniqueSpawnMutex.Update(Location);
		}
		if (maxShake > 0f)
		{
			if (shakeLeft)
			{
				shakeRotation -= (float)Math.PI / 200f;
				if (shakeRotation <= 0f - maxShake)
				{
					shakeLeft = false;
				}
			}
			else
			{
				shakeRotation += (float)Math.PI / 200f;
				if (shakeRotation >= maxShake)
				{
					shakeLeft = true;
				}
			}
			maxShake = Math.Max(0f, maxShake - 0.0030679617f);
		}
		if (shakeTimer <= 0f && size.Value != 4 && maxShake <= 0f)
		{
			base.NeedsUpdate = false;
		}
		return false;
	}

	public void shake(Vector2 tileLocation, bool doEvenIfStillShaking)
	{
		if (!(maxShake == 0f || doEvenIfStillShaking))
		{
			return;
		}
		shakeLeft = Game1.player.Tile.X > tileLocation.X || (Game1.player.Tile.X == tileLocation.X && Game1.random.NextBool());
		maxShake = (float)Math.PI / 128f;
		base.NeedsUpdate = true;
		if (!townBush && (int)tileSheetOffset == 1 && inBloom())
		{
			string shakeOff = GetShakeOffItem();
			if (shakeOff == null)
			{
				return;
			}
			tileSheetOffset.Value = 0;
			setUpSourceRect();
			switch ((long)size)
			{
			case 4L:
				uniqueSpawnMutex.RequestLock(delegate
				{
					Game1.player.team.MarkCollectedNut("Bush_" + Location.Name + "_" + tileLocation.X + "_" + tileLocation.Y);
					Game1.createItemDebris(ItemRegistry.Create(shakeOff), new Vector2(getBoundingBox().Center.X, getBoundingBox().Bottom - 2), 0, Location, getBoundingBox().Bottom);
				});
				break;
			case 3L:
				Game1.createObjectDebris(shakeOff, (int)tileLocation.X, (int)tileLocation.Y);
				break;
			default:
			{
				int number = Utility.CreateRandom(tileLocation.X, (double)tileLocation.Y * 5000.0, Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed).Next(1, 2) + Game1.player.ForagingLevel / 4;
				for (int i = 0; i < number; i++)
				{
					Item item = ItemRegistry.Create(shakeOff);
					if (Game1.player.professions.Contains(16))
					{
						item.Quality = 4;
					}
					Game1.createItemDebris(item, Utility.PointToVector2(getBoundingBox().Center), Game1.random.Next(1, 4));
				}
				Game1.player.gainExperience(2, number);
				break;
			}
			}
			if ((int)size != 3)
			{
				DelayedAction.playSoundAfterDelay("leafrustle", 100);
			}
		}
		else if (tileLocation.X == 20f && tileLocation.Y == 8f && Game1.dayOfMonth == 28 && Game1.timeOfDay == 1200 && !Game1.player.mailReceived.Contains("junimoPlush"))
		{
			Game1.player.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(F)1733"), junimoPlushCallback);
		}
		else if (Game1.currentLocation is Town town)
		{
			if (tileLocation.X == 28f && tileLocation.Y == 14f && Game1.player.eventsSeen.Contains("520702") && !Game1.player.hasMagnifyingGlass)
			{
				town.initiateMagnifyingGlassGet();
			}
			else if (tileLocation.X == 47f && tileLocation.Y == 100f && Game1.player.secretNotesSeen.Contains(21) && Game1.timeOfDay == 2440 && Game1.player.mailReceived.Add("secretNote21_done"))
			{
				town.initiateMarnieLewisBush();
			}
		}
	}

	/// <summary>Get the qualified or unqualified item ID to produce when the bush is shaken, assuming it's in bloom.</summary>
	public string GetShakeOffItem()
	{
		return size.Value switch
		{
			3 => "(O)815", 
			4 => "(O)73", 
			_ => Location.GetSeason() switch
			{
				Season.Spring => "(O)296", 
				Season.Fall => "(O)410", 
				_ => null, 
			}, 
		};
	}

	public void junimoPlushCallback(Item item, Farmer who)
	{
		if (item?.QualifiedItemId == "(F)1733")
		{
			who?.mailReceived.Add("junimoPlush");
		}
	}

	public override bool isPassable(Character c = null)
	{
		return c is JunimoHarvester;
	}

	public override void dayUpdate()
	{
		GameLocation environment = Location;
		base.NeedsUpdate = true;
		Season season = environment.GetSeason();
		if ((int)size != 4)
		{
			Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, Tile.X, (double)Tile.Y * 777.0);
			double extra = ((r.NextDouble() < 0.5) ? 0.0 : ((double)r.Next(6) / 100.0));
			if ((int)size == 1 && (int)tileSheetOffset == 0 && r.NextDouble() < 0.2 + extra && inBloom())
			{
				tileSheetOffset.Value = 1;
			}
			else if (season != Season.Summer && !inBloom())
			{
				tileSheetOffset.Value = 0;
			}
			if ((int)size == 3)
			{
				tileSheetOffset.Value = (inBloom() ? 1 : 0);
			}
			setUpSourceRect();
			Vector2 tileLocation = Tile;
			if (tileLocation.X != 6f || tileLocation.Y != 7f || !(environment.Name == "Sunroom"))
			{
				health = 0f;
			}
		}
	}

	/// <inheritdoc />
	public override bool seasonUpdate(bool onLoad)
	{
		if ((int)size == 4)
		{
			return false;
		}
		if (!Game1.IsMultiplayer || Game1.IsServer)
		{
			Season season = Location.GetSeason();
			tileSheetOffset.Value = (((int)size == 1 && season == Season.Summer && Game1.random.NextBool()) ? 1 : 0);
			loadSprite();
		}
		return false;
	}

	public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation)
	{
		GameLocation location = Location;
		base.NeedsUpdate = true;
		if ((int)size == 4)
		{
			return false;
		}
		if (explosion > 0)
		{
			shake(tileLocation, doEvenIfStillShaking: true);
			return false;
		}
		if ((int)size == 3 && t is MeleeWeapon { ItemId: "66" })
		{
			shake(tileLocation, doEvenIfStillShaking: true);
		}
		else if (t is Axe axe && isDestroyable())
		{
			location.playSound("leafrustle", tileLocation);
			shake(tileLocation, doEvenIfStillShaking: true);
			if ((int)axe.upgradeLevel >= 1 || (int)size == 3)
			{
				health -= (((int)size == 3) ? 0.5f : ((float)(int)axe.upgradeLevel / 5f));
				if (health <= -1f)
				{
					location.playSound("treethud", tileLocation);
					DelayedAction.playSoundAfterDelay("leafrustle", 100, location, tileLocation);
					Color c = Color.Green;
					Season season = location.GetSeason();
					if (!IsSheltered())
					{
						switch (season)
						{
						case Season.Spring:
							c = Color.Green;
							break;
						case Season.Summer:
							c = Color.ForestGreen;
							break;
						case Season.Fall:
							c = Color.IndianRed;
							break;
						case Season.Winter:
							c = Color.Cyan;
							break;
						}
					}
					if (location.Name == "Sunroom")
					{
						foreach (NPC character in location.characters)
						{
							character.jump();
							character.doEmote(12);
						}
					}
					for (int i = 0; i <= getEffectiveSize(); i++)
					{
						for (int j = 0; j < 12; j++)
						{
							Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(355, 1200 + (season.Equals("fall") ? 16 : (season.Equals("winter") ? (-16) : 0)), 16, 16), Utility.getRandomPositionInThisRectangle(getBoundingBox(), Game1.random) - new Vector2(0f, Game1.random.Next(64)), flipped: false, 0.01f, c)
							{
								motion = new Vector2((float)Game1.random.Next(-10, 11) / 10f, -Game1.random.Next(5, 7)),
								acceleration = new Vector2(0f, (float)Game1.random.Next(13, 17) / 100f),
								accelerationChange = new Vector2(0f, -0.001f),
								scale = 4f,
								layerDepth = (tileLocation.Y + 1f) * 64f / 10000f,
								animationLength = 11,
								totalNumberOfLoops = 99,
								interval = Game1.random.Next(20, 90),
								delayBeforeAnimationStart = (i + 1) * j * 20
							});
							if (j % 6 == 0)
							{
								Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(50, Utility.getRandomPositionInThisRectangle(getBoundingBox(), Game1.random) - new Vector2(32f, Game1.random.Next(32, 64)), c));
								Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, Utility.getRandomPositionInThisRectangle(getBoundingBox(), Game1.random) - new Vector2(32f, Game1.random.Next(32, 64)), Color.White));
							}
						}
					}
					if ((int)size == 3)
					{
						Game1.createItemDebris(ItemRegistry.Create("(O)251"), tileLocation * 64f, 2, location);
					}
					return true;
				}
				location.playSound("axchop", tileLocation);
			}
		}
		return false;
	}

	public bool isDestroyable()
	{
		if ((int)size == 3)
		{
			return true;
		}
		if (Location is Farm)
		{
			Vector2 tile = Tile;
			switch (Game1.whichFarm)
			{
			case 2:
				if (tile.X == 13f && tile.Y == 35f)
				{
					return true;
				}
				if (tile.X == 37f && tile.Y == 9f)
				{
					return true;
				}
				return new Rectangle(43, 11, 34, 50).Contains((int)tile.X, (int)tile.Y);
			case 1:
				return new Rectangle(32, 11, 11, 25).Contains((int)tile.X, (int)tile.Y);
			case 3:
				return new Rectangle(24, 56, 10, 8).Contains((int)tile.X, (int)tile.Y);
			case 6:
				return new Rectangle(20, 44, 36, 44).Contains((int)tile.X, (int)tile.Y);
			}
		}
		return false;
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
	{
		layerDepth += positionOnScreen.X / 100000f;
		spriteBatch.Draw(texture.Value, positionOnScreen + new Vector2(0f, -64f * scale), new Rectangle(32, 96, 16, 32), Color.White, 0f, Vector2.Zero, scale, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + 448f * scale - 1f) / 20000f);
	}

	public override void performPlayerEntryAction()
	{
		base.performPlayerEntryAction();
		Season season = Location.GetSeason();
		if (season != Season.Winter && !Location.IsRainingHere() && Game1.isDarkOut(Location) && Game1.random.NextBool((season == Season.Summer) ? 0.08 : 0.04))
		{
			AmbientLocationSounds.addSound(Tile, 3);
		}
		NetRectangle netRectangle = sourceRect;
		if ((object)netRectangle != null && netRectangle.X < 0)
		{
			setUpSourceRect();
		}
	}

	private int getEffectiveSize()
	{
		return size.Value switch
		{
			3 => 0, 
			4 => 1, 
			_ => size.Value, 
		};
	}

	public void draw(SpriteBatch spriteBatch, float yDrawOffset)
	{
		this.yDrawOffset = yDrawOffset;
		draw(spriteBatch);
	}

	public override void draw(SpriteBatch spriteBatch)
	{
		Vector2 tileLocation = Tile;
		if ((bool)drawShadow)
		{
			if (getEffectiveSize() > 0)
			{
				spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2((tileLocation.X + ((getEffectiveSize() == 1) ? 0.5f : 1f)) * 64f - 51f, tileLocation.Y * 64f - 16f + yDrawOffset)), shadowSourceRect, Color.White, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-06f);
			}
			else
			{
				spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 64f - 4f + yDrawOffset)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 1E-06f);
			}
		}
		spriteBatch.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + (float)((getEffectiveSize() + 1) * 64 / 2), (tileLocation.Y + 1f) * 64f - (float)((getEffectiveSize() > 0 && (!townBush || getEffectiveSize() != 1) && (int)size != 4) ? 64 : 0) + yDrawOffset)), sourceRect.Value, Color.White, shakeRotation, new Vector2((getEffectiveSize() + 1) * 16 / 2, 32f), 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)(getBoundingBox().Center.Y + 48) / 10000f - tileLocation.X / 1000000f);
	}
}
