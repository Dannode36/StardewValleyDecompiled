using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StardewValley.BellsAndWhistles;

[InstanceStatics]
public class AmbientLocationSounds
{
	public const int sound_babblingBrook = 0;

	public const int sound_cracklingFire = 1;

	public const int sound_engine = 2;

	public const int sound_cricket = 3;

	public const int sound_waterfall = 4;

	public const int sound_waterfall_big = 5;

	public const int numberOfSounds = 6;

	public const float doNotPlay = 9999999f;

	private static Dictionary<Vector2, int> sounds = new Dictionary<Vector2, int>();

	private static int updateTimer = 100;

	private static int farthestSoundDistance = 1024;

	private static float[] shortestDistanceForCue;

	private static ICue babblingBrook;

	private static ICue cracklingFire;

	private static ICue engine;

	private static ICue cricket;

	private static ICue waterfall;

	private static ICue waterfallBig;

	private static float volumeOverrideForLocChange;

	public static void InitShared()
	{
		if (babblingBrook == null)
		{
			Game1.playSound("babblingBrook", out babblingBrook);
			babblingBrook.Pause();
		}
		if (cracklingFire == null)
		{
			Game1.playSound("cracklingFire", out cracklingFire);
			cracklingFire.Pause();
		}
		if (engine == null)
		{
			Game1.playSound("heavyEngine", out engine);
			engine.Pause();
		}
		if (cricket == null)
		{
			Game1.playSound("cricketsAmbient", out cricket);
			cricket.Pause();
		}
		if (waterfall == null)
		{
			Game1.playSound("waterfall", out waterfall);
			waterfall.Pause();
		}
		if (waterfallBig == null)
		{
			Game1.playSound("waterfall_big", out waterfallBig);
			waterfallBig.Pause();
		}
		shortestDistanceForCue = new float[6];
	}

	public static void update(GameTime time)
	{
		if (sounds.Count == 0)
		{
			return;
		}
		if (volumeOverrideForLocChange < 1f)
		{
			volumeOverrideForLocChange += (float)time.ElapsedGameTime.Milliseconds * 0.0003f;
		}
		updateTimer -= time.ElapsedGameTime.Milliseconds;
		if (updateTimer > 0)
		{
			return;
		}
		for (int i = 0; i < shortestDistanceForCue.Length; i++)
		{
			shortestDistanceForCue[i] = 9999999f;
		}
		Vector2 farmerPosition = Game1.player.getStandingPosition();
		foreach (KeyValuePair<Vector2, int> pair in sounds)
		{
			float distance = Vector2.Distance(pair.Key, farmerPosition);
			if (shortestDistanceForCue[pair.Value] > distance)
			{
				shortestDistanceForCue[pair.Value] = distance;
			}
		}
		if (volumeOverrideForLocChange >= 0f)
		{
			for (int i = 0; i < shortestDistanceForCue.Length; i++)
			{
				if (shortestDistanceForCue[i] <= (float)farthestSoundDistance * 1.5f)
				{
					float volume = Math.Min(volumeOverrideForLocChange, Math.Min(1f, 1f - shortestDistanceForCue[i] / ((float)farthestSoundDistance * 1.5f)));
					volume = (float)Math.Pow(volume, 5.0);
					switch (i)
					{
					case 0:
						if (babblingBrook != null)
						{
							babblingBrook.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
							babblingBrook.Resume();
						}
						break;
					case 1:
						if (cracklingFire != null)
						{
							cracklingFire.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
							cracklingFire.Resume();
						}
						break;
					case 2:
						if (engine != null)
						{
							engine.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
							engine.Resume();
						}
						break;
					case 3:
						if (cricket != null)
						{
							cricket.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
							cricket.Resume();
						}
						break;
					case 4:
						if (waterfall != null)
						{
							waterfall.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
							waterfall.Resume();
						}
						break;
					case 5:
						if (waterfallBig != null)
						{
							waterfallBig.SetVariable("Volume", volume * 100f * Math.Min(Game1.ambientPlayerVolume, Game1.options.ambientVolumeLevel));
							waterfallBig.Resume();
						}
						break;
					}
				}
				else
				{
					switch (i)
					{
					case 0:
						babblingBrook?.Pause();
						break;
					case 1:
						cracklingFire?.Pause();
						break;
					case 2:
						engine?.Pause();
						break;
					case 3:
						cricket?.Pause();
						break;
					case 4:
						waterfall?.Pause();
						break;
					case 5:
						waterfallBig?.Pause();
						break;
					}
				}
			}
		}
		updateTimer = 100;
	}

	public static void changeSpecificVariable(string variableName, float value, int whichSound)
	{
		if (whichSound == 2)
		{
			engine?.SetVariable(variableName, value);
		}
	}

	public static void addSound(Vector2 tileLocation, int whichSound)
	{
		sounds.TryAdd(tileLocation * 64f, whichSound);
	}

	public static void removeSound(Vector2 tileLocation)
	{
		if (sounds.TryGetValue(tileLocation * 64f, out var sound))
		{
			switch (sound)
			{
			case 0:
				babblingBrook?.Pause();
				break;
			case 1:
				cracklingFire?.Pause();
				break;
			case 2:
				engine?.Pause();
				break;
			case 3:
				cricket?.Pause();
				break;
			case 4:
				waterfall?.Pause();
				break;
			case 5:
				waterfallBig?.Pause();
				break;
			}
			sounds.Remove(tileLocation * 64f);
		}
	}

	public static void onLocationLeave()
	{
		sounds.Clear();
		volumeOverrideForLocChange = -0.5f;
		babblingBrook?.Pause();
		cracklingFire?.Pause();
		if (engine != null)
		{
			engine.SetVariable("Frequency", 100f);
			engine.Pause();
		}
		cricket?.Pause();
		waterfall?.Pause();
		waterfallBig?.Pause();
	}
}
