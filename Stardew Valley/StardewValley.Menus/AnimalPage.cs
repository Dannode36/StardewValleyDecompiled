using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Characters;

namespace StardewValley.Menus;

public class AnimalPage : IClickableMenu
{
	/// <summary>An entry on the social page.</summary>
	public class AnimalEntry
	{
		/// <summary>The character instance.</summary>
		public Character Animal;

		/// <summary>The unique multiplayer ID for a player, or the internal name for an NPC.</summary>
		public readonly string InternalName;

		/// <summary>The translated display name.</summary>
		public readonly string DisplayName;

		public readonly string AnimalType;

		public readonly string AnimalBaseType;

		/// <summary>The current player's heart level with this animal. -1 means friendship is not tracked.</summary>
		public readonly int FriendshipLevel = -1;

		public readonly bool ReceivedAnimalCracker;

		/// <summary>
		/// 0 is no, 1 is auto-pet, 2 is hand pet
		/// </summary>
		public readonly int WasPetYet;

		public readonly int special;

		public Texture2D Texture;

		public Rectangle TextureSourceRect;

		/// <summary>Construct an instance.</summary>
		/// <param name="player">The player for which to create an entry.</param>
		/// <param name="friendship">The current player's friendship with this character.</param>
		public AnimalEntry(Character animal)
		{
			Animal = animal;
			DisplayName = animal.displayName;
			if (animal is FarmAnimal farmAnimal)
			{
				InternalName = farmAnimal.myID?.ToString() ?? "";
				FriendshipLevel = farmAnimal.friendshipTowardFarmer.Value;
				Texture = farmAnimal.Sprite.Texture;
				if (farmAnimal.Sprite.SourceRect.Height > 16)
				{
					if (farmAnimal.type.Equals("Ostrich"))
					{
						TextureSourceRect = new Rectangle(0, farmAnimal.Sprite.SourceRect.Height * 2 - 32, farmAnimal.Sprite.SourceRect.Width, 28);
					}
					else
					{
						TextureSourceRect = new Rectangle(0, farmAnimal.Sprite.SourceRect.Height * 2 - 28, farmAnimal.Sprite.SourceRect.Width, 28);
					}
				}
				else
				{
					TextureSourceRect = new Rectangle(0, 16, 16, 16);
				}
				AnimalType = farmAnimal.type.Value;
				if (AnimalType.Contains(' '))
				{
					AnimalBaseType = AnimalType.Split(' ')[1];
				}
				else
				{
					AnimalBaseType = AnimalType;
				}
				WasPetYet = (farmAnimal.wasPet.Value ? 2 : (farmAnimal.wasAutoPet.Value ? 1 : 0));
				ReceivedAnimalCracker = farmAnimal.hasEatenAnimalCracker.Value;
			}
			else if (animal is Pet pet)
			{
				InternalName = pet.petId?.ToString() ?? "";
				FriendshipLevel = pet.friendshipTowardFarmer.Value;
				Texture = pet.Sprite.Texture;
				TextureSourceRect = new Rectangle(0, pet.Sprite.SourceRect.Height * 2 - 24, pet.Sprite.SourceRect.Width, 24);
				AnimalType = pet.petType.Value;
				WasPetYet = (pet.grantedFriendshipForPet.Value ? 2 : 0);
			}
			else if (animal is Horse horse)
			{
				InternalName = horse.HorseId.ToString();
				Texture = horse.Sprite.Texture;
				TextureSourceRect = new Rectangle(0, horse.Sprite.SourceRect.Height * 2 - 26, horse.Sprite.SourceRect.Width, 24);
				AnimalType = "Horse";
				WasPetYet = -1;
				special = (horse.ateCarrotToday ? 1 : 0);
			}
		}
	}

	public const int slotsOnPage = 5;

	private string hoverText = "";

	private ClickableTextureComponent upButton;

	private ClickableTextureComponent downButton;

	private ClickableTextureComponent scrollBar;

	private Rectangle scrollBarRunner;

	/// <summary>The players and social NPCs shown in the list.</summary>
	public List<AnimalEntry> AnimalEntries;

	/// <summary>The character portrait components.</summary>
	private readonly List<ClickableTextureComponent> sprites = new List<ClickableTextureComponent>();

	/// <summary>The index of the <see cref="F:StardewValley.Menus.AnimalPage.AnimalEntries" /> entry shown at the top of the scrolled view.</summary>
	private int slotPosition;

	/// <summary>The clickable slots over which character info is drawn.</summary>
	public readonly List<ClickableTextureComponent> characterSlots = new List<ClickableTextureComponent>();

	private bool scrolling;

	public AnimalPage(int x, int y, int width, int height)
		: base(x, y, width, height)
	{
	}

