using System.IO;
using System.Text;

namespace Ionic.Zlib;

internal class SharedUtils
{
	/// <summary>
	/// Performs an unsigned bitwise right shift with the specified number
	/// </summary>
	/// <param name="number">Number to operate on</param>
	/// <param name="bits">Ammount of bits to shift</param>
	/// <returns>The resulting number from the shift operation</returns>
	public static int URShift(int number, int bits)
	{
		return number >>> bits;
	}

	/// <summary>
	///   Reads a number of characters from the current source TextReader and writes
	///   the data to the target array at the specified index.
	/// </summary>
	///
	/// <param name="sourceTextReader">The source TextReader to read from</param>
	/// <param name="target">Contains the array of characteres read from the source TextReader.</param>
	/// <param name="start">The starting index of the target array.</param>
	/// <param name="count">The maximum number of characters to read from the source TextReader.</param>
	///
	/// <returns>
	///   The number of characters read. The number will be less than or equal to
	///   count depending on the data available in the source TextReader. Returns -1
	///   if the end of the stream is reached.
	/// </returns>
	public static int ReadInput(TextReader sourceTextReader, byte[] target, int start, int count)
	{
		if (target.Length == 0)
		{
			return 0;
		}
		char[] charArray = new char[target.Length];
		int bytesRead = sourceTextReader.Read(charArray, start, count);
		if (bytesRead == 0)
		{
			return -1;
		}
		for (int index = start; index < start + bytesRead; index++)
		{
			target[index] = (byte)charArray[index];
		}
		return bytesRead;
	}

	internal static byte[] ToByteArray(string sourceString)
	{
		return Encoding.UTF8.GetBytes(sourceString);
	}

	internal static char[] ToCharArray(byte[] byteArray)
	{
		return Encoding.UTF8.GetChars(byteArray);
	}
}
