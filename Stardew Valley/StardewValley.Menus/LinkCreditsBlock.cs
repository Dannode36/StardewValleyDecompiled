using System;
using System.Diagnostics;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus;

internal class LinkCreditsBlock : ICreditsBlock
{
	private string text;

	private string url;

	private bool currentlyHovered;

	public LinkCreditsBlock(string text, string url)
	{
		this.text = text;
		this.url = url;
	}

	public override void draw(int topLeftX, int topLeftY, int widthToOccupy, SpriteBatch b)
	{
		SpriteText.drawString(b, text, topLeftX, topLeftY, 999999, widthToOccupy, 99999, 1f, 0.88f, junimoText: false, -1, "", currentlyHovered ? SpriteText.color_Green : SpriteText.color_Cyan);
		currentlyHovered = false;
	}

	public override int getHeight(int maxWidth)
	{
		if (!(text == ""))
		{
			return SpriteText.getHeightOfString(text, maxWidth);
		}
		return 64;
	}

	public override void hovered()
	{
		currentlyHovered = true;
	}

	public override void clicked()
	{
		Game1.playSound("bigSelect");
		try
		{
			Process.Start(url);
		}
		catch (Exception)
		{
		}
	}
}
