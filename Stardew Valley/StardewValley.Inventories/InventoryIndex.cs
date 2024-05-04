using System;
using System.Collections.Generic;

namespace StardewValley.Inventories;

/// <summary>Manages a lookup of items in an inventory by key.</summary>
public class InventoryIndex
{
	/// <summary>A cache of inventory items by key.</summary>
	private readonly Dictionary<string, List<Item>> Index = new Dictionary<string, List<Item>>();

	/// <summary>Adds an item to the index by key.</summary>
	private readonly Action<InventoryIndex, Item> AddImpl;

	/// <summary>Removes an item from the index by key.</summary>
	private readonly Action<InventoryIndex, Item> RemoveImpl;

	/// <summary>Construct an instance.</summary>
	/// <param name="addImpl">Adds an item to the index by key.</param>
	/// <param name="removeImpl">Removes an item from the index by key.</param>
	public InventoryIndex(Action<InventoryIndex, Item> addImpl, Action<InventoryIndex, Item> removeImpl)
	{
		AddImpl = addImpl;
		RemoveImpl = removeImpl;
	}

	/// <summary>Construct an index which caches items by their qualified ID.</summary>
	/// <param name="items">The items to index.</param>
	public static InventoryIndex ById(IList<Item> items)
	{
		InventoryIndex instance = new InventoryIndex(delegate(InventoryIndex index, Item item)
		{
			index.AddWithKey(item.QualifiedItemId, item);
		}, delegate(InventoryIndex index, Item item)
		{
			index.RemoveItem(item.QualifiedItemId, item);
		});
		foreach (Item item in items)
		{
			instance.Add(item);
		}
		return instance;
	}

	/// <summary>The number of unique keys used to index items.</summary>
	public int CountKeys()
	{
		return Index.Count;
	}

	/// <summary>The number of items in the inventory.</summary>
	public int CountItems()
	{
		int count = 0;
		foreach (List<Item> list in Index.Values)
		{
			count += list.Count;
		}
		return count;
	}

	/// <summary>Get whether any items match a given key.</summary>
	/// <param name="key">The index key.</param>
	public bool Contains(string key)
	{
		if (key != null)
		{
			return Index.ContainsKey(key);
		}
		return false;
	}

	/// <summary>Get a read-only list of items which match a given key, if any.</summary>
	/// <param name="key">The index key.</param>
	/// <param name="items">The matching items.</param>
	public bool TryGet(string key, out IReadOnlyList<Item> items)
	{
		if (key != null && Index.TryGetValue(key, out var indexed))
		{
			items = indexed;
			return true;
		}
		items = null;
		return false;
	}

	/// <summary>Get an editable list of items which match a given key, if any.</summary>
	/// <param name="key">The index key.</param>
	/// <param name="items">The matching items.</param>
	/// <remarks>Most code should use <see cref="M:StardewValley.Inventories.InventoryIndex.TryGet(System.String,System.Collections.Generic.IReadOnlyList{StardewValley.Item}@)" /> instead. Changes to the list will only affect the index, not the underlying inventory. This method is only provided for cases where you're directly changing both at once. If you clear the list, make sure to call <see cref="M:StardewValley.Inventories.InventoryIndex.RemoveKey(System.String)" /> too.</remarks>
	public bool TryGetMutable(string key, out IList<Item> items)
	{
		if (key != null && Index.TryGetValue(key, out var indexed))
		{
			items = indexed;
			return true;
		}
		items = null;
		return false;
	}

	/// <summary>Add an item to the index.</summary>
	/// <param name="item">The item to add.</param>
	public void Add(Item item)
	{
		if (item != null)
		{
			AddImpl(this, item);
		}
	}

	/// <summary>Add an item to the index.</summary>
	/// <param name="key">The key to index by.</param>
	/// <param name="item">The item to add.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="key" /> is null.</exception>
	public void AddWithKey(string key, Item item)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (item != null)
		{
			if (!Index.TryGetValue(key, out var list))
			{
				list = (Index[key] = new List<Item>());
			}
			list.Add(item);
		}
	}

	/// <summary>Remove an item from the index.</summary>
	/// <param name="item">The item to remove.</param>
	public void Remove(Item item)
	{
		if (item != null)
		{
			RemoveImpl(this, item);
		}
	}

	/// <summary>Remove a key from the index.</summary>
	/// <param name="key">The key to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="key" /> is null.</exception>
	public void RemoveKey(string key)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		Index.Remove(key);
	}

	/// <summary>Remove an item from the index.</summary>
	/// <param name="key">The key for which to remove the item.</param>
	/// <param name="item">The item to remove.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="key" /> is null.</exception>
	public void RemoveItem(string key, Item item)
	{
		if (key == null)
		{
			throw new ArgumentNullException("key");
		}
		if (item != null && Index.TryGetValue(key, out var list))
		{
			list.Remove(item);
			if (list.Count == 0)
			{
				Index.Remove(key);
			}
		}
	}
}
