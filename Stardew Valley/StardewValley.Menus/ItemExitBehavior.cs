namespace StardewValley.Menus;

/// <summary>What to do with a held item if the menu is closed before it can be put down.</summary>
public enum ItemExitBehavior
{
	/// <summary>Place the item in the player's inventory, if they have room.</summary>
	ReturnToPlayer,
	/// <summary>Return to the item menu's underlying source (e.g. chest or dresser). If that inventory isn't persisted, the item will be lost.</summary>
	ReturnToMenu,
	/// <summary>Drop the item on the ground.</summary>
	Drop,
	/// <summary>Discard the item, so it'll be lost if it's not referenced from elsewhere.</summary>
	Discard
}
