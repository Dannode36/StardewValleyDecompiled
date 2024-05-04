using System.Diagnostics.CodeAnalysis;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Extensions;

/// <summary>Provides utility extension methods on <see cref="T:StardewValley.Item" /> types.</summary>
public static class ItemExtensions
{
	/// <summary>Get whether the item has the given type definition ID.</summary>
	/// <param name="item">The item instance.</param>
	/// <param name="typeId">The type definition ID, matching a constant like <see cref="F:StardewValley.ItemRegistry.type_object" />.</param>
	public static bool HasTypeId([NotNullWhen(true)] this IHaveItemTypeId item, string typeId)
	{
		return item?.GetItemTypeId() == typeId;
	}

	/// <summary>Get whether the item has object type <see cref="F:StardewValley.ItemRegistry.type_object" />.</summary>
	/// <param name="item">The item instance.</param>
	public static bool HasTypeObject([NotNullWhen(true)] this IHaveItemTypeId item)
	{
		return item.HasTypeId("(O)");
	}

	/// <summary>Get whether the item has object type <see cref="F:StardewValley.ItemRegistry.type_bigCraftable" />.</summary>
	/// <param name="item">The item instance.</param>
	public static bool HasTypeBigCraftable([NotNullWhen(true)] this IHaveItemTypeId item)
	{
		return item.HasTypeId("(BC)");
	}
}
