using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData;

namespace StardewValley.Menus;

public class LanguageSelectionMenu : IClickableMenu
{
	public new static int width = 500;

	public new static int height = 728;

	private Texture2D texture;

	protected int _currentPage;

	protected int _pageCount = 1;

	public List<ClickableComponent> languages = new List<ClickableComponent>();

	public Dictionary<string, ModLanguage> modLanguageLookup = new Dictionary<string, ModLanguage>();

	public List<string> languageList = new List<string>();

	public ClickableTextureComponent nextPageButton;

	public ClickableTextureComponent previousPageButton;

	public LanguageSelectionMenu()
	{
		texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\LanguageButtons");
		List<ModLanguage> mod_languages = DataLoader.AdditionalLanguages(Game1.content);
		languageList.Clear();
		modLanguageLookup.Clear();
		languageList.AddRange(new string[12]
		{
			"English", "Russian", "Chinese", "German", "Portuguese", "French", "Spanish", "Japanese", "Korean", "Italian",
			"Turkish", "Hungarian"
		});
		if (mod_languages != null)
		{
			foreach (ModLanguage mod_language in mod_languages)
			{
				languageList.Add("ModLanguage_" + mod_language.Id);
				modLanguageLookup["ModLanguage_" + mod_language.Id] = mod_language;
			}
		}
		_pageCount = (int)Math.Floor((float)(languageList.Count - 1) / 12f) + 1;
		SetupButtons();
	}

