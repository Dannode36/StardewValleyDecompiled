using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects;

public class FishTankFurniture : StorageFurniture
{
	public enum FishTankCategories
	{
		None,
		Swim,
		Ground,
		Decoration
	}

	public const int TANK_DEPTH = 10;

	public const int FLOOR_DECORATION_OFFSET = 4;

	public const int TANK_SORT_REGION = 20;

	[XmlIgnore]
	public List<Vector4> bubbles = new List<Vector4>();

	[XmlIgnore]
	public List<TankFish> tankFish = new List<TankFish>();

	[XmlIgnore]
	public NetEvent0 refreshFishEvent = new NetEvent0();

	[XmlIgnore]
	public bool fishDirty = true;

	[XmlIgnore]
	private Texture2D _aquariumTexture;

	[XmlIgnore]
	public List<KeyValuePair<Rectangle, Vector2>?> floorDecorations = new List<KeyValuePair<Rectangle, Vector2>?>();

	[XmlIgnore]
	public List<Vector2> decorationSlots = new List<Vector2>();

	[XmlIgnore]
	public List<int> floorDecorationIndices = new List<int>();

	public NetInt generationSeed = new NetInt();

	[XmlIgnore]
	public Item localDepositedItem;

	[XmlIgnore]
	protected int _currentDecorationIndex;

	protected Dictionary<Item, TankFish> _fishLookup = new Dictionary<Item, TankFish>();

	public FishTankFurniture()
	{
		generationSeed.Value = Game1.random.Next();
	}

	public FishTankFurniture(string itemId, Vector2 tile, int initialRotations)
		: base(itemId, tile, initialRotations)
	{
		generationSeed.Value = Game1.random.Next();
	}

	public FishTankFurniture(string itemId, Vector2 tile)
		: base(itemId, tile)
	{
		generationSeed.Value = Game1.random.Next();
	}

	/// <inheritdoc />
	public override void actionOnPlayerEntryOrPlacement(GameLocation environment, bool dropDown)
	{
		base.actionOnPlayerEntryOrPlacement(environment, dropDown);
		ResetFish();
		UpdateFish();
	}

	public virtual void ResetFish()
	{
		bubbles.Clear();
		tankFish.Clear();
		_fishLookup.Clear();
		UpdateFish();
	}

	public Texture2D GetAquariumTexture()
	{
		if (_aquariumTexture == null)
		{
			_aquariumTexture = Game1.content.Load<Texture2D>("LooseSprites\\AquariumFish");
		}
		return _aquariumTexture;
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(generationSeed, "generationSeed").AddField(refreshFishEvent, "refreshFishEvent");
		refreshFishEvent.onEvent += UpdateDecorAndFish;
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new FishTankFurniture(base.ItemId, tileLocation.Value);
	}

	public virtual int GetCapacityForCategory(FishTankCategories category)
	{
		int extra = 0;
		if (base.QualifiedItemId.Equals("(F)JungleTank"))
		{
			extra++;
		}
		switch (category)
		{
		case FishTankCategories.Swim:
			return getTilesWide() - 1;
		case FishTankCategories.Ground:
			return getTilesWide() - 1 + extra;
		case FishTankCategories.Decoration:
			if (getTilesWide() <= 2)
			{
				return 1;
			}
			return -1;
		default:
			return 0;
		}
	}

	public FishTankCategories GetCategoryFromItem(Item item)
	{
		Dictionary<string, string> aquarium_data = GetAquariumData();
		if (!CanBeDeposited(item))
		{
			return FishTankCategories.None;
		}
		if (item.QualifiedItemId == "(TR)FrogEgg")
		{
			return FishTankCategories.Ground;
		}
		if (aquarium_data.TryGetValue(item.ItemId, out var rawData))
		{
			switch (ArgUtility.Get(rawData.Split('/'), 1))
			{
			case "crawl":
			case "ground":
			case "front_crawl":
			case "static":
				return FishTankCategories.Ground;
			default:
				return FishTankCategories.Swim;
			}
		}
		return FishTankCategories.Decoration;
	}

