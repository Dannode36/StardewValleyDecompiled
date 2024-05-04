using System;
using System.Collections.Generic;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley;

/// <summary>Manages item data for the various item types.</summary>
public static class ItemRegistry
{
	/// <summary>A cache of the registered <see cref="F:StardewValley.ItemRegistry.ItemTypes" /> by their <see cref="P:StardewValley.ItemTypeDefinitions.IItemDataDefinition.Identifier" />.</summary>
	private static readonly Dictionary<string, IItemDataDefinition> IdentifierLookup = new Dictionary<string, IItemDataDefinition>();

	/// <summary>A cache of parsed and/or resolved item IDs.</summary>
	private static readonly Dictionary<string, ItemMetadata> CachedItems = new Dictionary<string, ItemMetadata>();

	/// <summary>The item types to search for item IDs.</summary>
	/// <remarks>These should be listed in the priority order for unqualified item IDs. Make sure to call <see cref="M:StardewValley.ItemRegistry.ResetCache" /> when changing this list.</remarks>
	[NonInstancedStatic]
	public static readonly List<IItemDataDefinition> ItemTypes = new List<IItemDataDefinition>();

	/// <summary>The type identifier for items provided by <see cref="T:StardewValley.ItemTypeDefinitions.ObjectDataDefinition" />.</summary>
	public const string type_object = "(O)";

	/// <summary>The type identifier for items provided by <see cref="T:StardewValley.ItemTypeDefinitions.BigCraftableDataDefinition" />.</summary>
	public const string type_bigCraftable = "(BC)";

	/// <summary>The type identifier for items provided by <see cref="T:StardewValley.ItemTypeDefinitions.BootsDataDefinition" />.</summary>
	public const string type_boots = "(B)";

	/// <summary>The type identifier for items provided by <see cref="T:StardewValley.ItemTypeDefinitions.FlooringDataDefinition" />.</summary>
	public const string type_floorpaper = "(FL)";

	/// <summary>The type identifier for items provided by <see cref="T:StardewValley.ItemTypeDefinitions.FurnitureDataDefinition" />.</summary>
	public const string type_furniture = "(F)";

	/// <summary>The type identifier for items provided by <see cref="T:StardewValley.ItemTypeDefinitions.HatDataDefinition" />.</summary>
	public const string type_hat = "(H)";

	/// <summary>The type identifier for items provided by <see cref="!:MannequinDataDefintion" />.</summary>
	public const string type_mannequin = "(M)";

	/// <summary>The type identifier for items provided by <see cref="T:StardewValley.ItemTypeDefinitions.PantsDataDefinition" />.</summary>
	public const string type_pants = "(P)";

	/// <summary>The type identifier for items provided by <see cref="T:StardewValley.ItemTypeDefinitions.ShirtDataDefinition" />.</summary>
	public const string type_shirt = "(S)";

	/// <summary>The type identifier for items provided by <see cref="T:StardewValley.ItemTypeDefinitions.ToolDataDefinition" />.</summary>
	public const string type_tool = "(T)";

	/// <summary>The type identifier for items provided by <see cref="T:StardewValley.ItemTypeDefinitions.TrinketDataDefinition" />.</summary>
	public const string type_trinket = "(TR)";

	/// <summary>The type identifier for items provided by <see cref="T:StardewValley.ItemTypeDefinitions.WallpaperDataDefinition" />.</summary>
	public const string type_wallpaper = "(WP)";

	/// <summary>The type identifier for items provided by <see cref="T:StardewValley.ItemTypeDefinitions.WeaponDataDefinition" />.</summary>
	public const string type_weapon = "(W)";

	/// <summary>Register the vanilla item types.</summary>
	/// <remarks>This should only be invoked once.</remarks>
	internal static void RegisterItemTypes()
	{
		IItemDataDefinition[] array = new IItemDataDefinition[13]
		{
			new ObjectDataDefinition(),
			new BigCraftableDataDefinition(),
			new FurnitureDataDefinition(),
			new WeaponDataDefinition(),
			new BootsDataDefinition(),
			new HatDataDefinition(),
			new MannequinDataDefinition(),
			new PantsDataDefinition(),
			new ShirtDataDefinition(),
			new ToolDataDefinition(),
			new TrinketDataDefinition(),
			new WallpaperDataDefinition(),
			new FlooringDataDefinition()
		};
		for (int i = 0; i < array.Length; i++)
		{
			AddTypeDefinition(array[i]);
		}
	}

