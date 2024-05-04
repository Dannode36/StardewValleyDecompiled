using System;

namespace StardewValley;

[Flags]
public enum CollisionMask : byte
{
	None = 0,
	Buildings = 1,
	Characters = 2,
	Farmers = 4,
	Flooring = 8,
	Furniture = 0x10,
	Objects = 0x20,
	TerrainFeatures = 0x40,
	LocationSpecific = 0x80,
	All = byte.MaxValue
}
