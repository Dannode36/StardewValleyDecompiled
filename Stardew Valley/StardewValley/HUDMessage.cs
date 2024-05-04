using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace StardewValley;

public class HUDMessage
{
	public const float defaultTime = 3500f;

	public const int achievement_type = 1;

	public const int newQuest_type = 2;

	public const int error_type = 3;

	public const int stamina_type = 4;

	public const int health_type = 5;

	public const int screenshot_type = 6;

	/// <summary>The message text to show.</summary>
	public string message;

	/// <summary>A key used to prevent multiple HUD messages from stacking, or <c>null</c> to use the item name.</summary>
	public string type;

	/// <summary>The duration in milliseconds until the message should disappear.</summary>
	public float timeLeft;

	/// <summary>The current opacity, from 0 (fully transparent) to 1 (fully opaque).</summary>
	public float transparency = 1f;

	/// <summary>The count of the <see cref="F:StardewValley.HUDMessage.messageSubject" /> that was received, if applicable.</summary>
	public int number = -1;

	/// <summary>The icon to show, matching a constant like <see cref="F:StardewValley.HUDMessage.error_type" />.</summary>
	public int whatType;

	/// <summary>Whether this is an achievement-unlocked message.</summary>
	public bool achievement;

	/// <summary>Whether to hide the icon portion of the box.</summary>
	public bool noIcon;

	/// <summary>The item that was received, if applicable.</summary>
	public Item messageSubject;

	/// <summary>Construct an instance with the default time and an empty icon.</summary>
	/// <param name="message">The message text to show.</param>
	public HUDMessage(string message)
		: this(message, 3500f)
	{
	}

	/// <summary>Construct an instance with a specified icon type, and a duration 1.5× default.</summary>
	/// <param name="message">The message text to show.</param>
	/// <param name="whatType">The icon to show, matching a constant like <see cref="F:StardewValley.HUDMessage.error_type" />.</param>
	public HUDMessage(string message, int whatType)
		: this(message, 5250f)
	{
		achievement = true;
		this.whatType = whatType;
	}

	/// <summary>Construct an instance with the given values.</summary>
	/// <param name="message">The message text to show.</param>
	/// <param name="timeLeft">The duration in milliseconds for which to show the message.</param>
	/// <param name="fadeIn">Whether the message should start transparent and fade in.</param>
	public HUDMessage(string message, float timeLeft, bool fadeIn = false)
	{
		this.message = message;
		this.timeLeft = timeLeft;
		if (fadeIn)
		{
			transparency = 0f;
		}
	}

	/// <summary>Construct a message indicating an item received.</summary>
	/// <param name="item">The item that was received.</param>
	/// <param name="count">The number of the item received.</param>
	/// <param name="type">A key used to prevent multiple HUD messages from stacking, or <c>null</c> to use the item name.</param>
	public static HUDMessage ForItemGained(Item item, int count, string type = null)
	{
		return new HUDMessage(item.DisplayName)
		{
			number = count,
			type = (type ?? item.Name),
			messageSubject = item
		};
	}

	/// <summary>Construct a larger textbox with line wrapping and no icon.</summary>
	/// <param name="message">The message text to show.</param>
	public static HUDMessage ForCornerTextbox(string message)
	{
		message = Game1.parseText(message, Game1.dialogueFont, 384);
		return new HUDMessage(message)
		{
			noIcon = true,
			timeLeft = 5250f
		};
	}

	/// <summary>Construct an achievement display.</summary>
	/// <param name="achievementName">The translated achievement name.</param>
	public static HUDMessage ForAchievement(string achievementName)
	{
		return new HUDMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HUDMessage.cs.3824") + achievementName, 5250f)
		{
			achievement = true,
			whatType = 1
		};
	}

	public virtual bool update(GameTime time)
	{
		timeLeft -= time.ElapsedGameTime.Milliseconds;
		if (timeLeft < 0f)
		{
			transparency -= 0.02f;
			if (transparency < 0f)
			{
				return true;
			}
		}
		else if (transparency < 1f)
		{
			transparency = Math.Min(transparency + 0.02f, 1f);
		}
		return false;
	}

