using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace StardewValley.BellsAndWhistles;

public class Rabbit : Critter
{
	private int characterCheckTimer = 200;

	private bool running;

	public Rabbit(GameLocation location, Vector2 position, bool flip)
	{
		bool isWinter = location.IsWinterHere();
		base.position = position * 64f;
		position.Y += 48f;
		base.flip = flip;
		baseFrame = (isWinter ? 74 : 54);
		sprite = new AnimatedSprite(Critter.critterTexture, isWinter ? 69 : 68, 32, 32);
		sprite.loop = true;
		startingPosition = position;
	}

	public override bool update(GameTime time, GameLocation environment)
	{
		characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
		if (characterCheckTimer <= 0 && !running)
		{
			if (Utility.isOnScreen(position, -32))
			{
				running = true;
				sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(baseFrame, 40),
					new FarmerSprite.AnimationFrame(baseFrame + 1, 40),
					new FarmerSprite.AnimationFrame(baseFrame + 2, 40),
					new FarmerSprite.AnimationFrame(baseFrame + 3, 100),
					new FarmerSprite.AnimationFrame(baseFrame + 5, 70),
					new FarmerSprite.AnimationFrame(baseFrame + 5, 40)
				});
				sprite.loop = true;
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
