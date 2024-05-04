using System;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewValley.Pathfinding;
using xTile.Layers;

namespace StardewValley.Monsters;

public class ShadowGirl : Monster
{
	public const int blockTimeBeforePathfinding = 500;

	private new Vector2 lastPosition = Vector2.Zero;

	private int howLongOnThisPosition;

	public ShadowGirl()
	{
	}

	public ShadowGirl(Vector2 position)
		: base("Shadow Girl", position)
	{
		base.IsWalkingTowardPlayer = false;
		moveTowardPlayerThreshold.Value = 8;
		if (Game1.MasterPlayer.friendshipData.TryGetValue("???", out var friendship) && friendship.Points >= 1250)
		{
			base.DamageToFarmer = 0;
		}
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		Sprite = new AnimatedSprite("Characters\\Monsters\\Shadow Girl");
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		int actualDamage = Math.Max(1, damage - (int)resilience);
		if (Game1.random.NextDouble() < missChance.Value - missChance.Value * addedPrecision)
		{
			actualDamage = -1;
		}
		else
		{
			base.Health -= actualDamage;
			setTrajectory(xTrajectory, yTrajectory);
			if (base.Health <= 0)
			{
				deathAnimation();
			}
		}
		return actualDamage;
	}

	protected override void localDeathAnimation()
	{
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(45, base.Position, Color.White, 10));
	}

	protected override void sharedDeathAnimation()
	{
		Point standingPixel = base.StandingPixel;
		Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(Sprite.SourceRect.X, Sprite.SourceRect.Y, 64, 21), 64, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White);
		Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(Sprite.SourceRect.X + 10, Sprite.SourceRect.Y + 21, 64, 21), 42, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White);
	}

	public override void update(GameTime time, GameLocation location)
	{
		if (!location.farmers.Any())
		{
			return;
		}
		if (!base.Player.isRafting || !withinPlayerThreshold(4))
		{
			updateGlow();
			updateEmote(time);
			if (controller == null)
			{
				updateMovement(location, time);
			}
			if (controller != null && controller.update(time))
			{
				controller = null;
			}
		}
		behaviorAtGameTick(time);
		Layer backLayer = location.map.RequireLayer("Back");
		if (base.Position.X < 0f || base.Position.X > (float)(backLayer.LayerWidth * 64) || base.Position.Y < 0f || base.Position.Y > (float)(backLayer.LayerHeight * 64))
		{
			location.characters.Remove(this);
		}
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		base.behaviorAtGameTick(time);
		addedSpeed = 0f;
		base.speed = 3;
		if (howLongOnThisPosition > 500 && controller == null)
		{
			base.IsWalkingTowardPlayer = false;
			controller = new PathFindController(this, base.currentLocation, new Point(base.Player.TilePoint.X, base.Player.TilePoint.Y), Game1.random.Next(4), null, 300);
			timeBeforeAIMovementAgain = 2000f;
			howLongOnThisPosition = 0;
		}
		else if (controller == null)
		{
			base.IsWalkingTowardPlayer = true;
		}
		if (base.Position.Equals(lastPosition))
		{
			howLongOnThisPosition += time.ElapsedGameTime.Milliseconds;
		}
		else
		{
			howLongOnThisPosition = 0;
		}
		lastPosition = base.Position;
	}
}
