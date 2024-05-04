using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;

namespace StardewValley.Events;

public class SoundInTheNightEvent : BaseFarmEvent
{
	public const int cropCircle = 0;

	public const int meteorite = 1;

	public const int dogs = 2;

	public const int owl = 3;

	public const int earthquake = 4;

	public const int raccoonStump = 5;

	private readonly NetInt behavior = new NetInt();

	private float timer;

	private float timeUntilText = 7000f;

	private string soundName;

	private string message;

	private bool playedSound;

	private bool showedMessage;

	private bool finished;

	private Vector2 targetLocation;

	private Building targetBuilding;

	public SoundInTheNightEvent()
		: this(0)
	{
	}

	public SoundInTheNightEvent(int which)
	{
		behavior.Value = which;
	}

	/// <inheritdoc />
	public override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(behavior, "behavior");
	}

	/// <inheritdoc />
	public override bool setUp()
	{
		Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed);
		Farm f = Game1.getFarm();
		f.updateMap();
		timer = 0f;
		switch (behavior)
		{
		case 5L:
			soundName = "windstorm";
			message = Game1.content.LoadString("Strings\\1_6_Strings:windstorm");
			timeUntilText = 14000f;
			if (!Game1.player.mailReceived.Contains("raccoonTreeFallen"))
			{
				Game1.player.mailReceived.Add("raccoonTreeFallen");
			}
			break;
		case 0L:
		{
			soundName = "UFO";
			message = Game1.content.LoadString("Strings\\Events:SoundInTheNight_UFO");
			int attempts;
			for (attempts = 50; attempts > 0; attempts--)
			{
				targetLocation = new Vector2(r.Next(5, f.map.RequireLayer("Back").TileWidth - 4), r.Next(5, f.map.RequireLayer("Back").TileHeight - 4));
				if (f.CanItemBePlacedHere(targetLocation))
				{
					break;
				}
			}
			if (attempts <= 0)
			{
				return true;
			}
			break;
		}
		case 1L:
		{
			soundName = "Meteorite";
			message = Game1.content.LoadString("Strings\\Events:SoundInTheNight_Meteorite");
			targetLocation = new Vector2(r.Next(5, f.map.RequireLayer("Back").TileWidth - 20), r.Next(5, f.map.RequireLayer("Back").TileHeight - 4));
			for (int i = (int)targetLocation.X; (float)i <= targetLocation.X + 1f; i++)
			{
				for (int j = (int)targetLocation.Y; (float)j <= targetLocation.Y + 1f; j++)
				{
					Vector2 v = new Vector2(i, j);
					if (!f.isTileOpenBesidesTerrainFeatures(v) || !f.isTileOpenBesidesTerrainFeatures(new Vector2(v.X + 1f, v.Y)) || !f.isTileOpenBesidesTerrainFeatures(new Vector2(v.X + 1f, v.Y - 1f)) || !f.isTileOpenBesidesTerrainFeatures(new Vector2(v.X, v.Y - 1f)) || f.isWaterTile((int)v.X, (int)v.Y) || f.isWaterTile((int)v.X + 1, (int)v.Y))
					{
						return true;
					}
				}
			}
			break;
		}
		case 2L:
			soundName = "dogs";
			if (r.NextBool())
			{
				return true;
			}
			foreach (Building b in f.buildings)
			{
				if (b.GetIndoors() is AnimalHouse animalHouse && !b.animalDoorOpen && animalHouse.animalsThatLiveHere.Count > animalHouse.animals.Length && r.NextDouble() < (double)(1f / (float)f.buildings.Count))
				{
					targetBuilding = b;
					break;
				}
			}
			if (targetBuilding == null)
			{
				return true;
			}
			return false;
		case 3L:
		{
			soundName = "owl";
			int attempts;
			for (attempts = 50; attempts > 0; attempts--)
			{
				targetLocation = new Vector2(r.Next(5, f.map.RequireLayer("Back").TileWidth - 4), r.Next(5, f.map.RequireLayer("Back").TileHeight - 4));
				if (f.CanItemBePlacedHere(targetLocation))
				{
					break;
				}
			}
			if (attempts <= 0)
			{
				return true;
			}
			break;
		}
		case 4L:
			soundName = "thunder_small";
			message = Game1.content.LoadString("Strings\\Events:SoundInTheNight_Earthquake");
			break;
		}
		Game1.freezeControls = true;
		return false;
	}

	/// <inheritdoc />
	public override bool tickUpdate(GameTime time)
	{
		timer += (float)time.ElapsedGameTime.TotalMilliseconds;
		if (timer > 1500f && !playedSound)
		{
			if (!string.IsNullOrEmpty(soundName))
			{
				Game1.playSound(soundName);
				playedSound = true;
			}
			if (!playedSound && message != null)
			{
				Game1.drawObjectDialogue(message);
				Game1.globalFadeToClear();
				showedMessage = true;
				if (message == null)
				{
					finished = true;
				}
				else
				{
					Game1.afterDialogues = delegate
					{
						finished = true;
					};
				}
			}
		}
		if (timer > timeUntilText && !showedMessage)
		{
			Game1.pauseThenMessage(10, message);
			showedMessage = true;
			if (message == null)
			{
				finished = true;
			}
			else
			{
				Game1.afterDialogues = delegate
				{
					finished = true;
				};
			}
		}
		if (finished)
		{
			Game1.freezeControls = false;
			return true;
		}
		return false;
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), Color.Black);
		if (!showedMessage)
		{
			b.Draw(Game1.mouseCursors_1_6, new Vector2(12f, Game1.viewport.Height - 12 - 76), new Rectangle(256 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0 / 100.0) * 19, 413, 19, 19), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		}
	}

	/// <inheritdoc />
	public override void makeChangesToLocation()
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		Farm f = Game1.getFarm();
		switch (behavior)
		{
		case 0L:
		{
			Object o = ItemRegistry.Create<Object>("(BC)96");
			o.MinutesUntilReady = 24000 - Game1.timeOfDay;
			f.objects.Add(targetLocation, o);
			break;
		}
		case 1L:
			f.terrainFeatures.Remove(targetLocation);
			f.terrainFeatures.Remove(targetLocation + new Vector2(1f, 0f));
			f.terrainFeatures.Remove(targetLocation + new Vector2(1f, 1f));
			f.terrainFeatures.Remove(targetLocation + new Vector2(0f, 1f));
			f.resourceClumps.Add(new ResourceClump(622, 2, 2, targetLocation));
			break;
		case 2L:
		{
			AnimalHouse indoors = (AnimalHouse)targetBuilding.GetIndoors();
			long idOfRemove = 0L;
			foreach (long a in indoors.animalsThatLiveHere)
			{
				if (!indoors.animals.ContainsKey(a))
				{
					idOfRemove = a;
					break;
				}
			}
			if (!Game1.getFarm().animals.ContainsKey(idOfRemove))
			{
				break;
			}
			Game1.getFarm().animals.Remove(idOfRemove);
			indoors.animalsThatLiveHere.Remove(idOfRemove);
			{
				foreach (KeyValuePair<long, FarmAnimal> pair in Game1.getFarm().animals.Pairs)
				{
					pair.Value.moodMessage.Value = 5;
				}
				break;
			}
		}
		case 3L:
			f.objects.Add(targetLocation, ItemRegistry.Create<Object>("(BC)95"));
			break;
		}
	}
}
