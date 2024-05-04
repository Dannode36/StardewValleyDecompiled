using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Pants;

/// <summary>The metadata for a pants item that can be equipped by players.</summary>
public class PantsData
{
	/// <summary>The pants' internal name.</summary>
	public string Name = "Pants";

	/// <summary>A tokenizable string for the pants' display name.</summary>
	public string DisplayName = "[LocalizedText Strings\\Pants:Pants_Name]";

	/// <summary>A tokenizable string for the pants' description.</summary>
	public string Description = "[LocalizedText Strings\\Pants:Pants_Description]";

	/// <summary>The price when purchased from shops.</summary>
	[ContentSerializer(Optional = true)]
	public int Price = 50;

	/// <summary>The asset name for the texture containing the pants' sprite, or <c>null</c> for <c>Characters/Farmer/pants</c>.</summary>
	[ContentSerializer(Optional = true)]
	public string Texture;

	/// <summary>The sprite's index in the spritesheet.</summary>
	public int SpriteIndex;

	/// <summary>The default pants color.</summary>
	[ContentSerializer(Optional = true)]
	public string DefaultColor = "255 235 203";

	/// <summary>Whether the pants can be dyed.</summary>
	[ContentSerializer(Optional = true)]
	public bool CanBeDyed;

	/// <summary>Whether the pants continuously shift colors. This overrides <see cref="F:StardewValley.GameData.Pants.PantsData.DefaultColor" /> and <see cref="F:StardewValley.GameData.Pants.PantsData.CanBeDyed" /> if set.</summary>
	[ContentSerializer(Optional = true)]
	public bool IsPrismatic;

	/// <summary>Whether the pants can be selected on the customization screen.</summary>
	[ContentSerializer(Optional = true)]
	public bool CanChooseDuringCharacterCustomization;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
