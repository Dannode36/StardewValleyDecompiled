using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Characters;
using StardewValley.GameData.Characters;

namespace StardewValley.Menus;

public class SocialPage : IClickableMenu
{
	/// <summary>An entry on the social page.</summary>
	public class SocialEntry
	{
		/// <summary>The backing field for <see cref="M:StardewValley.Menus.SocialPage.SocialEntry.IsMarriedToAnyone" />.</summary>
		private bool? CachedIsMarriedToAnyone;

		/// <summary>The character instance.</summary>
		public Character Character;

		/// <summary>The unique multiplayer ID for a player, or the internal name for an NPC.</summary>
		public readonly string InternalName;

		/// <summary>The translated display name.</summary>
		public readonly string DisplayName;

		/// <summary>Whether the current player has met this character.</summary>
		public readonly bool IsMet;

		/// <summary>Whether players can romance this character.</summary>
		public readonly bool IsDatable;

		/// <summary>How the NPC is shown on the social tab.</summary>
		public readonly SocialTabBehavior SocialTabBehavior;

		/// <summary>Whether this character is a child.</summary>
		public readonly bool IsChild;

		/// <summary>Whether this character is a player.</summary>
		public readonly bool IsPlayer;

		/// <summary>The character's gender identity.</summary>
		public readonly Gender Gender;

		/// <summary>The current player's heart level with this character.</summary>
		public readonly int HeartLevel;

		/// <summary>The current player's friendship data with the character, if any.</summary>
		public readonly Friendship Friendship;

		/// <summary>The NPC's character data, if applicable.</summary>
		public readonly CharacterData Data;

		/// <summary>The order in which the current player met this NPC, if applicable.</summary>
		public int? OrderMet;

		/// <summary>Construct an instance.</summary>
		/// <param name="player">The player for which to create an entry.</param>
		/// <param name="friendship">The current player's friendship with this character.</param>
		public SocialEntry(Farmer player, Friendship friendship)
		{
			Character = player;
			InternalName = player.UniqueMultiplayerID.ToString();
			DisplayName = player.Name;
			IsMet = true;
			IsPlayer = true;
			Gender = player.Gender;
			Friendship = friendship;
		}

		/// <summary>Construct an instance.</summary>
		/// <param name="npc">The NPC for which to create an entry.</param>
		/// <param name="friendship">The current player's friendship with this character.</param>
		/// <param name="data">The NPC's character data, if applicable.</param>
		/// <param name="overrideDisplayName">The translated display name, or <c>null</c> to get it from <paramref name="npc" />.</param>
		public SocialEntry(NPC npc, Friendship friendship, CharacterData data, string overrideDisplayName = null)
		{
			Character = npc;
			InternalName = npc.Name;
			DisplayName = overrideDisplayName ?? npc.displayName;
			IsMet = friendship != null || npc is Child;
			IsDatable = data?.CanBeRomanced ?? false;
			SocialTabBehavior = data?.SocialTab ?? SocialTabBehavior.AlwaysShown;
			IsChild = npc is Child;
			Gender = npc.Gender;
			HeartLevel = (friendship?.Points ?? 0) / 250;
			Friendship = friendship;
			Data = data;
		}

		/// <summary>Get whether the current player is dating this character.</summary>
		public bool IsDatingCurrentPlayer()
		{
			return Friendship?.IsDating() ?? false;
		}

		/// <summary>Get whether the current player is married to this character.</summary>
		public bool IsMarriedToCurrentPlayer()
		{
			return Friendship?.IsMarried() ?? false;
		}

		/// <summary>Get whether the current player is a roommate with this character.</summary>
		public bool IsRoommateForCurrentPlayer()
		{
			return Friendship?.IsRoommate() ?? false;
		}

		/// <summary>Get whether the current player is married to this character.</summary>
		public bool IsDivorcedFromCurrentPlayer()
		{
			return Friendship?.IsDivorced() ?? false;
		}

