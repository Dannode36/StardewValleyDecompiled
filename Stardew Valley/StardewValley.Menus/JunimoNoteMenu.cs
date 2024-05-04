using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;

namespace StardewValley.Menus;

public class JunimoNoteMenu : IClickableMenu
{
	public const int region_ingredientSlotModifier = 250;

	public const int region_ingredientListModifier = 1000;

	public const int region_bundleModifier = 5000;

	public const int region_areaNextButton = 101;

	public const int region_areaBackButton = 102;

	public const int region_backButton = 103;

	public const int region_purchaseButton = 104;

	public const int region_presentButton = 105;

	public const string noteTextureName = "LooseSprites\\JunimoNote";

	public Texture2D noteTexture;

	public bool specificBundlePage;

	public const int baseWidth = 320;

	public const int baseHeight = 180;

	public InventoryMenu inventory;

	public Item partialDonationItem;

	public List<Item> partialDonationComponents = new List<Item>();

	public BundleIngredientDescription? currentPartialIngredientDescription;

	public int currentPartialIngredientDescriptionIndex = -1;

	public Item heldItem;

	public Item hoveredItem;

	public static bool canClick = true;

	public int whichArea;

	public int gameMenuTabToReturnTo = -1;

	public IClickableMenu menuToReturnTo;

	public bool bundlesChanged;

	public static ScreenSwipe screenSwipe;

	public static string hoverText = "";

	public List<Bundle> bundles = new List<Bundle>();

	public static TemporaryAnimatedSpriteList tempSprites = new TemporaryAnimatedSpriteList();

	public List<ClickableTextureComponent> ingredientSlots = new List<ClickableTextureComponent>();

	public List<ClickableTextureComponent> ingredientList = new List<ClickableTextureComponent>();

	public bool fromGameMenu;

	public bool fromThisMenu;

	public bool scrambledText;

	private bool singleBundleMenu;

	public ClickableTextureComponent backButton;

	public ClickableTextureComponent purchaseButton;

	public ClickableTextureComponent areaNextButton;

	public ClickableTextureComponent areaBackButton;

	public ClickableAnimatedComponent presentButton;

	public Action<int> onIngredientDeposit;

	public Action<JunimoNoteMenu> onBundleComplete;

	public Action<JunimoNoteMenu> onScreenSwipeFinished;

	public Bundle currentPageBundle;

