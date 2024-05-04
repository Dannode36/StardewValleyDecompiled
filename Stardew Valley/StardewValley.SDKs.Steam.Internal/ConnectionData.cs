using Steamworks;

namespace StardewValley.SDKs.Steam.Internal;

/// <summary>Extra bookkeeping data for a connected client.</summary>
internal sealed class ConnectionData
{
	/// <summary>The Farmer ID associated with the connected client.</summary>
	public long FarmerId = long.MinValue;

	/// <summary>The Steam ID of the connected client.</summary>
	public CSteamID SteamId;

	/// <summary>The connection used to send data to the client.</summary>
	public HSteamNetConnection Connection;

	/// <summary>Whether the client has an active farmhand.</summary>
	public bool Online;

	/// <summary>The Steam display name of the connected client.</summary>
	public string DisplayName;

	/// <summary>Construct an instance.</summary>
	/// <param name="connection">The connection used to send data to the client.</param>
	/// <param name="steamId">The Steam ID of the connected client.</param>
	/// <param name="displayName">The Steam display name of the connected client.</param>
	public ConnectionData(HSteamNetConnection connection, CSteamID steamId, string displayName)
	{
		Connection = connection;
		SteamId = steamId;
		DisplayName = displayName;
	}
}
