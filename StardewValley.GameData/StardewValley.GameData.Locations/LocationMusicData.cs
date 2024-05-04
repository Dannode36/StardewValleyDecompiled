using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Locations;

/// <summary>As part of <see cref="T:StardewValley.GameData.Locations.LocationData" />, a music cue to play when the player enters the location (subject to the other fields like <see cref="F:StardewValley.GameData.Locations.LocationData.MusicContext" />).</summary>
public class LocationMusicData
{
	/// <summary>The backing field for <see cref="P:StardewValley.GameData.Locations.LocationMusicData.Id" />.</summary>
	private string IdImpl;

	/// <summary>A unique string ID for this track within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_TrackName</c>. Defaults to <see cref="P:StardewValley.GameData.Locations.LocationMusicData.Track" /> if omitted.</summary>
	[ContentSerializer(Optional = true)]
	public string Id
	{
		get
		{
			return IdImpl ?? Track;
		}
		set
		{
			IdImpl = value;
		}
	}

	/// <summary>The audio track ID to play, or <c>null</c> to stop music.</summary>
	public string Track { get; set; }

	/// <summary>A game state query which indicates whether the music should be played. Defaults to true.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition { get; set; }
}
