using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Museum;

/// <summary>The data for a set of artifacts that can be donated to the museum, and the resulting reward.</summary>
public class MuseumRewards
{
	/// <summary>
	///   <para>The items that must be donated to complete this reward group. The player must fulfill every entry in the list to unlock the reward. For example, an entry with the tag <c>forage_item</c> and count 2 will require donating any two forage items.</para>
	///
	///   <para>Special case: an entry with the exact values <c>Tag: "", Count: -1</c> passes if the museum is complete (i.e. the player has donated the max number of items). </para>
	/// </summary>
	public List<MuseumDonationRequirement> TargetContextTags;

	/// <summary>The qualified item ID for the item given to the player when they donate all required items for this group. There's no reward item if omitted.</summary>
	[ContentSerializer(Optional = true)]
	public string RewardItemId;

	/// <summary>The stack size for the <see cref="F:StardewValley.GameData.Museum.MuseumRewards.RewardItemId" /> item (if the item supports stacking).</summary>
	[ContentSerializer(Optional = true)]
	public int RewardItemCount = 1;

	/// <summary>Whether to mark the <see cref="F:StardewValley.GameData.Museum.MuseumRewards.RewardItemId" /> item as a special permanent item, which can't be destroyed/dropped and can only be collected once.</summary>
	[ContentSerializer(Optional = true)]
	public bool RewardItemIsSpecial;

	/// <summary>Whether to give the player a cooking/crafting recipe which produces the <see cref="F:StardewValley.GameData.Museum.MuseumRewards.RewardItemId" /> item, instead of the item itself. Ignored if the item type can't be cooked/crafted (i.e. non-object-type items).</summary>
	[ContentSerializer(Optional = true)]
	public bool RewardItemIsRecipe;

	/// <summary>The actions to perform when the reward is collected. For example, this is used for the rusty key unlock at 60 donations.</summary>
	[ContentSerializer(Optional = true)]
	public List<string> RewardActions;

	/// <summary>Whether to add the ID value to the player's received mail. This is used to track whether the player has collected the reward, and should almost always be true. If this and <see cref="F:StardewValley.GameData.Museum.MuseumRewards.RewardItemIsSpecial" /> are both false, the player will be able to collect the reward infinite times.</summary>
	[ContentSerializer(Optional = true)]
	public bool FlagOnCompletion;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
