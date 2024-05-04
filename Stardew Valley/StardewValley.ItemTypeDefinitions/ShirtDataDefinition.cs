using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace StardewValley.ItemTypeDefinitions;

/// <summary>Manages the data for shirt clothing items.</summary>
/// <remarks>Shirt textures must be evenly split into two parts: the left half contains the clothing sprites, and the right half contains equivalent dye masks (if any). The texture can be any width as long as it's evenly split (e.g. three clothing sprites + three dye masks wide).</remarks>
public class ShirtDataDefinition : BaseItemDataDefinition
{
	/// <inheritdoc />
	public override string Identifier => "(S)";

	/// <inheritdoc />
	public override string StandardDescriptor => "C";

	/// <inheritdoc />
	public override IEnumerable<string> GetAllIds()
	{
		return Game1.shirtData.Keys;
	}

	/// <inheritdoc />
	public override bool Exists(string itemId)
	{
		if (itemId != null)
		{
			return Game1.shirtData.ContainsKey(itemId);
		}
		return false;
	}

	/// <inheritdoc />
	public override ParsedItemData GetData(string itemId)
	{
		if (itemId == null || !Game1.shirtData.TryGetValue(itemId, out var data))
		{
			return null;
		}
		return new ParsedItemData(this, itemId, data.SpriteIndex, data.Texture ?? "Characters\\Farmer\\shirts", data.Name, TokenParser.ParseText(data.DisplayName), TokenParser.ParseText(data.Description), -100, null, data);
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
		int spriteAreaWidth = texture.Width / 2;
		return new Rectangle(spriteIndex * 8 % spriteAreaWidth, spriteIndex * 8 / spriteAreaWidth * 32, 8, 8);
	}

	/// <inheritdoc />
	public override Item CreateItem(ParsedItemData data)
	{
		if (data == null)
		{
			throw new ArgumentNullException("data");
		}
		return new Clothing(data.ItemId);
	}
}
