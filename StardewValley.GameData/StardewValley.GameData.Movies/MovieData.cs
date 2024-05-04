using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Movies;

/// <summary>The metadata for a movie that can play at the movie theater.</summary>
public class MovieData
{
	/// <summary>A key which uniquely identifies this movie. This should only contain alphanumeric/underscore/dot characters. For custom movies, this should be prefixed with your mod ID like <c>Example.ModId_MovieName</c>.</summary>
	[ContentSerializer(Optional = true)]
	public string Id;

	/// <summary>The seasons when the movie plays, or none to allow any season.</summary>
	[ContentSerializer(Optional = true)]
	public List<Season> Seasons;

	/// <summary>If set, the movie is available when <c>{year} % <see cref="F:StardewValley.GameData.Movies.MovieData.YearModulus" /> == <see cref="F:StardewValley.GameData.Movies.MovieData.YearRemainder" /></c> (where <c>{year}</c> is the number of years since the movie theater was built and {remainder} defaults to zero). For example, a modulus of 2 with remainder 1 is shown in the second year and every other year thereafter.</summary>
	[ContentSerializer(Optional = true)]
	public int? YearModulus;

	/// <inheritdoc cref="F:StardewValley.GameData.Movies.MovieData.YearModulus" />
	[ContentSerializer(Optional = true)]
	public int? YearRemainder;

	/// <summary>The asset name for the movie poster and screen images, or <c>null</c> to use <c>LooseSprites\Movies</c>.</summary>
	/// <remarks>This must be a spritesheet with one 490×128 pixel row per movie. A 13×19 area in the top-left corner of the row should contain the movie poster. With a 16-pixel offset from the left edge, there should be two rows of five 90×61 pixel movie screen images, with a six-pixel gap between each image. (The movie doesn't need to use all of the image slots.)</remarks>
	[ContentSerializer(Optional = true)]
	public string Texture;

	/// <summary>The sprite index within the <see cref="F:StardewValley.GameData.Movies.MovieData.Texture" /> for this movie poster and screen images.</summary>
	public int SheetIndex;

	/// <summary>A tokenizable string for the translated movie title.</summary>
	public string Title;

	/// <summary>A tokenizable string for the translated movie description, shown when interacting with the movie poster.</summary>
	public string Description;

	/// <summary>A list of tags which describe the genre or other metadata, which can be matched by <see cref="F:StardewValley.GameData.Movies.MovieReaction.Tag" />.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> Tags;

	/// <summary>The prizes that can be grabbed in the crane game while this movie is playing (in addition to the default items).</summary>
	[ContentSerializer(Optional = true)]
	public List<MovieCranePrizeData> CranePrizes = new List<MovieCranePrizeData>();

	/// <summary>The prize rarity lists whose default items to clear when this movie is playing, so they're only taken from <see cref="F:StardewValley.GameData.Movies.MovieData.CranePrizes" />.</summary>
	[ContentSerializer(Optional = true)]
	public List<int> ClearDefaultCranePrizeGroups = new List<int>();

	/// <summary>The scenes to show when watching the movie.</summary>
	public List<MovieScene> Scenes;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
