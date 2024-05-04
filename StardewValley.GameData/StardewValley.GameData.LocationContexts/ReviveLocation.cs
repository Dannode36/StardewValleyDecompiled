using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.LocationContexts;

/// <summary>As part of <see cref="T:StardewValley.GameData.LocationContexts.LocationContextData" />, the locations where a player wakes up after passing out or getting knocked out.</summary>
public class ReviveLocation
{
	/// <summary>A unique string ID for this entry within the current location context.</summary>
	public string Id;

	/// <summary>A game state query which indicates whether this entry is active. Defaults to always applied.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>The internal location name.</summary>
	public string Location;

	/// <summary>The tile position within the location.</summary>
	public Point Position;
}
