using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Buildings;
using StardewValley.Objects;

namespace StardewValley.Menus;

public class ItemGrabMenu : MenuWithInventory
{
	public delegate void behaviorOnItemSelect(Item item, Farmer who);

	public class TransferredItemSprite
	{
		public Item item;

		public Vector2 position;

		public float age;

		public float alpha = 1f;

		public TransferredItemSprite(Item transferred_item, int start_x, int start_y)
		{
			item = transferred_item;
			position.X = start_x;
			position.Y = start_y;
		}

		public bool Update(GameTime time)
		{
			float life_time = 0.15f;
			position.Y -= (float)time.ElapsedGameTime.TotalSeconds * 128f;
			age += (float)time.ElapsedGameTime.TotalSeconds;
			alpha = 1f - age / life_time;
			if (age >= life_time)
			{
				return true;
			}
			return false;
		}

		public void Draw(SpriteBatch b)
		{
			item.drawInMenu(b, position, 1f, alpha, 0.9f, StackDrawType.Hide, Color.White, drawShadow: false);
		}
	}

	public const int region_organizationButtons = 15923;

	public const int region_itemsToGrabMenuModifier = 53910;

	public const int region_fillStacksButton = 12952;

	public const int region_organizeButton = 106;

	public const int region_colorPickToggle = 27346;

	public const int region_specialButton = 12485;

	public const int region_lastShippedHolder = 12598;

	/// <summary>The <see cref="F:StardewValley.Menus.ItemGrabMenu.source" /> value when a specific value doesn't apply.</summary>
	public const int source_none = 0;

	/// <summary>The <see cref="F:StardewValley.Menus.ItemGrabMenu.source" /> value when collecting items from a chest.</summary>
	public const int source_chest = 1;

	/// <summary>The <see cref="F:StardewValley.Menus.ItemGrabMenu.source" /> value when collecting items which couldn't be added directly to the player's inventory (e.g. from NPC dialogue).</summary>
	public const int source_gift = 2;

	/// <summary>The <see cref="F:StardewValley.Menus.ItemGrabMenu.source" /> value when collecting treasure found while fishing.</summary>
	public const int source_fishingChest = 3;

	/// <summary>The <see cref="F:StardewValley.Menus.ItemGrabMenu.source" /> value when collecting items which couldn't be added directly to the player's inventory via <see cref="M:StardewValley.Farmer.addItemByMenuIfNecessary(StardewValley.Item,StardewValley.Menus.ItemGrabMenu.behaviorOnItemSelect,System.Boolean)" />.</summary>
	public const int source_overflow = 4;

	public const int specialButton_junimotoggle = 1;

	/// <summary>The inventory from which the player can collect items.</summary>
	public InventoryMenu ItemsToGrabMenu;

	public TemporaryAnimatedSprite poof;

	public bool reverseGrab;

	public bool showReceivingMenu = true;

	public bool drawBG = true;

	public bool destroyItemOnClick;

	public bool canExitOnKey;

	public bool playRightClickSound;

	public bool allowRightClick;

	public bool shippingBin;

	public string message;

	/// <summary>The callback invoked when taking something out of the player inventory (e.g. putting something in the Luau soup), if any.</summary>
	public behaviorOnItemSelect behaviorFunction;

	/// <summary>The callback invoked when taking something from the menu (e.g. to put in the player's inventory), if any.</summary>
	public behaviorOnItemSelect behaviorOnItemGrab;

	/// <summary>The item for which the item menu was opened (e.g. the chest or storage furniture item being checked), if applicable.</summary>
	public Item sourceItem;

	public ClickableTextureComponent fillStacksButton;

	public ClickableTextureComponent organizeButton;

	public ClickableTextureComponent colorPickerToggleButton;

	public ClickableTextureComponent specialButton;

	public ClickableTextureComponent lastShippedHolder;

	public List<ClickableComponent> discreteColorPickerCC;

	/// <summary>The reason this menu was opened, usually matching a constant like <see cref="F:StardewValley.Menus.ItemGrabMenu.source_chest" />.</summary>
	public int source;

	public int whichSpecialButton;

	/// <summary>A contextual value for what opened the menu. This may be a chest, event, fishing rod, location, etc.</summary>
	public object context;

	public bool snappedtoBottom;

	public DiscreteColorPicker chestColorPicker;

	public bool essential;

	public bool superEssential;

	public int storageSpaceTopBorderOffset;

	/// <summary>Whether <see cref="M:StardewValley.Menus.ItemGrabMenu.update(Microsoft.Xna.Framework.GameTime)" /> has run at least once yet.</summary>
	private bool HasUpdateTicked;

	public List<TransferredItemSprite> _transferredItemSprites = new List<TransferredItemSprite>();

	/// <summary>Whether the source item was placed in the current location when the menu is opened.</summary>
	public bool _sourceItemInCurrentLocation;

	public ClickableTextureComponent junimoNoteIcon;

	public int junimoNotePulser;

