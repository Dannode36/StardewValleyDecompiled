using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Machines;
using StardewValley.Internal;
using StardewValley.Inventories;
using StardewValley.Objects;

namespace StardewValley;

/// <summary>Handles common logic for parsing and applying the data in <c>Data/Machines</c>.</summary>
/// <remarks>For more specific logic, see the logic in <see cref="T:StardewValley.Object" /> like <see cref="M:StardewValley.Object.PlaceInMachine(StardewValley.GameData.Machines.MachineData,StardewValley.Item,System.Boolean,StardewValley.Farmer,System.Boolean,System.Boolean)" />.</remarks>
public static class MachineDataUtility
{
	/// <summary>Get the value of a token placeholder like <c>DROP_IN_ID</c>.</summary>
	/// <param name="key">The token placeholder like <c>DROP_IN_ID</c>.</param>
	/// <param name="machine">The machine which will produce output.</param>
	/// <param name="outputData">The machine output data.</param>
	/// <param name="inputItem">The item that was dropped into the machine.</param>
	/// <param name="who">The player interacting with the machine, if any.</param>
	public delegate string GetOutputTokenValueDelegate(string key, Object machine, MachineItemOutput outputData, Item inputItem, Farmer who);

	/// <summary>The token placeholders which can appear in an <see cref="P:StardewValley.GameData.ISpawnItemData.ItemId" /> field, and the methods which return their value.</summary>
	public static readonly IDictionary<string, GetOutputTokenValueDelegate> OutputTokens = new Dictionary<string, GetOutputTokenValueDelegate>
	{
		["DROP_IN_ID"] = GetTokenValue,
		["DROP_IN_PRESERVE"] = GetTokenValue,
		["NEARBY_FLOWER_ID"] = GetTokenValue,
		["DROP_IN_QUALITY"] = GetTokenValue
	};

	/// <summary>Get whether the inventory contains the additional items needed to run the machine.</summary>
	/// <param name="inventory">The inventory to search for matching items.</param>
	/// <param name="requirements">The additional required items, if any.</param>
	/// <param name="failedRequirement">The requirement which isn't met, if applicable.</param>
	public static bool HasAdditionalRequirements(IInventory inventory, IList<MachineItemAdditionalConsumedItems> requirements, out MachineItemAdditionalConsumedItems failedRequirement)
	{
		if (requirements != null && requirements.Count > 0)
		{
			foreach (MachineItemAdditionalConsumedItems requirement in requirements)
			{
				if (inventory.CountId(requirement.ItemId) < requirement.RequiredCount)
				{
					failedRequirement = requirement;
					return false;
				}
			}
		}
		failedRequirement = null;
		return true;
	}

	/// <summary>Get whether an output rule matches the given item.</summary>
	/// <param name="machine">The machine instance.</param>
	/// <param name="rule">The machine output rule.</param>
	/// <param name="trigger">The rule trigger type to match.</param>
	/// <param name="inputItem">The item that was dropped into the machine.</param>
	/// <param name="who">The player interacting with the machine, if any.</param>
	/// <param name="location">The location containing the machine.</param>
	/// <param name="triggerRule">The output rule trigger that matched, if applicable.</param>
	/// <param name="matchesExceptCount">Whether the output can be applied if <see cref="P:StardewValley.GameData.Machines.MachineOutputTriggerRule.RequiredCount" /> is ignored.</param>
	public static bool CanApplyOutput(Object machine, MachineOutputRule rule, MachineOutputTrigger trigger, Item inputItem, Farmer who, GameLocation location, out MachineOutputTriggerRule triggerRule, out bool matchesExceptCount)
	{
		matchesExceptCount = false;
		triggerRule = null;
		if (rule.Triggers == null)
		{
			return false;
		}
		foreach (MachineOutputTriggerRule curTrigger in rule.Triggers)
		{
			if (!curTrigger.Trigger.HasFlag(trigger) || (curTrigger.Condition != null && !GameStateQuery.CheckConditions(curTrigger.Condition, location, who, null, inputItem)))
			{
				continue;
			}
			if (trigger.HasFlag(MachineOutputTrigger.ItemPlacedInMachine) || trigger.HasFlag(MachineOutputTrigger.OutputCollected))
			{
				if (curTrigger.RequiredItemId != null && !ItemRegistry.HasItemId(inputItem, curTrigger.RequiredItemId))
				{
					continue;
				}
				List<string> requiredTags = curTrigger.RequiredTags;
				if (requiredTags != null && requiredTags.Count > 0 && !ItemContextTagManager.DoAllTagsMatch(curTrigger.RequiredTags, inputItem.GetContextTags()))
				{
					continue;
				}
				if (curTrigger.RequiredCount > inputItem.Stack)
				{
					triggerRule = curTrigger;
					matchesExceptCount = true;
					continue;
				}
			}
			triggerRule = curTrigger;
			matchesExceptCount = false;
			return true;
		}
		return false;
	}

