using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.Locations;

namespace StardewValley.Events;

public class WorldChangeEvent : BaseFarmEvent
{
	public const int identifier = 942066;

	public const int jojaGreenhouse = 0;

	public const int junimoGreenHouse = 1;

	public const int jojaBoiler = 2;

	public const int junimoBoiler = 3;

	public const int jojaBridge = 4;

	public const int junimoBridge = 5;

	public const int jojaBus = 6;

	public const int junimoBus = 7;

	public const int jojaBoulder = 8;

	public const int junimoBoulder = 9;

	public const int jojaMovieTheater = 10;

	public const int junimoMovieTheater = 11;

	public const int movieTheaterLightning = 12;

	public const int willyBoatRepair = 13;

	public const int treehouseBuild = 14;

	public const int goldenParrots = 15;

	public readonly NetInt whichEvent = new NetInt();

	private int cutsceneLengthTimer;

	private int timerSinceFade;

	private int soundTimer;

	private int soundInterval = 99999;

	private GameLocation location;

	private string sound;

	private bool wasRaining;

	public GameLocation preEventLocation;

	public WorldChangeEvent()
		: this(0)
	{
	}

	public WorldChangeEvent(int which)
	{
		whichEvent.Value = which;
	}

	/// <inheritdoc />
	public override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(whichEvent, "whichEvent");
	}

	private void obliterateJojaMartDoor()
	{
		Town town = Game1.RequireLocation<Town>("Town");
		town.crackOpenAbandonedJojaMartDoor();
		for (int i = 0; i < 16; i++)
		{
			town.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(96f, 50f) * 64f + new Vector2(Game1.random.Next(-32, 64), 0f), flipped: false, 0.002f, Color.Gray)
			{
				alpha = 0.75f,
				motion = new Vector2(0f, -0.5f) + new Vector2((float)(Game1.random.Next(100) - 50) / 100f, (float)(Game1.random.Next(100) - 50) / 100f),
				interval = 99999f,
				layerDepth = 0.95f + (float)i * 0.001f,
				scale = 3f,
				scaleChange = 0.01f,
				rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
				delayBeforeAnimationStart = i * 25
			});
		}
		Utility.addDirtPuffs(town, 95, 49, 2, 2);
		town.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(96f, 50f) * 64f + new Vector2(Game1.random.Next(-32, 64), 0f), flipped: false, 0f, Color.Gray)
		{
			alpha = 0.01f,
			interval = 99999f,
			layerDepth = 0.9f,
			light = true,
			lightRadius = 4f,
			lightcolor = new Color(1, 1, 1)
		});
	}

	/// <inheritdoc />
	public override bool setUp()
	{
		preEventLocation = Game1.currentLocation;
		location = null;
		Point targetTile = Point.Zero;
		wasRaining = Game1.isRaining;
		switch (whichEvent)
		{
		case 15L:
			location = Game1.RequireLocation("IslandNorth");
			targetTile = new Point(40, 23);
			break;
		case 13L:
			location = Game1.RequireLocation("BoatTunnel");
			targetTile = new Point(7, 7);
			break;
		case 12L:
			location = Game1.RequireLocation("Town");
			targetTile = new Point(95, 48);
			break;
		case 10L:
			location = Game1.RequireLocation("Town");
			targetTile = new Point(52, 18);
			break;
		case 11L:
			location = Game1.RequireLocation("Town");
			targetTile = new Point(95, 48);
			break;
		case 0L:
		case 1L:
			location = Game1.getFarm();
			targetTile = ((Game1.whichFarm == 5) ? new Point(39, 32) : new Point(28, 13));
			foreach (Building b in location.buildings)
			{
				if (b is GreenhouseBuilding)
				{
					targetTile = new Point((int)b.tileX + 3, (int)b.tileY + 3);
					break;
				}
			}
			break;
		case 6L:
		case 7L:
			location = Game1.RequireLocation("BusStop");
			targetTile = new Point(24, 8);
			break;
		case 2L:
		case 3L:
			location = Game1.RequireLocation("Town");
			targetTile = new Point(105, 79);
			break;
		case 4L:
		case 5L:
			location = Game1.RequireLocation("Mountain");
			targetTile = new Point(95, 27);
			break;
		case 8L:
		case 9L:
			location = Game1.RequireLocation("Mountain");
			targetTile = new Point(48, 5);
			break;
		case 14L:
			location = Game1.RequireLocation("Mountain");
			targetTile = new Point(16, 7);
			break;
		}
		Game1.currentLocation = location;
		resetForPlayerEntry(targetTile);
		return false;
	}

	public void resetForPlayerEntry(Point targetTile)
	{
		location.resetForPlayerEntry();
		cutsceneLengthTimer = 8000;
		wasRaining = Game1.isRaining;
		Game1.isRaining = false;
		Game1.changeMusicTrack("nightTime");
		switch (whichEvent)
		{
		case 15L:
		{
			Game1.changeMusicTrack("jungle_ambience");
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(200, 89, 28, 32), new Vector2(39f, 32f) * 64f, flipped: false, 0f, Color.White)
			{
				animationLength = 2,
				interval = 700f,
				totalNumberOfLoops = 999,
				layerDepth = 0.1f,
				light = true,
				lightcolor = Color.Black,
				lightRadius = 2f,
				scale = 4f
			});
			int walnutsBought = 130 - Game1.netWorldState.Value.GoldenWalnutsFound;
			int bags = 1 + walnutsBought / 10;
			for (int i = 0; i < bags; i++)
			{
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(184, 104, 14, 15), new Vector2(39 + i % 3, 34.1f + (float)(i / 3) * 0.5f) * 64f, flipped: false, 0f, Color.White)
				{
					animationLength = 1,
					interval = 700f,
					totalNumberOfLoops = 999,
					layerDepth = 0.1f + (float)i * 0.01f,
					scale = 4f
				});
			}
			cutsceneLengthTimer = 10000;
			for (int i = 0; i < 20; i++)
			{
				Vector2 start_point = Utility.getRandomPositionInThisRectangle(new Rectangle(20, 1, 40, 2), Game1.random) * 64f;
				float xMotion = ((!(start_point.X > (float)(location.Map.DisplayWidth / 2))) ? 1 : (-1));
				TemporaryAnimatedSprite parrot = new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(48 + Game1.random.Next(2) * 72, 96, 24, 24), start_point, flipped: false, 0f, Color.White)
				{
					motion = new Vector2(xMotion * 3f, 6f + (float)Game1.random.NextDouble()),
					acceleration = new Vector2(0f, -0.01f),
					id = 778,
					scale = 4f,
					yStopCoordinate = (int)start_point.Y + Game1.random.Next(19, 27) * 64 + i * 64 / 2,
					totalNumberOfLoops = 99999,
					interval = 80f,
					animationLength = 3,
					pingPong = true,
					flipped = (xMotion > 0f),
					layerDepth = 1f,
					drawAboveAlwaysFront = true,
					light = true,
					lightcolor = Color.Black,
					lightRadius = 2f,
					alpha = 0.001f,
					alphaFade = -0.01f,
					delayBeforeAnimationStart = i * 250
				};
				DelayedAction.playSoundAfterDelay("parrot_flap", 500 + i * 250);
				DelayedAction.playSoundAfterDelay("parrot_flap", 5500 + i * 250);
				parrot.reachedStopCoordinateSprite = GoldenParrotBounce;
				location.temporarySprites.Add(parrot);
			}
			DelayedAction.functionAfterDelay(ParrotSquawk, 9000);
			DelayedAction.functionAfterDelay(ParrotFlyAway, 11000);
			break;
		}
		case 13L:
			if (Game1.IsMasterGame)
			{
				Game1.addMailForTomorrow("willyBoatFixed", noLetter: true);
			}
			Game1.mailbox.Add("willyHours");
			location.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Willy", new Rectangle(0, 320, 16, 32), 120f, 3, 999, new Vector2(412f, 332f), flicker: false, flipped: false)
			{
				pingPong = true,
				scale = 4f,
				layerDepth = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Robin", new Rectangle(0, 192, 16, 32), 140f, 4, 999, new Vector2(704f, 256f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			soundInterval = 560;
			sound = "crafting";
			break;
		case 12L:
		{
			cutsceneLengthTimer += 3000;
			Game1.isRaining = true;
			Game1.changeMusicTrack("rain");
			if (Game1.IsMasterGame)
			{
				Game1.addMailForTomorrow("abandonedJojaMartAccessible", noLetter: true);
			}
			Rectangle lightningSourceRect = new Rectangle(644, 1078, 37, 57);
			Vector2 strikePosition = new Vector2(96f, 50f) * 64f;
			Vector2 drawPosition = strikePosition + new Vector2(-lightningSourceRect.Width * 4 / 2, -lightningSourceRect.Height * 4);
			while (drawPosition.Y > (float)(-lightningSourceRect.Height * 4))
			{
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", lightningSourceRect, 9999f, 1, 999, drawPosition, flicker: false, Game1.random.NextBool(), (strikePosition.Y + 32f) / 10000f + 0.001f, 0.025f, Color.White, 4f, 0f, 0f, 0f)
				{
					light = true,
					lightRadius = 2f,
					delayBeforeAnimationStart = 6200,
					lightcolor = Color.Black
				});
				drawPosition.Y -= lightningSourceRect.Height * 4;
			}
			DelayedAction.playSoundAfterDelay("thunder_small", 6000);
			DelayedAction.playSoundAfterDelay("boulderBreak", 6300);
			DelayedAction.screenFlashAfterDelay(1f, 6000);
			DelayedAction.functionAfterDelay(obliterateJojaMartDoor, 6050);
			break;
		}
		case 10L:
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1349, 19, 28), 150f, 5, 999, new Vector2(3760f, 1056f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1377, 19, 28), 140f, 5, 999, new Vector2(2948f, 1088f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1000f, 2, 999, new Vector2(3144f, 1280f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			Game1.player.activeDialogueEvents.TryAdd("movieTheater", 3);
			soundInterval = 560;
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetTile.X, targetTile.Y) * 64f, 4f, LightSource.LightContext.None, 0L));
			sound = "axchop";
			break;
		case 11L:
			Utility.addSprinklesToLocation(location, targetTile.X, targetTile.Y, 7, 7, 15000, 150, Color.LightCyan);
			Utility.addStarsAndSpirals(location, targetTile.X, targetTile.Y, 7, 7, 15000, 150, Color.White);
			Game1.player.activeDialogueEvents.TryAdd("movieTheater", 3);
			sound = "junimoMeep1";
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetTile.X, targetTile.Y) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L));
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(6080f, 2880f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f,
				xPeriodic = true,
				xPeriodicLoopTime = 2000f,
				xPeriodicRange = 16f,
				light = true,
				lightcolor = Color.DarkGoldenrod,
				lightRadius = 1f
			});
			soundInterval = 800;
			break;
		case 0L:
		{
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1349, 19, 28), 150f, 5, 999, new Vector2((targetTile.X - 3) * 64 + 8, (targetTile.Y - 1) * 64 - 32), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1377, 19, 28), 140f, 5, 999, new Vector2((targetTile.X + 3) * 64 - 16, (targetTile.Y - 2) * 64), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1000f, 2, 999, new Vector2(targetTile.X * 64 + 8, (targetTile.Y - 4) * 64), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			soundInterval = 560;
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetTile.X, targetTile.Y) * 64f, 4f, LightSource.LightContext.None, 0L));
			sound = "axchop";
			float depthY = (float)((targetTile.Y + 3) * 64) / 10000f;
			TemporaryAnimatedSprite hole = new TemporaryAnimatedSprite("Buildings\\Greenhouse", new Rectangle(25, 133, 31, 19), 99999f, 1, 999, new Vector2(targetTile.X * 64, (targetTile.Y - 1) * 64 - 64) + new Vector2(-23f, 53f) * 4f, flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = depthY + 0.0008f
			};
			location.temporarySprites.Add(hole);
			TemporaryAnimatedSprite raccoon = new TemporaryAnimatedSprite("Characters\\raccoon", new Rectangle(0, 32, 32, 32), 99999f, 1, 999, new Vector2(targetTile.X * 64, (targetTile.Y - 1) * 64 - 64) + new Vector2(-20f, 40f) * 4f, flicker: false, flipped: false)
			{
				scale = 4f,
				shakeIntensity = 1f,
				layerDepth = depthY + 0.0004f,
				delayBeforeAnimationStart = 3000,
				motion = new Vector2(-1f, -6f),
				acceleration = new Vector2(0f, 0.17f),
				xStopCoordinate = targetTile.X * 64 - 136,
				startSound = "Raccoon"
			};
			raccoon.reachedStopCoordinate = delegate
			{
				hole.layerDepth = 0f;
				raccoon.motion.X = -1f;
				raccoon.yStopCoordinate = targetTile.Y * 64 + 72;
				raccoon.reachedStopCoordinate = delegate
				{
					raccoon.motion = new Vector2(0f, 4f);
					raccoon.acceleration = Vector2.Zero;
					raccoon.sourceRect = new Rectangle(0, 0, 32, 32);
					raccoon.animationLength = 8;
					raccoon.interval = 80f;
					raccoon.sourceRectStartingPos = Vector2.Zero;
					raccoon.yStopCoordinate = targetTile.Y * 64 + 160;
					raccoon.reachedStopCoordinate = delegate
					{
						raccoon.layerDepth = -1f;
						raccoon.motion = new Vector2(0f, 4f);
						raccoon.layerDepthOffset = 0.0128f;
					};
				};
			};
			location.temporarySprites.Add(raccoon);
			break;
		}
		case 1L:
		{
			Utility.addSprinklesToLocation(location, targetTile.X, targetTile.Y - 1, 7, 7, 15000, 150, Color.LightCyan);
			Utility.addStarsAndSpirals(location, targetTile.X, targetTile.Y - 1, 7, 7, 15000, 150, Color.White);
			if (!Game1.player.activeDialogueEvents.ContainsKey("cc_Greenhouse"))
			{
				Game1.player.activeDialogueEvents.TryAdd("cc_Greenhouse", 3);
			}
			sound = "junimoMeep1";
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetTile.X, targetTile.Y) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L));
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(targetTile.X * 64, (targetTile.Y - 1) * 64 - 64), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f,
				xPeriodic = true,
				xPeriodicLoopTime = 2000f,
				xPeriodicRange = 16f,
				light = true,
				lightcolor = Color.DarkGoldenrod,
				lightRadius = 1f
			});
			soundInterval = 800;
			float depthY2 = (float)((targetTile.Y + 3) * 64) / 10000f;
			TemporaryAnimatedSprite hole2 = new TemporaryAnimatedSprite("Buildings\\Greenhouse", new Rectangle(25, 133, 31, 19), 99999f, 1, 999, new Vector2(targetTile.X * 64, (targetTile.Y - 1) * 64 - 64) + new Vector2(-23f, 53f) * 4f, flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = depthY2 + 0.0008f
			};
			location.temporarySprites.Add(hole2);
			TemporaryAnimatedSprite raccoon2 = new TemporaryAnimatedSprite("Characters\\raccoon", new Rectangle(0, 32, 32, 32), 99999f, 1, 999, new Vector2(targetTile.X * 64, (targetTile.Y - 1) * 64 - 64) + new Vector2(-20f, 40f) * 4f, flicker: false, flipped: false)
			{
				scale = 4f,
				shakeIntensity = 1f,
				layerDepth = depthY2 + 0.0004f,
				delayBeforeAnimationStart = 3000,
				motion = new Vector2(-1f, -6f),
				acceleration = new Vector2(0f, 0.17f),
				xStopCoordinate = targetTile.X * 64 - 136,
				startSound = "Raccoon"
			};
			raccoon2.reachedStopCoordinate = delegate
			{
				hole2.layerDepth = 0f;
				raccoon2.motion.X = -1f;
				raccoon2.yStopCoordinate = targetTile.Y * 64 + 72;
				raccoon2.reachedStopCoordinate = delegate
				{
					raccoon2.motion = new Vector2(0f, 4f);
					raccoon2.acceleration = Vector2.Zero;
					raccoon2.sourceRect = new Rectangle(0, 0, 32, 32);
					raccoon2.animationLength = 8;
					raccoon2.interval = 80f;
					raccoon2.sourceRectStartingPos = Vector2.Zero;
					raccoon2.yStopCoordinate = targetTile.Y * 64 + 160;
					raccoon2.reachedStopCoordinate = delegate
					{
						raccoon2.layerDepth = -1f;
						raccoon2.motion = new Vector2(0f, 4f);
						raccoon2.layerDepthOffset = 0.0128f;
					};
				};
			};
			location.temporarySprites.Add(raccoon2);
			break;
		}
		case 6L:
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1349, 19, 28), 150f, 5, 999, new Vector2(1856f, 480f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1377, 19, 28), 140f, 5, 999, new Vector2(1280f, 512f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1500f, 2, 999, new Vector2(1544f, 192f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			Game1.player.activeDialogueEvents.TryAdd("cc_Bus", 7);
			soundInterval = 560;
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetTile.X, targetTile.Y) * 64f, 4f, LightSource.LightContext.None, 0L));
			sound = "clank";
			break;
		case 7L:
			Utility.addSprinklesToLocation(location, targetTile.X, targetTile.Y, 9, 4, 10000, 200, Color.LightCyan, null, motionTowardCenter: true);
			Utility.addStarsAndSpirals(location, targetTile.X, targetTile.Y, 9, 4, 15000, 150, Color.White);
			sound = "junimoMeep1";
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(1280f, 640f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f,
				xPeriodic = true,
				xPeriodicLoopTime = 2000f,
				xPeriodicRange = 16f,
				light = true,
				lightcolor = Color.DarkGoldenrod,
				lightRadius = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(1408f, 640f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f,
				xPeriodic = true,
				xPeriodicLoopTime = 2300f,
				xPeriodicRange = 16f,
				color = Color.Pink,
				light = true,
				lightcolor = Color.DarkGoldenrod,
				lightRadius = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(1536f, 640f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f,
				xPeriodic = true,
				xPeriodicLoopTime = 2200f,
				xPeriodicRange = 16f,
				color = Color.Yellow,
				light = true,
				lightcolor = Color.DarkGoldenrod,
				lightRadius = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(1664f, 640f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f,
				xPeriodic = true,
				xPeriodicLoopTime = 2100f,
				xPeriodicRange = 16f,
				color = Color.LightBlue,
				light = true,
				lightcolor = Color.DarkGoldenrod,
				lightRadius = 1f
			});
			Game1.player.activeDialogueEvents.TryAdd("cc_Bus", 7);
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetTile.X, targetTile.Y) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L));
			soundInterval = 500;
			break;
		case 2L:
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1377, 19, 28), 100f, 5, 999, new Vector2(6656f, 5024f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1406, 22, 26), 700f, 2, 999, new Vector2(6888f, 5014f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1500f, 2, 999, new Vector2(6792f, 4864f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(335, 1410, 21, 21), 999f, 1, 9999, new Vector2(6912f, 5136f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			Game1.player.activeDialogueEvents.TryAdd("cc_Minecart", 7);
			soundInterval = 500;
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetTile.X, targetTile.Y) * 64f, 4f, LightSource.LightContext.None, 0L));
			sound = "clank";
			break;
		case 3L:
			Utility.addSprinklesToLocation(location, targetTile.X + 1, targetTile.Y, 6, 4, 15000, 350, Color.LightCyan);
			Utility.addStarsAndSpirals(location, targetTile.X + 1, targetTile.Y, 6, 4, 15000, 350, Color.White);
			Game1.player.activeDialogueEvents.TryAdd("cc_Minecart", 7);
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(6656f, 5056f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f,
				xPeriodic = true,
				xPeriodicLoopTime = 2000f,
				xPeriodicRange = 16f,
				light = true,
				lightcolor = Color.DarkGoldenrod,
				lightRadius = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(6912f, 5056f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f,
				xPeriodic = true,
				xPeriodicLoopTime = 2300f,
				xPeriodicRange = 16f,
				color = Color.HotPink,
				light = true,
				lightcolor = Color.DarkGoldenrod,
				lightRadius = 1f
			});
			sound = "junimoMeep1";
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetTile.X, targetTile.Y) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L));
			soundInterval = 800;
			break;
		case 4L:
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(383, 1378, 28, 27), 400f, 2, 999, new Vector2(5504f, 1632f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f,
				motion = new Vector2(0.5f, 0f)
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1406, 22, 26), 350f, 2, 999, new Vector2(6272f, 1632f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(358, 1415, 31, 20), 999f, 1, 9999, new Vector2(5888f, 1648f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(335, 1410, 21, 21), 999f, 1, 9999, new Vector2(6400f, 1648f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1500f, 2, 999, new Vector2(5824f, 1584f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 0.8f
			});
			Game1.player.activeDialogueEvents.TryAdd("cc_Bridge", 7);
			soundInterval = 700;
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetTile.X, targetTile.Y) * 64f, 4f, LightSource.LightContext.None, 0L));
			sound = "axchop";
			break;
		case 5L:
			Utility.addSprinklesToLocation(location, targetTile.X, targetTile.Y, 7, 4, 15000, 150, Color.LightCyan);
			Utility.addStarsAndSpirals(location, targetTile.X + 1, targetTile.Y, 7, 4, 15000, 350, Color.White);
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(5824f, 1648f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f,
				xPeriodic = true,
				xPeriodicLoopTime = 2000f,
				xPeriodicRange = 16f,
				light = true,
				lightcolor = Color.DarkGoldenrod,
				lightRadius = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(6336f, 1648f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f,
				xPeriodic = true,
				xPeriodicLoopTime = 2300f,
				xPeriodicRange = 16f,
				color = Color.Yellow,
				light = true,
				lightcolor = Color.DarkGoldenrod,
				lightRadius = 1f
			});
			Game1.player.activeDialogueEvents.TryAdd("cc_Bridge", 7);
			sound = "junimoMeep1";
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetTile.X, targetTile.Y) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L));
			soundInterval = 800;
			break;
		case 8L:
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(288, 1377, 19, 28), 100f, 5, 999, new Vector2(2880f, 288f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(387, 1340, 17, 37), 50f, 2, 99999, new Vector2(3040f, 160f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f,
				yPeriodic = true,
				yPeriodicLoopTime = 100f,
				yPeriodicRange = 2f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(335, 1410, 21, 21), 999f, 1, 9999, new Vector2(2816f, 368f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(390, 1405, 18, 32), 1500f, 2, 999, new Vector2(3200f, 368f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f
			});
			Game1.player.activeDialogueEvents.TryAdd("cc_Boulder", 7);
			soundInterval = 100;
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetTile.X, targetTile.Y) * 64f, 4f, LightSource.LightContext.None, 0L));
			sound = "thudStep";
			break;
		case 9L:
			Game1.player.activeDialogueEvents.TryAdd("cc_Boulder", 7);
			Utility.addSprinklesToLocation(location, targetTile.X, targetTile.Y, 4, 4, 15000, 350, Color.LightCyan);
			Utility.addStarsAndSpirals(location, targetTile.X + 1, targetTile.Y, 4, 4, 15000, 550, Color.White);
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(2880f, 368f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f,
				xPeriodic = true,
				xPeriodicLoopTime = 2000f,
				xPeriodicRange = 16f,
				light = true,
				lightcolor = Color.DarkGoldenrod,
				lightRadius = 1f
			});
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(294, 1432, 16, 16), 300f, 4, 999, new Vector2(3200f, 368f), flicker: false, flipped: false)
			{
				scale = 4f,
				layerDepth = 1f,
				xPeriodic = true,
				xPeriodicLoopTime = 2300f,
				xPeriodicRange = 16f,
				color = Color.Yellow,
				light = true,
				lightcolor = Color.DarkGoldenrod,
				lightRadius = 1f
			});
			sound = "junimoMeep1";
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetTile.X, targetTile.Y) * 64f, 1f, LightSource.LightContext.None, 0L));
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetTile.X, targetTile.Y) * 64f, 1f, Color.DarkCyan, LightSource.LightContext.None, 0L));
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetTile.X, targetTile.Y) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L));
			soundInterval = 1000;
			break;
		case 14L:
		{
			cutsceneLengthTimer = 12000;
			Game1.currentLightSources.Add(new LightSource(4, new Vector2(targetTile.X, targetTile.Y) * 64f, 4f, Color.DarkGoldenrod, LightSource.LightContext.None, 0L));
			location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(0, 0, 24, 24), new Vector2(14f, 4.5f) * 64f, flipped: false, 0f, Color.White)
			{
				id = 777,
				scale = 4f,
				totalNumberOfLoops = 99999,
				interval = 9999f,
				animationLength = 1,
				layerDepth = 1f,
				drawAboveAlwaysFront = true
			});
			DelayedAction.functionAfterDelay(ParrotSquawk, 2000);
			for (int i = 0; i < 16; i++)
			{
				Rectangle rect = new Rectangle(15, 5, 3, 3);
				TemporaryAnimatedSprite t = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(49 + 16 * Game1.random.Next(3), 229, 16, 6), Utility.getRandomPositionInThisRectangle(rect, Game1.random) * 64f, Game1.random.NextBool(), 0f, Color.White)
				{
					motion = new Vector2(Game1.random.Next(-2, 3), -16f),
					acceleration = new Vector2(0f, 0.5f),
					rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
					scale = 4f,
					animationLength = 1,
					totalNumberOfLoops = 1,
					interval = 1000 + Game1.random.Next(500),
					layerDepth = 1f,
					drawAboveAlwaysFront = true,
					yStopCoordinate = (rect.Bottom + 1) * 64,
					delayBeforeAnimationStart = 4000 + i * 250
				};
				t.reachedStopCoordinate = t.bounce;
				location.TemporarySprites.Add(t);
				t = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(49 + 16 * Game1.random.Next(3), 229, 16, 6), Utility.getRandomPositionInThisRectangle(rect, Game1.random) * 64f, Game1.random.NextBool(), 0f, Color.White)
				{
					motion = new Vector2(Game1.random.Next(-2, 3), -16f),
					acceleration = new Vector2(0f, 0.5f),
					rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
					scale = 4f,
					animationLength = 1,
					totalNumberOfLoops = 1,
					interval = 1000 + Game1.random.Next(500),
					layerDepth = 1f,
					drawAboveAlwaysFront = true,
					delayBeforeAnimationStart = 4500 + i * 250,
					yStopCoordinate = (rect.Bottom + 1) * 64
				};
				t.reachedStopCoordinate = t.bounce;
				location.TemporarySprites.Add(t);
			}
			for (int i = 0; i < 20; i++)
			{
				Vector2 start_point = new Vector2(Utility.RandomFloat(13f, 19f), 0f) * 64f;
				float x_offset = 1024f - start_point.X;
				TemporaryAnimatedSprite parrot = new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(48 + Game1.random.Next(2) * 72, Game1.random.Next(2) * 48, 24, 24), start_point, flipped: false, 0f, Color.White)
				{
					motion = new Vector2(x_offset * 0.01f, 10f),
					acceleration = new Vector2(0f, -0.05f),
					id = 778,
					scale = 4f,
					yStopCoordinate = 448,
					totalNumberOfLoops = 99999,
					interval = 50f,
					animationLength = 3,
					flipped = (x_offset > 0f),
					layerDepth = 1f,
					drawAboveAlwaysFront = true,
					delayBeforeAnimationStart = 3500 + i * 250,
					alpha = 0f,
					alphaFade = -0.1f
				};
				DelayedAction.playSoundAfterDelay("batFlap", 3500 + i * 250);
				parrot.reachedStopCoordinateSprite = ParrotBounce;
				location.temporarySprites.Add(parrot);
			}
			DelayedAction.functionAfterDelay(FinishTreehouse, 8000);
			DelayedAction.functionAfterDelay(ParrotSquawk, 9000);
			DelayedAction.functionAfterDelay(ParrotFlyAway, 11000);
			break;
		}
		}
		soundTimer = soundInterval;
		Game1.fadeClear();
		Game1.nonWarpFade = true;
		Game1.timeOfDay = 2400;
		Game1.displayHUD = false;
		Game1.viewportFreeze = true;
		Game1.player.position.X = -999999f;
		Game1.viewport.X = Math.Max(0, Math.Min(location.map.DisplayWidth - Game1.viewport.Width, targetTile.X * 64 - Game1.viewport.Width / 2));
		Game1.viewport.Y = Math.Max(0, Math.Min(location.map.DisplayHeight - Game1.viewport.Height, targetTile.Y * 64 - Game1.viewport.Height / 2));
		if (!location.IsOutdoors)
		{
			Game1.viewport.X = targetTile.X * 64 - Game1.viewport.Width / 2;
			Game1.viewport.Y = targetTile.Y * 64 - Game1.viewport.Height / 2;
		}
		Game1.previousViewportPosition = new Vector2(Game1.viewport.X, Game1.viewport.Y);
		List<WeatherDebris> debrisWeather = Game1.debrisWeather;
		if (debrisWeather != null && debrisWeather.Count > 0)
		{
			Game1.randomizeDebrisWeatherPositions(Game1.debrisWeather);
		}
		Game1.randomizeRainPositions();
	}

	public virtual void ParrotFlyAway()
	{
		location.removeTemporarySpritesWithIDLocal(777);
		location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(48, 0, 24, 24), new Vector2(14f, 4.5f) * 64f, flipped: false, 0f, Color.White)
		{
			id = 777,
			scale = 4f,
			totalNumberOfLoops = 99999,
			layerDepth = 1f,
			drawAboveAlwaysFront = true,
			interval = 50f,
			animationLength = 3,
			motion = new Vector2(-2f, 0f),
			acceleration = new Vector2(0f, -0.1f)
		});
	}

	public virtual void ParrotSquawk()
	{
		TemporaryAnimatedSprite parrot = location.getTemporarySpriteByID(777);
		if (parrot != null)
		{
			parrot.shakeIntensity = 1f;
			parrot.sourceRectStartingPos.X = 24f;
			parrot.sourceRect.X = 24;
			DelayedAction.functionAfterDelay(ParrotStopSquawk, 500);
		}
		Game1.playSound("parrot");
	}

	public virtual void ParrotStopSquawk()
	{
		TemporaryAnimatedSprite temporarySpriteByID = location.getTemporarySpriteByID(777);
		temporarySpriteByID.shakeIntensity = 0f;
		temporarySpriteByID.sourceRectStartingPos.X = 0f;
		temporarySpriteByID.sourceRect.X = 0;
	}

	public virtual void FinishTreehouse()
	{
		Game1.flashAlpha = 1f;
		Game1.playSound("yoba");
		Game1.playSound("axchop");
		(location as Mountain).ApplyTreehouseIfNecessary();
		location.removeTemporarySpritesWithIDLocal(778);
		for (int i = 0; i < 20; i++)
		{
			Vector2 start_point = new Vector2(Utility.RandomFloat(13f, 19f), Utility.RandomFloat(4f, 7f)) * 64f;
			float x_offset = 1024f - start_point.X;
			TemporaryAnimatedSprite parrot = new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(192, Game1.random.Next(2) * 48, 24, 24), start_point, flipped: false, 0f, Color.White)
			{
				motion = new Vector2(x_offset * -0.01f, Utility.RandomFloat(-2f, 0f)),
				acceleration = new Vector2(0f, -0.05f),
				id = 778,
				scale = 4f,
				totalNumberOfLoops = 99999,
				interval = 50f,
				animationLength = 3,
				flipped = (x_offset > 0f),
				layerDepth = 1f,
				drawAboveAlwaysFront = true
			};
			location.TemporarySprites.Add(parrot);
		}
	}

	public void ParrotBounce(TemporaryAnimatedSprite sprite)
	{
		float x_offset = 1024f - sprite.Position.X;
		sprite.motion.X = (float)Math.Sign(x_offset) * Utility.RandomFloat(0.5f, 4f);
		sprite.motion.Y = Utility.RandomFloat(-15f, -10f);
		sprite.acceleration.Y = 0.5f;
		sprite.yStopCoordinate = 448;
		sprite.flipped = x_offset > 0f;
		sprite.sourceRectStartingPos.X = 48 + Game1.random.Next(2) * 72;
		if (Game1.random.NextDouble() < 0.05000000074505806)
		{
			Game1.playSound("axe");
		}
		else if (Game1.random.NextDouble() < 0.05000000074505806)
		{
			Game1.playSound("crafting");
		}
		else
		{
			Game1.playSound("dirtyHit");
		}
	}

	public void GoldenParrotBounce(TemporaryAnimatedSprite sprite)
	{
		sprite.motion.Y = Utility.RandomFloat(-3f, -5f);
		Game1.playSound("dirtyHit");
		location.temporarySprites.Add(new TemporaryAnimatedSprite(12, sprite.position, Color.White, 8, Game1.random.NextBool(), 50f));
	}

	/// <inheritdoc />
	public override bool tickUpdate(GameTime time)
	{
		Game1.UpdateGameClock(time);
		location.updateWater(time);
		if ((int)whichEvent == 15)
		{
			Game1.viewport.Y++;
		}
		cutsceneLengthTimer -= time.ElapsedGameTime.Milliseconds;
		if (timerSinceFade > 0)
		{
			timerSinceFade -= time.ElapsedGameTime.Milliseconds;
			Game1.globalFade = true;
			Game1.fadeToBlackAlpha = 1f;
			if (timerSinceFade <= 0)
			{
				return true;
			}
			return false;
		}
		if (cutsceneLengthTimer <= 0 && !Game1.globalFade)
		{
			Game1.globalFadeToBlack(endEvent, 0.01f);
		}
		soundTimer -= time.ElapsedGameTime.Milliseconds;
		if (soundTimer <= 0 && sound != null)
		{
			Game1.playSound(sound);
			soundTimer = soundInterval;
		}
		return false;
	}

	public override void makeChangesToLocation()
	{
		base.makeChangesToLocation();
		if ((int)whichEvent == 15 && Game1.IsMasterGame)
		{
			ParrotUpgradePerch.ActivateGoldenParrot();
		}
	}

	public void endEvent()
	{
		location.cleanupBeforePlayerExit();
		if (preEventLocation != null)
		{
			Game1.currentLocation = preEventLocation;
			Game1.currentLocation.resetForPlayerEntry();
			preEventLocation = null;
		}
		timerSinceFade = 1500;
		Game1.isRaining = wasRaining;
		Game1.getFarm().temporarySprites.Clear();
	}
}
