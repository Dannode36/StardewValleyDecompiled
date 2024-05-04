using Microsoft.Xna.Framework;

namespace StardewValley.Audio;

/// <summary>Encapsulates the game logic for playing sound effects (excluding music and background ambience).</summary>
public interface ISoundsHelper
{
	/// <summary>Whether to log sounds being played to the console to simplify troubleshooting.</summary>
	bool LogSounds { get; set; }

	/// <summary>Get whether local sounds for a category should be played right now.</summary>
	/// <param name="context">The source which triggered the game sound.</param>
	bool ShouldPlayLocal(SoundContext context);

	/// <summary>Get the volume at which to play a local sound based on its distance from the current player.</summary>
	/// <param name="location">The location in which the sound is playing, if applicable.</param>
	/// <param name="position">The tile position from which the sound is playing, if applicable.</param>
	float GetVolumeForDistance(GameLocation location, Vector2? position);

	/// <summary>Play a game sound for the local player.</summary>
	/// <param name="cueName">The sound ID to play.</param>
	/// <param name="location">The location in which the sound is playing, if applicable.</param>
	/// <param name="position">The tile position from which the sound is playing, or <c>null</c> if it's playing throughout the location.</param>
	/// <param name="pitch">The pitch modifier to apply, or <c>null</c> for the default pitch.</param>
	/// <param name="cue">The cue instance that was started, or a no-op cue if it failed.</param>
	/// <returns>Returns whether the cue exists and was started successfully.</returns>
	/// <param name="context">The source which triggered a game sound.</param>
	bool PlayLocal(string cueName, GameLocation location, Vector2? position, int? pitch, SoundContext context, out ICue cue);

	/// <summary>Play a game sound for all players who can hear it.</summary>
	/// <param name="cueName">The sound ID to play.</param>
	/// <param name="location">The location in which the sound is playing.</param>
	/// <param name="position">The tile position from which the sound is playing, or <c>null</c> if it's playing throughout the location.</param>
	/// <param name="pitch">The pitch modifier to apply, or <c>null</c> for the default pitch.</param>
	/// <param name="context">The source which triggered a game sound.</param>
	void PlayAll(string cueName, GameLocation location, Vector2? position, int? pitch, SoundContext context);

	/// <summary>Set the pitch value for an audio cue.</summary>
	/// <param name="cue">The audio cue to change.</param>
	/// <param name="pitch">The pitch to set.</param>
	/// <param name="forcePitch">If the cue doesn't have a built-in <c>Pitch</c> variable, set it dynamically if possible.</param>
	/// <remarks>This is only needed when working with audio cues directly. Most code should set the pitch through <see cref="M:StardewValley.Audio.ISoundsHelper.PlayAll(System.String,StardewValley.GameLocation,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{System.Int32},StardewValley.Audio.SoundContext)" /> or <see cref="M:StardewValley.Audio.ISoundsHelper.PlayLocal(System.String,StardewValley.GameLocation,System.Nullable{Microsoft.Xna.Framework.Vector2},System.Nullable{System.Int32},StardewValley.Audio.SoundContext,StardewValley.ICue@)" /> instead.</remarks>
	void SetPitch(ICue cue, float pitch, bool forcePitch = true);
}
