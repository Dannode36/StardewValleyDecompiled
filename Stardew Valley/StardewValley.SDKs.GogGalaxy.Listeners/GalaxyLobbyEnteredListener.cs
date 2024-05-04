using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners;

/// <summary>Listener for entering a Galaxy lobby.</summary>
internal sealed class GalaxyLobbyEnteredListener : ILobbyEnteredListener
{
	/// <summary>The callback to invoke when entering a Galaxy lobby succeeds or fails.</summary>
	private readonly Action<GalaxyID, LobbyEnterResult> Callback;

	/// <summary>Constructs an instance of the listener and registers it with the Galaxy SDK.</summary>
	/// <param name="callback">The callback to invoke when entering a Galaxy lobby succeeds or fails.</param>
	public GalaxyLobbyEnteredListener(Action<GalaxyID, LobbyEnterResult> callback)
	{
		Callback = callback;
		GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerLobbyEntered.GetListenerType(), this);
	}

	/// <summary>Handles success/failure for entering a Galaxy lobby, and passes the information to <see cref="F:StardewValley.SDKs.GogGalaxy.Listeners.GalaxyLobbyEnteredListener.Callback" />.</summary>
	/// <param name="lobbyID">The Galaxy ID of the lobby that was entered.</param>
	/// <param name="result">An enum representing whether or not we successfully entered the lobby.</param>
	public override void OnLobbyEntered(GalaxyID lobbyID, LobbyEnterResult result)
	{
		Callback?.Invoke(lobbyID, result);
	}

	/// <summary>Unregisters the listener from the Galaxy SDK.</summary>
	public override void Dispose()
	{
		GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerLobbyEntered.GetListenerType(), this);
		base.Dispose();
	}
}
