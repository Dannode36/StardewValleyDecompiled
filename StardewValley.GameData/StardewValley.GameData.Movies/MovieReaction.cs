using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Movies;

/// <summary>As part of <see cref="T:StardewValley.GameData.Movies.MovieCharacterReaction" />, a possible reactions to movies matching a tag.</summary>
public class MovieReaction
{
	/// <summary>
	///   <para>A pattern which determines which movies this reaction can apply to.</para>
	///
	///   <para>This can be any of the following:</para>
	///   <list type="bullet">
	///     <item><description><c>"*"</c> to match any movie.</description></item>
	///     <item><description>A tag to match any movie which has that tag in its <see cref="F:StardewValley.GameData.Movies.MovieData.Tags" /> list.</description></item>
	///     <item><description>An ID to match any movie with that <see cref="F:StardewValley.GameData.Movies.MovieData.Id" /> value.</description></item>
	///     <item><description>How much the NPC enjoys this movie, based on the <see cref="F:StardewValley.GameData.Movies.MovieReaction.Response" /> for matched entries. This performs a two-pass check: any <see cref="T:StardewValley.GameData.Movies.MovieReaction" /> entry which matches with a non-response <see cref="F:StardewValley.GameData.Movies.MovieReaction.Tag" /> is used to determine the NPC's response, defaulting to <c>like</c>. The result is then checked against this value.</description></item>
	///   </list>
	/// </summary>
	public string Tag;

	/// <summary>How much the NPC enjoys the movie (one of <c>love</c>, <c>like</c>, or <c>dislike</c>).</summary>
	[ContentSerializer(Optional = true)]
	public string Response = "like";

	/// <summary>A list of internal NPC names. If this isn't empty, at least one of these NPCs must be present in the theater for this reaction to apply.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> Whitelist = new List<string>();

	/// <summary>If set, possible dialogue from the NPC during the movie.</summary>
	[ContentSerializer(Optional = true)]
	public SpecialResponses SpecialResponses;

	/// <summary>A key which uniquely identifies this movie reaction. This should only contain alphanumeric/underscore/dot characters. For custom movie reactions, this should be prefixed with your mod ID like <c>Example.ModId_ReactionName</c>.</summary>
	public string Id = "";

	/// <summary>Whether this movie reaction should apply to a given movie.</summary>
	/// <param name="movieData">The movie data to match.</param>
	/// <param name="moviePatrons">The internal names for NPCs watching the movie.</param>
	/// <param name="otherValidTags">The other tags to match via <see cref="F:StardewValley.GameData.Movies.MovieReaction.Tag" />.</param>
	public bool ShouldApplyToMovie(MovieData movieData, IEnumerable<string> moviePatrons, params string[] otherValidTags)
	{
		if (Whitelist != null)
		{
			if (moviePatrons == null)
			{
				return false;
			}
			foreach (string requiredCharacter in Whitelist)
			{
				if (!moviePatrons.Contains(requiredCharacter))
				{
					return false;
				}
			}
		}
		if (Tag == movieData.Id)
		{
			return true;
		}
		if (movieData.Tags.Contains(Tag))
		{
			return true;
		}
		if (Tag == "*")
		{
			return true;
		}
		if (otherValidTags.Contains(Tag))
		{
			return true;
		}
		return false;
	}
}
