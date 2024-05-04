using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Pets;

public class PetGift
{
	/// <summary>A key which uniquely identifies the gift entry for this pet.</summary>
	public string Id;

	/// <summary>The friendship level that this pet must be at before it can give this gift. Defaults to 1000 (max friendship)</summary>
	[ContentSerializer(Optional = true)]
	public int MinimumFriendshipThreshold = 1000;

	/// <summary>The item's weight when randomly choosing a item, relative to other items in the list (e.g. 2 is twice as likely as 1).</summary>
	[ContentSerializer(Optional = true)]
	public float Weight = 1f;

	/// <summary>The qualified item ID of the gift.</summary>
	public string QualifiedItemID;

	/// <summary>How many of this item to drop.</summary>
	[ContentSerializer(Optional = true)]
	public int Stack = 1;
}
