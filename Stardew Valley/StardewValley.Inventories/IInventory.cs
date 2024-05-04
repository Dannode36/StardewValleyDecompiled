using System.Collections;
using System.Collections.Generic;

namespace StardewValley.Inventories;

/// <summary>A managed list of items.</summary>
public interface IInventory : IList<Item>, ICollection<Item>, IEnumerable<Item>, IEnumerable
{
	/// <summary>The <see cref="P:System.DateTime.UtcNow" /> <see cref="P:System.DateTime.Ticks" /> value when an item stack was last added or removed in this inventory.</summary>
	/// <remarks>This doesn't track changes to the data for each stack (including the stack size).</remarks>
	long LastTickSlotChanged { get; }

	/// <summary>Get whether the inventory contains any items (excluding <c>null</c> slots).</summary>
	bool HasAny();

	/// <summary>Get whether this inventory contains any <c>null</c> slots.</summary>
	bool HasEmptySlots();

	/// <summary>Get the number of item stacks in the inventory, excluding <c>null</c> slots.</summary>
	int CountItemStacks();

	/// <summary>Clear all item slots and overwrite them with the slots in the given list.</summary>
	/// <param name="list">The item list from which to copy values.</param>
	void OverwriteWith(IList<Item> list);

	/// <summary>Get a subset of the item slots in the inventory.</summary>
	/// <param name="index">The index from which to start the range.</param>
	/// <param name="count">The number of items to include.</param>
	IList<Item> GetRange(int index, int count);

	/// <summary>Add a list of items to the inventory.</summary>
	/// <param name="collection">The items to add.</param>
	void AddRange(ICollection<Item> collection);

	/// <summary>Remove a subset of items from the inventory.</summary>
	/// <param name="index">The index from which to start removing items.</param>
	/// <param name="count">The number of items to remove.</param>
	void RemoveRange(int index, int count);

	/// <summary>Remove all empty slots from the inventory.</summary>
	void RemoveEmptySlots();

	/// <summary>Get whether the inventory contains any items with the given item ID.</summary>
	/// <param name="itemId">The qualified or unqualified item ID to find.</param>
	bool ContainsId(string itemId);

	/// <summary>Get whether the inventory contains a minimum number of items with the given item ID.</summary>
	/// <param name="itemId">The qualified or unqualified item ID to find.</param>
	/// <param name="minimum">The minimum number of the item to require, totaling the <see cref="P:StardewValley.Item.Stack" /> values for all items with the given <paramref name="itemId" />.</param>
	bool ContainsId(string itemId, int minimum);

	/// <summary>Get a count of items with the given item ID.</summary>
	/// <param name="itemId">The qualified or unqualified item ID to find.</param>
	int CountId(string itemId);

	/// <summary>Get all items with the given item ID.</summary>
	/// <param name="itemId">The qualified or unqualified item ID to find.</param>
	IEnumerable<Item> GetById(string itemId);

	/// <summary>Remove the specified number of the item ID from the inventory. This reduces the stack size for matching items until a total of <paramref name="count" /> have been removed, and clears any slot which reaches a stack size of zero.</summary>
	/// <param name="itemId">The qualified or unqualified item ID to remove.</param>
	/// <param name="count">The number of the item to remove.</param>
	/// <returns>Returns the amount by which the items were reduced. If the inventory has sufficient items, this will match <paramref name="count" />.</returns>
	int ReduceId(string itemId, int count);

	/// <summary>Set the slot containing the given item to null, without removing the empty slot.</summary>
	/// <param name="item">The item to remove.</param>
	bool RemoveButKeepEmptySlot(Item item);
}
