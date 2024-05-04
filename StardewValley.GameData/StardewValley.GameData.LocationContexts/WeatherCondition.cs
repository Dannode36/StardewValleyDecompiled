using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.LocationContexts;

/// <summary>As part of <see cref="T:StardewValley.GameData.LocationContexts.LocationContextData" />, a weather rule to apply for locations in this context.</summary>
public class WeatherCondition
{
	/// <summary>A unique string ID for this entry within the current location context.</summary>
	public string Id;

	/// <summary>A game state query which indicates whether to apply the weather. Defaults to always applied.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>The weather ID to set.</summary>
	public string Weather;
}
