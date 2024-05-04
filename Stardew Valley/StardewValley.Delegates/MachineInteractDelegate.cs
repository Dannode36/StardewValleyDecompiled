namespace StardewValley.Delegates;

/// <summary>The method signature for a custom <see cref="F:StardewValley.GameData.Machines.MachineData.InteractMethod" /> method.</summary>
/// <param name="machine">The machine instance for which to produce output.</param>
/// <param name="location">The location containing the machine.</param>
/// <param name="player">The player using the machine.</param>
/// <returns>Returns whether the interaction was handled.</returns>
public delegate bool MachineInteractDelegate(Object machine, GameLocation location, Farmer player);
