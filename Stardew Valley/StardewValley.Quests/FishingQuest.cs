using System;
using System.Xml.Serialization;
using Netcode;
using StardewValley.Extensions;

namespace StardewValley.Quests;

public class FishingQuest : Quest
{
	/// <summary>The internal name for the NPC who gave the quest.</summary>
	[XmlElement("target")]
	public readonly NetString target = new NetString();

	/// <summary>The translated text for the NPC dialogue shown when the quest is completed.</summary>
	public string targetMessage;

	/// <summary>The number of fish which must be caught.</summary>
	[XmlElement("numberToFish")]
	public readonly NetInt numberToFish = new NetInt();

	/// <summary>The gold reward for finishing the quest.</summary>
	[XmlElement("reward")]
	public readonly NetInt reward = new NetInt();

	/// <summary>The number of fish caught so far.</summary>
	[XmlElement("numberFished")]
	public readonly NetInt numberFished = new NetInt();

	/// <summary>The qualified item ID for the fish to catch.</summary>
	[XmlElement("whichFish")]
	public readonly NetString ItemId = new NetString();

	/// <summary>The translatable text segments for the quest description in the quest log.</summary>
	public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

	/// <summary>The translatable text segments for the NPC dialogue shown when the quest is completed.</summary>
	public readonly NetDescriptionElementList dialogueparts = new NetDescriptionElementList();

	/// <summary>The translatable text segments for the objective shown in the quest log (like "0/5 caught").</summary>
	[XmlElement("objective")]
	public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

