using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

public class TrinketData
{
	public string ID;

	public string DisplayName;

	public string Description;

	public string Texture;

	public int SheetIndex;

	public string TrinketEffectClass;

	[ContentSerializer(Optional = true)]
	public bool DropsNaturally = true;

	[ContentSerializer(Optional = true)]
	public bool CanBeReforged = true;

	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> TrinketMetadata;
}
