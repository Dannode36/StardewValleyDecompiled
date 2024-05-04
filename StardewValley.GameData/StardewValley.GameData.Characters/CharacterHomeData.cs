using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Characters;

/// <summary>As part of <see cref="T:StardewValley.GameData.Characters.CharacterData" />, a possible location for the NPC's default map.</summary>
public class CharacterHomeData
{
	/// <summary>An ID for this entry within the home list. This only needs to be unique within the current list.</summary>
	public string Id;

	/// <summary>A game state query which indicates whether this entry applies. Default true.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>The internal name for the home location where this NPC spawns and returns each day.</summary>
	public string Location;

	/// <summary>The tile position within the home location where this NPC spawns and returns each day.</summary>
	public Point Tile = Point.Zero;

	/// <summary>The default direction the NPC faces when they start each day. The possible values are <c>down</c>, <c>left</c>, <c>right</c>, and <c>up</c>.</summary>
	[ContentSerializer(Optional = true)]
	public string Direction = "up";
}
