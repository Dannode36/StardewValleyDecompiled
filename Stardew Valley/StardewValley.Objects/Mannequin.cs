using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Delegates;
using StardewValley.GameData;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;

namespace StardewValley.Objects;

public class Mannequin : Object
{
	protected string _description;

	protected MannequinData _data;

	public string displayNameOverride;

	public readonly NetMutex changeMutex = new NetMutex();

	public readonly NetRef<Hat> hat = new NetRef<Hat>();

	public readonly NetRef<Clothing> shirt = new NetRef<Clothing>();

	public readonly NetRef<Clothing> pants = new NetRef<Clothing>();

	public readonly NetRef<Boots> boots = new NetRef<Boots>();

	public readonly NetDirection facing = new NetDirection();

	public readonly NetBool swappedWithFarmerTonight = new NetBool();

	private Farmer renderCache;

	internal int eyeTimer;

	public override string TypeDefinitionId { get; } = "(M)";


	public Mannequin()
	{
	}

	public Mannequin(string itemId)
		: this()
	{
		base.ItemId = itemId;
		base.name = itemId;
		ParsedItemData data = ItemRegistry.GetDataOrErrorItem(itemId);
		base.ParentSheetIndex = data.SpriteIndex;
		bigCraftable.Value = true;
		canBeSetDown.Value = true;
		setIndoors.Value = true;
		setOutdoors.Value = true;
		base.Type = "interactive";
		facing.Value = 2;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(changeMutex.NetFields, "changeMutex.NetFields").AddField(hat, "hat").AddField(shirt, "shirt")
			.AddField(pants, "pants")
			.AddField(boots, "boots")
			.AddField(facing, "facing")
			.AddField(swappedWithFarmerTonight, "swappedWithFarmerTonight");
		hat.fieldChangeVisibleEvent += OnMannequinUpdated;
		shirt.fieldChangeVisibleEvent += OnMannequinUpdated;
		pants.fieldChangeVisibleEvent += OnMannequinUpdated;
		boots.fieldChangeVisibleEvent += OnMannequinUpdated;
	}

	private void OnMannequinUpdated<TNetField, TValue>(TNetField field, TValue oldValue, TValue newValue)
	{
		renderCache = null;
	}

	protected internal MannequinData GetMannequinData()
	{
		if (_data == null && !DataLoader.Mannequins(Game1.content).TryGetValue(base.ItemId, out _data))
		{
			_data = null;
		}
		return _data;
	}

	protected override string loadDisplayName()
	{
		ParsedItemData data = ItemRegistry.GetDataOrErrorItem(base.ItemId);
		if (displayNameOverride == null)
		{
			return data.DisplayName;
		}
		return displayNameOverride;
	}

	public override string getDescription()
	{
		if (_description == null)
		{
			ParsedItemData data = ItemRegistry.GetDataOrErrorItem(base.ItemId);
			_description = Game1.parseText(TokenParser.ParseText(data.Description), Game1.smallFont, getDescriptionWidth());
		}
		return _description;
	}

	public override bool isPlaceable()
	{
		return true;
	}

	/// <inheritdoc />
	public override bool ForEachItem(ForEachItemDelegate handler)
	{
		if (base.ForEachItem(handler) && ForEachItemHelper.ApplyToField(hat, handler) && ForEachItemHelper.ApplyToField(shirt, handler) && ForEachItemHelper.ApplyToField(pants, handler))
		{
			return ForEachItemHelper.ApplyToField(boots, handler);
		}
		return false;
	}

	public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
	{
		Vector2 placementTile = new Vector2(x / 64, y / 64);
		Mannequin toPlace = getOne() as Mannequin;
		location.Objects.Add(placementTile, toPlace);
		location.playSound("woodyStep");
		return true;
	}

	private void emitGhost()
	{
		Location.temporarySprites.Add(new TemporaryAnimatedSprite(GetMannequinData().Texture, new Rectangle((!(Game1.random.NextDouble() < 0.5)) ? 64 : 0, 64, 16, 32), TileLocation * 64f + new Vector2(0f, -1f) * 64f, flipped: false, 0.004f, Color.White)
		{
			scale = 4f,
			layerDepth = 1f,
			motion = new Vector2(7 + Game1.random.Next(-1, 6), -8 + Game1.random.Next(-1, 5)),
			acceleration = new Vector2(-0.4f + (float)Game1.random.Next(10) / 100f, 0f),
			animationLength = 4,
			totalNumberOfLoops = 99,
			interval = 80f,
			scaleChangeChange = 0.01f
		});
		Location.playSound("cursed_mannequin");
	}

