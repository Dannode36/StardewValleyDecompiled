using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TokenizableStrings;

namespace StardewValley.Objects;

/// <summary>Handles incoming and outgoing phone calls for the base game.</summary>
public class DefaultPhoneHandler : IPhoneHandler
{
	/// <summary>The call IDs for phone numbers the player can call.</summary>
	public static class OutgoingCallIds
	{
		/// <summary>An outgoing call to the Adventurer's Guild.</summary>
		public const string AdventureGuild = "AdventureGuild";

		/// <summary>An outgoing call to Marnie's animal shop.</summary>
		public const string AnimalShop = "AnimalShop";

		/// <summary>An outgoing call to Clint's blacksmith shop.</summary>
		public const string Blacksmith = "Blacksmith";

		/// <summary>An outgoing call to Robin's shop.</summary>
		public const string Carpenter = "Carpenter";

		/// <summary>An outgoing call to Gus' Saloon.</summary>
		public const string Saloon = "Saloon";

		/// <summary>An outgoing call to Pierre's shop.</summary>
		public const string SeedShop = "SeedShop";
	}

	/// <inheritdoc />
	public string CheckForIncomingCall(Random random)
	{
		List<string> validCalls = new List<string>();
		bool baseChancePassed = random.NextDouble() < 0.01;
		foreach (KeyValuePair<string, IncomingPhoneCallData> entry in DataLoader.IncomingPhoneCalls(Game1.content))
		{
			if ((baseChancePassed || entry.Value.IgnoreBaseChance) && (entry.Value.TriggerCondition == null || GameStateQuery.CheckConditions(entry.Value.TriggerCondition, Game1.currentLocation, Game1.player, null, null, random)))
			{
				validCalls.Add(entry.Key);
			}
		}
		return random.ChooseFrom(validCalls);
	}

	/// <inheritdoc />
	public bool TryHandleIncomingCall(string callId, out Action showDialogue)
	{
		showDialogue = null;
		if (!DataLoader.IncomingPhoneCalls(Game1.content).TryGetValue(callId, out var call))
		{
			return false;
		}
		if (call.MaxCalls > -1 && Game1.player.callsReceived.TryGetValue(callId, out var previousCalls) && previousCalls >= call.MaxCalls)
		{
			return false;
		}
		if (call.RingCondition != null && !GameStateQuery.CheckConditions(call.RingCondition, Game1.currentLocation, Game1.player))
		{
			return false;
		}
		if (Game1.IsGreenRainingHere())
		{
			return false;
		}
		showDialogue = delegate
		{
			if (!string.IsNullOrWhiteSpace(call.SimpleDialogueSplitBy))
			{
				Game1.multipleDialogues((TokenParser.ParseText(call.Dialogue) ?? "...").Split(call.SimpleDialogueSplitBy));
			}
			else
			{
				NPC nPC = null;
				if (call.FromNpc != null)
				{
					nPC = Game1.getCharacterFromName(call.FromNpc);
					if (nPC == null)
					{
						Game1.log.Warn($"Can't find NPC '{call.FromNpc}' for incoming call ID '{callId}'.");
					}
				}
				string text = TokenParser.ParseText(call.FromDisplayName);
				Texture2D texture2D = null;
				if (call.FromPortrait != null)
				{
					if (!Game1.content.DoesAssetExist<Texture2D>(call.FromPortrait))
					{
						Game1.log.Warn($"Can't load custom portrait '{call.FromPortrait}' for incoming call ID '{callId}' because that texture doesn't exist.");
					}
					else
					{
						texture2D = Game1.content.Load<Texture2D>(call.FromPortrait);
					}
				}
				if (texture2D != null || text != null)
				{
					if (nPC != null)
					{
						nPC = new NPC(nPC.Sprite, Vector2.Zero, "", 0, nPC.Name, texture2D ?? nPC.Portrait, eventActor: false);
						nPC.displayName = text ?? nPC.displayName;
					}
					else if (texture2D != null)
					{
						nPC = new NPC(new AnimatedSprite("Characters\\Abigail", 0, 16, 16), Vector2.Zero, "", 0, "???", texture2D, eventActor: false)
						{
							displayName = (text ?? "???")
						};
					}
				}
				string translationKey = "Data\\IncomingPhoneCalls:" + callId;
				string dialogueText = TokenParser.ParseText(call.Dialogue) ?? "...";
				Game1.DrawDialogue(new Dialogue(nPC, translationKey, dialogueText));
			}
		};
		return true;
	}

