using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;

namespace StardewValley.Quests;

public class SocializeQuest : Quest
{
	public readonly NetStringList whoToGreet = new NetStringList();

	[XmlElement("total")]
	public readonly NetInt total = new NetInt();

	public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

	[XmlElement("objective")]
	public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

	public SocializeQuest()
	{
		questType.Value = 5;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(whoToGreet, "whoToGreet").AddField(total, "total").AddField(parts, "parts")
			.AddField(objective, "objective");
	}

	public void loadQuestInfo()
	{
		if (whoToGreet.Count > 0)
		{
			return;
		}
		base.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:SocializeQuest.cs.13785");
		parts.Clear();
		parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13786", new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs." + random.Choose("13787", "13788", "13789"))));
		parts.Add("Strings\\StringsFromCSFiles:SocializeQuest.cs.13791");
		int curTotal = 0;
		foreach (KeyValuePair<string, CharacterData> entry in Game1.characterData)
		{
			string name = entry.Key;
			CharacterData data = entry.Value;
			if (data.IntroductionsQuest ?? (data.HomeRegion == "Town"))
			{
				curTotal++;
				if (data.SocialTab != SocialTabBehavior.AlwaysShown || (bool)dailyQuest)
				{
					whoToGreet.Add(name);
				}
			}
		}
		total.Value = curTotal;
		objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13802", (int)total - whoToGreet.Count, total.Value);
	}

	public override void reloadDescription()
	{
		if (_questDescription == "")
		{
			loadQuestInfo();
		}
		if (parts.Count == 0 || parts == null)
		{
			return;
		}
		string descriptionBuilder = "";
		foreach (DescriptionElement a in parts)
		{
			descriptionBuilder += a.loadDescriptionElement();
		}
		base.questDescription = descriptionBuilder;
	}

	public override void reloadObjective()
	{
		loadQuestInfo();
		if (objective.Value == null && whoToGreet.Count > 0)
		{
			objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13802", (int)total - whoToGreet.Count, total.Value);
		}
		if (objective.Value != null)
		{
			base.currentObjective = objective.Value.loadDescriptionElement();
		}
	}

	public override bool checkIfComplete(NPC npc = null, int number1 = -1, int number2 = -1, Item item = null, string monsterName = null, bool probe = false)
	{
		loadQuestInfo();
		if (npc != null && !probe && whoToGreet.Remove(npc.Name))
		{
			Game1.dayTimeMoneyBox.moneyDial.animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(387, 497, 3, 8), 800f, 1, 0, Game1.dayTimeMoneyBox.position + new Vector2(228f, 244f), flicker: false, flipped: false, 1f, 0.01f, Color.White, 4f, 0.3f, 0f, 0f)
			{
				scaleChangeChange = -0.012f
			});
			Game1.dayTimeMoneyBox.pingQuest(this);
		}
		if (whoToGreet.Count == 0 && !completed)
		{
			if (!probe)
			{
				foreach (string s in Game1.player.friendshipData.Keys)
				{
					if (Game1.player.friendshipData[s].Points < 2729)
					{
						Game1.player.changeFriendship(100, Game1.getCharacterFromName(s));
					}
				}
				questComplete();
			}
			return true;
		}
		if (!probe)
		{
			objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:SocializeQuest.cs.13802", (int)total - whoToGreet.Count, total.Value);
		}
		return false;
	}
}
