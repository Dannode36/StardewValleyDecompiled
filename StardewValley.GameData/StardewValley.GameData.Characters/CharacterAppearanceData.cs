using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Characters;

public class CharacterAppearanceData
{
	/// <summary>An ID for this entry within the appearance list. This only needs to be unique within the current list.</summary>
	public string Id;

	/// <summary>A game state query which indicates whether this entry applies. Default true.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>The season when this appearance applies, or <c>null</c> for any season.</summary>
	[ContentSerializer(Optional = true)]
	public Season? Season;

	/// <summary>Whether the appearance can be used when the NPC is indoors.</summary>
	[ContentSerializer(Optional = true)]
	public bool Indoors = true;

	/// <summary>Whether the appearance can be used when the NPC is outdoors.</summary>
	[ContentSerializer(Optional = true)]
	public bool Outdoors = true;

	/// <summary>The asset name for the portrait texture, or null for the default portrait.</summary>
	[ContentSerializer(Optional = true)]
	public string Portrait;

	/// <summary>The asset name for the sprite texture, or null for the default sprite.</summary>
	[ContentSerializer(Optional = true)]
	public string Sprite;

	/// <summary>Whether this is island beach attire worn at the resort.</summary>
	/// <remarks>This is mutually exclusive: NPCs will never wear it in other contexts if it's true, and will never wear it as island attire if it's false.</remarks>
	[ContentSerializer(Optional = true)]
	public bool IsIslandAttire;

	/// <summary>The order in which this entry should be checked, where 0 is the default value used by most entries. Entries with the same precedence are checked in the order listed.</summary>
	[ContentSerializer(Optional = true)]
	public int Precedence;

	/// <summary>If multiple entries with the same <see cref="F:StardewValley.GameData.Characters.CharacterAppearanceData.Precedence" /> apply, the relative weight to use when randomly choosing one.</summary>
	/// <remarks>See remarks on <see cref="F:StardewValley.GameData.Characters.CharacterData.Appearance" />.</remarks>
	[ContentSerializer(Optional = true)]
	public int Weight;
}
