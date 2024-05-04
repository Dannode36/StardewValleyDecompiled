using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.FarmAnimals;
using xTile.Dimensions;

namespace StardewValley.Menus;

public class PurchaseAnimalsMenu : IClickableMenu
{
	public const int region_okButton = 101;

	public const int region_doneNamingButton = 102;

	public const int region_randomButton = 103;

	public const int region_namingBox = 104;

	public const int region_upArrow = 105;

	public const int region_downArrow = 106;

	public static int menuHeight = 320;

	public static int menuWidth = 384;

	public int clickedAnimalButton = -1;

	public List<ClickableTextureComponent> animalsToPurchase = new List<ClickableTextureComponent>();

	public ClickableTextureComponent okButton;

	public ClickableTextureComponent doneNamingButton;

	public ClickableTextureComponent randomButton;

	public ClickableTextureComponent upArrow;

	public ClickableTextureComponent downArrow;

	public ClickableTextureComponent hovered;

	public ClickableComponent textBoxCC;

	/// <summary>Whether the menu is currently showing the target location (regardless of whether it's the farm), so the player can choose a building to put animals in.</summary>
	public bool onFarm;

	public bool namingAnimal;

	public bool freeze;

	public FarmAnimal animalBeingPurchased;

	public TextBox textBox;

	public TextBoxEvent textBoxEvent;

	public Building newAnimalHome;

	public int priceOfAnimal;

	public bool readOnly;

	/// <summary>The index of the row shown at the top of the shop menu.</summary>
	public int currentScroll;

	/// <summary>The number of shop rows that are off-screen.</summary>
	public int scrollRows;

	/// <summary>The location in which to construct or manage buildings.</summary>
	public GameLocation TargetLocation;

