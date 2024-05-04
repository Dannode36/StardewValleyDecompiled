using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Movies;

/// <summary>Metadata for how an NPC can react to movies.</summary>
public class MovieCharacterReaction
{
	/// <summary>A unique ID for this entry.</summary>
	[ContentSerializerIgnore]
	public string Id => NPCName;

	/// <summary>The internal name of the NPC for which to define reactions.</summary>
	public string NPCName { get; set; }

	/// <summary>The possible movie reactions for this NPC.</summary>
	[ContentSerializer(Optional = true)]
	public List<MovieReaction> Reactions { get; set; }
}
