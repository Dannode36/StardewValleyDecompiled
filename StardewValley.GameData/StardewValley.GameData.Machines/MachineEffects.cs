using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Machines;

/// <summary>As part of <see cref="T:StardewValley.GameData.Machines.MachineData" />, a cosmetic effect shown when an item is loaded into the machine or while it's processing an input.</summary>
public class MachineEffects
{
	/// <summary>A unique string ID for this effect in this list.</summary>
	public string Id;

	/// <summary>A game state query which indicates whether to add this temporary sprite.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>The audio to play.</summary>
	[ContentSerializer(Optional = true)]
	public List<MachineSoundData> Sounds;

	/// <summary>The number of milliseconds for which each frame in <see cref="F:StardewValley.GameData.Machines.MachineEffects.Frames" /> is kept on-screen.</summary>
	[ContentSerializer(Optional = true)]
	public int Interval = 100;

	/// <summary>The animation to apply to the machine sprite, specified as a list of offsets relative to the base sprite index. Default none.</summary>
	[ContentSerializer(Optional = true)]
	public List<int> Frames;

	/// <summary>A duration in milliseconds during which the machine sprite should shake. Default none.</summary>
	[ContentSerializer(Optional = true)]
	public int ShakeDuration = -1;

	/// <summary>The temporary animated sprites to show.</summary>
	[ContentSerializer(Optional = true)]
	public List<TemporaryAnimatedSpriteDefinition> TemporarySprites;
}