	public void init()
	{
		AnimalEntries = FindAnimals();
		CreateComponents();
		slotPosition = 0;
		setScrollBarToCurrentIndex();
		updateSlots();
	}

	public override void populateClickableComponentList()
	{
		init();
		base.populateClickableComponentList();
	}

	/// <summary>Find all social NPCs which should be shown on the social page.</summary>
	public List<AnimalEntry> FindAnimals()
	{
		List<AnimalEntry> pets = new List<AnimalEntry>();
		List<AnimalEntry> farmAnimals = new List<AnimalEntry>();
		List<AnimalEntry> horses = new List<AnimalEntry>();
		foreach (Character animal in GetAllAnimals())
		{
			if (animal is Pet)
			{
				pets.Add(new AnimalEntry(animal));
			}
			else if (animal is Horse)
			{
				horses.Add(new AnimalEntry(animal));
			}
			else
			{
				farmAnimals.Add(new AnimalEntry(animal));
			}
		}
		foreach (Farmer f in Game1.getAllFarmers())
		{
			if (f.mount != null)
			{
				horses.Add(new AnimalEntry(f.mount));
			}
		}
		List<AnimalEntry> list = new List<AnimalEntry>();
		list.AddRange(pets);
		list.AddRange(horses);
		list.AddRange(from entry in farmAnimals
			orderby entry.AnimalBaseType, entry.AnimalType, entry.FriendshipLevel descending
			select entry);
		return list;
	}

	/// <summary>Get all animals from the world and friendship data.</summary>
	public IEnumerable<Character> GetAllAnimals()
	{
		List<Character> animals = new List<Character>();
		Utility.ForEachLocation(delegate(GameLocation location)
		{
			foreach (NPC current in location.characters)
			{
				if ((current is Pet || current is Horse) && !current.hideFromAnimalSocialMenu.Value)
				{
					animals.Add(current);
				}
			}
			foreach (FarmAnimal current2 in location.animals.Values)
			{
				if (!current2.hideFromAnimalSocialMenu.Value)
				{
					animals.Add(current2);
				}
			}
			return true;
		});
		return animals;
	}

	/// <summary>Load the clickable components to display.</summary>
	public void CreateComponents()
	{
		sprites.Clear();
		characterSlots.Clear();
		for (int i = 0; i < AnimalEntries.Count; i++)
		{
			sprites.Add(CreateSpriteComponent(AnimalEntries[i], i));
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
	public ClickableTextureComponent CreateSpriteComponent(AnimalEntry entry, int index)
	{
		Rectangle bounds = new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth + 4, 0, width, 64);
		Rectangle sourceRect = entry.TextureSourceRect;
		if (sourceRect.Height <= 16)
		{
			bounds.Height--;
			bounds.X += 24;
		}
		return new ClickableTextureComponent(index.ToString(), bounds, null, "", entry.Texture, sourceRect, 4f);
	}

	/// <summary>Get the social entry from its index in the list.</summary>
	/// <param name="index">The index in the social list.</param>
	public AnimalEntry GetSocialEntry(int index)
	{
		if (index < 0 || index >= AnimalEntries.Count)
		{
			index = 0;
		}
		if (AnimalEntries.Count == 0)
		{
			return null;
		}
		return AnimalEntries[index];
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
			if (slotPosition >= 0 && sprites.Count > i)
			{
				int y = yPositionOnScreen + IClickableMenu.borderWidth + 32 + 112 * index + 16;
				if (sprites[i].bounds.Height < 64)
				{
					y += 48;
				}
				sprites[i].bounds.Y = y;
			}
			index++;
		}
		base.populateClickableComponentList();
		addTabsToClickableComponents();
	}

	public void addTabsToClickableComponents()
	{
		if (Game1.activeClickableMenu is GameMenu gameMenu && !allClickableComponents.Contains(gameMenu.tabs[0]))
		{
			allClickableComponents.AddRange(gameMenu.tabs);
		}
	}

