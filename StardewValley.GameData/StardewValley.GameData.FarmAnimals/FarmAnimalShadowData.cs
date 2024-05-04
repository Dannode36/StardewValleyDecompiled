using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.FarmAnimals;

/// <summary>As part of <see cref="T:StardewValley.GameData.FarmAnimals.FarmAnimalData" />, configures how the animal's shadow should be rendered.</summary>
public class FarmAnimalShadowData
{
	/// <summary>Whether the shadow should be drawn.</summary>
	[ContentSerializer(Optional = true)]
	public bool Visible = true;

	/// <summary>A pixel offset applied to the shadow position.</summary>
	[ContentSerializer(Optional = true)]
	public Point? Offset;

	/// <summary>The scale at which to draw the shadow, or <c>null</c> to apply the default logic.</summary>
	[ContentSerializer(Optional = true)]
	public float? Scale;
}
