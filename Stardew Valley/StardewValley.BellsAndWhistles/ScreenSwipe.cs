using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.BellsAndWhistles;

public class ScreenSwipe
{
	public const int swipe_bundleComplete = 0;

	public const int swipe_raccoon = 1;

	public const int borderPixelWidth = 7;

	private Rectangle bgSource;

	private Rectangle flairSource;

	private Rectangle messageSource;

	private Rectangle movingFlairSource;

	private Rectangle bgDest;

	private int yPosition;

	private int durationAfterSwipe;

	private int originalBGSourceXLimit;

	private List<Vector2> flairPositions = new List<Vector2>();

	private Vector2 messagePosition;

	private Vector2 movingFlairPosition;

	private Vector2 movingFlairMotion;

	private float swipeVelocity;

	private Texture2D texture;

	public ScreenSwipe(int which, float swipeVelocity = -1f, int durationAfterSwipe = -1)
	{
		Game1.playSound("throw");
		if (swipeVelocity == -1f)
		{
			swipeVelocity = 5f;
		}
		if (durationAfterSwipe == -1)
		{
			durationAfterSwipe = 2700;
		}
		this.swipeVelocity = swipeVelocity;
		this.durationAfterSwipe = durationAfterSwipe;
		Vector2 screenCenter = new Vector2(Game1.uiViewport.Width / 2, Game1.uiViewport.Height / 2);
		if (which == 0)
		{
			messageSource = new Rectangle(128, 1367, 150, 14);
		}
		switch (which)
		{
		case 0:
			texture = Game1.mouseCursors;
			bgSource = new Rectangle(128, 1296, 1, 71);
			flairSource = new Rectangle(144, 1303, 144, 58);
			movingFlairSource = new Rectangle(643, 768, 8, 13);
			originalBGSourceXLimit = bgSource.X + bgSource.Width;
			yPosition = (int)screenCenter.Y - bgSource.Height * 4 / 2;
			messagePosition = new Vector2(screenCenter.X - (float)(messageSource.Width * 4 / 2), screenCenter.Y - (float)(messageSource.Height * 4 / 2));
			flairPositions.Add(new Vector2(messagePosition.X - (float)(flairSource.Width * 4) - 64f, yPosition + 28));
			flairPositions.Add(new Vector2(messagePosition.X + (float)(messageSource.Width * 4) + 64f, yPosition + 28));
			movingFlairPosition = new Vector2(messagePosition.X + (float)(messageSource.Width * 4) + 192f, screenCenter.Y + 32f);
			movingFlairMotion = new Vector2(0f, -0.5f);
			break;
		case 1:
			texture = Game1.mouseCursors_1_6;
			bgSource = new Rectangle(0, 361, 1, 71);
			flairSource = new Rectangle(1, 361, 159, 71);
			movingFlairSource = new Rectangle(161, 412, 17, 16);
			originalBGSourceXLimit = bgSource.X + bgSource.Width;
			yPosition = (int)screenCenter.Y - bgSource.Height * 4 / 2;
			messagePosition = new Vector2(screenCenter.X - (float)(messageSource.Width * 4 / 2), screenCenter.Y - (float)(messageSource.Height * 4 / 2));
			flairPositions.Add(new Vector2(messagePosition.X - (float)(flairSource.Width * 4 / 2), yPosition));
			movingFlairPosition = new Vector2(messagePosition.X + (float)(messageSource.Width * 4) + 192f, screenCenter.Y + 32f);
			movingFlairMotion = new Vector2(0f, -0.5f);
			break;
		}
		bgDest = new Rectangle(0, yPosition, bgSource.Width * 4, bgSource.Height * 4);
	}

	public bool update(GameTime time)
	{
		if (durationAfterSwipe > 0 && bgDest.Width <= Game1.uiViewport.Width)
		{
			bgDest.Width += (int)((double)swipeVelocity * time.ElapsedGameTime.TotalMilliseconds);
			if (bgDest.Width > Game1.uiViewport.Width)
			{
				Game1.playSound("newRecord");
			}
		}
		else if (durationAfterSwipe <= 0)
		{
			bgDest.X += (int)((double)swipeVelocity * time.ElapsedGameTime.TotalMilliseconds);
			for (int i = 0; i < flairPositions.Count; i++)
			{
				if ((float)bgDest.X > flairPositions[i].X)
				{
					flairPositions[i] = new Vector2(bgDest.X, flairPositions[i].Y);
				}
			}
			if ((float)bgDest.X > messagePosition.X)
			{
				messagePosition = new Vector2(bgDest.X, messagePosition.Y);
			}
			if ((float)bgDest.X > movingFlairPosition.X)
			{
				movingFlairPosition = new Vector2(bgDest.X, movingFlairPosition.Y);
			}
		}
		if (bgDest.Width > Game1.uiViewport.Width && durationAfterSwipe > 0)
		{
			if (Game1.oldMouseState.LeftButton == ButtonState.Pressed)
			{
				durationAfterSwipe = 0;
			}
			durationAfterSwipe -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (durationAfterSwipe <= 0)
			{
				Game1.playSound("tinyWhip");
			}
		}
		movingFlairPosition += movingFlairMotion;
		return bgDest.X > Game1.uiViewport.Width;
	}

	public Rectangle getAdjustedSourceRect(Rectangle sourceRect, float xStartPosition)
	{
		if (xStartPosition > (float)bgDest.Width || xStartPosition + (float)(sourceRect.Width * 4) < (float)bgDest.X)
		{
			return Rectangle.Empty;
		}
		Math.Min(sourceRect.X + sourceRect.Width, Math.Max(sourceRect.X, (float)sourceRect.X + ((float)bgDest.Width - xStartPosition) / 4f));
		return new Rectangle(sourceRect.X, sourceRect.Y, (int)Math.Min(sourceRect.Width, ((float)bgDest.Width - xStartPosition) / 4f), sourceRect.Height);
	}

	public void draw(SpriteBatch b)
	{
		b.Draw(texture, bgDest, bgSource, Color.White);
		foreach (Vector2 v in flairPositions)
		{
			Rectangle r = getAdjustedSourceRect(flairSource, v.X);
			_ = r.Right;
			_ = originalBGSourceXLimit;
			b.Draw(texture, v, r, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		}
		b.Draw(texture, movingFlairPosition, getAdjustedSourceRect(movingFlairSource, movingFlairPosition.X), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		b.Draw(texture, messagePosition, getAdjustedSourceRect(messageSource, messagePosition.X), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
	}
}