		/// <summary>Get whether this character is married to any player.</summary>
		public bool IsMarriedToAnyone()
		{
			if (!CachedIsMarriedToAnyone.HasValue)
			{
				if (IsMarriedToCurrentPlayer())
				{
					CachedIsMarriedToAnyone = true;
				}
				else
				{
					foreach (Farmer farmer in Game1.getAllFarmers())
					{
						if (farmer.spouse == InternalName && farmer.isMarriedOrRoommates())
						{
							CachedIsMarriedToAnyone = true;
							break;
						}
					}
					if (!CachedIsMarriedToAnyone.HasValue)
					{
						CachedIsMarriedToAnyone = false;
					}
				}
			}
			return CachedIsMarriedToAnyone.Value;
		}
	}

	public const int slotsOnPage = 5;

	private string hoverText = "";

	private ClickableTextureComponent upButton;

	private ClickableTextureComponent downButton;

	private ClickableTextureComponent scrollBar;

	private Rectangle scrollBarRunner;

	/// <summary>The players and social NPCs shown in the list.</summary>
	public readonly List<SocialEntry> SocialEntries;

	/// <summary>The character portrait components.</summary>
	private readonly List<ClickableTextureComponent> sprites = new List<ClickableTextureComponent>();

	/// <summary>The index of the <see cref="F:StardewValley.Menus.SocialPage.SocialEntries" /> entry shown at the top of the scrolled view.</summary>
	private int slotPosition;

	/// <summary>The number of players shown in the list.</summary>
	private int numFarmers;

	/// <summary>The clickable slots over which character info is drawn.</summary>
	public readonly List<ClickableTextureComponent> characterSlots = new List<ClickableTextureComponent>();

	private bool scrolling;

	public SocialPage(int x, int y, int width, int height)
		: base(x, y, width, height)
	{
		SocialEntries = FindSocialCharacters();
		numFarmers = SocialEntries.Count((SocialEntry p) => p.IsPlayer);
		CreateComponents();
		slotPosition = 0;
		for (int i = 0; i < SocialEntries.Count; i++)
		{
			if (!SocialEntries[i].IsPlayer)
			{
				slotPosition = i;
				break;
			}
		}
		setScrollBarToCurrentIndex();
		updateSlots();
	}

	/// <summary>Find all social NPCs which should be shown on the social page.</summary>
	public List<SocialEntry> FindSocialCharacters()
	{
		List<SocialEntry> players = new List<SocialEntry>();
		Dictionary<string, SocialEntry> villagers = new Dictionary<string, SocialEntry>();
		List<SocialEntry> children = new List<SocialEntry>();
		foreach (NPC npc in GetAllNpcs())
		{
			if (!Game1.player.friendshipData.TryGetValue(npc.Name, out var friendship))
			{
				friendship = null;
			}
			if (npc is Child)
			{
				children.Add(new SocialEntry(npc, friendship, null, npc.displayName));
			}
			else
			{
				if (!npc.CanSocialize)
				{
					continue;
				}
				CharacterData data = npc.GetData();
				string displayName = npc.displayName;
				switch (data?.SocialTab)
				{
				case SocialTabBehavior.HiddenUntilMet:
					if (friendship == null)
					{
						continue;
					}
					break;
				case SocialTabBehavior.UnknownUntilMet:
					if (friendship == null)
					{
						displayName = "???";
					}
					break;
				case SocialTabBehavior.AlwaysShown:
					if (friendship == null)
					{
						Game1.player.friendshipData.Add(npc.Name, friendship = new Friendship());
					}
					break;
				case SocialTabBehavior.HiddenAlways:
					continue;
				}
				villagers[npc.Name] = new SocialEntry(npc, friendship, data, displayName);
			}
		}
		int orderMet = 0;
		foreach (KeyValuePair<string, Friendship> pair in Game1.player.friendshipData.Pairs)
		{
			if (villagers.TryGetValue(pair.Key, out var entry))
			{
				entry.OrderMet = orderMet++;
			}
		}
		foreach (Farmer player in Game1.getAllFarmers())
		{
			if (!player.IsLocalPlayer && (player.IsMainPlayer || (bool)player.isCustomized))
			{
				Friendship friendship = Game1.player.team.GetFriendship(Game1.player.UniqueMultiplayerID, player.UniqueMultiplayerID);
				players.Add(new SocialEntry(player, friendship));
			}
		}
		List<SocialEntry> list = new List<SocialEntry>();
		list.AddRange(players);
		list.AddRange(from entry in villagers.Values
			orderby entry.Friendship?.Points descending, entry.OrderMet, entry.DisplayName
			select entry);
		list.AddRange(children.OrderBy((SocialEntry p) => p.DisplayName));
		return list;
	}

