using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners;

/// <summary>Listener for events related to Galaxy user authentication.</summary>
internal sealed class GalaxyAuthListener : IAuthListener
{
	/// <summary>The callback to invoke when Galaxy user authentication succeeds.</summary>
	private readonly Action OnSuccess;

	/// <summary>The callback to invoke when Galaxy user authentication fails.</summary>
	private readonly Action<FailureReason> OnFailure;

	/// <summary>The callback to invoke when Galaxy loses user authentication.</summary>
	private readonly Action OnLost;

	/// <summary>Constructs an instance of the listener and registers it with the Galaxy SDK.</summary>
	/// <param name="success">The callback to invoke when Galaxy user authentication succeeds.</param>
	/// <param name="failure">The callback to invoke when Galaxy user authentication fails.</param>
	/// <param name="lost">The callback to invoke when Galaxy loses user authentication.</param>
	public GalaxyAuthListener(Action success, Action<FailureReason> failure, Action lost)
	{
		OnSuccess = success;
		OnFailure = failure;
		OnLost = lost;
		GalaxyInstance.ListenerRegistrar().Register(GalaxyTypeAwareListenerAuth.GetListenerType(), this);
	}

	/// <summary>Handles user authentication success, and invokes <see cref="F:StardewValley.SDKs.GogGalaxy.Listeners.GalaxyAuthListener.OnSuccess" />.</summary>
	public override void OnAuthSuccess()
	{
		OnSuccess?.Invoke();
	}

	/// <summary>Handles user authentication failure, and invokes <see cref="F:StardewValley.SDKs.GogGalaxy.Listeners.GalaxyAuthListener.OnFailure" />.</summary>
	public override void OnAuthFailure(FailureReason reason)
	{
		OnFailure?.Invoke(reason);
	}

	/// <summary>Handles loosing user authentication, and invokes <see cref="F:StardewValley.SDKs.GogGalaxy.Listeners.GalaxyAuthListener.OnLost" />.</summary>
	public override void OnAuthLost()
	{
		OnLost?.Invoke();
	}

	/// <summary>Unregisters the listener from the Galaxy SDK.</summary>
	public override void Dispose()
	{
		GalaxyInstance.ListenerRegistrar().Unregister(GalaxyTypeAwareListenerAuth.GetListenerType(), this);
		base.Dispose();
	}
}
