using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Weapons;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;

namespace StardewValley.ItemTypeDefinitions;

/// <summary>Manages the data for weapon items.</summary>
public class WeaponDataDefinition : BaseItemDataDefinition
{
	/// <inheritdoc />
	public override string Identifier => "(W)";

	/// <inheritdoc />
	public override string StandardDescriptor => "W";

	/// <inheritdoc />
	public override IEnumerable<string> GetAllIds()
	{
		return Game1.weaponData.Keys;
	}

	/// <inheritdoc />
	public override bool Exists(string itemId)
	{
		if (itemId != null)
		{
			return Game1.weaponData.ContainsKey(itemId);
		}
		return false;
	}

	/// <inheritdoc />
	public override ParsedItemData GetData(string itemId)
	{
		WeaponData data = GetRawData(itemId);
		if (data == null)
		{
			return null;
		}
		return new ParsedItemData(this, itemId, data.SpriteIndex, data.Texture, data.Name, TokenParser.ParseText(data.DisplayName), TokenParser.ParseText(data.Description), MeleeWeapon.IsScythe("(W)" + itemId) ? (-99) : (-98), null, data);
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
		return Game1.getSourceRectForStandardTileSheet(texture, spriteIndex, 16, 16);
	}

	/// <inheritdoc />
	public override Item CreateItem(ParsedItemData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		string itemId = data.ItemId;
		switch (itemId)
		{
		default:
			return new MeleeWeapon(itemId);
		case "32":
		case "33":
		case "34":
			return new Slingshot(itemId);
		}
	}

	/// <summary>Get the raw data fields for an item.</summary>
	/// <param name="itemId">The unqualified item ID.</param>
	protected WeaponData GetRawData(string itemId)
	{
		if (itemId == null || !Game1.weaponData.TryGetValue(itemId, out var raw))
		{
			return null;
		}
		return raw;
	}

	/// <summary>Get the sprite index for an item.</summary>
	/// <param name="itemId">The unqualified item ID.</param>
	/// <param name="fields">The raw data fields.</param>
	protected int GetSpriteIndex(string itemId, string[] fields)
	{
		int overrideIndex = ArgUtility.GetInt(fields, 15, -1);
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
