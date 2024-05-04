using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValley.Events;

public class PlayerCoupleBirthingEvent : BaseFarmEvent
{
	private int timer;

	private string soundName;

	private string message;

	private string babyName;

	private bool playedSound;

	private bool isMale;

	private bool getBabyName;

	private bool naming;

	private FarmHouse farmHouse;

	private long spouseID;

	private Farmer spouse;

	private bool isPlayersTurn;

	private Child child;

	public PlayerCoupleBirthingEvent()
	{
		spouseID = Game1.player.team.GetSpouse(Game1.player.UniqueMultiplayerID).Value;
		Game1.otherFarmers.TryGetValue(spouseID, out spouse);
		farmHouse = chooseHome();
		if (farmHouse.getChildren().Count >= 1)
		{
			Game1.getSteamAchievement("Achievement_FullHouse");
		}
	}

	private bool isSuitableHome(FarmHouse home)
	{
		if (home.getChildrenCount() < 2)
		{
			return home.upgradeLevel >= 2;
		}
		return false;
	}

	private FarmHouse chooseHome()
	{
		List<Farmer> parents = new List<Farmer>();
		parents.Add(Game1.player);
		parents.Add(spouse);
		parents.Sort((Farmer p1, Farmer p2) => p1.UniqueMultiplayerID.CompareTo(p2.UniqueMultiplayerID));
		foreach (Farmer parent in parents)
		{
			if (Game1.getLocationFromName(parent.homeLocation) is FarmHouse home && home == parent.currentLocation && isSuitableHome(home))
			{
				return home;
			}
		}
		foreach (Farmer item in parents)
		{
			if (Game1.getLocationFromName(item.homeLocation) is FarmHouse home && isSuitableHome(home))
			{
				return home;
			}
		}
		return Game1.player.currentLocation as FarmHouse;
	}

	/// <inheritdoc />
	public override bool setUp()
	{
		if (spouse == null || farmHouse == null)
		{
			return true;
		}
		Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.Date.TotalDays);
		Game1.player.CanMove = false;
		if (farmHouse.getChildrenCount() == 0)
		{
			isMale = r.NextBool();
		}
		else
		{
			isMale = farmHouse.getChildren()[0].Gender == Gender.Female;
		}
		Friendship friendship = Game1.player.GetSpouseFriendship();
		isPlayersTurn = friendship.Proposer != Game1.player.UniqueMultiplayerID == (farmHouse.getChildrenCount() % 2 == 0);
		if (spouse.IsMale == Game1.player.IsMale)
		{
			message = Game1.content.LoadString("Strings\\Events:BirthMessage_Adoption", Lexicon.getGenderedChildTerm(isMale));
		}
		else if (spouse.IsMale)
		{
			message = Game1.content.LoadString("Strings\\Events:BirthMessage_PlayerMother", Lexicon.getGenderedChildTerm(isMale));
		}
		else
		{
			message = Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseMother", Lexicon.getGenderedChildTerm(isMale), spouse.Name);
		}
		return false;
	}

	public void returnBabyName(string name)
	{
		babyName = name;
		Game1.exitActiveMenu();
	}

	public void afterMessage()
	{
		if (isPlayersTurn)
		{
			getBabyName = true;
			double chance = (spouse.hasDarkSkin() ? 0.5 : 0.0);
			chance += (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
			bool isDarkSkinned = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed).NextDouble() < chance;
			farmHouse.characters.Add(child = new Child("Baby", isMale, isDarkSkinned, Game1.player));
			child.Age = 0;
			child.Position = new Vector2(16f, 4f) * 64f + new Vector2(0f, -24f);
			Game1.player.GetSpouseFriendship().NextBirthingDate = null;
		}
		else
		{
			Game1.afterDialogues = delegate
			{
				getBabyName = true;
			};
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseNaming_" + (isMale ? "Male" : "Female"), spouse.Name));
		}
	}

	/// <inheritdoc />
	public override bool tickUpdate(GameTime time)
	{
		Game1.player.CanMove = false;
		timer += time.ElapsedGameTime.Milliseconds;
		Game1.fadeToBlackAlpha = 1f;
		if (timer > 1500 && !playedSound && !getBabyName)
		{
			if (!string.IsNullOrEmpty(soundName))
			{
				Game1.playSound(soundName);
				playedSound = true;
			}
			if (!playedSound && message != null && !Game1.dialogueUp && Game1.activeClickableMenu == null)
			{
				Game1.drawObjectDialogue(message);
				Game1.afterDialogues = afterMessage;
			}
		}
		else if (getBabyName)
		{
			if (!isPlayersTurn)
			{
				Game1.globalFadeToClear();
				return true;
			}
			if (!naming)
			{
				Game1.activeClickableMenu = new NamingMenu(returnBabyName, Game1.content.LoadString(isMale ? "Strings\\Events:BabyNamingTitle_Male" : "Strings\\Events:BabyNamingTitle_Female"), "");
				naming = true;
			}
			if (!string.IsNullOrEmpty(babyName) && babyName.Length > 0)
			{
				string newBabyName = babyName;
				List<NPC> all_characters = Utility.getAllCharacters();
				bool collision_found;
				do
				{
					collision_found = false;
					foreach (NPC item in all_characters)
					{
						if (item.Name == newBabyName)
						{
							newBabyName += " ";
							collision_found = true;
							break;
						}
					}
				}
				while (collision_found);
				child.Name = newBabyName;
				Game1.playSound("smallSelect");
				if (Game1.keyboardDispatcher != null)
				{
					Game1.keyboardDispatcher.Subscriber = null;
				}
				Game1.globalFadeToClear();
				return true;
			}
		}
		return false;
	}
}
