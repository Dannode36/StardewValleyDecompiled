using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners;

/// <summary>Listener for when a user requests to join a game on Galaxy, either by accepting an invitation or by joining a friend.</summary>
internal sealed class GalaxyGameJoinRequestedListener : IGameJoinRequestedListener
{
	/// <summary>The callback to invoke when a Galaxy user requests to join a game.</summary>
	private readonly Action<GalaxyID, string> Callback;

	/// <summary>Constructs an instance of the listener and registers it with the Galaxy SDK.</summary>
	/// <param name="callback">The callback to invoke when a Galaxy user requests to join a game.</param>
	public GalaxyGameJoinRequestedListener(Action<GalaxyID, string> callback)
	{
		Callback = callback;
		GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerGameJoinRequested.GetListenerType(), this);
	}

	/// <summary>Handles user requests to join games, and passes the information to <see cref="F:StardewValley.SDKs.GogGalaxy.Listeners.GalaxyGameJoinRequestedListener.Callback" />.</summary>
	/// <param name="lobbyID">The Galaxy ID of the lobby.</param>
	/// <param name="result">The Galaxy connection string.</param>
	public override void OnGameJoinRequested(GalaxyID lobbyID, string result)
	{
		Callback?.Invoke(lobbyID, result);
	}

	/// <summary>Unregisters the listener from the Galaxy SDK.</summary>
	public override void Dispose()
	{
		GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerGameJoinRequested.GetListenerType(), this);
		base.Dispose();
	}
}
