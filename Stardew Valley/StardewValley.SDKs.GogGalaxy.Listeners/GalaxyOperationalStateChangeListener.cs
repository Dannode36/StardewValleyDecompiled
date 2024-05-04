using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners;

/// <summary>Listener for changes in Galaxy's operational state (e.g. signing in and logging on).</summary>
internal sealed class GalaxyOperationalStateChangeListener : IOperationalStateChangeListener
{
	/// <summary>The callback to invoke when Galaxy's operational state changes.</summary>
	private readonly Action<uint> Callback;

	/// <summary>Constructs an instance of the listener and registers it with the Galaxy SDK.</summary>
	/// <param name="callback">The callback to invoke when Galaxy's operational state changes.</param>
	public GalaxyOperationalStateChangeListener(Action<uint> callback)
	{
		Callback = callback;
		GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerOperationalStateChange.GetListenerType(), this);
	}

	/// <summary>Handles operational state changes, and passes the information to <see cref="F:StardewValley.SDKs.GogGalaxy.Listeners.GalaxyOperationalStateChangeListener.Callback" />.</summary>
	/// <param name="operationalState">A bit-field representing the operational state change.</param>
	public override void OnOperationalStateChanged(uint operationalState)
	{
		Callback?.Invoke(operationalState);
	}

	/// <summary>Unregisters the listener from the Galaxy SDK.</summary>
	public override void Dispose()
	{
		GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerOperationalStateChange.GetListenerType(), this);
		base.Dispose();
	}
}
