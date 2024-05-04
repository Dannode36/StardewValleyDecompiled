using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Movies;

/// <summary>As part of <see cref="T:StardewValley.GameData.Movies.SpecialResponses" />, a possible dialogue to show.</summary>
public class CharacterResponse
{
	/// <summary>
	///   <para>For <see cref="F:StardewValley.GameData.Movies.SpecialResponses.DuringMovie" />, the <see cref="F:StardewValley.GameData.Movies.MovieScene.ResponsePoint" /> used to decide whether it should be shown during a scene.</para>
	///
	///   <para>For <see cref="F:StardewValley.GameData.Movies.SpecialResponses.BeforeMovie" /> or <see cref="F:StardewValley.GameData.Movies.SpecialResponses.AfterMovie" />, this field is ignored.</para>
	/// </summary>
	[ContentSerializer(Optional = true)]
	public string ResponsePoint;

	/// <summary>
	///   <para>For <see cref="F:StardewValley.GameData.Movies.SpecialResponses.DuringMovie" />, an optional event script to run before the <see cref="F:StardewValley.GameData.Movies.CharacterResponse.Text" /> is shown.</para>
	///
	///   <para>For <see cref="F:StardewValley.GameData.Movies.SpecialResponses.BeforeMovie" /> or <see cref="F:StardewValley.GameData.Movies.SpecialResponses.AfterMovie" />, this field is ignored.</para>
	/// </summary>
	[ContentSerializer(Optional = true)]
	public string Script;

	/// <summary>The translated dialogue text to show.</summary>
	[ContentSerializer(Optional = true)]
	public string Text;
}
