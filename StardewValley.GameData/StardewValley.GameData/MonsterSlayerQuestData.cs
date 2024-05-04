using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>The data for an Adventurer's Guild monster eradication goal.</summary>
public class MonsterSlayerQuestData
{
	/// <summary>A tokenizable string for the goal's display name, shown on the board in the Adventurer's Guild.</summary>
	public string DisplayName;

	/// <summary>A list of monster IDs that are counted towards the <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.Count" />.</summary>
	public List<string> Targets;

	/// <summary>The total number of monsters matching <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.Targets" /> which must be defeated to complete this goal.</summary>
	public int Count;

	/// <summary>The qualified item ID for the item that can be collected from Gil when this goal is completed. There's no reward item if omitted.</summary>
	[ContentSerializer(Optional = true)]
	public string RewardItemId;

	/// <summary>The price of the <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardItemId" /> in Marlon's shop, or <c>-1</c> to disable buying it from Marlon.</summary>
	[ContentSerializer(Optional = true)]
	public int RewardItemPrice = -1;

	/// <summary>A tokenizable string for custom dialogue from Gil shown before collecting the rewards, if any.</summary>
	///  <remarks>If <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardDialogueFlag" /> isn't set, then this dialogue will be shown each time the reward menus is opened until the player collects the <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardItemId" /> (if any).</remarks>
	[ContentSerializer(Optional = true)]
	public string RewardDialogue;

	/// <summary>A mail flag ID which indicates whether the player has seen the <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardDialogue" />.</summary>
	/// <remarks>This doesn't send a letter; see <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardMail" /> or <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardMailAll" /> for that.</remarks>
	[ContentSerializer(Optional = true)]
	public string RewardDialogueFlag;

	/// <summary>The mail flag ID to set for the current player when this goal is completed, if any.</summary>
	/// <remarks>This doesn't send a letter; see <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardMail" /> or <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardMailAll" /> for that.</remarks>
	[ContentSerializer(Optional = true)]
	public string RewardFlag;

	/// <summary>The mail flag ID to set for all players when this goal is completed, if any.</summary>
	/// <remarks>This doesn't send a letter; see <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardMail" /> or <see cref="F:StardewValley.GameData.MonsterSlayerQuestData.RewardMailAll" /> for that.</remarks>
	[ContentSerializer(Optional = true)]
	public string RewardFlagAll;

	/// <summary>The mail letter ID to add to the current player's mailbox tomorrow, if set.</summary>
	[ContentSerializer(Optional = true)]
	public string RewardMail;

	/// <summary>The mail letter ID to add to all players' mailboxes tomorrow, if set.</summary>
	[ContentSerializer(Optional = true)]
	public string RewardMailAll;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
