using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>As part of an another entry like <see cref="T:StardewValley.GameData.Machines.MachineData" /> or <see cref="T:StardewValley.GameData.Shops.ShopData" />, a change to apply to a numeric quantity.</summary>
public class QuantityModifier
{
	/// <summary>The type of change to apply for a <see cref="T:StardewValley.GameData.QuantityModifier" />.</summary>
	public enum ModificationType
	{
		/// <summary>Add a number to the current value.</summary>
		Add,
		/// <summary>Subtract a number from the current value.</summary>
		Subtract,
		/// <summary>Multiply the current value by a number.</summary>
		Multiply,
		/// <summary>Divide the current value by a number.</summary>
		Divide,
		/// <summary>Overwrite the current value with a number.</summary>
		Set
	}

	/// <summary>Indicates how multiple quantity modifiers are combined.</summary>
	public enum QuantityModifierMode
	{
		/// <summary>Apply each modifier to the result of the previous one. For example, two modifiers which double a value will quadruple it.</summary>
		Stack,
		/// <summary>Apply the modifier which results in the lowest value.</summary>
		Minimum,
		/// <summary>Apply the modifier which results in the highest value.</summary>
		Maximum
	}

	/// <summary>An ID for this modifier. This only needs to be unique within the current modifier list. For a custom entry, you should use a globally unique ID which includes your mod ID like <c>ExampleMod.Id_ModifierName</c>.</summary>
	public string Id;

	/// <summary>A game state query which indicates whether this change should be applied. Item-only tokens are valid for this check, and will check the input (not output) item. Defaults to always true.</summary>
	[ContentSerializer(Optional = true)]
	public string Condition;

	/// <summary>The type of change to apply.</summary>
	public ModificationType Modification;

	/// <summary>The operand to apply to the target value (e.g. the multiplier if <see cref="F:StardewValley.GameData.QuantityModifier.Modification" /> is set to <see cref="F:StardewValley.GameData.QuantityModifier.ModificationType.Multiply" />).</summary>
	[ContentSerializer(Optional = true)]
	public float Amount;

	/// <summary>A list of random amounts to choose from, using the same format as <see cref="F:StardewValley.GameData.QuantityModifier.Amount" />. If set, <see cref="F:StardewValley.GameData.QuantityModifier.Amount" /> is ignored.</summary>
	[ContentSerializer(Optional = true)]
	public List<float> RandomAmount;

	/// <summary>Apply the change to a target value.</summary>
	/// <param name="value">The current target value.</param>
	/// <param name="modification">The type of change to apply.</param>
	/// <param name="amount">The operand to apply to the target value (e.g. the multiplier if <paramref name="modification" /> is set to <see cref="F:StardewValley.GameData.QuantityModifier.ModificationType.Multiply" />).</param>
	public static float Apply(float value, ModificationType modification, float amount)
	{
		return modification switch
		{
			ModificationType.Add => value + amount, 
			ModificationType.Subtract => value - amount, 
			ModificationType.Multiply => value * amount, 
			ModificationType.Divide => value / amount, 
			ModificationType.Set => amount, 
			_ => value, 
		};
	}
}
