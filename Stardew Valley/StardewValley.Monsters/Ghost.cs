using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Projectiles;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley.Monsters;

public class Ghost : Monster
{
	public enum GhostVariant
	{
		Normal,
		Putrid
	}

	public const float rotationIncrement = (float)Math.PI / 64f;

	private int wasHitCounter;

	private float targetRotation;

	private bool turningRight;

	private int identifier = Game1.random.Next(-99999, 99999);

	private new int yOffset;

	private int yOffsetExtra;

	public NetInt currentState = new NetInt(0);

	public float stateTimer = -1f;

	public float nextParticle;

	public NetEnum<GhostVariant> variant = new NetEnum<GhostVariant>(GhostVariant.Normal);

	public Ghost()
	{
	}

	public Ghost(Vector2 position)
		: base("Ghost", position)
	{
		base.Slipperiness = 8;
		isGlider.Value = true;
		base.HideShadow = true;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(variant, "variant").AddField(currentState, "currentState");
		currentState.fieldChangeVisibleEvent += delegate
		{
			stateTimer = -1f;
		};
	}

	/// <summary>
	/// constructor for non-default ghosts
	/// </summary>
	/// <param name="position"></param>
	/// <param name="name"></param>
	public Ghost(Vector2 position, string name)
		: base(name, position)
	{
		base.Slipperiness = 8;
		isGlider.Value = true;
		base.HideShadow = true;
		if (name == "Putrid Ghost")
		{
			variant.Value = GhostVariant.Putrid;
		}
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		Sprite = new AnimatedSprite("Characters\\Monsters\\" + name);
	}

	public override int GetBaseDifficultyLevel()
	{
		if (variant.Value == GhostVariant.Putrid)
		{
			return 1;
		}
		return base.GetBaseDifficultyLevel();
	}

	public override List<Item> getExtraDropItems()
	{
		if (Game1.random.NextDouble() < 0.095 && Game1.player.team.SpecialOrderActive("Wizard") && !Game1.MasterPlayer.hasOrWillReceiveMail("ectoplasmDrop"))
		{
			Object o = ItemRegistry.Create<Object>("(O)875");
			o.specialItem = true;
			o.questItem.Value = true;
			return new List<Item> { o };
		}
		return base.getExtraDropItems();
	}

