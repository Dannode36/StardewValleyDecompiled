using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners;

/// <summary>Listener for events related to Galaxy lobby member state changes (joining, leaving, disconnecting, etc.).</summary>
internal sealed class GalaxyLobbyMemberStateListener : ILobbyMemberStateListener
{
	/// <summary>The callback to invoke when a Galaxy lobby member changes state.</summary>
	private readonly Action<GalaxyID, GalaxyID, LobbyMemberStateChange> Callback;

	/// <summary>Constructs an instance of the listener and registers it with the Galaxy SDK.</summary>
	/// <param name="callback">The callback to invoke when a Galaxy lobby member changes state.</param>
	public GalaxyLobbyMemberStateListener(Action<GalaxyID, GalaxyID, LobbyMemberStateChange> callback)
	{
		Callback = callback;
		GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerLobbyMemberState.GetListenerType(), this);
	}

	/// <summary>Handles Galaxy lobby member state changes (joining, leaving, disconnecting, etc.) and passes the information to <see cref="F:StardewValley.SDKs.GogGalaxy.Listeners.GalaxyLobbyMemberStateListener.Callback" />.</summary>
	/// <param name="lobbyID">The Galaxy ID of the lobby.</param>
	/// <param name="memberID">The Galaxy ID of the lobby member whose state changed.</param>
	/// <param name="memberStateChange">The updated state of the lobby member.</param>
	public override void OnLobbyMemberStateChanged(GalaxyID lobbyID, GalaxyID memberID, LobbyMemberStateChange memberStateChange)
	{
		Callback?.Invoke(lobbyID, memberID, memberStateChange);
	}

	/// <summary>Unregisters the listener from the Galaxy SDK.</summary>
	public override void Dispose()
	{
		GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerLobbyMemberState.GetListenerType(), this);
		base.Dispose();
	}
}
