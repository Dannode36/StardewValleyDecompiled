using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Movies;

/// <summary>The metadata for concession tastes for one or more NPCs.</summary>
public class ConcessionTaste
{
	/// <summary>A unique ID for this entry.</summary>
	[ContentSerializerIgnore]
	public string Id => Name;

	/// <summary>The internal NPC name for which to set tastes, or <c>"*"</c> to apply to all NPCs.</summary>
	public string Name { get; set; }

	/// <summary>The concessions loved by the matched NPCs.</summary>
	/// <remarks>
	///   This can be one of...
	///
	///   <list type="bullet">
	///     <item><description>the <see cref="F:StardewValley.GameData.Movies.ConcessionItemData.Name" /> for a specific concession;</description></item>
	///     <item><description>or a tag to match in <see cref="F:StardewValley.GameData.Movies.ConcessionItemData.ItemTags" />.</description></item>
	///   </list>
	/// </remarks>
	[ContentSerializer(Optional = true)]
	public List<string> LovedTags { get; set; }

	/// <summary>The concessions liked by matched NPCs.</summary>
	/// <remarks>See remarks on <see cref="P:StardewValley.GameData.Movies.ConcessionTaste.LovedTags" />.</remarks>
	[ContentSerializer(Optional = true)]
	public List<string> LikedTags { get; set; }

	/// <summary>The concessions liked by matched NPCs.</summary>
	/// <remarks>See remarks on <see cref="P:StardewValley.GameData.Movies.ConcessionTaste.DislikedTags" />.</remarks>
	[ContentSerializer(Optional = true)]
	public List<string> DislikedTags { get; set; }
}
