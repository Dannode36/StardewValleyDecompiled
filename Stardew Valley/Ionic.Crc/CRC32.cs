using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Ionic.Crc;

/// <summary>
///   Computes a CRC-32. The CRC-32 algorithm is parameterized - you
///   can set the polynomial and enable or disable bit
///   reversal. This can be used for GZIP, BZip2, or ZIP.
/// </summary>
/// <remarks>
///   This type is used internally by DotNetZip; it is generally not used
///   directly by applications wishing to create, read, or manipulate zip
///   archive files.
/// </remarks>
[Guid("ebc25cf6-9120-4283-b972-0e5520d0000C")]
[ComVisible(true)]
[ClassInterface(ClassInterfaceType.AutoDispatch)]
public class CRC32
{
	private uint dwPolynomial;

	private long _TotalBytesRead;

	private bool reverseBits;

	private uint[] crc32Table;

	private const int BUFFER_SIZE = 8192;

	private uint _register = uint.MaxValue;

	/// <summary>
	///   Indicates the total number of bytes applied to the CRC.
	/// </summary>
	public long TotalBytesRead => _TotalBytesRead;

	/// <summary>
	/// Indicates the current CRC for all blocks slurped in.
	/// </summary>
	public int Crc32Result => (int)(~_register);

	/// <summary>
	/// Returns the CRC32 for the specified stream.
	/// </summary>
	/// <param name="input">The stream over which to calculate the CRC32</param>
	/// <returns>the CRC32 calculation</returns>
	public int GetCrc32(Stream input)
	{
		return GetCrc32AndCopy(input, null);
	}

	/// <summary>
	/// Returns the CRC32 for the specified stream, and writes the input into the
	/// output stream.
	/// </summary>
	/// <param name="input">The stream over which to calculate the CRC32</param>
	/// <param name="output">The stream into which to deflate the input</param>
	/// <returns>the CRC32 calculation</returns>
	public int GetCrc32AndCopy(Stream input, Stream output)
	{
		if (input == null)
		{
			throw new Exception("The input stream must not be null.");
		}
		byte[] buffer = new byte[8192];
		int readSize = 8192;
		_TotalBytesRead = 0L;
		int count = input.Read(buffer, 0, readSize);
		output?.Write(buffer, 0, count);
		_TotalBytesRead += count;
		while (count > 0)
		{
			SlurpBlock(buffer, 0, count);
			count = input.Read(buffer, 0, readSize);
			output?.Write(buffer, 0, count);
			_TotalBytesRead += count;
		}
		return (int)(~_register);
	}

	/// <summary>
	///   Get the CRC32 for the given (word,byte) combo.  This is a
	///   computation defined by PKzip for PKZIP 2.0 (weak) encryption.
	/// </summary>
	/// <param name="W">The word to start with.</param>
	/// <param name="B">The byte to combine it with.</param>
	/// <returns>The CRC-ized result.</returns>
	public int ComputeCrc32(int W, byte B)
	{
		return _InternalComputeCrc32((uint)W, B);
	}

	internal int _InternalComputeCrc32(uint W, byte B)
	{
		return (int)(crc32Table[(W ^ B) & 0xFF] ^ (W >> 8));
	}

	/// <summary>
	/// Update the value for the running CRC32 using the given block of bytes.
	/// This is useful when using the CRC32() class in a Stream.
	/// </summary>
	/// <param name="block">block of bytes to slurp</param>
	/// <param name="offset">starting point in the block</param>
	/// <param name="count">how many bytes within the block to slurp</param>
	public void SlurpBlock(byte[] block, int offset, int count)
	{
		if (block == null)
		{
			throw new Exception("The data buffer must not be null.");
		}
		for (int i = 0; i < count; i++)
		{
			int x = offset + i;
			byte b = block[x];
			if (reverseBits)
			{
				uint temp = (_register >> 24) ^ b;
				_register = (_register << 8) ^ crc32Table[temp];
			}
			else
			{
				uint temp = (_register & 0xFFu) ^ b;
				_register = (_register >> 8) ^ crc32Table[temp];
			}
		}
		_TotalBytesRead += count;
	}

