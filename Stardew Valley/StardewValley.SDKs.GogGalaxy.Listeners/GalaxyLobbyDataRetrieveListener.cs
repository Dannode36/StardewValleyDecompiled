using System;
using Galaxy.Api;

namespace StardewValley.SDKs.GogGalaxy.Listeners;

/// <summary>Listener notified when the Galaxy SDK retrieves lobby data.</summary>
internal sealed class GalaxyLobbyDataRetrieveListener : ILobbyDataRetrieveListener
{
	/// <summary>The callback to invoke when fetching Galaxy lobby data succeeds.</summary>
	private readonly Action<GalaxyID> OnSuccess;

	/// <summary>The callback to invoke when fetching Galaxy lobby data fails.</summary>
	private readonly Action<GalaxyID, FailureReason> OnFailure;

	/// <summary>Constructs an instance of the listener.</summary>
	/// <param name="success">The callback to invoke when fetching Galaxy lobby data succeeds.</param>
	/// <param name="failure">The callback to invoke when fetching Galaxy lobby data fails.</param>
	public GalaxyLobbyDataRetrieveListener(Action<GalaxyID> success, Action<GalaxyID, FailureReason> failure)
	{
		OnSuccess = success;
		OnFailure = failure;
	}

	/// <summary>Handles successful retrieval of Galaxy lobby data, and invokes <see cref="F:StardewValley.SDKs.GogGalaxy.Listeners.GalaxyLobbyDataRetrieveListener.OnSuccess" />.</summary>
	public override void OnLobbyDataRetrieveSuccess(GalaxyID lobbyID)
	{
		OnSuccess?.Invoke(lobbyID);
	}

	/// <summary>Handles failure to retrieve Galaxy lobby data, and invokes <see cref="F:StardewValley.SDKs.GogGalaxy.Listeners.GalaxyLobbyDataRetrieveListener.OnFailure" />.</summary>
	public override void OnLobbyDataRetrieveFailure(GalaxyID lobbyID, FailureReason failureReason)
	{
		OnFailure?.Invoke(lobbyID, failureReason);
	}
}
