using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Triggers;

namespace StardewValley.Menus;

public class LetterViewerMenu : IClickableMenu
{
	public const int region_backButton = 101;

	public const int region_forwardButton = 102;

	public const int region_acceptQuestButton = 103;

	public const int region_itemGrabButton = 104;

	public const int letterWidth = 320;

	public const int letterHeight = 180;

	public Texture2D letterTexture;

	public Texture2D secretNoteImageTexture;

	public int moneyIncluded;

	public int secretNoteImage = -1;

	public int whichBG;

	/// <summary>The ID of the quest attached to the letter being viewed, if any.</summary>
	public string questID;

	/// <summary>The ID of the special order attached to the letter being viewed, if any.</summary>
	public string specialOrderId;

	/// <summary>The translated name of the recipe learned from this letter, if any.</summary>
	public string learnedRecipe = "";

	public string cookingOrCrafting = "";

	public string mailTitle;

	public List<string> mailMessage = new List<string>();

	public int page;

	public readonly List<ClickableComponent> itemsToGrab = new List<ClickableComponent>();

	public float scale;

	public bool isMail;

	public bool isFromCollection;

	public new bool destroy;

	public Color? customTextColor;

	public bool usingCustomBackground;

	public ClickableTextureComponent backButton;

	public ClickableTextureComponent forwardButton;

	public ClickableComponent acceptQuestButton;

	public const float scaleChange = 0.003f;

	/// <summary>Whether the letter has an attached quest or special order which the player can accept.</summary>
	public bool HasQuestOrSpecialOrder
	{
		get
		{
			if (questID == null)
			{
				return specialOrderId != null;
			}
			return true;
		}
	}

