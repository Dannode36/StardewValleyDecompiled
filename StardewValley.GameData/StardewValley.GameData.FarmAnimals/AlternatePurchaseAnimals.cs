using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.FarmAnimals;

/// <summary>As part of <see cref="T:StardewValley.GameData.FarmAnimals.FarmAnimalData" />, a possible variant for a farm animal.</summary>
public class AlternatePurchaseAnimals
{
	/// <summary>A unique string ID for this entry within the current animal's list.</summary>
	public string Id;

	/// <summary>A game state query which indicates whether this variant entry is available. Default always enabled.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>A list of animal IDs to spawn instead of the main ID field. If multiple are listed, one is chosen at random on purchase.</summary>
	public List<string> AnimalIds;
}
