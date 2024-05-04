using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.GameData;
using StardewValley.Locations;

namespace StardewValley.Objects;

public class Wallpaper : Object
{
	[XmlElement("sourceRect")]
	public readonly NetRectangle sourceRect = new NetRectangle();

	/// <summary>Whether this is a flooring item; else it's a wallpaper item.</summary>
	[XmlElement("isFloor")]
	public readonly NetBool isFloor = new NetBool(value: false);

	/// <summary>The <c>Data/AdditionalWallpaperFlooring</c> set which contains this flooring or wallpaper, or <c>null</c> for a pre-1.6 vanilla wallpaper.</summary>
	[XmlElement("sourceTexture")]
	public readonly NetString setId = new NetString(null);

	/// <summary>The cached data for the flooring or wallpaper set.</summary>
	protected ModWallpaperOrFlooring setData;

	private static readonly Rectangle wallpaperContainerRect = new Rectangle(39, 31, 16, 16);

	private static readonly Rectangle floorContainerRect = new Rectangle(55, 31, 16, 16);

	/// <inheritdoc />
	public override string TypeDefinitionId
	{
		get
		{
			if (!isFloor.Value)
			{
				return "(WP)";
			}
			return "(FL)";
		}
	}

	/// <inheritdoc />
	[XmlIgnore]
	public override string Name => base.name;

	public Wallpaper()
	{
	}

	public Wallpaper(int which, bool isFloor = false)
		: this()
	{
		base.ItemId = which.ToString();
		this.isFloor.Value = isFloor;
		base.ParentSheetIndex = which;
		base.name = (isFloor ? "Flooring" : "Wallpaper");
		sourceRect.Value = (isFloor ? new Rectangle(which % 8 * 32, 336 + which / 8 * 32, 28, 26) : new Rectangle(which % 16 * 16, which / 16 * 48 + 8, 16, 28));
		price.Value = 100;
	}

	public Wallpaper(string setId, int which)
		: this()
	{
		base.ItemId = $"{setId}:{which}";
		this.setId.Value = setId;
		base.ParentSheetIndex = which;
		ModWallpaperOrFlooring setData = GetSetData();
		if (setData == null)
		{
			this.setId.Value = null;
		}
		isFloor.Value = setData?.IsFlooring ?? false;
		sourceRect.Value = (isFloor ? new Rectangle(which % 8 * 32, 336 + which / 8 * 32, 28, 26) : new Rectangle(which % 16 * 16, which / 16 * 48 + 8, 16, 28));
		if (setData != null && isFloor.Value)
		{
			sourceRect.Y = which / 8 * 32;
		}
		base.name = (isFloor ? "Flooring" : "Wallpaper");
		price.Value = 100;
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(sourceRect, "sourceRect").AddField(isFloor, "isFloor").AddField(setId, "setId");
	}

	/// <summary>Get the data for the flooring or wallpaper set which contains this item, if any.</summary>
	public virtual ModWallpaperOrFlooring GetSetData()
	{
		if (setId.Value == null)
		{
			return null;
		}
		if (setData != null)
		{
			return setData;
		}
		foreach (ModWallpaperOrFlooring entry in DataLoader.AdditionalWallpaperFlooring(Game1.content))
		{
			if (entry.Id == setId.Value)
			{
				setData = entry;
				return entry;
			}
		}
		return null;
	}

