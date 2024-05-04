namespace StardewValley.GameData.Characters;

/// <summary>How an NPC appears in the end-game perfection slide show.</summary>
public enum EndSlideShowBehavior
{
	/// <summary>The NPC doesn't appear in the slide show.</summary>
	Hidden,
	/// <summary>The NPC is added to the main group of NPCs which walk across the screen.</summary>
	MainGroup,
	/// <summary>The NPC is added to the trailing group of NPCs which follow the main group.</summary>
	TrailingGroup
}
