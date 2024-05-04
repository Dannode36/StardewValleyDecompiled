using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>An incoming phone call that the player can receive when they have a telephone.</summary>
public class IncomingPhoneCallData
{
	/// <summary>If set, a game state query which indicates whether to trigger this phone call.</summary>
	/// <remarks>Whether a player receives this call depends on two fields: <see cref="F:StardewValley.GameData.IncomingPhoneCallData.TriggerCondition" /> is checked on the host player before sending the call to all players, then <see cref="F:StardewValley.GameData.IncomingPhoneCallData.RingCondition" /> is checked on each player to determine whether the phone rings for them.</remarks>
	[ContentSerializer(Optional = true)]
	public string TriggerCondition;

	/// <summary>If set, a game state query which indicates whether the phone will ring when this call is received.</summary>
	/// <inheritdoc cref="F:StardewValley.GameData.IncomingPhoneCallData.TriggerCondition" path="/remarks" />
	[ContentSerializer(Optional = true)]
	public string RingCondition;

	/// <summary>The internal name of the NPC making the call. If specified, that NPC's name and portrait will be shown.</summary>
	/// <remarks>To show a portrait and NPC name, you must specify either <see cref="F:StardewValley.GameData.IncomingPhoneCallData.FromNpc" /> or <see cref="F:StardewValley.GameData.IncomingPhoneCallData.FromPortrait" />; otherwise a simple dialogue with no portrait/name will be shown.</remarks>
	[ContentSerializer(Optional = true)]
	public string FromNpc;

	/// <summary>If set, overrides the portrait shown based on <see cref="F:StardewValley.GameData.IncomingPhoneCallData.FromNpc" />.</summary>
	[ContentSerializer(Optional = true)]
	public string FromPortrait;

	/// <summary>If set, overrides the NPC display name shown based on <see cref="F:StardewValley.GameData.IncomingPhoneCallData.FromNpc" />.</summary>
	[ContentSerializer(Optional = true)]
	public string FromDisplayName;

	/// <summary>A tokenizable string for the call text.</summary>
	public string Dialogue;

	/// <summary>Whether to ignore the base chance of receiving a call for this call.</summary>
	[ContentSerializer(Optional = true)]
	public bool IgnoreBaseChance;

	/// <summary>If set, marks this as a simple dialogue box without an NPC name and portrait, with lines split into multiple boxes by this substring. For example, using <c>#</c> will split <c>Box A#Box B#Box C</c> into three consecutive dialogue boxes.</summary>
	/// <remarks>You should leave this null in most cases, and use the regular dialogue format in <see cref="F:StardewValley.GameData.IncomingPhoneCallData.Dialogue" /> to split lines if needed. This is mainly intended to support some older vanilla phone calls.</remarks>
	[ContentSerializer(Optional = true)]
	public string SimpleDialogueSplitBy;

	/// <summary>The maximum number of times this phone call can be received, or -1 for no limit.</summary>
	[ContentSerializer(Optional = true)]
	public int MaxCalls = 1;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
