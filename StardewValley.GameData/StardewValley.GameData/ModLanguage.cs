using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>The metadata for a custom language which can be selected by players.</summary>
public class ModLanguage
{
	/// <summary>A key which uniquely identifies this language. The ID should only contain alphanumeric/underscore/dot characters. This should be prefixed with your mod ID like <c>Example.ModId_Language.</c></summary>
	public string Id;

	/// <summary>The language code for this localization. This should ideally be an ISO 639-1 code, with only letters and hyphens.</summary>
	public string LanguageCode;

	/// <summary>The asset name for a 174x78 pixel texture containing the button of the language for language selection menu. The top half of the sprite is the default state, while the bottom half is the hover state.</summary>
	public string ButtonTexture;

	/// <summary>Whether the language uses the game's default fonts. Set to false to enable a custom font via <see cref="F:StardewValley.GameData.ModLanguage.FontFile" /> and <see cref="F:StardewValley.GameData.ModLanguage.FontPixelZoom" />.</summary>
	public bool UseLatinFont;

	/// <summary>If <see cref="F:StardewValley.GameData.ModLanguage.UseLatinFont" /> is false, the asset name for the custom BitMap font.</summary>
	[ContentSerializer(Optional = true)]
	public string FontFile;

	/// <summary>If <see cref="F:StardewValley.GameData.ModLanguage.UseLatinFont" /> is false, a factor by which to multiply the font size. The recommended baseline is 1.5, but you can adjust it to make your text smaller or bigger in-game.</summary>
	[ContentSerializer(Optional = true)]
	public float FontPixelZoom;

	/// <summary>Whether to shift the font up by four pixels (multiplied by the <see cref="F:StardewValley.GameData.ModLanguage.FontPixelZoom" /> if applicable), to better align languages with larger characters like Chinese and Japanese.</summary>
	[ContentSerializer(Optional = true)]
	public bool FontApplyYOffset;

	/// <summary>The line spacing value used by the game's <c>smallFont</c> font.</summary>
	[ContentSerializer(Optional = true)]
	public int SmallFontLineSpacing = 26;

	/// <summary>Whether the social tab and gift log will use gender-specific translations (like the vanilla Portuguese language).</summary>
	[ContentSerializer(Optional = true)]
	public bool UseGenderedCharacterTranslations;

	/// <summary>The string to use as the thousands separator (like <c>","</c> for <c>5,000,000</c>).</summary>
	[ContentSerializer(Optional = true)]
	public string NumberComma = ",";

	/// <summary>A string which describes the in-game time format, with tokens replaced by in-game values. For example, <c>[HOURS_12]:[MINUTES] [AM_PM]</c> would show "12:00 PM" at noon.</summary>
	/// <remarks>
	///   The valid tokens are:
	///
	///   <list type="bullet">
	///     <item><description><c>[HOURS_12]</c>: hours in 12-hour format, where midnight and noon are both "12".</description></item>
	///     <item><description><c>[HOURS_12_0]</c>: hours in 12-hour format, where midnight and noon are both "0".</description></item>
	///     <item><description><c>[HOURS_24]</c>: hours in 24-hour format, where midnight is "0" and noon is "12".</description></item>
	///     <item><description><c>[HOURS_24_0]</c>: hours in 24-hour format with zero-padding, where midnight is "00" and noon is "12".</description></item>
	///     <item><description><c>[MINUTES]</c>: minutes with zero-padding.</description></item>
	///     <item><description><c>[AM_PM]</c>: the localized text for "am" or "pm". The game shows "pm" between noon and 11:59pm inclusively, else "am".</description></item>
	///   </list>
	/// </remarks>
	public string TimeFormat;

	/// <summary>A string which describes the in-game time format. Equivalent to <see cref="F:StardewValley.GameData.ModLanguage.TimeFormat" />, but used for the in-game clock.</summary>
	public string ClockTimeFormat;

	/// <summary>A string which describes the in-game date format as shown in the in-game clock, with tokens replaced by in-game values. For example, <c>[DAY_OF_WEEK] [DAY_OF_MONTH]</c> would show <c>Mon 1</c>. </summary>
	/// <remarks>
	///   The valid tokens are:
	///
	///   <list type="bullet">
	///     <item><description><c>[DAY_OF_WEEK]</c>: the translated, abbreviated day of week.</description></item>
	///     <item><description><c>[DAY_OF_MONTH]</c>: the numerical day of the month.</description></item>
	///   </list>
	/// </remarks>
	public string ClockDateFormat;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
