namespace StardewValley.GameData;

public enum MusicContext
{
	Default,
	/// <remarks>
	/// Confusingly, <see cref="F:StardewValley.GameData.MusicContext.SubLocation" /> has a higher MusicContext value than <see cref="F:StardewValley.GameData.MusicContext.Default" />, but is used when figuring out what song to play in split-screen.
	/// Songs with this value are prioritized above ambient noises, but below other instances' default songs -- so this should be used for things like specialized ambient
	/// music.
	/// </remarks>
	SubLocation,
	MusicPlayer,
	Event,
	MiniGame,
	ImportantSplitScreenMusic,
	MAX
}
