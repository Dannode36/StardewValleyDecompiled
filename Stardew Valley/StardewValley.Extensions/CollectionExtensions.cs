using System;
using System.Collections.Generic;
using Netcode;

namespace StardewValley.Extensions;

/// <summary>Provides utility extension methods on .NET collection types.</summary>
public static class CollectionExtensions
{
	/// <summary>Remove all elements that match a condition.</summary>
	/// <param name="dictionary">The dictionary to update.</param>
	/// <param name="match">The predicate matching values to remove.</param>
	/// <returns>Returns the number of entries removed.</returns>
	public static int RemoveWhere<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Func<KeyValuePair<TKey, TValue>, bool> match)
	{
		if (dictionary.Count == 0)
		{
			return 0;
		}
		int removed = 0;
		foreach (KeyValuePair<TKey, TValue> pair in dictionary)
		{
			if (match(pair))
			{
				dictionary.Remove(pair.Key);
				removed++;
			}
		}
		return removed;
	}

	/// <summary>Add or remove value to the set.</summary>
	/// <param name="set">The set to update.</param>
	/// <param name="value">The value to add or remove.</param>
	/// <param name="add">Whether to add the value; else it's removed.</param>
	public static void Toggle<T>(this ISet<T> set, T value, bool add)
	{
		if (add)
		{
			set.Add(value);
		}
		else
		{
			set.Remove(value);
		}
	}

	/// <summary>Add a list of values to the set.</summary>
	/// <param name="set">The set to update.</param>
	/// <param name="values">The values to add to the set.</param>
	/// <returns>Returns the number of values added to the set.</returns>
	public static int AddRange<T>(this ISet<T> set, IEnumerable<T> values)
	{
		int added = 0;
		foreach (T value in values)
		{
			if (set.Add(value))
			{
				added++;
			}
		}
		return added;
	}

	/// <summary>Remove all elements that match a condition.</summary>
	/// <param name="set">The set to update.</param>
	/// <param name="match">The predicate matching values to remove.</param>
	/// <returns>Returns the number of values removed from the set.</returns>
	public static int RemoveWhere<T>(this ISet<T> set, Predicate<T> match)
	{
		if (!(set is HashSet<T> hashSet))
		{
			if (set is NetHashSet<T> netHashSet)
			{
				return netHashSet.RemoveWhere(match);
			}
			List<T> removed = null;
			foreach (T value in set)
			{
				if (match(value))
				{
					if (removed == null)
					{
						removed = new List<T>();
					}
					removed.Add(value);
				}
			}
			if (removed != null)
			{
				foreach (T value in removed)
				{
					set.Remove(value);
				}
				return removed.Count;
			}
			return 0;
		}
		return hashSet.RemoveWhere(match);
	}
}
