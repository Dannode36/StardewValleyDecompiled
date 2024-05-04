using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Locations;

/// <summary>The data for a location to add to the game.</summary>
public class LocationData
{
	/// <summary>A tokenizable string for the translated location name. This is used anytime the location name is shown in-game for base game logic or mods. If omitted, the location will default to its internal name (i.e. the key in <c>Data/AdditionalLocationData</c>).</summary>
	[ContentSerializer(Optional = true)]
	public string DisplayName;

	/// <summary>The default tile position where the player should be placed when they arrive in the location, if arriving from a warp that didn't specify a tile position.</summary>
	[ContentSerializer(Optional = true)]
	public Point? DefaultArrivalTile;

	/// <summary>Whether NPCs should ignore this location when pathfinding between locations.</summary>
	[ContentSerializer(Optional = true)]
	public bool ExcludeFromNpcPathfinding;

	/// <summary>If set, the location will be created automatically when the save is loaded using this data.</summary>
	[ContentSerializer(Optional = true)]
	public CreateLocationData CreateOnLoad;

	/// <summary>The former location names which may appear in save data.</summary>
	/// <remarks>If a location in save data has a name which (a) matches one of these values and (b) doesn't match the name of a loaded location, its data will be loaded into this location instead.</remarks>
	[ContentSerializer(Optional = true)]
	public List<string> FormerLocationNames = new List<string>();

	/// <summary>Whether crops and trees can be planted and grown here by default, unless overridden by their plantable rules. If omitted, defaults to <c>true</c> on the farm and <c>false</c> elsewhere.</summary>
	[ContentSerializer(Optional = true)]
	public bool? CanPlantHere;

	/// <summary>Whether green rain trees and debris can spawn here by default.</summary>
	[ContentSerializer(Optional = true)]
	public bool CanHaveGreenRainSpawns = true;

	/// <summary>The items that can be found when digging artifact spots in the location.</summary>
	/// <remarks>
	///   <para>The items that can be dug up in a location are decided by combining this field with the one from the <c>Default</c> entry, sorting them by <see cref="P:StardewValley.GameData.Locations.ArtifactSpotDropData.Precedence" />, and taking the first drop whose fields match. Items with the same precedence are checked in the order listed.</para>
	///
	///   <para>For consistency, vanilla artifact drops prefer using these precedence values:</para>
	///   <list type="bullet">
	///     <item><description>-1000: location items which should override the global priority items (e.g. fossils on Ginger Island);</description></item>
	///     <item><description>-100: global priority items (e.g. Qi Beans);</description></item>
	///     <item><description>0: normal items;</description></item>
	///     <item><description>100: global fallback items (e.g. clay).</description></item>
	///   </list>
	/// </remarks>
	[ContentSerializer(Optional = true)]
	public List<ArtifactSpotDropData> ArtifactSpots = new List<ArtifactSpotDropData>();

	/// <summary>The distinct fishing areas within the location.</summary>
	/// <remarks>These can be referenced by <see cref="F:StardewValley.GameData.Locations.LocationData.Fish" /> via <see cref="P:StardewValley.GameData.Locations.SpawnFishData.FishAreaId" />, and determine which fish are collected by crab pots.</remarks>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, FishAreaData> FishAreas = new Dictionary<string, FishAreaData>();

	/// <summary>The items that can be found by fishing in the location.</summary>
	/// <remarks>
	///   <para>The items to catch in a location are decided by combining this field with the one from the <c>Default</c> entry, sorting them by <see cref="P:StardewValley.GameData.Locations.SpawnFishData.Precedence" />, and taking the first fish whose fields match. Items with the same precedence are shuffled randomly.</para>
	///
	///   <para>For consistency, vanilla fish prefer precedence values in these ranges:</para>
	///   <list type="bullet">
	///     <item><description>-1100 to -1000: global priority items (e.g. Qi Beans);</description></item>
	///     <item><description>-200 to -100: unique location items (e.g. legendary fish or secret items);</description></item>
	///     <item><description>-50 to -1: normal high-priority items;</description></item>
	///     <item><description>0: normal items;</description></item>
	///     <item><description>1 to 100: normal low-priority items;</description></item>
	///     <item><description>1000+: global fallback items (e.g. trash).</description></item>
	///   </list>
	/// </remarks>
	[ContentSerializer(Optional = true)]
	public List<SpawnFishData> Fish = new List<SpawnFishData>();

