using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.ItemTypeDefinitions;

/// <summary>The base parsed metadata for an item.</summary>
public class ParsedItemData : IHaveItemTypeId
{
	/// <summary>Whether the <see cref="F:StardewValley.ItemTypeDefinitions.ParsedItemData.Texture" /> has been loaded, regardless of whether the load was successful.</summary>
	private bool LoadedTexture;

	/// <summary>The texture containing the sprites to render for this item.</summary>
	private Texture2D Texture;

	/// <summary>The pixel area for the default sprite within the <see cref="F:StardewValley.ItemTypeDefinitions.ParsedItemData.Texture" />.</summary>
	private Rectangle DefaultSourceRect;

	/// <summary>The item type which defines this item.</summary>
	public readonly IItemDataDefinition ItemType;

	/// <summary>The item's unqualified ID within the <see cref="F:StardewValley.ItemTypeDefinitions.ParsedItemData.ItemType" />.</summary>
	public readonly string ItemId;

	/// <summary>The item's qualified ID.</summary>
	public readonly string QualifiedItemId;

	/// <summary>The item's index within the sprite sheet.</summary>
	public readonly int SpriteIndex;

	/// <summary>The asset name for the sprite sheet to use when drawing the item to the screen.</summary>
	public readonly string TextureName;

	/// <summary>The internal (non-localized) item name.</summary>
	public readonly string InternalName;

	/// <summary>The localized item name.</summary>
	public readonly string DisplayName;

	/// <summary>The localized item description.</summary>
	public readonly string Description;

	/// <summary>The object category ID.</summary>
	public readonly int Category;

	/// <summary>The object type.</summary>
	/// <remarks>This is the in-game type like <see cref="P:StardewValley.Object.Type" />, not the item type definition.</remarks>
	public readonly string ObjectType;

	/// <summary>The raw data fields from the underlying data asset if applicable, else <c>null</c>.</summary>
	public readonly object RawData;

	/// <summary>Whether this is a broken Error Item instance.</summary>
	public readonly bool IsErrorItem;

	/// <summary>Whether to exclude this item from shops when selecting random items to sell, including catalogues.</summary>
	public readonly bool ExcludeFromRandomSale;

	/// <summary>Construct an instance.</summary>
	/// <param name="itemType">The item type which defines this item.</param>
	/// <param name="itemId">The item's unqualified ID within the <paramref name="itemType" />.&gt;</param>
	/// <param name="spriteIndex">The item's index within the sprite sheet.</param>
	/// <param name="textureName">The asset name for the sprite sheet to use when drawing the item to the screen.</param>
	/// <param name="internalName">The internal (non-localized) item name.</param>
	/// <param name="displayName">The localized item name.</param>
	/// <param name="description">The localized item description.</param>
	/// <param name="category">The object category ID.</param>
	/// <param name="objectType">The object type.</param>
	/// <param name="rawData">The raw data fields from the underlying data asset if applicable, else <c>null</c>.</param>
	/// <param name="isErrorItem">Whether this is a broken Error Item instance.</param>
	/// <param name="excludeFromRandomSale">Whether to exclude this item from shops when selecting random items to sell, including catalogues.</param>
	public ParsedItemData(IItemDataDefinition itemType, string itemId, int spriteIndex, string textureName, string internalName, string displayName, string description, int category, string objectType, object rawData, bool isErrorItem = false, bool excludeFromRandomSale = false)
	{
		string qualifiedItemId = itemType.Identifier + itemId;
		if (string.IsNullOrWhiteSpace(internalName))
		{
			internalName = qualifiedItemId;
		}
		if (string.IsNullOrWhiteSpace(displayName))
		{
			displayName = ItemRegistry.GetUnnamedItemName(qualifiedItemId);
		}
		ItemType = itemType;
		ItemId = itemId;
		QualifiedItemId = qualifiedItemId;
		SpriteIndex = spriteIndex;
		TextureName = textureName;
		InternalName = internalName;
		DisplayName = displayName;
		Description = description;
		Category = category;
		ObjectType = objectType;
		RawData = rawData;
		IsErrorItem = isErrorItem;
		ExcludeFromRandomSale = excludeFromRandomSale;
		if (IsErrorItem)
		{
			LoadedTexture = true;
		}
	}

	/// <inheritdoc />
	public string GetItemTypeId()
	{
		return ItemType.Identifier;
	}

	/// <summary>Get the texture to render for this item.</summary>
	public virtual Texture2D GetTexture()
	{
		if (!IsErrorItem)
		{
			LoadTextureIfNeeded();
			Texture2D texture = Texture;
			if (texture != null)
			{
				return texture;
			}
		}
		return ItemType.GetErrorTexture();
	}

	/// <summary>Get the texture name to render for this item.</summary>
	public virtual string GetTextureName()
	{
		if (!IsErrorItem)
		{
			LoadTextureIfNeeded();
			string textureName = TextureName;
			if (Texture != null && textureName != null)
			{
				return textureName;
			}
		}
		return ItemType.GetErrorTextureName();
	}

	/// <summary>Get the pixel rectangle to render for the item's sprite within the texture returned by <see cref="M:StardewValley.ItemTypeDefinitions.ParsedItemData.GetTexture" /> or <see cref="M:StardewValley.ItemTypeDefinitions.ParsedItemData.GetTextureName" />.</summary>
	/// <param name="offset">An index offset to apply to the sprite index.</param>
	/// <param name="spriteIndex">The sprite index to render, or <c>null</c> to use the parsed <see cref="F:StardewValley.ItemTypeDefinitions.ParsedItemData.SpriteIndex" />.</param>
	public virtual Rectangle GetSourceRect(int offset = 0, int? spriteIndex = null)
	{
		if (!IsErrorItem)
		{
			LoadTextureIfNeeded();
			if (Texture != null)
			{
				if (offset != 0 || (spriteIndex.HasValue && spriteIndex != SpriteIndex))
				{
					return ItemType.GetSourceRect(this, Texture, (spriteIndex ?? SpriteIndex) + offset);
				}
				return DefaultSourceRect;
			}
		}
		return ItemType.GetErrorSourceRect();
	}

	/// <summary>Get whether the item specifies an object category.</summary>
	public virtual bool HasCategory()
	{
		return Category < -1;
	}

	/// <summary>Load the texture data if it's not already loaded.</summary>
	protected virtual void LoadTextureIfNeeded()
	{
		if (!LoadedTexture)
		{
			if (IsErrorItem)
			{
				Texture = null;
				DefaultSourceRect = Rectangle.Empty;
				LoadedTexture = true;
			}
			else
			{
				Texture = TryLoadTexture();
				DefaultSourceRect = ((Texture == null) ? Rectangle.Empty : ItemType.GetSourceRect(this, Texture, SpriteIndex));
				LoadedTexture = true;
			}
		}
	}

	/// <summary>Load the texture instance.</summary>
	protected virtual Texture2D TryLoadTexture()
	{
		string textureName = TextureName;
		try
		{
			if (!Game1.content.DoesAssetExist<Texture2D>(textureName))
			{
				Game1.log.Error($"Failed loading texture {textureName} for item {QualifiedItemId}: asset doesn't exist.");
				return null;
			}
			return Game1.content.Load<Texture2D>(textureName);
		}
		catch (Exception ex)
		{
			Game1.log.Error($"Failed loading texture {textureName} for item {QualifiedItemId}.", ex);
			return null;
		}
	}
}
