using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace StardewValley.BellsAndWhistles;

public class Opossum : Critter
{
	private int characterCheckTimer = 1500;

	private bool running;

	private int jumpTimer = -1;

	public Opossum(GameLocation location, Vector2 position, bool flip)
	{
		characterCheckTimer = Game1.random.Next(500, 3000);
		base.position = position * 64f;
		position.Y += 48f;
		base.flip = flip;
		baseFrame = 150;
		sprite = new AnimatedSprite(Critter.critterTexture, 150, 32, 32);
		sprite.loop = true;
		sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
		{
			new FarmerSprite.AnimationFrame(baseFrame, 500),
			new FarmerSprite.AnimationFrame(baseFrame + 1, 50),
			new FarmerSprite.AnimationFrame(baseFrame + 2, 500),
			new FarmerSprite.AnimationFrame(baseFrame + 1, 50),
			new FarmerSprite.AnimationFrame(baseFrame, 1000),
			new FarmerSprite.AnimationFrame(baseFrame + 1, 50),
			new FarmerSprite.AnimationFrame(baseFrame + 2, 700),
			new FarmerSprite.AnimationFrame(baseFrame + 1, 50)
		});
		startingPosition = position;
	}

	public override bool update(GameTime time, GameLocation environment)
	{
		characterCheckTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		if (Utility.isThereAFarmerOrCharacterWithinDistance(position / 64f, 8, environment) != null)
		{
			characterCheckTimer = 0;
		}
		if (jumpTimer > -1)
		{
			jumpTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			yJumpOffset = (0f - (float)Math.Sin((double)((600f - (float)jumpTimer) / 600f) * Math.PI)) * 4f * 16f;
			if (jumpTimer <= -1)
			{
				running = true;
				sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(baseFrame + 5, 40),
					new FarmerSprite.AnimationFrame(baseFrame + 6, 40),
					new FarmerSprite.AnimationFrame(baseFrame + 7, 40),
					new FarmerSprite.AnimationFrame(baseFrame + 8, 40)
				});
				sprite.loop = true;
			}
		}
		else if (characterCheckTimer <= 0 && !running)
		{
			if (Utility.isOnScreen(position, -32) && jumpTimer == -1)
			{
				jumpTimer = 600;
				sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(baseFrame + 4, 20)
				});
			}
			characterCheckTimer = 200;
		}
		if (running)
		{
			position.X += (flip ? (-6) : 6);
		}
		if (running && characterCheckTimer <= 0)
		{
			characterCheckTimer = 200;
			if (environment.largeTerrainFeatures != null)
			{
				Rectangle tileRect = new Rectangle((int)position.X + 32, (int)position.Y - 32, 4, 192);
				foreach (LargeTerrainFeature f in environment.largeTerrainFeatures)
				{
					if (f is Bush bush && f.getBoundingBox().Intersects(tileRect))
					{
						bush.performUseAction(f.Tile);
						return true;
					}
				}
			}
		}
		return base.update(time, environment);
	}
}
