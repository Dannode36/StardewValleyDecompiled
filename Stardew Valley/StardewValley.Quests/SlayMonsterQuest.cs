using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Monsters;

namespace StardewValley.Quests;

public class SlayMonsterQuest : Quest
{
	public string targetMessage;

	[XmlElement("monsterName")]
	public readonly NetString monsterName = new NetString();

	[XmlElement("target")]
	public readonly NetString target = new NetString();

	[XmlElement("monster")]
	public readonly NetRef<Monster> monster = new NetRef<Monster>();

	[XmlElement("numberToKill")]
	public readonly NetInt numberToKill = new NetInt();

	[XmlElement("reward")]
	public readonly NetInt reward = new NetInt();

	[XmlElement("numberKilled")]
	public readonly NetInt numberKilled = new NetInt();

	public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

	public readonly NetDescriptionElementList dialogueparts = new NetDescriptionElementList();

	[XmlElement("objective")]
	public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

	public SlayMonsterQuest()
	{
		questType.Value = 4;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(parts, "parts").AddField(dialogueparts, "dialogueparts").AddField(objective, "objective")
			.AddField(monsterName, "monsterName")
			.AddField(target, "target")
			.AddField(monster, "monster")
			.AddField(numberToKill, "numberToKill")
			.AddField(reward, "reward")
			.AddField(numberKilled, "numberKilled");
	}

