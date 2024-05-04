using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;

namespace StardewValley.ItemTypeDefinitions;

/// <summary>Manages the data for trinket items.</summary>
public class TrinketDataDefinition : BaseItemDataDefinition
{
	/// <inheritdoc />
	public override string Identifier => "(TR)";

	/// <inheritdoc />
	public override string StandardDescriptor => "TR";

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
		return new ParsedItemData(this, itemId, data.SheetIndex, data.Texture ?? "TileSheets\\weapons", data.ID, TokenParser.ParseText(data.DisplayName), TokenParser.ParseText(data.Description), -101, null, null);
	}

	/// <inheritdoc />
	public override Item CreateItem(ParsedItemData data)
	{
		return new Trinket(data.ItemId, Game1.random.Next(9999999));
	}

	/// <inheritdoc />
	public override Rectangle GetSourceRect(ParsedItemData data, Texture2D texture, int spriteIndex)
	{
		return Game1.getSourceRectForStandardTileSheet(texture, spriteIndex, 16, 16);
	}

	/// <summary>Get the item type's data asset.</summary>
	protected Dictionary<string, TrinketData> GetDataSheet()
	{
		return DataLoader.Trinkets(Game1.content);
	}
}
