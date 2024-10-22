using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.Tools;

namespace StardewValley.TerrainFeatures;

[XmlInclude(typeof(GiantCrop))]
public class ResourceClump : TerrainFeature
{
	public const int stumpIndex = 600;

	public const int hollowLogIndex = 602;

	public const int meteoriteIndex = 622;

	public const int boulderIndex = 672;

	public const int mineRock1Index = 752;

	public const int mineRock2Index = 754;

	public const int mineRock3Index = 756;

	public const int mineRock4Index = 758;

	public const int quarryBoulderIndex = 148;

	[XmlElement("width")]
	public readonly NetInt width = new NetInt();

	[XmlElement("height")]
	public readonly NetInt height = new NetInt();

	[XmlElement("parentSheetIndex")]
	public readonly NetInt parentSheetIndex = new NetInt();

	[XmlElement("textureName")]
	public readonly NetString textureName = new NetString();

	[XmlElement("health")]
	public readonly NetFloat health = new NetFloat();

	/// <summary>The backing field for <see cref="P:StardewValley.TerrainFeatures.ResourceClump.Tile" />.</summary>
	[XmlElement("tile")]
	public readonly NetVector2 netTile = new NetVector2();

	[XmlIgnore]
	public float shakeTimer;

	private Texture2D texture;

	private int lastToolHitTicker;

	/// <inheritdoc />
	[XmlIgnore]
	public override Vector2 Tile
	{
		get
		{
			return netTile.Value;
		}
		set
		{
			netTile.Value = value;
		}
	}

	public ResourceClump()
		: base(needsTick: true)
	{
	}

	public ResourceClump(int parentSheetIndex, int width, int height, Vector2 tile, int? health = null, string textureName = null)
		: this()
	{
		this.width.Value = width;
		this.height.Value = height;
		this.parentSheetIndex.Value = parentSheetIndex;
		Tile = tile;
		this.textureName.Value = textureName;
		this.health.Value = health ?? GetDefaultHealth(parentSheetIndex);
		loadSprite();
	}

