using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Delegates;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Menus;
using StardewValley.Network;

namespace StardewValley.Objects;

[XmlInclude(typeof(FishTankFurniture))]
public class StorageFurniture : Furniture
{
	[XmlElement("heldItems")]
	public readonly NetObjectList<Item> heldItems = new NetObjectList<Item>();

	[XmlIgnore]
	public readonly NetMutex mutex = new NetMutex();

	public StorageFurniture()
	{
	}

	public StorageFurniture(string itemId, Vector2 tile, int initialRotations)
		: base(itemId, tile, initialRotations)
	{
	}

	public StorageFurniture(string itemId, Vector2 tile)
		: base(itemId, tile)
	{
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(heldItems, "heldItems").AddField(mutex.NetFields, "mutex.NetFields");
	}

	public override bool canBeRemoved(Farmer who)
	{
		if (mutex.IsLocked())
		{
			return false;
		}
		return base.canBeRemoved(who);
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		mutex.RequestLock(ShowMenu);
		return true;
	}

	public virtual void ShowMenu()
	{
		ShowShopMenu();
	}

	public virtual void ShowChestMenu()
	{
		Game1.activeClickableMenu = new ItemGrabMenu(heldItems, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, GrabItemFromInventory, null, GrabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, this, -1, this)
		{
			behaviorBeforeCleanup = delegate
			{
				mutex.ReleaseLock();
				OnMenuClose();
			}
		};
		Game1.playSound("dwop");
	}

	public virtual void GrabItemFromInventory(Item item, Farmer who)
	{
		if (item.Stack == 0)
		{
			item.Stack = 1;
		}
		Item tmp = AddItem(item);
		if (tmp == null)
		{
			who.removeItemFromInventory(item);
		}
		else
		{
			tmp = who.addItemToInventory(tmp);
		}
		ClearNulls();
		int oldID = ((Game1.activeClickableMenu.currentlySnappedComponent != null) ? Game1.activeClickableMenu.currentlySnappedComponent.myID : (-1));
		ShowChestMenu();
		(Game1.activeClickableMenu as ItemGrabMenu).heldItem = tmp;
		if (oldID != -1)
		{
			Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
			Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
		}
	}

	public virtual bool HighlightItems(Item item)
	{
		return InventoryMenu.highlightAllItems(item);
	}

	public virtual void GrabItemFromChest(Item item, Farmer who)
	{
		if (who.couldInventoryAcceptThisItem(item))
		{
			heldItems.Remove(item);
			ClearNulls();
			ShowChestMenu();
		}
	}

	public virtual void ClearNulls()
	{
		for (int i = heldItems.Count - 1; i >= 0; i--)
		{
			if (heldItems[i] == null)
			{
				heldItems.RemoveAt(i);
			}
		}
	}

	public virtual Item AddItem(Item item)
	{
		item.resetState();
		ClearNulls();
		for (int i = 0; i < heldItems.Count; i++)
		{
			if (heldItems[i] != null && heldItems[i].canStackWith(item))
			{
				item.Stack = heldItems[i].addToStack(item);
				if (item.Stack <= 0)
				{
					return null;
				}
			}
		}
		if (heldItems.Count < 36)
		{
			heldItems.Add(item);
			return null;
		}
		return item;
	}

	public virtual void ShowShopMenu()
	{
		List<Item> list = heldItems.ToList();
		list.Sort(SortItems);
		Dictionary<ISalable, ItemStockInformation> contents = new Dictionary<ISalable, ItemStockInformation>();
		foreach (Item item in list)
		{
			contents[item] = new ItemStockInformation(0, 1, null, null, LimitedStockMode.None);
		}
		Game1.activeClickableMenu = new ShopMenu(GetShopMenuContext(), contents, 0, null, onDresserItemWithdrawn, onDresserItemDeposited)
		{
			source = this,
			behaviorBeforeCleanup = delegate
			{
				mutex.ReleaseLock();
				OnMenuClose();
			}
		};
	}

	public virtual void OnMenuClose()
	{
	}

	public virtual string GetShopMenuContext()
	{
		return "Dresser";
	}

	/// <inheritdoc />
	public override bool canBeTrashed()
	{
		if (heldItems.Count > 0)
		{
			return false;
		}
		return base.canBeTrashed();
	}

	public override void DayUpdate()
	{
		base.DayUpdate();
		mutex.ReleaseLock();
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new StorageFurniture(base.ItemId, tileLocation.Value);
	}

	public virtual int SortItems(Item a, Item b)
	{
		if (a.Category != b.Category)
		{
			return a.Category.CompareTo(b.Category);
		}
		if (a is Clothing clothingA && b is Clothing clothingB && clothingA.clothesType.Value != clothingB.clothesType.Value)
		{
			return clothingA.clothesType.Value.CompareTo(clothingB.clothesType.Value);
		}
		return a.ParentSheetIndex.CompareTo(b.ParentSheetIndex);
	}

	public virtual bool onDresserItemWithdrawn(ISalable salable, Farmer who, int amount)
	{
		if (salable is Item item)
		{
			heldItems.Remove(item);
		}
		return false;
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		GameLocation environment = Location;
		if (environment != null)
		{
			mutex.Update(environment);
		}
		base.updateWhenCurrentLocation(time);
	}

	public virtual bool onDresserItemDeposited(ISalable deposited_salable)
	{
		if (deposited_salable is Item depositedItem)
		{
			heldItems.Add(depositedItem);
			if (Game1.activeClickableMenu is ShopMenu)
			{
				Dictionary<ISalable, ItemStockInformation> contents = new Dictionary<ISalable, ItemStockInformation>();
				List<Item> list = heldItems.ToList();
				list.Sort(SortItems);
				foreach (Item item in list)
				{
					contents[item] = new ItemStockInformation(0, 1, null, null, LimitedStockMode.None);
				}
				(Game1.activeClickableMenu as ShopMenu).setItemPriceAndStock(contents);
				Game1.playSound("dwop");
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc />
	public override bool ForEachItem(ForEachItemDelegate handler)
	{
		if (base.ForEachItem(handler))
		{
			return ForEachItemHelper.ApplyToList(heldItems, handler);
		}
		return false;
	}
}