	/// <summary>Add an item type definition which can provide items.</summary>
	/// <param name="definition">The item type definition to add.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="definition" /> is null.</exception>
	/// <exception cref="T:System.InvalidOperationException">The <paramref name="definition" />'s <see cref="P:StardewValley.ItemTypeDefinitions.IItemDataDefinition.Identifier" /> value is invalid or already exists in the registry.</exception>
	public static void AddTypeDefinition(IItemDataDefinition definition)
	{
		if (definition == null)
		{
			throw new ArgumentNullException("definition");
		}
		string identifier = definition.Identifier;
		if (string.IsNullOrWhiteSpace(identifier))
		{
			throw GetException("it has no identifier");
		}
		if (identifier.Length < 2 || identifier[0] != '(' || identifier[identifier.Length - 1] != ')')
		{
			throw GetException("its identifier must start with '(' and end with ')'");
		}
		if (identifier.IndexOf('(', 1) != -1 || identifier.IndexOf(')') != identifier.Length - 1)
		{
			throw GetException("its identifier can't contain '(' or ')' except as the first and last character respectively");
		}
		if (IdentifierLookup.ContainsKey(identifier))
		{
			throw GetException("its identifier is already registered");
		}
		ItemTypes.Add(definition);
		IdentifierLookup[identifier] = definition;
		ResetCache();
		InvalidOperationException GetException(string reason)
		{
			return new InvalidOperationException($"Can't add item data definition of type '{definition.GetType().FullName}'{((!string.IsNullOrWhiteSpace(definition.Identifier)) ? (" with identifier '" + definition.Identifier + "'") : "")} because {reason}.");
		}
	}

	/// <summary>Get the item type definition with the given identifier, or <c>null</c> if none is found.</summary>
	/// <param name="identifier">The <see cref="P:StardewValley.ItemTypeDefinitions.IItemDataDefinition.Identifier" /> value to match.</param>
	/// <returns>Returns the item type definition (if found), else <c>null</c>.</returns>
	public static IItemDataDefinition GetTypeDefinition(string identifier)
	{
		if (identifier == null)
		{
			return null;
		}
		if (!IdentifierLookup.TryGetValue(identifier, out var type))
		{
			return null;
		}
		return type;
	}

	/// <summary>Get the item type definition with the given identifier.</summary>
	/// <param name="identifier">The <see cref="P:StardewValley.ItemTypeDefinitions.IItemDataDefinition.Identifier" /> value to match.</param>
	/// <returns>Returns the item type definition.</returns>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">No type definition was found for the given identifier.</exception>
	public static IItemDataDefinition RequireTypeDefinition(string identifier)
	{
		return GetTypeDefinition(identifier) ?? throw new KeyNotFoundException("No item type definition found with ID '" + identifier + "'.");
	}

	/// <summary>Get the item type definition with the given identifier.</summary>
	/// <param name="identifier">The <see cref="P:StardewValley.ItemTypeDefinitions.IItemDataDefinition.Identifier" /> value to match.</param>
	/// <returns>Returns the item type definition.</returns>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">No type definition was found for the given identifier.</exception>
	/// <exception cref="T:System.InvalidCastException">The type definition found can't be cast to <typeparamref name="TItemDataDefinition" />.</exception>
	public static TItemDataDefinition RequireTypeDefinition<TItemDataDefinition>(string identifier) where TItemDataDefinition : class, IItemDataDefinition
	{
		IItemDataDefinition definition = GetTypeDefinition(identifier) ?? throw new KeyNotFoundException("No item type definition found with ID '" + identifier + "'.");
		return (definition as TItemDataDefinition) ?? throw new InvalidCastException($"The item type definition for ID '{identifier}' implements {definition.GetType().FullName}, but expected {typeof(TItemDataDefinition).FullName}.");
	}

