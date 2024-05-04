using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Movies;

/// <summary>As part of <see cref="T:StardewValley.GameData.Movies.MovieData" />, a scene to show when watching the movie.</summary>
public class MovieScene
{
	/// <summary>The screen index within the movie's spritesheet row.</summary>
	/// <remarks>See remarks on <see cref="F:StardewValley.GameData.Movies.MovieData.SheetIndex" /> for the expected sprite layout.</remarks>
	[ContentSerializer(Optional = true)]
	public int Image = -1;

	/// <summary>If set, the audio cue ID for the music to play while the scene is shown. Default none.</summary>
	[ContentSerializer(Optional = true)]
	public string Music;

	/// <summary>If set, the audio cue ID for a sound effect to play when the scene starts. Default none.</summary>
	[ContentSerializer(Optional = true)]
	public string Sound;

	/// <summary>The number of milliseconds to wait after the scene starts before showing the <see cref="F:StardewValley.GameData.Movies.MovieScene.Text" />, <see cref="F:StardewValley.GameData.Movies.MovieScene.Script" />, and <see cref="F:StardewValley.GameData.Movies.MovieScene.Image" />.</summary>
	[ContentSerializer(Optional = true)]
	public int MessageDelay = 500;

	/// <summary>If set, a tokenizable string for the custom event script to run for any custom audio, images, etc.</summary>
	[ContentSerializer(Optional = true)]
	public string Script;

	/// <summary>If set, a tokenizable string for the text to show in a message box while the scene plays. The scene will pause until the player closes it.</summary>
	[ContentSerializer(Optional = true)]
	public string Text;

	/// <summary>Whether to shake the movie screen image for the duration of the scene.</summary>
	[ContentSerializer(Optional = true)]
	public bool Shake;

	/// <summary>If set, an optional hook where NPCs may interject a reaction dialogue via <see cref="F:StardewValley.GameData.Movies.CharacterResponse.ResponsePoint" />.</summary>
	[ContentSerializer(Optional = true)]
	public string ResponsePoint;

	/// <summary>A key which uniquely identifies this movie scene. This should only contain alphanumeric/underscore/dot characters. For custom movie scenes, this should be prefixed with your mod ID like <c>Example.ModId_MovieScene</c>.</summary>
	public string Id;
}
