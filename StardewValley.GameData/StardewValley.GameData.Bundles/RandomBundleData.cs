using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Bundles;

public class RandomBundleData
{
	/// <summary>A unique ID for this entry.</summary>
	[ContentSerializerIgnore]
	public string Id => AreaName;

	public string AreaName { get; set; }

	public string Keys { get; set; }

	[ContentSerializer(Optional = true)]
	public List<BundleSetData> BundleSets { get; set; } = new List<BundleSetData>();


	[ContentSerializer(Optional = true)]
	public List<BundleData> Bundles { get; set; } = new List<BundleData>();

}