	/// <summary>
	///   Process one byte in the CRC.
	/// </summary>
	/// <param name="b">the byte to include into the CRC .  </param>
	public void UpdateCRC(byte b)
	{
		if (reverseBits)
		{
			uint temp = (_register >> 24) ^ b;
			_register = (_register << 8) ^ crc32Table[temp];
		}
		else
		{
			uint temp = (_register & 0xFFu) ^ b;
			_register = (_register >> 8) ^ crc32Table[temp];
		}
	}

	/// <summary>
	///   Process a run of N identical bytes into the CRC.
	/// </summary>
	/// <remarks>
	///   <para>
	///     This method serves as an optimization for updating the CRC when a
	///     run of identical bytes is found. Rather than passing in a buffer of
	///     length n, containing all identical bytes b, this method accepts the
	///     byte value and the length of the (virtual) buffer - the length of
	///     the run.
	///   </para>
	/// </remarks>
	/// <param name="b">the byte to include into the CRC.  </param>
	/// <param name="n">the number of times that byte should be repeated. </param>
	public void UpdateCRC(byte b, int n)
	{
		while (n-- > 0)
		{
			if (reverseBits)
			{
				uint temp = (_register >> 24) ^ b;
				_register = (_register << 8) ^ crc32Table[(temp >= 0) ? temp : (temp + 256)];
			}
			else
			{
				uint temp = (_register & 0xFFu) ^ b;
				_register = (_register >> 8) ^ crc32Table[(temp >= 0) ? temp : (temp + 256)];
			}
		}
	}

	private static uint ReverseBits(uint data)
	{
		uint ret = data;
		ret = ((ret & 0x55555555) << 1) | ((ret >> 1) & 0x55555555u);
		ret = ((ret & 0x33333333) << 2) | ((ret >> 2) & 0x33333333u);
		ret = ((ret & 0xF0F0F0F) << 4) | ((ret >> 4) & 0xF0F0F0Fu);
		return (ret << 24) | ((ret & 0xFF00) << 8) | ((ret >> 8) & 0xFF00u) | (ret >> 24);
	}

	private static byte ReverseBits(byte data)
	{
		int num = data * 131586;
		uint m = 17055760u;
		uint s = (uint)num & m;
		uint t = (uint)(num << 2) & (m << 1);
		return (byte)(16781313 * (s + t) >> 24);
	}

	private void GenerateLookupTable()
	{
		crc32Table = new uint[256];
		byte i = 0;
		do
		{
			uint dwCrc = i;
			for (byte j = 8; j > 0; j--)
			{
				dwCrc = (((dwCrc & 1) != 1) ? (dwCrc >> 1) : ((dwCrc >> 1) ^ dwPolynomial));
			}
			if (reverseBits)
			{
				crc32Table[ReverseBits(i)] = ReverseBits(dwCrc);
			}
			else
			{
				crc32Table[i] = dwCrc;
			}
			i++;
		}
		while (i != 0);
	}

	private uint gf2_matrix_times(uint[] matrix, uint vec)
	{
		uint sum = 0u;
		int i = 0;
		while (vec != 0)
		{
			if ((vec & 1) == 1)
			{
				sum ^= matrix[i];
			}
			vec >>= 1;
			i++;
		}
		return sum;
	}

	private void gf2_matrix_square(uint[] square, uint[] mat)
	{
		for (int i = 0; i < 32; i++)
		{
			square[i] = gf2_matrix_times(mat, mat[i]);
		}
	}

