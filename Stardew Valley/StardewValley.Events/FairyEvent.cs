using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;

namespace StardewValley.Events;

public class FairyEvent : BaseFarmEvent
{
	public const int identifier = 942069;

	private Vector2 fairyPosition;

	private Vector2 targetCrop;

	private Farm f;

	private int fairyFrame;

	private int fairyAnimationTimer;

	private int animationLoopsDone;

	private int timerSinceFade;

	private bool animateLeft;

	private bool terminate;

	/// <inheritdoc />
	public override bool setUp()
	{
		f = Game1.getFarm();
		if (f.IsRainingHere())
		{
			return true;
		}
		targetCrop = ChooseCrop();
		if (targetCrop == Vector2.Zero)
		{
			return true;
		}
		Game1.currentLocation.cleanupBeforePlayerExit();
		Game1.currentLightSources.Add(new LightSource(4, fairyPosition, 1f, Color.Black, 942069, LightSource.LightContext.None, 0L));
		Game1.currentLocation = f;
		f.resetForPlayerEntry();
		Game1.fadeClear();
		Game1.nonWarpFade = true;
		Game1.timeOfDay = 2400;
		Game1.displayHUD = false;
		Game1.freezeControls = true;
		Game1.viewportFreeze = true;
		Game1.displayFarmer = false;
		Game1.viewport.X = Math.Max(0, Math.Min(f.map.DisplayWidth - Game1.viewport.Width, (int)targetCrop.X * 64 - Game1.viewport.Width / 2));
		Game1.viewport.Y = Math.Max(0, Math.Min(f.map.DisplayHeight - Game1.viewport.Height, (int)targetCrop.Y * 64 - Game1.viewport.Height / 2));
		fairyPosition = new Vector2(Game1.viewport.X + Game1.viewport.Width + 128, targetCrop.Y * 64f - 64f);
		Game1.changeMusicTrack("nightTime");
		return false;
	}

	/// <inheritdoc />
	public override bool tickUpdate(GameTime time)
	{
		if (terminate)
		{
			return true;
		}
		Game1.UpdateGameClock(time);
		f.UpdateWhenCurrentLocation(time);
		f.updateEvenIfFarmerIsntHere(time);
		Game1.UpdateOther(time);
		Utility.repositionLightSource(942069, fairyPosition + new Vector2(32f, 32f));
		if (animationLoopsDone < 1)
		{
			timerSinceFade += time.ElapsedGameTime.Milliseconds;
		}
		if (fairyPosition.X > targetCrop.X * 64f + 32f)
		{
			if (timerSinceFade < 2000)
			{
				return false;
			}
			fairyPosition.X -= (float)time.ElapsedGameTime.Milliseconds * 0.1f;
			fairyPosition.Y += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * Math.PI / 512.0) * 1f;
			int num = fairyFrame;
			if (time.TotalGameTime.Milliseconds % 500 > 250)
			{
				fairyFrame = 1;
			}
			else
			{
				fairyFrame = 0;
			}
			if (num != fairyFrame && fairyFrame == 1)
			{
				Game1.playSound("batFlap");
				f.temporarySprites.Add(new TemporaryAnimatedSprite(11, fairyPosition + new Vector2(32f, 0f), Color.Purple));
			}
			if (fairyPosition.X <= targetCrop.X * 64f + 32f)
			{
				fairyFrame = 1;
			}
		}
		else if (animationLoopsDone < 4)
		{
			fairyAnimationTimer += time.ElapsedGameTime.Milliseconds;
			if (fairyAnimationTimer > 250)
			{
				fairyAnimationTimer = 0;
				if (!animateLeft)
				{
					fairyFrame++;
					if (fairyFrame == 3)
					{
						animateLeft = true;
						f.temporarySprites.Add(new TemporaryAnimatedSprite(10, fairyPosition + new Vector2(-16f, 64f), Color.LightPink));
						Game1.playSound("yoba");
						if (f.terrainFeatures.TryGetValue(targetCrop, out var terrainFeature) && terrainFeature is HoeDirt dirt)
						{
							dirt.crop.currentPhase.Value = Math.Min((int)dirt.crop.currentPhase + 1, dirt.crop.phaseDays.Count - 1);
						}
					}
				}
				else
				{
					fairyFrame--;
					if (fairyFrame == 1)
					{
						animateLeft = false;
						animationLoopsDone++;
						if (animationLoopsDone >= 4)
						{
							for (int i = 0; i < 10; i++)
							{
								DelayedAction.playSoundAfterDelay("batFlap", 4000 + 500 * i);
							}
						}
					}
				}
			}
		}
		else
		{
			fairyAnimationTimer += time.ElapsedGameTime.Milliseconds;
			if (time.TotalGameTime.Milliseconds % 500 > 250)
			{
				fairyFrame = 1;
			}
			else
			{
				fairyFrame = 0;
			}
			if (fairyAnimationTimer > 2000 && fairyPosition.Y > -999999f)
			{
				fairyPosition.X += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * Math.PI / 256.0) * 2f;
				fairyPosition.Y -= (float)time.ElapsedGameTime.Milliseconds * 0.2f;
			}
			if (fairyPosition.Y < (float)(Game1.viewport.Y - 128) || float.IsNaN(fairyPosition.Y))
			{
				if (!Game1.fadeToBlack && fairyPosition.Y != -999999f)
				{
					Game1.globalFadeToBlack(afterLastFade);
					Game1.changeMusicTrack("none");
					timerSinceFade = 0;
					fairyPosition.Y = -999999f;
				}
				timerSinceFade += time.ElapsedGameTime.Milliseconds;
			}
		}
		return false;
	}

	public void afterLastFade()
	{
		terminate = true;
		Game1.globalFadeToClear();
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, fairyPosition), new Rectangle(16 + fairyFrame * 16, 592, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9999999f);
	}

	/// <inheritdoc />
	public override void makeChangesToLocation()
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		for (int x = (int)targetCrop.X - 2; (float)x <= targetCrop.X + 2f; x++)
		{
			for (int y = (int)targetCrop.Y - 2; (float)y <= targetCrop.Y + 2f; y++)
			{
				Vector2 v = new Vector2(x, y);
				if (f.terrainFeatures.TryGetValue(v, out var terrainFeature) && terrainFeature is HoeDirt { crop: not null } dirt)
				{
					dirt.crop.growCompletely();
				}
			}
		}
	}

	/// <summary>Choose a random valid crop to target.</summary>
	protected Vector2 ChooseCrop()
	{
		Vector2[] validCropPositions = (from p in f.terrainFeatures.Pairs
			where p.Value is HoeDirt { crop: not null } hoeDirt && !hoeDirt.crop.dead && !hoeDirt.crop.isWildSeedCrop() && (int)hoeDirt.crop.currentPhase < hoeDirt.crop.phaseDays.Count - 1
			orderby p.Key.X, p.Key.Y
			select p.Key).ToArray();
		return Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed).ChooseFrom(validCropPositions);
	}
}
