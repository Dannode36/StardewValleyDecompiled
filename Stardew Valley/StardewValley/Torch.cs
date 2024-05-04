using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;

namespace StardewValley;

public class Torch : Object
{
	public const float yVelocity = 1f;

	public const float yDissapearLevel = -100f;

	public const double ashChance = 0.015;

	private float color;

	private Vector2[] ashes = new Vector2[3];

	private float smokePuffTimer;

	public Torch()
		: this(1)
	{
	}

	public Torch(int initialStack)
		: base("93", initialStack)
	{
	}

	public Torch(int initialStack, string itemId)
		: base(itemId, initialStack)
	{
	}

	public Torch(string index, bool bigCraftable)
		: base(Vector2.Zero, index)
	{
	}

	/// <inheritdoc />
	public override void RecalculateBoundingBox()
	{
		boundingBox.Value = new Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
	}

	/// <inheritdoc />
	protected override void MigrateLegacyItemId()
	{
		base.ItemId = parentSheetIndex.Value.ToString();
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		if (!bigCraftable.Value)
		{
			return new Torch();
		}
		return new Torch(base.ItemId, bigCraftable: true);
	}

	/// <inheritdoc />
	public override void actionOnPlayerEntry()
	{
		base.actionOnPlayerEntry();
		if ((bool)bigCraftable && (bool)isOn)
		{
			AmbientLocationSounds.addSound(tileLocation.Value, 1);
		}
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		if ((bool)bigCraftable)
		{
			if (justCheckingForActivity)
			{
				return true;
			}
			if (base.QualifiedItemId == "(BC)278")
			{
				Vector2 center = Utility.getTopLeftPositionForCenteringOnScreen(800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2);
				Game1.activeClickableMenu = new CraftingPage((int)center.X, (int)center.Y, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, cooking: true, standaloneMenu: true);
				return true;
			}
			isOn.Value = !isOn;
			if ((bool)isOn)
			{
				if ((bool)bigCraftable)
				{
					if (who != null)
					{
						Game1.playSound("fireball");
					}
					initializeLightSource(tileLocation.Value);
					AmbientLocationSounds.addSound(tileLocation.Value, 1);
				}
			}
			else if ((bool)bigCraftable)
			{
				performRemoveAction();
				if (who != null)
				{
					Game1.playSound("woodyHit");
				}
			}
			return true;
		}
		return base.checkForAction(who, justCheckingForActivity);
	}

	/// <inheritdoc />
	public override bool placementAction(GameLocation location, int x, int y, Farmer who)
	{
		Vector2 placementTile = new Vector2(x / 64, y / 64);
		Torch toPlace = (bigCraftable ? new Torch(base.ItemId, bigCraftable: true) : new Torch(1, base.ItemId));
		if ((bool)bigCraftable)
		{
			toPlace.isOn.Value = false;
		}
		location.objects.Add(placementTile, toPlace);
		toPlace.initializeLightSource(placementTile);
		if (who != null)
		{
			Game1.playSound("woodyStep");
		}
		return true;
	}

	public override bool isPassable()
	{
		return !bigCraftable;
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		base.updateWhenCurrentLocation(time);
		GameLocation environment = Location;
		if (environment == null)
		{
			return;
		}
		updateAshes((int)(tileLocation.X * 2000f + tileLocation.Y));
		smokePuffTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
		if (smokePuffTimer <= 0f)
		{
			smokePuffTimer = 1000f;
			if (base.QualifiedItemId == "(BC)278")
			{
				Utility.addSmokePuff(environment, tileLocation.Value * 64f + new Vector2(32f, -32f));
			}
		}
	}

	private void updateAshes(int identifier)
	{
		if (!Utility.isOnScreen(tileLocation.Value * 64f, 256))
		{
			return;
		}
		for (int i = ashes.Length - 1; i >= 0; i--)
		{
			Vector2 temp = ashes[i];
			temp.Y -= 1f * ((float)(i + 1) * 0.25f);
			if (i % 2 != 0)
			{
				temp.X += (float)Math.Sin((double)ashes[i].Y / (Math.PI * 2.0)) / 2f;
			}
			ashes[i] = temp;
			if (Game1.random.NextDouble() < 0.0075 && ashes[i].Y < -100f)
			{
				ashes[i] = new Vector2((float)(Game1.random.Next(-1, 3) * 4) * 0.75f, 0f);
			}
		}
		color = Math.Max(-0.8f, Math.Min(0.7f, color + ashes[0].Y / 1200f));
	}

	public override void performRemoveAction()
	{
		AmbientLocationSounds.removeSound(TileLocation);
		if ((bool)bigCraftable)
		{
			isOn.Value = false;
		}
		base.performRemoveAction();
	}

