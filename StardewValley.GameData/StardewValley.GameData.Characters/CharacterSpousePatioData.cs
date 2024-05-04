using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Characters;

/// <summary>As part of <see cref="T:StardewValley.GameData.Characters.CharacterData" />, the data about the NPC's patio area on the farm when the player marries them.</summary>
public class CharacterSpousePatioData
{
	/// <summary>The default value for <see cref="F:StardewValley.GameData.Characters.CharacterSpousePatioData.MapSourceRect" />.</summary>
	public static readonly Rectangle DefaultMapSourceRect = new Rectangle(0, 0, 4, 4);

	/// <summary>The asset name within the content <c>Maps</c> folder which contains the patio. Defaults to <c>spousePatios</c>.</summary>
	[ContentSerializer(Optional = true)]
	public string MapAsset;

	/// <summary>The tile area within the <see cref="F:StardewValley.GameData.Characters.CharacterSpousePatioData.MapAsset" /> containing the spouse's patio. This must be a 4x4 tile area or smaller.</summary>
	[ContentSerializer(Optional = true)]
	public Rectangle MapSourceRect = DefaultMapSourceRect;

	/// <summary>The spouse's animation frames when they're in the patio. Each frame is a tuple containing the [0] frame index and [1] optional duration in milliseconds (default 100). If omitted or empty, the NPC won't be animated.</summary>
	[ContentSerializer(Optional = true)]
	public List<int[]> SpriteAnimationFrames;

	/// <summary>The pixel offset to apply to the NPC's sprite when they're animated in the patio.</summary>
	[ContentSerializer(Optional = true)]
	public Point SpriteAnimationPixelOffset;
}
