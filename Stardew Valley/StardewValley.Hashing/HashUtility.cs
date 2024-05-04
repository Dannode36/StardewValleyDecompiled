using System;
using System.Data.HashFunction;
using System.Text;

namespace StardewValley.Hashing;

/// <inheritdoc cref="T:StardewValley.Hashing.IHashUtility" />
public class HashUtility : IHashUtility
{
	/// <summary>The underlying hashing API.</summary>
	private static readonly IHashFunction Hasher = (IHashFunction)new xxHash(32);

	/// <inheritdoc />
	public int GetDeterministicHashCode(string value)
	{
		byte[] data = Encoding.UTF8.GetBytes(value);
		return GetDeterministicHashCode(data);
	}

	/// <inheritdoc />
	public int GetDeterministicHashCode(params int[] values)
	{
		byte[] data = new byte[values.Length * 4];
		Buffer.BlockCopy(values, 0, data, 0, data.Length);
		return GetDeterministicHashCode(data);
	}

	/// <summary>Get a deterministic hash code for a byte data array.</summary>
	/// <param name="data">The data to hash.</param>
	public int GetDeterministicHashCode(byte[] data)
	{
		return BitConverter.ToInt32(Hasher.ComputeHash(data), 0);
	}
}
