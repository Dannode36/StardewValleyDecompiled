using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace StardewValley.ItemTypeDefinitions;

/// <summary>Manages the data for furniture items.</summary>
public class FurnitureDataDefinition : BaseItemDataDefinition
{
	/// <inheritdoc />
	public override string Identifier => "(F)";

	/// <inheritdoc />
	public override string StandardDescriptor => "F";

	/// <inheritdoc />
	public override IEnumerable<string> GetAllIds()
	{
		return GetDataSheet().Keys;
	}

	/// <inheritdoc />
	public override bool Exists(string itemId)
	{
		if (itemId != null)
		{
			return GetDataSheet().ContainsKey(itemId);
		}
		return false;
	}

	/// <inheritdoc />
	public override ParsedItemData GetData(string itemId)
	{
		string[] fields = GetRawData(itemId);
		if (fields == null)
		{
			return null;
		}
		return new ParsedItemData(this, itemId, GetSpriteIndex(itemId, fields), ArgUtility.Get(fields, 9, "TileSheets\\furniture", allowBlank: false), ArgUtility.Get(fields, 0), TokenParser.ParseText(ArgUtility.Get(fields, 7)), null, -24, null, fields, isErrorItem: false, ArgUtility.GetBool(fields, 10));
	}

	/// <inheritdoc />
	public override Rectangle GetSourceRect(ParsedItemData data, Texture2D texture, int spriteIndex)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (texture == null)
		{
			throw new ArgumentNullException("texture");
		}
		return Furniture.GetDefaultSourceRect(data.ItemId, texture);
	}

	/// <inheritdoc />
	public override Item CreateItem(ParsedItemData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return Furniture.GetFurnitureInstance(data.ItemId, Vector2.Zero);
	}

	/// <summary>Get the item type's data asset.</summary>
	protected Dictionary<string, string> GetDataSheet()
	{
		return DataLoader.Furniture(Game1.content);
	}

	/// <summary>Get the raw data fields for an item.</summary>
	/// <param name="itemId">The unqualified item ID.</param>
	private string[] GetRawData(string itemId)
	{
		if (itemId == null || !GetDataSheet().TryGetValue(itemId, out var raw))
		{
			return null;
		}
		return raw.Split('/');
	}

	/// <summary>Get the sprite index for an item.</summary>
	/// <param name="itemId">The unqualified item ID.</param>
	/// <param name="fields">The raw data fields.</param>
	protected int GetSpriteIndex(string itemId, string[] fields)
	{
		int overrideIndex = ArgUtility.GetInt(fields, 8, -1);
		if (overrideIndex > -1)
		{
			return overrideIndex;
		}
		if (int.TryParse(itemId, out var value))
		{
			return value;
		}
		return -1;
	}
}