	public bool HasRoomForThisItem(Item item)
	{
		if (!CanBeDeposited(item))
		{
			return false;
		}
		FishTankCategories category = GetCategoryFromItem(item);
		int capacity = GetCapacityForCategory(category);
		if (item is Hat)
		{
			capacity = 999;
		}
		if (capacity < 0)
		{
			foreach (Item held_item in heldItems)
			{
				if (held_item != null && held_item.QualifiedItemId == item.QualifiedItemId)
				{
					return false;
				}
			}
			return true;
		}
		int current_count = 0;
		foreach (Item held_item in heldItems)
		{
			if (held_item != null)
			{
				if (GetCategoryFromItem(held_item) == category)
				{
					current_count++;
				}
				if (current_count >= capacity)
				{
					return false;
				}
			}
		}
		return true;
	}

	public override string GetShopMenuContext()
	{
		return "FishTank";
	}

	public override void ShowMenu()
	{
		ShowShopMenu();
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		GameLocation location = Location;
		if (location == null)
		{
			return false;
		}
		if (justCheckingForActivity)
		{
			return true;
		}
		if (mutex.IsLocked())
		{
			return true;
		}
		if ((who.ActiveObject != null || who.CurrentItem is Hat || who.CurrentItem?.QualifiedItemId == "(TR)FrogEgg") && localDepositedItem == null && CanBeDeposited(who.CurrentItem))
		{
			if (!HasRoomForThisItem(who.CurrentItem))
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishTank_Full"));
				return true;
			}
			localDepositedItem = who.CurrentItem.getOne();
			who.CurrentItem.Stack--;
			if (who.CurrentItem.Stack <= 0 || who.CurrentItem is Hat)
			{
				who.removeItemFromInventory(who.CurrentItem);
				who.showNotCarrying();
			}
			mutex.RequestLock(delegate
			{
				location.playSound("dropItemInWater");
				heldItems.Add(localDepositedItem);
				localDepositedItem = null;
				refreshFishEvent.Fire();
				mutex.ReleaseLock();
			}, delegate
			{
				localDepositedItem = who.addItemToInventory(localDepositedItem);
				if (localDepositedItem != null)
				{
					Game1.createItemDebris(localDepositedItem, new Vector2(TileLocation.X + (float)getTilesWide() / 2f + 0.5f, TileLocation.Y + 0.5f) * 64f, -1, location);
				}
				localDepositedItem = null;
			});
			return true;
		}
		mutex.RequestLock(ShowMenu);
		return true;
	}

	public virtual bool CanBeDeposited(Item item)
	{
		if (item == null)
		{
			return false;
		}
		if (item.QualifiedItemId == "(TR)FrogEgg")
		{
			return true;
		}
		if (!(item is Hat) && !Utility.IsNormalObjectAtParentSheetIndex(item, item.ItemId))
		{
			return false;
		}
		if (item.QualifiedItemId == "(O)152" || item.QualifiedItemId == "(O)393" || item.QualifiedItemId == "(O)390" || item.QualifiedItemId == "(O)117" || item.QualifiedItemId == "(O)166" || item.QualifiedItemId == "(O)832" || item.QualifiedItemId == "(O)109" || item.QualifiedItemId == "(O)709" || item.QualifiedItemId == "(O)392" || item.QualifiedItemId == "(O)394" || item.QualifiedItemId == "(O)167" || item.QualifiedItemId == "(O)789" || item.QualifiedItemId == "(O)330" || item.QualifiedItemId == "(O)797")
		{
			return true;
		}
		if (item is Hat)
		{
			int numHatWearers = 0;
			int numHats = 0;
			foreach (TankFish item2 in tankFish)
			{
				if (item2.CanWearHat())
				{
					numHatWearers++;
				}
			}
			foreach (Item heldItem in heldItems)
			{
				if (heldItem is Hat)
				{
					numHats++;
				}
			}
			return numHats < numHatWearers;
		}
		return GetAquariumData().ContainsKey(item.ItemId);
	}

	public override void DayUpdate()
	{
		ResetFish();
		base.DayUpdate();
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		GameLocation environment = Location;
		if (Game1.currentLocation == environment)
		{
			if (fishDirty)
			{
				fishDirty = false;
				UpdateDecorAndFish();
			}
			foreach (TankFish item in tankFish)
			{
				item.Update(time);
			}
			for (int i = 0; i < bubbles.Count; i++)
			{
				Vector4 bubble = bubbles[i];
				bubble.W += 0.05f;
				if (bubble.W > 1f)
				{
					bubble.W = 1f;
				}
				bubble.Y += bubble.W;
				bubbles[i] = bubble;
				if (bubble.Y >= (float)GetTankBounds().Height)
				{
					bubbles.RemoveAt(i);
					i--;
				}
			}
		}
		base.updateWhenCurrentLocation(time);
		refreshFishEvent.Poll();
	}

	/// <inheritdoc />
	public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
	{
		generationSeed.Value = Game1.random.Next();
		fishDirty = true;
		return base.placementAction(location, x, y, who);
	}

	public Dictionary<string, string> GetAquariumData()
	{
		return DataLoader.AquariumFish(Game1.content);
	}

	public override bool onDresserItemWithdrawn(ISalable salable, Farmer who, int amount)
	{
		bool result = base.onDresserItemWithdrawn(salable, who, amount);
		refreshFishEvent.Fire();
		return result;
	}

	public virtual void UpdateFish()
	{
		List<Item> fish_items = new List<Item>();
		Dictionary<string, string> aquarium_data = GetAquariumData();
		foreach (Item item in heldItems)
		{
			if (item != null)
			{
				if (item is Object o)
				{
					o.reloadSprite();
				}
				bool forceValid = item.QualifiedItemId == "(TR)FrogEgg";
				if ((forceValid || Utility.IsNormalObjectAtParentSheetIndex(item, item.ItemId)) && (forceValid || aquarium_data.ContainsKey(item.ItemId)))
				{
					fish_items.Add(item);
				}
			}
		}
		List<Item> items_to_remove = new List<Item>();
		foreach (Item key in _fishLookup.Keys)
		{
			if (!heldItems.Contains(key))
			{
				items_to_remove.Add(key);
			}
		}
		for (int i = 0; i < fish_items.Count; i++)
		{
			Item item = fish_items[i];
			if (!_fishLookup.ContainsKey(item))
			{
				TankFish fish = new TankFish(this, item);
				tankFish.Add(fish);
				_fishLookup[item] = fish;
			}
		}
		foreach (Item removed_item in items_to_remove)
		{
			tankFish.Remove(_fishLookup[removed_item]);
			heldItems.Remove(removed_item);
		}
	}

	public virtual void UpdateDecorAndFish()
	{
		Random r = Utility.CreateRandom(generationSeed.Value);
		UpdateFish();
		decorationSlots.Clear();
		for (int y = 0; y < 3; y++)
		{
			for (int x = 0; x < getTilesWide(); x++)
			{
				Vector2 slot_position = default(Vector2);
				if (y % 2 == 0)
				{
					if (x == getTilesWide() - 1)
					{
						continue;
					}
					slot_position.X = 16 + x * 16;
				}
				else
				{
					slot_position.X = 8 + x * 16;
				}
				slot_position.Y = 4f;
				slot_position.Y += 3.3333333f * (float)y;
				decorationSlots.Add(slot_position);
			}
		}
		floorDecorationIndices.Clear();
		floorDecorations.Clear();
		_currentDecorationIndex = 0;
		for (int i = 0; i < decorationSlots.Count; i++)
		{
			floorDecorationIndices.Add(i);
			floorDecorations.Add(null);
		}
		Utility.Shuffle(r, floorDecorationIndices);
		Random decoration_random = Utility.CreateRandom(r.Next());
		bool add_decoration = GetItemCount("393") > 0;
		for (int i = 0; i < 1; i++)
		{
			if (add_decoration)
			{
				AddFloorDecoration(new Rectangle(16 * decoration_random.Next(0, 5), 256, 16, 16));
			}
			else
			{
				_AdvanceDecorationIndex();
			}
		}
		decoration_random = Utility.CreateRandom(r.Next());
		add_decoration = GetItemCount("152") > 0;
		for (int i = 0; i < 4; i++)
		{
			if (add_decoration)
			{
				AddFloorDecoration(new Rectangle(16 * decoration_random.Next(0, 3), 288, 16, 16));
			}
			else
			{
				_AdvanceDecorationIndex();
			}
		}
		decoration_random = Utility.CreateRandom(r.Next());
		add_decoration = GetItemCount("390") > 0;
		for (int i = 0; i < 2; i++)
		{
			if (add_decoration)
			{
				AddFloorDecoration(new Rectangle(16 * decoration_random.Next(0, 3), 272, 16, 16));
			}
			else
			{
				_AdvanceDecorationIndex();
			}
		}
		if (GetItemCount("117") > 0)
		{
			AddFloorDecoration(new Rectangle(48, 288, 16, 16));
		}
		else
		{
			_AdvanceDecorationIndex();
		}
		if (GetItemCount("166") > 0)
		{
			AddFloorDecoration(new Rectangle(64, 288, 16, 16));
		}
		else
		{
			_AdvanceDecorationIndex();
		}
		if (GetItemCount("797") > 0)
		{
			AddFloorDecoration(new Rectangle(80, 288, 16, 16));
		}
		else
		{
			_AdvanceDecorationIndex();
		}
		if (GetItemCount("832") > 0)
		{
			AddFloorDecoration(new Rectangle(96, 288, 16, 16));
		}
		else
		{
			_AdvanceDecorationIndex();
		}
		if (GetItemCount("109") > 0)
		{
			AddFloorDecoration(new Rectangle(112, 288, 16, 16));
		}
		else
		{
			_AdvanceDecorationIndex();
		}
		if (GetItemCount("709") > 0)
		{
			AddFloorDecoration(new Rectangle(128, 288, 16, 16));
		}
		else
		{
			_AdvanceDecorationIndex();
		}
		if (GetItemCount("392") > 0)
		{
			AddFloorDecoration(new Rectangle(144, 288, 16, 16));
		}
		else
		{
			_AdvanceDecorationIndex();
		}
		if (GetItemCount("394") > 0)
		{
			AddFloorDecoration(new Rectangle(160, 288, 16, 16));
		}
		else
		{
			_AdvanceDecorationIndex();
		}
		if (GetItemCount("167") > 0)
		{
			AddFloorDecoration(new Rectangle(176, 288, 16, 16));
		}
		else
		{
			_AdvanceDecorationIndex();
		}
		if (GetItemCount("789") > 0)
		{
			AddFloorDecoration(new Rectangle(192, 288, 16, 16));
		}
		else
		{
			_AdvanceDecorationIndex();
		}
		if (GetItemCount("330") > 0)
		{
			AddFloorDecoration(new Rectangle(208, 288, 16, 16));
		}
		else
		{
			_AdvanceDecorationIndex();
		}
	}

	public virtual void AddFloorDecoration(Rectangle source_rect)
	{
		if (_currentDecorationIndex != -1)
		{
			int index = floorDecorationIndices[_currentDecorationIndex];
			_AdvanceDecorationIndex();
			int center_x = (int)decorationSlots[index].X;
			int center_y = (int)decorationSlots[index].Y;
			if (center_x < source_rect.Width / 2)
			{
				center_x = source_rect.Width / 2;
			}
			if (center_x > GetTankBounds().Width / 4 - source_rect.Width / 2)
			{
				center_x = GetTankBounds().Width / 4 - source_rect.Width / 2;
			}
			KeyValuePair<Rectangle, Vector2> decoration = new KeyValuePair<Rectangle, Vector2>(source_rect, new Vector2(center_x, center_y));
			floorDecorations[index] = decoration;
		}
	}

	protected virtual void _AdvanceDecorationIndex()
	{
		for (int i = 0; i < decorationSlots.Count; i++)
		{
			_currentDecorationIndex++;
			if (_currentDecorationIndex >= decorationSlots.Count)
			{
				_currentDecorationIndex = 0;
			}
			if (!floorDecorations[floorDecorationIndices[_currentDecorationIndex]].HasValue)
			{
				return;
			}
		}
		_currentDecorationIndex = 1;
	}

	public override void OnMenuClose()
	{
		refreshFishEvent.Fire();
		base.OnMenuClose();
	}

	public Vector2 GetFishSortRegion()
	{
		return new Vector2(GetBaseDrawLayer() + 1E-06f, GetGlassDrawLayer() - 1E-06f);
	}

	public float GetGlassDrawLayer()
	{
		return GetBaseDrawLayer() + 0.0001f;
	}

	public float GetBaseDrawLayer()
	{
		if ((int)furniture_type != 12)
		{
			return (float)(boundingBox.Value.Bottom - (((int)furniture_type == 6 || (int)furniture_type == 13) ? 48 : 8)) / 10000f;
		}
		return 2E-09f;
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		Vector2 shake = Vector2.Zero;
		if (isTemporarilyInvisible)
		{
			return;
		}
		Vector2 draw_position = drawPosition.Value;
		if (!Furniture.isDrawingLocationFurniture)
		{
			draw_position = new Vector2(x, y) * 64f;
			draw_position.Y -= sourceRect.Height * 4 - boundingBox.Height;
		}
		if (shakeTimer > 0)
		{
			shake = new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
		}
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		Rectangle mainSourceRect = itemData.GetSourceRect();
		spriteBatch.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, draw_position + shake), new Rectangle(mainSourceRect.X + mainSourceRect.Width, mainSourceRect.Y, mainSourceRect.Width, mainSourceRect.Height), Color.White * alpha, 0f, Vector2.Zero, 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, GetGlassDrawLayer());
		if (Furniture.isDrawingLocationFurniture)
		{
			for (int i = 0; i < tankFish.Count; i++)
			{
				TankFish fish = tankFish[i];
				float fish_layer = Utility.Lerp(GetFishSortRegion().Y, GetFishSortRegion().X, fish.zPosition / 20f);
				fish_layer += 1E-07f * (float)i;
				fish.Draw(spriteBatch, alpha, fish_layer);
			}
			for (int i = 0; i < floorDecorations.Count; i++)
			{
				if (floorDecorations[i].HasValue)
				{
					KeyValuePair<Rectangle, Vector2> decoration = floorDecorations[i].Value;
					Vector2 decoration_position = decoration.Value;
					Rectangle decoration_source_rect = decoration.Key;
					float decoration_layer = Utility.Lerp(GetFishSortRegion().Y, GetFishSortRegion().X, decoration_position.Y / 20f) - 1E-06f;
					spriteBatch.Draw(GetAquariumTexture(), Game1.GlobalToLocal(new Vector2((float)GetTankBounds().Left + decoration_position.X * 4f, (float)(GetTankBounds().Bottom - 4) - decoration_position.Y * 4f)), decoration_source_rect, Color.White * alpha, 0f, new Vector2(decoration_source_rect.Width / 2, decoration_source_rect.Height - 4), 4f, SpriteEffects.None, decoration_layer);
				}
			}
			foreach (Vector4 bubble in bubbles)
			{
				float layer = Utility.Lerp(GetFishSortRegion().Y, GetFishSortRegion().X, bubble.Z / 20f) - 1E-06f;
				spriteBatch.Draw(GetAquariumTexture(), Game1.GlobalToLocal(new Vector2((float)GetTankBounds().Left + bubble.X, (float)(GetTankBounds().Bottom - 4) - bubble.Y - bubble.Z * 4f)), new Rectangle(0, 240, 16, 16), Color.White * alpha, 0f, new Vector2(8f, 8f), 4f * bubble.W, SpriteEffects.None, layer);
			}
		}
		base.draw(spriteBatch, x, y, alpha);
	}

	public int GetItemCount(string itemId)
	{
		int count = 0;
		foreach (Item item in heldItems)
		{
			if (Utility.IsNormalObjectAtParentSheetIndex(item, itemId))
			{
				count += item.Stack;
			}
		}
		return count;
	}

	public virtual Rectangle GetTankBounds()
	{
		Rectangle rectangle = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId).GetSourceRect();
		int height = rectangle.Height / 16;
		int width = rectangle.Width / 16;
		Rectangle tank_rect = new Rectangle((int)TileLocation.X * 64, (int)((TileLocation.Y - (float)getTilesHigh() - 1f) * 64f), width * 64, height * 64);
		tank_rect.X += 4;
		tank_rect.Width -= 8;
		if (base.QualifiedItemId == "(F)CCFishTank")
		{
			tank_rect.X += 24;
			tank_rect.Width -= 76;
		}
		tank_rect.Height -= 28;
		tank_rect.Y += 64;
		tank_rect.Height -= 64;
		return tank_rect;
	}
}
