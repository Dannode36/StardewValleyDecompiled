using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Menus;

namespace StardewValley.Events;

public class BirthingEvent : BaseFarmEvent
{
	private int timer;

	private string soundName;

	private string message;

	private string babyName;

	private bool playedSound;

	private bool isMale;

	private bool getBabyName;

	private bool naming;

	/// <inheritdoc />
	public override bool setUp()
	{
		Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed);
		NPC spouse = Game1.RequireCharacter(Game1.player.spouse);
		Game1.player.CanMove = false;
		if (Game1.player.getNumberOfChildren() == 0)
		{
			isMale = r.NextBool();
		}
		else
		{
			isMale = Game1.player.getChildren()[0].Gender == Gender.Female;
		}
		if (spouse.isAdoptionSpouse())
		{
			message = Game1.content.LoadString("Strings\\Events:BirthMessage_Adoption", Lexicon.getGenderedChildTerm(isMale));
		}
		else if (spouse.Gender == Gender.Male)
		{
			message = Game1.content.LoadString("Strings\\Events:BirthMessage_PlayerMother", Lexicon.getGenderedChildTerm(isMale));
		}
		else
		{
			message = Game1.content.LoadString("Strings\\Events:BirthMessage_SpouseMother", Lexicon.getGenderedChildTerm(isMale), spouse.displayName);
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
		getBabyName = true;
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
			if (!naming)
			{
				Game1.activeClickableMenu = new NamingMenu(returnBabyName, Game1.content.LoadString(isMale ? "Strings\\Events:BabyNamingTitle_Male" : "Strings\\Events:BabyNamingTitle_Female"), "");
				naming = true;
			}
			if (!string.IsNullOrEmpty(babyName) && babyName.Length > 0)
			{
				NPC spouse = Game1.player.getSpouse();
				double chance = (spouse.hasDarkSkin() ? 0.5 : 0.0) + (Game1.player.hasDarkSkin() ? 0.5 : 0.0);
				bool isDarkSkinned = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed).NextBool(chance);
				string newBabyName = babyName;
				List<NPC> all_characters = Utility.getAllCharacters();
				bool collision_found;
				do
				{
					collision_found = false;
					if (Game1.characterData.ContainsKey(newBabyName))
					{
						newBabyName += " ";
						collision_found = true;
						continue;
					}
					foreach (NPC item in all_characters)
					{
						if (item.Name == newBabyName)
						{
							newBabyName += " ";
							collision_found = true;
						}
					}
				}
				while (collision_found);
				Child baby = new Child(newBabyName, isMale, isDarkSkinned, Game1.player);
				baby.Age = 0;
				baby.Position = new Vector2(16f, 4f) * 64f + new Vector2(0f, -24f);
				Utility.getHomeOfFarmer(Game1.player).characters.Add(baby);
				Game1.playSound("smallSelect");
				spouse.daysAfterLastBirth = 5;
				Game1.player.GetSpouseFriendship().NextBirthingDate = null;
				if (Game1.player.getChildrenCount() == 2)
				{
					spouse.shouldSayMarriageDialogue.Value = true;
					spouse.currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_SecondChild" + Game1.random.Next(1, 3), true));
					Game1.getSteamAchievement("Achievement_FullHouse");
				}
				else if (spouse.isAdoptionSpouse())
				{
					spouse.currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_Adoption", true, babyName));
				}
				else
				{
					spouse.currentMarriageDialogue.Insert(0, new MarriageDialogueReference("Data\\ExtraDialogue", "NewChild_FirstChild", true, babyName));
				}
				Game1.morningQueue.Enqueue(delegate
				{
					string text = Game1.getCharacterFromName(Game1.player.spouse)?.GetTokenizedDisplayName() ?? Game1.player.spouse;
					Game1.multiplayer.globalChatInfoMessage("Baby", Lexicon.capitalize(Game1.player.Name), text, Lexicon.getTokenizedGenderedChildTerm(isMale), Lexicon.getTokenizedPronoun(isMale), baby.displayName);
				});
				if (Game1.keyboardDispatcher != null)
				{
					Game1.keyboardDispatcher.Subscriber = null;
				}
				Game1.player.Position = Utility.PointToVector2(Utility.getHomeOfFarmer(Game1.player).GetPlayerBedSpot()) * 64f;
				Game1.globalFadeToClear();
				return true;
			}
		}
		return false;
	}
}
