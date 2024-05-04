using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Weddings;

/// <summary>As part of <see cref="T:StardewValley.GameData.Weddings.WeddingData" />, an NPC which should attend wedding events.</summary>
public class WeddingAttendeeData
{
	/// <summary>The internal name for the NPC.</summary>
	public string Id;

	/// <summary>A game state query which indicates whether the NPC should attend. Defaults to always attend.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>The NPC's tile position and facing direction when they attend. This uses the same format as field index 2 in an event script.</summary>
	public string Setup;

	/// <summary>The event script to run during the celebration, like <c>faceDirection Pierre 3 true</c> which makes Pierre turn to face left. This can contain any number of slash-delimited script commands.</summary>
	[ContentSerializer(Optional = true)]
	public string Celebration;

	/// <summary>Whether to add this NPC regardless of their <see cref="F:StardewValley.GameData.Characters.CharacterData.UnlockConditions" />.</summary>
	[ContentSerializer(Optional = true)]
	public bool IgnoreUnlockConditions;
}
