using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.GiantCrops;

/// <summary>A custom giant crop that may spawn in-game.</summary>
public class GiantCropData
{
	/// <summary>The qualified or unqualified harvest item ID of the crops from which this giant crop can grow. If multiple giant crops have the same item ID, the first one whose <see cref="F:StardewValley.GameData.GiantCrops.GiantCropData.Chance" /> matches will be used.</summary>
	public string FromItemId;

	/// <summary>The items to produce when this giant crop is broken. All matching items will be produced.</summary>
	public List<GiantCropHarvestItemData> HarvestItems;

	/// <summary>The asset name for the texture containing the giant crop's sprite.</summary>
	public string Texture;

	/// <summary>The top-left pixel position of the sprite within the <see cref="F:StardewValley.GameData.GiantCrops.GiantCropData.Texture" />. Defaults to (0, 0).</summary>
	[ContentSerializer(Optional = true)]
	public Point TexturePosition;

	/// <summary>The area in tiles occupied by the giant crop. This affects both its sprite size (which should be 16 pixels per tile) and the grid of crops needed for it to grow. Note that giant crops are drawn with an extra tile's height.</summary>
	[ContentSerializer(Optional = true)]
	public Point TileSize = new Point(3, 3);

	/// <summary>The health points that must be depleted to break the giant crop. The number of points depleted per axe chop depends on the axe power level.</summary>
	[ContentSerializer(Optional = true)]
	public int Health = 3;

	/// <summary>The percentage chance a given grid of crops will grow into the giant crop each night, as a value between 0 (never) and 1 (always).</summary>
	[ContentSerializer(Optional = true)]
	public float Chance = 0.01f;

	/// <summary>A game state query which indicates whether the giant crop can be selected. Defaults to always enabled.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
