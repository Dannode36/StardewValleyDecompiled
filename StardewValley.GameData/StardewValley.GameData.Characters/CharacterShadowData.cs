using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Characters;

/// <summary>As part of <see cref="T:StardewValley.GameData.Characters.CharacterData" />, configures how the NPC's shadow should be rendered.</summary>
public class CharacterShadowData
{
	/// <summary>Whether the shadow should be drawn.</summary>
	[ContentSerializer(Optional = true)]
	public bool Visible = true;

	/// <summary>A pixel offset applied to the shadow position.</summary>
	[ContentSerializer(Optional = true)]
	public Point Offset = Point.Zero;

	/// <summary>The scale at which to draw the shadow.</summary>
	/// <remarks>This is a multiplier applied to the default shadow scale, which can change based on factors like whether the NPC is jumping. For example, <c>0.5</c> means half the size it'd be drawn if you didn't specify a scale.</remarks>
	[ContentSerializer(Optional = true)]
	public float Scale = 1f;
}