	/// <summary>
	///   Combines the given CRC32 value with the current running total.
	/// </summary>
	/// <remarks>
	///   This is useful when using a divide-and-conquer approach to
	///   calculating a CRC.  Multiple threads can each calculate a
	///   CRC32 on a segment of the data, and then combine the
	///   individual CRC32 values at the end.
	/// </remarks>
	/// <param name="crc">the crc value to be combined with this one</param>
	/// <param name="length">the length of data the CRC value was calculated on</param>
	public void Combine(int crc, int length)
	{
		uint[] even = new uint[32];
		uint[] odd = new uint[32];
		if (length == 0)
		{
			return;
		}
		uint crc1 = ~_register;
		odd[0] = dwPolynomial;
		uint row = 1u;
		for (int i = 1; i < 32; i++)
		{
			odd[i] = row;
			row <<= 1;
		}
		gf2_matrix_square(even, odd);
		gf2_matrix_square(odd, even);
		uint len2 = (uint)length;
		do
		{
			gf2_matrix_square(even, odd);
			if ((len2 & 1) == 1)
			{
				crc1 = gf2_matrix_times(even, crc1);
			}
			len2 >>= 1;
			if (len2 == 0)
			{
				break;
			}
			gf2_matrix_square(odd, even);
			if ((len2 & 1) == 1)
			{
				crc1 = gf2_matrix_times(odd, crc1);
			}
			len2 >>= 1;
		}
		while (len2 != 0);
		crc1 ^= (uint)crc;
		_register = ~crc1;
	}

	/// <summary>
	///   Create an instance of the CRC32 class using the default settings: no
	///   bit reversal, and a polynomial of 0xEDB88320.
	/// </summary>
	public CRC32()
		: this(reverseBits: false)
	{
	}

	/// <summary>
	///   Create an instance of the CRC32 class, specifying whether to reverse
	///   data bits or not.
	/// </summary>
	/// <param name="reverseBits">
	///   specify true if the instance should reverse data bits.
	/// </param>
	/// <remarks>
	///   <para>
	///     In the CRC-32 used by BZip2, the bits are reversed. Therefore if you
	///     want a CRC32 with compatibility with BZip2, you should pass true
	///     here. In the CRC-32 used by GZIP and PKZIP, the bits are not
	///     reversed; Therefore if you want a CRC32 with compatibility with
	///     those, you should pass false.
	///   </para>
	/// </remarks>
	public CRC32(bool reverseBits)
		: this(-306674912, reverseBits)
	{
	}

	/// <summary>
	///   Create an instance of the CRC32 class, specifying the polynomial and
	///   whether to reverse data bits or not.
	/// </summary>
	/// <param name="polynomial">
	///   The polynomial to use for the CRC, expressed in the reversed (LSB)
	///   format: the highest ordered bit in the polynomial value is the
	///   coefficient of the 0th power; the second-highest order bit is the
	///   coefficient of the 1 power, and so on. Expressed this way, the
	///   polynomial for the CRC-32C used in IEEE 802.3, is 0xEDB88320.
	/// </param>
	/// <param name="reverseBits">
	///   specify true if the instance should reverse data bits.
	/// </param>
	///
	/// <remarks>
	///   <para>
	///     In the CRC-32 used by BZip2, the bits are reversed. Therefore if you
	///     want a CRC32 with compatibility with BZip2, you should pass true
	///     here for the <c>reverseBits</c> parameter. In the CRC-32 used by
	///     GZIP and PKZIP, the bits are not reversed; Therefore if you want a
	///     CRC32 with compatibility with those, you should pass false for the
	///     <c>reverseBits</c> parameter.
	///   </para>
	/// </remarks>
	public CRC32(int polynomial, bool reverseBits)
	{
		this.reverseBits = reverseBits;
		dwPolynomial = (uint)polynomial;
		GenerateLookupTable();
	}

	/// <summary>
	///   Reset the CRC-32 class - clear the CRC "remainder register."
	/// </summary>
	/// <remarks>
	///   <para>
	///     Use this when employing a single instance of this class to compute
	///     multiple, distinct CRCs on multiple, distinct data blocks.
	///   </para>
	/// </remarks>
	public void Reset()
	{
		_register = uint.MaxValue;
	}
}