	/// <summary>Construct an instance.</summary>
	public FishingQuest()
	{
		questType.Value = 7;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="itemId">The qualified item ID for the fish to catch.</param>
	/// <param name="numberToFish">The number of fish which must be caught.</param>
	/// <param name="target">The internal name for the NPC who gave the quest.</param>
	/// <param name="returnDialogue">The translated text for the NPC dialogue shown when the quest is completed.</param>
	public FishingQuest(string itemId, int numberToFish, string target, string questTitle, string questDescription, string returnDialogue)
		: this()
	{
		ItemId.Value = ItemRegistry.QualifyItemId(itemId);
		this.numberToFish.Value = numberToFish;
		this.target.Value = target;
		base.questDescription = questDescription;
		base.questTitle = questTitle;
		_loadedTitle = true;
		targetMessage = returnDialogue;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(parts, "parts").AddField(dialogueparts, "dialogueparts").AddField(objective, "objective")
			.AddField(target, "target")
			.AddField(numberToFish, "numberToFish")
			.AddField(reward, "reward")
			.AddField(numberFished, "numberFished")
			.AddField(ItemId, "ItemId");
	}

	public void loadQuestInfo()
	{
		if (target.Value != null && ItemId.Value != null)
		{
			return;
		}
		base.questTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingQuest.cs.13227");
		if (random.NextBool())
		{
			switch (Game1.season)
			{
			case Season.Spring:
				ItemId.Value = random.Choose<string>("(O)129", "(O)131", "(O)136", "(O)137", "(O)142", "(O)143", "(O)145", "(O)147");
				break;
			case Season.Summer:
				ItemId.Value = random.Choose<string>("(O)130", "(O)136", "(O)138", "(O)142", "(O)144", "(O)145", "(O)146", "(O)149", "(O)150");
				break;
			case Season.Fall:
				ItemId.Value = random.Choose<string>("(O)129", "(O)131", "(O)136", "(O)137", "(O)139", "(O)142", "(O)143", "(O)150");
				break;
			case Season.Winter:
				ItemId.Value = random.Choose<string>("(O)130", "(O)131", "(O)136", "(O)141", "(O)144", "(O)146", "(O)147", "(O)150", "(O)151");
				break;
			}
			Item fish = ItemRegistry.Create(ItemId.Value);
			bool isOctopus = ItemId.Value == "(O)149";
			numberToFish.Value = (int)Math.Ceiling(90.0 / (double)Math.Max(1, GetGoldRewardPerItem(fish))) + Game1.player.FishingLevel / 5;
			reward.Value = numberToFish.Value * GetGoldRewardPerItem(fish);
			target.Value = "Demetrius";
			parts.Clear();
			parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13228", fish, numberToFish.Value));
			dialogueparts.Clear();
			dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13231", fish, random.Choose(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13233"), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13234"), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13235"), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13236", fish))));
			objective.Value = (isOctopus ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13243", 0, numberToFish.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13244", 0, numberToFish.Value, fish));
		}
		else
		{
			switch (Game1.season)
			{
			case Season.Spring:
				ItemId.Value = random.Choose<string>("(O)129", "(O)131", "(O)136", "(O)137", "(O)142", "(O)143", "(O)145", "(O)147", "(O)702");
				break;
			case Season.Summer:
				ItemId.Value = random.Choose<string>("(O)128", "(O)130", "(O)136", "(O)138", "(O)142", "(O)144", "(O)145", "(O)146", "(O)149", "(O)150", "(O)702");
				break;
			case Season.Fall:
				ItemId.Value = random.Choose<string>("(O)129", "(O)131", "(O)136", "(O)137", "(O)139", "(O)142", "(O)143", "(O)150", "(O)699", "(O)702", "(O)705");
				break;
			case Season.Winter:
				ItemId.Value = random.Choose<string>("(O)130", "(O)131", "(O)136", "(O)141", "(O)143", "(O)144", "(O)146", "(O)147", "(O)151", "(O)699", "(O)702", "(O)705");
				break;
			}
			target.Value = "Willy";
			Item fish = ItemRegistry.Create(ItemId.Value);
			bool isSquid = ItemId.Value == "(O)151";
			numberToFish.Value = (int)Math.Ceiling(90.0 / (double)Math.Max(1, GetGoldRewardPerItem(fish))) + Game1.player.FishingLevel / 5;
			reward.Value = numberToFish.Value * GetGoldRewardPerItem(fish);
			parts.Clear();
			parts.Add(isSquid ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13248", reward.Value, numberToFish.Value, new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13253")) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13248", reward.Value, numberToFish.Value, fish));
			dialogueparts.Clear();
			dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13256", fish));
			dialogueparts.Add(random.Choose(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13258"), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13259"), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13260", new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs." + random.Choose<string>("13261", "13262", "13263", "13264", "13265", "13266"))), new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13267")));
			dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13268"));
			objective.Value = (isSquid ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13255", 0, numberToFish.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13244", 0, numberToFish.Value, fish));
		}
		parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13274", reward.Value));
		parts.Add("Strings\\StringsFromCSFiles:FishingQuest.cs.13275");
	}

	public override void reloadDescription()
	{
		if (_questDescription == "")
		{
			loadQuestInfo();
		}
		if (parts.Count == 0 || parts == null || dialogueparts.Count == 0 || dialogueparts == null)
		{
			return;
		}
		string descriptionBuilder = "";
		string messageBuilder = "";
		foreach (DescriptionElement a in parts)
		{
			descriptionBuilder += a.loadDescriptionElement();
		}
		foreach (DescriptionElement b in dialogueparts)
		{
			messageBuilder += b.loadDescriptionElement();
		}
		base.questDescription = descriptionBuilder;
		targetMessage = messageBuilder;
	}

	public override void reloadObjective()
	{
		bool isOctopus = ItemId.Value == "(O)149";
		bool isSquid = ItemId.Value == "(O)151";
		if ((int)numberFished < (int)numberToFish)
		{
			objective.Value = (isOctopus ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13243", numberFished.Value, numberToFish.Value) : (isSquid ? new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13255", numberFished.Value, numberToFish.Value) : new DescriptionElement("Strings\\StringsFromCSFiles:FishingQuest.cs.13244", numberFished.Value, numberToFish.Value, ItemRegistry.Create(ItemId.Value))));
		}
		if (objective.Value != null)
		{
			base.currentObjective = objective.Value.loadDescriptionElement();
		}
	}

	public override bool checkIfComplete(NPC n = null, int number1 = -1, int number2 = 1, Item item = null, string fishid = null, bool probe = false)
	{
		loadQuestInfo();
		if (n == null && fishid != null && ItemRegistry.QualifyItemId(fishid) == ItemId && (int)numberFished < (int)numberToFish)
		{
			if (!probe)
			{
				numberFished.Value = Math.Min(numberToFish, (int)numberFished + number2);
				Game1.dayTimeMoneyBox.pingQuest(this);
				if ((int)numberFished >= (int)numberToFish)
				{
					if (target.Value == null)
					{
						target.Value = "Willy";
					}
					NPC actualTarget = Game1.getCharacterFromName(target);
					objective.Value = new DescriptionElement("Strings\\Quests:ObjectiveReturnToNPC", actualTarget);
					Game1.playSound("jingle1");
				}
			}
		}
		else if (n != null && (int)numberFished >= (int)numberToFish && target.Value != null && n.Name.Equals(target.Value) && n.IsVillager && !completed)
		{
			if (!probe)
			{
				n.CurrentDialogue.Push(new Dialogue(n, null, targetMessage));
				moneyReward.Value = reward;
				questComplete();
				Game1.drawDialogue(n);
			}
			return true;
		}
		return false;
	}

	/// <summary>Get the gold reward for a given item.</summary>
	/// <param name="item">The item instance.</param>
	private int GetGoldRewardPerItem(Item item)
	{
		if (item is Object obj)
		{
			return obj.Price;
		}
		return (int)((float)item.salePrice() * 1.5f);
	}
}
