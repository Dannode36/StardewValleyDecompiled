using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.BellsAndWhistles;

public abstract class Critter
{
	public const int spriteWidth = 32;

	public const int spriteHeight = 32;

	public const float gravity = 0.25f;

	public static string critterTexture = "TileSheets\\critters";

	public Vector2 position;

	public Vector2 startingPosition;

	public int baseFrame;

	public AnimatedSprite sprite;

	public bool flip;

	public float gravityAffectedDY;

	public float yOffset;

	public float yJumpOffset;

	public Critter()
	{
	}

	public Critter(int baseFrame, Vector2 position)
	{
		this.baseFrame = baseFrame;
		this.position = position;
		sprite = new AnimatedSprite(critterTexture, baseFrame, 32, 32);
		startingPosition = position;
	}

	public virtual Rectangle getBoundingBox(int xOffset, int yOffset)
	{
		return new Rectangle((int)position.X - 32 + xOffset, (int)position.Y - 16 + yOffset, 64, 32);
	}

	public virtual bool update(GameTime time, GameLocation environment)
	{
		sprite.animateOnce(time);
		if (gravityAffectedDY < 0f || yJumpOffset < 0f)
		{
			yJumpOffset += gravityAffectedDY;
			gravityAffectedDY += 0.25f;
		}
		if (position.X < -128f || position.Y < -128f || position.X > (float)environment.map.DisplayWidth || position.Y > (float)environment.map.DisplayHeight)
		{
			return true;
		}
		return false;
	}

	public virtual void draw(SpriteBatch b)
	{
		if (sprite != null)
		{
			sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(-64f, -128f + yJumpOffset + yOffset)), position.Y / 10000f + position.X / 1000000f, 0, 0, Color.White, flip, 4f);
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(0f, -4f)), Game1.shadowTexture.Bounds, Color.White * (1f - Math.Min(1f, Math.Abs((yJumpOffset + yOffset) / 64f))), 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (yJumpOffset + yOffset) / 64f), SpriteEffects.None, (position.Y - 1f) / 10000f);
		}
	}

	public virtual void drawAboveFrontLayer(SpriteBatch b)
	{
	}
}
