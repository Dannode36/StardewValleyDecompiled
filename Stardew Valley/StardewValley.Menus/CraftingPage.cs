using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.Inventories;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Menus;

public class CraftingPage : IClickableMenu
{
	public const int howManyRecipesFitOnPage = 40;

	public const int numInRow = 10;

	public const int numInCol = 4;

	public const int region_upArrow = 88;

	public const int region_downArrow = 89;

	public const int region_craftingSelectionArea = 8000;

	public const int region_craftingModifier = 200;

	public string hoverText = "";

	public Item hoverItem;

	public Item lastCookingHover;

	public InventoryMenu inventory;

	public Item heldItem;

	[SkipForClickableAggregation]
	public List<Dictionary<ClickableTextureComponent, CraftingRecipe>> pagesOfCraftingRecipes = new List<Dictionary<ClickableTextureComponent, CraftingRecipe>>();

	public int currentCraftingPage;

	public CraftingRecipe hoverRecipe;

	public ClickableTextureComponent upButton;

	public ClickableTextureComponent downButton;

	public bool cooking;

	public ClickableTextureComponent trashCan;

	public ClickableComponent dropItemInvisibleButton;

	public float trashCanLidRotation;

	public List<IInventory> _materialContainers;

	protected bool _standaloneMenu;

	public int hoverAmount;

	public List<ClickableComponent> currentPageClickableComponents = new List<ClickableComponent>();

	private string hoverTitle = "";