	/// <summary>Get all child or villager NPCs from the world and friendship data.</summary>
	public IEnumerable<NPC> GetAllNpcs()
	{
		HashSet<string> nonSocial = new HashSet<string>();
		Dictionary<string, NPC> found = new Dictionary<string, NPC>();
		Utility.ForEachCharacter(delegate(NPC npc)
		{
			if (npc is Child)
			{
				found[npc.Name + "$$child"] = npc;
			}
			else if (npc.IsVillager)
			{
				NPC value;
				if (!npc.CanSocialize)
				{
					nonSocial.Add(npc.Name);
				}
				else if (found.TryGetValue(npc.Name, out value) && npc != value)
				{
					bool flag = true;
					if (Game1.IsClient)
					{
						bool num = value.currentLocation.IsActiveLocation();
						bool flag2 = npc.currentLocation.IsActiveLocation();
						if (num != flag2)
						{
							if (flag2)
							{
								found[npc.Name] = npc;
							}
							flag = false;
						}
					}
					if (flag)
					{
						Game1.log.Warn($"The social page found conflicting NPCs with name {npc.Name} (one at {value.currentLocation?.NameOrUniqueName} {value.TilePoint}, the other at {npc.currentLocation?.NameOrUniqueName} {npc.TilePoint}); only the first will be shown.");
					}
				}
				else
				{
					found[npc.Name] = npc;
				}
			}
			return true;
		});
		Event @event = Game1.currentLocation?.currentEvent;
		if (@event != null)
		{
			foreach (NPC actor in @event.actors)
			{
				if (actor.IsVillager && actor.CanSocialize)
				{
					found[actor.Name] = actor;
				}
			}
		}
		foreach (string name in Game1.player.friendshipData.Keys)
		{
			if (nonSocial.Contains(name) || found.ContainsKey(name) || !NPC.TryGetData(name, out var _))
			{
				continue;
			}
			string textureName = NPC.getTextureNameForCharacter(name);
			string spriteAssetName = "Characters\\" + textureName;
			string portraitAssetName = "Portraits\\" + textureName;
			if (Game1.content.DoesAssetExist<Texture2D>(spriteAssetName) && Game1.content.DoesAssetExist<Texture2D>(portraitAssetName))
			{
				try
				{
					AnimatedSprite sprite = new AnimatedSprite(spriteAssetName, 0, 16, 32);
					Texture2D portraits = Game1.content.Load<Texture2D>(portraitAssetName);
					found[name] = new NPC(sprite, Vector2.Zero, "Town", 0, name, portraits, eventActor: false);
				}
				catch
				{
				}
			}
		}
		return found.Values;
	}

