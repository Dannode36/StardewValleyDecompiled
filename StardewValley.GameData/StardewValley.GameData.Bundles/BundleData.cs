using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Bundles;

public class BundleData
{
	/// <summary>A unique ID for this entry.</summary>
	[ContentSerializerIgnore]
	public string Id => Name;

	public string Name { get; set; }

	public int Index { get; set; }

	public string Sprite { get; set; }

	public string Color { get; set; }

	public string Items { get; set; }

	[ContentSerializer(Optional = true)]
	public int Pick { get; set; } = -1;


	[ContentSerializer(Optional = true)]
	public int RequiredItems { get; set; } = -1;


	public string Reward { get; set; }
}
