using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>As part of <see cref="T:StardewValley.GameData.FarmAnimals.FarmAnimalData" /> or <see cref="T:StardewValley.GameData.Machines.MachineData" />, a game state counter to increment.</summary>
public class StatIncrement
{
	/// <summary>The backing field for <see cref="P:StardewValley.GameData.StatIncrement.Id" />.</summary>
	private string IdImpl;

	/// <summary>A unique string ID for this entry within the current animal's list.</summary>
	[ContentSerializer(Optional = true)]
	public string Id
	{
		get
		{
			return StatName ?? IdImpl;
		}
		set
		{
			IdImpl = value;
		}
	}

	/// <summary>The qualified or unqualified item ID for the item to match.</summary>
	/// <remarks>You can specify any combination of <see cref="P:StardewValley.GameData.StatIncrement.RequiredItemId" /> and <see cref="P:StardewValley.GameData.StatIncrement.RequiredTags" />. The input item must match all specified fields; if none are specified, this conversion will always match.</remarks>
	[ContentSerializer(Optional = true)]
	public string RequiredItemId { get; set; }

	/// <summary>A comma-delimited list of context tags required on the main input item. The stat is only incremented if the item has all of these. You can negate a tag with <c>!</c> (like <c>bone_item,!fossil_item</c> for bone items that aren't fossils). Defaults to always enabled.</summary>
	/// <inheritdoc cref="P:StardewValley.GameData.StatIncrement.RequiredItemId" select="Remarks" />
	[ContentSerializer(Optional = true)]
	public List<string> RequiredTags { get; set; }

	/// <summary>The name of the stat counter field on <c>Game1.stats</c>.</summary>
	public string StatName { get; set; }
}
