using System.Collections.Generic;
using StardewValley.GameData.LocationContexts;

namespace StardewValley;

/// <summary>Manages data about the game's location contexts.</summary>
public static class LocationContexts
{
	/// <summary>The location context ID for the valley in <c>Data/LocationContexts</c>.</summary>
	public const string DefaultId = "Default";

	/// <summary>The location context ID for the desert in <c>Data/LocationContexts</c>.</summary>
	public const string DesertId = "Desert";

	/// <summary>The location context ID for Ginger Island in <c>Data/LocationContexts</c>.</summary>
	public const string IslandId = "Island";

	/// <summary>The location context data for Ginger Island.</summary>
	public static LocationContextData Island => Require("Island");

	/// <summary>The location context data for the valley.</summary>
	public static LocationContextData Default => Require("Default");

	/// <summary>Get a location context by ID.</summary>
	/// <param name="id">The location context's ID in <c>Data/LocationContext</c>.</param>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">There's no location context with the given <paramref name="id" />.</exception>
	public static LocationContextData Require(string id)
	{
		if (id == null || !Game1.locationContextData.TryGetValue(id, out var data))
		{
			throw new KeyNotFoundException("There's no entry in Data/LocationContexts with the required ID '" + id + "'.");
		}
		return data;
	}
}
