using System;
using System.Collections.Generic;
using Galaxy.Api;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy;
using StardewValley.SDKs.Steam.Internal;
using Steamworks;

namespace StardewValley.SDKs.Steam;

internal sealed class SteamNetHelper : SDKNetHelper
{
	/// <summary>List of active listeners to call when we receive lobby update events.</summary>
	private List<LobbyUpdateListener> LobbyUpdateListeners;

	/// <summary>The callback used to receive lobby data updates and pass them to <see cref="F:StardewValley.SDKs.Steam.SteamNetHelper.LobbyUpdateListeners" />.</summary>
	private readonly Callback<LobbyDataUpdate_t> LobbyDataUpdateCallback;

	/// <summary>The callback used to handle requests to join lobbies, either through Steam overlay or by invite.</summary>
	private readonly Callback<GameLobbyJoinRequested_t> GameLobbyJoinRequestedCallback;

	/// <summary>The lobby the player requested to join, either through Steam overlay or by invite.</summary>
	private HybridLobby RequestedLobby;

	/// <summary>Constructs an instance and registers its Steam SDK callbacks.</summary>
	public SteamNetHelper()
	{
		LobbyUpdateListeners = new List<LobbyUpdateListener>();
		GameLobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
		LobbyDataUpdateCallback = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
		RequestedLobby.Clear();
		FindLaunchLobby();
	}

	/// <summary>Cleans up the instance and unregisters its Steam SDK callbacks.</summary>
	~SteamNetHelper()
	{
		GameLobbyJoinRequestedCallback.Unregister();
		LobbyDataUpdateCallback.Unregister();
	}

