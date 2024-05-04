using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Crops;

/// <summary>The metadata for a crop that can be planted.</summary>
public class CropData
{
	/// <summary>The seasons in which this crop can grow.</summary>
	public List<Season> Seasons = new List<Season>();

	/// <summary>The number of days in each visual step of growth before the crop is harvestable.</summary>
	public List<int> DaysInPhase = new List<int>();

	/// <summary>The number of days before the crop regrows after harvesting, or -1 if it can't regrow.</summary>
	[ContentSerializer(Optional = true)]
	public int RegrowDays = -1;

	/// <summary>Whether this is a raised crop on a trellis that can't be walked through.</summary>
	[ContentSerializer(Optional = true)]
	public bool IsRaised;

	/// <summary>Whether this crop can be planted near water for a unique paddy dirt texture, faster growth time, and auto-watering.</summary>
	[ContentSerializer(Optional = true)]
	public bool IsPaddyCrop;

	/// <summary>Whether this crop needs to be watered to grow.</summary>
	[ContentSerializer(Optional = true)]
	public bool NeedsWatering = true;

	/// <summary>The rules which override which locations the crop can be planted in, if applicable. These don't override more specific checks (e.g. crops needing to be planted in dirt).</summary>
	[ContentSerializer(Optional = true)]
	public List<PlantableRule> PlantableLocationRules;

	/// <summary>The unqualified item ID produced when this crop is harvested.</summary>
	public string HarvestItemId;

	/// <summary>The minimum number of <see cref="F:StardewValley.GameData.Crops.CropData.HarvestItemId" /> to harvest.</summary>
	[ContentSerializer(Optional = true)]
	public int HarvestMinStack = 1;

	/// <summary>The maximum number of <see cref="F:StardewValley.GameData.Crops.CropData.HarvestItemId" /> to harvest, before <see cref="F:StardewValley.GameData.Crops.CropData.ExtraHarvestChance" /> and <see cref="F:StardewValley.GameData.Crops.CropData.HarvestMaxIncreasePerFarmingLevel" /> are applied.</summary>
	[ContentSerializer(Optional = true)]
	public int HarvestMaxStack = 1;

	/// <summary>The number of extra harvests to produce per farming level. This is rounded down to the nearest integer and added to <see cref="F:StardewValley.GameData.Crops.CropData.HarvestMaxStack" />.</summary>
	[ContentSerializer(Optional = true)]
	public float HarvestMaxIncreasePerFarmingLevel;

	/// <summary>The probability that harvesting the crop will produce extra harvest items, as a value between 0 (never) and 0.9 (nearly always). This is repeatedly rolled until it fails, then the number of successful rolls is added to the produced count.</summary>
	[ContentSerializer(Optional = true)]
	public double ExtraHarvestChance;

	/// <summary>How the crop can be harvested.</summary>
	[ContentSerializer(Optional = true)]
	public HarvestMethod HarvestMethod;

	/// <summary>If set, the minimum quality of the harvest crop.</summary>
	/// <remarks>These fields set a constraint that's applied after the quality is calculated normally, they don't affect the initial quality logic.</remarks>
	[ContentSerializer(Optional = true)]
	public int HarvestMinQuality;

	/// <summary>If set, the maximum quality of the harvest crop.</summary>
	/// <inheritdoc cref="F:StardewValley.GameData.Crops.CropData.HarvestMinQuality" path="/remarks" />
	[ContentSerializer(Optional = true)]
	public int? HarvestMaxQuality;

	/// <summary>The tint colors that can be applied to the crop sprite, if any. If multiple colors are listed, one is chosen at random for each crop. These can be MonoGame color field names (like <c>ForestGreen</c>), RGB hex codes (like <c>#AABBCC</c>), or RGBA hex codes (like <c>#AABBCCDD</c>).</summary>
	[ContentSerializer(Optional = true)]
	public List<string> TintColors = new List<string>();

	/// <summary>The asset name for the crop texture under the game's <c>Content</c> folder.</summary>
	public string Texture;

	/// <summary>The index of this crop in the <see cref="F:StardewValley.GameData.Crops.CropData.Texture" /> (one crop per row).</summary>
	public int SpriteIndex;

	/// <summary>Whether the player can ship 300 of this crop's harvest item to unlock the monoculture achievement.</summary>
	public bool CountForMonoculture;

	/// <summary>Whether the player must ship 15 of this crop's harvest item to unlock the polyculture achievement.</summary>
	public bool CountForPolyculture;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;

	/// <summary>Get the <see cref="F:StardewValley.GameData.Crops.CropData.Texture" /> if different from the default name.</summary>
	/// <param name="defaultName">The default asset name.</param>
	public string GetCustomTextureName(string defaultName)
	{
		if (string.IsNullOrWhiteSpace(Texture) || !(Texture != defaultName))
		{
			return null;
		}
		return Texture;
	}
}
