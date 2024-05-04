using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.BigCraftables;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace StardewValley.ItemTypeDefinitions;

/// <summary>Manages the data for big craftable items.</summary>
public class BigCraftableDataDefinition : BaseItemDataDefinition
{
	/// <inheritdoc />
	public override string Identifier => "(BC)";

	/// <inheritdoc />
	public override string StandardDescriptor => "BO";

	/// <inheritdoc />
	public override IEnumerable<string> GetAllIds()
	{
		return Game1.bigCraftableData.Keys;
	}

	/// <inheritdoc />
	public override bool Exists(string itemId)
	{
		if (itemId != null)
		{
			return Game1.bigCraftableData.ContainsKey(itemId);
		}
		return false;
	}

	/// <inheritdoc />
	public override ParsedItemData GetData(string itemId)
	{
		BigCraftableData data = GetRawData(itemId);
		if (data == null)
		{
			return null;
		}
		return new ParsedItemData(this, itemId, data.SpriteIndex, data.Texture ?? "TileSheets\\Craftables", data.Name, TokenParser.ParseText(data.DisplayName), TokenParser.ParseText(data.Description), -9, "Crafting", data);
	}

	/// <inheritdoc />
	public override Item CreateItem(ParsedItemData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		if (data.QualifiedItemId == "(BC)221")
		{
			return new ItemPedestal(Vector2.Zero, null, lock_on_success: false, Color.White);
		}
		return new Object(Vector2.Zero, data.ItemId);
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
		return Object.getSourceRectForBigCraftable(texture, spriteIndex);
	}

	/// <summary>Get the raw data fields for an item.</summary>
	/// <param name="itemId">The unqualified item ID.</param>
	protected BigCraftableData GetRawData(string itemId)
	{
		if (itemId == null || !Game1.bigCraftableData.TryGetValue(itemId, out var data))
		{
			return null;
		}
		return data;
	}
}
