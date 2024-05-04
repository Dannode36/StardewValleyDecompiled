using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.ItemTypeDefinitions;

/// <summary>Manages the data for items of a given type.</summary>
/// <remarks>This is the low-level implementation for the type. Most code should use the <see cref="T:StardewValley.ItemRegistry" /> instead.</remarks>
public interface IItemDataDefinition
{
	/// <summary>The unique identifier for the type, prefixed to item names to uniquely qualify them.</summary>
	/// <remarks>This must be surrounded with parentheses, like <c>(O)</c> for objects or <c>(BC)</c> for big craftables.</remarks>
	string Identifier { get; }

	/// <summary>A legacy type identifier used in context tags.</summary>
	/// <remarks>All items have a context tag in the form <c>id_{qualified id}</c>. When this value is set, created items will also have an extra context tag in the form <c>id_{descriptor}_{id}</c>. This is intended to support some older item types, and isn't needed for newer item types which have unique item IDs.</remarks>
	string StandardDescriptor { get; }

	/// <summary>Get a list of all the item IDs defined by this type.</summary>
	IEnumerable<string> GetAllIds();

	/// <summary>Get whether an item ID is defined for this type.</summary>
	/// <param name="itemId">The unqualified item ID.</param>
	bool Exists(string itemId);

	/// <summary>Get the base data for an item.</summary>
	/// <param name="itemId">The unqualified item ID.</param>
	/// <returns>Returns the item data (if valid), else <c>null</c>.</returns>
	ParsedItemData GetData(string itemId);

	/// <summary>Get the base data for a generic Error Item instance.</summary>
	/// <param name="itemId">The unqualified item ID.</param>
	ParsedItemData GetErrorData(string itemId);

	/// <summary>Create an item instance.</summary>
	/// <param name="data">The parsed item data.</param>
	/// <returns>Returns an item instance, or a generic Error Item if it's invalid.</returns>
	Item CreateItem(ParsedItemData data);

	/// <summary>Get the pixel area to render within an item's sprite sheet.</summary>
	/// <param name="data">The parsed item ID.</param>
	/// <param name="texture">The texture for which to get the source rectangle.</param>
	/// <param name="spriteIndex">The sprite index to render.</param>
	/// <remarks>Note: implementations should not try to access the texture or source rect through <paramref name="data" />, to avoid an infinite loop when this method is called to initialize it.</remarks>
	Rectangle GetSourceRect(ParsedItemData data, Texture2D texture, int spriteIndex);

	/// <summary>Get the texture to render for broken Error Item instances.</summary>
	Texture2D GetErrorTexture();

	/// <summary>Get the asset name for the texture returned by <see cref="M:StardewValley.ItemTypeDefinitions.IItemDataDefinition.GetErrorTexture" />.</summary>
	string GetErrorTextureName();

	/// <summary>Get the pixel area to render within the texture returned by <see cref="M:StardewValley.ItemTypeDefinitions.IItemDataDefinition.GetErrorTexture" />.</summary>
	Rectangle GetErrorSourceRect();
}
