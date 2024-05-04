using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Pets;

/// <summary>As part of <see cref="T:StardewValley.GameData.Pets.PetData" />, how to render the pet during the summit perfection slide-show.</summary>
public class PetSummitPerfectionEventData
{
	/// <summary>The source rectangle within the texture to draw.</summary>
	public Rectangle SourceRect;

	/// <summary>The number of frames to show starting from the <see cref="F:StardewValley.GameData.Pets.PetSummitPerfectionEventData.SourceRect" />.</summary>
	public int AnimationLength;

	/// <summary>Whether to flip the pet sprite left-to-right.</summary>
	[ContentSerializer(Optional = true)]
	public bool Flipped;

	/// <summary>The motion to apply to the pet sprite.</summary>
	public Vector2 Motion;

	/// <summary>Whether to apply the 'ping pong' effect to the pet sprite animation.</summary>
	[ContentSerializer(Optional = true)]
	public bool PingPong;
}