	public virtual void draw(SpriteBatch b, int i, ref int heightUsed)
	{
		Rectangle tsarea = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
		int height;
		if (noIcon)
		{
			int overrideX = tsarea.Left + 16;
			height = (int)Game1.dialogueFont.MeasureString(message).Y + 64;
			int overrideY = ((Game1.uiViewport.Width < 1400) ? (-64) : 0) + tsarea.Bottom - height - heightUsed - 64;
			heightUsed += height;
			IClickableMenu.drawHoverText(b, message, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, null, -1, overrideX, overrideY, transparency);
			return;
		}
		height = 112;
		Vector2 itemBoxPosition = new Vector2(tsarea.Left + 16, tsarea.Bottom - height - heightUsed - 64);
		heightUsed += height;
		if (Game1.isOutdoorMapSmallerThanViewport())
		{
			itemBoxPosition.X = Math.Max(tsarea.Left + 16, -Game1.uiViewport.X + 16);
		}
		if (Game1.uiViewport.Width < 1400)
		{
			itemBoxPosition.Y -= 48f;
		}
		b.Draw(Game1.mouseCursors, itemBoxPosition, (messageSubject is Object obj && obj.sellToStorePrice(-1L) > 500) ? new Rectangle(163, 399, 26, 24) : new Rectangle(293, 360, 26, 24), Color.White * transparency, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		float messageWidth = Game1.smallFont.MeasureString(message ?? "").X;
		b.Draw(Game1.mouseCursors, new Vector2(itemBoxPosition.X + 104f, itemBoxPosition.Y), new Rectangle(319, 360, 1, 24), Color.White * transparency, 0f, Vector2.Zero, new Vector2(messageWidth, 4f), SpriteEffects.None, 1f);
		b.Draw(Game1.mouseCursors, new Vector2(itemBoxPosition.X + 104f + messageWidth, itemBoxPosition.Y), new Rectangle(323, 360, 6, 24), Color.White * transparency, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		itemBoxPosition.X += 16f;
		itemBoxPosition.Y += 16f;
		if (messageSubject == null)
		{
			switch (whatType)
			{
			case 1:
				b.Draw(Game1.mouseCursors, itemBoxPosition + new Vector2(8f, 8f) * 4f, new Rectangle(294, 392, 16, 16), Color.White * transparency, 0f, new Vector2(8f, 8f), 4f + Math.Max(0f, (timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
				break;
			case 2:
				b.Draw(Game1.mouseCursors, itemBoxPosition + new Vector2(8f, 8f) * 4f, new Rectangle(403, 496, 5, 14), Color.White * transparency, 0f, new Vector2(3f, 7f), 4f + Math.Max(0f, (timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
				break;
			case 3:
				b.Draw(Game1.mouseCursors, itemBoxPosition + new Vector2(8f, 8f) * 4f, new Rectangle(268, 470, 16, 16), Color.White * transparency, 0f, new Vector2(8f, 8f), 4f + Math.Max(0f, (timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
				break;
			case 4:
				b.Draw(Game1.mouseCursors, itemBoxPosition + new Vector2(8f, 8f) * 4f, new Rectangle(0, 411, 16, 16), Color.White * transparency, 0f, new Vector2(8f, 8f), 4f + Math.Max(0f, (timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
				break;
			case 5:
				b.Draw(Game1.mouseCursors, itemBoxPosition + new Vector2(8f, 8f) * 4f, new Rectangle(16, 411, 16, 16), Color.White * transparency, 0f, new Vector2(8f, 8f), 4f + Math.Max(0f, (timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
				break;
			case 6:
				b.Draw(Game1.mouseCursors2, itemBoxPosition + new Vector2(8f, 8f) * 4f, new Rectangle(96, 32, 16, 16), Color.White * transparency, 0f, new Vector2(8f, 8f), 4f + Math.Max(0f, (timeLeft - 3000f) / 900f), SpriteEffects.None, 1f);
				break;
			}
		}
		else
		{
			messageSubject.drawInMenu(b, itemBoxPosition, 1f + Math.Max(0f, (timeLeft - 3000f) / 900f), transparency, 1f, StackDrawType.Hide);
		}
		itemBoxPosition.X += 51f;
		itemBoxPosition.Y += 51f;
		if (number > 1)
		{
			Utility.drawTinyDigits(number, b, itemBoxPosition, 3f, 1f, Color.White * transparency);
		}
		itemBoxPosition.X += 32f;
		itemBoxPosition.Y -= 33f;
		Utility.drawTextWithShadow(b, message ?? "", Game1.smallFont, itemBoxPosition, Game1.textColor * transparency, 1f, 1f, -1, -1, transparency);
	}

	public static void numbersEasterEgg(int number)
	{
		if (number > 100000 && !Game1.player.mailReceived.Contains("numbersEgg1"))
		{
			Game1.player.mailReceived.Add("numbersEgg1");
			Game1.chatBox.addMessage("...", new Color(255, 255, 255));
		}
		if (number > 200000 && !Game1.player.mailReceived.Contains("numbersEgg2"))
		{
			Game1.player.mailReceived.Add("numbersEgg2");
			Game1.chatBox.addMessage("......", new Color(255, 255, 255));
		}
		if (number > 250000 && !Game1.player.mailReceived.Contains("numbersEgg3"))
		{
			Game1.player.mailReceived.Add("numbersEgg3");
			Game1.chatBox.addMessage((Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en) ? "Shooting for a million?" : "...........???", new Color(255, 255, 255));
		}
		if (number > 500000 && !Game1.player.mailReceived.Contains("numbersEgg1.5"))
		{
			Game1.player.mailReceived.Add("numbersEgg1.5");
			Game1.chatBox.addMessage(".......................", new Color(255, 255, 255));
		}
		if (number <= 1000000 || Game1.player.mailReceived.Contains("numbersEgg7"))
		{
			return;
		}
		Game1.player.mailReceived.Add("numbersEgg7");
		Game1.chatBox.addMessage((Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en) ? "[196] Secret Iridium Stackmaster Trophy Achieved [196]" : "[196]", new Color(104, 214, 255));
		Game1.playSound("discoverMineral");
		if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.en)
		{
			DelayedAction.functionAfterDelay(delegate
			{
				Game1.chatBox.addMessage("Qi: *slow clap*... Congratulations, kid. Ya did it. Now, on to the next challenge!", new Color(100, 50, 255));
			}, 6000);
		}
	}
}
