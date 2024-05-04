using System;

namespace StardewValley.Network;

/// <inheritdoc cref="T:StardewValley.Network.IHookableClient" />
public abstract class HookableClient : Client, IHookableClient
{
	/// <inheritdoc />
	public Action<IncomingMessage, Action<OutgoingMessage>, Action> OnProcessingMessage { get; set; }

	/// <inheritdoc />
	public Action<OutgoingMessage, Action<OutgoingMessage>, Action> OnSendingMessage { get; set; }

	/// <summary>Construct an instance.</summary>
	public HookableClient()
	{
		OnProcessingMessage = OnClientProcessingMessage;
		OnSendingMessage = OnClientSendingMessage;
	}

	private void OnClientProcessingMessage(IncomingMessage message, Action<OutgoingMessage> sendMessage, Action resume)
	{
		resume();
	}

	private void OnClientSendingMessage(OutgoingMessage message, Action<OutgoingMessage> sendMessage, Action resume)
	{
		resume();
	}
}
