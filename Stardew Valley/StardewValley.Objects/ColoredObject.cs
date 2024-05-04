using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Objects;

public class ColoredObject : Object
{
	[XmlElement("color")]
	public readonly NetColor color = new NetColor();

	[XmlElement("colorSameIndexAsParentSheetIndex")]
	public readonly NetBool colorSameIndexAsParentSheetIndex = new NetBool();

	public bool ColorSameIndexAsParentSheetIndex
	{
		get
		{
			return colorSameIndexAsParentSheetIndex.Value;
		}
		set
		{
			colorSameIndexAsParentSheetIndex.Value = value;
		}
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(color, "color").AddField(colorSameIndexAsParentSheetIndex, "colorSameIndexAsParentSheetIndex");
	}

	public ColoredObject()
	{
	}

	public ColoredObject(string itemId, int stack, Color color)
		: base(itemId, stack)
	{
		this.color.Value = color;
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, StackDrawType drawStackNumber, Color colorOverride, bool drawShadow)
	{
		AdjustMenuDrawForRecipes(ref transparency, ref scaleSize);
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.ItemId);
		Texture2D texture = itemData.GetTexture();
		Vector2 origin = (bigCraftable ? new Vector2(32f, 64f) : new Vector2(8f, 8f));
		float scale = ((!bigCraftable) ? (4f * scaleSize) : ((scaleSize < 0.2f) ? scaleSize : (scaleSize / 2f)));
		if (base.ItemId == "SmokedFish")
		{
			drawSmokedFish(spriteBatch, location, scaleSize, layerDepth, (transparency == 1f && colorOverride.A < byte.MaxValue) ? ((float)(int)colorOverride.A / 255f) : transparency);
		}
		else if (!ColorSameIndexAsParentSheetIndex)
		{
			Rectangle coloredSourceRect = itemData.GetSourceRect(1, base.ParentSheetIndex);
			transparency = ((transparency == 1f && colorOverride.A < byte.MaxValue) ? ((float)(int)colorOverride.A / 255f) : transparency);
			spriteBatch.Draw(texture, location + new Vector2(32f, 32f) * scaleSize, itemData.GetSourceRect(0, base.ParentSheetIndex), Color.White * transparency, 0f, origin * scaleSize, scale, SpriteEffects.None, layerDepth);
			spriteBatch.Draw(texture, location + new Vector2(32f, 32f) * scaleSize, coloredSourceRect, color.Value * transparency, 0f, origin * scaleSize, scale, SpriteEffects.None, Math.Min(1f, layerDepth + 2E-05f));
		}
		else
		{
			spriteBatch.Draw(texture, location + new Vector2(32f, 32f) * scaleSize, itemData.GetSourceRect(0, base.ParentSheetIndex), color.Value * transparency, 0f, origin * scaleSize, scale, SpriteEffects.None, Math.Min(1f, layerDepth + 2E-05f));
		}
		DrawMenuIcons(spriteBatch, location, scaleSize, transparency, layerDepth + 3E-05f, drawStackNumber, colorOverride);
	}

	private void drawSmokedFish(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float layerDepth, float transparency = 1f)
	{
		Vector2 origin = new Vector2(8f, 8f);
		float scale = 4f * scaleSize;
		string tex = ItemRegistry.GetData(preservedParentSheetIndex.Value).TextureName;
		int index = ItemRegistry.GetData(preservedParentSheetIndex.Value).SpriteIndex;
		Texture2D parent_tex = Game1.content.Load<Texture2D>(tex);
		spriteBatch.Draw(parent_tex, location + new Vector2(32f, 32f) * scaleSize, Game1.getSourceRectForStandardTileSheet(parent_tex, index, 16, 16), Color.White * transparency, 0f, origin * scaleSize, scale, SpriteEffects.None, Math.Min(1f, layerDepth + 1E-05f));
		spriteBatch.Draw(parent_tex, location + new Vector2(32f, 32f) * scaleSize, Game1.getSourceRectForStandardTileSheet(parent_tex, index, 16, 16), new Color(80, 30, 10) * 0.6f * transparency, 0f, origin * scaleSize, scale, SpriteEffects.None, Math.Min(1f, layerDepth + 1.5E-05f));
		int interval = 700 + ((int)price + 17) * 7777 % 200;
		spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(32f, 32f) * scaleSize + new Vector2(0f, (float)((0.0 - Game1.currentGameTime.TotalGameTime.TotalMilliseconds) % 2000.0) * 0.03f), new Rectangle(372, 1956, 10, 10), new Color(80, 80, 80) * transparency * 0.53f * (1f - (float)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2000.0) / 2000f), (float)((0.0 - Game1.currentGameTime.TotalGameTime.TotalMilliseconds) % 2000.0) * 0.001f, origin * scaleSize, scale / 2f, SpriteEffects.None, Math.Min(1f, layerDepth + 2E-05f));
		spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(24f, 40f) * scaleSize + new Vector2(0f, (float)((0.0 - (Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)interval)) % 2000.0) * 0.03f), new Rectangle(372, 1956, 10, 10), new Color(80, 80, 80) * transparency * 0.53f * (1f - (float)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)interval) % 2000.0) / 2000f), (float)((0.0 - (Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)interval)) % 2000.0) * 0.001f, origin * scaleSize, scale / 2f, SpriteEffects.None, Math.Min(1f, layerDepth + 2E-05f));
		spriteBatch.Draw(Game1.mouseCursors, location + new Vector2(48f, 21f) * scaleSize + new Vector2(0f, (float)((0.0 - (Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(interval * 2))) % 2000.0) * 0.03f), new Rectangle(372, 1956, 10, 10), new Color(80, 80, 80) * transparency * 0.53f * (1f - (float)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(interval * 2)) % 2000.0) / 2000f), (float)((0.0 - (Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(interval * 2))) % 2000.0) * 0.001f, origin * scaleSize, scale / 2f, SpriteEffects.None, Math.Min(1f, layerDepth + 2E-05f));
	}

	public override void drawWhenHeld(SpriteBatch spriteBatch, Vector2 objectPosition, Farmer f)
	{
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
		if (base.ItemId == "SmokedFish")
		{
			drawSmokedFish(spriteBatch, objectPosition, 1f, f.getDrawLayer() + 1E-05f);
		}
		else if (!ColorSameIndexAsParentSheetIndex)
		{
			base.drawWhenHeld(spriteBatch, objectPosition, f);
			spriteBatch.Draw(itemData.GetTexture(), objectPosition, itemData.GetSourceRect(1, base.ParentSheetIndex), color.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.StandingPixel.Y + 4) / 10000f));
		}
		else
		{
			spriteBatch.Draw(itemData.GetTexture(), objectPosition, itemData.GetSourceRect(0, base.ParentSheetIndex), color.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, Math.Max(0f, (float)(f.StandingPixel.Y + 4) / 10000f));
		}
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new ColoredObject(base.ItemId, 1, color.Value);
	}

	/// <inheritdoc />
	protected override void GetOneCopyFrom(Item source)
	{
		base.GetOneCopyFrom(source);
		if (source is ColoredObject fromObj)
		{
			preserve.Value = fromObj.preserve.Value;
			preservedParentSheetIndex.Value = fromObj.preservedParentSheetIndex.Value;
			Name = fromObj.Name;
			colorSameIndexAsParentSheetIndex.Value = fromObj.colorSameIndexAsParentSheetIndex.Value;
		}
	}

	public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
	{
		if ((bool)bigCraftable)
		{
			Vector2 scaleFactor = getScale();
			Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64));
			Rectangle destination = new Rectangle((int)(position.X - scaleFactor.X / 2f), (int)(position.Y - scaleFactor.Y / 2f), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));
			int indexOffset = 0;
			if ((bool)showNextIndex)
			{
				indexOffset = 1;
			}
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			Texture2D texture = itemData.GetTexture();
			if (!ColorSameIndexAsParentSheetIndex)
			{
				Rectangle coloredSourceRect = itemData.GetSourceRect(indexOffset + 1, base.ParentSheetIndex);
				spriteBatch.Draw(texture, destination, itemData.GetSourceRect(indexOffset, base.ParentSheetIndex), Color.White, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
				spriteBatch.Draw(texture, destination, coloredSourceRect, color.Value, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
			}
			else
			{
				spriteBatch.Draw(texture, destination, itemData.GetSourceRect(0, base.ParentSheetIndex), color.Value, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
			}
			if (base.QualifiedItemId == "(BC)17" && base.MinutesUntilReady > 0)
			{
				spriteBatch.Draw(Game1.objectSpriteSheet, getLocalPosition(Game1.viewport) + new Vector2(32f, 0f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 435, 16, 16), Color.White, scale.X, new Vector2(32f, 32f), 1f, SpriteEffects.None, Math.Max(0f, (float)((y + 1) * 64 - 1) / 10000f));
			}
		}
		else if (!Game1.eventUp || Location.IsFarm)
		{
			if (base.QualifiedItemId != "(O)590")
			{
				spriteBatch.Draw(Game1.shadowTexture, getLocalPosition(Game1.viewport) + new Vector2(32f, 53f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, 1E-07f);
			}
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(base.QualifiedItemId);
			Texture2D texture = itemData.GetTexture();
			Rectangle bounds = GetBoundingBoxAt(x, y);
			if (!ColorSameIndexAsParentSheetIndex)
			{
				Rectangle coloredSourceRect = itemData.GetSourceRect(1, base.ParentSheetIndex);
				spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32, y * 64 + 32)), itemData.GetSourceRect(0, base.ParentSheetIndex), Color.White, 0f, new Vector2(8f, 8f), (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)bounds.Bottom / 10000f);
				spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), coloredSourceRect, color.Value, 0f, new Vector2(8f, 8f), (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)bounds.Bottom / 10000f);
			}
			else
			{
				spriteBatch.Draw(texture, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), y * 64 + 32 + ((shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0))), itemData.GetSourceRect(0, base.ParentSheetIndex), color.Value, 0f, new Vector2(8f, 8f), (scale.Y > 1f) ? getScale().Y : 4f, flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (float)bounds.Bottom / 10000f);
			}
		}
	}
}
