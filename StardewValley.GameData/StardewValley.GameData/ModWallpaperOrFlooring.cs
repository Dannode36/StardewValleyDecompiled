namespace StardewValley.GameData;

/// <summary>The metadata for a custom floor or wallpaper item.</summary>
public class ModWallpaperOrFlooring
{
	/// <summary>A key which uniquely identifies this wallpaper or flooring. This should only contain alphanumeric/underscore/dot characters. This should be prefixed with your mod ID like <c>Example.ModId_WallpaperName</c>.</summary>
	public string Id;

	/// <summary>The asset name which contains 32x32 pixel (flooring) or 16x48 pixel (wallpaper) sprites. The tilesheet must be 256 pixels wide, but can have any number of flooring/wallpaper rows.</summary>
	public string Texture;

	/// <summary>Whether this is a flooring tilesheet; else it's a wallpaper tilesheet.</summary>
	public bool IsFlooring;

	/// <summary>The number of flooring or wallpaper sprites in the tilesheet.</summary>
	public int Count;
}
