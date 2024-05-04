using System;
using System.Collections.Generic;

namespace StardewValley;

/// <summary>An implementation of <see cref="T:StardewValley.SerializableDictionary`2" /> that has case-insensitive keys.</summary>
/// <typeparam name="TValue">The value type.</typeparam>
/// <remarks>This avoids a limitation with <see cref="T:StardewValley.SerializableDictionary`2" /> where any custom comparer is lost on deserialization.</remarks>
public class SerializableDictionaryWithCaseInsensitiveKeys<TValue> : SerializableDictionary<string, TValue>
{
	/// <summary>Construct an empty instance.</summary>
	public SerializableDictionaryWithCaseInsensitiveKeys()
		: base((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase)
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="data">The data to copy.</param>
	public SerializableDictionaryWithCaseInsensitiveKeys(IDictionary<string, TValue> data)
		: base(data, (IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase)
	{
	}
}
