using System.IO;

namespace StardewValley.Network.Compress;

/// <summary>Handles compression and decompression of network messages to reduce network traffic.</summary>
public interface INetCompression
{
	/// <summary>Compress a message if it exceeds a minimum size.</summary>
	/// <param name="data">The data to compress.</param>
	/// <param name="minSizeToCompress">The minimum message size (in bytes) for compression to be applied.</param>
	/// <returns>Returns the compressed data, or the original data if compression wasn't needed.</returns>
	byte[] CompressAbove(byte[] data, int minSizeToCompress = 256);

	/// <summary>Decompress a message if it contains compressed data.</summary>
	/// <param name="data">The data to decompress.</param>
	/// <returns>Returns the decompressed data, or the original data if it wasn't compressed.</returns>
	byte[] DecompressBytes(byte[] data);

	/// <summary>Decompress a message if it contains compressed data.</summary>
	/// <param name="dataStream">The data to decompress.</param>
	/// <param name="decompressed">The decompressed data, or <c>null</c> if it wasn't compressed.</param>
	/// <returns>Returns whether the stream held compressed data which was decompressed into <paramref name="decompressed" />.</returns>
	bool TryDecompressStream(Stream dataStream, out byte[] decompressed);
}
