using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Movies;

/// <summary>The metadata for a concession which can be purchased at the movie theater.</summary>
public class ConcessionItemData
{
	/// <summary>A key which uniquely identifies this concession. This should only contain alphanumeric/underscore/dot characters. For custom concessions, this should be prefixed with your mod ID like <c>Example.ModId_ConcessionName</c>.</summary>
	public string Id;

	/// <summary>The internal name for the concession item.</summary>
	public string Name;

	/// <summary>The tokenizable string for the item's translated display name.</summary>
	public string DisplayName;

	/// <summary>The tokenizable string for the item's translated description.</summary>
	public string Description;

	/// <summary>The gold price to purchase the concession.</summary>
	public int Price;

	/// <summary>The asset name for the texture containing the concession's sprite.</summary>
	public string Texture;

	/// <summary>The index within the <see cref="F:StardewValley.GameData.Movies.ConcessionItemData.Texture" /> for the concession sprite, where 0 is the top-left icon.</summary>
	public int SpriteIndex;

	/// <summary>A list of tags which describe the concession, which can be matched by <see cref="T:StardewValley.GameData.Movies.ConcessionTaste" /> fields.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> ItemTags;
}
