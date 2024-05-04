using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.Characters;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TokenizableStrings;

namespace StardewValley.Menus;

public class Billboard : IClickableMenu
{
	/// <summary>An event type that can be shown in the calendar.</summary>
	[Flags]
	public enum BillboardEventType
	{
		/// <summary>No event.</summary>
		None = 0,
		/// <summary>An NPC's birthday.</summary>
		Birthday = 1,
		/// <summary>A non-passive festival.</summary>
		Festival = 2,
		/// <summary>A fishing derby like Trophy Derby or Squidfest.</summary>
		FishingDerby = 4,
		/// <summary>A passive festival.</summary>
		PassiveFestival = 8,
		/// <summary>A wedding between a player and a player/NPC.</summary>
		Wedding = 0x10,
		/// <summary>A day that Marcello's Books will be in town</summary>
		Bookseller = 0x20
	}

	/// <summary>The cached data for a calendar day.</summary>
	public class BillboardDay
	{
		/// <summary>The event types on this day.</summary>
		public BillboardEventType Type { get; }

		/// <summary>The events on this day.</summary>
		public BillboardEvent[] Events { get; }

		/// <summary>The combined hover text for the events on this day.</summary>
		public string HoverText { get; }

		/// <summary>The texture to show for the calendar slot, if any.</summary>
		public Texture2D Texture { get; }

		/// <summary>The pixel area to draw within the <see cref="P:StardewValley.Menus.Billboard.BillboardDay.Texture" />, if applicable.</summary>
		public Rectangle TextureSourceRect { get; }

		/// <summary>Construct an instance.</summary>
		/// <param name="events">The events on this day.</param>
		public BillboardDay(BillboardEvent[] events)
		{
			Events = events;
			HoverText = string.Empty;
			foreach (BillboardEvent @event in events)
			{
				Type |= @event.Type;
				if (Texture == null && @event.Texture != null)
				{
					Texture = @event.Texture;
					TextureSourceRect = @event.TextureSourceRect;
				}
				HoverText = HoverText + @event.DisplayName + Environment.NewLine;
			}
			HoverText = HoverText.Trim();
		}

		public BillboardEvent GetEventOfType(BillboardEventType type)
		{
			BillboardEvent[] events = Events;
			foreach (BillboardEvent b in events)
			{
				if (b.Type == type)
				{
					return b;
				}
			}
			return null;
		}
	}

	/// <summary>An event shown on the calendar.</summary>
	public class BillboardEvent
	{
		/// <summary>If this event is currently unavailable. (e.g. Desert festival before desert is open)</summary>
		public bool locked;

		/// <summary>The event type.</summary>
		public BillboardEventType Type { get; }

		/// <summary>The values related to the event (like the names of the players or NPCs getting married).</summary>
		public string[] Arguments { get; }

		/// <summary>The name to show on the calendar.</summary>
		public string DisplayName { get; }

		/// <summary>The texture to show for the calendar slot, if any.</summary>
		public Texture2D Texture { get; }

		/// <summary>The pixel area to draw within the <see cref="P:StardewValley.Menus.Billboard.BillboardEvent.Texture" />, if applicable.</summary>
		public Rectangle TextureSourceRect { get; }

		/// <summary>Construct an instance.</summary>
		/// <param name="type">The event type.</param>
		/// <param name="arguments">The values related to the event (like the names of the players or NPCs getting married).</param>
		/// <param name="displayName">The name to show on the calendar.</param>
		/// <param name="texture">The texture to show for the calendar slot, if any.</param>
		/// <param name="sourceRect">The pixel area to draw within the <paramref name="texture" />, if applicable.</param>
		public BillboardEvent(BillboardEventType type, string[] arguments, string displayName, Texture2D texture = null, Rectangle sourceRect = default(Rectangle))
		{
			Type = type;
			Arguments = arguments;
			DisplayName = displayName;
			Texture = texture;
			TextureSourceRect = sourceRect;
		}
	}

	private Texture2D billboardTexture;

	public const int basewidth = 338;

	public const int baseWidth_calendar = 301;

	public const int baseheight = 198;

	private bool dailyQuestBoard;

	public ClickableComponent acceptQuestButton;

	public List<ClickableTextureComponent> calendarDays;

	private string hoverText = "";

	private List<int> booksellerdays;

	/// <summary>The events to show on the calendar for each day.</summary>
	/// <remarks>This only has entries for days that have events.</remarks>
	public readonly Dictionary<int, BillboardDay> calendarDayData = new Dictionary<int, BillboardDay>();

