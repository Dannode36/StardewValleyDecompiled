using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Buildings;

/// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, an appearance which can be selected from the construction menu (like stone vs plank cabins).</summary>
public class BuildingSkin
{
	/// <summary>A key which uniquely identifies the skin. The ID should only contain alphanumeric/underscore/dot characters. For custom skins, it should be prefixed with your mod ID like <c>Example.ModId_SkinName</c>.</summary>
	public string Id;

	/// <summary>A tokenizable string for the skin's translated name.</summary>
	[ContentSerializer(Optional = true)]
	public string Name;

	/// <summary>A tokenizable string for the skin's translated description.</summary>
	[ContentSerializer(Optional = true)]
	public string Description;

	/// <summary>The asset name for the texture under the game's <c>Content</c> folder.</summary>
	public string Texture;

	/// <summary>If set, a game state query which indicates whether the skin should be available to apply. Defaults to always available.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>If set, overrides <see cref="F:StardewValley.GameData.Buildings.BuildingData.BuildDays" />.</summary>
	[ContentSerializer(Optional = true)]
	public int? BuildDays;

	/// <summary>If set, overrides <see cref="F:StardewValley.GameData.Buildings.BuildingData.BuildCost" />.</summary>
	[ContentSerializer(Optional = true)]
	public int? BuildCost;

	/// <summary>If set, overrides <see cref="F:StardewValley.GameData.Buildings.BuildingData.BuildMaterials" />.</summary>
	[ContentSerializer(Optional = true)]
	public List<BuildingMaterial> BuildMaterials;

	/// <summary>Whether this skin should be shown as a separate building option in the construction menu.</summary>
	[ContentSerializer(Optional = true)]
	public bool ShowAsSeparateConstructionEntry;

	/// <summary>Equivalent to the <see cref="F:StardewValley.GameData.Buildings.BuildingData.Metadata" /> field on the building. Properties defined in this field are added to the building's metadata when this skin is active, overwriting the previous property with the same name if applicable.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> Metadata = new Dictionary<string, string>();
}