	/// <summary>Get the item type definition for <see cref="F:StardewValley.ItemRegistry.type_object" />.</summary>
	/// <remarks>See <see cref="M:StardewValley.ItemRegistry.GetTypeDefinition(System.String)" /> or <see cref="M:StardewValley.ItemRegistry.RequireTypeDefinition(System.String)" /> for other item types.</remarks>
	public static ObjectDataDefinition GetObjectTypeDefinition()
	{
		return RequireTypeDefinition<ObjectDataDefinition>("(O)");
	}

	/// <summary>Reset all cached item data.</summary>
	public static void ResetCache()
	{
		CachedItems.Clear();
		ItemContextTagManager.ResetCache();
	}

	/// <summary>Get whether an item has the given qualified or unqualified item ID. If the item ID is unqualified, this checks whether it's the item that would be selected based on the item type priority order (e.g. <c>128</c> would always match object 128, not hat 128).</summary>
	/// <param name="item">The item instance to compare.</param>
	/// <param name="itemId">The item ID to compare with.</param>
	public static bool HasItemId(Item item, string itemId)
	{
		if (item == null)
		{
			return string.IsNullOrEmpty(itemId);
		}
		return item.QualifiedItemId == QualifyItemId(itemId);
	}

	/// <summary>Get whether the item ID is qualified with the type definition prefix (like <c>(O)128</c> instead of <c>128</c>).</summary>
	/// <param name="itemId">The item ID to check.</param>
	/// <remarks>This only checks the item ID format. To check if the item exists, use <see cref="M:StardewValley.ItemRegistry.Exists(System.String)" /> or <see cref="M:StardewValley.ItemRegistry.ResolveMetadata(System.String)" /> instead.</remarks>
	public static bool IsQualifiedItemId(string itemId)
	{
		if (itemId != null && itemId.StartsWith('('))
		{
			return itemId.Contains(')');
		}
		return false;
	}

	/// <summary>Get a qualified item ID for a given item ID.</summary>
	/// <param name="itemId">The qualified or unqualified item ID.</param>
	/// <returns>Returns the qualified item ID (if the item was successfully resolved), else the <paramref name="itemId" /> as-is (if it's already qualified), else <c>null</c>.</returns>
	public static string QualifyItemId(string itemId)
	{
		ItemMetadata metadata = GetMetadata(itemId);
		if (metadata == null)
		{
			return null;
		}
		if (metadata.QualifiedItemId != null)
		{
			return metadata.QualifiedItemId;
		}
		metadata.GetTypeDefinition();
		if (metadata.QualifiedItemId != null)
		{
			return metadata.QualifiedItemId;
		}
		if (!itemId.StartsWith('(') || !itemId.Contains(')'))
		{
			return null;
		}
		return itemId;
	}

	/// <summary>Qualify an item ID with the given type, without trying to resolve the item.</summary>
	/// <param name="itemId">The qualified or unqualified item ID.</param>
	/// <param name="typeDefinitionId">The type definition ID to set (like <see cref="F:StardewValley.ItemRegistry.type_object" />).</param>
	/// <param name="overrideIfQualified">If the <paramref name="itemId" /> is already qualified, whether to replace its type with <paramref name="typeDefinitionId" />. If false, this method will only change unqualified item IDs.</param>
	/// <remarks>This is specialized, and usually only used to migrate older data which has a predefined type. You should usually use <see cref="M:StardewValley.ItemRegistry.QualifyItemId(System.String)" /> instead.</remarks>
	public static string ManuallyQualifyItemId(string itemId, string typeDefinitionId, bool overrideIfQualified = false)
	{
		if (string.IsNullOrWhiteSpace(itemId))
		{
			return itemId;
		}
		if (itemId.StartsWith('('))
		{
			if (!overrideIfQualified)
			{
				return itemId;
			}
			int splitIndex = itemId.IndexOf(')') + 1;
			if (splitIndex > 0)
			{
				return typeDefinitionId + itemId.Substring(splitIndex).Trim();
			}
		}
		return typeDefinitionId + itemId;
	}

