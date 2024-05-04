using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>An audio change to apply to the game's sound bank.</summary>
/// <remarks>This describes an override applied to the sound bank. The override is applied permanently for the current game session, even if it's later removed from the data asset. Overriding a cue will reset all values to the ones specified.</remarks>
public class AudioCueData
{
	/// <summary>A unique cue ID, used when playing the sound in-game. The ID should only contain alphanumeric/underscore/dot characters. For custom audio cues, this should be prefixed with your mod ID like <c>Example.ModId_AudioName</c>.</summary>
	public string Id;

	/// <summary>A list of file paths (not asset names) from which to load the audio. These can be absolute paths or relative to the game's <c>Content</c> folder. Each file can be <c>.ogg</c> or <c>.wav</c>. If you list multiple paths, a random one will be chosen each time it's played.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> FilePaths;

	/// <summary>The audio category, which determines which volume slider in the game options applies. This should be one of <c>Default</c>, <c>Music</c>, <c>Sound</c>, <c>Ambient</c>, or <c>Footsteps</c>. Defaults to <c>Default</c>.</summary>
	[ContentSerializer(Optional = true)]
	public string Category;

	/// <summary>
	///   <para>Whether the audio should be streamed from disk when it's played, instead of being loaded into memory ahead of time. This is only possible for Ogg Vorbis (<c>.ogg</c>) files, which otherwise will be decompressed in-memory on load.</para>
	///
	///   <para>This is a tradeoff between memory usage and performance, so you should consider which value is best for each audio cue:</para>
	///   <list type="bullet">
	///     <item><description><c>true</c>: Reduces memory usage when the audio cue isn't active, but increases performance impact when it's played. Playing the audio multiple times will multiply the memory and performance impact while they're active, since each play will stream a new instance. Recommended for longer audio cues (like music or ambient noise), or cues that are rarely used in a specific scenario (e.g. a sound that only plays once in an event).</description></item>
	///     <item><description><c>false</c>: Increases memory usage (since it's fully loaded into memory), but reduces performance impact when it's played. It can be played any number of times without affecting memory or performance (it'll just play the cached audio). Recommended for sound effects, or short audio cues that are played occasionally.</description></item>
	///   </list>
	/// </summary>
	[ContentSerializer(Optional = true)]
	public bool StreamedVorbis;

	/// <summary>Whether the audio cue loops continuously until stopped.</summary>
	[ContentSerializer(Optional = true)]
	public bool Looped;

	/// <summary>Whether to apply a reverb effect to the audio.</summary>
	[ContentSerializer(Optional = true)]
	public bool UseReverb;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
