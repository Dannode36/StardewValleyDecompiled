using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.FloorsAndPaths;

/// <summary>The metadata for a craftable floor or path item.</summary>
public class FloorPathData
{
	/// <summary>A key which uniquely identifies this floor/path. The ID should only contain alphanumeric/underscore/dot characters. For vanilla floors and paths, this matches the spritesheet index in the <c>TerrainFeatures/Flooring</c> spritesheet; for custom floors and paths, this should be prefixed with your mod ID like <c>Example.ModId_FloorName.</c></summary>
	public string Id;

	/// <summary>The unqualified item ID for the corresponding object-type item.</summary>
	public string ItemId;

	/// <summary>The asset name for the texture when the item is placed.</summary>
	public string Texture;

	/// <summary>The top-left pixel position for the sprite within the <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.Texture" /> spritesheet.</summary>
	public Point Corner;

	/// <summary>Equivalent to <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.Texture" />, but applied if the current location is in winter. Defaults to <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.Texture" />.</summary>
	public string WinterTexture;

	/// <summary>Equivalent to <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.Corner" />, but used if <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.WinterTexture" /> is applied. Defaults to <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.Corner" />.</summary>
	public Point WinterCorner;

	/// <summary>The audio cue ID played when the item is placed (e.g. <c>axchop</c> used by Wood Floor).</summary>
	public string PlacementSound;

	/// <summary>The audio cue ID played when the item is picked up. Defaults to <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.PlacementSound" />.</summary>
	[ContentSerializer(Optional = true)]
	public string RemovalSound;

	/// <summary>The type of cosmetic debris particles to 'splash' from the tile when the item is picked up. The defined values are <c>0</c> (copper), <c>2</c> (iron), <c>4</c> (coal), <c>6</c> (gold), <c>8</c> (coins), <c>10</c> (iridium), <c>12</c> (wood), <c>14</c> (stone), <c>32</c> (big stone), and <c>34</c> (big wood). Default <c>14</c> (stone).</summary>
	[ContentSerializer(Optional = true)]
	public int RemovalDebrisType = 14;

	/// <summary>The audio cue ID played when the player steps on the tile (e.g. <c>woodyStep</c> used by Wood Floor).</summary>
	public string FootstepSound;

	/// <summary>When drawing adjacent flooring items across multiple tiles, how the flooring sprite for each tile is selected.</summary>
	[ContentSerializer(Optional = true)]
	public FloorPathConnectType ConnectType;

	/// <summary>The type of shadow to draw under the tile sprite.</summary>
	[ContentSerializer(Optional = true)]
	public FloorPathShadowType ShadowType;

	/// <summary>The pixel size of the decorative inner corner when the <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathData.ConnectType" /> field is set to <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathConnectType.CornerDecorated" /> or <see cref="F:StardewValley.GameData.FloorsAndPaths.FloorPathConnectType.Default" />.</summary>
	[ContentSerializer(Optional = true)]
	public int CornerSize = 4;

	/// <summary>The speed boost applied to the player, on the farm only, when they're walking on paths of this type. Negative values are ignored. Set to <c>-1</c> to use the default for vanilla paths.</summary>
	[ContentSerializer(Optional = true)]
	public float FarmSpeedBuff = -1f;
}
