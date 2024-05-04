using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.HomeRenovations;

/// <summary>A renovation which can be applied to customize the player's farmhouse after the second farmhouse upgrade.</summary>
public class HomeRenovation
{
	/// <summary>A translation key in the form <c>{asset name}:{key}</c>. The translation text should contain three slash-delimited fields: the translated display name, translated description, and the action message shown to ask the player which area to renovate.</summary>
	public string TextStrings;

	/// <summary>The animation to play when the renovation is applied. The possible values are <c>destroy</c> or <c>build</c>. Any other value defaults to <c>build</c>.</summary>
	public string AnimationType;

	/// <summary>Whether to prevent the player from applying the renovations if there are any players, NPCs, items, etc within the target area.</summary>
	public bool CheckForObstructions;

	/// <summary>A price to charge for this renovation (default free). Negative values will act as a refund the player (typically used when reverting a renovation).</summary>
	[ContentSerializer(Optional = true)]
	public int Price;

	/// <summary>A unique string ID which links this renovation to its counterpart add/remove renovation. Add/remove renovations for the same room should have the same ID.</summary>
	[ContentSerializer(Optional = true)]
	public string RoomId;

	/// <summary>The criteria that must match for the renovation to appear as an option.</summary>
	public List<RenovationValue> Requirements;

	/// <summary>The actions to perform after the renovation is applied.</summary>
	public List<RenovationValue> RenovateActions;

	/// <summary>The tile areas within the farmhouse where the renovation can be placed.</summary>
	[ContentSerializer(Optional = true)]
	public List<RectGroup> RectGroups;

	/// <summary>A dynamic area to add to the <see cref="F:StardewValley.GameData.HomeRenovations.HomeRenovation.RectGroups" /> field, if any. The only supported value is <c>crib</c>, which is the farmhouse area containing the cribs.</summary>
	[ContentSerializer(Optional = true)]
	public string SpecialRect;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
