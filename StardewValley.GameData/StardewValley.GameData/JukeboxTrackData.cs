using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>The data for a jukebox track.</summary>
public class JukeboxTrackData
{
	/// <summary>A tokenizable string for the track's display name, or <c>null</c> to use the ID (i.e. cue name).</summary>
	public string Name;

	/// <summary>Whether this track is available. This can be <c>true</c> (always available), <c>false</c> (never available), or <c>null</c> (available if the player has heard it).</summary>
	[ContentSerializer(Optional = true)]
	public bool? Available;

	/// <summary>A list of alternative track IDs. Any tracks matching one of these IDs will use this entry.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> AlternativeTrackIds;
}