	/// <summary>Get the first output rule which matches the given item, if any.</summary>
	/// <param name="machine">The machine instance.</param>
	/// <param name="machineData">The machine data from which to get an output rule.</param>
	/// <param name="trigger">The rule trigger type to match.</param>
	/// <param name="inputItem">The item that was dropped into the machine.</param>
	/// <param name="who">The player interacting with the machine, if any.</param>
	/// <param name="location">The location containing the machine.</param>
	/// <param name="rule">The output rule found, if applicable.</param>
	/// <param name="triggerRule">The output rule trigger that matched, if applicable.</param>
	/// <param name="ruleIgnoringCount">If no output rule was found, the output rule that would have matched if we ignore the <see cref="P:StardewValley.GameData.Machines.MachineOutputTriggerRule.RequiredCount" /> field. If there are multiple such rules, the first one with a <see cref="F:StardewValley.GameData.Machines.MachineOutputRule.InvalidCountMessage" /> set is selected, else the first one in the list.</param>
	/// <param name="triggerIgnoringCount">The output rule trigger that matched for <paramref name="ruleIgnoringCount" />, if applicable.</param>
	public static bool TryGetMachineOutputRule(Object machine, MachineData machineData, MachineOutputTrigger trigger, Item inputItem, Farmer who, GameLocation location, out MachineOutputRule rule, out MachineOutputTriggerRule triggerRule, out MachineOutputRule ruleIgnoringCount, out MachineOutputTriggerRule triggerIgnoringCount)
	{
		rule = null;
		triggerRule = null;
		ruleIgnoringCount = null;
		triggerIgnoringCount = null;
		if (machineData?.OutputRules == null)
		{
			return false;
		}
		foreach (MachineOutputRule curRule in machineData.OutputRules)
		{
			if (CanApplyOutput(machine, curRule, trigger, inputItem, who, location, out triggerRule, out var matchesExceptCount))
			{
				rule = curRule;
				return true;
			}
			if (matchesExceptCount && (ruleIgnoringCount == null || (ruleIgnoringCount.InvalidCountMessage == null && curRule.InvalidCountMessage != null)))
			{
				ruleIgnoringCount = curRule;
				triggerIgnoringCount = triggerRule;
			}
		}
		return false;
	}

	/// <summary>Get the output item data which matches the given item, if any.</summary>
	/// <param name="machine">The machine instance.</param>
	/// <param name="machineData">The machine data from which to get the output data.</param>
	/// <param name="outputRule">The output rule from which to get the output data, or <c>null</c> to get a matching rule from the machine data.</param>
	/// <param name="inputItem">The item that was dropped into the machine.</param>
	/// <param name="who">The player interacting with the machine, if any.</param>
	/// <param name="location">The location containing the machine.</param>
	public static MachineItemOutput GetOutputData(Object machine, MachineData machineData, MachineOutputRule outputRule, Item inputItem, Farmer who, GameLocation location)
	{
		if (outputRule == null && !TryGetMachineOutputRule(machine, machineData, MachineOutputTrigger.ItemPlacedInMachine, inputItem, who, location, out outputRule, out var _, out var _, out var _))
		{
			return null;
		}
		return GetOutputData(outputRule.OutputItem, outputRule.UseFirstValidOutput, inputItem, who, location);
	}

	/// <summary>Get the output item data which matches the given item, if any.</summary>
	/// <param name="outputs">The output entries to choose from.</param>
	/// <param name="useFirstValidOutput">Whether to return the first matching output; else a valid one will be chosen at random.</param>
	/// <param name="inputItem">The item that was dropped into the machine.</param>
	/// <param name="who">The player interacting with the machine, if any.</param>
	/// <param name="location">The location containing the machine.</param>
	public static MachineItemOutput GetOutputData(List<MachineItemOutput> outputs, bool useFirstValidOutput, Item inputItem, Farmer who, GameLocation location)
	{
		if (outputs == null || outputs.Count <= 0)
		{
			return null;
		}
		List<MachineItemOutput> validOutputs = ((!useFirstValidOutput) ? new List<MachineItemOutput>() : null);
		foreach (MachineItemOutput possibleOutput in outputs)
		{
			if (GameStateQuery.CheckConditions(possibleOutput.Condition, location, who, null, inputItem))
			{
				if (useFirstValidOutput)
				{
					return possibleOutput;
				}
				validOutputs.Add(possibleOutput);
			}
		}
		if (useFirstValidOutput)
		{
			return null;
		}
		return Game1.random.ChooseFrom(validOutputs);
	}

