using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Enchantments;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewValley.Menus;

public class ForgeMenu : MenuWithInventory
{
	public enum CraftState
	{
		MissingIngredients,
		MissingShards,
		Valid,
		InvalidRecipe
	}

	protected int _timeUntilCraft;

	protected int _clankEffectTimer;

	protected int _sparklingTimer;

	public const int region_leftIngredient = 998;

	public const int region_rightIngredient = 997;

	public const int region_startButton = 996;

	public const int region_resultItem = 995;

	public const int region_unforgeButton = 994;

	public ClickableTextureComponent craftResultDisplay;

	public ClickableTextureComponent leftIngredientSpot;

	public ClickableTextureComponent rightIngredientSpot;

	public ClickableTextureComponent startTailoringButton;

	public ClickableComponent unforgeButton;

	public List<ClickableComponent> equipmentIcons = new List<ClickableComponent>();

	public const int region_ring_1 = 110;

	public const int region_ring_2 = 111;

	public const int CRAFT_TIME = 1600;

	public Texture2D forgeTextures;

	protected Dictionary<Item, bool> _highlightDictionary;

	protected TemporaryAnimatedSpriteList tempSprites = new TemporaryAnimatedSpriteList();

	private bool unforging;

	protected string displayedDescription = "";

	protected CraftState _craftState;

	public Vector2 questionMarkOffset;

