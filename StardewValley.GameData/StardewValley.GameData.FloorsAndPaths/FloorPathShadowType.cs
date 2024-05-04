namespace StardewValley.GameData.FloorsAndPaths;

/// <summary>How the shadow under a floor or path tile sprite should be drawn.</summary>
public enum FloorPathShadowType
{
	/// <summary>Don't draw a shadow.</summary>
	None,
	/// <summary>Draw a shadow under the entire tile.</summary>
	Square,
	/// <summary>Draw a shadow that follows the lines of the path sprite.</summary>
	Contoured
}
