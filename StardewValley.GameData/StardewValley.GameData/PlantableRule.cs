using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>As part of assets like <see cref="T:StardewValley.GameData.Crops.CropData" /> or <see cref="T:StardewValley.GameData.WildTrees.WildTreeData" />, indicates when a seed or sapling can be planted in a location.</summary>
public class PlantableRule
{
	/// <summary>A key which uniquely identifies this entry within the list. The ID should only contain alphanumeric/underscore/dot characters. For custom entries for vanilla items, this should be prefixed with your mod ID like <c>Example.ModId_Id</c>.</summary>
	public string Id;

	/// <summary>A game state query which indicates whether this entry applies. Default true.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>When this rule should be applied.</summary>
	/// <remarks>Note that this doesn't allow bypassing built-in restrictions (e.g. trees can't be planted in garden pots regardless of the plantable location rules).</remarks>
	[ContentSerializer(Optional = true)]
	public PlantableRuleContext PlantedIn = PlantableRuleContext.Any;

	/// <summary>Indicates when the seed or sapling can be planted in a location if this entry is selected.</summary>
	public PlantableResult Result;

	/// <summary>If this rule prevents planting the seed or sapling, the tokenizable string to show to the player (or <c>null</c> to show a generic message).</summary>
	[ContentSerializer(Optional = true)]
	public string DeniedMessage;

	/// <summary>Get whether this rule should be applied.</summary>
	/// <param name="isGardenPot">Whether the seed or sapling is being planted in a garden pot (else the ground).</param>
	public bool ShouldApplyWhen(bool isGardenPot)
	{
		return PlantedIn.HasFlag((!isGardenPot) ? PlantableRuleContext.Ground : PlantableRuleContext.GardenPot);
	}
}
