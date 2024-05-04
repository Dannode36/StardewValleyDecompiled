using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Pets;

/// <summary>As part of <see cref="T:StardewValley.GameData.Pets.PetData" />, a state in the pet's possible actions and behaviors.</summary>
public class PetBehavior
{
	/// <summary>A unique string ID for the state. This only needs to be unique within the pet type (e.g. cats and dogs can have different behaviors with the same name).</summary>
	public string Id;

	/// <summary>Whether to constrain the pet's facing direction to left and right while the state is active.</summary>
	[ContentSerializer(Optional = true)]
	public bool IsSideBehavior;

	/// <summary>Whether to point the pet in a random direction at the start of this state. If set, this overrides <see cref="F:StardewValley.GameData.Pets.PetBehavior.Direction" />.</summary>
	[ContentSerializer(Optional = true)]
	public bool RandomizeDirection;

	/// <summary>The specific direction to face at the start of this state (one of <c>left</c>, <c>right</c>, <c>up</c>, or <c>down</c>), unless overridden by <see cref="F:StardewValley.GameData.Pets.PetBehavior.RandomizeDirection" />.</summary>
	[ContentSerializer(Optional = true)]
	public string Direction;

	/// <summary>Whether to walk in the pet's facing direction.</summary>
	[ContentSerializer(Optional = true)]
	public bool WalkInDirection;

	/// <summary>Overrides the pet's <see cref="F:StardewValley.GameData.Pets.PetData.MoveSpeed" /> while this state is active, or <c>-1</c> to inherit it.</summary>
	[ContentSerializer(Optional = true)]
	public int MoveSpeed = -1;

	/// <summary>The audio cue ID for the sound to play when the state starts. If set to the exact string <c>BARK</c>, the <see cref="F:StardewValley.GameData.Pets.PetData.BarkSound" /> or <see cref="F:StardewValley.GameData.Pets.PetBreed.BarkOverride" /> is used. Defaults to none.</summary>
	[ContentSerializer(Optional = true)]
	public string SoundOnStart;

	/// <summary>When set, the <see cref="F:StardewValley.GameData.Pets.PetBehavior.SoundOnStart" /> is only audible if the pet is within this many tiles past the border of the screen. Default -1 (no distance check).</summary>
	[ContentSerializer(Optional = true)]
	public int SoundRangeFromBorder = -1;

	/// <summary>When set, the <see cref="F:StardewValley.GameData.Pets.PetBehavior.SoundOnStart" /> is only audible if the pet is within this many tiles of the player. Default -1 (no distance check).</summary>
	[ContentSerializer(Optional = true)]
	public int SoundRange = -1;

	/// <summary>Whether to mute the <see cref="F:StardewValley.GameData.Pets.PetBehavior.SoundOnStart" /> when the 'mute animal sounds' option is set.</summary>
	[ContentSerializer(Optional = true)]
	public bool SoundIsVoice;

	/// <summary>The millisecond duration for which to shake the pet when the state starts.</summary>
	[ContentSerializer(Optional = true)]
	public int Shake;

	/// <summary>The animation frames to play while this state is active.</summary>
	[ContentSerializer(Optional = true)]
	public List<PetAnimationFrame> Animation;

	/// <summary>What to do when the last animation frame is reached while the behavior is still active.</summary>
	[ContentSerializer(Optional = true)]
	public PetAnimationLoopMode LoopMode;

	/// <summary>The minimum number of times to play the animation, or <c>-1</c> to disable repeating the animation.</summary>
	/// <remarks>Both <see cref="F:StardewValley.GameData.Pets.PetBehavior.AnimationMinimumLoops" /> and <see cref="F:StardewValley.GameData.Pets.PetBehavior.AnimationMaximumLoops" /> must be set to have any effect. The game will choose an inclusive random value between them.</remarks>
	[ContentSerializer(Optional = true)]
	public int AnimationMinimumLoops = -1;

