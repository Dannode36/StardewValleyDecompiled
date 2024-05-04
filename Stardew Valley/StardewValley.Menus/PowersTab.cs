using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Powers;
using StardewValley.TokenizableStrings;

namespace StardewValley.Menus;

public class PowersTab : IClickableMenu
{
	public const int region_forwardButton = 707;

	public const int region_backButton = 706;

	public const int distanceFromMenuBottomBeforeNewPage = 128;

	public int currentPage;

	private string descriptionText = "";

	private string hoverText = "";

	public ClickableTextureComponent backButton;

	public ClickableTextureComponent forwardButton;

	public List<List<ClickableTextureComponent>> powers;

	public PowersTab(int x, int y, int width, int height)
		: base(x, y, width, height)
	{
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
	}

	public override void snapToDefaultClickableComponent()
	{
		base.snapToDefaultClickableComponent();
		currentlySnappedComponent = getComponentWithID(0);
		snapCursorToCurrentSnappedComponent();
	}

	public override void populateClickableComponentList()
	{
		if (powers == null)
		{
			powers = new List<List<ClickableTextureComponent>>();
			Dictionary<string, PowersData> powersData = null;
			try
			{
				powersData = DataLoader.Powers(Game1.content);
			}
			catch (Exception)
			{
			}
			if (powersData != null)
			{
				int collectionWidth = 9;
				int widthUsed = 0;
				int baseX = xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
				int baseY = yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16;
				foreach (KeyValuePair<string, PowersData> power in powersData)
				{
					int xPos = baseX + widthUsed % collectionWidth * 76;
					int yPos = baseY + widthUsed / collectionWidth * 76;
					bool unlocked = GameStateQuery.CheckConditions(power.Value.UnlockedCondition);
					string name = TokenParser.ParseText(power.Value.DisplayName);
					string description = TokenParser.ParseText(power.Value.Description);
					Texture2D texture = Game1.content.Load<Texture2D>(power.Value.TexturePath);
					if (powers.Count == 0 || yPos > yPositionOnScreen + height - 128)
					{
						powers.Add(new List<ClickableTextureComponent>());
						widthUsed = 0;
						xPos = baseX;
						yPos = baseY;
					}
					List<ClickableTextureComponent> list = powers.Last();
					list.Add(new ClickableTextureComponent(name, new Rectangle(xPos, yPos, 64, 64), null, description, texture, new Rectangle(power.Value.TexturePosition.X, power.Value.TexturePosition.Y, 16, 16), 4f, unlocked)
					{
						myID = list.Count,
						rightNeighborID = (((list.Count + 1) % collectionWidth == 0) ? (-1) : (list.Count + 1)),
						leftNeighborID = ((list.Count % collectionWidth == 0) ? (-1) : (list.Count - 1)),
						downNeighborID = ((yPos + 76 > yPositionOnScreen + height - 128) ? (-7777) : (list.Count + collectionWidth)),
						upNeighborID = ((list.Count < collectionWidth) ? 12346 : (list.Count - collectionWidth)),
						fullyImmutable = true
					});
					widthUsed++;
				}
			}
		}
		base.populateClickableComponentList();
	}

	public override void performHoverAction(int x, int y)
	{
		hoverText = "";
		descriptionText = "";
		base.performHoverAction(x, y);
		foreach (ClickableTextureComponent c in powers[currentPage])
		{
			if (c.containsPoint(x, y))
			{
				c.scale = Math.Min(c.scale + 0.02f, c.baseScale + 0.1f);
				hoverText = (c.drawShadow ? c.name : "???");
				descriptionText = Game1.parseText(c.hoverText, Game1.smallFont, Math.Max((int)Game1.dialogueFont.MeasureString(hoverText).X, 320));
			}
			else
			{
				c.scale = Math.Max(c.scale - 0.02f, c.baseScale);
			}
		}
		forwardButton.tryHover(x, y, 0.5f);
		backButton.tryHover(x, y, 0.5f);
	}

	public override void draw(SpriteBatch b)
	{
		if (currentPage > 0)
		{
			backButton.draw(b);
		}
		if (currentPage < powers.Count - 1)
		{
			forwardButton.draw(b);
		}
		b.End();
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		foreach (ClickableTextureComponent item in powers[currentPage])
		{
			bool drawColor = item.drawShadow;
			item.draw(b, drawColor ? Color.White : (Color.Black * 0.2f), 0.86f);
		}
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (!descriptionText.Equals("") && hoverText != "???")
		{
			IClickableMenu.drawHoverText(b, descriptionText, Game1.smallFont, 0, 0, -1, hoverText);
		}
		else if (!hoverText.Equals(""))
		{
			IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
		}
	}
}
