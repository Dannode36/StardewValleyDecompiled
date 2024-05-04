using System;

namespace StardewValley.SDKs.GogGalaxy.Internal;

public class Base36
{
	private const string Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

	private const ulong Base = 36uL;

	public static string Encode(ulong value)
	{
		string result = "";
		if (value == 0L)
		{
			return "0";
		}
		while (value != 0L)
		{
			int digit = (int)(value % 36);
			value /= 36;
			result = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ"[digit] + result;
		}
		return result;
	}

	public static ulong Decode(string value)
	{
		value = value.ToUpper();
		ulong result = 0uL;
		string text = value;
		foreach (char ch in text)
		{
			result *= 36;
			int digit = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".IndexOf(ch);
			if (digit == -1)
			{
				throw new FormatException(value);
			}
			result += (ulong)digit;
		}
		return result;
	}
}
