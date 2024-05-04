using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;

namespace StardewValley.Monsters;

public class Spiker : Monster
{
	[XmlIgnore]
	public int targetDirection;

	[XmlIgnore]
	public NetBool moving = new NetBool(value: false);

	protected bool _localMoving;

	[XmlIgnore]
	public float nextMoveCheck;

	public Spiker()
	{
	}

	public Spiker(Vector2 position, int direction)
		: base("Spiker", position)
	{
		Sprite.SpriteWidth = 16;
		Sprite.SpriteHeight = 16;
		Sprite.UpdateSourceRect();
		targetDirection = direction;
		base.speed = 14;
		ignoreMovementAnimations = true;
		onCollision = collide;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(moving, "moving");
	}

	public override void update(GameTime time, GameLocation location)
	{
		base.update(time, location);
		if (moving.Value == _localMoving)
		{
			return;
		}
		_localMoving = moving.Value;
		if (_localMoving)
		{
			if (base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
			{
				Game1.playSound("parry");
			}
		}
		else if (base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
		{
			Game1.playSound("hammer");
		}
	}

	public override void draw(SpriteBatch b)
	{
		Sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, base.Position), (float)base.StandingPixel.Y / 10000f);
	}

	private void collide(GameLocation location)
	{
		Rectangle bb = nextPosition(FacingDirection);
		foreach (Farmer farmer in location.farmers)
		{
			if (farmer.GetBoundingBox().Intersects(bb))
			{
				return;
			}
		}
		if ((bool)moving)
		{
			moving.Value = false;
			targetDirection = (targetDirection + 2) % 4;
			nextMoveCheck = 0.75f;
		}
	}

	public override void updateMovement(GameLocation location, GameTime time)
	{
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		return -1;
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		if (nextMoveCheck > 0f)
		{
			nextMoveCheck -= (float)time.ElapsedGameTime.TotalSeconds;
		}
		if (nextMoveCheck <= 0f)
		{
			nextMoveCheck = 0.25f;
			foreach (Farmer farmer in base.currentLocation.farmers)
			{
				if ((targetDirection == 0 || targetDirection == 2) && Math.Abs(farmer.TilePoint.X - base.TilePoint.X) <= 1)
				{
					if (targetDirection == 0 && farmer.Position.Y < base.Position.Y)
					{
						moving.Value = true;
						break;
					}
					if (targetDirection == 2 && farmer.Position.Y > base.Position.Y)
					{
						moving.Value = true;
						break;
					}
				}
				if ((targetDirection == 3 || targetDirection == 1) && Math.Abs(farmer.TilePoint.Y - base.TilePoint.Y) <= 1)
				{
					if (targetDirection == 3 && farmer.Position.X < base.Position.X)
					{
						moving.Value = true;
						break;
					}
					if (targetDirection == 1 && farmer.Position.X > base.Position.X)
					{
						moving.Value = true;
						break;
					}
				}
			}
		}
		moveUp = false;
		moveDown = false;
		moveLeft = false;
		moveRight = false;
		if (moving.Value)
		{
			switch (targetDirection)
			{
			case 0:
				moveUp = true;
				break;
			case 2:
				moveDown = true;
				break;
			case 3:
				moveLeft = true;
				break;
			case 1:
				moveRight = true;
				break;
			}
			MovePosition(time, Game1.viewport, base.currentLocation);
		}
		faceDirection(2);
	}
}
