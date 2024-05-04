using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.LocationContexts;

/// <summary>As part of <see cref="T:StardewValley.GameData.LocationContexts.LocationContextData" />, a letter added to the player's mailbox when they pass out (due to exhaustion or at 2am).</summary>
public class PassOutMailData
{
	/// <summary>A unique string ID for this entry within the current location context.</summary>
	public string Id;

	/// <summary>A game state query which indicates whether this entry is active. Defaults to always true.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>The letter ID to add.</summary>
	/// <remarks>
	///   <para>The game will look for an existing letter ID in the <c>Data/mail</c> asset in this order (where <c>{billed}</c> is <c>Billed</c> if they lost gold or <c>NotBilled</c> otherwise, and <c>{gender}</c> is <c>Female</c> or <c>Male</c>): <c>{letter id}_{billed}_{gender}</c>, <c>{letter id}_{billed}</c>, <c>{letter id}</c>. If no match is found, the game will send <c>passedOut2</c> instead.</para>
	///
	///   <para>If the mail ID starts with <c>passedOut</c>, <c>{0}</c> in the letter text will be replaced with the gold amount lost, and the letter won't appear on the collections tab.</para>
	/// </remarks>
	public string Mail;

	/// <summary>The maximum amount of gold lost. This is applied after the context's <see cref="F:StardewValley.GameData.LocationContexts.LocationContextData.MaxPassOutCost" /> (i.e. the context's value is used to calculate the random amount, then this field caps the result). Defaults to unlimited.</summary>
	[ContentSerializer(Optional = true)]
	public int MaxPassOutCost = -1;

	/// <summary>When multiple mail entries match, whether to send this one instead of choosing one randomly.</summary>
	[ContentSerializer(Optional = true)]
	public bool SkipRandomSelection;
}
