using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData;
using StardewValley.Menus;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class AdventureGuild : GameLocation
{
	public NPC Gil = new NPC(null, new Vector2(-1000f, -1000f), "AdventureGuild", 2, "Gil", datable: false, Game1.content.Load<Texture2D>("Portraits\\Gil"));

	public bool talkedToGil;

	public AdventureGuild()
	{
	}

	public AdventureGuild(string mapPath, string name)
		: base(mapPath, name)
	{
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		switch (getTileIndexAt(tileLocation, "Buildings"))
		{
		case 1306:
			showMonsterKillList();
			return true;
		case 1291:
		case 1292:
		case 1355:
		case 1356:
		case 1357:
		case 1358:
			gil();
			return true;
		default:
			return base.checkAction(tileLocation, viewport, who);
		}
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		talkedToGil = false;
		Game1.player.mailReceived.Add("guildMember");
		addOneTimeGiftBox(ItemRegistry.Create("(O)Book_Marlon"), 10, 4);
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (!Game1.player.mailReceived.Contains("checkedMonsterBoard"))
		{
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(504f, 464f + yOffset)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.064801f);
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(544f, 504f + yOffset)), new Microsoft.Xna.Framework.Rectangle(175, 425, 12, 12), Color.White * 0.75f, 0f, new Vector2(6f, 6f), 4f, SpriteEffects.None, 0.06481f);
		}
	}

	private string killListLine(string monsterNamePlural, int killCount, int target)
	{
		if (killCount == 0)
		{
			return Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_LineFormat_None", killCount, target, monsterNamePlural) + "^";
		}
		if (killCount >= target)
		{
			return Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_LineFormat_OverTarget", killCount, target, monsterNamePlural) + "^";
		}
		return Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_LineFormat", killCount, target, monsterNamePlural) + "^";
	}

	public void showMonsterKillList()
	{
		Game1.player.mailReceived.Add("checkedMonsterBoard");
		StringBuilder s = new StringBuilder();
		s.Append(Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_Header").Replace('\n', '^') + "^");
		foreach (MonsterSlayerQuestData questData in DataLoader.MonsterSlayerQuests(Game1.content).Values)
		{
			int count = 0;
			if (questData.Targets != null)
			{
				foreach (string targetType in questData.Targets)
				{
					count += Game1.stats.getMonstersKilled(targetType);
				}
			}
			s.Append(killListLine(TokenParser.ParseText(questData.DisplayName), count, questData.Count));
		}
		s.Append(Game1.content.LoadString("Strings\\Locations:AdventureGuild_KillList_Footer").Replace('\n', '^'));
		Game1.drawLetterMessage(s.ToString());
	}

	public static bool areAllMonsterSlayerQuestsComplete()
	{
		foreach (MonsterSlayerQuestData questData in DataLoader.MonsterSlayerQuests(Game1.content).Values)
		{
			int count = 0;
			if (questData.Targets == null)
			{
				continue;
			}
			foreach (string targetType in questData.Targets)
			{
				count += Game1.stats.getMonstersKilled(targetType);
				if (count >= questData.Count)
				{
					break;
				}
			}
			if (count < questData.Count)
			{
				return false;
			}
		}
		return true;
	}

	public static bool willThisKillCompleteAMonsterSlayerQuest(string nameOfMonster)
	{
		foreach (MonsterSlayerQuestData questData in DataLoader.MonsterSlayerQuests(Game1.content).Values)
		{
			if (!questData.Targets.Contains(nameOfMonster))
			{
				continue;
			}
			int count = 0;
			if (questData.Targets == null)
			{
				continue;
			}
			foreach (string targetType in questData.Targets)
			{
				count += Game1.stats.getMonstersKilled(targetType);
				if (count >= questData.Count)
				{
					break;
				}
			}
			if (count < questData.Count && count + 1 >= questData.Count)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Handle a reward item collected for a completed monster eradication goal.</summary>
	/// <param name="item">The item that was collected.</param>
	/// <param name="who">The player who collected the item.</param>
	/// <param name="completedGoals">The goals for which rewards are being collected.</param>
	public void OnRewardCollected(Item item, Farmer who, List<KeyValuePair<string, MonsterSlayerQuestData>> completedGoals)
	{
		if (item != null)
		{
			int goalIndex = item.SpecialVariable;
			if (goalIndex >= 0 && goalIndex < completedGoals.Count)
			{
				KeyValuePair<string, MonsterSlayerQuestData> goal = completedGoals[goalIndex];
				who.mailReceived.Add("Gil_" + goal.Key);
			}
		}
	}

	private void gil()
	{
		List<Item> rewards = new List<Item>();
		List<KeyValuePair<string, MonsterSlayerQuestData>> completedGoals = new List<KeyValuePair<string, MonsterSlayerQuestData>>();
		List<string> dialogues = new List<string>();
		foreach (KeyValuePair<string, MonsterSlayerQuestData> pair in DataLoader.MonsterSlayerQuests(Game1.content))
		{
			string id = pair.Key;
			MonsterSlayerQuestData questData = pair.Value;
			if (HasCollectedReward(Game1.player, id) || !IsComplete(questData))
			{
				continue;
			}
			completedGoals.Add(pair);
			if (questData.RewardItemId != null)
			{
				Item item = ItemRegistry.Create(questData.RewardItemId);
				item.SpecialVariable = completedGoals.Count - 1;
				if (item is Object obj)
				{
					obj.specialItem = true;
				}
				rewards.Add(item);
			}
			if (questData.RewardDialogue != null && (questData.RewardDialogueFlag == null || !Game1.player.mailReceived.Contains(questData.RewardDialogueFlag)))
			{
				dialogues.Add(TokenParser.ParseText(questData.RewardDialogue));
			}
			if (questData.RewardMail != null)
			{
				Game1.addMailForTomorrow(questData.RewardMail);
			}
			if (questData.RewardMailAll != null)
			{
				Game1.addMailForTomorrow(questData.RewardMailAll, noLetter: false, sendToEveryone: true);
			}
			if (questData.RewardFlag != null)
			{
				Game1.addMail(questData.RewardFlag, noLetter: true);
			}
			if (questData.RewardFlagAll != null)
			{
				Game1.addMail(questData.RewardFlagAll, noLetter: true, sendToEveryone: true);
			}
		}
		if (rewards.Count > 0 || dialogues.Count > 0)
		{
			if (dialogues.Count > 0)
			{
				Game1.DrawDialogue(new Dialogue(Gil, null, string.Join("#$b#", dialogues)));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
				{
					OpenRewardMenuIfNeeded(rewards, completedGoals);
				});
			}
			else
			{
				OpenRewardMenuIfNeeded(rewards, completedGoals);
			}
		}
		else
		{
			if (talkedToGil)
			{
				Game1.DrawDialogue(Gil, "Characters\\Dialogue\\Gil:Snoring");
			}
			else
			{
				Game1.DrawDialogue(Gil, "Characters\\Dialogue\\Gil:ComeBackLater");
			}
			talkedToGil = true;
		}
	}

	/// <summary>Get whether a player has collected the reward for a monster eradication goal.</summary>
	/// <param name="player">The player to check.</param>
	/// <param name="goalId">The monster eradication goal ID.</param>
	public static bool HasCollectedReward(Farmer player, string goalId)
	{
		return player.mailReceived.Contains("Gil_" + goalId);
	}

	/// <summary>Get whether a monster eradication goal has been completed, regardless of whether the player has collected the rewards yet.</summary>
	/// <param name="goal">The monster eradication goal data.</param>
	public static bool IsComplete(MonsterSlayerQuestData goal)
	{
		if (goal.Targets == null)
		{
			return true;
		}
		int count = 0;
		foreach (string targetType in goal.Targets)
		{
			count += Game1.stats.getMonstersKilled(targetType);
			if (count >= goal.Count)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Open a menu to collect rewards for completed goals, if any.</summary>
	/// <param name="rewards">The rewards to collect.</param>
	/// <param name="completedGoals">The goals for which rewards are being collected.</param>
	private void OpenRewardMenuIfNeeded(List<Item> rewards, List<KeyValuePair<string, MonsterSlayerQuestData>> completedGoals)
	{
		if (rewards.Count != 0)
		{
			Game1.activeClickableMenu = new ItemGrabMenu(rewards, this)
			{
				behaviorOnItemGrab = delegate(Item item, Farmer who)
				{
					OnRewardCollected(item, who, completedGoals);
				}
			};
		}
	}
}
