using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Pets;

/// <summary>As part of <see cref="T:StardewValley.GameData.Pets.PetBehavior" />, a possible behavior transition that can be started.</summary>
public class PetBehaviorChanges
{
	/// <summary>The option's weight when randomly choosing a behavior, relative to other behaviors in the list (e.g. 2 is twice as likely as 1).</summary>
	[ContentSerializer(Optional = true)]
	public float Weight = 1f;

	/// <summary>Whether the transition can only happen if the pet is outside.</summary>
	[ContentSerializer(Optional = true)]
	public bool OutsideOnly;

	/// <summary>The name of the behavior to start if the pet is facing up.</summary>
	/// <remarks>See remarks on <see cref="F:StardewValley.GameData.Pets.PetBehaviorChanges.Behavior" />.</remarks>
	[ContentSerializer(Optional = true)]
	public string UpBehavior;

	/// <summary>The name of the behavior to start if the pet is facing down.</summary>
	/// <remarks>See remarks on <see cref="F:StardewValley.GameData.Pets.PetBehaviorChanges.Behavior" />.</remarks>
	[ContentSerializer(Optional = true)]
	public string DownBehavior;

	/// <summary>The name of the behavior to start if the pet is facing left.</summary>
	/// <remarks>See remarks on <see cref="F:StardewValley.GameData.Pets.PetBehaviorChanges.Behavior" />.</remarks>
	[ContentSerializer(Optional = true)]
	public string LeftBehavior;

	/// <summary>The name of the behavior to start if the pet is facing right.</summary>
	/// <remarks>See remarks on <see cref="F:StardewValley.GameData.Pets.PetBehaviorChanges.Behavior" />.</remarks>
	[ContentSerializer(Optional = true)]
	public string RightBehavior;

	/// <summary>The name of the behavior to start, if no directional behavior applies.</summary>
	/// <remarks>The pet will check for a behavior matching its facing direction first (like <see cref="F:StardewValley.GameData.Pets.PetBehaviorChanges.UpBehavior" />), then try the <see cref="F:StardewValley.GameData.Pets.PetBehaviorChanges.Behavior" />. If none are specified, the current behavior will continue unchanged.</remarks>
	[ContentSerializer(Optional = true)]
	public string Behavior;
}