	/// <inheritdoc />
	protected override string loadDisplayName()
	{
		if (!isFloor)
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13204");
		}
		return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13203");
	}

	public override string getDescription()
	{
		if (!isFloor)
		{
			return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13206");
		}
		return Game1.content.LoadString("Strings\\StringsFromCSFiles:Wallpaper.cs.13205");
	}

	/// <inheritdoc />
	public override bool performDropDownAction(Farmer who)
	{
		return true;
	}

	/// <inheritdoc />
	public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who, bool returnFalseIfItemConsumed = false)
	{
		return false;
	}

	public override bool canBePlacedHere(GameLocation l, Vector2 tile, CollisionMask collisionMask = CollisionMask.All, bool showError = false)
	{
		Vector2 nonTile = tile * 64f;
		nonTile.X += 32f;
		nonTile.Y += 32f;
		foreach (Furniture f in l.furniture)
		{
			if ((int)f.furniture_type != 12 && f.GetBoundingBox().Contains((int)nonTile.X, (int)nonTile.Y))
			{
				return false;
			}
		}
		return true;
	}

	public override bool placementAction(GameLocation location, int x, int y, Farmer who = null)
	{
		if (who == null)
		{
			who = Game1.player;
		}
		if (location is DecoratableLocation decoratableLocation)
		{
			Point tile = new Point(x / 64, y / 64);
			if ((bool)isFloor)
			{
				string floor_id = decoratableLocation.GetFloorID(tile.X, tile.Y);
				if (floor_id != null)
				{
					if (GetSetData() != null)
					{
						decoratableLocation.SetFloor(GetSetData().Id + ":" + parentSheetIndex, floor_id);
					}
					else
					{
						decoratableLocation.SetFloor(parentSheetIndex.ToString(), floor_id);
					}
					location.playSound("coin");
					return true;
				}
			}
			else
			{
				string wall_id = decoratableLocation.GetWallpaperID(tile.X, tile.Y);
				if (wall_id != null)
				{
					if (GetSetData() != null)
					{
						decoratableLocation.SetWallpaper(GetSetData().Id + ":" + parentSheetIndex, wall_id);
					}
					else
					{
						decoratableLocation.SetWallpaper(parentSheetIndex.ToString(), wall_id);
					}
					location.playSound("coin");
					return true;
				}
			}
		}
		return false;
	}

	public override bool isPlaceable()
	{
		return true;
	}

	/// <inheritdoc />
	public override int salePrice(bool ignoreProfitMargins = false)
	{
		return price;
	}

	public override int maximumStackSize()
	{
		return 1;
	}

	public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
	{
		drawInMenu(spriteBatch, objectPosition, 1f);
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color color, bool drawShadow)
	{
		AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
		Texture2D wallpaperTexture;
		if (GetSetData() != null)
		{
			try
			{
				wallpaperTexture = Game1.content.Load<Texture2D>(GetSetData().Texture);
			}
			catch (Exception)
			{
				wallpaperTexture = Game1.content.Load<Texture2D>("Maps\\walls_and_floors");
			}
		}
		else
		{
			wallpaperTexture = Game1.content.Load<Texture2D>("Maps\\walls_and_floors");
		}
		if ((bool)isFloor)
		{
			spriteBatch.Draw(Game1.mouseCursors2, location + new Vector2(32f, 32f), floorContainerRect, color * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
			spriteBatch.Draw(wallpaperTexture, location + new Vector2(32f, 30f), sourceRect.Value, color * transparency, 0f, new Vector2(14f, 13f), 2f * scaleSize, SpriteEffects.None, layerDepth + 0.001f);
		}
		else
		{
			spriteBatch.Draw(Game1.mouseCursors2, location + new Vector2(32f, 32f), wallpaperContainerRect, color * transparency, 0f, new Vector2(8f, 8f), 4f * scaleSize, SpriteEffects.None, layerDepth);
			spriteBatch.Draw(wallpaperTexture, location + new Vector2(32f, 32f), sourceRect.Value, color * transparency, 0f, new Vector2(8f, 14f), 2f * scaleSize, SpriteEffects.None, layerDepth + 0.001f);
		}
		DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color);
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		ModWallpaperOrFlooring data = GetSetData();
		if (data == null)
		{
			return new Wallpaper(parentSheetIndex.Value, isFloor);
		}
		return new Wallpaper(data.Id, parentSheetIndex.Value);
	}
}