	/// <inheritdoc />
	public IEnumerable<KeyValuePair<string, string>> GetOutgoingNumbers()
	{
		List<KeyValuePair<string, string>> numbers = new List<KeyValuePair<string, string>>
		{
			new KeyValuePair<string, string>("Carpenter", Game1.RequireCharacter("Robin").displayName),
			new KeyValuePair<string, string>("Blacksmith", Game1.RequireCharacter("Clint").displayName),
			new KeyValuePair<string, string>("SeedShop", Game1.RequireCharacter("Pierre").displayName),
			new KeyValuePair<string, string>("AnimalShop", Game1.RequireCharacter("Marnie").displayName),
			new KeyValuePair<string, string>("Saloon", Game1.RequireCharacter("Gus").displayName)
		};
		if (Game1.player.mailReceived.Contains("Gil_Telephone") || Game1.player.mailReceived.Contains("Gil_FlameSpirits"))
		{
			numbers.Add(new KeyValuePair<string, string>("AdventureGuild", Game1.RequireCharacter("Marlon").displayName));
		}
		return numbers;
	}

	/// <inheritdoc />
	public bool TryHandleOutgoingCall(string callId)
	{
		switch (callId)
		{
		case "AdventureGuild":
			CallAdventureGuild();
			return true;
		case "AnimalShop":
			CallAnimalShop();
			return true;
		case "Blacksmith":
			CallBlacksmith();
			return true;
		case "Carpenter":
			CallCarpenter();
			return true;
		case "Saloon":
			CallSaloon();
			return true;
		case "SeedShop":
			CallSeedShop();
			return true;
		default:
			return false;
		}
	}

