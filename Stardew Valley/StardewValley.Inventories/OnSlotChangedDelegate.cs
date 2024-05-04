namespace StardewValley.Inventories;

/// <summary>The delegate for <see cref="E:StardewValley.Inventories.Inventory.OnSlotChanged" />.</summary>
/// <param name="inventory">The inventory instance.</param>
/// <param name="index">The item slot's index within the inventory.</param>
/// <param name="before">The previous item value (which may be <c>null</c> when adding a stack).</param>
/// <param name="after">The new item value (which may be <c>null</c> when removing a stack).</param>
public delegate void OnSlotChangedDelegate(Inventory inventory, int index, Item before, Item after);