	public Billboard(bool dailyQuest = false)
		: base(0, 0, 0, 0, showUpperRightCloseButton: true)
	{
		if (!Game1.player.hasOrWillReceiveMail("checkedBulletinOnce"))
		{
			Game1.player.mailReceived.Add("checkedBulletinOnce");
			Game1.RequireLocation<Town>("Town").checkedBoard();
		}
		dailyQuestBoard = dailyQuest;
		billboardTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Billboard");
		width = (dailyQuest ? 338 : 301) * 4;
		height = 792;
		Vector2 center = Utility.getTopLeftPositionForCenteringOnScreen(width, height);
		xPositionOnScreen = (int)center.X;
		yPositionOnScreen = (int)center.Y;
		if (!dailyQuest)
		{
			booksellerdays = Utility.getDaysOfBooksellerThisSeason();
		}
		acceptQuestButton = new ClickableComponent(new Rectangle(xPositionOnScreen + width / 2 - 128, yPositionOnScreen + height - 128, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).X + 24, (int)Game1.dialogueFont.MeasureString(Game1.content.LoadString("Strings\\UI:AcceptQuest")).Y + 24), "")
		{
			myID = 0
		};
		UpdateDailyQuestButton();
		upperRightCloseButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 20, yPositionOnScreen, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
		Game1.playSound("bigSelect");
		if (!dailyQuest)
		{
			calendarDays = new List<ClickableTextureComponent>();
			Dictionary<int, List<NPC>> birthdays = GetBirthdays();
			for (int day = 1; day <= 28; day++)
			{
				List<BillboardEvent> curEvents = GetEventsForDay(day, birthdays);
				if (curEvents.Count > 0)
				{
					calendarDayData[day] = new BillboardDay(curEvents.ToArray());
				}
				int index = day - 1;
				calendarDays.Add(new ClickableTextureComponent(day.ToString(), new Rectangle(xPositionOnScreen + 152 + index % 7 * 32 * 4, yPositionOnScreen + 200 + index / 7 * 32 * 4, 124, 124), string.Empty, string.Empty, null, Rectangle.Empty, 1f)
				{
					myID = day,
					rightNeighborID = ((day % 7 != 0) ? (day + 1) : (-1)),
					leftNeighborID = ((day % 7 != 1) ? (day - 1) : (-1)),
					downNeighborID = day + 7,
					upNeighborID = ((day > 7) ? (day - 7) : (-1))
				});
			}
		}
		if (Game1.options.SnappyMenus)
		{
			populateClickableComponentList();
			snapToDefaultClickableComponent();
		}
	}

	/// <summary>Get all NPC birthdays that should be shown on the calendar this month, indexed by day.</summary>
	public virtual Dictionary<int, List<NPC>> GetBirthdays()
	{
		HashSet<string> addedBirthdays = new HashSet<string>();
		Dictionary<int, List<NPC>> birthdays = new Dictionary<int, List<NPC>>();
		Utility.ForEachVillager(delegate(NPC npc)
		{
			if (npc.Birthday_Season != Game1.currentSeason)
			{
				return true;
			}
			CalendarBehavior? calendarBehavior = npc.GetData()?.Calendar;
			if (calendarBehavior == CalendarBehavior.HiddenAlways || (calendarBehavior == CalendarBehavior.HiddenUntilMet && !Game1.player.friendshipData.ContainsKey(npc.Name)))
			{
				return true;
			}
			if (addedBirthdays.Contains(npc.Name))
			{
				return true;
			}
			if (!birthdays.TryGetValue(npc.Birthday_Day, out var value))
			{
				value = (birthdays[npc.Birthday_Day] = new List<NPC>());
			}
			value.Add(npc);
			addedBirthdays.Add(npc.Name);
			return true;
		});
		return birthdays;
	}

	/// <summary>Get the events to show on a given calendar day.</summary>
	/// <param name="day">The day of month.</param>
	/// <param name="birthdays">A cached lookup of birthdays by day.</param>
	public virtual List<BillboardEvent> GetEventsForDay(int day, Dictionary<int, List<NPC>> birthdays)
	{
		List<BillboardEvent> curEvents = new List<BillboardEvent>();
		string id;
		if (Utility.isFestivalDay(day, Game1.season))
		{
			id = Game1.currentSeason + day;
			string festivalName = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\Festivals\\" + id)["name"];
			curEvents.Add(new BillboardEvent(BillboardEventType.Festival, new string[1] { id }, festivalName));
		}
		if (Utility.TryGetPassiveFestivalDataForDay(day, Game1.season, null, out id, out var festivalData, ignoreConditionsCheck: true) && (festivalData?.ShowOnCalendar ?? false))
		{
			string festivalName = TokenParser.ParseText(festivalData.DisplayName);
			if (!GameStateQuery.CheckConditions(festivalData.Condition))
			{
				curEvents.Add(new BillboardEvent(BillboardEventType.PassiveFestival, new string[1] { id }, "???")
				{
					locked = true
				});
			}
			else
			{
				curEvents.Add(new BillboardEvent(BillboardEventType.PassiveFestival, new string[1] { id }, festivalName));
			}
		}
		if (Game1.IsSummer && (day == 20 || day == 21))
		{
			string festivalName = Game1.content.LoadString("Strings\\1_6_Strings:TroutDerby");
			curEvents.Add(new BillboardEvent(BillboardEventType.FishingDerby, LegacyShims.EmptyArray<string>(), festivalName));
		}
		else if (Game1.IsWinter && (day == 12 || day == 13))
		{
			string festivalName = Game1.content.LoadString("Strings\\1_6_Strings:SquidFest");
			curEvents.Add(new BillboardEvent(BillboardEventType.FishingDerby, LegacyShims.EmptyArray<string>(), festivalName));
		}
		if (booksellerdays.Contains(day))
		{
			string name = Game1.content.LoadString("Strings\\1_6_Strings:Bookseller");
			curEvents.Add(new BillboardEvent(BillboardEventType.Bookseller, LegacyShims.EmptyArray<string>(), name));
		}
		if (birthdays.TryGetValue(day, out var birthdayVillagers))
		{
			foreach (NPC n in birthdayVillagers)
			{
				char lastChar = n.displayName.Last();
				string displayText = ((lastChar == 's' || (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.de && (lastChar == 'x' || lastChar == 'ß' || lastChar == 'z'))) ? Game1.content.LoadString("Strings\\UI:Billboard_SBirthday", n.displayName) : Game1.content.LoadString("Strings\\UI:Billboard_Birthday", n.displayName));
				Texture2D character_texture;
				try
				{
					character_texture = Game1.content.Load<Texture2D>("Characters\\" + n.getTextureName());
				}
				catch
				{
					character_texture = n.Sprite.Texture;
				}
				curEvents.Add(new BillboardEvent(BillboardEventType.Birthday, new string[1] { n.Name }, displayText, character_texture, n.getMugShotSourceRect()));
			}
		}
		HashSet<Farmer> traversed_farmers = new HashSet<Farmer>();
		FarmerCollection onlineFarmers = Game1.getOnlineFarmers();
		foreach (Farmer farmer in onlineFarmers)
		{
			if (traversed_farmers.Contains(farmer) || !farmer.isEngaged() || farmer.hasCurrentOrPendingRoommate())
			{
				continue;
			}
			string spouse_name = null;
			WorldDate wedding_date = null;
			NPC spouse = Game1.getCharacterFromName(farmer.spouse);
			if (spouse != null)
			{
				wedding_date = farmer.friendshipData[farmer.spouse].WeddingDate;
				spouse_name = spouse.displayName;
			}
			else
			{
				long? spouseId = farmer.team.GetSpouse(farmer.UniqueMultiplayerID);
				if (spouseId.HasValue)
				{
					Farmer spouse_farmer = Game1.getFarmerMaybeOffline(spouseId.Value);
					if (spouse_farmer != null && onlineFarmers.Contains(spouse_farmer))
					{
						wedding_date = farmer.team.GetFriendship(farmer.UniqueMultiplayerID, spouseId.Value).WeddingDate;
						traversed_farmers.Add(spouse_farmer);
						spouse_name = spouse_farmer.Name;
					}
				}
			}
			if (!(wedding_date == null))
			{
				if (wedding_date.TotalDays < Game1.Date.TotalDays)
				{
					wedding_date = new WorldDate(Game1.Date);
					wedding_date.TotalDays++;
				}
				if (wedding_date?.TotalDays >= Game1.Date.TotalDays && Game1.season == wedding_date.Season && day == wedding_date.DayOfMonth)
				{
					curEvents.Add(new BillboardEvent(BillboardEventType.Wedding, new string[2] { farmer.Name, spouse_name }, Game1.content.LoadString("Strings\\UI:Calendar_Wedding", farmer.Name, spouse_name)));
					traversed_farmers.Add(farmer);
				}
			}
		}
		return curEvents;
	}

	public override void snapToDefaultClickableComponent()
	{
		currentlySnappedComponent = getComponentWithID((!dailyQuestBoard) ? 1 : 0);
		snapCursorToCurrentSnappedComponent();
	}

	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		Game1.activeClickableMenu = new Billboard(dailyQuestBoard);
	}

	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		Game1.playSound("bigDeSelect");
		exitThisMenu();
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		base.receiveLeftClick(x, y, playSound);
		if (acceptQuestButton.visible && acceptQuestButton.containsPoint(x, y))
		{
			Game1.playSound("newArtifact");
			Game1.questOfTheDay.dailyQuest.Value = true;
			Game1.questOfTheDay.dayQuestAccepted.Value = Game1.Date.TotalDays;
			Game1.questOfTheDay.accepted.Value = true;
			Game1.questOfTheDay.canBeCancelled.Value = true;
			Game1.questOfTheDay.daysLeft.Value = 2;
			Game1.player.questLog.Add(Game1.questOfTheDay);
			Game1.player.acceptedDailyQuest.Set(newValue: true);
			UpdateDailyQuestButton();
		}
	}

	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		hoverText = "";
		if (dailyQuestBoard && Game1.questOfTheDay != null && !Game1.questOfTheDay.accepted)
		{
			float oldScale = acceptQuestButton.scale;
			acceptQuestButton.scale = (acceptQuestButton.bounds.Contains(x, y) ? 1.5f : 1f);
			if (acceptQuestButton.scale > oldScale)
			{
				Game1.playSound("Cowboy_gunshot");
			}
		}
		if (calendarDays == null)
		{
			return;
		}
		foreach (ClickableTextureComponent c in calendarDays)
		{
			if (c.bounds.Contains(x, y))
			{
				hoverText = (calendarDayData.TryGetValue(c.myID, out var day) ? day.HoverText : string.Empty);
				break;
			}
		}
	}

	public override void draw(SpriteBatch b)
	{
		bool hide_mouse = false;
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
		}
		b.Draw(billboardTexture, new Vector2(xPositionOnScreen, yPositionOnScreen), dailyQuestBoard ? new Rectangle(0, 0, 338, 198) : new Rectangle(0, 198, 301, 198), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		if (!dailyQuestBoard)
		{
			b.DrawString(Game1.dialogueFont, Utility.getSeasonNameFromNumber(Game1.seasonIndex), new Vector2(xPositionOnScreen + 160, yPositionOnScreen + 80), Game1.textColor);
			b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:Billboard_Year", Game1.year), new Vector2(xPositionOnScreen + 448, yPositionOnScreen + 80), Game1.textColor);
			for (int i = 0; i < calendarDays.Count; i++)
			{
				ClickableTextureComponent component = calendarDays[i];
				if (calendarDayData.TryGetValue(component.myID, out var day))
				{
					if (day.Texture != null)
					{
						b.Draw(day.Texture, new Vector2(component.bounds.X + 48, component.bounds.Y + 28), day.TextureSourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					}
					if (day.Type.HasFlag(BillboardEventType.PassiveFestival))
					{
						Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(component.bounds.X + 12, (float)(component.bounds.Y + 60) - Game1.dialogueButtonScale / 2f), new Rectangle(346, 392, 8, 8), day.GetEventOfType(BillboardEventType.PassiveFestival).locked ? (Color.Black * 0.3f) : Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
					}
					if (day.Type.HasFlag(BillboardEventType.Festival))
					{
						Utility.drawWithShadow(b, billboardTexture, new Vector2(component.bounds.X + 40, (float)(component.bounds.Y + 56) - Game1.dialogueButtonScale / 2f), new Rectangle(1 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0 / 100.0) * 14, 398, 14, 12), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
					}
					if (day.Type.HasFlag(BillboardEventType.FishingDerby))
					{
						Utility.drawWithShadow(b, Game1.mouseCursors_1_6, new Vector2(calendarDays[i].bounds.X + 8, (float)(calendarDays[i].bounds.Y + 60) - Game1.dialogueButtonScale / 2f), new Rectangle(103, 2, 10, 11), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
					}
					if (day.Type.HasFlag(BillboardEventType.Wedding))
					{
						b.Draw(Game1.mouseCursors2, new Vector2(component.bounds.Right - 56, component.bounds.Top - 12), new Rectangle(112, 32, 16, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					}
					if (day.Type.HasFlag(BillboardEventType.Bookseller))
					{
						b.Draw(Game1.mouseCursors_1_6, new Vector2((float)(component.bounds.Right - 72) - 2f * (float)Math.Sin((Game1.currentGameTime.TotalGameTime.TotalSeconds + (double)i * 0.3) * 3.0), (float)(component.bounds.Top + 52) - 2f * (float)Math.Cos((Game1.currentGameTime.TotalGameTime.TotalSeconds + (double)i * 0.3) * 2.0)), new Rectangle(71, 63, 8, 15), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
					}
				}
				if (Game1.dayOfMonth > i + 1)
				{
					b.Draw(Game1.staminaRect, component.bounds, Color.Gray * 0.25f);
				}
				else if (Game1.dayOfMonth == i + 1)
				{
					int offset = (int)(4f * Game1.dialogueButtonScale / 8f);
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(379, 357, 3, 3), component.bounds.X - offset, component.bounds.Y - offset, component.bounds.Width + offset * 2, component.bounds.Height + offset * 2, Color.Blue, 4f, drawShadow: false);
				}
			}
		}
		else
		{
			if (Game1.options.SnappyMenus)
			{
				hide_mouse = true;
			}
			if (Game1.questOfTheDay == null || Game1.questOfTheDay.currentObjective == null || Game1.questOfTheDay.currentObjective.Length == 0)
			{
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:Billboard_NothingPosted"), new Vector2(xPositionOnScreen + 384, yPositionOnScreen + 320), Game1.textColor);
			}
			else
			{
				SpriteFont font = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? Game1.smallFont : Game1.dialogueFont);
				string description = Game1.parseText(Game1.questOfTheDay.questDescription, font, 640);
				Utility.drawTextWithShadow(b, description, font, new Vector2(xPositionOnScreen + 320 + 32, yPositionOnScreen + 256), Game1.textColor, 1f, -1f, -1, -1, 0.5f);
				if (acceptQuestButton.visible)
				{
					hide_mouse = false;
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), acceptQuestButton.bounds.X, acceptQuestButton.bounds.Y, acceptQuestButton.bounds.Width, acceptQuestButton.bounds.Height, (acceptQuestButton.scale > 1f) ? Color.LightPink : Color.White, 4f * acceptQuestButton.scale);
					Utility.drawTextWithShadow(b, Game1.content.LoadString("Strings\\UI:AcceptQuest"), Game1.dialogueFont, new Vector2(acceptQuestButton.bounds.X + 12, acceptQuestButton.bounds.Y + (LocalizedContentManager.CurrentLanguageLatin ? 16 : 12)), Game1.textColor);
				}
				if (Game1.stats.Get("BillboardQuestsDone") % 3 == 2 && (acceptQuestButton.visible || !Game1.questOfTheDay.completed))
				{
					Utility.drawWithShadow(b, Game1.content.Load<Texture2D>("TileSheets\\Objects_2"), base.Position + new Vector2(215f, 144f) * 4f, new Rectangle(80, 128, 16, 16), Color.White, 0f, Vector2.Zero, 4f);
					SpriteText.drawString(b, "x1", (int)base.Position.X + 936, (int)base.Position.Y + 596);
				}
			}
			bool drawAllStars = Game1.stats.Get("BillboardQuestsDone") % 3 == 0 && Game1.questOfTheDay != null && (bool)Game1.questOfTheDay.completed;
			for (int i = 0; i < (drawAllStars ? 3 : (Game1.stats.Get("BillboardQuestsDone") % 3)); i++)
			{
				b.Draw(billboardTexture, base.Position + new Vector2(18 + 12 * i, 36f) * 4f, new Rectangle(140, 397, 10, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.6f);
			}
			if (Game1.player.hasCompletedCommunityCenter())
			{
				b.Draw(billboardTexture, base.Position + new Vector2(290f, 59f) * 4f, new Rectangle(0, 427, 39, 54), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.6f);
			}
		}
		base.draw(b);
		if (!hide_mouse)
		{
			Game1.mouseCursorTransparency = 1f;
			drawMouse(b);
			if (hoverText.Length > 0)
			{
				IClickableMenu.drawHoverText(b, hoverText, Game1.dialogueFont);
			}
		}
	}

	public void UpdateDailyQuestButton()
	{
		if (acceptQuestButton != null)
		{
			if (!dailyQuestBoard)
			{
				acceptQuestButton.visible = false;
			}
			else
			{
				acceptQuestButton.visible = Game1.CanAcceptDailyQuest();
			}
		}
	}
}
