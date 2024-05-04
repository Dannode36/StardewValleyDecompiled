using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Netcode;
using StardewValley.Delegates;
using StardewValley.GameData;
using StardewValley.Network.NetEvents;
using StardewValley.SpecialOrders;

namespace StardewValley.Triggers;

/// <summary>Manages trigger actions defined in the <c>Data/TriggerActions</c> asset, which perform actions when their conditions are met.</summary>
public static class TriggerActionManager
{
	/// <summary>The low-level trigger actions defined by the base game. Most code should use <see cref="T:StardewValley.Triggers.TriggerActionManager" /> methods instead.</summary>
	/// <remarks>Every method within this class is an action whose name matches the method name. All actions must be static, public, and match <see cref="T:StardewValley.Delegates.TriggerActionDelegate" />.</remarks>
	public static class DefaultActions
	{
		/// <summary>An action which does nothing.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool Null(string[] args, TriggerActionContext context, out string error)
		{
			error = null;
			return true;
		}

		/// <summary>Perform an action if a game state query matches, with an optional fallback action.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool If(string[] args, TriggerActionContext context, out string error)
		{
			int startTrueIndex = -1;
			for (int i = 1; i < args.Length; i++)
			{
				if (args[i] == "##")
				{
					startTrueIndex = i + 1;
					break;
				}
			}
			if (startTrueIndex == -1 || startTrueIndex == args.Length)
			{
				return InvalidFormatError(out error);
			}
			int startFalseIndex = -1;
			for (int i = startTrueIndex + 1; i < args.Length; i++)
			{
				if (args[i] == "##")
				{
					startFalseIndex = i + 1;
					break;
				}
			}
			if (startFalseIndex == args.Length - 1)
			{
				return InvalidFormatError(out error);
			}
			Exception exception;
			if (GameStateQuery.CheckConditions(ArgUtility.UnsplitQuoteAware(ArgUtility.GetSubsetOf(args, 1, startTrueIndex - 1 - 1), ' ')))
			{
				int length = ((startFalseIndex > -1) ? (startFalseIndex - startTrueIndex - 1) : (-1));
				string action = ArgUtility.UnsplitQuoteAware(ArgUtility.GetSubsetOf(args, startTrueIndex, length), ' ');
				if (!TryRunAction(action, out error, out exception))
				{
					error = "failed applying if-true action '" + action + "': " + error;
					return false;
				}
			}
			else if (startFalseIndex > -1)
			{
				string action = ArgUtility.UnsplitQuoteAware(ArgUtility.GetSubsetOf(args, startFalseIndex), ' ');
				if (!TryRunAction(action, out error, out exception))
				{
					error = "failed applying if-false action '" + action + "': " + error;
					return false;
				}
			}
			error = null;
			return true;
			static bool InvalidFormatError(out string outError)
			{
				outError = "invalid format: expected a string in the form 'If <game state query> ## <do if true>' or 'If <game state query> ## <do if true> ## <do if false>'";
				return false;
			}
		}

		/// <summary>Apply a buff to the current player.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool AddBuff(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGet(args, 1, out var buffId, out error) || !ArgUtility.TryGetOptionalInt(args, 2, out var duration, out error, -1))
			{
				return false;
			}
			Buff buff = new Buff(buffId, null, null, duration, null, -1, null, false);
			Game1.player.applyBuff(buff);
			return true;
		}

