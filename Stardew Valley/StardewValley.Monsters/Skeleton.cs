using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Pathfinding;
using StardewValley.Projectiles;

namespace StardewValley.Monsters;

public class Skeleton : Monster
{
	private bool spottedPlayer;

	private readonly NetBool throwing = new NetBool();

	public readonly NetBool isMage = new NetBool();

	private int controllerAttemptTimer;

	public Skeleton()
	{
	}

	public Skeleton(Vector2 position, bool isMage = false)
		: base("Skeleton", position, Game1.random.Next(4))
	{
		this.isMage.Value = isMage;
		reloadSprite();
		Sprite.SpriteHeight = 32;
		Sprite.UpdateSourceRect();
		base.IsWalkingTowardPlayer = false;
		jitteriness.Value = 0.0;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(throwing, "throwing").AddField(isMage, "isMage");
		position.Field.AxisAlignedMovement = true;
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		Sprite = new AnimatedSprite("Characters\\Monsters\\Skeleton" + (isMage ? " Mage" : ""));
		Sprite.SpriteHeight = 32;
		Sprite.UpdateSourceRect();
	}

	public override List<Item> getExtraDropItems()
	{
		List<Item> extra = new List<Item>();
		if (Game1.random.NextDouble() < 0.04)
		{
			extra.Add(ItemRegistry.Create("(W)5"));
		}
		return extra;
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		base.currentLocation.playSound("skeletonHit");
		base.Slipperiness = 3;
		if ((bool)throwing)
		{
			throwing.Value = false;
			Halt();
		}
		if (base.Health - damage <= 0)
		{
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(46, base.Position, Color.White, 10, flipped: false, 70f));
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(46, base.Position + new Vector2(-16f, 0f), Color.White, 10, flipped: false, 70f)
			{
				delayBeforeAnimationStart = 100
			});
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(46, base.Position + new Vector2(16f, 0f), Color.White, 10, flipped: false, 70f)
			{
				delayBeforeAnimationStart = 200
			});
		}
		return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
	}

	public override void shedChunks(int number)
	{
		Point standingPixel = base.StandingPixel;
		Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(0, 128, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, 4f);
	}

	public override void BuffForAdditionalDifficulty(int additional_difficulty)
	{
		base.BuffForAdditionalDifficulty(additional_difficulty);
		if (!isMage)
		{
			base.MaxHealth += 300;
			base.Health += 300;
		}
	}

	protected override void sharedDeathAnimation()
	{
		Point standingPixel = base.StandingPixel;
		base.currentLocation.playSound("skeletonDie");
		shedChunks(20);
		Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(3, Game1.random.Choose(3, 35), 10, 10), 11, standingPixel.X, standingPixel.Y, 1, base.TilePoint.Y, Color.White, 4f);
	}

	public override void update(GameTime time, GameLocation location)
	{
		if (!throwing)
		{
			base.update(time, location);
			return;
		}
		if (Game1.IsMasterGame)
		{
			behaviorAtGameTick(time);
		}
		updateAnimation(time);
	}

	protected override void updateMonsterSlaveAnimation(GameTime time)
	{
		if ((bool)throwing)
		{
			if (invincibleCountdown > 0)
			{
				invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
				if (invincibleCountdown <= 0)
				{
					stopGlowing();
				}
			}
			if (Sprite.Animate(time, 20, 4, 150f))
			{
				Sprite.currentFrame = 23;
			}
		}
		else if (isMoving())
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
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		if (!throwing)
		{
			base.behaviorAtGameTick(time);
		}
		if (!spottedPlayer && !base.wildernessFarmMonster && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, base.Tile, base.Player.Tile, 8))
		{
			controller = new PathFindController(this, base.currentLocation, base.Player.TilePoint, -1, null, 200);
			spottedPlayer = true;
			if (controller == null || controller.pathToEndPoint == null || controller.pathToEndPoint.Count == 0)
			{
				Halt();
				facePlayer(base.Player);
			}
			base.currentLocation.playSound("skeletonStep");
			base.IsWalkingTowardPlayer = true;
		}
		else if ((bool)throwing)
		{
			if (invincibleCountdown > 0)
			{
				invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
				if (invincibleCountdown <= 0)
				{
					stopGlowing();
				}
			}
			if (Sprite.Animate(time, 20, 4, 150f))
			{
				throwing.Value = false;
				Sprite.currentFrame = 0;
				faceDirection(2);
				Vector2 v = Utility.getVelocityTowardPlayer(new Point((int)base.Position.X, (int)base.Position.Y), 8f, base.Player);
				if (isMage.Value)
				{
					if (Game1.random.NextBool())
					{
						base.currentLocation.projectiles.Add(new DebuffingProjectile("19", 14, 4, 4, (float)Math.PI / 16f, v.X, v.Y, new Vector2(base.Position.X, base.Position.Y), base.currentLocation, this));
					}
					else
					{
						base.currentLocation.projectiles.Add(new BasicProjectile(base.DamageToFarmer * 2, 9, 0, 4, 0f, v.X, v.Y, new Vector2(base.Position.X, base.Position.Y), "flameSpellHit", "flameSpell", null, explode: false, damagesMonsters: false, base.currentLocation, this));
					}
				}
				else
				{
					base.currentLocation.projectiles.Add(new BasicProjectile(base.DamageToFarmer, 4, 0, 0, (float)Math.PI / 16f, v.X, v.Y, new Vector2(base.Position.X, base.Position.Y), "skeletonHit", "skeletonStep", null, explode: false, damagesMonsters: false, base.currentLocation, this));
				}
			}
		}
		else if (spottedPlayer && controller == null && Game1.random.NextDouble() < (isMage ? 0.009 : 0.003) && !base.wildernessFarmMonster && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, base.Tile, base.Player.Tile, 8))
		{
			throwing.Value = true;
			Halt();
			Sprite.currentFrame = 20;
			shake(750);
		}
		else if (withinPlayerThreshold(2))
		{
			controller = null;
		}
		else if (spottedPlayer && controller == null && controllerAttemptTimer <= 0)
		{
			controller = new PathFindController(this, base.currentLocation, base.Player.TilePoint, -1, null, 200);
			controllerAttemptTimer = (base.wildernessFarmMonster ? 2000 : 1000);
			if (controller == null || controller.pathToEndPoint == null || controller.pathToEndPoint.Count == 0)
			{
				Halt();
			}
		}
		else if (base.wildernessFarmMonster)
		{
			spottedPlayer = true;
			base.IsWalkingTowardPlayer = true;
		}
		controllerAttemptTimer -= time.ElapsedGameTime.Milliseconds;
	}
}