	public override void draw(SpriteBatch spriteBatch, int xNonTile, int yNonTile, float layerDepth, float alpha = 1f)
	{
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		Rectangle sourceRect = itemData.GetSourceRect(0, base.ParentSheetIndex).Clone();
		sourceRect.Y += 8;
		sourceRect.Height /= 2;
		spriteBatch.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile, yNonTile + 32)), sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
		sourceRect.X = 276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(xNonTile * 320) + (double)(yNonTile * 49)) % 700.0 / 100.0) * 8;
		sourceRect.Y = 1965;
		sourceRect.Width = 8;
		sourceRect.Height = 8;
		spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile + 32 + 4, yNonTile + 16 + 4)), sourceRect, Color.White * 0.75f, 0f, new Vector2(4f, 4f), 3f, SpriteEffects.None, layerDepth + 1E-05f);
		spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(xNonTile + 32 + 4, yNonTile + 16 + 4)), new Rectangle(88, 1779, 30, 30), Color.PaleGoldenrod * (Game1.currentLocation.IsOutdoors ? 0.35f : 0.43f), 0f, new Vector2(15f, 15f), 8f + (float)(32.0 * Math.Sin((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(xNonTile * 777) + (double)(yNonTile * 9746)) % 3140.0 / 1000.0) / 50.0), SpriteEffects.None, 1f);
	}

	public static void drawBasicTorch(SpriteBatch spriteBatch, float x, float y, float layerDepth, float alpha = 1f)
	{
		Rectangle sourceRect = new Rectangle(336, 48, 16, 16);
		sourceRect.Y += 8;
		sourceRect.Height /= 2;
		spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y + 32f)), sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, layerDepth);
		spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x + 32f + 2f, y + 16f)), new Rectangle(88, 1779, 30, 30), Color.PaleGoldenrod * (Game1.currentLocation.IsOutdoors ? 0.35f : 0.43f), 0f, new Vector2(15f, 15f), 4f + (float)(64.0 * Math.Sin((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 777f) + (double)(y * 9746f)) % 3140.0 / 1000.0) / 50.0), SpriteEffects.None, 1f);
		sourceRect.X = 276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3204f) + (double)(y * 49f)) % 700.0 / 100.0) * 8;
		sourceRect.Y = 1965;
		sourceRect.Width = 8;
		sourceRect.Height = 8;
		spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x + 32f + 4f, y + 16f + 4f)), sourceRect, Color.White * 0.75f, 0f, new Vector2(4f, 4f), 3f, SpriteEffects.None, layerDepth + 0.0001f);
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		if (Game1.eventUp)
		{
			GameLocation currentLocation = Game1.currentLocation;
			if ((currentLocation == null || currentLocation.currentEvent?.showGroundObjects != true) && !Game1.currentLocation.IsFarm)
			{
				return;
			}
		}
		if (!bigCraftable)
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			Rectangle sourceRect = itemData.GetSourceRect(0, base.ParentSheetIndex).Clone();
			Rectangle bounds = GetBoundingBoxAt(x, y);
			sourceRect.Y += 8;
			sourceRect.Height /= 2;
			spriteBatch.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 + 32)), sourceRect, Color.White, 0f, Vector2.Zero, (scale.Y > 1f) ? getScale().Y : 4f, SpriteEffects.None, (float)(bounds.Center.Y - 16) / 10000f);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + 2, y * 64 + 16)), new Rectangle(88, 1779, 30, 30), Color.PaleGoldenrod * (Game1.currentLocation.IsOutdoors ? 0.35f : 0.43f), 0f, new Vector2(15f, 15f), 4f + (float)(64.0 * Math.Sin((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 64 * 777) + (double)(y * 64 * 9746)) % 3140.0 / 1000.0) / 50.0), SpriteEffects.None, (float)(bounds.Center.Y - 15) / 10000f);
			sourceRect.X = 276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3204) + (double)(y * 49)) % 700.0 / 100.0) * 8;
			sourceRect.Y = 1965;
			sourceRect.Width = 8;
			sourceRect.Height = 8;
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + 4, y * 64 + 16 + 4)), sourceRect, Color.White * 0.75f, 0f, new Vector2(4f, 4f), 3f, SpriteEffects.None, (float)(bounds.Center.Y - 16) / 10000f);
			for (int i = 0; i < ashes.Length; i++)
			{
				spriteBatch.Draw(Game1.objectSpriteSheet, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + 32) + ashes[i].X, (float)(y * 64 + 32) + ashes[i].Y)), new Rectangle(344 + i % 3, 53, 1, 1), Color.White * 0.5f * ((-100f - ashes[i].Y / 2f) / -100f), 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)(bounds.Center.Y - 16) / 10000f);
			}
			return;
		}
		base.draw(spriteBatch, x, y, alpha);
		float draw_layer = Math.Max(0f, (float)((y + 1) * 64 - 24) / 10000f) + (float)x * 1E-05f;
		if (!isOn)
		{
			return;
		}
		if (ItemContextTagManager.HasBaseTag(base.QualifiedItemId, "campfire_item"))
		{
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 16 - 4, y * 64 - 8)), new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3047) + (double)(y * 88)) % 400.0 / 100.0) * 12, 1985, 12, 11), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, draw_layer + 0.0008f);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 - 12, y * 64)), new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 2047) + (double)(y * 98)) % 400.0 / 100.0) * 12, 1985, 12, 11), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, draw_layer + 0.0009f);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 - 20, y * 64 + 12)), new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 2077) + (double)(y * 98)) % 400.0 / 100.0) * 12, 1985, 12, 11), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, draw_layer + 0.001f);
			if (base.QualifiedItemId == "(BC)278")
			{
				ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
				Rectangle r = itemData.GetSourceRect(1, base.ParentSheetIndex).Clone();
				r.Height -= 16;
				Vector2 scaleFactor = getScale();
				scaleFactor *= 4f;
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64 + 12));
				Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(64f + scaleFactor.Y / 2f));
				spriteBatch.Draw(itemData.GetTexture(), destination, r, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, draw_layer + 0.0028f);
			}
		}
		else
		{
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 16 - 8, y * 64 - 64 + 8)), new Rectangle(276 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(x * 3047) + (double)(y * 88)) % 400.0 / 100.0) * 12, 1985, 12, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer + 0.0008f);
		}
	}
}
