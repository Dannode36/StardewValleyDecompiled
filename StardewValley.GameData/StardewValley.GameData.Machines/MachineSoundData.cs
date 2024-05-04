using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Machines;

/// <summary>As part of <see cref="T:StardewValley.GameData.Machines.MachineData" />, an audio cue to play.</summary>
public class MachineSoundData
{
	/// <summary>The audio cue ID to play.</summary>
	public string Id;

	/// <summary>The number of milliseconds until the sound should play.</summary>
	[ContentSerializer(Optional = true)]
	public int Delay;
}