	/// <summary>Handles a request to join a Steam lobby.</summary>
	/// <param name="evt">A structure containing information about the lobby join request.</param>
	private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t evt)
	{
		RequestJoinLobby(evt.m_steamIDLobby);
	}

	/// <summary>Handles changes in lobby data (likely in response to <see cref="M:StardewValley.SDKs.Steam.SteamNetHelper.RequestFriendLobbyData" />) and invokes listeners from <see cref="F:StardewValley.SDKs.Steam.SteamNetHelper.LobbyUpdateListeners" />.</summary>
	/// <param name="evt">A structure containing information about the lobby whose data changed.</param>
	private void OnLobbyDataUpdate(LobbyDataUpdate_t evt)
	{
		CSteamID steamLobby = new CSteamID(evt.m_ulSteamIDLobby);
		if (SteamMatchmaking.GetLobbyOwner(steamLobby) == SteamUser.GetSteamID())
		{
			return;
		}
		HybridLobby lobby = new HybridLobby(steamLobby);
		foreach (LobbyUpdateListener lobbyUpdateListener in LobbyUpdateListeners)
		{
			lobbyUpdateListener.OnLobbyUpdate(lobby);
		}
	}

	/// <summary>Reads the command line arguments to find the launch option "+connect_lobby &lt;lobbyID&gt;".</summary>
	private void FindLaunchLobby()
	{
		CSteamID launchLobby = default(CSteamID);
		string[] args = Environment.GetCommandLineArgs();
		for (int argIdx = 0; argIdx < args.Length - 1; argIdx++)
		{
			if (!(args[argIdx] != "+connect_lobby"))
			{
				launchLobby.Clear();
				try
				{
					launchLobby = new CSteamID(Convert.ToUInt64(args[argIdx + 1]));
					Game1.log.Verbose($"Found startup Steam lobby {launchLobby.m_SteamID}");
					RequestJoinLobby(launchLobby);
					break;
				}
				catch (Exception)
				{
					Game1.log.Verbose("Could not parse argument for +connect_lobby: " + args[argIdx + 1]);
				}
			}
		}
	}

	/// <summary>Queues a request to a lobby if it's a valid Steam lobby ID.</summary>
	/// <param name="requestedLobby">The lobby we are requesting to join.</param>
	private void RequestJoinLobby(CSteamID requestedLobby)
	{
		if (requestedLobby.IsValid() && requestedLobby.IsLobby())
		{
			Game1.log.Verbose($"Requesting to join Steam lobby {requestedLobby.m_SteamID}");
			RequestedLobby = new HybridLobby(requestedLobby);
			Game1.multiplayer.inviteAccepted();
		}
		else
		{
			Game1.log.Verbose($"Denied request to join invalid Steam lobby {requestedLobby.m_SteamID}");
		}
	}

	public string GetUserID()
	{
		try
		{
			return GalaxyInstance.User().GetGalaxyID().ToUint64()
				.ToString();
		}
		catch (Exception)
		{
			return "";
		}
	}

	/// <summary>Creates a client corresponding to the type of <paramref name="lobby" />.</summary>
	/// <param name="lobby">The lobby that we will be joining with the resulting client.</param>
	/// <returns>Returns a client that will join <paramref name="lobby" />.</returns>
	private Client CreateClientFromHybrid(HybridLobby lobby)
	{
		return lobby.LobbyType switch
		{
			LobbyConnectionType.Steam => new SteamNetClient(new CSteamID(lobby.SteamId)), 
			LobbyConnectionType.Galaxy => new GalaxyNetClient(new GalaxyID(lobby.GalaxyId)), 
			LobbyConnectionType.Hybrid => new SteamNetClient(new GalaxyID(lobby.GalaxyId)), 
			_ => null, 
		};
	}

	/// <summary>Creates a client with <see cref="M:StardewValley.SDKs.Steam.SteamNetHelper.CreateClientFromHybrid(StardewValley.SDKs.Steam.Internal.HybridLobby)" /> and initializes it with <see cref="M:StardewValley.Multiplayer.InitClient(StardewValley.Network.Client)" />.</summary>
	/// <param name="lobby">The lobby that we will be joining with the resulting client.</param>
	/// <returns>Returns an initialized client that will join <paramref name="lobby" />.</returns>
	private Client CreateClientHelper(HybridLobby lobby)
	{
		Client client = CreateClientFromHybrid(lobby);
		if (client == null)
		{
			return null;
		}
		return Game1.multiplayer.InitClient(client);
	}

	public Client CreateClient(object lobby)
	{
		if (!(lobby is HybridLobby hybridLobby))
		{
			return null;
		}
		return CreateClientHelper(hybridLobby);
	}

	public Client GetRequestedClient()
	{
		Client result = CreateClientHelper(RequestedLobby);
		RequestedLobby.Clear();
		return result;
	}

	/// <summary>Creates an additional Steam server with an underlying <paramref name="gameServer" />.</summary>
	/// <param name="gameServer">The master game server that manages all <see cref="T:StardewValley.Network.Server" /> objects.</param>
	/// <returns>Returns an initialized instance of <see cref="T:StardewValley.SDKs.Steam.SteamNetServer" />.</returns>
	public Server CreateSteamServer(IGameServer gameServer)
	{
		return Game1.multiplayer.InitServer(new SteamNetServer(gameServer));
	}

	public Server CreateServer(IGameServer gameServer)
	{
		if (Program.sdk is SteamHelper { GalaxyConnected: false })
		{
			Game1.log.Error("Could not create a Galaxy server: not logged on");
			return null;
		}
		return Game1.multiplayer.InitServer(new GalaxyNetServer(gameServer));
	}

	public void AddLobbyUpdateListener(LobbyUpdateListener listener)
	{
		LobbyUpdateListeners.Add(listener);
	}

	public void RemoveLobbyUpdateListener(LobbyUpdateListener listener)
	{
		LobbyUpdateListeners.Remove(listener);
	}

	public void RequestFriendLobbyData()
	{
		int count = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagImmediate);
		for (int i = 0; i < count; i++)
		{
			CSteamID friendId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
			if (!(friendId == SteamUser.GetSteamID()))
			{
				SteamFriends.GetFriendGamePlayed(friendId, out var gameInfo);
				if (!(gameInfo.m_gameID.AppID() != SteamUtils.GetAppID()))
				{
					SteamMatchmaking.RequestLobbyData(gameInfo.m_steamIDLobby);
				}
			}
		}
	}

	public string GetLobbyData(object lobby, string key)
	{
		if (!(lobby is HybridLobby { LobbyType: var lobbyType } hybridLobby))
		{
			return "";
		}
		switch (lobbyType)
		{
		case LobbyConnectionType.Steam:
			return SteamMatchmaking.GetLobbyData(new CSteamID(hybridLobby.SteamId), key);
		case LobbyConnectionType.Galaxy:
		case LobbyConnectionType.Hybrid:
			try
			{
				return GalaxyInstance.Matchmaking().GetLobbyData(new GalaxyID(hybridLobby.GalaxyId), key);
			}
			catch (Exception)
			{
				return "";
			}
		default:
			return "";
		}
	}

	public string GetLobbyOwnerName(object lobby)
	{
		if (!(lobby is HybridLobby hybridLobby))
		{
			return null;
		}
		switch (hybridLobby.LobbyType)
		{
		case LobbyConnectionType.Steam:
			return SteamFriends.GetFriendPersonaName(SteamMatchmaking.GetLobbyOwner(new CSteamID(hybridLobby.SteamId)));
		case LobbyConnectionType.Hybrid:
			return GalaxyNetHelper.TryGetHostSteamDisplayName(new GalaxyID(hybridLobby.GalaxyId)) ?? "";
		case LobbyConnectionType.Galaxy:
			try
			{
				GalaxyID galaxyOwner = GalaxyInstance.Matchmaking().GetLobbyOwner(new GalaxyID(hybridLobby.GalaxyId));
				return GalaxyInstance.Friends().GetFriendPersonaName(galaxyOwner);
			}
			catch (Exception)
			{
				return "";
			}
		default:
			return "";
		}
	}

	public bool SupportsInviteCodes()
	{
		return true;
	}

	public object GetLobbyFromInviteCode(string inviteCode)
	{
		GalaxyID galaxyLobby = GalaxyNetHelper.GetLobbyFromGalaxyInvite(inviteCode);
		if (!(galaxyLobby != null))
		{
			return null;
		}
		return new HybridLobby(galaxyLobby, inviteCode[0] == 'S');
	}

	public void ShowInviteDialog(object lobby)
	{
		if (lobby is CSteamID steamLobby)
		{
			SteamFriends.ActivateGameOverlayInviteDialog(steamLobby);
		}
	}

	public void MutePlayer(string userId, bool mute)
	{
	}

	public bool IsPlayerMuted(string userId)
	{
		return false;
	}

	public void ShowProfile(string userId)
	{
	}
}
