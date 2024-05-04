using System;
using System.Text;
using Microsoft.Xna.Framework;

namespace StardewValley.Audio;

/// <inheritdoc cref="T:StardewValley.Audio.ISoundsHelper" />
public class SoundsHelper : ISoundsHelper
{
	/// <summary>The default pitch value.</summary>
	public const float DefaultPitch = 1200f;

	/// <summary>The maximum pitch value.</summary>
	public const float MaxPitch = 2400f;

	/// <summary>The maximum distance from the screen at which a positional sound can play. The audio volume drops linearly until it reaches zero.</summary>
	public static int MaxDistanceFromScreen = 12;

	/// <summary>The method which logs sounds, if logging is enabled.</summary>
	private Action<string, GameLocation, Vector2?, int?, float, SoundContext, string> LogSound;

	/// <inheritdoc />
	public virtual bool LogSounds
	{
		get
		{
			return LogSound != null;
		}
		set
		{
			if (value)
			{
				LogSound = LogSoundImpl;
			}
			else
			{
				LogSound = null;
			}
		}
	}

	/// <inheritdoc />
	public virtual bool ShouldPlayLocal(SoundContext context)
	{
		if (context == SoundContext.NPC && Game1.eventUp)
		{
			return false;
		}
		return true;
	}

	/// <inheritdoc />
	public virtual float GetVolumeForDistance(GameLocation location, Vector2? position)
	{
		if (location == null)
		{
			return 1f;
		}
		if (location.NameOrUniqueName != Game1.currentLocation?.NameOrUniqueName)
		{
			return 0f;
		}
		if (!position.HasValue)
		{
			return 1f;
		}
		float tileDistance = Utility.distanceFromScreen(position.Value * 64f) / 64f;
		if (tileDistance <= 0f)
		{
			return 1f;
		}
		if (tileDistance >= (float)MaxDistanceFromScreen)
		{
			return 0f;
		}
		return 1f - tileDistance / (float)MaxDistanceFromScreen;
	}

	/// <inheritdoc />
	public virtual bool PlayLocal(string cueName, GameLocation location, Vector2? position, int? pitch, SoundContext context, out ICue cue)
	{
		try
		{
			cue = Game1.soundBank.GetCue(cueName);
			SetPitch(cue, ((float?)pitch) ?? 1200f, pitch.HasValue);
			if (!ShouldPlayLocal(context))
			{
				LogSound?.Invoke(cueName, location, position, pitch, 1f, context, "disabled for context");
				return false;
			}
			float volume = GetVolumeForDistance(location, position);
			if (volume <= 0f)
			{
				LogSound?.Invoke(cueName, location, position, pitch, volume, context, "disabled for distance");
				return false;
			}
			cue.Play();
			if (volume < 1f)
			{
				cue.Volume *= volume;
			}
			LogSound?.Invoke(cueName, location, position, pitch, volume, context, null);
			return true;
		}
		catch (Exception ex)
		{
			Game1.debugOutput = Game1.parseText(ex.Message);
			Game1.log.Error("Error playing sound.", ex);
			cue = DummySoundBank.DummyCue;
			return false;
		}
	}

	/// <inheritdoc />
	public virtual void PlayAll(string cueName, GameLocation location, Vector2? position, int? pitch, SoundContext context)
	{
		if (CanSkipSoundSync(location, position, context))
		{
			PlayLocal(cueName, location, position, pitch, context, out var _);
		}
		else
		{
			location.netAudio.Fire(cueName, position, pitch, context);
		}
	}

	/// <inheritdoc />
	public void SetPitch(ICue cue, float pitch, bool forcePitch = true)
	{
		if (cue == null)
		{
			return;
		}
		cue.SetVariable("Pitch", pitch);
		if (!forcePitch)
		{
			return;
		}
		try
		{
			if (!cue.IsPitchBeingControlledByRPC)
			{
				cue.Pitch = Utility.Lerp(-1f, 1f, pitch / 2400f);
			}
		}
		catch
		{
		}
	}

	/// <summary>Get whether a multiplayer sound can be played directly without syncing it to other players.</summary>
	/// <param name="location">The location in which the sound is playing.</param>
	/// <param name="position">The tile position from which the sound is playing.</param>
	/// <param name="context">The source which triggered the sound.</param>
	public virtual bool CanSkipSoundSync(GameLocation location, Vector2? position, SoundContext context)
	{
		if (!LocalMultiplayer.IsLocalMultiplayer(is_local_only: true))
		{
			return false;
		}
		if (Game1.eventUp && context == SoundContext.NPC)
		{
			return false;
		}
		if (ShouldPlayLocal(context) && GetVolumeForDistance(location, position) > 0f)
		{
			return true;
		}
		if (location != null)
		{
			bool someoneCanHear = false;
			foreach (Game1 gameInstance in GameRunner.instance.gameInstances)
			{
				if (gameInstance.instanceGameLocation?.NameOrUniqueName == location.NameOrUniqueName)
				{
					someoneCanHear = true;
					break;
				}
			}
			if (someoneCanHear && position.HasValue && position != Vector2.Zero)
			{
				someoneCanHear = false;
				GameRunner.instance.ExecuteForInstances(delegate
				{
					if (!someoneCanHear && ShouldPlayLocal(context) && GetVolumeForDistance(location, position) > 0f)
					{
						someoneCanHear = true;
					}
				});
			}
			return someoneCanHear;
		}
		return true;
	}

	/// <summary>Play a game sound for the local player.</summary>
	/// <param name="cueName">The sound ID to play.</param>
	/// <param name="location">The location in which the sound is playing, if applicable.</param>
	/// <param name="position">The tile position from which the sound is playing, or <c>null</c> if it's playing throughout the location.</param>
	/// <param name="pitch">The pitch modifier to apply, or <c>null</c> for the default pitch.</param>
	/// <param name="volume">The distance-adjusted volume.</param>
	/// <param name="context">The source which triggered a game sound.</param>
	/// <param name="skipReason">The reason the sound wasn't played, if applicable.</param>
	protected virtual void LogSoundImpl(string cueName, GameLocation location, Vector2? position, int? pitch, float volume, SoundContext context, string skipReason = null)
	{
		bool num = skipReason != null;
		StringBuilder summary = new StringBuilder();
		summary.Append("Played sound '").Append(cueName).Append("'");
		if (location == null)
		{
			summary.Append(" everywhere");
		}
		else
		{
			summary.Append(" in ").Append(location.NameOrUniqueName);
			if (position.HasValue)
			{
				summary.Append(" (").Append(position.Value.X).Append(", ")
					.Append(position.Value.Y)
					.Append(")");
			}
		}
		if (pitch.HasValue)
		{
			summary.Append(" with pitch ").Append(pitch.Value);
		}
		if (!num && volume < 1f)
		{
			summary.Append(" with distance").Append(volume);
		}
		if (num)
		{
			summary.Append(" (").Append(skipReason).Append(")");
		}
		Game1.log.Debug(summary.ToString());
	}
}
