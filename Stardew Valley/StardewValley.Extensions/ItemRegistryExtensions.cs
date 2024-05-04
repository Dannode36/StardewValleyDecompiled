using System.Collections.Generic;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Extensions;

/// <summary>Provides utility extension methods on <see cref="T:StardewValley.ItemRegistry" /> and <see cref="T:StardewValley.ItemTypeDefinitions.IItemDataDefinition" /> types.</summary>
public static class ItemRegistryExtensions
{
	/// <summary>Get the parsed data for each item provided by this item data definition.</summary>
	/// <param name="definition">The item data definition to query.</param>
	public static IEnumerable<ParsedItemData> GetAllData(this IItemDataDefinition definition)
	{
		foreach (string id in definition.GetAllIds())
		{
			yield return ItemRegistry.GetDataOrErrorItem(definition.Identifier + id);
		}
	}
}
