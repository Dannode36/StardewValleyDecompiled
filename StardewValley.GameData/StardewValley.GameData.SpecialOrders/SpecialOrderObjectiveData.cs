using System.Collections.Generic;

namespace StardewValley.GameData.SpecialOrders;

/// <summary>As part of <see cref="T:StardewValley.GameData.SpecialOrders.SpecialOrderData" />, an objective that must be achieved to complete the special order.</summary>
public class SpecialOrderObjectiveData
{
	/// <summary>The name of the C# class which handles the logic for this objective.</summary>
	/// <remarks>The class must be in the <c>StardewValley</c> namespace, and its name must end with <c>Objective</c> (without including it in this field). For example, <c>"Gift"</c> will match the <c>StardewValley.GiftObjective</c> type.</remarks>
	public string Type;

	/// <summary>The translated description text for the objective.</summary>
	/// <remarks>Square brackets indicate a translation key from <c>Strings\SpecialOrderStrings</c>, like <c>[QiChallenge_Objective_0_Text]</c>. This can contain <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements" /> tokens.</remarks>
	public string Text;

	/// <summary>The number related to the objective.</summary>
	/// <remarks>This can contain <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements" /> tokens.</remarks>
	public string RequiredCount;

	/// <summary>The arbitrary data values understood by the C# class identified by <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderObjectiveData.Type" />. These may or may not allow <see cref="F:StardewValley.GameData.SpecialOrders.SpecialOrderData.RandomizedElements" /> tokens, depending on the class.</summary>
	public Dictionary<string, string> Data;
}
