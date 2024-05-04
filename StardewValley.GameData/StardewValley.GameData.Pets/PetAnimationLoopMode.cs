namespace StardewValley.GameData.Pets;

/// <summary>As part of <see cref="T:StardewValley.GameData.Pets.PetBehavior" />, what to do when the last animation frame is reached while the behavior is still active.</summary>
public enum PetAnimationLoopMode
{
	/// <summary>Equivalent to <see cref="F:StardewValley.GameData.Pets.PetAnimationLoopMode.Loop" />.</summary>
	None,
	/// <summary>Restart the animation from the first frame.</summary>
	Loop,
	/// <summary>Keep the last frame visible until the animation ends.</summary>
	Hold
}
