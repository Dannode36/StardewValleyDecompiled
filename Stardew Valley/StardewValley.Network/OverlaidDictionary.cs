using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network;

/// <summary>A hybrid synchronized/unsynchronized dictionary where values either come from a synchronized NetDictionary, or from a locally maintained overlay dictionary.</summary>
public class OverlaidDictionary : IEnumerable<SerializableDictionary<Vector2, Object>>, IEnumerable
{
	private NetVector2Dictionary<Object, NetRef<Object>> baseDict;

	private OverlayDictionary<Vector2, Object> overlayDict;

	private Dictionary<Vector2, Object> compositeDict;

	private bool _locked;

	private Dictionary<Vector2, Object> _changes = new Dictionary<Vector2, Object>();

	/// <summary>The number of key/value pairs in the dictionary.</summary>
	public int Length => compositeDict.Count;

	public Object this[Vector2 key]
	{
		get
		{
			if (overlayDict.TryGetValue(key, out var overlaid))
			{
				return overlaid;
			}
			if (_locked && _changes.TryGetValue(key, out var o))
			{
				if (o == null)
				{
					throw new KeyNotFoundException();
				}
				return o;
			}
			return baseDict[key];
		}
		set
		{
			if (_locked)
			{
				_changes[key] = value;
			}
			else
			{
				baseDict[key] = value;
			}
		}
	}

	public Dictionary<Vector2, Object>.KeyCollection Keys => compositeDict.Keys;

	public Dictionary<Vector2, Object>.ValueCollection Values => compositeDict.Values;

	public IEnumerable<KeyValuePair<Vector2, Object>> Pairs => compositeDict;

	public void OnValueAdded(Vector2 key, Object value)
	{
		if (overlayDict.TryGetValue(key, out var o))
		{
			compositeDict[key] = o;
		}
		else if (baseDict.TryGetValue(key, out o))
		{
			compositeDict[key] = o;
		}
	}

	public void OnValueRemoved(Vector2 key, Object value)
	{
		if (overlayDict.TryGetValue(key, out var o))
		{
			compositeDict[key] = o;
		}
		else if (baseDict.TryGetValue(key, out o))
		{
			compositeDict[key] = o;
		}
		else
		{
			compositeDict.Remove(key);
		}
	}

	public void SetEqualityComparer(IEqualityComparer<Vector2> comparer, ref NetVector2Dictionary<Object, NetRef<Object>> base_dict, ref OverlayDictionary<Vector2, Object> overlay_dict)
	{
		baseDict.SetEqualityComparer(comparer);
		overlayDict.onValueAdded -= OnValueAdded;
		overlayDict.onValueRemoved -= OnValueRemoved;
		overlayDict = new OverlayDictionary<Vector2, Object>(overlayDict, comparer);
		compositeDict = new Dictionary<Vector2, Object>(compositeDict, comparer);
		overlayDict.onValueAdded += OnValueAdded;
		overlayDict.onValueRemoved += OnValueRemoved;
		overlayDict.onValueAdded += OnValueAdded;
		overlayDict.onValueRemoved += OnValueRemoved;
		base_dict = baseDict;
		overlay_dict = overlayDict;
	}

	public OverlaidDictionary(NetVector2Dictionary<Object, NetRef<Object>> baseDict, OverlayDictionary<Vector2, Object> overlayDict)
	{
		this.baseDict = baseDict;
		this.overlayDict = overlayDict;
		compositeDict = new Dictionary<Vector2, Object>();
		foreach (KeyValuePair<Vector2, Object> pair in overlayDict)
		{
			OnValueAdded(pair.Key, pair.Value);
		}
		foreach (KeyValuePair<Vector2, Object> pair in baseDict.Pairs)
		{
			OnValueAdded(pair.Key, pair.Value);
		}
		baseDict.OnValueAdded += OnValueAdded;
		baseDict.OnConflictResolve += delegate(Vector2 key, NetRef<Object> rejected, NetRef<Object> accepted)
		{
			OnValueRemoved(key, rejected.Value);
			OnValueAdded(key, accepted.Value);
		};
		baseDict.OnValueRemoved += OnValueRemoved;
	}

	public bool Any()
	{
		return compositeDict.Count > 0;
	}

	public int Count()
	{
		return compositeDict.Count;
	}

	/// <summary>Freeze the object list, so changes will be queued until <see cref="M:StardewValley.Network.OverlaidDictionary.Unlock" /> is called.</summary>
	public void Lock()
	{
		_locked = true;
	}

	/// <summary>Remove the freeze added by <see cref="M:StardewValley.Network.OverlaidDictionary.Lock" /> and apply all changes that were queued while it was locked.</summary>
	public void Unlock()
	{
		if (!_locked)
		{
			return;
		}
		_locked = false;
		if (_changes.Count <= 0)
		{
			return;
		}
		foreach (KeyValuePair<Vector2, Object> kvp in _changes)
		{
			if (kvp.Value != null)
			{
				baseDict[kvp.Key] = kvp.Value;
			}
			else
			{
				baseDict.Remove(kvp.Key);
			}
		}
		_changes.Clear();
	}

	/// <summary>Add an object to the dictionary.</summary>
	/// <param name="key">The tile position.</param>
	/// <param name="value">The object instance.</param>
	/// <exception cref="T:System.ArgumentException">The key is already present in the dictionary.</exception>
	public void Add(Vector2 key, Object value)
	{
		if (_locked)
		{
			if (_changes.TryGetValue(key, out var existingValue))
			{
				if (existingValue != null)
				{
					throw new ArgumentException();
				}
				_changes[key] = value;
			}
			else
			{
				if (baseDict.ContainsKey(key))
				{
					throw new ArgumentException();
				}
				_changes[key] = value;
			}
		}
		else
		{
			baseDict.Add(key, value);
		}
	}

	/// <summary>Add an object to the dictionary if the key isn't already present.</summary>
	/// <param name="key">The tile position.</param>
	/// <param name="value">The object instance.</param>
	/// <returns>Returns whether the object was successfully added.</returns>
	public bool TryAdd(Vector2 key, Object value)
	{
		if (ContainsKey(key))
		{
			return false;
		}
		Add(key, value);
		return true;
	}

	public void Clear()
	{
		if (_locked)
		{
			throw new NotImplementedException();
		}
		baseDict.Clear();
		overlayDict.Clear();
		compositeDict.Clear();
	}

	public bool ContainsKey(Vector2 key)
	{
		if (_locked && _changes.TryGetValue(key, out var value))
		{
			return value != null;
		}
		return compositeDict.ContainsKey(key);
	}

	public bool Remove(Vector2 key)
	{
		if (overlayDict.Remove(key))
		{
			return true;
		}
		if (_locked)
		{
			if (_changes.TryGetValue(key, out var value))
			{
				_changes[key] = null;
				return value != null;
			}
			if (baseDict.ContainsKey(key))
			{
				_changes[key] = null;
				return true;
			}
			return false;
		}
		return baseDict.Remove(key);
	}

	public bool TryGetValue(Vector2 key, out Object value)
	{
		return compositeDict.TryGetValue(key, out value);
	}

	public IEnumerator<SerializableDictionary<Vector2, Object>> GetEnumerator()
	{
		return baseDict.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return baseDict.GetEnumerator();
	}

	public void Add(SerializableDictionary<Vector2, Object> dict)
	{
		foreach (KeyValuePair<Vector2, Object> pair in dict)
		{
			if (pair.Value != null)
			{
				Add(pair.Key, pair.Value);
			}
		}
	}
}
