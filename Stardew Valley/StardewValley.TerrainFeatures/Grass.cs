using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Netcode.Validation;
using StardewValley.Extensions;
using StardewValley.Tools;

namespace StardewValley.TerrainFeatures;

[XmlInclude(typeof(CosmeticPlant))]
[NotImplicitNetField]
public class Grass : TerrainFeature
{
	public const float defaultShakeRate = (float)Math.PI / 80f;

	public const float maximumShake = (float)Math.PI / 8f;

	public const float shakeDecayRate = (float)Math.PI / 350f;

	public const byte springGrass = 1;

	public const byte caveGrass = 2;

	public const byte frostGrass = 3;

	public const byte lavaGrass = 4;

	public const byte caveGrass2 = 5;

	public const byte cobweb = 6;

	public const byte blueGrass = 7;

	/// <summary>The backing field for <see cref="M:StardewValley.TerrainFeatures.Grass.PlayGrassSound" />.</summary>
	public static ICue grassSound;

	[XmlElement("grassType")]
	public readonly NetByte grassType = new NetByte();

	private bool shakeLeft;

	protected float shakeRotation;

	protected float maxShake;

	protected float shakeRate;

	[XmlElement("numberOfWeeds")]
	public readonly NetInt numberOfWeeds = new NetInt();

	[XmlElement("grassSourceOffset")]
	public readonly NetInt grassSourceOffset = new NetInt();

	private int grassBladeHealth = 1;

	[XmlIgnore]
	public Lazy<Texture2D> texture;

	private int[] whichWeed = new int[4];

	private int[] offset1 = new int[4];

	private int[] offset2 = new int[4];

	private int[] offset3 = new int[4];

	private int[] offset4 = new int[4];

	private bool[] flip = new bool[4];

	private double[] shakeRandom = new double[4];

	public Grass()
		: base(needsTick: true)
	{
		texture = new Lazy<Texture2D>(() => Game1.content.Load<Texture2D>(textureName()));
	}

	public Grass(int which, int numberOfWeeds)
		: this()
	{
		grassType.Value = (byte)which;
		loadSprite();
		this.numberOfWeeds.Value = numberOfWeeds;
	}

