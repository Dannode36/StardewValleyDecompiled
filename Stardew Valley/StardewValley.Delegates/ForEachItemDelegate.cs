using System;

namespace StardewValley.Delegates;

/// <summary>A callback invoked when iterating all items in the game.</summary>
/// <param name="item">The current item being iterated.</param>
/// <param name="remove">Delete this item instance.</param>
/// <param name="replaceWith">Replace this item with a new instance.</param>
/// <returns>Returns whether to continue iterating items in the game.</returns>
public delegate bool ForEachItemDelegate(Item item, Action remove, Action<Item> replaceWith);
