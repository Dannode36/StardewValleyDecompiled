using System;

namespace StardewValley;

/// <summary>Wraps newer .NET features that improve performance, but aren't available on .NET Framework platforms.</summary>
internal static class LegacyShims
{
	/// <summary>Get an empty array without allocating a new array each time.</summary>
	/// <typeparam name="T">The array value type.</typeparam>
	public static T[] EmptyArray<T>()
	{
		return Array.Empty<T>();
	}

	/// <summary>Split a string into substrings based on a specified separator, and trim the resulting substrings.</summary>
	/// <param name="str">The string to split.</param>
	/// <param name="separator">The character that delimits the substrings within the string.</param>
	/// <param name="options">The split options to apply.</param>
	/// <remarks>This method exists for cross-compatibility between .NET Framework and .NET platforms. Mod code should generally use <see cref="M:System.String.Split(System.Char,System.StringSplitOptions)" /> with <see cref="F:System.StringSplitOptions.TrimEntries" /> directly instead.</remarks>
	public static string[] SplitAndTrim(string str, char separator, StringSplitOptions options = StringSplitOptions.None)
	{
		return str.Split(separator, options | StringSplitOptions.TrimEntries);
	}

	/// <summary>Split a string into substrings based on a specified separator, and trim the resulting substrings.</summary>
	/// <param name="str">The string to split.</param>
	/// <param name="separator">The string that delimits the substrings within the string.</param>
	/// <param name="options">The split options to apply.</param>
	/// <remarks>This method exists for cross-compatibility between .NET Framework and .NET platforms. Mod code should generally use <see cref="M:System.String.Split(System.String,System.StringSplitOptions)" /> with <see cref="F:System.StringSplitOptions.TrimEntries" /> directly instead.</remarks>
	public static string[] SplitAndTrim(string str, string separator, StringSplitOptions options = StringSplitOptions.None)
	{
		return str.Split(separator, options | StringSplitOptions.TrimEntries);
	}
}
