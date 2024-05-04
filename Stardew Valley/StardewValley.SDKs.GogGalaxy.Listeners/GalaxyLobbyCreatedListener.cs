using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners;

/// <summary>Listener for Galaxy lobby creation.</summary>
internal sealed class GalaxyLobbyCreatedListener : ILobbyCreatedListener
{
	/// <summary>The callback to invoke when creating a Galaxy lobby succeeds or fails.</summary>
	private readonly Action<GalaxyID, LobbyCreateResult> Callback;

	/// <summary>Constructs an instance of the listener and registers it with the Galaxy SDK.</summary>
	/// <param name="callback">The callback to invoke when creating a Galaxy lobby succeeds or fails.</param>
	public GalaxyLobbyCreatedListener(Action<GalaxyID, LobbyCreateResult> callback)
	{
		Callback = callback;
		GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerLobbyCreated.GetListenerType(), this);
	}

	/// <summary>Handles success/failure for Galaxy lobby creation, and passes the information to <see cref="F:StardewValley.SDKs.GogGalaxy.Listeners.GalaxyLobbyCreatedListener.Callback" />.</summary>
	/// <param name="lobbyID">The Galaxy ID of the lobby being created.</param>
	/// <param name="result">An enum representing whether the lobby creation succeeded or failed.</param>
	public override void OnLobbyCreated(GalaxyID lobbyID, LobbyCreateResult result)
	{
		Callback?.Invoke(lobbyID, result);
	}

	/// <summary>Unregisters the listener from the Galaxy SDK.</summary>
	public override void Dispose()
	{
		GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerLobbyCreated.GetListenerType(), this);
		base.Dispose();
	}
}
