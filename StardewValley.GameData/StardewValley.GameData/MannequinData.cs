using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

public class MannequinData
{
	public string ID;

	public string DisplayName;

	public string Description;

	public string Texture;

	public string FarmerTexture;

	public int SheetIndex;

	public bool DisplaysClothingAsMale = true;

	[ContentSerializer(Optional = true)]
	public bool Cursed;

	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields = new Dictionary<string, string>();
}