	/// <summary>Construct an instance.</summary>
	/// <param name="stock">The animals available to purchase.</param>
	/// <param name="targetLocation">The location for which to purchase animals, or <c>null</c> for the farm.</param>
	public PurchaseAnimalsMenu(List<Object> stock, GameLocation targetLocation = null)
		: base(Game1.uiViewport.Width / 2 - menuWidth / 2 - IClickableMenu.borderWidth * 2, (Game1.uiViewport.Height - menuHeight - IClickableMenu.borderWidth * 2) / 4, menuWidth + IClickableMenu.borderWidth * 2 + ((GetOffScreenRows(stock.Count) > 0) ? 44 : 0), menuHeight + IClickableMenu.borderWidth)
	{
		height += 64;
		TargetLocation = targetLocation ?? Game1.getFarm();
		for (int i = 0; i < stock.Count; i++)
		{
			Texture2D texture;
			Microsoft.Xna.Framework.Rectangle sourceRect;
			if (Game1.farmAnimalData.TryGetValue(stock[i].Name, out var animalData) && animalData.ShopTexture != null)
			{
				texture = Game1.content.Load<Texture2D>(animalData.ShopTexture);
				sourceRect = animalData.ShopSourceRect;
			}
			else if (i >= 9)
			{
				texture = Game1.mouseCursors2;
				sourceRect = new Microsoft.Xna.Framework.Rectangle(128 + i % 3 * 16 * 2, i / 3 * 16, 32, 16);
			}
			else
			{
				texture = Game1.mouseCursors;
				sourceRect = new Microsoft.Xna.Framework.Rectangle(i % 3 * 16 * 2, 448 + i / 3 * 16, 32, 16);
			}
			ClickableTextureComponent animalButton = new ClickableTextureComponent(stock[i].salePrice().ToString() ?? "", new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + IClickableMenu.borderWidth + i % 3 * 64 * 2, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2 + i / 3 * 85, 128, 64), null, stock[i].Name, texture, sourceRect, 4f, stock[i].Type == null)
			{
				item = stock[i],
				myID = i,
				rightNeighborID = -99998,
				leftNeighborID = -99998,
				downNeighborID = -99998,
				upNeighborID = -99998
			};
			animalsToPurchase.Add(animalButton);
		}
		scrollRows = GetOffScreenRows(animalsToPurchase.Count);
		if (scrollRows < 0)
		{
			scrollRows = 0;
		}
		RepositionAnimalButtons();
		okButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width + 4, yPositionOnScreen + height - 64 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
		{
			myID = 101,
			rightNeighborID = -99998,
			leftNeighborID = -99998,
			downNeighborID = -99998,
			upNeighborID = -99998
		};
		randomButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(xPositionOnScreen + width + 51 + 64, Game1.uiViewport.Height / 2, 64, 64), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(381, 361, 10, 10), 4f)
		{
			myID = 103,
			rightNeighborID = -99998,
			leftNeighborID = -99998,
			downNeighborID = -99998,
			upNeighborID = -99998
		};
		menuHeight = 320;
		menuWidth = 384;
		textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor);
		textBox.X = Game1.uiViewport.Width / 2 - 192;
		textBox.Y = Game1.uiViewport.Height / 2;
		textBox.Width = 256;
		textBox.Height = 192;
		textBoxEvent = textBoxEnter;
		textBoxCC = new ClickableComponent(new Microsoft.Xna.Framework.Rectangle(textBox.X, textBox.Y, 192, 48), "")
		{
			myID = 104,
			rightNeighborID = -99998,
			leftNeighborID = -99998,
			downNeighborID = -99998,
			upNeighborID = -99998
		};
		randomButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(textBox.X + textBox.Width + 64 + 48 - 8, Game1.uiViewport.Height / 2 + 4, 64, 64), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(381, 361, 10, 10), 4f)
		{
			myID = 103,
			rightNeighborID = -99998,
			leftNeighborID = -99998,
			downNeighborID = -99998,
			upNeighborID = -99998
		};
		doneNamingButton = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(textBox.X + textBox.Width + 32 + 4, Game1.uiViewport.Height / 2 - 8, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			myID = 102,
			rightNeighborID = -99998,
			leftNeighborID = -99998,
			downNeighborID = -99998,
			upNeighborID = -99998
		};
		int arrowsX = xPositionOnScreen + width - 64 - 24;
		upArrow = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(arrowsX, yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 16, 44, 48), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(421, 459, 11, 12), 4f)
		{
			myID = 105,
			rightNeighborID = -99998,
			leftNeighborID = -99998,
			downNeighborID = -99998,
			upNeighborID = -99998
		};
		downArrow = new ClickableTextureComponent(new Microsoft.Xna.Framework.Rectangle(arrowsX, yPositionOnScreen + height - 64 - 24, 44, 48), Game1.mouseCursors, new Microsoft.Xna.Framework.Rectangle(421, 472, 11, 12), 4f)
		{
			myID = 106,
			rightNeighborID = -99998,
			leftNeighborID = -99998,
			downNeighborID = -99998,
			upNeighborID = -99998
		};
		doneNamingButton.visible = false;
		randomButton.visible = false;
		textBoxCC.visible = false;
		if (scrollRows <= 0)
		{
			upArrow.visible = false;
			downArrow.visible = false;
		}
		if (Game1.options.SnappyMenus)
		{
			populateClickableComponentList();
			snapToDefaultClickableComponent();
		}
	}

	/// <summary>Get the number of shop rows that are off-screen.</summary>
	/// <param name="animalsToPurchase">The number of animals available to purchase.</param>
	public static int GetOffScreenRows(int animalsToPurchase)
	{
		return (animalsToPurchase - 1) / 3 + 1 - 3;
	}

	public override bool shouldClampGamePadCursor()
	{
		return onFarm;
	}

	public override void snapToDefaultClickableComponent()
	{
		currentlySnappedComponent = getComponentWithID(0);
		snapCursorToCurrentSnappedComponent();
	}

	public void textBoxEnter(TextBox sender)
	{
		if (!namingAnimal)
		{
			return;
		}
		if (Game1.activeClickableMenu == null || !(Game1.activeClickableMenu is PurchaseAnimalsMenu))
		{
			textBox.OnEnterPressed -= textBoxEvent;
		}
		else if (sender.Text.Length >= 1)
		{
			if (Utility.areThereAnyOtherAnimalsWithThisName(sender.Text))
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11308"));
				return;
			}
			textBox.OnEnterPressed -= textBoxEvent;
			animalBeingPurchased.Name = sender.Text;
			animalBeingPurchased.displayName = sender.Text;
			((AnimalHouse)newAnimalHome.GetIndoors()).adoptAnimal(animalBeingPurchased);
			newAnimalHome = null;
			namingAnimal = false;
			Game1.player.Money -= priceOfAnimal;
			setUpForReturnAfterPurchasingAnimal();
		}
	}

	public void setUpForReturnAfterPurchasingAnimal()
	{
		LocationRequest locationRequest = Game1.getLocationRequest("AnimalShop");
		locationRequest.OnWarp += delegate
		{
			onFarm = false;
			Game1.player.viewingLocation.Value = null;
			okButton.bounds.X = xPositionOnScreen + width + 4;
			Game1.displayHUD = true;
			Game1.displayFarmer = true;
			freeze = false;
			textBox.OnEnterPressed -= textBoxEvent;
			textBox.Selected = false;
			Game1.viewportFreeze = false;
			marnieAnimalPurchaseMessage();
		};
		Game1.warpFarmer(locationRequest, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.FacingDirection);
	}

	public void marnieAnimalPurchaseMessage()
	{
		exitThisMenu();
		Game1.player.forceCanMove();
		freeze = false;
		Game1.DrawDialogue(Game1.getCharacterFromName("Marnie"), animalBeingPurchased.isMale() ? "Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11311" : "Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11314", animalBeingPurchased.displayName);
	}

	public void setUpForAnimalPlacement()
	{
		upArrow.visible = false;
		downArrow.visible = false;
		Game1.currentLocation.cleanupBeforePlayerExit();
		Game1.displayFarmer = false;
		Game1.currentLocation = TargetLocation;
		Game1.player.viewingLocation.Value = TargetLocation.NameOrUniqueName;
		Game1.currentLocation.resetForPlayerEntry();
		Game1.globalFadeToClear();
		onFarm = true;
		freeze = false;
		okButton.bounds.X = Game1.uiViewport.Width - 128;
		okButton.bounds.Y = Game1.uiViewport.Height - 128;
		Game1.displayHUD = false;
		Game1.viewportFreeze = true;
		Game1.viewport.Location = new Location(3136, 320);
		Building suggestedBuilding = GetSuggestedBuilding(animalBeingPurchased);
		if (suggestedBuilding != null)
		{
			Game1.viewport.Location = GetTopLeftPixelToCenterBuilding(suggestedBuilding);
		}
		Game1.panScreen(0, 0);
	}

	public void setUpForReturnToShopMenu()
	{
		freeze = false;
		if (scrollRows > 0)
		{
			upArrow.visible = true;
			downArrow.visible = true;
		}
		doneNamingButton.visible = false;
		randomButton.visible = false;
		Game1.displayFarmer = true;
		LocationRequest locationRequest = Game1.getLocationRequest("AnimalShop");
		locationRequest.OnWarp += delegate
		{
			onFarm = false;
			Game1.player.viewingLocation.Value = null;
			okButton.bounds.X = xPositionOnScreen + width + 4;
			okButton.bounds.Y = yPositionOnScreen + height - 64 - IClickableMenu.borderWidth;
			Game1.displayHUD = true;
			Game1.viewportFreeze = false;
			namingAnimal = false;
			textBox.OnEnterPressed -= textBoxEvent;
			textBox.Selected = false;
			if (Game1.options.SnappyMenus)
			{
				setCurrentlySnappedComponentTo(clickedAnimalButton);
				snapCursorToCurrentSnappedComponent();
			}
		};
		Game1.warpFarmer(locationRequest, Game1.player.TilePoint.X, Game1.player.TilePoint.Y, Game1.player.FacingDirection);
	}

	public virtual void Scroll(int offset)
	{
		currentScroll += offset;
		if (currentScroll < 0)
		{
			currentScroll = 0;
		}
		if (currentScroll > scrollRows)
		{
			currentScroll = scrollRows;
		}
		RepositionAnimalButtons();
	}

	public virtual void RepositionAnimalButtons()
	{
		foreach (ClickableTextureComponent item in animalsToPurchase)
		{
			item.visible = false;
		}
		for (int y = 0; y < 3; y++)
		{
			for (int x = 0; x < 3; x++)
			{
				int index = (y + currentScroll) * 3 + x;
				if (index >= animalsToPurchase.Count || index < 0)
				{
					break;
				}
				ClickableTextureComponent clickableTextureComponent = animalsToPurchase[index];
				clickableTextureComponent.bounds.X = xPositionOnScreen + IClickableMenu.borderWidth + x * 64 * 2;
				clickableTextureComponent.bounds.Y = yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2 + y * 85;
				clickableTextureComponent.visible = true;
			}
		}
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (Game1.IsFading() || freeze)
		{
			return;
		}
		if (upArrow.containsPoint(x, y))
		{
			Game1.playSound("shwip");
			Scroll(-1);
		}
		else if (downArrow.containsPoint(x, y))
		{
			Game1.playSound("shwip");
			Scroll(1);
		}
		if (okButton != null && okButton.containsPoint(x, y) && readyToClose())
		{
			if (onFarm)
			{
				setUpForReturnToShopMenu();
				Game1.playSound("smallSelect");
			}
			else
			{
				Game1.exitActiveMenu();
				Game1.playSound("bigDeSelect");
			}
		}
		if (onFarm)
		{
			Vector2 clickTile = new Vector2((int)((Utility.ModifyCoordinateFromUIScale(x) + (float)Game1.viewport.X) / 64f), (int)((Utility.ModifyCoordinateFromUIScale(y) + (float)Game1.viewport.Y) / 64f));
			Building selection = TargetLocation.getBuildingAt(clickTile);
			if (!namingAnimal && selection?.GetIndoors() is AnimalHouse animalHouse && !selection.isUnderConstruction())
			{
				if (animalBeingPurchased.CanLiveIn(selection))
				{
					if (animalHouse.isFull())
					{
						Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11321"));
					}
					else
					{
						namingAnimal = true;
						doneNamingButton.visible = true;
						randomButton.visible = true;
						textBoxCC.visible = true;
						newAnimalHome = selection;
						FarmAnimalData data = animalBeingPurchased.GetAnimalData();
						if (data != null)
						{
							if (data.BabySound != null)
							{
								Game1.playSound(data.BabySound, 1200 + Game1.random.Next(-200, 201));
							}
							else if (data.Sound != null)
							{
								Game1.playSound(data.Sound, 1200 + Game1.random.Next(-200, 201));
							}
						}
						textBox.OnEnterPressed += textBoxEvent;
						textBox.Text = animalBeingPurchased.displayName;
						Game1.keyboardDispatcher.Subscriber = textBox;
						if (Game1.options.SnappyMenus)
						{
							currentlySnappedComponent = getComponentWithID(104);
							snapCursorToCurrentSnappedComponent();
						}
					}
				}
				else
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11326", animalBeingPurchased.displayType));
				}
			}
			if (namingAnimal)
			{
				if (doneNamingButton.containsPoint(x, y))
				{
					textBoxEnter(textBox);
					Game1.playSound("smallSelect");
				}
				else if (namingAnimal && randomButton.containsPoint(x, y))
				{
					animalBeingPurchased.Name = Dialogue.randomName();
					animalBeingPurchased.displayName = animalBeingPurchased.Name;
					textBox.Text = animalBeingPurchased.displayName;
					randomButton.scale = randomButton.baseScale;
					Game1.playSound("drumkit6");
				}
				textBox.Update();
			}
			return;
		}
		foreach (ClickableTextureComponent c in animalsToPurchase)
		{
			if (readOnly || !c.containsPoint(x, y) || (c.item as Object).Type != null)
			{
				continue;
			}
			int price = c.item.salePrice();
			if (Game1.player.Money >= price)
			{
				clickedAnimalButton = c.myID;
				Game1.globalFadeToBlack(setUpForAnimalPlacement);
				Game1.playSound("smallSelect");
				onFarm = true;
				string animalType = c.hoverText;
				if (Game1.farmAnimalData.TryGetValue(animalType, out var animalData) && animalData.AlternatePurchaseTypes != null)
				{
					foreach (AlternatePurchaseAnimals alternateAnimal in animalData.AlternatePurchaseTypes)
					{
						if (GameStateQuery.CheckConditions(alternateAnimal.Condition))
						{
							animalType = Game1.random.ChooseFrom(alternateAnimal.AnimalIds);
							break;
						}
					}
				}
				animalBeingPurchased = new FarmAnimal(animalType, Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
				priceOfAnimal = price;
			}
			else
			{
				Game1.addHUDMessage(new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11325"), 3));
			}
		}
	}

	public override bool overrideSnappyMenuCursorMovementBan()
	{
		if (onFarm)
		{
			return !namingAnimal;
		}
		return false;
	}

	public override void receiveGamePadButton(Buttons b)
	{
		base.receiveGamePadButton(b);
		if (b == Buttons.B && !Game1.globalFade && onFarm && namingAnimal)
		{
			setUpForReturnToShopMenu();
			Game1.playSound("smallSelect");
		}
	}

	public override void receiveKeyPress(Keys key)
	{
		if (Game1.globalFade || freeze)
		{
			return;
		}
		if (!Game1.globalFade && onFarm)
		{
			if (!namingAnimal)
			{
				if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose() && !Game1.IsFading())
				{
					setUpForReturnToShopMenu();
				}
				else if (!Game1.options.SnappyMenus)
				{
					if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
					{
						Game1.panScreen(0, 4);
					}
					else if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
					{
						Game1.panScreen(4, 0);
					}
					else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
					{
						Game1.panScreen(0, -4);
					}
					else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
					{
						Game1.panScreen(-4, 0);
					}
				}
			}
			else if (Game1.options.SnappyMenus)
			{
				if (!textBox.Selected && Game1.options.doesInputListContain(Game1.options.menuButton, key))
				{
					setUpForReturnToShopMenu();
					Game1.playSound("smallSelect");
				}
				else if (!textBox.Selected || !Game1.options.doesInputListContain(Game1.options.menuButton, key))
				{
					base.receiveKeyPress(key);
				}
			}
		}
		else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && !Game1.IsFading())
		{
			if (readyToClose())
			{
				Game1.player.forceCanMove();
				Game1.exitActiveMenu();
				Game1.playSound("bigDeSelect");
			}
		}
		else if (Game1.options.SnappyMenus)
		{
			base.receiveKeyPress(key);
		}
	}

	public override void update(GameTime time)
	{
		base.update(time);
		if (!onFarm)
		{
			upArrow.visible = currentScroll > 0;
			downArrow.visible = currentScroll < scrollRows;
		}
		else if (!namingAnimal)
		{
			int mouseX = Game1.getOldMouseX(ui_scale: false) + Game1.viewport.X;
			int mouseY = Game1.getOldMouseY(ui_scale: false) + Game1.viewport.Y;
			if (mouseX - Game1.viewport.X < 64)
			{
				Game1.panScreen(-8, 0);
			}
			else if (mouseX - (Game1.viewport.X + Game1.viewport.Width) >= -64)
			{
				Game1.panScreen(8, 0);
			}
			if (mouseY - Game1.viewport.Y < 64)
			{
				Game1.panScreen(0, -8);
			}
			else if (mouseY - (Game1.viewport.Y + Game1.viewport.Height) >= -64)
			{
				Game1.panScreen(0, 8);
			}
			Keys[] pressedKeys = Game1.oldKBState.GetPressedKeys();
			foreach (Keys key in pressedKeys)
			{
				receiveKeyPress(key);
			}
		}
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
	}

	public override void performHoverAction(int x, int y)
	{
		hovered = null;
		if (Game1.IsFading() || freeze)
		{
			return;
		}
		upArrow.tryHover(x, y);
		downArrow.tryHover(x, y);
		if (okButton != null)
		{
			if (okButton.containsPoint(x, y))
			{
				okButton.scale = Math.Min(1.1f, okButton.scale + 0.05f);
			}
			else
			{
				okButton.scale = Math.Max(1f, okButton.scale - 0.05f);
			}
		}
		if (onFarm)
		{
			if (!namingAnimal)
			{
				Vector2 clickTile = new Vector2((int)((Utility.ModifyCoordinateFromUIScale(x) + (float)Game1.viewport.X) / 64f), (int)((Utility.ModifyCoordinateFromUIScale(y) + (float)Game1.viewport.Y) / 64f));
				GameLocation f = TargetLocation;
				foreach (Building building in f.buildings)
				{
					building.color = Color.White;
				}
				Building selection = f.getBuildingAt(clickTile);
				if (selection?.GetIndoors() is AnimalHouse animalHouse)
				{
					if (animalBeingPurchased.CanLiveIn(selection) && !animalHouse.isFull())
					{
						selection.color = Color.LightGreen * 0.8f;
					}
					else
					{
						selection.color = Color.Red * 0.8f;
					}
				}
			}
			if (doneNamingButton != null)
			{
				if (doneNamingButton.containsPoint(x, y))
				{
					doneNamingButton.scale = Math.Min(1.1f, doneNamingButton.scale + 0.05f);
				}
				else
				{
					doneNamingButton.scale = Math.Max(1f, doneNamingButton.scale - 0.05f);
				}
			}
			randomButton.tryHover(x, y, 0.5f);
			return;
		}
		foreach (ClickableTextureComponent c in animalsToPurchase)
		{
			if (c.containsPoint(x, y))
			{
				c.scale = Math.Min(c.scale + 0.05f, 4.1f);
				hovered = c;
			}
			else
			{
				c.scale = Math.Max(4f, c.scale - 0.025f);
			}
		}
	}

	public override void draw(SpriteBatch b)
	{
		if (!onFarm && !Game1.dialogueUp && !Game1.IsFading())
		{
			if (!Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
			}
			SpriteText.drawStringWithScrollBackground(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11354"), xPositionOnScreen + 96, yPositionOnScreen);
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, speaker: false, drawOnlyBox: true);
			Game1.dayTimeMoneyBox.drawMoneyBox(b);
			upArrow.draw(b);
			downArrow.draw(b);
			foreach (ClickableTextureComponent c in animalsToPurchase)
			{
				c.draw(b, ((c.item as Object).Type != null) ? (Color.Black * 0.4f) : Color.White, 0.87f);
			}
		}
		else if (!Game1.IsFading() && onFarm)
		{
			string s = Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11355", animalBeingPurchased.displayHouse, animalBeingPurchased.displayType);
			SpriteText.drawStringWithScrollBackground(b, s, Game1.uiViewport.Width / 2 - SpriteText.getWidthOfString(s) / 2, 16);
			if (namingAnimal)
			{
				if (!Game1.options.showClearBackgrounds)
				{
					b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
				}
				Game1.drawDialogueBox(Game1.uiViewport.Width / 2 - 256, Game1.uiViewport.Height / 2 - 192 - 32, 512, 192, speaker: false, drawOnlyBox: true);
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:PurchaseAnimalsMenu.cs.11357"), Game1.dialogueFont, new Vector2(Game1.uiViewport.Width / 2 - 256 + 32 + 8, Game1.uiViewport.Height / 2 - 128 + 8), Game1.textColor);
				textBox.Draw(b);
				doneNamingButton.draw(b);
				randomButton.draw(b);
			}
		}
		if (!Game1.IsFading() && okButton != null)
		{
			okButton.draw(b);
		}
		if (hovered != null)
		{
			if ((hovered.item as Object).Type != null)
			{
				IClickableMenu.drawHoverText(b, Game1.parseText((hovered.item as Object).Type, Game1.dialogueFont, 320), Game1.dialogueFont);
			}
			else
			{
				string displayName = FarmAnimal.GetDisplayName(hovered.hoverText, forShop: true);
				SpriteText.drawStringWithScrollBackground(b, displayName, xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 64, yPositionOnScreen + height + -32 + IClickableMenu.spaceToClearTopBorder / 2 + 8, "Truffle Pig");
				SpriteText.drawStringWithScrollBackground(b, "$" + Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", hovered.item.salePrice()), xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 128, yPositionOnScreen + height + 64 + IClickableMenu.spaceToClearTopBorder / 2 + 8, "$99999999g", (Game1.player.Money >= hovered.item.salePrice()) ? 1f : 0.5f);
				string description = FarmAnimal.GetShopDescription(hovered.hoverText);
				IClickableMenu.drawHoverText(b, Game1.parseText(description, Game1.smallFont, 320), Game1.smallFont, 0, 0, hovered.item.salePrice(), displayName);
			}
		}
		Game1.mouseCursorTransparency = (Game1.IsFading() ? 0f : 1f);
		drawMouse(b);
	}

	/// <summary>Get a suggested building to preselect when opening the menu.</summary>
	/// <param name="animal">The farm animal being placed.</param>
	/// <returns>Returns a building which has room for the animal, else a building which could accept the animal if it wasn't full, else null.</returns>
	public Building GetSuggestedBuilding(FarmAnimal animal)
	{
		Building bestBuilding = null;
		foreach (Building building in TargetLocation.buildings)
		{
			if (animalBeingPurchased.CanLiveIn(building))
			{
				bestBuilding = building;
				if (building.GetIndoors() is AnimalHouse animalHouse && !animalHouse.isFull())
				{
					return bestBuilding;
				}
			}
		}
		return bestBuilding;
	}

	/// <summary>Get the pixel position relative to the top-left corner of the map at which to set the viewpoint so a given building is centered on screen.</summary>
	/// <param name="building">The building to center on screen.</param>
	public Location GetTopLeftPixelToCenterBuilding(Building building)
	{
		Vector2 screenPosition = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, building.tilesWide.Value * 64, building.tilesHigh.Value * 64);
		int x = building.tileX.Value * 64 - (int)screenPosition.X;
		int yOrigin = building.tileY.Value * 64 - (int)screenPosition.Y;
		return new Location(x, yOrigin);
	}
}