	public override void drawAboveAllLayers(SpriteBatch b)
	{
		int standingY = base.StandingPixel.Y;
		b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 21 + yOffset), Sprite.SourceRect, Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)standingY / 10000f)));
		b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + (float)yOffset / 20f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		if (variant.Value == GhostVariant.Putrid && currentState.Value <= 2)
		{
			currentState.Value = 0;
		}
		int actualDamage = Math.Max(1, damage - (int)resilience);
		base.Slipperiness = 8;
		Utility.addSprinklesToLocation(base.currentLocation, base.TilePoint.X, base.TilePoint.Y, 2, 2, 101, 50, Color.LightBlue);
		if (Game1.random.NextDouble() < missChance.Value - missChance.Value * addedPrecision)
		{
			actualDamage = -1;
		}
		else
		{
			base.Health -= actualDamage;
			if (base.Health <= 0)
			{
				deathAnimation();
			}
			setTrajectory(xTrajectory, yTrajectory);
		}
		addedSpeed = -1f;
		Utility.removeLightSource(identifier);
		return actualDamage;
	}

	protected override void localDeathAnimation()
	{
		base.currentLocation.localSound("ghost");
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(Sprite.textureName, new Microsoft.Xna.Framework.Rectangle(0, 96, 16, 24), 100f, 4, 0, base.Position, flicker: false, flipped: false, 0.9f, 0.001f, Color.White, 4f, 0.01f, 0f, (float)Math.PI / 64f));
	}

	protected override void sharedDeathAnimation()
	{
	}

	protected override void updateAnimation(GameTime time)
	{
		nextParticle -= (float)time.ElapsedGameTime.TotalSeconds;
		if (nextParticle <= 0f)
		{
			nextParticle = 1f;
			if (variant.Value == GhostVariant.Putrid)
			{
				if (currentLocationRef.Value != null)
				{
					int standingY = base.StandingPixel.Y;
					TemporaryAnimatedSprite drip = new TemporaryAnimatedSprite(Sprite.textureName, new Microsoft.Xna.Framework.Rectangle(Game1.random.Next(4) * 16, 168, 16, 24), 100f, 1, 10, base.Position + new Vector2(Utility.RandomFloat(-16f, 16f), Utility.RandomFloat(-16f, 0f) - (float)yOffset), flicker: false, flipped: false, (float)standingY / 10000f, 0.01f, Color.White, 4f, -0.01f, 0f, 0f);
					drip.acceleration = new Vector2(0f, 0.025f);
					base.currentLocation.temporarySprites.Add(drip);
				}
				nextParticle = Utility.RandomFloat(0.3f, 0.5f);
			}
		}
		yOffset = (int)(Math.Sin((double)((float)time.TotalGameTime.Milliseconds / 1000f) * (Math.PI * 2.0)) * 20.0) - yOffsetExtra;
		if (base.currentLocation == Game1.currentLocation)
		{
			bool wasFound = false;
			foreach (LightSource l in Game1.currentLightSources)
			{
				if ((int)l.identifier == identifier)
				{
					l.position.Value = new Vector2(base.Position.X + 32f, base.Position.Y + 64f + (float)yOffset);
					wasFound = true;
				}
			}
			if (!wasFound)
			{
				if (name == "Carbon Ghost")
				{
					Game1.currentLightSources.Add(new LightSource(4, new Vector2(base.Position.X + 8f, base.Position.Y + 64f), 1f, new Color(80, 30, 0), identifier, LightSource.LightContext.None, 0L));
				}
				else
				{
					Game1.currentLightSources.Add(new LightSource(5, new Vector2(base.Position.X + 8f, base.Position.Y + 64f), 1f, Color.White * 0.7f, identifier, LightSource.LightContext.None, 0L));
				}
			}
		}
		if (variant.Value == GhostVariant.Putrid && UpdateVariantAnimation(time))
		{
			return;
		}
		Point monsterPixel = base.StandingPixel;
		Point standingPixel = base.Player.StandingPixel;
		float xSlope = -(standingPixel.X - monsterPixel.X);
		float ySlope = standingPixel.Y - monsterPixel.Y;
		float t = 400f;
		xSlope /= t;
		ySlope /= t;
		if (wasHitCounter <= 0)
		{
			targetRotation = (float)Math.Atan2(0f - ySlope, xSlope) - (float)Math.PI / 2f;
			if ((double)(Math.Abs(targetRotation) - Math.Abs(rotation)) > Math.PI * 7.0 / 8.0 && Game1.random.NextBool())
			{
				turningRight = true;
			}
			else if ((double)(Math.Abs(targetRotation) - Math.Abs(rotation)) < Math.PI / 8.0)
			{
				turningRight = false;
			}
			if (turningRight)
			{
				rotation -= (float)Math.Sign(targetRotation - rotation) * ((float)Math.PI / 64f);
			}
			else
			{
				rotation += (float)Math.Sign(targetRotation - rotation) * ((float)Math.PI / 64f);
			}
			rotation %= (float)Math.PI * 2f;
			wasHitCounter = 0;
		}
		float maxAccel = Math.Min(4f, Math.Max(1f, 5f - t / 64f / 2f));
		xSlope = (float)Math.Cos((double)rotation + Math.PI / 2.0);
		ySlope = 0f - (float)Math.Sin((double)rotation + Math.PI / 2.0);
		xVelocity += (0f - xSlope) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
		yVelocity += (0f - ySlope) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
		if (Math.Abs(xVelocity) > Math.Abs((0f - xSlope) * 5f))
		{
			xVelocity -= (0f - xSlope) * maxAccel / 6f;
		}
		if (Math.Abs(yVelocity) > Math.Abs((0f - ySlope) * 5f))
		{
			yVelocity -= (0f - ySlope) * maxAccel / 6f;
		}
		faceGeneralDirection(base.Player.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
		resetAnimationSpeed();
	}

	public virtual bool UpdateVariantAnimation(GameTime time)
	{
		if (variant.Value == GhostVariant.Putrid)
		{
			if (currentState.Value == 0)
			{
				if (Sprite.CurrentFrame >= 20)
				{
					Sprite.CurrentFrame = 0;
				}
				return false;
			}
			if (currentState.Value >= 1 && currentState.Value <= 3)
			{
				shakeTimer = 250;
				if (base.Player != null)
				{
					faceGeneralDirection(base.Player.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
				}
				switch (FacingDirection)
				{
				case 2:
					Sprite.CurrentFrame = 20;
					break;
				case 1:
					Sprite.CurrentFrame = 21;
					break;
				case 0:
					Sprite.CurrentFrame = 22;
					break;
				case 3:
					Sprite.CurrentFrame = 23;
					break;
				}
			}
			else if (currentState.Value >= 4)
			{
				shakeTimer = 250;
				switch (FacingDirection)
				{
				case 2:
					Sprite.CurrentFrame = 24;
					break;
				case 1:
					Sprite.CurrentFrame = 25;
					break;
				case 0:
					Sprite.CurrentFrame = 26;
					break;
				case 3:
					Sprite.CurrentFrame = 27;
					break;
				}
			}
			return true;
		}
		return false;
	}

	public override void noMovementProgressNearPlayerBehavior()
	{
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		if (stateTimer > 0f)
		{
			stateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
			if (stateTimer <= 0f)
			{
				stateTimer = 0f;
			}
		}
		if (variant.Value == GhostVariant.Putrid)
		{
			Farmer player = base.Player;
			switch (currentState.Value)
			{
			case 0:
				if (stateTimer == -1f)
				{
					stateTimer = Utility.RandomFloat(1f, 2f);
				}
				if (player != null && stateTimer == 0f && Math.Abs(player.Position.X - base.Position.X) < 448f && Math.Abs(player.Position.Y - base.Position.Y) < 448f)
				{
					currentState.Value = 1;
					base.currentLocation.playSound("croak");
					stateTimer = 0.5f;
				}
				break;
			case 1:
				xVelocity = 0f;
				yVelocity = 0f;
				if (stateTimer <= 0f)
				{
					currentState.Value = 2;
				}
				break;
			case 2:
			{
				if (player == null)
				{
					currentState.Value = 0;
					break;
				}
				if (Math.Abs(player.Position.X - base.Position.X) < 80f && Math.Abs(player.Position.Y - base.Position.Y) < 80f)
				{
					currentState.Value = 3;
					stateTimer = 0.05f;
					xVelocity = 0f;
					yVelocity = 0f;
					break;
				}
				Vector2 offset = player.getStandingPosition() - getStandingPosition();
				if (offset.LengthSquared() == 0f)
				{
					currentState.Value = 3;
					stateTimer = 0.15f;
					break;
				}
				offset.Normalize();
				offset *= 10f;
				xVelocity = offset.X;
				yVelocity = 0f - offset.Y;
				break;
			}
			case 3:
				xVelocity = 0f;
				yVelocity = 0f;
				if (stateTimer <= 0f)
				{
					currentState.Value = 4;
					stateTimer = 1f;
					Vector2 shot_velocity = FacingDirection switch
					{
						0 => new Vector2(0f, -1f), 
						3 => new Vector2(-1f, 0f), 
						1 => new Vector2(1f, 0f), 
						2 => new Vector2(0f, 1f), 
						_ => Vector2.Zero, 
					};
					shot_velocity *= 6f;
					base.currentLocation.playSound("fishSlap");
					BasicProjectile projectile = new BasicProjectile(base.DamageToFarmer, 7, 0, 1, (float)Math.PI / 32f, shot_velocity.X, shot_velocity.Y, base.Position, null, null, null, explode: false, damagesMonsters: false, base.currentLocation, this);
					projectile.debuff.Value = "25";
					projectile.scaleGrow.Value = 0.05f;
					projectile.ignoreTravelGracePeriod.Value = true;
					projectile.IgnoreLocationCollision = true;
					projectile.maxTravelDistance.Value = 192;
					base.currentLocation.projectiles.Add(projectile);
				}
				break;
			case 4:
				if (stateTimer <= 0f)
				{
					xVelocity = 0f;
					yVelocity = 0f;
					currentState.Value = 0;
					stateTimer = Utility.RandomFloat(3f, 4f);
				}
				break;
			}
		}
		base.behaviorAtGameTick(time);
		Microsoft.Xna.Framework.Rectangle playerBounds = base.Player.GetBoundingBox();
		if (!GetBoundingBox().Intersects(playerBounds) || !base.Player.temporarilyInvincible || currentState.Value != 0)
		{
			return;
		}
		Layer backLayer = base.currentLocation.map.RequireLayer("Back");
		Point playerCenter = playerBounds.Center;
		int attempts = 0;
		Vector2 attemptedPosition = new Vector2(playerCenter.X / 64 + Game1.random.Next(-12, 12), playerCenter.Y / 64 + Game1.random.Next(-12, 12));
		for (; attempts < 3; attempts++)
		{
			if (!(attemptedPosition.X >= (float)backLayer.LayerWidth) && !(attemptedPosition.Y >= (float)backLayer.LayerHeight) && !(attemptedPosition.X < 0f) && !(attemptedPosition.Y < 0f) && backLayer.Tiles[(int)attemptedPosition.X, (int)attemptedPosition.Y] != null && base.currentLocation.isTilePassable(new Location((int)attemptedPosition.X, (int)attemptedPosition.Y), Game1.viewport) && !attemptedPosition.Equals(new Vector2(playerCenter.X / 64, playerCenter.Y / 64)))
			{
				break;
			}
			attemptedPosition = new Vector2(playerCenter.X / 64 + Game1.random.Next(-12, 12), playerCenter.Y / 64 + Game1.random.Next(-12, 12));
		}
		if (attempts < 3)
		{
			base.Position = new Vector2(attemptedPosition.X * 64f, attemptedPosition.Y * 64f - 32f);
			Halt();
		}
	}
}
