using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Locations;

namespace StardewValley.Menus;

public class GameMenu : IClickableMenu
{
	public static readonly int inventoryTab = 0;

	public static readonly int skillsTab = 1;

	public static readonly int socialTab = 2;

	public static readonly int mapTab = 3;

	public static readonly int craftingTab = 4;

	public static readonly int animalsTab = 5;

	public static readonly int powersTab = 6;

	public static readonly int collectionsTab = 7;

	public static readonly int optionsTab = 8;

	public static readonly int exitTab = 9;

	public const int region_inventoryTab = 12340;

	public const int region_skillsTab = 12341;

	public const int region_socialTab = 12342;

	public const int region_mapTab = 12343;

	public const int region_craftingTab = 12344;

	public const int region_animalsTab = 12345;

	public const int region_powersTab = 12346;

	public const int region_collectionsTab = 12347;

	public const int region_optionsTab = 12348;

	public const int region_exitTab = 12349;

	public static readonly int numberOfTabs = 9;

	public int currentTab;

	public int lastOpenedNonMapTab = inventoryTab;

	public string hoverText = "";

	public string descriptionText = "";

	public List<ClickableComponent> tabs = new List<ClickableComponent>();

	public List<IClickableMenu> pages = new List<IClickableMenu>();

	public bool invisible;

	public static bool forcePreventClose;

	public static bool bundleItemHovered;

	/// <summary>The translation keys for tab names.</summary>
	private static readonly Dictionary<int, string> TabTranslationKeys = new Dictionary<int, string>
	{
		[inventoryTab] = "Strings\\UI:GameMenu_Inventory",
		[skillsTab] = "Strings\\UI:GameMenu_Skills",
		[socialTab] = "Strings\\UI:GameMenu_Social",
		[mapTab] = "Strings\\UI:GameMenu_Map",
		[craftingTab] = "Strings\\UI:GameMenu_Crafting",
		[powersTab] = "Strings\\1_6_Strings:GameMenu_Powers",
		[exitTab] = "Strings\\UI:GameMenu_Exit",
		[collectionsTab] = "Strings\\UI:GameMenu_Collections",
		[optionsTab] = "Strings\\UI:GameMenu_Options",
		[exitTab] = "Strings\\UI:GameMenu_Exit"
	};

