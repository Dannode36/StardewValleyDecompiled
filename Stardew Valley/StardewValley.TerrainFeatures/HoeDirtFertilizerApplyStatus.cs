namespace StardewValley.TerrainFeatures;

/// <summary>Indicates whether fertilizer can be applied to a given <see cref="T:StardewValley.TerrainFeatures.HoeDirt" /> instance.</summary>
public enum HoeDirtFertilizerApplyStatus
{
	/// <summary>The fertilizer can be applied.</summary>
	Okay,
	/// <summary>The fertilizer can't be applied because the dirt already has the same fertilizer.</summary>
	HasThisFertilizer,
	/// <summary>The fertilizer can't be applied because the dirt already has a different fertilizer.</summary>
	HasAnotherFertilizer,
	/// <summary>The fertilizer can't be applied because the crop has already sprouted, and this fertilizer must be placed before that point.</summary>
	CropAlreadySprouted
}
