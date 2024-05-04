using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Projectiles;
using StardewValley.SpecialOrders;

namespace StardewValley.Monsters;

public class GreenSlime : Monster
{
	public const float mutationFactor = 0.25f;

	public const int matingInterval = 120000;

	public const int childhoodLength = 120000;

	public const int durationOfMating = 2000;

	public const double chanceToMate = 0.001;

	public static int matingRange = 192;

	public const int AQUA_SLIME = 9999899;

	public NetIntDelta stackedSlimes = new NetIntDelta(0)
	{
		Minimum = 0
	};

	public float randomStackOffset;

	[XmlIgnore]
	public NetEvent1Field<Vector2, NetVector2> attackedEvent = new NetEvent1Field<Vector2, NetVector2>();

	[XmlElement("leftDrift")]
	public readonly NetBool leftDrift = new NetBool();

	[XmlElement("cute")]
	public readonly NetBool cute = new NetBool(value: true);

	private int readyToJump = -1;

	private int matingCountdown;

	private new int yOffset;

	private int wagTimer;

	public int readyToMate = 120000;

	[XmlElement("ageUntilFullGrown")]
	public readonly NetInt ageUntilFullGrown = new NetInt();

	public int animateTimer;

	public int timeSinceLastJump;

	[XmlElement("specialNumber")]
	public readonly NetInt specialNumber = new NetInt();

	[XmlElement("firstGeneration")]
	public readonly NetBool firstGeneration = new NetBool();

	[XmlElement("color")]
	public readonly NetColor color = new NetColor();

	private readonly NetBool pursuingMate = new NetBool();

	private readonly NetBool avoidingMate = new NetBool();

	private GreenSlime mate;

	public readonly NetBool prismatic = new NetBool();

	private readonly NetVector2 facePosition = new NetVector2();

