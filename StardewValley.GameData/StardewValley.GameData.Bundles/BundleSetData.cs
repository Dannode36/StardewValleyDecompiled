using System.Collections.Generic;

namespace StardewValley.GameData.Bundles;

public class BundleSetData
{
	/// <summary>A unique ID for this entry.</summary>
	public string Id;

	public List<BundleData> Bundles = new List<BundleData>();
}
