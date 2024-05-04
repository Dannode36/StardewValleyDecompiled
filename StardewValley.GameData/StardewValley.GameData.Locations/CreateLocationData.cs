using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Locations;

/// <summary>As part of <see cref="T:StardewValley.GameData.Locations.LocationData" />, the data to use to create a location.</summary>
public class CreateLocationData
{
	/// <summary>The asset name for the map to use for this location.</summary>
	public string MapPath;

	/// <summary>The full name of the C# location class to create. This must be one of the vanilla types to avoid a crash when saving. Defaults to a generic <c>StardewValley.GameLocation</c>.</summary>
	[ContentSerializer(Optional = true)]
	public string Type;

	/// <summary>Whether this location is always synchronized to farmhands in multiplayer, even if they're not in the location. Any location which allows building cabins <strong>must</strong> have this enabled to avoid breaking game logic.</summary>
	[ContentSerializer(Optional = true)]
	public bool AlwaysActive;
}
