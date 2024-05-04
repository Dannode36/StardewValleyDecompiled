using System;
using Microsoft.Xna.Framework;

namespace StardewValley.Events;

public class DiaryEvent : BaseFarmEvent
{
	public string NPCname;

	/// <inheritdoc />
	public override bool setUp()
	{
		if (Game1.player.isMarriedOrRoommates())
		{
			return true;
		}
		foreach (string s in Game1.player.mailReceived)
		{
			if (s.Contains("diary"))
			{
				string name = s.Split('_')[1];
				if (Game1.player.mailReceived.Add("diary_" + name + "_finished"))
				{
					NPCname = name.Split('/')[0];
					NPC who = Game1.getCharacterFromName(NPCname);
					string question = (Game1.player.IsMale ? Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6658") : Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6660")) + Environment.NewLine + Environment.NewLine + "-" + Utility.capitalizeFirstLetter(Game1.CurrentSeasonDisplayName) + " " + Game1.dayOfMonth + "-" + Environment.NewLine + Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6664", NPCname);
					Response[] diaryOptions = new Response[3]
					{
						new Response("...We're", Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6667")),
						new Response("...I", (who.Gender == Gender.Male) ? Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6669") : Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6670")),
						new Response("(Write", Game1.content.LoadString("Strings\\StringsFromCSFiles:DiaryEvent.cs.6672"))
					};
					Game1.currentLocation.createQuestionDialogue(Game1.parseText(question), diaryOptions, "diary");
					Game1.messagePause = true;
					return false;
				}
			}
		}
		return true;
	}

	/// <inheritdoc />
	public override bool tickUpdate(GameTime time)
	{
		return !Game1.dialogueUp;
	}

	public override void makeChangesToLocation()
	{
		Game1.messagePause = false;
	}
}
