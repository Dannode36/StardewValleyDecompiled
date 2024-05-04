using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace StardewValley.ItemTypeDefinitions;

/// <summary>Manages the data for mannequin items.</summary>
public class MannequinDataDefinition : BaseItemDataDefinition
{
	/// <inheritdoc />
	public override string Identifier => "(M)";

	/// <inheritdoc />
	public override string StandardDescriptor => "M";

	/// <inheritdoc />
	public override IEnumerable<string> GetAllIds()
	{
		return GetDataSheet().Keys;
	}

	/// <inheritdoc />
	public override bool Exists(string itemId)
	{
		return GetDataSheet().ContainsKey(itemId);
	}

	/// <inheritdoc />
	public override ParsedItemData GetData(string itemId)
	{
		if (!GetDataSheet().TryGetValue(itemId, out var data))
		{
			return null;
		}
		return new ParsedItemData(this, itemId, data.SheetIndex, data.Texture ?? "TileSheets/Mannequins", data.ID, TokenParser.ParseText(data.DisplayName), TokenParser.ParseText(data.Description), -24, null, null);
	}

	/// <inheritdoc />
	public override Item CreateItem(ParsedItemData data)
	{
		return new Mannequin(data.ItemId);
	}

	/// <inheritdoc />
	public override Rectangle GetSourceRect(ParsedItemData data, Texture2D texture, int spriteIndex)
	{
		return Object.getSourceRectForBigCraftable(texture, spriteIndex);
	}

	/// <summary>Get the item type's data asset.</summary>
	protected Dictionary<string, MannequinData> GetDataSheet()
	{
		return DataLoader.Mannequins(Game1.content);
	}
}
