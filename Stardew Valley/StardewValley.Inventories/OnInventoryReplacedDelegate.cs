using System.Collections.Generic;

namespace StardewValley.Inventories;

/// <summary>The delegate for <see cref="E:StardewValley.Inventories.Inventory.OnInventoryReplaced" />.</summary>
/// <param name="inventory">The inventory instance.</param>
/// <param name="before">The previous item list.</param>
/// <param name="after">The new item list.</param>
public delegate void OnInventoryReplacedDelegate(Inventory inventory, IList<Item> before, IList<Item> after);
