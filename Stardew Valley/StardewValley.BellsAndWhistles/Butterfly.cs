using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles;

public class Butterfly : Critter
{
	public const float maxSpeed = 3f;

	private int flapTimer;

	private int flapSpeed = 50;

	private Vector2 motion;

	private float motionMultiplier = 1f;

	private float prismaticCaptureTimer = -1f;

	private float prismaticSprinkleTimer;

	private bool summerButterfly;

	public bool stayInbounds;

	public bool isPrismatic;

	public bool isLit;

	private int lightID;

	public Butterfly(GameLocation location, Vector2 position, bool islandButterfly = false, bool forceSummerButterfly = false, int baseFrameOverride = -1, bool prismatic = false)
	{
		base.position = position * 64f;
		startingPosition = base.position;
		isPrismatic = prismatic;
		if (location.IsWinterHere())
		{
			baseFrame = 397;
			isLit = true;
		}
		else if (location.IsSpringHere() && !forceSummerButterfly)
		{
			baseFrame = (Game1.random.NextBool() ? (Game1.random.Next(3) * 3 + 160) : (Game1.random.Next(3) * 3 + 180));
		}
		else
		{
			baseFrame = (Game1.random.NextBool() ? (Game1.random.Next(3) * 4 + 128) : (Game1.random.Next(3) * 4 + 148));
			summerButterfly = true;
			if (Game1.random.NextDouble() < 0.05)
			{
				baseFrame = Game1.random.Next(2) * 4 + 169;
			}
			if (Game1.random.NextDouble() < 0.01)
			{
				baseFrame = Game1.random.Next(2) * 4 + 480;
			}
		}
		if (islandButterfly)
		{
			baseFrame = Game1.random.Next(4) * 4 + 364;
			summerButterfly = true;
		}
		if (baseFrameOverride != -1)
		{
			baseFrame = baseFrameOverride;
		}
		motion = new Vector2((float)(Game1.random.NextDouble() + 0.25) * 3f * (float)Game1.random.Choose(-1, 1) / 2f, (float)(Game1.random.NextDouble() + 0.5) * 3f * (float)Game1.random.Choose(-1, 1) / 2f);
		flapSpeed = Game1.random.Next(45, 80);
		sprite = new AnimatedSprite(Critter.critterTexture, baseFrame, 16, 16);
		sprite.loop = false;
		startingPosition = position;
		if (isLit)
		{
			lightID = Game1.random.Next();
			Game1.currentLightSources.Add(new LightSource(10, position + new Vector2(-30.72f, -93.44f), 0.66f, Color.Black * 0.75f, lightID, LightSource.LightContext.None, 0L));
		}
	}

	public void doneWithFlap(Farmer who)
	{
		flapTimer = 200 + Game1.random.Next(-5, 6);
	}

	public Butterfly setStayInbounds(bool stayInbounds)
	{
		this.stayInbounds = stayInbounds;
		return this;
	}

