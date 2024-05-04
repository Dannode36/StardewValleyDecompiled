using System;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Tools;

namespace StardewValley.Monsters;

public class RockCrab : Monster
{
	private bool waiter;

	private readonly NetBool shellGone = new NetBool();

	private readonly NetInt shellHealth = new NetInt(5);

	private readonly NetBool isStickBug = new NetBool();

	public RockCrab()
	{
	}

	public RockCrab(Vector2 position)
		: base("Rock Crab", position)
	{
		waiter = Game1.random.NextDouble() < 0.4;
		moveTowardPlayerThreshold.Value = 3;
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		base.reloadSprite(onlyAppearance);
		Sprite.UpdateSourceRect();
	}

	/// <summary>
	/// constructor for Lava Crab
	/// </summary>
	/// <param name="position"></param>
	/// <param name="name"></param>
	public RockCrab(Vector2 position, string name)
		: base(name, position)
	{
		waiter = Game1.random.NextDouble() < 0.4;
		moveTowardPlayerThreshold.Value = 3;
		switch (name)
		{
		case "Truffle Crab":
			waiter = false;
			moveTowardPlayerThreshold.Value = 1;
			break;
		case "Iridium Crab":
			waiter = true;
			moveTowardPlayerThreshold.Value = 1;
			break;
		case "False Magma Cap":
			waiter = false;
			break;
		}
	}

	public void makeStickBug()
	{
		isStickBug.Value = true;
		waiter = false;
		base.Name = "Stick Bug";
		base.DamageToFarmer = 20;
		base.MaxHealth = 700;
		base.Health = 700;
		base.reloadSprite(onlyAppearance: false);
		base.HideShadow = true;
		Sprite.SpriteHeight = 24;
		Sprite.UpdateSourceRect();
		objectsToDrop.Clear();
		objectsToDrop.Add("858");
		while (Game1.random.NextBool())
		{
			objectsToDrop.Add("858");
		}
		objectsToDrop.Add("829");
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(shellGone, "shellGone").AddField(shellHealth, "shellHealth").AddField(isStickBug, "isStickBug");
		position.Field.AxisAlignedMovement = true;
	}

	public override bool hitWithTool(Tool t)
	{
		if ((bool)isStickBug)
		{
			return false;
		}
		if (t is Pickaxe && t.getLastFarmerToUse() != null && (int)shellHealth > 0)
		{
			base.currentLocation.playSound("hammer");
			shellHealth.Value--;
			shake(500);
			waiter = false;
			moveTowardPlayerThreshold.Value = 3;
			setTrajectory(Utility.getAwayFromPlayerTrajectory(GetBoundingBox(), t.getLastFarmerToUse()));
			if ((int)shellHealth <= 0)
			{
				Point tile = base.TilePoint;
				shellGone.Value = true;
				moveTowardPlayer(-1);
				base.currentLocation.playSound("stoneCrack");
				Game1.createRadialDebris(base.currentLocation, 14, tile.X, tile.Y, Game1.random.Next(2, 7), resource: false);
				Game1.createRadialDebris(base.currentLocation, 14, tile.X, tile.Y, Game1.random.Next(2, 7), resource: false);
			}
			return true;
		}
		return base.hitWithTool(t);
	}

	public override void shedChunks(int number)
	{
		Point standingPixel = base.StandingPixel;
		Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(0, 120, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, 4f * scale.Value);
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		int actualDamage = Math.Max(1, damage - (int)resilience);
		if (isBomb && !isStickBug.Value)
		{
			shellGone.Value = true;
			waiter = false;
			moveTowardPlayer(-1);
		}
		if (Game1.random.NextDouble() < missChance.Value - missChance.Value * addedPrecision)
		{
			actualDamage = -1;
		}
		else if (Sprite.currentFrame % 4 == 0 && !shellGone)
		{
			actualDamage = 0;
			base.currentLocation.playSound("crafting");
		}
		else
		{
			base.Health -= actualDamage;
			base.Slipperiness = 3;
			setTrajectory(xTrajectory, yTrajectory);
			base.currentLocation.playSound("hitEnemy");
			glowingColor = Color.Cyan;
			if (base.Health <= 0)
			{
				base.currentLocation.playSound("monsterdead");
				deathAnimation();
				Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.Red, 10)
				{
					holdLastFrame = true,
					alphaFade = 0.01f
				}, base.currentLocation);
			}
		}
		return actualDamage;
	}

	public override void update(GameTime time, GameLocation location)
	{
		if (!location.farmers.Any())
		{
			return;
		}
		if (!shellGone && !base.Player.isRafting)
		{
			base.update(time, location);
		}
		else if (!base.Player.isRafting)
		{
			if (Game1.IsMasterGame)
			{
				behaviorAtGameTick(time);
			}
			updateAnimation(time);
		}
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		if (waiter && (int)shellHealth > 4)
		{
			moveTowardPlayerThreshold.Value = 0;
			return;
		}
		base.behaviorAtGameTick(time);
		if (isMoving() && Sprite.currentFrame % 4 == 0)
		{
			Sprite.currentFrame++;
			Sprite.UpdateSourceRect();
		}
		if (!withinPlayerThreshold() && !shellGone)
		{
			Halt();
		}
		else if (withinPlayerThreshold() && !shellGone && name.Equals("Truffle Crab"))
		{
			shellGone.Value = true;
		}
		else
		{
			if (!shellGone)
			{
				return;
			}
			updateGlow();
			if (invincibleCountdown > 0)
			{
				glowingColor = Color.Cyan;
				invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
				if (invincibleCountdown <= 0)
				{
					stopGlowing();
				}
			}
			base.IsWalkingTowardPlayer = false;
			_ = base.StandingPixel;
			_ = base.Player.StandingPixel;
			FacingDirection = getGeneralDirectionTowards(base.Player.getStandingPosition(), 0, opposite: true, useTileCalculations: false);
			moveUp = false;
			moveDown = false;
			moveRight = false;
			moveLeft = false;
			setMovingInFacingDirection();
			MovePosition(time, Game1.viewport, base.currentLocation);
			Sprite.CurrentFrame = 16 + Sprite.currentFrame % 4;
		}
	}

	protected override void updateMonsterSlaveAnimation(GameTime time)
	{
		if (isMoving())
		{
			switch (FacingDirection)
			{
			case 0:
				Sprite.AnimateUp(time);
				break;
			case 3:
				Sprite.AnimateLeft(time);
				break;
			case 1:
				Sprite.AnimateRight(time);
				break;
			case 2:
				Sprite.AnimateDown(time);
				break;
			}
		}
		else
		{
			Sprite.StopAnimation();
		}
		if (isMoving() && Sprite.currentFrame % 4 == 0)
		{
			Sprite.currentFrame++;
			Sprite.UpdateSourceRect();
		}
		if (!shellGone)
		{
			return;
		}
		updateGlow();
		if (invincibleCountdown > 0)
		{
			glowingColor = Color.Cyan;
			invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
			if (invincibleCountdown <= 0)
			{
				stopGlowing();
			}
		}
		Sprite.currentFrame = 16 + Sprite.currentFrame % 4;
	}
}
