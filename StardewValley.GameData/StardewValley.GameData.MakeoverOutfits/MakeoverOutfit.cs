using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.MakeoverOutfits;

public class MakeoverOutfit
{
	/// <summary>An ID for this entry within the list.</summary>
	public string Id;

	public List<MakeoverItem> OutfitParts;

	[ContentSerializer(Optional = true)]
	public string Gender;
}