	public override bool minutesElapsed(int minutes)
	{
		if (Game1.random.NextDouble() < 0.001 && GetMannequinData().Cursed)
		{
			if (Game1.timeOfDay > Game1.getTrulyDarkTime(Location) && Game1.random.NextDouble() < 0.1)
			{
				emitGhost();
			}
			else if (Game1.random.NextDouble() < 0.66)
			{
				if (Game1.random.NextDouble() < 0.5)
				{
					foreach (Farmer f in Location.farmers)
					{
						facing.Value = Utility.GetOppositeFacingDirection(Utility.getDirectionFromChange(TileLocation, f.Tile));
						renderCache = null;
					}
				}
				else
				{
					eyeTimer = 2500;
				}
			}
			else
			{
				Location.playSound("cursed_mannequin");
				shakeTimer = Game1.random.Next(500, 4000);
			}
		}
		return base.minutesElapsed(minutes);
	}

	public override void actionOnPlayerEntry()
	{
		if (Game1.random.NextDouble() < 0.001 && GetMannequinData().Cursed)
		{
			shakeTimer = Game1.random.Next(500, 1000);
		}
		base.actionOnPlayerEntry();
	}

	public override void DayUpdate()
	{
		base.DayUpdate();
		if (Game1.IsMasterGame && GetMannequinData().Cursed && Location != null && (Location is FarmHouse || Location is IslandFarmHouse || Location is Shed))
		{
			if (Game1.random.NextDouble() < 0.05)
			{
				Vector2 oldTile = TileLocation;
				Utility.spawnObjectAround(TileLocation, this, Location, playSound: false, delegate
				{
					if (!TileLocation.Equals(oldTile))
					{
						Location.objects.Remove(oldTile);
					}
				});
			}
			else if (swappedWithFarmerTonight.Value)
			{
				swappedWithFarmerTonight.Value = false;
			}
			else
			{
				if (Game1.random.NextDouble() < 0.005)
				{
					if (Location.farmers.Count <= 0)
					{
						return;
					}
					using FarmerCollection.Enumerator enumerator = Location.farmers.GetEnumerator();
					if (enumerator.MoveNext())
					{
						Farmer who = enumerator.Current;
						Vector2 oldTile = TileLocation;
						Vector2 bedTile = who.mostRecentBed / 64f;
						bedTile.X = (int)bedTile.X;
						bedTile.Y = (int)bedTile.Y;
						if (Utility.spawnObjectAround(bedTile, this, Location, playSound: false, delegate
						{
							if (!TileLocation.Equals(oldTile))
							{
								Location.objects.Remove(oldTile);
							}
						}))
						{
							facing.Value = Utility.GetOppositeFacingDirection(Utility.getDirectionFromChange(TileLocation, who.Tile));
							renderCache = null;
							eyeTimer = 2000;
						}
					}
					return;
				}
				if (Game1.random.NextDouble() < 0.001)
				{
					DecoratableLocation dec_location = Location as DecoratableLocation;
					string floorID = dec_location.GetFloorID((int)TileLocation.X, (int)TileLocation.Y);
					string wallpaperID = null;
					for (int y = (int)TileLocation.Y; y > 0; y--)
					{
						wallpaperID = dec_location.GetWallpaperID((int)TileLocation.X, y);
						if (wallpaperID != null)
						{
							break;
						}
					}
					if (floorID != null)
					{
						dec_location.SetFloor("MoreFloors:6", floorID);
					}
					if (wallpaperID != null)
					{
						dec_location.SetWallpaper("MoreWalls:21", wallpaperID);
					}
					shakeTimer = 10000;
				}
				else
				{
					if (!(Game1.random.NextDouble() < 0.02))
					{
						return;
					}
					DecoratableLocation dec_location = Location as DecoratableLocation;
					if (Game1.random.NextDouble() < 0.33)
					{
						for (int i = 0; i < 30; i++)
						{
							int xPos = Game1.random.Next(2, Location.Map.Layers[0].LayerWidth - 2);
							for (int y = 1; y < Location.Map.Layers[0].LayerHeight; y++)
							{
								Vector2 spot = new Vector2(xPos, y);
								if (Location.isTileLocationOpen(spot) && Location.isTilePlaceable(spot) && !dec_location.isTileOnWall(xPos, y) && !Location.IsTileOccupiedBy(spot))
								{
									facing.Value = 2;
									renderCache = null;
									Location.objects.Remove(TileLocation);
									TileLocation = spot;
									Location.objects.Add(TileLocation, this);
									return;
								}
							}
						}
						return;
					}
					int xStartingPoint;
					int xEndingPoint;
					int xDirection;
					if (Game1.random.NextDouble() < 0.5)
					{
						xStartingPoint = 1;
						xEndingPoint = Location.Map.Layers[0].LayerWidth - 1;
						xDirection = 1;
					}
					else
					{
						xStartingPoint = Location.Map.Layers[0].LayerWidth - 1;
						xEndingPoint = 1;
						xDirection = -1;
					}
					for (int i = 0; i < 30; i++)
					{
						int yPos = Game1.random.Next(2, Location.Map.Layers[0].LayerHeight - 2);
						for (int x = xStartingPoint; x != xEndingPoint; x += xDirection)
						{
							Vector2 spot = new Vector2(x, yPos);
							if (Location.isTileLocationOpen(spot) && Location.isTilePlaceable(spot) && !dec_location.isTileOnWall(x, yPos) && !Location.IsTileOccupiedBy(spot))
							{
								facing.Value = ((xDirection == 1) ? 1 : 3);
								renderCache = null;
								Location.objects.Remove(TileLocation);
								TileLocation = spot;
								Location.objects.Add(TileLocation, this);
								return;
							}
						}
					}
				}
			}
		}
		else if (Game1.IsMasterGame && Location != null && Location is SeedShop && TileLocation.X > 33f && TileLocation.Y > 14f)
		{
			if (base.ItemId.Equals("CursedMannequinMale"))
			{
				base.ItemId = "MannequinMale";
			}
			else if (base.ItemId.Equals("CursedMannequinFemale"))
			{
				base.ItemId = "MannequinFemale";
			}
			ResetParentSheetIndex();
			renderCache = null;
			_data = null;
		}
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		base.updateWhenCurrentLocation(time);
		changeMutex.Update(Location);
		if (eyeTimer > 0)
		{
			eyeTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		}
	}

