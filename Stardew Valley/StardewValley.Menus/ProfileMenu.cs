using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Menus;

public class ProfileMenu : IClickableMenu
{
	public class ProfileItemCategory
	{
		public string categoryName;

		public int[] validCategories;

		public ProfileItemCategory(string name, int[] valid_categories)
		{
			categoryName = name;
			validCategories = valid_categories;
		}
	}

	public const int region_characterSelectors = 500;

	public const int region_categorySelector = 501;

	public const int region_itemButtons = 502;

	public const int region_backButton = 101;

	public const int region_forwardButton = 102;

	public const int region_upArrow = 105;

	public const int region_downArrow = 106;

	public const int letterWidth = 320;

	public const int letterHeight = 180;

	public Texture2D letterTexture;

	protected string hoverText = "";

	protected List<ProfileItem> _profileItems;

	public Item hoveredItem;

	public ClickableTextureComponent backButton;

	public ClickableTextureComponent forwardButton;

	public ClickableTextureComponent nextCharacterButton;

	public ClickableTextureComponent previousCharacterButton;

	protected Rectangle characterSpriteBox;

	protected int _currentCategory;

	protected AnimatedSprite _animatedSprite;

	protected float _directionChangeTimer;

	protected float _hiddenEmoteTimer = -1f;

	protected int _currentDirection;

	protected int _hideTooltipTime;

	protected SocialPage _socialPage;

	protected string _status = "";

	protected string _printedName = "";

	protected Vector2 _characterEntrancePosition = new Vector2(0f, 0f);

	public ClickableTextureComponent upArrow;

	public ClickableTextureComponent downArrow;

	protected ClickableTextureComponent scrollBar;

	protected Rectangle scrollBarRunner;

	public List<ClickableComponent> clickableProfileItems;

	/// <summary>The current character being shown in the menu.</summary>
	public SocialPage.SocialEntry Current;

	/// <summary>The social entries for characters that can be viewed in the profile menu.</summary>
	public readonly List<SocialPage.SocialEntry> SocialEntries = new List<SocialPage.SocialEntry>();

	protected Vector2 _characterNamePosition;

	protected Vector2 _heartDisplayPosition;

	protected Vector2 _birthdayHeadingDisplayPosition;

	protected Vector2 _birthdayDisplayPosition;

	protected Vector2 _statusHeadingDisplayPosition;

	protected Vector2 _statusDisplayPosition;

	protected Vector2 _giftLogHeadingDisplayPosition;

	protected Vector2 _giftLogCategoryDisplayPosition;

	protected Vector2 _errorMessagePosition;

	protected Vector2 _characterSpriteDrawPosition;

	protected Rectangle _characterStatusDisplayBox;

	protected List<ClickableTextureComponent> _clickableTextureComponents;

	public Rectangle _itemDisplayRect;

	protected int scrollPosition;

	protected int scrollStep = 36;

	protected int scrollSize;

	public static ProfileItemCategory[] itemCategories = new ProfileItemCategory[10]
	{
		new ProfileItemCategory("Profile_Gift_Category_LikedGifts", null),
		new ProfileItemCategory("Profile_Gift_Category_FruitsAndVegetables", new int[2] { -75, -79 }),
		new ProfileItemCategory("Profile_Gift_Category_AnimalProduce", new int[4] { -6, -5, -14, -18 }),
		new ProfileItemCategory("Profile_Gift_Category_ArtisanItems", new int[1] { -26 }),
		new ProfileItemCategory("Profile_Gift_Category_CookedItems", new int[1] { -7 }),
		new ProfileItemCategory("Profile_Gift_Category_ForagedItems", new int[4] { -80, -81, -23, -17 }),
		new ProfileItemCategory("Profile_Gift_Category_Fish", new int[1] { -4 }),
		new ProfileItemCategory("Profile_Gift_Category_Ingredients", new int[2] { -27, -25 }),
		new ProfileItemCategory("Profile_Gift_Category_MineralsAndGems", new int[3] { -15, -12, -2 }),
		new ProfileItemCategory("Profile_Gift_Category_Misc", null)
	};

	protected Dictionary<int, List<Item>> _sortedItems;

	public bool scrolling;

	private int _characterSpriteRandomInt;

