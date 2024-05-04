using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Pets;

/// <summary>As part of <see cref="T:StardewValley.GameData.Pets.PetData" />, a cosmetic breed which can be selected in the character customization menu when creating a save.</summary>
public class PetBreed
{
	/// <summary>A key which uniquely identifies the pet breed. The ID should only contain alphanumeric/underscore/dot characters. For custom breeds, this should be prefixed with your mod ID like <c>Example.ModId_BreedName.</c></summary>
	public string Id;

	/// <summary>The asset name for the breed spritesheet for the pet's in-game sprite. This should be 128 pixels wide, and 256 (cat) or 288 (dog) pixels high.</summary>
	public string Texture;

	/// <summary>The asset name for the breed icon texture, shown on the character customization screen and in-game menu. This should be a 16x16 pixel icon.</summary>
	public string IconTexture;

	/// <summary>The icon's pixel area within the <see cref="F:StardewValley.GameData.Pets.PetBreed.IconTexture" />.</summary>
	public Rectangle IconSourceRect = Rectangle.Empty;

	/// <summary>Whether this pet can be chosen as a starter pet at character creation</summary>
	[ContentSerializer(Optional = true)]
	public bool CanBeChosenAtStart = true;

	/// <summary>Whether this pet can be adopted from Marnie once she starts offering pets.</summary>
	[ContentSerializer(Optional = true)]
	public bool CanBeAdoptedFromMarnie = true;

	/// <summary>The price this pet costs in Marnie's shop</summary>
	[ContentSerializer(Optional = true)]
	public int AdoptionPrice = 40000;

	/// <summary>Overrides the pet's <see cref="F:StardewValley.GameData.Pets.PetData.BarkSound" /> field for this breed, if set.</summary>
	[ContentSerializer(Optional = true)]
	public string BarkOverride;

	/// <summary>The pitch applied to the pet's bark sound, measured as a decimal value relative to 1.</summary>
	[ContentSerializer(Optional = true)]
	public float VoicePitch = 1f;
}
