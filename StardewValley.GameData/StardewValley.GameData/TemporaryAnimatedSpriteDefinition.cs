using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>A cosmetic sprite to show temporarily, with optional effects and animation.</summary>
public class TemporaryAnimatedSpriteDefinition
{
	/// <summary>The unique string ID for this entry in the list.</summary>
	public string Id;

	/// <summary>A game state query which indicates whether to add this temporary sprite.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>The asset name for the texture under the game's <c>Content</c> folder for the animated sprite.</summary>
	public string Texture;

	/// <summary>The pixel area for the first animated frame within the <see cref="F:StardewValley.GameData.TemporaryAnimatedSpriteDefinition.Texture" />.</summary>
	public Rectangle SourceRect;

	/// <summary>The millisecond duration for each frame in the animation.</summary>
	[ContentSerializer(Optional = true)]
	public float Interval = 100f;

	/// <summary>The number of frames in the animation.</summary>
	[ContentSerializer(Optional = true)]
	public int Frames = 1;

	/// <summary>The number of times to repeat the animation.</summary>
	[ContentSerializer(Optional = true)]
	public int Loops;

	/// <summary>A pixel offset applied to the sprite, relative to the top-left corner of the machine's collision box.</summary>
	[ContentSerializer(Optional = true)]
	public Vector2 PositionOffset = Vector2.Zero;

	[ContentSerializer(Optional = true)]
	public bool Flicker;

	/// <summary>Whether to flip the sprite horizontally when it's drawn.</summary>
	[ContentSerializer(Optional = true)]
	public bool Flip;

	/// <summary>The tile Y position to use in the layer depth calculation, which affects which sprite is drawn on top if two sprites overlap.</summary>
	[ContentSerializer(Optional = true)]
	public float SortOffset;

	[ContentSerializer(Optional = true)]
	public float AlphaFade;

	/// <summary>A multiplier applied to the sprite size (in addition to the normal 4× pixel zoom).</summary>
	[ContentSerializer(Optional = true)]
	public float Scale = 1f;

	[ContentSerializer(Optional = true)]
	public float ScaleChange;

	/// <summary>The rotation to apply to the sprite when drawn, measured in radians.</summary>
	[ContentSerializer(Optional = true)]
	public float Rotation;

	[ContentSerializer(Optional = true)]
	public float RotationChange;

	/// <summary>A tint color to apply to the sprite. This can be a MonoGame color field name (like <c>ForestGreen</c>), RGB hex code (like <c>#AABBCC</c>), or RGBA hex code (like <c>#AABBCCDD</c>). Default <c>White</c> (no tint).</summary>
	[ContentSerializer(Optional = true)]
	public string Color;
}
