using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Shirts;

/// <summary>The metadata for a shirt item that can be equipped by players.</summary>
public class ShirtData
{
	/// <summary>The shirt's internal name.</summary>
	[ContentSerializer(Optional = true)]
	public string Name = "Shirt";

	/// <summary>A tokenizable string for the shirt's display name.</summary>
	[ContentSerializer(Optional = true)]
	public string DisplayName = "[LocalizedText Strings\\Shirts:Shirt_Name]";

	/// <summary>A tokenizable string for the shirt's description.</summary>
	[ContentSerializer(Optional = true)]
	public string Description = "[LocalizedText Strings\\Shirts:Shirt_Description]";

	/// <summary>The price when purchased from shops.</summary>
	[ContentSerializer(Optional = true)]
	public int Price = 50;

	/// <summary>The asset name for the texture containing the shirt's sprite, or <c>null</c> for <c>Characters/Farmer/shirts</c>.</summary>
	[ContentSerializer(Optional = true)]
	public string Texture;

	/// <summary>The sprite's index in the spritesheet.</summary>
	public int SpriteIndex;

	/// <summary>The default shirt color.</summary>
	[ContentSerializer(Optional = true)]
	public string DefaultColor;

	/// <summary>Whether the shirt can be dyed.</summary>
	[ContentSerializer(Optional = true)]
	public bool CanBeDyed;

	/// <summary>Whether the shirt continuously shift colors. This overrides <see cref="F:StardewValley.GameData.Shirts.ShirtData.DefaultColor" /> and <see cref="F:StardewValley.GameData.Shirts.ShirtData.CanBeDyed" /> if set.</summary>
	[ContentSerializer(Optional = true)]
	public bool IsPrismatic;

	/// <summary>Whether the shirt has sleeves.</summary>
	[ContentSerializer(Optional = true)]
	public bool HasSleeves = true;

	/// <summary>Whether the shirt can be selected on the customization screen.</summary>
	[ContentSerializer(Optional = true)]
	public bool CanChooseDuringCharacterCustomization;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