	public ForgeMenu()
		: base(null, okButton: true, trashCan: true, 12, 132)
	{
		Game1.playSound("bigSelect");
		if (yPositionOnScreen == IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
		{
			movePosition(0, -IClickableMenu.spaceToClearTopBorder);
		}
		inventory.highlightMethod = HighlightItems;
		forgeTextures = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\ForgeMenu");
		_CreateButtons();
		if (trashCan != null)
		{
			trashCan.myID = 106;
		}
		if (okButton != null)
		{
			okButton.leftNeighborID = 11;
		}
		if (Game1.options.SnappyMenus)
		{
			populateClickableComponentList();
			snapToDefaultClickableComponent();
		}
		_ValidateCraft();
	}

	protected void _CreateButtons()
	{
		leftIngredientSpot = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 204, yPositionOnScreen + 212, 64, 64), forgeTextures, new Rectangle(142, 0, 16, 16), 4f)
		{
			myID = 998,
			downNeighborID = -99998,
			leftNeighborID = 110,
			rightNeighborID = 997,
			item = ((leftIngredientSpot != null) ? leftIngredientSpot.item : null),
			fullyImmutable = true
		};
		rightIngredientSpot = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 348, yPositionOnScreen + 212, 64, 64), forgeTextures, new Rectangle(142, 0, 16, 16), 4f)
		{
			myID = 997,
			downNeighborID = 996,
			leftNeighborID = 998,
			rightNeighborID = 994,
			item = ((rightIngredientSpot != null) ? rightIngredientSpot.item : null),
			fullyImmutable = true
		};
		startTailoringButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 204, yPositionOnScreen + 308, 52, 56), forgeTextures, new Rectangle(0, 80, 13, 14), 4f)
		{
			myID = 996,
			downNeighborID = -99998,
			leftNeighborID = 111,
			rightNeighborID = 994,
			upNeighborID = 998,
			item = ((startTailoringButton != null) ? startTailoringButton.item : null),
			fullyImmutable = true
		};
		unforgeButton = new ClickableComponent(new Rectangle(xPositionOnScreen + 484, yPositionOnScreen + 312, 40, 44), "Unforge")
		{
			myID = 994,
			downNeighborID = -99998,
			leftNeighborID = 996,
			rightNeighborID = 995,
			upNeighborID = 997,
			fullyImmutable = true
		};
		List<ClickableComponent> list = inventory.inventory;
		if (list != null && list.Count >= 12)
		{
			for (int i = 0; i < 12; i++)
			{
				if (inventory.inventory[i] != null)
				{
					inventory.inventory[i].upNeighborID = -99998;
				}
			}
		}
		craftResultDisplay = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 4 + 660, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 8 + 232, 64, 64), forgeTextures, new Rectangle(0, 208, 16, 16), 4f)
		{
			myID = 995,
			downNeighborID = -99998,
			leftNeighborID = 996,
			upNeighborID = 997,
			item = ((craftResultDisplay != null) ? craftResultDisplay.item : null)
		};
		equipmentIcons = new List<ClickableComponent>();
		equipmentIcons.Add(new ClickableComponent(new Rectangle(0, 0, 64, 64), "Ring1")
		{
			myID = 110,
			leftNeighborID = -99998,
			downNeighborID = -99998,
			upNeighborID = -99998,
			rightNeighborID = -99998
		});
		equipmentIcons.Add(new ClickableComponent(new Rectangle(0, 0, 64, 64), "Ring2")
		{
			myID = 111,
			upNeighborID = -99998,
			downNeighborID = -99998,
			rightNeighborID = -99998,
			leftNeighborID = -99998
		});
		for (int i = 0; i < equipmentIcons.Count; i++)
		{
			equipmentIcons[i].bounds.X = xPositionOnScreen - 64 + 9;
			equipmentIcons[i].bounds.Y = yPositionOnScreen + 192 + i * 64;
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		currentlySnappedComponent = getComponentWithID(0);
		snapCursorToCurrentSnappedComponent();
	}

	public bool IsBusy()
	{
		if (_timeUntilCraft <= 0)
		{
			return _sparklingTimer > 0;
		}
		return true;
	}

	public override bool readyToClose()
	{
		if (base.readyToClose() && base.heldItem == null)
		{
			return !IsBusy();
		}
		return false;
	}

	public bool HighlightItems(Item i)
	{
		if (i == null)
		{
			return false;
		}
		if (i != null && !IsValidCraftIngredient(i))
		{
			return false;
		}
		if (_highlightDictionary == null)
		{
			GenerateHighlightDictionary();
		}
		if (!_highlightDictionary.ContainsKey(i))
		{
			_highlightDictionary = null;
			GenerateHighlightDictionary();
		}
		return _highlightDictionary[i];
	}

	public virtual void GenerateHighlightDictionary()
	{
		_highlightDictionary = new Dictionary<Item, bool>();
		List<Item> item_list = new List<Item>(inventory.actualInventory);
		if (Game1.player.leftRing.Value != null)
		{
			item_list.Add(Game1.player.leftRing.Value);
		}
		if (Game1.player.rightRing.Value != null)
		{
			item_list.Add(Game1.player.rightRing.Value);
		}
		foreach (Item item in item_list)
		{
			if (item == null)
			{
				continue;
			}
			if (item.QualifiedItemId == "(O)848")
			{
				_highlightDictionary[item] = true;
			}
			else if (leftIngredientSpot.item == null && rightIngredientSpot.item == null)
			{
				bool valid = false;
				if (item is Ring)
				{
					valid = true;
				}
				if (item is Tool tool && BaseEnchantment.GetAvailableEnchantmentsForItem(tool).Count > 0)
				{
					valid = true;
				}
				if (BaseEnchantment.GetEnchantmentFromItem(null, item) != null)
				{
					valid = true;
				}
				_highlightDictionary[item] = valid;
			}
			else if (leftIngredientSpot.item != null && rightIngredientSpot.item != null)
			{
				_highlightDictionary[item] = false;
			}
			else if (leftIngredientSpot.item != null)
			{
				_highlightDictionary[item] = IsValidCraft(leftIngredientSpot.item, item);
			}
			else
			{
				_highlightDictionary[item] = IsValidCraft(item, rightIngredientSpot.item);
			}
		}
	}

	private void _leftIngredientSpotClicked()
	{
		Item old_item = leftIngredientSpot.item;
		if ((base.heldItem == null || IsValidCraftIngredient(base.heldItem)) && (base.heldItem == null || base.heldItem is Tool || base.heldItem is Ring))
		{
			Game1.playSound("stoneStep");
			leftIngredientSpot.item = base.heldItem;
			base.heldItem = old_item;
			_highlightDictionary = null;
			_ValidateCraft();
		}
	}

	public virtual bool IsValidCraftIngredient(Item item)
	{
		if (!item.canBeTrashed() && (!(item is Tool tool) || BaseEnchantment.GetAvailableEnchantmentsForItem(tool).Count <= 0))
		{
			return false;
		}
		return true;
	}

	private void _rightIngredientSpotClicked()
	{
		Item old_item = rightIngredientSpot.item;
		if ((base.heldItem == null || IsValidCraftIngredient(base.heldItem)) && !(base.heldItem?.QualifiedItemId == "(O)848"))
		{
			Game1.playSound("stoneStep");
			rightIngredientSpot.item = base.heldItem;
			base.heldItem = old_item;
			_highlightDictionary = null;
			_ValidateCraft();
		}
	}

	public override void receiveKeyPress(Keys key)
	{
		if (key == Keys.Delete)
		{
			if (base.heldItem != null && IsValidCraftIngredient(base.heldItem))
			{
				Utility.trashItem(base.heldItem);
				base.heldItem = null;
			}
		}
		else
		{
			base.receiveKeyPress(key);
		}
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		Item old_held_item = base.heldItem;
		base.receiveLeftClick(x, y, playSound: true);
		foreach (ClickableComponent c in equipmentIcons)
		{
			if (!c.containsPoint(x, y))
			{
				continue;
			}
			string name = c.name;
			if (!(name == "Ring1"))
			{
				if (!(name == "Ring2") || (!HighlightItems(Game1.player.rightRing.Value) && Game1.player.rightRing.Value != null))
				{
					return;
				}
				Item item_to_place = base.heldItem;
				if (item_to_place != Game1.player.rightRing.Value && (item_to_place == null || item_to_place is Ring))
				{
					base.heldItem = Game1.player.Equip(item_to_place as Ring, Game1.player.rightRing);
					if (Game1.player.rightRing.Value != null)
					{
						Game1.playSound("crit");
					}
					else if (base.heldItem != null)
					{
						Game1.playSound("dwop");
					}
					_highlightDictionary = null;
					_ValidateCraft();
				}
			}
			else
			{
				if (!HighlightItems(Game1.player.leftRing.Value) && Game1.player.leftRing.Value != null)
				{
					return;
				}
				Item item_to_place = base.heldItem;
				if (item_to_place != Game1.player.leftRing.Value && (item_to_place == null || item_to_place is Ring))
				{
					base.heldItem = Game1.player.Equip(item_to_place as Ring, Game1.player.leftRing);
					if (Game1.player.leftRing.Value != null)
					{
						Game1.playSound("crit");
					}
					else if (base.heldItem != null)
					{
						Game1.playSound("dwop");
					}
					_highlightDictionary = null;
					_ValidateCraft();
				}
			}
			return;
		}
		if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && old_held_item != base.heldItem && base.heldItem != null)
		{
			if (base.heldItem is Tool || (base.heldItem is Ring && leftIngredientSpot.item == null))
			{
				_leftIngredientSpotClicked();
			}
			else
			{
				_rightIngredientSpotClicked();
			}
		}
		if (IsBusy())
		{
			return;
		}
		if (leftIngredientSpot.containsPoint(x, y))
		{
			_leftIngredientSpotClicked();
			if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && base.heldItem != null)
			{
				if (Game1.player.IsEquippedItem(base.heldItem))
				{
					base.heldItem = null;
				}
				else
				{
					base.heldItem = inventory.tryToAddItem(base.heldItem, "");
				}
			}
		}
		else if (rightIngredientSpot.containsPoint(x, y))
		{
			_rightIngredientSpotClicked();
			if (Game1.GetKeyboardState().IsKeyDown(Keys.LeftShift) && base.heldItem != null)
			{
				if (Game1.player.IsEquippedItem(base.heldItem))
				{
					base.heldItem = null;
				}
				else
				{
					base.heldItem = inventory.tryToAddItem(base.heldItem, "");
				}
			}
		}
		else if (startTailoringButton.containsPoint(x, y))
		{
			if (base.heldItem == null)
			{
				bool fail = false;
				if (!CanFitCraftedItem())
				{
					Game1.playSound("cancel");
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
					_timeUntilCraft = 0;
					fail = true;
				}
				if (!fail && IsValidCraft(leftIngredientSpot.item, rightIngredientSpot.item) && Game1.player.Items.ContainsId("(O)848", GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item)))
				{
					Game1.playSound("bigSelect");
					startTailoringButton.scale = startTailoringButton.baseScale;
					_timeUntilCraft = 1600;
					_clankEffectTimer = 300;
					_UpdateDescriptionText();
					int crystals = GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item);
					for (int i = 0; i < crystals; i++)
					{
						tempSprites.Add(new TemporaryAnimatedSprite("", new Rectangle(143, 17, 14, 15), new Vector2(xPositionOnScreen + 276, yPositionOnScreen + 300), flipped: false, 0.1f, Color.White)
						{
							texture = forgeTextures,
							motion = new Vector2(-4f, -4f),
							scale = 4f,
							layerDepth = 1f,
							startSound = "boulderCrack",
							delayBeforeAnimationStart = 1400 / crystals * i
						});
					}
					if (rightIngredientSpot.item?.QualifiedItemId == "(O)74")
					{
						_sparklingTimer = 900;
						Rectangle r = leftIngredientSpot.bounds;
						r.Offset(-32, -32);
						TemporaryAnimatedSpriteList sparkles = Utility.sparkleWithinArea(r, 6, Color.White, 80, 1600);
						sparkles[0].startSound = "discoverMineral";
						tempSprites.AddRange(sparkles);
						r = rightIngredientSpot.bounds;
						r.Inflate(-16, -16);
						int num = 30;
						for (int i = 0; i < num; i++)
						{
							Vector2 position = Utility.getRandomPositionInThisRectangle(r, Game1.random);
							tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 48, 2, 2), position, flipped: false, 0f, Color.White)
							{
								motion = new Vector2(-4f, 0f),
								yPeriodic = true,
								yPeriodicRange = 16f,
								yPeriodicLoopTime = 1200f,
								scale = 4f,
								layerDepth = 1f,
								animationLength = 12,
								interval = Game1.random.Next(20, 40),
								totalNumberOfLoops = 1,
								delayBeforeAnimationStart = _clankEffectTimer / num * i
							});
						}
					}
				}
				else
				{
					Game1.playSound("sell");
				}
			}
			else
			{
				Game1.playSound("sell");
			}
		}
		else if (unforgeButton.containsPoint(x, y))
		{
			if (rightIngredientSpot.item == null)
			{
				if (IsValidUnforge())
				{
					if (leftIngredientSpot.item is MeleeWeapon leftWeapon && !Game1.player.couldInventoryAcceptThisItem("(O)848", leftWeapon.GetTotalForgeLevels() * 5 + (leftWeapon.GetTotalForgeLevels() - 1) * 2))
					{
						displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_noroom");
						Game1.playSound("cancel");
					}
					else if (leftIngredientSpot.item is CombinedRing && Game1.player.freeSpotsInInventory() < 2)
					{
						displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_noroom");
						Game1.playSound("cancel");
					}
					else
					{
						unforging = true;
						_timeUntilCraft = 1600;
						int crystals = GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item) / 2;
						for (int i = 0; i < crystals; i++)
						{
							Vector2 motion = new Vector2(Game1.random.Next(-4, 5), Game1.random.Next(-4, 5));
							if (motion.X == 0f && motion.Y == 0f)
							{
								motion = new Vector2(-4f, -4f);
							}
							tempSprites.Add(new TemporaryAnimatedSprite("", new Rectangle(143, 17, 14, 15), new Vector2(leftIngredientSpot.bounds.X, leftIngredientSpot.bounds.Y), flipped: false, 0.1f, Color.White)
							{
								alpha = 0.01f,
								alphaFade = -0.1f,
								alphaFadeFade = -0.005f,
								texture = forgeTextures,
								motion = motion,
								scale = 4f,
								layerDepth = 1f,
								startSound = "boulderCrack",
								delayBeforeAnimationStart = 1100 / crystals * i
							});
						}
						Game1.playSound("debuffHit");
					}
				}
				else
				{
					displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_unforge_invalid");
					Game1.playSound("cancel");
				}
			}
			else
			{
				if (IsValidUnforge(ignore_right_slot_occupancy: true))
				{
					displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_unforge_right_slot");
				}
				else
				{
					displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_unforge_invalid");
				}
				Game1.playSound("cancel");
			}
		}
		if (base.heldItem == null || isWithinBounds(x, y) || !base.heldItem.canBeTrashed())
		{
			return;
		}
		if (Game1.player.IsEquippedItem(base.heldItem))
		{
			if (base.heldItem == Game1.player.hat.Value)
			{
				Game1.player.Equip(null, Game1.player.hat);
			}
			else if (base.heldItem == Game1.player.shirtItem.Value)
			{
				Game1.player.Equip(null, Game1.player.shirtItem);
			}
			else if (base.heldItem == Game1.player.pantsItem.Value)
			{
				Game1.player.Equip(null, Game1.player.pantsItem);
			}
		}
		Game1.playSound("throwDownITem");
		Game1.createItemDebris(base.heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
		base.heldItem = null;
	}

	public virtual int GetForgeCostAtLevel(int level)
	{
		return 10 + level * 5;
	}

	public virtual int GetForgeCost(Item left_item, Item right_item)
	{
		switch (right_item?.QualifiedItemId)
		{
		case "(O)896":
		case "(O)74":
			return 20;
		case "(O)72":
			return 10;
		case "(O)852":
			return 10;
		default:
			if (!(left_item is Tool leftTool))
			{
				if (left_item is Ring)
				{
					if (!(right_item is Ring))
					{
						return 1;
					}
					return 20;
				}
				return 1;
			}
			if (!(leftTool is MeleeWeapon) || !(right_item is MeleeWeapon))
			{
				return GetForgeCostAtLevel(leftTool.GetTotalForgeLevels());
			}
			return 10;
		}
	}

	protected void _ValidateCraft()
	{
		Item left_item = leftIngredientSpot.item;
		Item right_item = rightIngredientSpot.item;
		if (left_item == null || right_item == null)
		{
			_craftState = CraftState.MissingIngredients;
		}
		else if (IsValidCraft(left_item, right_item))
		{
			_craftState = CraftState.Valid;
			Item left_item_clone = left_item.getOne();
			if (right_item?.QualifiedItemId == "(O)72")
			{
				(left_item_clone as Tool).AddEnchantment(new DiamondEnchantment());
				craftResultDisplay.item = left_item_clone;
			}
			else
			{
				craftResultDisplay.item = CraftItem(left_item_clone, right_item.getOne());
			}
		}
		else
		{
			_craftState = CraftState.InvalidRecipe;
		}
		_UpdateDescriptionText();
	}

	protected void _UpdateDescriptionText()
	{
		if (IsBusy())
		{
			displayedDescription = ((rightIngredientSpot.item?.QualifiedItemId == "(O)74") ? Game1.content.LoadString("Strings\\UI:Forge_enchanting") : Game1.content.LoadString("Strings\\UI:Forge_forging"));
			return;
		}
		switch (_craftState)
		{
		case CraftState.MissingIngredients:
			displayedDescription = (displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_description1") + Environment.NewLine + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Forge_description2"));
			break;
		case CraftState.MissingShards:
			displayedDescription = ((base.heldItem?.QualifiedItemId == "(O)848") ? Game1.content.LoadString("Strings\\UI:Forge_shards") : Game1.content.LoadString("Strings\\UI:Forge_notenoughshards"));
			break;
		case CraftState.Valid:
			displayedDescription = ((!CanFitCraftedItem()) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588") : Game1.content.LoadString("Strings\\UI:Forge_valid"));
			break;
		case CraftState.InvalidRecipe:
			displayedDescription = Game1.content.LoadString("Strings\\UI:Forge_wrongorder");
			break;
		default:
			displayedDescription = "";
			break;
		}
	}

	public virtual bool IsValidCraft(Item left_item, Item right_item)
	{
		if (left_item == null || right_item == null)
		{
			return false;
		}
		if (left_item is Tool leftTool && leftTool.CanForge(right_item))
		{
			return true;
		}
		if (left_item is Ring leftRing && right_item is Ring rightRing && leftRing.CanCombine(rightRing))
		{
			return true;
		}
		return false;
	}

	public virtual Item CraftItem(Item left_item, Item right_item, bool forReal = false)
	{
		if (left_item == null || right_item == null)
		{
			return null;
		}
		if (left_item is Tool leftTool && !leftTool.Forge(right_item, forReal))
		{
			return null;
		}
		if (left_item is Ring leftRing && right_item is Ring rightRing)
		{
			left_item = leftRing.Combine(rightRing);
		}
		return left_item;
	}

	public void SpendRightItem()
	{
		if (rightIngredientSpot.item != null)
		{
			rightIngredientSpot.item.Stack--;
			if (rightIngredientSpot.item.Stack <= 0 || rightIngredientSpot.item.maximumStackSize() == 1)
			{
				rightIngredientSpot.item = null;
			}
		}
	}

	public void SpendLeftItem()
	{
		if (leftIngredientSpot.item != null)
		{
			leftIngredientSpot.item.Stack--;
			if (leftIngredientSpot.item.Stack <= 0 || leftIngredientSpot.item.maximumStackSize() == 1)
			{
				leftIngredientSpot.item = null;
			}
		}
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		if (!IsBusy())
		{
			base.receiveRightClick(x, y, playSound: true);
		}
	}

	public override void performHoverAction(int x, int y)
	{
		if (IsBusy())
		{
			return;
		}
		hoveredItem = null;
		base.performHoverAction(x, y);
		hoverText = "";
		for (int i = 0; i < equipmentIcons.Count; i++)
		{
			if (!equipmentIcons[i].containsPoint(x, y))
			{
				continue;
			}
			string name = equipmentIcons[i].name;
			if (!(name == "Ring1"))
			{
				if (name == "Ring2")
				{
					hoveredItem = Game1.player.rightRing.Value;
				}
			}
			else
			{
				hoveredItem = Game1.player.leftRing.Value;
			}
		}
		if (craftResultDisplay.visible && craftResultDisplay.containsPoint(x, y) && craftResultDisplay.item != null)
		{
			hoveredItem = craftResultDisplay.item;
		}
		if (leftIngredientSpot.containsPoint(x, y) && leftIngredientSpot.item != null)
		{
			hoveredItem = leftIngredientSpot.item;
		}
		if (rightIngredientSpot.containsPoint(x, y) && rightIngredientSpot.item != null)
		{
			hoveredItem = rightIngredientSpot.item;
		}
		if (unforgeButton.containsPoint(x, y))
		{
			hoverText = Game1.content.LoadString("Strings\\UI:Forge_Unforge");
		}
		if (_craftState == CraftState.Valid && CanFitCraftedItem())
		{
			startTailoringButton.tryHover(x, y, 0.33f);
		}
		else
		{
			startTailoringButton.tryHover(-999, -999);
		}
	}

	public bool CanFitCraftedItem()
	{
		if (craftResultDisplay.item != null && !Utility.canItemBeAddedToThisInventoryList(craftResultDisplay.item, inventory.actualInventory))
		{
			return false;
		}
		return true;
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		int yPositionForInventory = yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16 + 128 + 4;
		inventory = new InventoryMenu(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 12, yPositionForInventory, playerInventory: false, null, inventory.highlightMethod);
		_CreateButtons();
	}

	public override void emergencyShutDown()
	{
		_OnCloseMenu();
		base.emergencyShutDown();
	}

	public override void update(GameTime time)
	{
		base.update(time);
		for (int i = tempSprites.Count - 1; i >= 0; i--)
		{
			if (tempSprites[i].update(time))
			{
				tempSprites.RemoveAt(i);
			}
		}
		if (leftIngredientSpot.item != null && rightIngredientSpot.item != null && !Game1.player.Items.ContainsId("(O)848", GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item)))
		{
			if (_craftState != CraftState.MissingShards)
			{
				_craftState = CraftState.MissingShards;
				craftResultDisplay.item = null;
				_UpdateDescriptionText();
			}
		}
		else if (_craftState == CraftState.MissingShards)
		{
			_ValidateCraft();
		}
		descriptionText = displayedDescription;
		questionMarkOffset.X = (float)Math.Sin(time.TotalGameTime.TotalSeconds * 2.5) * 4f;
		questionMarkOffset.Y = (float)Math.Cos(time.TotalGameTime.TotalSeconds * 5.0) * -4f;
		bool can_fit_crafted_item = CanFitCraftedItem();
		if (_craftState == CraftState.Valid && !IsBusy() && can_fit_crafted_item)
		{
			craftResultDisplay.visible = true;
		}
		else
		{
			craftResultDisplay.visible = false;
		}
		if (_timeUntilCraft <= 0 && _sparklingTimer <= 0)
		{
			return;
		}
		startTailoringButton.tryHover(startTailoringButton.bounds.Center.X, startTailoringButton.bounds.Center.Y, 0.33f);
		_timeUntilCraft -= (int)time.ElapsedGameTime.TotalMilliseconds;
		_clankEffectTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		if (_timeUntilCraft <= 0 && _sparklingTimer > 0)
		{
			_sparklingTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		}
		else if (_clankEffectTimer <= 0 && !unforging)
		{
			_clankEffectTimer = 450;
			if (rightIngredientSpot.item?.QualifiedItemId == "(O)74")
			{
				Rectangle r = rightIngredientSpot.bounds;
				r.Inflate(-16, -16);
				int num = 30;
				for (int i = 0; i < num; i++)
				{
					Vector2 position = Utility.getRandomPositionInThisRectangle(r, Game1.random);
					tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 48, 2, 2), position, flipped: false, 0f, Color.White)
					{
						motion = new Vector2(-4f, 0f),
						yPeriodic = true,
						yPeriodicRange = 16f,
						yPeriodicLoopTime = 1200f,
						scale = 4f,
						layerDepth = 1f,
						animationLength = 12,
						interval = Game1.random.Next(20, 40),
						totalNumberOfLoops = 1,
						delayBeforeAnimationStart = _clankEffectTimer / num * i
					});
				}
			}
			else
			{
				Game1.playSound("crafting");
				Game1.playSound("clank");
				Rectangle r = leftIngredientSpot.bounds;
				r.Inflate(-21, -21);
				Vector2 position = Utility.getRandomPositionInThisRectangle(r, Game1.random);
				tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), position, flipped: false, 0.015f, Color.White)
				{
					motion = new Vector2(-1f, -10f),
					acceleration = new Vector2(0f, 0.6f),
					scale = 4f,
					layerDepth = 1f,
					animationLength = 12,
					interval = 30f,
					totalNumberOfLoops = 1
				});
				tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), position, flipped: false, 0.015f, Color.White)
				{
					motion = new Vector2(0f, -8f),
					acceleration = new Vector2(0f, 0.48f),
					scale = 4f,
					layerDepth = 1f,
					animationLength = 12,
					interval = 30f,
					totalNumberOfLoops = 1
				});
				tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), position, flipped: false, 0.015f, Color.White)
				{
					motion = new Vector2(1f, -10f),
					acceleration = new Vector2(0f, 0.6f),
					scale = 4f,
					layerDepth = 1f,
					animationLength = 12,
					interval = 30f,
					totalNumberOfLoops = 1
				});
				tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), position, flipped: false, 0.015f, Color.White)
				{
					motion = new Vector2(-2f, -8f),
					acceleration = new Vector2(0f, 0.6f),
					scale = 2f,
					layerDepth = 1f,
					animationLength = 12,
					interval = 30f,
					totalNumberOfLoops = 1
				});
				tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(114, 46, 2, 2), position, flipped: false, 0.015f, Color.White)
				{
					motion = new Vector2(2f, -8f),
					acceleration = new Vector2(0f, 0.6f),
					scale = 2f,
					layerDepth = 1f,
					animationLength = 12,
					interval = 30f,
					totalNumberOfLoops = 1
				});
			}
		}
		if (_timeUntilCraft > 0 || _sparklingTimer > 0)
		{
			return;
		}
		if (unforging)
		{
			if (leftIngredientSpot.item is MeleeWeapon leftWeapon)
			{
				int cost = 0;
				if (leftWeapon != null)
				{
					int weapon_forge_levels = leftWeapon.GetTotalForgeLevels(for_unforge: true);
					for (int i = 0; i < weapon_forge_levels; i++)
					{
						cost += GetForgeCostAtLevel(i);
					}
					if (leftWeapon.hasEnchantmentOfType<DiamondEnchantment>())
					{
						cost += GetForgeCost(leftIngredientSpot.item, ItemRegistry.Create("(O)72"));
					}
					for (int i = leftWeapon.enchantments.Count - 1; i >= 0; i--)
					{
						if (leftWeapon.enchantments[i].IsForge())
						{
							leftWeapon.RemoveEnchantment(leftWeapon.enchantments[i]);
						}
					}
					if (leftWeapon.appearance.Value != null)
					{
						Utility.CollectOrDrop(ItemRegistry.Create(leftWeapon.appearance.Value));
						leftWeapon.appearance.Value = null;
						leftWeapon.ResetIndexOfMenuItemView();
						cost += 10;
					}
					leftIngredientSpot.item = null;
					Game1.playSound("coin");
					base.heldItem = leftWeapon;
				}
				Utility.CollectOrDrop(ItemRegistry.Create("(O)848", cost / 2));
			}
			else if (leftIngredientSpot.item is CombinedRing leftRing)
			{
				if (leftRing != null)
				{
					List<Ring> rings = new List<Ring>(leftRing.combinedRings);
					leftRing.combinedRings.Clear();
					foreach (Ring item in rings)
					{
						Utility.CollectOrDrop(item);
					}
					leftIngredientSpot.item = null;
					Game1.playSound("coin");
				}
				Utility.CollectOrDrop(ItemRegistry.Create("(O)848", 10));
			}
			unforging = false;
			_timeUntilCraft = 0;
			_ValidateCraft();
			return;
		}
		Game1.player.Items.ReduceId("(O)848", GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item));
		Item crafted_item = CraftItem(leftIngredientSpot.item, rightIngredientSpot.item, forReal: true);
		if (crafted_item != null && !Utility.canItemBeAddedToThisInventoryList(crafted_item, inventory.actualInventory))
		{
			Game1.playSound("cancel");
			Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
			_timeUntilCraft = 0;
			return;
		}
		if (leftIngredientSpot.item == crafted_item)
		{
			leftIngredientSpot.item = null;
		}
		else
		{
			SpendLeftItem();
		}
		SpendRightItem();
		Game1.playSound("coin");
		base.heldItem = crafted_item;
		_timeUntilCraft = 0;
		_ValidateCraft();
	}

	public virtual bool IsValidUnforge(bool ignore_right_slot_occupancy = false)
	{
		if (!ignore_right_slot_occupancy && rightIngredientSpot.item != null)
		{
			return false;
		}
		if (leftIngredientSpot.item is MeleeWeapon leftWeapon && (leftWeapon.GetTotalForgeLevels() > 0 || leftWeapon.appearance.Value != null))
		{
			return true;
		}
		if (leftIngredientSpot.item is CombinedRing)
		{
			return true;
		}
		return false;
	}

	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
		}
		Game1.DrawBox(xPositionOnScreen - 64, yPositionOnScreen + 128, 128, 201, new Color(116, 11, 3));
		Game1.player.FarmerRenderer.drawMiniPortrat(b, new Vector2((float)(xPositionOnScreen - 64) + 9.6f, yPositionOnScreen + 128), 0.87f, 4f, 2, Game1.player);
		base.draw(b, drawUpperPortion: true, drawDescriptionArea: true, 116, 11, 3);
		b.Draw(forgeTextures, new Vector2(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 - 4, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder), new Rectangle(0, 0, 142, 80), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
		Color draw_color = Color.White;
		if (_craftState == CraftState.MissingShards)
		{
			draw_color = Color.Gray * 0.75f;
		}
		b.Draw(forgeTextures, new Vector2(xPositionOnScreen + 276, yPositionOnScreen + 300), new Rectangle(142, 16, 17, 17), draw_color, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
		if (leftIngredientSpot.item != null && rightIngredientSpot.item != null && IsValidCraft(leftIngredientSpot.item, rightIngredientSpot.item))
		{
			int source_offset = (GetForgeCost(leftIngredientSpot.item, rightIngredientSpot.item) - 10) / 5;
			if (source_offset >= 0 && source_offset <= 2)
			{
				b.Draw(forgeTextures, new Vector2(xPositionOnScreen + 344, yPositionOnScreen + 320), new Rectangle(142, 38 + source_offset * 10, 17, 10), Color.White * ((_craftState == CraftState.MissingShards) ? 0.5f : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			}
		}
		if (IsValidUnforge())
		{
			b.Draw(forgeTextures, new Vector2(unforgeButton.bounds.X, unforgeButton.bounds.Y), new Rectangle(143, 69, 11, 10), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
		}
		if (_craftState == CraftState.Valid)
		{
			startTailoringButton.draw(b, Color.White, 0.96f, (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 200 % 12);
			startTailoringButton.drawItem(b, 16, 16);
		}
		Point random_shaking = new Point(0, 0);
		bool left_slot_accepts_this_item = false;
		bool right_slot_accepts_this_item = false;
		Item highlight_item = hoveredItem;
		if (base.heldItem != null)
		{
			highlight_item = base.heldItem;
		}
		if (highlight_item != null && highlight_item != leftIngredientSpot.item && highlight_item != rightIngredientSpot.item && highlight_item != craftResultDisplay.item)
		{
			if (highlight_item is Tool)
			{
				if (leftIngredientSpot.item is Tool)
				{
					right_slot_accepts_this_item = true;
				}
				else
				{
					left_slot_accepts_this_item = true;
				}
			}
			if (BaseEnchantment.GetEnchantmentFromItem(leftIngredientSpot.item, highlight_item) != null)
			{
				right_slot_accepts_this_item = true;
			}
			if (highlight_item is Ring && !(highlight_item is CombinedRing) && (leftIngredientSpot.item == null || leftIngredientSpot.item is Ring) && (rightIngredientSpot.item == null || rightIngredientSpot.item is Ring))
			{
				left_slot_accepts_this_item = true;
				right_slot_accepts_this_item = true;
			}
		}
		foreach (ClickableComponent c in equipmentIcons)
		{
			string name = c.name;
			if (!(name == "Ring1"))
			{
				if (!(name == "Ring2"))
				{
					continue;
				}
				if (Game1.player.rightRing.Value != null)
				{
					b.Draw(forgeTextures, c.bounds, new Rectangle(0, 96, 16, 16), Color.White);
					float transparency = 1f;
					if (!HighlightItems(Game1.player.rightRing.Value))
					{
						transparency = 0.5f;
					}
					if (Game1.player.rightRing.Value == base.heldItem)
					{
						transparency = 0.5f;
					}
					Game1.player.rightRing.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale, transparency, 0.866f, StackDrawType.Hide);
				}
				else
				{
					b.Draw(forgeTextures, c.bounds, new Rectangle(16, 96, 16, 16), Color.White);
				}
			}
			else if (Game1.player.leftRing.Value != null)
			{
				b.Draw(forgeTextures, c.bounds, new Rectangle(0, 96, 16, 16), Color.White);
				float transparency = 1f;
				if (!HighlightItems(Game1.player.leftRing.Value))
				{
					transparency = 0.5f;
				}
				if (Game1.player.leftRing.Value == base.heldItem)
				{
					transparency = 0.5f;
				}
				Game1.player.leftRing.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale, transparency, 0.866f, StackDrawType.Hide);
			}
			else
			{
				b.Draw(forgeTextures, c.bounds, new Rectangle(16, 96, 16, 16), Color.White);
			}
		}
		if (!IsBusy())
		{
			if (left_slot_accepts_this_item)
			{
				leftIngredientSpot.draw(b, Color.White, 0.87f);
			}
		}
		else if (_clankEffectTimer > 300 || (_timeUntilCraft > 0 && unforging))
		{
			random_shaking.X = Game1.random.Next(-1, 2);
			random_shaking.Y = Game1.random.Next(-1, 2);
		}
		leftIngredientSpot.drawItem(b, random_shaking.X * 4, random_shaking.Y * 4);
		if (craftResultDisplay.visible)
		{
			string make_result_text = Game1.content.LoadString("Strings\\UI:Tailor_MakeResult");
			Utility.drawTextWithColoredShadow(position: new Vector2((float)craftResultDisplay.bounds.Center.X - Game1.smallFont.MeasureString(make_result_text).X / 2f, (float)craftResultDisplay.bounds.Top - Game1.smallFont.MeasureString(make_result_text).Y), b: b, text: make_result_text, font: Game1.smallFont, color: Game1.textColor * 0.75f, shadowColor: Color.Black * 0.2f);
			if (craftResultDisplay.item != null)
			{
				craftResultDisplay.drawItem(b);
			}
		}
		if (!IsBusy() && right_slot_accepts_this_item)
		{
			rightIngredientSpot.draw(b, Color.White, 0.87f);
		}
		rightIngredientSpot.drawItem(b);
		foreach (TemporaryAnimatedSprite tempSprite in tempSprites)
		{
			tempSprite.draw(b, localPosition: true);
		}
		if (!hoverText.Equals(""))
		{
			IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont, (base.heldItem != null) ? 32 : 0, (base.heldItem != null) ? 32 : 0);
		}
		else if (hoveredItem != null)
		{
			if (hoveredItem == craftResultDisplay.item && rightIngredientSpot.item?.QualifiedItemId == "(O)74")
			{
				BaseEnchantment.hideEnchantmentName = true;
			}
			else if (hoveredItem == craftResultDisplay.item && rightIngredientSpot.item?.QualifiedItemId == "(O)852")
			{
				BaseEnchantment.hideSecondaryEnchantName = true;
			}
			IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem, base.heldItem != null);
			BaseEnchantment.hideEnchantmentName = false;
			BaseEnchantment.hideSecondaryEnchantName = false;
		}
		base.heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
		if (!Game1.options.hardwareCursor)
		{
			drawMouse(b);
		}
	}

	protected override void cleanupBeforeExit()
	{
		_OnCloseMenu();
	}

	protected void _OnCloseMenu()
	{
		if (!Game1.player.IsEquippedItem(base.heldItem))
		{
			Utility.CollectOrDrop(base.heldItem, 2);
		}
		if (!Game1.player.IsEquippedItem(leftIngredientSpot.item))
		{
			Utility.CollectOrDrop(leftIngredientSpot.item, 2);
		}
		if (!Game1.player.IsEquippedItem(rightIngredientSpot.item))
		{
			Utility.CollectOrDrop(rightIngredientSpot.item, 2);
		}
		if (!Game1.player.IsEquippedItem(startTailoringButton.item))
		{
			Utility.CollectOrDrop(startTailoringButton.item, 2);
		}
		base.heldItem = null;
		leftIngredientSpot.item = null;
		rightIngredientSpot.item = null;
		startTailoringButton.item = null;
	}
}
