using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley.Monsters;

public class AngryRoger : Monster
{
	public const float rotationIncrement = (float)Math.PI / 64f;

	private int wasHitCounter;

	private float targetRotation;

	private bool turningRight;

	private int identifier = Game1.random.Next(-99999, 99999);

	private new int yOffset;

	private int yOffsetExtra;

	public AngryRoger()
	{
	}

	public AngryRoger(Vector2 position)
		: base("Ghost", position)
	{
		base.Slipperiness = 8;
		isGlider.Value = true;
		base.HideShadow = true;
	}

	/// <summary>
	/// constructor for non-default ghosts
	/// </summary>
	/// <param name="position"></param>
	/// <param name="name"></param>
	public AngryRoger(Vector2 position, string name)
		: base(name, position)
	{
		base.Slipperiness = 8;
		isGlider.Value = true;
		base.HideShadow = true;
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		Sprite = new AnimatedSprite("Characters\\Monsters\\" + name);
	}

	public override void drawAboveAllLayers(SpriteBatch b)
	{
		int standingY = base.StandingPixel.Y;
		b.Draw(Sprite.Texture, getLocalPosition(Game1.viewport) + new Vector2(32f, 21 + yOffset), Sprite.SourceRect, Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, scale.Value) * 4f, flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, drawOnTop ? 0.991f : ((float)standingY / 10000f)));
		b.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + (float)yOffset / 20f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
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
				Game1.currentLightSources.Add(new LightSource(5, new Vector2(base.Position.X + 8f, base.Position.Y + 64f), 1f, Color.White * 0.7f, identifier, LightSource.LightContext.None, 0L));
			}
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
		faceGeneralDirection(base.Player.getStandingPosition());
		resetAnimationSpeed();
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		base.behaviorAtGameTick(time);
		Microsoft.Xna.Framework.Rectangle monsterBounds = GetBoundingBox();
		Microsoft.Xna.Framework.Rectangle playerBounds = base.Player.GetBoundingBox();
		if (!monsterBounds.Intersects(playerBounds) || !base.Player.temporarilyInvincible)
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