	public ProfileMenu(SocialPage.SocialEntry subject, List<SocialPage.SocialEntry> allSocialEntries)
		: base((int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y, 1280, 720, showUpperRightCloseButton: true)
	{
		_printedName = "";
		_characterEntrancePosition = new Vector2(0f, 4f);
		foreach (SocialPage.SocialEntry entry in allSocialEntries)
		{
			if (entry.Character is NPC && entry.IsMet)
			{
				SocialEntries.Add(entry);
			}
		}
		_profileItems = new List<ProfileItem>();
		clickableProfileItems = new List<ClickableComponent>();
		UpdateButtons();
		letterTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\letterBG");
		_SetCharacter(subject);
	}

	protected void _SetCharacter(SocialPage.SocialEntry entry)
	{
		Current = entry;
		_sortedItems = new Dictionary<int, List<Item>>();
		if (Current.Character is NPC npc)
		{
			_animatedSprite = npc.Sprite.Clone();
			_animatedSprite.tempSpriteHeight = -1;
			_animatedSprite.faceDirection(2);
			foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				if (!Game1.player.hasGiftTasteBeenRevealed(npc, data.ItemId))
				{
					continue;
				}
				Object item = ItemRegistry.Create<Object>(data.QualifiedItemId);
				if (item.IsBreakableStone())
				{
					continue;
				}
				for (int i = 0; i < itemCategories.Length; i++)
				{
					string categoryName = itemCategories[i].categoryName;
					if (!(categoryName == "Profile_Gift_Category_LikedGifts"))
					{
						if (categoryName == "Profile_Gift_Category_Misc")
						{
							bool is_accounted_for = false;
							for (int j = 0; j < itemCategories.Length; j++)
							{
								if (itemCategories[j].validCategories != null && itemCategories[j].validCategories.Contains(item.Category))
								{
									is_accounted_for = true;
									break;
								}
							}
							if (!is_accounted_for)
							{
								if (!_sortedItems.TryGetValue(i, out var categoryItems))
								{
									categoryItems = (_sortedItems[i] = new List<Item>());
								}
								categoryItems.Add(item);
							}
						}
						else if (itemCategories[i].validCategories.Contains(item.Category))
						{
							if (!_sortedItems.TryGetValue(i, out var categoryItems))
							{
								categoryItems = (_sortedItems[i] = new List<Item>());
							}
							categoryItems.Add(item);
						}
						continue;
					}
					int gift_taste = npc.getGiftTasteForThisItem(item);
					if (gift_taste == 2 || gift_taste == 0)
					{
						if (!_sortedItems.TryGetValue(i, out var categoryItems))
						{
							categoryItems = (_sortedItems[i] = new List<Item>());
						}
						categoryItems.Add(item);
					}
				}
			}
			Gender gender = Current.Gender;
			bool isDatable = Current.IsDatable;
			bool housemate = Current.IsRoommateForCurrentPlayer();
			_status = "";
			if (isDatable || housemate)
			{
				string text = ((!Game1.content.ShouldUseGenderedCharacterTranslations()) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635") : ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/')[0] : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').Last()));
				if (housemate)
				{
					text = Game1.content.LoadString("Strings\\StringsFromCSFiles:Housemate");
				}
				else if (Current.IsMarriedToCurrentPlayer())
				{
					text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11636") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11637"));
				}
				else if (Current.IsMarriedToAnyone())
				{
					text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_MaleNPC") : Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_FemaleNPC"));
				}
				else if (!Game1.player.isMarriedOrRoommates() && Current.IsDatingCurrentPlayer())
				{
					text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11639") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11640"));
				}
				else if (Current.IsDivorcedFromCurrentPlayer())
				{
					text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11642") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11643"));
				}
				text = Game1.parseText(text, Game1.smallFont, width);
				string status = text.Replace("(", "").Replace(")", "").Replace("（", "")
					.Replace("）", "");
				status = Utility.capitalizeFirstLetter(status);
				_status = status;
			}
			_UpdateList();
		}
		_directionChangeTimer = 2000f;
		_currentDirection = 2;
		_hiddenEmoteTimer = -1f;
	}

	public void ChangeCharacter(int offset)
	{
		int index = SocialEntries.IndexOf(Current);
		if (index == -1)
		{
			if (SocialEntries.Count > 0)
			{
				_SetCharacter(SocialEntries[0]);
			}
			return;
		}
		for (index += offset; index < 0; index += SocialEntries.Count)
		{
		}
		while (index >= SocialEntries.Count)
		{
			index -= SocialEntries.Count;
		}
		_SetCharacter(SocialEntries[index]);
		Game1.playSound("smallSelect");
		_printedName = "";
		_characterEntrancePosition = new Vector2(Math.Sign(offset) * -4, 0f);
		if (Game1.options.SnappyMenus && (currentlySnappedComponent == null || !currentlySnappedComponent.visible))
		{
			snapToDefaultClickableComponent();
		}
	}

	protected void _UpdateList()
	{
		for (int i = 0; i < _profileItems.Count; i++)
		{
			_profileItems[i].Unload();
		}
		_profileItems.Clear();
		if (!(Current.Character is NPC npc))
		{
			return;
		}
		List<Item> loved_items = new List<Item>();
		List<Item> liked_items = new List<Item>();
		List<Item> neutral_items = new List<Item>();
		List<Item> disliked_items = new List<Item>();
		List<Item> hated_items = new List<Item>();
		if (_sortedItems.TryGetValue(_currentCategory, out var categoryItems))
		{
			foreach (Item item in categoryItems)
			{
				switch (npc.getGiftTasteForThisItem(item))
				{
				case 0:
					loved_items.Add(item);
					break;
				case 2:
					liked_items.Add(item);
					break;
				case 8:
					neutral_items.Add(item);
					break;
				case 4:
					disliked_items.Add(item);
					break;
				case 6:
					hated_items.Add(item);
					break;
				}
			}
		}
		PI_ItemList item_display = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Loved"), loved_items);
		_profileItems.Add(item_display);
		item_display = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Liked"), liked_items);
		_profileItems.Add(item_display);
		item_display = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Neutral"), neutral_items);
		_profileItems.Add(item_display);
		item_display = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Disliked"), disliked_items);
		_profileItems.Add(item_display);
		item_display = new PI_ItemList(this, Game1.content.LoadString("Strings\\UI:Profile_Gift_Hated"), hated_items);
		_profileItems.Add(item_display);
		SetupLayout();
		populateClickableComponentList();
		if (Game1.options.snappyMenus && Game1.options.gamepadControls && (currentlySnappedComponent == null || !allClickableComponents.Contains(currentlySnappedComponent)))
		{
			snapToDefaultClickableComponent();
		}
	}

	public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
	{
		if (direction == 2 && a.region == 501 && b.region == 500)
		{
			return false;
		}
		return base.IsAutomaticSnapValid(direction, a, b);
	}

	public override void snapToDefaultClickableComponent()
	{
		if (clickableProfileItems.Count > 0)
		{
			currentlySnappedComponent = clickableProfileItems[0];
		}
		else
		{
			currentlySnappedComponent = backButton;
		}
		snapCursorToCurrentSnappedComponent();
	}

	public void UpdateButtons()
	{
		_clickableTextureComponents = new List<ClickableTextureComponent>();
		upArrow = new ClickableTextureComponent(new Rectangle(0, 0, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f)
		{
			myID = 105,
			upNeighborID = 102,
			upNeighborImmutable = true,
			downNeighborID = 106,
			downNeighborImmutable = true,
			leftNeighborID = -99998,
			leftNeighborImmutable = true
		};
		downArrow = new ClickableTextureComponent(new Rectangle(0, 0, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f)
		{
			myID = 106,
			upNeighborID = 105,
			upNeighborImmutable = true,
			leftNeighborID = -99998,
			leftNeighborImmutable = true
		};
		scrollBar = new ClickableTextureComponent(new Rectangle(0, 0, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
		backButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 101,
			name = "Back Button",
			upNeighborID = -99998,
			downNeighborID = -99998,
			downNeighborImmutable = true,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			region = 501
		};
		_clickableTextureComponents.Add(backButton);
		forwardButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 48, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 102,
			name = "Forward Button",
			upNeighborID = -99998,
			downNeighborID = -99998,
			downNeighborImmutable = true,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			region = 501
		};
		_clickableTextureComponents.Add(forwardButton);
		previousCharacterButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 32, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 0,
			name = "Previous Char",
			upNeighborID = -99998,
			downNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			region = 500
		};
		_clickableTextureComponents.Add(previousCharacterButton);
		nextCharacterButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 32 - 48, yPositionOnScreen + height - 32 - 64, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 0,
			name = "Next Char",
			upNeighborID = -99998,
			downNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			region = 500
		};
		_clickableTextureComponents.Add(nextCharacterButton);
		_clickableTextureComponents.Add(upArrow);
		_clickableTextureComponents.Add(downArrow);
	}

	public override void receiveScrollWheelAction(int direction)
	{
		base.receiveScrollWheelAction(direction);
		if (direction > 0)
		{
			Scroll(-scrollStep);
		}
		else if (direction < 0)
		{
			Scroll(scrollStep);
		}
	}

	public void ChangePage(int offset)
	{
		scrollPosition = 0;
		_currentCategory += offset;
		while (_currentCategory < 0)
		{
			_currentCategory += itemCategories.Length;
		}
		while (_currentCategory >= itemCategories.Length)
		{
			_currentCategory -= itemCategories.Length;
		}
		Game1.playSound("shwip");
		_UpdateList();
		if (Game1.options.SnappyMenus && (currentlySnappedComponent == null || !currentlySnappedComponent.visible))
		{
			snapToDefaultClickableComponent();
		}
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).X;
		yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(1280, 720).Y;
		UpdateButtons();
		SetupLayout();
		initializeUpperRightCloseButton();
		populateClickableComponentList();
	}

	public override void receiveGamePadButton(Buttons b)
	{
		base.receiveGamePadButton(b);
		switch (b)
		{
		case Buttons.LeftTrigger:
			ChangePage(-1);
			break;
		case Buttons.RightTrigger:
			ChangePage(1);
			break;
		case Buttons.RightShoulder:
			ChangeCharacter(1);
			break;
		case Buttons.LeftShoulder:
			ChangeCharacter(-1);
			break;
		case Buttons.Back:
			PlayHiddenEmote();
			break;
		}
	}

	public override void receiveKeyPress(Keys key)
	{
		if (key != 0)
		{
			if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && readyToClose())
			{
				exitThisMenu();
			}
			else if (Game1.options.snappyMenus && Game1.options.gamepadControls && !overrideSnappyMenuCursorMovementBan())
			{
				applyMovementKey(key);
			}
		}
	}

	public override void applyMovementKey(int direction)
	{
		base.applyMovementKey(direction);
		ConstrainSelectionToView();
	}

	public override void releaseLeftClick(int x, int y)
	{
		base.releaseLeftClick(x, y);
		scrolling = false;
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (scrollBar.containsPoint(x, y))
		{
			scrolling = true;
		}
		else if (scrollBarRunner.Contains(x, y))
		{
			scrolling = true;
			leftClickHeld(x, y);
			releaseLeftClick(x, y);
		}
		if (upperRightCloseButton != null && readyToClose() && upperRightCloseButton.containsPoint(x, y))
		{
			exitThisMenu();
		}
		else
		{
			if (Game1.activeClickableMenu == null && Game1.currentMinigame == null)
			{
				return;
			}
			if (backButton.containsPoint(x, y))
			{
				ChangePage(-1);
				return;
			}
			if (forwardButton.containsPoint(x, y))
			{
				ChangePage(1);
				return;
			}
			if (previousCharacterButton.containsPoint(x, y))
			{
				ChangeCharacter(-1);
				return;
			}
			if (nextCharacterButton.containsPoint(x, y))
			{
				ChangeCharacter(1);
				return;
			}
			if (downArrow.containsPoint(x, y))
			{
				Scroll(scrollStep);
			}
			if (upArrow.containsPoint(x, y))
			{
				Scroll(-scrollStep);
			}
			if (characterSpriteBox.Contains(x, y))
			{
				PlayHiddenEmote();
			}
		}
	}

	public void PlayHiddenEmote()
	{
		if (Current.HeartLevel >= 4)
		{
			_currentDirection = 2;
			_characterSpriteRandomInt = Game1.random.Next(4);
			CharacterData data = Current.Data;
			Game1.playSound(data?.HiddenProfileEmoteSound ?? "drumkit6");
			_hiddenEmoteTimer = ((data != null && data.HiddenProfileEmoteDuration >= 0) ? ((float)data.HiddenProfileEmoteDuration) : 4000f);
		}
		else
		{
			_currentDirection = 2;
			_directionChangeTimer = 5000f;
			Game1.playSound("Cowboy_Footstep");
		}
	}

	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		hoveredItem = null;
		if (_itemDisplayRect.Contains(x, y))
		{
			foreach (ProfileItem profileItem in _profileItems)
			{
				profileItem.performHover(x, y);
			}
		}
		upArrow.tryHover(x, y);
		downArrow.tryHover(x, y);
		backButton.tryHover(x, y, 0.6f);
		forwardButton.tryHover(x, y, 0.6f);
		nextCharacterButton.tryHover(x, y, 0.6f);
		previousCharacterButton.tryHover(x, y, 0.6f);
	}

	public void ConstrainSelectionToView()
	{
		if (!Game1.options.snappyMenus)
		{
			return;
		}
		ClickableComponent clickableComponent = currentlySnappedComponent;
		if (clickableComponent != null && clickableComponent.region == 502 && !_itemDisplayRect.Contains(currentlySnappedComponent.bounds))
		{
			if (currentlySnappedComponent.bounds.Bottom > _itemDisplayRect.Bottom)
			{
				int scroll = (int)Math.Ceiling(((double)currentlySnappedComponent.bounds.Bottom - (double)_itemDisplayRect.Bottom) / (double)scrollStep) * scrollStep;
				Scroll(scroll);
			}
			else if (currentlySnappedComponent.bounds.Top < _itemDisplayRect.Top)
			{
				int scroll = (int)Math.Floor(((double)currentlySnappedComponent.bounds.Top - (double)_itemDisplayRect.Top) / (double)scrollStep) * scrollStep;
				Scroll(scroll);
			}
		}
		if (scrollPosition <= scrollStep)
		{
			scrollPosition = 0;
			UpdateScroll();
		}
	}

	public override void update(GameTime time)
	{
		base.update(time);
		if (Current.DisplayName != null && _printedName.Length < Current.DisplayName.Length)
		{
			_printedName += Current.DisplayName[_printedName.Length];
		}
		if (_hideTooltipTime > 0)
		{
			_hideTooltipTime -= time.ElapsedGameTime.Milliseconds;
			if (_hideTooltipTime < 0)
			{
				_hideTooltipTime = 0;
			}
		}
		if (_characterEntrancePosition.X != 0f)
		{
			_characterEntrancePosition.X -= (float)Math.Sign(_characterEntrancePosition.X) * 0.25f;
		}
		if (_characterEntrancePosition.Y != 0f)
		{
			_characterEntrancePosition.Y -= (float)Math.Sign(_characterEntrancePosition.Y) * 0.25f;
		}
		if (_animatedSprite == null)
		{
			return;
		}
		if (_hiddenEmoteTimer > 0f)
		{
			_hiddenEmoteTimer -= time.ElapsedGameTime.Milliseconds;
			if (_hiddenEmoteTimer <= 0f)
			{
				_hiddenEmoteTimer = -1f;
				_currentDirection = 2;
				_directionChangeTimer = 2000f;
				if (Current.InternalName == "Leo")
				{
					Current.Character.Sprite.AnimateDown(time);
				}
			}
		}
		else if (_directionChangeTimer > 0f)
		{
			_directionChangeTimer -= time.ElapsedGameTime.Milliseconds;
			if (_directionChangeTimer <= 0f)
			{
				_directionChangeTimer = 2000f;
				_currentDirection = (_currentDirection + 1) % 4;
			}
		}
		if (_characterEntrancePosition != Vector2.Zero)
		{
			if (_characterEntrancePosition.X < 0f)
			{
				_animatedSprite.AnimateRight(time, 2);
			}
			else if (_characterEntrancePosition.X > 0f)
			{
				_animatedSprite.AnimateLeft(time, 2);
			}
			else if (_characterEntrancePosition.Y > 0f)
			{
				_animatedSprite.AnimateUp(time, 2);
			}
			else if (_characterEntrancePosition.Y < 0f)
			{
				_animatedSprite.AnimateDown(time, 2);
			}
			return;
		}
		if (_hiddenEmoteTimer > 0f)
		{
			CharacterData data = Current.Data;
			if (data != null && data.HiddenProfileEmoteStartFrame >= 0)
			{
				int startFrame = ((Current.InternalName == "Emily" && data.HiddenProfileEmoteStartFrame == 16) ? (data.HiddenProfileEmoteStartFrame + _characterSpriteRandomInt * 2) : data.HiddenProfileEmoteStartFrame);
				_animatedSprite.Animate(time, startFrame, data.HiddenProfileEmoteFrameCount, data.HiddenProfileEmoteFrameDuration);
			}
			else
			{
				_animatedSprite.AnimateDown(time, 2);
			}
			return;
		}
		switch (_currentDirection)
		{
		case 0:
			_animatedSprite.AnimateUp(time, 2);
			break;
		case 2:
			_animatedSprite.AnimateDown(time, 2);
			break;
		case 3:
			_animatedSprite.AnimateLeft(time, 2);
			break;
		case 1:
			_animatedSprite.AnimateRight(time, 2);
			break;
		}
	}

	public void SetupLayout()
	{
		int x = xPositionOnScreen + 64 - 12;
		int y = yPositionOnScreen + IClickableMenu.borderWidth;
		Rectangle left_pane_rectangle = new Rectangle(x, y, 400, 720 - IClickableMenu.borderWidth * 2);
		Rectangle content_rectangle = new Rectangle(x, y, 1204, 720 - IClickableMenu.borderWidth * 2);
		content_rectangle.X += left_pane_rectangle.Width;
		content_rectangle.Width -= left_pane_rectangle.Width;
		_characterStatusDisplayBox = new Rectangle(left_pane_rectangle.X, left_pane_rectangle.Y, left_pane_rectangle.Width, left_pane_rectangle.Height);
		left_pane_rectangle.Y += 32;
		left_pane_rectangle.Height -= 32;
		_characterSpriteDrawPosition = new Vector2(left_pane_rectangle.X + (left_pane_rectangle.Width - Game1.nightbg.Width) / 2, left_pane_rectangle.Y);
		characterSpriteBox = new Rectangle(xPositionOnScreen + 64 - 12 + (400 - Game1.nightbg.Width) / 2, yPositionOnScreen + IClickableMenu.borderWidth, Game1.nightbg.Width, Game1.nightbg.Height);
		previousCharacterButton.bounds.X = (int)_characterSpriteDrawPosition.X - 64 - previousCharacterButton.bounds.Width / 2;
		previousCharacterButton.bounds.Y = (int)_characterSpriteDrawPosition.Y + Game1.nightbg.Height / 2 - previousCharacterButton.bounds.Height / 2;
		nextCharacterButton.bounds.X = (int)_characterSpriteDrawPosition.X + Game1.nightbg.Width + 64 - nextCharacterButton.bounds.Width / 2;
		nextCharacterButton.bounds.Y = (int)_characterSpriteDrawPosition.Y + Game1.nightbg.Height / 2 - nextCharacterButton.bounds.Height / 2;
		left_pane_rectangle.Y += Game1.daybg.Height + 32;
		left_pane_rectangle.Height -= Game1.daybg.Height + 32;
		_characterNamePosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
		left_pane_rectangle.Y += 96;
		left_pane_rectangle.Height -= 96;
		_heartDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
		if (Current.Character is NPC npc)
		{
			left_pane_rectangle.Y += 56;
			left_pane_rectangle.Height -= 48;
			_birthdayHeadingDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
			if (npc.birthday_Season.Value != null && Utility.getSeasonNumber(npc.birthday_Season) >= 0)
			{
				left_pane_rectangle.Y += 48;
				left_pane_rectangle.Height -= 48;
				_birthdayDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
				left_pane_rectangle.Y += 64;
				left_pane_rectangle.Height -= 64;
			}
			if (_status != "")
			{
				_statusHeadingDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
				left_pane_rectangle.Y += 48;
				left_pane_rectangle.Height -= 48;
				_statusDisplayPosition = new Vector2(left_pane_rectangle.Center.X, left_pane_rectangle.Top);
				left_pane_rectangle.Y += 64;
				left_pane_rectangle.Height -= 64;
			}
		}
		content_rectangle.Height -= 96;
		content_rectangle.Y -= 8;
		_giftLogHeadingDisplayPosition = new Vector2(content_rectangle.Center.X, content_rectangle.Top);
		content_rectangle.Y += 80;
		content_rectangle.Height -= 70;
		backButton.bounds.X = content_rectangle.Left + 64 - forwardButton.bounds.Width / 2;
		backButton.bounds.Y = content_rectangle.Top;
		forwardButton.bounds.X = content_rectangle.Right - 64 - forwardButton.bounds.Width / 2;
		forwardButton.bounds.Y = content_rectangle.Top;
		content_rectangle.Width -= 250;
		content_rectangle.X += 125;
		_giftLogCategoryDisplayPosition = new Vector2(content_rectangle.Center.X, content_rectangle.Top);
		content_rectangle.Y += 64;
		content_rectangle.Y += 32;
		content_rectangle.Height -= 32;
		_itemDisplayRect = content_rectangle;
		int scroll_inset = 64;
		scrollBarRunner = new Rectangle(content_rectangle.Right + 48, content_rectangle.Top + scroll_inset, scrollBar.bounds.Width, content_rectangle.Height - scroll_inset * 2);
		downArrow.bounds.Y = scrollBarRunner.Bottom + 16;
		downArrow.bounds.X = scrollBarRunner.Center.X - downArrow.bounds.Width / 2;
		upArrow.bounds.Y = scrollBarRunner.Top - 16 - upArrow.bounds.Height;
		upArrow.bounds.X = scrollBarRunner.Center.X - upArrow.bounds.Width / 2;
		float draw_y = 0f;
		if (_profileItems.Count > 0)
		{
			int drawn_index = 0;
			for (int i = 0; i < _profileItems.Count; i++)
			{
				ProfileItem profile_item = _profileItems[i];
				if (profile_item.ShouldDraw())
				{
					draw_y = profile_item.HandleLayout(draw_y, _itemDisplayRect, drawn_index);
					drawn_index++;
				}
			}
		}
		scrollSize = (int)draw_y - _itemDisplayRect.Height;
		if (NeedsScrollBar())
		{
			upArrow.visible = true;
			downArrow.visible = true;
		}
		else
		{
			upArrow.visible = false;
			downArrow.visible = false;
		}
		UpdateScroll();
	}

	public override void leftClickHeld(int x, int y)
	{
		if (GameMenu.forcePreventClose)
		{
			return;
		}
		base.leftClickHeld(x, y);
		if (scrolling)
		{
			int num = scrollPosition;
			scrollPosition = (int)Math.Round((float)(y - scrollBarRunner.Top) / (float)scrollBarRunner.Height * (float)scrollSize / (float)scrollStep) * scrollStep;
			UpdateScroll();
			if (num != scrollPosition)
			{
				Game1.playSound("shiny4");
			}
		}
	}

	public bool NeedsScrollBar()
	{
		return scrollSize > 0;
	}

	public void Scroll(int offset)
	{
		if (NeedsScrollBar())
		{
			int num = scrollPosition;
			scrollPosition += offset;
			UpdateScroll();
			if (num != scrollPosition)
			{
				Game1.playSound("shwip");
			}
		}
	}

	public virtual void UpdateScroll()
	{
		scrollPosition = Utility.Clamp(scrollPosition, 0, scrollSize);
		float draw_y = _itemDisplayRect.Top - scrollPosition;
		_errorMessagePosition = new Vector2(_itemDisplayRect.Center.X, _itemDisplayRect.Center.Y);
		if (_profileItems.Count > 0)
		{
			int drawn_index = 0;
			for (int i = 0; i < _profileItems.Count; i++)
			{
				ProfileItem profile_item = _profileItems[i];
				if (profile_item.ShouldDraw())
				{
					draw_y = profile_item.HandleLayout(draw_y, _itemDisplayRect, drawn_index);
					drawn_index++;
				}
			}
		}
		if (scrollSize > 0)
		{
			scrollBar.bounds.X = scrollBarRunner.Center.X - scrollBar.bounds.Width / 2;
			scrollBar.bounds.Y = (int)Utility.Lerp(scrollBarRunner.Top, scrollBarRunner.Bottom - scrollBar.bounds.Height, (float)scrollPosition / (float)scrollSize);
			if (Game1.options.SnappyMenus)
			{
				snapCursorToCurrentSnappedComponent();
			}
		}
	}

	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
		}
		b.Draw(letterTexture, new Vector2(xPositionOnScreen + width / 2, yPositionOnScreen + height / 2), new Rectangle(0, 0, 320, 180), Color.White, 0f, new Vector2(160f, 90f), 4f, SpriteEffects.None, 0.86f);
		Game1.DrawBox(_characterStatusDisplayBox.X, _characterStatusDisplayBox.Y, _characterStatusDisplayBox.Width, _characterStatusDisplayBox.Height);
		b.Draw((Game1.timeOfDay >= 1900) ? Game1.nightbg : Game1.daybg, _characterSpriteDrawPosition, Color.White);
		Vector2 portraitPosition = new Vector2(_characterSpriteDrawPosition.X + (float)((Game1.daybg.Width - _animatedSprite.SpriteWidth * 4) / 2), _characterSpriteDrawPosition.Y + 32f + (float)((32 - _animatedSprite.SpriteHeight) * 4));
		NPC npc = Current.Character as NPC;
		if (npc != null)
		{
			_animatedSprite.draw(b, portraitPosition, 0.8f);
			bool isCurrentSpouse = Current.IsMarriedToCurrentPlayer();
			int drawn_hearts = Math.Max(10, Utility.GetMaximumHeartsForCharacter(npc));
			float heart_draw_start_x = _heartDisplayPosition.X - (float)(Math.Min(10, drawn_hearts) * 32 / 2);
			float heart_draw_offset_y = ((drawn_hearts > 10) ? (-16f) : 0f);
			for (int hearts = 0; hearts < drawn_hearts; hearts++)
			{
				drawNPCSlotHeart(b, heart_draw_start_x, heart_draw_offset_y, Current, hearts, Current.IsDatingCurrentPlayer(), isCurrentSpouse);
			}
		}
		if (_printedName.Length < Current.DisplayName.Length)
		{
			SpriteText.drawStringWithScrollCenteredAt(b, "", (int)_characterNamePosition.X, (int)_characterNamePosition.Y, _printedName);
		}
		else
		{
			SpriteText.drawStringWithScrollCenteredAt(b, Current.DisplayName, (int)_characterNamePosition.X, (int)_characterNamePosition.Y);
		}
		if (npc != null && npc.birthday_Season.Value != null)
		{
			int season_number = Utility.getSeasonNumber(npc.birthday_Season);
			if (season_number >= 0)
			{
				SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:Profile_Birthday"), (int)_birthdayHeadingDisplayPosition.X, (int)_birthdayHeadingDisplayPosition.Y);
				string birthday = Game1.content.LoadString("Strings\\UI:BirthdayOrder", npc.Birthday_Day, Utility.getSeasonNameFromNumber(season_number));
				b.DrawString(Game1.dialogueFont, birthday, new Vector2((0f - Game1.dialogueFont.MeasureString(birthday).X) / 2f + _birthdayDisplayPosition.X, _birthdayDisplayPosition.Y), Game1.textColor);
			}
			if (_status != "")
			{
				SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:Profile_Status"), (int)_statusHeadingDisplayPosition.X, (int)_statusHeadingDisplayPosition.Y);
				b.DrawString(Game1.dialogueFont, _status, new Vector2((0f - Game1.dialogueFont.MeasureString(_status).X) / 2f + _statusDisplayPosition.X, _statusDisplayPosition.Y), Game1.textColor);
			}
		}
		SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:Profile_GiftLog"), (int)_giftLogHeadingDisplayPosition.X, (int)_giftLogHeadingDisplayPosition.Y);
		SpriteText.drawStringHorizontallyCenteredAt(b, Game1.content.LoadString("Strings\\UI:" + itemCategories[_currentCategory].categoryName, Current.DisplayName), (int)_giftLogCategoryDisplayPosition.X, (int)_giftLogCategoryDisplayPosition.Y);
		bool drew_items = false;
		b.End();
		Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
		b.GraphicsDevice.ScissorRectangle = _itemDisplayRect;
		if (_profileItems.Count > 0)
		{
			for (int i = 0; i < _profileItems.Count; i++)
			{
				ProfileItem profile_item = _profileItems[i];
				if (profile_item.ShouldDraw())
				{
					drew_items = true;
					profile_item.Draw(b);
				}
			}
		}
		b.End();
		b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (NeedsScrollBar())
		{
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f, drawShadow: false);
			scrollBar.draw(b);
		}
		if (!drew_items)
		{
			string error_string = Game1.content.LoadString("Strings\\UI:Profile_GiftLog_NoGiftsGiven");
			b.DrawString(Game1.smallFont, error_string, new Vector2((0f - Game1.smallFont.MeasureString(error_string).X) / 2f + _errorMessagePosition.X, _errorMessagePosition.Y), Game1.textColor);
		}
		foreach (ClickableTextureComponent clickableTextureComponent in _clickableTextureComponents)
		{
			clickableTextureComponent.draw(b);
		}
		base.draw(b);
		drawMouse(b, ignore_transparency: true);
		if (hoveredItem == null)
		{
			return;
		}
		bool draw_tooltip = true;
		if (Game1.options.snappyMenus && Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse && _hideTooltipTime > 0)
		{
			draw_tooltip = false;
		}
		if (!draw_tooltip)
		{
			return;
		}
		string description = hoveredItem.getDescription();
		if (description.Contains("{0}") || hoveredItem.ItemId == "DriedMushrooms")
		{
			string replaced_desc = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + hoveredItem.ItemId + "_CollectionsTabDescription");
			if (replaced_desc == null)
			{
				replaced_desc = description;
			}
			string replaced_name = Game1.content.LoadStringReturnNullIfNotFound("Strings\\Objects:" + hoveredItem.ItemId + "_CollectionsTabName");
			if (replaced_name == null)
			{
				replaced_name = hoveredItem.DisplayName;
			}
			IClickableMenu.drawToolTip(b, replaced_desc, replaced_name, hoveredItem);
		}
		else
		{
			IClickableMenu.drawToolTip(b, description, hoveredItem.DisplayName, hoveredItem);
		}
	}

	/// <summary>Draw the heart sprite for an NPC's entry in the social page.</summary>
	/// <param name="b">The sprite batch being drawn.</param>
	/// <param name="heartDrawStartX">The left X position at which to draw the first heart.</param>
	/// <param name="heartDrawStartY">The top Y position at which to draw hearts.</param>
	/// <param name="entry">The NPC's cached social data.</param>
	/// <param name="hearts">The current heart index being drawn (starting at 0 for the first heart).</param>
	/// <param name="isDating">Whether the player is currently dating this NPC.</param>
	/// <param name="isCurrentSpouse">Whether the player is currently married to this NPC.</param>
	private void drawNPCSlotHeart(SpriteBatch b, float heartDrawStartX, float heartDrawStartY, SocialPage.SocialEntry entry, int hearts, bool isDating, bool isCurrentSpouse)
	{
		bool isLockedHeart = entry.IsDatable && !isDating && !isCurrentSpouse && hearts >= 8;
		int heartX = ((hearts < entry.HeartLevel || isLockedHeart) ? 211 : 218);
		Color heartTint = ((hearts < 10 && isLockedHeart) ? (Color.Black * 0.35f) : Color.White);
		if (hearts < 10)
		{
			b.Draw(Game1.mouseCursors, new Vector2(heartDrawStartX + (float)(hearts * 32), _heartDisplayPosition.Y + heartDrawStartY), new Rectangle(heartX, 428, 7, 6), heartTint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		}
		else
		{
			b.Draw(Game1.mouseCursors, new Vector2(heartDrawStartX + (float)((hearts - 10) * 32), _heartDisplayPosition.Y + heartDrawStartY + 32f), new Rectangle(heartX, 428, 7, 6), heartTint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		}
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		receiveLeftClick(x, y, playSound);
	}

	public void RegisterClickable(ClickableComponent clickable)
	{
		clickableProfileItems.Add(clickable);
	}

	public void UnregisterClickable(ClickableComponent clickable)
	{
		clickableProfileItems.Remove(clickable);
	}
}