	public void loadQuestInfo()
	{
		for (int i = 0; i < random.Next(1, 100); i++)
		{
			random.Next();
		}
		if (target.Value != null && monster != null)
		{
			return;
		}
		base.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13696");
		List<string> possibleMonsters = new List<string>();
		int mineLevel = Utility.GetAllPlayerDeepestMineLevel();
		if (mineLevel < 39)
		{
			possibleMonsters.Add("Green Slime");
			if (mineLevel > 10)
			{
				possibleMonsters.Add("Rock Crab");
			}
			if (mineLevel > 30)
			{
				possibleMonsters.Add("Duggy");
			}
		}
		else if (mineLevel < 79)
		{
			possibleMonsters.Add("Frost Jelly");
			if (mineLevel > 70)
			{
				possibleMonsters.Add("Skeleton");
			}
			possibleMonsters.Add("Dust Spirit");
		}
		else
		{
			possibleMonsters.Add("Sludge");
			possibleMonsters.Add("Ghost");
			possibleMonsters.Add("Lava Crab");
			possibleMonsters.Add("Squid Kid");
		}
		int num;
		if (monsterName.Value != null)
		{
			num = ((numberToKill.Value == 0) ? 1 : 0);
			if (num == 0)
			{
				goto IL_0125;
			}
		}
		else
		{
			num = 1;
		}
		monsterName.Value = random.ChooseFrom(possibleMonsters);
		goto IL_0125;
		IL_0125:
		if (monsterName.Value == "Frost Jelly" || monsterName.Value == "Sludge")
		{
			monster.Value = new Monster("Green Slime", Vector2.Zero);
			monster.Value.Name = monsterName.Value;
		}
		else
		{
			monster.Value = new Monster(monsterName.Value, Vector2.Zero);
		}
		if (num != 0)
		{
			switch (monsterName.Value)
			{
			case "Green Slime":
				numberToKill.Value = random.Next(4, 11);
				numberToKill.Value = (int)numberToKill - (int)numberToKill % 2;
				reward.Value = (int)numberToKill * 60;
				break;
			case "Rock Crab":
				numberToKill.Value = random.Next(2, 6);
				reward.Value = (int)numberToKill * 75;
				break;
			case "Duggy":
				parts.Clear();
				parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13711", numberToKill.Value));
				target.Value = "Clint";
				numberToKill.Value = random.Next(2, 4);
				reward.Value = (int)numberToKill * 150;
				break;
			case "Frost Jelly":
				numberToKill.Value = random.Next(4, 11);
				numberToKill.Value = (int)numberToKill - (int)numberToKill % 2;
				reward.Value = (int)numberToKill * 85;
				break;
			case "Ghost":
				numberToKill.Value = random.Next(2, 4);
				reward.Value = (int)numberToKill * 250;
				break;
			case "Sludge":
				numberToKill.Value = random.Next(4, 11);
				numberToKill.Value = (int)numberToKill - (int)numberToKill % 2;
				reward.Value = (int)numberToKill * 125;
				break;
			case "Lava Crab":
				numberToKill.Value = random.Next(2, 6);
				reward.Value = (int)numberToKill * 180;
				break;
			case "Squid Kid":
				numberToKill.Value = random.Next(1, 3);
				reward.Value = (int)numberToKill * 350;
				break;
			case "Skeleton":
				numberToKill.Value = random.Next(6, 12);
				reward.Value = (int)numberToKill * 100;
				break;
			case "Dust Spirit":
				numberToKill.Value = random.Next(10, 21);
				reward.Value = (int)numberToKill * 60;
				break;
			default:
				numberToKill.Value = random.Next(3, 7);
				reward.Value = (int)numberToKill * 120;
				break;
			}
		}
		switch (monsterName.Value)
		{
		case "Green Slime":
		case "Frost Jelly":
		case "Sludge":
			parts.Clear();
			parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13723", numberToKill.Value, monsterName.Value.Equals("Frost Jelly") ? new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13725") : (monsterName.Value.Equals("Sludge") ? new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13727") : new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13728"))));
			target.Value = "Lewis";
			dialogueparts.Clear();
			dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13730");
			if (random.NextBool())
			{
				dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13731");
				dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs." + random.Choose("13732", "13733"));
				dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13734", new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs." + random.Choose("13735", "13736")), new DescriptionElement("Strings\\StringsFromCSFiles:Dialogue.cs." + random.Choose<string>("795", "796", "797", "798", "799", "800", "801", "802", "803", "804", "805", "806", "807", "808", "809", "810")), new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs." + random.Choose("13740", "13741", "13742"))));
			}
			else
			{
				dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13744");
			}
			break;
		case "Rock Crab":
		case "Lava Crab":
			parts.Clear();
			parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13747", numberToKill.Value));
			target.Value = "Demetrius";
			dialogueparts.Clear();
			dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13750", monster.Value));
			break;
		default:
			parts.Clear();
			parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13752", monster.Value, numberToKill.Value, new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs." + random.Choose("13755", "13756", "13757"))));
			target.Value = "Wizard";
			dialogueparts.Clear();
			dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13760");
			break;
		}
		if (target.Value.Equals("Wizard") && !Utility.doesAnyFarmerHaveMail("wizardJunimoNote") && !Utility.doesAnyFarmerHaveMail("JojaMember"))
		{
			parts.Clear();
			parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13764", numberToKill.Value, monster.Value));
			target.Value = "Lewis";
			dialogueparts.Clear();
			dialogueparts.Add("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13767");
		}
		parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13274", reward.Value));
		objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13770", "0", numberToKill.Value, monster.Value);
	}

	public override void reloadDescription()
	{
		if (_questDescription == "")
		{
			loadQuestInfo();
		}
		string descriptionBuilder = "";
		string messageBuilder = "";
		if (parts != null && parts.Count != 0)
		{
			foreach (DescriptionElement a in parts)
			{
				descriptionBuilder += a.loadDescriptionElement();
			}
			base.questDescription = descriptionBuilder;
		}
		if (dialogueparts != null && dialogueparts.Count != 0)
		{
			foreach (DescriptionElement b in dialogueparts)
			{
				messageBuilder += b.loadDescriptionElement();
			}
			targetMessage = messageBuilder;
		}
		else if (HasId())
		{
			string[] fields = Quest.GetRawQuestFields(id.Value);
			targetMessage = ArgUtility.Get(fields, 9, targetMessage, allowBlank: false);
		}
	}

	public override void reloadObjective()
	{
		if ((int)numberKilled != 0 || !HasId())
		{
			if ((int)numberKilled < (int)numberToKill)
			{
				objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SlayMonsterQuest.cs.13770", numberKilled.Value, numberToKill.Value, monster.Value);
			}
			if (objective.Value != null)
			{
				base.currentObjective = objective.Value.loadDescriptionElement();
			}
		}
	}

	private bool isSlimeName(string s)
	{
		if (s.Contains("Slime") || s.Contains("Jelly") || s.Contains("Sludge"))
		{
			return true;
		}
		return false;
	}

	public override bool checkIfComplete(NPC n = null, int number1 = -1, int number2 = -1, Item item = null, string monsterName = null, bool probe = false)
	{
		if ((bool)completed)
		{
			return false;
		}
		if (monsterName == null)
		{
			monsterName = "Green Slime";
		}
		if (n == null && (monsterName.Contains(this.monsterName.Value) || (id.Equals("15") && isSlimeName(monsterName))) && (int)numberKilled < (int)numberToKill)
		{
			if (!probe)
			{
				numberKilled.Value = Math.Min(numberToKill, (int)numberKilled + 1);
				Game1.dayTimeMoneyBox.pingQuest(this);
				if ((int)numberKilled >= (int)numberToKill)
				{
					if (target.Value == null || target.Value.Equals("null"))
					{
						questComplete();
					}
					else
					{
						NPC actualTarget = Game1.getCharacterFromName(target);
						objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13277", actualTarget);
						Game1.playSound("jingle1");
					}
				}
				else if (monster.Value == null)
				{
					if (monsterName == "Frost Jelly" || monsterName == "Sludge")
					{
						monster.Value = new Monster("Green Slime", Vector2.Zero);
						monster.Value.Name = monsterName;
					}
					else
					{
						monster.Value = new Monster(monsterName, Vector2.Zero);
					}
				}
			}
		}
		else if (n != null && target.Value != null && !target.Value.Equals("null") && (int)numberKilled >= (int)numberToKill && n.Name.Equals(target.Value) && n.IsVillager)
		{
			if (!probe)
			{
				reloadDescription();
				n.CurrentDialogue.Push(new Dialogue(n, null, targetMessage));
				moneyReward.Value = reward;
				questComplete();
				Game1.drawDialogue(n);
			}
			return true;
		}
		return false;
	}
}
