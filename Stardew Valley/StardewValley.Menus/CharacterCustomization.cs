using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Pants;
using StardewValley.GameData.Pets;
using StardewValley.GameData.Shirts;
using StardewValley.Minigames;
using StardewValley.Objects;

namespace StardewValley.Menus;

public class CharacterCustomization : IClickableMenu
{
	public enum Source
	{
		NewGame,
		NewFarmhand,
		Wizard,
		HostNewFarm,
		Dresser,
		ClothesDye,
		DyePots
	}

	public const int region_okbutton = 505;

	public const int region_skipIntroButton = 506;

	public const int region_randomButton = 507;

	public const int region_male = 508;

	public const int region_female = 509;

	public const int region_dog = 510;

	public const int region_cat = 511;

	public const int region_shirtLeft = 512;

	public const int region_shirtRight = 513;

	public const int region_hairLeft = 514;

	public const int region_hairRight = 515;

	public const int region_accLeft = 516;

	public const int region_accRight = 517;

	public const int region_skinLeft = 518;

	public const int region_skinRight = 519;

	public const int region_directionLeft = 520;

	public const int region_directionRight = 521;

	public const int region_cabinsLeft = 621;

	public const int region_cabinsRight = 622;

	public const int region_cabinsClose = 623;

	public const int region_cabinsSeparate = 624;

	public const int region_coopHelp = 625;

	public const int region_coopHelpOK = 626;

	public const int region_difficultyLeft = 627;

	public const int region_difficultyRight = 628;

	public const int region_petLeft = 627;

	public const int region_petRight = 628;

	public const int region_pantsLeft = 629;

	public const int region_pantsRight = 630;

	public const int region_walletsLeft = 631;

	public const int region_walletsRight = 632;

	public const int region_coopHelpRight = 633;

	public const int region_coopHelpLeft = 634;

	public const int region_coopHelpButtons = 635;

	public const int region_advancedOptions = 636;

	public const int region_colorPicker1 = 522;

	public const int region_colorPicker2 = 523;

	public const int region_colorPicker3 = 524;

	public const int region_colorPicker4 = 525;

	public const int region_colorPicker5 = 526;

	public const int region_colorPicker6 = 527;

	public const int region_colorPicker7 = 528;

	public const int region_colorPicker8 = 529;

	public const int region_colorPicker9 = 530;

	public const int region_farmSelection1 = 531;

	public const int region_farmSelection2 = 532;

	public const int region_farmSelection3 = 533;

	public const int region_farmSelection4 = 534;

	public const int region_farmSelection5 = 535;

	public const int region_farmSelection6 = 545;

	public const int region_farmSelection7 = 546;

	public const int region_farmSelection8 = 547;

	public const int region_farmSelection9 = 548;

	public const int region_farmSelection10 = 549;

	public const int region_farmSelection11 = 550;

	public const int region_farmSelection12 = 551;

	public const int region_farmSelectionLeft = 647;

	public const int region_farmSelectionRight = 648;

	public const int region_nameBox = 536;

	public const int region_farmNameBox = 537;

	public const int region_favThingBox = 538;

	public const int colorPickerTimerDelay = 100;

	public const int widthOfMultiplayerArea = 256;

	private int colorPickerTimer;

	public ColorPicker pantsColorPicker;

	public ColorPicker hairColorPicker;

	public ColorPicker eyeColorPicker;

	public List<ClickableComponent> labels = new List<ClickableComponent>();

	public List<ClickableComponent> leftSelectionButtons = new List<ClickableComponent>();

	public List<ClickableComponent> rightSelectionButtons = new List<ClickableComponent>();

	public List<ClickableComponent> genderButtons = new List<ClickableComponent>();

	public List<ClickableTextureComponent> farmTypeButtons = new List<ClickableTextureComponent>();

	public ClickableTextureComponent farmTypeNextPageButton;

	public ClickableTextureComponent farmTypePreviousPageButton;

	private List<string> farmTypeButtonNames = new List<string>();

	private List<string> farmTypeHoverText = new List<string>();

	private List<KeyValuePair<Texture2D, Rectangle>> farmTypeIcons = new List<KeyValuePair<Texture2D, Rectangle>>();

	protected int _currentFarmPage;

	protected int _farmPages;

	public List<ClickableComponent> colorPickerCCs = new List<ClickableComponent>();

	public List<ClickableTextureComponent> cabinLayoutButtons = new List<ClickableTextureComponent>();

	public ClickableTextureComponent okButton;

	public ClickableTextureComponent skipIntroButton;

	public ClickableTextureComponent randomButton;

	public ClickableTextureComponent coopHelpButton;

	public ClickableTextureComponent coopHelpOkButton;

	public ClickableTextureComponent coopHelpRightButton;

	public ClickableTextureComponent coopHelpLeftButton;

	public ClickableTextureComponent advancedOptionsButton;

	private TextBox nameBox;

	private TextBox farmnameBox;

	private TextBox favThingBox;

	private bool skipIntro;

	public bool isModifyingExistingPet;

	public bool showingCoopHelp;

	public int coopHelpScreen;

	public Source source;

	private Vector2 helpStringSize;

	private string hoverText;

	private string hoverTitle;

	private string coopHelpString;

	private string noneString;

	private string normalDiffString;

	private string toughDiffString;

	private string hardDiffString;

	private string superDiffString;

	private string sharedWalletString;

	private string separateWalletString;

	public ClickableComponent nameBoxCC;

	public ClickableComponent farmnameBoxCC;

	public ClickableComponent favThingBoxCC;

	public ClickableComponent backButton;

	private ClickableComponent nameLabel;

	private ClickableComponent farmLabel;

	private ClickableComponent favoriteLabel;

	private ClickableComponent shirtLabel;

	private ClickableComponent skinLabel;

	private ClickableComponent hairLabel;

	private ClickableComponent accLabel;

	private ClickableComponent pantsStyleLabel;

	private ClickableComponent startingCabinsLabel;

	private ClickableComponent cabinLayoutLabel;

	private ClickableComponent separateWalletLabel;

	private ClickableComponent difficultyModifierLabel;

	private ColorPicker _sliderOpTarget;

	private Action _sliderAction;

	private readonly Action _recolorEyesAction;

	private readonly Action _recolorPantsAction;

	private readonly Action _recolorHairAction;

	protected Clothing _itemToDye;

	protected bool _shouldShowBackButton = true;

	protected bool _isDyeMenu;

	protected Farmer _displayFarmer;

	public Rectangle portraitBox;

	public Rectangle? petPortraitBox;

	public string oldName = "";

	private float advancedCCHighlightTimer;

	protected List<KeyValuePair<string, string>> _petTypesAndBreeds;

	private ColorPicker lastHeldColorPicker;

	private int timesRandom;

	public CharacterCustomization(Clothing item)
		: this(Source.ClothesDye)
	{
		_itemToDye = item;
		ResetComponents();
		if (source == Source.NewGame || source == Source.HostNewFarm)
		{
			Game1.spawnMonstersAtNight = false;
		}
		_recolorPantsAction = delegate
		{
			DyeItem(pantsColorPicker.getSelectedColor());
		};
		switch (_itemToDye.clothesType.Value)
		{
		case Clothing.ClothesType.SHIRT:
			_displayFarmer.Equip(_itemToDye, _displayFarmer.shirtItem);
			break;
		case Clothing.ClothesType.PANTS:
			_displayFarmer.Equip(_itemToDye, _displayFarmer.pantsItem);
			break;
		}
		_displayFarmer.UpdateClothing();
	}

	public void DyeItem(Color color)
	{
		if (_itemToDye != null)
		{
			_itemToDye.Dye(color, 1f);
			_displayFarmer.FarmerRenderer.MarkSpriteDirty();
		}
	}

	public CharacterCustomization(Source source)
		: base(Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (648 + IClickableMenu.borderWidth * 2) / 2 - 64, 632 + IClickableMenu.borderWidth * 2, 648 + IClickableMenu.borderWidth * 2 + 64)
	{
		if (source == Source.NewGame || source == Source.HostNewFarm)
		{
			Game1.player.difficultyModifier = 1f;
			Game1.player.team.useSeparateWallets.Value = false;
			Game1.startingCabins = ((source == Source.HostNewFarm) ? 1 : 0);
		}
		LoadFarmTypeData();
		oldName = Game1.player.Name;
		int items_to_dye = 0;
		if (source == Source.ClothesDye || source == Source.DyePots)
		{
			_isDyeMenu = true;
			switch (source)
			{
			case Source.ClothesDye:
				items_to_dye = 1;
				break;
			case Source.DyePots:
				if (Game1.player.CanDyePants())
				{
					items_to_dye++;
				}
				if (Game1.player.CanDyeShirt())
				{
					items_to_dye++;
				}
				break;
			}
			height = 308 + IClickableMenu.borderWidth * 2 + 64 + 72 * items_to_dye - 4;
			xPositionOnScreen = Game1.uiViewport.Width / 2 - width / 2;
			yPositionOnScreen = Game1.uiViewport.Height / 2 - height / 2 - 64;
		}
		this.source = source;
		ResetComponents();
		_recolorEyesAction = delegate
		{
			Game1.player.changeEyeColor(eyeColorPicker.getSelectedColor());
		};
		_recolorPantsAction = delegate
		{
			Game1.player.changePantsColor(pantsColorPicker.getSelectedColor());
		};
		_recolorHairAction = delegate
		{
			Game1.player.changeHairColor(hairColorPicker.getSelectedColor());
		};
		if (source == Source.DyePots)
		{
			_recolorHairAction = delegate
			{
				if (Game1.player.CanDyeShirt())
				{
					Game1.player.shirtItem.Value.clothesColor.Value = hairColorPicker.getSelectedColor();
					Game1.player.FarmerRenderer.MarkSpriteDirty();
					_displayFarmer.FarmerRenderer.MarkSpriteDirty();
				}
			};
			_recolorPantsAction = delegate
			{
				if (Game1.player.CanDyePants())
				{
					Game1.player.pantsItem.Value.clothesColor.Value = pantsColorPicker.getSelectedColor();
					Game1.player.FarmerRenderer.MarkSpriteDirty();
					_displayFarmer.FarmerRenderer.MarkSpriteDirty();
				}
			};
			favThingBoxCC.visible = false;
			nameBoxCC.visible = false;
			farmnameBoxCC.visible = false;
			favoriteLabel.visible = false;
			nameLabel.visible = false;
			farmLabel.visible = false;
		}
		_displayFarmer = GetOrCreateDisplayFarmer();
	}

	public Farmer GetOrCreateDisplayFarmer()
	{
		if (_displayFarmer == null)
		{
			if (source == Source.ClothesDye || source == Source.DyePots)
			{
				_displayFarmer = Game1.player.CreateFakeEventFarmer();
			}
			else
			{
				_displayFarmer = Game1.player;
			}
			if (source == Source.NewFarmhand)
			{
				if (_displayFarmer.pants.Value == null)
				{
					_displayFarmer.pants.Value = _displayFarmer.GetPantsId();
				}
				if (_displayFarmer.shirt.Value == null)
				{
					_displayFarmer.shirt.Value = _displayFarmer.GetShirtId();
				}
			}
			_displayFarmer.faceDirection(2);
			_displayFarmer.FarmerSprite.StopAnimation();
		}
		return _displayFarmer;
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		if (_isDyeMenu)
		{
			xPositionOnScreen = Game1.uiViewport.Width / 2 - width / 2;
			yPositionOnScreen = Game1.uiViewport.Height / 2 - height / 2 - 64;
		}
		else
		{
			xPositionOnScreen = Game1.uiViewport.Width / 2 - (632 + IClickableMenu.borderWidth * 2) / 2;
			yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 64;
		}
		ResetComponents();
	}

