namespace StardewValley.GameData.FloorsAndPaths;

/// <summary>When drawing adjacent flooring items across multiple tiles, how the flooring sprite for each tile is selected.</summary>
public enum FloorPathConnectType
{
	/// <summary>For normal floors, intended to cover large square areas. This uses some logic to draw inner corners.</summary>
	Default,
	/// <summary>For floors intended to be drawn as narrow paths. These are drawn without any consideration for inner corners.</summary>
	Path,
	/// <summary>For floors that have a decorative corner. Use <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.CornerSize" /> to change the size of this corner.</summary>
	CornerDecorated,
	/// <summary>For floors that don't connect. When placed, one of the tiles is randomly selected.</summary>
	Random
}