	/// <summary>Handle an outgoing call to the Adventurer's Guild.</summary>
	public void CallAdventureGuild()
	{
		Game1.currentLocation.playShopPhoneNumberSounds("AdventureGuild");
		Game1.player.freezePause = 4950;
		DelayedAction.functionAfterDelay(delegate
		{
			Game1.playSound("bigSelect");
			NPC character = Game1.getCharacterFromName("Marlon");
			if (Game1.player.mailForTomorrow.Contains("MarlonRecovery"))
			{
				Game1.DrawDialogue(character, "Strings\\Characters:Phone_Marlon_AlreadyRecovering");
			}
			else
			{
				Game1.DrawDialogue(character, "Strings\\Characters:Phone_Marlon_Open");
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
				{
					if (Game1.player.itemsLostLastDeath.Count > 0)
					{
						Game1.player.forceCanMove();
						Utility.TryOpenShopMenu("AdventureGuildRecovery", "Marlon");
					}
					else
					{
						Game1.DrawDialogue(character, "Strings\\Characters:Phone_Marlon_NoDeathItems");
					}
				});
			}
		}, 4950);
	}

	/// <summary>Handle an outgoing call to Marnie's animal shop.</summary>
	public void CallAnimalShop()
	{
		GameLocation location = Game1.currentLocation;
		location.playShopPhoneNumberSounds("AnimalShop");
		Game1.player.freezePause = 4950;
		DelayedAction.functionAfterDelay(delegate
		{
			Game1.playSound("bigSelect");
			NPC characterFromName = Game1.getCharacterFromName("Marnie");
			if (GameLocation.AreStoresClosedForFestival())
			{
				Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Marnie_ClosedDay");
			}
			else if (characterFromName.ScheduleKey == "fall_18" || characterFromName.ScheduleKey == "winter_18" || characterFromName.ScheduleKey == "Tue" || characterFromName.ScheduleKey == "Mon")
			{
				Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Marnie_ClosedDay");
			}
			else if (Game1.timeOfDay >= 900 && Game1.timeOfDay < 1600)
			{
				Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Marnie_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""));
			}
			else
			{
				Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Marnie_Closed");
			}
			Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
			{
				Response[] answerChoices = new Response[2]
				{
					new Response("AnimalShop_CheckAnimalPrices", Game1.content.LoadString("Strings\\Characters:Phone_CheckAnimalPrices")),
					new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp"))
				};
				location.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), answerChoices, "telephone");
			});
		}, 4950);
	}

	/// <summary>Handle an outgoing call to Clint's blacksmith shop.</summary>
	public void CallBlacksmith()
	{
		GameLocation location = Game1.currentLocation;
		location.playShopPhoneNumberSounds("Blacksmith");
		Game1.player.freezePause = 4950;
		DelayedAction.functionAfterDelay(delegate
		{
			Game1.playSound("bigSelect");
			NPC characterFromName = Game1.getCharacterFromName("Clint");
			if (GameLocation.AreStoresClosedForFestival())
			{
				Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Festival");
			}
			else if (Game1.player.daysLeftForToolUpgrade.Value > 0)
			{
				int value = Game1.player.daysLeftForToolUpgrade.Value;
				if (value == 1)
				{
					Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Working_OneDay");
				}
				else
				{
					Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Working", value);
				}
			}
			else
			{
				string scheduleKey = characterFromName.ScheduleKey;
				if (!(scheduleKey == "winter_16"))
				{
					if (scheduleKey == "Fri")
					{
						Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Festival");
					}
					else if (Game1.timeOfDay >= 900 && Game1.timeOfDay < 1600)
					{
						Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""));
					}
					else
					{
						Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Closed");
					}
				}
				else
				{
					Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Clint_Festival");
				}
			}
			Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
			{
				Response[] answerChoices = new Response[2]
				{
					new Response("Blacksmith_UpgradeCost", Game1.content.LoadString("Strings\\Characters:Phone_CheckToolCost")),
					new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp"))
				};
				location.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), answerChoices, "telephone");
			});
		}, 4950);
	}

	/// <summary>Handle an outgoing call to Robin's shop.</summary>
	public void CallCarpenter()
	{
		GameLocation location = Game1.currentLocation;
		location.playShopPhoneNumberSounds("Carpenter");
		Game1.player.freezePause = 4950;
		DelayedAction.functionAfterDelay(delegate
		{
			Game1.playSound("bigSelect");
			NPC characterFromName = Game1.getCharacterFromName("Robin");
			if (GameLocation.AreStoresClosedForFestival())
			{
				Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Robin_Festival");
			}
			else if (Game1.getLocationFromName("Town") is Town town && town.daysUntilCommunityUpgrade.Value > 0)
			{
				int value = town.daysUntilCommunityUpgrade.Value;
				if (value == 1)
				{
					Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Robin_Working_OneDay");
				}
				else
				{
					Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Robin_Working", value);
				}
			}
			else if (Game1.IsThereABuildingUnderConstruction())
			{
				BuilderData builderData = Game1.netWorldState.Value.GetBuilderData("Robin");
				int num = 0;
				if (builderData != null)
				{
					num = builderData.daysUntilBuilt.Value;
				}
				if (num == 1)
				{
					Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Robin_Working_OneDay");
				}
				else
				{
					Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Robin_Working", num);
				}
			}
			else
			{
				string scheduleKey = characterFromName.ScheduleKey;
				if (!(scheduleKey == "summer_18"))
				{
					if (scheduleKey == "Tue")
					{
						Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Robin_Workout");
					}
					else if (Game1.timeOfDay >= 900 && Game1.timeOfDay < 1700)
					{
						Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Robin_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""));
					}
					else
					{
						Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Robin_Closed");
					}
				}
				else
				{
					Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Robin_Festival");
				}
			}
			Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
			{
				List<Response> list = new List<Response>
				{
					new Response("Carpenter_ShopStock", Game1.content.LoadString("Strings\\Characters:Phone_CheckSeedStock"))
				};
				if ((int)Game1.player.houseUpgradeLevel < 3)
				{
					list.Add(new Response("Carpenter_HouseCost", Game1.content.LoadString("Strings\\Characters:Phone_CheckHouseCost")));
				}
				list.Add(new Response("Carpenter_BuildingCost", Game1.content.LoadString("Strings\\Characters:Phone_CheckBuildingCost")));
				list.Add(new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp")));
				location.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), list.ToArray(), "telephone");
			});
		}, 4950);
	}

	/// <summary>Handle an outgoing call to Gus' saloon.</summary>
	public void CallSaloon()
	{
		GameLocation location = Game1.currentLocation;
		location.playShopPhoneNumberSounds("Saloon");
		Game1.player.freezePause = 4950;
		DelayedAction.functionAfterDelay(delegate
		{
			Game1.playSound("bigSelect");
			NPC characterFromName = Game1.getCharacterFromName("Gus");
			if (GameLocation.AreStoresClosedForFestival())
			{
				Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Gus_Festival");
			}
			else if (Game1.timeOfDay >= 1200 && Game1.timeOfDay < 2400 && (characterFromName.ScheduleKey != "fall_4" || Game1.timeOfDay >= 1700))
			{
				if (Game1.dishOfTheDay != null)
				{
					Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Gus_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""), Game1.dishOfTheDay.DisplayName);
				}
				else
				{
					Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Gus_Open_NoDishOfTheDay");
				}
			}
			else if (Game1.dishOfTheDay != null && Game1.timeOfDay < 2400)
			{
				Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Gus_Closed", Game1.dishOfTheDay.DisplayName);
			}
			else
			{
				Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Gus_Closed_NoDishOfTheDay");
			}
			location.answerDialogueAction("HangUp", LegacyShims.EmptyArray<string>());
		}, 4950);
	}

	/// <summary>Handle an outgoing call to Pierre's shop.</summary>
	public void CallSeedShop()
	{
		GameLocation location = Game1.currentLocation;
		location.playShopPhoneNumberSounds("SeedShop");
		Game1.player.freezePause = 4950;
		DelayedAction.functionAfterDelay(delegate
		{
			Game1.playSound("bigSelect");
			NPC characterFromName = Game1.getCharacterFromName("Pierre");
			string text = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
			if (GameLocation.AreStoresClosedForFestival())
			{
				Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Pierre_Festival");
			}
			else if ((Game1.isLocationAccessible("CommunityCenter") || text != "Wed") && Game1.timeOfDay >= 900 && Game1.timeOfDay < 1700)
			{
				Game1.DrawDialogue(characterFromName, "Strings\\Characters:Phone_Pierre_Open" + ((Game1.random.NextDouble() < 0.01) ? "_Rare" : ""));
			}
			else
			{
				Game1.DrawAnsweringMachineDialogue(characterFromName, "Strings\\Characters:Phone_Pierre_Closed");
			}
			Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
			{
				Response[] answerChoices = new Response[2]
				{
					new Response("SeedShop_CheckSeedStock", Game1.content.LoadString("Strings\\Characters:Phone_CheckSeedStock")),
					new Response("HangUp", Game1.content.LoadString("Strings\\Characters:Phone_HangUp"))
				};
				location.createQuestionDialogue(Game1.content.LoadString("Strings\\Characters:Phone_SelectOption"), answerChoices, "telephone");
			});
		}, 4950);
	}
}