	public CraftingPage(int x, int y, int width, int height, bool cooking = false, bool standaloneMenu = false, List<IInventory> materialContainers = null)
		: base(x, y, width, height)
	{
		_standaloneMenu = standaloneMenu;
		this.cooking = cooking;
		inventory = new InventoryMenu(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 - 16, playerInventory: false);
		inventory.showGrayedOutSlots = true;
		currentPageClickableComponents = new List<ClickableComponent>();
		foreach (ClickableComponent item in inventory.GetBorder(InventoryMenu.BorderSide.Top))
		{
			item.upNeighborID = -99998;
		}
		_materialContainers = materialContainers;
		if (_standaloneMenu)
		{
			initializeUpperRightCloseButton();
		}
		trashCan = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 192 - 32 - IClickableMenu.borderWidth - 104, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f)
		{
			myID = 106
		};
		dropItemInvisibleButton = new ClickableComponent(new Rectangle(xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, trashCan.bounds.Y, 64, 64), "")
		{
			myID = 107,
			rightNeighborID = 0
		};
		if (_standaloneMenu)
		{
			Game1.playSound("bigSelect");
		}
		RepositionElements();
		if (Game1.options.SnappyMenus)
		{
			snapToDefaultClickableComponent();
		}
	}

	/// <summary>Get the recipes to display in the menu.</summary>
	protected virtual List<string> GetRecipesToDisplay()
	{
		List<string> recipes = new List<string>();
		if (!cooking)
		{
			foreach (string key in CraftingRecipe.craftingRecipes.Keys)
			{
				if (Game1.player.craftingRecipes.ContainsKey(key))
				{
					recipes.Add(key);
				}
			}
		}
		else
		{
			foreach (string key in CraftingRecipe.cookingRecipes.Keys)
			{
				if (!key.Equals("Moss Soup"))
				{
					recipes.Add(key);
				}
			}
			recipes.Sort(delegate(string a, string b)
			{
				int num = -1;
				int value = -1;
				if (a != null && CraftingRecipe.cookingRecipes.TryGetValue(a, out var value2))
				{
					num = ArgUtility.GetInt(value2.Split('/'), 2, -1);
				}
				if (b != null && CraftingRecipe.cookingRecipes.TryGetValue(b, out var value3))
				{
					value = ArgUtility.GetInt(value3.Split('/'), 2, -1);
				}
				return num.CompareTo(value);
			});
			recipes.Add("Moss Soup");
		}
		return recipes;
	}

	protected virtual IList<Item> getContainerContents()
	{
		if (_materialContainers == null)
		{
			return null;
		}
		List<Item> items = new List<Item>();
		foreach (IInventory container in _materialContainers)
		{
			items.AddRange(container);
		}
		return items;
	}

	private int craftingPageY()
	{
		return yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth - 16;
	}

	private ClickableTextureComponent[,] createNewPageLayout()
	{
		return new ClickableTextureComponent[10, 4];
	}

	private Dictionary<ClickableTextureComponent, CraftingRecipe> createNewPage()
	{
		Dictionary<ClickableTextureComponent, CraftingRecipe> page = new Dictionary<ClickableTextureComponent, CraftingRecipe>();
		pagesOfCraftingRecipes.Add(page);
		return page;
	}

	private bool spaceOccupied(ClickableTextureComponent[,] pageLayout, int x, int y, CraftingRecipe recipe)
	{
		if (pageLayout[x, y] != null)
		{
			return true;
		}
		if (!recipe.bigCraftable)
		{
			return false;
		}
		if (y + 1 < 4)
		{
			return pageLayout[x, y + 1] != null;
		}
		return true;
	}

	private void layoutRecipes(List<string> playerRecipes)
	{
		int craftingPageX = xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth - 16;
		int spaceBetweenCraftingIcons = 8;
		Dictionary<ClickableTextureComponent, CraftingRecipe> currentPage = createNewPage();
		int x = 0;
		int y = 0;
		int i = 0;
		ClickableTextureComponent[,] pageLayout = createNewPageLayout();
		List<ClickableTextureComponent[,]> pageLayouts = new List<ClickableTextureComponent[,]>();
		pageLayouts.Add(pageLayout);
		foreach (string playerRecipe in playerRecipes)
		{
			i++;
			CraftingRecipe recipe = new CraftingRecipe(playerRecipe, cooking);
			while (spaceOccupied(pageLayout, x, y, recipe))
			{
				x++;
				if (x >= 10)
				{
					x = 0;
					y++;
					if (y >= 4)
					{
						currentPage = createNewPage();
						pageLayout = createNewPageLayout();
						pageLayouts.Add(pageLayout);
						x = 0;
						y = 0;
					}
				}
			}
			int id = 200 + i;
			string itemId = recipe.getIndexOfMenuView();
			ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(recipe.bigCraftable ? ("(BC)" + itemId) : itemId);
			Texture2D texture = dataOrErrorItem.GetTexture();
			Rectangle sourceRect = dataOrErrorItem.GetSourceRect();
			ClickableTextureComponent component = new ClickableTextureComponent("", new Rectangle(craftingPageX + x * (64 + spaceBetweenCraftingIcons), craftingPageY() + y * 72, 64, recipe.bigCraftable ? 128 : 64), null, (cooking && !Game1.player.cookingRecipes.ContainsKey(recipe.name)) ? "ghosted" : "", texture, sourceRect, 4f)
			{
				myID = id,
				rightNeighborID = -99998,
				leftNeighborID = -99998,
				upNeighborID = -99998,
				downNeighborID = -99998,
				fullyImmutable = true,
				region = 8000
			};
			currentPage.Add(component, recipe);
			pageLayout[x, y] = component;
			if (recipe.bigCraftable)
			{
				pageLayout[x, y + 1] = component;
			}
		}
	}

	protected override void noSnappedComponentFound(int direction, int oldRegion, int oldID)
	{
		base.noSnappedComponentFound(direction, oldRegion, oldID);
		if (oldRegion == 8000 && direction == 2)
		{
			currentlySnappedComponent = getComponentWithID(oldID % 10);
			currentlySnappedComponent.upNeighborID = oldID;
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		currentlySnappedComponent = ((currentCraftingPage < pagesOfCraftingRecipes.Count) ? pagesOfCraftingRecipes[currentCraftingPage].First().Key : null);
		snapCursorToCurrentSnappedComponent();
	}

	protected override void actionOnRegionChange(int oldRegion, int newRegion)
	{
		base.actionOnRegionChange(oldRegion, newRegion);
		if (newRegion != 9000 || oldRegion == 0)
		{
			return;
		}
		for (int i = 0; i < 10; i++)
		{
			if (inventory.inventory.Count > i)
			{
				inventory.inventory[i].upNeighborID = currentlySnappedComponent.upNeighborID;
			}
		}
	}

	public override void receiveKeyPress(Keys key)
	{
		base.receiveKeyPress(key);
		if (key.Equals(Keys.Delete) && heldItem != null && heldItem.canBeTrashed())
		{
			Utility.trashItem(heldItem);
			heldItem = null;
		}
		if (Game1.isAnyGamePadButtonBeingPressed() && Game1.options.doesInputListContain(Game1.options.menuButton, key) && heldItem != null)
		{
			Game1.setMousePosition(trashCan.bounds.Center);
		}
	}

	public override void receiveScrollWheelAction(int direction)
	{
		base.receiveScrollWheelAction(direction);
		if (direction > 0 && currentCraftingPage > 0)
		{
			currentCraftingPage--;
			_UpdateCurrentPageButtons();
			Game1.playSound("shwip");
			if (Game1.options.SnappyMenus)
			{
				setCurrentlySnappedComponentTo(88);
				snapCursorToCurrentSnappedComponent();
			}
		}
		else if (direction < 0 && currentCraftingPage < pagesOfCraftingRecipes.Count - 1)
		{
			currentCraftingPage++;
			_UpdateCurrentPageButtons();
			Game1.playSound("shwip");
			if (Game1.options.SnappyMenus)
			{
				setCurrentlySnappedComponentTo(89);
				snapCursorToCurrentSnappedComponent();
			}
		}
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		RepositionElements();
	}

	public virtual void RepositionElements()
	{
		List<string> playerRecipes = GetRecipesToDisplay();
		pagesOfCraftingRecipes.Clear();
		layoutRecipes(playerRecipes);
		if (pagesOfCraftingRecipes.Count > 1)
		{
			upButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 768 + 32, craftingPageY(), 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12), 0.8f)
			{
				myID = 88,
				downNeighborID = 89,
				rightNeighborID = 106,
				leftNeighborID = -99998
			};
			downButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 768 + 32, craftingPageY() + 192 + 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11), 0.8f)
			{
				myID = 89,
				upNeighborID = 88,
				rightNeighborID = 106,
				leftNeighborID = -99998
			};
		}
		inventory.SetPosition(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 - 16);
		trashCan.bounds.X = xPositionOnScreen + width + 4;
		trashCan.bounds.Y = yPositionOnScreen + height - 192 - 32 - IClickableMenu.borderWidth - 104;
		dropItemInvisibleButton.bounds.X = xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64;
		dropItemInvisibleButton.bounds.Y = trashCan.bounds.Y;
		if (upButton != null)
		{
			upButton.bounds.X = xPositionOnScreen + 768 + 32;
			upButton.bounds.Y = craftingPageY();
		}
		if (downButton != null)
		{
			downButton.bounds.X = xPositionOnScreen + 768 + 32;
			downButton.bounds.Y = craftingPageY() + 192 + 32;
		}
		_UpdateCurrentPageButtons();
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		base.receiveLeftClick(x, y);
		heldItem = inventory.leftClick(x, y, heldItem);
		if (upButton != null && upButton.containsPoint(x, y) && currentCraftingPage > 0)
		{
			Game1.playSound("coin");
			currentCraftingPage = Math.Max(0, currentCraftingPage - 1);
			_UpdateCurrentPageButtons();
			upButton.scale = upButton.baseScale;
		}
		if (downButton != null && downButton.containsPoint(x, y) && currentCraftingPage < pagesOfCraftingRecipes.Count - 1)
		{
			Game1.playSound("coin");
			currentCraftingPage = Math.Min(pagesOfCraftingRecipes.Count - 1, currentCraftingPage + 1);
			_UpdateCurrentPageButtons();
			downButton.scale = downButton.baseScale;
		}
		foreach (ClickableTextureComponent c in pagesOfCraftingRecipes[currentCraftingPage].Keys)
		{
			int times = ((!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) ? 1 : (Game1.oldKBState.IsKeyDown(Keys.LeftControl) ? 25 : 5));
			for (int i = 0; i < times; i++)
			{
				if (c.containsPoint(x, y, 4) && !c.hoverText.Equals("ghosted") && pagesOfCraftingRecipes[currentCraftingPage][c].doesFarmerHaveIngredientsInInventory(getContainerContents()))
				{
					clickCraftingRecipe(c, i == 0);
				}
			}
			if (heldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift) && heldItem.maximumStackSize() == 1 && Game1.player.couldInventoryAcceptThisItem(heldItem))
			{
				Game1.player.addItemToInventoryBool(heldItem);
				heldItem = null;
			}
		}
		if (trashCan != null && trashCan.containsPoint(x, y) && heldItem != null && heldItem.canBeTrashed())
		{
			Utility.trashItem(heldItem);
			heldItem = null;
		}
		else if (heldItem != null && !isWithinBounds(x, y) && heldItem.canBeTrashed())
		{
			Game1.playSound("throwDownITem");
			Game1.createItemDebris(heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
			heldItem = null;
		}
	}

	protected void _UpdateCurrentPageButtons()
	{
		currentPageClickableComponents.Clear();
		foreach (ClickableTextureComponent component in pagesOfCraftingRecipes[currentCraftingPage].Keys)
		{
			currentPageClickableComponents.Add(component);
		}
		populateClickableComponentList();
	}

	private void clickCraftingRecipe(ClickableTextureComponent c, bool playSound = true)
	{
		CraftingRecipe recipe = pagesOfCraftingRecipes[currentCraftingPage][c];
		Item crafted = recipe.createItem();
		List<KeyValuePair<string, int>> seasoning = null;
		if (cooking && crafted.Quality == 0)
		{
			seasoning = new List<KeyValuePair<string, int>>();
			seasoning.Add(new KeyValuePair<string, int>("917", 1));
			if (CraftingRecipe.DoesFarmerHaveAdditionalIngredientsInInventory(seasoning, getContainerContents()))
			{
				crafted.Quality = 2;
			}
			else
			{
				seasoning = null;
			}
		}
		if (heldItem == null)
		{
			recipe.consumeIngredients(_materialContainers);
			heldItem = crafted;
			if (playSound)
			{
				Game1.playSound("coin");
			}
		}
		else
		{
			if (!(heldItem.Name == crafted.Name) || !heldItem.getOne().canStackWith(crafted.getOne()) || heldItem.Stack + recipe.numberProducedPerCraft - 1 >= heldItem.maximumStackSize())
			{
				return;
			}
			heldItem.Stack += recipe.numberProducedPerCraft;
			recipe.consumeIngredients(_materialContainers);
			if (playSound)
			{
				Game1.playSound("coin");
			}
		}
		if (seasoning != null)
		{
			if (playSound)
			{
				Game1.playSound("breathin");
			}
			CraftingRecipe.ConsumeAdditionalIngredients(seasoning, _materialContainers);
			if (!CraftingRecipe.DoesFarmerHaveAdditionalIngredientsInInventory(seasoning, getContainerContents()))
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Seasoning_UsedLast"));
			}
		}
		Game1.player.checkForQuestComplete(null, -1, -1, crafted, null, 2);
		if (!cooking && Game1.player.craftingRecipes.ContainsKey(recipe.name))
		{
			Game1.player.craftingRecipes[recipe.name] += recipe.numberProducedPerCraft;
		}
		if (cooking)
		{
			Game1.player.cookedRecipe(heldItem.ItemId);
			Game1.stats.checkForCookingAchievements();
		}
		else
		{
			Game1.stats.checkForCraftingAchievements();
		}
		if (Game1.options.gamepadControls && heldItem != null && Game1.player.couldInventoryAcceptThisItem(heldItem))
		{
			Game1.player.addItemToInventoryBool(heldItem);
			heldItem = null;
		}
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		heldItem = inventory.rightClick(x, y, heldItem);
		foreach (ClickableTextureComponent c in pagesOfCraftingRecipes[currentCraftingPage].Keys)
		{
			if (c.containsPoint(x, y) && !c.hoverText.Equals("ghosted") && pagesOfCraftingRecipes[currentCraftingPage][c].doesFarmerHaveIngredientsInInventory(getContainerContents()))
			{
				clickCraftingRecipe(c);
			}
		}
	}

	public override void performHoverAction(int x, int y)
	{
		CraftingRecipe prevHoverRecipe = hoverRecipe;
		base.performHoverAction(x, y);
		hoverTitle = "";
		hoverText = "";
		hoverRecipe = null;
		hoverItem = inventory.hover(x, y, hoverItem);
		hoverAmount = -1;
		if (hoverItem != null)
		{
			hoverTitle = inventory.hoverTitle;
			hoverText = inventory.hoverText;
		}
		foreach (ClickableTextureComponent c in pagesOfCraftingRecipes[currentCraftingPage].Keys)
		{
			if (c.containsPoint(x, y, 4))
			{
				if (c.hoverText.Equals("ghosted"))
				{
					hoverText = "???";
					continue;
				}
				hoverRecipe = pagesOfCraftingRecipes[currentCraftingPage][c];
				if (prevHoverRecipe == null || prevHoverRecipe.name != hoverRecipe.name)
				{
					lastCookingHover = hoverRecipe.createItem();
				}
				c.scale = Math.Min(c.scale + 0.02f, c.baseScale + 0.1f);
			}
			else
			{
				c.scale = Math.Max(c.scale - 0.02f, c.baseScale);
			}
		}
		if (upButton != null)
		{
			if (upButton.containsPoint(x, y))
			{
				upButton.scale = Math.Min(upButton.scale + 0.02f, upButton.baseScale + 0.1f);
			}
			else
			{
				upButton.scale = Math.Max(upButton.scale - 0.02f, upButton.baseScale);
			}
		}
		if (downButton != null)
		{
			if (downButton.containsPoint(x, y))
			{
				downButton.scale = Math.Min(downButton.scale + 0.02f, downButton.baseScale + 0.1f);
			}
			else
			{
				downButton.scale = Math.Max(downButton.scale - 0.02f, downButton.baseScale);
			}
		}
		if (trashCan == null)
		{
			return;
		}
		if (trashCan.containsPoint(x, y))
		{
			if (trashCanLidRotation <= 0f)
			{
				Game1.playSound("trashcanlid");
			}
			trashCanLidRotation = Math.Min(trashCanLidRotation + (float)Math.PI / 48f, (float)Math.PI / 2f);
			if (heldItem != null && Utility.getTrashReclamationPrice(heldItem, Game1.player) > 0)
			{
				hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
				hoverAmount = Utility.getTrashReclamationPrice(heldItem, Game1.player);
			}
		}
		else
		{
			trashCanLidRotation = Math.Max(trashCanLidRotation - (float)Math.PI / 48f, 0f);
		}
	}

	public override bool readyToClose()
	{
		return heldItem == null;
	}

	public override void draw(SpriteBatch b)
	{
		if (_standaloneMenu)
		{
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
		}
		drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256);
		inventory.draw(b);
		if (trashCan != null)
		{
			trashCan.draw(b);
			b.Draw(Game1.mouseCursors, new Vector2(trashCan.bounds.X + 60, trashCan.bounds.Y + 40), new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10), Color.White, trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
		}
		b.End();
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		foreach (ClickableTextureComponent c in pagesOfCraftingRecipes[currentCraftingPage].Keys)
		{
			if (c.hoverText.Equals("ghosted"))
			{
				c.draw(b, Color.Black * 0.35f, 0.89f);
			}
			else if (!pagesOfCraftingRecipes[currentCraftingPage][c].doesFarmerHaveIngredientsInInventory(getContainerContents()))
			{
				c.draw(b, Color.DimGray * 0.4f, 0.89f);
				if (pagesOfCraftingRecipes[currentCraftingPage][c].numberProducedPerCraft > 1)
				{
					NumberSprite.draw(pagesOfCraftingRecipes[currentCraftingPage][c].numberProducedPerCraft, b, new Vector2(c.bounds.X + 64 - 2, c.bounds.Y + 64 - 2), Color.LightGray * 0.75f, 0.5f * (c.scale / 4f), 0.97f, 1f, 0);
				}
			}
			else
			{
				c.draw(b);
				if (pagesOfCraftingRecipes[currentCraftingPage][c].numberProducedPerCraft > 1)
				{
					NumberSprite.draw(pagesOfCraftingRecipes[currentCraftingPage][c].numberProducedPerCraft, b, new Vector2(c.bounds.X + 64 - 2, c.bounds.Y + 64 - 2), Color.White, 0.5f * (c.scale / 4f), 0.97f, 1f, 0);
				}
			}
		}
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (hoverItem != null)
		{
			IClickableMenu.drawToolTip(b, hoverText, hoverTitle, hoverItem, heldItem != null);
		}
		else if (!string.IsNullOrEmpty(hoverText))
		{
			if (hoverAmount > 0)
			{
				IClickableMenu.drawToolTip(b, hoverText, hoverTitle, null, heldItem: true, -1, 0, null, -1, null, hoverAmount);
			}
			else
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont, (heldItem != null) ? 64 : 0, (heldItem != null) ? 64 : 0);
			}
		}
		heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
		base.draw(b);
		if (downButton != null && currentCraftingPage < pagesOfCraftingRecipes.Count - 1)
		{
			downButton.draw(b);
		}
		if (upButton != null && currentCraftingPage > 0)
		{
			upButton.draw(b);
		}
		if (_standaloneMenu)
		{
			Game1.mouseCursorTransparency = 1f;
			drawMouse(b);
		}
		if (hoverRecipe == null)
		{
			return;
		}
		string[] buffIconsToDisplay = null;
		if (cooking && lastCookingHover.HasTypeObject() && Game1.objectData.TryGetValue(lastCookingHover.ItemId, out var rawHoverData))
		{
			BuffEffects effects = new BuffEffects();
			foreach (Buff buff in Object.TryCreateBuffsFromData(rawHoverData, lastCookingHover.Name, lastCookingHover.DisplayName, 1f, lastCookingHover.ModifyItemBuffs))
			{
				effects.Add(buff.effects);
			}
			if (effects.HasAnyValue())
			{
				buffIconsToDisplay = effects.ToLegacyAttributeFormat();
			}
		}
		IClickableMenu.drawHoverText(b, " ", Game1.smallFont, (heldItem != null) ? 48 : 0, (heldItem != null) ? 48 : 0, -1, hoverRecipe.DisplayName + ((hoverRecipe.numberProducedPerCraft > 1) ? (" x" + hoverRecipe.numberProducedPerCraft) : ""), -1, buffIconsToDisplay, lastCookingHover, 0, null, -1, -1, -1, 1f, hoverRecipe, getContainerContents());
	}

	protected override bool _ShouldAutoSnapPrioritizeAlignedElements()
	{
		return false;
	}

	public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
	{
		if ((a == downButton || a == upButton) && direction == 3 && b.region != 8000)
		{
			return false;
		}
		if (a.region == 8000 && (direction == 3 || direction == 1) && b.region == 9000)
		{
			return false;
		}
		if (a.region == 8000 && direction == 2 && (b == upButton || b == downButton))
		{
			return false;
		}
		return base.IsAutomaticSnapValid(direction, a, b);
	}

	public override void emergencyShutDown()
	{
		base.emergencyShutDown();
		if (heldItem != null)
		{
			Item item = heldItem;
			heldItem = null;
			Utility.CollectOrDrop(item);
		}
	}
}
