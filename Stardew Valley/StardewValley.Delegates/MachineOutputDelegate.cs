using StardewValley.GameData.Machines;

namespace StardewValley.Delegates;

/// <summary>The method signature for a custom <see cref="P:StardewValley.GameData.Machines.MachineItemOutput.OutputMethod" /> method.</summary>
/// <param name="machine">The machine instance for which to produce output.</param>
/// <param name="inputItem">The item being dropped into the machine, if applicable.</param>
/// <param name="probe">Whether the machine is only checking whether the input is valid. If so, the input/machine shouldn't be changed and no animations/sounds should play.</param>
/// <param name="outputData">The item output data from <c>Data/Machines</c> for which output is being created, if applicable.</param>
/// <param name="overrideMinutesUntilReady">The in-game minutes until the item will be ready to collect, if set. This overrides the equivalent fields in the machine data if set.</param>
/// <returns>Returns the item to produce, or <c>null</c> if none should be produced.</returns>
public delegate Item MachineOutputDelegate(Object machine, Item inputItem, bool probe, MachineItemOutput outputData, out int? overrideMinutesUntilReady);
