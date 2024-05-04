namespace StardewValley.SDKs.Steam.Internal;

/// <summary>A connection type supported by a lobby.</summary>
internal enum LobbyConnectionType
{
	/// <summary>A lobby which only allows Steam connections.</summary>
	Steam,
	/// <summary>A lobby which only allows GOG Galaxy connections.</summary>
	Galaxy,
	/// <summary>A lobby which allows both GOG Galaxy and Steam connections.</summary>
	Hybrid,
	/// <summary>An invalid or cleared lobby.</summary>
	Invalid
}