	private void SetupButtons()
	{
		Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen((int)((float)width * 2.5f), height);
		languages.Clear();
		int buttonHeight = 83;
		int index = 12 * _currentPage;
		if (index < languageList.Count)
		{
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 64, (int)topLeft.Y + height - 30 - buttonHeight * 6 - 16, width - 128, buttonHeight), languageList[index], null)
			{
				myID = 0,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			index++;
		}
		if (index < languageList.Count)
		{
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 448, (int)topLeft.Y + height - 30 - buttonHeight * 6 - 16, width - 128, buttonHeight), languageList[index], null)
			{
				myID = 3,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			index++;
		}
		if (index < languageList.Count)
		{
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 832, (int)topLeft.Y + height - 30 - buttonHeight * 6 - 16, width - 128, buttonHeight), languageList[index], null)
			{
				myID = 6,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			index++;
		}
		if (index < languageList.Count)
		{
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 64, (int)topLeft.Y + height - 30 - buttonHeight * 5 - 16, width - 128, buttonHeight), languageList[index], null)
			{
				myID = 1,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			index++;
		}
		if (index < languageList.Count)
		{
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 448, (int)topLeft.Y + height - 30 - buttonHeight * 5 - 16, width - 128, buttonHeight), languageList[index], null)
			{
				myID = 4,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			index++;
		}
		if (index < languageList.Count)
		{
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 832, (int)topLeft.Y + height - 30 - buttonHeight * 5 - 16, width - 128, buttonHeight), languageList[index], null)
			{
				myID = 7,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			index++;
		}
		if (index < languageList.Count)
		{
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 64, (int)topLeft.Y + height - 30 - buttonHeight * 4 - 16, width - 128, buttonHeight), languageList[index], null)
			{
				myID = 2,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			index++;
		}
		if (index < languageList.Count)
		{
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 448, (int)topLeft.Y + height - 30 - buttonHeight * 4 - 16, width - 128, buttonHeight), languageList[index], null)
			{
				myID = 5,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			index++;
		}
		if (index < languageList.Count)
		{
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 832, (int)topLeft.Y + height - 30 - buttonHeight * 4 - 16, width - 128, buttonHeight), languageList[index], null)
			{
				myID = 8,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			index++;
		}
		if (index < languageList.Count)
		{
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 64, (int)topLeft.Y + height - 30 - buttonHeight * 3 - 16, width - 128, buttonHeight), languageList[index], null)
			{
				myID = 9,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			index++;
		}
		if (index < languageList.Count)
		{
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 448, (int)topLeft.Y + height - 30 - buttonHeight * 3 - 16, width - 128, buttonHeight), languageList[index], null)
			{
				myID = 10,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			index++;
		}
		if (index < languageList.Count)
		{
			languages.Add(new ClickableComponent(new Rectangle((int)topLeft.X + 832, (int)topLeft.Y + height - 30 - buttonHeight * 3 - 16, width - 128, buttonHeight), languageList[index], null)
			{
				myID = 11,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			});
			index++;
		}
		previousPageButton = null;
		if (_currentPage > 0)
		{
			previousPageButton = new ClickableTextureComponent(new Rectangle((int)topLeft.X + 4, (int)topLeft.Y + height / 2 - 25, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
			{
				myID = 554,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			};
		}
		nextPageButton = null;
		if (_currentPage < _pageCount - 1)
		{
			nextPageButton = new ClickableTextureComponent(new Rectangle((int)(topLeft.X + (float)width * 2.5f) - 32, (int)topLeft.Y + height / 2 - 25, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
			{
				myID = 555,
				downNeighborID = -99998,
				leftNeighborID = -99998,
				rightNeighborID = -99998,
				upNeighborID = -99998
			};
		}
		if (Game1.options.SnappyMenus)
		{
			int id = ((currentlySnappedComponent != null) ? currentlySnappedComponent.myID : 0);
			populateClickableComponentList();
			currentlySnappedComponent = getComponentWithID(id);
			snapCursorToCurrentSnappedComponent();
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		currentlySnappedComponent = getComponentWithID(0);
		snapCursorToCurrentSnappedComponent();
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		base.receiveLeftClick(x, y, playSound);
		if (nextPageButton != null && nextPageButton.containsPoint(x, y))
		{
			Game1.playSound("shwip");
			_currentPage++;
			SetupButtons();
			return;
		}
		if (previousPageButton != null && previousPageButton.containsPoint(x, y))
		{
			Game1.playSound("shwip");
			_currentPage--;
			SetupButtons();
			return;
		}
		foreach (ClickableComponent component in languages)
		{
			if (!component.containsPoint(x, y))
			{
				continue;
			}
			Game1.playSound("select");
			bool changed_language = true;
			if (component.name.StartsWith("ModLanguage_"))
			{
				LocalizedContentManager.SetModLanguage(modLanguageLookup[component.name]);
			}
			else
			{
				switch (component.name)
				{
				case "English":
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.en;
					break;
				case "German":
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.de;
					break;
				case "Russian":
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.ru;
					break;
				case "Chinese":
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.zh;
					break;
				case "Japanese":
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.ja;
					break;
				case "Spanish":
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.es;
					break;
				case "Portuguese":
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.pt;
					break;
				case "French":
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.fr;
					break;
				case "Korean":
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.ko;
					break;
				case "Italian":
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.it;
					break;
				case "Turkish":
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.tr;
					break;
				case "Hungarian":
					LocalizedContentManager.CurrentLanguageCode = LocalizedContentManager.LanguageCode.hu;
					break;
				default:
					changed_language = false;
					break;
				}
			}
			if (Game1.options.SnappyMenus)
			{
				Game1.activeClickableMenu.setCurrentlySnappedComponentTo(81118);
				Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
			}
			if (changed_language)
			{
				ApplyLanguageChange();
				exitThisMenu();
			}
		}
		isWithinBounds(x, y);
	}

	public virtual void ApplyLanguageChange()
	{
	}

	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		foreach (ClickableComponent component in languages)
		{
			if (component.containsPoint(x, y))
			{
				if (component.label == null)
				{
					Game1.playSound("Cowboy_Footstep");
					component.label = "hovered";
				}
			}
			else
			{
				component.label = null;
			}
		}
		previousPageButton?.tryHover(x, y);
		nextPageButton?.tryHover(x, y);
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
	}

	public override void draw(SpriteBatch b)
	{
		Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen((int)((float)width * 2.5f), height);
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
		}
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(473, 36, 24, 24), (int)topLeft.X + 32, (int)topLeft.Y + 156, (int)((float)width * 2.55f) - 64, height / 2 + 25, Color.White, 4f);
		foreach (ClickableComponent c in languages)
		{
			int i = 0;
			switch (c.name)
			{
			case "English":
				i = 0;
				break;
			case "Spanish":
				i = 1;
				break;
			case "Portuguese":
				i = 2;
				break;
			case "Russian":
				i = 3;
				break;
			case "Chinese":
				i = 4;
				break;
			case "Japanese":
				i = 5;
				break;
			case "German":
				i = 6;
				break;
			case "French":
				i = 7;
				break;
			case "Korean":
				i = 8;
				break;
			case "Italian":
				i = 10;
				break;
			case "Turkish":
				i = 9;
				break;
			case "Hungarian":
				i = 11;
				break;
			}
			if (modLanguageLookup.TryGetValue(c.name, out var languageData))
			{
				Texture2D mod_button_texture = Game1.temporaryContent.Load<Texture2D>(languageData.ButtonTexture);
				Rectangle source_rect = new Rectangle(0, 0, 174, 39);
				if (c.label != null)
				{
					source_rect.Y += 39;
				}
				b.Draw(mod_button_texture, c.bounds, source_rect, Color.White, 0f, new Vector2(0f, 0f), SpriteEffects.None, 0f);
			}
			else
			{
				int buttonSourceY = ((i <= 6) ? (i * 78) : ((i - 7) * 78));
				buttonSourceY += ((c.label != null) ? 39 : 0);
				int buttonSourceX = ((i > 6) ? 174 : 0);
				b.Draw(texture, c.bounds, new Rectangle(buttonSourceX, buttonSourceY, 174, 40), Color.White, 0f, new Vector2(0f, 0f), SpriteEffects.None, 0f);
			}
		}
		previousPageButton?.draw(b);
		nextPageButton?.draw(b);
		if (Game1.activeClickableMenu == this)
		{
			drawMouse(b);
		}
	}

	public override bool readyToClose()
	{
		return true;
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		SetupButtons();
	}
}