	public override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(grassType, "grassType").AddField(numberOfWeeds, "numberOfWeeds").AddField(grassSourceOffset, "grassSourceOffset");
	}

	/// <summary>Play the sound of walking through grass, if it's not already playing.</summary>
	public static void PlayGrassSound()
	{
		ICue cue = grassSound;
		if (cue == null || !cue.IsPlaying)
		{
			Game1.playSound("grassyStep", out grassSound);
		}
	}

	public virtual string textureName()
	{
		return "TerrainFeatures\\grass";
	}

	public override bool isPassable(Character c = null)
	{
		return true;
	}

	public override void loadSprite()
	{
		try
		{
			switch (grassType.Value)
			{
			case 1:
				switch (Game1.GetSeasonForLocation(Location))
				{
				case Season.Spring:
					grassSourceOffset.Value = 0;
					break;
				case Season.Summer:
					grassSourceOffset.Value = 20;
					break;
				case Season.Fall:
					grassSourceOffset.Value = 40;
					break;
				case Season.Winter:
					grassSourceOffset.Value = ((Location != null && Location.IsOutdoors) ? 80 : 0);
					break;
				}
				break;
			case 2:
				grassSourceOffset.Value = 60;
				break;
			case 3:
				grassSourceOffset.Value = 80;
				break;
			case 4:
				grassSourceOffset.Value = 100;
				break;
			case 7:
				switch (Game1.GetSeasonForLocation(Location))
				{
				case Season.Spring:
					grassSourceOffset.Value = 160;
					break;
				case Season.Summer:
					grassSourceOffset.Value = 180;
					break;
				case Season.Fall:
					grassSourceOffset.Value = 200;
					break;
				case Season.Winter:
					grassSourceOffset.Value = ((Location != null && Location.IsOutdoors) ? 220 : 160);
					break;
				}
				break;
			default:
				grassSourceOffset.Value = (grassType.Value + 1) * 20;
				break;
			}
		}
		catch
		{
		}
	}

	public override void OnAddedToLocation(GameLocation location, Vector2 tile)
	{
		base.OnAddedToLocation(location, tile);
		loadSprite();
	}

	public override Rectangle getBoundingBox()
	{
		Vector2 tileLocation = Tile;
		return new Rectangle((int)(tileLocation.X * 64f), (int)(tileLocation.Y * 64f), 64, 64);
	}

	public override Rectangle getRenderBounds()
	{
		Vector2 tileLocation = Tile;
		return new Rectangle((int)(tileLocation.X * 64f) - 32, (int)(tileLocation.Y * 64f) - 32, 128, 112);
	}

	public override void doCollisionAction(Rectangle positionOfCollider, int speedOfCollision, Vector2 tileLocation, Character who)
	{
		GameLocation location = Location;
		if (location != Game1.currentLocation)
		{
			return;
		}
		if (speedOfCollision > 0 && maxShake == 0f && positionOfCollider.Intersects(getBoundingBox()))
		{
			if ((who == null || !(who is FarmAnimal)) && Utility.isOnScreen(new Point((int)tileLocation.X, (int)tileLocation.Y), 2, location))
			{
				PlayGrassSound();
			}
			shake((float)Math.PI / 8f / Math.Min(1f, 5f / (float)speedOfCollision), (float)Math.PI / 80f / Math.Min(1f, 5f / (float)speedOfCollision), (float)positionOfCollider.Center.X > tileLocation.X * 64f + 32f);
		}
		if (who is Farmer && Game1.player.CurrentTool is MeleeWeapon { isOnSpecial: not false } weapon && (int)weapon.type == 0 && Math.Abs(shakeRotation) < 0.001f && performToolAction(Game1.player.CurrentTool, -1, tileLocation))
		{
			Game1.currentLocation.terrainFeatures.Remove(tileLocation);
		}
		if (who is Farmer player)
		{
			if (player.stats.Get("Book_Grass") != 0)
			{
				player.temporarySpeedBuff = -0.33f;
			}
			else
			{
				player.temporarySpeedBuff = -1f;
			}
			if (grassType.Value == 6)
			{
				player.temporarySpeedBuff = -3f;
			}
		}
	}

	public bool reduceBy(int number, bool showDebris)
	{
		int grassToDeplete = 0;
		grassBladeHealth -= number;
		if (grassBladeHealth > 0)
		{
			return true;
		}
		if (grassType.Value == 7)
		{
			grassToDeplete = 1 + grassBladeHealth / -2;
			grassBladeHealth = 2 - grassBladeHealth % 2;
		}
		else
		{
			grassBladeHealth = 1;
			grassToDeplete = number;
		}
		numberOfWeeds.Value -= grassToDeplete;
		if (showDebris)
		{
			Vector2 tileLocation = Tile;
			Game1.createRadialDebris(Game1.currentLocation, textureName(), new Rectangle(2, 8 + (int)grassSourceOffset, 8, 8), 1, (int)((tileLocation.X + 1f) * 64f), ((int)tileLocation.Y + 1) * 64, Game1.random.Next(2, 5), (int)tileLocation.Y + 1, Color.White, 4f);
			Game1.createRadialDebris(Game1.currentLocation, textureName(), new Rectangle(2, 8 + (int)grassSourceOffset, 8, 8), 1, (int)((tileLocation.X + 1.1f) * 64f), (int)((tileLocation.Y + 1.1f) * 64f), Game1.random.Next(2, 5), (int)tileLocation.Y + 1, Color.White, 4f);
			Game1.createRadialDebris(Game1.currentLocation, textureName(), new Rectangle(2, 8 + (int)grassSourceOffset, 8, 8), 1, (int)((tileLocation.X + 0.9f) * 64f), (int)((tileLocation.Y + 1.1f) * 64f), Game1.random.Next(2, 5), (int)tileLocation.Y + 1, Color.White, 4f);
			createDestroySprites(Game1.currentLocation, tileLocation);
		}
		return (int)numberOfWeeds <= 0;
	}

	protected void shake(float shake, float rate, bool left)
	{
		maxShake = shake;
		shakeRate = rate;
		shakeRotation = 0f;
		shakeLeft = left;
		base.NeedsUpdate = true;
	}

	public override void performPlayerEntryAction()
	{
		base.performPlayerEntryAction();
		if (shakeRandom[0] == 0.0)
		{
			setUpRandom();
		}
	}

	public override bool tickUpdate(GameTime time)
	{
		if (shakeRandom[0] == 0.0)
		{
			setUpRandom();
		}
		if (maxShake > 0f)
		{
			if (shakeLeft)
			{
				shakeRotation -= shakeRate;
				if (Math.Abs(shakeRotation) >= maxShake)
				{
					shakeLeft = false;
				}
			}
			else
			{
				shakeRotation += shakeRate;
				if (shakeRotation >= maxShake)
				{
					shakeLeft = true;
					shakeRotation -= shakeRate;
				}
			}
			maxShake = Math.Max(0f, maxShake - (float)Math.PI / 350f);
		}
		else
		{
			shakeRotation /= 2f;
			if (shakeRotation <= 0.01f)
			{
				base.NeedsUpdate = false;
				shakeRotation = 0f;
			}
		}
		return false;
	}

	public override void dayUpdate()
	{
		GameLocation environment = Location;
		if (grassType.Value == 1 && (environment.GetSeason() != Season.Winter || environment.HasMapPropertyWithValue("AllowGrassGrowInWinter")) && (int)numberOfWeeds < 4)
		{
			numberOfWeeds.Value = Utility.Clamp(numberOfWeeds.Value + Game1.random.Next(1, 4), 0, 4);
		}
		setUpRandom();
		if (grassType.Value == 7)
		{
			grassBladeHealth = 2;
		}
		else
		{
			grassBladeHealth = 1;
		}
	}

	public void setUpRandom()
	{
		Vector2 tileLocation = Tile;
		Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)Game1.stats.DaysPlayed / 28.0, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0);
		GameLocation location = Location;
		int num;
		if (location == null)
		{
			num = 0;
		}
		else
		{
			location.getTileIndexAt((int)tileLocation.X, (int)tileLocation.Y, "Front");
			num = 1;
		}
		bool front = (byte)num != 0;
		for (int i = 0; i < 4; i++)
		{
			whichWeed[i] = r.Next(3);
			offset1[i] = r.Next(-2, 3);
			offset2[i] = r.Next(-2, 3) + (front ? (-7) : 0);
			offset3[i] = r.Next(-2, 3);
			offset4[i] = r.Next(-2, 3) + (front ? (-7) : 0);
			flip[i] = r.NextBool();
			shakeRandom[i] = r.NextDouble();
		}
	}

	/// <inheritdoc />
	public override bool seasonUpdate(bool onLoad)
	{
		if (grassType.Value == 1 || grassType.Value == 7)
		{
			if (Location.IsOutdoors && Location.IsWinterHere() && Location.HasMapPropertyWithValue("AllowGrassSurviveInWinter") && Location.getMapProperty("AllowGrassSurviveInWinter").ToLower().StartsWith("f") && !onLoad)
			{
				return true;
			}
			loadSprite();
		}
		return false;
	}

	public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation)
	{
		GameLocation location = Location ?? Game1.currentLocation;
		MeleeWeapon weapon = t as MeleeWeapon;
		if ((weapon != null && (int)weapon.type != 2) || explosion > 0)
		{
			if (weapon != null && (int)weapon.type != 1)
			{
				DelayedAction.playSoundAfterDelay("daggerswipe", 50, location, tileLocation);
			}
			else
			{
				location.playSound("swordswipe", tileLocation);
			}
			shake((float)Math.PI * 3f / 32f, (float)Math.PI / 40f, Game1.random.NextBool());
			int numberOfWeedsToDestroy = ((explosion <= 0) ? 1 : Math.Max(1, explosion + 2 - Game1.recentMultiplayerRandom.Next(2)));
			if (weapon != null && t.ItemId == "53")
			{
				numberOfWeedsToDestroy = 2;
			}
			else if (weapon != null && t.ItemId == "66")
			{
				numberOfWeedsToDestroy = 4;
			}
			if (grassType.Value == 6 && Game1.random.NextBool())
			{
				numberOfWeedsToDestroy = 0;
			}
			numberOfWeeds.Value = (int)numberOfWeeds - numberOfWeedsToDestroy;
			createDestroySprites(location, tileLocation);
			return TryDropItemsOnCut(t);
		}
		return false;
	}

	private void createDestroySprites(GameLocation location, Vector2 tileLocation)
	{
		Color c;
		switch (grassType.Value)
		{
		case 1:
			c = location.GetSeason() switch
			{
				Season.Spring => new Color(60, 180, 58), 
				Season.Summer => new Color(110, 190, 24), 
				Season.Fall => new Color(219, 102, 58), 
				Season.Winter => new Color(63, 167, 156), 
				_ => Color.Green, 
			};
			break;
		case 2:
			c = new Color(148, 146, 71);
			break;
		case 3:
			c = new Color(216, 240, 255);
			break;
		case 4:
			c = new Color(165, 93, 58);
			break;
		case 6:
			c = Color.White * 0.6f;
			break;
		case 7:
			switch (location.GetSeason())
			{
			case Season.Spring:
			case Season.Summer:
				c = new Color(0, 178, 174);
				break;
			case Season.Fall:
				c = new Color(129, 80, 148);
				break;
			case Season.Winter:
				c = new Color(40, 125, 178);
				break;
			default:
				c = Color.Green;
				break;
			}
			break;
		default:
			c = Color.Green;
			break;
		}
		Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(28, tileLocation * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), c, 8, Game1.random.NextBool(), Game1.random.Next(60, 100)));
	}

	/// <summary>Drop an item when this grass is cut, if any.</summary>
	/// <param name="tool">The tool used to cut the grass.</param>
	/// <param name="addAnimation">Whether to show animations for the cut grass.</param>
	public bool TryDropItemsOnCut(Tool tool, bool addAnimation = true)
	{
		Vector2 tileLocation = Tile;
		GameLocation location = Location;
		if ((int)numberOfWeeds <= 0)
		{
			if (grassType.Value != 1 && grassType.Value != 7)
			{
				Random grassRandom = (Game1.IsMultiplayer ? Game1.recentMultiplayerRandom : Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)tileLocation.X * 1000.0, (double)tileLocation.Y * 11.0, Game1.CurrentMineLevel, Game1.player.timesReachedMineBottom));
				if (grassRandom.NextDouble() < 0.005)
				{
					Game1.createObjectDebris("(O)114", (int)tileLocation.X, (int)tileLocation.Y, -1, 0, 1f, location);
				}
				else if (grassRandom.NextDouble() < 0.01)
				{
					Game1.createDebris(4, (int)tileLocation.X, (int)tileLocation.Y, grassRandom.Next(1, 2), location);
				}
				else if (grassRandom.NextDouble() < 0.02)
				{
					Game1.createObjectDebris("(O)92", (int)tileLocation.X, (int)tileLocation.Y, grassRandom.Next(2, 4), location);
				}
			}
			else if (tool != null && tool.isScythe())
			{
				Farmer player = tool.getLastFarmerToUse() ?? Game1.player;
				Random obj = (Game1.IsMultiplayer ? Game1.recentMultiplayerRandom : Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)tileLocation.X * 1000.0, (double)tileLocation.Y * 11.0));
				double chance = ((tool.ItemId == "66") ? 1.0 : ((tool.ItemId == "53") ? 0.75 : 0.5));
				if (player.currentLocation.IsWinterHere())
				{
					chance *= 0.33;
				}
				if (obj.NextDouble() < chance)
				{
					int num = ((grassType.Value != 7) ? 1 : 2);
					if (GameLocation.StoreHayInAnySilo(num, Location) == 0)
					{
						if (addAnimation)
						{
							TemporaryAnimatedSprite tmpSprite = new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 178, 16, 16), 750f, 1, 0, player.Position - new Vector2(0f, 128f), flicker: false, flipped: false, player.Position.Y / 10000f, 0.005f, Color.White, 4f, -0.005f, 0f, 0f);
							tmpSprite.motion.Y = -3f + (float)Game1.random.Next(-10, 11) / 100f;
							tmpSprite.acceleration.Y = 0.07f + (float)Game1.random.Next(-10, 11) / 1000f;
							tmpSprite.motion.X = (float)Game1.random.Next(-20, 21) / 10f;
							tmpSprite.layerDepth = 1f - (float)Game1.random.Next(100) / 10000f;
							tmpSprite.delayBeforeAnimationStart = Game1.random.Next(150);
							Game1.multiplayer.broadcastSprites(Location, tmpSprite);
						}
						Game1.addHUDMessage(HUDMessage.ForItemGained(ItemRegistry.Create("(O)178"), num));
					}
				}
			}
			return true;
		}
		return false;
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
	{
		Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, (double)Game1.stats.DaysPlayed / 28.0, (double)positionOnScreen.X * 7.0, (double)positionOnScreen.Y * 11.0);
		for (int i = 0; i < (int)numberOfWeeds; i++)
		{
			int whichWeed = r.Next(3);
			spriteBatch.Draw(position: (i != 4) ? (tileLocation * 64f + new Vector2((float)(i % 2 * 64 / 2 + r.Next(-2, 2) * 4 - 4) + 30f, i / 2 * 64 / 2 + r.Next(-2, 2) * 4 + 40)) : (tileLocation * 64f + new Vector2((float)(16 + r.Next(-2, 2) * 4 - 4) + 30f, 16 + r.Next(-2, 2) * 4 + 40)), texture: texture.Value, sourceRectangle: new Rectangle(whichWeed * 15, grassSourceOffset, 15, 20), color: Color.White, rotation: shakeRotation / (float)(r.NextDouble() + 1.0), origin: Vector2.Zero, scale: scale, effects: SpriteEffects.None, layerDepth: layerDepth + (32f * scale + 300f) / 20000f);
		}
	}

	public override void draw(SpriteBatch spriteBatch)
	{
		Vector2 tileLocation = Tile;
		for (int i = 0; i < (int)numberOfWeeds; i++)
		{
			Vector2 pos = ((i != 4) ? (tileLocation * 64f + new Vector2((float)(i % 2 * 64 / 2 + offset3[i] * 4 - 4) + 30f, i / 2 * 64 / 2 + offset4[i] * 4 + 40)) : (tileLocation * 64f + new Vector2((float)(16 + offset1[i] * 4 - 4) + 30f, 16 + offset2[i] * 4 + 40)));
			spriteBatch.Draw(texture.Value, Game1.GlobalToLocal(Game1.viewport, pos), new Rectangle(whichWeed[i] * 15, grassSourceOffset, 15, 20), Color.White, shakeRotation / (float)(shakeRandom[i] + 1.0), new Vector2(7.5f, 17.5f), 4f, flip[i] ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (pos.Y + 16f - 20f) / 10000f + pos.X / 10000000f);
		}
	}
}