	public override bool performToolAction(Tool t)
	{
		if (t == null)
		{
			return false;
		}
		if (!(t is MeleeWeapon) && t.isHeavyHitter())
		{
			if (hat.Value != null || shirt.Value != null || pants.Value != null || boots.Value != null)
			{
				if (hat.Value != null)
				{
					DropItem(Utility.PerformSpecialItemGrabReplacement(hat.Value));
					hat.Value = null;
				}
				else if (shirt.Value != null)
				{
					DropItem(Utility.PerformSpecialItemGrabReplacement(shirt.Value));
					shirt.Value = null;
				}
				else if (pants.Value != null)
				{
					DropItem(Utility.PerformSpecialItemGrabReplacement(pants.Value));
					pants.Value = null;
				}
				else if (boots.Value != null)
				{
					DropItem(Utility.PerformSpecialItemGrabReplacement(boots.Value));
					boots.Value = null;
				}
				Location.playSound("hammer");
				shakeTimer = 100;
				return false;
			}
			Location.objects.Remove(TileLocation);
			Location.playSound("hammer");
			DropItem(new Mannequin(base.ItemId));
			return false;
		}
		return false;
	}

	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		if (who.CurrentItem is Hat || who.CurrentItem is Clothing || who.CurrentItem is Boots)
		{
			return false;
		}
		if (justCheckingForActivity)
		{
			return true;
		}
		if (hat.Value == null && shirt.Value == null && pants.Value == null && boots.Value == null)
		{
			facing.Value = (facing.Value + 1) % 4;
			renderCache = null;
			Game1.playSound("shwip");
		}
		else
		{
			changeMutex.RequestLock(delegate
			{
				hat.Value = who.Equip(hat.Value, who.hat);
				shirt.Value = who.Equip(shirt.Value, who.shirtItem);
				pants.Value = who.Equip(pants.Value, who.pantsItem);
				boots.Value = who.Equip(boots.Value, who.boots);
				changeMutex.ReleaseLock();
			});
			Game1.playSound("coin");
		}
		if (GetMannequinData().Cursed && Game1.random.NextDouble() < 0.001)
		{
			emitGhost();
		}
		return true;
	}

	/// <inheritdoc />
	public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
	{
		if (!(dropInItem is Hat newHat))
		{
			if (!(dropInItem is Clothing newClothing))
			{
				if (!(dropInItem is Boots newBoots))
				{
					return false;
				}
				if (!probe)
				{
					DropItem(boots.Value);
					boots.Value = (Boots)newBoots.getOne();
				}
			}
			else if (!probe)
			{
				if (newClothing.clothesType.Value == Clothing.ClothesType.SHIRT)
				{
					DropItem(shirt.Value);
					shirt.Value = (Clothing)newClothing.getOne();
				}
				else
				{
					DropItem(pants.Value);
					pants.Value = (Clothing)newClothing.getOne();
				}
			}
		}
		else if (!probe)
		{
			DropItem(hat.Value);
			hat.Value = (Hat)newHat.getOne();
		}
		if (!probe)
		{
			Game1.playSound("dirtyHit");
		}
		return true;
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		base.draw(spriteBatch, x, y, alpha);
		if (eyeTimer > 0 && facing.Value != 0)
		{
			float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1.1E-05f;
			Vector2 pos = Game1.GlobalToLocal(new Vector2(x, y) * 64f + new Vector2(20f, -40f));
			if (facing.Value == 1)
			{
				pos.X += 12f;
			}
			else if (facing.Value == 3)
			{
				pos.X += 4f;
			}
			if (facing.Value != 2)
			{
				pos.Y -= 4f;
			}
			spriteBatch.Draw(Game1.mouseCursors_1_6, pos, new Rectangle(377 + 5 * (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1620.0 / 60.0), 330, 5 + ((facing.Value != 2) ? (-3) : 0), 3), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer);
		}
		float drawLayer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
		Farmer fakeFarmer = GetFarmerForRendering();
		fakeFarmer.position.Value = new Vector2(x * 64, y * 64 - 4 + (GetMannequinData().DisplaysClothingAsMale ? 20 : 16));
		if (shakeTimer > 0)
		{
			fakeFarmer.position.Value += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
		}
		fakeFarmer.FarmerRenderer.draw(spriteBatch, fakeFarmer.FarmerSprite, fakeFarmer.FarmerSprite.SourceRect, fakeFarmer.getLocalPosition(Game1.viewport), new Vector2(0f, fakeFarmer.GetBoundingBox().Height), drawLayer + 0.0001f, Color.White, 0f, fakeFarmer);
		FarmerRenderer.FarmerSpriteLayers armLayer = FarmerRenderer.FarmerSpriteLayers.Arms;
		if (fakeFarmer.facingDirection.Value == 0)
		{
			armLayer = FarmerRenderer.FarmerSpriteLayers.ArmsUp;
		}
		if (fakeFarmer.FarmerSprite.CurrentAnimationFrame.armOffset > 0)
		{
			Rectangle sourceRect = fakeFarmer.FarmerSprite.SourceRect;
			sourceRect.Offset(-288 + fakeFarmer.FarmerSprite.CurrentAnimationFrame.armOffset * 16, 0);
			spriteBatch.Draw(fakeFarmer.FarmerRenderer.baseTexture, fakeFarmer.getLocalPosition(Game1.viewport) + new Vector2(0f, fakeFarmer.GetBoundingBox().Height) + fakeFarmer.FarmerRenderer.positionOffset + fakeFarmer.armOffset, sourceRect, Color.White, 0f, new Vector2(0f, fakeFarmer.GetBoundingBox().Height), 4f * scale, fakeFarmer.FarmerSprite.CurrentAnimationFrame.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, FarmerRenderer.GetLayerDepth(drawLayer + 0.0001f, armLayer));
		}
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new Mannequin(base.ItemId);
	}

	private void DropItem(Item item)
	{
		if (item != null)
		{
			Vector2 position = new Vector2((TileLocation.X + 0.5f) * 64f, (TileLocation.Y + 0.5f) * 64f);
			Location.debris.Add(new Debris(item, position));
		}
	}

	private Farmer GetFarmerForRendering()
	{
		renderCache = renderCache ?? CreateInstance();
		return renderCache;
		Farmer CreateInstance()
		{
			MannequinData data = GetMannequinData();
			Farmer farmer = new Farmer();
			farmer.changeGender(data.DisplaysClothingAsMale);
			farmer.faceDirection(facing.Value);
			farmer.changeHairColor(Color.Transparent);
			farmer.skin.Set(farmer.FarmerRenderer.recolorSkin(-12345));
			farmer.hat.Value = hat.Value;
			farmer.shirtItem.Value = shirt.Value;
			if (shirt.Value != null)
			{
				farmer.changeShirt("-1");
			}
			farmer.pantsItem.Value = pants.Value;
			if (pants.Value != null)
			{
				farmer.changePantStyle("-1");
			}
			farmer.boots.Value = boots.Value;
			if (boots.Value != null)
			{
				farmer.changeShoeColor(boots.Value.GetBootsColorString());
			}
			farmer.FarmerRenderer.textureName.Value = data.FarmerTexture;
			farmer.FarmerSprite.PauseForSingleAnimation = true;
			farmer.currentEyes = 0;
			return farmer;
		}
	}
}
