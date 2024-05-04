using StardewValley.GameData;

namespace StardewValley.Objects;

public class MusicPlayerTrinketEffect : TrinketEffect
{
	public MusicPlayerTrinketEffect(Trinket trinket)
		: base(trinket)
	{
	}

	public override void Apply(Farmer farmer)
	{
	}

	public override void Unapply(Farmer farmer)
	{
		Game1.stopMusicTrack(MusicContext.MusicPlayer);
	}
}
