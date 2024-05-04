using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.SpecialOrders;

namespace StardewValley.Internal;

/// <summary>Iterates through every item in the game state and optionally edits, replaces, or removes instances.</summary>
/// <remarks>This is a low-level class. Most code should use a utility method like <see cref="M:StardewValley.Utility.ForEachItem(System.Func{StardewValley.Item,System.Boolean})" /> or <see cref="M:StardewValley.Utility.ForEachItem(StardewValley.Delegates.ForEachItemDelegate)" /> instead.</remarks>
public static class ForEachItemHelper
{
	/// <summary>Perform an action for each item in the game world, including items within items (e.g. in a chest or on a table), hats placed on children, items in player inventories, etc.</summary>
	/// <param name="handler">The action to perform for each item.</param>
	/// <returns>Returns whether to continue iterating if needed (i.e. returns false if the last <paramref name="handler" /> call did).</returns>
	public static bool ForEachItemInWorld(ForEachItemDelegate handler)
	{
		bool canContinue = true;
		Utility.ForEachLocation((GameLocation location) => canContinue = ForEachItemInLocation(location, handler));
		if (!canContinue)
		{
			return false;
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			Farmer farmer = allFarmer;
			int toolIndex = farmer.CurrentToolIndex;
			if (!ApplyToList(farmer.Items, handler, leaveNullSlotsOnRemoval: true, OnChangedItemSlot) || !ApplyToField(farmer.shirtItem, handler, OnChangedEquipment) || !ApplyToField(farmer.pantsItem, handler, OnChangedEquipment) || !ApplyToField(farmer.boots, handler, OnChangedEquipment) || !ApplyToField(farmer.hat, handler, OnChangedEquipment) || !ApplyToField(farmer.leftRing, handler, OnChangedEquipment) || !ApplyToField(farmer.rightRing, handler, OnChangedEquipment) || !ApplyToItem(farmer.recoveredItem, handler, delegate
			{
				farmer.recoveredItem = null;
			}, delegate(Item newItem)
			{
				farmer.recoveredItem = PrepareForReplaceWith(farmer.recoveredItem, newItem);
			}) || !ApplyToField(farmer.toolBeingUpgraded, handler) || !ApplyToList(farmer.itemsLostLastDeath, handler))
			{
				return false;
			}
			void OnChangedEquipment(Item oldItem, Item newItem)
			{
				oldItem?.onUnequip(farmer);
				newItem?.onEquip(farmer);
			}
			void OnChangedItemSlot(Item oldItem, Item newItem, int index)
			{
				if (index == toolIndex)
				{
					(oldItem as Tool)?.onUnequip(farmer);
					(newItem as Tool)?.onEquip(farmer);
				}
			}
		}
		if (!ApplyToList(Game1.player.team.returnedDonations, handler))
		{
			return false;
		}
		foreach (Inventory value in Game1.player.team.globalInventories.Values)
		{
			if (!ApplyToList(value, handler))
			{
				return false;
			}
		}
		foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
		{
			if (!ApplyToList(specialOrder.donatedItems, handler))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Perform an action for each item within a location, including items within items (e.g. in a chest or on a table), hats placed on children, items in player inventories, etc.</summary>
	/// <param name="location">The location whose items to iterate.</param>
	/// <param name="handler">The action to perform for each item.</param>
	/// <returns>Returns whether to continue iterating if needed (i.e. returns false if the last <paramref name="handler" /> call did).</returns>
	public static bool ForEachItemInLocation(GameLocation location, ForEachItemDelegate handler)
	{
		if (location == null)
		{
			return true;
		}
		if (!ApplyToList(location.furniture, handler))
		{
			return false;
		}
		foreach (NPC character in location.characters)
		{
			if (!(character is Child child))
			{
				if (!(character is Horse horse))
				{
					if (character is Pet pet && !ApplyToField(pet.hat, handler))
					{
						return false;
					}
				}
				else if (!ApplyToField(horse.hat, handler))
				{
					return false;
				}
			}
			else if (!ApplyToField(child.hat, handler))
			{
				return false;
			}
		}
		foreach (Building building in location.buildings)
		{
			if (!building.ForEachItemExcludingInterior(handler))
			{
				return false;
			}
		}
		if ((!(location.GetFridge(onlyUnlocked: false)?.ForEachItem(handler))) ?? false)
		{
			return false;
		}
		if (location.objects.Length > 0)
		{
			foreach (Vector2 tile in location.objects.Keys)
			{
				if (!ApplyToItem(location.objects[tile], handler, delegate
				{
					location.objects.Remove(tile);
				}, delegate(Item newItem)
				{
					location.objects[tile] = PrepareForReplaceWith(location.objects[tile], (Object)newItem);
				}))
				{
					return false;
				}
			}
		}
		for (int i = location.debris.Count - 1; i >= 0; i--)
		{
			Debris d = location.debris[i];
			if (d.item != null && !ApplyToItem(d.item, handler, Remove, ReplaceWith))
			{
				return false;
			}
			void Remove()
			{
				if (d.itemId.Value == null || ItemRegistry.HasItemId(d.item, d.itemId.Value))
				{
					location.debris.RemoveAt(i);
				}
				else
				{
					d.item = null;
				}
			}
			void ReplaceWith(Item newItem)
			{
				if (ItemRegistry.HasItemId(newItem, d.itemId.Value))
				{
					d.itemId.Value = newItem.QualifiedItemId;
				}
				d.item = PrepareForReplaceWith(d.item, newItem);
			}
		}
		if (location is ShopLocation shopLocation)
		{
			if (!ApplyToList(shopLocation.itemsFromPlayerToSell, handler))
			{
				return false;
			}
			if (!ApplyToList(shopLocation.itemsToStartSellingTomorrow, handler))
			{
				return false;
			}
		}
		return true;
	}

	/// <summary>Apply a for-each-item callback to an item.</summary>
	/// <typeparam name="TItem">The item type.</typeparam>
	/// <param name="item">The item instance to iterate.</param>
	/// <param name="handler">The action to perform for each item.</param>
	/// <param name="remove">Delete this item instance.</param>
	/// <param name="replaceWith">Replace this item with a new instance.</param>
	/// <returns>Returns whether to continue iterating if needed.</returns>
	public static bool ApplyToItem<TItem>(TItem item, ForEachItemDelegate handler, Action remove, Action<Item> replaceWith) where TItem : Item
	{
		if (item == null)
		{
			return true;
		}
		if (handler(item, Remove, ReplaceWith))
		{
			return item?.ForEachItem(handler) ?? true;
		}
		return false;
		void Remove()
		{
			remove();
			item = null;
		}
		void ReplaceWith(Item newItem)
		{
			if (newItem == null)
			{
				Remove();
			}
			else
			{
				item = PrepareForReplaceWith(item, (TItem)newItem);
				replaceWith(item);
			}
		}
	}

	/// <summary>Apply a for-each-item callback to an item.</summary>
	/// <typeparam name="TItem">The item type.</typeparam>
	/// <param name="field">The field instance to iterate.</param>
	/// <param name="handler">The action to perform for each item.</param>
	/// <param name="onChanged">A callback to invoke when the assigned value changes, which receives the old and new items.</param>
	/// <returns>Returns whether to continue iterating if needed.</returns>
	public static bool ApplyToField<TItem>(NetRef<TItem> field, ForEachItemDelegate handler, Action<Item, Item> onChanged = null) where TItem : Item
	{
		Item oldValue = field.Value;
		return ApplyToItem(field.Value, handler, delegate
		{
			field.Value = null;
			onChanged?.Invoke(oldValue, null);
		}, delegate(Item newItem)
		{
			field.Value = PrepareForReplaceWith(field.Value, (TItem)newItem);
			onChanged?.Invoke(oldValue, newItem);
		});
	}

	/// <summary>Apply a for-each-item callback to an item.</summary>
	/// <typeparam name="TItem">The item type.</typeparam>
	/// <param name="list">The list of items to iterate.</param>
	/// <param name="handler">The action to perform for each item.</param>
	/// <param name="leaveNullSlotsOnRemoval">Whether to leave a null entry in the list when an item is removed. If <c>false</c>, the index is removed from the list instead.</param>
	/// <param name="onChanged">A callback to invoke when the assigned value changes, which receives the old and new items.</param>
	/// <returns>Returns whether to continue iterating if needed.</returns>
	public static bool ApplyToList<TItem>(IList<TItem> list, ForEachItemDelegate handler, bool leaveNullSlotsOnRemoval = false, Action<Item, Item, int> onChanged = null) where TItem : Item
	{
		for (int i = list.Count - 1; i >= 0; i--)
		{
			Item oldValue = list[i];
			if (!ApplyToItem(list[i], handler, Remove, ReplaceWith))
			{
				return false;
			}
			void Remove()
			{
				if (leaveNullSlotsOnRemoval)
				{
					list[i] = null;
				}
				else
				{
					list.RemoveAt(i);
				}
				onChanged?.Invoke(oldValue, null, i);
			}
			void ReplaceWith(Item newItem)
			{
				list[i] = PrepareForReplaceWith(list[i], (TItem)newItem);
				onChanged?.Invoke(oldValue, newItem, i);
			}
		}
		return true;
	}

	/// <summary>Prepare a new item instance as a replacement for an existing item.</summary>
	/// <param name="previousItem">The existing item that's being replaced.</param>
	/// <param name="newItem">The new item that will replace <paramref name="previousItem" />.</param>
	/// <returns>Returns the <paramref name="newItem" /> for convenience.</returns>
	private static TItem PrepareForReplaceWith<TItem>(TItem previousItem, TItem newItem) where TItem : Item
	{
		Object previousObj = previousItem as Object;
		Object newObj = newItem as Object;
		if (previousObj != null && newObj != null)
		{
			newObj.TileLocation = previousObj.TileLocation;
		}
		return newItem;
	}
}