	/// <summary>Construct an instance.</summary>
	/// <param name="inventory">The items that can be collected by the player.</param>
	/// <param name="context">A contextual value for what opened the menu. This may be a chest, event, fishing rod, location, etc.</param>
	public ItemGrabMenu(IList<Item> inventory, object context = null)
		: base(null, okButton: true, trashCan: true)
	{
		this.context = context;
		ItemsToGrabMenu = new InventoryMenu(xPositionOnScreen + 32, yPositionOnScreen, playerInventory: false, inventory);
		trashCan.myID = 106;
		ItemsToGrabMenu.populateClickableComponentList();
		for (int i = 0; i < ItemsToGrabMenu.inventory.Count; i++)
		{
			if (ItemsToGrabMenu.inventory[i] != null)
			{
				ItemsToGrabMenu.inventory[i].myID += 53910;
				ItemsToGrabMenu.inventory[i].upNeighborID += 53910;
				ItemsToGrabMenu.inventory[i].rightNeighborID += 53910;
				ItemsToGrabMenu.inventory[i].downNeighborID = -7777;
				ItemsToGrabMenu.inventory[i].leftNeighborID += 53910;
				ItemsToGrabMenu.inventory[i].fullyImmutable = true;
				if (i % (ItemsToGrabMenu.capacity / ItemsToGrabMenu.rows) == 0)
				{
					ItemsToGrabMenu.inventory[i].leftNeighborID = dropItemInvisibleButton.myID;
				}
				if (i % (ItemsToGrabMenu.capacity / ItemsToGrabMenu.rows) == ItemsToGrabMenu.capacity / ItemsToGrabMenu.rows - 1)
				{
					ItemsToGrabMenu.inventory[i].rightNeighborID = trashCan.myID;
				}
			}
		}
		for (int i = 0; i < GetColumnCount(); i++)
		{
			if (base.inventory?.inventory?.Count >= GetColumnCount())
			{
				base.inventory.inventory[i].upNeighborID = (shippingBin ? 12598 : (-7777));
			}
		}
		if (!shippingBin)
		{
			for (int i = 0; i < GetColumnCount() * 3; i++)
			{
				InventoryMenu inventoryMenu = base.inventory;
				if (inventoryMenu != null && inventoryMenu.inventory?.Count > i)
				{
					base.inventory.inventory[i].upNeighborID = -7777;
					base.inventory.inventory[i].upNeighborImmutable = true;
				}
			}
		}
		if (trashCan != null)
		{
			trashCan.leftNeighborID = 11;
		}
		if (okButton != null)
		{
			okButton.leftNeighborID = 11;
		}
		populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			snapToDefaultClickableComponent();
		}
		base.inventory.showGrayedOutSlots = true;
		SetupBorderNeighbors();
	}

	/// <summary>Drop any remaining items that weren't grabbed by the player onto the ground at their feet.</summary>
	public virtual void DropRemainingItems()
	{
		if (ItemsToGrabMenu?.actualInventory == null)
		{
			return;
		}
		foreach (Item item in ItemsToGrabMenu.actualInventory)
		{
			if (item != null)
			{
				Game1.createItemDebris(item, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
			}
		}
		ItemsToGrabMenu.actualInventory.Clear();
	}

	public ItemGrabMenu(IList<Item> inventory, bool reverseGrab, bool showReceivingMenu, InventoryMenu.highlightThisItem highlightFunction, behaviorOnItemSelect behaviorOnItemSelectFunction, string message, behaviorOnItemSelect behaviorOnItemGrab = null, bool snapToBottom = false, bool canBeExitedWithKey = false, bool playRightClickSound = true, bool allowRightClick = true, bool showOrganizeButton = false, int source = 0, Item sourceItem = null, int whichSpecialButton = -1, object context = null, ItemExitBehavior heldItemExitBehavior = ItemExitBehavior.ReturnToPlayer, bool allowExitWithHeldItem = false)
		: base(highlightFunction, okButton: true, trashCan: true, 0, 0, 64, heldItemExitBehavior, allowExitWithHeldItem)
	{
		this.source = source;
		this.message = message;
		this.reverseGrab = reverseGrab;
		this.showReceivingMenu = showReceivingMenu;
		this.playRightClickSound = playRightClickSound;
		this.allowRightClick = allowRightClick;
		base.inventory.showGrayedOutSlots = true;
		this.sourceItem = sourceItem;
		if (sourceItem != null && Game1.currentLocation.objects.Values.Contains(sourceItem))
		{
			_sourceItemInCurrentLocation = true;
		}
		else
		{
			_sourceItemInCurrentLocation = false;
		}
		if (source == 1 && sourceItem is Chest sourceChest && (sourceChest.SpecialChestType == Chest.SpecialChestTypes.None || sourceChest.SpecialChestType == Chest.SpecialChestTypes.BigChest))
		{
			Chest itemToDrawColored = new Chest(playerChest: true, sourceItem.ItemId);
			chestColorPicker = new DiscreteColorPicker(xPositionOnScreen, yPositionOnScreen - 64 - IClickableMenu.borderWidth * 2, sourceChest.playerChoiceColor.Value, itemToDrawColored);
			itemToDrawColored.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(chestColorPicker.colorSelection);
			colorPickerToggleButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - 64 + -160, 64, 64), Game1.mouseCursors, new Rectangle(119, 469, 16, 16), 4f)
			{
				hoverText = Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker"),
				myID = 27346,
				downNeighborID = -99998,
				leftNeighborID = 53921,
				region = 15923
			};
			if (InventoryPage.ShouldShowJunimoNoteIcon())
			{
				junimoNoteIcon = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - 64 + -216, 64, 64), "", Game1.content.LoadString("Strings\\UI:GameMenu_JunimoNote_Hover"), Game1.mouseCursors, new Rectangle(331, 374, 15, 14), 4f)
				{
					myID = 898,
					leftNeighborID = 11,
					downNeighborID = 106
				};
			}
		}
		this.whichSpecialButton = whichSpecialButton;
		this.context = context;
		if (whichSpecialButton == 1)
		{
			specialButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - 64 + -160, 64, 64), Game1.mouseCursors, new Rectangle(108, 491, 16, 16), 4f)
			{
				myID = 12485,
				downNeighborID = (showOrganizeButton ? 12952 : 5948),
				region = 15923,
				leftNeighborID = 53921
			};
			if (context is JunimoHut hut)
			{
				specialButton.sourceRect.X = (hut.noHarvest ? 124 : 108);
			}
		}
		if (snapToBottom)
		{
			movePosition(0, Game1.uiViewport.Height - (yPositionOnScreen + height - IClickableMenu.spaceToClearTopBorder));
			snappedtoBottom = true;
		}
		if (source == 1 && sourceItem is Chest chest && chest.GetActualCapacity() != 36)
		{
			int capacity = chest.GetActualCapacity();
			int rows = ((capacity >= 70) ? 5 : 3);
			if (capacity < 9)
			{
				rows = 1;
			}
			int containerWidth = 64 * (capacity / rows);
			ItemsToGrabMenu = new InventoryMenu(Game1.uiViewport.Width / 2 - containerWidth / 2, yPositionOnScreen + ((capacity < 70) ? 64 : (-21)), playerInventory: false, inventory, highlightFunction, capacity, rows);
			if (chest.SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin)
			{
				base.inventory.moveItemSound = "Ship";
			}
			if (rows > 3)
			{
				yPositionOnScreen += 42;
				base.inventory.SetPosition(base.inventory.xPositionOnScreen, base.inventory.yPositionOnScreen + 38 + 4);
				ItemsToGrabMenu.SetPosition(ItemsToGrabMenu.xPositionOnScreen - 32 + 8, ItemsToGrabMenu.yPositionOnScreen);
				storageSpaceTopBorderOffset = 20;
				trashCan.bounds.X = ItemsToGrabMenu.width + ItemsToGrabMenu.xPositionOnScreen + IClickableMenu.borderWidth * 2;
				okButton.bounds.X = ItemsToGrabMenu.width + ItemsToGrabMenu.xPositionOnScreen + IClickableMenu.borderWidth * 2;
			}
		}
		else
		{
			ItemsToGrabMenu = new InventoryMenu(xPositionOnScreen + 32, yPositionOnScreen, playerInventory: false, inventory, highlightFunction);
		}
		ItemsToGrabMenu.populateClickableComponentList();
		for (int i = 0; i < ItemsToGrabMenu.inventory.Count; i++)
		{
			if (ItemsToGrabMenu.inventory[i] != null)
			{
				ItemsToGrabMenu.inventory[i].myID += 53910;
				ItemsToGrabMenu.inventory[i].upNeighborID += 53910;
				ItemsToGrabMenu.inventory[i].rightNeighborID += 53910;
				ItemsToGrabMenu.inventory[i].downNeighborID = -7777;
				ItemsToGrabMenu.inventory[i].leftNeighborID += 53910;
				ItemsToGrabMenu.inventory[i].fullyImmutable = true;
			}
		}
		behaviorFunction = behaviorOnItemSelectFunction;
		this.behaviorOnItemGrab = behaviorOnItemGrab;
		canExitOnKey = canBeExitedWithKey;
		if (showOrganizeButton)
		{
			fillStacksButton = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - 64 - 64 - 16, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_FillStacks"), Game1.mouseCursors, new Rectangle(103, 469, 16, 16), 4f)
			{
				myID = 12952,
				upNeighborID = ((colorPickerToggleButton != null) ? 27346 : ((specialButton != null) ? 12485 : (-500))),
				downNeighborID = 106,
				leftNeighborID = 53921,
				region = 15923
			};
			organizeButton = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - 64, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), 4f)
			{
				myID = 106,
				upNeighborID = 12952,
				downNeighborID = 5948,
				leftNeighborID = 53921,
				region = 15923
			};
		}
		RepositionSideButtons();
		if (chestColorPicker != null)
		{
			discreteColorPickerCC = new List<ClickableComponent>();
			for (int i = 0; i < DiscreteColorPicker.totalColors; i++)
			{
				List<ClickableComponent> list = discreteColorPickerCC;
				ClickableComponent obj = new ClickableComponent(new Rectangle(chestColorPicker.xPositionOnScreen + IClickableMenu.borderWidth / 2 + i * 9 * 4, chestColorPicker.yPositionOnScreen + IClickableMenu.borderWidth / 2, 36, 28), "")
				{
					myID = i + 4343,
					rightNeighborID = ((i < DiscreteColorPicker.totalColors - 1) ? (i + 4343 + 1) : (-1)),
					leftNeighborID = ((i > 0) ? (i + 4343 - 1) : (-1))
				};
				InventoryMenu itemsToGrabMenu = ItemsToGrabMenu;
				obj.downNeighborID = ((itemsToGrabMenu != null && itemsToGrabMenu.inventory.Count > 0) ? 53910 : 0);
				list.Add(obj);
			}
		}
		if (organizeButton != null)
		{
			foreach (ClickableComponent item in ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Right))
			{
				item.rightNeighborID = organizeButton.myID;
			}
		}
		if (trashCan != null && base.inventory.inventory.Count >= 12 && base.inventory.inventory[11] != null)
		{
			base.inventory.inventory[11].rightNeighborID = 5948;
		}
		if (trashCan != null)
		{
			trashCan.leftNeighborID = 11;
		}
		if (okButton != null)
		{
			okButton.leftNeighborID = 11;
		}
		ClickableComponent top_right = ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Right).FirstOrDefault();
		if (top_right != null)
		{
			if (organizeButton != null)
			{
				organizeButton.leftNeighborID = top_right.myID;
			}
			if (specialButton != null)
			{
				specialButton.leftNeighborID = top_right.myID;
			}
			if (fillStacksButton != null)
			{
				fillStacksButton.leftNeighborID = top_right.myID;
			}
			if (junimoNoteIcon != null)
			{
				junimoNoteIcon.leftNeighborID = top_right.myID;
			}
		}
		populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			snapToDefaultClickableComponent();
		}
		SetupBorderNeighbors();
	}

	/// <summary>Create an item grab menu to collect items which couldn't be added to the player's inventory directly.</summary>
	/// <param name="items">The items to collect.</param>
	/// <param name="onCollectItem">The callback to invoke when an item is retrieved.</param>
	public static ItemGrabMenu CreateOverflowMenu(IList<Item> items, behaviorOnItemSelect onCollectItem = null)
	{
		ItemGrabMenu itemGrabMenu = new ItemGrabMenu(items).setEssential(essential: true);
		itemGrabMenu.inventory.showGrayedOutSlots = true;
		itemGrabMenu.inventory.onAddItem = onCollectItem;
		itemGrabMenu.source = 4;
		return itemGrabMenu;
	}

	/// <summary>Position the buttons that appear on the right side of the screen (e.g. to organize or fill stacks), and update their neighbor IDs.</summary>
	public virtual void RepositionSideButtons()
	{
		List<ClickableComponent> side_buttons = new List<ClickableComponent>();
		int slotsPerRow = ItemsToGrabMenu.capacity / ItemsToGrabMenu.rows;
		if (organizeButton != null)
		{
			organizeButton.leftNeighborID = slotsPerRow - 1 + 53910;
			side_buttons.Add(organizeButton);
		}
		if (fillStacksButton != null)
		{
			fillStacksButton.leftNeighborID = slotsPerRow - 1 + 53910;
			side_buttons.Add(fillStacksButton);
		}
		if (colorPickerToggleButton != null)
		{
			colorPickerToggleButton.leftNeighborID = slotsPerRow - 1 + 53910;
			side_buttons.Add(colorPickerToggleButton);
		}
		if (specialButton != null)
		{
			side_buttons.Add(specialButton);
		}
		if (junimoNoteIcon != null)
		{
			junimoNoteIcon.leftNeighborID = slotsPerRow - 1;
			side_buttons.Add(junimoNoteIcon);
		}
		int step_size = 80;
		if (side_buttons.Count >= 4)
		{
			step_size = 72;
		}
		for (int i = 0; i < side_buttons.Count; i++)
		{
			ClickableComponent button = side_buttons[i];
			if (i > 0 && side_buttons.Count > 1)
			{
				button.downNeighborID = side_buttons[i - 1].myID;
			}
			if (i < side_buttons.Count - 1 && side_buttons.Count > 1)
			{
				button.upNeighborID = side_buttons[i + 1].myID;
			}
			button.bounds.X = ItemsToGrabMenu.xPositionOnScreen + ItemsToGrabMenu.width + IClickableMenu.borderWidth * 2;
			button.bounds.Y = ItemsToGrabMenu.yPositionOnScreen + height / 3 - 64 - step_size * i;
		}
	}

	public void SetupBorderNeighbors()
	{
		List<ClickableComponent> border = inventory.GetBorder(InventoryMenu.BorderSide.Right);
		foreach (ClickableComponent item in border)
		{
			item.rightNeighborID = -99998;
			item.rightNeighborImmutable = true;
		}
		border = ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Right);
		bool has_organizational_buttons = false;
		foreach (ClickableComponent allClickableComponent in allClickableComponents)
		{
			if (allClickableComponent.region == 15923)
			{
				has_organizational_buttons = true;
				break;
			}
		}
		foreach (ClickableComponent slot in border)
		{
			if (has_organizational_buttons)
			{
				slot.rightNeighborID = -99998;
				slot.rightNeighborImmutable = true;
			}
			else
			{
				slot.rightNeighborID = -1;
			}
		}
		for (int i = 0; i < GetColumnCount(); i++)
		{
			InventoryMenu inventoryMenu = inventory;
			ClickableComponent clickableComponent;
			int upNeighborID;
			if (inventoryMenu != null && inventoryMenu.inventory?.Count >= 12)
			{
				clickableComponent = inventory.inventory[i];
				if (!shippingBin)
				{
					if (discreteColorPickerCC != null)
					{
						InventoryMenu itemsToGrabMenu = ItemsToGrabMenu;
						if (itemsToGrabMenu != null && itemsToGrabMenu.inventory.Count <= i && Game1.player.showChestColorPicker)
						{
							upNeighborID = 4343;
							goto IL_01b0;
						}
					}
					upNeighborID = ((ItemsToGrabMenu.inventory.Count > i) ? (53910 + i) : 53910);
				}
				else
				{
					upNeighborID = 12598;
				}
				goto IL_01b0;
			}
			goto IL_01b5;
			IL_01b5:
			if (discreteColorPickerCC != null)
			{
				InventoryMenu itemsToGrabMenu2 = ItemsToGrabMenu;
				if (itemsToGrabMenu2 != null && itemsToGrabMenu2.inventory.Count > i && Game1.player.showChestColorPicker)
				{
					ItemsToGrabMenu.inventory[i].upNeighborID = 4343;
					continue;
				}
			}
			ItemsToGrabMenu.inventory[i].upNeighborID = -1;
			continue;
			IL_01b0:
			clickableComponent.upNeighborID = upNeighborID;
			goto IL_01b5;
		}
		if (shippingBin)
		{
			return;
		}
		for (int i = 0; i < 36; i++)
		{
			InventoryMenu inventoryMenu2 = inventory;
			if (inventoryMenu2 != null && inventoryMenu2.inventory?.Count > i)
			{
				inventory.inventory[i].upNeighborID = -7777;
				inventory.inventory[i].upNeighborImmutable = true;
			}
		}
	}

	public virtual int GetColumnCount()
	{
		return ItemsToGrabMenu.capacity / ItemsToGrabMenu.rows;
	}

	/// <summary>Set whether to rescue items from the menu when it's force-closed (e.g. from passing out at 2am). Rescued items will be added to the player's inventory if possible, else dropped onto the ground at their feet.</summary>
	/// <param name="essential">Whether to rescue items on force-close.</param>
	/// <param name="superEssential">Whether to rescue items on normal close.</param>
	public ItemGrabMenu setEssential(bool essential, bool superEssential = false)
	{
		this.essential = essential || superEssential;
		this.superEssential = superEssential;
		return this;
	}

	public void initializeShippingBin()
	{
		shippingBin = true;
		lastShippedHolder = new ClickableTextureComponent("", new Rectangle(xPositionOnScreen + width / 2 - 48, yPositionOnScreen + height / 2 - 80 - 64, 96, 96), "", Game1.content.LoadString("Strings\\UI:ShippingBin_LastItem"), Game1.mouseCursors, new Rectangle(293, 360, 24, 24), 4f)
		{
			myID = 12598,
			region = 12598
		};
		for (int i = 0; i < GetColumnCount(); i++)
		{
			if (inventory?.inventory?.Count >= GetColumnCount())
			{
				inventory.inventory[i].upNeighborID = -7777;
				if (i == 11)
				{
					inventory.inventory[i].rightNeighborID = 5948;
				}
			}
		}
		populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			snapToDefaultClickableComponent();
		}
	}

	protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
	{
		switch (direction)
		{
		case 2:
		{
			for (int i = 0; i < 12; i++)
			{
				if (inventory?.inventory?.Count >= GetColumnCount() && shippingBin)
				{
					inventory.inventory[i].upNeighborID = (shippingBin ? 12598 : (Math.Min(i, ItemsToGrabMenu.inventory.Count - 1) + 53910));
				}
			}
			if (!shippingBin && oldID >= 53910)
			{
				int index = oldID - 53910;
				if (index + GetColumnCount() <= ItemsToGrabMenu.inventory.Count - 1)
				{
					currentlySnappedComponent = getComponentWithID(index + GetColumnCount() + 53910);
					snapCursorToCurrentSnappedComponent();
					break;
				}
			}
			if (inventory != null)
			{
				int inventoryRowLength = inventory.capacity / inventory.rows;
				int diff = GetColumnCount() - inventoryRowLength;
				currentlySnappedComponent = getComponentWithID((oldRegion != 12598) ? Math.Max(0, Math.Min((oldID - 53910) % GetColumnCount() - diff / 2, inventory.capacity / inventory.rows - diff / 2)) : 0);
			}
			else
			{
				currentlySnappedComponent = getComponentWithID((oldRegion != 12598) ? ((oldID - 53910) % GetColumnCount()) : 0);
			}
			snapCursorToCurrentSnappedComponent();
			break;
		}
		case 0:
		{
			if (shippingBin && Game1.getFarm().lastItemShipped != null && oldID < 12)
			{
				currentlySnappedComponent = getComponentWithID(12598);
				currentlySnappedComponent.downNeighborID = oldID;
				snapCursorToCurrentSnappedComponent();
				break;
			}
			if (oldID < 53910 && oldID >= 12)
			{
				currentlySnappedComponent = getComponentWithID(oldID - 12);
				break;
			}
			int id = oldID + GetColumnCount() * (ItemsToGrabMenu.rows - 1);
			for (int i = 0; i < 3; i++)
			{
				if (ItemsToGrabMenu.inventory.Count > id)
				{
					break;
				}
				id -= GetColumnCount();
			}
			if (showReceivingMenu)
			{
				if (id < 0)
				{
					if (ItemsToGrabMenu.inventory.Count > 0)
					{
						currentlySnappedComponent = getComponentWithID(53910 + ItemsToGrabMenu.inventory.Count - 1);
					}
					else if (discreteColorPickerCC != null)
					{
						currentlySnappedComponent = getComponentWithID(4343);
					}
				}
				else
				{
					int inventoryRowLength = inventory.capacity / inventory.rows;
					int diff = GetColumnCount() - inventoryRowLength;
					currentlySnappedComponent = getComponentWithID(id + 53910 + diff / 2);
					if (currentlySnappedComponent == null)
					{
						currentlySnappedComponent = getComponentWithID(53910);
					}
				}
			}
			snapCursorToCurrentSnappedComponent();
			break;
		}
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		if (shippingBin)
		{
			currentlySnappedComponent = getComponentWithID(0);
		}
		else if (source == 1 && sourceItem is Chest { SpecialChestType: Chest.SpecialChestTypes.MiniShippingBin })
		{
			currentlySnappedComponent = getComponentWithID(0);
		}
		else
		{
			currentlySnappedComponent = getComponentWithID((ItemsToGrabMenu.inventory.Count > 0 && showReceivingMenu) ? 53910 : 0);
		}
		snapCursorToCurrentSnappedComponent();
	}

	public void setSourceItem(Item item)
	{
		sourceItem = item;
		chestColorPicker = null;
		colorPickerToggleButton = null;
		if (source == 1 && sourceItem is Chest chest && (chest.SpecialChestType == Chest.SpecialChestTypes.None || chest.SpecialChestType == Chest.SpecialChestTypes.BigChest))
		{
			Chest itemToDrawColored = new Chest(playerChest: true, sourceItem.ItemId);
			chestColorPicker = new DiscreteColorPicker(xPositionOnScreen, yPositionOnScreen - 64 - IClickableMenu.borderWidth * 2, chest.playerChoiceColor.Value, itemToDrawColored);
			if (chest.SpecialChestType == Chest.SpecialChestTypes.BigChest)
			{
				chestColorPicker.yPositionOnScreen -= 42;
			}
			itemToDrawColored.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(chestColorPicker.colorSelection);
			colorPickerToggleButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height / 3 - 64 + -160, 64, 64), Game1.mouseCursors, new Rectangle(119, 469, 16, 16), 4f)
			{
				hoverText = Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker")
			};
		}
		RepositionSideButtons();
	}

	public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
	{
		if (direction == 1 && ItemsToGrabMenu.inventory.Contains(a) && inventory.inventory.Contains(b))
		{
			return false;
		}
		return base.IsAutomaticSnapValid(direction, a, b);
	}

	public void setBackgroundTransparency(bool b)
	{
		drawBG = b;
	}

	public void setDestroyItemOnClick(bool b)
	{
		destroyItemOnClick = b;
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		if (!allowRightClick)
		{
			receiveRightClickOnlyToolAttachments(x, y);
			return;
		}
		base.receiveRightClick(x, y, playSound && playRightClickSound);
		if (base.heldItem == null && showReceivingMenu)
		{
			base.heldItem = ItemsToGrabMenu.rightClick(x, y, base.heldItem, playSound: false);
			if (base.heldItem != null && behaviorOnItemGrab != null)
			{
				behaviorOnItemGrab(base.heldItem, Game1.player);
				if (Game1.activeClickableMenu is ItemGrabMenu itemGrabMenu)
				{
					itemGrabMenu.setSourceItem(sourceItem);
					if (Game1.options.SnappyMenus)
					{
						itemGrabMenu.currentlySnappedComponent = currentlySnappedComponent;
						itemGrabMenu.snapCursorToCurrentSnappedComponent();
					}
				}
			}
			if (base.heldItem?.QualifiedItemId == "(O)326")
			{
				base.heldItem = null;
				Game1.player.canUnderstandDwarves = true;
				poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
				Game1.playSound("fireball");
			}
			else if (base.heldItem is Object obj && obj?.QualifiedItemId == "(O)434")
			{
				base.heldItem = null;
				exitThisMenu(playSound: false);
				Game1.player.eatObject(obj, overrideFullness: true);
			}
			else if (base.heldItem != null && base.heldItem.IsRecipe)
			{
				string recipeName = base.heldItem.Name.Substring(0, base.heldItem.Name.IndexOf("Recipe") - 1);
				try
				{
					if (base.heldItem.Category == -7)
					{
						Game1.player.cookingRecipes.TryAdd(recipeName, 0);
					}
					else
					{
						Game1.player.craftingRecipes.TryAdd(recipeName, 0);
					}
					poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
					Game1.playSound("newRecipe");
				}
				catch (Exception)
				{
				}
				base.heldItem = null;
			}
			else if (Game1.player.addItemToInventoryBool(base.heldItem))
			{
				base.heldItem = null;
				Game1.playSound("coin");
			}
		}
		else if (reverseGrab || behaviorFunction != null)
		{
			behaviorFunction(base.heldItem, Game1.player);
			if (Game1.activeClickableMenu is ItemGrabMenu itemGrabMenu)
			{
				itemGrabMenu.setSourceItem(sourceItem);
			}
			if (destroyItemOnClick)
			{
				base.heldItem = null;
			}
		}
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		if (snappedtoBottom)
		{
			movePosition((newBounds.Width - oldBounds.Width) / 2, Game1.uiViewport.Height - (yPositionOnScreen + height - IClickableMenu.spaceToClearTopBorder));
		}
		else
		{
			movePosition((newBounds.Width - oldBounds.Width) / 2, (newBounds.Height - oldBounds.Height) / 2);
		}
		ItemsToGrabMenu?.gameWindowSizeChanged(oldBounds, newBounds);
		RepositionSideButtons();
		if (source == 1 && sourceItem is Chest chest && (chest.SpecialChestType == Chest.SpecialChestTypes.None || chest.SpecialChestType == Chest.SpecialChestTypes.BigChest))
		{
			chestColorPicker = new DiscreteColorPicker(xPositionOnScreen, yPositionOnScreen - 64 - IClickableMenu.borderWidth * 2, chest.playerChoiceColor.Value, new Chest(playerChest: true, sourceItem.ItemId));
		}
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		base.receiveLeftClick(x, y, !destroyItemOnClick);
		if (shippingBin && lastShippedHolder.containsPoint(x, y))
		{
			if (Game1.getFarm().lastItemShipped == null)
			{
				return;
			}
			Game1.getFarm().getShippingBin(Game1.player).Remove(Game1.getFarm().lastItemShipped);
			if (Game1.player.addItemToInventoryBool(Game1.getFarm().lastItemShipped))
			{
				Game1.playSound("coin");
				Game1.getFarm().lastItemShipped = null;
				if (Game1.player.ActiveObject != null)
				{
					Game1.player.showCarrying();
					Game1.player.Halt();
				}
			}
			else
			{
				Game1.getFarm().getShippingBin(Game1.player).Add(Game1.getFarm().lastItemShipped);
			}
			return;
		}
		if (chestColorPicker != null)
		{
			chestColorPicker.receiveLeftClick(x, y);
			if (sourceItem is Chest chest)
			{
				chest.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(chestColorPicker.colorSelection);
			}
		}
		if (colorPickerToggleButton != null && colorPickerToggleButton.containsPoint(x, y))
		{
			Game1.player.showChestColorPicker = !Game1.player.showChestColorPicker;
			chestColorPicker.visible = Game1.player.showChestColorPicker;
			try
			{
				Game1.playSound("drumkit6");
			}
			catch (Exception)
			{
			}
			SetupBorderNeighbors();
			return;
		}
		if (whichSpecialButton != -1 && specialButton != null && specialButton.containsPoint(x, y))
		{
			Game1.playSound("drumkit6");
			if (whichSpecialButton == 1 && context is JunimoHut hut)
			{
				hut.noHarvest.Value = !hut.noHarvest;
				specialButton.sourceRect.X = (hut.noHarvest ? 124 : 108);
			}
			return;
		}
		if (base.heldItem == null && showReceivingMenu)
		{
			base.heldItem = ItemsToGrabMenu.leftClick(x, y, base.heldItem, playSound: false);
			if (base.heldItem != null && behaviorOnItemGrab != null)
			{
				behaviorOnItemGrab(base.heldItem, Game1.player);
				if (Game1.activeClickableMenu is ItemGrabMenu itemGrabMenu)
				{
					itemGrabMenu.setSourceItem(sourceItem);
					if (Game1.options.SnappyMenus)
					{
						itemGrabMenu.currentlySnappedComponent = currentlySnappedComponent;
						itemGrabMenu.snapCursorToCurrentSnappedComponent();
					}
				}
			}
			string text = base.heldItem?.QualifiedItemId;
			if (!(text == "(O)326"))
			{
				if (text == "(O)102")
				{
					base.heldItem = null;
					Game1.player.foundArtifact("102", 1);
					poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
					Game1.playSound("fireball");
				}
			}
			else
			{
				base.heldItem = null;
				Game1.player.canUnderstandDwarves = true;
				poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
				Game1.playSound("fireball");
			}
			if (base.heldItem is Object stardrop && stardrop?.QualifiedItemId == "(O)434")
			{
				base.heldItem = null;
				exitThisMenu(playSound: false);
				Game1.player.eatObject(stardrop, overrideFullness: true);
			}
			else if (base.heldItem != null && base.heldItem.IsRecipe)
			{
				string recipeName = base.heldItem.Name.Substring(0, base.heldItem.Name.IndexOf("Recipe") - 1);
				try
				{
					if (base.heldItem.Category == -7)
					{
						Game1.player.cookingRecipes.TryAdd(recipeName, 0);
					}
					else
					{
						Game1.player.craftingRecipes.TryAdd(recipeName, 0);
					}
					poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
					Game1.playSound("newRecipe");
				}
				catch (Exception)
				{
				}
				base.heldItem = null;
			}
			else if (Game1.player.addItemToInventoryBool(base.heldItem))
			{
				base.heldItem = null;
				Game1.playSound("coin");
			}
		}
		else if ((reverseGrab || behaviorFunction != null) && isWithinBounds(x, y))
		{
			behaviorFunction(base.heldItem, Game1.player);
			if (Game1.activeClickableMenu is ItemGrabMenu itemGrabMenu)
			{
				itemGrabMenu.setSourceItem(sourceItem);
				if (Game1.options.SnappyMenus)
				{
					itemGrabMenu.currentlySnappedComponent = currentlySnappedComponent;
					itemGrabMenu.snapCursorToCurrentSnappedComponent();
				}
			}
			if (destroyItemOnClick)
			{
				base.heldItem = null;
				return;
			}
		}
		if (organizeButton != null && organizeButton.containsPoint(x, y))
		{
			ClickableComponent last_snapped_component = currentlySnappedComponent;
			organizeItemsInList(ItemsToGrabMenu.actualInventory);
			Item held_item = base.heldItem;
			base.heldItem = null;
			ItemGrabMenu itemGrabMenu = new ItemGrabMenu(ItemsToGrabMenu.actualInventory, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, behaviorFunction, null, behaviorOnItemGrab, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, source, sourceItem, whichSpecialButton, context, HeldItemExitBehavior, AllowExitWithHeldItem).setEssential(essential);
			if (last_snapped_component != null)
			{
				itemGrabMenu.setCurrentlySnappedComponentTo(last_snapped_component.myID);
				if (Game1.options.SnappyMenus)
				{
					snapCursorToCurrentSnappedComponent();
				}
			}
			itemGrabMenu.heldItem = held_item;
			Game1.activeClickableMenu = itemGrabMenu;
			Game1.playSound("Ship");
		}
		else if (fillStacksButton != null && fillStacksButton.containsPoint(x, y))
		{
			FillOutStacks();
			Game1.playSound("Ship");
		}
		else if (junimoNoteIcon != null && junimoNoteIcon.containsPoint(x, y))
		{
			if (readyToClose())
			{
				Game1.activeClickableMenu = new JunimoNoteMenu(fromGameMenu: true)
				{
					menuToReturnTo = this
				};
			}
		}
		else if (base.heldItem != null && !isWithinBounds(x, y) && base.heldItem.canBeTrashed())
		{
			DropHeldItem();
		}
	}

	/// <summary>Merge any items from the player inventory into an equivalent stack in the chest where possible.</summary>
	public void FillOutStacks()
	{
		for (int i = 0; i < ItemsToGrabMenu.actualInventory.Count; i++)
		{
			Item chest_item = ItemsToGrabMenu.actualInventory[i];
			if (chest_item == null || chest_item.maximumStackSize() <= 1)
			{
				continue;
			}
			for (int j = 0; j < inventory.actualInventory.Count; j++)
			{
				Item inventory_item = inventory.actualInventory[j];
				if (inventory_item == null || !chest_item.canStackWith(inventory_item))
				{
					continue;
				}
				TransferredItemSprite item_sprite = new TransferredItemSprite(inventory_item.getOne(), inventory.inventory[j].bounds.X, inventory.inventory[j].bounds.Y);
				_transferredItemSprites.Add(item_sprite);
				int stack_count = inventory_item.Stack;
				if (chest_item.getRemainingStackSpace() > 0)
				{
					stack_count = chest_item.addToStack(inventory_item);
					ItemsToGrabMenu.ShakeItem(chest_item);
				}
				inventory_item.Stack = stack_count;
				while (inventory_item.Stack > 0)
				{
					Item overflow_stack = null;
					if (!Utility.canItemBeAddedToThisInventoryList(chest_item.getOne(), ItemsToGrabMenu.actualInventory, ItemsToGrabMenu.capacity))
					{
						break;
					}
					if (overflow_stack == null)
					{
						for (int k = 0; k < ItemsToGrabMenu.actualInventory.Count; k++)
						{
							if (ItemsToGrabMenu.actualInventory[k] != null && ItemsToGrabMenu.actualInventory[k].canStackWith(chest_item) && ItemsToGrabMenu.actualInventory[k].getRemainingStackSpace() > 0)
							{
								overflow_stack = ItemsToGrabMenu.actualInventory[k];
								break;
							}
						}
					}
					if (overflow_stack == null)
					{
						for (int k = 0; k < ItemsToGrabMenu.actualInventory.Count; k++)
						{
							if (ItemsToGrabMenu.actualInventory[k] == null)
							{
								Item item = (ItemsToGrabMenu.actualInventory[k] = chest_item.getOne());
								overflow_stack = item;
								overflow_stack.Stack = 0;
								break;
							}
						}
					}
					if (overflow_stack == null && ItemsToGrabMenu.actualInventory.Count < ItemsToGrabMenu.capacity)
					{
						overflow_stack = chest_item.getOne();
						overflow_stack.Stack = 0;
						ItemsToGrabMenu.actualInventory.Add(overflow_stack);
					}
					if (overflow_stack == null)
					{
						break;
					}
					stack_count = overflow_stack.addToStack(inventory_item);
					ItemsToGrabMenu.ShakeItem(overflow_stack);
					inventory_item.Stack = stack_count;
				}
				if (inventory_item.Stack == 0)
				{
					inventory.actualInventory[j] = null;
				}
			}
		}
	}

	/// <summary>Consolidate and sort item stacks in an item list.</summary>
	/// <param name="items">The item list to change.</param>
	public static void organizeItemsInList(IList<Item> items)
	{
		List<Item> copy = new List<Item>(items);
		List<Item> tools = new List<Item>();
		for (int i = 0; i < copy.Count; i++)
		{
			Item item = copy[i];
			if (item != null)
			{
				if (item is Tool)
				{
					tools.Add(copy[i]);
					copy.RemoveAt(i);
					i--;
				}
			}
			else
			{
				copy.RemoveAt(i);
				i--;
			}
		}
		for (int i = 0; i < copy.Count; i++)
		{
			Item current_item = copy[i];
			if (current_item.getRemainingStackSpace() <= 0)
			{
				continue;
			}
			for (int j = i + 1; j < copy.Count; j++)
			{
				Item other_item = copy[j];
				if (current_item.canStackWith(other_item))
				{
					other_item.Stack = current_item.addToStack(other_item);
					if (other_item.Stack == 0)
					{
						copy.RemoveAt(j);
						j--;
					}
				}
			}
		}
		copy.Sort();
		copy.InsertRange(0, tools);
		for (int i = 0; i < items.Count; i++)
		{
			items[i] = null;
		}
		for (int i = 0; i < copy.Count; i++)
		{
			items[i] = copy[i];
		}
	}

	public bool areAllItemsTaken()
	{
		for (int i = 0; i < ItemsToGrabMenu.actualInventory.Count; i++)
		{
			if (ItemsToGrabMenu.actualInventory[i] != null)
			{
				return false;
			}
		}
		return true;
	}

	public override void receiveGamePadButton(Buttons b)
	{
		base.receiveGamePadButton(b);
		if (b == Buttons.Back && organizeButton != null)
		{
			organizeItemsInList(Game1.player.Items);
			Game1.playSound("Ship");
		}
		if (b == Buttons.RightShoulder)
		{
			ClickableComponent fill_stacks_component = getComponentWithID(12952);
			if (fill_stacks_component != null)
			{
				setCurrentlySnappedComponentTo(fill_stacks_component.myID);
				snapCursorToCurrentSnappedComponent();
			}
			else
			{
				int highest_y = -1;
				ClickableComponent highest_component = null;
				foreach (ClickableComponent component in allClickableComponents)
				{
					if (component.region == 15923 && (highest_y == -1 || component.bounds.Y < highest_y))
					{
						highest_y = component.bounds.Y;
						highest_component = component;
					}
				}
				if (highest_component != null)
				{
					setCurrentlySnappedComponentTo(highest_component.myID);
					snapCursorToCurrentSnappedComponent();
				}
			}
		}
		if (shippingBin || b != Buttons.LeftShoulder)
		{
			return;
		}
		ClickableComponent component53910 = getComponentWithID(53910);
		if (component53910 != null)
		{
			setCurrentlySnappedComponentTo(component53910.myID);
			snapCursorToCurrentSnappedComponent();
			return;
		}
        component53910 = getComponentWithID(0);
		if (component53910 != null)
		{
			setCurrentlySnappedComponentTo(0);
			snapCursorToCurrentSnappedComponent();
		}
	}

	public override void receiveKeyPress(Keys key)
	{
		if (Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			applyMovementKey(key);
		}
		if ((canExitOnKey || areAllItemsTaken()) && Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
		{
			exitThisMenu();
			Event currentEvent = Game1.currentLocation.currentEvent;
			if (currentEvent != null && currentEvent.CurrentCommand > 0)
			{
				Game1.currentLocation.currentEvent.CurrentCommand++;
			}
		}
		else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && base.heldItem != null)
		{
			Game1.setMousePosition(trashCan.bounds.Center);
		}
		if (key == Keys.Delete && base.heldItem != null && base.heldItem.canBeTrashed())
		{
			Utility.trashItem(base.heldItem);
			base.heldItem = null;
		}
	}

	public override void update(GameTime time)
	{
		base.update(time);
		if (!HasUpdateTicked)
		{
			HasUpdateTicked = true;
			if (source == 4)
			{
				IList<Item> items = ItemsToGrabMenu.actualInventory;
				for (int i = 0; i < items.Count; i++)
				{
					if (items[i]?.QualifiedItemId == "(O)434")
					{
						List<Item> remainingItems = new List<Item>(items);
						remainingItems.RemoveAt(i);
						remainingItems.RemoveAll((Item p) => p == null);
						if (remainingItems.Count > 0)
						{
							Game1.nextClickableMenu.Insert(0, CreateOverflowMenu(remainingItems, inventory.onAddItem));
						}
						essential = false;
						superEssential = false;
						exitThisMenu(playSound: false);
						Game1.player.eatObject(items[i] as Object, overrideFullness: true);
						return;
					}
				}
			}
		}
		if (poof != null && poof.update(time))
		{
			poof = null;
		}
		chestColorPicker?.update(time);
		if (sourceItem is Chest chest && _sourceItemInCurrentLocation)
		{
			Vector2 tileLocation = chest.tileLocation.Value;
			if (tileLocation != Vector2.Zero && !Game1.currentLocation.objects.ContainsKey(tileLocation))
			{
				if (Game1.activeClickableMenu != null)
				{
					Game1.activeClickableMenu.emergencyShutDown();
				}
				Game1.exitActiveMenu();
			}
		}
		for (int i = 0; i < _transferredItemSprites.Count; i++)
		{
			if (_transferredItemSprites[i].Update(time))
			{
				_transferredItemSprites.RemoveAt(i);
				i--;
			}
		}
	}

	public override void performHoverAction(int x, int y)
	{
		hoveredItem = null;
		hoverText = "";
		base.performHoverAction(x, y);
		if (colorPickerToggleButton != null)
		{
			colorPickerToggleButton.tryHover(x, y, 0.25f);
			if (colorPickerToggleButton.containsPoint(x, y))
			{
				hoverText = colorPickerToggleButton.hoverText;
			}
		}
		if (organizeButton != null)
		{
			organizeButton.tryHover(x, y, 0.25f);
			if (organizeButton.containsPoint(x, y))
			{
				hoverText = organizeButton.hoverText;
			}
		}
		if (fillStacksButton != null)
		{
			fillStacksButton.tryHover(x, y, 0.25f);
			if (fillStacksButton.containsPoint(x, y))
			{
				hoverText = fillStacksButton.hoverText;
			}
		}
		specialButton?.tryHover(x, y, 0.25f);
		if (showReceivingMenu)
		{
			Item item_grab_hovered_item = ItemsToGrabMenu.hover(x, y, base.heldItem);
			if (item_grab_hovered_item != null)
			{
				hoveredItem = item_grab_hovered_item;
			}
		}
		if (junimoNoteIcon != null)
		{
			junimoNoteIcon.tryHover(x, y);
			if (junimoNoteIcon.containsPoint(x, y))
			{
				hoverText = junimoNoteIcon.hoverText;
			}
			if (GameMenu.bundleItemHovered)
			{
				junimoNoteIcon.scale = junimoNoteIcon.baseScale + (float)Math.Sin((float)junimoNotePulser / 100f) / 4f;
				junimoNotePulser += (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			}
			else
			{
				junimoNotePulser = 0;
				junimoNoteIcon.scale = junimoNoteIcon.baseScale;
			}
		}
		if (hoverText != null)
		{
			return;
		}
		if (organizeButton != null)
		{
			hoverText = null;
			organizeButton.tryHover(x, y);
			if (organizeButton.containsPoint(x, y))
			{
				hoverText = organizeButton.hoverText;
			}
		}
		if (shippingBin)
		{
			hoverText = null;
			if (lastShippedHolder.containsPoint(x, y) && Game1.getFarm().lastItemShipped != null)
			{
				hoverText = lastShippedHolder.hoverText;
			}
		}
		chestColorPicker?.performHoverAction(x, y);
	}

	public override void draw(SpriteBatch b)
	{
		if (drawBG && !Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
		}
		base.draw(b, drawUpperPortion: false, drawDescriptionArea: false);
		if (showReceivingMenu)
		{
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen - 64, yPositionOnScreen + height / 2 + 64 + 16), new Rectangle(16, 368, 12, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen - 64, yPositionOnScreen + height / 2 + 64 - 16), new Rectangle(21, 368, 11, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen - 40, yPositionOnScreen + height / 2 + 64 - 44), new Rectangle(4, 372, 8, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			Game1.drawDialogueBox(ItemsToGrabMenu.xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder, ItemsToGrabMenu.yPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + storageSpaceTopBorderOffset, ItemsToGrabMenu.width + IClickableMenu.borderWidth * 2 + IClickableMenu.spaceToClearSideBorder * 2, ItemsToGrabMenu.height + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth * 2 - storageSpaceTopBorderOffset, speaker: false, drawOnlyBox: true);
			if ((source != 1 || !(sourceItem is Chest chest) || (chest.SpecialChestType != Chest.SpecialChestTypes.MiniShippingBin && chest.SpecialChestType != Chest.SpecialChestTypes.JunimoChest && chest.SpecialChestType != Chest.SpecialChestTypes.Enricher)) && source != 0)
			{
				b.Draw(Game1.mouseCursors, new Vector2(ItemsToGrabMenu.xPositionOnScreen - 100, yPositionOnScreen + 64 + 16), new Rectangle(16, 368, 12, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(ItemsToGrabMenu.xPositionOnScreen - 100, yPositionOnScreen + 64 - 16), new Rectangle(21, 368, 11, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				Rectangle sourceRect = new Rectangle(127, 412, 10, 11);
				switch (source)
				{
				case 3:
					sourceRect.X += 10;
					break;
				case 4:
					sourceRect.X += 20;
					break;
				}
				b.Draw(Game1.mouseCursors, new Vector2(ItemsToGrabMenu.xPositionOnScreen - 80, yPositionOnScreen + 64 - 44), sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			ItemsToGrabMenu.draw(b);
		}
		else if (message != null)
		{
			Game1.drawDialogueBox(Game1.uiViewport.Width / 2, ItemsToGrabMenu.yPositionOnScreen + ItemsToGrabMenu.height / 2, speaker: false, drawOnlyBox: false, message);
		}
		poof?.draw(b, localPosition: true);
		foreach (TransferredItemSprite transferredItemSprite in _transferredItemSprites)
		{
			transferredItemSprite.Draw(b);
		}
		if (shippingBin && Game1.getFarm().lastItemShipped != null)
		{
			lastShippedHolder.draw(b);
			Game1.getFarm().lastItemShipped.drawInMenu(b, new Vector2(lastShippedHolder.bounds.X + 16, lastShippedHolder.bounds.Y + 16), 1f);
			b.Draw(Game1.mouseCursors, new Vector2(lastShippedHolder.bounds.X + -8, lastShippedHolder.bounds.Bottom - 100), new Rectangle(325, 448, 5, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(lastShippedHolder.bounds.X + 84, lastShippedHolder.bounds.Bottom - 100), new Rectangle(325, 448, 5, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(lastShippedHolder.bounds.X + -8, lastShippedHolder.bounds.Bottom - 44), new Rectangle(325, 452, 5, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(lastShippedHolder.bounds.X + 84, lastShippedHolder.bounds.Bottom - 44), new Rectangle(325, 452, 5, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		}
		if (colorPickerToggleButton != null)
		{
			colorPickerToggleButton.draw(b);
		}
		else
		{
			specialButton?.draw(b);
		}
		chestColorPicker?.draw(b);
		organizeButton?.draw(b);
		fillStacksButton?.draw(b);
		junimoNoteIcon?.draw(b);
		if (hoverText != null && (hoveredItem == null || hoveredItem == null || ItemsToGrabMenu == null))
		{
			if (hoverAmount > 0)
			{
				IClickableMenu.drawToolTip(b, hoverText, "", null, heldItem: true, -1, 0, null, -1, null, hoverAmount);
			}
			else
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
			}
		}
		if (hoveredItem != null)
		{
			IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem, base.heldItem != null);
		}
		else if (hoveredItem != null && ItemsToGrabMenu != null)
		{
			IClickableMenu.drawToolTip(b, ItemsToGrabMenu.descriptionText, ItemsToGrabMenu.descriptionTitle, hoveredItem, base.heldItem != null);
		}
		base.heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
		Game1.mouseCursorTransparency = 1f;
		drawMouse(b);
	}

	protected override void cleanupBeforeExit()
	{
		base.cleanupBeforeExit();
		if (superEssential)
		{
			DropRemainingItems();
		}
	}

	public override void emergencyShutDown()
	{
		base.emergencyShutDown();
		if (!essential)
		{
			return;
		}
		foreach (Item item in ItemsToGrabMenu.actualInventory)
		{
			if (item != null)
			{
				Item leftOver = Game1.player.addItemToInventory(item);
				if (leftOver != null)
				{
					Game1.createItemDebris(leftOver, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
				}
			}
		}
	}
}
