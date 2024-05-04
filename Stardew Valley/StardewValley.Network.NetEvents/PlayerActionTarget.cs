namespace StardewValley.Network.NetEvents;

/// <summary>The player(s) to change for a net event request.</summary>
public enum PlayerActionTarget : byte
{
	/// <summary>Apply the action to the current player.</summary>
	Current,
	/// <summary>Apply the action to the main player.</summary>
	Host,
	/// <summary>Apply the action to all players (regardless of whether they're online).</summary>
	All
}
