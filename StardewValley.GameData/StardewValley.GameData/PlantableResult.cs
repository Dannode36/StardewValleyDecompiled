namespace StardewValley.GameData;

/// <summary>Indicates when a seed/sapling can be planted in a location.</summary>
public enum PlantableResult
{
	/// <summary>The seed/sapling can be planted if the location normally allows it.</summary>
	Default,
	/// <summary>The seed/sapling can be planted here, regardless of whether the location normally allows it.</summary>
	Allow,
	/// <summary>The seed/sapling can't be planted here, regardless of whether the location normally allows it.</summary>
	Deny
}
