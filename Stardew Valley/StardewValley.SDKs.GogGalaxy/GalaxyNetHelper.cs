using System;
using System.Collections.Generic;
using Galaxy.Api;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy.Internal;
using StardewValley.SDKs.GogGalaxy.Listeners;

namespace StardewValley.SDKs.GogGalaxy;

public class GalaxyNetHelper : SDKNetHelper
{
	public const string GalaxyConnectionStringPrefix = "-connect-lobby-";

	public const string SteamConnectionStringPrefix = "+connect_lobby";

	/// <summary>The invite code prefix for a GOG Galaxy lobby.</summary>
	public const char GalaxyInvitePrefix = 'G';

	/// <summary>The invite code prefix for a Steam lobby.</summary>
	public const char SteamInvitePrefix = 'S';

	protected GalaxyID lobbyRequested;

	private GalaxyLobbyEnteredListener lobbyEntered;

	private GalaxyGameJoinRequestedListener lobbyJoinRequested;

	private GalaxyLobbyDataListener lobbyDataListener;

	private GalaxyRichPresenceListener richPresenceListener;

	private List<LobbyUpdateListener> lobbyUpdateListeners = new List<LobbyUpdateListener>();

	public GalaxyNetHelper()
	{
		lobbyRequested = getStartupLobby();
		lobbyJoinRequested = new GalaxyGameJoinRequestedListener(onLobbyJoinRequested);
		lobbyEntered = new GalaxyLobbyEnteredListener(onLobbyEntered);
		lobbyDataListener = new GalaxyLobbyDataListener(onLobbyDataUpdated);
		richPresenceListener = new GalaxyRichPresenceListener(onRichPresenceUpdated);
		if (lobbyRequested != null)
		{
			Game1.multiplayer.inviteAccepted();
		}
	}

	/// <summary>Get the host's Steam display name from the underlying GOG Galaxy SDK if it's set.</summary>
	/// <param name="lobbyId">The GOG Galaxy lobby ID.</param>
	/// <returns>Returns the host's display name, or <c>null</c> if it's not set.</returns>
	public static string TryGetHostSteamDisplayName(GalaxyID lobbyId)
	{
		try
		{
			return GalaxyInstance.Matchmaking().GetLobbyData(lobbyId, "HostDisplayName");
		}
		catch (Exception)
		{
			return null;
		}
	}

	public virtual string GetUserID()
	{
		return Convert.ToString(GalaxyInstance.User().GetGalaxyID().ToUint64());
	}

	protected virtual Client createClient(GalaxyID lobby)
	{
		return Game1.multiplayer.InitClient(new GalaxyNetClient(lobby));
	}

	public Client CreateClient(object lobby)
	{
		return createClient(new GalaxyID((ulong)lobby));
	}

	public virtual Server CreateServer(IGameServer gameServer)
	{
		return Game1.multiplayer.InitServer(new GalaxyNetServer(gameServer));
	}

	protected GalaxyID parseConnectionString(string connectionString)
	{
		if (connectionString == null)
		{
			return null;
		}
		if (connectionString.StartsWith("-connect-lobby-"))
		{
			return new GalaxyID(Convert.ToUInt64(connectionString.Substring("-connect-lobby-".Length)));
		}
		if (connectionString.StartsWith("+connect_lobby "))
		{
			return new GalaxyID(Convert.ToUInt64(connectionString.Substring("+connect_lobby".Length + 1)));
		}
		return null;
	}

	protected virtual GalaxyID getStartupLobby()
	{
		string[] args = Environment.GetCommandLineArgs();
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i].StartsWith("-connect-lobby-"))
			{
				return parseConnectionString(args[i]);
			}
		}
		return null;
	}

	public Client GetRequestedClient()
	{
		if (lobbyRequested != null)
		{
			return createClient(lobbyRequested);
		}
		return null;
	}

	public void AddLobbyUpdateListener(LobbyUpdateListener listener)
	{
		lobbyUpdateListeners.Add(listener);
	}

	public void RemoveLobbyUpdateListener(LobbyUpdateListener listener)
	{
		lobbyUpdateListeners.Remove(listener);
	}

	public virtual void RequestFriendLobbyData()
	{
		uint count = GalaxyInstance.Friends().GetFriendCount();
		for (uint i = 0u; i < count; i++)
		{
			GalaxyID friend = GalaxyInstance.Friends().GetFriendByIndex(i);
			GalaxyInstance.Friends().RequestRichPresence(friend);
		}
	}

	private void onRichPresenceUpdated(GalaxyID userID)
	{
		GalaxyID lobby = parseConnectionString(GalaxyInstance.Friends().GetRichPresence("connect", userID));
		if (lobby != null)
		{
			GalaxyInstance.Matchmaking().RequestLobbyData(lobby);
		}
	}

	private void onLobbyDataUpdated(GalaxyID lobbyID, GalaxyID memberID)
	{
		foreach (LobbyUpdateListener lobbyUpdateListener in lobbyUpdateListeners)
		{
			lobbyUpdateListener.OnLobbyUpdate(lobbyID.ToUint64());
		}
	}

	public virtual string GetLobbyData(object lobby, string key)
	{
		return GalaxyInstance.Matchmaking().GetLobbyData(new GalaxyID((ulong)lobby), key);
	}

	public virtual string GetLobbyOwnerName(object lobbyId)
	{
		GalaxyID lobby = new GalaxyID((ulong)lobbyId);
		GalaxyID owner = GalaxyInstance.Matchmaking().GetLobbyOwner(lobby);
		return GalaxyInstance.Friends().GetFriendPersonaName(owner);
	}

	protected virtual void onLobbyEntered(GalaxyID lobby_id, LobbyEnterResult result)
	{
	}

	private void onLobbyJoinRequested(GalaxyID userID, string connectionString)
	{
		lobbyRequested = parseConnectionString(connectionString);
		if (lobbyRequested != null)
		{
			Game1.multiplayer.inviteAccepted();
		}
	}

	public bool SupportsInviteCodes()
	{
		return true;
	}

	/// <summary>Gets a GOG Galaxy user ID from an invite code.</summary>
	/// <param name="inviteCode">The invite code string to parse.</param>
	/// <returns>Returns a valid GOG Galaxy user ID for the lobby corresponding to <paramref name="inviteCode" />, or <c>null</c> if none was found.</returns>
	public static GalaxyID GetLobbyFromGalaxyInvite(string inviteCode)
	{
		if (inviteCode.Length <= 1)
		{
			return null;
		}
		char c = inviteCode[0];
		if (c != 'G' && c != 'S')
		{
			return null;
		}
		ulong decoded;
		try
		{
			decoded = Base36.Decode(inviteCode.Substring(1));
		}
		catch (FormatException)
		{
			return null;
		}
		if (decoded == 0L || decoded >> 56 != 0L)
		{
			return null;
		}
		return GalaxyID.FromRealID(GalaxyID.IDType.ID_TYPE_LOBBY, decoded);
	}

	public object GetLobbyFromInviteCode(string inviteCode)
	{
		GalaxyID lobbyID = GetLobbyFromGalaxyInvite(inviteCode);
		if (lobbyID == null)
		{
			return null;
		}
		return lobbyID.ToUint64();
	}

	public virtual void ShowInviteDialog(object lobby)
	{
		GalaxyInstance.Friends().ShowOverlayInviteDialog("-connect-lobby-" + Convert.ToString((ulong)lobby));
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
