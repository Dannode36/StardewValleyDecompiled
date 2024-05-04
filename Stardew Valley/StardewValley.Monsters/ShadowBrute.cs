using Microsoft.Xna.Framework;

namespace StardewValley.Monsters;

public class ShadowBrute : Monster
{
	public ShadowBrute()
	{
	}

	public ShadowBrute(Vector2 position)
		: base("Shadow Brute", position)
	{
		Sprite.SpriteHeight = 32;
		Sprite.UpdateSourceRect();
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		Sprite = new AnimatedSprite("Characters\\Monsters\\Shadow Brute");
		Sprite.SpriteHeight = 32;
		Sprite.UpdateSourceRect();
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		base.currentLocation.playSound("shadowHit");
		return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
	}

	protected override void localDeathAnimation()
	{
		Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(45, base.Position, Color.White, 10), base.currentLocation);
		for (int i = 1; i < 3; i++)
		{
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(0f, 1f) * 64f * i, Color.Gray * 0.75f, 10)
			{
				delayBeforeAnimationStart = i * 159
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(0f, -1f) * 64f * i, Color.Gray * 0.75f, 10)
			{
				delayBeforeAnimationStart = i * 159
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(1f, 0f) * 64f * i, Color.Gray * 0.75f, 10)
			{
				delayBeforeAnimationStart = i * 159
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(-1f, 0f) * 64f * i, Color.Gray * 0.75f, 10)
			{
				delayBeforeAnimationStart = i * 159
			});
		}
		base.currentLocation.localSound("shadowDie");
	}

	protected override void sharedDeathAnimation()
	{
		Point standingPixel = base.StandingPixel;
		Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(Sprite.SourceRect.X, Sprite.SourceRect.Y, 16, 5), 16, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White, 4f);
		Game1.createRadialDebris(base.currentLocation, Sprite.textureName, new Rectangle(Sprite.SourceRect.X + 2, Sprite.SourceRect.Y + 5, 16, 5), 10, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White, 4f);
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
	}
}