	/// <summary>The forage objects that can spawn in the location.</summary>
	[ContentSerializer(Optional = true)]
	public List<SpawnForageData> Forage = new List<SpawnForageData>();

	/// <summary>The minimum number of weeds to spawn in a day.</summary>
	[ContentSerializer(Optional = true)]
	public int MinDailyWeeds = 2;

	/// <summary>The maximum number of weeds to spawn in a day.</summary>
	[ContentSerializer(Optional = true)]
	public int MaxDailyWeeds = 5;

	/// <summary>A multiplier applied to the number of weeds spawned on the first day of the year.</summary>
	[ContentSerializer(Optional = true)]
	public int FirstDayWeedMultiplier = 15;

	/// <summary>The minimum forage to try spawning in one day, if the location has fewer than <see cref="F:StardewValley.GameData.Locations.LocationData.MaxSpawnedForageAtOnce" /> forage.</summary>
	[ContentSerializer(Optional = true)]
	public int MinDailyForageSpawn = 1;

	/// <summary>The maximum forage to try spawning in one day, if the location has fewer than <see cref="F:StardewValley.GameData.Locations.LocationData.MaxSpawnedForageAtOnce" /> forage.</summary>
	[ContentSerializer(Optional = true)]
	public int MaxDailyForageSpawn = 4;

	/// <summary>The maximum number of spawned forage that can be present at once on the map before they stop spawning.</summary>
	[ContentSerializer(Optional = true)]
	public int MaxSpawnedForageAtOnce = 6;

	/// <summary>The probability that digging a tile will produce clay, as a value between 0 (never) and 1 (always).</summary>
	[ContentSerializer(Optional = true)]
	public double ChanceForClay = 0.03;

	/// <summary>The music to play when the player enters the location (subject to the other fields like <see cref="F:StardewValley.GameData.Locations.LocationData.MusicContext" />).</summary>
	/// <remarks>The first matching entry is used. If none match, falls back to <see cref="F:StardewValley.GameData.Locations.LocationData.MusicDefault" />.s</remarks>
	[ContentSerializer(Optional = true)]
	public List<LocationMusicData> Music = new List<LocationMusicData>();

	/// <summary>The music to play if none of the options in <see cref="F:StardewValley.GameData.Locations.LocationData.Music" /> matched.</summary>
	/// <remarks>If this is null, falls back to the <c>Music</c> map property (if set).</remarks>
	[ContentSerializer(Optional = true)]
	public string MusicDefault;

	/// <summary>The music context for this location. The recommended values are <c>Default</c> or <c>SubLocation</c>.</summary>
	[ContentSerializer(Optional = true)]
	public MusicContext MusicContext;

	/// <summary>Whether to ignore the <c>Music</c> map property when it's raining in this location.</summary>
	[ContentSerializer(Optional = true)]
	public bool MusicIgnoredInRain;

	/// <summary>Whether to ignore the <c>Music</c> map property when it's spring in this location.</summary>
	[ContentSerializer(Optional = true)]
	public bool MusicIgnoredInSpring;

	/// <summary>Whether to ignore the <c>Music</c> map property when it's summer in this location.</summary>
	[ContentSerializer(Optional = true)]
	public bool MusicIgnoredInSummer;

	/// <summary>Whether to ignore the <c>Music</c> map property when it's fall in this location.</summary>
	[ContentSerializer(Optional = true)]
	public bool MusicIgnoredInFall;

	/// <summary>Whether to ignore the <c>Music</c> map property when it's fall and windy weather in this location.</summary>
	[ContentSerializer(Optional = true)]
	public bool MusicIgnoredInFallDebris;

	/// <summary>Whether to ignore the <c>Music</c> map property when it's winter in this location.</summary>
	[ContentSerializer(Optional = true)]
	public bool MusicIgnoredInWinter;

	/// <summary>Whether to use the same music behavior as Pelican Town's music: it will start playing after the day music has finished, and will continue playing while the player travels through indoor areas, but will stop when entering another outdoor area that isn't marked with the same <c>Music</c> map property and <c>MusicIsTownTheme</c> data field.</summary>
	[ContentSerializer(Optional = true)]
	public bool MusicIsTownTheme;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
