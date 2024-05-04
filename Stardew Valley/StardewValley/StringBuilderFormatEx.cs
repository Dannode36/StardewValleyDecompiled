using System;
using System.Globalization;
using System.Text;

namespace StardewValley;

/// <summary>
/// StringBuilder extension methods for garbage free append and format of numeric types.
/// </summary>
/// <remarks>
/// Based on the work of Gavin Pugh.
/// http://www.gavpugh.com/2010/04/05/xnac-a-garbage-free-stringbuilder-format-method/
/// </remarks>
public static class StringBuilderFormatEx
{
	private static readonly char[] MsDigits;

	private const uint MsDefaultDecimalPlaces = 5u;

	private const char MsDefaultPadChar = '0';

	private static char[] _buffer;

	public static bool StringsEqual(this StringBuilder sb, string value)
	{
		if (sb == null != (value == null))
		{
			return false;
		}
		if (value == null)
		{
			return true;
		}
		if (sb.Length != value.Length)
		{
			return false;
		}
		for (int i = 0; i < value.Length; i++)
		{
			if (value[i] != sb[i])
			{
				return false;
			}
		}
		return true;
	}

	private static char[] _getBuffer(int len)
	{
		if (_buffer == null || _buffer.Length < len)
		{
			_buffer = new char[len];
		}
		return _buffer;
	}

	public static StringBuilder AppendEx(this StringBuilder stringBuilder, StringBuilder value)
	{
		int len = value.Length;
		char[] buff = _getBuffer(len);
		value.CopyTo(0, buff, 0, len);
		stringBuilder.Append(buff, 0, len);
		return stringBuilder;
	}

	static StringBuilderFormatEx()
	{
		MsDigits = new char[16]
		{
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
			'A', 'B', 'C', 'D', 'E', 'F'
		};
		Init();
	}

	public static void Init()
	{
	}