	public void showAdvancedCharacterCreationHighlight()
	{
		advancedCCHighlightTimer = 4000f;
	}

	private void ResetComponents()
	{
		colorPickerCCs.Clear();
		if (source == Source.ClothesDye && _itemToDye == null)
		{
			return;
		}
		bool creatingNewSave = source == Source.NewGame || source == Source.HostNewFarm;
		bool allow_clothing_changes = source != Source.Wizard && source != Source.ClothesDye && source != Source.DyePots;
		bool allow_accessory_changes = source != Source.ClothesDye && source != Source.DyePots;
		labels.Clear();
		genderButtons.Clear();
		cabinLayoutButtons.Clear();
		leftSelectionButtons.Clear();
		rightSelectionButtons.Clear();
		farmTypeButtons.Clear();
		if (creatingNewSave)
		{
			advancedOptionsButton = new ClickableTextureComponent("Advanced", new Rectangle(xPositionOnScreen - 80, yPositionOnScreen + height - 80 - 16, 80, 80), null, null, Game1.mouseCursors2, new Rectangle(154, 154, 20, 20), 4f)
			{
				myID = 636,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
		}
		else
		{
			advancedOptionsButton = null;
		}
		okButton = new ClickableTextureComponent("OK", new Rectangle(xPositionOnScreen + width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, yPositionOnScreen + height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			myID = 505,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		};
		backButton = new ClickableComponent(new Rectangle(Game1.uiViewport.Width + -198 - 48, Game1.uiViewport.Height - 81 - 24, 198, 81), "")
		{
			myID = 81114,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		};
		nameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
		{
			X = xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256,
			Y = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16,
			Text = Game1.player.Name
		};
		nameBoxCC = new ClickableComponent(new Rectangle(xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16, 192, 48), "")
		{
			myID = 536,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		};
		int textBoxLabelsXOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt) ? (-4) : 0);
		labels.Add(nameLabel = new ClickableComponent(new Rectangle(xPositionOnScreen + textBoxLabelsXOffset + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 8, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Name")));
		farmnameBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
		{
			X = xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256,
			Y = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64,
			Text = Game1.MasterPlayer.farmName
		};
		farmnameBoxCC = new ClickableComponent(new Rectangle(xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64, 192, 48), "")
		{
			myID = 537,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		};
		int farmLabelXOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? (-16) : 0);
		labels.Add(farmLabel = new ClickableComponent(new Rectangle(xPositionOnScreen + textBoxLabelsXOffset * 3 + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4 + farmLabelXOffset, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 64, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Farm")));
		int favThingBoxXoffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 48 : 0);
		favThingBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
		{
			X = xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256 + favThingBoxXoffset,
			Y = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 128,
			Text = Game1.player.favoriteThing
		};
		favThingBoxCC = new ClickableComponent(new Rectangle(xPositionOnScreen + 64 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 256, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 128, 192, 48), "")
		{
			myID = 538,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		};
		labels.Add(favoriteLabel = new ClickableComponent(new Rectangle(xPositionOnScreen + textBoxLabelsXOffset + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 4, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16 + 128, 1, 1), Game1.content.LoadString("Strings\\UI:Character_FavoriteThing")));
		randomButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 48, yPositionOnScreen + 64 + 56, 40, 40), Game1.mouseCursors, new Rectangle(381, 361, 10, 10), 4f)
		{
			myID = 507,
			upNeighborID = -99998,
			leftNeighborImmutable = true,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		};
		if (source == Source.DyePots || source == Source.ClothesDye)
		{
			randomButton.visible = false;
		}
		portraitBox = new Rectangle(xPositionOnScreen + 64 + 42 - 2, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16, 128, 192);
		if (_isDyeMenu)
		{
			portraitBox.X = xPositionOnScreen + (width - portraitBox.Width) / 2;
			randomButton.bounds.X = portraitBox.X - 56;
		}
		int yOffset = 128;
		leftSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(portraitBox.X - 32, portraitBox.Y + 144, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
		{
			myID = 520,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			leftNeighborImmutable = true,
			rightNeighborID = -99998,
			downNeighborID = -99998
		});
		rightSelectionButtons.Add(new ClickableTextureComponent("Direction", new Rectangle(portraitBox.Right - 32, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
		{
			myID = 521,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = -99998
		});
		int leftSelectionXOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt) ? (-20) : 0);
		isModifyingExistingPet = false;
		if (creatingNewSave)
		{
			petPortraitBox = new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 448 - 16 + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? 60 : 0), yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192 - 16, 64, 64);
			labels.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8 + textBoxLabelsXOffset, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 8 + 192, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Animal")));
		}
		if (creatingNewSave || source == Source.NewFarmhand || source == Source.Wizard)
		{
			genderButtons.Add(new ClickableTextureComponent("Male", new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 32 + 8, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192, 64, 64), null, "Male", Game1.mouseCursors, new Rectangle(128, 192, 16, 16), 4f)
			{
				myID = 508,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			genderButtons.Add(new ClickableTextureComponent("Female", new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 32 + 64 + 24, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192, 64, 64), null, "Female", Game1.mouseCursors, new Rectangle(144, 192, 16, 16), 4f)
			{
				myID = 509,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			if (source == Source.Wizard)
			{
				List<ClickableComponent> list = genderButtons;
				if (list != null && list.Count > 0)
				{
					int start_x = xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 320 + 16;
					int start_y = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 64 + 48;
					for (int i = 0; i < genderButtons.Count; i++)
					{
						genderButtons[i].bounds.X = start_x + 80 * i;
						genderButtons[i].bounds.Y = start_y;
					}
				}
			}
			yOffset = 256;
			if (source == Source.Wizard)
			{
				yOffset = 192;
			}
			leftSelectionXOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.es || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.pt || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr) ? (-20) : 0);
			leftSelectionButtons.Add(new ClickableTextureComponent("Skin", new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 16 + leftSelectionXOffset, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 518,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			labels.Add(skinLabel = new ClickableComponent(new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 16 + 64 + 8 + leftSelectionXOffset / 2, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Skin")));
			rightSelectionButtons.Add(new ClickableTextureComponent("Skin", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 128, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 519,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
		}
		if (creatingNewSave)
		{
			RefreshFarmTypeButtons();
		}
		if (source == Source.HostNewFarm)
		{
			labels.Add(startingCabinsLabel = new ClickableComponent(new Rectangle(xPositionOnScreen - 21 - 128, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 84, 1, 1), Game1.content.LoadString("Strings\\UI:Character_StartingCabins")));
			leftSelectionButtons.Add(new ClickableTextureComponent("Cabins", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth / 2 + 8, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 108, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 621,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			rightSelectionButtons.Add(new ClickableTextureComponent("Cabins", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 + 8, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 108, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 622,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			labels.Add(cabinLayoutLabel = new ClickableComponent(new Rectangle(xPositionOnScreen - 128 - (int)(Game1.smallFont.MeasureString(Game1.content.LoadString("Strings\\UI:Character_CabinLayout")).X / 2f), yPositionOnScreen + IClickableMenu.borderWidth * 2 + 120 + 64, 1, 1), Game1.content.LoadString("Strings\\UI:Character_CabinLayout")));
			cabinLayoutButtons.Add(new ClickableTextureComponent("Close", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 160 + 64, 64, 64), null, Game1.content.LoadString("Strings\\UI:Character_Close"), Game1.mouseCursors, new Rectangle(208, 192, 16, 16), 4f)
			{
				myID = 623,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			cabinLayoutButtons.Add(new ClickableTextureComponent("Separate", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 - 8, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 160 + 64, 64, 64), null, Game1.content.LoadString("Strings\\UI:Character_Separate"), Game1.mouseCursors, new Rectangle(224, 192, 16, 16), 4f)
			{
				myID = 624,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			labels.Add(difficultyModifierLabel = new ClickableComponent(new Rectangle(xPositionOnScreen - 21 - 128, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 256 + 56, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Difficulty")));
			leftSelectionButtons.Add(new ClickableTextureComponent("Difficulty", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth / 2 - 4, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 256 + 80, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 627,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			rightSelectionButtons.Add(new ClickableTextureComponent("Difficulty", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 + 12, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 256 + 80, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 628,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			int walletY = yPositionOnScreen + IClickableMenu.borderWidth * 2 + 320 + 100;
			labels.Add(separateWalletLabel = new ClickableComponent(new Rectangle(xPositionOnScreen - 21 - 128, walletY - 24, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Wallets")));
			leftSelectionButtons.Add(new ClickableTextureComponent("Wallets", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth / 2 - 4, walletY, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 631,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			rightSelectionButtons.Add(new ClickableTextureComponent("Wallets", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 + 12, walletY, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 632,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			coopHelpButton = new ClickableTextureComponent("CoopHelp", new Rectangle(xPositionOnScreen - 256 + IClickableMenu.borderWidth + 128 - 8, yPositionOnScreen + IClickableMenu.borderWidth * 2 + 448 + 40, 64, 64), null, Game1.content.LoadString("Strings\\UI:Character_CoopHelp"), Game1.mouseCursors, new Rectangle(240, 192, 16, 16), 4f)
			{
				myID = 625,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			coopHelpOkButton = new ClickableTextureComponent("CoopHelpOK", new Rectangle(xPositionOnScreen - 256 - 12, yPositionOnScreen + height - 64, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = 626,
				region = 635,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			noneString = Game1.content.LoadString("Strings\\UI:Character_none");
			normalDiffString = Game1.content.LoadString("Strings\\UI:Character_Normal");
			toughDiffString = Game1.content.LoadString("Strings\\UI:Character_Tough");
			hardDiffString = Game1.content.LoadString("Strings\\UI:Character_Hard");
			superDiffString = Game1.content.LoadString("Strings\\UI:Character_Super");
			separateWalletString = Game1.content.LoadString("Strings\\UI:Character_SeparateWallet");
			sharedWalletString = Game1.content.LoadString("Strings\\UI:Character_SharedWallet");
			coopHelpRightButton = new ClickableTextureComponent("CoopHelpRight", new Rectangle(xPositionOnScreen + width, yPositionOnScreen + height, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 633,
				region = 635,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
			coopHelpLeftButton = new ClickableTextureComponent("CoopHelpLeft", new Rectangle(xPositionOnScreen, yPositionOnScreen + height, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 634,
				region = 635,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
		}
		Point top = new Point(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
		int label_position = xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8;
		if (_isDyeMenu)
		{
			label_position = xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth;
		}
		if (creatingNewSave || source == Source.NewFarmhand || source == Source.Wizard)
		{
			labels.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 192 + 8, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_EyeColor")));
			eyeColorPicker = new ColorPicker("Eyes", top.X, top.Y);
			eyeColorPicker.setColor(Game1.player.newEyeColor.Value);
			colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
			{
				myID = 522,
				downNeighborID = -99998,
				upNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
			{
				myID = 523,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
			{
				myID = 524,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			yOffset += 68;
			leftSelectionButtons.Add(new ClickableTextureComponent("Hair", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder + leftSelectionXOffset, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 514,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			labels.Add(hairLabel = new ClickableComponent(new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + leftSelectionXOffset / 2, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Hair")));
			rightSelectionButtons.Add(new ClickableTextureComponent("Hair", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 515,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
		}
		top = new Point(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
		if (creatingNewSave || source == Source.NewFarmhand || source == Source.Wizard)
		{
			labels.Add(new ClickableComponent(new Rectangle(label_position, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_HairColor")));
			hairColorPicker = new ColorPicker("Hair", top.X, top.Y);
			hairColorPicker.setColor(Game1.player.hairstyleColor.Value);
			colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
			{
				myID = 525,
				downNeighborID = -99998,
				upNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
			{
				myID = 526,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
			colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
			{
				myID = 527,
				upNeighborID = -99998,
				downNeighborID = -99998,
				leftNeighborImmutable = true,
				rightNeighborImmutable = true
			});
		}
		if (source == Source.DyePots)
		{
			yOffset += 68;
			if (Game1.player.CanDyeShirt())
			{
				top = new Point(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
				top.X = xPositionOnScreen + width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 160;
				labels.Add(new ClickableComponent(new Rectangle(label_position, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_ShirtColor")));
				hairColorPicker = new ColorPicker("Hair", top.X, top.Y);
				hairColorPicker.setColor(Game1.player.GetShirtColor());
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
				{
					myID = 525,
					downNeighborID = -99998,
					upNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
				{
					myID = 526,
					upNeighborID = -99998,
					downNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
				{
					myID = 527,
					upNeighborID = -99998,
					downNeighborID = -99998,
					leftNeighborImmutable = true,
					rightNeighborImmutable = true
				});
				yOffset += 64;
			}
			if (Game1.player.CanDyePants())
			{
				top = new Point(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
				top.X = xPositionOnScreen + width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 160;
				int pantsColorLabelYOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr) ? (-16) : 0);
				labels.Add(new ClickableComponent(new Rectangle(label_position, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16 + pantsColorLabelYOffset, 1, 1), Game1.content.LoadString("Strings\\UI:Character_PantsColor")));
				pantsColorPicker = new ColorPicker("Pants", top.X, top.Y);
				pantsColorPicker.setColor(Game1.player.GetPantsColor());
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
				{
					myID = 528,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
				{
					myID = 529,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
				colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
				{
					myID = 530,
					downNeighborID = -99998,
					upNeighborID = -99998,
					rightNeighborImmutable = true,
					leftNeighborImmutable = true
				});
			}
		}
		else if (allow_clothing_changes)
		{
			yOffset += 68;
			int shirtArrowsExtraWidth = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr) ? 8 : 0);
			leftSelectionButtons.Add(new ClickableTextureComponent("Shirt", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + leftSelectionXOffset - shirtArrowsExtraWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 512,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			labels.Add(shirtLabel = new ClickableComponent(new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + leftSelectionXOffset / 2, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Shirt")));
			rightSelectionButtons.Add(new ClickableTextureComponent("Shirt", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth + shirtArrowsExtraWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 513,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			int pantsColorLabelYOffset = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr) ? (-16) : 0);
			labels.Add(new ClickableComponent(new Rectangle(label_position, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16 + pantsColorLabelYOffset, 1, 1), Game1.content.LoadString("Strings\\UI:Character_PantsColor")));
			top = new Point(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
			pantsColorPicker = new ColorPicker("Pants", top.X, top.Y);
			pantsColorPicker.setColor(Game1.player.GetPantsColor());
			colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
			{
				myID = 528,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborImmutable = true,
				leftNeighborImmutable = true
			});
			colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
			{
				myID = 529,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborImmutable = true,
				leftNeighborImmutable = true
			});
			colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
			{
				myID = 530,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborImmutable = true,
				leftNeighborImmutable = true
			});
		}
		else if (source == Source.ClothesDye)
		{
			yOffset += 60;
			top = new Point(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 + 48 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset);
			top.X = xPositionOnScreen + width - IClickableMenu.spaceToClearSideBorder - IClickableMenu.borderWidth - 160;
			labels.Add(new ClickableComponent(new Rectangle(label_position, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_DyeColor")));
			pantsColorPicker = new ColorPicker("Pants", top.X, top.Y);
			pantsColorPicker.setColor(_itemToDye.clothesColor.Value);
			colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y, 128, 20), "")
			{
				myID = 528,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborImmutable = true,
				leftNeighborImmutable = true
			});
			colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 20, 128, 20), "")
			{
				myID = 529,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborImmutable = true,
				leftNeighborImmutable = true
			});
			colorPickerCCs.Add(new ClickableComponent(new Rectangle(top.X, top.Y + 40, 128, 20), "")
			{
				myID = 530,
				downNeighborID = -99998,
				upNeighborID = -99998,
				rightNeighborImmutable = true,
				leftNeighborImmutable = true
			});
		}
		skipIntroButton = new ClickableTextureComponent("Skip Intro", new Rectangle(xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + 320 - 48 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 80, 36, 36), null, Game1.content.LoadString("Strings\\UI:Character_SkipIntro"), Game1.mouseCursors, new Rectangle(227, 425, 9, 9), 4f)
		{
			myID = 506,
			upNeighborID = 530,
			leftNeighborID = 517,
			rightNeighborID = 505
		};
		skipIntroButton.sourceRect.X = (skipIntro ? 236 : 227);
		if (allow_clothing_changes)
		{
			yOffset += 68;
			leftSelectionButtons.Add(new ClickableTextureComponent("Pants Style", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + leftSelectionXOffset, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 629,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			labels.Add(pantsStyleLabel = new ClickableComponent(new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + leftSelectionXOffset / 2, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Pants")));
			rightSelectionButtons.Add(new ClickableTextureComponent("Pants Style", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 517,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
		}
		yOffset += 68;
		if (allow_accessory_changes)
		{
			int accessoryArrowsExtraWidth = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr) ? 32 : 0);
			leftSelectionButtons.Add(new ClickableTextureComponent("Acc", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + leftSelectionXOffset - accessoryArrowsExtraWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 516,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			labels.Add(accLabel = new ClickableComponent(new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth + 64 + 8 + leftSelectionXOffset / 2, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset + 16, 1, 1), Game1.content.LoadString("Strings\\UI:Character_Accessory")));
			rightSelectionButtons.Add(new ClickableTextureComponent("Acc", new Rectangle(xPositionOnScreen + 16 + IClickableMenu.spaceToClearSideBorder + 128 + IClickableMenu.borderWidth + accessoryArrowsExtraWidth, yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + yOffset, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 517,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
		}
		if (Game1.gameMode == 3)
		{
			_ = Game1.locations;
		}
		if (petPortraitBox.HasValue)
		{
			leftSelectionButtons.Add(new ClickableTextureComponent("Pet", new Rectangle(petPortraitBox.Value.Left - 64, petPortraitBox.Value.Top, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 511,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			rightSelectionButtons.Add(new ClickableTextureComponent("Pet", new Rectangle(petPortraitBox.Value.Left + 64, petPortraitBox.Value.Top, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 510,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			List<ClickableComponent> list2 = colorPickerCCs;
			if (list2 != null && list2.Count > 0)
			{
				colorPickerCCs[0].upNeighborID = 511;
				colorPickerCCs[0].upNeighborImmutable = true;
			}
		}
		_shouldShowBackButton = true;
		if (source == Source.Dresser || source == Source.Wizard || source == Source.ClothesDye)
		{
			_shouldShowBackButton = false;
		}
		if (source == Source.Dresser || source == Source.Wizard || _isDyeMenu)
		{
			nameBoxCC.visible = false;
			farmnameBoxCC.visible = false;
			favThingBoxCC.visible = false;
			farmLabel.visible = false;
			nameLabel.visible = false;
			favoriteLabel.visible = false;
		}
		if (source == Source.Wizard)
		{
			nameLabel.visible = true;
			nameBoxCC.visible = true;
			favThingBoxCC.visible = true;
			favoriteLabel.visible = true;
			favThingBoxCC.bounds.Y = farmnameBoxCC.bounds.Y;
			favoriteLabel.bounds.Y = farmLabel.bounds.Y;
			favThingBox.Y = farmnameBox.Y;
		}
		skipIntroButton.visible = creatingNewSave;
		if (Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			populateClickableComponentList();
			snapToDefaultClickableComponent();
		}
	}

	public virtual void LoadFarmTypeData()
	{
		List<ModFarmType> farm_types = DataLoader.AdditionalFarms(Game1.content);
		farmTypeButtonNames.Add("Standard");
		farmTypeButtonNames.Add("Riverland");
		farmTypeButtonNames.Add("Forest");
		farmTypeButtonNames.Add("Hills");
		farmTypeButtonNames.Add("Wilderness");
		farmTypeButtonNames.Add("Four Corners");
		farmTypeButtonNames.Add("Beach");
		farmTypeHoverText.Add(GetFarmTypeTooltip("Strings\\UI:Character_FarmStandard"));
		farmTypeHoverText.Add(GetFarmTypeTooltip("Strings\\UI:Character_FarmFishing"));
		farmTypeHoverText.Add(GetFarmTypeTooltip("Strings\\UI:Character_FarmForaging"));
		farmTypeHoverText.Add(GetFarmTypeTooltip("Strings\\UI:Character_FarmMining"));
		farmTypeHoverText.Add(GetFarmTypeTooltip("Strings\\UI:Character_FarmCombat"));
		farmTypeHoverText.Add(GetFarmTypeTooltip("Strings\\UI:Character_FarmFourCorners"));
		farmTypeHoverText.Add(GetFarmTypeTooltip("Strings\\UI:Character_FarmBeach"));
		farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(0, 324, 22, 20)));
		farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(22, 324, 22, 20)));
		farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(44, 324, 22, 20)));
		farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(66, 324, 22, 20)));
		farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(88, 324, 22, 20)));
		farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(0, 345, 22, 20)));
		farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(22, 345, 22, 20)));
		if (farm_types != null)
		{
			foreach (ModFarmType farm_type in farm_types)
			{
				farmTypeButtonNames.Add("ModFarm_" + farm_type.Id);
				farmTypeHoverText.Add(GetFarmTypeTooltip(farm_type.TooltipStringPath));
				if (farm_type.IconTexture != null)
				{
					Texture2D texture = Game1.content.Load<Texture2D>(farm_type.IconTexture);
					farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(texture, new Rectangle(0, 0, 22, 20)));
				}
				else
				{
					farmTypeIcons.Add(new KeyValuePair<Texture2D, Rectangle>(Game1.mouseCursors, new Rectangle(1, 324, 22, 20)));
				}
			}
		}
		_farmPages = 1;
		if (farm_types != null)
		{
			_farmPages = (int)Math.Floor((float)(farmTypeButtonNames.Count - 1) / 12f) + 1;
		}
	}

	public virtual void RefreshFarmTypeButtons()
	{
		farmTypeButtons.Clear();
		Point baseFarmButton = new Point(xPositionOnScreen + width + 4 + 8, yPositionOnScreen + IClickableMenu.borderWidth);
		int index = _currentFarmPage * 12;
		if (index < farmTypeButtonNames.Count)
		{
			farmTypeButtons.Add(new ClickableTextureComponent(farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 88, 88, 80), null, farmTypeHoverText[index], farmTypeIcons[index].Key, farmTypeIcons[index].Value, 4f)
			{
				myID = 531,
				downNeighborID = -99998,
				leftNeighborID = 537
			});
			index++;
		}
		if (index < farmTypeButtonNames.Count)
		{
			farmTypeButtons.Add(new ClickableTextureComponent(farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 176, 88, 80), null, farmTypeHoverText[index], farmTypeIcons[index].Key, farmTypeIcons[index].Value, 4f)
			{
				myID = 532,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < farmTypeButtonNames.Count)
		{
			farmTypeButtons.Add(new ClickableTextureComponent(farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 264, 88, 80), null, farmTypeHoverText[index], farmTypeIcons[index].Key, farmTypeIcons[index].Value, 4f)
			{
				myID = 533,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < farmTypeButtonNames.Count)
		{
			farmTypeButtons.Add(new ClickableTextureComponent(farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 352, 88, 80), null, farmTypeHoverText[index], farmTypeIcons[index].Key, farmTypeIcons[index].Value, 4f)
			{
				myID = 534,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < farmTypeButtonNames.Count)
		{
			farmTypeButtons.Add(new ClickableTextureComponent(farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 440, 88, 80), null, farmTypeHoverText[index], farmTypeIcons[index].Key, farmTypeIcons[index].Value, 4f)
			{
				myID = 535,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < farmTypeButtonNames.Count)
		{
			farmTypeButtons.Add(new ClickableTextureComponent(farmTypeButtonNames[index], new Rectangle(baseFarmButton.X, baseFarmButton.Y + 528, 88, 80), null, farmTypeHoverText[index], farmTypeIcons[index].Key, farmTypeIcons[index].Value, 4f)
			{
				myID = 545,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < farmTypeButtonNames.Count)
		{
			farmTypeButtons.Add(new ClickableTextureComponent(farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 88, 88, 80), null, farmTypeHoverText[index], farmTypeIcons[index].Key, farmTypeIcons[index].Value, 4f)
			{
				myID = 546,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < farmTypeButtonNames.Count)
		{
			farmTypeButtons.Add(new ClickableTextureComponent(farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 176, 88, 80), null, farmTypeHoverText[index], farmTypeIcons[index].Key, farmTypeIcons[index].Value, 4f)
			{
				myID = 547,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < farmTypeButtonNames.Count)
		{
			farmTypeButtons.Add(new ClickableTextureComponent(farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 264, 88, 80), null, farmTypeHoverText[index], farmTypeIcons[index].Key, farmTypeIcons[index].Value, 4f)
			{
				myID = 548,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < farmTypeButtonNames.Count)
		{
			farmTypeButtons.Add(new ClickableTextureComponent(farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 352, 88, 80), null, farmTypeHoverText[index], farmTypeIcons[index].Key, farmTypeIcons[index].Value, 4f)
			{
				myID = 549,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < farmTypeButtonNames.Count)
		{
			farmTypeButtons.Add(new ClickableTextureComponent(farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 440, 88, 80), null, farmTypeHoverText[index], farmTypeIcons[index].Key, farmTypeIcons[index].Value, 4f)
			{
				myID = 550,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		if (index < farmTypeButtonNames.Count)
		{
			farmTypeButtons.Add(new ClickableTextureComponent(farmTypeButtonNames[index], new Rectangle(baseFarmButton.X + 96, baseFarmButton.Y + 528, 88, 80), null, farmTypeHoverText[index], farmTypeIcons[index].Key, farmTypeIcons[index].Value, 4f)
			{
				myID = 551,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			});
			index++;
		}
		farmTypePreviousPageButton = null;
		farmTypeNextPageButton = null;
		if (_currentFarmPage > 0)
		{
			farmTypePreviousPageButton = new ClickableTextureComponent("", new Rectangle(baseFarmButton.X - 64 + 16, baseFarmButton.Y + 352 + 12, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
			{
				myID = 647,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
		}
		if (_currentFarmPage < _farmPages - 1)
		{
			farmTypeNextPageButton = new ClickableTextureComponent("", new Rectangle(baseFarmButton.X + 172, baseFarmButton.Y + 352 + 12, 64, 64), null, "", Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
			{
				myID = 647,
				upNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				downNeighborID = -99998
			};
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		if (showingCoopHelp)
		{
			currentlySnappedComponent = getComponentWithID(626);
		}
		else
		{
			currentlySnappedComponent = getComponentWithID(521);
		}
		snapCursorToCurrentSnappedComponent();
	}

	public override void gamePadButtonHeld(Buttons b)
	{
		base.gamePadButtonHeld(b);
		if (currentlySnappedComponent == null)
		{
			return;
		}
		switch (b)
		{
		case Buttons.DPadRight:
		case Buttons.LeftThumbstickRight:
			switch (currentlySnappedComponent.myID)
			{
			case 522:
				eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
				eyeColorPicker.changeHue(1);
				eyeColorPicker.Dirty = true;
				_sliderOpTarget = eyeColorPicker;
				_sliderAction = _recolorEyesAction;
				break;
			case 523:
				eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
				eyeColorPicker.changeSaturation(1);
				eyeColorPicker.Dirty = true;
				_sliderOpTarget = eyeColorPicker;
				_sliderAction = _recolorEyesAction;
				break;
			case 524:
				eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
				eyeColorPicker.changeValue(1);
				eyeColorPicker.Dirty = true;
				_sliderOpTarget = eyeColorPicker;
				_sliderAction = _recolorEyesAction;
				break;
			case 525:
				hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
				hairColorPicker.changeHue(1);
				hairColorPicker.Dirty = true;
				_sliderOpTarget = hairColorPicker;
				_sliderAction = _recolorHairAction;
				break;
			case 526:
				hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
				hairColorPicker.changeSaturation(1);
				hairColorPicker.Dirty = true;
				_sliderOpTarget = hairColorPicker;
				_sliderAction = _recolorHairAction;
				break;
			case 527:
				hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
				hairColorPicker.changeValue(1);
				hairColorPicker.Dirty = true;
				_sliderOpTarget = hairColorPicker;
				_sliderAction = _recolorHairAction;
				break;
			case 528:
				pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
				pantsColorPicker.changeHue(1);
				pantsColorPicker.Dirty = true;
				_sliderOpTarget = pantsColorPicker;
				_sliderAction = _recolorPantsAction;
				break;
			case 529:
				pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
				pantsColorPicker.changeSaturation(1);
				pantsColorPicker.Dirty = true;
				_sliderOpTarget = pantsColorPicker;
				_sliderAction = _recolorPantsAction;
				break;
			case 530:
				pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
				pantsColorPicker.changeValue(1);
				pantsColorPicker.Dirty = true;
				_sliderOpTarget = pantsColorPicker;
				_sliderAction = _recolorPantsAction;
				break;
			}
			break;
		case Buttons.DPadLeft:
		case Buttons.LeftThumbstickLeft:
			switch (currentlySnappedComponent.myID)
			{
			case 522:
				eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
				eyeColorPicker.changeHue(-1);
				eyeColorPicker.Dirty = true;
				_sliderOpTarget = eyeColorPicker;
				_sliderAction = _recolorEyesAction;
				break;
			case 523:
				eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
				eyeColorPicker.changeSaturation(-1);
				eyeColorPicker.Dirty = true;
				_sliderOpTarget = eyeColorPicker;
				_sliderAction = _recolorEyesAction;
				break;
			case 524:
				eyeColorPicker.LastColor = eyeColorPicker.getSelectedColor();
				eyeColorPicker.changeValue(-1);
				eyeColorPicker.Dirty = true;
				_sliderOpTarget = eyeColorPicker;
				_sliderAction = _recolorEyesAction;
				break;
			case 525:
				hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
				hairColorPicker.changeHue(-1);
				hairColorPicker.Dirty = true;
				_sliderOpTarget = hairColorPicker;
				_sliderAction = _recolorHairAction;
				break;
			case 526:
				hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
				hairColorPicker.changeSaturation(-1);
				hairColorPicker.Dirty = true;
				_sliderOpTarget = hairColorPicker;
				_sliderAction = _recolorHairAction;
				break;
			case 527:
				hairColorPicker.LastColor = hairColorPicker.getSelectedColor();
				hairColorPicker.changeValue(-1);
				hairColorPicker.Dirty = true;
				_sliderOpTarget = hairColorPicker;
				_sliderAction = _recolorHairAction;
				break;
			case 528:
				pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
				pantsColorPicker.changeHue(-1);
				pantsColorPicker.Dirty = true;
				_sliderOpTarget = pantsColorPicker;
				_sliderAction = _recolorPantsAction;
				break;
			case 529:
				pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
				pantsColorPicker.changeSaturation(-1);
				pantsColorPicker.Dirty = true;
				_sliderOpTarget = pantsColorPicker;
				_sliderAction = _recolorPantsAction;
				break;
			case 530:
				pantsColorPicker.LastColor = pantsColorPicker.getSelectedColor();
				pantsColorPicker.changeValue(-1);
				pantsColorPicker.Dirty = true;
				_sliderOpTarget = pantsColorPicker;
				_sliderAction = _recolorPantsAction;
				break;
			}
			break;
		}
	}

	public override void receiveGamePadButton(Buttons b)
	{
		base.receiveGamePadButton(b);
		if (currentlySnappedComponent == null)
		{
			return;
		}
		switch (b)
		{
		case Buttons.RightTrigger:
		{
			int myID = currentlySnappedComponent.myID;
			if ((uint)(myID - 512) <= 9u)
			{
				selectionClick(currentlySnappedComponent.name, 1);
			}
			break;
		}
		case Buttons.LeftTrigger:
		{
			int myID = currentlySnappedComponent.myID;
			if ((uint)(myID - 512) <= 9u)
			{
				selectionClick(currentlySnappedComponent.name, -1);
			}
			break;
		}
		case Buttons.B:
			if (showingCoopHelp)
			{
				receiveLeftClick(coopHelpOkButton.bounds.Center.X, coopHelpOkButton.bounds.Center.Y);
			}
			break;
		}
	}

	private void optionButtonClick(string name)
	{
		if (name.StartsWith("ModFarm_"))
		{
			if (source == Source.NewGame || source == Source.HostNewFarm)
			{
				List<ModFarmType> list = DataLoader.AdditionalFarms(Game1.content);
				string farmId = name.Substring("ModFarm_".Length);
				foreach (ModFarmType farmType in list)
				{
					if (farmType.Id == farmId)
					{
						Game1.whichFarm = 7;
						Game1.whichModFarm = farmType;
						Game1.spawnMonstersAtNight = farmType.SpawnMonstersByDefault;
						break;
					}
				}
			}
		}
		else
		{
			switch (name)
			{
			case "Standard":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.whichFarm = 0;
					Game1.whichModFarm = null;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Riverland":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.whichFarm = 1;
					Game1.whichModFarm = null;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Forest":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.whichFarm = 2;
					Game1.whichModFarm = null;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Hills":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.whichFarm = 3;
					Game1.whichModFarm = null;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Wilderness":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.whichFarm = 4;
					Game1.whichModFarm = null;
					Game1.spawnMonstersAtNight = true;
				}
				break;
			case "Four Corners":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.whichFarm = 5;
					Game1.whichModFarm = null;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Beach":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.whichFarm = 6;
					Game1.whichModFarm = null;
					Game1.spawnMonstersAtNight = false;
				}
				break;
			case "Male":
				Game1.player.changeGender(male: true);
				if (source != Source.Wizard)
				{
					Game1.player.changeHairStyle(0);
				}
				break;
			case "Close":
				Game1.cabinsSeparate = false;
				break;
			case "Separate":
				Game1.cabinsSeparate = true;
				break;
			case "Female":
				Game1.player.changeGender(male: false);
				if (source != Source.Wizard)
				{
					Game1.player.changeHairStyle(16);
				}
				break;
			case "Cat":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.player.whichPetType = "Cat";
				}
				break;
			case "Dog":
				if (source == Source.NewGame || source == Source.HostNewFarm)
				{
					Game1.player.whichPetType = "Dog";
				}
				break;
			case "OK":
			{
				if (!canLeaveMenu())
				{
					return;
				}
				if (_itemToDye != null)
				{
					if (!Game1.player.IsEquippedItem(_itemToDye))
					{
						Utility.CollectOrDrop(_itemToDye);
					}
					_itemToDye = null;
				}
				if (source == Source.ClothesDye)
				{
					Game1.exitActiveMenu();
					break;
				}
				Game1.player.Name = Program.sdk.FilterDirtyWords(nameBox.Text.Trim());
				Game1.player.displayName = Program.sdk.FilterDirtyWords(Game1.player.Name);
				Game1.player.favoriteThing.Value = Program.sdk.FilterDirtyWords(favThingBox.Text.Trim());
				Game1.player.isCustomized.Value = true;
				Game1.player.ConvertClothingOverrideToClothesItems();
				if (source == Source.HostNewFarm)
				{
					Game1.multiplayerMode = 2;
				}
				try
				{
					if (Game1.player.Name != oldName)
					{
						int start = Game1.player.Name.IndexOf("[");
						int end = Game1.player.Name.IndexOf("]");
						if (start >= 0 && end > start)
						{
							string itemName = ItemRegistry.GetData(Game1.player.Name.Substring(start + 1, end - start - 1))?.DisplayName;
							if (itemName != null)
							{
								switch (Game1.random.Next(5))
								{
								case 0:
									Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg1"), new Color(104, 214, 255));
									break;
								case 1:
									Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg2", Lexicon.makePlural(itemName)), new Color(100, 50, 255));
									break;
								case 2:
									Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg3", Lexicon.makePlural(itemName)), new Color(0, 220, 40));
									break;
								case 3:
									Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg4"), new Color(0, 220, 40));
									DelayedAction.functionAfterDelay(delegate
									{
										Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg5"), new Color(104, 214, 255));
									}, 12000);
									break;
								case 4:
									Game1.chatBox.addMessage(Game1.content.LoadString("Strings\\UI:NameChange_EasterEgg6", Lexicon.getProperArticleForWord(itemName), itemName), new Color(100, 120, 255));
									break;
								}
							}
						}
					}
				}
				catch
				{
				}
				string changed_pet_name = null;
				if (petPortraitBox.HasValue && Game1.IsMasterGame && Game1.gameMode == 3 && Game1.locations != null)
				{
					Pet pet = Game1.getCharacterFromName<Pet>(Game1.player.getPetName(), mustBeVillager: false);
					if (pet != null && petHasChanges(pet))
					{
						pet.petType.Value = Game1.player.whichPetType;
						pet.whichBreed.Value = Game1.player.whichPetBreed;
						changed_pet_name = pet.getName();
					}
				}
				if (Game1.activeClickableMenu is TitleMenu titleMenu)
				{
					titleMenu.createdNewCharacter(skipIntro);
					break;
				}
				Game1.exitActiveMenu();
				if (Game1.currentMinigame is Intro intro)
				{
					intro.doneCreatingCharacter();
					break;
				}
				switch (source)
				{
				case Source.Wizard:
					if (changed_pet_name != null)
					{
						Game1.multiplayer.globalChatInfoMessage("Makeover_Pet", Game1.player.Name, changed_pet_name);
					}
					else
					{
						Game1.multiplayer.globalChatInfoMessage("Makeover", Game1.player.Name);
					}
					Game1.flashAlpha = 1f;
					Game1.playSound("yoba");
					break;
				case Source.ClothesDye:
					Game1.playSound("yoba");
					break;
				}
				break;
			}
			}
		}
		Game1.playSound("coin");
	}

	public bool petHasChanges(Pet pet)
	{
		if (Game1.player.whichPetType != pet.petType.Value)
		{
			return true;
		}
		if (Game1.player.whichPetBreed != pet.whichBreed.Value)
		{
			return true;
		}
		return false;
	}

	/// <summary>Load the tooltip translation for a farm type in the expected format.</summary>
	/// <param name="translationKey">The translation key to load.</param>
	/// <remarks>This returns a tooltip string in the form <c>name_description</c>.</remarks>
	protected virtual string GetFarmTypeTooltip(string translationKey)
	{
		string text = Game1.content.LoadString(translationKey);
		string[] parts = text.Split('_', 2);
		if (parts.Length == 1 || parts[1].Length == 0)
		{
			text = parts[0] + "_ ";
		}
		return text;
	}

	protected List<KeyValuePair<string, string>> GetPetTypesAndBreeds()
	{
		if (_petTypesAndBreeds == null)
		{
			_petTypesAndBreeds = new List<KeyValuePair<string, string>>();
			foreach (KeyValuePair<string, PetData> pair in Game1.petData)
			{
				if (isModifyingExistingPet && Game1.player.whichPetType != pair.Key)
				{
					continue;
				}
				foreach (PetBreed breed in pair.Value.Breeds)
				{
					if (breed.CanBeChosenAtStart)
					{
						_petTypesAndBreeds.Add(new KeyValuePair<string, string>(pair.Key, breed.Id));
					}
				}
			}
		}
		return _petTypesAndBreeds;
	}

	private void selectionClick(string name, int change)
	{
		switch (name)
		{
		case "Skin":
			Game1.player.changeSkinColor((int)Game1.player.skin + change);
			Game1.playSound("skeletonStep");
			break;
		case "Hair":
		{
			List<int> all_hairs = Farmer.GetAllHairstyleIndices();
			int current_index = all_hairs.IndexOf(Game1.player.hair);
			current_index += change;
			if (current_index >= all_hairs.Count)
			{
				current_index = 0;
			}
			else if (current_index < 0)
			{
				current_index = all_hairs.Count - 1;
			}
			Game1.player.changeHairStyle(all_hairs[current_index]);
			Game1.playSound("grassyStep");
			break;
		}
		case "Shirt":
			Game1.player.rotateShirt(change, GetValidShirtIds());
			Game1.playSound("coin");
			break;
		case "Pants Style":
			Game1.player.rotatePantStyle(change, GetValidPantsIds());
			Game1.playSound("coin");
			break;
		case "Acc":
			Game1.player.changeAccessory((int)Game1.player.accessory + change);
			Game1.playSound("purchase");
			break;
		case "Direction":
			_displayFarmer.faceDirection((_displayFarmer.FacingDirection - change + 4) % 4);
			_displayFarmer.FarmerSprite.StopAnimation();
			_displayFarmer.completelyStopAnimatingOrDoingAction();
			Game1.playSound("pickUpItem");
			break;
		case "Cabins":
			if ((Game1.startingCabins != 0 || change >= 0) && (Game1.startingCabins != Game1.multiplayer.playerLimit - 1 || change <= 0))
			{
				Game1.playSound("axchop");
			}
			Game1.startingCabins += change;
			Game1.startingCabins = Math.Max(0, Math.Min(Game1.multiplayer.playerLimit - 1, Game1.startingCabins));
			break;
		case "Difficulty":
			if (Game1.player.difficultyModifier < 1f && change < 0)
			{
				Game1.playSound("breathout");
				Game1.player.difficultyModifier += 0.25f;
			}
			else if (Game1.player.difficultyModifier > 0.25f && change > 0)
			{
				Game1.playSound("batFlap");
				Game1.player.difficultyModifier -= 0.25f;
			}
			break;
		case "Wallets":
			if ((bool)Game1.player.team.useSeparateWallets)
			{
				Game1.playSound("coin");
				Game1.player.team.useSeparateWallets.Value = false;
			}
			else
			{
				Game1.playSound("coin");
				Game1.player.team.useSeparateWallets.Value = true;
			}
			break;
		case "Pet":
		{
			List<KeyValuePair<string, string>> pets = GetPetTypesAndBreeds();
			int index = pets.IndexOf(new KeyValuePair<string, string>(Game1.player.whichPetType, Game1.player.whichPetBreed));
			index = ((index != -1) ? (index + change) : 0);
			if (index < 0)
			{
				index = pets.Count - 1;
			}
			else if (index >= pets.Count)
			{
				index = 0;
			}
			KeyValuePair<string, string> selectedPetType = pets[index];
			Game1.player.whichPetType = selectedPetType.Key;
			Game1.player.whichPetBreed = selectedPetType.Value;
			Game1.playSound("coin");
			break;
		}
		}
	}

	public void ShowAdvancedOptions()
	{
		AddDependency();
		(TitleMenu.subMenu = new AdvancedGameOptions()).exitFunction = delegate
		{
			TitleMenu.subMenu = this;
			RemoveDependency();
			populateClickableComponentList();
			if (Game1.options.SnappyMenus)
			{
				setCurrentlySnappedComponentTo(636);
				snapCursorToCurrentSnappedComponent();
			}
		};
	}

	public override bool readyToClose()
	{
		if (showingCoopHelp)
		{
			return false;
		}
		if (Game1.lastCursorMotionWasMouse)
		{
			foreach (ClickableTextureComponent farmTypeButton in farmTypeButtons)
			{
				if (farmTypeButton.containsPoint(Game1.getMouseX(ui_scale: true), Game1.getMouseY(ui_scale: true)))
				{
					return false;
				}
			}
		}
		return base.readyToClose();
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (showingCoopHelp)
		{
			if (coopHelpOkButton != null && coopHelpOkButton.containsPoint(x, y))
			{
				showingCoopHelp = false;
				Game1.playSound("bigDeSelect");
				if (Game1.options.SnappyMenus)
				{
					currentlySnappedComponent = coopHelpButton;
					snapCursorToCurrentSnappedComponent();
				}
			}
			if (coopHelpScreen == 0 && coopHelpRightButton != null && coopHelpRightButton.containsPoint(x, y))
			{
				coopHelpScreen++;
				coopHelpString = Game1.parseText(Game1.content.LoadString("Strings\\UI:Character_CoopHelpString2").Replace("^", Environment.NewLine), Game1.dialogueFont, width + 384 - IClickableMenu.borderWidth * 2);
				Game1.playSound("shwip");
			}
			if (coopHelpScreen == 1 && coopHelpLeftButton != null && coopHelpLeftButton.containsPoint(x, y))
			{
				coopHelpScreen--;
				string rawText = string.Format(Game1.content.LoadString("Strings\\UI:Character_CoopHelpString").Replace("^", Environment.NewLine), Game1.multiplayer.playerLimit - 1);
				coopHelpString = Game1.parseText(rawText, Game1.dialogueFont, width + 384 - IClickableMenu.borderWidth * 2);
				Game1.playSound("shwip");
			}
			return;
		}
		if (genderButtons.Count > 0)
		{
			foreach (ClickableComponent c in genderButtons)
			{
				if (c.containsPoint(x, y))
				{
					optionButtonClick(c.name);
					c.scale -= 0.5f;
					c.scale = Math.Max(3.5f, c.scale);
				}
			}
		}
		if (farmTypeNextPageButton != null && farmTypeNextPageButton.containsPoint(x, y))
		{
			Game1.playSound("shwip");
			_currentFarmPage++;
			RefreshFarmTypeButtons();
		}
		else if (farmTypePreviousPageButton != null && farmTypePreviousPageButton.containsPoint(x, y))
		{
			Game1.playSound("shwip");
			_currentFarmPage--;
			RefreshFarmTypeButtons();
		}
		else if (farmTypeButtons.Count > 0)
		{
			foreach (ClickableTextureComponent c in farmTypeButtons)
			{
				if (c.containsPoint(x, y) && !c.name.Contains("Gray"))
				{
					optionButtonClick(c.name);
					c.scale -= 0.5f;
					c.scale = Math.Max(3.5f, c.scale);
				}
			}
		}
		if (cabinLayoutButtons.Count > 0)
		{
			foreach (ClickableTextureComponent c in cabinLayoutButtons)
			{
				if (Game1.startingCabins > 0 && c.containsPoint(x, y))
				{
					optionButtonClick(c.name);
					c.scale -= 0.5f;
					c.scale = Math.Max(3.5f, c.scale);
				}
			}
		}
		if (leftSelectionButtons.Count > 0)
		{
			foreach (ClickableComponent c in leftSelectionButtons)
			{
				if (c.containsPoint(x, y))
				{
					selectionClick(c.name, -1);
					if (c.scale != 0f)
					{
						c.scale -= 0.25f;
						c.scale = Math.Max(0.75f, c.scale);
					}
				}
			}
		}
		if (rightSelectionButtons.Count > 0)
		{
			foreach (ClickableComponent c in rightSelectionButtons)
			{
				if (c.containsPoint(x, y))
				{
					selectionClick(c.name, 1);
					if (c.scale != 0f)
					{
						c.scale -= 0.25f;
						c.scale = Math.Max(0.75f, c.scale);
					}
				}
			}
		}
		if (okButton.containsPoint(x, y) && canLeaveMenu())
		{
			optionButtonClick(okButton.name);
			okButton.scale -= 0.25f;
			okButton.scale = Math.Max(0.75f, okButton.scale);
		}
		if (hairColorPicker != null && hairColorPicker.containsPoint(x, y))
		{
			Color color = hairColorPicker.click(x, y);
			if (source == Source.DyePots)
			{
				if (Game1.player.CanDyeShirt())
				{
					Game1.player.shirtItem.Value.clothesColor.Value = color;
					Game1.player.FarmerRenderer.MarkSpriteDirty();
					_displayFarmer.FarmerRenderer.MarkSpriteDirty();
				}
			}
			else
			{
				Game1.player.changeHairColor(color);
			}
			lastHeldColorPicker = hairColorPicker;
		}
		else if (pantsColorPicker != null && pantsColorPicker.containsPoint(x, y))
		{
			Color color = pantsColorPicker.click(x, y);
			switch (source)
			{
			case Source.DyePots:
				if (Game1.player.CanDyePants())
				{
					Game1.player.pantsItem.Value.clothesColor.Value = color;
					Game1.player.FarmerRenderer.MarkSpriteDirty();
					_displayFarmer.FarmerRenderer.MarkSpriteDirty();
				}
				break;
			case Source.ClothesDye:
				DyeItem(color);
				break;
			default:
				Game1.player.changePantsColor(color);
				break;
			}
			lastHeldColorPicker = pantsColorPicker;
		}
		else if (eyeColorPicker != null && eyeColorPicker.containsPoint(x, y))
		{
			Game1.player.changeEyeColor(eyeColorPicker.click(x, y));
			lastHeldColorPicker = eyeColorPicker;
		}
		if (source != Source.Dresser && source != Source.ClothesDye && source != Source.DyePots)
		{
			nameBox.Update();
			if (source == Source.NewGame || source == Source.HostNewFarm)
			{
				farmnameBox.Update();
			}
			else
			{
				farmnameBox.Text = Game1.MasterPlayer.farmName.Value;
			}
			favThingBox.Update();
			if ((source == Source.NewGame || source == Source.HostNewFarm) && skipIntroButton.containsPoint(x, y))
			{
				Game1.playSound("drumkit6");
				skipIntroButton.sourceRect.X = ((skipIntroButton.sourceRect.X == 227) ? 236 : 227);
				skipIntro = !skipIntro;
			}
		}
		if (coopHelpButton != null && coopHelpButton.containsPoint(x, y))
		{
			if (Game1.options.SnappyMenus)
			{
				currentlySnappedComponent = coopHelpOkButton;
				snapCursorToCurrentSnappedComponent();
			}
			Game1.playSound("bigSelect");
			showingCoopHelp = true;
			coopHelpScreen = 0;
			string rawText = string.Format(Game1.content.LoadString("Strings\\UI:Character_CoopHelpString").Replace("^", Environment.NewLine), Game1.multiplayer.playerLimit - 1);
			coopHelpString = Game1.parseText(rawText, Game1.dialogueFont, width + 384 - IClickableMenu.borderWidth * 2);
			helpStringSize = Game1.dialogueFont.MeasureString(coopHelpString);
			coopHelpRightButton.bounds.Y = yPositionOnScreen + (int)helpStringSize.Y + IClickableMenu.borderWidth * 2 - 4;
			coopHelpRightButton.bounds.X = xPositionOnScreen + (int)helpStringSize.X - IClickableMenu.borderWidth * 5;
			coopHelpLeftButton.bounds.Y = yPositionOnScreen + (int)helpStringSize.Y + IClickableMenu.borderWidth * 2 - 4;
			coopHelpLeftButton.bounds.X = xPositionOnScreen - IClickableMenu.borderWidth * 4;
		}
		if (advancedOptionsButton != null && advancedOptionsButton.containsPoint(x, y))
		{
			Game1.playSound("drumkit6");
			ShowAdvancedOptions();
		}
		if (!randomButton.containsPoint(x, y))
		{
			return;
		}
		string sound = "drumkit6";
		if (timesRandom > 0)
		{
			switch (Game1.random.Next(15))
			{
			case 0:
				sound = "drumkit1";
				break;
			case 1:
				sound = "dirtyHit";
				break;
			case 2:
				sound = "axchop";
				break;
			case 3:
				sound = "hoeHit";
				break;
			case 4:
				sound = "fishSlap";
				break;
			case 5:
				sound = "drumkit6";
				break;
			case 6:
				sound = "drumkit5";
				break;
			case 7:
				sound = "drumkit6";
				break;
			case 8:
				sound = "junimoMeep1";
				break;
			case 9:
				sound = "coin";
				break;
			case 10:
				sound = "axe";
				break;
			case 11:
				sound = "hammer";
				break;
			case 12:
				sound = "drumkit2";
				break;
			case 13:
				sound = "drumkit4";
				break;
			case 14:
				sound = "drumkit3";
				break;
			}
		}
		Game1.playSound(sound);
		timesRandom++;
		if (accLabel != null && accLabel.visible)
		{
			if (Game1.random.NextDouble() < 0.33)
			{
				if (Game1.player.IsMale)
				{
					if (Game1.random.NextDouble() < 0.33)
					{
						if (Game1.random.NextDouble() < 0.8)
						{
							Game1.player.changeAccessory(Game1.random.Next(7));
						}
						else
						{
							Game1.player.changeAccessory(Game1.random.Next(19, 21));
						}
					}
					else if (Game1.random.NextDouble() < 0.33)
					{
						Game1.player.changeAccessory(Game1.random.Choose<int>(25, 14, 17, 10, 9));
					}
					else if (Game1.random.NextDouble() < 0.1)
					{
						Game1.player.changeAccessory(Game1.random.Next(19));
					}
				}
				else if (Game1.random.NextDouble() < 0.33)
				{
					Game1.player.changeAccessory(Game1.random.Next(6, 19));
				}
				else if (Game1.random.NextDouble() < 0.5)
				{
					Game1.player.changeAccessory(Game1.random.Choose(23, 27, 28));
				}
				else
				{
					Game1.player.changeAccessory(Game1.random.Choose<int>(25, 14, 17, 10, 9));
				}
			}
			else
			{
				Game1.player.changeAccessory(-1);
			}
		}
		if (skinLabel != null && skinLabel.visible)
		{
			Game1.player.changeSkinColor(Game1.random.Next(6));
			if (Game1.random.NextDouble() < 0.15)
			{
				Game1.player.changeSkinColor(Game1.random.Next(24));
			}
		}
		if (hairLabel != null && hairLabel.visible)
		{
			if (Game1.player.IsMale)
			{
				Game1.player.changeHairStyle(Game1.random.NextBool() ? Game1.random.Next(16) : Game1.random.Next(108, 118));
			}
			else
			{
				Game1.player.changeHairStyle(Game1.random.Next(16, 41));
			}
			Color hairColor = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
			if (Game1.random.NextBool())
			{
				hairColor.R /= 2;
				hairColor.G /= 2;
				hairColor.B /= 2;
			}
			if (Game1.random.NextBool())
			{
				hairColor.R = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextBool())
			{
				hairColor.G = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextBool())
			{
				hairColor.B = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextBool())
			{
				if (hairColor.B > hairColor.R)
				{
					hairColor.B = (byte)Math.Max(0, hairColor.B - 50);
				}
				if (hairColor.B > hairColor.G)
				{
					hairColor.B = (byte)Math.Max(0, hairColor.B - 50);
				}
				if (hairColor.G > hairColor.R)
				{
					hairColor.G = (byte)Math.Max(0, hairColor.R - 50);
				}
				hairColor.R = (byte)Math.Min(255, hairColor.R + 50);
				hairColor.G = (byte)Math.Min(255, hairColor.G + 50);
			}
			else if (Game1.random.NextDouble() < 0.33)
			{
				hairColor = new Color(Game1.random.Next(80, 130), Game1.random.Next(35, 70), 0);
			}
			if (hairColor.R < 100 && hairColor.G < 100 && hairColor.B < 100 && Game1.random.NextDouble() < 0.8)
			{
				hairColor = Utility.getBlendedColor(hairColor, Color.Tan);
			}
			if (Game1.player.hasDarkSkin() && Game1.random.NextDouble() < 0.5)
			{
				hairColor = new Color(Game1.random.Next(50, 100), Game1.random.Next(25, 40), 0);
			}
			Game1.player.changeHairColor(hairColor);
			hairColorPicker.setColor(hairColor);
		}
		if (shirtLabel != null && shirtLabel.visible)
		{
			string shirtSelection = "";
			Utility.TryGetRandomExcept(GetValidShirtIds(), Game1.player.IsMale ? new HashSet<string>
			{
				"1056", "1057", "1070", "1046", "1040", "1060", "1090", "1051", "1082", "1107",
				"1080", "1083", "1092", "1072", "1076", "1041"
			} : new HashSet<string>(), Game1.random, out shirtSelection);
			Game1.player.changeShirt(shirtSelection);
		}
		if (pantsStyleLabel != null && pantsStyleLabel.visible)
		{
			Color pantsColor = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
			if (Game1.random.NextBool())
			{
				pantsColor.R /= 2;
				pantsColor.G /= 2;
				pantsColor.B /= 2;
			}
			if (Game1.random.NextBool())
			{
				pantsColor.R = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextBool())
			{
				pantsColor.G = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextBool())
			{
				pantsColor.B = (byte)Game1.random.Next(15, 50);
			}
			switch (Game1.player.GetShirtIndex())
			{
			case 50:
				pantsColor = new Color(226, 133, 160);
				break;
			case 0:
			case 7:
			case 71:
				pantsColor = new Color(34, 29, 173);
				break;
			case 68:
			case 88:
				pantsColor = new Color(119, 215, 130);
				break;
			case 67:
			case 72:
				pantsColor = new Color(108, 134, 224);
				break;
			case 79:
			case 99:
			case 103:
				pantsColor = new Color(55, 55, 60);
				break;
			}
			Game1.player.changePantsColor(pantsColor);
			pantsColorPicker.setColor(Game1.player.GetPantsColor());
		}
		if (eyeColorPicker != null)
		{
			Color eyeColor = new Color(Game1.random.Next(25, 254), Game1.random.Next(25, 254), Game1.random.Next(25, 254));
			eyeColor.R /= 2;
			eyeColor.G /= 2;
			eyeColor.B /= 2;
			if (Game1.random.NextBool())
			{
				eyeColor.R = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextBool())
			{
				eyeColor.G = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextBool())
			{
				eyeColor.B = (byte)Game1.random.Next(15, 50);
			}
			if (Game1.random.NextBool())
			{
				if (eyeColor.B > eyeColor.R)
				{
					eyeColor.B = (byte)Math.Max(0, eyeColor.B - 50);
				}
				if (eyeColor.B > eyeColor.G)
				{
					eyeColor.B = (byte)Math.Max(0, eyeColor.B - 50);
				}
				if (eyeColor.G > eyeColor.R)
				{
					eyeColor.G = (byte)Math.Max(0, eyeColor.R - 50);
				}
			}
			Game1.player.changeEyeColor(eyeColor);
			eyeColorPicker.setColor(Game1.player.newEyeColor.Value);
		}
		randomButton.scale = 3.5f;
	}

	/// <summary>Get the shirts or pants which can be selected on the character customization screen.</summary>
	/// <typeparam name="TData">The clothing data.</typeparam>
	/// <param name="equippedId">The unqualified item ID for the item equipped by the player.</param>
	/// <param name="data">The data to search.</param>
	/// <param name="canChooseDuringCharacterCustomization">Get whether a clothing item should be visible on the character customization screen.</param>
	public List<string> GetValidClothingIds<TData>(string equippedId, IDictionary<string, TData> data, Func<TData, bool> canChooseDuringCharacterCustomization)
	{
		List<string> validIds = new List<string>();
		foreach (KeyValuePair<string, TData> pair in data)
		{
			if (pair.Key == equippedId || canChooseDuringCharacterCustomization(pair.Value))
			{
				validIds.Add(pair.Key);
			}
		}
		return validIds;
	}

	/// <summary>Get the pants which can be selected on the character customization screen.</summary>
	public List<string> GetValidPantsIds()
	{
		return GetValidClothingIds(Game1.player.pants, Game1.pantsData, (PantsData data) => data.CanChooseDuringCharacterCustomization);
	}

	/// <summary>Get the shirts which can be selected on the character customization screen.</summary>
	public List<string> GetValidShirtIds()
	{
		return GetValidClothingIds(Game1.player.shirt, Game1.shirtData, (ShirtData data) => data.CanChooseDuringCharacterCustomization);
	}

	public override void leftClickHeld(int x, int y)
	{
		colorPickerTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
		if (colorPickerTimer > 0)
		{
			return;
		}
		if (lastHeldColorPicker != null && !Game1.options.SnappyMenus)
		{
			if (lastHeldColorPicker.Equals(hairColorPicker))
			{
				Color color = hairColorPicker.clickHeld(x, y);
				if (source == Source.DyePots)
				{
					if (Game1.player.CanDyeShirt())
					{
						Game1.player.shirtItem.Value.clothesColor.Value = color;
						Game1.player.FarmerRenderer.MarkSpriteDirty();
						_displayFarmer.FarmerRenderer.MarkSpriteDirty();
					}
				}
				else
				{
					Game1.player.changeHairColor(color);
				}
			}
			if (lastHeldColorPicker.Equals(pantsColorPicker))
			{
				Color color = pantsColorPicker.clickHeld(x, y);
				switch (source)
				{
				case Source.DyePots:
					if (Game1.player.CanDyePants())
					{
						Game1.player.pantsItem.Value.clothesColor.Value = color;
						Game1.player.FarmerRenderer.MarkSpriteDirty();
						_displayFarmer.FarmerRenderer.MarkSpriteDirty();
					}
					break;
				case Source.ClothesDye:
					DyeItem(color);
					break;
				default:
					Game1.player.changePantsColor(color);
					break;
				}
			}
			if (lastHeldColorPicker.Equals(eyeColorPicker))
			{
				Game1.player.changeEyeColor(eyeColorPicker.clickHeld(x, y));
			}
		}
		colorPickerTimer = 100;
	}

	public override void releaseLeftClick(int x, int y)
	{
		hairColorPicker?.releaseClick();
		pantsColorPicker?.releaseClick();
		eyeColorPicker?.releaseClick();
		lastHeldColorPicker = null;
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
	}

	public override void receiveKeyPress(Keys key)
	{
		if (key == Keys.Tab)
		{
			switch (source)
			{
			case Source.NewGame:
			case Source.HostNewFarm:
				if (nameBox.Selected)
				{
					farmnameBox.SelectMe();
					nameBox.Selected = false;
				}
				else if (farmnameBox.Selected)
				{
					farmnameBox.Selected = false;
					favThingBox.SelectMe();
				}
				else
				{
					favThingBox.Selected = false;
					nameBox.SelectMe();
				}
				break;
			case Source.NewFarmhand:
				if (nameBox.Selected)
				{
					favThingBox.SelectMe();
					nameBox.Selected = false;
				}
				else
				{
					favThingBox.Selected = false;
					nameBox.SelectMe();
				}
				break;
			}
		}
		if (Game1.options.SnappyMenus && !Game1.options.doesInputListContain(Game1.options.menuButton, key) && Game1.GetKeyboardState().GetPressedKeys().Length == 0)
		{
			base.receiveKeyPress(key);
		}
	}

	public override void performHoverAction(int x, int y)
	{
		hoverText = "";
		hoverTitle = "";
		foreach (ClickableTextureComponent c in leftSelectionButtons)
		{
			if (c.containsPoint(x, y))
			{
				c.scale = Math.Min(c.scale + 0.02f, c.baseScale + 0.1f);
			}
			else
			{
				c.scale = Math.Max(c.scale - 0.02f, c.baseScale);
			}
			if (c.name.Equals("Cabins") && Game1.startingCabins == 0)
			{
				c.scale = 0f;
			}
		}
		foreach (ClickableTextureComponent c in rightSelectionButtons)
		{
			if (c.containsPoint(x, y))
			{
				c.scale = Math.Min(c.scale + 0.02f, c.baseScale + 0.1f);
			}
			else
			{
				c.scale = Math.Max(c.scale - 0.02f, c.baseScale);
			}
			if (c.name.Equals("Cabins") && Game1.startingCabins == Game1.multiplayer.playerLimit - 1)
			{
				c.scale = 0f;
			}
		}
		if (source == Source.NewGame || source == Source.HostNewFarm)
		{
			foreach (ClickableTextureComponent c in farmTypeButtons)
			{
				if (c.containsPoint(x, y) && !c.name.Contains("Gray"))
				{
					c.scale = Math.Min(c.scale + 0.02f, c.baseScale + 0.1f);
					hoverTitle = c.hoverText.Split('_')[0];
					hoverText = c.hoverText.Split('_')[1];
					continue;
				}
				c.scale = Math.Max(c.scale - 0.02f, c.baseScale);
				if (c.name.Contains("Gray") && c.containsPoint(x, y))
				{
					hoverText = "Reach level 10 " + Game1.content.LoadString("Strings\\UI:Character_" + c.name.Split('_')[1]) + " to unlock.";
				}
			}
		}
		foreach (ClickableTextureComponent c in genderButtons)
		{
			if (c.containsPoint(x, y))
			{
				c.scale = Math.Min(c.scale + 0.05f, c.baseScale + 0.5f);
			}
			else
			{
				c.scale = Math.Max(c.scale - 0.05f, c.baseScale);
			}
		}
		if (source == Source.NewGame || source == Source.HostNewFarm)
		{
			foreach (ClickableTextureComponent c in cabinLayoutButtons)
			{
				if (Game1.startingCabins > 0 && c.containsPoint(x, y))
				{
					c.scale = Math.Min(c.scale + 0.05f, c.baseScale + 0.5f);
					hoverText = c.hoverText;
				}
				else
				{
					c.scale = Math.Max(c.scale - 0.05f, c.baseScale);
				}
			}
		}
		if (okButton.containsPoint(x, y) && canLeaveMenu())
		{
			okButton.scale = Math.Min(okButton.scale + 0.02f, okButton.baseScale + 0.1f);
		}
		else
		{
			okButton.scale = Math.Max(okButton.scale - 0.02f, okButton.baseScale);
		}
		if (coopHelpButton != null)
		{
			if (coopHelpButton.containsPoint(x, y))
			{
				coopHelpButton.scale = Math.Min(coopHelpButton.scale + 0.05f, coopHelpButton.baseScale + 0.5f);
				hoverText = coopHelpButton.hoverText;
			}
			else
			{
				coopHelpButton.scale = Math.Max(coopHelpButton.scale - 0.05f, coopHelpButton.baseScale);
			}
		}
		if (coopHelpOkButton != null)
		{
			if (coopHelpOkButton.containsPoint(x, y))
			{
				coopHelpOkButton.scale = Math.Min(coopHelpOkButton.scale + 0.025f, coopHelpOkButton.baseScale + 0.2f);
			}
			else
			{
				coopHelpOkButton.scale = Math.Max(coopHelpOkButton.scale - 0.025f, coopHelpOkButton.baseScale);
			}
		}
		if (coopHelpRightButton != null)
		{
			if (coopHelpRightButton.containsPoint(x, y))
			{
				coopHelpRightButton.scale = Math.Min(coopHelpRightButton.scale + 0.025f, coopHelpRightButton.baseScale + 0.2f);
			}
			else
			{
				coopHelpRightButton.scale = Math.Max(coopHelpRightButton.scale - 0.025f, coopHelpRightButton.baseScale);
			}
		}
		if (coopHelpLeftButton != null)
		{
			if (coopHelpLeftButton.containsPoint(x, y))
			{
				coopHelpLeftButton.scale = Math.Min(coopHelpLeftButton.scale + 0.025f, coopHelpLeftButton.baseScale + 0.2f);
			}
			else
			{
				coopHelpLeftButton.scale = Math.Max(coopHelpLeftButton.scale - 0.025f, coopHelpLeftButton.baseScale);
			}
		}
		advancedOptionsButton?.tryHover(x, y);
		farmTypeNextPageButton?.tryHover(x, y);
		farmTypePreviousPageButton?.tryHover(x, y);
		randomButton.tryHover(x, y, 0.25f);
		randomButton.tryHover(x, y, 0.25f);
		if ((hairColorPicker != null && hairColorPicker.containsPoint(x, y)) || (pantsColorPicker != null && pantsColorPicker.containsPoint(x, y)) || (eyeColorPicker != null && eyeColorPicker.containsPoint(x, y)))
		{
			Game1.SetFreeCursorDrag();
		}
		nameBox.Hover(x, y);
		farmnameBox.Hover(x, y);
		favThingBox.Hover(x, y);
		skipIntroButton.tryHover(x, y);
	}

	public bool canLeaveMenu()
	{
		if (source != Source.ClothesDye && source != Source.DyePots)
		{
			if (Game1.player.Name.Length > 0 && Game1.player.farmName.Length > 0)
			{
				return Game1.player.favoriteThing.Length > 0;
			}
			return false;
		}
		return true;
	}

	private string getNameOfDifficulty()
	{
		if (Game1.player.difficultyModifier < 0.5f)
		{
			return superDiffString;
		}
		if (Game1.player.difficultyModifier < 0.75f)
		{
			return hardDiffString;
		}
		if (Game1.player.difficultyModifier < 1f)
		{
			return toughDiffString;
		}
		return normalDiffString;
	}

	public override void draw(SpriteBatch b)
	{
		if (showingCoopHelp)
		{
			IClickableMenu.drawTextureBox(b, xPositionOnScreen - 192, yPositionOnScreen + 64, (int)helpStringSize.X + IClickableMenu.borderWidth * 2, (int)helpStringSize.Y + IClickableMenu.borderWidth * 2, Color.White);
			Utility.drawTextWithShadow(b, coopHelpString, Game1.dialogueFont, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth - 192, yPositionOnScreen + IClickableMenu.borderWidth + 64), Game1.textColor);
			coopHelpOkButton?.draw(b, Color.White, 0.95f);
			coopHelpRightButton?.draw(b, Color.White, 0.95f);
			coopHelpLeftButton?.draw(b, Color.White, 0.95f);
			drawMouse(b);
			return;
		}
		Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, base.width, height, speaker: false, drawOnlyBox: true);
		if (source == Source.HostNewFarm)
		{
			IClickableMenu.drawTextureBox(b, xPositionOnScreen - 256 + 4 - ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 25 : 0), yPositionOnScreen + IClickableMenu.borderWidth * 2 + 68, (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? 320 : 256, 512, Color.White);
			foreach (ClickableTextureComponent c in cabinLayoutButtons)
			{
				c.draw(b, Color.White * ((Game1.startingCabins > 0) ? 1f : 0.5f), 0.9f);
				if (Game1.startingCabins > 0 && ((c.name.Equals("Close") && !Game1.cabinsSeparate) || (c.name.Equals("Separate") && Game1.cabinsSeparate)))
				{
					b.Draw(Game1.mouseCursors, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 34), Color.White);
				}
			}
		}
		b.Draw(Game1.daybg, new Vector2(portraitBox.X, portraitBox.Y), Color.White);
		foreach (ClickableTextureComponent c in genderButtons)
		{
			if (c.visible)
			{
				c.draw(b);
				if ((c.name.Equals("Male") && Game1.player.IsMale) || (c.name.Equals("Female") && !Game1.player.IsMale))
				{
					b.Draw(Game1.mouseCursors, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 34), Color.White);
				}
			}
		}
		if (nameBoxCC.visible)
		{
			Game1.player.Name = nameBox.Text;
		}
		if (favThingBoxCC.visible)
		{
			Game1.player.favoriteThing.Value = favThingBox.Text;
		}
		if (farmnameBoxCC.visible)
		{
			Game1.player.farmName.Value = farmnameBox.Text;
		}
		if (source == Source.NewFarmhand)
		{
			Game1.player.farmName.Value = Game1.MasterPlayer.farmName.Value;
		}
		foreach (ClickableTextureComponent leftSelectionButton in leftSelectionButtons)
		{
			leftSelectionButton.draw(b);
		}
		foreach (ClickableComponent c in labels)
		{
			if (!c.visible)
			{
				continue;
			}
			string sub = "";
			float offset = 0f;
			float subYOffset = 0f;
			Color color = Game1.textColor;
			if (c == nameLabel)
			{
				string name = Game1.player.Name;
				color = ((name != null && name.Length < 1) ? Color.Red : Game1.textColor);
			}
			else if (c == farmLabel)
			{
				color = ((Game1.player.farmName.Value != null && Game1.player.farmName.Length < 1) ? Color.Red : Game1.textColor);
			}
			else if (c == favoriteLabel)
			{
				color = ((Game1.player.favoriteThing.Value != null && Game1.player.favoriteThing.Length < 1) ? Color.Red : Game1.textColor);
			}
			else if (c == shirtLabel)
			{
				offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
				sub = Game1.player.GetShirtIndex().ToString();
				if (int.TryParse(sub, out var id))
				{
					sub = (id + 1).ToString();
				}
			}
			else if (c == skinLabel)
			{
				offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
				sub = ((int)Game1.player.skin + 1).ToString() ?? "";
			}
			else if (c == hairLabel)
			{
				offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
				if (!c.name.Contains("Color"))
				{
					sub = (Farmer.GetAllHairstyleIndices().IndexOf(Game1.player.hair) + 1).ToString() ?? "";
				}
			}
			else if (c == accLabel)
			{
				offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
				sub = ((int)Game1.player.accessory + 2).ToString() ?? "";
			}
			else if (c == pantsStyleLabel)
			{
				offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
				sub = Game1.player.GetPantsIndex().ToString();
				if (int.TryParse(sub, out var id))
				{
					sub = (id + 1).ToString();
				}
			}
			else if (c == startingCabinsLabel)
			{
				offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
				sub = ((Game1.startingCabins == 0 && noneString != null) ? noneString : (Game1.startingCabins.ToString() ?? ""));
				subYOffset = 4f;
			}
			else if (c == difficultyModifierLabel)
			{
				offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
				subYOffset = 4f;
				sub = getNameOfDifficulty();
			}
			else if (c == separateWalletLabel)
			{
				offset = 21f - Game1.smallFont.MeasureString(c.name).X / 2f;
				subYOffset = 4f;
				sub = (Game1.player.team.useSeparateWallets ? separateWalletString : sharedWalletString);
			}
			else
			{
				color = Game1.textColor;
			}
			Utility.drawTextWithShadow(b, c.name, Game1.smallFont, new Vector2((float)c.bounds.X + offset, c.bounds.Y), color);
			if (sub.Length > 0)
			{
				Utility.drawTextWithShadow(b, sub, Game1.smallFont, new Vector2((float)(c.bounds.X + 21) - Game1.smallFont.MeasureString(sub).X / 2f, (float)(c.bounds.Y + 32) + subYOffset), color);
			}
		}
		foreach (ClickableTextureComponent rightSelectionButton in rightSelectionButtons)
		{
			rightSelectionButton.draw(b);
		}
		if (farmTypeButtons.Count > 0)
		{
			IClickableMenu.drawTextureBox(b, farmTypeButtons[0].bounds.X - 16, farmTypeButtons[0].bounds.Y - 20, 220, 564, Color.White);
			for (int i = 0; i < farmTypeButtons.Count; i++)
			{
				farmTypeButtons[i].draw(b, farmTypeButtons[i].name.Contains("Gray") ? (Color.Black * 0.5f) : Color.White, 0.88f);
				if (farmTypeButtons[i].name.Contains("Gray"))
				{
					b.Draw(Game1.mouseCursors, new Vector2(farmTypeButtons[i].bounds.Center.X - 12, farmTypeButtons[i].bounds.Center.Y - 8), new Rectangle(107, 442, 7, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
				}
				bool farm_is_selected = false;
				int index = i + _currentFarmPage * 6;
				if (Game1.whichFarm == 7)
				{
					if ("ModFarm_" + Game1.whichModFarm.Id == farmTypeButtonNames[index])
					{
						farm_is_selected = true;
					}
				}
				else if (Game1.whichFarm == index)
				{
					farm_is_selected = true;
				}
				if (farm_is_selected)
				{
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), farmTypeButtons[i].bounds.X, farmTypeButtons[i].bounds.Y - 4, farmTypeButtons[i].bounds.Width, farmTypeButtons[i].bounds.Height + 8, Color.White, 4f, drawShadow: false);
				}
			}
			farmTypeNextPageButton?.draw(b);
			farmTypePreviousPageButton?.draw(b);
		}
		if (petPortraitBox.HasValue && Pet.TryGetData(Game1.MasterPlayer.whichPetType, out var petData))
		{
			Texture2D texture = null;
			Rectangle sourceRect = Rectangle.Empty;
			foreach (PetBreed breed in petData.Breeds)
			{
				if (breed.Id == Game1.MasterPlayer.whichPetBreed)
				{
					texture = Game1.content.Load<Texture2D>(breed.IconTexture);
					sourceRect = breed.IconSourceRect;
					break;
				}
			}
			if (texture != null)
			{
				b.Draw(texture, petPortraitBox.Value, sourceRect, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0.89f);
			}
		}
		advancedOptionsButton?.draw(b);
		if (canLeaveMenu())
		{
			okButton.draw(b, Color.White, 0.75f);
		}
		else
		{
			okButton.draw(b, Color.White, 0.75f);
			okButton.draw(b, Color.Black * 0.5f, 0.751f);
		}
		coopHelpButton?.draw(b, Color.White, 0.75f);
		hairColorPicker?.draw(b);
		pantsColorPicker?.draw(b);
		eyeColorPicker?.draw(b);
		if (source != Source.Dresser && source != Source.DyePots && source != Source.ClothesDye)
		{
			nameBox.Draw(b);
			favThingBox.Draw(b);
		}
		if (farmnameBoxCC.visible)
		{
			farmnameBox.Draw(b);
			Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:Character_FarmNameSuffix"), Game1.smallFont, new Vector2(farmnameBox.X + farmnameBox.Width + 8, farmnameBox.Y + 12), Game1.textColor);
		}
		if (skipIntroButton != null && skipIntroButton.visible)
		{
			skipIntroButton.draw(b);
			Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:Character_SkipIntro"), Game1.smallFont, new Vector2(skipIntroButton.bounds.X + skipIntroButton.bounds.Width + 8, skipIntroButton.bounds.Y + 8), Game1.textColor);
		}
		if (advancedCCHighlightTimer > 0f)
		{
			b.Draw(Game1.mouseCursors, advancedOptionsButton.getVector2() + new Vector2(4f, 84f), new Rectangle(128 + ((advancedCCHighlightTimer % 500f < 250f) ? 16 : 0), 208, 16, 16), Color.White * Math.Min(1f, advancedCCHighlightTimer / 500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.5f);
		}
		randomButton.draw(b);
		b.End();
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		_displayFarmer.FarmerRenderer.draw(b, _displayFarmer.FarmerSprite.CurrentAnimationFrame, _displayFarmer.FarmerSprite.CurrentFrame, _displayFarmer.FarmerSprite.SourceRect, new Vector2(portraitBox.Center.X - 32, portraitBox.Bottom - 160), Vector2.Zero, 0.8f, Color.White, 0f, 1f, _displayFarmer);
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		string text = hoverTitle;
		if (text != null && text.Length > 0)
		{
			int width = Math.Max((int)Game1.dialogueFont.MeasureString(hoverTitle).X, 256);
			IClickableMenu.drawHoverText(b, Game1.parseText(hoverText, Game1.smallFont, width), Game1.smallFont, 0, 0, -1, hoverTitle);
		}
		drawMouse(b);
	}

	public override void emergencyShutDown()
	{
		if (_itemToDye != null)
		{
			if (!Game1.player.IsEquippedItem(_itemToDye))
			{
				Utility.CollectOrDrop(_itemToDye);
			}
			_itemToDye = null;
		}
		base.emergencyShutDown();
	}

	public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
	{
		if (a.region != b.region)
		{
			return false;
		}
		if (advancedOptionsButton != null && backButton != null && a == advancedOptionsButton && b == backButton)
		{
			return false;
		}
		if (source == Source.Wizard)
		{
			if (a == favThingBoxCC && b.myID >= 522 && b.myID <= 530)
			{
				return false;
			}
			if (b == favThingBoxCC && a.myID >= 522 && a.myID <= 530)
			{
				return false;
			}
		}
		if (source == Source.Wizard)
		{
			if (a.name == "Direction" && b.name == "Pet")
			{
				return false;
			}
			if (b.name == "Direction" && a.name == "Pet")
			{
				return false;
			}
		}
		if (randomButton != null)
		{
			switch (direction)
			{
			case 3:
				if (b == randomButton && a.name == "Direction")
				{
					return false;
				}
				break;
			default:
				if (a == randomButton && b.name != "Direction")
				{
					return false;
				}
				if (b == randomButton && a.name != "Direction")
				{
					return false;
				}
				break;
			case 0:
				break;
			}
			if (a.myID == 622 && direction == 1 && (b == nameBoxCC || b == favThingBoxCC || b == farmnameBoxCC))
			{
				return false;
			}
		}
		return base.IsAutomaticSnapValid(direction, a, b);
	}

	public override void update(GameTime time)
	{
		base.update(time);
		if (showingCoopHelp)
		{
			backButton.visible = false;
			switch (coopHelpScreen)
			{
			case 0:
				coopHelpRightButton.visible = true;
				coopHelpLeftButton.visible = false;
				break;
			case 1:
				coopHelpRightButton.visible = false;
				coopHelpLeftButton.visible = true;
				break;
			}
		}
		else
		{
			backButton.visible = _shouldShowBackButton;
		}
		if (_sliderOpTarget != null)
		{
			Color col = _sliderOpTarget.getSelectedColor();
			if (_sliderOpTarget.Dirty && _sliderOpTarget.LastColor == col)
			{
				_sliderAction();
				_sliderOpTarget.LastColor = _sliderOpTarget.getSelectedColor();
				_sliderOpTarget.Dirty = false;
				_sliderOpTarget = null;
			}
			else
			{
				_sliderOpTarget.LastColor = col;
			}
		}
		if (advancedCCHighlightTimer > 0f)
		{
			advancedCCHighlightTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
		}
	}

	protected override bool _ShouldAutoSnapPrioritizeAlignedElements()
	{
		return true;
	}
}