	/// <summary>Load the clickable components to display.</summary>
	public void CreateComponents()
	{
		sprites.Clear();
		characterSlots.Clear();
		for (int i = 0; i < SocialEntries.Count; i++)
		{
			sprites.Add(CreateSpriteComponent(SocialEntries[i], i));
			ClickableTextureComponent slot = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth, 0, width - IClickableMenu.borderWidth * 2, rowPosition(1) - rowPosition(0)), null, new Rectangle(0, 0, 0, 0), 4f)
			{
				myID = i,
				downNeighborID = i + 1,
				upNeighborID = i - 1
			};
			if (slot.upNeighborID < 0)
			{
				slot.upNeighborID = 12342;
			}
			characterSlots.Add(slot);
		}
		upButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
		downButton = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
		scrollBar = new ClickableTextureComponent(new Rectangle(upButton.bounds.X + 12, upButton.bounds.Y + upButton.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
		scrollBarRunner = new Rectangle(scrollBar.bounds.X, upButton.bounds.Y + upButton.bounds.Height + 4, scrollBar.bounds.Width, height - 128 - upButton.bounds.Height - 8);
	}

	/// <summary>Create the clickable texture component for a character's portrait.</summary>
	/// <param name="entry">The social character to render.</param>
	/// <param name="index">The index in the list of entries.</param>
	public ClickableTextureComponent CreateSpriteComponent(SocialEntry entry, int index)
	{
		Rectangle bounds = new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth + 4, 0, width, 64);
		Rectangle sourceRect = ((!entry.IsPlayer && entry.Character is NPC npc) ? npc.getMugShotSourceRect() : Rectangle.Empty);
		return new ClickableTextureComponent(index.ToString(), bounds, null, "", entry.Character.Sprite.Texture, sourceRect, 4f);
	}

	/// <summary>Get the social entry from its index in the list.</summary>
	/// <param name="index">The index in the social list.</param>
	public SocialEntry GetSocialEntry(int index)
	{
		if (index < 0 || index >= SocialEntries.Count)
		{
			index = 0;
		}
		return SocialEntries[index];
	}

	public override void snapToDefaultClickableComponent()
	{
		if (slotPosition < characterSlots.Count)
		{
			currentlySnappedComponent = characterSlots[slotPosition];
		}
		snapCursorToCurrentSnappedComponent();
	}

	public void updateSlots()
	{
		for (int i = 0; i < characterSlots.Count; i++)
		{
			characterSlots[i].bounds.Y = rowPosition(i - 1);
		}
		int index = 0;
		for (int i = slotPosition; i < slotPosition + 5; i++)
		{
			if (sprites.Count > i)
			{
				int y = yPositionOnScreen + IClickableMenu.borderWidth + 32 + 112 * index + 32;
				sprites[i].bounds.Y = y;
			}
			index++;
		}
		populateClickableComponentList();
		addTabsToClickableComponents();
	}

	public void addTabsToClickableComponents()
	{
		if (Game1.activeClickableMenu is GameMenu gameMenu && !allClickableComponents.Contains(gameMenu.tabs[0]))
		{
			allClickableComponents.AddRange(gameMenu.tabs);
		}
	}

	protected void _SelectSlot(SocialEntry entry)
	{
		bool found = false;
		for (int i = 0; i < SocialEntries.Count; i++)
		{
			SocialEntry cur = SocialEntries[i];
			if (cur.InternalName == entry.InternalName && cur.IsPlayer == entry.IsPlayer && cur.IsChild == entry.IsChild)
			{
				_SelectSlot(characterSlots[i]);
				found = true;
				break;
			}
		}
		if (!found)
		{
			_SelectSlot(characterSlots[0]);
		}
	}

	protected void _SelectSlot(ClickableComponent slot_component)
	{
		if (slot_component != null && characterSlots.Contains(slot_component))
		{
			int index = characterSlots.IndexOf(slot_component as ClickableTextureComponent);
			currentlySnappedComponent = slot_component;
			if (index < slotPosition)
			{
				slotPosition = index;
			}
			else if (index >= slotPosition + 5)
			{
				slotPosition = index - 5 + 1;
			}
			setScrollBarToCurrentIndex();
			updateSlots();
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				snapCursorToCurrentSnappedComponent();
			}
		}
	}

	public void ConstrainSelectionToVisibleSlots()
	{
		if (characterSlots.Contains(currentlySnappedComponent))
		{
			int index = characterSlots.IndexOf(currentlySnappedComponent as ClickableTextureComponent);
			if (index < slotPosition)
			{
				index = slotPosition;
			}
			else if (index >= slotPosition + 5)
			{
				index = slotPosition + 5 - 1;
			}
			currentlySnappedComponent = characterSlots[index];
			if (Game1.options.snappyMenus && Game1.options.gamepadControls)
			{
				snapCursorToCurrentSnappedComponent();
			}
		}
	}

	public override void snapCursorToCurrentSnappedComponent()
	{
		if (currentlySnappedComponent != null && characterSlots.Contains(currentlySnappedComponent))
		{
			Game1.setMousePosition(currentlySnappedComponent.bounds.Left + 64, currentlySnappedComponent.bounds.Center.Y);
		}
		else
		{
			base.snapCursorToCurrentSnappedComponent();
		}
	}

	public override void applyMovementKey(int direction)
	{
		base.applyMovementKey(direction);
		if (characterSlots.Contains(currentlySnappedComponent))
		{
			_SelectSlot(currentlySnappedComponent);
		}
	}

	public override void leftClickHeld(int x, int y)
	{
		base.leftClickHeld(x, y);
		if (scrolling)
		{
			int y2 = scrollBar.bounds.Y;
			scrollBar.bounds.Y = Math.Min(yPositionOnScreen + height - 64 - 12 - scrollBar.bounds.Height, Math.Max(y, yPositionOnScreen + upButton.bounds.Height + 20));
			float percentage = (float)(y - scrollBarRunner.Y) / (float)scrollBarRunner.Height;
			slotPosition = Math.Min(sprites.Count - 5, Math.Max(0, (int)((float)sprites.Count * percentage)));
			setScrollBarToCurrentIndex();
			if (y2 != scrollBar.bounds.Y)
			{
				Game1.playSound("shiny4");
			}
		}
	}

	public override void releaseLeftClick(int x, int y)
	{
		base.releaseLeftClick(x, y);
		scrolling = false;
	}

	private void setScrollBarToCurrentIndex()
	{
		if (sprites.Count > 0)
		{
			scrollBar.bounds.Y = scrollBarRunner.Height / Math.Max(1, sprites.Count - 5 + 1) * slotPosition + upButton.bounds.Bottom + 4;
			if (slotPosition == sprites.Count - 5)
			{
				scrollBar.bounds.Y = downButton.bounds.Y - scrollBar.bounds.Height - 4;
			}
		}
		updateSlots();
	}

	public override void receiveScrollWheelAction(int direction)
	{
		base.receiveScrollWheelAction(direction);
		if (direction > 0 && slotPosition > 0)
		{
			upArrowPressed();
			ConstrainSelectionToVisibleSlots();
			Game1.playSound("shiny4");
		}
		else if (direction < 0 && slotPosition < Math.Max(0, sprites.Count - 5))
		{
			downArrowPressed();
			ConstrainSelectionToVisibleSlots();
			Game1.playSound("shiny4");
		}
	}

	public void upArrowPressed()
	{
		slotPosition--;
		updateSlots();
		upButton.scale = 3.5f;
		setScrollBarToCurrentIndex();
	}

	public void downArrowPressed()
	{
		slotPosition++;
		updateSlots();
		downButton.scale = 3.5f;
		setScrollBarToCurrentIndex();
	}

	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (upButton.containsPoint(x, y) && slotPosition > 0)
		{
			upArrowPressed();
			Game1.playSound("shwip");
			return;
		}
		if (downButton.containsPoint(x, y) && slotPosition < sprites.Count - 5)
		{
			downArrowPressed();
			Game1.playSound("shwip");
			return;
		}
		if (scrollBar.containsPoint(x, y))
		{
			scrolling = true;
			return;
		}
		if (!downButton.containsPoint(x, y) && x > xPositionOnScreen + width && x < xPositionOnScreen + width + 128 && y > yPositionOnScreen && y < yPositionOnScreen + height)
		{
			scrolling = true;
			leftClickHeld(x, y);
			releaseLeftClick(x, y);
			return;
		}
		for (int i = 0; i < characterSlots.Count; i++)
		{
			if (i < slotPosition || i >= slotPosition + 5 || !characterSlots[i].bounds.Contains(x, y))
			{
				continue;
			}
			SocialEntry entry = GetSocialEntry(i);
			if (!entry.IsPlayer && !entry.IsChild)
			{
				Character character = entry.Character;
				if (Game1.player.friendshipData.ContainsKey(character.name))
				{
					Game1.playSound("bigSelect");
					int cached_slot_position = slotPosition;
					ProfileMenu profileMenu = new ProfileMenu(entry, SocialEntries);
					profileMenu.exitFunction = delegate
					{
						if (((GameMenu)(Game1.activeClickableMenu = new GameMenu(GameMenu.socialTab, -1, playOpeningSound: false))).GetCurrentPage() is SocialPage socialPage)
						{
							socialPage.slotPosition = cached_slot_position;
							socialPage._SelectSlot(profileMenu.Current);
						}
					};
					Game1.activeClickableMenu = profileMenu;
					if (Game1.options.SnappyMenus)
					{
						profileMenu.snapToDefaultClickableComponent();
					}
					return;
				}
			}
			Game1.playSound("shiny4");
			break;
		}
		slotPosition = Math.Max(0, Math.Min(sprites.Count - 5, slotPosition));
	}

	public override void performHoverAction(int x, int y)
	{
		hoverText = "";
		upButton.tryHover(x, y);
		downButton.tryHover(x, y);
	}

	private bool isCharacterSlotClickable(int i)
	{
		SocialEntry entry = GetSocialEntry(i);
		if (entry != null && !entry.IsPlayer && !entry.IsChild)
		{
			return entry.IsMet;
		}
		return false;
	}

	/// <summary>Draw an NPC's entry in the social page.</summary>
	/// <param name="b">The sprite batch being drawn.</param>
	/// <param name="i">The index of the NPC in <see cref="F:StardewValley.Menus.SocialPage.sprites" />.</param>
	private void drawNPCSlot(SpriteBatch b, int i)
	{
		SocialEntry entry = GetSocialEntry(i);
		if (entry == null)
		{
			return;
		}
		if (isCharacterSlotClickable(i) && characterSlots[i].bounds.Contains(Game1.getMouseX(), Game1.getMouseY()))
		{
			b.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth - 4, sprites[i].bounds.Y - 4, characterSlots[i].bounds.Width, characterSlots[i].bounds.Height - 12), Color.White * 0.25f);
		}
		sprites[i].draw(b);
		string name = entry.InternalName;
		Gender gender = entry.Gender;
		bool datable = entry.IsDatable;
		bool isDating = entry.IsDatingCurrentPlayer();
		bool isCurrentSpouse = entry.IsMarriedToCurrentPlayer();
		bool housemate = entry.IsRoommateForCurrentPlayer();
		float lineHeight = Game1.smallFont.MeasureString("W").Y;
		float russianOffsetY = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? ((0f - lineHeight) / 2f) : 0f);
		b.DrawString(Game1.dialogueFont, entry.DisplayName, new Vector2((float)(xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 64 - 20 + 96) - Game1.dialogueFont.MeasureString(entry.DisplayName).X / 2f, (float)(sprites[i].bounds.Y + 48) + russianOffsetY - (float)(datable ? 24 : 20)), Game1.textColor);
		for (int hearts = 0; hearts < Math.Max(Utility.GetMaximumHeartsForCharacter(Game1.getCharacterFromName(name)), 10); hearts++)
		{
			drawNPCSlotHeart(b, i, entry, hearts, isDating, isCurrentSpouse);
		}
		if (datable || housemate)
		{
			string text = ((!Game1.content.ShouldUseGenderedCharacterTranslations()) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635") : ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/')[0] : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').Last()));
			if (housemate)
			{
				text = Game1.content.LoadString("Strings\\StringsFromCSFiles:Housemate");
			}
			else if (isCurrentSpouse)
			{
				text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11636") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11637"));
			}
			else if (entry.IsMarriedToAnyone())
			{
				text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_MaleNPC") : Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_FemaleNPC"));
			}
			else if (!Game1.player.isMarriedOrRoommates() && isDating)
			{
				text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11639") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11640"));
			}
			else if (entry.IsDivorcedFromCurrentPlayer())
			{
				text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11642") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11643"));
			}
			int width = (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2;
			text = Game1.parseText(text, Game1.smallFont, width);
			Vector2 textSize = Game1.smallFont.MeasureString(text);
			b.DrawString(Game1.smallFont, text, new Vector2((float)(xPositionOnScreen + 192 + 8) - textSize.X / 2f, (float)sprites[i].bounds.Bottom - (textSize.Y - lineHeight)), Game1.textColor);
		}
		if (!isCurrentSpouse && !entry.IsChild)
		{
			Utility.drawWithShadow(b, Game1.mouseCursors2, new Vector2(xPositionOnScreen + 384 + 304, sprites[i].bounds.Y - 4), new Rectangle(166, 174, 14, 12), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f, 0, -1, 0.2f);
			Texture2D mouseCursors = Game1.mouseCursors;
			Vector2 position = new Vector2(xPositionOnScreen + 384 + 296, sprites[i].bounds.Y + 32 + 20);
			Friendship friendship = entry.Friendship;
			b.Draw(mouseCursors, position, new Rectangle(227 + ((friendship != null && friendship.GiftsThisWeek >= 2) ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			Texture2D mouseCursors2 = Game1.mouseCursors;
			Vector2 position2 = new Vector2(xPositionOnScreen + 384 + 336, sprites[i].bounds.Y + 32 + 20);
			Friendship friendship2 = entry.Friendship;
			b.Draw(mouseCursors2, position2, new Rectangle(227 + ((friendship2 != null && friendship2.GiftsThisWeek >= 1) ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
			Utility.drawWithShadow(b, Game1.mouseCursors2, new Vector2(xPositionOnScreen + 384 + 424, sprites[i].bounds.Y), new Rectangle(180, 175, 13, 11), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.88f, 0, -1, 0.2f);
			Texture2D mouseCursors3 = Game1.mouseCursors;
			Vector2 position3 = new Vector2(xPositionOnScreen + 384 + 432, sprites[i].bounds.Y + 32 + 20);
			Friendship friendship3 = entry.Friendship;
			b.Draw(mouseCursors3, position3, new Rectangle(227 + ((friendship3 != null && friendship3.TalkedToToday) ? 9 : 0), 425, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		}
		if (isCurrentSpouse)
		{
			if (!housemate || name == "Krobus")
			{
				b.Draw(Game1.objectSpriteSheet, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, housemate ? 808 : 460, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
			}
		}
		else if (isDating)
		{
			b.Draw(Game1.objectSpriteSheet, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, housemate ? 808 : 458, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
		}
	}

	/// <summary>Draw the heart sprite for an NPC's entry in the social page.</summary>
	/// <param name="b">The sprite batch being drawn.</param>
	/// <param name="npcIndex">The index of the NPC in <see cref="F:StardewValley.Menus.SocialPage.sprites" />.</param>
	/// <param name="entry">The NPC's cached social data.</param>
	/// <param name="hearts">The current heart index being drawn (starting at 0 for the first heart).</param>
	/// <param name="isDating">Whether the player is currently dating this NPC.</param>
	/// <param name="isCurrentSpouse">Whether the player is currently married to this NPC.</param>
	private void drawNPCSlotHeart(SpriteBatch b, int npcIndex, SocialEntry entry, int hearts, bool isDating, bool isCurrentSpouse)
	{
		bool isLockedHeart = entry.IsDatable && !isDating && !isCurrentSpouse && hearts >= 8;
		int heartX = ((hearts < entry.HeartLevel || isLockedHeart) ? 211 : 218);
		Color heartTint = ((hearts < 10 && isLockedHeart) ? (Color.Black * 0.35f) : Color.White);
		if (hearts < 10)
		{
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 320 - 4 + hearts * 32, sprites[npcIndex].bounds.Y + 64 - 28), new Rectangle(heartX, 428, 7, 6), heartTint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		}
		else
		{
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 320 - 4 + (hearts - 10) * 32, sprites[npcIndex].bounds.Y + 64), new Rectangle(heartX, 428, 7, 6), heartTint, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		}
	}

	private int rowPosition(int i)
	{
		int j = i - slotPosition;
		int rowHeight = 112;
		return yPositionOnScreen + IClickableMenu.borderWidth + 160 + 4 + j * rowHeight;
	}

	private void drawFarmerSlot(SpriteBatch b, int i)
	{
		SocialEntry entry = GetSocialEntry(i);
		if (entry == null)
		{
			return;
		}
		if (!entry.IsPlayer)
		{
			Game1.log.Warn($"Social page can't draw farmer slot for index {i}: this is NPC '{entry.InternalName}', not a farmer.");
			return;
		}
		Farmer farmer = (Farmer)entry.Character;
		Gender gender = entry.Gender;
		ClickableTextureComponent clickableTextureComponent = sprites[i];
		int x = clickableTextureComponent.bounds.X;
		int y = clickableTextureComponent.bounds.Y;
		Rectangle origClip = b.GraphicsDevice.ScissorRectangle;
		Rectangle newClip = origClip;
		newClip.Height = Math.Min(newClip.Bottom, rowPosition(i)) - newClip.Y - 4;
		b.GraphicsDevice.ScissorRectangle = newClip;
		FarmerRenderer.isDrawingForUI = true;
		try
		{
			farmer.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(farmer.bathingClothes ? 108 : 0, 0, secondaryArm: false, flip: false), farmer.bathingClothes ? 108 : 0, new Rectangle(0, farmer.bathingClothes ? 576 : 0, 16, 32), new Vector2(x, y), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, farmer);
		}
		finally
		{
			b.GraphicsDevice.ScissorRectangle = origClip;
		}
		FarmerRenderer.isDrawingForUI = false;
		bool num = entry.IsMarriedToCurrentPlayer();
		float lineHeight = Game1.smallFont.MeasureString("W").Y;
		float russianOffsetY = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? ((0f - lineHeight) / 2f) : 0f);
		b.DrawString(Game1.dialogueFont, farmer.Name, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 96 - 20, (float)(sprites[i].bounds.Y + 48) + russianOffsetY - 24f), Game1.textColor);
		string text = ((!Game1.content.ShouldUseGenderedCharacterTranslations()) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635") : ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/')[0] : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11635").Split('/').Last()));
		if (num)
		{
			text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11636") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11637"));
		}
		else if (farmer.isMarriedOrRoommates() && !farmer.hasRoommate())
		{
			text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_MaleNPC") : Game1.content.LoadString("Strings\\UI:SocialPage_MarriedToOtherPlayer_FemaleNPC"));
		}
		else if (!Game1.player.isMarriedOrRoommates() && entry.IsDatingCurrentPlayer())
		{
			text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11639") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11640"));
		}
		else if (entry.IsDivorcedFromCurrentPlayer())
		{
			text = ((gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11642") : Game1.content.LoadString("Strings\\StringsFromCSFiles:SocialPage.cs.11643"));
		}
		text = Game1.parseText(width: (IClickableMenu.borderWidth * 3 + 128 - 40 + 192) / 2, text: text, whichFont: Game1.smallFont);
		Vector2 textSize = Game1.smallFont.MeasureString(text);
		b.DrawString(Game1.smallFont, text, new Vector2((float)(xPositionOnScreen + 192 + 8) - textSize.X / 2f, (float)sprites[i].bounds.Bottom - (textSize.Y - lineHeight)), Game1.textColor);
		if (num)
		{
			b.Draw(Game1.objectSpriteSheet, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 801, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
		}
		else if (entry.IsDatingCurrentPlayer())
		{
			b.Draw(Game1.objectSpriteSheet, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth * 7 / 4 + 192, sprites[i].bounds.Y), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 458, 16, 16), Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.88f);
		}
	}

	public override void draw(SpriteBatch b)
	{
		b.End();
		b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
		drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + 128 + 4, small: true);
		drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + 192 + 32 + 20, small: true);
		drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + 320 + 36, small: true);
		drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + 384 + 32 + 52, small: true);
		for (int i = slotPosition; i < slotPosition + 5 && i < sprites.Count; i++)
		{
			SocialEntry entry = GetSocialEntry(i);
			if (entry != null)
			{
				if (entry.IsPlayer)
				{
					drawFarmerSlot(b, i);
				}
				else
				{
					drawNPCSlot(b, i);
				}
			}
		}
		Rectangle origClip = b.GraphicsDevice.ScissorRectangle;
		Rectangle newClip = origClip;
		newClip.Y = Math.Max(0, rowPosition(numFarmers - 1));
		newClip.Height -= newClip.Y;
		if (newClip.Height > 0)
		{
			b.GraphicsDevice.ScissorRectangle = newClip;
			try
			{
				drawVerticalPartition(b, xPositionOnScreen + 256 + 12, small: true);
				drawVerticalPartition(b, xPositionOnScreen + 384 + 368, small: true);
				drawVerticalPartition(b, xPositionOnScreen + 256 + 12 + 352, small: true);
			}
			finally
			{
				b.GraphicsDevice.ScissorRectangle = origClip;
			}
		}
		upButton.draw(b);
		downButton.draw(b);
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f);
		scrollBar.draw(b);
		if (!hoverText.Equals(""))
		{
			IClickableMenu.drawHoverText(b, hoverText, Game1.smallFont);
		}
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
	}
}
