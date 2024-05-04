namespace StardewValley.Network.NetReady.Internal;

/// <summary>The network message types used to synchronize ready checks.</summary>
internal enum ReadyCheckMessageType : byte
{
	/// <summary>Sent by clients to mark the check as ready.</summary>
	Ready,
	/// <summary>Sent by clients to cancel a ready check.</summary>
	Cancel,
	/// <summary>Broadcast by the server to request that clients prohibit any further cancellations.</summary>
	Lock,
	/// <summary>Broadcast by the server to signal to clients that they can cancel the ready check.</summary>
	Release,
	/// <summary>Sent by clients to accept a lock.</summary>
	AcceptLock,
	/// <summary>Sent by clients to reject a lock.</summary>
	RejectLock,
	/// <summary>Broadcast by the server to update the displayed ready and required player counts.</summary>
	UpdateAmounts,
	/// <summary>Sent by clients to update the required players for a check.</summary>
	RequireFarmers,
	/// <summary>Broadcast by the server when all players are ready, locked, and should proceed.</summary>
	Finish
}
