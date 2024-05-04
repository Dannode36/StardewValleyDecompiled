using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData.Characters;
using StardewValley.Network;

namespace StardewValley.Quests;

public class ItemDeliveryQuest : Quest
{
	/// <summary>The translated NPC dialogue shown when the quest is completed.</summary>
	public string targetMessage;

	/// <summary>The internal name for the NPC who gave the quest.</summary>
	[XmlElement("target")]
	public readonly NetString target = new NetString();

	/// <summary>The qualified item ID that must be delivered.</summary>
	[XmlElement("item")]
	public readonly NetString ItemId = new NetString();

	/// <summary>The number of items that must be delivered.</summary>
	[XmlElement("number")]
	public readonly NetInt number = new NetInt(1);

	/// <summary>The translatable text segments for the quest description shown in the quest log.</summary>
	public readonly NetDescriptionElementList parts = new NetDescriptionElementList();

	/// <summary>The translatable text segments for the <see cref="F:StardewValley.Quests.ItemDeliveryQuest.targetMessage" />.</summary>
	public readonly NetDescriptionElementList dialogueparts = new NetDescriptionElementList();

	/// <summary>The translatable text segments for the objective shown in the quest log (like "0/5 caught").</summary>
	[XmlElement("objective")]
	public readonly NetDescriptionElementRef objective = new NetDescriptionElementRef();

