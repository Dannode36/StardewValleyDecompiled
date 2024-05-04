using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Buildings;

/// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, an output item produced when an input item is converted.</summary>
public class BuildingItemConversion
{
	/// <summary>A unique identifier for this entry. This only needs to be unique within the current list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ItemName</c>.</summary>
	public string Id;

	/// <summary>A list of context tags to match against an input item. An item must have all of these tags to be accepted.</summary>
	public List<string> RequiredTags;

	/// <summary>The number of the input item to consume.</summary>
	[ContentSerializer(Optional = true)]
	public int RequiredCount = 1;

	/// <summary>The maximum number of the input item which can be processed each day. Each conversion rule has its own separate maximum (e.g. if you have two rules each with a max of 1, then you can convert one of each daily). Set to -1 to allow unlimited conversions.</summary>
	[ContentSerializer(Optional = true)]
	public int MaxDailyConversions = 1;

	/// <summary>The name of the inventory defined in <see cref="F:StardewValley.GameData.Buildings.BuildingData.Chests" /> from which to take input items.</summary>
	public string SourceChest;

	/// <summary>The name of the inventory defined in <see cref="F:StardewValley.GameData.Buildings.BuildingData.Chests" /> in which to store output items.</summary>
	public string DestinationChest;

	/// <summary>The output items produced when an input item is converted.</summary>
	public List<GenericSpawnItemDataWithCondition> ProducedItems;
}