	public JunimoNoteMenu(bool fromGameMenu, int area = 1, bool fromThisMenu = false)
		: base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 360, 1280, 720, showUpperRightCloseButton: true)
	{
		CommunityCenter cc = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
		if (fromGameMenu && !fromThisMenu)
		{
			for (int i = 0; i < cc.areasComplete.Count; i++)
			{
				if (cc.shouldNoteAppearInArea(i) && !cc.areasComplete[i])
				{
					area = i;
					whichArea = area;
					break;
				}
			}
			if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("abandonedJojaMartAccessible") && !Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater"))
			{
				area = 6;
			}
		}
		setUpMenu(area, cc.bundlesDict());
		Game1.player.forceCanMove();
		areaNextButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 128, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			visible = false,
			myID = 101,
			leftNeighborID = 102,
			leftNeighborImmutable = true,
			downNeighborID = -99998
		};
		areaBackButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 64, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			visible = false,
			myID = 102,
			rightNeighborID = 101,
			rightNeighborImmutable = true,
			downNeighborID = -99998
		};
		int area_count = 6;
		for (int i = 0; i < area_count; i++)
		{
			if (i != area && cc.shouldNoteAppearInArea(i))
			{
				areaNextButton.visible = true;
				areaBackButton.visible = true;
				break;
			}
		}
		this.fromGameMenu = fromGameMenu;
		this.fromThisMenu = fromThisMenu;
		foreach (Bundle bundle in bundles)
		{
			bundle.depositsAllowed = false;
		}
		if (Game1.options.SnappyMenus)
		{
			populateClickableComponentList();
			snapToDefaultClickableComponent();
		}
	}

	public JunimoNoteMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete)
		: base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 360, 1280, 720, showUpperRightCloseButton: true)
	{
		setUpMenu(whichArea, bundlesComplete);
		if (Game1.options.SnappyMenus)
		{
			populateClickableComponentList();
			snapToDefaultClickableComponent();
		}
	}

	public JunimoNoteMenu(Bundle b, string noteTexturePath)
		: base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 360, 1280, 720, showUpperRightCloseButton: true)
	{
		singleBundleMenu = true;
		whichArea = -1;
		noteTexture = Game1.temporaryContent.Load<Texture2D>(noteTexturePath);
		tempSprites.Clear();
		inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, playerInventory: true, null, HighlightObjects, 36, 6, 8, 8, drawSlots: false)
		{
			capacity = 36
		};
		for (int i = 0; i < inventory.inventory.Count; i++)
		{
			if (i >= inventory.actualInventory.Count)
			{
				inventory.inventory[i].visible = false;
			}
		}
		foreach (ClickableComponent item in inventory.GetBorder(InventoryMenu.BorderSide.Bottom))
		{
			item.downNeighborID = -99998;
		}
		foreach (ClickableComponent item2 in inventory.GetBorder(InventoryMenu.BorderSide.Right))
		{
			item2.rightNeighborID = -99998;
		}
		inventory.dropItemInvisibleButton.visible = false;
		canClick = true;
		setUpBundleSpecificPage(b);
		if (Game1.options.SnappyMenus)
		{
			populateClickableComponentList();
			snapToDefaultClickableComponent();
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		if (specificBundlePage)
		{
			currentlySnappedComponent = getComponentWithID(0);
		}
		else
		{
			currentlySnappedComponent = getComponentWithID(5000);
		}
		snapCursorToCurrentSnappedComponent();
	}

	protected override bool _ShouldAutoSnapPrioritizeAlignedElements()
	{
		if (specificBundlePage)
		{
			return false;
		}
		return true;
	}

	protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
	{
		if (!Game1.player.hasOrWillReceiveMail("canReadJunimoText") || oldID - 5000 < 0 || oldID - 5000 >= 10 || currentlySnappedComponent == null)
		{
			return;
		}
		int lowestScoreBundle = -1;
		int lowestScore = 999999;
		Point startingPosition = currentlySnappedComponent.bounds.Center;
		for (int i = 0; i < bundles.Count; i++)
		{
			if (bundles[i].myID == oldID)
			{
				continue;
			}
			int score = 999999;
			Point bundlePosition = bundles[i].bounds.Center;
			switch (direction)
			{
			case 3:
				if (bundlePosition.X < startingPosition.X)
				{
					score = startingPosition.X - bundlePosition.X + Math.Abs(startingPosition.Y - bundlePosition.Y) * 3;
				}
				break;
			case 0:
				if (bundlePosition.Y < startingPosition.Y)
				{
					score = startingPosition.Y - bundlePosition.Y + Math.Abs(startingPosition.X - bundlePosition.X) * 3;
				}
				break;
			case 1:
				if (bundlePosition.X > startingPosition.X)
				{
					score = bundlePosition.X - startingPosition.X + Math.Abs(startingPosition.Y - bundlePosition.Y) * 3;
				}
				break;
			case 2:
				if (bundlePosition.Y > startingPosition.Y)
				{
					score = bundlePosition.Y - startingPosition.Y + Math.Abs(startingPosition.X - bundlePosition.X) * 3;
				}
				break;
			}
			if (score < 10000 && score < lowestScore)
			{
				lowestScore = score;
				lowestScoreBundle = i;
			}
		}
		if (lowestScoreBundle != -1)
		{
			currentlySnappedComponent = getComponentWithID(lowestScoreBundle + 5000);
			snapCursorToCurrentSnappedComponent();
			return;
		}
		switch (direction)
		{
		case 2:
			if (presentButton != null)
			{
				currentlySnappedComponent = presentButton;
				snapCursorToCurrentSnappedComponent();
				presentButton.upNeighborID = oldID;
			}
			break;
		case 3:
			if (areaBackButton != null && areaBackButton.visible)
			{
				currentlySnappedComponent = areaBackButton;
				snapCursorToCurrentSnappedComponent();
				areaBackButton.rightNeighborID = oldID;
			}
			break;
		case 1:
			if (areaNextButton != null && areaNextButton.visible)
			{
				currentlySnappedComponent = areaNextButton;
				snapCursorToCurrentSnappedComponent();
				areaNextButton.leftNeighborID = oldID;
			}
			break;
		}
	}

	public void setUpMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete)
	{
		noteTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\JunimoNote");
		if (!Game1.player.hasOrWillReceiveMail("seenJunimoNote"))
		{
			Game1.player.removeQuest("26");
			Game1.player.mailReceived.Add("seenJunimoNote");
		}
		if (!Game1.player.hasOrWillReceiveMail("wizardJunimoNote"))
		{
			Game1.addMailForTomorrow("wizardJunimoNote");
		}
		if (!Game1.player.hasOrWillReceiveMail("hasSeenAbandonedJunimoNote") && whichArea == 6)
		{
			Game1.player.mailReceived.Add("hasSeenAbandonedJunimoNote");
		}
		scrambledText = !Game1.player.hasOrWillReceiveMail("canReadJunimoText");
		tempSprites.Clear();
		this.whichArea = whichArea;
		inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, playerInventory: true, null, HighlightObjects, 36, 6, 8, 8, drawSlots: false)
		{
			capacity = 36
		};
		for (int i = 0; i < inventory.inventory.Count; i++)
		{
			if (i >= inventory.actualInventory.Count)
			{
				inventory.inventory[i].visible = false;
			}
		}
		foreach (ClickableComponent item in inventory.GetBorder(InventoryMenu.BorderSide.Bottom))
		{
			item.downNeighborID = -99998;
		}
		foreach (ClickableComponent item2 in inventory.GetBorder(InventoryMenu.BorderSide.Right))
		{
			item2.rightNeighborID = -99998;
		}
		inventory.dropItemInvisibleButton.visible = false;
		Dictionary<string, string> bundlesInfo = Game1.netWorldState.Value.BundleData;
		string areaName = CommunityCenter.getAreaNameFromNumber(whichArea);
		int bundlesAdded = 0;
		foreach (string k in bundlesInfo.Keys)
		{
			if (k.Contains(areaName))
			{
				int bundleIndex = Convert.ToInt32(k.Split('/')[1]);
				bundles.Add(new Bundle(bundleIndex, bundlesInfo[k], bundlesComplete[bundleIndex], getBundleLocationFromNumber(bundlesAdded), "LooseSprites\\JunimoNote", this)
				{
					myID = bundlesAdded + 5000,
					rightNeighborID = -7777,
					leftNeighborID = -7777,
					upNeighborID = -7777,
					downNeighborID = -7777,
					fullyImmutable = true
				});
				bundlesAdded++;
			}
		}
		backButton = new ClickableTextureComponent("Back", new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth * 2 + 8, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 4, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
		{
			myID = 103
		};
		checkForRewards();
		canClick = true;
		Game1.playSound("shwip");
		bool isOneIncomplete = false;
		foreach (Bundle b in bundles)
		{
			if (!b.complete && !b.Equals(currentPageBundle))
			{
				isOneIncomplete = true;
				break;
			}
		}
		if (!isOneIncomplete)
		{
			CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
			communityCenter.markAreaAsComplete(whichArea);
			exitFunction = restoreAreaOnExit;
			communityCenter.areaCompleteReward(whichArea);
		}
	}

	public virtual bool HighlightObjects(Item item)
	{
		if (currentPageBundle != null)
		{
			if (partialDonationItem != null && currentPartialIngredientDescriptionIndex >= 0)
			{
				return currentPageBundle.IsValidItemForThisIngredientDescription(item, currentPageBundle.ingredients[currentPartialIngredientDescriptionIndex]);
			}
			foreach (BundleIngredientDescription ingredient in currentPageBundle.ingredients)
			{
				if (currentPageBundle.IsValidItemForThisIngredientDescription(item, ingredient))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override bool readyToClose()
	{
		if (!specificBundlePage || singleBundleMenu)
		{
			return isReadyToCloseMenuOrBundle();
		}
		return false;
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (!canClick)
		{
			return;
		}
		base.receiveLeftClick(x, y, playSound);
		if (scrambledText)
		{
			return;
		}
		if (specificBundlePage)
		{
			if (!currentPageBundle.complete && currentPageBundle.completionTimer <= 0)
			{
				heldItem = inventory.leftClick(x, y, heldItem);
			}
			if (backButton != null && backButton.containsPoint(x, y) && heldItem == null)
			{
				closeBundlePage();
			}
			if (partialDonationItem != null)
			{
				if (heldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
				{
					for (int i = 0; i < ingredientSlots.Count; i++)
					{
						if (ingredientSlots[i].item == partialDonationItem)
						{
							HandlePartialDonation(heldItem, ingredientSlots[i]);
						}
					}
				}
				else
				{
					for (int i = 0; i < ingredientSlots.Count; i++)
					{
						if (ingredientSlots[i].containsPoint(x, y) && ingredientSlots[i].item == partialDonationItem)
						{
							if (heldItem != null)
							{
								HandlePartialDonation(heldItem, ingredientSlots[i]);
								return;
							}
							bool return_to_inventory = Game1.oldKBState.IsKeyDown(Keys.LeftShift);
							ReturnPartialDonations(!return_to_inventory);
							return;
						}
					}
				}
			}
			else if (heldItem != null)
			{
				if (Game1.oldKBState.IsKeyDown(Keys.LeftShift))
				{
					for (int i = 0; i < ingredientSlots.Count; i++)
					{
						if (currentPageBundle.canAcceptThisItem(heldItem, ingredientSlots[i]))
						{
							if (ingredientSlots[i].item == null)
							{
								heldItem = currentPageBundle.tryToDepositThisItem(heldItem, ingredientSlots[i], "LooseSprites\\JunimoNote", this);
								checkIfBundleIsComplete();
								return;
							}
						}
						else if (ingredientSlots[i].item == null)
						{
							HandlePartialDonation(heldItem, ingredientSlots[i]);
						}
					}
				}
				for (int i = 0; i < ingredientSlots.Count; i++)
				{
					if (ingredientSlots[i].containsPoint(x, y))
					{
						if (currentPageBundle.canAcceptThisItem(heldItem, ingredientSlots[i]))
						{
							heldItem = currentPageBundle.tryToDepositThisItem(heldItem, ingredientSlots[i], "LooseSprites\\JunimoNote", this);
							checkIfBundleIsComplete();
						}
						else if (ingredientSlots[i].item == null)
						{
							HandlePartialDonation(heldItem, ingredientSlots[i]);
						}
					}
				}
			}
			if (purchaseButton != null && purchaseButton.containsPoint(x, y))
			{
				int moneyRequired = currentPageBundle.ingredients.Last().stack;
				if (Game1.player.Money >= moneyRequired)
				{
					Game1.player.Money -= moneyRequired;
					Game1.playSound("select");
					currentPageBundle.completionAnimation(this);
					if (purchaseButton != null)
					{
						purchaseButton.scale = purchaseButton.baseScale * 0.75f;
					}
					CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
					communityCenter.bundleRewards[currentPageBundle.bundleIndex] = true;
					communityCenter.bundles.FieldDict[currentPageBundle.bundleIndex][0] = true;
					checkForRewards();
					bool isOneIncomplete = false;
					foreach (Bundle b in bundles)
					{
						if (!b.complete && !b.Equals(currentPageBundle))
						{
							isOneIncomplete = true;
							break;
						}
					}
					if (!isOneIncomplete)
					{
						communityCenter.markAreaAsComplete(whichArea);
						exitFunction = restoreAreaOnExit;
						communityCenter.areaCompleteReward(whichArea);
					}
					else
					{
						communityCenter.getJunimoForArea(whichArea)?.bringBundleBackToHut(Bundle.getColorFromColorIndex(currentPageBundle.bundleColor), Game1.RequireLocation("CommunityCenter"));
					}
					Game1.multiplayer.globalChatInfoMessage("Bundle");
				}
				else
				{
					Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
				}
			}
			if (upperRightCloseButton != null && isReadyToCloseMenuOrBundle() && upperRightCloseButton.containsPoint(x, y))
			{
				closeBundlePage();
				return;
			}
		}
		else
		{
			foreach (Bundle b in bundles)
			{
				if (b.canBeClicked() && b.containsPoint(x, y))
				{
					setUpBundleSpecificPage(b);
					Game1.playSound("shwip");
					return;
				}
			}
			if (presentButton != null && presentButton.containsPoint(x, y) && !fromGameMenu && !fromThisMenu)
			{
				openRewardsMenu();
			}
			if (fromGameMenu)
			{
				if (areaNextButton.containsPoint(x, y))
				{
					SwapPage(1);
				}
				else if (areaBackButton.containsPoint(x, y))
				{
					SwapPage(-1);
				}
			}
		}
		if (heldItem != null && !isWithinBounds(x, y) && heldItem.canBeTrashed())
		{
			Game1.playSound("throwDownITem");
			Game1.createItemDebris(heldItem, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
			heldItem = null;
		}
	}

	public virtual void ReturnPartialDonation(Item item, bool play_sound = true)
	{
		List<Item> affected_items = new List<Item>();
		Item remainder = Game1.player.addItemToInventory(item, affected_items);
		foreach (Item affected_item in affected_items)
		{
			inventory.ShakeItem(affected_item);
		}
		if (remainder != null)
		{
			Utility.CollectOrDrop(remainder);
			inventory.ShakeItem(remainder);
		}
		if (play_sound)
		{
			Game1.playSound("coin");
		}
	}

	public virtual void ReturnPartialDonations(bool to_hand = true)
	{
		if (partialDonationComponents.Count > 0)
		{
			bool play_sound = true;
			foreach (Item item in partialDonationComponents)
			{
				if (heldItem == null && to_hand)
				{
					Game1.playSound("dwop");
					heldItem = item;
				}
				else
				{
					ReturnPartialDonation(item, play_sound);
					play_sound = false;
				}
			}
		}
		ResetPartialDonation();
	}

	public virtual void ResetPartialDonation()
	{
		partialDonationComponents.Clear();
		currentPartialIngredientDescription = null;
		currentPartialIngredientDescriptionIndex = -1;
		foreach (ClickableTextureComponent slot in ingredientSlots)
		{
			if (slot.item == partialDonationItem)
			{
				slot.item = null;
			}
		}
		partialDonationItem = null;
	}

	public virtual bool CanBePartiallyOrFullyDonated(Item item)
	{
		if (currentPageBundle == null)
		{
			return false;
		}
		int index = currentPageBundle.GetBundleIngredientDescriptionIndexForItem(item);
		if (index < 0)
		{
			return false;
		}
		BundleIngredientDescription description = currentPageBundle.ingredients[index];
		int count = 0;
		if (currentPageBundle.IsValidItemForThisIngredientDescription(item, description))
		{
			count += item.Stack;
		}
		foreach (Item inventory_item in Game1.player.Items)
		{
			if (currentPageBundle.IsValidItemForThisIngredientDescription(inventory_item, description))
			{
				count += inventory_item.Stack;
			}
		}
		if (index == currentPartialIngredientDescriptionIndex && partialDonationItem != null)
		{
			count += partialDonationItem.Stack;
		}
		return count >= description.stack;
	}

	public virtual void HandlePartialDonation(Item item, ClickableTextureComponent slot)
	{
		if ((currentPageBundle != null && !currentPageBundle.depositsAllowed) || (partialDonationItem != null && slot.item != partialDonationItem) || !CanBePartiallyOrFullyDonated(item))
		{
			return;
		}
		if (!currentPartialIngredientDescription.HasValue)
		{
			currentPartialIngredientDescriptionIndex = currentPageBundle.GetBundleIngredientDescriptionIndexForItem(item);
			if (currentPartialIngredientDescriptionIndex != -1)
			{
				currentPartialIngredientDescription = currentPageBundle.ingredients[currentPartialIngredientDescriptionIndex];
			}
		}
		if (!currentPartialIngredientDescription.HasValue || !currentPageBundle.IsValidItemForThisIngredientDescription(item, currentPartialIngredientDescription.Value))
		{
			return;
		}
		bool play_sound = true;
		int amount_to_donate;
		if (slot.item == null)
		{
			Game1.playSound("sell");
			play_sound = false;
			partialDonationItem = item.getOne();
			amount_to_donate = Math.Min(currentPartialIngredientDescription.Value.stack, item.Stack);
			partialDonationItem.Stack = amount_to_donate;
			item.Stack -= amount_to_donate;
			partialDonationItem.Quality = currentPartialIngredientDescription.Value.quality;
			slot.item = partialDonationItem;
			slot.sourceRect.X = 512;
			slot.sourceRect.Y = 244;
		}
		else
		{
			amount_to_donate = Math.Min(currentPartialIngredientDescription.Value.stack - partialDonationItem.Stack, item.Stack);
			partialDonationItem.Stack += amount_to_donate;
			item.Stack -= amount_to_donate;
		}
		if (amount_to_donate > 0)
		{
			Item donated_item = heldItem.getOne();
			donated_item.Stack = amount_to_donate;
			foreach (Item contributed_item in partialDonationComponents)
			{
				if (contributed_item.canStackWith(heldItem))
				{
					donated_item.Stack = contributed_item.addToStack(donated_item);
				}
			}
			if (donated_item.Stack > 0)
			{
				partialDonationComponents.Add(donated_item);
			}
			partialDonationComponents.Sort((Item a, Item b) => b.Stack.CompareTo(a.Stack));
		}
		if (item.Stack <= 0 && item == heldItem)
		{
			heldItem = null;
		}
		if (partialDonationItem.Stack >= currentPartialIngredientDescription.Value.stack)
		{
			slot.item = null;
			partialDonationItem = currentPageBundle.tryToDepositThisItem(partialDonationItem, slot, "LooseSprites\\JunimoNote", this);
			Item item2 = partialDonationItem;
			if (item2 != null && item2.Stack > 0)
			{
				ReturnPartialDonation(partialDonationItem);
			}
			partialDonationItem = null;
			ResetPartialDonation();
			checkIfBundleIsComplete();
		}
		else if (amount_to_donate > 0 && play_sound)
		{
			Game1.playSound("sell");
		}
	}

	public bool isReadyToCloseMenuOrBundle()
	{
		if (specificBundlePage)
		{
			Bundle bundle = currentPageBundle;
			if (bundle != null && bundle.completionTimer > 0)
			{
				return false;
			}
		}
		if (heldItem != null)
		{
			return false;
		}
		return true;
	}

	public override void receiveGamePadButton(Buttons b)
	{
		base.receiveGamePadButton(b);
		if (fromGameMenu && !specificBundlePage)
		{
			switch (b)
			{
			case Buttons.RightTrigger:
				SwapPage(1);
				break;
			case Buttons.LeftTrigger:
				SwapPage(-1);
				break;
			}
		}
		else
		{
			if (!specificBundlePage)
			{
				return;
			}
			switch (b)
			{
			case Buttons.RightTrigger:
			{
				if (currentlySnappedComponent == null || currentlySnappedComponent.myID >= 50)
				{
					break;
				}
				int id = 250;
				foreach (ClickableTextureComponent c in ingredientSlots)
				{
					if (c.item == null)
					{
						id = c.myID;
						break;
					}
				}
				setCurrentlySnappedComponentTo(id);
				snapCursorToCurrentSnappedComponent();
				break;
			}
			case Buttons.LeftTrigger:
				if (currentlySnappedComponent != null && currentlySnappedComponent.myID >= 250)
				{
					setCurrentlySnappedComponentTo(0);
					snapCursorToCurrentSnappedComponent();
				}
				break;
			}
		}
	}

	public void SwapPage(int direction)
	{
		if ((direction > 0 && !areaNextButton.visible) || (direction < 0 && !areaBackButton.visible))
		{
			return;
		}
		CommunityCenter cc = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
		int area = whichArea;
		int area_count = 6;
		for (int i = 0; i < area_count; i++)
		{
			area += direction;
			if (area < 0)
			{
				area += area_count;
			}
			if (area >= area_count)
			{
				area -= area_count;
			}
			if (cc.shouldNoteAppearInArea(area))
			{
				int selected_id = -1;
				if (currentlySnappedComponent != null && (currentlySnappedComponent.myID >= 5000 || currentlySnappedComponent.myID == 101 || currentlySnappedComponent.myID == 102))
				{
					selected_id = currentlySnappedComponent.myID;
				}
				JunimoNoteMenu new_menu = (JunimoNoteMenu)(Game1.activeClickableMenu = new JunimoNoteMenu(fromGameMenu: true, area, fromThisMenu: true)
				{
					gameMenuTabToReturnTo = gameMenuTabToReturnTo
				});
				if (selected_id >= 0)
				{
					new_menu.currentlySnappedComponent = new_menu.getComponentWithID(currentlySnappedComponent.myID);
					new_menu.snapCursorToCurrentSnappedComponent();
				}
				if (new_menu.getComponentWithID(areaNextButton.leftNeighborID) != null)
				{
					new_menu.areaNextButton.leftNeighborID = areaNextButton.leftNeighborID;
				}
				else
				{
					new_menu.areaNextButton.leftNeighborID = new_menu.areaBackButton.myID;
				}
				new_menu.areaNextButton.rightNeighborID = areaNextButton.rightNeighborID;
				new_menu.areaNextButton.upNeighborID = areaNextButton.upNeighborID;
				new_menu.areaNextButton.downNeighborID = areaNextButton.downNeighborID;
				if (new_menu.getComponentWithID(areaBackButton.rightNeighborID) != null)
				{
					new_menu.areaBackButton.leftNeighborID = areaBackButton.leftNeighborID;
				}
				else
				{
					new_menu.areaBackButton.leftNeighborID = new_menu.areaNextButton.myID;
				}
				new_menu.areaBackButton.rightNeighborID = areaBackButton.rightNeighborID;
				new_menu.areaBackButton.upNeighborID = areaBackButton.upNeighborID;
				new_menu.areaBackButton.downNeighborID = areaBackButton.downNeighborID;
				break;
			}
		}
	}

	public override void receiveKeyPress(Keys key)
	{
		if (gameMenuTabToReturnTo != -1)
		{
			closeSound = "shwip";
		}
		base.receiveKeyPress(key);
		if (key.Equals(Keys.Delete) && heldItem != null && heldItem.canBeTrashed())
		{
			Utility.trashItem(heldItem);
			heldItem = null;
		}
		if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && isReadyToCloseMenuOrBundle())
		{
			if (singleBundleMenu)
			{
				exitThisMenu(gameMenuTabToReturnTo == -1);
			}
			closeBundlePage();
		}
	}

	protected override void cleanupBeforeExit()
	{
		base.cleanupBeforeExit();
		if (gameMenuTabToReturnTo != -1)
		{
			Game1.activeClickableMenu = new GameMenu(gameMenuTabToReturnTo, -1, playOpeningSound: false);
		}
		else if (menuToReturnTo != null)
		{
			Game1.activeClickableMenu = menuToReturnTo;
		}
	}

	private void closeBundlePage()
	{
		if (partialDonationItem != null)
		{
			ReturnPartialDonations(to_hand: false);
		}
		else if (specificBundlePage)
		{
			hoveredItem = null;
			inventory.descriptionText = "";
			if (heldItem == null)
			{
				takeDownBundleSpecificPage();
				Game1.playSound("shwip");
			}
			else
			{
				heldItem = inventory.tryToAddItem(heldItem);
			}
		}
	}

	private void reOpenThisMenu()
	{
		bool num = specificBundlePage;
		JunimoNoteMenu newMenu = ((!fromGameMenu && !fromThisMenu) ? new JunimoNoteMenu(whichArea, Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundlesDict())
		{
			gameMenuTabToReturnTo = gameMenuTabToReturnTo,
			menuToReturnTo = menuToReturnTo
		} : new JunimoNoteMenu(fromGameMenu, whichArea, fromThisMenu)
		{
			gameMenuTabToReturnTo = gameMenuTabToReturnTo,
			menuToReturnTo = menuToReturnTo
		});
		if (num)
		{
			foreach (Bundle bundle in newMenu.bundles)
			{
				if (bundle.bundleIndex == currentPageBundle.bundleIndex)
				{
					newMenu.setUpBundleSpecificPage(bundle);
					break;
				}
			}
		}
		Game1.activeClickableMenu = newMenu;
	}

	private void updateIngredientSlots()
	{
		int slotNumber = 0;
		foreach (BundleIngredientDescription ingredient in currentPageBundle.ingredients)
		{
			if (ingredient.completed && slotNumber < ingredientSlots.Count)
			{
				string id = GetRepresentativeItemId(ingredient);
				if (ingredient.preservesId != null)
				{
					ingredientSlots[slotNumber].item = Utility.CreateFlavoredItem(id, ingredient.preservesId, ingredient.quality, ingredient.stack);
				}
				else
				{
					ingredientSlots[slotNumber].item = ItemRegistry.Create(id, ingredient.stack, ingredient.quality);
				}
				currentPageBundle.ingredientDepositAnimation(ingredientSlots[slotNumber], "LooseSprites\\JunimoNote", skipAnimation: true);
				slotNumber++;
			}
		}
	}

	/// <summary>Get the qualified item ID to draw in the bundle UI for an ingredient.</summary>
	/// <param name="ingredient">The ingredient to represent.</param>
	public static string GetRepresentativeItemId(BundleIngredientDescription ingredient)
	{
		if (ingredient.category.HasValue)
		{
			foreach (ParsedItemData data in ItemRegistry.GetObjectTypeDefinition().GetAllData())
			{
				if (data.Category == ingredient.category)
				{
					return data.QualifiedItemId;
				}
			}
			return "0";
		}
		return ingredient.id;
	}

	public static void GetBundleRewards(int area, List<Item> rewards)
	{
		CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
		Dictionary<string, string> bundlesInfo = Game1.netWorldState.Value.BundleData;
		foreach (string k in bundlesInfo.Keys)
		{
			if (k.Contains(CommunityCenter.getAreaNameFromNumber(area)))
			{
				int bundleIndex = Convert.ToInt32(k.Split('/')[1]);
				if (communityCenter.bundleRewards[bundleIndex])
				{
					Item i = Utility.getItemFromStandardTextDescription(bundlesInfo[k].Split('/')[1], Game1.player);
					i.SpecialVariable = bundleIndex;
					rewards.Add(i);
				}
			}
		}
	}

	private void openRewardsMenu()
	{
		Game1.playSound("smallSelect");
		List<Item> rewards = new List<Item>();
		GetBundleRewards(whichArea, rewards);
		Game1.activeClickableMenu = new ItemGrabMenu(rewards, reverseGrab: false, showReceivingMenu: true, null, null, null, rewardGrabbed, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: false, 0, null, -1, this);
		Game1.activeClickableMenu.exitFunction = ((exitFunction != null) ? exitFunction : new onExit(reOpenThisMenu));
	}

	private void rewardGrabbed(Item item, Farmer who)
	{
		Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundleRewards[item.SpecialVariable] = false;
	}

	private void checkIfBundleIsComplete()
	{
		ReturnPartialDonations();
		if (!specificBundlePage || currentPageBundle == null)
		{
			return;
		}
		int numberOfFilledSlots = 0;
		foreach (ClickableTextureComponent c in ingredientSlots)
		{
			if (c.item != null && c.item != partialDonationItem)
			{
				numberOfFilledSlots++;
			}
		}
		if (numberOfFilledSlots < currentPageBundle.numberOfIngredientSlots)
		{
			return;
		}
		if (heldItem != null)
		{
			Game1.player.addItemToInventory(heldItem);
			heldItem = null;
		}
		if (!singleBundleMenu)
		{
			CommunityCenter communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
			for (int i = 0; i < communityCenter.bundles[currentPageBundle.bundleIndex].Length; i++)
			{
				communityCenter.bundles.FieldDict[currentPageBundle.bundleIndex][i] = true;
			}
			communityCenter.checkForNewJunimoNotes();
			screenSwipe = new ScreenSwipe(0);
			currentPageBundle.completionAnimation(this, playSound: true, 400);
			canClick = false;
			communityCenter.bundleRewards[currentPageBundle.bundleIndex] = true;
			Game1.multiplayer.globalChatInfoMessage("Bundle");
			bool isOneIncomplete = false;
			foreach (Bundle b in bundles)
			{
				if (!b.complete && !b.Equals(currentPageBundle))
				{
					isOneIncomplete = true;
					break;
				}
			}
			if (!isOneIncomplete)
			{
				if (whichArea == 6)
				{
					exitFunction = restoreaAreaOnExit_AbandonedJojaMart;
				}
				else
				{
					communityCenter.markAreaAsComplete(whichArea);
					exitFunction = restoreAreaOnExit;
					communityCenter.areaCompleteReward(whichArea);
				}
			}
			else
			{
				communityCenter.getJunimoForArea(whichArea)?.bringBundleBackToHut(Bundle.getColorFromColorIndex(currentPageBundle.bundleColor), communityCenter);
			}
			checkForRewards();
		}
		else if (onBundleComplete != null)
		{
			onBundleComplete(this);
		}
	}

	private void restoreaAreaOnExit_AbandonedJojaMart()
	{
		Game1.RequireLocation<AbandonedJojaMart>("AbandonedJojaMart").restoreAreaCutscene();
	}

	private void restoreAreaOnExit()
	{
		if (!fromGameMenu)
		{
			Game1.RequireLocation<CommunityCenter>("CommunityCenter").restoreAreaCutscene(whichArea);
		}
	}

	public void checkForRewards()
	{
		Dictionary<string, string> bundlesInfo = Game1.netWorldState.Value.BundleData;
		foreach (string k in bundlesInfo.Keys)
		{
			if (k.Contains(CommunityCenter.getAreaNameFromNumber(whichArea)) && bundlesInfo[k].Split('/')[1].Length > 1)
			{
				int bundleIndex = Convert.ToInt32(k.Split('/')[1]);
				if (Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundleRewards[bundleIndex])
				{
					presentButton = new ClickableAnimatedComponent(new Rectangle(xPositionOnScreen + 592, yPositionOnScreen + 512, 72, 72), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10783"), new TemporaryAnimatedSprite("LooseSprites\\JunimoNote", new Rectangle(548, 262, 18, 20), 70f, 4, 99999, new Vector2(-64f, -64f), flicker: false, flipped: false, 0.5f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true));
					break;
				}
			}
		}
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		if (!canClick)
		{
			return;
		}
		if (specificBundlePage)
		{
			heldItem = inventory.rightClick(x, y, heldItem);
			if (partialDonationItem != null)
			{
				for (int i = 0; i < ingredientSlots.Count; i++)
				{
					if (!ingredientSlots[i].containsPoint(x, y) || ingredientSlots[i].item != partialDonationItem)
					{
						continue;
					}
					if (partialDonationComponents.Count <= 0)
					{
						break;
					}
					Item item = partialDonationComponents[0].getOne();
					bool valid = false;
					if (heldItem == null)
					{
						heldItem = item;
						Game1.playSound("dwop");
						valid = true;
					}
					else if (heldItem.canStackWith(item))
					{
						heldItem.addToStack(item);
						Game1.playSound("dwop");
						valid = true;
					}
					if (!valid)
					{
						break;
					}
					partialDonationComponents[0].Stack--;
					if (partialDonationComponents[0].Stack <= 0)
					{
						partialDonationComponents.RemoveAt(0);
					}
					int count = 0;
					foreach (Item contributed_item in partialDonationComponents)
					{
						count += contributed_item.Stack;
					}
					if (partialDonationItem != null)
					{
						partialDonationItem.Stack = count;
					}
					if (partialDonationComponents.Count == 0)
					{
						ResetPartialDonation();
					}
					break;
				}
			}
		}
		if (!specificBundlePage && isReadyToCloseMenuOrBundle())
		{
			exitThisMenu(gameMenuTabToReturnTo == -1);
		}
	}

	public override void update(GameTime time)
	{
		if (specificBundlePage && currentPageBundle != null && currentPageBundle.completionTimer <= 0 && isReadyToCloseMenuOrBundle() && currentPageBundle.complete)
		{
			takeDownBundleSpecificPage();
		}
		foreach (Bundle bundle in bundles)
		{
			bundle.update(time);
		}
		for (int i = tempSprites.Count - 1; i >= 0; i--)
		{
			if (tempSprites[i].update(time))
			{
				tempSprites.RemoveAt(i);
			}
		}
		presentButton?.update(time);
		if (screenSwipe != null)
		{
			canClick = false;
			if (screenSwipe.update(time))
			{
				screenSwipe = null;
				canClick = true;
				onScreenSwipeFinished?.Invoke(this);
			}
		}
		if (bundlesChanged && fromGameMenu)
		{
			reOpenThisMenu();
		}
	}

	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		if (scrambledText)
		{
			return;
		}
		hoverText = "";
		if (specificBundlePage)
		{
			backButton?.tryHover(x, y);
			if (!currentPageBundle.complete && currentPageBundle.completionTimer <= 0)
			{
				hoveredItem = inventory.hover(x, y, heldItem);
			}
			else
			{
				hoveredItem = null;
			}
			foreach (ClickableTextureComponent c in ingredientList)
			{
				if (c.bounds.Contains(x, y))
				{
					hoverText = c.hoverText;
					break;
				}
			}
			if (heldItem != null)
			{
				foreach (ClickableTextureComponent c in ingredientSlots)
				{
					if (c.bounds.Contains(x, y) && CanBePartiallyOrFullyDonated(heldItem) && (partialDonationItem == null || c.item == partialDonationItem))
					{
						c.sourceRect.X = 530;
						c.sourceRect.Y = 262;
					}
					else
					{
						c.sourceRect.X = 512;
						c.sourceRect.Y = 244;
					}
				}
			}
			purchaseButton?.tryHover(x, y);
			return;
		}
		if (presentButton != null)
		{
			hoverText = presentButton.tryHover(x, y);
		}
		foreach (Bundle bundle in bundles)
		{
			bundle.tryHoverAction(x, y);
		}
		if (fromGameMenu)
		{
			areaNextButton.tryHover(x, y);
			areaBackButton.tryHover(x, y);
		}
	}

	public override void draw(SpriteBatch b)
	{
		if (Game1.options.showMenuBackground)
		{
			base.drawBackground(b);
		}
		else if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
		}
		if (!specificBundlePage)
		{
			b.Draw(noteTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(0, 0, 320, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			SpriteText.drawStringHorizontallyCenteredAt(b, scrambledText ? CommunityCenter.getAreaEnglishDisplayNameFromNumber(whichArea) : CommunityCenter.getAreaDisplayNameFromNumber(whichArea), xPositionOnScreen + width / 2 + 16, yPositionOnScreen + 12, 999999, -1, 99999, 0.88f, 0.88f, scrambledText);
			if (scrambledText)
			{
				SpriteText.drawString(b, LocalizedContentManager.CurrentLanguageLatin ? Game1.content.LoadString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786") : Game1.content.LoadBaseString("Strings\\StringsFromCSFiles:JunimoNoteMenu.cs.10786"), xPositionOnScreen + 96, yPositionOnScreen + 96, 999999, width - 192, 99999, 0.88f, 0.88f, junimoText: true);
				base.draw(b);
				if (!Game1.options.SnappyMenus && canClick)
				{
					drawMouse(b);
				}
				return;
			}
			foreach (Bundle bundle in bundles)
			{
				bundle.draw(b);
			}
			presentButton?.draw(b);
			foreach (TemporaryAnimatedSprite tempSprite in tempSprites)
			{
				tempSprite.draw(b, localPosition: true);
			}
			if (fromGameMenu)
			{
				if (areaNextButton.visible)
				{
					areaNextButton.draw(b);
				}
				if (areaBackButton.visible)
				{
					areaBackButton.draw(b);
				}
			}
		}
		else
		{
			b.Draw(noteTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), new Rectangle(320, 0, 320, 180), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
			if (currentPageBundle != null)
			{
				int bundle_index = currentPageBundle.bundleIndex;
				Texture2D bundle_texture = noteTexture;
				int y_offset = 180;
				if (currentPageBundle.bundleTextureIndexOverride >= 0)
				{
					bundle_index = currentPageBundle.bundleTextureIndexOverride;
				}
				if (currentPageBundle.bundleTextureOverride != null)
				{
					bundle_texture = currentPageBundle.bundleTextureOverride;
					y_offset = 0;
				}
				b.Draw(bundle_texture, new Vector2(xPositionOnScreen + 872, yPositionOnScreen + 88), new Rectangle(bundle_index * 16 * 2 % bundle_texture.Width, y_offset + 32 * (bundle_index * 16 * 2 / bundle_texture.Width), 32, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.15f);
				if (currentPageBundle.label != null)
				{
					float textX = Game1.dialogueFont.MeasureString((!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label)).X;
					b.Draw(noteTexture, new Vector2(xPositionOnScreen + 936 - (int)textX / 2 - 16, yPositionOnScreen + 228), new Rectangle(517, 266, 4, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
					b.Draw(noteTexture, new Rectangle(xPositionOnScreen + 936 - (int)textX / 2, yPositionOnScreen + 228, (int)textX, 68), new Rectangle(520, 266, 1, 17), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.1f);
					b.Draw(noteTexture, new Vector2(xPositionOnScreen + 936 + (int)textX / 2, yPositionOnScreen + 228), new Rectangle(524, 266, 4, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
					b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)(xPositionOnScreen + 936) - textX / 2f, yPositionOnScreen + 236) + new Vector2(2f, 2f), Game1.textShadowColor);
					b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)(xPositionOnScreen + 936) - textX / 2f, yPositionOnScreen + 236) + new Vector2(0f, 2f), Game1.textShadowColor);
					b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)(xPositionOnScreen + 936) - textX / 2f, yPositionOnScreen + 236) + new Vector2(2f, 0f), Game1.textShadowColor);
					b.DrawString(Game1.dialogueFont, (!Game1.player.hasOrWillReceiveMail("canReadJunimoText")) ? "???" : Game1.content.LoadString("Strings\\UI:JunimoNote_BundleName", currentPageBundle.label), new Vector2((float)(xPositionOnScreen + 936) - textX / 2f, yPositionOnScreen + 236), Game1.textColor * 0.9f);
				}
			}
			if (backButton != null)
			{
				backButton.draw(b);
			}
			if (purchaseButton != null)
			{
				purchaseButton.draw(b);
				Game1.dayTimeMoneyBox.drawMoneyBox(b);
			}
			float completed_slot_alpha = 1f;
			if (partialDonationItem != null)
			{
				completed_slot_alpha = 0.25f;
			}
			foreach (TemporaryAnimatedSprite tempSprite2 in tempSprites)
			{
				tempSprite2.draw(b, localPosition: true, 0, 0, completed_slot_alpha);
			}
			foreach (ClickableTextureComponent c in ingredientSlots)
			{
				float alpha_mult = 1f;
				if (partialDonationItem != null && c.item != partialDonationItem)
				{
					alpha_mult = 0.25f;
				}
				if (c.item == null || (partialDonationItem != null && c.item == partialDonationItem))
				{
					c.draw(b, (fromGameMenu ? (Color.LightGray * 0.5f) : Color.White) * alpha_mult, 0.89f);
				}
				c.drawItem(b, 4, 4, alpha_mult);
			}
			for (int i = 0; i < ingredientList.Count; i++)
			{
				float alpha_mult = 1f;
				if (currentPartialIngredientDescriptionIndex >= 0 && currentPartialIngredientDescriptionIndex != i)
				{
					alpha_mult = 0.25f;
				}
				ClickableTextureComponent c = ingredientList[i];
				bool completed = false;
				if (i < currentPageBundle?.ingredients?.Count && currentPageBundle.ingredients[i].completed)
				{
					completed = true;
				}
				if (!completed)
				{
					b.Draw(Game1.shadowTexture, new Vector2(c.bounds.Center.X - Game1.shadowTexture.Bounds.Width * 4 / 2 - 4, c.bounds.Center.Y + 4), Game1.shadowTexture.Bounds, Color.White * alpha_mult, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
				}
				if (c.item != null && c.visible)
				{
					c.item.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale / 4f, 1f, 0.9f, StackDrawType.Draw, Color.White * (completed ? 0.25f : alpha_mult), drawShadow: false);
				}
			}
			inventory.draw(b);
		}
		if (getRewardNameForArea(whichArea) != "")
		{
			SpriteText.drawStringWithScrollCenteredAt(b, getRewardNameForArea(whichArea), xPositionOnScreen + width / 2, Math.Min(yPositionOnScreen + height + 20, Game1.uiViewport.Height - 64 - 8));
		}
		base.draw(b);
		Game1.mouseCursorTransparency = 1f;
		if (canClick)
		{
			drawMouse(b);
		}
		heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
		if (inventory.descriptionText.Length > 0)
		{
			if (hoveredItem != null)
			{
				IClickableMenu.drawToolTip(b, hoveredItem.getDescription(), hoveredItem.DisplayName, hoveredItem);
			}
		}
		else
		{
			IClickableMenu.drawHoverText(b, (!singleBundleMenu && !Game1.player.hasOrWillReceiveMail("canReadJunimoText") && hoverText.Length > 0) ? "???" : hoverText, Game1.dialogueFont);
		}
		screenSwipe?.draw(b);
	}

	public string getRewardNameForArea(int whichArea)
	{
		return whichArea switch
		{
			-1 => "", 
			3 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardBoiler"), 
			5 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardBulletin"), 
			1 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardCrafts"), 
			0 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardPantry"), 
			4 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardVault"), 
			2 => Game1.content.LoadString("Strings\\UI:JunimoNote_RewardFishTank"), 
			_ => "???", 
		};
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		tempSprites.Clear();
		xPositionOnScreen = Game1.uiViewport.Width / 2 - 640;
		yPositionOnScreen = Game1.uiViewport.Height / 2 - 360;
		backButton = new ClickableTextureComponent("Back", new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth * 2 + 8, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 4, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f);
		if (fromGameMenu)
		{
			areaNextButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 128, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				visible = false
			};
			areaBackButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 64, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				visible = false
			};
		}
		inventory = new InventoryMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, playerInventory: true, null, HighlightObjects, Game1.player.maxItems, 6, 8, 8, drawSlots: false);
		for (int i = 0; i < inventory.inventory.Count; i++)
		{
			if (i >= inventory.actualInventory.Count)
			{
				inventory.inventory[i].visible = false;
			}
		}
		for (int i = 0; i < bundles.Count; i++)
		{
			Point p = getBundleLocationFromNumber(i);
			bundles[i].bounds.X = p.X;
			bundles[i].bounds.Y = p.Y;
			bundles[i].sprite.position = new Vector2(p.X, p.Y);
		}
		if (!specificBundlePage)
		{
			return;
		}
		int numberOfIngredientSlots = currentPageBundle.numberOfIngredientSlots;
		List<Rectangle> ingredientSlotRectangles = new List<Rectangle>();
		addRectangleRowsToList(ingredientSlotRectangles, numberOfIngredientSlots, 932, 540);
		ingredientSlots.Clear();
		for (int i = 0; i < ingredientSlotRectangles.Count; i++)
		{
			ingredientSlots.Add(new ClickableTextureComponent(ingredientSlotRectangles[i], noteTexture, new Rectangle(512, 244, 18, 18), 4f));
		}
		List<Rectangle> ingredientListRectangles = new List<Rectangle>();
		ingredientList.Clear();
		addRectangleRowsToList(ingredientListRectangles, currentPageBundle.ingredients.Count, 932, 364);
		for (int i = 0; i < ingredientListRectangles.Count; i++)
		{
			BundleIngredientDescription ingredient = currentPageBundle.ingredients[i];
			ItemMetadata metadata = ItemRegistry.GetMetadata(ingredient.id);
			if (metadata?.TypeIdentifier == "(O)")
			{
				ParsedItemData parsedOrErrorData = metadata.GetParsedOrErrorData();
				Texture2D texture = parsedOrErrorData.GetTexture();
				Rectangle sourceRect = parsedOrErrorData.GetSourceRect();
				Item item = ((ingredient.preservesId != null) ? Utility.CreateFlavoredItem(ingredient.id, ingredient.preservesId, ingredient.quality, ingredient.stack) : ItemRegistry.Create(ingredient.id, ingredient.stack, ingredient.quality));
				ingredientList.Add(new ClickableTextureComponent("", ingredientListRectangles[i], "", item.DisplayName, texture, sourceRect, 4f)
				{
					myID = i + 1000,
					item = item,
					upNeighborID = -99998,
					rightNeighborID = -99998,
					leftNeighborID = -99998,
					downNeighborID = -99998
				});
			}
		}
		updateIngredientSlots();
	}

	private void setUpBundleSpecificPage(Bundle b)
	{
		tempSprites.Clear();
		currentPageBundle = b;
		specificBundlePage = true;
		if (whichArea == 4)
		{
			if (!fromGameMenu)
			{
				purchaseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 800, yPositionOnScreen + 504, 260, 72), noteTexture, new Rectangle(517, 286, 65, 20), 4f)
				{
					myID = 797,
					leftNeighborID = 103
				};
				if (Game1.options.SnappyMenus)
				{
					currentlySnappedComponent = purchaseButton;
					snapCursorToCurrentSnappedComponent();
				}
			}
			return;
		}
		int numberOfIngredientSlots = b.numberOfIngredientSlots;
		List<Rectangle> ingredientSlotRectangles = new List<Rectangle>();
		addRectangleRowsToList(ingredientSlotRectangles, numberOfIngredientSlots, 932, 540);
		for (int i = 0; i < ingredientSlotRectangles.Count; i++)
		{
			ingredientSlots.Add(new ClickableTextureComponent(ingredientSlotRectangles[i], noteTexture, new Rectangle(512, 244, 18, 18), 4f)
			{
				myID = i + 250,
				upNeighborID = -99998,
				rightNeighborID = -99998,
				leftNeighborID = -99998,
				downNeighborID = -99998
			});
		}
		List<Rectangle> ingredientListRectangles = new List<Rectangle>();
		addRectangleRowsToList(ingredientListRectangles, b.ingredients.Count, 932, 364);
		for (int i = 0; i < ingredientListRectangles.Count; i++)
		{
			BundleIngredientDescription ingredient = b.ingredients[i];
			string id = GetRepresentativeItemId(ingredient);
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(id);
			if (itemData.HasTypeObject())
			{
				string displayName = ingredient.category switch
				{
					-2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.569"), 
					-75 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.570"), 
					-4 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.571"), 
					-5 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.572"), 
					-6 => Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.573"), 
					_ => itemData.DisplayName, 
				};
				Item item;
				if (ingredient.preservesId != null)
				{
					item = Utility.CreateFlavoredItem(ingredient.id, ingredient.preservesId, ingredient.quality, ingredient.stack);
					displayName = item.DisplayName;
				}
				else
				{
					item = ItemRegistry.Create(id, ingredient.stack, ingredient.quality);
				}
				Texture2D texture = itemData.GetTexture();
				Rectangle sourceRect = itemData.GetSourceRect();
				ingredientList.Add(new ClickableTextureComponent("ingredient_list_slot", ingredientListRectangles[i], "", displayName, texture, sourceRect, 4f)
				{
					myID = i + 1000,
					item = item,
					upNeighborID = -99998,
					rightNeighborID = -99998,
					leftNeighborID = -99998,
					downNeighborID = -99998
				});
			}
		}
		updateIngredientSlots();
		if (!Game1.options.SnappyMenus)
		{
			return;
		}
		populateClickableComponentList();
		if (inventory?.inventory != null)
		{
			for (int i = 0; i < inventory.inventory.Count; i++)
			{
				if (inventory.inventory[i] != null)
				{
					if (inventory.inventory[i].downNeighborID == 101)
					{
						inventory.inventory[i].downNeighborID = -1;
					}
					if (inventory.inventory[i].leftNeighborID == -1)
					{
						inventory.inventory[i].leftNeighborID = 103;
					}
					if (inventory.inventory[i].upNeighborID >= 1000)
					{
						inventory.inventory[i].upNeighborID = 103;
					}
				}
			}
		}
		currentlySnappedComponent = getComponentWithID(0);
		snapCursorToCurrentSnappedComponent();
	}

	public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
	{
		if (currentPartialIngredientDescriptionIndex >= 0)
		{
			if (ingredientSlots.Contains(b) && b.item != partialDonationItem)
			{
				return false;
			}
			if (ingredientList.Contains(b) && ingredientList.IndexOf(b as ClickableTextureComponent) != currentPartialIngredientDescriptionIndex)
			{
				return false;
			}
		}
		return (a.myID >= 5000 || a.myID == 101 || a.myID == 102) == (b.myID >= 5000 || b.myID == 101 || b.myID == 102);
	}

	private void addRectangleRowsToList(List<Rectangle> toAddTo, int numberOfItems, int centerX, int centerY)
	{
		switch (numberOfItems)
		{
		case 1:
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 1, 72, 72, 12));
			break;
		case 2:
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 2, 72, 72, 12));
			break;
		case 3:
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 3, 72, 72, 12));
			break;
		case 4:
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY, 4, 72, 72, 12));
			break;
		case 5:
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 2, 72, 72, 12));
			break;
		case 6:
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 3, 72, 72, 12));
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
			break;
		case 7:
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 3, 72, 72, 12));
			break;
		case 8:
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 4, 72, 72, 12));
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
			break;
		case 9:
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 4, 72, 72, 12));
			break;
		case 10:
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 5, 72, 72, 12));
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
			break;
		case 11:
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 5, 72, 72, 12));
			break;
		case 12:
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY - 36, 6, 72, 72, 12));
			toAddTo.AddRange(createRowOfBoxesCenteredAt(xPositionOnScreen + centerX, yPositionOnScreen + centerY + 40, 6, 72, 72, 12));
			break;
		}
	}

	private List<Rectangle> createRowOfBoxesCenteredAt(int xStart, int yStart, int numBoxes, int boxWidth, int boxHeight, int horizontalGap)
	{
		List<Rectangle> rectangles = new List<Rectangle>();
		int actualXStart = xStart - numBoxes * (boxWidth + horizontalGap) / 2;
		int actualYStart = yStart - boxHeight / 2;
		for (int i = 0; i < numBoxes; i++)
		{
			rectangles.Add(new Rectangle(actualXStart + i * (boxWidth + horizontalGap), actualYStart, boxWidth, boxHeight));
		}
		return rectangles;
	}

	public void takeDownBundleSpecificPage()
	{
		if (!isReadyToCloseMenuOrBundle())
		{
			return;
		}
		ReturnPartialDonations(to_hand: false);
		hoveredItem = null;
		if (!specificBundlePage)
		{
			return;
		}
		specificBundlePage = false;
		ingredientSlots.Clear();
		ingredientList.Clear();
		tempSprites.Clear();
		purchaseButton = null;
		if (Game1.options.SnappyMenus)
		{
			if (currentPageBundle != null)
			{
				currentlySnappedComponent = currentPageBundle;
				snapCursorToCurrentSnappedComponent();
			}
			else
			{
				snapToDefaultClickableComponent();
			}
		}
	}

	private Point getBundleLocationFromNumber(int whichBundle)
	{
		Point location = new Point(xPositionOnScreen, yPositionOnScreen);
		switch (whichBundle)
		{
		case 0:
			location.X += 592;
			location.Y += 136;
			break;
		case 1:
			location.X += 392;
			location.Y += 384;
			break;
		case 2:
			location.X += 784;
			location.Y += 388;
			break;
		case 5:
			location.X += 588;
			location.Y += 276;
			break;
		case 6:
			location.X += 588;
			location.Y += 380;
			break;
		case 3:
			location.X += 304;
			location.Y += 252;
			break;
		case 4:
			location.X += 892;
			location.Y += 252;
			break;
		case 7:
			location.X += 440;
			location.Y += 164;
			break;
		case 8:
			location.X += 776;
			location.Y += 164;
			break;
		}
		return location;
	}
}