		/// <summary>Remove a buff from the current player.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool RemoveBuff(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGet(args, 1, out var buffId, out error))
			{
				return false;
			}
			Game1.player.buffs.Remove(buffId);
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool AddMail(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGetEnum<PlayerActionTarget>(args, 1, out var playerTarget, out error) || !ArgUtility.TryGet(args, 2, out var mailId, out error) || !ArgUtility.TryGetOptionalEnum(args, 3, out var mailType, out error, MailType.Tomorrow))
			{
				return false;
			}
			Game1.player.team.RequestSetMail(playerTarget, mailId, mailType, add: true);
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool RemoveMail(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGetEnum<PlayerActionTarget>(args, 1, out var playerTarget, out error) || !ArgUtility.TryGet(args, 2, out var mailId, out error) || !ArgUtility.TryGetOptionalEnum(args, 3, out var mailType, out error, MailType.All))
			{
				return false;
			}
			Game1.player.team.RequestSetMail(playerTarget, mailId, mailType, add: false);
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool AddQuest(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGet(args, 1, out var questId, out error))
			{
				return false;
			}
			Game1.player.addQuest(questId);
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool RemoveQuest(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGet(args, 1, out var questId, out error))
			{
				return false;
			}
			Game1.player.removeQuest(questId);
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool AddSpecialOrder(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGet(args, 1, out var orderId, out error))
			{
				return false;
			}
			Game1.player.team.AddSpecialOrder(orderId);
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool RemoveSpecialOrder(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGet(args, 1, out var orderId, out error))
			{
				return false;
			}
			NetList<SpecialOrder, NetRef<SpecialOrder>> orders = Game1.player.team.specialOrders;
			for (int i = orders.Count - 1; i >= 0; i--)
			{
				if (orders[i].questKey.Value == orderId)
				{
					orders.RemoveAt(i);
				}
			}
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool AddItem(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGet(args, 1, out var itemId, out error) || !ArgUtility.TryGetOptionalInt(args, 2, out var count, out error, 1) || !ArgUtility.TryGetOptionalInt(args, 3, out var quality, out error))
			{
				return false;
			}
			Item item = ItemRegistry.Create(itemId, count, quality);
			if (item != null)
			{
				Game1.player.addItemByMenuIfNecessary(item);
			}
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool RemoveItem(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGet(args, 1, out var itemId, out error) || !ArgUtility.TryGetOptionalInt(args, 2, out var count, out error, 1))
			{
				return false;
			}
			Game1.player.removeFirstOfThisItemFromInventory(itemId, count);
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool AddMoney(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGetInt(args, 1, out var amount, out error))
			{
				return false;
			}
			Game1.player.Money += amount;
			if (Game1.player.Money < 0)
			{
				Game1.player.Money = 0;
			}
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool AddFriendshipPoints(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGet(args, 1, out var npcName, out error) || !ArgUtility.TryGetInt(args, 2, out var points, out error))
			{
				return false;
			}
			NPC npc = Game1.getCharacterFromName(npcName);
			if (npc == null)
			{
				error = "no NPC found with name '" + npcName + "'";
				return false;
			}
			Game1.player.changeFriendship(points, npc);
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool AddConversationTopic(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGet(args, 1, out var topicId, out error) || !ArgUtility.TryGetOptionalInt(args, 2, out var daysDuration, out error, 4))
			{
				return false;
			}
			Game1.player.activeDialogueEvents[topicId] = daysDuration;
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool RemoveConversationTopic(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGet(args, 1, out var topicId, out error))
			{
				return false;
			}
			Game1.player.activeDialogueEvents.Remove(topicId);
			return true;
		}

		/// <summary>Increment or decrement a stats value for the current player.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool IncrementStat(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGet(args, 1, out var statKey, out error, allowBlank: false) || !ArgUtility.TryGetOptionalInt(args, 2, out var amount, out error, 1))
			{
				return false;
			}
			Game1.player.stats.Increment(statKey, amount);
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool MarkActionApplied(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGetEnum<PlayerActionTarget>(args, 1, out var playerTarget, out error) || !ArgUtility.TryGet(args, 2, out var actionId, out error, allowBlank: false) || !ArgUtility.TryGetOptionalBool(args, 3, out var applied, out error, defaultValue: true))
			{
				return false;
			}
			Game1.player.team.RequestSetSimpleFlag(SimpleFlagType.ActionApplied, playerTarget, actionId, applied);
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool MarkCookingRecipeKnown(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGetEnum<PlayerActionTarget>(args, 1, out var playerTarget, out error) || !ArgUtility.TryGet(args, 2, out var recipeKey, out error) || !ArgUtility.TryGetOptionalBool(args, 3, out var learned, out error, defaultValue: true))
			{
				return false;
			}
			Game1.player.team.RequestSetSimpleFlag(SimpleFlagType.CookingRecipeKnown, playerTarget, recipeKey, learned);
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool MarkCraftingRecipeKnown(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGetEnum<PlayerActionTarget>(args, 1, out var playerTarget, out error) || !ArgUtility.TryGet(args, 2, out var recipeKey, out error) || !ArgUtility.TryGetOptionalBool(args, 3, out var learned, out error, defaultValue: true))
			{
				return false;
			}
			Game1.player.team.RequestSetSimpleFlag(SimpleFlagType.CraftingRecipeKnown, playerTarget, recipeKey, learned);
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool MarkEventSeen(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGetEnum<PlayerActionTarget>(args, 1, out var playerTarget, out error) || !ArgUtility.TryGet(args, 2, out var eventId, out error, allowBlank: false) || !ArgUtility.TryGetOptionalBool(args, 3, out var seen, out error, defaultValue: true))
			{
				return false;
			}
			Game1.player.team.RequestSetSimpleFlag(SimpleFlagType.EventSeen, playerTarget, eventId, seen);
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool MarkQuestionAnswered(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGetEnum<PlayerActionTarget>(args, 1, out var playerTarget, out error) || !ArgUtility.TryGet(args, 2, out var questionId, out error, allowBlank: false) || !ArgUtility.TryGetOptionalBool(args, 3, out var answered, out error, defaultValue: true))
			{
				return false;
			}
			Game1.player.team.RequestSetSimpleFlag(SimpleFlagType.DialogueAnswerSelected, playerTarget, questionId, answered);
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool MarkSongHeard(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGetEnum<PlayerActionTarget>(args, 1, out var playerTarget, out error) || !ArgUtility.TryGet(args, 2, out var trackId, out error, allowBlank: false) || !ArgUtility.TryGetOptionalBool(args, 3, out var heard, out error, defaultValue: true))
			{
				return false;
			}
			Game1.player.team.RequestSetSimpleFlag(SimpleFlagType.SongHeard, playerTarget, trackId, heard);
			return true;
		}

		/// <summary>Remove all temporary animated sprites in the current location.</summary>
		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool RemoveTemporaryAnimatedSprites(string[] args, TriggerActionContext context, out string error)
		{
			Game1.currentLocation?.TemporarySprites.Clear();
			error = null;
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool SetNpcInvisible(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGet(args, 1, out var npcName, out error, allowBlank: false) || !ArgUtility.TryGetInt(args, 2, out var daysDuration, out error))
			{
				return false;
			}
			NPC npc = Game1.getCharacterFromName(npcName);
			if (npc == null)
			{
				error = "no NPC found with name '" + npcName + "'";
				return false;
			}
			npc.IsInvisible = true;
			npc.daysUntilNotInvisible = daysDuration;
			return true;
		}

		/// <inheritdoc cref="T:StardewValley.Delegates.TriggerActionDelegate" />
		public static bool SetNpcVisible(string[] args, TriggerActionContext context, out string error)
		{
			if (!ArgUtility.TryGet(args, 1, out var npcName, out error, allowBlank: false))
			{
				return false;
			}
			NPC npc = Game1.getCharacterFromName(npcName);
			if (npc == null)
			{
				error = "no NPC found with name '" + npcName + "'";
				return false;
			}
			npc.IsInvisible = false;
			npc.daysUntilNotInvisible = 0;
			return true;
		}
	}

	/// <summary>The trigger type raised overnight immediately before the game changes the date, sets up the new day, and saves.</summary>
	public const string trigger_dayEnding = "DayEnding";

	/// <summary>The trigger type raised when the player starts a day, after either sleeping or loading.</summary>
	public const string trigger_dayStarted = "DayStarted";

	/// <summary>The trigger type raised when the player arrives in a new location.</summary>
	public const string trigger_locationChanged = "LocationChanged";

	/// <summary>The trigger type used for actions that are triggered elsewhere than <c>Data/TriggerActions</c>.</summary>
	public const string trigger_manual = "Manual";

	/// <summary>The trigger types that can be used in the <see cref="F:StardewValley.GameData.TriggerActionData.Trigger" /> field.</summary>
	private static readonly HashSet<string> ValidTriggerTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "DayEnding", "DayStarted", "LocationChanged", "Manual" };

	/// <summary>The action handlers indexed by name.</summary>
	/// <remarks>Action names are case-insensitive.</remarks>
	private static readonly Dictionary<string, TriggerActionDelegate> ActionHandlers = new Dictionary<string, TriggerActionDelegate>(StringComparer.OrdinalIgnoreCase);

	/// <summary>A cached lookup of actions by trigger name.</summary>
	private static readonly Dictionary<string, List<CachedTriggerAction>> ActionsByTrigger = new Dictionary<string, List<CachedTriggerAction>>(StringComparer.OrdinalIgnoreCase);

	/// <summary>A cached lookup of parsed action strings.</summary>
	private static readonly Dictionary<string, CachedAction> ActionCache = new Dictionary<string, CachedAction>(StringComparer.OrdinalIgnoreCase);

	/// <summary>A parsed action which does nothing.</summary>
	private static CachedAction NullAction;

	/// <summary>The trigger action context used for a default manual option.</summary>
	private static readonly TriggerActionContext EmptyManualContext = new TriggerActionContext("Manual", LegacyShims.EmptyArray<object>(), null);

	/// <summary>Register a trigger type.</summary>
	/// <param name="name">The trigger key. This is case-insensitive.</param>
	public static void RegisterTrigger(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			Game1.log.Error("Can't register an empty trigger type for Data/Triggers.");
			return;
		}
		ValidTriggerTypes.Add(name);
		Game1.log.Verbose("Registered Data/Triggers trigger type: " + name + ".");
	}

	/// <summary>Register an action handler.</summary>
	/// <param name="name">The action name. This is case-insensitive.</param>
	/// <param name="action">The handler to call when the action should apply.</param>
	public static void RegisterAction(string name, TriggerActionDelegate action)
	{
		InitializeIfNeeded();
		if (ActionHandlers.ContainsKey(name))
		{
			Game1.log.Warn("Warning: event command " + name + " is already defined and will be overwritten.");
			return;
		}
		ActionHandlers[name] = action;
		Game1.log.Verbose("Registered Data/Triggers action: " + name);
	}

	/// <summary>Run all actions for a given trigger key.</summary>
	/// <param name="trigger">The trigger key to raise.</param>
	/// <param name="triggerArgs">The contextual arguments provided with the trigger, if applicable. For example, an 'item received' trigger might provide the item instance and index.</param>
	/// <param name="location">The location for which to check action conditions, or <c>null</c> to use the current location.</param>
	/// <param name="player">The player for which to check action conditions, or <c>null</c> to use the current player.</param>
	/// <param name="targetItem">The target item (e.g. machine output or tree fruit) for which to check action conditions, or <c>null</c> if not applicable.</param>
	/// <param name="inputItem">The input item (e.g. machine input) for which to check action conditions, or <c>null</c> if not applicable.</param>
	public static void Raise(string trigger, object[] triggerArgs = null, GameLocation location = null, Farmer player = null, Item targetItem = null, Item inputItem = null)
	{
		InitializeIfNeeded();
		if (ValidTriggerTypes.TryGetValue(trigger, out var actualTrigger))
		{
			trigger = actualTrigger;
			triggerArgs = triggerArgs ?? LegacyShims.EmptyArray<object>();
			{
				foreach (CachedTriggerAction entry in GetActionsForTrigger(trigger))
				{
					TriggerActionData data = entry.Data;
					TriggerActionContext context = new TriggerActionContext(trigger, triggerArgs, entry.Data);
					if (!CanApply(data, location, player, targetItem, inputItem))
					{
						continue;
					}
					CachedAction[] actions = entry.Actions;
					foreach (CachedAction action in actions)
					{
						if (TryRunAction(action, context, out var error, out var ex))
						{
							Game1.log.Verbose($"Applied trigger action '{data.Id}' with actions [{string.Join("], [", entry.ActionStrings)}].");
						}
						else
						{
							Game1.log.Error($"Trigger action '{data.Id}' has action string '{string.Join(" ", action.Args)}' which couldn't be applied: {error}.", ex);
						}
					}
					if (data.MarkActionApplied)
					{
						Game1.player.triggerActionsRun.Add(data.Id);
					}
					Game1.log.Verbose($"Applied trigger action '{data.Id}' with actions [{string.Join("], [", entry.ActionStrings)}].");
				}
				return;
			}
		}
		Game1.log.Error("Can't raise unknown trigger type '" + trigger + "'.");
	}

	/// <summary>Parse a raw action value.</summary>
	/// <param name="action">The action string to parse.</param>
	/// <remarks>This is a low-level method. Most code should use <see cref="M:StardewValley.Triggers.TriggerActionManager.TryRunAction(System.String,System.String@,System.Exception@)" /> instead.</remarks>
	public static CachedAction ParseAction(string action)
	{
		if (string.IsNullOrWhiteSpace(action))
		{
			return NullAction;
		}
		action = action.Trim();
		if (!ActionCache.TryGetValue(action, out var parsed))
		{
			string[] args = ArgUtility.SplitBySpaceQuoteAware(action);
			string actionKey = args[0];
			parsed = (TryGetActionHandler(actionKey, out var handler) ? new CachedAction(args, handler, null, isNullHandler: false) : new CachedAction(args, NullAction.Handler, $"unknown action '{actionKey}' ignored (expected one of '{string.Join("', '", ActionHandlers.Keys.OrderBy<string, string>((string p) => p, StringComparer.OrdinalIgnoreCase))}')", isNullHandler: true));
			ActionCache[action] = parsed;
		}
		return parsed;
	}

	/// <summary>Get whether an action matches an existing action.</summary>
	/// <param name="action">The action string to validate.</param>
	/// <param name="error">An error phrase indicating why parsing the action failed (like 'unknown action X'), if applicable.</param>
	/// <returns>Returns whether the action was parsed successfully and matches an existing command.</returns>
	public static bool TryValidateActionExists(string action, out string error)
	{
		CachedAction parsed = ParseAction(action);
		error = parsed.Error;
		return error == null;
	}

	/// <summary>Run an action if it's valid.</summary>
	/// <param name="action">The action string to run.</param>
	/// <param name="error">An error phrase indicating why parsing or running the action failed (like 'unknown action X'), if applicable.</param>
	/// <param name="exception">An exception which accompanies <paramref name="error" />, if applicable.</param>
	/// <returns>Returns whether the action was applied successfully (regardless of whether it did anything).</returns>
	public static bool TryRunAction(string action, out string error, out Exception exception)
	{
		bool num = TryRunAction(ParseAction(action), EmptyManualContext, out error, out exception);
		if (!num && string.IsNullOrWhiteSpace(error))
		{
			error = ((exception != null) ? "an unhandled error occurred" : "the action failed but didn't provide an error message");
		}
		return num;
	}

	/// <summary>Run an action if it's valid.</summary>
	/// <param name="action">The action string to run.</param>
	/// <param name="trigger">The trigger key to raise.</param>
	/// <param name="triggerArgs">The contextual arguments provided with the trigger, if applicable. For example, an 'item received' trigger might provide the item instance and index.</param>
	/// <param name="error">An error phrase indicating why parsing or running the action failed (like 'unknown action X'), if applicable.</param>
	/// <param name="exception">An exception which accompanies <paramref name="error" />, if applicable.</param>
	/// <returns>Returns whether the action was applied successfully (regardless of whether it did anything).</returns>
	public static bool TryRunAction(string action, string trigger, object[] triggerArgs, out string error, out Exception exception)
	{
		if (trigger == null)
		{
			throw new ArgumentNullException("trigger");
		}
		if (triggerArgs == null)
		{
			throw new ArgumentNullException("triggerArgs");
		}
		TriggerActionContext context = ((trigger == "Manual" && triggerArgs.Length == 0) ? EmptyManualContext : new TriggerActionContext(trigger, triggerArgs, null));
		return TryRunAction(ParseAction(action), context, out error, out exception);
	}

	/// <summary>Run an action if it's valid.</summary>
	/// <param name="action">The action to run.</param>
	/// <param name="context">The trigger action context.</param>
	/// <param name="error">An error phrase indicating why parsing or running the action failed (like 'unknown action X'), if applicable.</param>
	/// <param name="exception">An exception which accompanies <paramref name="error" />, if applicable.</param>
	/// <returns>Returns whether the action was applied successfully (regardless of whether it did anything).</returns>
	/// <remarks>This is a low-level method. Most code should use <see cref="M:StardewValley.Triggers.TriggerActionManager.TryRunAction(System.String,System.String@,System.Exception@)" /> instead.</remarks>
	public static bool TryRunAction(CachedAction action, TriggerActionContext context, out string error, out Exception exception)
	{
		if (action == null)
		{
			error = null;
			exception = null;
			return true;
		}
		if (action.Error != null)
		{
			error = action.Error;
			exception = null;
			return false;
		}
		try
		{
			action.Handler(action.Args, context, out error);
			if (error != null)
			{
				exception = null;
				return false;
			}
			exception = null;
			return true;
		}
		catch (Exception ex)
		{
			error = "an unexpected error occurred";
			exception = ex;
			return false;
		}
	}

	/// <summary>Get the handler for an action key, if any.</summary>
	/// <param name="key">The action key. This is case-insensitive.</param>
	/// <param name="handler">The action handler, if found.</param>
	/// <returns>Returns whether a handler was found for the action key.</returns>
	public static bool TryGetActionHandler(string key, out TriggerActionDelegate handler)
	{
		InitializeIfNeeded();
		return ActionHandlers.TryGetValue(key, out handler);
	}

	/// <summary>Get the trigger actions in <c>Data/TriggerActions</c> registered for a given trigger, or an empty list if none are registered.</summary>
	/// <param name="trigger">The trigger key to raise.</param>
	/// <remarks>This is a low-level method. Most code should use <see cref="M:StardewValley.Triggers.TriggerActionManager.TryRunAction(System.String,System.String@,System.Exception@)" /> instead.</remarks>
	public static IReadOnlyList<CachedTriggerAction> GetActionsForTrigger(string trigger)
	{
		if (ActionsByTrigger.TryGetValue(trigger, out var cached))
		{
			return cached;
		}
		return LegacyShims.EmptyArray<CachedTriggerAction>();
	}

	/// <summary>Get whether an action can be applied based on its conditions and whether it has already been run.</summary>
	/// <param name="action">The action to check.</param>
	/// <param name="location">The location for which to check action conditions, or <c>null</c> to use the current location.</param>
	/// <param name="player">The player for which to check action conditions, or <c>null</c> to use the current player.</param>
	/// <param name="targetItem">The target item (e.g. machine output or tree fruit) for which to check action conditions, or <c>null</c> if not applicable.</param>
	/// <param name="inputItem">The input item (e.g. machine input) for which to check action conditions, or <c>null</c> if not applicable.</param>
	public static bool CanApply(TriggerActionData action, GameLocation location = null, Farmer player = null, Item targetItem = null, Item inputItem = null)
	{
		if ((!action.HostOnly || Game1.IsMasterGame) && !Game1.player.triggerActionsRun.Contains(action.Id))
		{
			return GameStateQuery.CheckConditions(action.Condition, location, player, targetItem, inputItem);
		}
		return false;
	}

	/// <summary>Rebuild the cached data from <c>Data/TriggerActions</c>.</summary>
	public static void ResetDataCache()
	{
		ActionCache.Clear();
		ActionsByTrigger.Clear();
	}

	/// <summary>Register the vanilla event commands and preconditions if they haven't already been registered.</summary>
	private static void InitializeIfNeeded()
	{
		if (ActionHandlers.Count == 0)
		{
			MethodInfo[] methods = typeof(DefaultActions).GetMethods(BindingFlags.Static | BindingFlags.Public);
			foreach (MethodInfo method in methods)
			{
				TriggerActionDelegate action = (TriggerActionDelegate)Delegate.CreateDelegate(typeof(TriggerActionDelegate), method);
				ActionHandlers.Add(method.Name, action);
			}
		}
		NullAction = new CachedAction(LegacyShims.EmptyArray<string>(), ActionHandlers["Null"], null, isNullHandler: true);
		if (ActionsByTrigger.Count != 0)
		{
			return;
		}
		foreach (string triggerType in ValidTriggerTypes)
		{
			ActionsByTrigger[triggerType] = new List<CachedTriggerAction>();
		}
		HashSet<string> seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		List<CachedAction> actions = new List<CachedAction>();
		foreach (TriggerActionData data in DataLoader.TriggerActions(Game1.content))
		{
			if (string.IsNullOrWhiteSpace(data.Id))
			{
				Game1.log.Error("Trigger action has no ID field and will be ignored.");
				continue;
			}
			if (string.IsNullOrWhiteSpace(data.Trigger))
			{
				Game1.log.Error($"Trigger action '{data.Id}' has no trigger; expected one of '{string.Join("', '", ValidTriggerTypes)}'.");
				continue;
			}
			if (string.IsNullOrWhiteSpace(data.Action))
			{
				List<string> actions2 = data.Actions;
				if (actions2 == null || actions2.Count <= 0)
				{
					Game1.log.Error("Trigger action '" + data.Id + "' has no defined actions.");
					continue;
				}
			}
			if (!seenIds.Add(data.Id))
			{
				Game1.log.Error("Trigger action '" + data.Id + "' has a duplicate ID. Only the first instance will be used.");
				continue;
			}
			actions.Clear();
			if (data.Action != null)
			{
				CachedAction parsed = ParseAction(data.Action);
				if (parsed.Error != null)
				{
					Game1.log.Error($"Trigger action '{data.Id}' will skip invalid action '{data.Action}': {parsed.Error}.");
				}
				else if (!parsed.IsNullHandler)
				{
					actions.Add(parsed);
				}
			}
			if (data.Actions != null)
			{
				foreach (string action2 in data.Actions)
				{
					CachedAction parsed = ParseAction(action2);
					if (parsed.Error != null)
					{
						Game1.log.Error($"Trigger action '{data.Id}' will skip invalid action '{data.Action}': {parsed.Error}.");
					}
					else if (!parsed.IsNullHandler)
					{
						actions.Add(parsed);
					}
				}
			}
			CachedTriggerAction cachedTriggerAction = new CachedTriggerAction(data, actions.ToArray());
			string[] array = ArgUtility.SplitBySpace(data.Trigger);
			foreach (string trigger in array)
			{
				if (!ValidTriggerTypes.Contains(trigger))
				{
					Game1.log.Error($"Trigger action '{data.Id}' has unknown trigger '{trigger}'; expected one of '{string.Join("', '", ValidTriggerTypes)}'.");
				}
				else
				{
					ActionsByTrigger[trigger].Add(cachedTriggerAction);
				}
			}
		}
	}
}
