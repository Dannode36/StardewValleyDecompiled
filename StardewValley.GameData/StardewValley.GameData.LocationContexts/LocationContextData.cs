using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using StardewValley.GameData.Locations;

namespace StardewValley.GameData.LocationContexts;

/// <summary>A world area which groups multiple in-game locations with shared settings and metadata.</summary>
public class LocationContextData
{
	/// <summary>The season which is always active for locations within this context. For example, setting <see cref="F:StardewValley.Season.Summer" /> will make it always summer there regardless of the calendar season. If not set, the calendar season applies.</summary>
	[ContentSerializer(Optional = true)]
	public Season? SeasonOverride;

	/// <summary>The cue ID for the music to play when the player is in the location, unless overridden by a <c>Music</c> map property. Despite the name, this has a higher priority than the seasonal music fields like <see cref="!:SpringMusic" />. Ignored if omitted.</summary>
	[ContentSerializer(Optional = true)]
	public string DefaultMusic;

	/// <summary>A game state query which returns whether the <see cref="F:StardewValley.GameData.LocationContexts.LocationContextData.DefaultMusic" /> field should be applied (if more specific music isn't playing). Defaults to always true.</summary>
	[ContentSerializer(Optional = true)]
	public string DefaultMusicCondition;

	/// <summary>When the player warps and the music changes, whether to silence the music and play the ambience (if any) until the next warp. This is similar to the default valley locations.</summary>
	[ContentSerializer(Optional = true)]
	public bool DefaultMusicDelayOneScreen = true;

	/// <summary>A list of cue IDs to play before noon unless it's raining, there's a <c>Music</c> map property, or the context has a <see cref="F:StardewValley.GameData.LocationContexts.LocationContextData.DefaultMusic" /> value. If multiple values are specified, the game will play one per day in sequence.</summary>
	[ContentSerializer(Optional = true)]
	public List<LocationMusicData> Music = new List<LocationMusicData>();

	/// <summary>The cue ID for the background ambience to before dark, when there's no music active. Defaults to none.</summary>
	[ContentSerializer(Optional = true)]
	public string DayAmbience;

	/// <summary>The cue ID for the background ambience to after dark, when there's no music active. Defaults to none.</summary>
	[ContentSerializer(Optional = true)]
	public string NightAmbience;

	/// <summary>Whether to play random ambience sounds when outdoors depending on factors like the season and time of day (e.g. birds and crickets). This is unrelated to the <see cref="F:StardewValley.GameData.LocationContexts.LocationContextData.DayAmbience" /> and <see cref="F:StardewValley.GameData.LocationContexts.LocationContextData.NightAmbience" /> fields.</summary>
	[ContentSerializer(Optional = true)]
	public bool PlayRandomAmbientSounds = true;

	/// <summary>Whether a rain totem can be used to force rain in this context tomorrow.</summary>
	[ContentSerializer(Optional = true)]
	public bool AllowRainTotem = true;

	/// <summary>If set, using a rain totem within the context changes the weather in the given context instead.</summary>
	/// <remarks>This is ignored if <see cref="F:StardewValley.GameData.LocationContexts.LocationContextData.AllowRainTotem" /> is false.</remarks>
	[ContentSerializer(Optional = true)]
	public string RainTotemAffectsContext;

	/// <summary>The weather rules to apply for locations in this context (ignored if <see cref="F:StardewValley.GameData.LocationContexts.LocationContextData.CopyWeatherFromLocation" /> is set). Defaults to always sunny. If multiple are specified, the first matching weather is applied.</summary>
	[ContentSerializer(Optional = true)]
	public List<WeatherCondition> WeatherConditions = new List<WeatherCondition>();

	/// <summary>The ID of the location context from which to inherit weather, if any. If this is set, the <see cref="F:StardewValley.GameData.LocationContexts.LocationContextData.WeatherConditions" /> field is ignored.</summary>
	[ContentSerializer(Optional = true)]
	public string CopyWeatherFromLocation;

	/// <summary>
	///   <para>When the player gets knocked out in combat, the locations where they can wake up. If multiple locations match, the first match will be used. If none match, the player will wake up at Harvey's clinic.</para>
	///
	///   <para>If the selected location has a standard event with the exact key <c>PlayerKilled</c>, that event will play when the player wakes up and the game will apply the lost items or gold logic. The game won't track this event, so it'll repeat each time the player is revived. If there's no such event, the player will wake up without an event, and no items or gold will be lost.</para>
	/// </summary>
	[ContentSerializer(Optional = true)]
	public List<ReviveLocation> ReviveLocations;

	/// <summary>When the player passes out (due to exhaustion or at 2am) in this context, the maximum amount of gold lost. If set to <c>-1</c>, uses the same value as the default context.</summary>
	[ContentSerializer(Optional = true)]
	public int MaxPassOutCost = -1;

	/// <summary>When the player passes out (due to exhaustion or at 2am) in this context, the possible letters to add to their mailbox (if they haven't received it before).</summary>
	/// <remarks>If multiple letters are valid, one will be chosen randomly (unless one of them specifies <see cref="F:StardewValley.GameData.LocationContexts.PassOutMailData.SkipRandomSelection" />).</remarks>
	[ContentSerializer(Optional = true)]
	public List<PassOutMailData> PassOutMail;

	/// <summary>When the player passes out (due to exhaustion or at 2am), the locations where they can wake up.</summary>
	/// <remarks>
	///   <para>If multiple locations match, the first match will be used. If none match, the player will wake up in their bed at home.</para>
	///
	///   <para>The selected location must either have a bed or the <c>AllowWakeUpWithoutBed: true</c> map property, otherwise the player will be warped home instead.</para>
	/// </remarks>
	[ContentSerializer(Optional = true)]
	public List<ReviveLocation> PassOutLocations;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
