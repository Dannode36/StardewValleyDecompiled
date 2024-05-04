namespace StardewValley.Network.NetReady.Internal;

/// <summary>The possible states for a ready check.</summary>
internal enum ReadyState : byte
{
	/// <summary>Not marked as ready to proceed with the check.</summary>
	NotReady,
	/// <summary>Ready to proceed, but can still cancel.</summary>
	Ready,
	/// <summary>Ready to proceed, and can no longer cancel.</summary>
	Locked
}
