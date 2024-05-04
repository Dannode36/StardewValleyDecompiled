using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Buildings;

/// <summary>As part of <see cref="T:StardewValley.GameData.Buildings.BuildingData" />, the materials needed to construct a building.</summary>
public class BuildingMaterial
{
	/// <summary>A key which uniquely identifies the building material.</summary>
	[ContentSerializerIgnore]
	public string Id => ItemId;

	/// <summary>The required item ID (qualified or unqualified).</summary>
	public string ItemId { get; set; }

	/// <summary>The number of the item required.</summary>
	public int Amount { get; set; }
}
