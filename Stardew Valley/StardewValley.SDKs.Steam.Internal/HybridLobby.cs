using Galaxy.Api;
using Steamworks;

namespace StardewValley.SDKs.Steam.Internal;

/// <summary>A lobby that can accept Steam connections, Galaxy connections, or both.</summary>
internal struct HybridLobby
{
	/// <summary>Whether this is a Galaxy lobby which allows Steam connections. This is only relevant to lobbies from invite codes.</summary>
	private bool IsHybrid;

	/// <summary>The underlying Steam lobby ID.</summary>
	public ulong SteamId { get; private set; }

	/// <summary>The underlying Galaxy lobby ID.</summary>
	public ulong GalaxyId { get; private set; }

	/// <summary>The type of lobby represented by this instance.</summary>
	public LobbyConnectionType LobbyType
	{
		get
		{
			CSteamID steamID = new CSteamID(SteamId);
			if (steamID.IsValid() && steamID.IsLobby())
			{
				return LobbyConnectionType.Steam;
			}
			if (!new GalaxyID(GalaxyId).IsValid())
			{
				return LobbyConnectionType.Invalid;
			}
			if (IsHybrid)
			{
				return LobbyConnectionType.Hybrid;
			}
			return LobbyConnectionType.Galaxy;
		}
	}

	/// <summary>Constructs an instance which allows only Steam connections.</summary>
	/// <param name="steamID">The ID of the Steam lobby.</param>
	public HybridLobby(CSteamID steamID)
	{
		SteamId = steamID.m_SteamID;
		GalaxyId = 0uL;
		IsHybrid = false;
	}

	/// <summary>Constructs an instance which allows GOG Galaxy (and possibly Steam) connections.</summary>
	/// <param name="galaxyID">The ID of the Galaxy lobby.</param>
	/// <param name="isHybrid">Whether the Galaxy lobby supports Steam connections.</param>
	public HybridLobby(GalaxyID galaxyID, bool isHybrid = false)
	{
		SteamId = 0uL;
		GalaxyId = galaxyID.ToUint64();
		IsHybrid = isHybrid;
	}

	/// <summary>Invalidates the lobby and its lobby ID members.</summary>
	public void Clear()
	{
		SteamId = 0uL;
		GalaxyId = 0uL;
		IsHybrid = false;
	}
}
