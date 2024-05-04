using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.SpecialOrders;

public class SpecialOrderData
{
	/// <summary>The translated display name for the special order.</summary>
	/// <remarks>Square brackets indicate a translation key from <c>Strings\SpecialOrderStrings</c>, like <c>[QiChallenge_Name]</c>.</remarks>
	public string Name;

	/// <summary>The internal name of the NPC requesting the special order.</summary>
	public string Requester;

	/// <summary>How long the player has to complete the special order.</summary>
	public QuestDuration Duration;

	/// <summary>Whether the special order can be chosen again if the player has previously completed it.</summary>
	[ContentSerializer(Optional = true)]
	public bool Repeatable;

	/// <summary>A set of hardcoded tags that check conditions like the season, received mail, etc. Most code should use <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderData.Condition" /> instead.</summary>
	[ContentSerializer(Optional = true)]
	public string RequiredTags = "";

	/// <summary>A game state query which indicates whether this special order can be given.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition = "";

	/// <summary>The order type (one of <c>Qi</c> or an empty string).</summary>
	/// <remarks>Setting this to <c>Qi</c> enables some custom game logic for Qi's challenges.</remarks>
	[ContentSerializer(Optional = true)]
	public string OrderType = "";

	/// <summary>An arbitrary rule ID that can be checked by game or mod logic to enable special behavior while this order is active.</summary>
	[ContentSerializer(Optional = true)]
	public string SpecialRule = "";

	/// <summary>The translated description text for the special order.</summary>
	/// <remarks>Square brackets indicate a translation key from <c>Strings\SpecialOrderStrings</c>, like <c>[QiChallenge_Text]</c>. This can contain <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements" /> tokens.</remarks>
	public string Text;

	/// <summary>If set, an unqualified item ID to remove everywhere in the world when this special order ends.</summary>
	[ContentSerializer(Optional = true)]
	public string ItemToRemoveOnEnd;

	/// <summary>If set, a mail ID to remove from all players when this special order ends.</summary>
	[ContentSerializer(Optional = true)]
	public string MailToRemoveOnEnd;

	/// <summary>The randomized tokens which can be referenced by other special order fields.</summary>
	/// <remarks>
	///   <para>These can be used in some special order fields (noted in their code docs) in the form <c>{Name}</c> (like <c>{FishType}</c>), which returns the element's value.</para>
	///
	///   <para>If a randomized element selects an item, you can use the <c>{Name:ValueType}</c> form (like <c>{FishType:Text}</c>) to get a value related to the selected item:</para>
	///   <list type="bullet">
	///     <item><description><c>Text</c>: the item's translated display name.</description></item>
	///     <item><description><c>TextPlural</c>: equivalent to <c>Text</c> but pluralized if possible.</description></item>
	///     <item><description><c>TextPluralCapitalized</c>: equivalent to <c>Text</c> but pluralized if possible and its first letter capitalized.</description></item>
	///     <item><description><c>Tags</c>: a context tag which identifies the item, like <c>id_o_128</c> for a pufferfish.</description></item>
	///     <item><description><c>Price</c>: for objects only, the gold price for selling this item to a store (all other item types will have the value <c>1</c>).</description></item>
	///   </list>
	/// </remarks>
	[ContentSerializer(Optional = true)]
	public List<RandomizedElement> RandomizedElements;

	/// <summary>The objectives which must be achieved to complete this special order.</summary>
	public List<SpecialOrderObjectiveData> Objectives;

	/// <summary>The rewards given to the player when they complete this special order.</summary>
	public List<SpecialOrderRewardData> Rewards;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