	/// <summary>
	/// Convert an unsigned integer value to a string and concatenate into StringBuilder. Any base value allowed.
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, uint uintVal, uint padAmount, char padChar, uint baseVal)
	{
		uint length = 0u;
		uint lengthCalc = uintVal;
		do
		{
			lengthCalc /= baseVal;
			length++;
		}
		while (lengthCalc != 0);
		stringBuilder.Append(padChar, (int)Math.Max(padAmount, length));
		int strpos = stringBuilder.Length;
		while (length != 0)
		{
			strpos--;
			stringBuilder[strpos] = MsDigits[uintVal % baseVal];
			uintVal /= baseVal;
			length--;
		}
		return stringBuilder;
	}

	/// <summary>
	/// Convert an unsigned integer value to a string and concatenate into StringBuilder. Assumes no padding and base ten.        
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, uint uintVal)
	{
		stringBuilder.AppendEx(uintVal, 0u, '0', 10u);
		return stringBuilder;
	}

	/// <summary>
	/// Convert an unsigned integer value to a string and concatenate into StringBuilder. Assumes base ten.
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, uint uintVal, uint padAmount)
	{
		stringBuilder.AppendEx(uintVal, padAmount, '0', 10u);
		return stringBuilder;
	}

	/// <summary>
	/// Convert an unsigned integer value to a string and concatenate into StringBuilder. Assumes base ten.
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, uint uintVal, uint padAmount, char padChar)
	{
		stringBuilder.AppendEx(uintVal, padAmount, padChar, 10u);
		return stringBuilder;
	}

	/// <summary>
	/// Converts a signed integer value to a string and concatenate into StringBuilder. Any base value allowed.
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, int intVal, uint padAmount, char padChar, uint baseVal)
	{
		if (intVal < 0)
		{
			stringBuilder.Append('-');
			uint uintVal = (uint)(-1 - intVal + 1);
			stringBuilder.AppendEx(uintVal, padAmount, padChar, baseVal);
		}
		else
		{
			stringBuilder.AppendEx((uint)intVal, padAmount, padChar, baseVal);
		}
		return stringBuilder;
	}

	/// <summary>
	/// Converts a signed integer value to a string and concatenate into StringBuilder. Assumes no padding and base ten.
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, int intVal)
	{
		stringBuilder.AppendEx(intVal, 0u, '0', 10u);
		return stringBuilder;
	}

	/// <summary>
	/// Convert a signed integer value to a string and concatenate into StringBuilder. Assumes base ten.
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, int intVal, uint padAmount)
	{
		stringBuilder.AppendEx(intVal, padAmount, '0', 10u);
		return stringBuilder;
	}

	/// <summary>
	/// Convert a signed integer value to a string and concatenate into StringBuilder. Assumes base ten.
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, int intVal, uint padAmount, char padChar)
	{
		stringBuilder.AppendEx(intVal, padAmount, padChar, 10u);
		return stringBuilder;
	}

	/// <summary>
	/// Convert an unsigned long value to a string and concatenate into StringBuilder. Any base value allowed.
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, ulong uintVal, uint padAmount, char padChar, uint baseVal)
	{
		uint length = 0u;
		ulong lengthCalc = uintVal;
		do
		{
			lengthCalc /= baseVal;
			length++;
		}
		while (lengthCalc != 0);
		stringBuilder.Append(padChar, (int)Math.Max(padAmount, length));
		int strpos = stringBuilder.Length;
		while (length != 0)
		{
			strpos--;
			stringBuilder[strpos] = MsDigits[uintVal % baseVal];
			uintVal /= baseVal;
			length--;
		}
		return stringBuilder;
	}

	/// <summary>
	/// Convert an unsigned long value to a string and concatenate into StringBuilder. Assumes no padding and base ten.        
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, ulong uintVal)
	{
		stringBuilder.AppendEx(uintVal, 0u, '0', 10u);
		return stringBuilder;
	}

	/// <summary>
	/// Convert an unsigned long value to a string and concatenate into StringBuilder. Assumes base ten.
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, ulong uintVal, uint padAmount)
	{
		stringBuilder.AppendEx(uintVal, padAmount, '0', 10u);
		return stringBuilder;
	}

	/// <summary>
	/// Convert an unsigned long value to a string and concatenate into StringBuilder. Assumes base ten.
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, ulong uintVal, uint padAmount, char padChar)
	{
		stringBuilder.AppendEx(uintVal, padAmount, padChar, 10u);
		return stringBuilder;
	}

	/// <summary>
	/// Converts a signed long value to a string and concatenate into StringBuilder. Any base value allowed.
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, long intVal, uint padAmount, char padChar, uint baseVal)
	{
		if (intVal < 0)
		{
			stringBuilder.Append('-');
			uint uintVal = (uint)(-1 - (int)intVal + 1);
			stringBuilder.AppendEx(uintVal, padAmount, padChar, baseVal);
		}
		else
		{
			stringBuilder.AppendEx((uint)intVal, padAmount, padChar, baseVal);
		}
		return stringBuilder;
	}

	/// <summary>
	/// Converts a signed long value to a string and concatenate into StringBuilder. Assumes no padding and base ten.
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, long intVal)
	{
		stringBuilder.AppendEx(intVal, 0u, '0', 10u);
		return stringBuilder;
	}

	/// <summary>
	/// Convert a signed long value to a string and concatenate into StringBuilder. Assumes base ten.
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, long intVal, uint padAmount)
	{
		stringBuilder.AppendEx(intVal, padAmount, '0', 10u);
		return stringBuilder;
	}

	/// <summary>
	/// Convert a signed long value to a string and concatenate into StringBuilder. Assumes base ten.
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, long intVal, uint padAmount, char padChar)
	{
		stringBuilder.AppendEx(intVal, padAmount, padChar, 10u);
		return stringBuilder;
	}

	/// <summary>
	/// Convert a float value to a string and concatenate into StringBuilder.
	/// </summary>
	public static StringBuilder AppendEx(this StringBuilder stringBuilder, float floatVal, uint decimalPlaces, uint padAmount, char padChar)
	{
		if (decimalPlaces == 0)
		{
			int intVal = ((!(floatVal >= 0f)) ? ((int)(floatVal - 0.5f)) : ((int)(floatVal + 0.5f)));
			stringBuilder.AppendEx(intVal, padAmount, padChar, 10u);
		}
		else
		{
			int intPart = (int)floatVal;
			stringBuilder.AppendEx(intPart, padAmount, padChar, 10u);
			stringBuilder.Append('.');
			float remainder = Math.Abs(floatVal - (float)intPart);
			for (int i = 0; i < decimalPlaces; i++)
			{
				remainder *= 10f;
			}
			stringBuilder.AppendEx((int)remainder, decimalPlaces, '0', 10u);
		}
		return stringBuilder;
	}

	/// <summary>
	/// Convert a float value to a string and concatenate into StringBuilder. Assumes five decimal places, and no padding.
	/// </summary>
	public static StringBuilder AppendFormatEx(this StringBuilder stringBuilder, float floatVal)
	{
		stringBuilder.AppendEx(floatVal, 5u, 0u, '0');
		return stringBuilder;
	}

	public static StringBuilder AppendFormatEx(this StringBuilder stringBuilder, float floatVal, uint decimalPlaces)
	{
		stringBuilder.AppendEx(floatVal, decimalPlaces, 0u, '0');
		return stringBuilder;
	}

	/// <summary>
	/// Convert a float value to a string and concatenate into StringBuilder.
	/// </summary>
	public static StringBuilder AppendFormatEx(this StringBuilder stringBuilder, float floatVal, uint decimalPlaces, uint padAmount)
	{
		stringBuilder.AppendEx(floatVal, decimalPlaces, padAmount, '0');
		return stringBuilder;
	}

	/// <summary>
	/// Concatenate a formatted string with arguments.
	/// </summary> 
	public static StringBuilder AppendFormatEx<TA>(this StringBuilder stringBuilder, string formatString, TA arg1) where TA : IConvertible
	{
		return stringBuilder.AppendFormatEx(formatString, arg1, 0, 0, 0, 0);
	}

	/// <summary>
	/// Concatenate a formatted string with arguments.
	/// </summary> 
	public static StringBuilder AppendFormatEx<TA, TB>(this StringBuilder stringBuilder, string formatString, TA arg1, TB arg2) where TA : IConvertible where TB : IConvertible
	{
		return stringBuilder.AppendFormatEx(formatString, arg1, arg2, 0, 0, 0);
	}

	/// <summary>
	/// Concatenate a formatted string with arguments.
	/// </summary> 
	public static StringBuilder AppendFormatEx<TA, TB, TC>(this StringBuilder stringBuilder, string formatString, TA arg1, TB arg2, TC arg3) where TA : IConvertible where TB : IConvertible where TC : IConvertible
	{
		return stringBuilder.AppendFormatEx(formatString, arg1, arg2, arg3, 0, 0);
	}

	/// <summary>
	/// Concatenate a formatted string with arguments.
	/// </summary> 
	public static StringBuilder AppendFormatEx<TA, TB, TC, TD>(this StringBuilder stringBuilder, string formatString, TA arg1, TB arg2, TC arg3, TD arg4) where TA : IConvertible where TB : IConvertible where TC : IConvertible where TD : IConvertible
	{
		return stringBuilder.AppendFormatEx(formatString, arg1, arg2, arg3, arg4, 0);
	}

	/// <summary>
	/// Concatenate a formatted string with arguments.
	/// </summary>
	public static StringBuilder AppendFormatEx<TA, TB, TC, TD, TE>(this StringBuilder stringBuilder, string formatString, TA arg1, TB arg2, TC arg3, TD arg4, TE arg5) where TA : IConvertible where TB : IConvertible where TC : IConvertible where TD : IConvertible where TE : IConvertible
	{
		int verbatimRangeStart = 0;
		for (int index = 0; index < formatString.Length; index++)
		{
			if (formatString[index] != '{')
			{
				continue;
			}
			if (verbatimRangeStart < index)
			{
				stringBuilder.Append(formatString, verbatimRangeStart, index - verbatimRangeStart);
			}
			uint baseValue = 10u;
			uint padding = 0u;
			uint decimalPlaces = 5u;
			index++;
			char formatChar = formatString[index];
			if (formatChar == '{')
			{
				stringBuilder.Append('{');
				index++;
			}
			else
			{
				index++;
				if (formatString[index] == ':')
				{
					index++;
					while (formatString[index] == '0')
					{
						index++;
						padding++;
					}
					switch (formatString[index])
					{
					case 'X':
						index++;
						baseValue = 16u;
						if (formatString[index] >= '0' && formatString[index] <= '9')
						{
							padding = (uint)(formatString[index] - 48);
							index++;
						}
						break;
					case '.':
						index++;
						decimalPlaces = 0u;
						while (formatString[index] == '0')
						{
							index++;
							decimalPlaces++;
						}
						break;
					}
				}
				for (; formatString[index] != '}'; index++)
				{
				}
				switch (formatChar)
				{
				case '0':
					stringBuilder.AppendFormatValue(arg1, padding, baseValue, decimalPlaces);
					break;
				case '1':
					stringBuilder.AppendFormatValue(arg2, padding, baseValue, decimalPlaces);
					break;
				case '2':
					stringBuilder.AppendFormatValue(arg3, padding, baseValue, decimalPlaces);
					break;
				case '3':
					stringBuilder.AppendFormatValue(arg4, padding, baseValue, decimalPlaces);
					break;
				case '4':
					stringBuilder.AppendFormatValue(arg5, padding, baseValue, decimalPlaces);
					break;
				}
			}
			verbatimRangeStart = index + 1;
		}
		if (verbatimRangeStart < formatString.Length)
		{
			stringBuilder.Append(formatString, verbatimRangeStart, formatString.Length - verbatimRangeStart);
		}
		return stringBuilder;
	}

	private static void AppendFormatValue<T>(this StringBuilder stringBuilder, T arg, uint padding, uint baseValue, uint decimalPlaces) where T : IConvertible
	{
		switch ((TypeCode)(((int?)arg?.GetTypeCode()) ?? ((!(arg is string)) ? 1 : 18)))
		{
		case TypeCode.Byte:
			stringBuilder.AppendEx(arg.ToUInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
			break;
		case TypeCode.SByte:
			stringBuilder.AppendEx(arg.ToInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
			break;
		case TypeCode.UInt16:
			stringBuilder.AppendEx(arg.ToUInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
			break;
		case TypeCode.Int16:
			stringBuilder.AppendEx(arg.ToInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
			break;
		case TypeCode.UInt32:
			stringBuilder.AppendEx(arg.ToUInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
			break;
		case TypeCode.Int32:
			stringBuilder.AppendEx(arg.ToInt32(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
			break;
		case TypeCode.UInt64:
			stringBuilder.AppendEx(arg.ToUInt64(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
			break;
		case TypeCode.Int64:
			stringBuilder.AppendEx(arg.ToInt64(NumberFormatInfo.CurrentInfo), padding, '0', baseValue);
			break;
		case TypeCode.Single:
		case TypeCode.Double:
			stringBuilder.AppendEx(arg.ToSingle(NumberFormatInfo.CurrentInfo), decimalPlaces, padding, '0');
			break;
		case TypeCode.Object:
		case TypeCode.Boolean:
			stringBuilder.Append(Convert.ToString(arg));
			break;
		case TypeCode.String:
			stringBuilder.Append(arg);
			break;
		case TypeCode.DBNull:
		case TypeCode.Char:
		case TypeCode.Decimal:
		case TypeCode.DateTime:
		case (TypeCode)17:
			break;
		}
	}
}