	protected void _SelectSlot(AnimalEntry entry)
	{
		bool found = false;
		for (int i = 0; i < AnimalEntries.Count; i++)
		{
			if (AnimalEntries[i].InternalName == entry.InternalName)
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
			if (i >= slotPosition)
			{
				_ = slotPosition + 5;
			}
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
		GetSocialEntry(i);
		return false;
	}

	private void drawNPCSlot(SpriteBatch b, int i)
	{
		AnimalEntry entry = GetSocialEntry(i);
		if (entry == null || i < 0)
		{
			return;
		}
		if (isCharacterSlotClickable(i) && characterSlots[i].bounds.Contains(Game1.getMouseX(), Game1.getMouseY()))
		{
			b.Draw(Game1.staminaRect, new Rectangle(xPositionOnScreen + IClickableMenu.borderWidth - 4, sprites[i].bounds.Y - 4, characterSlots[i].bounds.Width, characterSlots[i].bounds.Height - 12), Color.White * 0.25f);
		}
		sprites[i].draw(b);
		_ = entry.InternalName;
		_ = entry.FriendshipLevel;
		float lineHeight = Game1.smallFont.MeasureString("W").Y;
		float russianOffsetY = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? ((0f - lineHeight) / 2f) : 0f);
		int yOffset = ((entry.TextureSourceRect.Height <= 16) ? (-40) : 8);
		b.DrawString(Game1.dialogueFont, entry.DisplayName, new Vector2(xPositionOnScreen + IClickableMenu.borderWidth * 3 / 2 + 192 - 20 + 96 - (int)(Game1.dialogueFont.MeasureString(entry.DisplayName).X / 2f), (float)(sprites[i].bounds.Y + 48 + yOffset) + russianOffsetY - 20f), Game1.textColor);
		if (entry.FriendshipLevel != -1)
		{
			double loveLevel = (float)entry.FriendshipLevel / 1000f;
			int halfHeart = (int)((loveLevel * 1000.0 % 200.0 >= 100.0) ? (loveLevel * 1000.0 / 200.0) : (-100.0));
			int heartYOffset = (entry.ReceivedAnimalCracker ? (-24) : 0);
			for (int hearts = 0; hearts < 5; hearts++)
			{
				b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 512 - 4 + hearts * 32, sprites[i].bounds.Y + heartYOffset + yOffset + 64 - 24), new Rectangle(211 + ((loveLevel * 1000.0 <= (double)((hearts + 1) * 195)) ? 7 : 0), 428, 7, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
				if (halfHeart == hearts)
				{
					b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 512 - 4 + hearts * 32, sprites[i].bounds.Y + heartYOffset + yOffset + 64 - 24), new Rectangle(211, 428, 4, 6), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.891f);
				}
			}
		}
		if (entry.WasPetYet != -1)
		{
			b.Draw(Game1.mouseCursors, new Vector2(xPositionOnScreen + 704 - 4, sprites[i].bounds.Y + yOffset + 64 - 52), new Rectangle(32, 0, 10, 10), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
			b.Draw(Game1.mouseCursors_1_6, new Vector2(xPositionOnScreen + 704 - 4, sprites[i].bounds.Y + yOffset + 64 - 8), new Rectangle(273 + entry.WasPetYet * 9, 253, 9, 9), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
		}
		if (entry.special == 1)
		{
			Utility.drawWithShadow(b, Game1.objectSpriteSheet_2, new Vector2(xPositionOnScreen + 704 - 16, sprites[i].bounds.Y + yOffset + 64 - 52), new Rectangle(0, 160, 16, 16), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.8f, 0, 8);
		}
		if (entry.ReceivedAnimalCracker)
		{
			Utility.drawWithShadow(b, Game1.objectSpriteSheet_2, new Vector2(xPositionOnScreen + 576 - 20, sprites[i].bounds.Y + yOffset + 64 - 16), new Rectangle(16, 242, 15, 11), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.8f);
		}
	}

	private int rowPosition(int i)
	{
		int j = i - slotPosition;
		int rowHeight = 112;
		return yPositionOnScreen + IClickableMenu.borderWidth + 160 + 4 + j * rowHeight;
	}

	public override void draw(SpriteBatch b)
	{
		b.End();
		b.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
		if (sprites.Count > 0)
		{
			drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + 128 + 4, small: true);
		}
		if (sprites.Count > 1)
		{
			drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + 192 + 32 + 20, small: true);
		}
		if (sprites.Count > 2)
		{
			drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + 320 + 36, small: true);
		}
		if (sprites.Count > 3)
		{
			drawHorizontalPartition(b, yPositionOnScreen + IClickableMenu.borderWidth + 384 + 32 + 52, small: true);
		}
		for (int i = slotPosition; i < slotPosition + 5 && i < sprites.Count; i++)
		{
			if (GetSocialEntry(i) != null)
			{
				drawNPCSlot(b, i);
			}
		}
		Rectangle newClip = b.GraphicsDevice.ScissorRectangle;
		newClip.Y = Math.Max(0, rowPosition(4 - sprites.Count));
		newClip.Height -= newClip.Y;
		if (newClip.Height > 0)
		{
			int heightOverride = ((sprites.Count >= 5) ? (-1) : ((108 + sprites.Count) * sprites.Count));
			drawVerticalPartition(b, xPositionOnScreen + 448 + 12, small: true, -1, -1, -1, heightOverride);
			drawVerticalPartition(b, xPositionOnScreen + 256 + 12 + 376, small: true, -1, -1, -1, heightOverride);
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
