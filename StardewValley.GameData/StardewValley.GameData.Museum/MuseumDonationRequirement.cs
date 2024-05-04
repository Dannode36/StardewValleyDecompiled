namespace StardewValley.GameData.Museum;

/// <summary>As part of <see cref="T:StardewValley.GameData.Museum.MuseumRewards" />, an item that must be donated to complete this reward group.</summary>
public class MuseumDonationRequirement
{
	/// <summary>The context tag for the items to require.</summary>
	public string Tag;

	/// <summary>The minimum number of items matching the <see cref="F:StardewValley.GameData.Museum.MuseumDonationRequirement.Tag" /> that must be donated.</summary>
	public int Count;
}
