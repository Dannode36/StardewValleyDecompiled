namespace StardewValley.GameData.HomeRenovations;

/// <summary>As part of <see cref="T:StardewValley.GameData.HomeRenovations.HomeRenovation" />, a renovation requirement or action.</summary>
public class RenovationValue
{
	/// <summary>The requirement or action type. This can be <c>Mail</c> (check/change a mail flag for the current player) or <c>Value</c> (check/set a C# field on the farmhouse instance).</summary>
	public string Type;

	/// <summary>The mail flag (if <see cref="F:StardewValley.GameData.HomeRenovations.RenovationValue.Type" /> is <c>Mail</c>) or field name (if <see cref="F:StardewValley.GameData.HomeRenovations.RenovationValue.Type" /> is <c>Value</c>) to check or set.</summary>
	public string Key;

	/// <summary>
	/// The effect of this field depends on whether this is used in <see cref="F:StardewValley.GameData.HomeRenovations.HomeRenovation.Requirements" /> or <see cref="F:StardewValley.GameData.HomeRenovations.HomeRenovation.RenovateActions" />, and the value of <see cref="F:StardewValley.GameData.HomeRenovations.RenovationValue.Type" />:
	///
	/// <list type="bullet">
	///   <item><description>
	///     For a renovation requirement:
	///     <list type="bullet">
	///       <item><description>If the <see cref="F:StardewValley.GameData.HomeRenovations.RenovationValue.Type" /> is <c>Mail</c>, either <c>"0"</c> (player must not have the flag) or <c>"1"</c> (player must have it).</description></item>
	///       <item><description>If the <see cref="F:StardewValley.GameData.HomeRenovations.RenovationValue.Type" /> is <c>Value</c>, the required field value. This can be prefixed with <c>!</c> to require any value <em>except</em> this one.</description></item>
	///     </list>
	///   </description></item>
	///   <item><description>
	///     For a renovate action:
	///     <list type="bullet">
	///       <item><description>If the <see cref="F:StardewValley.GameData.HomeRenovations.RenovationValue.Type" /> is <c>Mail</c>, either <c>"0"</c> (remove the mail flag) or <c>"1"</c> (add it).</description></item>
	///       <item><description>If the <see cref="F:StardewValley.GameData.HomeRenovations.RenovationValue.Type" /> is <c>Value</c>, either the integer value to set, or the exact string <c>"selected"</c> to set it to the index of the applied renovation.</description></item>
	///     </list>
	///   </description></item>
	/// </list>
	/// </summary>
	public string Value;
}
