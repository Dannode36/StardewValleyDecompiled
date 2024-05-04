using System;

namespace StardewValley.GameData.Machines;

/// <summary>As part of <see cref="T:StardewValley.GameData.Machines.MachineData" />, indicates when a machine should start producing output.</summary>
[Flags]
public enum MachineOutputTrigger
{
	/// <summary>The machine is never triggered automatically.</summary>
	None = 0,
	/// <summary>Apply this rule when an item is placed into the machine.</summary>
	ItemPlacedInMachine = 1,
	/// <summary>Apply this rule when the machine's previous output is collected. An output-collected rule won't require or consume the input items, and the input item will be the previous output.</summary>
	OutputCollected = 2,
	/// <summary>Apply this rule when the machine is put down. For example, the worm bin uses this to start as soon as it's put down.</summary>
	MachinePutDown = 4,
	/// <summary>Apply this rule when a new day starts, if it isn't already processing output. For example, the soda machine does this.</summary>
	DayUpdate = 8
}
