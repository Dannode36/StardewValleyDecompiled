using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewValley.Monsters;

public class Bat : Monster
{
	public const float rotationIncrement = (float)Math.PI / 64f;

	private readonly NetInt wasHitCounter = new NetInt(0);

	private float targetRotation;

	private readonly NetBool turningRight = new NetBool();

	private readonly NetBool seenPlayer = new NetBool();

	public readonly NetBool cursedDoll = new NetBool();

	public readonly NetBool hauntedSkull = new NetBool();

	public readonly NetBool magmaSprite = new NetBool();

	public readonly NetBool canLunge = new NetBool();

	private ICue batFlap;

	private float extraVelocity;

	private float maxSpeed = 5f;

	public int lungeFrequency = 3000;

	public int lungeChargeTime = 500;

	public int lungeSpeed = 30;

	public int lungeDecelerationTicks = 60;

	public int nextLunge = -1;

	public int lungeTimer;

	public Vector2 lungeVelocity = Vector2.Zero;

	private List<Vector2> previousPositions = new List<Vector2>();

	public Bat()
	{
	}

	public Bat(Vector2 position)
		: base("Bat", position)
	{
		base.Slipperiness = 24 + Game1.random.Next(-10, 11);
		Halt();
		base.IsWalkingTowardPlayer = false;
		base.HideShadow = true;
	}

	public Bat(Vector2 position, int mineLevel)
		: base("Bat", position)
	{
		base.Slipperiness = 20 + Game1.random.Next(-5, 6);
		switch (mineLevel)
		{
		case 77377:
			parseMonsterInfo("Lava Bat");
			base.Name = "Haunted Skull";
			reloadSprite();
			extraVelocity = 1f;
			extraVelocity = 3f;
			maxSpeed = 8f;
			shakeTimer = 100;
			cursedDoll.Value = true;
			hauntedSkull.Value = true;
			objectsToDrop.Clear();
			break;
		case -555:
			parseMonsterInfo("Magma Sprite");
			base.Name = "Magma Sprite";
			reloadSprite();
			base.Slipperiness *= 2;
			extraVelocity = 2f;
			maxSpeed = Game1.random.Next(6, 9);
			shakeTimer = 100;
			cursedDoll.Value = true;
			magmaSprite.Value = true;
			break;
		case -556:
			parseMonsterInfo("Magma Sparker");
			base.Name = "Magma Sparker";
			reloadSprite();
			extraVelocity = 2f;
			base.Slipperiness += 3;
			maxSpeed = Game1.random.Next(6, 8);
			shakeTimer = 100;
			cursedDoll.Value = true;
			magmaSprite.Value = true;
			canLunge.Value = true;
			break;
		case -789:
			parseMonsterInfo("Iridium Bat");
			reloadSprite();
			extraVelocity = 1f;
			extraVelocity = 3f;
			maxSpeed = 4f;
			base.Health *= 2;
			shakeTimer = 100;
			cursedDoll.Value = true;
			objectsToDrop.Clear();
			base.Age = 789;
			break;
		case -666:
			parseMonsterInfo("Iridium Bat");
			reloadSprite();
			extraVelocity = 1f;
			extraVelocity = 3f;
			maxSpeed = 8f;
			base.Health *= 2;
			shakeTimer = 100;
			cursedDoll.Value = true;
			objectsToDrop.Clear();
			break;
		default:
			if (mineLevel >= 40 && mineLevel < 80)
			{
				base.Name = "Frost Bat";
				parseMonsterInfo("Frost Bat");
				reloadSprite();
			}
			else if (mineLevel >= 80 && mineLevel < 171)
			{
				base.Name = "Lava Bat";
				parseMonsterInfo("Lava Bat");
				reloadSprite();
			}
			else if (mineLevel >= 171)
			{
				base.Name = "Iridium Bat";
				parseMonsterInfo("Iridium Bat");
				reloadSprite();
				extraVelocity = 1f;
			}
			break;
		}
		if (mineLevel > 999)
		{
			extraVelocity = 3f;
			maxSpeed = 8f;
			base.Health *= 2;
			shakeTimer = 999999;
		}
		if (canLunge.Value)
		{
			nextLunge = lungeFrequency;
		}
		Halt();
		base.IsWalkingTowardPlayer = false;
		base.HideShadow = true;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(wasHitCounter, "wasHitCounter").AddField(turningRight, "turningRight").AddField(seenPlayer, "seenPlayer")
			.AddField(cursedDoll, "cursedDoll")
			.AddField(hauntedSkull, "hauntedSkull")
			.AddField(magmaSprite, "magmaSprite")
			.AddField(canLunge, "canLunge");
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		if (Sprite == null)
		{
			Sprite = new AnimatedSprite("Characters\\Monsters\\" + base.Name);
		}
		else
		{
			Sprite.textureName.Value = "Characters\\Monsters\\" + base.Name;
		}
		base.HideShadow = true;
	}