	public LetterViewerMenu(string text)
		: base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y, 1280, 720, showUpperRightCloseButton: true)
	{
		Game1.playSound("shwip");
		backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 101,
			rightNeighborID = 102
		};
		forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 48, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 102,
			leftNeighborID = 101
		};
		letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
		text = ApplyCustomFormatting(text);
		mailMessage = SpriteText.getStringBrokenIntoSectionsOfHeight(text, width - 64, height - 128);
		forwardButton.visible = page < mailMessage.Count - 1;
		backButton.visible = page > 0;
		OnPageChange();
		populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			snapToDefaultClickableComponent();
		}
	}

	public LetterViewerMenu(int secretNoteIndex)
		: base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y, 1280, 720, showUpperRightCloseButton: true)
	{
		Game1.playSound("shwip");
		backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 101,
			rightNeighborID = 102
		};
		forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 48, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 102,
			leftNeighborID = 101
		};
		letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
		string data = DataLoader.SecretNotes(Game1.content)[secretNoteIndex];
		if (data[0] == '!')
		{
			secretNoteImageTexture = Game1.temporaryContent.Load<Texture2D>("TileSheets\\SecretNotesImages");
			secretNoteImage = Convert.ToInt32(ArgUtility.SplitBySpaceAndGet(data, 1));
		}
		else
		{
			whichBG = ((secretNoteIndex <= 1000) ? 1 : 0);
			string note_text = ApplyCustomFormatting(Utility.ParseGiftReveals(data.Replace("@", Game1.player.name)));
			mailMessage = SpriteText.getStringBrokenIntoSectionsOfHeight(note_text, width - 64, height - 128);
		}
		OnPageChange();
		forwardButton.visible = page < mailMessage.Count - 1;
		backButton.visible = page > 0;
		populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			snapToDefaultClickableComponent();
		}
	}

	public virtual void OnPageChange()
	{
		forwardButton.visible = page < mailMessage.Count - 1;
		backButton.visible = page > 0;
		foreach (ClickableComponent item in itemsToGrab)
		{
			item.visible = ShouldShowInteractable();
		}
		if (acceptQuestButton != null)
		{
			acceptQuestButton.visible = ShouldShowInteractable();
		}
		if (Game1.options.SnappyMenus && (currentlySnappedComponent == null || !currentlySnappedComponent.visible))
		{
			snapToDefaultClickableComponent();
		}
	}

	public LetterViewerMenu(string mail, string mailTitle, bool fromCollection = false)
		: base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y, 1280, 720, showUpperRightCloseButton: true)
	{
		isFromCollection = fromCollection;
		this.mailTitle = mailTitle;
		isMail = true;
		Game1.playSound("shwip");
		backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 101,
			rightNeighborID = 102
		};
		forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 48, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 102,
			leftNeighborID = 101
		};
		acceptQuestButton = new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2 - 128, yPositionOnScreen + height - 128, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).X + 24, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).Y + 24), "")
		{
			myID = 103,
			rightNeighborID = 102,
			leftNeighborID = 101
		};
		letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
		if (mailTitle.Equals("winter_5_2") || mailTitle.Equals("winter_12_1") || mailTitle.ToLower().Contains("wizard"))
		{
			whichBG = 2;
		}
		else if (mailTitle.Equals("Sandy"))
		{
			whichBG = 1;
		}
		else if (mailTitle.Contains("Krobus"))
		{
			whichBG = 3;
		}
		else if (mailTitle.Contains("passedOut1") || mailTitle.Equals("landslideDone") || mailTitle.Equals("FizzIntro"))
		{
			whichBG = 4;
		}
		try
		{
			mail = mail.Split("[#]")[0];
			mail = mail.Replace("@", Game1.player.Name);
			mail = Dialogue.applyGenderSwitch(Game1.player.Gender, mail, altTokenOnly: true);
			mail = ApplyCustomFormatting(mail);
			mail = HandleActionCommand(mail);
			mail = HandleItemCommand(mail);
			bool hideSecretSanta = fromCollection && (Game1.season != Season.Winter || Game1.dayOfMonth < 18 || Game1.dayOfMonth > 25);
			mail = mail.Replace("%secretsanta", hideSecretSanta ? "???" : Utility.GetRandomWinterStarParticipant().displayName);
			Game1.player.mailReceived.Add("sawSecretSanta" + Game1.year);
		}
		catch (Exception ex)
		{
			Game1.log.Error("Letter '" + this.mailTitle + "' couldn't be parsed.", ex);
			mail = "...";
		}
		if (mailTitle == "ccBulletinThankYou" && !Game1.player.hasOrWillReceiveMail("ccBulletinThankYouReceived"))
		{
			Utility.ForEachVillager(delegate(NPC n)
			{
				if (!n.datable)
				{
					Game1.player.changeFriendship(500, n);
				}
				return true;
			});
			Game1.addMailForTomorrow("ccBulletinThankYouReceived", noLetter: true);
		}
		int page_height = height - 128;
		if (HasInteractable())
		{
			page_height = height - 128 - 32;
		}
		mailMessage = SpriteText.getStringBrokenIntoSectionsOfHeight(mail, width - 64, page_height);
		if (mailMessage.Count == 0)
		{
			mailMessage.Add("[" + mailTitle + "]");
		}
		forwardButton.visible = page < mailMessage.Count - 1;
		backButton.visible = page > 0;
		if (Game1.options.SnappyMenus)
		{
			populateClickableComponentList();
			snapToDefaultClickableComponent();
			if (mailMessage.Count <= 1)
			{
				backButton.myID = -100;
				forwardButton.myID = -100;
			}
		}
	}

	/// <summary>Handle the <c>%action</c> command in the mail text, if present. This runs the action(s) and return the mail text with the commands stripped.</summary>
	/// <param name="mail">The mail text to parse.</param>
	public string HandleActionCommand(string mail)
	{
		int searchFromIndex = 0;
		while (true)
		{
			int startItemIndex = mail.IndexOf("%action", searchFromIndex, StringComparison.InvariantCulture);
			if (startItemIndex < 0)
			{
				break;
			}
			int endItemIndex = mail.IndexOf("%%", startItemIndex, StringComparison.InvariantCulture);
			if (endItemIndex < 0)
			{
				break;
			}
			string substring = mail.Substring(startItemIndex, endItemIndex + 2 - startItemIndex);
			mail = mail.Substring(0, startItemIndex) + mail.Substring(startItemIndex + substring.Length);
			string action = substring.Substring("%action".Length, substring.Length - "%action".Length - "%%".Length);
			searchFromIndex = startItemIndex;
			if (!isFromCollection && !TriggerActionManager.TryRunAction(action, out var error, out var ex))
			{
				Game1.log.Error($"Letter '{mailTitle}' has invalid action command '{action}': {error}", ex);
			}
		}
		return mail;
	}

	/// <summary>Handle the <c>%item</c> command in the mail text, if present. This adds the matching item to the letter and return the mail text with the command stripped.</summary>
	/// <param name="mail">The mail text to parse.</param>
	public string HandleItemCommand(string mail)
	{
		int searchFromIndex = 0;
		while (true)
		{
			int startItemIndex = mail.IndexOf("%item", searchFromIndex, StringComparison.InvariantCulture);
			if (startItemIndex < 0)
			{
				break;
			}
			int endItemIndex = mail.IndexOf("%%", startItemIndex, StringComparison.InvariantCulture);
			if (endItemIndex < 0)
			{
				break;
			}
			string substring = mail.Substring(startItemIndex, endItemIndex + 2 - startItemIndex);
			mail = mail.Substring(0, startItemIndex) + mail.Substring(startItemIndex + substring.Length);
			string[] typeAndArgs = ArgUtility.SplitBySpace(substring.Substring("%item".Length, substring.Length - "%item".Length - "%%".Length), 2);
			string type = typeAndArgs[0];
			string[] args = ((typeAndArgs.Length > 1) ? ArgUtility.SplitBySpace(typeAndArgs[1]) : LegacyShims.EmptyArray<string>());
			searchFromIndex = startItemIndex;
			if (isFromCollection)
			{
				continue;
			}
			switch (type.ToLower())
			{
			case "id":
			{
				string id;
				int count;
				if (args.Length == 1)
				{
					id = args[0];
					count = 1;
				}
				else
				{
					int index = Game1.random.Next(args.Length);
					index -= index % 2;
					id = args[index];
					count = int.Parse(args[index + 1]);
				}
				Item item = ItemRegistry.Create(id, count);
				itemsToGrab.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2 - 48, yPositionOnScreen + height - 32 - 96, 96, 96), item)
				{
					myID = 104,
					leftNeighborID = 101,
					rightNeighborID = 102
				});
				backButton.rightNeighborID = 104;
				forwardButton.leftNeighborID = 104;
				break;
			}
			case "object":
			{
				int which = Game1.random.Next(args.Length);
				which -= which % 2;
				Item o = ItemRegistry.Create(args[which], int.Parse(args[which + 1]));
				itemsToGrab.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2 - 48, yPositionOnScreen + height - 32 - 96, 96, 96), o)
				{
					myID = 104,
					leftNeighborID = 101,
					rightNeighborID = 102
				});
				backButton.rightNeighborID = 104;
				forwardButton.leftNeighborID = 104;
				break;
			}
			case "tools":
			{
				string[] array = args;
				foreach (string arg in array)
				{
					Item tool = null;
					switch (arg)
					{
					case "Axe":
					case "Hoe":
					case "Pickaxe":
						tool = ItemRegistry.Create("(T)" + arg);
						break;
					case "Can":
						tool = ItemRegistry.Create("(T)WateringCan");
						break;
					case "Scythe":
						tool = ItemRegistry.Create("(W)47");
						break;
					}
					if (tool != null)
					{
						itemsToGrab.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2 - 48, yPositionOnScreen + height - 32 - 96, 96, 96), tool));
					}
				}
				break;
			}
			case "bigobject":
			{
				string id = Game1.random.ChooseFrom(args);
				Item o = ItemRegistry.Create("(BC)" + id);
				itemsToGrab.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2 - 48, yPositionOnScreen + height - 32 - 96, 96, 96), o)
				{
					myID = 104,
					leftNeighborID = 101,
					rightNeighborID = 102
				});
				backButton.rightNeighborID = 104;
				forwardButton.leftNeighborID = 104;
				break;
			}
			case "furniture":
			{
				string id = Game1.random.ChooseFrom(args);
				Item o = ItemRegistry.Create("(F)" + id);
				itemsToGrab.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2 - 48, yPositionOnScreen + height - 32 - 96, 96, 96), o)
				{
					myID = 104,
					leftNeighborID = 101,
					rightNeighborID = 102
				});
				backButton.rightNeighborID = 104;
				forwardButton.leftNeighborID = 104;
				break;
			}
			case "money":
			{
				int moneyToAdd = ((args.Length > 1) ? Game1.random.Next(Convert.ToInt32(args[0]), Convert.ToInt32(args[1])) : Convert.ToInt32(args[0]));
				moneyToAdd -= moneyToAdd % 10;
				Game1.player.Money += moneyToAdd;
				moneyIncluded = moneyToAdd;
				break;
			}
			case "conversationtopic":
			{
				string topic = args[0];
				int numDays = Convert.ToInt32(args[1]);
				Game1.player.activeDialogueEvents.Add(topic, numDays);
				if (topic.Equals("ElliottGone3"))
				{
					Utility.getHomeOfFarmer(Game1.player).fridge.Value.addItem(ItemRegistry.Create("(O)732"));
				}
				break;
			}
			case "cookingrecipe":
			{
				Dictionary<string, string> cookingRecipes = DataLoader.CookingRecipes(Game1.content);
				string recipeKey = string.Join(" ", args);
				if (string.IsNullOrWhiteSpace(recipeKey))
				{
					int lowest_required_heart_level = 1000;
					foreach (string s in cookingRecipes.Keys)
					{
						string[] getConditions = ArgUtility.SplitBySpace(ArgUtility.Get(cookingRecipes[s].Split('/'), 3));
						string conditionKey = ArgUtility.Get(getConditions, 0);
						string npcName = ArgUtility.Get(getConditions, 1);
						if (conditionKey == "f" && npcName == mailTitle.Replace("Cooking", "") && !Game1.player.cookingRecipes.ContainsKey(s))
						{
							int required_heart_level = Convert.ToInt32(getConditions[2]);
							if (required_heart_level <= lowest_required_heart_level)
							{
								lowest_required_heart_level = required_heart_level;
								recipeKey = s;
							}
						}
					}
				}
				if (!string.IsNullOrWhiteSpace(recipeKey))
				{
					if (cookingRecipes.ContainsKey(recipeKey))
					{
						Game1.player.cookingRecipes.TryAdd(recipeKey, 0);
						learnedRecipe = new CraftingRecipe(recipeKey, isCookingRecipe: true).DisplayName;
						cookingOrCrafting = Game1.content.LoadString("Strings\\UI:LearnedRecipe_cooking");
						break;
					}
					Game1.log.Warn($"Letter '{mailTitle}' has unknown cooking recipe '{recipeKey}'.");
				}
				break;
			}
			case "craftingrecipe":
			{
				Dictionary<string, string> craftingRecipes = DataLoader.CraftingRecipes(Game1.content);
				if (craftingRecipes.TryGetValue(args[0], out var rawFields))
				{
					learnedRecipe = args[0];
				}
				else
				{
					string fallbackKey = args[0].Replace('_', ' ');
					if (!craftingRecipes.TryGetValue(fallbackKey, out rawFields))
					{
						Game1.log.Warn($"Letter '{mailTitle}' has unknown crafting recipe '{args[0]}'{((args[0] != fallbackKey) ? (" or '" + fallbackKey + "'") : "")}.");
						break;
					}
					learnedRecipe = fallbackKey;
				}
				Game1.player.craftingRecipes.TryAdd(learnedRecipe, 0);
				learnedRecipe = new CraftingRecipe(learnedRecipe, isCookingRecipe: false).DisplayName;
				cookingOrCrafting = Game1.content.LoadString("Strings\\UI:LearnedRecipe_crafting");
				break;
			}
			case "itemrecovery":
				if (Game1.player.recoveredItem != null)
				{
					Item item = Game1.player.recoveredItem;
					Game1.player.recoveredItem = null;
					itemsToGrab.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2 - 48, yPositionOnScreen + height - 32 - 96, 96, 96), item)
					{
						myID = 104,
						leftNeighborID = 101,
						rightNeighborID = 102
					});
					backButton.rightNeighborID = 104;
					forwardButton.leftNeighborID = 104;
				}
				break;
			case "quest":
				questID = args[0];
				if (args.Length > 1)
				{
					if (!Game1.player.mailReceived.Contains("NOQUEST_" + questID))
					{
						Game1.player.addQuest(questID);
					}
					questID = null;
				}
				backButton.rightNeighborID = 103;
				forwardButton.leftNeighborID = 103;
				break;
			case "specialorder":
			{
				specialOrderId = args[0];
				if (ArgUtility.TryGetBool(args, 1, out var addImmediately, out var _) && addImmediately)
				{
					if (!Game1.player.mailReceived.Contains("NOSPECIALORDER_" + specialOrderId))
					{
						Game1.player.team.AddSpecialOrder(specialOrderId);
					}
					specialOrderId = null;
				}
				backButton.rightNeighborID = 103;
				forwardButton.leftNeighborID = 103;
				break;
			}
			}
		}
		return mail;
	}

	public virtual string ApplyCustomFormatting(string text)
	{
		text = Dialogue.applyGenderSwitchBlocks(Game1.player.Gender, text);
		for (int index = text.IndexOf("["); index >= 0; index = text.IndexOf("[", index + 1))
		{
			int end_index = text.IndexOf("]", index);
			if (end_index >= 0)
			{
				bool valid_tag = false;
				try
				{
					string[] split = ArgUtility.SplitBySpace(text.Substring(index + 1, end_index - index - 1));
					string text2 = split[0];
					if (!(text2 == "letterbg"))
					{
						if (text2 == "textcolor")
						{
							string color_string = split[1].ToLower();
							string[] color_lookup = new string[10] { "black", "blue", "red", "purple", "white", "orange", "green", "cyan", "gray", "jojablue" };
							customTextColor = null;
							for (int i = 0; i < color_lookup.Length; i++)
							{
								if (color_string == color_lookup[i])
								{
									customTextColor = SpriteText.getColorFromIndex(i);
									break;
								}
							}
							valid_tag = true;
						}
					}
					else
					{
						switch (split.Length)
						{
						case 2:
							whichBG = int.Parse(split[1]);
							break;
						case 3:
							usingCustomBackground = true;
							letterTexture = Game1.temporaryContent.Load<Texture2D>(split[1]);
							whichBG = int.Parse(split[2]);
							break;
						}
						valid_tag = true;
					}
				}
				catch (Exception)
				{
				}
				if (valid_tag)
				{
					text = text.Remove(index, end_index - index + 1);
					index--;
				}
			}
		}
		return text;
	}

	public override void snapToDefaultClickableComponent()
	{
		if (HasQuestOrSpecialOrder && ShouldShowInteractable())
		{
			currentlySnappedComponent = getComponentWithID(103);
		}
		else if (itemsToGrab.Count > 0 && ShouldShowInteractable())
		{
			currentlySnappedComponent = getComponentWithID(104);
		}
		else if (currentlySnappedComponent == null || (currentlySnappedComponent != backButton && currentlySnappedComponent != forwardButton))
		{
			currentlySnappedComponent = forwardButton;
		}
		snapCursorToCurrentSnappedComponent();
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X;
		yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y;
		backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 101,
			rightNeighborID = 102
		};
		forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 48, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 102,
			leftNeighborID = 101
		};
		acceptQuestButton = new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2 - 128, yPositionOnScreen + height - 128, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).X + 24, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).Y + 24), "")
		{
			myID = 103,
			rightNeighborID = 102,
			leftNeighborID = 101
		};
		foreach (ClickableComponent item in itemsToGrab)
		{
			item.bounds = new Rectangle(xPositionOnScreen + width / 2 - 48, yPositionOnScreen + height - 32 - 96, 96, 96);
		}
	}

	public override void receiveKeyPress(Keys key)
	{
		if (key != 0)
		{
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
			{
				exitThisMenu(ShouldPlayExitSound());
			}
			else
			{
				base.receiveKeyPress(key);
			}
		}
	}

	public override void receiveGamePadButton(Buttons b)
	{
		base.receiveGamePadButton(b);
		switch (b)
		{
		case Buttons.B:
			if (isFromCollection)
			{
				exitThisMenu(playSound: false);
			}
			break;
		case Buttons.LeftTrigger:
			if (page > 0)
			{
				page--;
				Game1.playSound("shwip");
				OnPageChange();
			}
			break;
		case Buttons.RightTrigger:
			if (page < mailMessage.Count - 1)
			{
				page++;
				Game1.playSound("shwip");
				OnPageChange();
			}
			break;
		}
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (scale < 1f)
		{
			return;
		}
		if (upperRightCloseButton != null && readyToClose() && upperRightCloseButton.containsPoint(x, y))
		{
			if (playSound)
			{
				Game1.playSound("bigDeSelect");
			}
			if (!isFromCollection)
			{
				exitThisMenu(ShouldPlayExitSound());
			}
			else
			{
				destroy = true;
			}
		}
		if (Game1.activeClickableMenu == null && Game1.currentMinigame == null)
		{
			unload();
			return;
		}
		if (ShouldShowInteractable())
		{
			for (int i = 0; i < itemsToGrab.Count; i++)
			{
				ClickableComponent c = itemsToGrab[i];
				if (c.containsPoint(x, y) && c.item != null)
				{
					Game1.playSound("coin");
					Game1.player.addItemByMenuIfNecessary(c.item);
					c.item = null;
					if (itemsToGrab.Count > 1)
					{
						itemsToGrab.RemoveAt(i);
					}
					return;
				}
			}
		}
		if (backButton.containsPoint(x, y) && page > 0)
		{
			page--;
			Game1.playSound("shwip");
			OnPageChange();
		}
		else if (forwardButton.containsPoint(x, y) && page < mailMessage.Count - 1)
		{
			page++;
			Game1.playSound("shwip");
			OnPageChange();
		}
		else if (ShouldShowInteractable() && acceptQuestButton != null && acceptQuestButton.containsPoint(x, y))
		{
			AcceptQuest();
		}
		else if (isWithinBounds(x, y))
		{
			if (page < mailMessage.Count - 1)
			{
				page++;
				Game1.playSound("shwip");
				OnPageChange();
			}
			else if (!isMail)
			{
				exitThisMenuNoSound();
				Game1.playSound("shwip");
			}
			else if (isFromCollection)
			{
				destroy = true;
			}
		}
		else if (!itemsLeftToGrab())
		{
			if (!isFromCollection)
			{
				exitThisMenuNoSound();
				Game1.playSound("shwip");
			}
			else
			{
				destroy = true;
			}
		}
	}

	public virtual bool ShouldPlayExitSound()
	{
		if (HasQuestOrSpecialOrder)
		{
			return false;
		}
		if (isFromCollection)
		{
			return false;
		}
		return true;
	}

	public bool itemsLeftToGrab()
	{
		foreach (ClickableComponent item in itemsToGrab)
		{
			if (item.item != null)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Add the attached quest or special order to the player.</summary>
	public void AcceptQuest()
	{
		if (questID != null)
		{
			Game1.player.addQuest(questID);
			if (questID == "20")
			{
				MineShaft.CheckForQiChallengeCompletion();
			}
			questID = null;
			Game1.playSound("newArtifact");
		}
		else if (specialOrderId != null)
		{
			Game1.player.team.AddSpecialOrder(specialOrderId);
			specialOrderId = null;
			Game1.playSound("newArtifact");
		}
	}

	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		if (ShouldShowInteractable())
		{
			foreach (ClickableComponent c in itemsToGrab)
			{
				if (c.containsPoint(x, y))
				{
					c.scale = Math.Min(c.scale + 0.03f, 1.1f);
				}
				else
				{
					c.scale = Math.Max(1f, c.scale - 0.03f);
				}
			}
		}
		backButton.tryHover(x, y, 0.6f);
		forwardButton.tryHover(x, y, 0.6f);
		if (ShouldShowInteractable() && HasQuestOrSpecialOrder)
		{
			float oldScale = acceptQuestButton.scale;
			acceptQuestButton.scale = (acceptQuestButton.bounds.Contains(x, y) ? 1.5f : 1f);
			if (acceptQuestButton.scale > oldScale)
			{
				Game1.playSound("Cowboy_gunshot");
			}
		}
	}

	public override void update(GameTime time)
	{
		base.update(time);
		forwardButton.visible = page < mailMessage.Count - 1;
		backButton.visible = page > 0;
		if (scale < 1f)
		{
			scale += (float)time.ElapsedGameTime.Milliseconds * 0.003f;
			if (scale >= 1f)
			{
				scale = 1f;
			}
		}
		if (page < mailMessage.Count - 1 && !forwardButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()))
		{
			forwardButton.scale = 4f + (float)Math.Sin((double)(float)time.TotalGameTime.Milliseconds / (Math.PI * 64.0)) / 1.5f;
		}
	}

	public virtual Color? getTextColor()
	{
		if (customTextColor.HasValue)
		{
			return customTextColor.Value;
		}
		if (usingCustomBackground)
		{
			return null;
		}
		return whichBG switch
		{
			1 => SpriteText.color_Gray, 
			2 => SpriteText.color_Cyan, 
			3 => SpriteText.color_White, 
			4 => SpriteText.color_JojaBlue, 
			_ => null, 
		};
	}

	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
		}
		b.Draw(letterTexture, new Vector2(xPositionOnScreen + width / 2, yPositionOnScreen + height / 2), new Rectangle(whichBG % 4 * 320, (whichBG >= 4) ? (204 + (whichBG / 4 - 1) * 180) : 0, 320, 180), Color.White, 0f, new Vector2(160f, 90f), 4f * scale, SpriteEffects.None, 0.86f);
		if (scale == 1f)
		{
			if (secretNoteImage != -1)
			{
				b.Draw(secretNoteImageTexture, new Vector2(xPositionOnScreen + width / 2 - 128 - 4, yPositionOnScreen + height / 2 - 128 + 8), new Rectangle(secretNoteImage * 64 % secretNoteImageTexture.Width, secretNoteImage * 64 / secretNoteImageTexture.Width * 64, 64, 64), Color.Black * 0.4f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
				b.Draw(secretNoteImageTexture, new Vector2(xPositionOnScreen + width / 2 - 128, yPositionOnScreen + height / 2 - 128), new Rectangle(secretNoteImage * 64 % secretNoteImageTexture.Width, secretNoteImage * 64 / secretNoteImageTexture.Width * 64, 64, 64), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.865f);
				b.Draw(secretNoteImageTexture, new Vector2(xPositionOnScreen + width / 2 - 40, yPositionOnScreen + height / 2 - 192), new Rectangle(193, 65, 14, 21), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.867f);
			}
			else
			{
				SpriteText.drawString(b, mailMessage[page], xPositionOnScreen + 32, yPositionOnScreen + 32, 999999, width - 64, 999999, 0.75f, 0.865f, junimoText: false, -1, "", getTextColor());
			}
			if (ShouldShowInteractable())
			{
				using (List<ClickableComponent>.Enumerator enumerator = itemsToGrab.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						ClickableComponent c = enumerator.Current;
						b.Draw(letterTexture, c.bounds, new Rectangle(whichBG * 24, 180, 24, 24), Color.White);
						c.item?.drawInMenu(b, new Vector2(c.bounds.X + 16, c.bounds.Y + 16), c.scale);
					}
				}
				if (moneyIncluded > 0)
				{
					string moneyText = Game1.content.LoadString("Strings\\UI:LetterViewer_MoneyIncluded", moneyIncluded);
					SpriteText.drawString(b, moneyText, xPositionOnScreen + width / 2 - SpriteText.getWidthOfString(moneyText) / 2, yPositionOnScreen + height - 96, 999999, -1, 9999, 0.75f, 0.865f, junimoText: false, -1, "", getTextColor());
				}
				else
				{
					string text = learnedRecipe;
					if (text != null && text.Length > 0)
					{
						string recipeText = Game1.content.LoadString("Strings\\UI:LetterViewer_LearnedRecipe", cookingOrCrafting);
						SpriteText.drawStringHorizontallyCenteredAt(b, recipeText, xPositionOnScreen + width / 2, yPositionOnScreen + height - 32 - SpriteText.getHeightOfString(recipeText) * 2, 999999, -1, 9999, 0.65f, 0.865f, junimoText: false, getTextColor());
						SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:LetterViewer_LearnedRecipeName", learnedRecipe), xPositionOnScreen + width / 2, yPositionOnScreen + height - 32 - SpriteText.getHeightOfString("t"), 999999, -1, 9999, 0.9f, 0.865f, junimoText: false, getTextColor());
					}
				}
			}
			base.draw(b);
			forwardButton.draw(b);
			backButton.draw(b);
			if (ShouldShowInteractable() && HasQuestOrSpecialOrder)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), acceptQuestButton.bounds.X, acceptQuestButton.bounds.Y, acceptQuestButton.bounds.Width, acceptQuestButton.bounds.Height, (acceptQuestButton.scale > 1f) ? Color.LightPink : Color.White, 4f * acceptQuestButton.scale);
				Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AcceptQuest"), Game1.dialogueFont, new Vector2(acceptQuestButton.bounds.X + 12, acceptQuestButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12)), Game1.textColor);
			}
		}
		if ((!Game1.options.SnappyMenus || !(scale < 1f)) && (!Game1.options.SnappyMenus || forwardButton.visible || backButton.visible || HasQuestOrSpecialOrder || itemsLeftToGrab()))
		{
			drawMouse(b);
		}
	}

	public virtual bool ShouldShowInteractable()
	{
		if (!HasInteractable())
		{
			return false;
		}
		return page == mailMessage.Count - 1;
	}

	public virtual bool HasInteractable()
	{
		if (isFromCollection)
		{
			return false;
		}
		if (HasQuestOrSpecialOrder)
		{
			return true;
		}
		if (moneyIncluded > 0)
		{
			return true;
		}
		if (itemsToGrab.Count > 0)
		{
			return true;
		}
		string text = learnedRecipe;
		if (text != null && text.Length > 0)
		{
			return true;
		}
		return false;
	}

	public void unload()
	{
	}

	protected override void cleanupBeforeExit()
	{
		if (HasQuestOrSpecialOrder)
		{
			AcceptQuest();
		}
		if (itemsLeftToGrab())
		{
			List<Item> items = new List<Item>();
			foreach (ClickableComponent c in itemsToGrab)
			{
				if (c.item != null)
				{
					items.Add(c.item);
				}
			}
			itemsToGrab.Clear();
			if (items.Count > 0)
			{
				Game1.playSound("coin");
				Game1.player.addItemsByMenuIfNecessary(items);
			}
		}
		if (isFromCollection)
		{
			destroy = true;
			Game1.oldKBState = Game1.GetKeyboardState();
			Game1.oldMouseState = Game1.input.GetMouseState();
			Game1.oldPadState = Game1.input.GetGamePadState();
		}
		base.cleanupBeforeExit();
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		if (isFromCollection)
		{
			destroy = true;
		}
		else
		{
			receiveLeftClick(x, y, playSound);
		}
	}
}
