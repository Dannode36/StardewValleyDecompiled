using System;
using System.Collections;
using System.Collections.Generic;

namespace StardewValley.Network;

public class OverlayDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ICollection<KeyValuePair<TKey, TValue>>, IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
{
	protected Dictionary<TKey, TValue> _dictionary;

	protected List<KeyValuePair<TKey, TValue>> _removedPairs = new List<KeyValuePair<TKey, TValue>>();

	public TValue this[TKey key]
	{
		get
		{
			return _dictionary[key];
		}
		set
		{
			_dictionary[key] = value;
			this.onValueAdded?.Invoke(key, value);
		}
	}

	public ICollection<TKey> Keys => _dictionary.Keys;

	public ICollection<TValue> Values => _dictionary.Values;

	public int Count => _dictionary.Count;

	public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).IsReadOnly;

	public event Action<TKey, TValue> onValueAdded;

	public event Action<TKey, TValue> onValueRemoved;

	public OverlayDictionary()
	{
		_dictionary = new Dictionary<TKey, TValue>();
	}

	public OverlayDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
	{
		_dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);
	}

	public OverlayDictionary(IEqualityComparer<TKey> comparer)
	{
		_dictionary = new Dictionary<TKey, TValue>(comparer);
	}

	public void Add(TKey key, TValue value)
	{
		_dictionary.Add(key, value);
		this.onValueAdded?.Invoke(key, value);
	}

	public void Add(KeyValuePair<TKey, TValue> item)
	{
		Add(item.Key, item.Value);
	}

	public void Clear()
	{
		_removedPairs.AddRange(_dictionary);
		((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Clear();
		foreach (KeyValuePair<TKey, TValue> pair in _removedPairs)
		{
			this.onValueRemoved(pair.Key, pair.Value);
		}
		_removedPairs.Clear();
	}

	public bool Contains(KeyValuePair<TKey, TValue> item)
	{
		return ((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).Contains(item);
	}

	public bool ContainsKey(TKey key)
	{
		return _dictionary.ContainsKey(key);
	}

	public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
	{
		((ICollection<KeyValuePair<TKey, TValue>>)_dictionary).CopyTo(array, arrayIndex);
	}

	public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
	{
		return _dictionary.GetEnumerator();
	}

	public bool Remove(TKey key)
	{
		if (_dictionary.TryGetValue(key, out var value))
		{
			_dictionary.Remove(key);
			this.onValueRemoved?.Invoke(key, value);
			return true;
		}
		return false;
	}

	public bool Remove(KeyValuePair<TKey, TValue> item)
	{
		if (Contains(item))
		{
			return Remove(item.Key);
		}
		return false;
	}

	public bool TryGetValue(TKey key, out TValue value)
	{
		return _dictionary.TryGetValue(key, out value);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return _dictionary.GetEnumerator();
	}
}
