namespace StardewValley.GameData.Characters;

/// <summary>How an NPC is shown on the social tab when unlocked.</summary>
public enum SocialTabBehavior
{
	/// <summary>Until the player meets them, their name on the social tab is replaced with "???".</summary>
	UnknownUntilMet,
	/// <summary>They always appear on the social tab (including their name).</summary>
	AlwaysShown,
	/// <summary>Until the player meets them, they don't appear on the social tab.</summary>
	HiddenUntilMet,
	/// <summary>They never appear on the social tab.</summary>
	HiddenAlways
}