	public GameMenu(bool playOpeningSound = true)
		: base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
	{
		tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 64, yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "inventory", Game1.content.LoadString("Strings\\UI:GameMenu_Inventory"))
		{
			myID = 12340,
			downNeighborID = 0,
			rightNeighborID = 12341,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		pages.Add(new InventoryPage(xPositionOnScreen, yPositionOnScreen, width, height));
		tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 128, yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "skills", Game1.content.LoadString("Strings\\UI:GameMenu_Skills"))
		{
			myID = 12341,
			downNeighborID = 1,
			rightNeighborID = 12342,
			leftNeighborID = 12340,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		pages.Add(new SkillsPage(xPositionOnScreen, yPositionOnScreen, width + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it) ? 64 : 0), height));
		tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 192, yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "social", Game1.content.LoadString("Strings\\UI:GameMenu_Social"))
		{
			myID = 12342,
			downNeighborID = 2,
			rightNeighborID = 12343,
			leftNeighborID = 12341,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		pages.Add(new SocialPage(xPositionOnScreen, yPositionOnScreen, width + 36, height));
		tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 256, yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "map", Game1.content.LoadString("Strings\\UI:GameMenu_Map"))
		{
			myID = 12343,
			downNeighborID = 3,
			rightNeighborID = 12344,
			leftNeighborID = 12342,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		pages.Add(new MapPage(xPositionOnScreen, yPositionOnScreen, width, height));
		tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 320, yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "crafting", Game1.content.LoadString("Strings\\UI:GameMenu_Crafting"))
		{
			myID = 12344,
			downNeighborID = 4,
			rightNeighborID = 12345,
			leftNeighborID = 12343,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		pages.Add(new CraftingPage(xPositionOnScreen, yPositionOnScreen, width, height));
		tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 384, yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "animals", Game1.content.LoadString("Strings\\1_6_Strings:GameMenu_Animals"))
		{
			myID = 12345,
			downNeighborID = 5,
			rightNeighborID = 12346,
			leftNeighborID = 12344,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		pages.Add(new AnimalPage(xPositionOnScreen, yPositionOnScreen, width - 64 - 16, height));
		tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 448, yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "powers", Game1.content.LoadString("Strings\\1_6_Strings:GameMenu_Powers"))
		{
			myID = 12346,
			downNeighborID = 6,
			rightNeighborID = 12347,
			leftNeighborID = 12345,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		pages.Add(new PowersTab(xPositionOnScreen, yPositionOnScreen, width - 64 - 16, height));
		tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 512, yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "collections", Game1.content.LoadString("Strings\\UI:GameMenu_Collections"))
		{
			myID = 12347,
			downNeighborID = 7,
			rightNeighborID = 12348,
			leftNeighborID = 12346,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		pages.Add(new CollectionsPage(xPositionOnScreen, yPositionOnScreen, width - 64 - 16, height));
		tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 576, yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "options", Game1.content.LoadString("Strings\\UI:GameMenu_Options"))
		{
			myID = 12348,
			downNeighborID = 8,
			rightNeighborID = 12349,
			leftNeighborID = 12347,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		int extraWidth = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? 96 : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr) ? 192 : 0));
		pages.Add(new OptionsPage(xPositionOnScreen, yPositionOnScreen, width + extraWidth, height));
		tabs.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 640, yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "exit", Game1.content.LoadString("Strings\\UI:GameMenu_Exit"))
		{
			myID = 12349,
			downNeighborID = 9,
			leftNeighborID = 12348,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		pages.Add(new ExitPage(xPositionOnScreen, yPositionOnScreen, width - 64 - 16, height));
		if (Game1.activeClickableMenu == null && playOpeningSound)
		{
			Game1.playSound("bigSelect");
		}
		forcePreventClose = false;
		Game1.RequireLocation<CommunityCenter>("CommunityCenter").refreshBundlesIngredientsInfo();
		pages[currentTab].populateClickableComponentList();
		AddTabsToClickableComponents(pages[currentTab]);
		if (Game1.options.SnappyMenus)
		{
			snapToDefaultClickableComponent();
		}
	}

	public void AddTabsToClickableComponents(IClickableMenu menu)
	{
		menu.allClickableComponents.AddRange(tabs);
	}

	public GameMenu(int startingTab, int extra = -1, bool playOpeningSound = true)
		: this(playOpeningSound)
	{
		changeTab(startingTab, playSound: false);
		if (startingTab == optionsTab && extra != -1)
		{
			(pages[optionsTab] as OptionsPage).currentItemIndex = extra;
		}
	}

	public override void automaticSnapBehavior(int direction, int oldRegion, int oldID)
	{
		if (GetCurrentPage() != null)
		{
			GetCurrentPage().automaticSnapBehavior(direction, oldRegion, oldID);
		}
		else
		{
			base.automaticSnapBehavior(direction, oldRegion, oldID);
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		if (currentTab < pages.Count)
		{
			pages[currentTab].snapToDefaultClickableComponent();
		}
	}

	public override void receiveGamePadButton(Buttons b)
	{
		base.receiveGamePadButton(b);
		switch (b)
		{
		case Buttons.RightTrigger:
			if (currentTab == mapTab)
			{
				Game1.activeClickableMenu = new GameMenu(mapTab + 1);
				Game1.playSound("smallSelect");
			}
			else if (currentTab < numberOfTabs && pages[currentTab].readyToClose())
			{
				changeTab(currentTab + 1);
			}
			break;
		case Buttons.LeftTrigger:
			if (currentTab == mapTab)
			{
				Game1.activeClickableMenu = new GameMenu(mapTab - 1);
				Game1.playSound("smallSelect");
			}
			else if (currentTab > 0 && pages[currentTab].readyToClose())
			{
				changeTab(currentTab - 1);
			}
			break;
		default:
			pages[currentTab].receiveGamePadButton(b);
			break;
		}
	}

	public override void setUpForGamePadMode()
	{
		base.setUpForGamePadMode();
		if (pages.Count > currentTab)
		{
			pages[currentTab].setUpForGamePadMode();
		}
	}

	public override ClickableComponent getCurrentlySnappedComponent()
	{
		return pages[currentTab].getCurrentlySnappedComponent();
	}

	public override void setCurrentlySnappedComponentTo(int id)
	{
		pages[currentTab].setCurrentlySnappedComponentTo(id);
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if ((pages[currentTab] as CollectionsPage)?.letterviewerSubMenu == null)
		{
			base.receiveLeftClick(x, y, playSound);
		}
		if (!invisible && !forcePreventClose)
		{
			for (int i = 0; i < tabs.Count; i++)
			{
				if (tabs[i].containsPoint(x, y) && currentTab != i && pages[currentTab].readyToClose())
				{
					changeTab(getTabNumberFromName(tabs[i].name));
					return;
				}
			}
		}
		pages[currentTab].receiveLeftClick(x, y);
	}

	public static string getLabelOfTabFromIndex(int index)
	{
		if (!TabTranslationKeys.TryGetValue(index, out var translationKey))
		{
			return "";
		}
		return Game1.content.LoadString(translationKey);
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		pages[currentTab].receiveRightClick(x, y);
	}

	public override void receiveScrollWheelAction(int direction)
	{
		base.receiveScrollWheelAction(direction);
		pages[currentTab].receiveScrollWheelAction(direction);
	}

	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		hoverText = "";
		pages[currentTab].performHoverAction(x, y);
		foreach (ClickableComponent c in tabs)
		{
			if (c.containsPoint(x, y))
			{
				hoverText = c.label;
				break;
			}
		}
	}

	public int getTabNumberFromName(string name)
	{
		int whichTab = -1;
		switch (name)
		{
		case "inventory":
			whichTab = inventoryTab;
			break;
		case "skills":
			whichTab = skillsTab;
			break;
		case "social":
			whichTab = socialTab;
			break;
		case "map":
			whichTab = mapTab;
			break;
		case "crafting":
			whichTab = craftingTab;
			break;
		case "collections":
			whichTab = collectionsTab;
			break;
		case "options":
			whichTab = optionsTab;
			break;
		case "exit":
			whichTab = exitTab;
			break;
		case "powers":
			whichTab = powersTab;
			break;
		case "animals":
			whichTab = animalsTab;
			break;
		}
		return whichTab;
	}

	public override void update(GameTime time)
	{
		base.update(time);
		pages[currentTab].update(time);
	}

	public override void releaseLeftClick(int x, int y)
	{
		base.releaseLeftClick(x, y);
		pages[currentTab].releaseLeftClick(x, y);
	}

	public override void leftClickHeld(int x, int y)
	{
		base.leftClickHeld(x, y);
		pages[currentTab].leftClickHeld(x, y);
	}

	public override bool readyToClose()
	{
		if (!forcePreventClose)
		{
			return pages[currentTab].readyToClose();
		}
		return false;
	}

	public void changeTab(int whichTab, bool playSound = true)
	{
		currentTab = getTabNumberFromName(tabs[whichTab].name);
		if (currentTab == mapTab)
		{
			invisible = true;
			width += 128;
			initializeUpperRightCloseButton();
		}
		else
		{
			lastOpenedNonMapTab = currentTab;
			width = 800 + IClickableMenu.borderWidth * 2;
			initializeUpperRightCloseButton();
			invisible = false;
		}
		if (playSound)
		{
			Game1.playSound("smallSelect");
		}
		pages[currentTab].populateClickableComponentList();
		AddTabsToClickableComponents(pages[currentTab]);
		setTabNeighborsForCurrentPage();
		if (Game1.options.SnappyMenus)
		{
			snapToDefaultClickableComponent();
		}
	}

	public IClickableMenu GetCurrentPage()
	{
		if (currentTab >= pages.Count || currentTab < 0)
		{
			return null;
		}
		return pages[currentTab];
	}

	public void setTabNeighborsForCurrentPage()
	{
		if (currentTab == inventoryTab)
		{
			for (int i = 0; i < tabs.Count; i++)
			{
				tabs[i].downNeighborID = i;
			}
		}
		else if (currentTab == exitTab)
		{
			for (int i = 0; i < tabs.Count; i++)
			{
				tabs[i].downNeighborID = 535;
			}
		}
		else
		{
			for (int i = 0; i < tabs.Count; i++)
			{
				tabs[i].downNeighborID = -99999;
			}
		}
	}

	public override void draw(SpriteBatch b)
	{
		if (!invisible)
		{
			if (!Game1.options.showMenuBackground && !Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
			}
			Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, pages[currentTab].width, pages[currentTab].height, speaker: false, drawOnlyBox: true);
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
			foreach (ClickableComponent c in tabs)
			{
				int sheetIndex = -1;
				switch (c.name)
				{
				case "inventory":
					sheetIndex = 0;
					break;
				case "skills":
					sheetIndex = 1;
					break;
				case "social":
					sheetIndex = 2;
					break;
				case "map":
					sheetIndex = 3;
					break;
				case "crafting":
					sheetIndex = 4;
					break;
				case "catalogue":
					sheetIndex = 7;
					break;
				case "collections":
					sheetIndex = 5;
					break;
				case "options":
					sheetIndex = 6;
					break;
				case "exit":
					sheetIndex = 7;
					break;
				case "coop":
					sheetIndex = 1;
					break;
				case "powers":
					b.Draw(Game1.mouseCursors_1_6, new Vector2(c.bounds.X, c.bounds.Y + ((currentTab == getTabNumberFromName(c.name)) ? 8 : 0)), new Rectangle(216, 494, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
					break;
				case "animals":
					b.Draw(Game1.mouseCursors_1_6, new Vector2(c.bounds.X, c.bounds.Y + ((currentTab == getTabNumberFromName(c.name)) ? 8 : 0)), new Rectangle(257, 246, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
					break;
				}
				if (sheetIndex != -1)
				{
					b.Draw(Game1.mouseCursors, new Vector2(c.bounds.X, c.bounds.Y + ((currentTab == getTabNumberFromName(c.name)) ? 8 : 0)), new Rectangle(sheetIndex * 16, 368, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
				}
				if (c.name.Equals("skills"))
				{
					Game1.player.FarmerRenderer.drawMiniPortrat(b, new Vector2(c.bounds.X + 8, c.bounds.Y + 12 + ((currentTab == getTabNumberFromName(c.name)) ? 8 : 0)), 0.00011f, 3f, 2, Game1.player);
				}
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			pages[currentTab].draw(b);
			if (!hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
			}
		}
		else
		{
			pages[currentTab].draw(b);
		}
		if (!forcePreventClose && pages[currentTab].shouldDrawCloseButton())
		{
			base.draw(b);
		}
		if ((!Game1.options.SnappyMenus || (pages[currentTab] as CollectionsPage)?.letterviewerSubMenu == null) && !Game1.options.hardwareCursor)
		{
			drawMouse(b, ignore_transparency: true);
		}
	}

	public override bool areGamePadControlsImplemented()
	{
		return false;
	}

	public override void receiveKeyPress(Keys key)
	{
		if (Game1.options.menuButton.Contains(new InputButton(key)) && readyToClose())
		{
			Game1.exitActiveMenu();
			Game1.playSound("bigDeSelect");
		}
		pages[currentTab].receiveKeyPress(key);
	}

	public override void emergencyShutDown()
	{
		base.emergencyShutDown();
		pages[currentTab].emergencyShutDown();
	}

	protected override void cleanupBeforeExit()
	{
		base.cleanupBeforeExit();
		if (Game1.options.optionsDirty)
		{
			Game1.options.SaveDefaultOptions();
		}
	}
}
