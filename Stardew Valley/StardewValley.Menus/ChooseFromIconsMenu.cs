using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Tools;

namespace StardewValley.Menus;

public class ChooseFromIconsMenu : IClickableMenu
{
	private Rectangle iconBackRectangle;

	private Texture2D texture;

	private Point iconBackHighlightPosition;

	private Point iconFrontHighlightPositionOffset;

	private string which;

	public List<ClickableTextureComponent> icons = new List<ClickableTextureComponent>();

	public List<ClickableTextureComponent> iconFronts = new List<ClickableTextureComponent>();

	private int iconXOffset;

	private int maxTooltipHeight;

	private int maxTooltipWidth;

	private float destroyTimer = -1f;

	private List<TemporaryAnimatedSprite> temporarySprites = new List<TemporaryAnimatedSprite>();

	public Object sourceObject;

	private bool hasTooltips = true;

	private string title;

	private string hoverSound;

	private int titleStyle = 3;

	private int selected = -1;

	public ChooseFromIconsMenu(string which)
	{
		setUpIcons(which);
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		setUpIcons(which);
	}

	public void setUpIcons(string which)
	{
		int iconSpacing = 32;
		int iconOffsetXMargin = 12;
		int iconOffsetYMargin = 4;
		this.which = which;
		title = Game1.content.LoadString("Strings\\1_6_Strings:ChooseOne");
		hoverSound = "boulderCrack";
		icons.Clear();
		iconFronts.Clear();
		if (!(which == "dwarfStatue"))
		{
			if (which == "bobbers")
			{
				if (Game1.player.usingRandomizedBobber)
				{
					Game1.player.bobberStyle.Value = -2;
				}
				int available = Game1.player.fishCaught.Count() / 2;
				iconSpacing = 4;
				iconBackRectangle = new Rectangle(222, 317, 16, 16);
				iconBackHighlightPosition = new Point(256, 317);
				texture = Game1.mouseCursors_1_6;
				for (int i = 0; i < FishingRod.NUM_BOBBER_STYLES; i++)
				{
					bool num = i > available;
					Rectangle src = Game1.getSourceRectForStandardTileSheet(Game1.bobbersTexture, i, 16, 32);
					src.Height = 16;
					icons.Add(new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), texture, iconBackRectangle, 4f, drawShadow: true)
					{
						name = (i.ToString() ?? "")
					});
					if (num)
					{
						iconFronts.Add(new ClickableTextureComponent(new Rectangle(0, 0, 16, 16), Game1.mouseCursors_1_6, new Rectangle(272, 317, 16, 16), 4f)
						{
							name = "ghosted"
						});
					}
					else
					{
						iconFronts.Add(new ClickableTextureComponent(new Rectangle(0, 0, 16, 16), Game1.bobbersTexture, src, 4f, drawShadow: true));
					}
				}
				icons.Add(new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), null, new Rectangle(0, 0, 0, 0), 4f, drawShadow: true)
				{
					name = "-2"
				});
				iconFronts.Add(new ClickableTextureComponent(new Rectangle(0, 0, 10, 10), Game1.mouseCursors_1_6, new Rectangle(496, 28, 16, 16), 4f, drawShadow: true));
				selected = Game1.player.bobberStyle.Value;
				iconOffsetXMargin = 0;
				iconOffsetYMargin = 0;
				hasTooltips = false;
				title = Game1.content.LoadString("Strings\\1_6_Strings:ChooseBobber");
				titleStyle = 0;
				hoverSound = null;
			}
		}
		else
		{
			Game1.playSound("stone_button");
			iconBackRectangle = new Rectangle(127, 123, 21, 21);
			iconBackHighlightPosition = new Point(127, 144);
			iconFrontHighlightPositionOffset = new Point(0, 17);
			texture = Game1.mouseCursors_1_6;
			Random dwarf_random = Utility.CreateRandom(Game1.stats.DaysPlayed * 77, Game1.uniqueIDForThisGame);
			int icon1 = dwarf_random.Next(5);
			int icon2 = -1;
			do
			{
				icon2 = dwarf_random.Next(5);
			}
			while (icon2 == icon1);
			icons.Add(new ClickableTextureComponent(new Rectangle(0, 0, 84, 84), texture, iconBackRectangle, 4f, drawShadow: true)
			{
				name = (icon1.ToString() ?? ""),
				hoverText = Game1.content.LoadString("Strings\\1_6_Strings:DwarfStatue_" + icon1)
			});
			icons.Add(new ClickableTextureComponent(new Rectangle(0, 0, 84, 84), texture, iconBackRectangle, 4f, drawShadow: true)
			{
				name = (icon2.ToString() ?? ""),
				hoverText = Game1.content.LoadString("Strings\\1_6_Strings:DwarfStatue_" + icon2)
			});
			iconFronts.Add(new ClickableTextureComponent(new Rectangle(0, 0, 17, 17), texture, new Rectangle(148 + icon1 * 17, 123, 17, 17), 4f));
			iconFronts.Add(new ClickableTextureComponent(new Rectangle(0, 0, 17, 17), texture, new Rectangle(148 + icon2 * 17, 123, 17, 17), 4f));
		}
		int toolTipWidth = (hasTooltips ? 240 : 0);
		int iconWidth = Math.Max(iconBackRectangle.Width * 4, toolTipWidth) + iconSpacing;
		iconXOffset = iconWidth / 2 - iconBackRectangle.Width * 4 / 2 - 4;
		width = Math.Max(800, Game1.uiViewport.Width / 3);
		xPositionOnScreen = Game1.uiViewport.Width / 2 - width / 2;
		height = 100;
		maxTooltipHeight = 0;
		maxTooltipWidth = 0;
		if (hasTooltips)
		{
			foreach (ClickableTextureComponent i in icons)
			{
				i.hoverText = Game1.parseText(i.hoverText, Game1.smallFont, toolTipWidth - 32);
				maxTooltipHeight = Math.Max(maxTooltipHeight, (int)Game1.smallFont.MeasureString(i.hoverText).Y);
				maxTooltipWidth = Math.Max(maxTooltipWidth, (int)Game1.smallFont.MeasureString(i.hoverText).X);
			}
			maxTooltipHeight += 48;
			maxTooltipWidth += 48;
		}
		height += (icons.Count * iconWidth / width + 1) * (maxTooltipHeight + icons[0].bounds.Height + iconSpacing);
		int maxIconsPerRow = width / iconWidth;
		yPositionOnScreen = Game1.uiViewport.Height / 2 - height / 2;
		int y = yPositionOnScreen + 100;
		for (int i = 0; i < icons.Count; i += maxIconsPerRow)
		{
			int rowCount = Math.Min(icons.Count - i, maxIconsPerRow);
			int x = xPositionOnScreen + width / 2 - rowCount * iconWidth / 2;
			for (int j = 0; j < rowCount; j++)
			{
				int index = j + i;
				icons[index].bounds.X = x + j * iconWidth;
				icons[index].bounds.Y = y;
				icons[index].bounds.Width = iconWidth;
				icons[index].bounds.Height += maxTooltipHeight;
				iconFronts[index].bounds.X = icons[index].bounds.X + iconOffsetXMargin;
				iconFronts[index].bounds.Y = icons[index].bounds.Y + iconOffsetYMargin;
				icons[index].myID = index;
				icons[index].leftNeighborID = index - 1;
				icons[index].rightNeighborID = index + 1;
				icons[index].downNeighborID = index + rowCount;
				icons[index].upNeighborID = index - rowCount;
			}
			y += maxTooltipHeight + icons[0].bounds.Height + iconSpacing;
		}
		initialize(xPositionOnScreen, yPositionOnScreen, width, height, showUpperRightCloseButton: true);
		if (Game1.options.SnappyMenus)
		{
			populateClickableComponentList();
			currentlySnappedComponent = getComponentWithID(0);
			snapCursorToCurrentSnappedComponent();
		}
	}

	public override void update(GameTime time)
	{
		base.update(time);
		if (destroyTimer > 0f)
		{
			destroyTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			if (destroyTimer <= 0f)
			{
				flairOnDestroy();
				Game1.activeClickableMenu = null;
			}
		}
		for (int i = temporarySprites.Count - 1; i >= 0; i--)
		{
			if (temporarySprites[i].update(time))
			{
				temporarySprites.RemoveAt(i);
			}
		}
	}

	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		for (int i = 0; i < icons.Count; i++)
		{
			ClickableTextureComponent c = icons[i];
			iconFronts[i].sourceRect = iconFronts[i].startingSourceRect;
			if (c.containsPoint(x, y) && destroyTimer == -1f)
			{
				if (c.sourceRect == c.startingSourceRect && hoverSound != null)
				{
					Game1.playSound(hoverSound);
				}
				c.sourceRect.Location = iconBackHighlightPosition;
				iconFronts[i].sourceRect.Location = new Point(iconFronts[i].sourceRect.Location.X + iconFrontHighlightPositionOffset.X, iconFronts[i].sourceRect.Location.Y + iconFrontHighlightPositionOffset.Y);
			}
			else
			{
				c.sourceRect = iconBackRectangle;
			}
		}
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (destroyTimer >= 0f)
		{
			return;
		}
		base.receiveLeftClick(x, y, playSound);
		for (int i = 0; i < icons.Count; i++)
		{
			ClickableTextureComponent c = icons[i];
			if (!c.containsPoint(x, y))
			{
				continue;
			}
			bool ghosted = iconFronts[i].name.Contains("ghosted");
			string text = which;
			if (!(text == "dwarfStatue"))
			{
				if (text == "bobbers")
				{
					if (ghosted)
					{
						Game1.playSound("smallSelect");
						break;
					}
					int selection = Convert.ToInt32(c.name);
					if (Game1.player.bobberStyle.Value != selection)
					{
						Game1.playSound("button1");
						hoverSound = null;
						Game1.player.bobberStyle.Value = Convert.ToInt32(c.name);
						selected = Game1.player.bobberStyle.Value;
						if (selected == -2)
						{
							Game1.player.usingRandomizedBobber = true;
						}
						else
						{
							Game1.player.usingRandomizedBobber = false;
						}
					}
				}
			}
			else
			{
				Game1.playSound("button_tap");
				DelayedAction.playSoundAfterDelay("button_tap", 70);
				DelayedAction.playSoundAfterDelay("discoverMineral", 750);
				for (int j = 0; j < 16; j++)
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(98 + Game1.random.Next(3) * 4, 161, 4, 4), Utility.getRandomPositionInThisRectangle(c.bounds, Game1.random), flipped: false, 0f, Color.White)
					{
						local = true,
						scale = 4f,
						interval = 9999f,
						motion = new Vector2((float)Game1.random.Next(-15, 16) / 10f, -7f + (float)Game1.random.Next(-10, 11) / 10f),
						acceleration = new Vector2(0f, 0.5f)
					});
				}
				destroyTimer = 800f;
			}
			doIconAction(c.name);
		}
	}

	private void doIconAction(string iconName)
	{
		if (which == "dwarfStatue" && !Game1.player.hasBuffWithNameContainingString("dwarfStatue"))
		{
			Game1.player.applyBuff(which + "_" + iconName);
		}
	}

	private void flairOnDestroy()
	{
		if (which == "dwarfStatue")
		{
			sourceObject.shakeTimer = 500;
			if (sourceObject.Location != null)
			{
				Utility.addSprinklesToLocation(sourceObject.Location, (int)sourceObject.TileLocation.X, (int)sourceObject.TileLocation.Y, 3, 4, 800, 40, Color.White);
			}
		}
	}

	public override void draw(SpriteBatch b)
	{
		b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.7f);
		base.draw(b);
		SpriteText.drawStringWithScrollCenteredAt(b, title, xPositionOnScreen + width / 2, yPositionOnScreen + 20, "", 1f, (titleStyle == 3) ? Color.LightGray : Game1.textColor, titleStyle);
		for (int i = 0; i < icons.Count; i++)
		{
			if (selected == i || (selected == -2 && i == icons.Count - 1))
			{
				if (selected == i)
				{
					Rectangle rect = icons[i].bounds;
					rect.Inflate(2, 4);
					rect.X += iconXOffset - 2;
					b.Draw(Game1.staminaRect, rect, Color.Red);
					if (icons[i].sourceRect.Width > 0)
					{
						icons[i].sourceRect.X = iconBackHighlightPosition.X;
						icons[i].sourceRect.Y = iconBackHighlightPosition.Y;
					}
				}
				else
				{
					b.Draw(Game1.mouseCursors_1_6, icons[i].getVector2(), new Rectangle(480, 28, 16, 16), Color.Red, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
			}
			icons[i].draw(b, Color.White, 0f, 0, iconXOffset);
			iconFronts[i].draw(b, iconFronts[i].name.Equals("ghosted_fade") ? (Color.Black * 0.4f) : Color.White, 0.87f, 0, iconXOffset);
			IClickableMenu.drawHoverText(b, icons[i].hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, icons[i].bounds.X + 4, icons[i].bounds.Y + icons[i].bounds.Height - maxTooltipHeight + 4, 1f, null, null, Game1.mouseCursors_1_6, (icons[i].sourceRect != icons[i].startingSourceRect) ? new Rectangle(111, 145, 15, 15) : new Rectangle(96, 145, 15, 15), Color.White, new Color(26, 26, 43), 4f, maxTooltipWidth, maxTooltipHeight);
		}
		foreach (TemporaryAnimatedSprite temporarySprite in temporarySprites)
		{
			temporarySprite.draw(b);
		}
		drawMouse(b);
	}
}
