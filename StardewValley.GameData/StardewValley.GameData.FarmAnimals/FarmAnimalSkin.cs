using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.FarmAnimals;

/// <summary>As part of <see cref="T:StardewValley.GameData.FarmAnimals.FarmAnimalData" />, an alternate appearance for a farm animal.</summary>
public class FarmAnimalSkin
{
	/// <summary>A key which uniquely identifies the skin for this animal type. The ID should only contain alphanumeric/underscore/dot characters. For custom skins, this should be prefixed with your mod ID like <c>Example.ModId_SkinName</c>.</summary>
	public string Id;

	/// <summary>A multiplier for the probability to choose this skin when an animal is purchased. For example, <c>2</c> will double the chance this skin is selected relative to skins with the default <c>1</c>.</summary>
	[ContentSerializer(Optional = true)]
	public float Weight = 1f;

	/// <summary>If set, overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.Texture" />.</summary>
	[ContentSerializer(Optional = true)]
	public string Texture;

	/// <summary>If set, overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.HarvestedTexture" />.</summary>
	[ContentSerializer(Optional = true)]
	public string HarvestedTexture;

	/// <summary>If set, overrides <see cref="F:StardewValley.GameData.FarmAnimals.FarmAnimalData.BabyTexture" />.</summary>
	[ContentSerializer(Optional = true)]
	public string BabyTexture;
}
