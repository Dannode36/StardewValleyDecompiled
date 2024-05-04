using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buffs;
using StardewValley.Extensions;
using StardewValley.GameData.MakeoverOutfits;
using StardewValley.GameData.Shops;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class DesertFestival : Desert
{
	public enum RaceState
	{
		PreRace,
		StartingLine,
		Ready,
		Set,
		Go,
		AnnounceWinner,
		AnnounceWinner2,
		AnnounceWinner3,
		AnnounceWinner4,
		RaceEnd,
		RacesOver
	}

	public const int CALICO_STATUE_GHOST_INVASION = 0;

	public const int CALICO_STATUE_SERPENT_INVASION = 1;

	public const int CALICO_STATUE_SKELETON_INVASION = 2;

	public const int CALICO_STATUE_BAT_INVASION = 3;

	public const int CALICO_STATUE_ASSASSIN_BUGS = 4;

	public const int CALICO_STATUE_THIN_SHELLS = 5;

	public const int CALICO_STATUE_MEAGER_MEALS = 6;

	public const int CALICO_STATUE_MONSTER_SURGE = 7;

	public const int CALICO_STATUE_SHARP_TEETH = 8;

	public const int CALICO_STATUE_MUMMY_CURSE = 9;

	public const int CALICO_STATUE_SPEED_BOOST = 10;

	public const int CALICO_STATUE_REFRESH = 11;

	public const int CALICO_STATUE_50_EGG_TREASURE = 12;

	public const int CALICO_STATUE_NO_EFFECT = 13;

	public const int CALICO_STATUE_TOOTH_FILE = 14;

	public const int CALICO_STATUE_25_EGG_TREASURE = 15;

	public const int CALICO_STATUE_10_EGG_TREASURE = 16;

	public const int CALICO_STATUE_100_EGG_TREASURE = 17;

	public static readonly int[] CalicoStatueInvasionIds = new int[4] { 3, 0, 1, 2 };

	public const int NUM_SCHOLAR_QUESTIONS = 4;

	public const string FISHING_QUEST_ID = "98765";

	protected RandomizedPlantFurniture _cactusGuyRevealItem;

	protected float _cactusGuyRevealTimer = -1f;

	protected float _cactusShakeTimer = -1f;

	protected int _currentlyShownCactusID;

	protected NetEvent1Field<int, NetInt> _revealCactusEvent = new NetEvent1Field<int, NetInt>();

	protected NetEvent1Field<int, NetInt> _hideCactusEvent = new NetEvent1Field<int, NetInt>();

	protected bool hasAcceptedFishingQuestToday;

	protected MoneyDial eggMoneyDial;

	[XmlIgnore]
	public NetList<Racer, NetRef<Racer>> netRacers = new NetList<Racer, NetRef<Racer>>();

	[XmlIgnore]
	protected List<Racer> _localRacers = new List<Racer>();

	[XmlIgnore]
	protected float festivalChimneyTimer;

	[XmlIgnore]
	public List<int> finishedRacers = new List<int>();

	[XmlIgnore]
	public int racerCount = 3;

	[XmlIgnore]
	public int totalRacers = 5;

	[XmlIgnore]
	public NetEvent1Field<string, NetString> announceRaceEvent = new NetEvent1Field<string, NetString>();

	[XmlIgnore]
	public NetEnum<RaceState> currentRaceState = new NetEnum<RaceState>(RaceState.PreRace);

	[XmlIgnore]
	public NetLongDictionary<int, NetInt> sabotages = new NetLongDictionary<int, NetInt>();

	[XmlIgnore]
	public NetLongDictionary<int, NetInt> raceGuesses = new NetLongDictionary<int, NetInt>();

	[XmlIgnore]
	public NetLongDictionary<int, NetInt> nextRaceGuesses = new NetLongDictionary<int, NetInt>();

	[XmlIgnore]
	public NetLongDictionary<bool, NetBool> specialRewardsCollected = new NetLongDictionary<bool, NetBool>();

	[XmlIgnore]
	public NetLongDictionary<int, NetInt> rewardsToCollect = new NetLongDictionary<int, NetInt>();

	[XmlIgnore]
	public NetInt lastRaceWinner = new NetInt();

	[XmlIgnore]
	protected float _raceStateTimer;

	protected string _raceText;

	protected float _raceTextTimer;

	protected bool _raceTextShake;

	protected int _localSabotageText = -1;

	protected int _currentScholarQuestion = -1;

	protected int _cookIngredient = -1;

	protected int _cookSauce = -1;

	public Vector3[][] raceTrack = new Vector3[16][]
	{
		new Vector3[2]
		{
			new Vector3(41f, 39f, 0f),
			new Vector3(42f, 39f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(41f, 29f, 0f),
			new Vector3(42f, 28f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(6f, 29f, 0f),
			new Vector3(5f, 28f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(6f, 35f, 0f),
			new Vector3(5f, 36f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(10f, 35f, 2f),
			new Vector3(10f, 36f, 2f)
		},
		new Vector3[2]
		{
			new Vector3(12.5f, 35f, 0f),
			new Vector3(12.5f, 36f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(17.5f, 35f, 1f),
			new Vector3(17.5f, 36f, 1f)
		},
		new Vector3[2]
		{
			new Vector3(23.5f, 35f, 0f),
			new Vector3(23.5f, 36f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(28.5f, 35f, 1f),
			new Vector3(28.5f, 36f, 1f)
		},
		new Vector3[2]
		{
			new Vector3(31f, 35f, 0f),
			new Vector3(31f, 36f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(32f, 35f, 0f),
			new Vector3(31f, 36f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(32f, 38f, 3f),
			new Vector3(31f, 38f, 3f)
		},
		new Vector3[2]
		{
			new Vector3(32f, 43f, 0f),
			new Vector3(31f, 43f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(32f, 46f, 0f),
			new Vector3(31f, 47f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(41f, 46f, 0f),
			new Vector3(42f, 47f, 0f)
		},
		new Vector3[2]
		{
			new Vector3(41f, 39f, 0f),
			new Vector3(42f, 39f, 0f)
		}
	};

	private bool checkedMineExplanation;

	public DesertFestival()
	{
		forceLoadPathLayerLights = true;
	}

	public DesertFestival(string mapPath, string name)
		: base(mapPath, name)
	{
		forceLoadPathLayerLights = true;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(_revealCactusEvent, "_revealCactusEvent").AddField(_hideCactusEvent, "_hideCactusEvent").AddField(netRacers, "netRacers")
			.AddField(announceRaceEvent, "announceRaceEvent")
			.AddField(sabotages, "sabotages")
			.AddField(raceGuesses, "raceGuesses")
			.AddField(rewardsToCollect, "rewardsToCollect")
			.AddField(specialRewardsCollected, "specialRewardsCollected")
			.AddField(nextRaceGuesses, "nextRaceGuesses")
			.AddField(lastRaceWinner, "lastRaceWinner")
			.AddField(currentRaceState, "currentRaceState");
		_revealCactusEvent.onEvent += CactusGuyRevealCactus;
		_hideCactusEvent.onEvent += CactusGuyHideCactus;
		announceRaceEvent.onEvent += AnnounceRace;
	}

	public static void SetupMerchantSchedule(NPC character, int shop_index)
	{
		StringBuilder schedule = new StringBuilder();
		if (shop_index == 0)
		{
			schedule.Append("/a1130 Desert 15 40 2");
		}
		else
		{
			schedule.Append("/a1140 Desert 26 40 2");
		}
		schedule.Append("/2400 bed");
		schedule.Remove(0, 1);
		GameLocation defaultMap = Game1.getLocationFromName(character.DefaultMap);
		if (defaultMap != null)
		{
			Game1.warpCharacter(character, defaultMap, new Vector2((int)(character.DefaultPosition.X / 64f), (int)(character.DefaultPosition.Y / 64f)));
		}
		character.islandScheduleName.Value = "festival_vendor";
		character.TryLoadSchedule("desertFestival", schedule.ToString());
		character.performSpecialScheduleChanges();
	}

	public override void OnCamel()
	{
		Game1.playSound("camel");
		ShowCamelAnimation();
		Game1.player.faceDirection(0);
		Game1.haltAfterCheck = false;
	}

	public override void ShowCamelAnimation()
	{
		temporarySprites.Add(new TemporaryAnimatedSprite
		{
			texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1"),
			sourceRect = new Microsoft.Xna.Framework.Rectangle(273, 524, 65, 49),
			sourceRectStartingPos = new Vector2(273f, 524f),
			animationLength = 1,
			totalNumberOfLoops = 1,
			interval = 300f,
			scale = 4f,
			position = new Vector2(536f, 340f) * 4f,
			layerDepth = 0.1332f,
			id = 999
		});
	}

	public override void checkForMusic(GameTime time)
	{
		Game1.changeMusicTrack(GetFestivalMusic(), track_interruptable: true);
	}

	public virtual string GetFestivalMusic()
	{
		if (Utility.IsPassiveFestivalOpen("DesertFestival"))
		{
			return "event2";
		}
		return "summer_day_ambient";
	}

	public override string GetLocationSpecificMusic()
	{
		return GetFestivalMusic();
	}

	public override void digUpArtifactSpot(int xLocation, int yLocation, Farmer who)
	{
		Random r = Utility.CreateDaySaveRandom(xLocation * 2000, yLocation);
		Game1.createMultipleObjectDebris("CalicoEgg", xLocation, yLocation, r.Next(3, 7), who.UniqueMultiplayerID, this);
		base.digUpArtifactSpot(xLocation, yLocation, who);
	}

	public virtual void CollectRacePrizes()
	{
		List<Item> rewards = new List<Item>();
		if (specialRewardsCollected.ContainsKey(Game1.player.UniqueMultiplayerID) && !specialRewardsCollected[Game1.player.UniqueMultiplayerID])
		{
			specialRewardsCollected[Game1.player.UniqueMultiplayerID] = true;
			rewards.Add(ItemRegistry.Create("CalicoEgg", 100));
		}
		for (int i = 0; i < rewardsToCollect[Game1.player.UniqueMultiplayerID]; i++)
		{
			rewards.Add(ItemRegistry.Create("CalicoEgg", 20));
		}
		rewardsToCollect[Game1.player.UniqueMultiplayerID] = 0;
		Game1.activeClickableMenu = new ItemGrabMenu(rewards, reverseGrab: false, showReceivingMenu: true, null, null, "Rewards", null, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: false, allowRightClick: false, showOrganizeButton: false, 0, null, -1, this);
	}

	public override void performTouchAction(string full_action_string, Vector2 player_standing_position)
	{
		if (Game1.eventUp)
		{
			return;
		}
		if (full_action_string.Split(' ')[0] == "DesertMakeover")
		{
			if (Game1.player.controller != null)
			{
				return;
			}
			bool fail = false;
			string failMessageKey = null;
			NPC stylist = GetStylist();
			if (!fail && stylist == null)
			{
				stylist = null;
				failMessageKey = "Strings\\1_6_Strings:MakeOver_NoStylist";
				fail = true;
			}
			if (!fail && Game1.player.activeDialogueEvents.ContainsKey("DesertMakeover"))
			{
				failMessageKey = "Strings\\1_6_Strings:MakeOver_" + stylist.Name + "_AlreadyStyled";
				fail = true;
			}
			int required_space = 0;
			if (Game1.player.hat.Value != null)
			{
				required_space++;
			}
			if (Game1.player.shirtItem.Value != null)
			{
				required_space++;
			}
			if (Game1.player.pantsItem.Value != null)
			{
				required_space++;
			}
			if (!fail && Game1.player.freeSpotsInInventory() < required_space)
			{
				failMessageKey = "Strings\\1_6_Strings:MakeOver_" + stylist.Name + "_InventoryFull";
				fail = true;
			}
			if (fail)
			{
				Game1.freezeControls = true;
				Game1.displayHUD = false;
				int end_direction = 2;
				if (stylist != null)
				{
					end_direction = 3;
				}
				Game1.player.controller = new PathFindController(Game1.player, this, new Point(26, 52), end_direction, delegate
				{
					Game1.freezeControls = false;
					Game1.displayHUD = true;
					if (stylist != null)
					{
						stylist.faceTowardFarmerForPeriod(1000, 2, faceAway: false, Game1.player);
						if (failMessageKey != null)
						{
							Game1.DrawDialogue(stylist, failMessageKey);
						}
					}
					else if (failMessageKey != null)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString(failMessageKey));
					}
				});
			}
			else
			{
				Game1.player.activeDialogueEvents["DesertMakeover"] = 0;
				Game1.freezeControls = true;
				Game1.displayHUD = false;
				Game1.player.controller = new PathFindController(Game1.player, this, new Point(27, 50), 0);
				Game1.globalFadeToBlack(delegate
				{
					Game1.freezeControls = false;
					Game1.forceSnapOnNextViewportUpdate = true;
					Event @event = new Event(GetMakeoverEvent());
					@event.onEventFinished = (Action)Delegate.Combine(@event.onEventFinished, new Action(ReceiveMakeOver));
					startEvent(@event);
					Game1.globalFadeToClear();
				});
			}
		}
		else
		{
			base.performTouchAction(full_action_string, player_standing_position);
		}
	}

	public virtual string GetMakeoverEvent()
	{
		NPC stylist = GetStylist();
		Random r = Utility.CreateDaySaveRandom(Game1.year);
		StringBuilder sb = new StringBuilder();
		sb.Append("continue/26 51/farmer 27 50 2 ");
		foreach (NPC npc in characters)
		{
			if (!(npc.Name == stylist.Name) && !(npc.Name == "Sandy"))
			{
				sb.Append(npc.Name + " " + npc.Tile.X + " " + npc.Tile.Y + " " + npc.FacingDirection + " ");
			}
		}
		if (stylist.Name == "Emily")
		{
			sb.Append("Emily 25 52 2 Sandy 22 52 2/skippable/pause 1200/speak Emily \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_1"));
			sb.Append("\"/pause 100/");
			switch (r.Next(0, 3))
			{
			case 0:
				sb.Append("animate Emily false true 200 39 39/");
				break;
			case 1:
				sb.Append("animate Emily false true 300 16 17 18 19 20 21 22 23/");
				break;
			case 2:
				sb.Append("animate Emily false true 300 31 48 49/");
				break;
			}
			sb.Append("pause 1000/faceDirection Sandy 1 true/pause 2000/textAboveHead Emily \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_2"));
			sb.Append("\"/pause 3000/stopAnimation Emily 2/playSound dwop/shake Emily 100/jump Emily 4/pause 300/speak Emily \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_3"));
			sb.Append("\"/pause 100/advancedMove Emily false 1 0 0 -1 0 -1 0 -1 1 100/pause 100/");
			sb.Append("advancedMove Sandy false 1 0 1 0 1 0 1 0 2 100/pause 3000/playSound openChest/pause 1000/");
			List<string> reactions = new List<string>();
			reactions.Add(string.Format("playSound dustMeep/pause 300/playSound dustMeep/pause 300/playSound dustMeep/textAboveHead Emily \"{0}\"/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction1")));
			reactions.Add(string.Format("playSound rooster/playSound dwop/shake Sandy 400/jump Sandy 4/pause 500/textAboveHead Emily \"{0}\"/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction2")));
			reactions.Add(string.Format("playSound slimeHit/pause 300/playSound slimeHit/pause 600/playSound slimedead/textAboveHead Emily \"{0}\"/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction3")));
			reactions.Add(string.Format("textAboveHead Emily \"{0}\"/playSound trashcanlid/pause 1000/playSound trashcan/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction4")));
			reactions.Add(string.Format("textAboveHead Emily \"{0}\"/pause 1000/playSound cast/pause 500/playSound axe/pause 200/playSound ow/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction5")));
			reactions.Add(string.Format("textAboveHead Emily \"{0}\"/pause 1000/playSound eat/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction6")));
			reactions.Add(string.Format("textAboveHead Emily \"{0}\"/playSound scissors/pause 300/playSound scissors/pause 300/playSound scissors/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction7")));
			reactions.Add(string.Format("textAboveHead Emily \"{0}\"/pause 500/playSound trashbear/pause 300/playSound trashbear/pause 300/playSound trashbear/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction8")));
			reactions.Add(string.Format("textAboveHead Emily \"{0}\"/pause 1000/playSound fishingRodBend/pause 500/playSound fishingRodBend/pause 1000/playSound fishingRodBend/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_Reaction9")));
			Utility.Shuffle(r, reactions);
			for (int i = 0; i < 3; i++)
			{
				sb.Append("pause 500/");
				sb.Append(reactions[i]);
				sb.Append("pause 1500/");
			}
			sb.Append("pause 500/playSound money/textAboveHead Emily \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_4"));
			sb.Append("\"/playSound dwop/shake Sandy 400/jump Sandy 4/pause 750/advancedMove Sandy false -1 0 -1 0 -1 0 -1 0 1 100/pause 2000/advancedMove Emily false 0 1 0 1 0 1 2 100/pause 2000/speak Emily \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Emily_5"));
		}
		else
		{
			sb.Append("Sandy 22 52 2/skippable/pause 2000/textAboveHead Sandy \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_1"));
			sb.Append("\"/");
			sb.Append("pause 1000/playSound dwop/shake Sandy 400/jump Sandy 4/textAboveHead Sandy \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_2"));
			sb.Append("\"/");
			sb.Append("pause 200/advancedMove Sandy false 1 0 1 0 1 0 1 0 4 100/");
			sb.Append("pause 2500/speak Sandy \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_3"));
			sb.Append("\"/");
			sb.Append("pause 500/advancedMove Sandy false 0 -1 0 -1 0 -1/pause 3000/playSound openChest/pause 1000/");
			sb.Append(string.Format("textAboveHead Sandy \"{0}\"/pause 1000/playSound fishingRodBend/pause 500/playSound fishingRodBend/pause 1000/playSound fishingRodBend/", Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_4")));
			sb.Append("pause 1500/");
			sb.Append("pause 500/playSound money/textAboveHead Sandy \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_5"));
			sb.Append("\"/pause 200/advancedMove Sandy false 0 1 0 1 0 1 2 100/pause 2000/speak Sandy \"");
			sb.Append(Game1.content.LoadString("Strings\\1_6_Strings:MakeOver_Sandy_6"));
		}
		sb.Append("\"/pause 500/end");
		return sb.ToString();
	}

	private void ReceiveMakeOver()
	{
		ReceiveMakeOver(-1);
	}

	public virtual void ReceiveMakeOver(int randomSeedOverride = -1)
	{
		Random r = ((randomSeedOverride == -1) ? Utility.CreateDaySaveRandom(Game1.year) : Utility.CreateRandom(randomSeedOverride));
		if (randomSeedOverride == -1 && r.NextDouble() < 0.75)
		{
			r = Utility.CreateDaySaveRandom(Game1.year, (int)Game1.player.uniqueMultiplayerID.Value);
		}
		List<MakeoverOutfit> makeoverOutfits = DataLoader.MakeoverOutfits(Game1.content);
		if (makeoverOutfits == null)
		{
			return;
		}
		List<MakeoverOutfit> valid_outfits = new List<MakeoverOutfit>(makeoverOutfits);
		for (int i = 0; i < valid_outfits.Count; i++)
		{
			MakeoverOutfit outfit = valid_outfits[i];
			if ((outfit.Gender == "Male" && !Game1.player.IsMale) || (outfit.Gender == "Female" && Game1.player.IsMale))
			{
				valid_outfits.RemoveAt(i);
				i--;
				continue;
			}
			bool match = false;
			foreach (MakeoverItem outfitPart in outfit.OutfitParts)
			{
				ParsedItemData item_data = ItemRegistry.GetDataOrErrorItem(outfitPart.ItemId);
				if (Game1.player.hat.Value != null && Game1.player.hat.Value.QualifiedItemId == item_data.QualifiedItemId)
				{
					match = true;
					break;
				}
				if (Game1.player.shirtItem.Value != null && Game1.player.shirtItem.Value.QualifiedItemId == item_data.QualifiedItemId)
				{
					match = true;
					break;
				}
			}
			if (match)
			{
				valid_outfits.RemoveAt(i);
				i--;
			}
		}
		List<Item> old_clothes = new List<Item>();
		if (Game1.player.shirtItem.Value != null)
		{
			old_clothes.Add(Utility.PerformSpecialItemGrabReplacement(Game1.player.shirtItem.Value));
			Game1.player.shirtItem.Value = null;
		}
		if (Game1.player.pantsItem.Value != null)
		{
			old_clothes.Add(Utility.PerformSpecialItemGrabReplacement(Game1.player.pantsItem.Value));
			Game1.player.pantsItem.Value = null;
		}
		if (Game1.player.hat.Value != null)
		{
			old_clothes.Add(Utility.PerformSpecialItemGrabReplacement(Game1.player.hat.Value));
			Game1.player.hat.Value = null;
		}
		foreach (Item clothes in old_clothes)
		{
			if (Game1.player.addItemToInventory(clothes) != null)
			{
				Game1.player.team.returnedDonations.Add(clothes);
				Game1.player.team.newLostAndFoundItems.Value = true;
			}
		}
		MakeoverOutfit selected_outfit = r.ChooseFrom(valid_outfits);
		Random toga_random = Utility.CreateDaySaveRandom();
		if (Utility.GetDayOfPassiveFestival("DesertFestival") == 2 && toga_random.NextDouble() < 0.03)
		{
			selected_outfit = new MakeoverOutfit
			{
				OutfitParts = new List<MakeoverItem>
				{
					new MakeoverItem
					{
						ItemId = "(H)LaurelWreathCrown"
					},
					new MakeoverItem
					{
						ItemId = "(P)3",
						Color = "247 245 205"
					},
					new MakeoverItem
					{
						ItemId = "(S)1199"
					}
				}
			};
		}
		if (selected_outfit == null || selected_outfit.OutfitParts == null)
		{
			return;
		}
		foreach (MakeoverItem part in selected_outfit.OutfitParts)
		{
			Item item = ItemRegistry.Create(part.ItemId);
			if (item is Hat)
			{
				Game1.player.hat.Value = item as Hat;
			}
			if (item is Clothing)
			{
				Clothing clothing = item as Clothing;
				Color? color = Utility.StringToColor(part.Color);
				if (color.HasValue)
				{
					clothing.clothesColor.Value = color.Value;
				}
				if (clothing.clothesType.Value == Clothing.ClothesType.PANTS)
				{
					Game1.player.pantsItem.Value = clothing;
				}
				else if (clothing.clothesType.Value == Clothing.ClothesType.SHIRT)
				{
					Game1.player.shirtItem.Value = clothing;
				}
			}
		}
	}

	public virtual void AfterMakeOver()
	{
		Game1.player.canOnlyWalk = false;
		Game1.freezeControls = false;
		Game1.displayHUD = true;
		NPC stylist = GetStylist();
		if (stylist != null)
		{
			Game1.DrawDialogue(stylist, "Strings\\1_6_Strings:MakeOver_" + stylist.Name + "_Done");
			stylist.faceTowardFarmerForPeriod(1000, 2, faceAway: false, Game1.player);
		}
	}

	public NPC GetStylist()
	{
		NPC stylist = getCharacterFromName("Emily");
		if (stylist != null && stylist.TilePoint == new Point(25, 52))
		{
			return stylist;
		}
		stylist = getCharacterFromName("Sandy");
		if (stylist != null && stylist.TilePoint == new Point(22, 52))
		{
			NPC emily = getCharacterFromName("Emily");
			if (emily != null && emily.islandScheduleName.Value == "festival_vendor")
			{
				return stylist;
			}
		}
		return null;
	}

	public static void addCalicoStatueSpeedBuff()
	{
		BuffEffects speedBuff = new BuffEffects();
		speedBuff.Speed.Value = 1f;
		Game1.player.applyBuff(new Buff("CalicoStatueSpeed", "Calico Statue", Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_CalicoStatue"), 300000, Game1.buffsIcons, 9, speedBuff, false, Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_CalicoStatue_Name_10")));
	}

	public override bool performAction(string action, Farmer who, Location tile_location)
	{
		string festival_id = "DesertFestival";
		DataLoader.Shops(Game1.content);
		switch (action)
		{
		case "DesertFestivalMineExplanation":
			Game1.player.mailReceived.Add("Checked_DF_Mine_Explanation");
			checkedMineExplanation = true;
			Game1.multipleDialogues(new string[3]
			{
				Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_Explanation"),
				Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_Explanation_2"),
				Game1.content.LoadString("Strings\\1_6_Strings:DF_Mine_Explanation_3")
			});
			break;
		case "DesertFishingBoard":
			if (!hasAcceptedFishingQuestToday)
			{
				List<Response> responses = new List<Response>();
				responses.Add(new Response("Yes", Game1.content.LoadString("Strings\\1_6_Strings:Accept")));
				responses.Add(new Response("No", Game1.content.LoadString("Strings\\1_6_Strings:Decline")));
				createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Willy_DesertFishing" + Utility.GetDayOfPassiveFestival("DesertFestival")), responses.ToArray(), "Fishing_Quest");
			}
			break;
		case "DesertVendor":
		{
			Game1.player.faceDirection(0);
			if (!Utility.IsPassiveFestivalOpen(festival_id))
			{
				return false;
			}
			Microsoft.Xna.Framework.Rectangle shop_tile_rect = new Microsoft.Xna.Framework.Rectangle(tile_location.X, tile_location.Y - 1, 1, 1);
			foreach (NPC npc in characters)
			{
				if (shop_tile_rect.Contains(npc.TilePoint) && Utility.TryOpenShopMenu(festival_id + "_" + npc.Name, npc.Name))
				{
					return true;
				}
			}
			break;
		}
		case "DesertCactusMan":
			Game1.player.faceDirection(0);
			if (!Utility.IsPassiveFestivalOpen(festival_id))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Closed"));
			}
			else if (Game1.player.isInventoryFull())
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Yes_Full"));
			}
			else if (!Game1.player.mailReceived.Contains(GetCactusMail()))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Intro_" + Game1.random.Next(1, 4)));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
				{
					createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Question"), createYesNoResponses(), "CactusMan");
				});
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Collected"));
			}
			break;
		case "DesertEggShop":
			if (!Utility.IsPassiveFestivalOpen(festival_id))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:EggShop_Closed"));
			}
			else
			{
				Utility.TryOpenShopMenu("DesertFestival_EggShop", "Vendor");
			}
			break;
		case "DesertRacerMan":
			Game1.player.faceGeneralDirection(new Vector2((float)tile_location.X + 0.5f, (float)tile_location.Y + 0.5f) * 64f);
			if (specialRewardsCollected.ContainsKey(Game1.player.UniqueMultiplayerID) && !specialRewardsCollected[Game1.player.UniqueMultiplayerID])
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Collect_Prize_Special"));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
				{
					CollectRacePrizes();
				});
				return true;
			}
			if (rewardsToCollect.ContainsKey(Game1.player.UniqueMultiplayerID) && rewardsToCollect[Game1.player.UniqueMultiplayerID] > 0)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Collect_Prize"));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
				{
					CollectRacePrizes();
				});
				return true;
			}
			if (!Utility.IsPassiveFestivalOpen(festival_id) && Game1.timeOfDay < 1000)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Closed"));
				return true;
			}
			if (currentRaceState.Value >= RaceState.Go && currentRaceState.Value < RaceState.AnnounceWinner4)
			{
				if (raceGuesses.ContainsKey(Game1.player.UniqueMultiplayerID) && currentRaceState.Value == RaceState.Go)
				{
					int guessed_racer = raceGuesses[Game1.player.UniqueMultiplayerID];
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Guess_Already_Made", Game1.content.LoadString("Strings\\1_6_Strings:Racer_" + guessed_racer)));
					return true;
				}
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Ongoing"));
				return true;
			}
			if (!CanMakeAnotherRaceGuess())
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Ended"));
				return true;
			}
			if (nextRaceGuesses.ContainsKey(Game1.player.UniqueMultiplayerID))
			{
				int guessed_racer = nextRaceGuesses[Game1.player.UniqueMultiplayerID];
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Guess_Already_Made", Game1.content.LoadString("Strings\\1_6_Strings:Racer_" + guessed_racer)));
				return true;
			}
			createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Question"), createYesNoResponses(), "Race");
			return true;
		case "DesertShadyGuy":
			Game1.player.faceDirection(0);
			if (!Utility.IsPassiveFestivalOpen(festival_id) && Game1.timeOfDay < 1000)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Closed"));
				return true;
			}
			if (currentRaceState.Value >= RaceState.Go && currentRaceState.Value < RaceState.AnnounceWinner4)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Ongoing"));
				return true;
			}
			if (!CanMakeAnotherRaceGuess())
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Ended"));
				return true;
			}
			if (sabotages.ContainsKey(Game1.player.UniqueMultiplayerID))
			{
				ShowSabotagedRaceText();
				return true;
			}
			if (!Game1.player.mailReceived.Contains("Desert_Festival_Shady_Guy"))
			{
				Game1.player.mailReceived.Add("Desert_Festival_Shady_Guy");
				Game1.multipleDialogues(new string[3]
				{
					Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Intro"),
					Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Intro_2"),
					Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Intro_3")
				});
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
				{
					createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy"), createYesNoResponses(), "Shady_Guy");
				});
				return true;
			}
			createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_2nd"), createYesNoResponses(), "Shady_Guy");
			return true;
		case "DesertGil":
			if (Game1.Date == who.lastGotPrizeFromGil.Value)
			{
				if (Utility.GetDayOfPassiveFestival("DesertFestival") == 3)
				{
					Game1.DrawDialogue((Game1.getLocationFromName("AdventureGuild") as AdventureGuild).Gil, "Strings\\1_6_Strings:Gil_NextYear");
				}
				else
				{
					Game1.DrawDialogue((Game1.getLocationFromName("AdventureGuild") as AdventureGuild).Gil, "Strings\\1_6_Strings:Gil_ComeBack");
				}
				return true;
			}
			if (Game1.player.team.highestCalicoEggRatingToday.Value == 0)
			{
				Game1.DrawDialogue((Game1.getLocationFromName("AdventureGuild") as AdventureGuild).Gil, "Strings\\1_6_Strings:Gil_NoRating");
				return true;
			}
			createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Gil_SubmitRating", Game1.player.team.highestCalicoEggRatingToday.Value + 1), createYesNoResponses(), "Gil_EggRating");
			return true;
		case "DesertMarlon":
		{
			if (!Game1.player.mailReceived.Contains("Desert_Festival_Marlon"))
			{
				Game1.player.mailReceived.Add("Desert_Festival_Marlon");
				Game1.DrawDialogue(Game1.getCharacterFromName("Marlon"), "Strings\\1_6_Strings:Marlon_Intro");
				break;
			}
			bool order_chosen = false;
			bool order_complete = false;
			if (Game1.player.team.acceptedSpecialOrderTypes.Contains("DesertFestivalMarlon"))
			{
				order_complete = true;
				foreach (SpecialOrder order in Game1.player.team.specialOrders)
				{
					if (order.orderType == "DesertFestivalMarlon")
					{
						order_chosen = true;
						if (order.questState.Value == SpecialOrderStatus.InProgress || order.questState.Value == SpecialOrderStatus.Failed)
						{
							order_complete = false;
						}
						break;
					}
				}
			}
			if (order_complete)
			{
				if (Utility.GetDayOfPassiveFestival("DesertFestival") < 3)
				{
					Game1.DrawDialogue(Game1.getCharacterFromName("Marlon"), "Strings\\1_6_Strings:Marlon_Challenge_Finished");
					return true;
				}
				Game1.DrawDialogue(Game1.getCharacterFromName("Marlon"), "Strings\\1_6_Strings:Marlon_Challenge_Finished_LastDay");
				return true;
			}
			if (order_chosen)
			{
				Game1.DrawDialogue(Game1.getCharacterFromName("Marlon"), "Strings\\1_6_Strings:Marlon_Challenge_Chosen");
			}
			else
			{
				Game1.DrawDialogue(Game1.getCharacterFromName("Marlon"), "Strings\\1_6_Strings:Marlon_" + Game1.random.Next(1, 5));
			}
			Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
			{
				Game1.activeClickableMenu = new SpecialOrdersBoard("DesertFestivalMarlon");
			});
			return true;
		}
		case "DesertScholar":
			if (!Utility.IsPassiveFestivalOpen(festival_id))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Closed"));
				return true;
			}
			if (Game1.player.mailReceived.Contains(GetScholarMail()))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_DoneThisYear"));
				return true;
			}
			if (_currentScholarQuestion == -2)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Failed"));
				return true;
			}
			createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Intro"), createYesNoResponses(), "DesertScholar");
			break;
		case "DesertFood":
			Game1.player.faceDirection(0);
			createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Intro"), createYesNoResponses(), "Cook_Intro");
			break;
		}
		return base.performAction(action, who, tile_location);
	}

	public string GetCactusMail()
	{
		return "Y" + Game1.year + "_Cactus";
	}

	public string GetScholarMail()
	{
		return "Y" + Game1.year + "_Scholar";
	}

	public virtual Response[] GetRacerResponses()
	{
		List<Response> responses = new List<Response>();
		foreach (Racer racer in netRacers)
		{
			responses.Add(new Response(racer.racerIndex.ToString(), Game1.content.LoadString("Strings\\1_6_Strings:Racer_" + racer.racerIndex)));
		}
		responses.Add(new Response("cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
		return responses.ToArray();
	}

	public virtual void ShowSabotagedRaceText()
	{
		if (sabotages.ContainsKey(Game1.player.UniqueMultiplayerID))
		{
			int sabotaged_racer = sabotages[Game1.player.UniqueMultiplayerID];
			if (_localSabotageText == -1)
			{
				_localSabotageText = Game1.random.Next(1, 4);
			}
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Selected_" + _localSabotageText, Game1.content.LoadString("Strings\\1_6_Strings:Racer_" + sabotaged_racer)));
		}
	}

	private void generateNextScholarQuestion()
	{
		Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame);
		int whichQuestion = r.Next(3);
		whichQuestion += Game1.year;
		whichQuestion %= 3;
		string questionKey = "Scholar_Question_" + _currentScholarQuestion + "_" + whichQuestion;
		string optionsKey = "Scholar_Question_" + _currentScholarQuestion + "_" + whichQuestion + "_Options";
		string answersKey = "Scholar_Question_" + _currentScholarQuestion + "_" + whichQuestion + "_Answers";
		string[] options = null;
		int optionIndex = 0;
		try
		{
			options = Game1.content.LoadString("Strings\\1_6_Strings:" + optionsKey).Split(',');
			optionIndex = r.Next(options.Length);
		}
		catch (Exception)
		{
		}
		string[] answers = Game1.content.LoadString("Strings\\1_6_Strings:" + answersKey).Split(',');
		string question = ((options != null) ? Game1.content.LoadString("Strings\\1_6_Strings:" + questionKey, options[optionIndex]) : Game1.content.LoadString("Strings\\1_6_Strings:" + questionKey));
		List<Response> choices = new List<Response>();
		if (_currentScholarQuestion == 2 && whichQuestion == 1)
		{
			choices.Add(new Response("Correct", Game1.stats.StepsTaken.ToString() ?? ""));
			choices.Add(new Response("Wrong", (Game1.stats.StepsTaken * 2).ToString() ?? ""));
			choices.Add(new Response("Wrong", (Game1.stats.StepsTaken / 2).ToString() ?? ""));
		}
		else
		{
			choices.Add(new Response("Correct", answers[optionIndex]));
			int index;
			for (index = optionIndex; index == optionIndex; index = r.Next(answers.Length))
			{
			}
			choices.Add(new Response("Wrong", answers[index]));
			int index2 = optionIndex;
			while (index2 == optionIndex || index2 == index)
			{
				index2 = r.Next(answers.Length);
			}
			choices.Add(new Response("Wrong", answers[index2]));
		}
		Utility.Shuffle(r, choices);
		createQuestionDialogue(question, choices.ToArray(), "DesertScholar_Answer_");
		_currentScholarQuestion++;
	}

	public override void customQuestCompleteBehavior(string questId)
	{
		if (questId == "98765")
		{
			switch (Utility.GetDayOfPassiveFestival("DesertFestival"))
			{
			case 1:
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("CalicoEgg", 25));
				break;
			case 2:
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("CalicoEgg", 50));
				break;
			case 3:
				Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("CalicoEgg", 30));
				break;
			}
		}
		base.customQuestCompleteBehavior(questId);
	}

	public override bool answerDialogueAction(string question_and_answer, string[] question_params)
	{
		if (question_and_answer == null)
		{
			return false;
		}
		if (question_and_answer.Equals("WarperQuestion_Yes"))
		{
			if (Game1.player.Money < 250)
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:BusStop_NotEnoughMoneyForTicket"));
			}
			else
			{
				Game1.player.Money -= 250;
				Game1.player.CanMove = true;
				ItemRegistry.Create<Object>("(O)688").performUseAction(this);
				Game1.player.freezePause = 5000;
			}
			return true;
		}
		if (question_and_answer.Equals("Fishing_Quest_Yes"))
		{
			Quest q = null;
			q = ((Utility.GetDayOfPassiveFestival("DesertFestival") != 3) ? ((Quest)new FishingQuest((Utility.GetDayOfPassiveFestival("DesertFestival") == 1) ? "164" : "165", (Utility.GetDayOfPassiveFestival("DesertFestival") != 1) ? 1 : 3, "Willy", Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge"), Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge_Description_" + Utility.GetDayOfPassiveFestival("DesertFestival")), Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge_Return_" + Utility.GetDayOfPassiveFestival("DesertFestival")))) : ((Quest)new ItemDeliveryQuest("Willy", "GoldenBobber", Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge"), Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge_Description_" + Utility.GetDayOfPassiveFestival("DesertFestival")), "Strings\\1_6_Strings:Willy_GoldenBobber", Game1.content.LoadString("Strings\\1_6_Strings:Willy_Challenge_Return_" + Utility.GetDayOfPassiveFestival("DesertFestival")))));
			q.daysLeft.Value = 1;
			q.id.Value = "98765";
			Game1.player.questLog.Add(q);
			hasAcceptedFishingQuestToday = true;
			return true;
		}
		if (question_and_answer.StartsWith("Race_Guess_"))
		{
			string s = question_and_answer.Substring("Race_Guess_".Length + 1);
			int guessed_racer = -1;
			if (int.TryParse(s, out guessed_racer))
			{
				if (currentRaceState.Value >= RaceState.Go && currentRaceState.Value < RaceState.AnnounceWinner4)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Late_Guess"));
					return true;
				}
				string racerNameKey = "Strings\\1_6_Strings:Racer_" + guessed_racer;
				string racer_name = Game1.content.LoadString(racerNameKey);
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Guess_Made", racer_name));
				Game1.multiplayer.globalChatInfoMessage("GuessRacer_" + Game1.random.Next(1, 11), Game1.player.Name, TokenStringBuilder.LocalizedText(racerNameKey));
				nextRaceGuesses[Game1.player.UniqueMultiplayerID] = guessed_racer;
			}
			return true;
		}
		if (question_and_answer.Equals("Gil_EggRating_Yes"))
		{
			Game1.player.lastGotPrizeFromGil.Value = Game1.Date;
			Game1.player.freezePause = 1400;
			DelayedAction.playSoundAfterDelay("coin", 500);
			DelayedAction.functionAfterDelay(delegate
			{
				int num = Game1.player.team.highestCalicoEggRatingToday.Value + 1;
				int eggPrize = 0;
				Item extraPrize = null;
				if (num >= 1000)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Gil_Rating_1000"));
				}
				else if (num >= 55)
				{
					Game1.DrawDialogue((Game1.getLocationFromName("AdventureGuild") as AdventureGuild).Gil, "Strings\\1_6_Strings:Gil_Rating_50", num);
					eggPrize = 500;
					extraPrize = new Object("279", 1);
				}
				else if (num >= 25)
				{
					Game1.DrawDialogue((Game1.getLocationFromName("AdventureGuild") as AdventureGuild).Gil, "Strings\\1_6_Strings:Gil_Rating_25", num);
					eggPrize = 200;
					if (!Game1.player.mailReceived.Contains("DF_Gil_Hat"))
					{
						extraPrize = new Hat("GilsHat");
						Game1.player.mailReceived.Add("DF_Gil_Hat");
					}
					else
					{
						extraPrize = new Object("253", 5);
					}
				}
				else if (num >= 20)
				{
					Game1.DrawDialogue((Game1.getLocationFromName("AdventureGuild") as AdventureGuild).Gil, "Strings\\1_6_Strings:Gil_Rating_20to24", num);
					eggPrize = 100;
					extraPrize = new Object("253", 5);
				}
				else if (num >= 15)
				{
					Game1.DrawDialogue((Game1.getLocationFromName("AdventureGuild") as AdventureGuild).Gil, "Strings\\1_6_Strings:Gil_Rating_15to19", num);
					eggPrize = 50;
					extraPrize = new Object("253", 3);
				}
				else if (num >= 10)
				{
					Game1.DrawDialogue((Game1.getLocationFromName("AdventureGuild") as AdventureGuild).Gil, "Strings\\1_6_Strings:Gil_Rating_10to14", num);
					eggPrize = 25;
					extraPrize = new Object("253", 1);
				}
				else if (num >= 5)
				{
					Game1.DrawDialogue((Game1.getLocationFromName("AdventureGuild") as AdventureGuild).Gil, "Strings\\1_6_Strings:Gil_Rating_5to9", num);
					eggPrize = 10;
					extraPrize = new Object("395", 1);
				}
				else
				{
					Game1.DrawDialogue((Game1.getLocationFromName("AdventureGuild") as AdventureGuild).Gil, "Strings\\1_6_Strings:Gil_Rating_1to4", num);
					eggPrize = 1;
					extraPrize = new Object("243", 1);
				}
				Game1.afterDialogues = delegate
				{
					Game1.player.addItemByMenuIfNecessaryElseHoldUp(new Object("CalicoEgg", eggPrize));
					if (extraPrize != null)
					{
						Game1.afterDialogues = delegate
						{
							Game1.player.addItemByMenuIfNecessary(extraPrize);
						};
					}
				};
			}, 1000);
		}
		if (question_and_answer.StartsWith("Shady_Guy_Sabotage_"))
		{
			string s2 = question_and_answer.Substring("Shady_Guy_Sabotage_".Length + 1);
			int sabotaged_racer = -1;
			if (int.TryParse(s2, out sabotaged_racer))
			{
				if (currentRaceState.Value >= RaceState.Go && currentRaceState.Value < RaceState.AnnounceWinner4)
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Late"));
					return true;
				}
				if (!sabotages.Any() && Game1.random.NextDouble() < 0.25)
				{
					Game1.multiplayer.globalChatInfoMessage("RaceSabotage_" + Game1.random.Next(1, 6));
				}
				sabotages[Game1.player.UniqueMultiplayerID] = sabotaged_racer;
				_localSabotageText = -1;
				ShowSabotagedRaceText();
			}
			return true;
		}
		if (question_and_answer == "Race_Yes")
		{
			createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Race_Guess"), GetRacerResponses(), "Race_Guess_");
			return true;
		}
		if (question_and_answer == "Shady_Guy_Yes")
		{
			if (Game1.player.Items.CountId("CalicoEgg") >= 1)
			{
				Game1.player.Items.ReduceId("CalicoEgg", 1);
				createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_Question"), GetRacerResponses(), "Shady_Guy_Sabotage_");
			}
			else
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Shady_Guy_NoEgg"));
			}
		}
		if (question_and_answer.StartsWith("DesertScholar"))
		{
			if (question_and_answer == "DesertScholar_Yes")
			{
				_currentScholarQuestion++;
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Intro2"));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
				{
					generateNextScholarQuestion();
				});
			}
			else if (question_and_answer.StartsWith("DesertScholar_Answer_"))
			{
				if (question_and_answer == "DesertScholar_Answer__Wrong")
				{
					Game1.playSound("cancel");
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Wrong"));
					_currentScholarQuestion = -2;
				}
				else if (question_and_answer == "DesertScholar_Answer__Correct")
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Correct"));
					Game1.playSound("give_gift");
					if (_currentScholarQuestion == 4)
					{
						Game1.player.mailReceived.Add(GetScholarMail());
						Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
						{
							Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Scholar_Win"));
							Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
							{
								Game1.player.addItemByMenuIfNecessary(ItemRegistry.Create("CalicoEgg", 50));
								Game1.playSound("coin");
							});
						});
					}
					else
					{
						Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
						{
							generateNextScholarQuestion();
						});
					}
				}
			}
		}
		if (question_and_answer.StartsWith("Cook"))
		{
			if (question_and_answer.EndsWith("No"))
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Intro_No"));
			}
			else if (question_and_answer.StartsWith("Cook_ChoseSauce"))
			{
				Game1.playSound("smallSelect");
				_cookSauce = Convert.ToInt32(question_and_answer[question_and_answer.Length - 1].ToString() ?? "");
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_ChoseSauce", Game1.content.LoadString("Strings\\1_6_Strings:Cook_Sauce" + _cookSauce)));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
				{
					temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\desert_festival_tilesheet", new Microsoft.Xna.Framework.Rectangle(320, 280, 29, 24), new Vector2(480f, 1372f), flipped: false, 0f, Color.White)
					{
						id = 1001,
						animationLength = 2,
						interval = 200f,
						totalNumberOfLoops = 9999,
						scale = 4f,
						layerDepth = 0.1343f
					});
					temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\desert_festival_tilesheet", new Microsoft.Xna.Framework.Rectangle(378, 280, 29, 24), new Vector2(480f, 1372f), flipped: false, 0f, Color.White)
					{
						id = 1002,
						animationLength = 4,
						interval = 100f,
						totalNumberOfLoops = 4,
						delayBeforeAnimationStart = 400,
						scale = 4f,
						layerDepth = 0.1344f
					});
					DelayedAction.playSoundAfterDelay("hammer", 800, this);
					DelayedAction.playSoundAfterDelay("hammer", 1200, this);
					DelayedAction.playSoundAfterDelay("hammer", 1600, this);
					DelayedAction.playSoundAfterDelay("hammer", 2000, this);
					DelayedAction.playSoundAfterDelay("furnace", 2500, this);
					for (int j = 0; j < 12; j++)
					{
						temporarySprites.Add(new TemporaryAnimatedSprite(30, new Vector2(460.8f + (float)Game1.random.Next(-10, 10), 1388 + Game1.random.Next(-10, 10)), Color.White, 4, flipped: false, 100f, 2)
						{
							delayBeforeAnimationStart = 2700 + j * 80,
							motion = new Vector2(-1f + (float)Game1.random.Next(-5, 5) / 10f, -1f + (float)Game1.random.Next(-5, 5) / 10f),
							drawAboveAlwaysFront = true
						});
						temporarySprites.Add(new TemporaryAnimatedSprite(30, new Vector2(544f + (float)Game1.random.Next(-10, 10), 1388 + Game1.random.Next(-10, 10)), Color.White, 4, flipped: false, 100f, 2)
						{
							delayBeforeAnimationStart = 2700 + j * 80,
							motion = new Vector2(1f + (float)Game1.random.Next(-5, 5) / 10f, -1f + (float)Game1.random.Next(-5, 5) / 10f),
							drawAboveAlwaysFront = true
						});
						if (j % 2 == 0)
						{
							temporarySprites.Add(new TemporaryAnimatedSprite("Tilesheets\\Animations", new Microsoft.Xna.Framework.Rectangle(0, 2944, 64, 64), new Vector2(505.6f + (float)Game1.random.Next(-16, 16), 1344f), Game1.random.NextDouble() < 0.5, 0f, Color.Gray)
							{
								delayBeforeAnimationStart = 2700 + j * 80,
								motion = new Vector2(0f, -0.25f),
								animationLength = 8,
								interval = 70f,
								drawAboveAlwaysFront = true
							});
						}
					}
					Game1.player.freezePause = 4805;
					DelayedAction.functionAfterDelay(delegate
					{
						removeTemporarySpritesWithID(1001);
						removeTemporarySpritesWithID(1002);
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Done", Game1.content.LoadString("Strings\\1_6_Strings:Cook_DishNames_" + _cookIngredient + "_" + _cookSauce)));
						Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
						{
							Object food = new Object();
							food.edibility.Value = Game1.player.maxHealth;
							string text = "Strings\\1_6_Strings:Cook_DishNames_" + _cookIngredient + "_" + _cookSauce;
							food.name = Game1.content.LoadString(text);
							food.displayNameFormat = "[LocalizedText " + text + "]";
							BuffEffects effects = new BuffEffects();
							switch (_cookIngredient)
							{
							case 0:
								effects.Defense.Value = 3f;
								break;
							case 1:
								effects.MiningLevel.Value = 3f;
								break;
							case 2:
								effects.LuckLevel.Value = 3f;
								break;
							case 3:
								effects.Attack.Value = 3f;
								break;
							case 4:
								effects.FishingLevel.Value = 3f;
								break;
							}
							switch (_cookSauce)
							{
							case 0:
								effects.Defense.Value = 1f;
								break;
							case 1:
								effects.MiningLevel.Value = 1f;
								break;
							case 2:
								effects.LuckLevel.Value = 1f;
								break;
							case 3:
								effects.Attack.Value = 1f;
								break;
							case 4:
								effects.Speed.Value = 1f;
								break;
							}
							food.customBuff = () => new Buff("DesertFestival", food.Name, food.Name, 600 * Game1.realMilliSecondsPerGameMinute, null, -1, effects, false);
							int sourceIndex = _cookIngredient * 4 + _cookSauce + ((_cookSauce > _cookIngredient) ? (-1) : 0);
							Game1.player.tempFoodItemTextureName.Value = "TileSheets\\Objects_2";
							Game1.player.tempFoodItemSourceRect.Value = Utility.getSourceRectWithinRectangularRegion(0, 32, 128, sourceIndex, 16, 16);
							Game1.player.faceDirection(2);
							Game1.player.eatObject(food);
						});
					}, 4800);
				});
			}
			else if (question_and_answer.StartsWith("Cook_PickedIngredient"))
			{
				Game1.playSound("smallSelect");
				_cookIngredient = Convert.ToInt32(question_and_answer[question_and_answer.Length - 1].ToString() ?? "");
				List<Response> sauces = new List<Response>();
				for (int i = 0; i < 5; i++)
				{
					if (i != _cookIngredient || _cookIngredient == 4)
					{
						sauces.Add(new Response(i.ToString() ?? "", Game1.content.LoadString("Strings\\1_6_Strings:Cook_Sauce" + i)));
					}
				}
				createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_ChoseIngredient", Game1.content.LoadString("Strings\\1_6_Strings:Cook_Ingredient" + _cookIngredient)), sauces.ToArray(), "Cook_ChoseSauce");
			}
			else if (!(question_and_answer == "Cook_Intro_Yes"))
			{
				if (question_and_answer == "Cook_Intro2_Yes")
				{
					Game1.playSound("smallSelect");
					Response[] ingredients = new Response[5];
					for (int i = 0; i < 5; i++)
					{
						ingredients[i] = new Response(i.ToString() ?? "", Game1.content.LoadString("Strings\\1_6_Strings:Cook_Ingredient" + i));
					}
					createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Intro_Yes3"), ingredients, "Cook_PickedIngredient");
				}
			}
			else
			{
				Game1.playSound("smallSelect");
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Intro_Yes"));
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
				{
					Game1.playSound("smallSelect");
					createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:Cook_Intro_Yes2"), createYesNoResponses(), "Cook_Intro2");
				});
			}
		}
		if (question_and_answer == "CactusMan_Yes")
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Yes_Intro"));
			Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
			{
				if (Game1.player.isInventoryFull())
				{
					Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Yes_Full"));
				}
				else
				{
					int seed = (int)(Game1.player.UniqueMultiplayerID + Game1.year);
					Game1.player.freezePause = 4000;
					DelayedAction.functionAfterDelay(delegate
					{
						_revealCactusEvent.Fire(seed);
					}, 1000);
					DelayedAction.functionAfterDelay(delegate
					{
						Random random = Utility.CreateRandom(seed);
						random.Next();
						random.Next();
						random.Next();
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_Yes_" + random.Next(1, 6)));
						Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
						{
							RandomizedPlantFurniture item = new RandomizedPlantFurniture("FreeCactus", Vector2.Zero, seed);
							if (Game1.player.addItemToInventoryBool(item))
							{
								Game1.playSound("coin");
								Game1.player.mailReceived.Add(GetCactusMail());
							}
							_hideCactusEvent.Fire(seed);
							Game1.player.freezePause = 100;
						});
					}, 3000);
				}
			});
			return true;
		}
		if (question_and_answer == "CactusMan_No")
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:CactusMan_No"));
			return true;
		}
		return base.answerDialogueAction(question_and_answer, question_params);
	}

	public void CactusGuyHideCactus(int seed)
	{
		if (_currentlyShownCactusID == seed)
		{
			_cactusGuyRevealItem = null;
			_cactusGuyRevealTimer = -1f;
			_cactusShakeTimer = -1f;
			_currentlyShownCactusID = -1;
		}
	}

	public void CactusGuyRevealCactus(int seed)
	{
		RandomizedPlantFurniture cactus = new RandomizedPlantFurniture("FreeCactus", Vector2.Zero, seed);
		_currentlyShownCactusID = seed;
		_cactusGuyRevealItem = cactus.getOne() as RandomizedPlantFurniture;
		_cactusGuyRevealTimer = 0f;
		_cactusShakeTimer = -1f;
		Random random = Utility.CreateRandom(seed);
		random.Next();
		random.Next();
		List<string> sounds = new List<string> { "pig", "Duck", "dog_bark", "cat", "camel" };
		Game1.playSound("throwDownITem");
		DelayedAction.playSoundAfterDelay("thudStep", 500);
		DelayedAction.playSoundAfterDelay("thudStep", 750);
		DelayedAction.playSoundAfterDelay(random.ChooseFrom(sounds), 1000);
		DelayedAction.functionAfterDelay(delegate
		{
			_cactusShakeTimer = 0.25f;
		}, 1000);
	}

	public bool CanMakeAnotherRaceGuess()
	{
		if (Game1.timeOfDay >= 2200 && currentRaceState.Value >= RaceState.Go)
		{
			return false;
		}
		return true;
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		if (_cactusShakeTimer > 0f)
		{
			_cactusShakeTimer -= (float)time.ElapsedGameTime.TotalSeconds;
			if (_cactusShakeTimer <= 0f)
			{
				_cactusShakeTimer = -1f;
			}
		}
		if (_raceTextTimer > 0f)
		{
			_raceTextTimer -= (float)time.ElapsedGameTime.TotalSeconds;
			if (_raceTextTimer < 0f)
			{
				_raceTextTimer = 0f;
			}
		}
		if (_cactusGuyRevealTimer >= 0f && _cactusGuyRevealTimer < 1f)
		{
			_cactusGuyRevealTimer += (float)time.ElapsedGameTime.TotalSeconds / 0.75f;
			if (_cactusGuyRevealTimer >= 1f)
			{
				_cactusGuyRevealTimer = 1f;
			}
		}
		_revealCactusEvent.Poll();
		_hideCactusEvent.Poll();
		announceRaceEvent.Poll();
		if (Game1.shouldTimePass())
		{
			if (Game1.IsMasterGame)
			{
				if (_raceStateTimer >= 0f)
				{
					_raceStateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
					if (_raceStateTimer <= 0f)
					{
						_raceStateTimer = 0f;
						switch (currentRaceState.Value)
						{
						case RaceState.StartingLine:
							announceRaceEvent.Fire("Race_Ready");
							_raceStateTimer = 3f;
							currentRaceState.Value = RaceState.Ready;
							break;
						case RaceState.Ready:
							currentRaceState.Value = RaceState.Set;
							announceRaceEvent.Fire("Race_Set");
							_raceStateTimer = 3f;
							break;
						case RaceState.Set:
							currentRaceState.Value = RaceState.Go;
							announceRaceEvent.Fire("Race_Go");
							raceGuesses.Clear();
							foreach (KeyValuePair<long, int> kvp in nextRaceGuesses.Pairs)
							{
								raceGuesses[kvp.Key] = kvp.Value;
							}
							nextRaceGuesses.Clear();
							foreach (Racer racer in netRacers)
							{
								racer.sabotages.Value = 0;
								foreach (int value in sabotages.Values)
								{
									if (value == (int)racer.racerIndex)
									{
										racer.sabotages.Value++;
									}
								}
								racer.ResetMoveSpeed();
							}
							sabotages.Clear();
							_raceStateTimer = 3f;
							break;
						case RaceState.AnnounceWinner:
						case RaceState.AnnounceWinner2:
						case RaceState.AnnounceWinner3:
						case RaceState.AnnounceWinner4:
							_raceStateTimer = 2f;
							switch (currentRaceState.Value)
							{
							case RaceState.AnnounceWinner:
								announceRaceEvent.Fire("Race_Comment_" + Game1.random.Next(1, 5));
								_raceStateTimer = 4f;
								break;
							case RaceState.AnnounceWinner2:
								announceRaceEvent.Fire("Race_Winner");
								_raceStateTimer = 2f;
								break;
							case RaceState.AnnounceWinner3:
								announceRaceEvent.Fire("Racer_" + lastRaceWinner.Value);
								_raceStateTimer = 4f;
								break;
							case RaceState.AnnounceWinner4:
								announceRaceEvent.Fire("RESULT");
								_raceStateTimer = 2f;
								finishedRacers.Clear();
								break;
							}
							currentRaceState.Value++;
							break;
						case RaceState.RaceEnd:
							if (!CanMakeAnotherRaceGuess())
							{
								if (Utility.GetDayOfPassiveFestival("DesertFestival") < 3)
								{
									announceRaceEvent.Fire("Race_Close");
								}
								else
								{
									announceRaceEvent.Fire("Race_Close_LastDay");
								}
								currentRaceState.Value = RaceState.RacesOver;
							}
							else
							{
								currentRaceState.Value = RaceState.PreRace;
							}
							break;
						}
					}
				}
				if (currentRaceState.Value == RaceState.Go)
				{
					if (finishedRacers.Count >= racerCount)
					{
						currentRaceState.Value = RaceState.AnnounceWinner;
						_raceStateTimer = 2f;
					}
					else
					{
						foreach (Racer netRacer in netRacers)
						{
							netRacer.UpdateRaceProgress(this);
						}
					}
				}
			}
			foreach (Racer netRacer2 in netRacers)
			{
				netRacer2.Update(this);
			}
		}
		festivalChimneyTimer -= time.ElapsedGameTime.Milliseconds;
		if (festivalChimneyTimer <= 0f)
		{
			AddSmokePuff(new Vector2(7.25f, 16.25f) * 64f);
			AddSmokePuff(new Vector2(28.25f, 6f) * 64f);
			festivalChimneyTimer = 500f;
		}
		if (Game1.isStartingToGetDarkOut(this) && Game1.outdoorLight.R > 160)
		{
			Game1.outdoorLight.R = 160;
			Game1.outdoorLight.G = 160;
			Game1.outdoorLight.B = 0;
		}
		base.UpdateWhenCurrentLocation(time);
	}

	public void OnRaceWon(int winner)
	{
		lastRaceWinner.Value = winner;
		if (raceGuesses.FieldDict.Count <= 0)
		{
			return;
		}
		List<string> winning_farmers = new List<string>();
		foreach (KeyValuePair<long, int> kvp in raceGuesses.Pairs)
		{
			if (kvp.Value != winner)
			{
				continue;
			}
			if (winner == 3 && !specialRewardsCollected.ContainsKey(kvp.Key))
			{
				specialRewardsCollected[kvp.Key] = false;
				continue;
			}
			if (!rewardsToCollect.ContainsKey(kvp.Key))
			{
				rewardsToCollect[kvp.Key] = 0;
			}
			rewardsToCollect[kvp.Key]++;
			Farmer winner_farmer = Game1.getFarmerMaybeOffline(kvp.Key);
			if (winner_farmer != null)
			{
				winning_farmers.Add(winner_farmer.Name);
			}
		}
		string tokenizedWinnerName = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:Racer_" + winner);
		if (winning_farmers.Count == 0)
		{
			Game1.multiplayer.globalChatInfoMessage("RaceWinners_Zero", tokenizedWinnerName);
			return;
		}
		if (winning_farmers.Count == 1)
		{
			Game1.multiplayer.globalChatInfoMessage("RaceWinners_One", tokenizedWinnerName, winning_farmers[0]);
			return;
		}
		if (winning_farmers.Count == 2)
		{
			Game1.multiplayer.globalChatInfoMessage("RaceWinners_Two", tokenizedWinnerName, winning_farmers[0], winning_farmers[1]);
			return;
		}
		Game1.multiplayer.globalChatInfoMessage("RaceWinners_Many", tokenizedWinnerName);
		for (int i = 0; i < winning_farmers.Count; i++)
		{
			if (i < winning_farmers.Count - 1)
			{
				Game1.multiplayer.globalChatInfoMessage("RaceWinners_List", winning_farmers[i]);
			}
			else
			{
				Game1.multiplayer.globalChatInfoMessage("RaceWinners_Final", winning_farmers[i]);
			}
		}
	}

	public void AddSmokePuff(Vector2 v)
	{
		temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), v, flipped: false, 0.002f, Color.Gray)
		{
			alpha = 0.75f,
			motion = new Vector2(0f, -0.5f),
			acceleration = new Vector2(0.002f, 0f),
			interval = 99999f,
			layerDepth = 1f,
			scale = 2f,
			scaleChange = 0.02f,
			drawAboveAlwaysFront = true,
			rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f
		});
	}

	public static void CleanupFestival()
	{
		Game1.player.team.itemsToRemoveOvernight.Add("CalicoEgg");
		SpecialOrder.RemoveAllSpecialOrders("DesertFestivalMarlon");
	}

	public override void draw(SpriteBatch spriteBatch)
	{
		if (_cactusGuyRevealTimer > 0f && _cactusGuyRevealItem != null)
		{
			Vector2 start = new Vector2(29f, 66.5f) * 64f;
			Vector2 end = new Vector2(27.5f, 66.5f) * 64f;
			float height = 0f;
			float bounce_point = 0.6f;
			height = ((!(_cactusGuyRevealTimer < bounce_point)) ? ((float)Math.Sin((double)((_cactusGuyRevealTimer - bounce_point) / (1f - bounce_point)) * Math.PI) * 8f * 4f) : ((float)Math.Sin((double)(_cactusGuyRevealTimer / bounce_point) * Math.PI) * 16f * 4f));
			Vector2 position = new Vector2(Utility.Lerp(start.X, end.X, _cactusGuyRevealTimer), Utility.Lerp(start.Y, end.Y, _cactusGuyRevealTimer));
			float sort_y = position.Y;
			if (_cactusShakeTimer > 0f)
			{
				position.X += Game1.random.Next(-1, 2);
				position.Y += Game1.random.Next(-1, 2);
			}
			_cactusGuyRevealItem.DrawFurniture(spriteBatch, Game1.GlobalToLocal(Game1.viewport, position + new Vector2(0f, 0f - height)), 1f, new Vector2(8f, 16f), 4f, sort_y / 10000f);
			spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, position), null, Color.White * 0.75f, 0f, new Vector2(Game1.shadowTexture.Width / 2, Game1.shadowTexture.Height / 2), new Vector2(4f, 4f), SpriteEffects.None, sort_y / 10000f - 1E-07f);
		}
		foreach (Racer racer in _localRacers)
		{
			if (!racer.drawAboveMap.Value)
			{
				racer.Draw(spriteBatch);
			}
		}
		if (!hasAcceptedFishingQuestToday)
		{
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(984f, 842f + yOffset)), new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset / 16f), SpriteEffects.None, 1f);
		}
		if (!checkedMineExplanation)
		{
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(609.6f, 320f + yOffset)), new Microsoft.Xna.Framework.Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset / 16f), SpriteEffects.None, 1f);
		}
		if (Game1.timeOfDay < 1000)
		{
			spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(new Vector2(45f, 14f) * 64f + new Vector2(7f, 9f) * 4f), new Microsoft.Xna.Framework.Rectangle(239, 317, 16, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.096f);
		}
		base.draw(spriteBatch);
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		switch (getTileIndexAt(tileLocation, "Buildings"))
		{
		case 796:
		case 797:
			Utility.TryOpenShopMenu("Traveler", this);
			return true;
		case 792:
		case 793:
			playSound("pig");
			return true;
		case 1073:
			createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:BeachNightMarket_WarperQuestion"), createYesNoResponses(), "WarperQuestion");
			return true;
		default:
			return base.checkAction(tileLocation, viewport, who);
		}
	}

	public override void drawOverlays(SpriteBatch b)
	{
		SpecialCurrencyDisplay.Draw(b, new Vector2(16f, 0f), eggMoneyDial, Game1.player.Items.CountId("CalicoEgg"), Game1.mouseCursors_1_6, new Microsoft.Xna.Framework.Rectangle(0, 21, 0, 0));
		base.drawOverlays(b);
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch sb)
	{
		base.drawAboveAlwaysFrontLayer(sb);
		_localRacers.Sort((Racer a, Racer b) => a.position.Y.CompareTo(b.position.Y));
		foreach (Racer racer in _localRacers)
		{
			if (racer.drawAboveMap.Value)
			{
				racer.Draw(sb);
			}
		}
		if (_raceTextTimer > 0f && _raceText != null)
		{
			Vector2 local = Game1.GlobalToLocal(new Vector2(44.5f, 39.5f) * 64f);
			if (_raceTextShake)
			{
				local += new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
			}
			float alpha = Utility.Clamp(_raceTextTimer / 0.25f, 0f, 1f);
			SpriteText.drawStringWithScrollCenteredAt(sb, _raceText, (int)local.X, (int)local.Y - 192, "", alpha, null, 1, local.Y / 10000f + 0.001f);
		}
	}

	public Vector3 GetTrackPosition(int track_index, float horizontal_position)
	{
		Vector2 inner_edge = new Vector2(raceTrack[track_index][0].X + 0.5f, raceTrack[track_index][0].Y + 0.5f);
		Vector2 outer_edge = new Vector2(raceTrack[track_index][1].X + 0.5f, raceTrack[track_index][1].Y + 0.5f);
		_ = inner_edge == outer_edge;
		Vector2 delta = outer_edge - inner_edge;
		delta.Normalize();
		inner_edge *= 64f;
		outer_edge *= 64f;
		inner_edge -= delta * 64f / 4f;
		outer_edge += delta * 64f / 4f;
		return new Vector3(Utility.Lerp(inner_edge.X, outer_edge.X, horizontal_position), Utility.Lerp(inner_edge.Y, outer_edge.Y, horizontal_position), raceTrack[track_index][0].Z);
	}

	public override void performTenMinuteUpdate(int timeOfDay)
	{
		string festival_id = "DesertFestival";
		base.performTenMinuteUpdate(timeOfDay);
		if (Game1.IsMasterGame && Utility.IsPassiveFestivalOpen(festival_id) && timeOfDay % 200 == 0 && timeOfDay < 2400 && currentRaceState.Value == RaceState.PreRace)
		{
			announceRaceEvent.Fire("Race_Begin");
			currentRaceState.Value = RaceState.StartingLine;
			if (nextRaceGuesses.FieldDict.Count > 0)
			{
				Game1.multiplayer.globalChatInfoMessage("RaceStarting");
			}
			_raceStateTimer = 5f;
		}
	}

	public virtual void AnnounceRace(string text)
	{
		_raceTextShake = false;
		_raceTextTimer = 2f;
		if (text == "Race_Go" || text == "Race_Finish" || text.StartsWith("Racer_"))
		{
			_raceTextShake = true;
		}
		if (text.StartsWith("Race_Close"))
		{
			_raceTextTimer = 4f;
		}
		if (text == "RESULT")
		{
			_raceTextTimer = 4f;
			if (raceGuesses.ContainsKey(Game1.player.UniqueMultiplayerID))
			{
				if (lastRaceWinner.Value == raceGuesses[Game1.player.UniqueMultiplayerID])
				{
					_raceText = Game1.content.LoadString("Strings\\1_6_Strings:Race_Win");
				}
				else
				{
					_raceText = Game1.content.LoadString("Strings\\1_6_Strings:Race_Lose");
				}
			}
		}
		else
		{
			_raceText = Game1.content.LoadString("Strings\\1_6_Strings:" + text);
			if (text.StartsWith("Racer_"))
			{
				_raceText += "!";
			}
		}
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		Game1.player.team.calicoEggSkullCavernRating.Value = 0;
		Game1.player.team.highestCalicoEggRatingToday.Value = 0;
		Game1.player.team.calicoStatueEffects.Clear();
		MineShaft.totalCalicoStatuesActivatedToday = 0;
		hasAcceptedFishingQuestToday = false;
		finishedRacers.Clear();
		lastRaceWinner.Value = -1;
		rewardsToCollect.Clear();
		specialRewardsCollected.Clear();
		raceGuesses.Clear();
		nextRaceGuesses.Clear();
		sabotages.Clear();
		currentRaceState.Value = RaceState.PreRace;
		_raceStateTimer = 0f;
		_currentScholarQuestion = -1;
	}

	public override void cleanupBeforePlayerExit()
	{
		_localRacers.Clear();
		_cactusGuyRevealTimer = -1f;
		_cactusGuyRevealItem = null;
		base.cleanupBeforePlayerExit();
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (Game1.player.mailReceived.Contains("Checked_DF_Mine_Explanation"))
		{
			checkedMineExplanation = true;
		}
		_localRacers.Clear();
		_localRacers.AddRange(netRacers);
		if (critters == null)
		{
			critters = new List<Critter>();
		}
		for (int i = 0; i < 8; i++)
		{
			critters.Add(new Butterfly(this, getRandomTile(), islandButterfly: false, forceSummerButterfly: true));
		}
		eggMoneyDial = new MoneyDial(4, playSound: false);
		eggMoneyDial.currentValue = Game1.player.Items.CountId("CalicoEgg");
	}

	public static void SetupFestivalDay()
	{
		string festival_id = "DesertFestival";
		int day_number = Utility.GetDayOfPassiveFestival(festival_id);
		Dictionary<string, ShopData> store_data_sheet = DataLoader.Shops(Game1.content);
		List<NPC> characters = Utility.getAllVillagers();
		for (int i = 0; i < characters.Count; i++)
		{
			NPC character = characters[i];
			if (!store_data_sheet.ContainsKey(festival_id + "_" + character.Name) || (character.Name == "Leo" && !Game1.MasterPlayer.mailReceived.Contains("leoMoved")))
			{
				characters.RemoveAt(i);
				i--;
			}
			else if (character.getMasterScheduleRawData().ContainsKey(festival_id + "_" + day_number))
			{
				characters.RemoveAt(i);
				i--;
			}
		}
		Random r = Utility.CreateDaySaveRandom();
		for (int i = 0; i < day_number - 1; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				NPC character = r.ChooseFrom(characters);
				characters.Remove(character);
				if (characters.Count == 0)
				{
					break;
				}
			}
		}
		if (characters.Count > 0)
		{
			NPC character = r.ChooseFrom(characters);
			characters.Remove(character);
			SetupMerchantSchedule(character, 0);
		}
		if (characters.Count > 0)
		{
			NPC character = r.ChooseFrom(characters);
			characters.Remove(character);
			SetupMerchantSchedule(character, 1);
		}
		if (Game1.getLocationFromName("DesertFestival") is DesertFestival festival_location)
		{
			festival_location.netRacers.Clear();
			List<int> racers = new List<int>();
			for (int i = 0; i < festival_location.totalRacers; i++)
			{
				racers.Add(i);
			}
			for (int i = 0; i < festival_location.racerCount; i++)
			{
				int racer_index = r.ChooseFrom(racers);
				racers.Remove(racer_index);
				Racer racer = new Racer(racer_index);
				racer.position.Value = new Vector2(44.5f, 37.5f - (float)i) * 64f;
				racer.segmentStart = racer.position.Value;
				racer.segmentEnd = racer.position.Value;
				festival_location.netRacers.Add(racer);
			}
		}
		SpecialOrder.UpdateAvailableSpecialOrders("DesertFestivalMarlon", forceRefresh: true);
	}
}