	/// <summary>Construct an instance.</summary>
	public ItemDeliveryQuest()
	{
		questType.Value = 3;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="target">The internal name for the NPC who gave the quest.</param>
	/// <param name="itemId">The qualified or unqualified item ID that must be delivered.</param>
	public ItemDeliveryQuest(string target, string itemId)
		: this()
	{
		this.target.Value = target;
		ItemId.Value = ItemRegistry.QualifyItemId(itemId) ?? itemId;
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="target">The internal name for the NPC who gave the quest.</param>
	/// <param name="itemId">The qualified or unqualified item ID that must be delivered.</param>
	/// <param name="objective">The translatable text segments for the objective shown in the quest log (like "0/5 caught").</param>
	/// <param name="returnDialogue">The translated NPC dialogue shown when the quest is completed.</param>
	public ItemDeliveryQuest(string target, string itemId, string questTitle, string questDescription, string objective, string returnDialogue)
		: this(target, itemId)
	{
		base.questDescription = questDescription;
		base.questTitle = questTitle;
		_loadedTitle = true;
		targetMessage = returnDialogue;
		this.objective = new NetDescriptionElementRef(new DescriptionElement(objective));
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(target, "target").AddField(ItemId, "ItemId").AddField(number, "number")
			.AddField(parts, "parts")
			.AddField(dialogueparts, "dialogueparts")
			.AddField(objective, "objective");
	}

	public List<NPC> GetValidTargetList()
	{
		Farmer[] source = Game1.getAllFarmers().ToArray();
		HashSet<string> friendshipKeys = new HashSet<string>(source.SelectMany((Farmer player) => player.friendshipData.Keys));
		HashSet<string> spouses = new HashSet<string>(source.Select((Farmer p) => p.spouse));
		List<NPC> validTargets = new List<NPC>();
		foreach (KeyValuePair<string, CharacterData> pair in Game1.characterData)
		{
			CharacterData data = pair.Value;
			if (GameStateQuery.CheckConditions(data.CanSocialize) && ((data.ItemDeliveryQuests != null) ? GameStateQuery.CheckConditions(data.ItemDeliveryQuests) : (data.HomeRegion == "Town")) && friendshipKeys.Contains(pair.Key) && !spouses.Contains(pair.Key) && pair.Value.Age != NpcAge.Child)
			{
				NPC npc = Game1.getCharacterFromName(pair.Key);
				if (npc != null && !npc.IsInvisible)
				{
					validTargets.Add(npc);
				}
			}
		}
		return validTargets;
	}

	public void loadQuestInfo()
	{
		if (target.Value != null)
		{
			return;
		}
		List<NPC> valid_targets = GetValidTargetList();
		NetStringDictionary<Friendship, NetRef<Friendship>> friendshipData = Game1.player.friendshipData;
		if (friendshipData == null || friendshipData.Length <= 0 || valid_targets.Count <= 0)
		{
			return;
		}
		NPC actualTarget = valid_targets[random.Next(valid_targets.Count)];
		if (actualTarget == null)
		{
			return;
		}
		target.Value = actualTarget.name;
		if (target.Value.Equals("Wizard") && !Game1.player.mailReceived.Contains("wizardJunimoNote") && !Game1.player.mailReceived.Contains("JojaMember"))
		{
			target.Value = "Demetrius";
			actualTarget = Game1.getCharacterFromName(target.Value);
		}
		base.questTitle = Game1.content.LoadString("Strings\\1_6_Strings:ItemDeliveryQuestTitle", NPC.GetDisplayName(target.Value));
		Item item;
		if (Game1.season != Season.Winter && random.NextDouble() < 0.15)
		{
			ItemId.Value = random.ChooseFrom(Utility.possibleCropsAtThisTime(Game1.season, Game1.dayOfMonth <= 7));
			ItemId.Value = ItemRegistry.QualifyItemId(ItemId.Value) ?? ItemId.Value;
			item = ItemRegistry.Create(ItemId.Value);
			if (dailyQuest.Value || moneyReward.Value == 0)
			{
				moneyReward.Value = GetGoldRewardPerItem(item);
			}
			switch (target.Value)
			{
			case "Demetrius":
				parts.Clear();
				parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13311", "13314"), item));
				break;
			case "Marnie":
				parts.Clear();
				parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13317", "13320"), item));
				break;
			case "Sebastian":
				parts.Clear();
				parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13324", "13327"), item));
				break;
			default:
				parts.Clear();
				parts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13299", "13300", "13301"));
				parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13302", "13303", "13304"), item));
				parts.Add(random.Choose("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13306", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13307", "", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13308"));
				parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget));
				break;
			}
		}
		else
		{
			string rawId = Utility.getRandomItemFromSeason(Game1.season, 1000, forQuest: true);
			if (!(rawId == "-5"))
			{
				if (rawId == "-6")
				{
					ItemId.Value = "(O)184";
				}
				else
				{
					ItemId.Value = ItemRegistry.QualifyItemId(rawId) ?? rawId;
				}
			}
			else
			{
				ItemId.Value = "(O)176";
			}
			item = ItemRegistry.Create(ItemId.Value);
			if (dailyQuest.Value || moneyReward.Value == 0)
			{
				moneyReward.Value = GetGoldRewardPerItem(item);
			}
			DescriptionElement[] questDescriptions = null;
			DescriptionElement[] questDescriptions2 = null;
			DescriptionElement[] questDescriptions3 = null;
			if ((item as Object)?.Type == "Cooking" && target.Value != "Wizard")
			{
				if (random.NextDouble() < 0.33)
				{
					DescriptionElement[] questStrings = new DescriptionElement[12]
					{
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13336"),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13337"),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13338"),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13339"),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13340"),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13341"),
						(!(Game1.samBandName == Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156"))) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13347", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2156")) : ((Game1.elliottBookName != Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2157")) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13342", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2157")) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13346")),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13349"),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13350"),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13351"),
						(Game1.season == Season.Winter) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13353") : ((Game1.season == Season.Summer) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13355") : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13356")),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13357")
					};
					parts.Clear();
					parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13333", "13334"), item, random.ChooseFrom(questStrings)));
					parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget));
				}
				else
				{
					DescriptionElement day = (Game1.dayOfMonth % 7) switch
					{
						0 => new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3042"), 
						1 => new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3043"), 
						2 => new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3044"), 
						3 => new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3045"), 
						4 => new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3046"), 
						5 => new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3047"), 
						_ => new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.3048"), 
					};
					questDescriptions = new DescriptionElement[5]
					{
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13360", item),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13364", item),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13367", item),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13370", item),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13373", day, item, actualTarget)
					};
					questDescriptions2 = new DescriptionElement[5]
					{
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
						new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
						new DescriptionElement("")
					};
					questDescriptions3 = new DescriptionElement[5]
					{
						new DescriptionElement(""),
						new DescriptionElement(""),
						new DescriptionElement(""),
						new DescriptionElement(""),
						new DescriptionElement("")
					};
				}
				parts.Clear();
				int rand = random.Next(questDescriptions.Length);
				parts.Add(questDescriptions[rand]);
				parts.Add(questDescriptions2[rand]);
				parts.Add(questDescriptions3[rand]);
				if (target.Value.Equals("Sebastian"))
				{
					parts.Clear();
					parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13378", "13381"), item));
				}
			}
			else
			{
				int rand;
				if (random.NextBool())
				{
					Object obj = item as Object;
					if (obj != null && obj.Edibility > 0)
					{
						questDescriptions = new DescriptionElement[1]
						{
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13383", item, new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose<string>("13385", "13386", "13387", "13388", "13389", "13390", "13391", "13392", "13393", "13394", "13395", "13396")), new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13400", item))
						};
						questDescriptions2 = new DescriptionElement[2]
						{
							new DescriptionElement(random.Choose("", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13398")),
							new DescriptionElement(random.Choose("", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13402"))
						};
						questDescriptions3 = new DescriptionElement[2]
						{
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
							new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget)
						};
						if (random.NextDouble() < 0.33)
						{
							DescriptionElement[] questStrings = new DescriptionElement[12]
							{
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13336"),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13337"),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13338"),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13339"),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13340"),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13341"),
								(!(Game1.samBandName == Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2156"))) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13347", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2156")) : ((Game1.elliottBookName != Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.2157")) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13342", new DescriptionElement("Strings\\StringsFromCSFiles:Game1.cs.2157")) : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13346")),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13420"),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13421"),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13422"),
								(Game1.season == Season.Winter) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13424") : ((Game1.season == Season.Summer) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13426") : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13427")),
								new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13357")
							};
							parts.Clear();
							parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13333", "13334"), item, random.ChooseFrom(questStrings)));
							parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget));
						}
						else
						{
							parts.Clear();
							rand = random.Next(questDescriptions.Length);
							parts.Add(questDescriptions[rand]);
							parts.Add(questDescriptions2[rand]);
							parts.Add(questDescriptions3[rand]);
						}
						switch (target.Value)
						{
						case "Demetrius":
							parts.Clear();
							parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13311", "13314"), item));
							break;
						case "Marnie":
							parts.Clear();
							parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13317", "13320"), item));
							break;
						case "Harvey":
							parts.Clear();
							parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13446", item, new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose<string>("13448", "13449", "13450", "13451", "13452", "13453", "13454", "13455", "13456", "13457", "13458", "13459"))));
							break;
						case "Gus":
							if (random.NextDouble() < 0.6)
							{
								parts.Clear();
								parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13462", item));
							}
							break;
						}
						goto IL_13f8;
					}
				}
				if (random.NextBool())
				{
					Object obj2 = item as Object;
					if (obj2 == null || obj2.Edibility < 0)
					{
						parts.Clear();
						parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13464", item, new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose<string>("13465", "13466", "13467", "13468", "13469"))));
						parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget));
						if (target.Value.Equals("Emily"))
						{
							parts.Clear();
							parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13473", "13476"), item));
						}
						goto IL_13f8;
					}
				}
				questDescriptions = new DescriptionElement[9]
				{
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13480", actualTarget, item),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13481", item),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13485", item),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13491", "13492"), item),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13494", item),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13497", item),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13500", item, new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose<string>("13502", "13503", "13504", "13505", "13506", "13507", "13508", "13509", "13510", "13511", "13512", "13513"))),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13518", actualTarget, item),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13520", "13523"), item)
				};
				questDescriptions2 = new DescriptionElement[9]
				{
					new DescriptionElement(""),
					new DescriptionElement(random.Choose("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13482", "", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13483")),
					new DescriptionElement(random.Choose("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13487", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13488", "", "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13489")),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13514", "13516")),
					new DescriptionElement(""),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget)
				};
				questDescriptions3 = new DescriptionElement[9]
				{
					new DescriptionElement(""),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
					new DescriptionElement(""),
					new DescriptionElement(""),
					new DescriptionElement(""),
					new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13620", actualTarget),
					new DescriptionElement(""),
					new DescriptionElement("")
				};
				parts.Clear();
				rand = random.Next(questDescriptions.Length);
				parts.Add(questDescriptions[rand]);
				parts.Add(questDescriptions2[rand]);
				parts.Add(questDescriptions3[rand]);
			}
		}
		goto IL_13f8;
		IL_13f8:
		dialogueparts.Clear();
		dialogueparts.Add((random.NextBool(0.3) || target.Value == "Evelyn") ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13526") : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13527", "13528")));
		dialogueparts.Add(random.NextBool(0.3) ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13530", item) : (random.NextBool() ? new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13532") : new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13534", "13535", "13536"))));
		dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13538", "13539", "13540"));
		dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13542", "13543", "13544"));
		switch (target.Value)
		{
		case "Wizard":
			parts.Clear();
			parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13546", "13548", "13551", "13553"), item));
			dialogueparts.Clear();
			dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13555");
			break;
		case "Haley":
			parts.Clear();
			parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13557", "13560"), item));
			dialogueparts.Clear();
			dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13566");
			break;
		case "Sam":
			parts.Clear();
			parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13568", "13571"), item));
			dialogueparts.Clear();
			dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13577"));
			break;
		case "Maru":
		{
			bool rand = random.NextBool();
			parts.Clear();
			parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + (rand ? "13580" : "13583"), item));
			dialogueparts.Clear();
			dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + (rand ? "13585" : "13587")));
			break;
		}
		case "Abigail":
		{
			bool rand = random.NextBool();
			parts.Clear();
			parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + (rand ? "13590" : "13593"), item));
			dialogueparts.Clear();
			dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + (rand ? "13597" : "13599")));
			break;
		}
		case "Sebastian":
			dialogueparts.Clear();
			dialogueparts.Add("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13602");
			break;
		case "Elliott":
			dialogueparts.Clear();
			dialogueparts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13604", item));
			break;
		}
		DescriptionElement lastPart = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs." + random.Choose("13608", "13610", "13612"), actualTarget);
		parts.Add(new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13607", moneyReward.Value));
		parts.Add(lastPart);
		objective.Value = new DescriptionElement("Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13614", actualTarget, item);
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
		if (objective.Value != null)
		{
			base.currentObjective = objective.Value.loadDescriptionElement();
		}
	}

	public override bool checkIfComplete(NPC n = null, int number1 = -1, int number2 = -1, Item item = null, string monsterName = null, bool probe = false)
	{
		if ((bool)completed)
		{
			return false;
		}
		bool? flag = n?.IsVillager;
		if (flag.HasValue && flag.GetValueOrDefault() && n.Name == target.Value && item?.QualifiedItemId == ItemId)
		{
			if (item.Stack >= (int)number)
			{
				if (!probe)
				{
					Game1.player.ActiveObject.Stack -= (int)number - 1;
					reloadDescription();
					n.CurrentDialogue.Push(new Dialogue(n, null, targetMessage));
					Game1.drawDialogue(n);
					Game1.player.reduceActiveItemByOne();
					if ((bool)dailyQuest)
					{
						Game1.player.changeFriendship(150, n);
					}
					else
					{
						Game1.player.changeFriendship(255, n);
					}
					questComplete();
				}
				return true;
			}
			if (!probe)
			{
				n.CurrentDialogue.Push(Dialogue.FromTranslation(n, "Strings\\StringsFromCSFiles:ItemDeliveryQuest.cs.13615", number.Value));
				Game1.drawDialogue(n);
			}
			return false;
		}
		return false;
	}

	/// <summary>Get the gold reward for a given item.</summary>
	/// <param name="item">The item instance.</param>
	public int GetGoldRewardPerItem(Item item)
	{
		if (item is Object obj)
		{
			return obj.Price * 3;
		}
		return (int)((float)item.salePrice() * 1.5f);
	}
}
