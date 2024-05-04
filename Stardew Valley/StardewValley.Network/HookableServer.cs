using System;

namespace StardewValley.Network;

/// <inheritdoc cref="T:StardewValley.Network.IHookableServer" />
public abstract class HookableServer : Server, IHookableServer
{
	/// <inheritdoc />
	public Action<IncomingMessage, Action<OutgoingMessage>, Action> OnProcessingMessage { get; set; }

	/// <summary>Construct an instance.</summary>
	/// <param name="gameServer">The underlying game server.</param>
	public HookableServer(IGameServer gameServer)
		: base(gameServer)
	{
		OnProcessingMessage = OnServerProcessingMessage;
	}

	private void OnServerProcessingMessage(IncomingMessage message, Action<OutgoingMessage> sendMessage, Action resume)
	{
		resume();
	}
}
