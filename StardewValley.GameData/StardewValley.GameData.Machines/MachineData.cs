using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Machines;

/// <summary>The behavior and metadata for a machine which takes input, produces output, or both.</summary>
public class MachineData
{
	/// <summary>Whether to force adding the <c>machine_input</c> context tag, which indicates the machine can accept input.</summary>
	/// <remarks>If false, this will be set automatically if any <see cref="F:StardewValley.GameData.Machines.MachineData.OutputRules" /> use the <see cref="F:StardewValley.GameData.Machines.MachineOutputTrigger.ItemPlacedInMachine" /> trigger.</remarks>
	[ContentSerializer(Optional = true)]
	public bool HasInput;

	/// <summary>Whether to force adding the <c>machine_output</c> context tag, which indicates the machine can produce output.</summary>
	/// <remarks>If false, this will be set automatically if there are <see cref="F:StardewValley.GameData.Machines.MachineData.OutputRules" />.</remarks>
	[ContentSerializer(Optional = true)]
	public bool HasOutput;

	/// <summary>A C# method invoked when the player interacts with the machine while it doesn't have output ready to harvest.</summary>
	/// <remarks><strong>This is an advanced field. Most machines shouldn't use this.</strong> This must be specified in the form <c>{full type name}: {method name}</c> (like <c>StardewValley.Object, Stardew Valley: SomeInteractMethod</c>). The method must be static, take three arguments (<c>Object machine, GameLocation location, Farmer player</c>), and return a boolean indicating whether the interaction succeeded.</remarks>
	[ContentSerializer(Optional = true)]
	public string InteractMethod;

	/// <summary>The rules which define how to process input items and produce output.</summary>
	[ContentSerializer(Optional = true)]
	public List<MachineOutputRule> OutputRules;

	/// <summary>A list of extra items required before <see cref="F:StardewValley.GameData.Machines.MachineData.OutputRules" /> will be checked. If specified, every listed item must be present in the player, hopper, or chest inventory (depending how the machine is being loaded).</summary>
	[ContentSerializer(Optional = true)]
	public List<MachineItemAdditionalConsumedItems> AdditionalConsumedItems;

	/// <summary>A list of cases when the machine should be paused, so the timer on any item being produced doesn't decrement.</summary>
	[ContentSerializer(Optional = true)]
	public List<MachineTimeBlockers> PreventTimePass;

	/// <summary>Changes to apply to the processing time before output is ready.</summary>
	/// <remarks>If multiple entries match, they'll be applied sequentially (e.g. two matching rules to double processing time will quadruple it).</remarks>
	[ContentSerializer(Optional = true)]
	public List<QuantityModifier> ReadyTimeModifiers;

	/// <summary>How multiple <see cref="F:StardewValley.GameData.Machines.MachineData.ReadyTimeModifiers" /> should be combined.</summary>
	[ContentSerializer(Optional = true)]
	public QuantityModifier.QuantityModifierMode ReadyTimeModifierMode;

	/// <summary>A tokenizable string for the message shown in a toaster notification if the player tries to input an item that isn't accepted by the machine.</summary>
	[ContentSerializer(Optional = true)]
	public string InvalidItemMessage;

	/// <summary>An extra condition that must be met before <see cref="F:StardewValley.GameData.Machines.MachineData.InvalidItemMessage" /> is shown.</summary>
	[ContentSerializer(Optional = true)]
	public string InvalidItemMessageCondition;

	/// <summary>A tokenizable string for the message shown in a toaster notification if the input inventory doesn't contain this item, unless overridden by <see cref="F:StardewValley.GameData.Machines.MachineOutputRule.InvalidCountMessage" /> under <see cref="F:StardewValley.GameData.Machines.MachineData.OutputRules" />.</summary>
	/// <remarks>
	///   This can use extra tokens:
	///   <list type="bullet">
	///     <item><description><c>[ItemCount]</c>: the number of remaining items needed. For example, if you're holding three and need five, <c>[ItemCount]</c> will be replaced with 2.</description></item>
	///   </list>
	/// </remarks>
	[ContentSerializer(Optional = true)]
	public string InvalidCountMessage;

	/// <summary>The cosmetic effects to show when an item is loaded into the machine.</summary>
	[ContentSerializer(Optional = true)]
	public List<MachineEffects> LoadEffects;

	/// <summary>The cosmetic effects to show while the machine is processing an input, based on the <see cref="F:StardewValley.GameData.Machines.MachineData.WorkingEffectChance" />.</summary>
	[ContentSerializer(Optional = true)]
	public List<MachineEffects> WorkingEffects;

	/// <summary>The percentage chance to apply <see cref="F:StardewValley.GameData.Machines.MachineData.WorkingEffects" /> each time the day starts or the in-game clock changes, as a value between 0 (never) and 1 (always).</summary>
	[ContentSerializer(Optional = true)]
	public float WorkingEffectChance = 0.33f;

	/// <summary>Whether the player can drop a new item into the machine before it's done processing the last one (like the crystalarium). The previous item will be lost.</summary>
	[ContentSerializer(Optional = true)]
	public bool AllowLoadWhenFull;

	/// <summary>Whether the machine sprite should bulge in &amp; out while it's processing an item.</summary>
	[ContentSerializer(Optional = true)]
	public bool WobbleWhileWorking = true;

	/// <summary>A light emitted while the machine is processing an item.</summary>
	[ContentSerializer(Optional = true)]
	public MachineLight LightWhileWorking;

	/// <summary>Whether to show the next sprite in the machine's spritesheet while it's processing an item.</summary>
	[ContentSerializer(Optional = true)]
	public bool ShowNextIndexWhileWorking;

	/// <summary>Whether to show the next sprite in the machine's spritesheet while it has an output ready to collect.</summary>
	[ContentSerializer(Optional = true)]
	public bool ShowNextIndexWhenReady;

	/// <summary>Whether the player can add fairy dust to speed up the machine.</summary>
	[ContentSerializer(Optional = true)]
	public bool AllowFairyDust = true;

	/// <summary>Whether this machine acts as an incubator when placed in a building, so players can incubate eggs in it.</summary>
	/// <remarks>This is used by the incubator and ostrich incubator. The game logic assumes there's only one such machine in each building, so this generally shouldn't be used by custom machines that can be built in a vanilla barn or coop.</remarks>
	[ContentSerializer(Optional = true)]
	public bool IsIncubator;

	/// <summary>Whether the machine should only produce output overnight. If it finishes processing during the day, it'll pause until its next day update.</summary>
	[ContentSerializer(Optional = true)]
	public bool OnlyCompleteOvernight;

	/// <summary>A game state query which indicates whether the machine should be emptied overnight, so any current output will be lost. Defaults to always false.</summary>
	[ContentSerializer(Optional = true)]
	public string ClearContentsOvernightCondition;

	/// <summary>The game stat counters to increment when an item is placed in the machine.</summary>
	[ContentSerializer(Optional = true)]
	public List<StatIncrement> StatsToIncrementWhenLoaded;

	/// <summary>The game stat counters to increment when the processed output is collected.</summary>
	[ContentSerializer(Optional = true)]
	public List<StatIncrement> StatsToIncrementWhenHarvested;

	/// <summary>A list of (skillName) (amount), e.g. Farming 7 Fishing 5 </summary>
	[ContentSerializer(Optional = true)]
	public string ExperienceGainOnHarvest;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
