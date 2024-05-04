using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners;

/// <summary>Listener for leaving a Galaxy lobby.</summary>
internal sealed class GalaxyLobbyLeftListener : ILobbyLeftListener
{
	/// <summary>The callback to invoke when leaving a Galaxy lobby for any reason.</summary>
	private readonly Action<GalaxyID, LobbyLeaveReason> Callback;

	/// <summary>Constructs an instance of the listener and registers it with the Galaxy SDK.</summary>
	/// <param name="callback">The callback to invoke when leaving a Galaxy lobby for any reason.</param>
	public GalaxyLobbyLeftListener(Action<GalaxyID, LobbyLeaveReason> callback)
	{
		Callback = callback;
		GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerLobbyLeft.GetListenerType(), this);
	}

	/// <summary>Handles leaving a lobby for any reason (leaving normally, losing connection, etc.) and passes the information to <see cref="F:StardewValley.SDKs.GogGalaxy.Listeners.GalaxyLobbyLeftListener.Callback" />.</summary>
	/// <param name="lobbyID">The Galaxy ID of the lobby that was left.</param>
	/// <param name="leaveReason">The reason why we left the lobby.</param>
	public override void OnLobbyLeft(GalaxyID lobbyID, LobbyLeaveReason leaveReason)
	{
		Callback?.Invoke(lobbyID, leaveReason);
	}

	/// <summary>Unregisters the listener from the Galaxy SDK.</summary>
	public override void Dispose()
	{
		GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerLobbyLeft.GetListenerType(), this);
		base.Dispose();
	}
}