	/// <summary>Parse an item ID and get the associated item metadata.</summary>
	/// <param name="itemId">The qualified or unqualified item ID to parse.</param>
	/// <returns>Returns the item metadata (if the <paramref name="itemId" /> format is valid), else <c>null</c>. This may return an item which doesn't exist, as long as the ID format is valid. You can use <see cref="M:StardewValley.ItemRegistry.ResolveMetadata(System.String)" /> instead if you need a valid item, or check <see cref="M:StardewValley.ItemTypeDefinitions.ItemMetadata.Exists" /> on the returned value.</returns>
	public static ItemMetadata GetMetadata(string itemId)
	{
		if (string.IsNullOrWhiteSpace(itemId))
		{
			return null;
		}
		if (CachedItems.Count == 0)
		{
			RebuildCache();
		}
		if (!CachedItems.TryGetValue(itemId, out var metadata))
		{
			if (itemId[0] == '(')
			{
				int splitIndex = itemId.IndexOf(')') + 1;
				if (splitIndex >= 0)
				{
					metadata = new ItemMetadata(itemId, itemId.Substring(splitIndex), itemId.Substring(0, splitIndex));
				}
			}
			else
			{
				metadata = new ItemMetadata(null, itemId, null);
			}
			CachedItems[itemId] = metadata;
		}
		return metadata;
	}

	/// <summary>Get whether an item exists with the given ID.</summary>
	/// <param name="itemId">The qualified or unqualified item ID.</param>
	public static bool Exists(string itemId)
	{
		return GetMetadata(itemId)?.Exists() ?? false;
	}

	/// <summary>Parse an item ID and get the associated item metadata if it exists.</summary>
	/// <param name="itemId">The raw item ID to parse.</param>
	/// <returns>Returns the item metadata (if the <paramref name="itemId" /> format is valid and the item exists), else <c>null</c>. This is a shortcut for calling <see cref="M:StardewValley.ItemRegistry.GetMetadata(System.String)" /> and then checking <see cref="M:StardewValley.ItemTypeDefinitions.ItemMetadata.Exists" />.</returns>
	public static ItemMetadata ResolveMetadata(string itemId)
	{
		ItemMetadata metadata = GetMetadata(itemId);
		if (metadata == null || !metadata.Exists())
		{
			return null;
		}
		return metadata;
	}

	/// <summary>Get the type definition for the given item metadata.</summary>
	/// <param name="metadata">The item meta data to resolve.</param>
	/// <returns>Returns the type definition, or <c>null</c> if none was found.</returns>
	/// <remarks>This is called from <see cref="M:StardewValley.ItemTypeDefinitions.ItemMetadata.GetTypeDefinition" /> if needed, and should not be called by other code.</remarks>
	internal static IItemDataDefinition GetTypeDefinitionFor(ItemMetadata metadata)
	{
		if (metadata.TypeIdentifier != null)
		{
			return GetTypeDefinition(metadata.TypeIdentifier);
		}
		foreach (IItemDataDefinition type in ItemTypes)
		{
			if (type.Exists(metadata.LocalItemId))
			{
				return type;
			}
		}
		return null;
	}

	/// <summary>Get the parsed data for an item.</summary>
	/// <param name="itemId">The qualified or unqualified item ID.</param>
	/// <returns>Returns the item data if found, else <c>null</c>.</returns>
	/// <remarks>This is a shortcut for calling <see cref="M:StardewValley.ItemRegistry.ResolveMetadata(System.String)" /> and then <see cref="M:StardewValley.ItemTypeDefinitions.ItemMetadata.GetParsedData" />.</remarks>
	public static ParsedItemData GetData(string itemId)
	{
		return ResolveMetadata(itemId)?.GetParsedData();
	}

	/// <summary>Get the parsed data for an item.</summary>
	/// <param name="itemId">The qualified or unqualified item ID.</param>
	/// <returns>Returns the data for the item if found, else for a default error item.</returns>
	public static ParsedItemData GetDataOrErrorItem(string itemId)
	{
		ItemMetadata metadata = GetMetadata(itemId);
		IItemDataDefinition itemType = metadata?.GetTypeDefinition();
		if (itemType != null)
		{
			ParsedItemData data = metadata.GetParsedData();
			if (data != null)
			{
				return data;
			}
		}
		return itemType?.GetErrorData(metadata?.LocalItemId ?? itemId) ?? RequireTypeDefinition("(O)").GetErrorData(metadata?.LocalItemId ?? itemId);
	}

