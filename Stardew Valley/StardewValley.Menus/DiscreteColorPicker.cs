using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;

namespace StardewValley.Menus;

public class DiscreteColorPicker : IClickableMenu
{
	public const int sizeOfEachSwatch = 7;

	public Item itemToDrawColored;

	public bool visible = true;

	public static int totalColors = 21;

	public int colorSelection;

	public DiscreteColorPicker(int xPosition, int yPosition, int startingColor = 0, Item itemToDrawColored = null)
	{
		xPositionOnScreen = xPosition;
		yPositionOnScreen = yPosition;
		width = totalColors * 9 * 4 + IClickableMenu.borderWidth;
		height = 28 + IClickableMenu.borderWidth;
		colorSelection = ((startingColor != 0 && getColorFromSelection(startingColor) != Color.Black) ? startingColor : 0);
		this.itemToDrawColored = itemToDrawColored;
		if (this.itemToDrawColored is Chest chest)
		{
			chest.resetLidFrame();
		}
		visible = Game1.player.showChestColorPicker;
	}

	public DiscreteColorPicker(int xPosition, int yPosition, Color startingColor, Item itemToDrawColored = null)
		: this(xPosition, yPosition, getSelectionFromColor(startingColor), itemToDrawColored)
	{
	}

	public static int getSelectionFromColor(Color c)
	{
		for (int i = 0; i < totalColors; i++)
		{
			if (getColorFromSelection(i).Equals(c))
			{
				return i;
			}
		}
		return -1;
	}

	public Color getCurrentColor()
	{
		return getColorFromSelection(colorSelection);
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (!visible)
		{
			return;
		}
		base.receiveLeftClick(x, y, playSound);
		Rectangle area = new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth / 2, yPositionOnScreen + IClickableMenu.borderWidth / 2, 36 * totalColors, 28);
		if (area.Contains(x, y))
		{
			colorSelection = (x - area.X) / 36;
			try
			{
				Game1.playSound("coin");
			}
			catch
			{
			}
			if (itemToDrawColored is Chest chest)
			{
				chest.playerChoiceColor.Value = getColorFromSelection(colorSelection);
				chest.resetLidFrame();
			}
		}
	}

	public static Color getColorFromSelection(int selection)
	{
		return selection switch
		{
			1 => new Color(85, 85, 255), 
			2 => new Color(119, 191, 255), 
			3 => new Color(0, 170, 170), 
			4 => new Color(0, 234, 175), 
			5 => new Color(0, 170, 0), 
			6 => new Color(159, 236, 0), 
			7 => new Color(255, 234, 18), 
			8 => new Color(255, 167, 18), 
			9 => new Color(255, 105, 18), 
			10 => new Color(255, 0, 0), 
			11 => new Color(135, 0, 35), 
			12 => new Color(255, 173, 199), 
			13 => new Color(255, 117, 195), 
			14 => new Color(172, 0, 198), 
			15 => new Color(143, 0, 255), 
			16 => new Color(89, 11, 142), 
			17 => new Color(64, 64, 64), 
			18 => new Color(100, 100, 100), 
			19 => new Color(200, 200, 200), 
			20 => new Color(254, 254, 254), 
			_ => Color.Black, 
		};
	}

	public override void draw(SpriteBatch b)
	{
		if (!visible)
		{
			return;
		}
		IClickableMenu.drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.LightGray);
		for (int i = 0; i < totalColors; i++)
		{
			if (i == 0)
			{
				b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth / 2, yPositionOnScreen + IClickableMenu.borderWidth / 2), new Rectangle(295, 503, 7, 7), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			}
			else
			{
				b.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth / 2 + i * 9 * 4, yPositionOnScreen + IClickableMenu.borderWidth / 2, 28, 28), getColorFromSelection(i));
			}
			if (i == colorSelection)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), xPositionOnScreen + IClickableMenu.borderWidth / 2 - 4 + i * 9 * 4, yPositionOnScreen + IClickableMenu.borderWidth / 2 - 4, 36, 36, Color.Black, 4f, drawShadow: false);
			}
		}
		if (itemToDrawColored is Chest chest)
		{
			chest.draw(b, xPositionOnScreen + width + IClickableMenu.borderWidth / 2, yPositionOnScreen + 16, 1f, local: true);
		}
	}
}
