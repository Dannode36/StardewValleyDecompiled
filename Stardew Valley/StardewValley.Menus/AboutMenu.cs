using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

public class AboutMenu : IClickableMenu
{
	public const int region_upArrow = 94444;

	public const int region_downArrow = 95555;

	public new const int height = 700;

	public ClickableComponent backButton;

	public ClickableTextureComponent upButton;

	public ClickableTextureComponent downButton;

	public List<ICreditsBlock> credits = new List<ICreditsBlock>();

	private int currentCreditsIndex;

	public AboutMenu()
	{
		width = 1280;
		base.height = 700;
		SetUpCredits();
		if (Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			populateClickableComponentList();
			snapToDefaultClickableComponent();
		}
	}

	public void SetUpCredits()
	{
		foreach (string s in Game1.temporaryContent.Load<List<string>>("Strings\\credits"))
		{
			if (s != null && s.Length >= 6 && s.StartsWith("[image"))
			{
				string[] split = ArgUtility.SplitBySpace(s);
				string path = split[1];
				int sourceX = Convert.ToInt32(split[2]);
				int sourceY = Convert.ToInt32(split[3]);
				int sourceWidth = Convert.ToInt32(split[4]);
				int sourceHeight = Convert.ToInt32(split[5]);
				int zoom = Convert.ToInt32(split[6]);
				int animationFrames = ((split.Length <= 7) ? 1 : Convert.ToInt32(split[7]));
				Texture2D tex = null;
				try
				{
					tex = Game1.temporaryContent.Load<Texture2D>(path);
				}
				catch (Exception)
				{
				}
				if (tex != null)
				{
					if (sourceWidth == -1)
					{
						sourceWidth = tex.Width;
						sourceHeight = tex.Height;
					}
					credits.Add(new ImageCreditsBlock(tex, new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), zoom, animationFrames));
				}
			}
			else if (s != null && s.Length >= 6 && s.StartsWith("[link"))
			{
				string[] array = ArgUtility.SplitBySpace(s, 3);
				string url = array[1];
				string text = array[2];
				credits.Add(new LinkCreditsBlock(text, url));
			}
			else
			{
				credits.Add(new TextCreditsBlock(s));
			}
		}
		Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(width, 700);
		xPositionOnScreen = (int)topLeft.X;
		yPositionOnScreen = (int)topLeft.Y;
		upButton = new ClickableTextureComponent(new Rectangle((int)topLeft.X + width - 80, (int)topLeft.Y + 64 + 16, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12), 0.8f)
		{
			myID = 94444,
			downNeighborID = 95555,
			rightNeighborID = -99998,
			leftNeighborID = -99998
		};
		downButton = new ClickableTextureComponent(new Rectangle((int)topLeft.X + width - 80, (int)topLeft.Y + 700 - 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11), 0.8f)
		{
			myID = 95555,
			upNeighborID = -99998,
			rightNeighborID = -99998,
			leftNeighborID = -99998
		};
		backButton = new ClickableComponent(new Rectangle(Game1.uiViewport.Width + -198 - 48, Game1.uiViewport.Height - 81 - 24, 198, 81), "")
		{
			myID = 81114,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			upNeighborID = 95555
		};
	}

	public override void snapToDefaultClickableComponent()
	{
		currentlySnappedComponent = getComponentWithID(81114);
		snapCursorToCurrentSnappedComponent();
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		base.receiveLeftClick(x, y, playSound);
		if (upButton.containsPoint(x, y))
		{
			if (currentCreditsIndex > 0)
			{
				currentCreditsIndex--;
				Game1.playSound("shiny4");
				upButton.scale = upButton.baseScale;
			}
		}
		else if (downButton.containsPoint(x, y))
		{
			if (currentCreditsIndex < credits.Count - 1)
			{
				currentCreditsIndex++;
				Game1.playSound("shiny4");
				downButton.scale = downButton.baseScale;
			}
		}
		else
		{
			if (!isWithinBounds(x, y))
			{
				return;
			}
			int yPos = yPositionOnScreen + 96;
			int oldYpos = yPos;
			int i = 0;
			while (yPos < yPositionOnScreen + 700 - 64 && credits.Count > currentCreditsIndex + i)
			{
				yPos += credits[currentCreditsIndex + i].getHeight(width - 64) + ((credits.Count <= currentCreditsIndex + i + 1 || !(credits[currentCreditsIndex + i + 1] is ImageCreditsBlock)) ? 8 : 0);
				if (y >= oldYpos && y < yPos)
				{
					credits[currentCreditsIndex + i].clicked();
					break;
				}
				i++;
				oldYpos = yPos;
			}
		}
	}

	public override void update(GameTime time)
	{
		base.update(time);
		upButton.visible = currentCreditsIndex > 0;
		downButton.visible = currentCreditsIndex < credits.Count - 1;
	}

	public override void receiveScrollWheelAction(int direction)
	{
		if (direction > 0 && currentCreditsIndex > 0)
		{
			currentCreditsIndex--;
			Game1.playSound("shiny4");
		}
		else if (direction < 0 && currentCreditsIndex < credits.Count - 1)
		{
			currentCreditsIndex++;
			Game1.playSound("shiny4");
		}
	}

	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		upButton.tryHover(x, y);
		downButton.tryHover(x, y);
		if (!isWithinBounds(x, y))
		{
			return;
		}
		int yPos = yPositionOnScreen + 96;
		int oldYpos = yPos;
		int i = 0;
		while (yPos < yPositionOnScreen + 700 - 64 && credits.Count > currentCreditsIndex + i)
		{
			yPos += credits[currentCreditsIndex + i].getHeight(width - 64) + ((credits.Count <= currentCreditsIndex + i + 1 || !(credits[currentCreditsIndex + i + 1] is ImageCreditsBlock)) ? 8 : 0);
			if (y >= oldYpos && y < yPos)
			{
				credits[currentCreditsIndex + i].hovered();
				break;
			}
			i++;
			oldYpos = yPos;
		}
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
	}

	public override void draw(SpriteBatch b)
	{
		Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(width, 600);
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
		}
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(473, 36, 24, 24), (int)topLeft.X, (int)topLeft.Y, width, 700, Color.White, 4f, drawShadow: false);
		int yPos = yPositionOnScreen + 96;
		int i = 0;
		while (yPos < yPositionOnScreen + 700 - 64 && credits.Count > currentCreditsIndex + i)
		{
			credits[currentCreditsIndex + i].draw(xPositionOnScreen + 32, yPos, width - 64, b);
			yPos += credits[currentCreditsIndex + i].getHeight(width - 64) + ((credits.Count <= currentCreditsIndex + i + 1 || !(credits[currentCreditsIndex + i + 1] is ImageCreditsBlock)) ? 8 : 0);
			i++;
		}
		if (currentCreditsIndex > 0)
		{
			upButton.draw(b);
		}
		if (currentCreditsIndex < credits.Count - 1)
		{
			downButton.draw(b);
		}
		string versionText = "v" + Game1.GetVersionString();
		float versionTextHeight = Game1.smallFont.MeasureString(versionText).Y;
		b.DrawString(Game1.smallFont, versionText, new Vector2(16f, (float)Game1.uiViewport.Height - versionTextHeight - 8f), Color.White);
		if (Game1.activeClickableMenu is TitleMenu titleMenu && !string.IsNullOrWhiteSpace(titleMenu.startupMessage))
		{
			string tipText = Game1.parseText(titleMenu.startupMessage, Game1.smallFont, 640);
			float tipHeight = Game1.smallFont.MeasureString(tipText).Y;
			b.DrawString(Game1.smallFont, tipText, new Vector2(8f, (float)Game1.uiViewport.Height - versionTextHeight - tipHeight - 4f), Color.White);
		}
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		SetUpCredits();
		if (Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			int id = ((currentlySnappedComponent != null) ? currentlySnappedComponent.myID : 81114);
			populateClickableComponentList();
			currentlySnappedComponent = getComponentWithID(id);
			snapCursorToCurrentSnappedComponent();
		}
	}
}
