using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.TerrainFeatures;

namespace StardewValley.Events;

public class WitchEvent : BaseFarmEvent
{
	public const int identifier = 942069;

	private Vector2 witchPosition;

	private Building targetBuilding;

	private Farm f;

	private Random r;

	private int witchFrame;

	private int witchAnimationTimer;

	private int animationLoopsDone;

	private int timerSinceFade;

	private bool animateLeft;

	private bool terminate;

	public bool goldenWitch;

	/// <inheritdoc />
	public override bool setUp()
	{
		f = Game1.getFarm();
		r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed);
		foreach (Building b in f.buildings)
		{
			if (!(b.buildingType.Value == "Big Coop") && !(b.buildingType.Value == "Deluxe Coop"))
			{
				continue;
			}
			AnimalHouse animalHouse = (AnimalHouse)b.GetIndoors();
			if (!animalHouse.isFull() && animalHouse.objects.Length < 50 && r.NextDouble() < 0.8)
			{
				targetBuilding = b;
				if (Game1.MasterPlayer.mailReceived.Contains("Farm_Eternal") && r.NextDouble() < 0.6)
				{
					goldenWitch = true;
				}
			}
		}
		if (targetBuilding == null)
		{
			foreach (Building b in f.buildings)
			{
				if (b.buildingType.Value == "Slime Hutch")
				{
					GameLocation indoors = b.GetIndoors();
					if (indoors.characters.Count > 0 && r.NextBool() && indoors.numberOfObjectsOfType("83", bigCraftable: true) == 0)
					{
						targetBuilding = b;
					}
				}
			}
		}
		if (targetBuilding == null)
		{
			return true;
		}
		Game1.currentLightSources.Add(new LightSource(4, witchPosition, 2f, Color.Black, 942069, LightSource.LightContext.None, 0L));
		Game1.currentLocation = f;
		f.resetForPlayerEntry();
		Game1.fadeClear();
		Game1.nonWarpFade = true;
		Game1.timeOfDay = 2400;
		Game1.ambientLight = new Color(200, 190, 40);
		Game1.displayHUD = false;
		Game1.freezeControls = true;
		Game1.viewportFreeze = true;
		Game1.displayFarmer = false;
		Game1.viewport.X = Math.Max(0, Math.Min(f.map.DisplayWidth - Game1.viewport.Width, (int)targetBuilding.tileX * 64 - Game1.viewport.Width / 2));
		Game1.viewport.Y = Math.Max(0, Math.Min(f.map.DisplayHeight - Game1.viewport.Height, ((int)targetBuilding.tileY - 3) * 64 - Game1.viewport.Height / 2));
		witchPosition = new Vector2(Game1.viewport.X + Game1.viewport.Width + 128, (int)targetBuilding.tileY * 64 - 64);
		Game1.changeMusicTrack("nightTime");
		DelayedAction.playSoundAfterDelay(goldenWitch ? "yoba" : "cacklingWitch", 3200);
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
		Utility.repositionLightSource(942069, witchPosition + new Vector2(32f, 32f));
		if (animationLoopsDone < 1)
		{
			timerSinceFade += time.ElapsedGameTime.Milliseconds;
		}
		if (witchPosition.X > (float)((int)targetBuilding.tileX * 64 + 96))
		{
			if (timerSinceFade < 2000)
			{
				return false;
			}
			witchPosition.X -= (float)time.ElapsedGameTime.Milliseconds * 0.4f;
			witchPosition.Y += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * Math.PI / 512.0) * 1f;
		}
		else if (animationLoopsDone < 4)
		{
			witchPosition.Y += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * Math.PI / 512.0) * 1f;
			witchAnimationTimer += time.ElapsedGameTime.Milliseconds;
			if (witchAnimationTimer > 2000)
			{
				witchAnimationTimer = 0;
				if (!animateLeft)
				{
					witchFrame++;
					if (witchFrame == 1)
					{
						animateLeft = true;
						for (int i = 0; i < 75; i++)
						{
							f.temporarySprites.Add(new TemporaryAnimatedSprite(10, witchPosition + new Vector2(8f, 80f), goldenWitch ? (r.NextBool() ? Color.Gold : new Color(255, 150, 0)) : (r.NextBool() ? Color.Lime : Color.DarkViolet))
							{
								motion = new Vector2((float)r.Next(-100, 100) / 100f, 1.5f),
								alphaFade = 0.015f,
								delayBeforeAnimationStart = i * 30,
								layerDepth = 1f
							});
						}
						Game1.playSound(goldenWitch ? "discoverMineral" : "debuffSpell");
					}
				}
				else
				{
					witchFrame--;
					animationLoopsDone = 4;
					DelayedAction.playSoundAfterDelay(goldenWitch ? "yoba" : "cacklingWitch", 2500);
				}
			}
		}
		else
		{
			witchAnimationTimer += time.ElapsedGameTime.Milliseconds;
			witchFrame = 0;
			if (witchAnimationTimer > 1000 && witchPosition.X > -999999f)
			{
				witchPosition.Y += (float)Math.Cos((double)time.TotalGameTime.Milliseconds * Math.PI / 256.0) * 2f;
				witchPosition.X -= (float)time.ElapsedGameTime.Milliseconds * 0.4f;
			}
			if (witchPosition.X < (float)(Game1.viewport.X - 128) || float.IsNaN(witchPosition.X))
			{
				if (!Game1.fadeToBlack && witchPosition.X != -999999f)
				{
					Game1.globalFadeToBlack(afterLastFade);
					Game1.changeMusicTrack("none");
					timerSinceFade = 0;
					witchPosition.X = -999999f;
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
		if (goldenWitch)
		{
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, witchPosition), new Rectangle(215, 262 + witchFrame * 29, 34, 29), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9999999f);
		}
		else
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, witchPosition), new Rectangle(277, 1886 + witchFrame * 29, 34, 29), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9999999f);
		}
	}

	/// <inheritdoc />
	public override void makeChangesToLocation()
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		GameLocation indoors = targetBuilding.GetIndoors();
		if (targetBuilding.buildingType.Value == "Slime Hutch")
		{
			foreach (NPC character in indoors.characters)
			{
				if (character is GreenSlime slime)
				{
					slime.color.Value = new Color(40 + r.Next(10), 40 + r.Next(10), 40 + r.Next(10));
				}
			}
			return;
		}
		for (int tries = 0; tries < 200; tries++)
		{
			Vector2 v = new Vector2(r.Next(2, indoors.Map.Layers[0].LayerWidth - 2), r.Next(2, indoors.Map.Layers[0].LayerHeight - 2));
			if ((indoors.CanItemBePlacedHere(v) || (indoors.terrainFeatures.TryGetValue(v, out var terrainFeature) && terrainFeature is Flooring)) && !indoors.objects.ContainsKey(v))
			{
				Object egg = ItemRegistry.Create<Object>(goldenWitch ? "(O)928" : "(O)305");
				egg.CanBeSetDown = false;
				egg.IsSpawnedObject = true;
				indoors.objects.Add(v, egg);
				break;
			}
		}
	}
}
