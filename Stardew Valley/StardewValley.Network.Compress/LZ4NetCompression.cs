using System;
using System.IO;
using System.Runtime.InteropServices;
using LWJGL;

namespace StardewValley.Network.Compress;

/// <summary>Handles compression and decompression of network messages using LZ4 to reduce network traffic.</summary>
internal class LZ4NetCompression : INetCompression
{
	/// <summary>The size of the header used for compressed messages.</summary>
	private const int HeaderSize = 9;

	/// <inheritdoc />
	public byte[] CompressAbove(byte[] data, int minSizeToCompress = 256)
	{
		if (data.Length < minSizeToCompress)
		{
			return data;
		}
		int destSize = LZ4.CompressBound(data.Length);
		IntPtr intPtr = Marshal.AllocHGlobal(destSize + 9);
		IntPtr dest = IntPtr.Add(intPtr, 9);
		int compressedSize = LZ4.CompressDefault(data, dest, data.Length, destSize);
		Marshal.WriteByte(intPtr, 0, 127);
		Marshal.WriteInt32(intPtr, 1, compressedSize);
		Marshal.WriteInt32(intPtr, 5, data.Length);
		byte[] compressed = new byte[compressedSize + 9];
		Marshal.Copy(intPtr, compressed, 0, compressed.Length);
		Marshal.FreeHGlobal(intPtr);
		return compressed;
	}

	/// <inheritdoc />
	public byte[] DecompressBytes(byte[] data)
	{
		if (data[0] != 127)
		{
			return data;
		}
		return DecompressImpl(data);
	}

	/// <inheritdoc />
	/// <exception cref="T:System.ArgumentException">The stream doesn't support both reading and seeking.</exception>
	public bool TryDecompressStream(Stream dataStream, out byte[] decompressed)
	{
		decompressed = null;
		if (!dataStream.CanSeek || !dataStream.CanRead)
		{
			throw new ArgumentException("dataStream must support both reading and seeking");
		}
		long startPosition = dataStream.Position;
		if ((byte)dataStream.ReadByte() != 127)
		{
			dataStream.Seek(startPosition, SeekOrigin.Begin);
			return false;
		}
		byte[] compressedSizeHeader = new byte[4];
		dataStream.Read(compressedSizeHeader, 0, 4);
		int compressedSize = BitConverter.ToInt32(compressedSizeHeader, 0);
		byte[] data = new byte[compressedSize + 9];
		dataStream.Read(data, 5, 4 + compressedSize);
		decompressed = DecompressImpl(data);
		return true;
	}

	/// <summary>Decompress raw data without checking whether it's compressed.</summary>
	/// <param name="data">The compressed data.</param>
	/// <returns>Returns the data decompressed from <paramref name="data" />.</returns>
	private unsafe byte[] DecompressImpl(byte[] data)
	{
		int decompressedSize = BitConverter.ToInt32(data, 5);
		byte[] decompressed = new byte[decompressedSize];
		fixed (byte* ptr = data)
		{
			LZ4.DecompressSafe(IntPtr.Add((IntPtr)ptr, 9), decompressed, data.Length - 9, decompressedSize);
		}
		return decompressed;
	}
}
