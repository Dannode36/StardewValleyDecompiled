using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Characters;

/// <summary>As part of <see cref="T:StardewValley.GameData.Characters.CharacterData" />, the data about the NPC's spouse room in the farmhouse when the player marries them.</summary>
public class CharacterSpouseRoomData
{
	/// <summary>The default value for <see cref="F:StardewValley.GameData.Characters.CharacterSpouseRoomData.MapSourceRect" />.</summary>
	public static readonly Rectangle DefaultMapSourceRect = new Rectangle(0, 0, 6, 9);

	/// <summary>The asset name within the content <c>Maps</c> folder which contains the spouse room. Defaults to <c>spouseRooms</c>.</summary>
	[ContentSerializer(Optional = true)]
	public string MapAsset;

	/// <summary>The tile area within the <see cref="F:StardewValley.GameData.Characters.CharacterSpouseRoomData.MapAsset" /> containing the spouse's room.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle MapSourceRect = DefaultMapSourceRect;
}