	public override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(width, "width").AddField(height, "height").AddField(parentSheetIndex, "parentSheetIndex")
			.AddField(health, "health")
			.AddField(netTile, "netTile")
			.AddField(textureName, "textureName");
	}

	protected virtual int GetDefaultHealth(int parentSheetIndex)
	{
		switch (parentSheetIndex)
		{
		case 600:
		case 672:
			return 10;
		case 148:
		case 602:
		case 622:
			return 20;
		case 752:
		case 754:
		case 756:
		case 758:
			return 8;
		default:
			return 1;
		}
	}

	public override bool isPassable(Character c = null)
	{
		return false;
	}

	public override bool performToolAction(Tool t, int damage, Vector2 tileLocation)
	{
		if (t == null || lastToolHitTicker == t.swingTicker)
		{
			return false;
		}
		lastToolHitTicker = t.swingTicker;
		float power = Math.Max(1f, (float)((int)t.upgradeLevel + 1) * 0.75f);
		GameLocation location = Location;
		int radialDebris = 12;
		switch (parentSheetIndex)
		{
		case 600L:
			if (t is Axe && (int)t.upgradeLevel < 1)
			{
				location.playSound("axe", tileLocation);
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13945"));
				Game1.player.jitterStrength = 1f;
				return false;
			}
			if (!(t is Axe))
			{
				return false;
			}
			location.playSound("axchop", tileLocation);
			break;
		case 602L:
			if (t is Axe && (int)t.upgradeLevel < 2)
			{
				location.playSound("axe", tileLocation);
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13948"));
				Game1.player.jitterStrength = 1f;
				return false;
			}
			if (!(t is Axe))
			{
				return false;
			}
			location.playSound("axchop", tileLocation);
			break;
		case 148L:
		case 622L:
			if (t is Pickaxe && (int)t.upgradeLevel < 3)
			{
				location.playSound("clubhit", tileLocation);
				location.playSound("clank", tileLocation);
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13952"));
				Game1.player.jitterStrength = 1f;
				return false;
			}
			if (!(t is Pickaxe))
			{
				return false;
			}
			location.playSound("hammer", tileLocation);
			radialDebris = 14;
			break;
		case 672L:
			if (t is Pickaxe && (int)t.upgradeLevel < 2)
			{
				location.playSound("clubhit", tileLocation);
				location.playSound("clank", tileLocation);
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13956"));
				Game1.player.jitterStrength = 1f;
				return false;
			}
			if (!(t is Pickaxe))
			{
				return false;
			}
			location.playSound("hammer", tileLocation);
			radialDebris = 14;
			break;
		case 752L:
		case 754L:
		case 756L:
		case 758L:
			if (!(t is Pickaxe))
			{
				return false;
			}
			location.playSound("hammer", tileLocation);
			radialDebris = 14;
			shakeTimer = 500f;
			base.NeedsUpdate = true;
			break;
		case 44L:
		case 46L:
			location.playSound((health.Value - power <= 0f) ? "cut" : "weed_cut", tileLocation);
			shakeTimer = 500f;
			radialDebris = 36;
			break;
		}
		health.Value -= power;
		if (t is Axe && t.hasEnchantmentOfType<ShavingEnchantment>() && Game1.random.NextDouble() <= (double)(power / 12f) && ((int)parentSheetIndex == 602 || (int)parentSheetIndex == 600))
		{
			Debris d = new Debris(709, new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), Game1.player.getStandingPosition());
			d.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
			d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
			location.debris.Add(d);
		}
		Game1.createRadialDebris(Game1.currentLocation, radialDebris, (int)tileLocation.X + Game1.random.Next((int)width / 2 + 1), (int)tileLocation.Y + Game1.random.Next((int)height / 2 + 1), Game1.random.Next(4, 9), resource: false);
		if (health.Value <= 0f)
		{
			return destroy(t, location, tileLocation);
		}
		shakeTimer = 100f;
		base.NeedsUpdate = true;
		return false;
	}

	public bool destroy(Tool t, GameLocation location, Vector2 tileLocation)
	{
		if (t != null && location.HasUnlockedAreaSecretNotes(t.getLastFarmerToUse()) && Game1.random.NextDouble() < 0.05)
		{
			Object o = location.tryToCreateUnseenSecretNote(t.getLastFarmerToUse());
			if (o != null)
			{
				Game1.createItemDebris(o, tileLocation * 64f + new Vector2((int)width / 2, (int)height / 2) * 64f, -1, location);
			}
		}
		switch (parentSheetIndex)
		{
		case 600L:
		case 602L:
		{
			if (t == null)
			{
				return false;
			}
			if (t.getLastFarmerToUse() == Game1.player)
			{
				Game1.stats.StumpsChopped++;
			}
			t.getLastFarmerToUse().gainExperience(2, 25);
			int numChunks = (((int)parentSheetIndex == 602) ? 8 : 2);
			Random r;
			if (Game1.IsMultiplayer)
			{
				Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, tileLocation.Y);
				r = Game1.recentMultiplayerRandom;
			}
			else
			{
				r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0);
			}
			if (t.getLastFarmerToUse().professions.Contains(12))
			{
				if (numChunks == 8)
				{
					numChunks = 10;
				}
				else if (r.NextBool())
				{
					numChunks++;
				}
			}
			Item hardwood = ItemRegistry.Create("(O)709", numChunks);
			if (Game1.IsMultiplayer)
			{
				Game1.createMultipleItemDebris(hardwood, tileLocation * 64f + new Vector2((float)(int)width / 4f, (float)(int)height / 4f) * 64f, -1, Game1.currentLocation);
			}
			else
			{
				Game1.createMultipleItemDebris(hardwood, tileLocation * 64f + new Vector2((float)(int)width / 4f, (float)(int)height / 4f) * 64f, -1, Game1.currentLocation);
			}
			location.playSound("stumpCrack", tileLocation);
			Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(23, tileLocation * 64f, Color.White, 4, flipped: false, 140f, 0, 128, -1f, 128));
			Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(385, 1522, 127, 79), 2000f, 1, 1, tileLocation * 64f + new Vector2(0f, 49f), flicker: false, flipped: false, 1E-05f, 0.016f, Color.White, 1f, 0f, 0f, 0f));
			Game1.createRadialDebris(Game1.currentLocation, 34, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(4, 9), resource: false);
			if (r.NextDouble() < 0.1)
			{
				Game1.createMultipleObjectDebris("(O)292", (int)tileLocation.X, (int)tileLocation.Y, 1);
			}
			if (Game1.random.NextDouble() <= 0.25 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
			{
				Game1.createObjectDebris("(O)890", (int)tileLocation.X, (int)tileLocation.Y, (int)tileLocation.Y, 0, 1f, location);
			}
			return true;
		}
		case 148L:
		case 672L:
		case 752L:
		case 754L:
		case 756L:
		case 758L:
		{
			if (t == null)
			{
				return false;
			}
			int numChunks = (((int)parentSheetIndex == 672) ? 15 : 10);
			if (Game1.IsMultiplayer)
			{
				Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, tileLocation.Y);
				Game1.createMultipleObjectDebris("(O)390", (int)tileLocation.X, (int)tileLocation.Y, numChunks, t.getLastFarmerToUse().UniqueMultiplayerID);
			}
			else
			{
				Game1.createRadialDebris(Game1.currentLocation, 390, (int)tileLocation.X, (int)tileLocation.Y, numChunks, resource: false, -1, item: true);
			}
			location.playSound("boulderBreak", tileLocation);
			Game1.createRadialDebris(Game1.currentLocation, 32, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(6, 12), resource: false);
			Color c = Color.White;
			switch (parentSheetIndex)
			{
			case 752L:
				c = new Color(188, 119, 98);
				break;
			case 754L:
				c = new Color(168, 120, 95);
				break;
			case 756L:
			case 758L:
				c = new Color(67, 189, 238);
				break;
			}
			Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(48, tileLocation * 64f, c, 5, flipped: false, 180f, 0, 128, -1f, 128)
			{
				alphaFade = 0.01f
			});
			return true;
		}
		case 622L:
			if (t == null)
			{
				return false;
			}
			if (Game1.IsMultiplayer)
			{
				Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, tileLocation.Y);
				Random random = new Random((int)Game1.uniqueIDForThisGame + (int)tileLocation.X + (int)tileLocation.Y * 983728);
				Game1.createMultipleObjectDebris("(O)386", (int)tileLocation.X, (int)tileLocation.Y, 10, t.getLastFarmerToUse().UniqueMultiplayerID);
				Game1.createMultipleObjectDebris("(O)390", (int)tileLocation.X, (int)tileLocation.Y, 8, t.getLastFarmerToUse().UniqueMultiplayerID);
				Game1.createMultipleObjectDebris("(O)749", (int)tileLocation.X, (int)tileLocation.Y, 2, t.getLastFarmerToUse().UniqueMultiplayerID);
				if (random.NextDouble() < 0.25)
				{
					Game1.createMultipleItemDebris(ItemRegistry.Create("(O)74"), tileLocation * 64f + new Vector2((float)(int)width / 4f, (float)(int)height / 4f) * 64f, -1, Game1.currentLocation);
				}
			}
			else
			{
				Game1.createMultipleItemDebris(ItemRegistry.Create("(O)386", 10), tileLocation * 64f + new Vector2((float)(int)width / 4f, (float)(int)height / 4f) * 64f, -1, Game1.currentLocation);
				Game1.createMultipleItemDebris(ItemRegistry.Create("(O)390", 8), tileLocation * 64f + new Vector2((float)(int)width / 4f, (float)(int)height / 4f) * 64f, -1, Game1.currentLocation);
				Game1.createMultipleObjectDebris("(O)749", (int)tileLocation.X, (int)tileLocation.Y, 2);
				if (new Random((int)Game1.uniqueIDForThisGame + (int)tileLocation.X + (int)tileLocation.Y * 983728).NextDouble() < 0.25)
				{
					Game1.createMultipleItemDebris(ItemRegistry.Create("(O)74"), tileLocation * 64f + new Vector2((float)(int)width / 4f, (float)(int)height / 4f) * 64f, -1, Game1.currentLocation);
				}
			}
			location.playSound("boulderBreak", tileLocation);
			Game1.createRadialDebris(Game1.currentLocation, 32, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(6, 12), resource: false);
			Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(5, tileLocation * 64f, Color.White));
			Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(1f, 0f)) * 64f, Color.White, 8, flipped: false, 110f));
			Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(1f, 1f)) * 64f, Color.White, 8, flipped: true, 80f));
			Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(5, (tileLocation + new Vector2(0f, 1f)) * 64f, Color.White, 8, flipped: false, 90f));
			Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(5, tileLocation * 64f + new Vector2(32f, 32f), Color.White, 8, flipped: false, 70f));
			return true;
		case 44L:
		case 46L:
		{
			Color col = Color.Green;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 0; y < 2; y++)
				{
					Vector2 tile = tileLocation + new Vector2(x, y);
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(50, tile * 64f, col));
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(50, tile * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), col * 0.75f)
					{
						scale = 0.75f,
						flipped = true
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(50, tile * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), col * 0.75f)
					{
						scale = 0.75f,
						delayBeforeAnimationStart = 50
					});
					Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(50, tile * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), col * 0.75f)
					{
						scale = 0.75f,
						flipped = true,
						delayBeforeAnimationStart = 100
					});
				}
			}
			t?.getLastFarmerToUse().gainExperience(2, 15);
			Random ran = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0);
			Game1.createMultipleItemDebris(ItemRegistry.Create("(O)Moss", ran.Next(2, 4)), tileLocation * 64f + new Vector2((float)(int)width / 4f, (float)(int)height / 4f) * 64f, -1, Game1.currentLocation);
			Game1.createMultipleItemDebris(ItemRegistry.Create("(O)771", ran.Next(2, 4)), tileLocation * 64f + new Vector2((float)(int)width / 3f, (float)(int)height / 3f) * 64f, -1, Game1.currentLocation);
			if (ran.NextDouble() < 0.05)
			{
				Game1.createMultipleItemDebris(ItemRegistry.Create("(O)MossySeed"), tileLocation * 64f + new Vector2((float)(int)width / 4f, (float)(int)height / 4f) * 64f, -1, Game1.currentLocation);
			}
			return true;
		}
		default:
			return false;
		}
	}

	public override Rectangle getBoundingBox()
	{
		Vector2 tileLocation = Tile;
		return new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, (int)width * 64, (int)height * 64);
	}

	public bool occupiesTile(int x, int y)
	{
		Vector2 tile = Tile;
		if ((float)x >= tile.X && (float)x - tile.X < (float)(int)width && (float)y >= tile.Y)
		{
			return (float)y - tile.Y < (float)(int)height;
		}
		return false;
	}

	public override void draw(SpriteBatch spriteBatch)
	{
		if (texture == null)
		{
			loadSprite();
		}
		Rectangle sourceRect = Game1.getSourceRectForStandardTileSheet(texture, parentSheetIndex, 16, 16);
		sourceRect.Width = (int)width * 16;
		sourceRect.Height = (int)height * 16;
		Vector2 tile = Tile;
		Vector2 position = tile * 64f;
		if (shakeTimer > 0f)
		{
			position.X += (float)Math.Sin(Math.PI * 2.0 / (double)shakeTimer) * 4f;
		}
		spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, position), sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (tile.Y + 1f) * 64f / 10000f + tile.X / 100000f);
	}

	public override void loadSprite()
	{
		texture = ((textureName.Value != null) ? Game1.content.Load<Texture2D>(textureName.Value) : Game1.objectSpriteSheet);
	}

	public override bool performUseAction(Vector2 tileLocation)
	{
		if (!Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
		{
			Game1.haltAfterCheck = false;
			return false;
		}
		switch (parentSheetIndex)
		{
		case 602L:
			Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13962")));
			return true;
		case 672L:
			Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13963")));
			return true;
		case 622L:
			Game1.drawObjectDialogue(Game1.parseText(Game1.content.LoadString("Strings\\StringsFromCSFiles:ResourceClump.cs.13964")));
			return true;
		default:
			return false;
		}
	}

	public override bool tickUpdate(GameTime time)
	{
		if (shakeTimer > 0f)
		{
			shakeTimer -= time.ElapsedGameTime.Milliseconds;
		}
		else
		{
			base.NeedsUpdate = false;
		}
		return false;
	}
}
