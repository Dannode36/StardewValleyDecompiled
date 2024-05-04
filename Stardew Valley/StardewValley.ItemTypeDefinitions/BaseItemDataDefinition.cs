using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.ItemTypeDefinitions;

/// <summary>The base implementation for <see cref="T:StardewValley.ItemTypeDefinitions.IItemDataDefinition" /> instances.</summary>
public abstract class BaseItemDataDefinition : IItemDataDefinition
{
	/// <summary>A cache of parsed data by item ID.</summary>
	public Dictionary<string, ParsedItemData> ParsedItemCache = new Dictionary<string, ParsedItemData>();

	/// <inheritdoc />
	public abstract string Identifier { get; }

	/// <inheritdoc />
	public virtual string StandardDescriptor => null;

	/// <inheritdoc />
	public abstract IEnumerable<string> GetAllIds();

	/// <inheritdoc />
	public abstract bool Exists(string itemId);

	/// <inheritdoc />
	public abstract ParsedItemData GetData(string itemId);

	/// <inheritdoc />
	public ParsedItemData GetErrorData(string itemId)
	{
		return new ParsedItemData(this, itemId, 0, GetErrorTextureName(), "ErrorItem", ItemRegistry.GetErrorItemName(itemId), "???", -1, null, null, isErrorItem: true);
	}

	/// <inheritdoc />
	public abstract Item CreateItem(ParsedItemData data);

	/// <inheritdoc />
	public abstract Rectangle GetSourceRect(ParsedItemData data, Texture2D texture, int spriteIndex);

	/// <inheritdoc />
	public virtual Texture2D GetErrorTexture()
	{
		return Game1.mouseCursors;
	}

	/// <inheritdoc />
	public virtual string GetErrorTextureName()
	{
		return "LooseSprites\\Cursors";
	}

	/// <inheritdoc />
	public virtual Rectangle GetErrorSourceRect()
	{
		return new Rectangle(320, 496, 16, 16);
	}
}
