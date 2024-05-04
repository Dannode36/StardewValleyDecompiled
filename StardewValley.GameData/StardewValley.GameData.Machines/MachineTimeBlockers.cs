namespace StardewValley.GameData.Machines;

/// <summary>As part of a <see cref="T:StardewValley.GameData.Machines.MachineTimeBlockers" />, indicates when the machine should be paused.</summary>
public enum MachineTimeBlockers
{
	/// <summary>Pause when placed in an outside location.</summary>
	Outside,
	/// <summary>Pause when placed in an inside location.</summary>
	Inside,
	/// <summary>Pause in spring.</summary>
	Spring,
	/// <summary>Pause in summer.</summary>
	Summer,
	/// <summary>Pause in fall.</summary>
	Fall,
	/// <summary>Pause in winter.</summary>
	Winter,
	/// <summary>Pause on sunny days.</summary>
	Sun,
	/// <summary>Pause on rainy days.</summary>
	Rain,
	/// <summary>Always pause the machine. This is used in specialized cases where the timer is handled by advanced machine logic.</summary>
	Always
}
