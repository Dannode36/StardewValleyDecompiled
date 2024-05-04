namespace StardewValley.Network.ChestHit.Internal;

/// <summary>The network message types used to synchronize chest hits.</summary>
internal enum ChestHitMessageType : byte
{
	/// <summary>Sent by clients when they hit a chest.</summary>
	Sync,
	/// <summary>Sent by the server to signal a chest has been moved.</summary>
	Move,
	/// <summary>Sent by the server to signal a chest has been deleted.</summary>
	Delete
}
