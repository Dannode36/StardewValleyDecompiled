using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Locations;

/// <summary>As part of <see cref="T:StardewValley.GameData.Locations.LocationData" />, a distinct fish area within the location which may have its own fish (via <see cref="P:StardewValley.GameData.Locations.SpawnFishData.FishAreaId" />) or crab pot catches.</summary>
public class FishAreaData
{
	/// <summary>A tokenizable string for the translated area name, if any.</summary>
	[ContentSerializer(Optional = true)]
	public string DisplayName { get; set; }

	/// <summary>If set, the tile area within the location where the crab pot must be placed.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle? Position { get; set; }

	/// <summary>The fish types that can be caught with crab pots in this area.</summary>
	/// <remarks>These will be matched against field index 4 in <c>Data/Fish</c> for crab pot fish. If this list is null or empty, it'll default to <c>freshwater</c>.</remarks>
	[ContentSerializer(Optional = true)]
	public List<string> CrabPotFishTypes { get; set; } = new List<string>();


	/// <summary>The chance that crab pots will find junk instead of a fish in this area, if the player doesn't have the Mariner profession.</summary>
	[ContentSerializer(Optional = true)]
	public float CrabPotJunkChance { get; set; } = 0.2f;

}
