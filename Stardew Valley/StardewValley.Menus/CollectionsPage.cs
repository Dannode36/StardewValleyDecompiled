using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;
using StardewValley.GameData.Objects;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Menus;

public class CollectionsPage : IClickableMenu
{
	public const int region_sideTabShipped = 7001;

	public const int region_sideTabFish = 7002;

	public const int region_sideTabArtifacts = 7003;

	public const int region_sideTabMinerals = 7004;

	public const int region_sideTabCooking = 7005;

	public const int region_sideTabAchivements = 7006;

	public const int region_sideTabSecretNotes = 7007;

	public const int region_sideTabLetters = 7008;

	public const int region_forwardButton = 707;

	public const int region_backButton = 706;

	public static int widthToMoveActiveTab = 8;

	public const int organicsTab = 0;

	public const int fishTab = 1;

	public const int archaeologyTab = 2;

	public const int mineralsTab = 3;

	public const int cookingTab = 4;

	public const int achievementsTab = 5;

	public const int secretNotesTab = 6;

	public const int lettersTab = 7;

	public const int distanceFromMenuBottomBeforeNewPage = 128;

	private string descriptionText = "";

	private string hoverText = "";

	public ClickableTextureComponent backButton;

	public ClickableTextureComponent forwardButton;

	public Dictionary<int, ClickableTextureComponent> sideTabs = new Dictionary<int, ClickableTextureComponent>();

	public int currentTab;

	public int currentPage;

	public int secretNoteImage = -1;

	public Dictionary<int, List<List<ClickableTextureComponent>>> collections = new Dictionary<int, List<List<ClickableTextureComponent>>>();

	public Dictionary<int, string> secretNotesData;

	public Texture2D secretNoteImageTexture;

	public LetterViewerMenu letterviewerSubMenu;

	private Item hoverItem;

	private CraftingRecipe hoverCraftingRecipe;

	private int value;