	/// <summary>Create an item instance.</summary>
	/// <param name="itemId">The qualified or unqualified item ID.</param>
	/// <param name="amount">The stack size for the created item, if applicable.</param>
	/// <param name="quality">The quality for the created item, if applicable.</param>
	/// <param name="allowNull">Whether to return <c>null</c> if the item doesn't exist. If this is false, an Error Item instance will be returned instead.</param>
	/// <returns>Returns the item instance (if the <paramref name="itemId" /> is valid), else an Error Item instance (if <paramref name="allowNull" /> is false), else <c>null</c>.</returns>
	public static Item Create(string itemId, int amount = 1, int quality = 0, bool allowNull = false)
	{
		ParsedItemData data = (allowNull ? GetData(itemId) : GetDataOrErrorItem(itemId));
		if (data == null || data.IsErrorItem)
		{
			if (allowNull)
			{
				return null;
			}
			if (data == null)
			{
				data = RequireTypeDefinition("(O)").GetErrorData(itemId);
			}
		}
		Item item = data.ItemType.CreateItem(data);
		if (amount != 1)
		{
			item.Stack = amount;
			item.FixStackSize();
		}
		if (quality != 0)
		{
			item.Quality = quality;
			item.FixQuality();
		}
		return item;
	}

	/// <summary>Create an item instance as the given type.</summary>
	/// <typeparam name="TItem">The expected item type.</typeparam>
	/// <param name="itemId">The qualified or unqualified item ID.</param>
	/// <param name="amount">The stack size for the created item, if applicable.</param>
	/// <param name="quality">The quality for the created item, if applicable.</param>
	/// <param name="allowNull">Whether to return <c>null</c> if the item doesn't exist. If this is false, an Error Item instance will be returned instead.</param>
	/// <returns>Returns the item instance (if the <paramref name="itemId" /> is valid), else an Error Item instance (if <paramref name="allowNull" /> is false), else <c>null</c>.</returns>
	/// <exception cref="T:System.InvalidCastException">The item type isn't compatible with <typeparamref name="TItem" />.</exception>
	/// <remarks>In most cases you should call the non-generic implementation and work with the base <see cref="T:StardewValley.Item" /> type instead.</remarks>
	public static TItem Create<TItem>(string itemId, int amount = 1, int quality = 0, bool allowNull = false) where TItem : Item
	{
		Item item = Create(itemId, amount, quality, allowNull);
		if (item != null)
		{
			if (item is TItem castItem)
			{
				return castItem;
			}
			throw new InvalidCastException($"Can't create item ID '{itemId}' as a {typeof(TItem).Name} type because it's a {item.GetType()} instance.");
		}
		return null;
	}

	/// <summary>Get a translated 'Error Item' name.</summary>
	public static string GetErrorItemName()
	{
		return Game1.content.LoadString("Strings\\StringsFromCSFiles:CraftingRecipe.cs.575");
	}

	/// <summary>Get a translated 'Error Item ({id})' name.</summary>
	/// <param name="itemId">The item ID to display.</param>
	public static string GetErrorItemName(string itemId)
	{
		return GetErrorItemName() + " (" + itemId + ")";
	}

	/// <summary>Get a translated 'Unnamed Item' name.</summary>
	public static string GetUnnamedItemName()
	{
		return Game1.content.LoadString("Strings\\StringsFromCSFiles:UnnamedItem");
	}

	/// <summary>Get a translated 'Unnamed Item ({id})' name.</summary>
	/// <param name="itemId">The item ID to display.</param>
	public static string GetUnnamedItemName(string itemId)
	{
		return GetUnnamedItemName() + " (" + itemId + ")";
	}

	/// <summary>Reset the cache and store all known item IDs.</summary>
	/// <remarks>This minimizes re-scanning item types for unqualified IDs and parsing qualified IDs. See also <see cref="M:StardewValley.ItemRegistry.ResetCache" /> which defers the rebuild until the next item lookup.</remarks>
	private static void RebuildCache()
	{
		CachedItems.Clear();
		foreach (IItemDataDefinition type in ItemTypes)
		{
			string qualifier = type.Identifier;
			foreach (string id in type.GetAllIds())
			{
				string qualifiedId = qualifier + id;
				ItemMetadata parsed = new ItemMetadata(qualifiedId, id, qualifier);
				parsed.SetTypeDefinition(qualifier, type, true);
				CachedItems[qualifiedId] = parsed;
				CachedItems.TryAdd(id, parsed);
			}
		}
	}
}