	/// <summary>The maximum number of times to play the animation, or <c>-1</c> to disable repeating the animation.</summary>
	/// <remarks>See remarks on <see cref="F:StardewValley.GameData.Pets.PetBehavior.AnimationMinimumLoops" />.</remarks>
	[ContentSerializer(Optional = true)]
	public int AnimationMaximumLoops = -1;

	/// <summary>The possible behavior transitions to start when the current behavior's animation ends. If multiple transitions are listed, one is selected at random.</summary>
	[ContentSerializer(Optional = true)]
	public List<PetBehaviorChanges> AnimationEndBehaviorChanges;

	/// <summary>The millisecond duration until the pet transitions to a behavior in the <see cref="F:StardewValley.GameData.Pets.PetBehavior.TimeoutBehaviorChanges" /> field, if set. This overrides <see cref="F:StardewValley.GameData.Pets.PetBehavior.MinimumDuration" /> and <see cref="F:StardewValley.GameData.Pets.PetBehavior.MaximumDuration" />.</summary>
	[ContentSerializer(Optional = true)]
	public int Duration = -1;

	/// <summary>The minimum millisecond duration until the pet transitions to a behavior in the <see cref="F:StardewValley.GameData.Pets.PetBehavior.TimeoutBehaviorChanges" /> field, if set. This is ignored if <see cref="F:StardewValley.GameData.Pets.PetBehavior.Duration" /> is set.</summary>
	/// <remarks>Both <see cref="F:StardewValley.GameData.Pets.PetBehavior.MinimumDuration" /> and <see cref="F:StardewValley.GameData.Pets.PetBehavior.MaximumDuration" /> must have a non-negative value to take effect.</remarks>
	[ContentSerializer(Optional = true)]
	public int MinimumDuration = -1;

	/// <summary>The maximum millisecond duration until the pet transitions to a behavior in the <see cref="F:StardewValley.GameData.Pets.PetBehavior.TimeoutBehaviorChanges" /> field, if set. This is ignored if <see cref="F:StardewValley.GameData.Pets.PetBehavior.Duration" /> is set.</summary>
	/// <remarks>See remarks on <see cref="F:StardewValley.GameData.Pets.PetBehavior.MinimumDuration" />.</remarks>
	[ContentSerializer(Optional = true)]
	public int MaximumDuration = -1;

	/// <summary>The possible behavior transitions to start when the <see cref="F:StardewValley.GameData.Pets.PetBehavior.Duration" /> or <see cref="F:StardewValley.GameData.Pets.PetBehavior.MinimumDuration" /> + <see cref="F:StardewValley.GameData.Pets.PetBehavior.MaximumDuration" /> values are reached. If multiple transitions are listed, one is selected at random.</summary>
	[ContentSerializer(Optional = true)]
	public List<PetBehaviorChanges> TimeoutBehaviorChanges;

	/// <summary>The possible behavior transitions to start when the player is within two tiles of the pet. If multiple transitions are listed, one is selected at random.</summary>
	[ContentSerializer(Optional = true)]
	public List<PetBehaviorChanges> PlayerNearbyBehaviorChanges;

	/// <summary>The probability at the start of each frame that the pet will transition to a behavior in the <see cref="F:StardewValley.GameData.Pets.PetBehavior.RandomBehaviorChanges" /> field, if set. Specified as a value between 0 (never) and 1 (always).</summary>
	[ContentSerializer(Optional = true)]
	public float RandomBehaviorChangeChance;

	/// <summary>The possible behavior transitions to start, based on a <see cref="F:StardewValley.GameData.Pets.PetBehavior.RandomBehaviorChangeChance" /> check at the start of each frame. If multiple transitions are listed, one is selected at random.</summary>
	[ContentSerializer(Optional = true)]
	public List<PetBehaviorChanges> RandomBehaviorChanges;

	/// <summary>The possible behavior transitions to start when the pet lands after jumping. If multiple transitions are listed, one is selected at random.</summary>
	[ContentSerializer(Optional = true)]
	public List<PetBehaviorChanges> JumpLandBehaviorChanges;
}
