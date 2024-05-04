using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Inventories;

/// <summary>A managed list of items.</summary>
[XmlRoot("items")]
public class Inventory : INetObject<NetFields>, IXmlSerializable, IInventory, IList<Item>, ICollection<Item>, IEnumerable<Item>, IEnumerable
{
	/// <summary>The underlying list of items.</summary>
	private readonly NetObjectList<Item> Items = new NetObjectList<Item>();

	/// <summary>The <see cref="F:StardewValley.Inventories.Inventory.Items" /> indexed by their qualified item ID.</summary>
	private InventoryIndex ItemsById;

	/// <summary>The backing field for <see cref="M:StardewValley.Inventories.Inventory.CountItemStacks" />.</summary>
	private int? CachedItemStackCount;

	/// <inheritdoc />
	public NetFields NetFields { get; } = new NetFields("Inventory");


	/// <summary>The number of items in the inventory, including <c>null</c> slots.</summary>
	public int Count => Items.Count;

	/// <inheritdoc />
	public bool IsReadOnly => Items.IsReadOnly;

	/// <inheritdoc />
	public Item this[int index]
	{
		get
		{
			return Items[index];
		}
		set
		{
			Items[index] = value;
		}
	}

	/// <inheritdoc />
	public long LastTickSlotChanged { get; private set; }

	/// <summary>An event raised when an item stack is added or removed.</summary>
	public event OnSlotChangedDelegate OnSlotChanged;

	/// <summary>An event raised when the inventory is cleared or replaced.</summary>
	public event OnInventoryReplacedDelegate OnInventoryReplaced;

	/// <summary>Construct an instance.</summary>
	public Inventory()
	{
		NetFields.SetOwner(this).AddField(Items, "this.Items");
		Items.OnElementChanged += HandleElementChanged;
		Items.OnArrayReplaced += HandleArrayReplaced;
	}

	/// <inheritdoc />
	public bool HasAny()
	{
		return GetItemsById().CountKeys() > 0;
	}

	/// <inheritdoc />
	public bool HasEmptySlots()
	{
		return Count > CountItemStacks();
	}

	/// <inheritdoc />
	public int CountItemStacks()
	{
		int? cachedItemStackCount = CachedItemStackCount;
		if (!cachedItemStackCount.HasValue)
		{
			int? num = (CachedItemStackCount = GetItemsById().CountItems());
			return num.Value;
		}
		return cachedItemStackCount.GetValueOrDefault();
	}

	/// <inheritdoc />
	public void OverwriteWith(IList<Item> list)
	{
		if (this != list && Items != list)
		{
			ClearIndex();
			Items.CopyFrom(list);
		}
	}

	/// <inheritdoc />
	public IList<Item> GetRange(int index, int count)
	{
		return Items.GetRange(index, count);
	}

	/// <inheritdoc />
	public void AddRange(ICollection<Item> collection)
	{
		Items.AddRange(collection);
	}

	/// <inheritdoc />
	public void RemoveRange(int index, int count)
	{
		Items.RemoveRange(index, count);
	}

	/// <inheritdoc />
	public void RemoveEmptySlots()
	{
		if (!HasEmptySlots())
		{
			return;
		}
		for (int i = Count - 1; i >= 0; i--)
		{
			if (this[i] == null)
			{
				RemoveAt(i);
			}
		}
	}

	/// <inheritdoc />
	public bool ContainsId(string itemId)
	{
		itemId = ItemRegistry.QualifyItemId(itemId);
		if (itemId == null)
		{
			return false;
		}
		return GetItemsById().Contains(itemId);
	}