	public override Debris ModifyMonsterLoot(Debris debris)
	{
		if (debris != null && magmaSprite.Value)
		{
			debris.chunksMoveTowardPlayer = true;
		}
		return debris;
	}

	public override List<Item> getExtraDropItems()
	{
		List<Item> extraDrops = new List<Item>();
		if ((bool)cursedDoll && Game1.random.NextDouble() < 0.1429 && (bool)hauntedSkull)
		{
			switch (Game1.random.Next(11))
			{
			case 0:
				switch (Game1.random.Next(6))
				{
				case 0:
				{
					Clothing v = ItemRegistry.Create<Clothing>("(P)10");
					v.clothesColor.Value = Color.DimGray;
					extraDrops.Add(v);
					break;
				}
				case 1:
					extraDrops.Add(ItemRegistry.Create<Clothing>("(S)1004"));
					break;
				case 2:
					extraDrops.Add(ItemRegistry.Create<Clothing>("(S)1014"));
					break;
				case 3:
					extraDrops.Add(ItemRegistry.Create<Clothing>("(S)1263"));
					break;
				case 4:
					extraDrops.Add(ItemRegistry.Create<Clothing>("(S)1262"));
					break;
				case 5:
				{
					Clothing v = ItemRegistry.Create<Clothing>("(P)12");
					v.clothesColor.Value = Color.DimGray;
					extraDrops.Add(v);
					break;
				}
				}
				break;
			case 1:
			{
				MeleeWeapon weapon = ItemRegistry.Create<MeleeWeapon>("(W)2");
				weapon.AddEnchantment(new VampiricEnchantment());
				extraDrops.Add(weapon);
				break;
			}
			case 2:
				extraDrops.Add(ItemRegistry.Create("(O)288"));
				break;
			case 3:
				extraDrops.Add(new Ring("534"));
				break;
			case 4:
				extraDrops.Add(new Ring("531"));
				break;
			case 5:
				do
				{
					extraDrops.Add(ItemRegistry.Create("(O)768"));
					extraDrops.Add(ItemRegistry.Create("(O)769"));
				}
				while (Game1.random.NextDouble() < 0.33);
				break;
			case 6:
				extraDrops.Add(ItemRegistry.Create("(O)581"));
				break;
			case 7:
				extraDrops.Add(ItemRegistry.Create("(O)582"));
				break;
			case 8:
				extraDrops.Add(ItemRegistry.Create("(O)725"));
				break;
			case 9:
				extraDrops.Add(ItemRegistry.Create("(O)86"));
				break;
			case 10:
				if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccVault"))
				{
					extraDrops.Add(ItemRegistry.Create("(O)275"));
				}
				else
				{
					extraDrops.Add(ItemRegistry.Create("(O)749"));
				}
				break;
			}
			return extraDrops;
		}
		if ((bool)hauntedSkull && Game1.random.NextDouble() < 0.25 && Game1.IsWinter)
		{
			do
			{
				extraDrops.Add(ItemRegistry.Create("(O)273"));
			}
			while (Game1.random.NextDouble() < 0.4);
		}
		if ((bool)hauntedSkull && Game1.random.NextDouble() < 0.01)
		{
			extraDrops.Add(ItemRegistry.Create("(M)CursedMannequin" + ((Game1.random.NextDouble() < 0.5) ? "Male" : "Female")));
		}
		if ((bool)hauntedSkull && Game1.random.NextDouble() < 0.001502)
		{
			extraDrops.Add(ItemRegistry.Create("(O)279"));
		}
		if (extraDrops.Count > 0)
		{
			return extraDrops;
		}
		return base.getExtraDropItems();
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		if (base.Age == 789)
		{
			return -1;
		}
		lungeVelocity = Vector2.Zero;
		if (lungeTimer > 0)
		{
			nextLunge = lungeFrequency;
			lungeTimer = 0;
		}
		else if (nextLunge < 1000)
		{
			nextLunge = 1000;
		}
		int actualDamage = Math.Max(1, damage - (int)resilience);
		seenPlayer.Value = true;
		if (Game1.random.NextDouble() < missChance.Value - missChance.Value * addedPrecision)
		{
			actualDamage = -1;
		}
		else
		{
			base.Health -= actualDamage;
			setTrajectory(xTrajectory / 3, yTrajectory / 3);
			wasHitCounter.Value = 500;
			if ((bool)magmaSprite)
			{
				base.currentLocation.playSound("magma_sprite_hit");
			}
			else
			{
				base.currentLocation.playSound("hitEnemy");
			}
			if (base.Health <= 0)
			{
				deathAnimation();
				if (!magmaSprite)
				{
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position, Color.DarkMagenta, 10));
				}
				if ((bool)cursedDoll)
				{
					Vector2 pixelPosition = base.Position;
					if ((bool)magmaSprite)
					{
						base.currentLocation.playSound("magma_sprite_die");
						for (int i = 0; i < 20; i++)
						{
							base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Monsters\\Magma Sprite", new Rectangle(0, 64, 8, 8), pixelPosition + new Vector2(Game1.random.Next(64), Game1.random.Next(64)), flipped: false, 0f, Color.White)
							{
								scale = 4f,
								scaleChange = 0f,
								motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, -6f),
								acceleration = new Vector2(0f, 0.25f),
								layerDepth = 0.9f,
								animationLength = 6,
								totalNumberOfLoops = 2,
								interval = 60f,
								delayBeforeAnimationStart = i * 10
							});
						}
						Utility.addSmokePuff(base.currentLocation, pixelPosition, 0, 4f, 0.01f, 1f, 0.01f);
						Utility.addSmokePuff(base.currentLocation, pixelPosition + new Vector2(32f, 16f), 400, 4f, 0.01f, 1f, 0.02f);
						Utility.addSmokePuff(base.currentLocation, pixelPosition + new Vector2(-32f, -16f), 200, 4f, 0.01f, 1f, 0.02f);
						Utility.addSmokePuff(base.currentLocation, pixelPosition + new Vector2(0f, 32f), 200, 4f, 0.01f, 1f, 0.01f);
						Utility.addSmokePuff(base.currentLocation, pixelPosition, 0, 3f, 0.01f, 1f, 0.02f);
						Utility.addSmokePuff(base.currentLocation, pixelPosition + new Vector2(21f, 16f), 500, 3f, 0.01f, 1f, 0.01f);
						Utility.addSmokePuff(base.currentLocation, pixelPosition + new Vector2(-32f, -21f), 100, 3f, 0.01f, 1f, 0.02f);
						Utility.addSmokePuff(base.currentLocation, pixelPosition + new Vector2(0f, 32f), 250, 3f, 0.01f, 1f, 0.01f);
					}
					else
					{
						base.currentLocation.playSound("rockGolemHit");
					}
					if ((bool)hauntedSkull)
					{
						Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite(Sprite.textureName, new Rectangle(0, 32, 16, 16), 2000f, 1, 9999, pixelPosition, flicker: false, flipped: false, 1f, 0.02f, Color.White, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2((float)xTrajectory / 4f, Game1.random.Next(-12, -7)),
							acceleration = new Vector2(0f, 0.4f),
							rotationChange = (float)Game1.random.Next(-200, 200) / 1000f
						});
					}
					else if (who != null && !magmaSprite)
					{
						Game1.multiplayer.broadcastSprites(who.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(388, 1894, 24, 22), 40f, 6, 9999, pixelPosition, flicker: false, flipped: true, 1f, 0f, Color.Black * 0.67f, 4f, 0f, 0f, 0f)
						{
							motion = new Vector2(8f, -4f)
						});
					}
				}
				else
				{
					base.currentLocation.playSound("batScreech");
				}
			}
		}
		addedSpeed = Game1.random.Next(-1, 1);
		return actualDamage;
	}

	public override void shedChunks(int number, float scale)
	{
		Point standingPixel = base.StandingPixel;
		if ((bool)cursedDoll && (bool)hauntedSkull)
		{
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(0, 64, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, 4f);
		}
		else
		{
			Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(0, 384, 64, 64), 32, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, scale);
		}
	}

	public override void onDealContactDamage(Farmer who)
	{
		base.onDealContactDamage(who);
		if (magmaSprite.Value && Game1.random.NextDouble() < 0.5 && name.Equals("Magma Sparker") && Game1.random.Next(11) >= who.Immunity && !who.hasBuff("28") && !who.hasTrinketWithID("BasiliskPaw"))
		{
			who.applyBuff("12");
		}
	}

	public override void drawAboveAllLayers(SpriteBatch b)
	{
		if (!Utility.isOnScreen(base.Position, 128))
		{
			return;
		}
		if ((bool)cursedDoll)
		{
			if ((bool)hauntedSkull)
			{
				Vector2 pos_offset = Vector2.Zero;
				if (previousPositions.Count > 2)
				{
					pos_offset = base.Position - previousPositions[1];
				}
				int direction = ((!(Math.Abs(pos_offset.X) > Math.Abs(pos_offset.Y))) ? ((!(pos_offset.Y < 0f)) ? 2 : 0) : ((pos_offset.X > 0f) ? 1 : 3));
				if (direction == -1)
				{
					direction = 2;
				}
				Vector2 offset = new Vector2(0f, 8f * (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / (Math.PI * 60.0)));
				b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + offset.Y / 20f, SpriteEffects.None, 0.0001f);
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32 + Game1.random.Next(-6, 7), 32 + Game1.random.Next(-6, 7)) + offset, Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 2 + (((bool)seenPlayer && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), 16, 16), Color.Red * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f - 1f) / 10000f);
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32 + Game1.random.Next(-6, 7), 32 + Game1.random.Next(-6, 7)) + offset, Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 2 + (((bool)seenPlayer && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), 16, 16), Color.Yellow * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f) / 10000f);
				if ((bool)seenPlayer)
				{
					for (int i = previousPositions.Count - 1; i >= 0; i -= 2)
					{
						b.Draw(Sprite.Texture, new Vector2(previousPositions[i].X - (float)Game1.viewport.X, previousPositions[i].Y - (float)Game1.viewport.Y + (float)yJumpOffset) + drawOffset + new Vector2(32f, 32f) + offset, Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 2 + (((bool)seenPlayer && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), 16, 16), Color.White * (0f + 0.125f * (float)i), 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f - (float)i) / 10000f);
					}
				}
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 32f) + offset, Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 2 + (((bool)seenPlayer && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 500.0 < 250.0) ? 1 : 0), 16, 16), Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f + 1f) / 10000f);
			}
			else if ((bool)magmaSprite)
			{
				Vector2 pos_offset = Vector2.Zero;
				if (previousPositions.Count > 2)
				{
					pos_offset = base.Position - previousPositions[1];
				}
				int direction = ((!(Math.Abs(pos_offset.X) > Math.Abs(pos_offset.Y))) ? ((!(pos_offset.Y < 0f)) ? 2 : 0) : ((pos_offset.X > 0f) ? 1 : 3));
				if (direction == -1)
				{
					direction = 2;
				}
				Vector2 offset = new Vector2(0f, 8f * (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / (Math.PI * 60.0)));
				b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + offset.Y / 20f, SpriteEffects.None, 0.0001f);
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32 + Game1.random.Next(-6, 7), 32 + Game1.random.Next(-6, 7)) + offset, Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 7 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 490.0 / 70.0), 16, 16), Color.Red * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.9955f);
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32 + Game1.random.Next(-6, 7), 32 + Game1.random.Next(-6, 7)) + offset, Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 7 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 490.0 / 70.0), 16, 16), Color.Yellow * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.9975f);
				for (int i = previousPositions.Count - 1; i >= 0; i -= 2)
				{
					b.Draw(Sprite.Texture, new Vector2(previousPositions[i].X - (float)Game1.viewport.X, previousPositions[i].Y - (float)Game1.viewport.Y + (float)yJumpOffset) + drawOffset + new Vector2(32f, 32f) + offset, Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 7 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 490.0 / 70.0), 16, 16), Color.White * (0f + 0.125f * (float)i), 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.9985f);
				}
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 32f) + offset, Game1.getSourceRectForStandardTileSheet(Sprite.Texture, direction * 7 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 490.0 / 70.0), 16, 16), Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1f);
			}
			else
			{
				int index = 103;
				if (base.Age == 789)
				{
					index = 789;
				}
				Vector2 offset = new Vector2(0f, 8f * (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / (Math.PI * 60.0)));
				b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + offset.Y / 20f, SpriteEffects.None, 0.0001f);
				b.Draw(Game1.objectSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(32 + Game1.random.Next(-6, 7), 32 + Game1.random.Next(-6, 7)) + offset, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, index, 16, 16), Color.Violet * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f - 1f) / 10000f);
				b.Draw(Game1.objectSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(32 + Game1.random.Next(-6, 7), 32 + Game1.random.Next(-6, 7)) + offset, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, index, 16, 16), Color.Lime * 0.44f, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f) / 10000f);
				b.Draw(Game1.objectSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(32f, 32f) + offset, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, index, 16, 16), (index == 789) ? Color.White : new Color(255, 50, 50), 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (position.Y + 128f + 1f) / 10000f);
			}
		}
		else
		{
			int standingY = base.StandingPixel.Y;
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 32f), Sprite.SourceRect, (shakeTimer > 0) ? Color.Red : Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0.92f);
			b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, base.wildernessFarmMonster ? 0.0001f : ((float)(standingY - 1) / 10000f));
			if (isGlowing)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 32f), Sprite.SourceRect, glowingColor * glowingTransparency, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.99f : ((float)standingY / 10000f + 0.001f)));
			}
		}
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		base.behaviorAtGameTick(time);
		if ((int)wasHitCounter >= 0)
		{
			wasHitCounter.Value -= time.ElapsedGameTime.Milliseconds;
		}
		if (double.IsNaN(xVelocity) || double.IsNaN(yVelocity) || base.Position.X < -2000f || base.Position.Y < -2000f)
		{
			base.Health = -500;
		}
		if (base.Position.X <= -640f || base.Position.Y <= -640f || base.Position.X >= (float)(base.currentLocation.Map.Layers[0].LayerWidth * 64 + 640) || base.Position.Y >= (float)(base.currentLocation.Map.Layers[0].LayerHeight * 64 + 640))
		{
			base.Health = -500;
		}
		if (canLunge.Value)
		{
			if (nextLunge == -1)
			{
				nextLunge = lungeFrequency;
			}
			if (lungeVelocity.LengthSquared() > 0f)
			{
				float decel = (float)lungeSpeed / (float)lungeDecelerationTicks;
				lungeVelocity = new Vector2(Utility.MoveTowards(lungeVelocity.X, 0f, decel), Utility.MoveTowards(lungeVelocity.Y, 0f, decel));
				xVelocity = lungeVelocity.X;
				yVelocity = 0f - lungeVelocity.Y;
				if (lungeVelocity.LengthSquared() == 0f)
				{
					xVelocity = 0f;
					yVelocity = 0f;
				}
				return;
			}
			if (lungeTimer > 0)
			{
				lungeTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
				Vector2 offset = Utility.PointToVector2(base.Player.StandingPixel) - Utility.PointToVector2(base.StandingPixel);
				if (offset.LengthSquared() == 0f)
				{
					offset = new Vector2(1f, 0f);
				}
				offset.Normalize();
				if (lungeTimer < 0)
				{
					lungeVelocity = offset * 25f;
					lungeTimer = 0;
					nextLunge = lungeFrequency;
				}
				xVelocity = offset.X * 0.5f;
				yVelocity = (0f - offset.Y) * 0.5f;
			}
			else if (nextLunge > 0 && withinPlayerThreshold(6))
			{
				nextLunge -= (int)time.ElapsedGameTime.TotalMilliseconds;
				if (nextLunge < 0)
				{
					base.currentLocation.playSound("magma_sprite_spot");
					nextLunge = 0;
					lungeTimer = lungeChargeTime;
					return;
				}
			}
		}
		if (!base.focusedOnFarmers && !withinPlayerThreshold(6) && !seenPlayer)
		{
			return;
		}
		if ((bool)magmaSprite && !seenPlayer)
		{
			base.currentLocation.playSound("magma_sprite_spot");
		}
		seenPlayer.Value = true;
		if (invincibleCountdown > 0)
		{
			if (base.Name.Equals("Lava Bat"))
			{
				glowingColor = Color.Cyan;
			}
			return;
		}
		Point monsterPixel = base.StandingPixel;
		Point standingPixel = base.Player.StandingPixel;
		float xSlope = -(standingPixel.X - monsterPixel.X);
		float ySlope = standingPixel.Y - monsterPixel.Y;
		float t = Math.Max(1f, Math.Abs(xSlope) + Math.Abs(ySlope));
		if (t < (float)((extraVelocity > 0f) ? 192 : 64))
		{
			xVelocity = Math.Max(0f - maxSpeed, Math.Min(maxSpeed, xVelocity * 1.05f));
			yVelocity = Math.Max(0f - maxSpeed, Math.Min(maxSpeed, yVelocity * 1.05f));
		}
		xSlope /= t;
		ySlope /= t;
		if ((int)wasHitCounter <= 0)
		{
			targetRotation = (float)Math.Atan2(0f - ySlope, xSlope) - (float)Math.PI / 2f;
			if ((double)(Math.Abs(targetRotation) - Math.Abs(rotation)) > Math.PI * 7.0 / 8.0 && Game1.random.NextBool())
			{
				turningRight.Value = true;
			}
			else if ((double)(Math.Abs(targetRotation) - Math.Abs(rotation)) < Math.PI / 8.0)
			{
				turningRight.Value = false;
			}
			if ((bool)turningRight)
			{
				rotation -= (float)Math.Sign(targetRotation - rotation) * ((float)Math.PI / 64f);
			}
			else
			{
				rotation += (float)Math.Sign(targetRotation - rotation) * ((float)Math.PI / 64f);
			}
			rotation %= (float)Math.PI * 2f;
			wasHitCounter.Value = 0;
		}
		float maxAccel = Math.Min(5f, Math.Max(1f, 5f - t / 64f / 2f)) + extraVelocity;
		xSlope = (float)Math.Cos((double)rotation + Math.PI / 2.0);
		ySlope = 0f - (float)Math.Sin((double)rotation + Math.PI / 2.0);
		xVelocity += (0f - xSlope) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
		yVelocity += (0f - ySlope) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
		if (Math.Abs(xVelocity) > Math.Abs((0f - xSlope) * maxSpeed))
		{
			xVelocity -= (0f - xSlope) * maxAccel / 6f;
		}
		if (Math.Abs(yVelocity) > Math.Abs((0f - ySlope) * maxSpeed))
		{
			yVelocity -= (0f - ySlope) * maxAccel / 6f;
		}
	}

	protected override void updateAnimation(GameTime time)
	{
		if (base.focusedOnFarmers || withinPlayerThreshold(6) || (bool)seenPlayer || magmaSprite.Value)
		{
			Sprite.Animate(time, 0, 4, 80f);
			if (Sprite.currentFrame % 3 == 0 && Utility.isOnScreen(base.Position, 512) && (batFlap == null || !batFlap.IsPlaying) && base.currentLocation == Game1.currentLocation && !cursedDoll)
			{
				Game1.playSound("batFlap", out batFlap);
			}
			if (cursedDoll.Value)
			{
				shakeTimer -= time.ElapsedGameTime.Milliseconds;
				if (shakeTimer < 0)
				{
					shakeTimer = 50;
					if (magmaSprite.Value)
					{
						shakeTimer = ((lungeTimer > 0) ? 50 : 100);
						base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Monsters\\Magma Sprite", new Rectangle(0, 64, 8, 8), base.Position + new Vector2(Game1.random.Next(32), -16 - Game1.random.Next(32)), flipped: false, 0f, Color.White)
						{
							scale = 4f,
							scaleChange = -0.05f,
							motion = new Vector2((lungeTimer > 0) ? ((float)Game1.random.Next(-30, 31) / 10f) : 0f, (0f - maxSpeed) / ((lungeTimer > 0) ? 2f : 8f)),
							layerDepth = 0.9f,
							animationLength = 6,
							totalNumberOfLoops = 1,
							interval = 50f,
							xPeriodic = (lungeTimer <= 0),
							xPeriodicLoopTime = Game1.random.Next(500, 800),
							xPeriodicRange = 4 * ((lungeTimer <= 0) ? 1 : 2)
						});
					}
					else if (!hauntedSkull.Value)
					{
						base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, (base.Age == 789) ? 789 : 103, 16, 16), base.Position + new Vector2(0f, -32f), flipped: false, 0.1f, new Color(255, 50, 255) * 0.8f)
						{
							scale = 4f
						});
					}
				}
				previousPositions.Add(base.Position);
				if (previousPositions.Count > 8)
				{
					previousPositions.RemoveAt(0);
				}
			}
		}
		else
		{
			Sprite.currentFrame = 4;
			Halt();
		}
		resetAnimationSpeed();
	}
}