	/// <summary>Get the item to produce for a given output data.</summary>
	/// <param name="machine">The machine which will produce output.</param>
	/// <param name="outputData">The machine output data.</param>
	/// <param name="inputItem">The item that was dropped into the machine.</param>
	/// <param name="who">The player interacting with the machine, if any.</param>
	/// <param name="probe">Whether the machine is only checking whether the input is valid. If so, the input/machine shouldn't be changed and no animations/sounds should play.</param>
	/// <param name="overrideMinutesUntilReady">The in-game minutes until the item will be ready to collect, if set. This overrides the equivalent fields in the machine data if set.</param>
	public static Item GetOutputItem(Object machine, MachineItemOutput outputData, Item inputItem, Farmer who, bool probe, out int? overrideMinutesUntilReady)
	{
		overrideMinutesUntilReady = null;
		if (outputData == null)
		{
			return null;
		}
		ItemQueryContext context = new ItemQueryContext(machine.Location, who, Game1.random);
		Item item;
		if (outputData.OutputMethod != null)
		{
			if (!StaticDelegateBuilder.TryCreateDelegate<MachineOutputDelegate>(outputData.OutputMethod, out var method, out var error))
			{
				Game1.log.Warn($"Machine {machine.QualifiedItemId} has invalid item output method '{outputData.OutputMethod}': {error}");
				return null;
			}
			item = method(machine, inputItem, probe, outputData, out overrideMinutesUntilReady);
			item = (Item)ItemQueryResolver.ApplyItemFields(item, outputData, context, inputItem);
		}
		else if (outputData.ItemId == "DROP_IN")
		{
			item = inputItem?.getOne();
			item = (Item)ItemQueryResolver.ApplyItemFields(item, outputData, context, inputItem);
		}
		else
		{
			item = ItemQueryResolver.TryResolveRandomItem(outputData, context, avoidRepeat: false, null, (string id) => FormatOutputId(id, machine, outputData, inputItem, who), inputItem, delegate(string query, string error)
			{
				Game1.log.Error($"Machine '{machine.QualifiedItemId}' failed parsing item query '{query}' for output '{outputData.Id}': {error}.");
			});
		}
		if (item == null)
		{
			return null;
		}
		if (outputData.CopyColor && inputItem is ColoredObject droppedInColoredObj)
		{
			Color color = droppedInColoredObj.color.Value;
			if (item is ColoredObject newColoredObj)
			{
				newColoredObj.color.Value = color;
			}
			else if (item.HasTypeObject())
			{
				item = new ColoredObject(item.ItemId, 1, color);
				item = (Item)ItemQueryResolver.ApplyItemFields(item, outputData, context, inputItem);
			}
		}
		if (outputData.CopyQuality && inputItem != null)
		{
			item.Quality = inputItem.Quality;
		}
		if (item is Object obj && outputData.ObjectInternalName != null)
		{
			obj.Name = string.Format(outputData.ObjectInternalName, inputItem?.Name ?? "");
		}
		if (item is Object heldObj)
		{
			if (outputData.CopyPrice && inputItem is Object inputObj)
			{
				heldObj.Price = inputObj.Price;
			}
			List<QuantityModifier> priceModifiers = outputData.PriceModifiers;
			if (priceModifiers != null && priceModifiers.Count > 0)
			{
				heldObj.Price = (int)Utility.ApplyQuantityModifiers(heldObj.Price, outputData.PriceModifiers, outputData.PriceModifierMode, machine.Location, who, item, inputItem);
			}
			if (!string.IsNullOrEmpty(outputData.PreserveType))
			{
				heldObj.preserve.Value = (Object.PreserveType)Enum.Parse(typeof(Object.PreserveType), outputData.PreserveType);
			}
			if (!string.IsNullOrEmpty(outputData.PreserveId))
			{
				heldObj.preservedParentSheetIndex.Value = ((!(outputData.PreserveId == "DROP_IN")) ? outputData.PreserveId : inputItem?.ItemId);
			}
		}
		return item;
	}

	/// <summary>Increment stats when an item is placed in the machine, if applicable.</summary>
	/// <param name="stats">The stats data to apply.</param>
	/// <param name="item">The item that was placed in the machine.</param>
	/// <param name="amount">The number of items that were placed in the machine.</param>
	public static void UpdateStats(List<StatIncrement> stats, Item item, int amount)
	{
		if (stats == null)
		{
			return;
		}
		foreach (StatIncrement stat in stats)
		{
			if (stat.RequiredItemId == null || ItemRegistry.HasItemId(item, stat.RequiredItemId))
			{
				List<string> requiredTags = stat.RequiredTags;
				if (requiredTags == null || requiredTags.Count <= 0 || ItemContextTagManager.DoAllTagsMatch(stat.RequiredTags, item.GetContextTags()))
				{
					Game1.stats.Increment(stat.StatName, amount);
				}
			}
		}
	}

