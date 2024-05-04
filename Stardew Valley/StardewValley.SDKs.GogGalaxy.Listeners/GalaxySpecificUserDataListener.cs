using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners;

/// <summary>Listener for events related to Galaxy user data changes for any user.</summary>
internal sealed class GalaxySpecificUserDataListener : ISpecificUserDataListener
{
	/// <summary>The callback to invoke when the user data changes for a Galaxy user.</summary>
	private readonly Action<GalaxyID> Callback;

	/// <summary>Constructs an instance of the listener and registers it with the Galaxy SDK.</summary>
	/// <param name="callback">The callback to invoke when the user data changes for a Galaxy user.</param>
	public GalaxySpecificUserDataListener(Action<GalaxyID> callback)
	{
		Callback = callback;
		GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerSpecificUserData.GetListenerType(), this);
	}

	/// <summary>Handles Galaxy user data changes, and passes the information to <see cref="F:StardewValley.SDKs.GogGalaxy.Listeners.GalaxySpecificUserDataListener.Callback" />.</summary>
	/// <param name="userID">The Galaxy ID of the user whose data was updated.</param>
	public override void OnSpecificUserDataUpdated(GalaxyID userID)
	{
		Callback?.Invoke(userID);
	}

	/// <summary>Unregisters the listener from the Galaxy SDK.</summary>
	public override void Dispose()
	{
		GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerSpecificUserData.GetListenerType(), this);
		base.Dispose();
	}
}
