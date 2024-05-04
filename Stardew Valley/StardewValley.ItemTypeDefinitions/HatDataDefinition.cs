using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;

namespace StardewValley.ItemTypeDefinitions;

/// <summary>Manages the data for hat items.</summary>
public class HatDataDefinition : BaseItemDataDefinition
{
	/// <inheritdoc />
	public override string Identifier => "(H)";

	/// <inheritdoc />
	public override string StandardDescriptor => "H";

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
		return new ParsedItemData(this, itemId, GetSpriteIndex(itemId, fields), ArgUtility.Get(fields, 7) ?? "Characters\\Farmer\\hats", ArgUtility.Get(fields, 0), ArgUtility.Get(fields, 5), ArgUtility.Get(fields, 1), -95, null, fields);
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
		return new Rectangle(spriteIndex * 20 % texture.Width, spriteIndex * 20 / texture.Width * 20 * 4, 20, 20);
	}

	/// <inheritdoc />
	public override Item CreateItem(ParsedItemData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return new Hat(data.ItemId);
	}

	/// <summary>Get the item type's data asset.</summary>
	protected Dictionary<string, string> GetDataSheet()
	{
		return DataLoader.Hats(Game1.content);
	}

	/// <summary>Get the raw data fields for an item.</summary>
	/// <param name="itemId">The unqualified item ID.</param>
	protected string[] GetRawData(string itemId)
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
		int overrideIndex = ArgUtility.GetInt(fields, 6, -1);
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