	/// <summary>Apply a machine effect, if it's valid and its fields match.</summary>
	/// <param name="machine">The machine for which to apply effects.</param>
	/// <param name="effect">The machine effect to apply.</param>
	/// <param name="playSounds">Whether to play sounds when the item is placed.</param>
	public static bool PlayEffects(Object machine, MachineEffects effect, bool playSounds = true)
	{
		if (effect == null)
		{
			return false;
		}
		if (!GameStateQuery.CheckConditions(effect.Condition, machine.Location, null, inputItem: machine.lastInputItem.Value, targetItem: machine.heldObject.Value))
		{
			return false;
		}
		if (playSounds)
		{
			List<MachineSoundData> sounds = effect.Sounds;
			if (sounds != null && sounds.Count > 0)
			{
				foreach (MachineSoundData sound in effect.Sounds)
				{
					if (sound.Delay <= 0)
					{
						machine.Location.playSound(sound.Id, machine.TileLocation);
					}
					else
					{
						DelayedAction.playSoundAfterDelay(sound.Id, sound.Delay, machine.Location, machine.TileLocation);
					}
				}
			}
		}
		if (effect.ShakeDuration >= 0)
		{
			machine.shakeTimer = effect.ShakeDuration;
		}
		if (effect.TemporarySprites != null)
		{
			foreach (TemporaryAnimatedSpriteDefinition temporarySprite in effect.TemporarySprites)
			{
				if (GameStateQuery.CheckConditions(temporarySprite.Condition, machine.Location, null, inputItem: machine.lastInputItem.Value, targetItem: machine.heldObject.Value))
				{
					TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.CreateFromData(temporarySprite, machine.tileLocation.X, machine.tileLocation.Y, (machine.tileLocation.Y + 1f) * 64f / 10000f);
					Game1.multiplayer.broadcastSprites(machine.Location, sprite);
				}
			}
		}
		return true;
	}

	/// <summary>Replace machine placeholder tokens for an <see cref="P:StardewValley.GameData.ISpawnItemData.ItemId" /> field.</summary>
	/// <param name="id">The <see cref="P:StardewValley.GameData.ISpawnItemData.ItemId" /> value.</param>
	/// <param name="machine">The machine producing the output.</param>
	/// <param name="outputData">The machine output data.</param>
	/// <param name="inputItem">The item dropped into the machine, if any.</param>
	/// <param name="who">The player interacting with the machine, if any.</param>
	public static string FormatOutputId(string id, Object machine, MachineItemOutput outputData, Item inputItem, Farmer who)
	{
		if (string.IsNullOrWhiteSpace(id))
		{
			return id;
		}
		bool changed = false;
		string[] words = ArgUtility.SplitBySpace(id);
		for (int i = 0; i < words.Length; i++)
		{
			if (OutputTokens.TryGetValue(words[i], out var getValue))
			{
				string oldValue = words[i];
				words[i] = getValue(words[i], machine, outputData, inputItem, who);
				changed = changed || words[i] != oldValue;
			}
		}
		if (!changed)
		{
			return id;
		}
		return string.Join(" ", words);
	}

	/// <summary>Get the value of a default output placeholder like <c>DROP_IN_ID</c>.</summary>
	/// <inheritdoc cref="T:StardewValley.MachineDataUtility.GetOutputTokenValueDelegate" />
	private static string GetTokenValue(string key, Object machine, MachineItemOutput outputData, Item inputItem, Farmer who)
	{
		return key switch
		{
			"DROP_IN_ID" => inputItem?.QualifiedItemId ?? "0", 
			"DROP_IN_PRESERVE" => (inputItem as Object)?.preservedParentSheetIndex.Value ?? "0", 
			"NEARBY_FLOWER_ID" => GetNearbyFlowerItemId(machine) ?? "-1", 
			"DROP_IN_QUALITY" => (inputItem?.Quality).ToString() ?? "", 
			_ => key, 
		};
	}

	/// <summary>Get the item ID produced by a flower within 5 tiles of the machine, if any.</summary>
	/// <param name="machine">The machine around which to check.</param>
	public static string GetNearbyFlowerItemId(Object machine)
	{
		return Utility.findCloseFlower(machine.Location, machine.tileLocation.Value, 5, (Crop curCrop) => !curCrop.forageCrop.Value)?.indexOfHarvest.Value;
	}
}
