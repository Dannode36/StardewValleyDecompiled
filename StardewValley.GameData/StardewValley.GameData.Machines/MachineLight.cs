using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Machines;

/// <summary>As part of <see cref="T:StardewValley.GameData.Machines.MachineData" />, a light effect shown around the machine.</summary>
public class MachineLight
{
	/// <summary>The radius of the light emitted.</summary>
	[ContentSerializer(Optional = true)]
	public float Radius = 1f;

	/// <summary>A tint color to apply to the light. This can be a MonoGame color field name (like <c>ForestGreen</c>), RGB hex code (like <c>#AABBCC</c>), or RGBA hex code (like <c>#AABBCCDD</c>). Default <c>White</c> (no tint).</summary>
	[ContentSerializer(Optional = true)]
	public string Color;
}
