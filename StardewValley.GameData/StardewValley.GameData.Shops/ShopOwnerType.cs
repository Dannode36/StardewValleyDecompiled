namespace StardewValley.GameData.Shops;

/// <summary>Specifies how a shop owner entry matches NPCs.</summary>
public enum ShopOwnerType
{
	/// <summary>The entry matches an NPC whose name is the entry's name.</summary>
	NamedNpc,
	/// <summary>The entry matches any NPC.</summary>
	Any,
	/// <summary>The entry matches regardless of whether an NPC is present.</summary>
	AnyOrNone,
	/// <summary>The entry matches only if no NPC is present.</summary>
	None
}
