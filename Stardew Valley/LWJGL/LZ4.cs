using System;
using System.Runtime.InteropServices;

namespace LWJGL;

/// <summary>Provides LZ4 compression methods from the Lightweight Java Game Library (LWJGL)</summary>
public class LZ4
{
	/// <summary>Compute the maximum size (in bytes) needed to store a compressed representation of <paramref name="inputSize" /> bytes.</summary>
	/// <param name="env">Unused.</param>
	/// <param name="clazz">Unused.</param>
	/// <param name="inputSize">The length (in bytes) of the data to compress.</param>
	[DllImport("liblwjgl_lz4", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Java_org_lwjgl_util_lz4_LZ4_LZ4_1compressBound")]
	private static extern int lwjgl_compressBound(IntPtr env, IntPtr clazz, int inputSize);

	/// <summary>Compress data from <paramref name="src" /> and store it in <paramref name="dest" />.</summary>
	/// <param name="env">Unused.</param>
	/// <param name="clazz">Unused.</param>
	/// <param name="src">The byte array representation of the data to compress.</param>
	/// <param name="dest">A user-provided buffer to store compressed data.</param>
	/// <param name="srcSize">The number of bytes to read from <paramref name="src" />, starting from index 0.</param>
	/// <param name="dstCapacity">The size of the buffer provided by <paramref name="dest" />.</param>
	[DllImport("liblwjgl_lz4", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Java_org_lwjgl_util_lz4_LZ4_nLZ4_1compress_1default")]
	private static extern int lwjgl_compress_default(IntPtr env, IntPtr clazz, byte[] src, IntPtr dest, int srcSize, int dstCapacity);

	/// <summary>Decompress data from <paramref name="src" /> and store it in <paramref name="dest" />.</summary>
	/// <param name="env">Unused.</param>
	/// <param name="clazz">Unused.</param>
	/// <param name="src">A buffer that holds the compressed data.</param>
	/// <param name="dest">A user-provided byte array to store decompressed data.</param>
	/// <param name="compressedSize">The number of bytes to read from the <paramref name="src" /> buffer.</param>
	/// <param name="dstCapacity">The size of the byte array provided by <paramref name="dest" />.</param>
	[DllImport("liblwjgl_lz4", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Java_org_lwjgl_util_lz4_LZ4_nLZ4_1decompress_1safe")]
	private static extern int lwjgl_decompress_safe(IntPtr env, IntPtr clazz, IntPtr src, byte[] dest, int compressedSize, int dstCapacity);

	/// <summary>Compute the maximum size (in bytes) needed to store a compressed representation of <paramref name="inputSize" /> bytes.</summary>
	/// <param name="inputSize">The length (in bytes) of the data to compress.</param>
	public static int CompressBound(int inputSize)
	{
		return lwjgl_compressBound(IntPtr.Zero, IntPtr.Zero, inputSize);
	}

	/// <summary>Compress data from <paramref name="src" /> and store it in <paramref name="dest" />.</summary>
	/// <param name="src">The byte array representation of the data to compress.</param>
	/// <param name="dest">A user-provided buffer to store compressed data.</param>
	/// <param name="srcSize">The number of bytes to read from <paramref name="src" />, starting from index 0.</param>
	/// <param name="dstCapacity">The size of the buffer provided by <paramref name="dest" />.</param>
	public static int CompressDefault(byte[] src, IntPtr dest, int srcSize, int dstCapacity)
	{
		return lwjgl_compress_default(IntPtr.Zero, IntPtr.Zero, src, dest, srcSize, dstCapacity);
	}

	/// <summary>Decompress data from <paramref name="src" /> and store it in <paramref name="dest" />.</summary>
	/// <param name="src">A buffer that holds the compressed data.</param>
	/// <param name="dest">A user-provided byte array to store decompressed data.</param>
	/// <param name="compressedSize">The number of bytes to read from the <paramref name="src" /> buffer.</param>
	/// <param name="dstCapacity">The size of the byte array provided by <paramref name="dest" />.</param>
	public static int DecompressSafe(IntPtr src, byte[] dest, int compressedSize, int dstCapacity)
	{
		return lwjgl_decompress_safe(IntPtr.Zero, IntPtr.Zero, src, dest, compressedSize, dstCapacity);
	}
}
