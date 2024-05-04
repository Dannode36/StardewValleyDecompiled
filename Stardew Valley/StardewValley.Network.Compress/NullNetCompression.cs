using System.IO;

namespace StardewValley.Network.Compress;

/// <summary>A no-op compression wrapper for platforms that provide no compression.</summary>
internal class NullNetCompression : INetCompression
{
	/// <inheritdoc />
	public byte[] CompressAbove(byte[] data, int minSizeToCompress = 256)
	{
		return data;
	}

	/// <inheritdoc />
	public byte[] DecompressBytes(byte[] data)
	{
		return data;
	}

	/// <inheritdoc />
	public bool TryDecompressStream(Stream dataStream, out byte[] decompressed)
	{
		decompressed = null;
		return false;
	}
}
