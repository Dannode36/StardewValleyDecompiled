using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Movies;

/// <summary>As part of <see cref="T:StardewValley.GameData.Movies.MovieReaction" />, possible dialogue from the NPC during the movie.</summary>
public class SpecialResponses
{
	/// <summary>The dialogue to show when the player interacts with the NPC in the theater lobby before the movie starts, if any.</summary>
	[ContentSerializer(Optional = true)]
	public CharacterResponse BeforeMovie;

	/// <summary>The dialogue to show during the movie based on the <see cref="F:StardewValley.GameData.Movies.CharacterResponse.ResponsePoint" />, if any.</summary>
	[ContentSerializer(Optional = true)]
	public CharacterResponse DuringMovie;

	/// <summary>The dialogue to show when the player interacts with the NPC in the theater lobby after the movie ends, if any.</summary>
	[ContentSerializer(Optional = true)]
	public CharacterResponse AfterMovie;
}