	/// <inheritdoc />
	public bool ContainsId(string itemId, int minimum)
	{
		itemId = ItemRegistry.QualifyItemId(itemId);
		if (itemId == null)
		{
			return false;
		}
		if (GetItemsById().TryGet(itemId, out var items))
		{
			if (minimum <= 1)
			{
				return true;
			}
			int count = 0;
			foreach (Item item in items)
			{
				if (item.QualifiedItemId == itemId)
				{
					count += item.Stack;
				}
				if (count >= minimum)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <inheritdoc />
	public int CountId(string itemId)
	{
		itemId = ItemRegistry.QualifyItemId(itemId);
		if (itemId == null)
		{
			return 0;
		}
		if (GetItemsById().TryGet(itemId, out var items))
		{
			int count = 0;
			{
				foreach (Item item in items)
				{
					if (item.QualifiedItemId == itemId)
					{
						count += item.Stack;
					}
				}
				return count;
			}
		}
		return 0;
	}

	/// <inheritdoc />
	public IEnumerable<Item> GetById(string itemId)
	{
		itemId = ItemRegistry.QualifyItemId(itemId);
		if (itemId == null || !GetItemsById().TryGet(itemId, out var items))
		{
			return LegacyShims.EmptyArray<Item>();
		}
		return items;
	}

	/// <inheritdoc />
	public int ReduceId(string itemId, int count)
	{
		itemId = ItemRegistry.QualifyItemId(itemId);
		if (itemId == null)
		{
			return 0;
		}
		InventoryIndex itemsById = GetItemsById();
		if (itemsById.TryGetMutable(itemId, out var items))
		{
			bool anyStacksRemoved = false;
			int remaining = count;
			for (int i = 0; i < items.Count; i++)
			{
				if (remaining <= 0)
				{
					break;
				}
				Item item = items[i];
				int toRemove = Math.Min(remaining, item.Stack);
				items[i] = item.ConsumeStack(toRemove);
				if (items[i] == null)
				{
					anyStacksRemoved = true;
					item.Stack = 0;
					items.RemoveAt(i);
					i--;
				}
				remaining -= toRemove;
			}
			if (items.Count == 0)
			{
				itemsById.RemoveKey(itemId);
			}
			if (anyStacksRemoved)
			{
				for (int i = Items.Count - 1; i >= 0; i--)
				{
					Item item2 = Items[i];
					if (item2 != null && item2.Stack == 0)
					{
						Items[i] = null;
					}
				}
			}
			return count - remaining;
		}
		return 0;
	}

	/// <inheritdoc />
	public bool RemoveButKeepEmptySlot(Item item)
	{
		if (item == null)
		{
			return false;
		}
		int index = Items.IndexOf(item);
		if (index == -1)
		{
			return false;
		}
		Items[index] = null;
		return true;
	}

	/// <inheritdoc />
	public IEnumerator<Item> GetEnumerator()
	{
		return Items.GetEnumerator();
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return Items.GetEnumerator();
	}

	/// <inheritdoc />
	public void Add(Item item)
	{
		Items.Add(item);
	}

	/// <inheritdoc />
	public void Clear()
	{
		ClearIndex();
		Items.Clear();
	}

	/// <inheritdoc />
	public bool Contains(Item item)
	{
		if (item == null)
		{
			return false;
		}
		if (GetItemsById().TryGetMutable(item.QualifiedItemId, out var list))
		{
			return list.Contains(item);
		}
		return false;
	}

	/// <inheritdoc />
	public void CopyTo(Item[] array, int arrayIndex)
	{
		Items.CopyTo(array, arrayIndex);
	}

	/// <inheritdoc />
	public bool Remove(Item item)
	{
		if (item == null)
		{
			return false;
		}
		return Items.Remove(item);
	}

	/// <inheritdoc />
	public int IndexOf(Item item)
	{
		return Items.IndexOf(item);
	}

	/// <inheritdoc />
	public void Insert(int index, Item item)
	{
		Items.Insert(index, item);
	}

	/// <inheritdoc />
	public void RemoveAt(int index)
	{
		Items.RemoveAt(index);
	}

	/// <inheritdoc />
	public XmlSchema GetSchema()
	{
		return null;
	}

	/// <inheritdoc />
	public void ReadXml(XmlReader reader)
	{
		if (reader.IsEmptyElement)
		{
			reader.Read();
			return;
		}
		reader.Read();
		XmlSerializer itemSerializer = SaveGame.GetSerializer(typeof(Item));
		while (reader.NodeType != XmlNodeType.EndElement)
		{
			Item item = (Item)itemSerializer.Deserialize(reader);
			Items.Add(item);
		}
		reader.ReadEndElement();
	}

	/// <inheritdoc />
	public void WriteXml(XmlWriter writer)
	{
		XmlSerializer itemSerializer = SaveGame.GetSerializer(typeof(Item));
		foreach (Item item in Items)
		{
			itemSerializer.Serialize(writer, item);
		}
	}

	/// <summary>Get an index of items by ID.</summary>
	private InventoryIndex GetItemsById()
	{
		return ItemsById ?? (ItemsById = InventoryIndex.ById(Items));
	}

	/// <summary>Handle the <see cref="F:StardewValley.Inventories.Inventory.Items" /> data getting replaced.</summary>
	/// <param name="list">The item list.</param>
	/// <param name="before">The previous item list.</param>
	/// <param name="after">The new item list.</param>
	private void HandleArrayReplaced(NetList<Item, NetRef<Item>> list, IList<Item> before, IList<Item> after)
	{
		if (before.Count != 0 || after.Count != 0)
		{
			ClearIndex();
			CachedItemStackCount = null;
			LastTickSlotChanged = DateTime.UtcNow.Ticks;
			this.OnInventoryReplaced?.Invoke(this, before, after);
		}
	}

	/// <summary>Handle a slot in the <see cref="F:StardewValley.Inventories.Inventory.Items" /> data changing.</summary>
	/// <param name="list">The item list.</param>
	/// <param name="index">The item slot's index within the inventory.</param>
	/// <param name="before">The previous item value (which may be <c>null</c> when adding a stack).</param>
	/// <param name="after">The new item value (which may be <c>null</c> when removing a stack).</param>
	private void HandleElementChanged(NetList<Item, NetRef<Item>> list, int index, Item before, Item after)
	{
		if (before != after)
		{
			ItemsById?.Remove(before);
			ItemsById?.Add(after);
			CachedItemStackCount = null;
			LastTickSlotChanged = DateTime.UtcNow.Ticks;
			this.OnSlotChanged?.Invoke(this, index, before, after);
		}
	}

	/// <summary>Clear the item index, so it'll be rebuilt next time it's needed.</summary>
	private void ClearIndex()
	{
		ItemsById = null;
	}
}