	private readonly NetEvent1Field<Vector2, NetVector2> jumpEvent = new NetEvent1Field<Vector2, NetVector2>
	{
		InterpolationWait = false
	};

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(leftDrift, "leftDrift").AddField(cute, "cute").AddField(ageUntilFullGrown, "ageUntilFullGrown")
			.AddField(specialNumber, "specialNumber")
			.AddField(firstGeneration, "firstGeneration")
			.AddField(color, "color")
			.AddField(pursuingMate, "pursuingMate")
			.AddField(avoidingMate, "avoidingMate")
			.AddField(facePosition, "facePosition")
			.AddField(jumpEvent, "jumpEvent")
			.AddField(prismatic, "prismatic")
			.AddField(stackedSlimes, "stackedSlimes")
			.AddField(attackedEvent.NetFields, "attackedEvent.NetFields");
		attackedEvent.onEvent += OnAttacked;
		jumpEvent.onEvent += doJump;
	}

	public GreenSlime()
	{
	}

	public GreenSlime(Vector2 position)
		: base("Green Slime", position)
	{
		if (Game1.random.NextBool())
		{
			leftDrift.Value = true;
		}
		base.Slipperiness = 4;
		readyToMate = Game1.random.Next(1000, 120000);
		int green = Game1.random.Next(200, 256);
		color.Value = new Color(green / Game1.random.Next(2, 10), Game1.random.Next(180, 256), (Game1.random.NextDouble() < 0.1) ? 255 : (255 - green));
		firstGeneration.Value = true;
		flip = Game1.random.NextBool();
		cute.Value = Game1.random.NextDouble() < 0.49;
		base.HideShadow = true;
	}

	public GreenSlime(Vector2 position, int mineLevel)
		: base("Green Slime", position)
	{
		randomStackOffset = Utility.RandomFloat(0f, 100f);
		cute.Value = Game1.random.NextDouble() < 0.49;
		flip = Game1.random.NextBool();
		specialNumber.Value = Game1.random.Next(100);
		if (mineLevel < 40)
		{
			parseMonsterInfo("Green Slime");
			int green = Game1.random.Next(200, 256);
			color.Value = new Color(green / Game1.random.Next(2, 10), green, (Game1.random.NextDouble() < 0.01) ? 255 : (255 - green));
			if (Game1.random.NextDouble() < 0.01 && mineLevel % 5 != 0 && mineLevel % 5 != 1)
			{
				color.Value = new Color(205, 255, 0) * 0.7f;
				hasSpecialItem.Value = true;
				base.Health *= 3;
				base.DamageToFarmer *= 2;
			}
			if (Game1.random.NextDouble() < 0.01 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
			{
				objectsToDrop.Add("680");
			}
		}
		else if (mineLevel < 80)
		{
			base.Name = "Frost Jelly";
			parseMonsterInfo("Frost Jelly");
			int blue = Game1.random.Next(200, 256);
			color.Value = new Color((Game1.random.NextDouble() < 0.01) ? 180 : (blue / Game1.random.Next(2, 10)), (Game1.random.NextDouble() < 0.1) ? 255 : (255 - blue / 3), blue);
			if (Game1.random.NextDouble() < 0.01 && mineLevel % 5 != 0 && mineLevel % 5 != 1)
			{
				color.Value = new Color(0, 0, 0) * 0.7f;
				hasSpecialItem.Value = true;
				base.Health *= 3;
				base.DamageToFarmer *= 2;
			}
			if (Game1.random.NextDouble() < 0.01 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
			{
				objectsToDrop.Add("413");
			}
		}
		else if (mineLevel >= 77377 && mineLevel < 77387)
		{
			base.Name = "Sludge";
			parseMonsterInfo("Sludge");
		}
		else if (mineLevel > 120)
		{
			base.Name = "Sludge";
			parseMonsterInfo("Sludge");
			color.Value = Color.BlueViolet;
			base.Health *= 2;
			int r = color.R;
			int g = color.G;
			int b = color.B;
			r += Game1.random.Next(-20, 21);
			g += Game1.random.Next(-20, 21);
			b += Game1.random.Next(-20, 21);
			color.R = (byte)Math.Max(Math.Min(255, r), 0);
			color.G = (byte)Math.Max(Math.Min(255, g), 0);
			color.B = (byte)Math.Max(Math.Min(255, b), 0);
			while (Game1.random.NextDouble() < 0.08)
			{
				objectsToDrop.Add("386");
			}
			if (Game1.random.NextDouble() < 0.009)
			{
				objectsToDrop.Add("337");
			}
			if (Game1.random.NextDouble() < 0.01 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
			{
				objectsToDrop.Add("439");
			}
		}
		else
		{
			base.Name = "Sludge";
			parseMonsterInfo("Sludge");
			int green = Game1.random.Next(200, 256);
			color.Value = new Color(green, (Game1.random.NextDouble() < 0.01) ? 255 : (255 - green), green / Game1.random.Next(2, 10));
			if (Game1.random.NextDouble() < 0.01 && mineLevel % 5 != 0 && mineLevel % 5 != 1)
			{
				color.Value = new Color(50, 10, 50) * 0.7f;
				hasSpecialItem.Value = true;
				base.Health *= 3;
				base.DamageToFarmer *= 2;
			}
			if (Game1.random.NextDouble() < 0.01 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
			{
				objectsToDrop.Add("437");
			}
		}
		if ((bool)cute)
		{
			base.Health += base.Health / 4;
			base.DamageToFarmer++;
		}
		if (Game1.random.NextBool())
		{
			leftDrift.Value = true;
		}
		base.Slipperiness = 3;
		readyToMate = Game1.random.Next(1000, 120000);
		if (Game1.random.NextDouble() < 0.001)
		{
			color.Value = new Color(255, 255, 50);
			objectsToDrop.Add("GoldCoin");
			double extraChance = (double)(int)(Game1.stats.DaysPlayed / 28) * 0.08;
			extraChance = Math.Min(extraChance, 0.55);
			while (Game1.random.NextDouble() < 0.1 + extraChance)
			{
				objectsToDrop.Add("GoldCoin");
			}
		}
		if (mineLevel == 9999899)
		{
			color.Value = new Color(0, 255, 200);
			base.Health *= 2;
			objectsToDrop.Clear();
			if (Game1.random.NextDouble() < 0.02)
			{
				objectsToDrop.Add("394");
			}
			if (Game1.random.NextDouble() < 0.02)
			{
				objectsToDrop.Add("60");
			}
			if (Game1.random.NextDouble() < 0.02)
			{
				objectsToDrop.Add("62");
			}
			if (Game1.random.NextDouble() < 0.01)
			{
				objectsToDrop.Add("797");
			}
			if (Game1.random.NextDouble() < 0.03 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
			{
				objectsToDrop.Add("413");
			}
			while (Game1.random.NextBool())
			{
				objectsToDrop.Add("766");
			}
		}
		firstGeneration.Value = true;
		base.HideShadow = true;
	}

	public GreenSlime(Vector2 position, Color color)
		: base("Green Slime", position)
	{
		this.color.Value = color;
		firstGeneration.Value = true;
		base.HideShadow = true;
	}

	public void makeTigerSlime(bool onlyAppearance = false)
	{
		string oldName = base.Name;
		try
		{
			base.Name = "Tiger Slime";
			base.reloadSprite(onlyAppearance: false);
		}
		finally
		{
			if (onlyAppearance)
			{
				base.Name = oldName;
			}
		}
		Sprite.SpriteHeight = 24;
		Sprite.UpdateSourceRect();
		color.Value = Color.White;
		if (!onlyAppearance)
		{
			parseMonsterInfo("Tiger Slime");
		}
	}

	public void makePrismatic()
	{
		prismatic.Value = true;
		base.Name = "Prismatic Slime";
		base.Health = 1000;
		damageToFarmer.Value = 35;
		hasSpecialItem.Value = false;
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		if (base.Name == "Tiger Slime")
		{
			makeTigerSlime(onlyAppearance);
			return;
		}
		string oldName = name;
		try
		{
			base.Name = "Green Slime";
			base.reloadSprite(onlyAppearance);
		}
		finally
		{
			base.Name = oldName;
		}
		Sprite.SpriteHeight = 24;
		Sprite.UpdateSourceRect();
		base.HideShadow = true;
	}

	public virtual void OnAttacked(Vector2 trajectory)
	{
		if (Game1.IsMasterGame && stackedSlimes.Value > 0)
		{
			stackedSlimes.Value--;
			if (trajectory.LengthSquared() == 0f)
			{
				trajectory = new Vector2(0f, -1f);
			}
			else
			{
				trajectory.Normalize();
			}
			trajectory *= 16f;
			BasicProjectile projectile = new BasicProjectile(base.DamageToFarmer / 3 * 2, 13, 3, 0, (float)Math.PI / 16f, trajectory.X, trajectory.Y, base.Position, null, null, null, explode: true, damagesMonsters: false, base.currentLocation, this);
			projectile.height.Value = 24f;
			projectile.color.Value = color.Value;
			projectile.ignoreMeleeAttacks.Value = true;
			projectile.hostTimeUntilAttackable = 0.1f;
			if (Game1.random.NextBool())
			{
				projectile.debuff.Value = "13";
			}
			base.currentLocation.projectiles.Add(projectile);
		}
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		if (stackedSlimes.Value > 0)
		{
			attackedEvent.Fire(new Vector2(xTrajectory, -yTrajectory));
			xTrajectory = 0;
			yTrajectory = 0;
			damage = 1;
		}
		int actualDamage = Math.Max(1, damage - (int)resilience);
		if (Game1.random.NextDouble() < missChance.Value - missChance.Value * addedPrecision)
		{
			actualDamage = -1;
		}
		else
		{
			if (Game1.random.NextDouble() < 0.025 && (bool)cute)
			{
				if (!base.focusedOnFarmers)
				{
					base.DamageToFarmer += base.DamageToFarmer / 2;
					shake(1000);
				}
				base.focusedOnFarmers = true;
			}
			base.Slipperiness = 3;
			base.Health -= actualDamage;
			setTrajectory(xTrajectory, yTrajectory);
			base.currentLocation.playSound("slimeHit");
			readyToJump = -1;
			base.IsWalkingTowardPlayer = true;
			if (base.Health <= 0)
			{
				base.currentLocation.playSound("slimedead");
				Game1.stats.SlimesKilled++;
				if (mate != null)
				{
					mate.mate = null;
				}
				if (Game1.gameMode == 3 && scale.Value > 1.8f)
				{
					base.Health = 10;
					int toCreate = ((!(scale.Value > 1.8f)) ? 1 : Game1.random.Next(3, 5));
					base.Scale *= 2f / 3f;
					Rectangle bounds = GetBoundingBox();
					for (int i = 0; i < toCreate; i++)
					{
						GreenSlime slime = new GreenSlime(base.Position + new Vector2(i * bounds.Width, 0f), Game1.CurrentMineLevel);
						slime.setTrajectory(xTrajectory + Game1.random.Next(-20, 20), yTrajectory + Game1.random.Next(-20, 20));
						slime.willDestroyObjectsUnderfoot = false;
						slime.moveTowardPlayer(4);
						slime.Scale = 0.75f + (float)Game1.random.Next(-5, 10) / 100f;
						base.currentLocation.characters.Add(slime);
					}
				}
				else
				{
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position, color.Value * 0.66f, 10)
					{
						interval = 70f,
						holdLastFrame = true,
						alphaFade = 0.01f
					});
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position + new Vector2(-16f, 0f), color.Value * 0.66f, 10)
					{
						interval = 70f,
						delayBeforeAnimationStart = 0,
						holdLastFrame = true,
						alphaFade = 0.01f
					});
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position + new Vector2(0f, 16f), color.Value * 0.66f, 10)
					{
						interval = 70f,
						delayBeforeAnimationStart = 100,
						holdLastFrame = true,
						alphaFade = 0.01f
					});
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position + new Vector2(16f, 0f), color.Value * 0.66f, 10)
					{
						interval = 70f,
						delayBeforeAnimationStart = 200,
						holdLastFrame = true,
						alphaFade = 0.01f
					});
				}
			}
		}
		return actualDamage;
	}

	public override void shedChunks(int number, float scale)
	{
		Point standingPixel = base.StandingPixel;
		Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(0, 120, 16, 16), 8, standingPixel.X + 32, standingPixel.Y, number, base.TilePoint.Y, color.Value, 4f * scale);
	}

	public override void collisionWithFarmerBehavior()
	{
		farmerPassesThrough = base.Player.isWearingRing("520");
	}

	public override void onDealContactDamage(Farmer who)
	{
		if (Game1.random.NextDouble() < 0.3 && base.Player == Game1.player && !base.Player.temporarilyInvincible && !base.Player.isWearingRing("520") && Game1.random.Next(11) >= who.Immunity && !base.Player.hasBuff("28") && !base.Player.hasTrinketWithID("BasiliskPaw"))
		{
			base.Player.applyBuff("13");
			base.currentLocation.playSound("slime");
		}
		base.onDealContactDamage(who);
	}

	public override void draw(SpriteBatch b)
	{
		if (base.IsInvisible || !Utility.isOnScreen(base.Position, 128))
		{
			return;
		}
		int boundsHeight = GetBoundingBox().Height;
		int standingY = base.StandingPixel.Y;
		for (int i = 0; i <= stackedSlimes.Value; i++)
		{
			bool top_slime = i == stackedSlimes.Value;
			Vector2 stack_adjustment = Vector2.Zero;
			if (stackedSlimes.Value > 0)
			{
				stack_adjustment = new Vector2((float)Math.Sin((double)randomStackOffset + Game1.currentGameTime.TotalGameTime.TotalSeconds * Math.PI * 2.0 + (double)(i * 30)) * 8f, -30 * i);
			}
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, boundsHeight / 2 + yOffset) + stack_adjustment, Sprite.SourceRect, prismatic ? Utility.GetPrismaticColor(348 + (int)specialNumber, 5f) : color.Value, 0f, new Vector2(8f, 16f), 4f * Math.Max(0.2f, scale.Value - 0.4f * ((float)ageUntilFullGrown.Value / 120000f)), SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(standingY + i * 2) / 10000f)));
			b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(boundsHeight / 2 * 7) / 4f + (float)yOffset + 8f * scale.Value - (float)(((int)ageUntilFullGrown > 0) ? 8 : 0)) + stack_adjustment, Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + scale.Value - (float)(int)ageUntilFullGrown / 120000f - ((Sprite.currentFrame % 4 % 3 != 0 || i != 0) ? 1f : 0f) + (float)yOffset / 30f, SpriteEffects.None, (float)(standingY - 1 + i * 2) / 10000f);
			if ((int)ageUntilFullGrown <= 0)
			{
				if (top_slime && ((bool)cute || (bool)hasSpecialItem))
				{
					int xDongleSource = ((isMoving() || wagTimer > 0) ? (16 * Math.Min(7, Math.Abs(((wagTimer > 0) ? (992 - wagTimer) : (Game1.currentGameTime.TotalGameTime.Milliseconds % 992)) - 496) / 62) % 64) : 48);
					int yDongleSource = ((isMoving() || wagTimer > 0) ? (24 * Math.Min(1, Math.Max(1, Math.Abs(((wagTimer > 0) ? (992 - wagTimer) : (Game1.currentGameTime.TotalGameTime.Milliseconds % 992)) - 496) / 62) / 4)) : 24);
					if ((bool)hasSpecialItem)
					{
						yDongleSource += 48;
					}
					b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + stack_adjustment + new Vector2(32f, boundsHeight - 16 + ((readyToJump <= 0) ? (4 * (-2 + Math.Abs(Sprite.currentFrame % 4 - 2))) : (4 + 4 * (Sprite.currentFrame % 4 % 3))) + yOffset) * scale.Value, new Rectangle(xDongleSource, 168 + yDongleSource, 16, 24), hasSpecialItem ? Color.White : color.Value, 0f, new Vector2(8f, 16f), 4f * Math.Max(0.2f, scale.Value - 0.4f * ((float)ageUntilFullGrown.Value / 120000f)), flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)standingY / 10000f + 0.0001f)));
				}
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + stack_adjustment + (new Vector2(32f, boundsHeight / 2 + ((readyToJump <= 0) ? (4 * (-2 + Math.Abs(Sprite.currentFrame % 4 - 2))) : (4 - 4 * (Sprite.currentFrame % 4 % 3))) + yOffset) + facePosition.Value) * Math.Max(0.2f, scale.Value - 0.4f * ((float)ageUntilFullGrown.Value / 120000f)), new Rectangle(32 + ((readyToJump > 0 || base.focusedOnFarmers) ? 16 : 0), 120 + ((readyToJump < 0 && (base.focusedOnFarmers || invincibleCountdown > 0)) ? 24 : 0), 16, 24), Color.White * ((FacingDirection == 0) ? 0.5f : 1f), 0f, new Vector2(8f, 16f), 4f * Math.Max(0.2f, scale.Value - 0.4f * ((float)ageUntilFullGrown.Value / 120000f)), SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)(standingY + i * 2) / 10000f + 0.0001f)));
			}
			if (isGlowing)
			{
				b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + stack_adjustment + new Vector2(32f, boundsHeight / 2 + yOffset), Sprite.SourceRect, glowingColor * glowingTransparency, 0f, new Vector2(8f, 16f), 4f * Math.Max(0.2f, scale.Value), SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.99f : ((float)standingY / 10000f + 0.001f)));
			}
		}
		if ((bool)pursuingMate)
		{
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, -32 + yOffset), new Rectangle(16, 120, 8, 8), Color.White, 0f, new Vector2(3f, 3f), 4f, SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)base.StandingPixel.Y / 10000f)));
		}
		else if ((bool)avoidingMate)
		{
			b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, -32 + yOffset), new Rectangle(24, 120, 8, 8), Color.White, 0f, new Vector2(4f, 4f), 4f, SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)base.StandingPixel.Y / 10000f)));
		}
	}

	public void moveTowardOtherSlime(GreenSlime other, bool moveAway, GameTime time)
	{
		Point curPixel = base.StandingPixel;
		Point otherPixel = other.StandingPixel;
		int xToGo = Math.Abs(otherPixel.X - curPixel.X);
		int yToGo = Math.Abs(otherPixel.Y - curPixel.Y);
		if (xToGo > 4 || yToGo > 4)
		{
			int dx = ((otherPixel.X > curPixel.X) ? 1 : (-1));
			int dy = ((otherPixel.Y > curPixel.Y) ? 1 : (-1));
			if (moveAway)
			{
				dx = -dx;
				dy = -dy;
			}
			double chanceForX = (double)xToGo / (double)(xToGo + yToGo);
			if (Game1.random.NextDouble() < chanceForX)
			{
				tryToMoveInDirection((dx > 0) ? 1 : 3, isFarmer: false, base.DamageToFarmer, glider: false);
			}
			else
			{
				tryToMoveInDirection((dy > 0) ? 2 : 0, isFarmer: false, base.DamageToFarmer, glider: false);
			}
		}
		Sprite.AnimateDown(time);
		if (invincibleCountdown > 0)
		{
			invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
			if (invincibleCountdown <= 0)
			{
				stopGlowing();
			}
		}
	}

	public void doneMating()
	{
		readyToMate = 120000;
		matingCountdown = 2000;
		mate = null;
		pursuingMate.Value = false;
		avoidingMate.Value = false;
	}

	public override void noMovementProgressNearPlayerBehavior()
	{
		faceGeneralDirection(base.Player.getStandingPosition());
	}

	public void mateWith(GreenSlime mateToPursue, GameLocation location)
	{
		if (location.canSlimeMateHere())
		{
			GreenSlime baby = new GreenSlime(Vector2.Zero);
			Utility.recursiveFindPositionForCharacter(baby, location, base.Tile, 30);
			Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, (double)Game1.uniqueIDForThisGame / 10.0, (double)scale.Value * 100.0, (double)mateToPursue.scale.Value * 100.0);
			switch (r.Next(4))
			{
			case 0:
				baby.color.Value = new Color(Math.Min(255, Math.Max(0, color.R + r.Next((int)((float)(-color.R) * 0.25f), (int)((float)(int)color.R * 0.25f)))), Math.Min(255, Math.Max(0, color.G + r.Next((int)((float)(-color.G) * 0.25f), (int)((float)(int)color.G * 0.25f)))), Math.Min(255, Math.Max(0, color.B + r.Next((int)((float)(-color.B) * 0.25f), (int)((float)(int)color.B * 0.25f)))));
				break;
			case 1:
			case 2:
				baby.color.Value = Utility.getBlendedColor(color.Value, mateToPursue.color.Value);
				break;
			case 3:
				baby.color.Value = new Color(Math.Min(255, Math.Max(0, mateToPursue.color.R + r.Next((int)((float)(-mateToPursue.color.R) * 0.25f), (int)((float)(int)mateToPursue.color.R * 0.25f)))), Math.Min(255, Math.Max(0, mateToPursue.color.G + r.Next((int)((float)(-mateToPursue.color.G) * 0.25f), (int)((float)(int)mateToPursue.color.G * 0.25f)))), Math.Min(255, Math.Max(0, mateToPursue.color.B + r.Next((int)((float)(-mateToPursue.color.B) * 0.25f), (int)((float)(int)mateToPursue.color.B * 0.25f)))));
				break;
			}
			int red = baby.color.R;
			int green = baby.color.G;
			int blue = baby.color.B;
			baby.Name = name;
			if (baby.Name == "Tiger Slime")
			{
				baby.makeTigerSlime();
			}
			else if (red > 100 && blue > 100 && green < 50)
			{
				baby.parseMonsterInfo("Sludge");
				while (r.NextDouble() < 0.1)
				{
					baby.objectsToDrop.Add("386");
				}
				if (r.NextDouble() < 0.01)
				{
					baby.objectsToDrop.Add("337");
				}
			}
			else if (red >= 200 && green < 75)
			{
				baby.parseMonsterInfo("Sludge");
			}
			else if (blue >= 200 && red < 100)
			{
				baby.parseMonsterInfo("Frost Jelly");
			}
			baby.Health = r.Choose(base.Health, mateToPursue.Health);
			baby.Health = Math.Max(1, base.Health + r.Next(-4, 5));
			baby.DamageToFarmer = r.Choose(base.DamageToFarmer, mateToPursue.DamageToFarmer);
			baby.DamageToFarmer = Math.Max(0, base.DamageToFarmer + r.Next(-1, 2));
			baby.resilience.Value = r.Choose(resilience, mateToPursue.resilience);
			baby.resilience.Value = Math.Max(0, (int)resilience + r.Next(-1, 2));
			baby.missChance.Value = r.Choose(missChance.Value, mateToPursue.missChance.Value);
			baby.missChance.Value = Math.Max(0.0, missChance.Value + (double)((float)r.Next(-1, 2) / 100f));
			baby.Scale = r.Choose(scale.Value, mateToPursue.scale.Value);
			baby.Scale = Math.Max(0.6f, Math.Min(1.5f, scale.Value + (float)r.Next(-2, 3) / 100f));
			baby.Slipperiness = 8;
			base.speed = r.Choose(base.speed, mateToPursue.speed);
			if (r.NextDouble() < 0.015)
			{
				base.speed = Math.Max(1, Math.Min(6, base.speed + r.Next(-1, 2)));
			}
			baby.setTrajectory(Utility.getAwayFromPositionTrajectory(baby.GetBoundingBox(), getStandingPosition()) / 2f);
			baby.ageUntilFullGrown.Value = 120000;
			baby.Halt();
			baby.firstGeneration.Value = false;
			if (Utility.isOnScreen(base.Position, 128))
			{
				base.currentLocation.playSound("slime");
			}
		}
		mateToPursue.doneMating();
		doneMating();
	}

	public override List<Item> getExtraDropItems()
	{
		List<Item> extra = new List<Item>();
		if (name != "Tiger Slime")
		{
			if (color.R >= 50 && color.R <= 100 && color.G >= 25 && color.G <= 50 && color.B <= 25)
			{
				extra.Add(ItemRegistry.Create("(O)388", Game1.random.Next(3, 7)));
				if (Game1.random.NextDouble() < 0.1)
				{
					extra.Add(ItemRegistry.Create("(O)709"));
				}
			}
			else if (color.R < 80 && color.G < 80 && color.B < 80)
			{
				extra.Add(ItemRegistry.Create("(O)382"));
				Random random = Utility.CreateRandom((double)base.Position.X * 777.0, (double)base.Position.Y * 77.0, Game1.stats.DaysPlayed);
				if (random.NextDouble() < 0.05)
				{
					extra.Add(ItemRegistry.Create("(O)553"));
				}
				if (random.NextDouble() < 0.05)
				{
					extra.Add(ItemRegistry.Create("(O)539"));
				}
			}
			else if (color.R > 200 && color.G > 180 && color.B < 50)
			{
				extra.Add(ItemRegistry.Create("(O)384", 2));
			}
			else if (color.R > 220 && color.G > 90 && color.G < 150 && color.B < 50)
			{
				extra.Add(ItemRegistry.Create("(O)378", 2));
			}
			else if (color.R > 230 && color.G > 230 && color.B > 230)
			{
				if (color.R % 2 == 1)
				{
					extra.Add(ItemRegistry.Create("(O)338"));
					if (color.G % 2 == 1)
					{
						extra.Add(ItemRegistry.Create("(O)338"));
					}
				}
				else
				{
					extra.Add(ItemRegistry.Create("(O)380"));
				}
				if ((color.R % 2 == 0 && color.G % 2 == 0 && color.B % 2 == 0) || color.Equals(Color.White))
				{
					extra.Add(new Object("72", 1));
				}
			}
			else if (color.R > 150 && color.G > 150 && color.B > 150)
			{
				extra.Add(ItemRegistry.Create("(O)390", 2));
			}
			else if (color.R > 150 && color.B > 180 && color.G < 50 && (int)specialNumber % (firstGeneration ? 4 : 2) == 0)
			{
				extra.Add(ItemRegistry.Create("(O)386", 2));
				if ((bool)firstGeneration && Game1.random.NextDouble() < 0.005)
				{
					extra.Add(ItemRegistry.Create("(O)485"));
				}
			}
		}
		if (Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt") && (int)specialNumber == 1)
		{
			switch (base.Name)
			{
			case "Green Slime":
				extra.Add(ItemRegistry.Create("(O)680"));
				break;
			case "Frost Jelly":
				extra.Add(ItemRegistry.Create("(O)413"));
				break;
			case "Tiger Slime":
				extra.Add(ItemRegistry.Create("(O)857"));
				break;
			}
		}
		if (base.Name == "Tiger Slime")
		{
			if (Game1.random.NextDouble() < 0.001)
			{
				extra.Add(ItemRegistry.Create("(H)91"));
			}
			if (Game1.random.NextDouble() < 0.1)
			{
				extra.Add(ItemRegistry.Create("(O)831"));
				while (Game1.random.NextBool())
				{
					extra.Add(ItemRegistry.Create("(O)831"));
				}
			}
			else if (Game1.random.NextDouble() < 0.1)
			{
				extra.Add(ItemRegistry.Create("(O)829"));
			}
			else if (Game1.random.NextDouble() < 0.02)
			{
				extra.Add(ItemRegistry.Create("(O)833"));
				while (Game1.random.NextBool())
				{
					extra.Add(ItemRegistry.Create("(O)833"));
				}
			}
			else if (Game1.random.NextDouble() < 0.006)
			{
				extra.Add(ItemRegistry.Create("(O)835"));
			}
		}
		if (prismatic.Value && Game1.player.team.specialOrders.Where((SpecialOrder x) => x.questKey == "Wizard2") != null)
		{
			Object o = ItemRegistry.Create<Object>("(O)876");
			o.specialItem = true;
			o.questItem.Value = true;
			return new List<Item> { o };
		}
		return extra;
	}

	public override void dayUpdate(int dayOfMonth)
	{
		if ((int)ageUntilFullGrown > 0)
		{
			ageUntilFullGrown.Value /= 2;
		}
		if (readyToMate > 0)
		{
			readyToMate /= 2;
		}
		base.dayUpdate(dayOfMonth);
	}

	protected override void updateAnimation(GameTime time)
	{
		if (wagTimer > 0)
		{
			wagTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		}
		if ((int)stunTime > 0)
		{
			yOffset = 0;
		}
		else
		{
			yOffset = Math.Max(yOffset - (int)Math.Abs(xVelocity + yVelocity) / 2, -64);
			if (yOffset < 0)
			{
				yOffset = Math.Min(0, yOffset + 4 + (int)((yOffset <= -64) ? ((float)(-yOffset) / 8f) : ((float)(-yOffset) / 16f)));
			}
			timeSinceLastJump += time.ElapsedGameTime.Milliseconds;
		}
		if (Game1.random.NextDouble() < 0.01 && wagTimer <= 0)
		{
			wagTimer = 992;
		}
		if (Math.Abs(xVelocity) >= 0.5f || Math.Abs(yVelocity) >= 0.5f)
		{
			Sprite.AnimateDown(time);
		}
		else if (!base.Position.Equals(lastPosition))
		{
			animateTimer = 500;
		}
		if (animateTimer > 0 && readyToJump <= 0)
		{
			animateTimer -= time.ElapsedGameTime.Milliseconds;
			Sprite.AnimateDown(time);
		}
		resetAnimationSpeed();
	}

	public override void update(GameTime time, GameLocation location)
	{
		base.update(time, location);
		jumpEvent.Poll();
		attackedEvent.Poll();
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		if (mate == null)
		{
			pursuingMate.Value = false;
			avoidingMate.Value = false;
		}
		switch (FacingDirection)
		{
		case 2:
			if (facePosition.X > 0f)
			{
				facePosition.X -= 2f;
			}
			else if (facePosition.X < 0f)
			{
				facePosition.X += 2f;
			}
			if (facePosition.Y < 0f)
			{
				facePosition.Y += 2f;
			}
			break;
		case 1:
			if (facePosition.X < 8f)
			{
				facePosition.X += 2f;
			}
			if (facePosition.Y < 0f)
			{
				facePosition.Y += 2f;
			}
			break;
		case 3:
			if (facePosition.X > -8f)
			{
				facePosition.X -= 2f;
			}
			if (facePosition.Y < 0f)
			{
				facePosition.Y += 2f;
			}
			break;
		case 0:
			if (facePosition.X > 0f)
			{
				facePosition.X -= 2f;
			}
			else if (facePosition.X < 0f)
			{
				facePosition.X += 2f;
			}
			if (facePosition.Y > -8f)
			{
				facePosition.Y -= 2f;
			}
			break;
		}
		if (stackedSlimes.Value <= 0)
		{
			if ((int)ageUntilFullGrown <= 0)
			{
				readyToMate -= time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				ageUntilFullGrown.Value -= time.ElapsedGameTime.Milliseconds;
			}
		}
		if ((bool)pursuingMate && mate != null)
		{
			if (readyToMate <= -35000)
			{
				mate.doneMating();
				doneMating();
				return;
			}
			moveTowardOtherSlime(mate, moveAway: false, time);
			if (mate.mate != null && (bool)mate.pursuingMate && !mate.mate.Equals(this))
			{
				doneMating();
				return;
			}
			Vector2 curStandingPosition = getStandingPosition();
			Vector2 mateStandingPosition = mate.getStandingPosition();
			if (Vector2.Distance(curStandingPosition, mateStandingPosition) < (float)(GetBoundingBox().Width + 4))
			{
				if (mate.mate != null && (bool)mate.avoidingMate && mate.mate.Equals(this))
				{
					mate.avoidingMate.Value = false;
					mate.matingCountdown = 2000;
					mate.pursuingMate.Value = true;
				}
				matingCountdown -= time.ElapsedGameTime.Milliseconds;
				if (base.currentLocation != null && matingCountdown <= 0 && (bool)pursuingMate && (!base.currentLocation.isOutdoors || Utility.getNumberOfCharactersInRadius(base.currentLocation, Utility.Vector2ToPoint(base.Position), 1) <= 4))
				{
					mateWith(mate, base.currentLocation);
				}
			}
			else if (Vector2.Distance(curStandingPosition, mateStandingPosition) > (float)(matingRange * 2))
			{
				mate.mate = null;
				mate.avoidingMate.Value = false;
				mate = null;
			}
			return;
		}
		if ((bool)avoidingMate && mate != null)
		{
			moveTowardOtherSlime(mate, moveAway: true, time);
			return;
		}
		if (readyToMate < 0 && (bool)cute)
		{
			readyToMate = -1;
			if (Game1.random.NextDouble() < 0.001)
			{
				Point standingPixel = base.StandingPixel;
				GreenSlime newMate = (GreenSlime)Utility.checkForCharacterWithinArea(GetType(), base.Position, base.currentLocation, new Rectangle(standingPixel.X - matingRange, standingPixel.Y - matingRange, matingRange * 2, matingRange * 2));
				if (newMate != null && newMate.readyToMate <= 0 && !newMate.cute && newMate.stackedSlimes.Value <= 0)
				{
					matingCountdown = 2000;
					mate = newMate;
					pursuingMate.Value = true;
					newMate.mate = this;
					newMate.avoidingMate.Value = true;
					addedSpeed = 1f;
					mate.addedSpeed = 1f;
					return;
				}
			}
		}
		else if (!isGlowing)
		{
			addedSpeed = 0f;
		}
		base.behaviorAtGameTick(time);
		if (readyToJump != -1)
		{
			Halt();
			base.IsWalkingTowardPlayer = false;
			readyToJump -= time.ElapsedGameTime.Milliseconds;
			Sprite.currentFrame = 16 + (800 - readyToJump) / 200;
			if (readyToJump <= 0)
			{
				timeSinceLastJump = timeSinceLastJump;
				base.Slipperiness = 10;
				base.IsWalkingTowardPlayer = true;
				readyToJump = -1;
				invincibleCountdown = 0;
				Vector2 trajectory = Utility.getAwayFromPlayerTrajectory(GetBoundingBox(), base.Player);
				trajectory.X = (0f - trajectory.X) / 2f;
				trajectory.Y = (0f - trajectory.Y) / 2f;
				jumpEvent.Fire(trajectory);
				setTrajectory((int)trajectory.X, (int)trajectory.Y);
			}
		}
		else if (Game1.random.NextDouble() < 0.1 && !base.focusedOnFarmers)
		{
			if (FacingDirection == 0 || FacingDirection == 2)
			{
				if ((bool)leftDrift && !base.currentLocation.isCollidingPosition(nextPosition(3), Game1.viewport, isFarmer: false, 1, glider: false, this))
				{
					position.X -= base.speed;
				}
				else if (!leftDrift && !base.currentLocation.isCollidingPosition(nextPosition(1), Game1.viewport, isFarmer: false, 1, glider: false, this))
				{
					position.X += base.speed;
				}
			}
			else if ((bool)leftDrift && !base.currentLocation.isCollidingPosition(nextPosition(0), Game1.viewport, isFarmer: false, 1, glider: false, this))
			{
				position.Y -= base.speed;
			}
			else if (!leftDrift && !base.currentLocation.isCollidingPosition(nextPosition(2), Game1.viewport, isFarmer: false, 1, glider: false, this))
			{
				position.Y += base.speed;
			}
			if (Game1.random.NextDouble() < 0.08)
			{
				leftDrift.Value = !leftDrift;
			}
		}
		else if (withinPlayerThreshold() && timeSinceLastJump > (base.focusedOnFarmers ? 1000 : 4000) && Game1.random.NextDouble() < 0.01 && stackedSlimes.Value <= 0)
		{
			if (base.Name.Equals("Frost Jelly") && Game1.random.NextDouble() < 0.25)
			{
				addedSpeed = 2f;
				startGlowing(Color.Cyan, border: false, 0.15f);
			}
			else
			{
				addedSpeed = 0f;
				stopGlowing();
				readyToJump = 800;
			}
		}
	}

	private void doJump(Vector2 trajectory)
	{
		if (Utility.isOnScreen(base.Position, 128))
		{
			base.currentLocation.localSound("slime");
		}
		Sprite.currentFrame = 1;
	}
}