	public CollectionsPage(int x, int y, int width, int height)
		: base(x, y, width, height)
	{
		sideTabs.Add(0, new ClickableTextureComponent(0.ToString() ?? "", new Rectangle(xPositionOnScreen - 48 + widthToMoveActiveTab, yPositionOnScreen + 64 * (2 + sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Shipped"), Game1.mouseCursors, new Rectangle(640, 80, 16, 16), 4f)
		{
			myID = 7001,
			downNeighborID = -99998,
			rightNeighborID = 0
		});
		collections.Add(0, new List<List<ClickableTextureComponent>>());
		sideTabs.Add(1, new ClickableTextureComponent(1.ToString() ?? "", new Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 64 * (2 + sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Fish"), Game1.mouseCursors, new Rectangle(640, 64, 16, 16), 4f)
		{
			myID = 7002,
			upNeighborID = -99998,
			downNeighborID = -99998,
			rightNeighborID = 0
		});
		collections.Add(1, new List<List<ClickableTextureComponent>>());
		sideTabs.Add(2, new ClickableTextureComponent(2.ToString() ?? "", new Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 64 * (2 + sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Artifacts"), Game1.mouseCursors, new Rectangle(656, 64, 16, 16), 4f)
		{
			myID = 7003,
			upNeighborID = -99998,
			downNeighborID = -99998,
			rightNeighborID = 0
		});
		collections.Add(2, new List<List<ClickableTextureComponent>>());
		sideTabs.Add(3, new ClickableTextureComponent(3.ToString() ?? "", new Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 64 * (2 + sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Minerals"), Game1.mouseCursors, new Rectangle(672, 64, 16, 16), 4f)
		{
			myID = 7004,
			upNeighborID = -99998,
			downNeighborID = -99998,
			rightNeighborID = 0
		});
		collections.Add(3, new List<List<ClickableTextureComponent>>());
		sideTabs.Add(4, new ClickableTextureComponent(4.ToString() ?? "", new Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 64 * (2 + sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Cooking"), Game1.mouseCursors, new Rectangle(688, 64, 16, 16), 4f)
		{
			myID = 7005,
			upNeighborID = -99998,
			downNeighborID = -99998,
			rightNeighborID = 0
		});
		collections.Add(4, new List<List<ClickableTextureComponent>>());
		sideTabs.Add(5, new ClickableTextureComponent(5.ToString() ?? "", new Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 64 * (2 + sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Achievements"), Game1.mouseCursors, new Rectangle(656, 80, 16, 16), 4f)
		{
			myID = 7006,
			upNeighborID = 7005,
			downNeighborID = -99998,
			rightNeighborID = 0
		});
		collections.Add(5, new List<List<ClickableTextureComponent>>());
		sideTabs.Add(7, new ClickableTextureComponent(7.ToString() ?? "", new Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 64 * (2 + sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_Letters"), Game1.mouseCursors, new Rectangle(688, 80, 16, 16), 4f)
		{
			myID = 7008,
			upNeighborID = -99998,
			downNeighborID = -99998,
			rightNeighborID = 0
		});
		collections.Add(7, new List<List<ClickableTextureComponent>>());
		if (Game1.player.secretNotesSeen.Count > 0)
		{
			sideTabs.Add(6, new ClickableTextureComponent(6.ToString() ?? "", new Rectangle(xPositionOnScreen - 48, yPositionOnScreen + 64 * (2 + sideTabs.Count), 64, 64), "", Game1.content.LoadString("Strings\\UI:Collections_SecretNotes"), Game1.mouseCursors, new Rectangle(672, 80, 16, 16), 4f)
			{
				myID = 7007,
				upNeighborID = -99998,
				rightNeighborID = 0
			});
			collections.Add(6, new List<List<ClickableTextureComponent>>());
		}
		sideTabs[0].upNeighborID = -1;
		sideTabs[0].upNeighborImmutable = true;
		int last_tab = 0;
		int last_y = 0;
		foreach (int key in sideTabs.Keys)
		{
			if (sideTabs[key].bounds.Y > last_y)
			{
				last_y = sideTabs[key].bounds.Y;
				last_tab = key;
			}
		}
		sideTabs[last_tab].downNeighborID = -1;
		sideTabs[last_tab].downNeighborImmutable = true;
		widthToMoveActiveTab = 8;
		backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 48, yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 706,
			rightNeighborID = -7777
		};
		forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 60, yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 707,
			leftNeighborID = -7777
		};
		int[] widthUsed = new int[8];
		int baseX = xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
		int baseY = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16;
		int collectionWidth = 10;
		List<ParsedItemData> dataEntries = new List<ParsedItemData>(from entry in ItemRegistry.GetObjectTypeDefinition().GetAllData()
			orderby entry.TextureName, entry.SpriteIndex
			select entry);
		List<ParsedItemData> wineAndFriends = new List<ParsedItemData>();
		for (int i = dataEntries.Count - 1; i >= 0; i--)
		{
			string s = dataEntries[i].InternalName;
			if (s.Equals("Wine") || s.Equals("Pickles") || s.Equals("Jelly") || s.Equals("Juice"))
			{
				wineAndFriends.Add(dataEntries[i]);
				dataEntries.RemoveAt(i);
			}
			if (wineAndFriends.Count == 4)
			{
				break;
			}
		}
		wineAndFriends.Sort((ParsedItemData a, ParsedItemData b) => a.InternalName.CompareTo(b.InternalName));
		dataEntries.Insert(278, wineAndFriends[2]);
		dataEntries.Insert(279, wineAndFriends[0]);
		dataEntries.Insert(283, wineAndFriends[3]);
		dataEntries.Insert(284, wineAndFriends[1]);
		foreach (ParsedItemData data in dataEntries)
		{
			string id = data.ItemId;
			string type = data.ObjectType;
			bool farmerHas = false;
			bool farmerHasButNotMade = false;
			int whichCollection;
			switch (type)
			{
			case "Arch":
				whichCollection = 2;
				if (Game1.player.archaeologyFound.ContainsKey(id))
				{
					farmerHas = true;
				}
				break;
			case "Fish":
				if (data.RawData is ObjectData { ExcludeFromFishingCollection: not false })
				{
					continue;
				}
				whichCollection = 1;
				if (Game1.player.fishCaught.ContainsKey(data.QualifiedItemId))
				{
					farmerHas = true;
				}
				break;
			default:
				if (data.Category != -2)
				{
					if (type == "Cooking" || data.Category == -7)
					{
						whichCollection = 4;
						string last_minute_1_5_hack_name = data.InternalName;
						switch (last_minute_1_5_hack_name)
						{
						case "Cheese Cauli.":
							last_minute_1_5_hack_name = "Cheese Cauliflower";
							break;
						case "Cheese Cauliflower":
							last_minute_1_5_hack_name = "Cheese Cauli.";
							break;
						case "Vegetable Medley":
							last_minute_1_5_hack_name = "Vegetable Stew";
							break;
						case "Cookie":
							last_minute_1_5_hack_name = "Cookies";
							break;
						case "Eggplant Parmesan":
							last_minute_1_5_hack_name = "Eggplant Parm.";
							break;
						case "Cranberry Sauce":
							last_minute_1_5_hack_name = "Cran. Sauce";
							break;
						case "Dish O' The Sea":
							last_minute_1_5_hack_name = "Dish o' The Sea";
							break;
						}
						if (Game1.player.recipesCooked.ContainsKey(id))
						{
							farmerHas = true;
						}
						else if (Game1.player.cookingRecipes.ContainsKey(last_minute_1_5_hack_name))
						{
							farmerHasButNotMade = true;
						}
						switch (id)
						{
						case "217":
						case "772":
						case "773":
						case "279":
						case "873":
							continue;
						}
					}
					else
					{
						if (!Object.isPotentialBasicShipped(id, data.Category, data.ObjectType))
						{
							continue;
						}
						whichCollection = 0;
						if (Game1.player.basicShipped.ContainsKey(id))
						{
							farmerHas = true;
						}
					}
					break;
				}
				goto case "Minerals";
			case "Minerals":
				whichCollection = 3;
				if (Game1.player.mineralsFound.ContainsKey(id))
				{
					farmerHas = true;
				}
				break;
			}
			int xPos = baseX + widthUsed[whichCollection] % collectionWidth * 68;
			int yPos = baseY + widthUsed[whichCollection] / collectionWidth * 68;
			if (yPos > yPositionOnScreen + height - 128)
			{
				collections[whichCollection].Add(new List<ClickableTextureComponent>());
				widthUsed[whichCollection] = 0;
				xPos = baseX;
				yPos = baseY;
			}
			if (collections[whichCollection].Count == 0)
			{
				collections[whichCollection].Add(new List<ClickableTextureComponent>());
			}
			List<ClickableTextureComponent> list = collections[whichCollection].Last();
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(id);
			list.Add(new ClickableTextureComponent(id + " " + farmerHas + " " + farmerHasButNotMade, new Rectangle(xPos, yPos, 64, 64), null, "", itemData.GetTexture(), itemData.GetSourceRect(), 4f, farmerHas)
			{
				myID = list.Count,
				rightNeighborID = (((list.Count + 1) % collectionWidth == 0) ? (-1) : (list.Count + 1)),
				leftNeighborID = ((list.Count % collectionWidth == 0) ? 7001 : (list.Count - 1)),
				downNeighborID = ((yPos + 68 > yPositionOnScreen + height - 128) ? (-7777) : (list.Count + collectionWidth)),
				upNeighborID = ((list.Count < collectionWidth) ? 12347 : (list.Count - collectionWidth)),
				fullyImmutable = true
			});
			widthUsed[whichCollection]++;
		}
		if (collections[5].Count == 0)
		{
			collections[5].Add(new List<ClickableTextureComponent>());
		}
		foreach (KeyValuePair<int, string> kvp in Game1.achievements)
		{
			bool farmerHas = Game1.player.achievements.Contains(kvp.Key);
			string[] split = kvp.Value.Split('^');
			if (farmerHas || (split[2].Equals("true") && (split[3].Equals("-1") || farmerHasAchievements(split[3]))))
			{
				int xPos = baseX + widthUsed[5] % collectionWidth * 68;
				int yPos = baseY + widthUsed[5] / collectionWidth * 68;
				collections[5][0].Add(new ClickableTextureComponent(kvp.Key + " " + farmerHas, new Rectangle(xPos, yPos, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 25), 1f));
				widthUsed[5]++;
			}
			else
			{
				int xPos = baseX + widthUsed[5] % collectionWidth * 68;
				int yPos = baseY + widthUsed[5] / collectionWidth * 68;
				collections[5][0].Add(new ClickableTextureComponent("??? false", new Rectangle(xPos, yPos, 64, 64), null, "???", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 25), 1f));
				widthUsed[5]++;
			}
		}
		if (Game1.player.secretNotesSeen.Count > 0)
		{
			if (collections[6].Count == 0)
			{
				collections[6].Add(new List<ClickableTextureComponent>());
			}
			secretNotesData = DataLoader.SecretNotes(Game1.content);
			secretNoteImageTexture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\SecretNotesImages");
			bool show_journals = Game1.player.secretNotesSeen.Contains(GameLocation.JOURNAL_INDEX + 1);
			foreach (int i in secretNotesData.Keys)
			{
				if (i >= GameLocation.JOURNAL_INDEX)
				{
					if (!show_journals)
					{
						continue;
					}
				}
				else if (!Game1.player.hasMagnifyingGlass)
				{
					continue;
				}
				int xPos = baseX + widthUsed[6] % collectionWidth * 68;
				int yPos = baseY + widthUsed[6] / collectionWidth * 68;
				if (i >= GameLocation.JOURNAL_INDEX)
				{
					collections[6][0].Add(new ClickableTextureComponent(i + " " + Game1.player.secretNotesSeen.Contains(i), new Rectangle(xPos, yPos, 64, 64), null, "", Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 842, 16, 16), 4f, Game1.player.secretNotesSeen.Contains(i)));
				}
				else
				{
					collections[6][0].Add(new ClickableTextureComponent(i + " " + Game1.player.secretNotesSeen.Contains(i), new Rectangle(xPos, yPos, 64, 64), null, "", Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 79, 16, 16), 4f, Game1.player.secretNotesSeen.Contains(i)));
				}
				widthUsed[6]++;
			}
		}
		if (collections[7].Count == 0)
		{
			collections[7].Add(new List<ClickableTextureComponent>());
		}
		List<ClickableTextureComponent> letters = collections[7].Last();
		Dictionary<string, string> mail = DataLoader.Mail(Game1.content);
		foreach (string s in Game1.player.mailReceived)
		{
			if (mail.TryGetValue(s, out var rawText))
			{
				int xPos = baseX + widthUsed[7] % collectionWidth * 68;
				int yPos = baseY + widthUsed[7] / collectionWidth * 68;
				string[] split = rawText.Split("[#]");
				if (yPos > yPositionOnScreen + height - 128)
				{
					collections[7].Add(new List<ClickableTextureComponent>());
					widthUsed[7] = 0;
					xPos = baseX;
					yPos = baseY;
					letters = collections[7].Last();
				}
				letters.Add(new ClickableTextureComponent(s + " true " + ((split.Length > 1) ? split[1] : "???"), new Rectangle(xPos, yPos, 64, 64), null, "", Game1.mouseCursors, new Rectangle(190, 423, 14, 11), 4f, drawShadow: true)
				{
					myID = letters.Count,
					rightNeighborID = (((letters.Count + 1) % collectionWidth == 0) ? (-1) : (letters.Count + 1)),
					leftNeighborID = ((letters.Count % collectionWidth == 0) ? 7008 : (letters.Count - 1)),
					downNeighborID = ((yPos + 68 > yPositionOnScreen + height - 128) ? (-7777) : (letters.Count + collectionWidth)),
					upNeighborID = ((letters.Count < collectionWidth) ? 12347 : (letters.Count - collectionWidth)),
					fullyImmutable = true
				});
				widthUsed[7]++;
			}
		}
	}

	protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
	{
		base.customSnapBehavior(direction, oldRegion, oldID);
		switch (direction)
		{
		case 2:
			if (currentPage > 0)
			{
				currentlySnappedComponent = getComponentWithID(706);
			}
			else if (currentPage == 0 && collections[currentTab].Count > 1)
			{
				currentlySnappedComponent = getComponentWithID(707);
			}
			backButton.upNeighborID = oldID;
			forwardButton.upNeighborID = oldID;
			break;
		case 3:
			if (oldID == 707 && currentPage > 0)
			{
				currentlySnappedComponent = getComponentWithID(706);
			}
			break;
		case 1:
			if (oldID == 706 && collections[currentTab].Count > currentPage + 1)
			{
				currentlySnappedComponent = getComponentWithID(707);
			}
			break;
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.snapToDefaultClickableComponent();
		currentlySnappedComponent = getComponentWithID(0);
		snapCursorToCurrentSnappedComponent();
	}

	private bool farmerHasAchievements(string listOfAchievementNumbers)
	{
		string[] array = ArgUtility.SplitBySpace(listOfAchievementNumbers);
		foreach (string s in array)
		{
			if (!Game1.player.achievements.Contains(Convert.ToInt32(s)))
			{
				return false;
			}
		}
		return true;
	}

	public override bool readyToClose()
	{
		if (letterviewerSubMenu != null)
		{
			return false;
		}
		return base.readyToClose();
	}

	public override void update(GameTime time)
	{
		base.update(time);
		if (letterviewerSubMenu == null)
		{
			return;
		}
		letterviewerSubMenu.update(time);
		if (letterviewerSubMenu.destroy)
		{
			letterviewerSubMenu = null;
			if (Game1.options.SnappyMenus)
			{
				snapCursorToCurrentSnappedComponent();
			}
		}
	}

	public override void receiveKeyPress(Keys key)
	{
		base.receiveKeyPress(key);
		letterviewerSubMenu?.receiveKeyPress(key);
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (letterviewerSubMenu != null)
		{
			letterviewerSubMenu.receiveLeftClick(x, y);
			return;
		}
		foreach (KeyValuePair<int, ClickableTextureComponent> v in sideTabs)
		{
			if (v.Value.containsPoint(x, y) && currentTab != v.Key)
			{
				Game1.playSound("smallSelect");
				sideTabs[currentTab].bounds.X -= widthToMoveActiveTab;
				currentTab = Convert.ToInt32(v.Value.name);
				currentPage = 0;
				v.Value.bounds.X += widthToMoveActiveTab;
			}
		}
		if (currentPage > 0 && backButton.containsPoint(x, y))
		{
			currentPage--;
			Game1.playSound("shwip");
			backButton.scale = backButton.baseScale;
			if (Game1.options.snappyMenus && Game1.options.gamepadControls && currentPage == 0)
			{
				currentlySnappedComponent = forwardButton;
				Game1.setMousePosition(currentlySnappedComponent.bounds.Center);
			}
		}
		if (currentPage < collections[currentTab].Count - 1 && forwardButton.containsPoint(x, y))
		{
			currentPage++;
			Game1.playSound("shwip");
			forwardButton.scale = forwardButton.baseScale;
			if (Game1.options.snappyMenus && Game1.options.gamepadControls && currentPage == collections[currentTab].Count - 1)
			{
				currentlySnappedComponent = backButton;
				Game1.setMousePosition(currentlySnappedComponent.bounds.Center);
			}
		}
		switch (currentTab)
		{
		case 7:
		{
			Dictionary<string, string> mail = DataLoader.Mail(Game1.content);
			{
				foreach (ClickableTextureComponent c in collections[currentTab][currentPage])
				{
					if (c.containsPoint(x, y))
					{
						string id = ArgUtility.SplitBySpaceAndGet(c.name, 0);
						letterviewerSubMenu = new LetterViewerMenu(mail[id], id, fromCollection: true);
					}
				}
				break;
			}
		}
		case 6:
		{
			foreach (ClickableTextureComponent c in collections[currentTab][currentPage])
			{
				if (c.containsPoint(x, y))
				{
					string[] split = ArgUtility.SplitBySpace(c.name);
					if (split[1] == "True" && int.TryParse(split[0], out var index))
					{
						letterviewerSubMenu = new LetterViewerMenu(index);
						letterviewerSubMenu.isFromCollection = true;
						break;
					}
				}
			}
			break;
		}
		}
	}

	public override bool shouldDrawCloseButton()
	{
		return letterviewerSubMenu == null;
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		letterviewerSubMenu?.receiveRightClick(x, y);
	}

	public override void applyMovementKey(int direction)
	{
		if (letterviewerSubMenu != null)
		{
			letterviewerSubMenu.applyMovementKey(direction);
		}
		else
		{
			base.applyMovementKey(direction);
		}
	}

	public override void gamePadButtonHeld(Buttons b)
	{
		if (letterviewerSubMenu != null)
		{
			letterviewerSubMenu.gamePadButtonHeld(b);
		}
		else
		{
			base.gamePadButtonHeld(b);
		}
	}

	public override void receiveGamePadButton(Buttons b)
	{
		if (letterviewerSubMenu != null)
		{
			letterviewerSubMenu.receiveGamePadButton(b);
		}
		else
		{
			base.receiveGamePadButton(b);
		}
	}

	public override void performHoverAction(int x, int y)
	{
		descriptionText = "";
		hoverText = "";
		value = -1;
		secretNoteImage = -1;
		if (letterviewerSubMenu != null)
		{
			letterviewerSubMenu.performHoverAction(x, y);
			return;
		}
		foreach (ClickableTextureComponent c in sideTabs.Values)
		{
			if (c.containsPoint(x, y))
			{
				hoverText = c.hoverText;
				return;
			}
		}
		bool hoveredAny = false;
		foreach (ClickableTextureComponent c in collections[currentTab][currentPage])
		{
			if (c.containsPoint(x, y, 2))
			{
				c.scale = Math.Min(c.scale + 0.02f, c.baseScale + 0.1f);
				string[] data_split = ArgUtility.SplitBySpace(c.name);
				if (currentTab == 5 || (data_split.Length > 1 && Convert.ToBoolean(data_split[1])) || (data_split.Length > 2 && Convert.ToBoolean(data_split[2])))
				{
					if (currentTab == 7)
					{
						hoverText = Game1.parseText(c.name.Substring(c.name.IndexOf(' ', c.name.IndexOf(' ') + 1) + 1), Game1.smallFont, 256);
					}
					else
					{
						hoverText = createDescription(data_split[0]);
					}
				}
				else
				{
					if (hoverText != "???")
					{
						hoverItem = null;
					}
					hoverText = "???";
				}
				hoveredAny = true;
			}
			else
			{
				c.scale = Math.Max(c.scale - 0.02f, c.baseScale);
			}
		}
		if (!hoveredAny)
		{
			hoverItem = null;
		}
		forwardButton.tryHover(x, y, 0.5f);
		backButton.tryHover(x, y, 0.5f);
	}

	public string createDescription(string id)
	{
		string description = "";
		switch (currentTab)
		{
		case 5:
		{
			if (id == "???")
			{
				return "???";
			}
			int index = int.Parse(id);
			string[] split = Game1.achievements[index].Split('^');
			description = description + split[0] + Environment.NewLine + Environment.NewLine;
			description += split[1];
			break;
		}
		case 6:
		{
			if (secretNotesData == null)
			{
				break;
			}
			int index = int.Parse(id);
			description = ((index >= GameLocation.JOURNAL_INDEX) ? (description + Game1.content.LoadString("Strings\\Locations:Journal_Name") + " #" + (index - GameLocation.JOURNAL_INDEX)) : (description + Game1.content.LoadString("Strings\\Locations:Secret_Note_Name") + " #" + index));
			if (secretNotesData[index][0] == '!')
			{
				secretNoteImage = Convert.ToInt32(ArgUtility.SplitBySpaceAndGet(secretNotesData[index], 1));
				break;
			}
			string letter_text = Game1.parseText(Utility.ParseGiftReveals(secretNotesData[index]).TrimStart(' ', '^').Replace("^", Environment.NewLine)
				.Replace("@", Game1.player.name), Game1.smallFont, 512);
			string[] split = letter_text.Split(Environment.NewLine);
			int max_lines = 15;
			if (split.Length > max_lines)
			{
				string[] new_split = new string[max_lines];
				for (int i = 0; i < max_lines; i++)
				{
					new_split[i] = split[i];
				}
				letter_text = string.Join(Environment.NewLine, new_split).Trim() + Environment.NewLine + "(...)";
			}
			description = description + Environment.NewLine + Environment.NewLine + letter_text;
			break;
		}
		default:
		{
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem(id);
			string displayName = data.DisplayName;
			if (data.Description.Contains("{0}") || data.ItemId == "DriedMushrooms")
			{
				string replaced_desc = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + data.ItemId + "_CollectionsTabDescription");
				if (replaced_desc == null)
				{
					replaced_desc = data.Description;
				}
				string replaced_name = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + data.ItemId + "_CollectionsTabName");
				if (replaced_name == null)
				{
					replaced_name = displayName;
				}
				description = description + replaced_name + Environment.NewLine + Environment.NewLine + Game1.parseText(replaced_desc, Game1.smallFont, 256) + Environment.NewLine + Environment.NewLine;
			}
			else
			{
				description = description + displayName + Environment.NewLine + Environment.NewLine + Game1.parseText(data.Description, Game1.smallFont, 256) + Environment.NewLine + Environment.NewLine;
			}
			int[] fields;
			if (data.ObjectType == "Arch")
			{
				description += (Game1.player.archaeologyFound.TryGetValue(id, out fields) ? Game1.content.LoadString("Strings\\UI:Collections_Description_ArtifactsFound", fields[0]) : "");
			}
			else if (data.ObjectType == "Cooking")
			{
				description += (Game1.player.recipesCooked.TryGetValue(id, out var timesCooked) ? Game1.content.LoadString("Strings\\UI:Collections_Description_RecipesCooked", timesCooked) : "");
				if (hoverItem == null || hoverItem.ItemId != id)
				{
					hoverItem = new Object(id, 1);
					string last_minute_1_5_hack_name = hoverItem.Name;
					switch (last_minute_1_5_hack_name)
					{
					case "Cheese Cauli.":
						last_minute_1_5_hack_name = "Cheese Cauliflower";
						break;
					case "Cheese Cauliflower":
						last_minute_1_5_hack_name = "Cheese Cauli.";
						break;
					case "Vegetable Medley":
						last_minute_1_5_hack_name = "Vegetable Stew";
						break;
					case "Cookie":
						last_minute_1_5_hack_name = "Cookies";
						break;
					case "Eggplant Parmesan":
						last_minute_1_5_hack_name = "Eggplant Parm.";
						break;
					case "Cranberry Sauce":
						last_minute_1_5_hack_name = "Cran. Sauce";
						break;
					case "Dish O' The Sea":
						last_minute_1_5_hack_name = "Dish o' The Sea";
						break;
					}
					hoverCraftingRecipe = new CraftingRecipe(last_minute_1_5_hack_name, isCookingRecipe: true);
				}
			}
			else if (!(data.ObjectType == "Fish"))
			{
				description = ((!(data.ObjectType == "Minerals") && data.Category != -2) ? (description + Game1.content.LoadString("Strings\\UI:Collections_Description_NumberShipped", Game1.player.basicShipped.TryGetValue(id, out var timesFound) ? timesFound : 0)) : (description + Game1.content.LoadString("Strings\\UI:Collections_Description_MineralsFound", Game1.player.mineralsFound.TryGetValue(id, out timesFound) ? timesFound : 0)));
			}
			else if (Game1.player.fishCaught.TryGetValue("(O)" + id, out fields))
			{
				description += Game1.content.LoadString("Strings\\UI:Collections_Description_FishCaught", fields[0]);
				if (fields[1] > 0)
				{
					description = description + Environment.NewLine + Game1.content.LoadString("Strings\\UI:Collections_Description_BiggestCatch", Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14083", (LocalizedContentManager.CurrentLanguageCode != 0) ? Math.Round((double)fields[1] * 2.54) : ((double)fields[1])));
				}
			}
			else
			{
				description += Game1.content.LoadString("Strings\\UI:Collections_Description_FishCaught", 0);
			}
			value = ObjectDataDefinition.GetRawPrice(data);
			break;
		}
		}
		return description;
	}

	public override void draw(SpriteBatch b)
	{
		foreach (ClickableTextureComponent value in sideTabs.Values)
		{
			value.draw(b);
		}
		if (currentPage > 0)
		{
			backButton.draw(b);
		}
		if (currentPage < collections[currentTab].Count - 1)
		{
			forwardButton.draw(b);
		}
		b.End();
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		foreach (ClickableTextureComponent c in collections[currentTab][currentPage])
		{
			string[] nameParts = ArgUtility.SplitBySpace(c.name);
			bool drawColor = Convert.ToBoolean(nameParts[1]);
			bool drawColorFaded = currentTab == 4 && Convert.ToBoolean(nameParts[2]);
			if (currentTab == 5 && !drawColor && c.hoverText != "???")
			{
				drawColorFaded = true;
			}
			c.draw(b, drawColorFaded ? (Color.DimGray * 0.4f) : (drawColor ? Color.White : (Color.Black * 0.2f)), 0.86f);
			if (currentTab == 5 && drawColor)
			{
				int startPos = Utility.CreateRandom(Convert.ToInt32(nameParts[0])).Next(12);
				b.Draw(Game1.mouseCursors, new Vector2(c.bounds.X + 16 + 16, c.bounds.Y + 20 + 16), new Rectangle(256 + startPos % 6 * 64 / 2, 128 + startPos / 6 * 64 / 2, 32, 32), Color.White, 0f, new Vector2(16f, 16f), c.scale, SpriteEffects.None, 0.88f);
			}
		}
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (hoverItem != null)
		{
			string desc = hoverItem.getDescription();
			string name = hoverItem.DisplayName;
			if (desc.Contains("{0}"))
			{
				string replaced_desc = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + hoverItem.Name + "_CollectionsTabDescription");
				if (replaced_desc != null)
				{
					desc = replaced_desc;
				}
				string replaced_name = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + hoverItem.Name + "_CollectionsTabName");
				if (replaced_name != null)
				{
					name = replaced_name;
				}
			}
			IClickableMenu.drawToolTip(b, desc, name, hoverItem, heldItem: false, -1, 0, null, -1, hoverCraftingRecipe);
		}
		else if (!hoverText.Equals(""))
		{
			IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont, 0, 0, this.value);
			if (secretNoteImage != -1)
			{
				IClickableMenu.drawTextureBox(b, Game1.getOldMouseX(), Game1.getOldMouseY() + 64 + 32, 288, 288, Color.White);
				b.Draw(secretNoteImageTexture, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 64 + 32 + 16), new Rectangle(secretNoteImage * 64 % secretNoteImageTexture.Width, secretNoteImage * 64 / secretNoteImageTexture.Width * 64, 64, 64), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
			}
		}
		letterviewerSubMenu?.draw(b);
	}
}
