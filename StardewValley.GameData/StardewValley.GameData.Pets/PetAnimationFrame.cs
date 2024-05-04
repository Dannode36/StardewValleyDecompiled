using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Pets;

/// <summary>As part of <see cref="T:StardewValley.GameData.Pets.PetBehavior" />, the animation frames to play while the state is active.</summary>
public class PetAnimationFrame
{
	/// <summary>The frame index in the animation. This should be an incremental number starting at 0.</summary>
	public int Frame;

	/// <summary>The millisecond duration for which the frame should be kept on-screen before continuing to the next frame.</summary>
	public int Duration;

	/// <summary>Whether to play the footstep sound for the tile under the pet when the frame starts.</summary>
	[ContentSerializer(Optional = true)]
	public bool HitGround;

	/// <summary>Whether the pet should perform a small hop when the frame starts, including a 'dwop' sound.</summary>
	[ContentSerializer(Optional = true)]
	public bool Jump;

	/// <summary>The audio cue ID for the sound to play when the animation starts or loops. If set to the exact string <c>BARK</c>, the <see cref="F:StardewValley.GameData.Pets.PetData.BarkSound" /> or <see cref="F:StardewValley.GameData.Pets.PetBreed.BarkOverride" /> is used. Defaults to none.</summary>
	[ContentSerializer(Optional = true)]
	public string Sound;

	/// <summary>When set, the <see cref="F:StardewValley.GameData.Pets.PetAnimationFrame.Sound" /> is only audible if the pet is within this many tiles past the border of the screen. Default -1 (no distance check).</summary>
	[ContentSerializer(Optional = true)]
	public int SoundRangeFromBorder = -1;

	/// <summary>When set, the <see cref="F:StardewValley.GameData.Pets.PetAnimationFrame.Sound" /> is only audible if the pet is within this many tiles of the player. Default -1 (no distance check).</summary>
	[ContentSerializer(Optional = true)]
	public int SoundRange = -1;

	/// <summary>Whether to mute the <see cref="F:StardewValley.GameData.Pets.PetAnimationFrame.Sound" /> when the 'mute animal sounds' option is set.</summary>
	[ContentSerializer(Optional = true)]
	public bool SoundIsVoice;
}