	public override bool update(GameTime time, GameLocation environment)
	{
		flapTimer -= time.ElapsedGameTime.Milliseconds;
		if (flapTimer <= 0 && sprite.CurrentAnimation == null)
		{
			motionMultiplier = 1f;
			motion.X += (float)Game1.random.Next(-80, 81) / 100f;
			motion.Y = (float)(Game1.random.NextDouble() + 0.25) * -3f / 2f;
			if (Math.Abs(motion.X) > 1.5f)
			{
				motion.X = 3f * (float)Math.Sign(motion.X) / 2f;
			}
			if (Math.Abs(motion.Y) > 3f)
			{
				motion.Y = 3f * (float)Math.Sign(motion.Y);
			}
			if (stayInbounds)
			{
				if (position.X < 128f)
				{
					motion.X = 0.8f;
				}
				if (position.Y < 192f)
				{
					motion.Y /= 2f;
					flapTimer = 1000;
				}
				if (position.X > (float)(environment.map.DisplayWidth - 128))
				{
					motion.X = -0.8f;
				}
				if (position.Y > (float)(environment.map.DisplayHeight - 128))
				{
					motion.Y = -1f;
					flapTimer = 100;
				}
			}
			if (summerButterfly)
			{
				sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(baseFrame + 1, flapSpeed),
					new FarmerSprite.AnimationFrame(baseFrame + 2, flapSpeed),
					new FarmerSprite.AnimationFrame(baseFrame + 3, flapSpeed),
					new FarmerSprite.AnimationFrame(baseFrame + 2, flapSpeed),
					new FarmerSprite.AnimationFrame(baseFrame + 1, flapSpeed),
					new FarmerSprite.AnimationFrame(baseFrame, flapSpeed, secondaryArm: false, flip: false, doneWithFlap)
				});
			}
			else
			{
				sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(baseFrame + 1, flapSpeed),
					new FarmerSprite.AnimationFrame(baseFrame + 2, flapSpeed),
					new FarmerSprite.AnimationFrame(baseFrame + 1, flapSpeed),
					new FarmerSprite.AnimationFrame(baseFrame, flapSpeed, secondaryArm: false, flip: false, doneWithFlap)
				});
			}
			if (isPrismatic && prismaticCaptureTimer < 0f)
			{
				Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(144, 249, 7, 7), Game1.random.Next(100, 200), 6, 1, position + new Vector2(-48 + Game1.random.Next(-32, 32), -96 + Game1.random.Next(-32, 32)), flicker: false, flipped: false, Math.Max(0f, (position.Y + 64f - 24f) / 10000f) + position.X / 64f * 1E-05f, 0f, Utility.GetPrismaticColor(Game1.random.Next(7), 10f), 4f, 0f, 0f, 0f)
				{
					drawAboveAlwaysFront = true
				}, environment);
			}
		}
		if (prismaticCaptureTimer > 0f)
		{
			motion = Game1.player.position.Value + new Vector2(64f, -32f) - position;
			motion *= 0.1f;
			prismaticCaptureTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			position += motion;
			position += new Vector2((float)Math.Cos(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0) * (prismaticCaptureTimer / 150f), (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 100.0) * (prismaticCaptureTimer / 150f));
			prismaticSprinkleTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			if (prismaticSprinkleTimer <= 0f)
			{
				environment.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(144, 249, 7, 7), Game1.random.Next(100, 200), 6, 1, position + new Vector2(-48f, -96f), flicker: false, flipped: false, Math.Max(0f, (position.Y + 64f - 24f) / 10000f) + position.X / 64f * 1E-05f, 0f, Utility.GetPrismaticColor(Game1.random.Next(7), 10f), 4f, 0f, 0f, 0f)
				{
					drawAboveAlwaysFront = true
				});
				prismaticSprinkleTimer = 80f;
			}
			if (prismaticCaptureTimer <= 0f)
			{
				Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(144, 249, 7, 7), Game1.random.Next(100, 200), 6, 1, position + new Vector2(-48f, -96f), flicker: false, flipped: false, Math.Max(0f, (position.Y + 64f - 24f) / 10000f) + position.X / 64f * 1E-05f, 0f, Color.White, 4f, 0f, 0f, 0f)
				{
					drawAboveAlwaysFront = true
				}, environment, 16);
				Game1.playSound("yoba");
				Game1.player.buffs.Remove("statue_of_blessings_6");
				if (Utility.CreateDaySaveRandom(Game1.player.UniqueMultiplayerID % 10000).NextDouble() < 0.05000000074505806 + Game1.player.DailyLuck)
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)74"), position + new Vector2(-48f, -96f), 2, environment, (int)Game1.player.position.Y);
				}
				Game1.player.Money += Math.Max(100, Math.Min(50000, (int)((float)Game1.player.totalMoneyEarned * 0.005f)));
				return true;
			}
		}
		else
		{
			position += motion * motionMultiplier;
			motion.Y += 0.005f * (float)time.ElapsedGameTime.Milliseconds;
			motionMultiplier -= 0.0005f * (float)time.ElapsedGameTime.Milliseconds;
			if (motionMultiplier <= 0f)
			{
				motionMultiplier = 0f;
			}
		}
		if (isPrismatic && prismaticCaptureTimer < 0f && Utility.distance(position.X, Game1.player.position.X, position.Y, Game1.player.position.Y) < 128f)
		{
			prismaticCaptureTimer = 2000f;
		}
		if (isLit)
		{
			Utility.repositionLightSource(lightID, position + new Vector2(-30.72f, -93.44f));
		}
		return base.update(time, environment);
	}

	public override void draw(SpriteBatch b)
	{
	}

	public override void drawAboveFrontLayer(SpriteBatch b)
	{
		sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(-64f, -128f + yJumpOffset + yOffset)), position.Y / 10000f, 0, 0, isPrismatic ? Utility.GetPrismaticColor(0, 10f) : Color.White, flip, 4f);
	}
}
