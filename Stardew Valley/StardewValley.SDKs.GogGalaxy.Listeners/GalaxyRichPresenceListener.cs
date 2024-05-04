using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners;

/// <summary>Listener for any change in a Galaxy user's rich presence.</summary>
internal sealed class GalaxyRichPresenceListener : IRichPresenceListener
{
	/// <summary>The callback to invoke when the rich presence for a Galaxy user changes.</summary>
	private readonly Action<GalaxyID> Callback;

	/// <summary>Constructs an instance of the listener and registers it with the Galaxy SDK.</summary>
	/// <param name="callback">The callback to invoke when the rich presence for a Galaxy user changes.</param>
	public GalaxyRichPresenceListener(Action<GalaxyID> callback)
	{
		Callback = callback;
		GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerRichPresence.GetListenerType(), this);
	}

	/// <summary>Handles changes to a Galaxy user's rich presence, and passes the information to <see cref="F:StardewValley.SDKs.GogGalaxy.Listeners.GalaxyRichPresenceListener.Callback" />.</summary>
	/// <param name="userID">The Galaxy ID of the user whose rich presence was updated.</param>
	public override void OnRichPresenceUpdated(GalaxyID userID)
	{
		Callback?.Invoke(userID);
	}

	/// <summary>Unregisters the listener from the Galaxy SDK.</summary>
	public override void Dispose()
	{
		GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerRichPresence.GetListenerType(), this);
		base.Dispose();
	}
}
